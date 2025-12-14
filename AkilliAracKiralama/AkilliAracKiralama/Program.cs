using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

// --- 1. Temel Model Sınıfları ---

public class Arac
{
    public string Plaka { get; set; } // Plaka bilgisi tutulmalıdır
    public string MarkaModel { get; set; } // Marka/model bilgisi tutulmalıdır
    public double GunlukFiyat { get; set; } // Günlük fiyat bilgisi tutulmalıdır

    public Arac(string plaka, string markaModel, double gunlukFiyat)
    {
        Plaka = plaka;
        MarkaModel = markaModel;
        GunlukFiyat = gunlukFiyat;
    }
}

public class Rezervasyon
{
    public string MusteriAdi { get; set; }
    public string KiralananPlaka { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public double Ucret { get; set; } // Rezervasyon ücreti otomatik hesaplanmalıdır

    public Rezervasyon(string musteriAdi, string plaka, DateTime baslangic, DateTime bitis, double ucret)
    {
        MusteriAdi = musteriAdi;
        KiralananPlaka = plaka;
        BaslangicTarihi = baslangic;
        BitisTarihi = bitis;
        Ucret = ucret;
    }
}


// --- 2. İş Mantığı Sınıfı (KiralamaSistemi) ---

public class KiralamaSistemi
{
    private List<Arac> TumAraclar { get; set; }
    private List<Rezervasyon> TumRezervasyonlar { get; set; }

    public KiralamaSistemi()
    {
        TumAraclar = new List<Arac>();
        TumRezervasyonlar = new List<Rezervasyon>();

        // Başlangıç Araç Verileri
        TumAraclar.Add(new Arac("34 ABC 123", "VW Passat", 500.0));
        TumAraclar.Add(new Arac("06 XYZ 456", "Fiat Egea", 350.0));
        TumAraclar.Add(new Arac("42 KCM 789", "BMW X5", 900.0));
    }

    // --- Zorunlu Fonksiyonlar: Araç ---

    // 1. double AracGunlukFiyatiniGetir(string plaka)
    public double AracGunlukFiyatiniGetir(string plaka)
    {
        Arac bulunanArac = TumAraclar.FirstOrDefault(a => a.Plaka.Equals(plaka, StringComparison.OrdinalIgnoreCase));
        if (bulunanArac != null)
        {
            return bulunanArac.GunlukFiyat;
        }
        return 0.0;
    }

