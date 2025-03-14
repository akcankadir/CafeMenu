# Kafe Menü Yönetim Sistemi

Bu proje, bir kafe menü yönetim sistemi oluşturmayı hedeflemektedir. Sistem, yönetici ve müşteri panelleri olmak üzere iki ana bölümden oluşmaktadır. Yönetici paneli, kullanıcıların veritabanındaki tüm tablolarda CRUD (Create, Read, Update, Delete) işlemleri yapabilmesini sağlarken, müşteri paneli ise kullanıcıların sadece ürünleri görüntüleyebileceği bir arayüz sunmaktadır.

## Teknolojiler

- .NET Core 8 (C#)
- MSSQL
- Entity Framework Core (Db-First)
- Stored Procedure'ler (SP)
- Bootstrap 5
- HTML/CSS/JavaScript

## Proje Yapısı

- **db Klasörü:** Veritabanı script dosyaları (CREATE TABLE ve stored procedure'ler)
- **Models:** Veritabanı tablolarına karşılık gelen model sınıfları
- **Services:** Veritabanı işlemleri için servis sınıfları
- **Controllers:** MVC mimarisindeki controller sınıfları
- **Views:** Kullanıcı arayüzü için view dosyaları
- **wwwroot:** Statik dosyalar (CSS, JavaScript, resimler)

## Veritabanı Yapısı

- **User Tablosu:** Sistemdeki kullanıcıların bilgilerini tutar.
- **Category Tablosu:** Kafe menüsündeki kategorileri tutar. Hiyerarşik yapıyı destekler.
- **Product Tablosu:** Kafe menüsündeki ürünleri tutar.
- **Property Tablosu:** Ürünlerin özelliklerini tutar.
- **ProductProperty Tablosu:** Ürünler ve özellikler arasındaki ilişkiyi tutar.

## Kurulum

1. Veritabanını oluşturmak için `db` klasöründeki SQL scriptlerini sırasıyla çalıştırın:
   - `01_CreateTables.sql`
   - `02_CreateStoredProcedures.sql`
   - `03_SampleData.sql`

2. `appsettings.json` dosyasındaki veritabanı bağlantı bilgilerini kendi ortamınıza göre güncelleyin.

3. Projeyi derleyin ve çalıştırın.

## Kullanım

### Müşteri Paneli

- Ana sayfa: Kategorileri ve ürünleri görüntüleyebilirsiniz.
- Kategori sayfası: Belirli bir kategorideki ürünleri görüntüleyebilirsiniz.
- Ürün detay sayfası: Ürün detaylarını görüntüleyebilirsiniz.

### Yönetici Paneli

- Giriş: `/Account/Login` adresinden giriş yapabilirsiniz.
  - Örnek kullanıcı: admin / admin123
- Dashboard: Hızlı erişim için özet bilgiler.
- Kullanıcılar: Kullanıcı hesaplarını yönetebilirsiniz.
- Kategoriler: Menü kategorilerini yönetebilirsiniz.
- Ürünler: Menü ürünlerini yönetebilirsiniz.

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır. 