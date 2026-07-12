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