    // 2. bool AracMusaitMi(string plaka, DateTime bas, DateTime bit)
    public bool AracMusaitMi(string plaka, DateTime bas, DateTime bit)
    {
        var ilgiliRezervasyonlar = TumRezervasyonlar
            .Where(r => r.KiralananPlaka.Equals(plaka, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var mevcutRez in ilgiliRezervasyonlar)
        {
            // Çakışma Kontrolü: (Yeni Başlangıç < Mevcut Bitiş) VE (Yeni Bitiş > Mevcut Başlangıç)
            if (bas < mevcutRez.BitisTarihi && bit > mevcutRez.BaslangicTarihi)
            {
                return false; // Çakışma bulundu
            }
        }
        return true; // Araç MÜSAİT
    }

    // 3. List<string> MusaitAraclariGetir(DateTime baslangic, DateTime bitis)
    public List<string> MusaitAraclariGetir(DateTime baslangic, DateTime bitis)
    {
        List<string> musaitPlakalar = new List<string>();
        foreach (var arac in TumAraclar)
        {
            if (AracMusaitMi(arac.Plaka, baslangic, bitis))
            {
                musaitPlakalar.Add(arac.Plaka);
            }
        }
        return musaitPlakalar;
    }

    // --- Zorunlu Fonksiyonlar: Rezervasyon ---

    // 4. double RezervasyonUcretiHesapla(string plaka, DateTime bas, DateTime bit)
    public double RezervasyonUcretiHesapla(string plaka, DateTime bas, DateTime bit)
    {
        double gunlukFiyat = AracGunlukFiyatiniGetir(plaka);

        if (gunlukFiyat == 0.0 || bas >= bit)
        {
            return 0.0;
        }

        TimeSpan sure = bit.Subtract(bas);
        int gunSayisi = (int)Math.Ceiling(sure.TotalDays);

        return gunlukFiyat * gunSayisi;
    }

    // 5. void RezervasyonEkle(string musteri, string plaka, DateTime bas, DateTime bit)
    public void RezervasyonEkle(string musteri, string plaka, DateTime bas, DateTime bit)
    {
        // 1. Giriş ve Geçerlilik Kontrolleri
        if (string.IsNullOrWhiteSpace(musteri) || string.IsNullOrWhiteSpace(plaka) || bas >= bit)
        {
            Console.WriteLine("HATA: Girdi bilgileri eksik veya süre geçersiz.");
            return;
        }

        // 2. Fiyat Hesaplama (Araç varlığını da kontrol eder)
        double ucret = RezervasyonUcretiHesapla(plaka, bas, bit);
        if (ucret == 0.0)
        {
            Console.WriteLine($"HATA: '{plaka}' plakalı araç sistemde bulunamadı veya süre hesaplanamadı.");
            return;
        }

        // 3. Müsaitlik Kontrolü (Tarih çakışması engellenir)
        if (!AracMusaitMi(plaka, bas, bit))
        {
            Console.WriteLine($"HATA: '{plaka}' plakalı araç, {bas.ToShortDateString()} - {bit.ToShortDateString()} tarihleri arasında müsait değil.");
            return;
        }

        // 4. Rezervasyonu Oluşturma ve Ekleme
        Rezervasyon yeniRezervasyon = new Rezervasyon(musteri, plaka, bas, bit, ucret);
        TumRezervasyonlar.Add(yeniRezervasyon);

        Console.WriteLine($"\nBAŞARILI: Yeni rezervasyon oluşturuldu!");
        Console.WriteLine($"  Müşteri: {musteri}, Araç: {plaka}");
        Console.WriteLine($"  Süre: {bas.ToShortDateString()} - {bit.ToShortDateString()}");
        Console.WriteLine($"  Toplam Ücret: {ucret:C}");
    }

    // 6. void RezervasyonIptal(string plaka)
    public void RezervasyonIptal(string plaka)
    {
        Rezervasyon iptalEdilecekRez = TumRezervasyonlar
            .FirstOrDefault(r => r.KiralananPlaka.Equals(plaka, StringComparison.OrdinalIgnoreCase));

        if (iptalEdilecekRez != null)
        {
            TumRezervasyonlar.Remove(iptalEdilecekRez);
            Console.WriteLine($"\nBAŞARILI: '{plaka}' plakalı araca ait rezervasyon ({iptalEdilecekRez.MusteriAdi} adına) iptal edildi.");
        }
        else
        {
            Console.WriteLine($"\nHATA: '{plaka}' plakasına ait aktif bir rezervasyon bulunamadı.");
        }
    }

    // --- Zorunlu Fonksiyonlar: Raporlama ---

    // 7. double ToplamGelir()
    public double ToplamGelir()
    {
        // Yapılan tüm rezervasyonların ücretlerini toplar
        return TumRezervasyonlar.Sum(r => r.Ucret);
    }

    // 8. List<string> MusteriRezervasyonlariniGetir(string musteri)
    public List<string> MusteriRezervasyonlariniGetir(string musteri)
    {
        List<string> ozetListesi = new List<string>();

        var ilgiliRezervasyonlar = TumRezervasyonlar
            .Where(r => r.MusteriAdi.Equals(musteri, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!ilgiliRezervasyonlar.Any())
        {
            ozetListesi.Add($"'{musteri}' adına kayıtlı rezervasyon bulunmamaktadır.");
            return ozetListesi;
        }

        foreach (var rez in ilgiliRezervasyonlar)
        {
            string ozet = $"Araç: {rez.KiralananPlaka}, " +
                          $"Tarih: {rez.BaslangicTarihi.ToShortDateString()} - {rez.BitisTarihi.ToShortDateString()}, " +
                          $"Ücret: {rez.Ucret:C}";
            ozetListesi.Add(ozet);
        }
        return ozetListesi;
    }

    // 9. string EnCokKiralananArac()
    public string EnCokKiralananArac()
    {
        if (!TumRezervasyonlar.Any())
        {
            return "Sistemde henüz rezervasyon bulunmamaktadır.";
        }

        var enCokKiralanan = TumRezervasyonlar
            .GroupBy(r => r.KiralananPlaka)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        Arac enCokAracBilgisi = TumAraclar.FirstOrDefault(a => a.Plaka == enCokKiralanan);
        int kiralamaSayisi = TumRezervasyonlar.Count(r => r.KiralananPlaka == enCokKiralanan);

        if (enCokAracBilgisi != null)
        {
            return $"{enCokAracBilgisi.MarkaModel} ({enCokAracBilgisi.Plaka}) - Toplam Kiralama: {kiralamaSayisi} kez";
        }
        return "En çok kiralanan araç bilgisi alınamadı.";
    }
}


// --- 3. Ana Uygulama Başlangıcı ve Menü Mantığı ---

public static class Program
{
    // 10. Zorunlu Fonksiyon (Yardımcı/Bonus Fonksiyon): Hata Kontrollü Tarih Girdisi
    public static DateTime GecerliTarihGirisiAl(string mesaj)
    {
        DateTime tarih;
        bool gecerliGirdi = false;

        do
        {
            Console.Write(mesaj);
            string girdi = Console.ReadLine();

            // Tarih formatının doğru olup olmadığını kontrol et (gg.aa.yyyy bekleniyor)
            if (DateTime.TryParseExact(girdi, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tarih))
            {
                if (tarih >= DateTime.Today)
                {
                    gecerliGirdi = true;
                }
                else
                {
                    Console.WriteLine("HATA: Tarih bugünden önceki bir tarih olamaz.");
                }
            }
            else
            {
                Console.WriteLine("HATA: Geçersiz tarih formatı! Lütfen 'gg.aa.yyyy' formatında giriniz.");
            }
        } while (!gecerliGirdi);

        return tarih;
    }

    public static void Main(string[] args)
    {
        KiralamaSistemi sistem = new KiralamaSistemi();
        bool cikis = false;

        Console.WriteLine("--- Akıllı Araç Kiralama Rezervasyon Sistemine Hoş Geldiniz! ---");

        // Başlangıç verisi ekleyelim (Raporlama testleri ve menü başlangıcı için)
        sistem.RezervasyonEkle("Deniz Kaya", "34 ABC 123", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3));
        sistem.RezervasyonEkle("Deniz Kaya", "06 XYZ 456", DateTime.Today.AddDays(1), DateTime.Today.AddDays(5));
        sistem.RezervasyonEkle("Hakan Öztürk", "34 ABC 123", DateTime.Today.AddDays(10), DateTime.Today.AddDays(12));


        while (!cikis)
        {
            Console.WriteLine("\n================ ANA MENÜ ================");
            Console.WriteLine("1. Araç Müsaitlik Sorgulama");
            Console.WriteLine("2. Yeni Rezervasyon Oluştur");
            Console.WriteLine("3. Rezervasyon İptal Et");
            Console.WriteLine("4. Raporları Görüntüle");
            Console.WriteLine("5. Çıkış");
            Console.Write("Lütfen seçiminizi yapın (1-5): ");

            string secim = Console.ReadLine();
            Console.WriteLine("------------------------------------------");

            switch (secim)
            {
                case "1":
                    MenuMusaitlikSorgula(sistem);
                    break;
                case "2":
                    MenuRezervasyonEkle(sistem);
                    break;
                case "3":
                    MenuRezervasyonIptal(sistem);
                    break;
                case "4":
                    MenuRaporlariGoster(sistem);
                    break;
                case "5":
                    cikis = true;
                    Console.WriteLine("Sistem kapatılıyor. İyi günler dileriz.");
                    break;
                default:
                    Console.WriteLine("HATA: Geçersiz seçim! Lütfen 1 ile 5 arasında bir değer giriniz.");
                    break;
            }
        }
    }

    // --- Menü İşlevleri (UI/UX) ---

    public static void MenuMusaitlikSorgula(KiralamaSistemi sistem)
    {
        Console.WriteLine("\n--- Müsait Araç Sorgulama ---");
        DateTime bas = GecerliTarihGirisiAl("Başlangıç Tarihi (gg.aa.yyyy): ");
        DateTime bit = GecerliTarihGirisiAl("Bitiş Tarihi (gg.aa.yyyy): ");

        if (bas >= bit)
        {
            Console.WriteLine("HATA: Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
            return;
        }

        List<string> musaitler = sistem.MusaitAraclariGetir(bas, bit);

        Console.WriteLine($"\n>> {bas.ToShortDateString()} - {bit.ToShortDateString()} Aralığında Müsait Araçlar:");
        if (musaitler.Any())
        {
            foreach (var plaka in musaitler)
            {
                Console.WriteLine($"- {plaka} (Günlük Fiyat: {sistem.AracGunlukFiyatiniGetir(plaka):C})");
            }
        }
        else
        {
            Console.WriteLine("Bu tarihler arasında müsait araç bulunmamaktadır.");
        }
    }

    public static void MenuRezervasyonEkle(KiralamaSistemi sistem)
    {
        Console.WriteLine("\n--- Yeni Rezervasyon Oluşturma ---");

        Console.Write("Müşteri Adı ve Soyadı: ");
        string musteriAdi = Console.ReadLine();

        Console.Write("Kiralanacak Aracın Plakası: ");
        string plaka = Console.ReadLine().ToUpper();

        DateTime baslangicTarihi = GecerliTarihGirisiAl("Kiralama Başlangıç Tarihi (gg.aa.yyyy): ");
        DateTime bitisTarihi = GecerliTarihGirisiAl("Kiralama Bitiş Tarihi (gg.aa.yyyy): ");

        sistem.RezervasyonEkle(musteriAdi, plaka, baslangicTarihi, bitisTarihi);
    }

    public static void MenuRezervasyonIptal(KiralamaSistemi sistem)
    {
        Console.WriteLine("\n--- Rezervasyon İptali ---");
        Console.Write("İptal edilecek rezervasyonun Plakası: ");
        string plaka = Console.ReadLine().ToUpper();

        sistem.RezervasyonIptal(plaka);
    }

    public static void MenuRaporlariGoster(KiralamaSistemi sistem)
    {
        Console.WriteLine("\n--- Raporlama ve İstatistikler ---");

        // Rapor 1: Toplam Gelir
        Console.WriteLine($"\n1. Toplam Kazanılan Gelir: {sistem.ToplamGelir():C}");

        // Rapor 2: En Çok Kiralanan Araç
        Console.WriteLine($"\n2. En Çok Kiralanan Araç:");
        Console.WriteLine($"- {sistem.EnCokKiralananArac()}");

        // Rapor 3: Müşteri Rezervasyonları
        Console.Write("\n3. Bir Müşterinin Rezervasyonlarını Görüntüle (Müşteri Adı Girin): ");
        string musteriAdi = Console.ReadLine();

        List<string> musteriRezervasyonlari = sistem.MusteriRezervasyonlariniGetir(musteriAdi);
        Console.WriteLine($"\n>> {musteriAdi} Adına Kayıtlı Rezervasyonlar:");
        foreach (var ozet in musteriRezervasyonlari)
        {
            Console.WriteLine($"- {ozet}");
        }
    }
}