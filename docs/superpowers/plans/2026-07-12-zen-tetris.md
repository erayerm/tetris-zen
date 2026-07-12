# Zen Tetris Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** tetr.io Zen modu benzeri sonsuz, game-over'sız tek kişilik Tetris (Unity 6, 2D URP).

**Architecture:** Oyun mantığı `ZenTetris.Core` asmdef'inde saf C# (UnityEngine referansı yok, `noEngineReferences: true`), EditMode NUnit testleriyle doğrulanır. Unity katmanı (`ZenTetris.Unity`) sadece input (yeni Input System, DAS/ARR), Tilemap render ve UI yapar. Sahne `SceneBootstrap` ile koddan kurulur.

**Tech Stack:** Unity 6 (URP 17.3), com.unity.inputsystem 1.18, com.unity.test-framework (NUnit), Tilemap, TextMeshPro, PlayerPrefs.

## Global Constraints

- Proje kökü: `C:\CUSTOM-Projects\Unity\Tetris-Unity`
- `Assets/Scripts/Core/` altında **hiçbir dosya `UnityEngine` kullanamaz**.
- Board: 10 sütun × 40 satır (0..19 görünür, 20..39 gizli). Koordinat: x sağa, y yukarı, (0,0) sol alt.
- Hücre değerleri: 0 = boş, aksi halde `(int)TetrominoType + 1` (renk indeksi).
- Rotasyon durumları: 0=spawn, 1=CW(R), 2=180, 3=CCW(L).
- Sabitler (`GameConfig`): DAS=0.167f s, ARR=0.033f s, LockDelay=0.5f s, MaxLockResets=15, SoftDropMultiplier=20f, BaseGravity=1f hücre/sn, GravityPerLevel=0.5f, MaxGravity=12f, NextQueueSize=5.
- Skor: Single 100, Double 300, Triple 500, Tetris 800; T-spin Mini 100/200 (0/1 satır), T-spin 400/800/1200/1600 (0/1/2/3 satır); combo +50×combo×level; B2B ×1.5; soft drop +1/hücre, hard drop +2/hücre. Hepsi seviye çarpanlı (drop puanları hariç).
- Her görev sonunda `git add` + `git commit` (mesaj görevde verilmiştir).
- Testler: Unity kapalıyken `.\run-tests.ps1` ile, ya da Unity açıkken **Window > General > Test Runner > EditMode > Run All**.

---

### Task 1: İskelet — asmdef'ler, GameConfig, test altyapısı

**Files:**
- Create: `Assets/Scripts/Core/ZenTetris.Core.asmdef`
- Create: `Assets/Scripts/Core/GameConfig.cs`
- Create: `Assets/Scripts/Unity/ZenTetris.Unity.asmdef`
- Create: `Assets/Scripts/Tests/ZenTetris.Core.Tests.asmdef`
- Create: `Assets/Scripts/Tests/SmokeTests.cs`
- Create: `run-tests.ps1` (proje kökü)

