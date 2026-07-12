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
