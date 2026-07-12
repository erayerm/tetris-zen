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