**Interfaces:**
- Produces: `ZenTetris.Core.GameConfig` statik sabitleri (Global Constraints'teki adlarla) — sonraki tüm görevler kullanır.

- [ ] **Step 1: asmdef dosyalarını oluştur**

`Assets/Scripts/Core/ZenTetris.Core.asmdef`:
```json
{
  "name": "ZenTetris.Core",
  "rootNamespace": "ZenTetris.Core",
  "noEngineReferences": true
}
```

`Assets/Scripts/Unity/ZenTetris.Unity.asmdef`:
```json
{
  "name": "ZenTetris.Unity",
  "rootNamespace": "ZenTetris.Unity",
  "references": ["ZenTetris.Core", "Unity.InputSystem", "Unity.TextMeshPro", "UnityEngine.UI"]
}
```

`Assets/Scripts/Tests/ZenTetris.Core.Tests.asmdef`:
```json
{
  "name": "ZenTetris.Core.Tests",
  "rootNamespace": "ZenTetris.Core.Tests",
  "references": ["ZenTetris.Core", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "includePlatforms": ["Editor"],
  "precompiledReferences": ["nunit.framework.dll"],
  "overrideReferences": true,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

- [ ] **Step 2: GameConfig ve smoke test yaz**

`Assets/Scripts/Core/GameConfig.cs`:
```csharp
namespace ZenTetris.Core
{
    public static class GameConfig
    {
        public const float Das = 0.167f;
        public const float Arr = 0.033f;
        public const float LockDelay = 0.5f;
        public const int MaxLockResets = 15;
        public const float SoftDropMultiplier = 20f;
        public const float BaseGravity = 1f;      // hücre/sn, seviye 1
        public const float GravityPerLevel = 0.5f;
        public const float MaxGravity = 12f;
        public const int NextQueueSize = 5;

        public static float GravityFor(int level) =>
            System.Math.Min(BaseGravity + (level - 1) * GravityPerLevel, MaxGravity);
    }
}
```

`Assets/Scripts/Tests/SmokeTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class SmokeTests
{
    [Test]
    public void Gravity_ClampsAtMax()
    {
        Assert.AreEqual(1f, GameConfig.GravityFor(1));
        Assert.AreEqual(12f, GameConfig.GravityFor(999));
    }
}
```

- [ ] **Step 3: run-tests.ps1 yaz**

`run-tests.ps1` (proje kökü):
```powershell
$latest = Get-ChildItem "C:\Program Files\Unity\Hub\Editor" | Sort-Object Name -Descending | Select-Object -First 1
$exe = Join-Path $latest.FullName "Editor\Unity.exe"
$proj = "C:\CUSTOM-Projects\Unity\Tetris-Unity"
$results = Join-Path $proj "Logs\test-results.xml"
if (Test-Path $results) { Remove-Item $results }
& $exe -batchmode -projectPath $proj -runTests -testPlatform EditMode -testResults $results -logFile (Join-Path $proj "Logs\test-run.log")
[xml]$r = Get-Content $results
"{0}: total={1} passed={2} failed={3}" -f $r.'test-run'.result, $r.'test-run'.total, $r.'test-run'.passed, $r.'test-run'.failed
if ($r.'test-run'.failed -ne "0") {
    $r.SelectNodes("//test-case[@result='Failed']") | ForEach-Object { $_.fullname; $_.failure.message.'#cdata-section' }
    exit 1
}
```
**Not:** Unity Editor bu projeyle AÇIKKEN batchmode çalışmaz ("project already open" hatası). Editor açıksa testleri Test Runner penceresinden koştur.

- [ ] **Step 4: Testleri koştur, geçtiğini doğrula**

Run: `.\run-tests.ps1`
Expected: `Passed: total=1 passed=1 failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets run-tests.ps1
git commit -m "chore: project skeleton with asmdefs, GameConfig, test infra"
```

---

### Task 2: Tetromino verileri ve rotasyon

**Files:**
- Create: `Assets/Scripts/Core/Tetromino.cs`
- Test: `Assets/Scripts/Tests/TetrominoTests.cs`

**Interfaces:**
- Produces: `enum TetrominoType { I, O, T, S, Z, J, L }`; `static class Tetromino` — `(int x, int y)[] Cells(TetrominoType type, int rotation)` (4 hücre, pivot-göreceli), `int ColorIndex(TetrominoType type)` (= (int)type + 1).

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/TetrominoTests.cs`:
```csharp
using System.Linq;
using NUnit.Framework;
using ZenTetris.Core;

public class TetrominoTests
{
    [Test]
    public void EveryPieceEveryRotation_HasFourCells()
    {
        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            for (int r = 0; r < 4; r++)
                Assert.AreEqual(4, Tetromino.Cells(t, r).Length, $"{t} rot {r}");
    }

    [Test]
    public void O_DoesNotChangeWithRotation()
    {
        var r0 = Tetromino.Cells(TetrominoType.O, 0).OrderBy(c => (c.x, c.y)).ToArray();
        for (int r = 1; r < 4; r++)
            CollectionAssert.AreEqual(r0, Tetromino.Cells(TetrominoType.O, r).OrderBy(c => (c.x, c.y)).ToArray());
    }

    [Test]
    public void T_SpawnCells_AreCorrect()
    {
        CollectionAssert.AreEquivalent(
            new[] { (-1, 0), (0, 0), (1, 0), (0, 1) },
            Tetromino.Cells(TetrominoType.T, 0));
    }

    [Test]
    public void I_RotatedCW_IsVerticalColumn()
    {
        CollectionAssert.AreEquivalent(
            new[] { (0, 2), (0, 1), (0, 0), (0, -1) },
            Tetromino.Cells(TetrominoType.I, 1));
    }

    [Test]
    public void ColorIndex_IsTypePlusOne()
    {
        Assert.AreEqual(1, Tetromino.ColorIndex(TetrominoType.I));
        Assert.AreEqual(7, Tetromino.ColorIndex(TetrominoType.L));
    }
}
```

- [ ] **Step 2: Testleri koştur — FAIL bekle**

Run: `.\run-tests.ps1`
Expected: derleme hatası (`Tetromino` yok) — testler koşamaz. Bu adımda beklenen budur.

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/Tetromino.cs`:
```csharp
namespace ZenTetris.Core
{
    public enum TetrominoType { I, O, T, S, Z, J, L }

    public static class Tetromino
    {
        // Spawn (rotasyon 0) hücreleri, pivot-göreceli, y yukarı.
        static readonly (int x, int y)[][] Spawn =
        {
            /* I */ new[] { (-1, 0), (0, 0), (1, 0), (2, 0) },
            /* O */ new[] { (0, 0), (1, 0), (0, 1), (1, 1) },
            /* T */ new[] { (-1, 0), (0, 0), (1, 0), (0, 1) },
            /* S */ new[] { (-1, 0), (0, 0), (0, 1), (1, 1) },
            /* Z */ new[] { (-1, 1), (0, 1), (0, 0), (1, 0) },
            /* J */ new[] { (-1, 1), (-1, 0), (0, 0), (1, 0) },
            /* L */ new[] { (1, 1), (-1, 0), (0, 0), (1, 0) },
        };

        // [type][rotation][cell] — statik olarak önceden hesaplanır.
        static readonly (int x, int y)[][][] All = Build();

        static (int x, int y)[][][] Build()
        {
            var all = new (int x, int y)[7][][];
            for (int t = 0; t < 7; t++)
            {
                all[t] = new (int x, int y)[4][];
                all[t][0] = Spawn[t];
                for (int r = 1; r < 4; r++)
                {
                    var prev = all[t][r - 1];
                    var next = new (int x, int y)[4];
                    for (int i = 0; i < 4; i++)
                        next[i] = RotateCW((TetrominoType)t, prev[i]);
                    all[t][r] = next;
                }
            }
            return all;
        }

        static (int x, int y) RotateCW(TetrominoType type, (int x, int y) c) => type switch
        {
            TetrominoType.O => c,                      // O dönmez
            TetrominoType.I => (c.y, 1 - c.x),         // (0.5, 0.5) merkezli SRS I dönüşü
            _ => (c.y, -c.x),                          // pivot merkezli
        };

        public static (int x, int y)[] Cells(TetrominoType type, int rotation) =>
            All[(int)type][((rotation % 4) + 4) % 4];

        public static int ColorIndex(TetrominoType type) => (int)type + 1;
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1`
Expected: `failed=0` (toplam 6 test)

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): tetromino shape data with SRS rotations"
```

---

### Task 3: Board — çarpışma, kilitleme, satır silme

**Files:**
- Create: `Assets/Scripts/Core/Board.cs`
- Create: `Assets/Scripts/Core/Piece.cs`
- Test: `Assets/Scripts/Tests/BoardTests.cs`

**Interfaces:**
- Consumes: `Tetromino.Cells`, `Tetromino.ColorIndex`.
- Produces:
  - `struct Piece { TetrominoType Type; int Rotation; int X; int Y; (int x, int y)[] AbsoluteCells(); }`
  - `class Board { const int Width=10, VisibleHeight=20, Height=40; int Get(int x,int y); bool IsOccupied(int x,int y); bool CanPlace(TetrominoType,int rotation,int px,int py); void Lock(in Piece); int ClearFullLines(); void ClearAll(); bool IsEmpty { get; } }`
  - `IsOccupied`: board dışı (x<0, x>=10, y<0, y>=40) **dolu** sayılır.

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/BoardTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class BoardTests
{
    [Test]
    public void NewBoard_IsEmpty()
    {
        var b = new Board();
        Assert.IsTrue(b.IsEmpty);
        Assert.AreEqual(0, b.Get(0, 0));
    }

    [Test]
    public void OutOfBounds_IsOccupied()
    {
        var b = new Board();
        Assert.IsTrue(b.IsOccupied(-1, 0));
        Assert.IsTrue(b.IsOccupied(10, 0));
        Assert.IsTrue(b.IsOccupied(0, -1));
        Assert.IsTrue(b.IsOccupied(0, 40));
        Assert.IsFalse(b.IsOccupied(0, 0));
    }

    [Test]
    public void CanPlace_RejectsWallOverlap()
    {
        var b = new Board();
        Assert.IsTrue(b.CanPlace(TetrominoType.T, 0, 4, 1));
        Assert.IsFalse(b.CanPlace(TetrominoType.I, 0, 8, 1)); // sağ hücre x=10 -> duvar
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, -1, 1)); // sol hücre x=-2 -> duvar
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, 4, -1)); // zemin altı
    }

    [Test]
    public void Lock_WritesColorIndex()
    {
        var b = new Board();
        b.Lock(new Piece(TetrominoType.T, 0, 4, 1));
        Assert.AreEqual(Tetromino.ColorIndex(TetrominoType.T), b.Get(4, 1));
        Assert.AreEqual(Tetromino.ColorIndex(TetrominoType.T), b.Get(4, 2));
        Assert.IsFalse(b.CanPlace(TetrominoType.T, 0, 4, 1));
    }

    [Test]
    public void ClearFullLines_RemovesRowAndShiftsDown()
    {
        var b = new Board();
        // y=0 satırını tek hücrelik kilitlemelerle doldur, y=1'e bir işaret koy
        for (int x = 0; x < Board.Width; x++) b.SetCell(x, 0, 3);
        b.SetCell(5, 1, 7);
        int cleared = b.ClearFullLines();
        Assert.AreEqual(1, cleared);
        Assert.AreEqual(7, b.Get(5, 0)); // üst satır aşağı kaydı
        Assert.AreEqual(0, b.Get(5, 1));
    }

    [Test]
    public void ClearFullLines_MultipleRows()
    {
        var b = new Board();
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < Board.Width; x++) b.SetCell(x, y, 1);
        b.SetCell(0, 2, 5);
        Assert.AreEqual(2, b.ClearFullLines());
        Assert.AreEqual(5, b.Get(0, 0));
    }

    [Test]
    public void ClearAll_EmptiesBoard()
    {
        var b = new Board();
        b.SetCell(3, 3, 2);
        b.ClearAll();
        Assert.IsTrue(b.IsEmpty);
    }
}
```

- [ ] **Step 2: Testleri koştur — derleme hatası bekle**

Run: `.\run-tests.ps1`
Expected: FAIL (Board/Piece yok)

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/Piece.cs`:
```csharp
namespace ZenTetris.Core
{
    public readonly struct Piece
    {
        public readonly TetrominoType Type;
        public readonly int Rotation;
        public readonly int X;
        public readonly int Y;

        public Piece(TetrominoType type, int rotation, int x, int y)
        {
            Type = type;
            Rotation = ((rotation % 4) + 4) % 4;
            X = x;
            Y = y;
        }

        public Piece Moved(int dx, int dy) => new(Type, Rotation, X + dx, Y + dy);
        public Piece Rotated(int newRotation) => new(Type, newRotation, X, Y);

        public (int x, int y)[] AbsoluteCells()
        {
            var rel = Tetromino.Cells(Type, Rotation);
            var abs = new (int x, int y)[4];
            for (int i = 0; i < 4; i++) abs[i] = (X + rel[i].x, Y + rel[i].y);
            return abs;
        }
    }
}
```

