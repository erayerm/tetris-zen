namespace ZenTetris.Core
{
    public static class GameConfig
    {
        public const float Das = 0.167f;
        public const float Arr = 0.033f;
        public const float LockDelay = 0.5f;
        public const int MaxLockResets = 15;
        public const float SoftDropCellsPerSecond = 8f; // soft drop sabit hız (seviyeden bağımsız)
        public const float BaseGravity = 0.5f;     // hücre/sn, seviye 1 (Zen: sakin)
        public const float GravityPerLevel = 0.08f;
        public const float MaxGravity = 3f;
        public const int NextQueueSize = 5;

        public static float GravityFor(int level) =>
            System.Math.Min(BaseGravity + (level - 1) * GravityPerLevel, MaxGravity);
    }
}
