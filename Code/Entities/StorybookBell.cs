using System;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities;

[CustomEntity("SS2024/StorybookBell")]
public class StorybookBell : Entity{

    public const string DisableFlag = "SSC_storybook_bell_collected";

    internal bool disabled, swinging;
    private Image bellFront, bellBack, bellClapper;
    private float time = 0;
    
    public StorybookBell(EntityData data, Vector2 offset) : base(data.Position + offset){
        Add(bellBack = new Image(GFX.Game["objects/ss2024/storybookBell/Spooooky_BellBack"]).JustifyOrigin(0.5f, 0));
        Add(bellClapper = new Image(GFX.Game["objects/ss2024/storybookBell/Spooooky_BellClapper"]).JustifyOrigin(0.5f, 0));
        Add(bellFront = new Image(GFX.Game["objects/ss2024/storybookBell/Spooooky_BellFront"]).JustifyOrigin(0.5f, 0));
        Add(new PlayerCollider(OnPlayer, new Hitbox(48, 64)));
        
        bellFront.Position = bellBack.Position = bellClapper.Position = new(24, 0);
        
        Depth = 1;
    }

    public override void Awake(Scene scene){
        base.Awake(scene);
        disabled = (scene as Level).Session.GetFlag(DisableFlag);
        
        Apply(false);
        if(disabled)
            GetBerry(false);
    }

    public override void Update(){
        base.Update();

        if(swinging){
            float fac = 1 - (time / 52f);
            float offset = (float)(Math.Sin(time * 1.4f) / 9f) * fac;
            bellFront.Rotation = bellBack.Rotation = bellClapper.Rotation = (float)Math.Sin(time) * fac / 4f;
            bellFront.Rotation -= offset;
            bellBack.Rotation -= offset;
            bellClapper.Rotation += offset;
            time += 2 * Engine.DeltaTime;
            if(fac < 0.1f)
                swinging = false;
        }else
            bellFront.Rotation = bellBack.Rotation = bellClapper.Rotation = 0;
    }

    public void Apply(bool visible = true, Scene scene = null){
        foreach(CutsceneNode e in Scene.Tracker.GetEntities<CutsceneNode>().OfType<CutsceneNode>().ToList()){
            if(e.Name == "bell_crystal"){
                foreach(Refill crystal in Scene.Entities.OfType<Refill>().Where(x => x.CollidePoint(e.Position))){
                    bool target = crystal.GetType() == typeof(Refill) != disabled;
                    crystal.Visible = crystal.Active = crystal.Collidable = target;
                    if(visible && !target){
                        ((scene ?? Scene) as Level)?.ParticlesFG?.Emit(crystal.p_shatter, 5, crystal.Position, Vector2.One * 4f, -1.57f);
                        ((scene ?? Scene) as Level)?.ParticlesFG?.Emit(crystal.p_shatter, 5, crystal.Position, Vector2.One * 4f, 1.57f);
                    }
                }
            }

            if(disabled && e.Name.StartsWith("bell_start", StringComparison.Ordinal)){
                string suffix = e.Name.Substring("bell_start".Length);
                CutsceneNode target = CutsceneNode.Find("bell_end" + suffix);
                if(target != null){
                    Logger.Log(LogLevel.Info, "StorybookBell", "found a target to move to!");
                    // grab the closest entity
                    float min = float.MaxValue;
                    Entity eg = null;
                    foreach(Entity entity in Scene.Entities.Where(x => !(x is CutsceneNode))){
                        float n = (entity.Position - e.Position).LengthSquared();
                        if(n < min){
                            min = n;
                            eg = entity;
                        }
                    }
                    if(eg != null && min <= 64)
                        eg.Position = target.Position;
                }
            }

            if(!disabled && e.Name.Equals("bell_strawberry", StringComparison.Ordinal)){
                Strawberry s = Scene.Entities.OfType<Strawberry>().FirstOrDefault(s => (s.Position - e.Position).LengthSquared() <= 64);
                if(s != null){
                    s.Visible = s.Active = s.bloom.Visible = s.light.Visible = false;
                    s.WaitingOnSeeds = true;
                }
            }
        }
    }

    public void GetBerry(bool visible = true, Scene scene = null){
        CutsceneNode berry = (scene ?? Scene).Tracker.GetEntities<CutsceneNode>().OfType<CutsceneNode>().First(x => x.Name == "bell_strawberry");
        Strawberry s = Scene.Entities.OfType<Strawberry>().FirstOrDefault(s => (s.Position - berry.Position).LengthSquared() <= 64);
        if(s != null){
            s.CollectedSeeds();
            s.Active = true;
            CutsceneNode target = (scene ?? Scene).Tracker.GetEntities<CutsceneNode>().OfType<CutsceneNode>().First(x => x.Name == "bell_strawberry_target");
            s.Position = target.Position;
            if(visible){
                for(int i = 0; i < 12; i++){
                    float num = Calc.Random.NextFloat(MathHelper.TwoPi);
                    (Scene as Level).ParticlesFG.Emit(StrawberrySeed.P_Burst, 1, s.Position + Calc.AngleToVector(num, 4), Vector2.Zero, num);
                }
            }
        }
    }

    private void OnPlayer(Player player){
        if(player.DashAttacking && !disabled)
            Scene.Add(new CS_StorybookBell(this));
    }    
}