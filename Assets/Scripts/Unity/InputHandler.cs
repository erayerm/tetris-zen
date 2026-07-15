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

        const float StickDeadzone = 0.5f;

        public InputHandler(GameState state) => this.state = state;

        public void Update(float dt)
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;
            if (kb == null && gp == null) return;

            float sx = gp != null ? gp.leftStick.x.ReadValue() : 0f;
            float sy = gp != null ? gp.leftStick.y.ReadValue() : 0f;

            // Kenar tetiklemeli aksiyonlar (klavye VEYA gamepad).
            // Pause (Esc/Start) AppController tarafından yönetilir.
            bool cw = (kb?.upArrowKey.wasPressedThisFrame ?? false)
                    || (kb?.xKey.wasPressedThisFrame ?? false)
                    || (gp?.buttonSouth.wasPressedThisFrame ?? false);   // A
            bool ccw = (kb?.zKey.wasPressedThisFrame ?? false)
                     || (gp?.buttonEast.wasPressedThisFrame ?? false)    // B
                     || (gp?.buttonWest.wasPressedThisFrame ?? false);   // X
            bool hard = (kb?.spaceKey.wasPressedThisFrame ?? false)
                      || (gp?.buttonNorth.wasPressedThisFrame ?? false)  // Y
                      || (gp?.dpad.up.wasPressedThisFrame ?? false);
            bool hold = (kb?.cKey.wasPressedThisFrame ?? false)
                      || (kb?.leftShiftKey.wasPressedThisFrame ?? false)
                      || (gp?.leftShoulder.wasPressedThisFrame ?? false)  // LB
                      || (gp?.rightShoulder.wasPressedThisFrame ?? false);// RB

            if (cw) state.RotateCW();
            if (ccw) state.RotateCCW();
            if (hard) state.HardDrop();
            if (hold) state.TryHold();

            // Soft drop (basılı)
            bool soft = (kb?.downArrowKey.isPressed ?? false)
                      || (gp?.dpad.down.isPressed ?? false)
                      || sy < -StickDeadzone;
            state.SetSoftDrop(soft);

            // Yatay hareket — DAS/ARR (klavye ok / d-pad / sol analog)
            bool left = (kb?.leftArrowKey.isPressed ?? false)
                      || (gp?.dpad.left.isPressed ?? false)
                      || sx < -StickDeadzone;
            bool right = (kb?.rightArrowKey.isPressed ?? false)
                       || (gp?.dpad.right.isPressed ?? false)
                       || sx > StickDeadzone;

            int dir = 0;
            if (left) dir -= 1;
            if (right) dir += 1;

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
