# Zen Tetris — Tasarım Dokümanı

**Tarih:** 2026-07-12
**Hedef:** tetr.io'nun Zen modunu temel alan, sonsuz ve baskısız tek kişilik bir Tetris klonu (Unity 6, 2D URP).

## 1. Amaç ve Kapsam

Sadece Zen modu: sonsuz oyun, game over yok, garbage yok, süre baskısı yok. Amaç rahatlatıcı, akıcı bir solo deneyim.

Kapsam dışı (ilk sürüm): ses/müzik, çoklu oyun modları, ayar menüsü, online özellikler, mobil input.

## 2. Oynanış Kuralları

### Board
- 10 sütun × 20 görünür satır; üstte 20 gizli satır (toplam 10×40 grid, guideline standardı).
- Parçalar 21-22. satırlarda (gizli bölge alt kısmı) spawn olur.

### Parçalar ve Randomizer
- 7 standart tetromino. Renkler (tetr.io şeması): I=camgöbeği, O=sarı, T=mor, S=yeşil, Z=kırmızı, J=mavi, L=turuncu.
- **7-bag randomizer:** her 7 parçalık torba karıştırılır, tükenince yenisi.

### Rotasyon
- **SRS (Super Rotation System)** tam kick tablolarıyla (I için ayrı tablo). T-spin'ler doğal olarak mümkün.

### Kontroller (klavye, yeni Input System)
| Aksiyon | Tuş |
|---|---|
| Sola/sağa hareket | ← / → |
| Soft drop | ↓ |
| Hard drop | Space |
| Döndür CW | ↑ veya X |
| Döndür CCW | Z |
| Hold | C veya Shift |
| Pause | Esc |

- **DAS:** 167 ms, **ARR:** 33 ms, **soft drop hızı:** yerçekiminin 20 katı. Hepsi tek bir config sınıfında sabit — kolayca değiştirilebilir.

### Mekanikler
- **Hold:** parça başına 1 kez; panelde soluk gösterim.
- **Next:** 5 parçalık kuyruk.
- **Ghost piece:** yarı saydam, düşüş noktasında.
- **Lock delay:** 500 ms; hareket/rotasyon sıfırlar; parça başına en fazla 15 reset.
- **Yerçekimi:** seviyeyle artan düşüş hızı, üst sınırlı (Zen'de asla oynanamaz hıza çıkmaz). Eğri: `hız = min(baseSpeed * çarpan^seviye, üstSınır)`.

### Skor ve Seviye (Zen tarzı)
- Satır silme puanları (seviye çarpanlı): Single 100, Double 300, Triple 500, Tetris 800.
- **T-spin:** Mini 100/200, T-spin 400/800/1200/1600 (0/1/2/3 satır).
- **Combo:** ardışık silmelerde +50 × combo × seviye.
- **Back-to-Back:** Tetris/T-spin zincirinde ×1.5.
- Soft drop +1/hücre, hard drop +2/hücre.
- **Seviye:** her 10 silinen satırda +1, sonsuz.
- **Kalıcılık:** skor, seviye ve toplam silinen satır PlayerPrefs'e kaydedilir; oyun açılınca kaldığı yerden devam eder (tetr.io Zen hissi). Board durumu kaydedilmez, temiz başlar.

### Top-out
- Spawn engellenirse veya parça tamamen gizli bölgede kilitlenirse: **board temizlenir, skor/seviye korunur, oyun devam eder.** Game over ekranı yoktur.

## 3. Görsel Tasarım

- Ekran düzeni (referans: tetr.io ekran görüntüsü): ortada siyah yarı saydam board (ince grid çizgileri), sol üstte HOLD paneli, sağda NEXT paneli (5 parça), board altında skor ve seviye metni.
- Arkaplan: tam ekran görsel (kullanıcı kendi görselini koyabilir; başlangıçta düz koyu gradient placeholder).
- Bloklar: dolgu rengi + hafif iç çerçeve görünümlü tek sprite (programatik üretilebilir veya basit PNG).
- Ghost: aynı sprite, ~%30 alfa.
- UI metinleri: TextMeshPro.

## 4. Teknik Mimari

**Yaklaşım:** Saf C# çekirdek + ince Unity katmanı (render Tilemap ile).

```
Assets/Scripts/
  Core/                 (UnityEngine referansı YOK — test edilebilir)
    Board.cs            — 10×40 grid, çarpışma, satır silme, top-out temizliği
    Piece.cs            — aktif parça durumu (tip, rotasyon, pozisyon)
    Tetromino.cs        — şekil verileri, spawn ofsetleri, renk indeksleri
    SrsData.cs          — SRS kick tabloları
    BagRandomizer.cs    — 7-bag (System.Random, seed'lenebilir)
    GameState.cs        — tick döngüsü: yerçekimi, lock delay, hold/next, top-out
    ScoreSystem.cs      — satır puanı, combo, B2B, T-spin tespiti (3-köşe kuralı), seviye
    GameConfig.cs       — DAS/ARR, lock delay, yerçekimi eğrisi sabitleri
  Unity/
    GameController.cs   — MonoBehaviour: input'u GameState'e iletir, tick eder
    InputHandler.cs     — yeni Input System ile klavye okuma + DAS/ARR zamanlama
    BoardRenderer.cs    — Tilemap: yerleşik bloklar + aktif parça + ghost
    PiecePreviewUI.cs   — Hold & Next panelleri
    HudUI.cs            — skor/seviye (TMP)
    SaveSystem.cs       — PlayerPrefs okuma/yazma
    SceneBootstrap.cs   — sahneyi koddan kurar (kamera, tilemap, UI) — Inspector bağımlılığı minimum
```

- **Zaman modeli:** `GameState.Tick(deltaTime)` — Update'ten çağrılır; Core kendi zamanlayıcılarını yönetir (deterministik, test edilebilir).
- **Input akışı:** InputHandler ham tuşları DAS/ARR ile aksiyonlara çevirir → GameController → GameState metotları (`MoveLeft`, `RotateCW`, `HardDrop`...).
- **Render akışı:** GameState değişiklik bayrağı/olayı yayınlar → BoardRenderer ve UI günceller.

## 5. Hata Yönetimi

- Core metotları geçersiz hamleyi sessizce reddeder (bool döner); istisna fırlatmaz.
- PlayerPrefs bozuk/eksikse sıfırdan başlar.
- Pause: Esc ile tick durur, yarı saydam "PAUSED" yazısı.

## 6. Test Stratejisi

- **EditMode unit testleri (NUnit, com.unity.test-framework):** Core katmanı için —
  - 7-bag: 7 parçada her tip 1 kez; seed determinizmi.
  - SRS: bilinen kick senaryoları (duvar kick, T-spin triple setup'ı).
  - Board: satır silme, çoklu silme, top-out tespiti ve temizlik.
  - Skor: Tetris, T-spin, combo, B2B kombinasyonları.
  - Lock delay: reset sayacı, 15 sınırı.
- **PlayMode/manuel:** input hissi (DAS/ARR), görsel doğrulama.

## 7. Başarı Kriterleri

- SRS ve 7-bag guideline'a uygun (testlerle kanıtlı).
- Kontroller tetr.io varsayılanlarına yakın hissettirir.
- Top-out'ta kesintisiz devam; skor/seviye oturumlar arası korunur.
- Ekran düzeni referans görüntüye benzer.
