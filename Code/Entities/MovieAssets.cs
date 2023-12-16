using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Entities
{
    [CustomEntity("SS2024/MortisDummy")]
    public class MortisDummy : Entity
    {
        public class AuxiliaryLightEntity : Entity
        {
            public VertexLight light;
            public BloomPoint bloom;
            public AuxiliaryLightEntity(Vector2 pos) : base(pos) 
            {
                Add(light = new VertexLight(Color.LimeGreen, 0f, 20, 50));
                Add(bloom = new BloomPoint(0f, 32f));
            }
        }

        public Sprite portal;
        public float mortisPercent;
        public Vector2 mortisPosition;
        public float pausefade;
        public MTexture rickmortis;
        public MTexture whitemortis;
        public AuxiliaryLightEntity aux;

        public MortisDummy(EntityData data, Vector2 offset) : this(data.Position + offset) 
        {
            Tag = Tags.HUD | Tags.PauseUpdate;
        }
        public MortisDummy(Vector2 position) : base(position) 
        {
            Add(portal = GFX.SpriteBank.Create("SS2024_SunsetQuasar_portal"));
            portal.Play("idle");
            portal.Rotation = 135f * Calc.DegToRad;
            portal.Scale = Vector2.Zero;
            rickmortis = GFX.Game["objects/SS2024/SunsetQuasar/portal/rickmortis"];
            whitemortis = GFX.Game["objects/SS2024/SunsetQuasar/portal/whitemortis"];
            mortisPosition = new Vector2(0f, 0f);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(spawnPortal()));
            Scene.Add(aux = new AuxiliaryLightEntity(Position));
        }
        public override void Update()
        {
            if (!(Scene as Level).Paused)
            {
                base.Update();
            }
            pausefade = Calc.Approach(pausefade, (Scene as Level).Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
        }
        public override void Render()
        {
            Color col = Color.Lerp(Color.White, Color.Black, pausefade * 0.7f);
            Position -= (Scene as Level).Camera.Position;
            Position *= 6;
            portal.SetColor(col);
            base.Render();
            rickmortis.Draw(Position + mortisPosition, new Vector2(68f, 87f), col, mortisPercent);
            whitemortis.Draw(Position + mortisPosition, new Vector2(68f, 87f), col * (1-mortisPercent), mortisPercent);
            Position /= 6;
            Position += (Scene as Level).Camera.Position;
        }

        public IEnumerator spawnPortal()
        {
            yield return 1f;
            for (float i = 0; i < 1f; i += Engine.DeltaTime / 1f)
            {
                portal.Scale = Vector2.One * Ease.BackOut(i);
                aux.light.Alpha = aux.bloom.Alpha = Ease.SineOut(Math.Min(i * 3, 1));
                aux.light.startRadius = Ease.SineOut(i) * 20;
                aux.light.endRadius = aux.light.startRadius + (Ease.SineOut(i) * 30);
                aux.light.HandleGraphicsReset();
                yield return null;
            }
            Add(new Coroutine(mortisAppear(new Vector2(-108f, 20f))));
        }

        public IEnumerator mortisAppear(Vector2 offset)
        {
            for (float i = 0; i < 1f; i += Engine.DeltaTime / 1f)
            {
                mortisPercent = Ease.SineInOut(i);
                mortisPosition.X = Ease.SineInOut(i) * offset.X;
                mortisPosition.Y = Ease.ExpoIn(i) * offset.Y;
                yield return null;
            }
        }
    }
}