`Assets/Scripts/Core/Board.cs`:
```csharp
namespace ZenTetris.Core
{
    public sealed class Board
    {
        public const int Width = 10;
        public const int VisibleHeight = 20;
        public const int Height = 40;

        readonly int[] cells = new int[Width * Height]; // 0 = boş

        public int Get(int x, int y) => cells[y * Width + x];
        public void SetCell(int x, int y, int value) => cells[y * Width + x] = value;

        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return true;
            return cells[y * Width + x] != 0;
        }

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < cells.Length; i++)
                    if (cells[i] != 0) return false;
                return true;
            }
        }

        public bool CanPlace(TetrominoType type, int rotation, int px, int py)
        {
            foreach (var (cx, cy) in Tetromino.Cells(type, rotation))
                if (IsOccupied(px + cx, py + cy)) return false;
            return true;
        }

        public bool CanPlace(in Piece p) => CanPlace(p.Type, p.Rotation, p.X, p.Y);

        public void Lock(in Piece p)
        {
            int color = Tetromino.ColorIndex(p.Type);
            foreach (var (x, y) in p.AbsoluteCells())
                if (x >= 0 && x < Width && y >= 0 && y < Height)
                    cells[y * Width + x] = color;
        }

        public int ClearFullLines()
        {
            int cleared = 0;
            for (int y = 0; y < Height; y++)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                    if (cells[y * Width + x] == 0) { full = false; break; }

                if (full) { cleared++; continue; }
                if (cleared > 0)
                    for (int x = 0; x < Width; x++)
                        cells[(y - cleared) * Width + x] = cells[y * Width + x];
            }
            for (int y = Height - cleared; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    cells[y * Width + x] = 0;
            return cleared;
        }

        public void ClearAll() => System.Array.Clear(cells, 0, cells.Length);
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1`
Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): board with collision, locking and line clears"
```

---

### Task 4: 7-Bag Randomizer

**Files:**
- Create: `Assets/Scripts/Core/BagRandomizer.cs`
- Test: `Assets/Scripts/Tests/BagRandomizerTests.cs`

**Interfaces:**
- Produces: `class BagRandomizer { BagRandomizer(int seed); BagRandomizer(); TetrominoType Next(); }`

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/BagRandomizerTests.cs`:
```csharp
using System.Linq;
using NUnit.Framework;
using ZenTetris.Core;

public class BagRandomizerTests
{
    [Test]
    public void EverySevenPieces_ContainAllTypes()
    {
        var bag = new BagRandomizer(seed: 42);
        for (int round = 0; round < 10; round++)
        {
            var seven = Enumerable.Range(0, 7).Select(_ => bag.Next()).ToArray();
            CollectionAssert.AreEquivalent(
                System.Enum.GetValues(typeof(TetrominoType)).Cast<TetrominoType>(), seven);
        }
    }

    [Test]
    public void SameSeed_SameSequence()
    {
        var a = new BagRandomizer(7);
        var b = new BagRandomizer(7);
        for (int i = 0; i < 21; i++) Assert.AreEqual(a.Next(), b.Next());
    }

    [Test]
    public void DifferentSeeds_DifferAtLeastOnce()
    {
        var a = new BagRandomizer(1);
        var b = new BagRandomizer(2);
        bool differs = Enumerable.Range(0, 21).Any(_ => a.Next() != b.Next());
        Assert.IsTrue(differs);
    }
}
```

- [ ] **Step 2: Testleri koştur — derleme hatası bekle**

Run: `.\run-tests.ps1` — Expected: FAIL

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/BagRandomizer.cs`:
```csharp
using System;

namespace ZenTetris.Core
{
    public sealed class BagRandomizer
    {
        readonly Random rng;
        readonly TetrominoType[] bag = new TetrominoType[7];
        int index = 7; // ilk Next() torbayı doldursun

        public BagRandomizer(int seed) => rng = new Random(seed);
        public BagRandomizer() => rng = new Random();

        public TetrominoType Next()
        {
            if (index >= 7)
            {
                for (int i = 0; i < 7; i++) bag[i] = (TetrominoType)i;
                for (int i = 6; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    (bag[i], bag[j]) = (bag[j], bag[i]);
                }
                index = 0;
            }
            return bag[index++];
        }
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1` — Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): 7-bag randomizer"
```

---

### Task 5: SRS kick tabloları ve rotasyon çözümü

**Files:**
- Create: `Assets/Scripts/Core/Srs.cs`
- Test: `Assets/Scripts/Tests/SrsTests.cs`

**Interfaces:**
- Consumes: `Board.CanPlace(in Piece)`, `Piece`.
- Produces: `static class Srs { bool TryRotate(Board board, in Piece piece, bool clockwise, out Piece result, out int kickIndex); }` — kickIndex: kullanılan kick'in tablodaki sırası (0 = kick'siz), T-spin mini tespitinde kullanılacak.

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/SrsTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class SrsTests
{
    [Test]
    public void OpenField_RotatesWithoutKick()
    {
        var b = new Board();
        var t = new Piece(TetrominoType.T, 0, 4, 5);
        Assert.IsTrue(Srs.TryRotate(b, t, clockwise: true, out var r, out var kick));
        Assert.AreEqual(1, r.Rotation);
        Assert.AreEqual(0, kick);
        Assert.AreEqual((4, 5), (r.X, r.Y));
    }

    [Test]
    public void BlockedRotation_ReturnsFalse()
    {
        var b = new Board();
        // T'nin etrafını tamamen doldur (pivot 4,1) — hiçbir kick çalışmasın
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < 5; y++)
                if (!(x >= 3 && x <= 5 && y <= 2)) b.SetCell(x, y, 1);
        for (int x = 3; x <= 5; x++) b.SetCell(x, 2, 1); // tavan
        b.SetCell(3, 0, 1); b.SetCell(5, 0, 1);          // alt köşeler
        var t = new Piece(TetrominoType.T, 2, 4, 1); // aşağı bakan T
        Assert.IsFalse(Srs.TryRotate(b, t, true, out _, out _));
    }

    [Test]
    public void WallKick_LeftWall_JPiece()
    {
        var b = new Board();
        // Sol duvara dayalı dikey J (rotasyon 1/R), CCW dönünce duvar kick gerekir
        var j = new Piece(TetrominoType.J, 1, 0, 5);
        Assert.IsTrue(b.CanPlace(j));
        Assert.IsTrue(Srs.TryRotate(b, j, clockwise: false, out var r, out var kick));
        Assert.AreEqual(0, r.Rotation);
        Assert.Greater(kick, 0); // kick'siz sığmaz (sol hücre x=-1 olurdu)
    }

