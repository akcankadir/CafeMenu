-- Hash ve Salt üretme fonksiyonu
CREATE OR ALTER FUNCTION [dbo].[GenerateSalt]()
RETURNS UNIQUEIDENTIFIER
AS
BEGIN
    RETURN NEWID()
END
GO

-- Şifre hashleme fonksiyonu
CREATE OR ALTER FUNCTION [dbo].[HashPassword]
(
    @password NVARCHAR(MAX),
    @salt UNIQUEIDENTIFIER
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    RETURN CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_512', CONCAT(@password, CAST(@salt AS NVARCHAR(36)))), 2)
END
GO

-- Kullanıcı ekleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[CreateUser]
    @username NVARCHAR(50),
    @password NVARCHAR(MAX),
    @email NVARCHAR(100),
    @firstName NVARCHAR(50),
    @lastName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @salt UNIQUEIDENTIFIER = dbo.GenerateSalt()
    DECLARE @hashPassword NVARCHAR(MAX) = dbo.HashPassword(@password, @salt)
    
    INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, FirstName, LastName, IsActive, CreatedAt)
    VALUES (@username, @hashPassword, @salt, @email, @firstName, @lastName, 1, GETDATE())
    
    SELECT SCOPE_IDENTITY() AS UserId
END
GO

-- Kullanıcı doğrulama stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[ValidateUser]
    @username NVARCHAR(50),
    @password NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @salt UNIQUEIDENTIFIER
    SELECT @salt = PasswordSalt FROM Users WHERE Username = @username AND IsActive = 1
    
    IF @salt IS NOT NULL
    BEGIN
        DECLARE @hashPassword NVARCHAR(MAX) = dbo.HashPassword(@password, @salt)
        
        SELECT u.UserId, u.Username, u.Email, u.FirstName, u.LastName, u.IsActive,
               r.RoleId, r.RoleName, r.Description
        FROM Users u
        LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
        LEFT JOIN Roles r ON ur.RoleId = r.RoleId
        WHERE u.Username = @username 
        AND u.PasswordHash = @hashPassword 
        AND u.IsActive = 1

        -- Son giriş tarihini güncelle
        UPDATE Users 
        SET LastLoginAt = GETDATE()
        WHERE Username = @username
    END
END
GO

-- Şifre güncelleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[UpdatePassword]
    @userId INT,
    @oldPassword NVARCHAR(MAX),
    @newPassword NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @salt UNIQUEIDENTIFIER
    SELECT @salt = PasswordSalt FROM Users WHERE UserId = @userId AND IsActive = 1
    
    IF @salt IS NOT NULL
    BEGIN
        DECLARE @oldHashPassword NVARCHAR(MAX) = dbo.HashPassword(@oldPassword, @salt)
        
        IF EXISTS (SELECT 1 FROM Users WHERE UserId = @userId AND PasswordHash = @oldHashPassword)
        BEGIN
            DECLARE @newSalt UNIQUEIDENTIFIER = dbo.GenerateSalt()
            DECLARE @newHashPassword NVARCHAR(MAX) = dbo.HashPassword(@newPassword, @newSalt)
            
            UPDATE Users
            SET PasswordHash = @newHashPassword,
                PasswordSalt = @newSalt
            WHERE UserId = @userId
            
            SELECT 1 AS Success
            RETURN
        END
    END
    
    SELECT 0 AS Success
END
GO

