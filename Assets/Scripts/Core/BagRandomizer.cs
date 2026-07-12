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