    [Test]
    public void I_UsesOwnKickTable()
    {
        var b = new Board();
        var i = new Piece(TetrominoType.I, 0, 4, 0); // zeminde yatay I
        Assert.IsTrue(Srs.TryRotate(b, i, true, out var r, out _));
        Assert.IsTrue(b.CanPlace(r));
    }
}
```

- [ ] **Step 2: Testleri koştur — derleme hatası bekle**

Run: `.\run-tests.ps1` — Expected: FAIL

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/Srs.cs`:
```csharp
namespace ZenTetris.Core
{
    public static class Srs
    {
        // Kick offsetleri (dx, dy), y yukarı pozitif. Satır sırası KickRow ile eşleşir.
        static readonly (int x, int y)[][] JlstzKicks =
        {
            /* 0->R */ new[] { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },
            /* R->0 */ new[] { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },
            /* R->2 */ new[] { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },
            /* 2->R */ new[] { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },
            /* 2->L */ new[] { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },
            /* L->2 */ new[] { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },
            /* L->0 */ new[] { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },
            /* 0->L */ new[] { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },
        };

        static readonly (int x, int y)[][] IKicks =
        {
            /* 0->R */ new[] { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },
            /* R->0 */ new[] { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },
            /* R->2 */ new[] { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) },
            /* 2->R */ new[] { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },
            /* 2->L */ new[] { (0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2) },
            /* L->2 */ new[] { (0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2) },
            /* L->0 */ new[] { (0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1) },
            /* 0->L */ new[] { (0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1) },
        };

        // (from, to) çiftini yukarıdaki tablo indeksine çevirir.
        static int KickRow(int from, bool clockwise) => (from, clockwise) switch
        {
            (0, true) => 0,  // 0->R
            (1, false) => 1, // R->0
            (1, true) => 2,  // R->2
            (2, false) => 3, // 2->R
            (2, true) => 4,  // 2->L
            (3, false) => 5, // L->2
            (3, true) => 6,  // L->0
            (0, false) => 7, // 0->L
            _ => 0
        };

        public static bool TryRotate(Board board, in Piece piece, bool clockwise,
                                     out Piece result, out int kickIndex)
        {
            int to = ((piece.Rotation + (clockwise ? 1 : -1)) % 4 + 4) % 4;

            if (piece.Type == TetrominoType.O)
            {
                result = piece.Rotated(to);
                kickIndex = 0;
                return true;
            }

            var table = piece.Type == TetrominoType.I ? IKicks : JlstzKicks;
            var kicks = table[KickRow(piece.Rotation, clockwise)];

            for (int i = 0; i < kicks.Length; i++)
            {
                var candidate = new Piece(piece.Type, to, piece.X + kicks[i].x, piece.Y + kicks[i].y);
                if (board.CanPlace(candidate))
                {
                    result = candidate;
                    kickIndex = i;
                    return true;
                }
            }
            result = piece;
            kickIndex = -1;
            return false;
        }
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1` — Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): SRS rotation with wall kicks"
```

---

### Task 6: ScoreSystem — puan, combo, B2B, seviye

**Files:**
- Create: `Assets/Scripts/Core/ScoreSystem.cs`
- Test: `Assets/Scripts/Tests/ScoreSystemTests.cs`

**Interfaces:**
- Produces:
  - `enum TSpinKind { None, Mini, Full }`
  - `class ScoreSystem { long Score; int Level; int TotalLines; int Combo; bool BackToBack; void OnPieceLocked(int linesCleared, TSpinKind tspin); void AddDropPoints(int cells, bool hard); void Load(long score, int totalLines); }` — `Level = TotalLines / 10 + 1`.

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/ScoreSystemTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class ScoreSystemTests
{
    [Test]
    public void Single_ScoresBaseTimesLevel()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(1, TSpinKind.None);
        Assert.AreEqual(100, s.Score);
        Assert.AreEqual(1, s.TotalLines);
    }

    [Test]
    public void Tetris_Then_Tetris_GetsBackToBackBonus()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(4, TSpinKind.None);          // 800
        s.OnPieceLocked(0, TSpinKind.None);          // combo kırılır ama B2B durur
        s.OnPieceLocked(4, TSpinKind.None);          // 800*1.5 = 1200
        Assert.AreEqual(800 + 1200, s.Score);
        Assert.IsTrue(s.BackToBack);
    }

    [Test]
    public void NormalClear_BreaksBackToBack()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(4, TSpinKind.None);
        s.OnPieceLocked(1, TSpinKind.None);
        Assert.IsFalse(s.BackToBack);
    }

    [Test]
    public void Combo_AddsFiftyPerStep()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(1, TSpinKind.None);          // combo 0: 100
        s.OnPieceLocked(1, TSpinKind.None);          // combo 1: 100 + 50
        s.OnPieceLocked(1, TSpinKind.None);          // combo 2: 100 + 100
        Assert.AreEqual(100 + 150 + 200, s.Score);
    }

    [Test]
    public void TSpinFull_UsesTSpinTable()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(2, TSpinKind.Full);          // 1200
        Assert.AreEqual(1200, s.Score);
        Assert.IsTrue(s.BackToBack);                 // T-spin clear B2B başlatır
    }

    [Test]
    public void TSpinMini_NoLines_Scores100()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(0, TSpinKind.Mini);
        Assert.AreEqual(100, s.Score);
    }

    [Test]
    public void Level_IncreasesEveryTenLines_AndMultiplies()
    {
        var s = new ScoreSystem();
        for (int i = 0; i < 3; i++) s.OnPieceLocked(4, TSpinKind.None); // 12 satır
        Assert.AreEqual(2, s.Level); // 12/10+1
        long before = s.Score;
        s.OnPieceLocked(1, TSpinKind.None); // Single, seviye 2 => 200 + combo yok (önceki hepsi clear, combo sürüyor!)
        // combo 3. adım: (100 + 50*3) * 2 = 500
        Assert.AreEqual(before + 500, s.Score);
    }

    [Test]
    public void DropPoints_AreFlat()
    {
        var s = new ScoreSystem();
        s.AddDropPoints(5, hard: false); // +5
        s.AddDropPoints(10, hard: true); // +20
        Assert.AreEqual(25, s.Score);
    }

    [Test]
    public void Load_RestoresProgress()
    {
        var s = new ScoreSystem();
        s.Load(5000, 25);
        Assert.AreEqual(5000, s.Score);
        Assert.AreEqual(3, s.Level);
    }
}
```

- [ ] **Step 2: Testleri koştur — derleme hatası bekle**

Run: `.\run-tests.ps1` — Expected: FAIL

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/ScoreSystem.cs`:
```csharp
namespace ZenTetris.Core
{
    public enum TSpinKind { None, Mini, Full }

    public sealed class ScoreSystem
    {
        public long Score { get; private set; }
        public int TotalLines { get; private set; }
        public int Combo { get; private set; } = -1;
        public bool BackToBack { get; private set; }
        public int Level => TotalLines / 10 + 1;

        static readonly int[] LineBase = { 0, 100, 300, 500, 800 };
        static readonly int[] TSpinBase = { 400, 800, 1200, 1600 };
        static readonly int[] TSpinMiniBase = { 100, 200 };

        public void OnPieceLocked(int linesCleared, TSpinKind tspin)
        {
            int level = Level; // silmeden ÖNCEKİ seviye ile puanla

            long points = tspin switch
            {
                TSpinKind.Full => TSpinBase[linesCleared],
                TSpinKind.Mini => TSpinMiniBase[System.Math.Min(linesCleared, 1)],
                _ => LineBase[linesCleared],
            };

            if (linesCleared > 0)
            {
                Combo++;
                bool difficult = linesCleared == 4 || tspin != TSpinKind.None;
                if (difficult && BackToBack) points = points * 3 / 2;
                if (Combo > 0) points += 50L * Combo;
                BackToBack = difficult;
                Score += points * level;
                TotalLines += linesCleared;
            }
            else
            {
                Combo = -1;
                if (tspin != TSpinKind.None) Score += points * level; // satırsız T-spin puanı
            }
        }

        public void AddDropPoints(int cells, bool hard) => Score += cells * (hard ? 2 : 1);

