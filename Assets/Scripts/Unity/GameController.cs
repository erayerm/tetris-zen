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
            // Her taş kilitlendiğinde de kaydet -> çıkış olayı kaçsa bile board korunur.
            state.PieceLocked += _ => SaveSystem.Save(state);
        }

        void Update()
        {
            if (state == null) return;
            input.Update(Time.deltaTime);
            state.Tick(Time.deltaTime);
        }

        void OnApplicationQuit()
        {
            if (state != null) SaveSystem.Save(state);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused && state != null) SaveSystem.Save(state);
        }
    }
}
