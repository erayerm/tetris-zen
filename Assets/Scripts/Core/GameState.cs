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

            // Yerden ayrıldıysa (ör. kenardan döndürme) lock delay birikimini sıfırla —
            // inişte beklenmedik erken kilitlenmeyi önler.
            lockTimer = 0f;

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
            bool fullyHidden = true;
            foreach (var (_, y) in Active.AbsoluteCells())
                if (y < Board.VisibleHeight) fullyHidden = false;

            if (fullyHidden)
            {
                // Zen top-out: skor korunur, tahta temizlenir, normal skorlama atlanır.
                Board.ClearAll();
                Spawn();
                return;
            }

            var tspin = DetectTSpin();
            Board.Lock(Active);

            int lines = Board.ClearFullLines();
            Score.OnPieceLocked(lines, tspin);
            if (lines > 0) LinesCleared?.Invoke(lines);

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
