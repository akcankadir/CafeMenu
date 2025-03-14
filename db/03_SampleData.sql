USE CafeMenuDB;
GO

-- Örnek kategoriler
-- Ana kategoriler
IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Sıcak İçecekler')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Sıcak İçecekler', NULL, 1);
END

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Soğuk İçecekler')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Soğuk İçecekler', NULL, 1);
END

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Tatlılar')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Tatlılar', NULL, 1);
END

-- Alt kategoriler
DECLARE @SicakIceceklerID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Sıcak İçecekler');
DECLARE @SogukIceceklerID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Soğuk İçecekler');

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Kahveler')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Kahveler', @SicakIceceklerID, 1);
END

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Çaylar')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Çaylar', @SicakIceceklerID, 1);
END

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Meyve Suları')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Meyve Suları', @SogukIceceklerID, 1);
END

IF NOT EXISTS (SELECT * FROM Category WHERE CategoryName = 'Milkshake')
BEGIN
    INSERT INTO Category (CategoryName, ParentCategoryId, CreatedUserId)
    VALUES ('Milkshake', @SogukIceceklerID, 1);
END

-- Örnek ürünler
DECLARE @KahvelerID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Kahveler');
DECLARE @CaylarID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Çaylar');
DECLARE @MeyveSulariID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Meyve Suları');
DECLARE @MilkshakeID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Milkshake');
DECLARE @TatlilarID INT = (SELECT CategoryId FROM Category WHERE CategoryName = 'Tatlılar');

-- Kahveler
IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Espresso')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Espresso', @KahvelerID, 25.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Americano')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Americano', @KahvelerID, 30.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Latte')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Latte', @KahvelerID, 35.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Cappuccino')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Cappuccino', @KahvelerID, 35.00, 1);
END

-- Çaylar
IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Türk Çayı')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Türk Çayı', @CaylarID, 15.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Yeşil Çay')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Yeşil Çay', @CaylarID, 20.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Ihlamur')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Ihlamur', @CaylarID, 20.00, 1);
END

-- Meyve Suları
IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Portakal Suyu')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Portakal Suyu', @MeyveSulariID, 25.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Elma Suyu')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Elma Suyu', @MeyveSulariID, 25.00, 1);
END

-- Milkshake
IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Çikolatalı Milkshake')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Çikolatalı Milkshake', @MilkshakeID, 40.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Vanilyalı Milkshake')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Vanilyalı Milkshake', @MilkshakeID, 40.00, 1);
END

-- Tatlılar
IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Cheesecake')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Cheesecake', @TatlilarID, 45.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Tiramisu')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Tiramisu', @TatlilarID, 50.00, 1);
END

IF NOT EXISTS (SELECT * FROM Product WHERE ProductName = 'Sufle')
BEGIN
    INSERT INTO Product (ProductName, CategoryId, Price, CreatedUserId)
    VALUES ('Sufle', @TatlilarID, 55.00, 1);
END

-- Örnek özellikler
IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Yüksek')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Kafein Oranı', 'Yüksek');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Orta')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Kafein Oranı', 'Orta');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Düşük')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Kafein Oranı', 'Düşük');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Sıcaklık' AND Value = 'Sıcak')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Sıcaklık', 'Sıcak');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Sıcaklık' AND Value = 'Soğuk')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Sıcaklık', 'Soğuk');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Şeker İçeriği' AND Value = 'Şekerli')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Şeker İçeriği', 'Şekerli');
END

IF NOT EXISTS (SELECT * FROM Property WHERE [Key] = 'Şeker İçeriği' AND Value = 'Şekersiz')
BEGIN
    INSERT INTO Property ([Key], Value)
    VALUES ('Şeker İçeriği', 'Şekersiz');
END

-- Ürün-Özellik ilişkileri
-- Espresso özellikleri
DECLARE @EspressoID INT = (SELECT ProductId FROM Product WHERE ProductName = 'Espresso');
DECLARE @YuksekKafeinID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Yüksek');
DECLARE @SicakID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Sıcaklık' AND Value = 'Sıcak');
DECLARE @SekersizID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Şeker İçeriği' AND Value = 'Şekersiz');

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @EspressoID AND PropertyId = @YuksekKafeinID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@EspressoID, @YuksekKafeinID);
END

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @EspressoID AND PropertyId = @SicakID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@EspressoID, @SicakID);
END

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @EspressoID AND PropertyId = @SekersizID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@EspressoID, @SekersizID);
END

-- Türk Çayı özellikleri
DECLARE @TurkCayiID INT = (SELECT ProductId FROM Product WHERE ProductName = 'Türk Çayı');
DECLARE @OrtaKafeinID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Orta');

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @TurkCayiID AND PropertyId = @OrtaKafeinID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@TurkCayiID, @OrtaKafeinID);
END

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @TurkCayiID AND PropertyId = @SicakID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@TurkCayiID, @SicakID);
END

-- Portakal Suyu özellikleri
DECLARE @PortakalSuyuID INT = (SELECT ProductId FROM Product WHERE ProductName = 'Portakal Suyu');
DECLARE @DusukKafeinID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Kafein Oranı' AND Value = 'Düşük');
DECLARE @SogukID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Sıcaklık' AND Value = 'Soğuk');
DECLARE @SekerliID INT = (SELECT PropertyId FROM Property WHERE [Key] = 'Şeker İçeriği' AND Value = 'Şekerli');

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @PortakalSuyuID AND PropertyId = @DusukKafeinID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@PortakalSuyuID, @DusukKafeinID);
END

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @PortakalSuyuID AND PropertyId = @SogukID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@PortakalSuyuID, @SogukID);
END

IF NOT EXISTS (SELECT * FROM ProductProperty WHERE ProductId = @PortakalSuyuID AND PropertyId = @SekerliID)
BEGIN
    INSERT INTO ProductProperty (ProductId, PropertyId)
    VALUES (@PortakalSuyuID, @SekerliID);
END 