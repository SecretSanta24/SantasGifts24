using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Cutscenes;

public class CS_StorybookBell : CutsceneEntity{
    
    private readonly StorybookBell bell;
    private bool berryAppeared;
    private bool applied;

    public CS_StorybookBell(StorybookBell bell){
        this.bell = bell;
        bell.disabled = true;
    }

    public override void OnBegin(Level level){
        Add(new Coroutine(Cutscene(level)));
    }

    public override void OnEnd(Level level){
        Player p = level.Tracker.GetEntity<Player>();
        p.StateMachine.State = Player.StNormal;
        p.DummyAutoAnimate = true;
        p.Speed = Vector2.Zero;
        
        bell.disabled = true;
        if(WasSkipped){
            bell.swinging = false;
            // drop the player off at their nearest target
            p.Position = NearestTarget(level, p.Position);
            if(!applied)
                bell.Apply();
            if(!berryAppeared)
                bell.GetBerry();
        }
        
        level.Session.SetFlag(StorybookBell.DisableFlag);
    }

    private IEnumerator Cutscene(Level level){
        Player p = level.Tracker.GetEntity<Player>();
        p.StateMachine.State = Player.StDummy;
        p.DummyMaxspeed = false;
        var centre = CutsceneNode.Find("bell_strawberry_target").Position;
        int dirRaw = Math.Sign(centre.X - p.Position.X);
        int dir = dirRaw == 0 ? -1 : dirRaw;
        p.Facing = (Facings)dir;
        p.Speed.X = -dir * 300;
        p.Speed.Y = -190;
        bell.swinging = true;
        bell.swingMul = -dir;
        // 6f
        for (int i = 0; i < 4; i++){
            level.Displacement.AddBurst(centre, 1, 1, 220, 0.5f);
            var inst = Audio.Play("event:/game/general/strawberry_get", centre, "colour", 2, "count", 0);
            inst.setPitch(1.3f + Calc.Random.NextFloat(0.3f) + i * 0.05f);
            inst.setVolume(0.6f + Calc.Random.NextFloat(0.2f));
            yield return 1.2f;
        }
        bell.Apply();
        applied = true;
        // 6f
        for (int i = 0; i < 4; i++){
            level.Displacement.AddBurst(centre, 1, 1, 220, 0.4f);
            var inst = Audio.Play("event:/game/general/strawberry_get", centre, "colour", 2, "count", 0);
            inst.setPitch(1.4f + Calc.Random.NextFloat(0.6f));
            inst.setVolume(0.4f + Calc.Random.NextFloat(0.1f));
            yield return 1.2f;
        }
        bell.GetBerry();
        berryAppeared = true;
        // 1f
        level.Displacement.AddBurst(centre, 1, 1, 220, 0.3f);
        var instL = Audio.Play("event:/game/general/strawberry_get", centre, "colour", 2, "count", 0);
        instL.setPitch(2.3f);
        instL.setVolume(1.1f);
        yield return 0.1f;
        Audio.Play("event:/game/general/seed_complete_berry", centre);
        yield return 1f;
        EndCutscene(level);
    }

    private Vector2 NearestTarget(Level level, Vector2 to){
        float min = float.MaxValue;
        Vector2 there = Vector2.Zero;
        foreach(CutsceneNode node in level.Tracker.GetEntities<CutsceneNode>().OfType<CutsceneNode>().Where(x=>x.Name == "bell_playertarget")){
            float n = (to - node.Position).LengthSquared();
            if (n < min){
                min = n;
                there = node.Position;
            }
        }
        return there;
    }
}