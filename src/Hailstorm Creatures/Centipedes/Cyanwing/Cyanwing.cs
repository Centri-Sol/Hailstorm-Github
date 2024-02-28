using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using RWCustom;
using MoreSlugcats;
using System.Runtime.ConstrainedExecution;

namespace Hailstorm;

public class Cyanwing : Centipede
{
    public CyanwingState CyanState => CentiState as CyanwingState;
    public CyanwingGraphics CyanGraphics => graphicsModule as CyanwingGraphics;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public float minHue;
    public float maxHue;
    public float saturation;
    public Color bodyColor;
    public bool offcolor;

    public float superCharge;
    public int shockGiveUpDelayCounter;

    public HailstormFireSmokeCreator vaporSmoke;

    public int emitSparkCounter;
    public int SelfDestructCountdown;
    public FirecrackerPlant.ScareObject scareObj;

    public Cyanwing(AbstractCreature absCyn, World world) : base(absCyn, world)
    {
        Random.State state = Random.state;
        Random.InitState(absCyn.ID.RandomSeed);
        absCyn.state.meatLeft = 12;
        AssignSize();
        offcolor = Random.value < 0.07f;
        minHue = 160/360f + Mathf.Pow((50/360f * Random.value), 2f);
        Random.state = state;
        maxHue = minHue + (50/360f);
        saturation = 1;
        bodyColor = new HSLColor(0, 0, 0.8f).rgb;

        bodyChunks = new BodyChunk[(int)Mathf.Lerp(7, 17, size)];
        for (int c = 0; c < bodyChunks.Length; c++)
        {
            float bodyLengthProgression = c / (float)(bodyChunks.Length - 1);
            float chunkRad =
                Mathf.Lerp(
                    Mathf.Lerp(2, 3.5f, size),
                    Mathf.Lerp(4, 6.5f, size),
                    Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * bodyLengthProgression), 0, 1), Mathf.Lerp(0.7f, 0.3f, size)));
            chunkRad += 0.3f;

            float chunkMass = Mathf.Lerp(3/70f, 11/34f, Mathf.Pow(size, 1.4f));

            chunkMass += 0.02f + (0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, bodyChunks.Length - 1, c) * Mathf.PI)) * 0.5f);

            bodyChunks[c] = new(this, c, default, chunkRad, chunkMass);

        }

        mainBodyChunkIndex = bodyChunks.Length / 2;

        if (CyanState is not null && (CyanState.shells is null || CyanState.shells.Length != bodyChunks.Length))
        {
            CyanState.shells = new bool[bodyChunks.Length];
            for (int k = 0; k < CyanState.shells.Length; k++)
            {
                CyanState.shells[k] = Random.value < 0.97f;
            }
        }

        if (CyanState.superShells is null ||
            CyanState.superShells.Count != bodyChunks.Length)
        {
            ResetShellData();
        }

        bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
        int bodyConnectionNum = 0;
        for (int c = 0; c < bodyChunks.Length; c++)
        {
            for (int m = c + 1; m < bodyChunks.Length; m++)
            {
                bodyChunkConnections[bodyConnectionNum] = new BodyChunkConnection(bodyChunks[c], bodyChunks[m], (bodyChunks[c].rad + bodyChunks[m].rad) * 1.2f, BodyChunkConnection.Type.Push, 1, -1);
                bodyConnectionNum++;
            }
        }
    }
    public virtual void AssignSize()
    {
        size = Random.Range(1f, 1.2f);
    }
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule is null)
        {
            graphicsModule = new CyanwingGraphics(this);
        }
    }
    public virtual void ResetShellData()
    {
        List<CyanwingState.Shell> oldShells = CyanState.superShells;
        CyanState.superShells = new(bodyChunks.Length);
        if (oldShells is not null && oldShells.Count > 0)
        {

        }
        Random.State state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        for (int s = 0; s < CyanState.superShells.Capacity; s++)
        {
            bool startGoingBackwards = s % 20 >= 10;
            CyanState.superShells.Add(new CyanwingState.Shell(s));
            CyanState.superShells[s].hue =
                Mathf.Lerp(
                    minHue,
                    maxHue,
                    startGoingBackwards ?
                        Mathf.InverseLerp(20, 10, s) :
                        Mathf.InverseLerp(0, 10, s));
            CyanState.superShells[s].gradientDirection = startGoingBackwards;
        }
        Random.state = state;
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {

        base.Update(eu);

        GrabUpdate();

        if (room is null)
        {
            return;
        }

        if (grabbedBy.Count > 0 &&
            grabbedBy[0]?.grabbedChunk is not null &&
            CyanState.shells is not null &&
            (CyanState.shells[grabbedBy[0].grabbedChunk.index] || shellJustFellOff == grabbedBy[0].grabbedChunk.index))
        {
            ZapGrabber(grabbedBy[0].grabber, grabbedBy[0].grabbedChunk);
        }
        if (shellJustFellOff != -1)
        {
            shellJustFellOff = -1;
        }

        UpdateHeadGlow();

        if (Random.value < (dead ? 0.02f : Mathf.Lerp(0.06f, 0.03f, CentiState.ClampedHealth)))
        {
            EmitAmbientSpark();
        }

        if (SelfDestructCountdown > 0)
        {
            SelfDestructSequence();
        }
        ManageScareObject();

        if (dead)
        {
            return;
        }

        ManageShellColors();

        ManageCharge();

    }
    public virtual void GrabUpdate()
    {
        bool beingGrabby = false;
        for (int g = 0; g < grasps.Length; g++)
        {
            if (grasps[g] is not null)
            {
                beingGrabby = true;
                break;
            }
        }
        if (beingGrabby)
        {
            shockGiveUpDelayCounter++;
            if (shockGiveUpDelayCounter > 2)
            {
                shockGiveUpDelayCounter = 0;
                shockGiveUpCounter--;
            }
        }
        else
        {
            shockGiveUpCounter--;
        }
    }
    public virtual void UpdateHeadGlow()
    {
        if (Glower is null)
        {
            GlowerHead = HeadChunk;
            Glower = new LightSource(GlowerHead.pos, environmentalLight: false, Custom.HSL2RGB(minHue, saturation, 0.625f), this);
            room.AddObject(Glower);
            Glower.alpha = 0;
            Glower.rad = 0;
            Glower.submersible = true;
        }
        else
        {
            if (GlowerHead == HeadChunk && superCharge < 0.2f && Consious)
            {
                if (Glower.rad < 300f)
                {
                    Glower.rad += 11f;
                }
                if (Glower.Alpha < 0.5f)
                {
                    Glower.alpha += 0.2f;
                }
            }
            else
            {
                if (Glower.rad > 0f)
                {
                    Glower.rad -= 5f;
                }
                if (Glower.Alpha > 0f)
                {
                    Glower.alpha -= 0.05f;
                }
                if (Glower.Alpha <= 0f && Glower.rad <= 0f)
                {
                    room.RemoveObject(Glower);
                    Glower = null;
                }
            }
            if (Glower is not null)
            {
                Glower.pos = GlowerHead.pos;
            }
        }
    }
    public virtual void ManageShellColors()
    {
        for (int s = 0; s < bodyChunks.Length; s++)
        {
            if (!CyanState.shells[s])
            {
                continue;
            }

            CyanwingState.Shell shell = CyanState.superShells[s];

            if (shell.hue >= maxHue)
            {
                shell.gradientDirection = true;
            }
            else if (shell.hue <= minHue)
            {
                shell.gradientDirection = false;
            }

            shell.hue +=
                (Stunned ? 0.5f / 360f : 1 / 360f) * (shell.gradientDirection ? -1 : 1);

            if (Random.value < Mathf.Lerp(0.05f, 0.01f, CentiState.ClampedHealth))
            {
                room.AddObject(new Spark(bodyChunks[s].pos, Custom.RNV() * Random.Range(16f, 24f), new HSLColor(shell.hue, saturation, 0.5f).rgb, null, 4, 50));
            }
        }
    }
    public virtual void FlipShellGradientDirections()
    {
        for (int s = 0; s < bodyChunks.Length; s++)
        {
            CyanState.superShells[s].gradientDirection = !CyanState.superShells[s].gradientDirection;
        }
    }
    public virtual void ManageCharge()
    {
        if (shockCharge > 0)
        {
            superCharge += 1 / Mathf.Lerp(100f, 5f, size);
            shockCharge = 0;
        }
        else if (superCharge != 0)
        {
            superCharge = Mathf.Max(0, superCharge - (1 / Mathf.Lerp(100f, 5f, size) / 2f));
        }
    }
    public virtual void SelfDestructSequence()
    {
        SelfDestructCountdown--;
        emitSparkCounter++;
        float sparkFac = SelfDestructCountdown > 20 ?
            Custom.LerpMap(SelfDestructCountdown, 320, 20, 70, 15) :
            Custom.LerpMap(SelfDestructCountdown,  20,  0,  5,  1);
        if (emitSparkCounter > sparkFac && Random.value < 0.3f)
        {
            float boomFac = 0.33f + Mathf.InverseLerp(640, 0, SelfDestructCountdown);
            BodyChunk randomChunk = bodyChunks[Random.Range(0, bodyChunks.Length)];
            room.InGameNoise(new Noise.InGameNoise(randomChunk.pos, 6000f * boomFac, this, 4f));
            room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, randomChunk.pos, 1, Random.Range(0.75f, 1.25f) * boomFac);
            room.AddObject(new ColorableElectrodeathSpark(randomChunk.pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), ShortCutColor()));
            room.AddObject(new ColorableElectrodeathSpark(randomChunk.pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), ShortCutColor()));
            emitSparkCounter = 0;
        }
        if (SelfDestructCountdown == 40)
        {
            room.PlaySound(SoundEffects.CyanwingDeath, mainBodyChunk);
        }
        if (SelfDestructCountdown < 1)
        {
            SelfDestruct();
        }
    }
    public virtual void ManageScareObject()
    {
        if (scareObj is not null)
        {
            scareObj.pos = mainBodyChunk.pos;
        }
    }

    public virtual void CyanwingCrawl(Vector2[] bodyChunkVels)
    {
        int segmentsAppliedForceTo = 0;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            BodyChunk chunk = bodyChunks[b];
            chunk.vel = bodyChunkVels[b];
            if (!AccessibleTile(room.GetTilePosition(chunk.pos)))
            {
                continue;
            }
            segmentsAppliedForceTo++;
            chunk.vel *= 0.7f;
            chunk.vel.y += gravity;
            if (b > 0 && !AccessibleTile(room.GetTilePosition(bodyChunks[b - 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += gravity;
            }
            if (b < bodyChunks.Length - 1 && !AccessibleTile(room.GetTilePosition(bodyChunks[b + 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += gravity;
            }
            if (b <= 0 || b >= bodyChunks.Length - 1)
            {
                continue;
            }
            if (moving)
            {
                int bodyDir = (!bodyDirection ? 1 : -1);
                if (AccessibleTile(room.GetTilePosition(bodyChunks[b + bodyDir].pos)))
                {
                    chunk.vel += Custom.DirVec(chunk.pos, bodyChunks[b + bodyDir].pos) * 1.5f * Mathf.Lerp(0.5f, 1.5f, size);
                }
                chunk.vel -= Custom.DirVec(chunk.pos, bodyChunks[b + (bodyDir * -1)].pos) * 0.8f * Mathf.Lerp(0.7f, 1.3f, size);
                continue;
            }
            Vector2 moveDir = chunk.pos - bodyChunks[b - 1].pos;
            Vector2 moveAngle = moveDir.normalized;
            moveDir = bodyChunks[b + 1].pos - chunk.pos;
            Vector2 finalMoveDir = (moveAngle + moveDir.normalized) / 2f;
            if (Mathf.Abs(finalMoveDir.x) > 0.5f)
            {
                chunk.vel.y -= (chunk.pos.y - (room.MiddleOfTile(chunk.pos).y + VerticalSitSurface(chunk.pos) * (10f - chunk.rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(size, 1.2f));
            }
            if (Mathf.Abs(finalMoveDir.y) > 0.5f)
            {
                chunk.vel.x -= (chunk.pos.x - (room.MiddleOfTile(chunk.pos).x + HorizontalSitSurface(chunk.pos) * (10f - chunk.rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(size, 1.2f));
            }
        }
        if (segmentsAppliedForceTo > 0)
        {
            HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * Custom.LerpMap(segmentsAppliedForceTo, 0f, bodyChunks.Length, 6f, 3f) * Mathf.Lerp(0.7f, 1.3f, size * 0.7f);
        }
        if (segmentsAppliedForceTo == 0)
        {
            flyModeCounter += 10;
            wantToFly = true;
        }
    }
    public virtual void CyanwingFly(Vector2[] bodyChunkVels)
    {
        bodyWave += 1f;

        for (int i = 0; i < bodyChunks.Length; i++)
        {
            BodyChunk chunk = bodyChunks[i];
            chunk.vel = bodyChunkVels[i];
            float bodyLengthProgress = i / (float)(bodyChunks.Length - 1);
            if (!bodyDirection)
            {
                bodyLengthProgress = 1f - bodyLengthProgress;
            }
            float bodyWaveFac = Mathf.Sin((bodyWave - bodyLengthProgress * Mathf.Lerp(12f, 28f, size)) * Mathf.PI * 0.11f);
            bodyChunks[i].vel *= 0.9f;
            bodyChunks[i].vel.y += gravity * wingsStartedUp;
            if (i <= 0 || i >= bodyChunks.Length - 1)
            {
                continue;
            }
            Vector2 dirToNextChunk = Custom.DirVec(bodyChunks[i].pos, bodyChunks[i + (!bodyDirection ? 1 : -1)].pos);
            Vector2 perpAngle = Custom.PerpendicularVector(dirToNextChunk);
            bodyChunks[i].vel += dirToNextChunk * 0.5f * Mathf.Lerp(0.5f, 1.5f, size);
            bodyChunks[i].pos += perpAngle * 2.5f * bodyWaveFac;
        }
        if (room.aimap.getAItile(moveToPos).terrainProximity > 2)
        {
            HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos + Custom.DegToVec(bodyWave * 10f) * 60f) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
        }
        else
        {
            HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
        }
    }

    // - - - - - - - - - - - - - - - - - - - -

    public override void Violence(BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppen, DamageType dmgType, float damage, float bonusStun)
    {
        if (hitChunk is not null)
        {
            if (damage >= 0.5f)
            {
                LoseAllGrasps();
            }

            if (damage >= 0.01f &&
                CyanGraphics is not null)
            {
                if (CyanState.shells[hitChunk.index])
                {
                    DropShell(hitChunk);
                }
                if (Random.value < damage * 3f)
                {
                    FlipShellGradientDirections();
                }
            }

            damage *= 4/3f; // Without this, Cyanwing max HP would sit at about 13.33. This effectively brings the HP down to around 10, matching the Red Centipede.
        }

        base.Violence(source, dirAndMomentum, hitChunk, hitAppen, dmgType, damage, bonusStun);

        Debug.Log("damage " + damage + " | source: " + (source?.owner?.abstractPhysicalObject?.type?.value ?? "null"));
    }

    public virtual void ZapGrabber(Creature grabber, BodyChunk grabbedChunk)
    {
        room.AddObject(new ZapCoil.ZapFlash(grabbedChunk.pos, 1));
        room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, grabbedChunk.pos);
        if (dead)
        {
            DropShell(grabbedChunk);
        }

        grabber.LoseAllGrasps();
        grabber.Stun(Random.Range(40, 61));
        room.AddObject(new CreatureSpasmer(grabber, allowDead: false, grabber.stun));
    }
    public virtual void DropShell(BodyChunk chunk)
    {
        if (!CyanState.shells[chunk.index] &&
            shellJustFellOff != chunk.index)
        {
            return;
        }
        CyanState.shells[chunk.index] = false;

        for (int s = 0; s < 2; s++)
        {
            CyanwingState.Shell shell = CyanState.superShells[chunk.index];
            Color shellColor;
            if (s == 0)
            {
                float lightness = Mathf.Lerp(0.85f, 0.75f, CyanState.ClampedHealth) - (0.25f * room.Darkness(chunk.pos) * CyanState.ClampedHealth);
                shellColor = Custom.HSL2RGB(shell.hue, saturation, lightness);
                room.AddObject(new DroppedCyanwingShell(
                    this,
                    chunk.pos,
                    Custom.RNV() * Random.Range(3f, 9f),
                    shellColor,
                    chunk.rad * 0.165f,
                    chunk.rad * 0.143f,
                    Random.value < 0.2f ? 200 : 130,
                    false));
            }
            else
            {
                float lightness = Mathf.Lerp(0.85f, 0.5f, CyanState.ClampedHealth) - (0.2f * room.Darkness(chunk.pos) * CyanState.ClampedHealth);
                shellColor = Custom.HSL2RGB(shell.hue, saturation, lightness);
                room.AddObject(new DroppedCyanwingShell(
                    this,
                    chunk.pos,
                    Custom.RNV() * Random.Range(5f, 15f),
                    shellColor,
                    chunk.rad * 0.165f,
                    chunk.rad * 0.143f,
                    Random.value < 0.2f ? 200 : 130,
                    true));
            }
        }
    }
    public override void Stun(int time)
    {
        time = (int)Mathf.Min(time, Mathf.Lerp(0, 15, CyanState.health));
        base.Stun(time);
    }
    public override void Die()
    {
        if (!dead)
        {
            SelfDestructCountdown += 240;
            scareObj = new FirecrackerPlant.ScareObject(mainBodyChunk.pos);
            scareObj.fearScavs = true;
            room.AddObject(scareObj);
        }
        base.Die();
    }

    public virtual void Vaporize(PhysicalObject unfortunateMotherfucker)
    {
        if (unfortunateMotherfucker is null)
        {
            return;
        }
        BodyChunk sootChunk = unfortunateMotherfucker.bodyChunks[unfortunateMotherfucker.bodyChunks.Length / 2];
        room.PlaySound(SoundID.Centipede_Shock, mainBodyChunk.pos, 1.5f, 1);
        room.PlaySound(SoundID.Zapper_Zap, mainBodyChunk.pos, 1.5f, Random.Range(1.5f, 2.5f));
        room.PlaySound(SoundID.Death_Lightning_Spark_Object, mainBodyChunk.pos, 1.25f, 1);
        room.InGameNoise(new Noise.InGameNoise(mainBodyChunk.pos, 12000f, this, 1f));
        room.AddObject(new SootMark(room, sootChunk.pos, 100f, bigSprite: false));

        if (graphicsModule is not null)
        {
            (graphicsModule as CentipedeGraphics).lightFlash = 1f;
            room.AddObject(new ColorableZapFlash(HeadChunk.pos, 10f, ShortCutColor()));
            for (int s = 0; s < Random.Range(16, 21); s++)
            {
                room.AddObject(new Spark(HeadChunk.pos, Custom.RNV() * Mathf.Lerp(10, 28, Random.value), ShortCutColor(), null, 8, 14));
            }
        }
        for (int j = 0; j < bodyChunks.Length; j++)
        {
            bodyChunks[j].vel += Custom.RNV() * 10f * Random.value;
            bodyChunks[j].pos += Custom.RNV() * 10f * Random.value;
        }

        vaporSmoke = new HailstormFireSmokeCreator(room);
        for (int s = 0; s < 5 * unfortunateMotherfucker.bodyChunks.Length; s++)
        {
            BodyChunk smokeChunk = unfortunateMotherfucker.bodyChunks[Random.Range(0, unfortunateMotherfucker.bodyChunks.Length)];
            if (vaporSmoke.AddParticle(smokeChunk.pos, (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
            {
                vapor.lifeTime = 240f;
                vapor.colorFadeTime = 100;
                vapor.effectColor = ShortCutColor();
                vapor.rad *= 6f;
            }
        }
        vaporSmoke = null;

        if (unfortunateMotherfucker is not Creature ctr)
        {
            return;
        }

        float ElectricResistance = 1;
        float ElecStunResistance = 1;
        if (ctr.Template.damageRestistances[DamageType.Electric.index, 0] > 0)
        {
            ElectricResistance *= ctr.Template.damageRestistances[DamageType.Electric.index, 0];
        }
        if (ctr.Template.damageRestistances[DamageType.Electric.index, 1] > 0)
        {
            ElecStunResistance *= ctr.Template.damageRestistances[DamageType.Electric.index, 1];
        }
        if (CentiHooks.IsIncanStory(room?.game))
        {
            ElectricResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, false);
            ElecStunResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, true);
        }

        bool Vaporize = false;

        if (ctr is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            plr.killTag = abstractCreature;
            plr.PyroDeath();
        }
        else if (TotalMass > ctr.TotalMass * ElectricResistance)
        {
            ctr.killTag = abstractCreature;
            ctr.Die();
            if (HSRemix.CyanwingAtomization.Value && TotalMass / 2f > ctr.TotalMass * ElectricResistance)
            {
                Vaporize = true;
            }
            else
            {
                int spasmTime = (int)(Custom.LerpMap(ctr.TotalMass, TotalMass, TotalMass / 2f, 240, 480) / ElecStunResistance);
                room.AddObject(new CreatureSpasmer(ctr, true, spasmTime));
                if (ctr.State is not null && ctr.State.meatLeft > 0)
                {
                    ctr.State.meatLeft = (int)(ctr.State.meatLeft * Mathf.InverseLerp(TotalMass / 2f, TotalMass, ctr.TotalMass));
                }
                ctr.Hypothermia -= 2f / ElectricResistance;
            }
        }
        else
        {
            int spasmTime = (int)(Custom.LerpMap(ctr.TotalMass, TotalMass * 2f, TotalMass, 80, 240) / ElecStunResistance);
            room.AddObject(new CreatureSpasmer(ctr, true, spasmTime));
            ctr.Stun(spasmTime);
            ctr.LoseAllGrasps();
            ctr.Hypothermia -= 1 / ElectricResistance;

            shockGiveUpCounter = Math.Max(shockGiveUpCounter, 30);
            AI.annoyingCollisions = Math.Min(AI.annoyingCollisions / 2, 150);
        }

        if (ctr is Chillipede chl)
        {
            int[] shells = new int[chl.bodyChunks.Length];
            for (int s = 0; s < shells.Length; s++)
            {
                shells[s] = s;
            }
            chl.DamageChillipedeShells(shells, 10, HeadChunk);
        }

        if (!Vaporize)
        {
            for (int k = 0; k < unfortunateMotherfucker.bodyChunks.Length; k++)
            {
                unfortunateMotherfucker.bodyChunks[k].vel += Custom.RNV() * 12f * Random.value;
                unfortunateMotherfucker.bodyChunks[k].pos += Custom.RNV() * 12f * Random.value;
            }
        }

        Stun(15);

        if (ctr.Submersion > 0f)
        {
            room.AddObject(new UnderwaterShock(room, this, HeadChunk.pos, 20, 2000f * size, 3f * size, this, ShortCutColor()));
        }

        foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null)
            {
                continue;
            }

            Creature collateral = absCtr.realizedCreature;

            if (collateral == ctr ||
                collateral == this ||
                !room.VisualContact(HeadChunk.pos, collateral.DangerPos) ||
                !Custom.DistLess(HeadChunk.pos, collateral.DangerPos, 300))
            {
                continue;
            }

            ElectricResistance = 1;
            ElecStunResistance = 1;
            if (collateral.Template.damageRestistances[DamageType.Electric.index, 0] > 0)
            {
                ElectricResistance *= collateral.Template.damageRestistances[DamageType.Electric.index, 0];
            }
            if (collateral.Template.damageRestistances[DamageType.Electric.index, 1] > 0)
            {
                ElecStunResistance *= collateral.Template.damageRestistances[DamageType.Electric.index, 1];
            }
            if (CentiHooks.IsIncanStory(room?.game))
            {
                ElectricResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, false);
                ElecStunResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, true);
            }

            float RangeFac = Mathf.InverseLerp(300, 30, Custom.Dist(HeadChunk.pos, collateral.DangerPos));
            collateral.Hypothermia -= RangeFac / ElectricResistance;
            int spasmTime = (int)(Custom.LerpMap(collateral.TotalMass, TotalMass * 2f, TotalMass, 80, 240) * RangeFac / ElecStunResistance);
            collateral.Stun(spasmTime);
            room.AddObject(new CreatureSpasmer(collateral, true, spasmTime));

            if (collateral is Chillipede chl2)
            {
                int[] shells = new int[chl2.bodyChunks.Length];
                for (int s = 0; s < shells.Length; s++)
                {
                    shells[s] = s;
                }
                chl2.DamageChillipedeShells(shells, (int)(10 * RangeFac), HeadChunk);
            }
        }

        if (Vaporize)
        {
            ctr.Destroy();
        }
        else for (int k = 0; k < unfortunateMotherfucker.bodyChunks.Length; k++)
            {
                unfortunateMotherfucker.bodyChunks[k].vel += Custom.RNV() * 12f * Random.value;
                unfortunateMotherfucker.bodyChunks[k].pos += Custom.RNV() * 12f * Random.value;
            }

    }
    public virtual void SelfDestruct()
    {
        Vector2 BoomPos = mainBodyChunk.pos;
        room.InGameNoise(new Noise.InGameNoise(BoomPos, 24000f, this, 4f));

        room.PlaySound(SoundID.Bomb_Explode, BoomPos, 2f, 1.1f);
        room.PlaySound(SoundID.Zapper_Zap, BoomPos, 2f, Random.Range(1.5f, 2.5f));
        room.PlaySound(SoundID.Death_Lightning_Spark_Object, BoomPos, 2.5f, 1);
        room.PlaySound(SoundID.Centipede_Shock, BoomPos, 1.5f, 1);

        room.AddObject(new ColorableZapFlash(BoomPos, 50f, ShortCutColor()));
        room.AddObject(new ShockWave(BoomPos, 600, 1.5f, 15));
        room.AddObject(new Explosion(room, this, BoomPos, 1, 350, 10, 0, 0, 0, this, 0, 0, 0));
        room.AddObject(new SootMark(room, BoomPos, 300f, bigSprite: true));

        room.ScreenMovement(BoomPos, default, 1.8f);

        if (CyanGraphics is not null)
        {
            CyanGraphics.lightFlash = 1f;
        }

        vaporSmoke = new HailstormFireSmokeCreator(room);
        bool WaterShock = false;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            CentiState.shells[b] = false;
            bodyChunks[b].vel += Custom.RNV() * 20f * Random.value;
            bodyChunks[b].pos += Custom.RNV() * 20f * Random.value;
            room.AddObject(new ColorableElectrodeathSpark(bodyChunks[b].pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), ShortCutColor()));
            if (vaporSmoke.AddParticle(bodyChunks[b].pos, (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
            {
                vapor.lifeTime = 240f;
                vapor.colorFadeTime = 100;
                vapor.effectColor = ShortCutColor();
                vapor.rad *= 12;
            }

            if (!WaterShock && bodyChunks[b].submersion > 0.33f)
            {
                WaterShock = true;
                room.AddObject(new UnderwaterShock(room, this, bodyChunks[b].pos, 40, 3600f * size, 10f * size, this, ShortCutColor()));
            }
        }

        foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null ||
                absCtr.realizedCreature == this)
            {
                continue;
            }

            Creature UnfortunateMotherfucker = absCtr.realizedCreature;

            for (int c = 0; c < UnfortunateMotherfucker.bodyChunks.Length; c++)
            {
                BodyChunk chunk = UnfortunateMotherfucker.bodyChunks[c];
                if (!room.VisualContact(HeadChunk.pos, chunk.pos) ||
                    !Custom.DistLess(HeadChunk.pos, chunk.pos, 350))
                {
                    continue;
                }

                float RangeFac = Mathf.Max(0, 1f - (Custom.Dist(BoomPos, chunk.pos) / 300f));
                float ElectricResistance = 1;
                float ElecStunResistance = 1;
                if (UnfortunateMotherfucker.Template.damageRestistances[DamageType.Electric.index, 0] > 0)
                {
                    ElectricResistance *= UnfortunateMotherfucker.Template.damageRestistances[DamageType.Electric.index, 0];
                }
                if (UnfortunateMotherfucker.Template.damageRestistances[DamageType.Electric.index, 1] > 0)
                {
                    ElecStunResistance *= UnfortunateMotherfucker.Template.damageRestistances[DamageType.Electric.index, 1];
                }
                if (CentiHooks.IsIncanStory(room?.game))
                {
                    ElectricResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(UnfortunateMotherfucker.Template, DamageType.Electric, false);
                    ElecStunResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(UnfortunateMotherfucker.Template, DamageType.Electric, true);
                }

                for (int b = 0; b < UnfortunateMotherfucker.bodyChunks.Length; b++)
                {
                    BodyChunk smokeChunk = UnfortunateMotherfucker.bodyChunks[b];
                    int smokeCount = Random.Range(2, 5);
                    for (int s = 0; s < smokeCount; s++)
                    {
                        if (vaporSmoke.AddParticle(smokeChunk.pos + (Custom.RNV() * Random.Range(8f, 12f)), (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
                        {
                            vapor.colorFadeTime = 100;
                            vapor.rad *= Mathf.Max(4f, smokeChunk.rad / 2f);
                        }
                    }
                }

                bool Vaporize = false;
                int spasmTime = (int)(Custom.LerpMap(UnfortunateMotherfucker.TotalMass / Mathf.Pow(RangeFac, 0.25f), TotalMass * 2f, TotalMass / 2f, 120, 480) / ElecStunResistance);

                if (TotalMass * 1.5f * RangeFac > UnfortunateMotherfucker.TotalMass * ElectricResistance)
                {
                    if (UnfortunateMotherfucker is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                    {
                        plr.killTag = abstractCreature;
                        plr.PyroDeath();
                    }
                    else
                    {
                        UnfortunateMotherfucker.killTag = abstractCreature;
                        UnfortunateMotherfucker.Die();
                    }

                    if (HSRemix.CyanwingAtomization.Value &&
                        TotalMass * 0.75f * RangeFac > UnfortunateMotherfucker.TotalMass * ElectricResistance)
                    {
                        Vaporize = true;
                    }
                    else
                    {
                        room.AddObject(new CreatureSpasmer(UnfortunateMotherfucker, true, spasmTime));
                        if (UnfortunateMotherfucker.State is not null && UnfortunateMotherfucker.State.meatLeft > 0)
                        {
                            UnfortunateMotherfucker.State.meatLeft = (int)(UnfortunateMotherfucker.State.meatLeft * Mathf.InverseLerp(TotalMass / 2f, TotalMass, UnfortunateMotherfucker.TotalMass) * RangeFac);
                        }
                        UnfortunateMotherfucker.Hypothermia -= 2f / ElectricResistance * RangeFac;
                    }
                }
                else
                {
                    room.AddObject(new CreatureSpasmer(UnfortunateMotherfucker, true, spasmTime));
                    UnfortunateMotherfucker.Stun(spasmTime);
                    UnfortunateMotherfucker.LoseAllGrasps();
                    UnfortunateMotherfucker.Hypothermia -= 1 / ElectricResistance * RangeFac;
                }

                if (UnfortunateMotherfucker is Chillipede chl)
                {
                    int[] shells = new int[chl.bodyChunks.Length];
                    for (int s = 0; s < shells.Length; s++)
                    {
                        shells[s] = s;
                    }
                    chl.DamageChillipedeShells(shells, (int)(15 * RangeFac), HeadChunk);
                }

                room.PlaySound(SoundID.Centipede_Shock, mainBodyChunk.pos, 2f * RangeFac, 1);
                room.PlaySound(SoundID.Death_Lightning_Spark_Object, mainBodyChunk.pos, 2f * RangeFac, 1);

                if (Vaporize)
                {
                    UnfortunateMotherfucker.Destroy();
                }
                break;
            }
        }

        vaporSmoke = null;

    }

    public override Color ShortCutColor()
    {
        return Custom.HSL2RGB((minHue + maxHue)/2f, saturation, 0.5f);
    }

    // - - - - - - - - - - - - - - - - - - - -

    public virtual void EmitAmbientSpark()
    {
        room.AddObject(new AmbientSpark(bodyChunks[Random.Range(0, bodyChunks.Length)].pos, ShortCutColor(), Random.value < 0.5f));
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------