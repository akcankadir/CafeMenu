-- Kullanıcılar tablosu
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PasswordSalt UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME NULL
);

-- Roller tablosu
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL
);

-- Kullanıcı Rolleri tablosu
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Varsayılan rolleri ekle
INSERT INTO Roles (RoleName, Description) VALUES 
('Admin', 'Sistem yöneticisi'),
('Manager', 'Mağaza yöneticisi'),
('Staff', 'Personel');

-- Varsayılan admin kullanıcısı ekle (şifre: Admin123!)
DECLARE @AdminSalt UNIQUEIDENTIFIER = NEWID();
INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, FirstName, LastName)
VALUES (
    'admin',
    CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_512', CONCAT('Admin123!', CAST(@AdminSalt AS NVARCHAR(36)))), 2),
    @AdminSalt,
    'admin@cafemenu.com',
    'System',
    'Administrator'
);

-- Admin kullanıcısına admin rolü ata
INSERT INTO UserRoles (UserId, RoleId)
SELECT 
    (SELECT UserId FROM Users WHERE Username = 'admin'),
    (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

-- Tenant (Çoklu Kiracı) Tablosu
CREATE TABLE Tenants (
    TenantId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Domain NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatorUserId INT,
    IsDeleted BIT DEFAULT 0
);

-- Kategori Tablosu
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    ParentCategoryId INT,
    IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatorUserId INT,
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (CreatorUserId) REFERENCES Users(UserId)
);

-- Özellik Tablosu
CREATE TABLE Properties (
    PropertyId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL,
    [Key] NVARCHAR(50) NOT NULL,
    Value NVARCHAR(200) NOT NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatorUserId INT,
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (CreatorUserId) REFERENCES Users(UserId)
);

-- Ürün Tablosu
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    TenantId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    CategoryId INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImagePath NVARCHAR(500),
    IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatorUserId INT,
    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    FOREIGN KEY (CreatorUserId) REFERENCES Users(UserId)
);

-- Ürün Özellik İlişki Tablosu
CREATE TABLE ProductProperties (
    ProductPropertyId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    PropertyId INT NOT NULL,
    IsDeleted BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatorUserId INT,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    FOREIGN KEY (PropertyId) REFERENCES Properties(PropertyId),
    FOREIGN KEY (CreatorUserId) REFERENCES Users(UserId)
);

-- Döviz Kuru Tablosu
CREATE TABLE ExchangeRates (
    ExchangeRateId INT IDENTITY(1,1) PRIMARY KEY,
    CurrencyCode NVARCHAR(3) NOT NULL,
    Rate DECIMAL(18,4) NOT NULL,
    UpdateDate DATETIME DEFAULT GETDATE()
);

-- İndeksler
CREATE INDEX IX_Categories_TenantId ON Categories(TenantId);
CREATE INDEX IX_Categories_ParentCategoryId ON Categories(ParentCategoryId);
CREATE INDEX IX_Products_TenantId ON Products(TenantId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Properties_TenantId ON Properties(TenantId);
CREATE INDEX IX_ProductProperties_ProductId ON ProductProperties(ProductId);
CREATE INDEX IX_ProductProperties_PropertyId ON ProductProperties(PropertyId);
CREATE INDEX IX_ExchangeRates_CurrencyCode ON ExchangeRates(CurrencyCode);

-- Benzersiz domain kontrolü
CREATE UNIQUE INDEX IX_Tenants_Domain ON Tenants(Domain);

-- Benzersiz rol adı kontrolü (tenant bazlı)
CREATE UNIQUE INDEX IX_Roles_Name_TenantId ON Roles(RoleName, TenantId) WHERE TenantId IS NOT NULL;
CREATE UNIQUE INDEX IX_Roles_Name_System ON Roles(RoleName) WHERE TenantId IS NULL;

-- Benzersiz kullanıcı adı kontrolü (tenant bazlı)
CREATE UNIQUE INDEX IX_Users_Username_TenantId ON Users(Username, TenantId) WHERE TenantId IS NOT NULL;
CREATE UNIQUE INDEX IX_Users_Username_System ON Users(Username) WHERE TenantId IS NULL;

-- Benzersiz kullanıcı-rol ilişkisi
CREATE UNIQUE INDEX IX_UserRoles_UserId_RoleId ON UserRoles(UserId, RoleId);

-- Kategori indeksi
CREATE INDEX IX_Categories_ParentCategoryId ON Categories(ParentCategoryId);
CREATE INDEX IX_Categories_IsDeleted ON Categories(IsDeleted);

-- Ürün indeksleri
CREATE INDEX IX_Products_IsDeleted ON Products(IsDeleted);
CREATE INDEX IX_Products_TenantId_CategoryId_IsDeleted ON Products(TenantId, CategoryId, IsDeleted);

-- Özellik indeksi
CREATE INDEX IX_Properties_TenantId ON Properties(TenantId);
CREATE UNIQUE INDEX IX_Properties_Key_TenantId ON Properties([Key], TenantId);

-- Ürün-özellik indeksi
CREATE UNIQUE INDEX IX_ProductProperties_ProductId_PropertyId ON ProductProperties(ProductId, PropertyId);
CREATE INDEX IX_ProductProperties_TenantId ON ProductProperties(TenantId);

-- Döviz kuru indeksi
CREATE INDEX IX_ExchangeRates_UpdatedAt ON ExchangeRates(UpdateDate);

-- Varsayılan tenant oluştur
INSERT INTO Tenants (Name, Domain, Description, IsActive)
VALUES ('Default', 'localhost', 'Default Tenant', 1);

-- Sistem rolleri oluştur
INSERT INTO Roles (RoleName, Description, IsSystemRole)
VALUES 
('Admin', 'System Administrator', 1),
('Manager', 'Tenant Manager', 1),
('User', 'Regular User', 1);

-- Admin kullanıcısı oluştur (şifre: Admin123!)
DECLARE @salt UNIQUEIDENTIFIER = NEWID();
DECLARE @hash NVARCHAR(MAX) = CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_512', CONCAT('Admin123!', CAST(@salt AS NVARCHAR(36)))), 2);

INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, FirstName, LastName)
VALUES ('admin', @hash, @salt, 'admin@example.com', 'System', 'Admin');

-- Admin kullanıcısına Admin rolü ver
INSERT INTO UserRoles (UserId, RoleId)
VALUES (1, 1); 