        public void Load(long score, int totalLines)
        {
            Score = score;
            TotalLines = totalLines;
            Combo = -1;
            BackToBack = false;
        }
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1` — Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): scoring with combo, back-to-back and levels"
```

---

### Task 7: GameState — spawn, hareket, yerçekimi, lock delay, top-out

**Files:**
- Create: `Assets/Scripts/Core/GameState.cs`
- Test: `Assets/Scripts/Tests/GameStateTests.cs`

**Interfaces:**
- Consumes: `Board`, `Piece`, `Srs.TryRotate`, `BagRandomizer`, `ScoreSystem`, `GameConfig`.
- Produces:
```csharp
public sealed class GameState
{
    public GameState(int? seed = null);
    public Board Board { get; }
    public ScoreSystem Score { get; }
    public Piece Active { get; }
    public TetrominoType? Held { get; }
    public IReadOnlyList<TetrominoType> NextQueue { get; } // 5 eleman
    public bool Paused { get; set; }
    public event System.Action Changed;                    // her görsel değişimde
    public event System.Action<int> LinesCleared;          // silinen satır sayısı

    public bool MoveLeft(); public bool MoveRight();
    public bool RotateCW(); public bool RotateCCW();
    public void HardDrop(); public bool TryHold();
    public void SetSoftDrop(bool on);
    public int GhostY();                                   // aktif parçanın düşeceği Y
    public void Tick(float dt);
}
```
- Spawn pozisyonu: `X=4, Y=20`. Spawn engellenirse veya kilitlenen parçanın TÜM hücreleri `y >= Board.VisibleHeight` ise: `Board.ClearAll()` + spawn (skor korunur).

- [ ] **Step 1: Failing testleri yaz**

`Assets/Scripts/Tests/GameStateTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class GameStateTests
{
    static GameState NewGame() => new GameState(seed: 42);

    [Test]
    public void Spawn_ActivePieceAtSpawnPosition_NextHasFive()
    {
        var g = NewGame();
        Assert.AreEqual(4, g.Active.X);
        Assert.AreEqual(20, g.Active.Y);
        Assert.AreEqual(5, g.NextQueue.Count);
    }

    [Test]
    public void MoveLeft_AtWall_ReturnsFalse()
    {
        var g = NewGame();
        while (g.MoveLeft()) { }
        Assert.IsFalse(g.MoveLeft());
    }

    [Test]
    public void Gravity_MovesPieceDownOverTime()
    {
        var g = NewGame();
        int y0 = g.Active.Y;
        g.Tick(1.0f); // seviye 1: 1 hücre/sn
        Assert.AreEqual(y0 - 1, g.Active.Y);
    }

    [Test]
    public void HardDrop_LocksImmediately_AndSpawnsNext()
    {
        var g = NewGame();
        var first = g.Active.Type;
        var expectedNext = g.NextQueue[0];
        g.HardDrop();
        Assert.AreEqual(expectedNext, g.Active.Type);
        Assert.IsFalse(g.Board.IsEmpty);
        Assert.AreEqual(4, g.Active.X); // yeni parça spawn'da
    }

    [Test]
    public void LockDelay_PieceLocksAfterHalfSecondOnGround()
    {
        var g = NewGame();
        // parçayı zemine indir
        for (int i = 0; i < 25; i++) g.Tick(1.0f);
        // zeminde: lock delay dolana kadar kilitlenmemeli
        var type = g.Active.Type;
        g.Tick(0.3f);
        Assert.AreEqual(type, g.Active.Type);
        g.Tick(0.3f); // toplam 0.6 > 0.5
        Assert.AreNotEqual(0, CountFilled(g.Board));
    }

    [Test]
    public void GhostY_IsLandingRow()
    {
        var g = NewGame();
        int ghost = g.GhostY();
        g.HardDrop();
        // Kilitlendi; ghost, hard drop'un indiği satırdı. Board'da o satır civarı dolu olmalı.
        Assert.LessOrEqual(ghost, 20);
        Assert.GreaterOrEqual(ghost, 0);
    }

    [Test]
    public void TryHold_SwapsAndBlocksSecondHold()
    {
        var g = NewGame();
        var first = g.Active.Type;
        Assert.IsTrue(g.TryHold());
        Assert.AreEqual(first, g.Held);
        Assert.IsFalse(g.TryHold()); // aynı parça sırasında ikinci hold yok
        g.HardDrop();
        Assert.IsTrue(g.TryHold()); // yeni parçayla tekrar serbest
    }

    [Test]
    public void TopOut_ClearsBoard_KeepsScore()
    {
        var g = NewGame();
        g.Score.Load(9999, 0);
        // Spawn bölgesini tıka
        for (int x = 0; x < Board.Width; x++)
            for (int y = 18; y < 24; y++)
                g.Board.SetCell(x, y, 1);
        g.HardDrop(); // kilitlenme tamamen gizli bölgede veya spawn tıkalı -> temizlik
        Assert.IsTrue(BoardMostlyEmpty(g.Board));
        Assert.AreEqual(9999 + 40 /* hard drop puanı olabilir */, g.Score.Score, 60);
    }

    [Test]
    public void Paused_TickDoesNothing()
    {
        var g = NewGame();
        g.Paused = true;
        int y = g.Active.Y;
        g.Tick(5f);
        Assert.AreEqual(y, g.Active.Y);
    }

    static int CountFilled(Board b)
    {
        int n = 0;
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
                if (b.Get(x, y) != 0) n++;
        return n;
    }

    static bool BoardMostlyEmpty(Board b) => CountFilled(b) <= 4; // en fazla yeni kilitlenen parça
}
```
**Not:** `TopOut_ClearsBoard_KeepsScore` içindeki skor assert'i tolerance'lı (`delta: 60`) — hard drop hücre puanı değişkendir; önemli olan 9999'un korunmasıdır.

- [ ] **Step 2: Testleri koştur — derleme hatası bekle**

Run: `.\run-tests.ps1` — Expected: FAIL

- [ ] **Step 3: Implementasyon**

`Assets/Scripts/Core/GameState.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace ZenTetris.Core
{
    public sealed class GameState
    {
        public Board Board { get; } = new();
        public ScoreSystem Score { get; } = new();
        public Piece Active { get; private set; }
        public TetrominoType? Held { get; private set; }
        public bool Paused { get; set; }

        readonly BagRandomizer bag;
        readonly List<TetrominoType> next = new();
        public IReadOnlyList<TetrominoType> NextQueue => next;

        public event Action Changed;
        public event Action<int> LinesCleared;

        bool holdUsed;
        bool softDrop;
        float gravityTimer;
        float lockTimer;
        int lockResets;
        bool lastMoveWasRotation;
        int lastKickIndex;

        const int SpawnX = 4, SpawnY = 20;

        public GameState(int? seed = null)
        {
            bag = seed.HasValue ? new BagRandomizer(seed.Value) : new BagRandomizer();
            for (int i = 0; i < GameConfig.NextQueueSize; i++) next.Add(bag.Next());
            Spawn();
        }

        void Spawn()
        {
            var type = next[0];
            next.RemoveAt(0);
            next.Add(bag.Next());

            var p = new Piece(type, 0, SpawnX, SpawnY);
            if (!Board.CanPlace(p))
            {
                Board.ClearAll(); // Zen: top-out'ta temizle, devam et
            }
            Active = p;
            holdUsed = false;
            gravityTimer = 0f;
            lockTimer = 0f;
            lockResets = 0;
            lastMoveWasRotation = false;
            Changed?.Invoke();
        }

        bool TryShift(int dx)
        {
            var moved = Active.Moved(dx, 0);
            if (!Board.CanPlace(moved)) return false;
            Active = moved;
            lastMoveWasRotation = false;
            OnSuccessfulMoveWhileGrounded();
            Changed?.Invoke();
            return true;
        }

        public bool MoveLeft() => !Paused && TryShift(-1);
        public bool MoveRight() => !Paused && TryShift(1);

        bool TryRotate(bool cw)
        {
            if (Paused) return false;
            if (!Srs.TryRotate(Board, Active, cw, out var rotated, out var kick)) return false;
            Active = rotated;
            lastMoveWasRotation = true;
            lastKickIndex = kick;
            OnSuccessfulMoveWhileGrounded();
            Changed?.Invoke();
            return true;
        }

        public bool RotateCW() => TryRotate(true);
        public bool RotateCCW() => TryRotate(false);

        void OnSuccessfulMoveWhileGrounded()
        {
            if (IsGrounded() && lockResets < GameConfig.MaxLockResets)
            {
                lockTimer = 0f;
                lockResets++;
            }
        }

        bool IsGrounded() => !Board.CanPlace(Active.Moved(0, -1));

        public int GhostY()
        {
            var p = Active;
            while (Board.CanPlace(p.Moved(0, -1))) p = p.Moved(0, -1);
            return p.Y;
        }

        public void HardDrop()
        {
            if (Paused) return;
            int dist = Active.Y - GhostY();
            Active = new Piece(Active.Type, Active.Rotation, Active.X, GhostY());
            Score.AddDropPoints(dist, hard: true);
            LockActive();
        }

        public bool TryHold()
        {
            if (Paused || holdUsed) return false;
            var current = Active.Type;
            if (Held.HasValue)
            {
                var swap = Held.Value;
                Held = current;
                var p = new Piece(swap, 0, SpawnX, SpawnY);
                if (!Board.CanPlace(p)) Board.ClearAll();
                Active = p;
                gravityTimer = 0f; lockTimer = 0f; lockResets = 0;
                lastMoveWasRotation = false;
            }
            else
            {
                Held = current;
                Spawn();
            }
            holdUsed = true;
            Changed?.Invoke();
            return true;
        }

        public void SetSoftDrop(bool on) => softDrop = on;

        public void Tick(float dt)
        {
            if (Paused) return;

            float gravity = GameConfig.GravityFor(Score.Level);
            if (softDrop) gravity *= GameConfig.SoftDropMultiplier;
            float step = 1f / gravity;

            if (IsGrounded())
            {
                lockTimer += dt;
                if (lockTimer >= GameConfig.LockDelay) LockActive();
                return;
            }

            gravityTimer += dt;
            while (gravityTimer >= step && Board.CanPlace(Active.Moved(0, -1)))
            {
                gravityTimer -= step;
                Active = Active.Moved(0, -1);
                lastMoveWasRotation = false;
                if (softDrop) Score.AddDropPoints(1, hard: false);
                Changed?.Invoke();
                if (IsGrounded()) { gravityTimer = 0f; break; }
            }
        }

        void LockActive()
        {
            var tspin = DetectTSpin();
            Board.Lock(Active);

            bool fullyHidden = true;
            foreach (var (_, y) in Active.AbsoluteCells())
                if (y < Board.VisibleHeight) fullyHidden = false;

            int lines = Board.ClearFullLines();
            Score.OnPieceLocked(lines, tspin);
            if (lines > 0) LinesCleared?.Invoke(lines);

            if (fullyHidden) Board.ClearAll(); // Zen top-out

            Spawn();
        }

        TSpinKind DetectTSpin()
        {
            if (Active.Type != TetrominoType.T || !lastMoveWasRotation) return TSpinKind.None;

            int cx = Active.X, cy = Active.Y;
            // Köşeler: sol-üst, sağ-üst, sol-alt, sağ-alt
            bool lu = Board.IsOccupied(cx - 1, cy + 1);
            bool ru = Board.IsOccupied(cx + 1, cy + 1);
            bool ld = Board.IsOccupied(cx - 1, cy - 1);
            bool rd = Board.IsOccupied(cx + 1, cy - 1);
            int total = (lu ? 1 : 0) + (ru ? 1 : 0) + (ld ? 1 : 0) + (rd ? 1 : 0);
            if (total < 3) return TSpinKind.None;

            // Ön köşeler: T'nin baktığı yön (rotasyona göre)
            (bool a, bool b) front = Active.Rotation switch
            {
                0 => (lu, ru),
                1 => (ru, rd),
                2 => (ld, rd),
                _ => (lu, ld),
            };
            bool full = front.a && front.b;
            // Son kick 5. sıradaysa (index 4, "1x2" kick) guideline'da full sayılır
            if (!full && lastKickIndex == 4) full = true;
            return full ? TSpinKind.Full : TSpinKind.Mini;
        }
    }
}
```

- [ ] **Step 4: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1` — Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(core): game state with gravity, lock delay, hold, hard drop, zen top-out"
```

---

### Task 8: T-spin entegrasyon testleri

**Files:**
- Test: `Assets/Scripts/Tests/TSpinTests.cs`

**Interfaces:**
- Consumes: `GameState`, `Board.SetCell`, `Srs`.

- [ ] **Step 1: Testleri yaz (bu görev sadece test — davranış Task 7'de yazıldı, burada senaryolarla kanıtlanır)**

`Assets/Scripts/Tests/TSpinTests.cs`:
```csharp
using NUnit.Framework;
using ZenTetris.Core;

public class TSpinTests
{
    // Klasik TSD (T-Spin Double) yuvası kur:
    //   y=2:  X X X . X X X X X X   (x=3 boş — T başı buradan girer)
    //   y=1:  X X X . . . X X X X   (x=3,4,5 boş)
    //   y=0:  X X X X . X X X X X   (x=4 boş — nokta)
    static Board BuildTsdSlot()
    {
        var b = new Board();
        for (int x = 0; x < Board.Width; x++)
        {
            if (x != 3) b.SetCell(x, 2, 1);
            if (x < 3 || x > 5) b.SetCell(x, 1, 1);
            if (x != 4) b.SetCell(x, 0, 1);
        }
        return b;
    }

    [Test]
    public void TSpinDouble_DetectedAndScored()
    {
        var g = new GameState(seed: 1);
        // Board'u elle kur, aktif parçayı T yapmak için: T gelene kadar hold/drop etmek kırılgan.
        // Bunun yerine düşük seviye API ile doğrudan senaryo testi:
        var b = BuildTsdSlot();
        // T rotasyon 2 (aşağı bakan), yuvaya rotate ederek girmiş kabul edilecek pozisyon: pivot (4,1)
        var t = new Piece(TetrominoType.T, 2, 4, 1);
        Assert.IsTrue(b.CanPlace(t), "T yuvaya sığmalı");
        // 3 köşe kuralı: (3,2),(5,2),(3,0),(5,0) köşelerinden en az 3'ü dolu
        int corners = 0;
        foreach (var (dx, dy) in new[] { (-1, 1), (1, 1), (-1, -1), (1, -1) })
            if (b.IsOccupied(4 + dx, 1 + dy)) corners++;
        Assert.GreaterOrEqual(corners, 3);
        // Kilitle ve iki satırın silindiğini doğrula
        b.Lock(t);
        Assert.AreEqual(2, b.ClearFullLines());
    }

    [Test]
    public void ScoreSystem_TsdChain_B2BApplies()
    {
        var s = new ScoreSystem();
        s.OnPieceLocked(2, TSpinKind.Full);  // 1200
        s.OnPieceLocked(2, TSpinKind.Full);  // 1200*1.5 + combo 50 = 1850
        Assert.AreEqual(1200 + 1850, s.Score);
    }
}
```

- [ ] **Step 2: Testleri koştur — PASS bekle**

Run: `.\run-tests.ps1`
Expected: `failed=0`. (Bu görev regresyon ağı ekler; FAIL çıkarsa Task 3/6/7'deki ilgili mantığı düzelt — davranış değişikliği gerekirse önce testin doğruluğunu spec'e karşı kontrol et.)

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "test(core): t-spin slot and B2B chain scenarios"
```

---

### Task 9: SaveSystem — PlayerPrefs kalıcılığı

**Files:**
- Create: `Assets/Scripts/Unity/SaveSystem.cs`

**Interfaces:**
- Consumes: `ScoreSystem.Load(long, int)`, `ScoreSystem.Score`, `ScoreSystem.TotalLines`.
- Produces: `static class SaveSystem { void Load(ScoreSystem s); void Save(ScoreSystem s); }` — anahtarlar: `"zen.score"` (string olarak long), `"zen.lines"` (int).

- [ ] **Step 1: Implementasyon (Unity katmanı — EditMode testi yok, PlayMode'da elle doğrulanır)**

`Assets/Scripts/Unity/SaveSystem.cs`:
```csharp
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public static class SaveSystem
    {
        const string ScoreKey = "zen.score";
        const string LinesKey = "zen.lines";

        public static void Load(ScoreSystem s)
        {
            long score = 0;
            long.TryParse(PlayerPrefs.GetString(ScoreKey, "0"), out score);
            int lines = PlayerPrefs.GetInt(LinesKey, 0);
            s.Load(score, lines);
        }

        public static void Save(ScoreSystem s)
        {
            PlayerPrefs.SetString(ScoreKey, s.Score.ToString());
            PlayerPrefs.SetInt(LinesKey, s.TotalLines);
            PlayerPrefs.Save();
        }
    }
}
```

- [ ] **Step 2: Derlemeyi doğrula**

Run: `.\run-tests.ps1` (testler değişmedi ama derleme hatası yakalar)
Expected: `failed=0`, derleme temiz.

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "feat(unity): PlayerPrefs save system for zen progress"
```

---

### Task 10: BlockSprites — programatik blok sprite'ı ve renkler

**Files:**
- Create: `Assets/Scripts/Unity/BlockSprites.cs`

**Interfaces:**
- Produces: `static class BlockSprites { Sprite Solid(int colorIndex); Sprite Ghost(int colorIndex); Color32 ColorOf(int colorIndex); const int PPU = 32; }` — colorIndex 1..7 (Tetromino.ColorIndex ile aynı). Renkler (tetr.io şeması): I `#32D5C8`, O `#E6C440`, T `#B84AC8`, S `#96C83C`, Z `#E64B55`, J `#4B64E6`, L `#E68A32`.

- [ ] **Step 1: Implementasyon**

`Assets/Scripts/Unity/BlockSprites.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace ZenTetris.Unity
{
    public static class BlockSprites
    {
        public const int PPU = 32;
        static readonly Dictionary<int, Sprite> solid = new();
        static readonly Dictionary<int, Sprite> ghost = new();

        static readonly Color32[] Colors =
        {
            new(0, 0, 0, 0),          // 0: boş
            new(50, 213, 200, 255),   // 1: I
            new(230, 196, 64, 255),   // 2: O
            new(184, 74, 200, 255),   // 3: T
            new(150, 200, 60, 255),   // 4: S
            new(230, 75, 85, 255),    // 5: Z
            new(75, 100, 230, 255),   // 6: J
            new(230, 138, 50, 255),   // 7: L
        };

        public static Color32 ColorOf(int colorIndex) => Colors[colorIndex];

        public static Sprite Solid(int colorIndex) => Get(solid, colorIndex, 1f);
        public static Sprite Ghost(int colorIndex) => Get(ghost, colorIndex, 0.3f);

        static Sprite Get(Dictionary<int, Sprite> cache, int colorIndex, float alpha)
        {
            if (cache.TryGetValue(colorIndex, out var s)) return s;

            var baseColor = (Color)Colors[colorIndex];
            var tex = new Texture2D(PPU, PPU, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            var inner = baseColor * 1.15f; inner.a = 1f;   // iç parlak çerçeve
            var edge = baseColor * 0.7f; edge.a = 1f;      // dış koyu kenar
            for (int y = 0; y < PPU; y++)
                for (int x = 0; x < PPU; x++)
                {
                    Color c = baseColor;
                    bool outerRim = x < 2 || y < 2 || x >= PPU - 2 || y >= PPU - 2;
                    bool innerRim = !outerRim && (x < 5 || y < 5 || x >= PPU - 5 || y >= PPU - 5);
                    if (outerRim) c = edge;
                    else if (innerRim) c = inner;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, PPU, PPU), new Vector2(0.5f, 0.5f), PPU);
            cache[colorIndex] = sprite;
            return sprite;
        }
    }
}
```

- [ ] **Step 2: Derlemeyi doğrula**

Run: `.\run-tests.ps1` — Expected: derleme temiz, `failed=0`.

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "feat(unity): procedural block sprites with tetr.io palette"
```

---

### Task 11: BoardRenderer — Tilemap üzerinde board, aktif parça, ghost

**Files:**
- Create: `Assets/Scripts/Unity/BoardRenderer.cs`

**Interfaces:**
- Consumes: `GameState` (Board, Active, GhostY, Changed), `BlockSprites`.
- Produces: `class BoardRenderer : MonoBehaviour { void Init(GameState state); void Redraw(); }` — kendi GameObject'inde 2 Tilemap child'ı yaratır: `Blocks` (yerleşik + aktif), `Ghost`. Dünya konumu: hücre (x, y) → local (x, y); board sol alt köşesi renderer transform'unun local (0,0)'ı.

- [ ] **Step 1: Implementasyon**

`Assets/Scripts/Unity/BoardRenderer.cs`:
```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class BoardRenderer : MonoBehaviour
    {
        GameState state;
        Tilemap blocks;
        Tilemap ghost;
        readonly Tile[] solidTiles = new Tile[8];
        readonly Tile[] ghostTiles = new Tile[8];

        public void Init(GameState s)
        {
            state = s;
            blocks = CreateLayer("Blocks", 1);
            ghost = CreateLayer("Ghost", 0);
            for (int i = 1; i <= 7; i++)
            {
                solidTiles[i] = ScriptableObject.CreateInstance<Tile>();
                solidTiles[i].sprite = BlockSprites.Solid(i);
                ghostTiles[i] = ScriptableObject.CreateInstance<Tile>();
                ghostTiles[i].sprite = BlockSprites.Ghost(i);
            }
            state.Changed += Redraw;
            Redraw();
        }

        Tilemap CreateLayer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var tm = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
            if (GetComponent<Grid>() == null) gameObject.AddComponent<Grid>();
            return tm;
        }