-- Kullanıcı listeleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[GetUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT u.UserId, u.Username, u.Email, u.FirstName, u.LastName, 
           u.IsActive, u.CreatedAt, u.LastLoginAt,
           r.RoleId, r.RoleName, r.Description
    FROM Users u
    LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN Roles r ON ur.RoleId = r.RoleId
    ORDER BY u.CreatedAt DESC
END
GO

-- Kullanıcı detayı getirme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[GetUserById]
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT u.UserId, u.Username, u.Email, u.FirstName, u.LastName, 
           u.IsActive, u.CreatedAt, u.LastLoginAt,
           r.RoleId, r.RoleName, r.Description
    FROM Users u
    LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE u.UserId = @userId
END
GO

-- Kullanıcı güncelleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[UpdateUser]
    @userId INT,
    @email NVARCHAR(100),
    @firstName NVARCHAR(50),
    @lastName NVARCHAR(50),
    @isActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET Email = @email,
        FirstName = @firstName,
        LastName = @lastName,
        IsActive = @isActive
    WHERE UserId = @userId
    
    SELECT @userId AS UserId
END
GO

-- Kullanıcı silme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[DeleteUser]
    @userId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET IsActive = 0
    WHERE UserId = @userId
    
    SELECT @userId AS UserId
END
GO

-- Kullanıcı rollerini güncelleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[UpdateUserRoles]
    @userId INT,
    @roleIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Önce mevcut rolleri temizle
    DELETE FROM UserRoles WHERE UserId = @userId
    
    -- Yeni rolleri ekle
    INSERT INTO UserRoles (UserId, RoleId)
    SELECT @userId, value
    FROM STRING_SPLIT(@roleIds, ',')
    WHERE ISNUMERIC(value) = 1
END
GO

-- Tüm rolleri getirme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[GetAllRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT RoleId, RoleName, Description
    FROM Roles
    ORDER BY RoleName
END
GO

-- Rol ekleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[CreateRole]
    @roleName NVARCHAR(50),
    @description NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Roles (RoleName, Description)
    VALUES (@roleName, @description)
    
    SELECT SCOPE_IDENTITY() AS RoleId
END
GO

-- Rol güncelleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[UpdateRole]
    @roleId INT,
    @roleName NVARCHAR(50),
    @description NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Roles
    SET RoleName = @roleName,
        Description = @description
    WHERE RoleId = @roleId
    
    SELECT @roleId AS RoleId
END
GO

-- Rol silme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[DeleteRole]
    @roleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Önce rol-kullanıcı ilişkilerini sil
    DELETE FROM UserRoles WHERE RoleId = @roleId
    
    -- Sonra rolü sil
    DELETE FROM Roles WHERE RoleId = @roleId
    
    SELECT @roleId AS RoleId
END
GO

-- Rol detayı getirme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[GetRoleById]
    @roleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT r.RoleId, r.RoleName, r.Description,
           u.UserId, u.Username, u.Email, u.FirstName, u.LastName, u.IsActive
    FROM Roles r
    LEFT JOIN UserRoles ur ON r.RoleId = ur.RoleId
    LEFT JOIN Users u ON ur.UserId = u.UserId
    WHERE r.RoleId = @roleId
END
GO

-- Rol adının benzersiz olup olmadığını kontrol etme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[IsRoleNameUnique]
    @roleName NVARCHAR(50),
    @roleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @roleId IS NULL
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM Roles WHERE RoleName = @roleName)
            THEN 0 ELSE 1 END AS IsUnique
    ELSE
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM Roles WHERE RoleName = @roleName AND RoleId != @roleId)
            THEN 0 ELSE 1 END AS IsUnique
END
GO

-- Profil güncelleme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[UpdateProfile]
    @userId INT,
    @email NVARCHAR(100),
    @firstName NVARCHAR(50),
    @lastName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET Email = @email,
        FirstName = @firstName,
        LastName = @lastName
    WHERE UserId = @userId
    
    SELECT u.UserId, u.Username, u.Email, u.FirstName, u.LastName, 
           u.IsActive, u.CreatedAt, u.LastLoginAt,
           r.RoleId, r.RoleName, r.Description
    FROM Users u
    LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE u.UserId = @userId
END
GO

-- E-posta adresinin benzersiz olup olmadığını kontrol etme stored procedure'u
CREATE OR ALTER PROCEDURE [dbo].[IsEmailUnique]
    @email NVARCHAR(100),
    @userId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @userId IS NULL
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @email AND IsActive = 1)
            THEN 0 ELSE 1 END AS IsUnique
    ELSE
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @email AND UserId != @userId AND IsActive = 1)
            THEN 0 ELSE 1 END AS IsUnique
END
GO 