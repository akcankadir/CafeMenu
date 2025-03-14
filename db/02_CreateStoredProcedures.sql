USE CafeMenuDB;
GO

-- User tablosu için stored procedure'ler
-- sp_User_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_GetAll')
    DROP PROCEDURE sp_User_GetAll
GO

CREATE PROCEDURE sp_User_GetAll
AS
BEGIN
    SELECT UserId, Name, UserName, Password
    FROM [User];
END
GO

-- sp_User_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_GetById')
    DROP PROCEDURE sp_User_GetById
GO

CREATE PROCEDURE sp_User_GetById
    @UserId INT
AS
BEGIN
    SELECT UserId, Name, UserName, Password
    FROM [User]
    WHERE UserId = @UserId;
END
GO

-- sp_User_Insert
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_Insert')
    DROP PROCEDURE sp_User_Insert
GO

CREATE PROCEDURE sp_User_Insert
    @Name NVARCHAR(100),
    @UserName NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    INSERT INTO [User] (Name, UserName, Password)
    VALUES (@Name, @UserName, @Password);
    
    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

-- sp_User_Update
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_Update')
    DROP PROCEDURE sp_User_Update
GO

CREATE PROCEDURE sp_User_Update
    @UserId INT,
    @Name NVARCHAR(100),
    @UserName NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    UPDATE [User]
    SET Name = @Name,
        UserName = @UserName,
        Password = @Password
    WHERE UserId = @UserId;
END
GO

-- sp_User_Delete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_User_Delete')
    DROP PROCEDURE sp_User_Delete
GO

CREATE PROCEDURE sp_User_Delete
    @UserId INT
AS
BEGIN
    DELETE FROM [User]
    WHERE UserId = @UserId;
END
GO

-- Category tablosu için stored procedure'ler
-- sp_Category_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Category_GetAll')
    DROP PROCEDURE sp_Category_GetAll
GO

CREATE PROCEDURE sp_Category_GetAll
AS
BEGIN
    SELECT c.CategoryId, c.CategoryName, c.ParentCategoryId, c.IsDeleted, c.CreatedDate, c.CreatedUserId,
           p.CategoryName AS ParentCategoryName
    FROM Category c
    LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
    WHERE c.IsDeleted = 0;
END
GO

-- sp_Category_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Category_GetById')
    DROP PROCEDURE sp_Category_GetById
GO

CREATE PROCEDURE sp_Category_GetById
    @CategoryId INT
AS
BEGIN
    SELECT c.CategoryId, c.CategoryName, c.ParentCategoryId, c.IsDeleted, c.CreatedDate, c.CreatedUserId,
           p.CategoryName AS ParentCategoryName
    FROM Category c
    LEFT JOIN Category p ON c.ParentCategoryId = p.CategoryId
    WHERE c.CategoryId = @CategoryId AND c.IsDeleted = 0;
END
GO

-- sp_Category_Insert
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Category_Insert')
    DROP PROCEDURE sp_Category_Insert
GO

CREATE PROCEDURE sp_Category_Insert
    @CategoryName NVARCHAR(100),
    @ParentCategoryId INT = NULL,
    @CreatedUserId INT
AS
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES (@CategoryName, @ParentCategoryId, @CreatedUserId);
    
    SELECT SCOPE_IDENTITY() AS CategoryId;
END
GO

-- sp_Category_Update
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Category_Update')
    DROP PROCEDURE sp_Category_Update
GO

CREATE PROCEDURE sp_Category_Update
    @CategoryId INT,
    @CategoryName NVARCHAR(100),
    @ParentCategoryId INT = NULL
AS
BEGIN
    UPDATE Category
    SET CategoryName = @CategoryName,
        ParentCategoryId = @ParentCategoryId
    WHERE CategoryId = @CategoryId;
END
GO

-- sp_Category_Delete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Category_Delete')
    DROP PROCEDURE sp_Category_Delete
GO

CREATE PROCEDURE sp_Category_Delete
    @CategoryId INT
AS
BEGIN
    UPDATE Category
    SET IsDeleted = 1
    WHERE CategoryId = @CategoryId;
END
GO

-- Product tablosu için stored procedure'ler
-- sp_Product_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_GetAll')
    DROP PROCEDURE sp_Product_GetAll
GO

CREATE PROCEDURE sp_Product_GetAll
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, p.Price, p.ImagePath, p.IsDeleted, p.CreatedDate, p.CreatedUserId,
           c.CategoryName
    FROM Product p
    INNER JOIN Category c ON p.CategoryId = c.CategoryId
    WHERE p.IsDeleted = 0;
END
GO

-- sp_Product_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_GetById')
    DROP PROCEDURE sp_Product_GetById
GO

CREATE PROCEDURE sp_Product_GetById
    @ProductId INT
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, p.Price, p.ImagePath, p.IsDeleted, p.CreatedDate, p.CreatedUserId,
           c.CategoryName
    FROM Product p
    INNER JOIN Category c ON p.CategoryId = c.CategoryId
    WHERE p.ProductId = @ProductId AND p.IsDeleted = 0;
END
GO

-- sp_Product_GetByCategoryId
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_GetByCategoryId')
    DROP PROCEDURE sp_Product_GetByCategoryId
GO

CREATE PROCEDURE sp_Product_GetByCategoryId
    @CategoryId INT
AS
BEGIN
    SELECT p.ProductId, p.ProductName, p.CategoryId, p.Price, p.ImagePath, p.IsDeleted, p.CreatedDate, p.CreatedUserId,
           c.CategoryName
    FROM Product p
    INNER JOIN Category c ON p.CategoryId = c.CategoryId
    WHERE p.CategoryId = @CategoryId AND p.IsDeleted = 0;
