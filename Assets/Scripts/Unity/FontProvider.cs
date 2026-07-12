using TMPro;
using UnityEngine;

namespace ZenTetris.Unity
{
    // Yumuşak/yuvarlak font sağlar. Öncelik: Assets/Resources/GameFont.ttf ->
    // sistemde bulunan yuvarlak bir font -> TMP varsayılanı.
    public static class FontProvider
    {
        static TMP_FontAsset cached;
        static bool tried;

        public static TMP_FontAsset Get()
        {
            if (tried) return cached;
            tried = true;

            Font src = Resources.Load<Font>("GameFont") ?? Resources.Load<Font>("Gamefont");
            if (src == null)
            {
                string[] preferred = { "Nunito", "Quicksand", "Baloo 2", "Fredoka",
                                       "Varela Round", "Comic Sans MS", "Segoe UI" };
                src = Font.CreateDynamicFontFromOSFont(preferred, 48);
            }
            if (src != null)
            {
                try { cached = TMP_FontAsset.CreateFontAsset(src); }
                catch { cached = null; }
            }
            return cached;
        }

        public static void Apply(TMP_Text text)
        {
            var f = Get();
            if (f != null) text.font = f;
        }
    }
}