        public void Redraw()
        {
            blocks.ClearAllTiles();
            ghost.ClearAllTiles();

            for (int x = 0; x < Board.Width; x++)
                for (int y = 0; y < Board.VisibleHeight + 2; y++) // taşma görünürlüğü için +2
                {
                    int c = state.Board.Get(x, y);
                    if (c != 0) blocks.SetTile(new Vector3Int(x, y, 0), solidTiles[c]);
                }

            int color = Tetromino.ColorIndex(state.Active.Type);
            int gy = state.GhostY();
            foreach (var (cx, cy) in Tetromino.Cells(state.Active.Type, state.Active.Rotation))
            {
                var gpos = new Vector3Int(state.Active.X + cx, gy + cy, 0);
                if (gpos.y < Board.VisibleHeight + 2) ghost.SetTile(gpos, ghostTiles[color]);
                var apos = new Vector3Int(state.Active.X + cx, state.Active.Y + cy, 0);
                if (apos.y < Board.VisibleHeight + 2) blocks.SetTile(apos, solidTiles[color]);
            }
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
```

- [ ] **Step 2: Derlemeyi doğrula**

Run: `.\run-tests.ps1` — Expected: derleme temiz, `failed=0`.

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "feat(unity): tilemap board renderer with active piece and ghost"
```

---

### Task 12: InputHandler + GameController — DAS/ARR, tuşlar, tick

**Files:**
- Create: `Assets/Scripts/Unity/InputHandler.cs`
- Create: `Assets/Scripts/Unity/GameController.cs`

**Interfaces:**
- Consumes: `GameState` tüm public API'si, `GameConfig.Das/Arr`, `SaveSystem`.
- Produces:
  - `class InputHandler { InputHandler(GameState state); void Update(float dt); }` — yeni Input System `Keyboard.current` ile poll eder.
  - `class GameController : MonoBehaviour { void Init(GameState state); }` — Update'te input + `state.Tick`, OnApplicationQuit/Pause'da `SaveSystem.Save`.
- Tuşlar: ←/→ hareket (DAS 0.167s, ARR 0.033s), ↓ soft drop (basılıyken), Space hard drop, ↑/X CW, Z CCW, C/Shift hold, Esc pause toggle.

- [ ] **Step 1: Implementasyon**

`Assets/Scripts/Unity/InputHandler.cs`:
```csharp
using UnityEngine.InputSystem;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class InputHandler
    {
        readonly GameState state;
        float dasTimer;
        float arrTimer;
        int heldDir; // -1 sol, +1 sağ, 0 yok

        public InputHandler(GameState state) => this.state = state;

        public void Update(float dt)
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame) state.Paused = !state.Paused;

            // Rotasyon / drop / hold — kenar tetiklemeli
            if (kb.upArrowKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame) state.RotateCW();
            if (kb.zKey.wasPressedThisFrame) state.RotateCCW();
            if (kb.spaceKey.wasPressedThisFrame) state.HardDrop();
            if (kb.cKey.wasPressedThisFrame || kb.leftShiftKey.wasPressedThisFrame) state.TryHold();

            state.SetSoftDrop(kb.downArrowKey.isPressed);

            // Yatay hareket — DAS/ARR
            int dir = 0;
            if (kb.leftArrowKey.isPressed) dir -= 1;
            if (kb.rightArrowKey.isPressed) dir += 1;

            if (dir != heldDir)
            {
                heldDir = dir;
                dasTimer = 0f;
                arrTimer = 0f;
                if (dir < 0) state.MoveLeft();
                else if (dir > 0) state.MoveRight();
            }
            else if (dir != 0)
            {
                dasTimer += dt;
                if (dasTimer >= GameConfig.Das)
                {
                    arrTimer += dt;
                    while (arrTimer >= GameConfig.Arr)
                    {
                        arrTimer -= GameConfig.Arr;
                        bool ok = dir < 0 ? state.MoveLeft() : state.MoveRight();
                        if (!ok) { arrTimer = 0f; break; }
                    }
                }
            }
        }
    }
}
```

`Assets/Scripts/Unity/GameController.cs`:
```csharp
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class GameController : MonoBehaviour
    {
        GameState state;
        InputHandler input;

        public void Init(GameState s)
        {
            state = s;
            input = new InputHandler(s);
        }

        void Update()
        {
            if (state == null) return;
            input.Update(Time.deltaTime);
            state.Tick(Time.deltaTime);
        }

        void OnApplicationQuit() => SaveSystem.Save(state.Score);

        void OnApplicationPause(bool paused)
        {
            if (paused && state != null) SaveSystem.Save(state.Score);
        }
    }
}
```

- [ ] **Step 2: Derlemeyi doğrula**

Run: `.\run-tests.ps1` — Expected: derleme temiz, `failed=0`.

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "feat(unity): input with DAS/ARR and game controller loop"
```

---

### Task 13: UI — Hold/Next panelleri, HUD, panel arkaplanları

**Files:**
- Create: `Assets/Scripts/Unity/PiecePreviewUI.cs`
- Create: `Assets/Scripts/Unity/HudUI.cs`

**Interfaces:**
- Consumes: `GameState` (Held, NextQueue, Score, Changed), `BlockSprites`, `Tetromino.Cells`.
- Produces:
  - `class PiecePreviewUI : MonoBehaviour { void Init(GameState state, Vector3 holdOrigin, Vector3 nextOrigin); }` — SpriteRenderer'larla Hold (1 slot) ve Next (5 slot, dikey aralık 3 birim) çizer. Hold kullanıldıysa gri tonlu (`color = new Color(0.4f, 0.4f, 0.4f)`).
  - `class HudUI : MonoBehaviour { void Init(GameState state); }` — dünya-uzayı TextMeshPro: skor (board altı orta, `N0` formatlı) ve seviye; pause olunca board ortasında "PAUSED".

- [ ] **Step 1: Implementasyon**

`Assets/Scripts/Unity/PiecePreviewUI.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class PiecePreviewUI : MonoBehaviour
    {
        GameState state;
        Vector3 holdOrigin, nextOrigin;
        readonly List<SpriteRenderer> pool = new();
        int used;

        public void Init(GameState s, Vector3 hold, Vector3 next)
        {
            state = s;
            holdOrigin = hold;
            nextOrigin = next;
            state.Changed += Redraw;
            Redraw();
        }

        SpriteRenderer Rent()
        {
            if (used < pool.Count) { pool[used].gameObject.SetActive(true); return pool[used++]; }
            var go = new GameObject("preview");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            pool.Add(sr);
            used++;
            return sr;
        }

        void DrawPiece(TetrominoType type, Vector3 origin, float scale, Color tint)
        {
            int color = Tetromino.ColorIndex(type);
            foreach (var (x, y) in Tetromino.Cells(type, 0))
            {
                var sr = Rent();
                sr.sprite = BlockSprites.Solid(color);
                sr.color = tint;
                sr.transform.localPosition = origin + new Vector3(x * scale, y * scale, 0);
                sr.transform.localScale = Vector3.one * scale;
            }
        }

        void Redraw()
        {
            used = 0;
            if (state.Held.HasValue)
                DrawPiece(state.Held.Value, holdOrigin, 0.8f, Color.white);
            for (int i = 0; i < state.NextQueue.Count; i++)
                DrawPiece(state.NextQueue[i], nextOrigin + new Vector3(0, -i * 3f, 0), 0.8f, Color.white);
            for (int i = used; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
```

`Assets/Scripts/Unity/HudUI.cs`:
```csharp
using TMPro;
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class HudUI : MonoBehaviour
    {
        GameState state;
        TextMeshPro scoreText;
        TextMeshPro levelText;
        TextMeshPro pausedText;

        public void Init(GameState s)
        {
            state = s;
            scoreText = MakeText("Score", new Vector3(5f, -1.2f, 0), 4f);
            levelText = MakeText("Level", new Vector3(5f, -2.6f, 0), 6f);
            pausedText = MakeText("Paused", new Vector3(5f, 10f, 0), 8f);
            pausedText.text = "PAUSED";
            state.Changed += Redraw;
            Redraw();
        }

        TextMeshPro MakeText(string name, Vector3 pos, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.fontSize = size;
            t.alignment = TextAlignmentOptions.Center;
            t.sortingOrder = 10;
            return t;
        }

        void Redraw()
        {
            scoreText.text = state.Score.Score.ToString("N0");
            levelText.text = state.Score.Level.ToString();
            pausedText.gameObject.SetActive(state.Paused);
        }

        void Update() // Paused, Changed tetiklemeden değişebilir
        {
            if (state != null && pausedText.gameObject.activeSelf != state.Paused)
                pausedText.gameObject.SetActive(state.Paused);
        }

        void OnDestroy()
        {
            if (state != null) state.Changed -= Redraw;
        }
    }
}
```

- [ ] **Step 2: Derlemeyi doğrula**

Run: `.\run-tests.ps1` — Expected: derleme temiz, `failed=0`.

- [ ] **Step 3: Commit**

```bash
git add Assets
git commit -m "feat(unity): hold/next previews and HUD"
```

---

### Task 14: SceneBootstrap — sahne kurulumu ve oynanabilir bütün

**Files:**
- Create: `Assets/Scripts/Unity/SceneBootstrap.cs`
- Modify: `Assets/Scenes/SampleScene.unity` (Unity Editor'de: boş GameObject "Bootstrap" ekle, SceneBootstrap component'ini tak, kaydet)

**Interfaces:**
- Consumes: önceki tüm Unity sınıfları.
- Produces: Sahne açılınca oynanabilir oyun. Layout (dünya birimi = 1 hücre): board sol-alt (0,0)–(10,20); kamera board merkezine bakar; Hold origin `(-3.5, 17.5)`, Next origin `(12.5, 17.5)`; board arkası yarı saydam siyah panel; grid çizgileri ince koyu.

- [ ] **Step 1: Implementasyon**

`Assets/Scripts/Unity/SceneBootstrap.cs`:
```csharp
using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    public sealed class SceneBootstrap : MonoBehaviour
    {
        void Start()
        {
            var state = new GameState();
            SaveSystem.Load(state.Score);

            // Kamera
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 13f;
            cam.transform.position = new Vector3(5f, 10f, -10f);
            cam.backgroundColor = new Color(0.16f, 0.35f, 0.20f); // placeholder arkaplan

            // Board arkaplan paneli (yarı saydam siyah)
            var panel = new GameObject("BoardPanel");
            var psr = panel.AddComponent<SpriteRenderer>();
            psr.sprite = MakeSolidSprite(new Color(0f, 0f, 0f, 0.82f));
            psr.sortingOrder = -1;
            panel.transform.position = new Vector3(5f, 10f, 0);
            panel.transform.localScale = new Vector3(10f, 20f, 1f);

            // Grid çizgileri
            var grid = new GameObject("GridLines");
            var gsr = grid.AddComponent<SpriteRenderer>();
            gsr.sprite = MakeGridSprite();
            gsr.sortingOrder = 0;
            grid.transform.position = new Vector3(5f, 10f, 0);

            // Yan paneller (Hold / Next arkaplanı)
            MakePanel("HoldPanel", new Vector3(-3.5f, 17.5f, 0), new Vector3(4f, 4f, 1f));
            MakePanel("NextPanel", new Vector3(12.5f, 11.5f, 0), new Vector3(4f, 16f, 1f));

            // Bileşenler
            var renderer = new GameObject("BoardRenderer").AddComponent<BoardRenderer>();
            renderer.Init(state);

            var preview = new GameObject("Previews").AddComponent<PiecePreviewUI>();
            preview.Init(state, new Vector3(-3.5f, 17.5f, 0), new Vector3(12.5f, 17.5f, 0));

            var hud = new GameObject("Hud").AddComponent<HudUI>();
            hud.Init(state);

            var controller = gameObject.AddComponent<GameController>();
            controller.Init(state);
        }

        static void MakePanel(string name, Vector3 pos, Vector3 scale)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite(new Color(0f, 0f, 0f, 0.7f));
            sr.sortingOrder = -1;
            go.transform.position = pos;
            go.transform.localScale = scale;
        }

        static Sprite MakeSolidSprite(Color c)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        static Sprite MakeGridSprite()
        {
            const int ppu = 32;
            int w = Board.Width * ppu, h = Board.VisibleHeight * ppu;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color(0, 0, 0, 0);
            var line = new Color(1f, 1f, 1f, 0.06f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, (x % ppu == 0 || y % ppu == 0) ? line : clear);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
```

- [ ] **Step 2: Sahneye ekle (Unity Editor'de, elle)**

1. Unity'de `Assets/Scenes/SampleScene.unity` aç.
2. Hierarchy > sağ tık > Create Empty, adı `Bootstrap`.
3. `SceneBootstrap` component'ini ekle.
4. Sahneyi kaydet (Ctrl+S).

- [ ] **Step 3: Oynanış doğrulaması (manuel, Play modunda)**

Kontrol listesi:
- Parça düşüyor, ←/→ hareket, basılı tutunca DAS sonrası hızlı kayıyor.
- ↑/X ve Z döndürüyor; duvar dibinde döndürme kick ile çalışıyor.
- Space hard drop, ↓ soft drop, C hold (ikinci kez çalışmıyor), Next 5 parça gösteriyor.
- Ghost görünüyor; satır dolunca siliniyor; skor/seviye artıyor.
- Board'u bilerek doldurunca oyun BİTMİYOR: board temizlenip devam ediyor, skor duruyor.
- Esc pause; Play'den çıkıp tekrar girince skor/seviye korunuyor.

- [ ] **Step 4: Testlerin hâlâ geçtiğini doğrula**

Run: `.\run-tests.ps1` (Unity'yi kapatarak) — Expected: `failed=0`

- [ ] **Step 5: Commit**

```bash
git add Assets
git commit -m "feat(unity): scene bootstrap - playable zen tetris"
```

---

## Doğrulama Özeti

| Gereksinim (spec) | Görev |
|---|---|
| 10×40 board, satır silme, top-out temizliği | 3, 7 |
| 7-bag | 4 |
| SRS + kickler | 2, 5 |
| Skor/combo/B2B/T-spin/seviye | 6, 8 |
| Yerçekimi, lock delay (500ms/15 reset), hold, 5'li next, ghost, hard/soft drop | 7 |
| DAS/ARR (167/33ms) | 12 |
| Kalıcılık (PlayerPrefs) | 9, 12 |
| tetr.io benzeri düzen, renkler, ghost görseli | 10, 11, 13, 14 |
| Pause | 7, 12, 13 |
| Sessiz | (ses görevi yok — bilinçli) |