END
GO

-- sp_Product_Insert
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_Insert')
    DROP PROCEDURE sp_Product_Insert
GO

CREATE PROCEDURE sp_Product_Insert
    @ProductName NVARCHAR(100),
    @CategoryId INT,
    @Price DECIMAL(10,2),
    @ImagePath NVARCHAR(255) = NULL,
    @CreatedUserId INT
AS
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, ImagePath, CreatedUserId)
    VALUES (@ProductName, @CategoryId, @Price, @ImagePath, @CreatedUserId);
    
    SELECT SCOPE_IDENTITY() AS ProductId;
END
GO

-- sp_Product_Update
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_Update')
    DROP PROCEDURE sp_Product_Update
GO

CREATE PROCEDURE sp_Product_Update
    @ProductId INT,
    @ProductName NVARCHAR(100),
    @CategoryId INT,
    @Price DECIMAL(10,2),
    @ImagePath NVARCHAR(255) = NULL
AS
BEGIN
    UPDATE Product
    SET ProductName = @ProductName,
        CategoryId = @CategoryId,
        Price = @Price,
        ImagePath = @ImagePath
    WHERE ProductId = @ProductId;
END
GO

-- sp_Product_Delete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Product_Delete')
    DROP PROCEDURE sp_Product_Delete
GO

CREATE PROCEDURE sp_Product_Delete
    @ProductId INT
AS
BEGIN
    UPDATE Product
    SET IsDeleted = 1
    WHERE ProductId = @ProductId;
END
GO

-- Property tablosu için stored procedure'ler
-- sp_Property_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Property_GetAll')
    DROP PROCEDURE sp_Property_GetAll
GO

CREATE PROCEDURE sp_Property_GetAll
AS
BEGIN
    SELECT PropertyId, [Key], Value
    FROM Property;
END
GO

-- sp_Property_GetById
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Property_GetById')
    DROP PROCEDURE sp_Property_GetById
GO

CREATE PROCEDURE sp_Property_GetById
    @PropertyId INT
AS
BEGIN
    SELECT PropertyId, [Key], Value
    FROM Property
    WHERE PropertyId = @PropertyId;
END
GO

-- sp_Property_Insert
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Property_Insert')
    DROP PROCEDURE sp_Property_Insert
GO

CREATE PROCEDURE sp_Property_Insert
    @Key NVARCHAR(100),
    @Value NVARCHAR(255)
AS
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES (@Key, @Value);
    
    SELECT SCOPE_IDENTITY() AS PropertyId;
END
GO

-- sp_Property_Update
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Property_Update')
    DROP PROCEDURE sp_Property_Update
GO

CREATE PROCEDURE sp_Property_Update
    @PropertyId INT,
    @Key NVARCHAR(100),
    @Value NVARCHAR(255)
AS
BEGIN
    UPDATE Property
    SET [Key] = @Key,
        Value = @Value
    WHERE PropertyId = @PropertyId;
END
GO

-- sp_Property_Delete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Property_Delete')
    DROP PROCEDURE sp_Property_Delete
GO

CREATE PROCEDURE sp_Property_Delete
    @PropertyId INT
AS
BEGIN
    DELETE FROM Property
    WHERE PropertyId = @PropertyId;
END
GO

-- ProductProperty tablosu için stored procedure'ler
-- sp_ProductProperty_GetAll
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ProductProperty_GetAll')
    DROP PROCEDURE sp_ProductProperty_GetAll
GO

CREATE PROCEDURE sp_ProductProperty_GetAll
AS
BEGIN
    SELECT pp.ProductPropertyId, pp.ProductId, pp.PropertyId,
           p.ProductName, pr.[Key], pr.Value
    FROM ProductProperty pp
    INNER JOIN Product p ON pp.ProductId = p.ProductId
    INNER JOIN Property pr ON pp.PropertyId = pr.PropertyId
    WHERE p.IsDeleted = 0;
END
GO

-- sp_ProductProperty_GetByProductId
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ProductProperty_GetByProductId')
    DROP PROCEDURE sp_ProductProperty_GetByProductId
GO

CREATE PROCEDURE sp_ProductProperty_GetByProductId
    @ProductId INT
AS
BEGIN
    SELECT pp.ProductPropertyId, pp.ProductId, pp.PropertyId,
           p.ProductName, pr.[Key], pr.Value
    FROM ProductProperty pp
    INNER JOIN Product p ON pp.ProductId = p.ProductId
    INNER JOIN Property pr ON pp.PropertyId = pr.PropertyId
    WHERE pp.ProductId = @ProductId AND p.IsDeleted = 0;
END
GO

-- sp_ProductProperty_Insert
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ProductProperty_Insert')
    DROP PROCEDURE sp_ProductProperty_Insert
GO

CREATE PROCEDURE sp_ProductProperty_Insert
    @ProductId INT,
    @PropertyId INT
AS
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@ProductId, @PropertyId);
    
    SELECT SCOPE_IDENTITY() AS ProductPropertyId;
END
GO

-- sp_ProductProperty_Delete
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ProductProperty_Delete')
    DROP PROCEDURE sp_ProductProperty_Delete
GO

CREATE PROCEDURE sp_ProductProperty_Delete
    @ProductPropertyId INT
AS
BEGIN
    DELETE FROM ProductProperty
    WHERE ProductPropertyId = @ProductPropertyId;
END
GO 