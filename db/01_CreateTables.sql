-- Veritabanı oluşturma
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CafeMenuDB')
BEGIN
    CREATE DATABASE CafeMenuDB;
END
GO

USE CafeMenuDB;
GO

-- User tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User')
BEGIN
    CREATE TABLE [User] (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        UserName NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL
    );
END
GO

-- Category tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Category')
BEGIN
    CREATE TABLE Category (
        CategoryId INT PRIMARY KEY IDENTITY(1,1),
        CategoryName NVARCHAR(100) NOT NULL,
        ParentCategoryId INT NULL,
        IsDeleted BIT DEFAULT 0,
        CreatedDate DATETIME DEFAULT GETDATE(),
        CreatedUserId INT NOT NULL,
        FOREIGN KEY (ParentCategoryId) REFERENCES Category(CategoryId),
        FOREIGN KEY (CreatedUserId) REFERENCES [User](UserId)
    );
END
GO

-- Product tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Product')
BEGIN
    CREATE TABLE Product (
        ProductId INT PRIMARY KEY IDENTITY(1,1),
        ProductName NVARCHAR(100) NOT NULL,
        CategoryId INT NOT NULL,
        Price DECIMAL(10,2) NOT NULL,
        ImagePath NVARCHAR(255) NULL,
        IsDeleted BIT DEFAULT 0,
        CreatedDate DATETIME DEFAULT GETDATE(),
        CreatedUserId INT NOT NULL,
        FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
        FOREIGN KEY (CreatedUserId) REFERENCES [User](UserId)
    );
END
GO

-- Property tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Property')
BEGIN
    CREATE TABLE Property (
        PropertyId INT PRIMARY KEY IDENTITY(1,1),
        [Key] NVARCHAR(100) NOT NULL,
        Value NVARCHAR(255) NOT NULL
    );
END
GO

-- ProductProperty tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductProperty')
BEGIN
    CREATE TABLE ProductProperty (
        ProductPropertyId INT PRIMARY KEY IDENTITY(1,1),
        ProductId INT NOT NULL,
        PropertyId INT NOT NULL,
        FOREIGN KEY (ProductId) REFERENCES Product(ProductId),
        FOREIGN KEY (PropertyId) REFERENCES Property(PropertyId),
        CONSTRAINT UQ_ProductProperty UNIQUE (ProductId, PropertyId)
    );
END
GO

-- Örnek kullanıcılar ekleme
IF NOT EXISTS (SELECT * FROM [User] WHERE UserName = 'admin')
BEGIN
    INSERT INTO [User] (Name, UserName, Password)
    VALUES ('Admin User', 'admin', 'admin123');
END

IF NOT EXISTS (SELECT * FROM [User] WHERE UserName = 'barista')
BEGIN
    INSERT INTO [User] (Name, UserName, Password)
    VALUES ('Barista User', 'barista', 'barista123');
END
GO 