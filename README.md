# CafeMenu - Çok Kiracılı Menü Yönetim Sistemi

Bu proje, restoran ve kafeler için geliştirilmiş çok kiracılı (multi-tenant) bir menü yönetim sistemidir. Her işletme kendi menüsünü oluşturabilir, düzenleyebilir ve müşterilerine sunabilir.

## Özellikler

- **Çok Kiracılı Mimari**: Her işletme kendi verileri üzerinde çalışır
- **Rol Tabanlı Yetkilendirme**: Admin, Yönetici, Personel rolleri
- **Kategori ve Ürün Yönetimi**: Kategoriler ve ürünler için CRUD işlemleri
- **Özellik Yönetimi**: Ürünlere özel özellikler ekleyebilme
- **Döviz Kuru Desteği**: Farklı para birimlerinde fiyat gösterimi
- **Performans Optimizasyonu**: Redis ve Memory Cache kullanımı
- **Gerçek Zamanlı Güncelleme**: SignalR ile döviz kuru güncellemeleri
- **Responsive Tasarım**: Mobil uyumlu kullanıcı arayüzü

## Teknolojiler

- **Backend**: ASP.NET Core 8.0
- **Veritabanı**: SQL Server, Entity Framework Core
- **Önbellek**: Redis, Memory Cache
- **Gerçek Zamanlı İletişim**: SignalR
- **Frontend**: Bootstrap, jQuery, JavaScript
- **API**: RESTful API
- **Güvenlik**: Cookie Authentication, CSRF koruması
- **Performans**: Rate Limiting, Async/Await pattern

## Mimari

Proje, Clean Architecture prensiplerine uygun olarak geliştirilmiştir:

- **Presentation Layer**: Controllers, Views
- **Business Layer**: Services
- **Data Access Layer**: Repository Pattern, Entity Framework Core
- **Domain Layer**: Models

## Kurulum

```bash
# Projeyi klonlayın
git clone https://github.com/akcankadir/CafeMenu.git

# Proje dizinine gidin
cd CafeMenu

# Bağımlılıkları yükleyin
dotnet restore

# Veritabanını oluşturun
dotnet ef database update

# Projeyi çalıştırın
dotnet run
```

## Kullanım

Sistem ilk çalıştırıldığında varsayılan bir admin kullanıcısı oluşturulur:

- **Kullanıcı Adı**: admin
- **Şifre**: Admin123!

Bu kullanıcı ile giriş yaparak yeni tenant'lar (işletmeler) oluşturabilir ve yönetebilirsiniz.

## Geliştirme Prensipleri

- SOLID prensipleri
- DRY (Don't Repeat Yourself)
- Dependency Injection
- Asenkron programlama
- Unit testler
- Performans optimizasyonu

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakınız. 