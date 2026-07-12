using UnityEngine;
using ZenTetris.Core;

namespace ZenTetris.Unity
{
    // "Juice": taş kilitlenince/satır silinince board'u yumuşakça sektirir ve
    // küçük konfeti partikülleri saçar. Satır silmede daha görkemli, renkli.
    public sealed class Juice : MonoBehaviour
    {
        GameState state;
        Transform boardGroup;
        Vector3 basePos;
        float offset, vel;

        const float Stiffness = 90f;   // yumuşak, görünür yaylanma
        const float Damping = 13f;

        ParticleSystem ps;

        public void Init(GameState s, Transform group)
        {
            state = s;
            boardGroup = group;
            basePos = group.localPosition;
            SetupParticles();

            state.PieceLocked += OnPieceLocked;
            state.RowsCleared += OnRowsCleared;
        }

        void SetupParticles()
        {
            var go = new GameObject("Confetti");
            go.transform.SetParent(transform, false);
            ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = 0.55f;
            main.startSpeed = 0f;
            main.startSize = 0.12f;
            main.gravityModifier = 0.9f;
            main.maxParticles = 2000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission; emission.enabled = false; // elle Emit
            var shape = ps.shape; shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.material = new Material(Shader.Find("Sprites/Default"));
            r.sortingOrder = 25;
            ps.Play();
        }

        void OnPieceLocked(Piece p)
        {
            Bounce(1.6f); // yumuşak (~0.23 hücre dip)
            var color = (Color)BlockSprites.ColorOf(Tetromino.ColorIndex(p.Type));

            var pts = new System.Collections.Generic.List<Vector3>();
            foreach (var (x, y) in p.AbsoluteCells())
                if (y < Board.VisibleHeight) pts.Add(new Vector3(x + 0.5f, y + 0.5f, 0));
            if (pts.Count == 0) return;

            // Eski: hücre başına 3 (~12). Yarıya indir -> ~6 toplam, rastgele hücrelerden.
            int total = Mathf.RoundToInt(pts.Count * 3 * 0.5f);
            for (int i = 0; i < total; i++)
                Burst(pts[Random.Range(0, pts.Count)], color, 1, 1.5f, 0.09f);
        }

        void OnRowsCleared(System.Collections.Generic.IReadOnlyList<Cell> cells)
        {
            int rows = cells.Count / Board.Width;
            Bounce(2.4f + 1.4f * rows); // daha güçlü
            foreach (var c in cells)
            {
                if (c.Y >= Board.VisibleHeight) continue;
                var color = (Color)BlockSprites.ColorOf(c.Color);
                Burst(new Vector3(c.X + 0.5f, c.Y + 0.5f, 0), color, 1, 2.6f, 0.11f); // hücre başına 1
            }
        }

        void Bounce(float impulse) => vel -= impulse; // aşağı it, yay geri getirir

        void Burst(Vector3 pos, Color color, int count, float speed, float size)
        {
            for (int i = 0; i < count; i++)
            {
                var ep = new ParticleSystem.EmitParams();
                ep.position = pos + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
                float ang = Random.Range(0f, Mathf.PI * 2f);
                float sp = speed * Random.Range(0.5f, 1f);
                ep.velocity = new Vector3(Mathf.Cos(ang) * sp, Mathf.Abs(Mathf.Sin(ang)) * sp + speed * 0.4f, 0);
                ep.startColor = color;
                ep.startSize = size * Random.Range(0.7f, 1.3f);
                ep.startLifetime = Random.Range(0.4f, 0.7f);
                ep.rotation = Random.Range(0f, 360f);
                ep.angularVelocity = Random.Range(-180f, 180f);
                ps.Emit(ep, 1);
            }
        }

        void Update()
        {
            if (boardGroup == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            float acc = -Stiffness * offset - Damping * vel;
            vel += acc * dt;
            offset += vel * dt;
            boardGroup.localPosition = new Vector3(basePos.x, basePos.y + offset, basePos.z);
        }

        void OnDestroy()
        {
            if (state != null)
            {
                state.PieceLocked -= OnPieceLocked;
                state.RowsCleared -= OnRowsCleared;
            }
        }
    }
}
