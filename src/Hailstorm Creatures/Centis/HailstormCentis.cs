using System;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using RWCustom;
using MoreSlugcats;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Net.NetworkInformation;
using System.Linq;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

namespace Hailstorm;

//------------------------------------------------------------------------

public class ChillipedeState : Centipede.CentipedeState
{
    public int[] ScaleRegenTime;
    public Color scaleColor;
    public Color accentColor;
    public List<int[]> ScaleSprites;
    public List<int> ScaleStages;
    public int StartOfNewSprites;
    public int mistTimer = 160;
    public ChillipedeState(AbstractCreature absCtr) : base(absCtr)
    {
    }
}

//------------------------------------------------------------------------

internal class HailstormCentis
{
    public static void Hooks()
    {
        // Main Centi Functions
        new Hook(typeof(Centipede).GetMethod("get_Centiwing", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.abstractCreature.creatureTemplate.type == HailstormEnums.Cyanwing || orig(cnt));
        new Hook(typeof(Centipede).GetMethod("get_Small", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.abstractCreature.creatureTemplate.type == HailstormEnums.InfantAquapede || orig(cnt));
        new Hook(typeof(Centipede).GetMethod("get_AquaCenti", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.abstractCreature.creatureTemplate.type == HailstormEnums.InfantAquapede || orig(cnt));
        new Hook(typeof(Centipede).GetMethod("get_FoodPoints", Public | NonPublic | Instance), (Func<Centipede, int> orig, Centipede cnt) => cnt.abstractCreature.creatureTemplate.type == HailstormEnums.InfantAquapede ? 3 : orig(cnt));

        On.Centipede.ctor += WinterCentipedes;

        On.Centipede.Update += HailstormCentiUpdate;
        On.Centipede.UpdateGrasp += ChillipedeUpdateGrasp;
        On.Centipede.Crawl += CyanwingCrawling;

        On.Centipede.Violence += DMGvsCentis;
        On.Centipede.Stun += CentiStun;
        On.Centipede.BitByPlayer += ConsumeChild;
        On.Centipede.Shock += ChillipedeZappage;
        On.Centipede.Die += CyanwingSelfDestruct;
        ModifiedCentiStuff();

        // Centi AI
        On.CentipedeAI.ctor += HailstormCentiwingAI;
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += CentiwingBravery;
        On.CentipedeAI.Update += CyanwingAggression;

        // Centi Graphics
        On.CentipedeGraphics.ctor += WinterCentipedeColors;
        On.CentipedeGraphics.ApplyPalette += WintercentiPalette;
        On.CentipedeGraphics.InitiateSprites += WintercentiColorableAntennae;
        On.CentipedeGraphics.DrawSprites += WintercentiColoration;
        On.CentipedeGraphics.AddToContainer += WintercentiSpriteLayering;
        On.Centipede.ShortCutColor += WinterCentiShortcutColors;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSSlugs.Incandescent);
    }

    //-----------------------------------------

    public static ConditionalWeakTable<Centipede, CentiInfo> CentiData = new();
    public static void WinterCentipedes(On.Centipede.orig_ctor orig, Centipede cnt, AbstractCreature absCnt, World world)
    {
        bool meatNotSetYet = false;
        if (absCnt.state is Centipede.CentipedeState CS && !CS.meatInitated)
        {
            meatNotSetYet = true;
        }

        orig(cnt, absCnt, world);

        CreatureTemplate.Type type = absCnt.creatureTemplate.type;

        if (type == HailstormEnums.InfantAquapede || type == HailstormEnums.Cyanwing || type == HailstormEnums.Chillipede || (IsIncanStory(world?.game) && (type == CreatureTemplate.Type.RedCentipede || type == CreatureTemplate.Type.Centiwing || type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)))
        {
            Random.State state = Random.state;
            Random.InitState(absCnt.ID.RandomSeed);
            float sizeFac = 0.5f;
            if (type == CreatureTemplate.Type.Centiwing)
            {
                cnt.size = Random.Range(0.4f, 0.8f);
                sizeFac = Mathf.InverseLerp(0.4f, 0.8f, cnt.size);
            }
            else
            if (type == HailstormEnums.InfantAquapede)
            {
                cnt.size = 0.2f;
            }
            else
            if (type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
            {
                cnt.size = Random.Range(0.6f, 1.2f);
                sizeFac = Mathf.InverseLerp(0.6f, 1.2f, cnt.size);
            }
            else
            if (type == CreatureTemplate.Type.RedCentipede ||
                type == HailstormEnums.Cyanwing)
            {
                cnt.size = Random.Range(0.9f, 1.1f);
                sizeFac = Mathf.InverseLerp(0.9f, 1.1f, cnt.size);
            }
            else if (type == HailstormEnums.Chillipede)
            {
                cnt.size = 0.3f;
                sizeFac = 1;

            }

            if (absCnt.spawnData is not null &&
                absCnt.spawnData.Length > 2 &&
                (type == CreatureTemplate.Type.Centiwing ||
                type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti))
            {
                string s = absCnt.spawnData.Substring(1, absCnt.spawnData.Length - 2);
                try
                {
                    cnt.size = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    sizeFac =
                        type == CreatureTemplate.Type.Centiwing ?
                        Mathf.InverseLerp(0.4f, 0.8f, cnt.size) :
                        Mathf.InverseLerp(0.6f, 1.2f, cnt.size);
                }
                catch
                {
                    // rip lmao
                }
            }

            Random.state = state;

            if (meatNotSetYet)
            {
                if (type == CreatureTemplate.Type.Centiwing)
                {
                    absCnt.state.meatLeft =
                        Mathf.RoundToInt(Mathf.Lerp(2.3f, 4, sizeFac));
                }
                else if (type == HailstormEnums.Cyanwing)
                {
                    absCnt.state.meatLeft = 12;
                }
                else if (type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
                {
                    absCnt.state.meatLeft =
                        Mathf.RoundToInt(Mathf.Lerp(2.3f, 8, sizeFac));
                }
                else if (type == HailstormEnums.Chillipede)
                {
                    absCnt.state.meatLeft = 6;
                }
            }
            else if (type == HailstormEnums.InfantAquapede)
            {
                cnt.bites = 7;
            }

            cnt.bodyChunks = new BodyChunk[
                type == HailstormEnums.InfantAquapede ? 7 :
                type == HailstormEnums.Chillipede ? 5 :
                type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti ? (int)Mathf.Lerp(7f, 27f, sizeFac) :
                type == CreatureTemplate.Type.RedCentipede ? (int)Mathf.Lerp(17.33f, 19.66f, sizeFac) :
                (int)Mathf.Lerp(7f, 17f, cnt.size)];

            for (int i = 0; i < cnt.bodyChunks.Length; i++)
            {
                float num = i / (float)(cnt.bodyChunks.Length - 1);
                float chunkRad =
                    Mathf.Lerp(
                        Mathf.Lerp(2f, 3.5f, cnt.size),
                        Mathf.Lerp(4f, 6.5f, cnt.size),
                        Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * num), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, cnt.size)));
                float chunkMass =
                    Mathf.Lerp(3f / 70f, 11f / 34f, Mathf.Pow(cnt.size, 1.4f));

                if (type == CreatureTemplate.Type.RedCentipede)
                {
                    chunkRad += 1.5f;
                }
                else if (type == CreatureTemplate.Type.Centiwing)
                {
                    chunkRad = Mathf.Lerp(chunkRad, Mathf.Lerp(2f, 3.5f, chunkRad), 0.4f);
                }
                else if (type == HailstormEnums.Cyanwing)
                {
                    chunkRad += 0.3f;
                }
                else if (type == HailstormEnums.Chillipede)
                {
                    chunkRad *= 3f;
                    chunkMass *= 5f;
                }

                cnt.bodyChunks[i] = new(cnt, i, new Vector2(0f, 0f), chunkRad, chunkMass);
                cnt.bodyChunks[i].loudness = 0;

            }

            if (type == CreatureTemplate.Type.RedCentipede || type == HailstormEnums.Cyanwing || type == HailstormEnums.Chillipede)
            {
                if (type == HailstormEnums.Chillipede)
                {
                    cnt.bodyChunks[0].rad *= 1.6f;
                    cnt.bodyChunks[2].rad *= 0.9f;
                    cnt.bodyChunks[4].rad *= 1.6f;
                }
                else for (int j = 0; j < cnt.bodyChunks.Length; j++)
                    {
                        cnt.bodyChunks[j].mass +=
                            0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, cnt.bodyChunks.Length - 1, j) * Mathf.PI)) * (type == HailstormEnums.Cyanwing ? 0.5f : 1);
                    }
            }
            cnt.mainBodyChunkIndex = cnt.bodyChunks.Length / 2;

            if (cnt.CentiState is not null && (cnt.CentiState.shells is null || cnt.CentiState.shells.Length != cnt.bodyChunks.Length))
            {
                cnt.CentiState.shells = new bool[cnt.bodyChunks.Length];
                for (int k = 0; k < cnt.CentiState.shells.Length; k++)
                {
                    cnt.CentiState.shells[k] =
                        type == HailstormEnums.Chillipede ? true : Random.value < (cnt.Red ? 0.9f : 0.97f);
                }
                if (cnt.CentiState is ChillipedeState cS)
                {
                    cS.ScaleRegenTime = new int[cS.shells.Length];
                    cS.ScaleSprites = new();
                    cS.ScaleStages = new();
                    for (int b = 0; b < cS.shells.Length; b++)
                    {
                        cS.ScaleStages.Add(0);
                    }
                    Random.State chlState = Random.state;
                    Random.InitState(absCnt.ID.RandomSeed);
                    for (int b = 0; b < cS.shells.Length; b++)
                    {
                        cS.ScaleSprites.Add(new int[2] { Random.Range(0, 3), Random.Range(0, 3) });
                    }
                    Random.state = chlState;
                }
            }

            if (cnt.bodyChunkConnections is not null)
            {
                cnt.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[cnt.bodyChunks.Length * (cnt.bodyChunks.Length - 1) / 2];
                int chunkConNum = 0;
                for (int l = 0; l < cnt.bodyChunks.Length; l++)
                {
                    for (int m = l + 1; m < cnt.bodyChunks.Length; m++)
                    {
                        cnt.bodyChunkConnections[chunkConNum] = new(cnt.bodyChunks[l], cnt.bodyChunks[m], (cnt.bodyChunks[l].rad + cnt.bodyChunks[m].rad) * 1.1f, PhysicalObject.BodyChunkConnection.Type.Push, 1f - (cnt.AquaCenti ? 0.7f : 0f), -1f);
                        chunkConNum++;
                    }
                }
            }
        }

        if (!CentiData.TryGetValue(cnt, out _) &&
            (IsIncanStory(world?.game) || type == HailstormEnums.InfantAquapede || type == HailstormEnums.Cyanwing || type == HailstormEnums.Chillipede))
        {
            CentiData.Add(cnt, new CentiInfo(cnt));
        }
        
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Main Centi Functions
    //----Update----//
    public static void HailstormCentiUpdate(On.Centipede.orig_Update orig, Centipede cnt, bool eu)
    {
        if (cnt is null || !CentiData.TryGetValue(cnt, out CentiInfo cI))
        {
            orig(cnt, eu);
            return;
        }

        if (cnt.shockCharge > 0 && !cnt.safariControlled)
        {
            cI.Charge = cnt.shockCharge;
            PhysicalObject target = null;
            if (cnt.grabbedBy.Count > 0 && !cnt.dead && cnt.Small)
            {
                cI.Charge += 1/60f;
                if (cI.Charge >= 1 && cnt.grabbedBy[0].grabber is not null)
                {
                    target = cnt.grabbedBy[0].grabber;
                }
            }
            if (cI.Charge > 0)
            {
                for (int g = 0; g < cnt.grasps.Length; g++)
                {
                    if (cnt.grasps[g]?.grabbed is null)
                    {
                        continue;
                    }
                    BodyChunk otherChunk = cnt.bodyChunks[(g == 0) ? (cnt.bodyChunks.Length - 1) : 0];
                    for (int i = 0; i < cnt.grasps[g].grabbed.bodyChunks.Length; i++)
                    {
                        PhysicalObject grabbed = cnt.grasps[g].grabbed;
                        if (grabbed is null)
                        {
                            continue;
                        }
                        cI.Charge += 1/Mathf.Lerp(100f, 5f, cnt.size);
                        if (cI.Charge >= 1)
                        {
                            target = grabbed;
                        }
                    }
                }
            }
            if (target is not null)
            {
                cnt.shockCharge = 0; 
                if (cI.Chillipede)
                {
                    Freeze(cnt, target);
                }
                else if (cI.Cyanwing)
                {
                    Vaporize(cnt, cI, target);
                }
                else
                {
                    Fry(cnt, target);
                }
            }
        }

        orig(cnt, eu);

        if (cnt.room is null || cnt.graphicsModule is null || cnt.graphicsModule is not CentipedeGraphics cg)
        {
            return;
        }

        if (cnt.AquaCenti && cnt.lungs < 0.005f)
        {
            cnt.lungs = 1;
        }

        if (cI.BabyAquapede)
        {
            if (cnt.grabbedBy.Count > 0 && !cnt.dead)
            {
                cnt.shockCharge -= 0.2f / 60f;
            }
        }
        else if (cnt.CentiState is ChillipedeState cS)
        {
            ChillipedeUpdate(cnt, cS);
        }
        else if (cI.Cyanwing)
        {
            CyanwingUpdate(cnt, cI, cg);
        }
        
    }
    public static void CyanwingUpdate(Centipede cnt, CentiInfo cI, CentipedeGraphics cg)
    {
        bool grabby = false;
        for (int g = 0; g < cnt.grasps.Length; g++)
        {
            if (cnt.grasps[g] is not null)
            {
                grabby = true;
                break;
            }
        }
        if (!grabby || Random.value < 0.33f)
        {
            cnt.shockGiveUpCounter--;
        }


        if (cnt.grabbedBy.Count > 0 && cnt.grabbedBy[0]?.grabbedChunk is not null && cnt.CentiState is not null && (cnt.CentiState.shells[cnt.grabbedBy[0].grabbedChunk.index] || cnt.shellJustFellOff == cnt.grabbedBy[0].grabbedChunk.index))
        {
            cnt.room.AddObject(new ZapCoil.ZapFlash(cnt.grabbedBy[0].grabbedChunk.pos, 1));
            cnt.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, cnt.grabbedBy[0].grabbedChunk.pos);
            if (cnt.dead)
            {
                if (cnt.CentiState.shells[cnt.grabbedBy[0].grabbedChunk.index])
                {
                    cnt.CentiState.shells[cnt.grabbedBy[0].grabbedChunk.index] = false;
                }
                for (int j = 0; j < 2; j++)
                {
                    Color shellColor =
                        j == 0 ?
                        new HSLColor(cI.segmentHues[cnt.grabbedBy[0].grabbedChunk.index], cg.saturation, 0.5f).rgb :
                        Color.Lerp(Color.Lerp(Custom.HSL2RGB(cI.segmentHues[cnt.grabbedBy[0].grabbedChunk.index], cg.saturation, 0.625f), cg.blackColor, 0.5f), new Color(0.4392f, 0.0745f, 0f), 0.25f);

                    CyanwingShell cyanwingShell =
                        j == 0 ?
                        new(cnt, cnt.grabbedBy[0].grabbedChunk.pos, Custom.RNV() * Random.Range(3f,  9f), shellColor, cnt.grabbedBy[0].grabbedChunk.rad * 0.15f, cnt.grabbedBy[0].grabbedChunk.rad * 0.13f, Random.value < 0.2f ? 200 : 130) :
                        new(cnt, cnt.grabbedBy[0].grabbedChunk.pos, Custom.RNV() * Random.Range(5f, 15f), shellColor, cnt.grabbedBy[0].grabbedChunk.rad * 0.15f, cnt.grabbedBy[0].grabbedChunk.rad * 0.13f, Random.value < 0.2f ? 200 : 130);

                    cnt.room.AddObject(cyanwingShell);
                }
            }

            Creature grabber = cnt.grabbedBy[0].grabber;
            grabber.LoseAllGrasps();
            grabber.Stun(Random.Range(40, 61));
            cnt.room.AddObject(new CreatureSpasmer(grabber, allowDead: true, grabber.stun));
        }
        if (cnt.shellJustFellOff != -1)
        {
            cnt.shellJustFellOff = -1;
        }

        if (cnt.Glower is null)
        {
            cnt.GlowerHead = cnt.HeadChunk;
            cnt.Glower = new LightSource(cnt.GlowerHead.pos, environmentalLight: false, Custom.HSL2RGB(cg.hue, cg.saturation, 0.625f), cnt);
            cnt.room.AddObject(cnt.Glower);
            cnt.Glower.alpha = 0;
            cnt.Glower.rad = 0;
            cnt.Glower.submersible = true;
        }
        else if (cnt.Glower is not null)
        {
            if (cnt.GlowerHead == cnt.HeadChunk && !cnt.Stunned && cnt.shockCharge < 0.2f && cnt.Consious)
            {
                if (cnt.Glower.rad < 300f)
                {
                    cnt.Glower.rad += 11f;
                }
                if (cnt.Glower.Alpha < 0.5f)
                {
                    cnt.Glower.alpha += 0.2f;
                }
            }
            else
            {
                if (cnt.Glower.rad > 0f)
                {
                    cnt.Glower.rad -= 5f;
                }
                if (cnt.Glower.Alpha > 0f)
                {
                    cnt.Glower.alpha -= 0.05f;
                }
                if (cnt.Glower.Alpha <= 0f && cnt.Glower.rad <= 0f)
                {
                    cnt.room.RemoveObject(cnt.Glower);
                    cnt.Glower = null;
                }
            }
            if (cnt.Glower is not null)
            {
                cnt.Glower.pos = cnt.GlowerHead.pos;
            }
        }

        if (Random.value < (cnt.dead ? 0.02f : Mathf.Lerp(0.06f, 0.03f, cnt.CentiState.ClampedHealth)))
        {
            GreenSparks.GreenSpark notgreenSpark =
                new(cnt.bodyChunks[Random.Range(0, cnt.bodyChunks.Length)].pos)
                { 
                    col = cnt.ShortCutColor()
                };
            cnt.room.AddObject(notgreenSpark);
        }

        if (cnt.dead)
        {
            if (cI.SelfDestruct > 0)
            {
                cI.SelfDestruct++;
                cI.SparkCounter++;
                float sparkFac = cI.SelfDestruct < 210 ?
                    Custom.LerpMap(cI.SelfDestruct,   0, 210, 60, 20) :
                    Custom.LerpMap(cI.SelfDestruct, 210, 240,  5,  1);
                if (cI.SparkCounter > sparkFac && Random.value < 0.33f)
                {
                    float boomFac = Mathf.Lerp(1, 1.33f, cI.SelfDestruct/240f);
                    BodyChunk randomChunk = cnt.bodyChunks[Random.Range(0, cnt.bodyChunks.Length)];
                    cnt.room.InGameNoise(new Noise.InGameNoise(randomChunk.pos, 6000f * boomFac, cnt, 4f));
                    cnt.room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, randomChunk.pos, 1, Random.Range(0.75f, 1.25f) * boomFac);
                    cnt.room.AddObject(new CyanwingSpark(randomChunk.pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), cnt.ShortCutColor()));
                    cnt.room.AddObject(new CyanwingSpark(randomChunk.pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), cnt.ShortCutColor()));
                    cI.SparkCounter = 0;
                }
                if (cI.SelfDestruct > 240)
                {
                    cI.SelfDestruct = 0;
                    CyanwingExplosion(cnt, cI);
                }
            }
            return;
        }

        if (cnt.CentiState is not null &&
            cI.segmentHues is not null &&
            cI.segmentGradientDirections is not null &&
            cI.segmentHues.Length == cI.segmentGradientDirections.Length)
        {
            for (int s = 0; s < cnt.bodyChunks.Length; s++)
            {
                if (!cnt.CentiState.shells[s])
                {
                    continue;
                }

                if (cI.segmentHues[s] >= cg.hue + 50 / 360f)
                {
                    cI.segmentGradientDirections[s] = true;
                }
                else if (cI.segmentHues[s] <= cg.hue)
                {
                    cI.segmentGradientDirections[s] = false;
                }

                cI.segmentHues[s] +=
                    (cnt.Stunned ? 0.5f / 360f : 1 / 360f) * (cI.segmentGradientDirections[s] ? -1 : 1);

                if (Random.value < Mathf.Lerp(0.05f, 0.01f ,cnt.CentiState.ClampedHealth))
                {
                    cnt.room.AddObject(new Spark(cnt.bodyChunks[s].pos, Custom.RNV() * Random.Range(16f, 24f), new HSLColor(cI.segmentHues[s], cg.saturation, 0.5f).rgb, null, 4, 50));
                }
            }
        }
    }
    public static void ChillipedeUpdate(Centipede cnt, ChillipedeState cS)
    {
        if (cnt.HypothermiaExposure > 0.1f || cnt.Hypothermia > 0.1f)
        {
            cnt.HypothermiaExposure = 0;
            cnt.Hypothermia = 0;
        }
        if (cS.ScaleRegenTime.Length != cnt.bodyChunks.Length)
        {
            Array.Resize(ref cS.ScaleRegenTime, cnt.bodyChunks.Length);
        }
        if (!cnt.dead || (Random.value < cnt.HypothermiaExposure && cnt.room.blizzardGraphics is not null))
        {
            for (int s = 0; s < cS.ScaleRegenTime.Length; s++)
            {
                if (cS.ScaleRegenTime[s] != 0)
                {
                    int regen =
                        cnt.room.blizzardGraphics is null ? 1 :
                        cnt.dead ? (Random.value < cnt.HypothermiaExposure * Mathf.Clamp(cnt.room.blizzardGraphics.SnowfallIntensity, 0.2f, 1) ? 2 : 1) :
                        (int)Mathf.Max(1, Mathf.Lerp(1, 5, cnt.HypothermiaExposure) * Mathf.Clamp(cnt.room.blizzardGraphics.SnowfallIntensity, 0.2f, 1));

                    cS.ScaleRegenTime[s] = Mathf.Max(0, cS.ScaleRegenTime[s] - regen);

                    if (cS.ScaleStages[s] == 1 && cS.ScaleRegenTime[s] == 0)
                    {
                        cS.ScaleStages[s] = 0;
                        for (int sn = 8; sn >= 0; sn--)
                        {
                            cnt.room.AddObject(new HailstormSnowflake(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 6f, cS.scaleColor, cS.accentColor));
                        }
                    }
                    else if (cS.ScaleRegenTime[s] > 0)
                    {
                        if (cS.ScaleStages[s] == 2 && cS.ScaleRegenTime[s] <= 2000)
                        {
                            cS.ScaleStages[s] = 1;
                            for (int sn = 6; sn >= 0; sn--)
                            {
                                cnt.room.AddObject(new HailstormSnowflake(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 6f, cS.scaleColor, cS.accentColor));
                            }
                        }
                        else if (!cnt.CentiState.shells[s] && cS.ScaleRegenTime[s] > 2000 && cS.ScaleRegenTime[s] <= 4000)
                        {
                            cS.ScaleStages[s] = 2;
                            cnt.CentiState.shells[s] = true;
                            for (int sn = 4; sn >= 0; sn--)
                            {
                                cnt.room.AddObject(new HailstormSnowflake(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 6f, cS.scaleColor, cS.accentColor));
                            }
                        }
                    }

                    if (Random.value < 0.00225f)
                    {
                        cnt.room.AddObject(Random.value < 0.5f ?
                            new HailstormSnowflake(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 2f, cS.scaleColor, cS.accentColor) :
                            new PuffBallSkin(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 2f, cS.scaleColor, cS.accentColor));
                    }
                }
                else
                {
                    if (Random.value < 0.0015f)
                    {
                        cnt.room.AddObject(new HailstormSnowflake(cnt.bodyChunks[s].pos, Custom.RNV() * Random.value * 4f, cS.scaleColor, cS.accentColor));
                    }
                }
            }
        }
    }
    public static void ChillipedeUpdateGrasp(On.Centipede.orig_UpdateGrasp orig, Centipede cnt, int grasp)
    {
        orig(cnt, grasp);
        if (cnt is null || cnt.Template.type != HailstormEnums.Chillipede || cnt.grasps[1 - grasp] is not null)
        {
            return;
        }
        BodyChunk otherHead = cnt.bodyChunks[(grasp == 0) ? cnt.bodyChunks.Length - 1 : 0];
        otherHead.vel.y += 0.5f;
    }
    public static void CyanwingCrawling(On.Centipede.orig_Crawl orig, Centipede cnt)
    {
        if (cnt is null || cnt.Template.type != HailstormEnums.Cyanwing)
        {
            orig(cnt);
            return;
        }

        int flyCounter = cnt.flyModeCounter;
        bool wantToFly = cnt.wantToFly;
        Vector2[] bodyChunkVels = new Vector2[cnt.bodyChunks.Length];
        for (int b = 0; b < cnt.bodyChunks.Length; b++)
        {
            bodyChunkVels[b] = cnt.bodyChunks[b].vel;
        }
        orig(cnt);
        cnt.flyModeCounter = flyCounter;
        cnt.wantToFly = wantToFly;

        int segmentsAppliedForceTo = 0;
        for (int b = 0; b < cnt.bodyChunks.Length; b++)
        {
            BodyChunk chunk = cnt.bodyChunks[b];
            chunk.vel = bodyChunkVels[b];
            if (!cnt.AccessibleTile(cnt.room.GetTilePosition(chunk.pos)))
            {
                continue;
            }
            segmentsAppliedForceTo++;
            chunk.vel *= 0.7f;
            chunk.vel.y += cnt.gravity;
            if (b > 0 && !cnt.AccessibleTile(cnt.room.GetTilePosition(cnt.bodyChunks[b - 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += cnt.gravity;
            }
            if (b < cnt.bodyChunks.Length - 1 && !cnt.AccessibleTile(cnt.room.GetTilePosition(cnt.bodyChunks[b + 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += cnt.gravity;
            }
            if (b <= 0 || b >= cnt.bodyChunks.Length - 1)
            {
                continue;
            }
            if (cnt.moving)
            {
                int bodyDirection = (!cnt.bodyDirection ? 1 : -1);
                if (cnt.AccessibleTile(cnt.room.GetTilePosition(cnt.bodyChunks[b + bodyDirection].pos)))
                {
                    chunk.vel += Custom.DirVec(chunk.pos, cnt.bodyChunks[b + bodyDirection].pos) * 1.5f * Mathf.Lerp(0.5f, 1.5f, cnt.size);
                }
                chunk.vel -= Custom.DirVec(chunk.pos, cnt.bodyChunks[b + (bodyDirection * - 1)].pos) * 0.8f * Mathf.Lerp(0.7f, 1.3f, cnt.size);
                continue;
            }
            Vector2 moveDir = chunk.pos - cnt.bodyChunks[b - 1].pos;
            Vector2 moveAngle = moveDir.normalized;
            moveDir = cnt.bodyChunks[b + 1].pos - chunk.pos;
            Vector2 finalMoveDir = (moveAngle + moveDir.normalized) / 2f;
            if (Mathf.Abs(finalMoveDir.x) > 0.5f)
            {
                chunk.vel.y -= (chunk.pos.y - (cnt.room.MiddleOfTile(chunk.pos).y + cnt.VerticalSitSurface(chunk.pos) * (10f - chunk.rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(cnt.size, 1.2f));
            }
            if (Mathf.Abs(finalMoveDir.y) > 0.5f)
            {
                chunk.vel.x -= (chunk.pos.x - (cnt.room.MiddleOfTile(chunk.pos).x + cnt.HorizontalSitSurface(chunk.pos) * (10f - chunk.rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(cnt.size, 1.2f));
            }
        }
        if (segmentsAppliedForceTo > 0)
        {
            cnt.HeadChunk.vel += Custom.DirVec(cnt.HeadChunk.pos, cnt.moveToPos) * Custom.LerpMap(segmentsAppliedForceTo, 0f, cnt.bodyChunks.Length, 6f, 3f) * Mathf.Lerp(0.7f, 1.3f, cnt.size * 0.7f);
        }
        if (segmentsAppliedForceTo == 0)
        {
            cnt.flyModeCounter += 10;
            cnt.wantToFly = true;
        }
    }

    //----Centi Functions----//
    public static void DMGvsCentis(On.Centipede.orig_Violence orig, Centipede cnt, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float damage, float bonusStun)
    {
        if (CentiData.TryGetValue(cnt, out CentiInfo cI))
        {
            if (cnt.CentiState is ChillipedeState cS && cnt.room is not null && hitChunk is not null)
            {
                if (damage >= 0.01f && cnt.CentiState.shells[hitChunk.index])
                {
                    cnt.CentiState.shells[hitChunk.index] = false;
                    if (source?.owner is null || (source.owner is not IceCrystal && (source.owner is not Spear spr || !(spr.bugSpear || spr.abstractSpear is AbstractBurnSpear))))
                    {
                        damage *=
                            cS.ScaleStages[hitChunk.index] == 0 ? 0.2f :
                            cS.ScaleStages[hitChunk.index] == 1 ? 0.6f : 0.85f;
                    }
                    float volume =
                        cS.ScaleStages[hitChunk.index] == 0 ? 1.25f :
                        cS.ScaleStages[hitChunk.index] == 1 ? 1.00f : 0.75f;
                    cS.ScaleStages[hitChunk.index] = 2;
                    cS.ScaleRegenTime[hitChunk.index] = 6000;
                    cnt.room.PlaySound(SoundID.Coral_Circuit_Break, hitChunk.pos, volume, 1.5f);
                    for (int j = 0; j < 18; j++)
                    {
                        cnt.room.AddObject(j % 3 == 1 ?
                                new HailstormSnowflake(hitChunk.pos, Custom.RNV() * Random.value * 12f, cS.scaleColor, cS.accentColor) :
                                new PuffBallSkin(hitChunk.pos, Custom.RNV() * Random.value * 12f, cS.scaleColor, cS.accentColor));
                    }
                }
                if (damage >= 0.25f && dmgType == Creature.DamageType.Electric)
                {
                    for (int chnk = 0; chnk < cnt.CentiState.shells.Length; chnk++)
                    {
                        if (!cnt.CentiState.shells[chnk]) continue;
                        cnt.CentiState.shells[chnk] = false;
                        float volume =
                            cS.ScaleStages[hitChunk.index] == 0 ? 1.25f :
                            cS.ScaleStages[hitChunk.index] == 1 ? 1.00f : 0.75f;
                        cS.ScaleRegenTime[chnk] = Mathf.Min(6000, cS.ScaleRegenTime[chnk] + 3000);
                        cnt.room.PlaySound(SoundID.Coral_Circuit_Break, cnt.bodyChunks[chnk].pos, volume, 1.5f);
                        for (int j = 0; j < 18; j++)
                        {
                            cnt.room.AddObject(j % 3 == 1 ?
                                    new HailstormSnowflake(cnt.bodyChunks[chnk].pos, Custom.RNV() * Random.Range(12f, 24f), cS.scaleColor, cS.accentColor) :
                                    new PuffBallSkin(cnt.bodyChunks[chnk].pos, Custom.RNV() * Random.Range(12f, 24f), cS.scaleColor, cS.accentColor));
                        }
                    }
                }
            }
            else if (cI.Cyanwing && hitChunk is not null && cnt.CentiState is not null)
            {
                if (damage >= 0.5f && !cnt.CentiState.shells[hitChunk.index])
                {
                    cnt.LoseAllGrasps();
                }
                if (damage >= 0.01f &&
                    cnt.graphicsModule is not null &&
                    cnt.graphicsModule is CentipedeGraphics cg)
                {
                    if (cnt.CentiState.shells[hitChunk.index] && cI.segmentHues is not null)
                    {
                        damage *= 0.6f;
                        cnt.CentiState.shells[hitChunk.index] = false;
                        for (int j = 0; j < 2; j++)
                        {
                            Color shellColor =
                                j == 0 ?
                                new HSLColor(cI.segmentHues[hitChunk.index], cg.saturation, 0.5f).rgb :
                                Color.Lerp(Color.Lerp(Custom.HSL2RGB(cI.segmentHues[hitChunk.index], cg.saturation, 0.625f), cg.blackColor, 0.5f), new Color(0.4392f, 0.0745f, 0f), 0.25f);

                            CyanwingShell cyanwingShell =
                                j == 0 ?
                                new(cnt, hitChunk.pos, Custom.RNV() * Random.Range(3f, 9f), shellColor, hitChunk.rad * 0.15f, hitChunk.rad * 0.13f, Random.value < 0.2f ? 200 : 130) :
                                new(cnt, hitChunk.pos, Custom.RNV() * Random.Range(5f, 15f), shellColor, hitChunk.rad * 0.15f, hitChunk.rad * 0.13f, Random.value < 0.2f ? 200 : 130);

                            cnt.room.AddObject(cyanwingShell);
                        }
                    }
                    if (cI.segmentGradientDirections is not null && Random.value < 0.33f)
                    {
                        for (int s = 0; s < cnt.bodyChunks.Length; s++)
                        {
                            if (!cnt.CentiState.shells[s])
                            {
                                continue;
                            }
                            cI.segmentGradientDirections[s] = !cI.segmentGradientDirections[s];
                        }
                    }
                }
            }

        }
        if (source is not null && cnt.Red)
        {
            bool hitArmoredSegment =
                hitChunk is not null &&
                cnt.room is not null &&
                hitChunk.index >= 0 &&
                hitChunk.index < cnt.CentiState.shells.Length &&
                cnt.CentiState.shells[hitChunk.index];

            bool hitByIncan =
                source.owner is Player self &&
                self.SlugCatClass == HSSlugs.Incandescent &&
                damage < 1;

            bool spearedByIncan =
                source.owner is Spear spr &&
                spr.thrownBy is Player plr &&
                plr.SlugCatClass == HSSlugs.Incandescent &&
                damage < 1;

            if (hitArmoredSegment)
            {
                if (source.owner is IceCrystal) // Sets the hit armor chunk as already off before orig is even run, allowing Ice Crystals to completely ignore it.
                {
                    cnt.CentiState.shells[hitChunk.index] = false;
                    if (cnt.graphicsModule is not null && cnt.graphicsModule is CentipedeGraphics cGraphics)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            CentipedeShell centipedeShell = new(hitChunk.pos, dirAndMomentum.Value * Mathf.Lerp(0.7f, 1.6f, Random.value) + Custom.RNV() * Random.value * ((j == 0) ? 3f : 6f), cGraphics.hue, cGraphics.saturation, hitChunk.rad * 1.8f * (1f / 14f) * 1.2f, hitChunk.rad * 1.3f * (1f / 11f) * 1.2f);
                            cnt.room.AddObject(centipedeShell);
                        }
                    }
                    cnt.room.PlaySound(SoundID.Red_Centipede_Shield_Falloff, hitChunk);
                }
                else if (hitByIncan || spearedByIncan) // Allows some more of the Incandescent's movement tricks to be guaranteed to knock off Red Centipede armor. The slide and pounce still can't manage it.
                {
                    damage *= 1.34f;
                }
            }
        }
        orig(cnt, source, dirAndMomentum, hitChunk, hitAppen, dmgType, damage, bonusStun);
    }
    public static void CentiStun(On.Centipede.orig_Stun orig, Centipede cnt, int stun)
    {
        if (cnt is not null && CentiData.TryGetValue(cnt, out CentiInfo cI))
        {
            if (cI.Cyanwing)
            {
                stun = (int)Mathf.Lerp(0, 15, cnt.CentiState.health);
            }
            else if (IsIncanStory(cnt.room?.game))
            {
                if (cnt.Centiwing && !cI.Cyanwing)
                {
                    stun = (int)(stun * 0.75f);
                }
                else if (cnt.AquaCenti)
                {
                    stun *= (int)Mathf.Lerp(1.1f, 0.66f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
                }
            }
        }
        orig(cnt, stun);
    }
    public static void ConsumeChild(On.Centipede.orig_BitByPlayer orig, Centipede cnt, Creature.Grasp grasp, bool eu)
    {
        if (cnt is not null)
        {
            if (cnt.Template.type == HailstormEnums.InfantAquapede && cnt.bites > 1 && cnt.bodyChunks is not null)
            {
                for (int b = 0; b < cnt.bodyChunks.Length; b++)
                {
                    cnt.bodyChunks[b].mass *= 0.85f;
                }
            }
            if (!cnt.dead && cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe && grasp?.grabber is not null)
            {
                cnt.killTag = grasp.grabber.abstractCreature;
            }
        }
        orig(cnt, grasp, eu);
    }
    public static void ChillipedeZappage(On.Centipede.orig_Shock orig, Centipede cnt, PhysicalObject target)
    {
        orig(cnt, target);
        ShockChillipedeArmor(cnt, target);
    }
    public static void CyanwingSelfDestruct(On.Centipede.orig_Die orig, Centipede cnt)
    {
        if (cnt is null || cnt.dead)
        {
            orig(cnt);
            return;
        }

        if (CentiData.TryGetValue(cnt, out CentiInfo cI) && cI.Cyanwing)
        {
            cI.SelfDestruct++;
        }

        if (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe &&
            CWT.AbsCtrData.TryGetValue(cnt.abstractCreature, out AbsCtrInfo aI) &&
            aI.ctrList is not null && aI.ctrList.Count > 0 &&
            aI.ctrList[0]?.abstractAI is not null &&
            cnt.killTag is not null &&
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID) is not null)
        {
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID).like = -1f;
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID).tempLike = -1f;
            aI.ctrList[0].abstractAI.followCreature = cnt.killTag;
            Debug.Log("[Hailstorm] A Cyanwing's comin' after " + cnt.killTag.ToString() + "!");
        }

        orig(cnt);

    }
    

    public static void ModifiedCentiStuff()
    {
        IL.Centipede.Fly += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Centipede cnt) =>
            {
                return CyanwingFlyingAndInfantAquapedeSwimming(cnt);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        }; 
        
        IL.Centipede.Stun += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall<Centipede>("get_Centiwing"),
                x => x.MatchBrfalse(out label)))
            {
                c = new(IL);
                if (c.TryGotoNext(
                    MoveType.Before,
                    x => x.MatchCall(typeof(Random), "get_value"),
                    x => x.MatchLdcR4(0.5f),
                    x => x.MatchBlt(out _)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate((Centipede cnt) => cnt.Template.type != HailstormEnums.Cyanwing);
                    c.Emit(OpCodes.Brfalse, label);
                }
                else
                    Plugin.logger.LogError("[Hailstorm] A Cyanwing IL anti-stun hook (part 2) got totally beaned! Report this, would ya?");
            }
            else
                Plugin.logger.LogError("[Hailstorm] A Cyanwing IL anti-stun hook (part 1) got totally beaned! Report this, would ya?");
        };
        
        IL.CentipedeAI.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeAI>(nameof(CentipedeAI.centipede)),
                x => x.MatchCallvirt<Centipede>("get_Red"),
                x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((CentipedeAI cntAI) => cntAI.centipede.Template.type != HailstormEnums.Cyanwing);
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] A Cyanwing IL hook for prey-tracking is busted! Tell me about it, please!");
        };

        IL.Centipede.UpdateGrasp += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_safariControlled"),
                x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Centipede cnt) => cnt.Template.type != HailstormEnums.Cyanwing);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] A Cyanwing grasp-related IL hook got totally beaned! Report this, would ya?");
        };

        IL.Centipede.Act += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Centipede>(nameof(Centipede.AI)),
                    x => x.MatchCallvirt<ArtificialIntelligence>(nameof(ArtificialIntelligence.Update)))
                &&
                c.TryGotoNext(
                MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Centipede>(nameof(Centipede.flying)),
                    x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Centipede cnt) => cnt.Template.type == HailstormEnums.Cyanwing && cnt.AI.run > 0);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] A Cyanwing IL hook for their crawling stopped functioning! Tell me about this, would ya?");
        };

    }
    public static bool CyanwingFlyingAndInfantAquapedeSwimming(Centipede cnt)
    {
        if (cnt is null || !(cnt.Template.type == HailstormEnums.Cyanwing || cnt.Template.type == HailstormEnums.InfantAquapede)) return false;

        cnt.bodyWave +=
                cnt.Template.type == HailstormEnums.Cyanwing ? 1 : Mathf.Clamp(Vector2.Distance(cnt.HeadChunk.pos, cnt.AI.tempIdlePos.Tile.ToVector2()) / 80f, 0.1f, 1f) * 0.75f;

        for (int i = 0; i < cnt.bodyChunks.Length; i++)
        {
            float num = (float)i / (float)(cnt.bodyChunks.Length - 1);
            if (!cnt.bodyDirection)
            {
                num = 1f - num;
            }
            float num2 = Mathf.Sin((cnt.bodyWave - num * Mathf.Lerp(12f, 28f, cnt.size)) * Mathf.PI * 0.11f);
            cnt.bodyChunks[i].vel *= 0.9f;
            cnt.bodyChunks[i].vel.y += cnt.gravity * cnt.wingsStartedUp;
            if (i <= 0 || i >= cnt.bodyChunks.Length - 1)
            {
                continue;
            }
            Vector2 val = Custom.DirVec(cnt.bodyChunks[i].pos, cnt.bodyChunks[i + (!cnt.bodyDirection ? 1 : -1)].pos);
            Vector2 val2 = Custom.PerpendicularVector(val);
            cnt.bodyChunks[i].vel += val * 0.5f * Mathf.Lerp(0.5f, 1.5f, cnt.size);
            if (cnt.Template.type == HailstormEnums.InfantAquapede && cnt.AI.behavior == CentipedeAI.Behavior.Idle)
            {
                cnt.bodyChunks[i].vel *= Mathf.Clamp(Vector2.Distance(cnt.HeadChunk.pos, cnt.AI.tempIdlePos.Tile.ToVector2()) / 40f, 0.02f, 1f) * 0.7f;
                if (Vector2.Distance(cnt.HeadChunk.pos, cnt.AI.tempIdlePos.Tile.ToVector2()) < 20f)
                {
                    cnt.bodyChunks[i].vel *= 0.28f * 0.7f;
                }
            }
            cnt.bodyChunks[i].pos += val2 * 2.5f * num2;
        }
        if (cnt.room.aimap.getAItile(cnt.moveToPos).terrainProximity > 2)
        {
            cnt.HeadChunk.vel +=
                cnt.AquacentiSwim ?
                Custom.DirVec(cnt.HeadChunk.pos, cnt.moveToPos + Custom.DegToVec(cnt.bodyWave * 5f) * 10f) * 5f * Mathf.Lerp(0.7f, 1.3f, cnt.size) * 0.7f :
                Custom.DirVec(cnt.HeadChunk.pos, cnt.moveToPos + Custom.DegToVec(cnt.bodyWave * 10f) * 60f) * 4f * Mathf.Lerp(0.7f, 1.3f, cnt.size);
        }
        else
        {
            cnt.HeadChunk.vel +=
                cnt.AquacentiSwim ?
                Custom.DirVec(cnt.HeadChunk.pos, cnt.moveToPos) * 1.4f * Mathf.Lerp(0.2f, 0.8f, cnt.size) :
                Custom.DirVec(cnt.HeadChunk.pos, cnt.moveToPos) * 4f * Mathf.Lerp(0.7f, 1.3f, cnt.size);
        }
        return true;
    }

    //----    kill    ----//
    public static void Freeze(Centipede cnt, PhysicalObject freezee)
    {
        if (freezee is null || cnt?.CentiState is null || cnt.CentiState is not ChillipedeState cS)
        {
            return;
        }

        cnt.room.PlaySound(SoundID.Coral_Circuit_Break, cnt.mainBodyChunk.pos, 1.25f, 1.75f);
        cnt.room.PlaySound(SoundID.Coral_Circuit_Break, cnt.mainBodyChunk.pos, 1.25f, 1.00f);
        cnt.room.PlaySound(SoundID.Coral_Circuit_Break, cnt.mainBodyChunk.pos, 1.25f, 0.25f);
        if (cnt.graphicsModule is not null)
        {
            (cnt.graphicsModule as CentipedeGraphics).lightFlash = 1f;
            InsectCoordinator smallInsects = null;
            for (int i = 0; i < cnt.room.updateList.Count; i++)
            {
                if (cnt.room.updateList[i] is InsectCoordinator)
                {
                    smallInsects = cnt.room.updateList[i] as InsectCoordinator;
                    break;
                }
            }
            for (int i = 0; i < Random.Range(14, 19); i++)
            {
                cnt.room.AddObject(new HailstormSnowflake(cnt.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(6, 18, Random.value), cS.scaleColor, cS.accentColor));
                if (i % 2 != 1) cnt.room.AddObject(new FreezerMist(cnt.bodyChunks[Random.Range(0, cnt.bodyChunks.Length - 1)].pos, Custom.RNV() * Random.value * 6f, cS.scaleColor, cS.accentColor, 1.5f, cnt.abstractCreature, smallInsects, true));
            }
        }


        for (int j = 0; j < cnt.bodyChunks.Length; j++)
        {
            cnt.bodyChunks[j].vel += Custom.RNV() * 6f * Random.value;
            cnt.bodyChunks[j].pos += Custom.RNV() * 6f * Random.value;
        }

        if (freezee is not Creature ctr)
        {
            return;
        }

        bool immune =
            ctr.Template.type == HailstormEnums.IcyBlue ||
            ctr.Template.type == HailstormEnums.Freezer ||
            ctr.Template.type == HailstormEnums.Chillipede ||
            ctr.Template.type == HailstormEnums.GorditoGreenie;

        float ColdResistance = ctr.abstractCreature.HypothermiaImmune ? 4 : 1;
        float ColdStunResistance = ColdResistance;
        if (ctr is Player plr && CWT.PlayerData.TryGetValue(plr, out HSSlugs hS))
        {
            ColdResistance /= hS.ColdDMGmult;
        }
        else
        {
            if (ctr.Template.damageRestistances[HailstormEnums.Cold.index, 0] > 0)
            {
                ColdResistance *= ctr.Template.damageRestistances[HailstormEnums.Cold.index, 0];
            }
            if (ctr.Template.damageRestistances[HailstormEnums.Cold.index, 1] > 0)
            {
                ColdStunResistance *= ctr.Template.damageRestistances[HailstormEnums.Cold.index, 1];
            }
        }

        if (ctr is Player inc && inc.SlugCatClass == HSSlugs.Incandescent)
        {
            inc.Die();
            ctr.Hypothermia += 2f / ColdResistance;
        }
        else if (!immune && cnt.TotalMass > ctr.TotalMass / ColdResistance)
        {
            ctr.Die();
            ctr.Hypothermia += 2f / ColdResistance;
        }
        else
        {
            ctr.Stun((int)(200 / ColdStunResistance));
            ctr.LoseAllGrasps();
            ctr.Hypothermia += 1f / ColdResistance;

            cnt.Stun(immune ? 40 : 12);
            cnt.shockGiveUpCounter = Math.Max(cnt.shockGiveUpCounter, 30);
            cnt.AI.annoyingCollisions = immune ? 0 : Math.Min(cnt.AI.annoyingCollisions / 2, 150);
        }

        if (ctr.State is ColdLizState lS && !lS.crystals.All(intact => intact))
        {
            if (ctr.Template.type == HailstormEnums.IcyBlue)
            {
                for (int s = 0; s < lS.crystals.Length; s++)
                {
                    lS.crystals[s] = true;
                }
            }
            else if (ctr.Template.type == HailstormEnums.Freezer)
            {
                for (int s = Random.Range(0, lS.crystals.Length); /**/ ; /**/ )
                {
                    if (lS.crystals[s])
                    {
                        lS.crystals[s] = true;
                        break;
                    }
                    if (s >= lS.crystals.Length) s = 0;
                    else s++;
                }
            }
            lS.armored = true;
        }

        foreach (AbstractCreature absCtr in cnt.room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null ||
                absCtr.realizedCreature == ctr ||
                absCtr.realizedCreature == cnt ||
                !Custom.DistLess(cnt.HeadChunk.pos, absCtr.realizedCreature.DangerPos, 500))
            {
                continue;
            }

            Creature collateralChill = absCtr.realizedCreature;

            immune =
                collateralChill.Template.type == HailstormEnums.IcyBlue ||
                collateralChill.Template.type == HailstormEnums.Freezer ||
                collateralChill.Template.type == HailstormEnums.Chillipede ||
                collateralChill.Template.type == HailstormEnums.GorditoGreenie;

            ColdResistance = absCtr.HypothermiaImmune ? 4 : 1;
            if (collateralChill is Player otherPlr && CWT.PlayerData.TryGetValue(otherPlr, out HSSlugs otherHS))
            {
                ColdResistance /= otherHS.ColdDMGmult;
            }
            else if (collateralChill.Template.damageRestistances[HailstormEnums.Cold.index, 0] > 0)
            {
                ColdResistance *= collateralChill.Template.damageRestistances[HailstormEnums.Cold.index, 0];
            }
            collateralChill.Hypothermia += 1f / ColdResistance * Mathf.InverseLerp(500, 50, Custom.Dist(cnt.HeadChunk.pos, collateralChill.DangerPos));
        }

    }
    public static void Fry(Centipede cnt, PhysicalObject shockee)
    {
        cnt.room.PlaySound(SoundID.Centipede_Shock, cnt.mainBodyChunk.pos);
        if (cnt.graphicsModule is not null)
        {
            (cnt.graphicsModule as CentipedeGraphics).lightFlash = 1f;
            for (int i = 0; i < (int)Mathf.Lerp(4, 8, cnt.size); i++)
            {
                cnt.room.AddObject(new Spark(cnt.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4, 14, Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
            }
        }
        for (int c = 0; c < cnt.bodyChunks.Length; c++)
        {
            cnt.bodyChunks[c].vel += Custom.RNV() * 6f * Random.value;
            cnt.bodyChunks[c].pos += Custom.RNV() * 6f * Random.value;
        }
        for (int s = 0; s < shockee.bodyChunks.Length; s++)
        {
            shockee.bodyChunks[s].vel += Custom.RNV() * 6f * Random.value;
            shockee.bodyChunks[s].pos += Custom.RNV() * 6f * Random.value;
        }
        if (cnt.AquaCenti)
        {
            if (shockee is Creature aquaCtr)
            {
                if (shockee is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    plr.PyroDeath();
                }
                else
                {
                    float dmg = 2f;
                    int stun = 200;
                    if (IsIncanStory(cnt.room.game))
                    {
                        if (aquaCtr.Template.type == CreatureTemplate.Type.YellowLizard ||
                            aquaCtr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                        {
                            dmg *= 2f;
                            stun *= 5;
                        }
                        else if (aquaCtr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                        {
                            dmg *= 5f;
                            stun *= 5;
                        }
                    }
                    aquaCtr.Violence(cnt.mainBodyChunk, default, aquaCtr.mainBodyChunk, null, Creature.DamageType.Electric, dmg, stun);
                    cnt.room.AddObject(new CreatureSpasmer(aquaCtr, false, aquaCtr.stun));
                    aquaCtr.LoseAllGrasps();
                }
            }
            if (shockee.Submersion > 0f)
            {
                cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.HeadChunk.pos, 14, Mathf.Lerp(50, 100, cnt.size), 1, cnt, new Color(0.7f, 0.7f, 1f)));
            }
            return;
        }
        if (shockee is Creature ctr)
        {
            float ElectricResistance = 1;
            float ElecStunResistance = 1;
            if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0] > 0)
            {
                ElectricResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
            }
            if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1] > 0)
            {
                ElecStunResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1];
            }
            if (IsIncanStory(cnt.room.game))
            {
                if (ctr.Template.type == CreatureTemplate.Type.YellowLizard ||
                    ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                {
                    ElectricResistance *= 2f;
                    ElecStunResistance *= 2f;
                }
                else if (ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    ElectricResistance *= 5f;
                    ElecStunResistance *= 5f;
                }
            }

            if (cnt.Small)
            {
                ctr.Stun((int)(120 / ElecStunResistance));
                cnt.room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));
                ctr.LoseAllGrasps();
            }
            else if (ctr.TotalMass > shockee.TotalMass * ElectricResistance)
            {
                if (shockee is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    plr.PyroDeath();
                }
                else
                {
                    ctr.Die();
                    cnt.room.AddObject(new CreatureSpasmer(ctr, true, (int)Mathf.Lerp(70, 120, cnt.size)));
                }
            }
            else
            {
                ctr.Stun((int)(Custom.LerpMap(shockee.TotalMass, 0f, cnt.TotalMass * 2f, 300f, 30f) / ElecStunResistance));
                ctr.LoseAllGrasps();
                cnt.room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));

                cnt.Stun(6);
                cnt.shockGiveUpCounter = Math.Max(cnt.shockGiveUpCounter, 30);
                cnt.AI.annoyingCollisions = Math.Min(cnt.AI.annoyingCollisions / 2, 150);
            }
        }
        if (shockee.Submersion > 0f)
        {
            cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.HeadChunk.pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, cnt.size), 0.2f + 1.9f * cnt.size, cnt, new Color(0.7f, 0.7f, 1f)));
        }
    }
    public static void Vaporize(Centipede cnt, CentiInfo cI, PhysicalObject unfortunateMotherfucker)
    {
        if (unfortunateMotherfucker is null || cnt is null)
        {
            return;
        }
        cnt.room.PlaySound(SoundID.Centipede_Shock, cnt.mainBodyChunk.pos, 1.5f, 1);
        cnt.room.PlaySound(SoundID.Zapper_Zap, cnt.mainBodyChunk.pos, 1.5f, Random.Range(1.5f, 2.5f));
        cnt.room.PlaySound(SoundID.Death_Lightning_Spark_Object, cnt.mainBodyChunk.pos, 1.25f, 1);
        cnt.room.InGameNoise(new Noise.InGameNoise(cnt.mainBodyChunk.pos, 12000f, cnt, 1f));

        if (cnt.graphicsModule is not null)
        {
            (cnt.graphicsModule as CentipedeGraphics).lightFlash = 1f;
            cnt.room.AddObject(new ColorableZapFlash(cnt.HeadChunk.pos, 10f, cnt.ShortCutColor()));
            for (int s = 0; s < Random.Range(16, 21); s++)
            {
                cnt.room.AddObject(new Spark(cnt.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(10, 28, Random.value), cnt.ShortCutColor(), null, 8, 14));
            }
        }
        for (int j = 0; j < cnt.bodyChunks.Length; j++)
        {
            cnt.bodyChunks[j].vel += Custom.RNV() * 10f * Random.value;
            cnt.bodyChunks[j].pos += Custom.RNV() * 10f * Random.value;
        }

        cI.vaporSmoke = new HailstormFireSmokeCreator(cnt.room);
        for (int s = 0; s < 5 * unfortunateMotherfucker.bodyChunks.Length; s++)
        {
            BodyChunk smokeChunk = unfortunateMotherfucker.bodyChunks[Random.Range(0, unfortunateMotherfucker.bodyChunks.Length)];
            if (cI.vaporSmoke.AddParticle(smokeChunk.pos, (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
            {
                vapor.colorFadeTime = 100;
                vapor.effectColor = cnt.ShortCutColor();
                vapor.rad *= Mathf.Max(3f, smokeChunk.rad / 3f);
            }
        }
        cI.vaporSmoke = null;

        if (unfortunateMotherfucker is not Creature ctr)
        {
            return;
        }

        float ElectricResistance = 1;
        float ElecStunResistance = 1;
        if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0] > 0)
        {
            ElectricResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
        }
        if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1] > 0)
        {
            ElecStunResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1];
        }
        if (IsIncanStory(cnt.room?.game))
        {
            if (ctr.Template.type == CreatureTemplate.Type.YellowLizard ||
                ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
            {
                ElectricResistance *= 2f;
                ElecStunResistance *= 2f;
            }
            else if (ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
            {
                ElectricResistance *= 5f;
                ElecStunResistance *= 5f;
            }
        }

        bool Vaporize = false;

        if (ctr is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            plr.PyroDeath();
        }
        else if (cnt.TotalMass > ctr.TotalMass * ElectricResistance)
        {
            ctr.Die();
            if (HSRemix.CyanwingAtomization.Value && cnt.TotalMass/2f > ctr.TotalMass * ElectricResistance)
            {
                Vaporize = true;
            }
            else
            {
                int spasmTime = (int)(Custom.LerpMap(ctr.TotalMass, cnt.TotalMass, cnt.TotalMass / 2f, 240, 480) / ElecStunResistance);
                cnt.room.AddObject(new CreatureSpasmer(ctr, true, spasmTime));
                if (ctr.State is not null && ctr.State.meatLeft > 0)
                {
                    ctr.State.meatLeft = (int)(ctr.State.meatLeft * Mathf.InverseLerp(cnt.TotalMass / 2f, cnt.TotalMass, ctr.TotalMass));
                }
                ctr.Hypothermia -= 2f / ElectricResistance;
            }
        }
        else
        {
            int spasmTime = (int)(Custom.LerpMap(ctr.TotalMass, cnt.TotalMass * 2f, cnt.TotalMass, 80, 240) / ElecStunResistance);
            cnt.room.AddObject(new CreatureSpasmer(ctr, true, spasmTime));
            ctr.Stun(spasmTime);
            ctr.LoseAllGrasps();
            ctr.Hypothermia -= 1 / ElectricResistance;

            cnt.shockGiveUpCounter = Math.Max(cnt.shockGiveUpCounter, 30);
            cnt.AI.annoyingCollisions = Math.Min(cnt.AI.annoyingCollisions / 2, 150);
        }

        ShockChillipedeArmor(cnt, ctr);

        if (!Vaporize)
        {
            for (int k = 0; k < unfortunateMotherfucker.bodyChunks.Length; k++)
            {
                unfortunateMotherfucker.bodyChunks[k].vel += Custom.RNV() * 12f * Random.value;
                unfortunateMotherfucker.bodyChunks[k].pos += Custom.RNV() * 12f * Random.value;
            }
        }

        cnt.Stun(40);

        if (ctr.Submersion > 0f)
        {
            cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.HeadChunk.pos, 20, 2000f * cnt.size, 3f * cnt.size, cnt, cnt.ShortCutColor()));
        }

        foreach (AbstractCreature absCtr in cnt.room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null ||
                absCtr.realizedCreature == ctr ||
                absCtr.realizedCreature == cnt ||
                !Custom.DistLess(cnt.HeadChunk.pos, absCtr.realizedCreature.DangerPos, 300))
            {
                continue;
            }

            Creature collateralZap = absCtr.realizedCreature;
            ElectricResistance = 1;
            ElecStunResistance = 1;
            if (collateralZap.Template.damageRestistances[Creature.DamageType.Electric.index, 0] > 0)
            {
                ElectricResistance *= collateralZap.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
            }
            if (collateralZap.Template.damageRestistances[Creature.DamageType.Electric.index, 1] > 0)
            {
                ElecStunResistance *= collateralZap.Template.damageRestistances[Creature.DamageType.Electric.index, 1];
            }
            if (IsIncanStory(cnt.room?.game))
            {
                if (collateralZap.Template.type == CreatureTemplate.Type.YellowLizard ||
                    collateralZap.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                {
                    ElectricResistance *= 2f;
                    ElecStunResistance *= 2f;
                }
                else if (collateralZap.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    ElectricResistance *= 5f;
                    ElecStunResistance *= 5f;
                }
            }
            float distFac = Mathf.InverseLerp(300, 30, Custom.Dist(cnt.HeadChunk.pos, collateralZap.DangerPos));
            collateralZap.Hypothermia -= distFac / ElectricResistance;
            int spasmTime = (int)(Custom.LerpMap(collateralZap.TotalMass, cnt.TotalMass * 2f, cnt.TotalMass, 80, 240) * distFac / ElecStunResistance);
            collateralZap.Stun(spasmTime);
            cnt.room.AddObject(new CreatureSpasmer(collateralZap, true, spasmTime));

            ShockChillipedeArmor(cnt, collateralZap);
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
    public static void CyanwingExplosion(Centipede cnt, CentiInfo cI)
    {
        if (cnt is null)
        {
            return;
        }

        cnt.room.InGameNoise(new Noise.InGameNoise(cnt.mainBodyChunk.pos, 24000f, cnt, 4f));
        cnt.room.PlaySound(SoundID.Bomb_Explode, cnt.mainBodyChunk.pos, 2f, 1.1f);
        cnt.room.PlaySound(SoundID.Zapper_Zap, cnt.mainBodyChunk.pos, 2f, Random.Range(1.5f, 2.5f));
        cnt.room.PlaySound(SoundID.Death_Lightning_Spark_Object, cnt.mainBodyChunk.pos, 2.5f, 1);
        cnt.room.AddObject(new ColorableZapFlash(cnt.mainBodyChunk.pos, 50f, cnt.ShortCutColor()));
        cnt.room.AddObject(new ShockWave(cnt.mainBodyChunk.pos, 600, 1.5f, 15));
        cnt.room.AddObject(new Explosion(cnt.room, cnt, cnt.mainBodyChunk.pos, 1, 350, 10, 0, 0, 0, cnt, 0, 0, 0));

        if (cnt.graphicsModule is not null)
        {
            (cnt.graphicsModule as CentipedeGraphics).lightFlash = 1f;
        }

        cI.vaporSmoke = new HailstormFireSmokeCreator(cnt.room);
        bool WaterShock = false;
        for (int b = 0; b < cnt.bodyChunks.Length; b++)
        {
            cnt.CentiState.shells[b] = false;
            cnt.bodyChunks[b].vel += Custom.RNV() * 20f * Random.value;
            cnt.bodyChunks[b].pos += Custom.RNV() * 20f * Random.value;
            cnt.room.AddObject(new CyanwingSpark(cnt.bodyChunks[b].pos + (Custom.RNV() * Random.Range(5f, 10f)), 0.5f + (Random.value / 2f), cnt.ShortCutColor()));
            if (cI.vaporSmoke.AddParticle(cnt.bodyChunks[b].pos, (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
            {
                vapor.colorFadeTime = 100;
                vapor.rad *= Mathf.Max(4f, cnt.bodyChunks[b].rad / 2f);
            }

            if (!WaterShock && cnt.bodyChunks[b].submersion > 0.33f)
            {
                WaterShock = true;
                cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.bodyChunks[b].pos, 40, 3600f * cnt.size, 10f * cnt.size, cnt, cnt.ShortCutColor()));
            }
        }

        foreach (AbstractCreature absCtr in cnt.room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null ||
                absCtr.realizedCreature == cnt ||
                !Custom.DistLess(cnt.mainBodyChunk.pos, absCtr.realizedCreature.DangerPos, 350))
            {
                continue;
            }

            float RangeFac = Mathf.Max(0, 1f - (Custom.Dist(cnt.mainBodyChunk.pos, absCtr.realizedCreature.DangerPos) / 300f));
            Creature UnfortunateMotherfucker = absCtr.realizedCreature;
            float ElectricResistance = 1;
            float ElecStunResistance = 1;
            if (UnfortunateMotherfucker.Template.damageRestistances[Creature.DamageType.Electric.index, 0] > 0)
            {
                ElectricResistance *= UnfortunateMotherfucker.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
            }
            if (UnfortunateMotherfucker.Template.damageRestistances[Creature.DamageType.Electric.index, 1] > 0)
            {
                ElecStunResistance *= UnfortunateMotherfucker.Template.damageRestistances[Creature.DamageType.Electric.index, 1];
            }
            if (IsIncanStory(cnt.room?.game))
            {
                if (UnfortunateMotherfucker.Template.type == CreatureTemplate.Type.YellowLizard ||
                    UnfortunateMotherfucker.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                {
                    ElectricResistance *= 2f;
                    ElecStunResistance *= 2f;
                }
                else if (UnfortunateMotherfucker.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    ElectricResistance *= 5f;
                    ElecStunResistance *= 5f;
                }
            }

            for (int b = 0; b < UnfortunateMotherfucker.bodyChunks.Length; b++)
            {
                BodyChunk smokeChunk = UnfortunateMotherfucker.bodyChunks[b];
                int smokeCount = Random.Range(2, 5);
                for (int s = 0; s < smokeCount; s++)
                {
                    if (cI.vaporSmoke.AddParticle(smokeChunk.pos + (Custom.RNV() * Random.Range(8f, 12f)), (Custom.RNV() * Random.Range(8f, 12f)) + new Vector2(0f, 30f), 200) is Smoke.FireSmoke.FireSmokeParticle vapor)
                    {
                        vapor.colorFadeTime = 100;
                        vapor.rad *= Mathf.Max(4f, smokeChunk.rad / 2f);
                    }
                }
            }

            bool Vaporize = false;
            int spasmTime = (int)(Custom.LerpMap(UnfortunateMotherfucker.TotalMass / Mathf.Pow(RangeFac, 0.25f), cnt.TotalMass * 2f, cnt.TotalMass / 2f, 120, 480) / ElecStunResistance);

            if (cnt.TotalMass * 1.5f * RangeFac > UnfortunateMotherfucker.TotalMass * ElectricResistance)
            {
                if (UnfortunateMotherfucker is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    plr.PyroDeath();
                }
                else UnfortunateMotherfucker.Die();

                if (HSRemix.CyanwingAtomization.Value && cnt.TotalMass * 0.75f * RangeFac > UnfortunateMotherfucker.TotalMass * ElectricResistance)
                {
                    Vaporize = true;
                }
                else
                {
                    cnt.room.AddObject(new CreatureSpasmer(UnfortunateMotherfucker, true, spasmTime));
                    if (UnfortunateMotherfucker.State is not null && UnfortunateMotherfucker.State.meatLeft > 0)
                    {
                        UnfortunateMotherfucker.State.meatLeft = (int)(UnfortunateMotherfucker.State.meatLeft * Mathf.InverseLerp(cnt.TotalMass / 2f, cnt.TotalMass, UnfortunateMotherfucker.TotalMass) * RangeFac);
                    }
                    UnfortunateMotherfucker.Hypothermia -= 2f / ElectricResistance * RangeFac;
                }
            }
            else
            {
                cnt.room.AddObject(new CreatureSpasmer(UnfortunateMotherfucker, true, spasmTime));
                UnfortunateMotherfucker.Stun(spasmTime);
                UnfortunateMotherfucker.LoseAllGrasps();
                UnfortunateMotherfucker.Hypothermia -= 1 / ElectricResistance * RangeFac;
            }

            ShockChillipedeArmor(cnt, UnfortunateMotherfucker);

            cnt.room.PlaySound(SoundID.Centipede_Shock, cnt.mainBodyChunk.pos, 2f * RangeFac, 1);
            cnt.room.PlaySound(SoundID.Death_Lightning_Spark_Object, cnt.mainBodyChunk.pos, 2f * RangeFac, 1);

            if (Vaporize)
            {
                UnfortunateMotherfucker.Destroy();
            }
        }

        cI.vaporSmoke = null;

    }
    public static void ShockChillipedeArmor(Centipede cnt, PhysicalObject target)
    {
        if (cnt is null || cnt.Template.type == HailstormEnums.Chillipede || target is null || target is not Centipede chl || chl.CentiState is not ChillipedeState cS)
        {
            return;
        }

        for (int s = 0; s < chl.CentiState.shells.Length; s++)
        {
            if (!chl.CentiState.shells[s])
            {
                continue;
            }
            chl.CentiState.shells[s] = false;
            float volume =
                (cS.ScaleRegenTime[s] <= 0) ? 1.25f :
                (cS.ScaleRegenTime[s] <= 2000) ? 1f : 0.75f;
            cS.ScaleRegenTime[s] = 6000;
            chl.room.PlaySound(SoundID.Coral_Circuit_Break, chl.bodyChunks[s].pos, volume, 1.5f);
            for (int j = 0; j < 18; j++)
            {
                chl.room.AddObject(j % 3 == 1 ?
                        new HailstormSnowflake(chl.bodyChunks[s].pos, Custom.RNV() * Random.Range(12f, 24f), cS.scaleColor, cS.accentColor) :
                        new PuffBallSkin(chl.bodyChunks[s].pos, Custom.RNV() * Random.Range(12f, 24f), cS.scaleColor, cS.accentColor));
            }
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Centi AI

    public static void HailstormCentiwingAI(On.CentipedeAI.orig_ctor orig, CentipedeAI cntAI, AbstractCreature absCnt, World world)
    {
        orig(cntAI, absCnt, world);
        if (cntAI?.centipede is null)
        {
            return;
        }

        if (absCnt.creatureTemplate.type == HailstormEnums.Cyanwing)
        {
            cntAI.pathFinder.stepsPerFrame = 15;
            cntAI.preyTracker.persistanceBias = 4f;
            cntAI.preyTracker.sureToGetPreyDistance = 150f;
            cntAI.preyTracker.sureToLosePreyDistance = 600f;
            cntAI.utilityComparer.GetUtilityTracker(cntAI.preyTracker).weight = 1.5f;
        }

        if (absCnt.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede && absCnt.superSizeMe &&
            CWT.AbsCtrData.TryGetValue(absCnt, out AbsCtrInfo aI) && (aI.ctrList is null || aI.ctrList.Count < 1))
        {
            FindBabyCentiwingMother(absCnt, world, aI);
        }
    }
    public static void CyanwingAggression(On.CentipedeAI.orig_Update orig, CentipedeAI cntAI)
    {
        if (cntAI?.centipede is null)
        {
            orig(cntAI);
            return;
        }
        Centipede cnt = cntAI.centipede;

        if (cnt.Template.type == HailstormEnums.Cyanwing)
        {
            float weight = (cntAI.preyTracker.MostAttractivePrey is not null) ? 0 : 0.1f;
            cntAI.utilityComparer.GetUtilityTracker(cntAI.injuryTracker).weight = weight;
        }

        orig(cntAI);

        if (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cntAI.creature.superSizeMe && CWT.AbsCtrData.TryGetValue(cntAI.creature, out AbsCtrInfo aI) && (aI.ctrList is null || aI.ctrList.Count < 1 || aI.ctrList[0].state.dead))
        {
            FindBabyCentiwingMother(cntAI.creature, cntAI.creature.world, aI);
        }

        if (cnt.Template.type == HailstormEnums.Cyanwing && cntAI.creature.abstractAI?.followCreature is not null)
        {
            cntAI.creature.abstractAI.AbstractBehavior(1);
        }

        if (cnt.CentiState is ChillipedeState cS && cntAI.preyTracker.MostAttractivePrey is not null)
        {
            if (cntAI.preyTracker.MostAttractivePrey.TicksSinceSeen >= 400 && cS.mistTimer < 160)
            {
                cS.mistTimer = 160;
            }
            else if (cntAI.preyTracker.MostAttractivePrey.TicksSinceSeen < 400)
            {
                if (cS.mistTimer > 0)
                {
                    cS.mistTimer -= (int)Mathf.Lerp(1, 3, Mathf.InverseLerp(0, cnt.CentiState.shells.Length, cnt.CentiState.shells.Count(intact => intact)));
                    if (Burn.IsCreatureBurning(cnt))
                    {
                        cS.mistTimer--;
                    }
                }
                else
                {
                    cS.mistTimer = 120;
                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < cnt.room.updateList.Count; i++)
                    {
                        if (cnt.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = cnt.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }
                    cnt.room.AddObject(new FreezerMist(cnt.bodyChunks[Random.Range(0, cnt.bodyChunks.Length - 1)].pos, Custom.RNV() * Random.value * 6f, cS.scaleColor, cS.accentColor, 1f, cnt.abstractCreature, smallInsects, true));
                }
            }
        }
    }
    public static CreatureTemplate.Relationship CentiwingBravery(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI cntAI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        if (cntAI?.centipede is not null && CentiData.TryGetValue(cntAI.centipede, out CentiInfo cI))
        {
            AbstractCreature absCtr = dynamRelat.trackerRep.representedCreature;
            CreatureTemplate.Relationship defaultRelation = cntAI.StaticRelationship(absCtr);
            Centipede cnt = cntAI.centipede;
            if (cnt.Centiwing && absCtr.realizedCreature is not null)
            {
                if (defaultRelation.type == CreatureTemplate.Relationship.Type.Eats && dynamRelat.trackerRep.representedCreature.realizedCreature.TotalMass < cnt.TotalMass)
                {
                    if (cI.Cyanwing)
                    {
                        if (cntAI.creature.abstractAI?.followCreature is not null && cntAI.creature.abstractAI.followCreature == absCtr)
                        {
                            return new CreatureTemplate.Relationship
                                (CreatureTemplate.Relationship.Type.Eats, 1);
                        }
                        float intensity = Mathf.InverseLerp(0f, cnt.TotalMass, dynamRelat.trackerRep.representedCreature.realizedCreature.TotalMass * 1.2f);
                        if (cnt.CentiState is not null)
                        {
                            intensity *= 2 - cnt.CentiState.health;
                        }
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, intensity * defaultRelation.intensity);
                    }
                    if (IsIncanStory(cntAI?.centipede?.room?.game) && cnt.Template.type == CreatureTemplate.Type.Centiwing)
                    {
                        float massFac = Mathf.Pow(Mathf.InverseLerp(0f, cnt.TotalMass, dynamRelat.trackerRep.representedCreature.realizedCreature.TotalMass), 0.75f);
                        float courageThreshold = Mathf.Lerp(360, 100, Mathf.InverseLerp(0.4f, 1, cnt.size));
                        if (dynamRelat.trackerRep.age < Mathf.Lerp(360, 100, Mathf.InverseLerp(0.4f, 1, cnt.size)))
                        {
                            massFac *= 1f - cntAI.OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                            return new CreatureTemplate.Relationship
                                (CreatureTemplate.Relationship.Type.Afraid, massFac * Mathf.InverseLerp(courageThreshold, 0f, dynamRelat.trackerRep.age));
                        }
                        massFac *= 1f - cntAI.OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(courageThreshold, courageThreshold * 2.66f, dynamRelat.trackerRep.age) * massFac);
                    }
                }
            }
        }
        return orig(cntAI, dynamRelat);
    }

    //------------------------------------------

    public static void FindBabyCentiwingMother(AbstractCreature absCnt, World world, AbsCtrInfo aI)
    {
        if (absCnt is null || world is null) return;

        if (aI.ctrList is null)
        {
            aI.ctrList = new List<AbstractCreature>();
        }
        else if (aI.ctrList.Count > 0 && aI.ctrList[0].state.dead)
        {
            aI.ctrList.Clear();
        }

        if (absCnt.Room is not null)
        {
            foreach (AbstractCreature ctr in absCnt.Room.creatures)
            {
                if (ctr is not null && ctr.creatureTemplate.type == HailstormEnums.Cyanwing && ctr.state.alive)
                {
                    aI.ctrList.Add(ctr);
                    return;
                }
            }
        }

        foreach (AbstractRoom room in world.abstractRooms)
        {
            if (room is null) continue;

            foreach (AbstractCreature ctr in room.creatures)
            {
                if (ctr is not null && ctr.creatureTemplate.type == HailstormEnums.Cyanwing && ctr.state.alive)
                {
                    aI.ctrList.Add(ctr);
                }
            }
            foreach (AbstractWorldEntity denEntity in room.entitiesInDens)
            {
                if (denEntity is not null && denEntity is AbstractCreature denCtr && denCtr.creatureTemplate.type == HailstormEnums.Cyanwing && denCtr.state.alive)
                {
                    aI.ctrList.Add(denCtr);
                }
            }
        }

        for (; aI.ctrList.Count > 1;)
        {
            if (aI.ctrList[0] is null || aI.ctrList[1] is null) continue;

            if (Custom.WorldCoordFloatDist(aI.ctrList[0].pos, absCnt.pos) > Custom.WorldCoordFloatDist(aI.ctrList[1].pos, absCnt.pos))
            {
                aI.ctrList.RemoveAt(0);
            }
            else if (Custom.WorldCoordFloatDist(aI.ctrList[1].pos, absCnt.pos) > Custom.WorldCoordFloatDist(aI.ctrList[0].pos, absCnt.pos))
            {
                aI.ctrList.RemoveAt(1);
            }
            else
                aI.ctrList.RemoveAt(Random.Range(0, 2));
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Centi Graphics

    public static void WinterCentipedeColors(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics cg, PhysicalObject centi)
    {
        orig(cg, centi);
        if (cg?.centipede is not null && CentiData.TryGetValue(cg.centipede, out CentiInfo cI))
        {
            Centipede cnt = cg.centipede;

            Random.State state = Random.state;
            Random.InitState(cnt.abstractCreature.ID.RandomSeed);

            if (cI.Cyanwing)
            {
                cg.hue = Random.Range(160 / 360f, 210 / 360f);
                cg.saturation = 1;
                if (cnt.bodyChunks is not null)
                {
                    if (cI.segmentHues is null)
                    {
                        cI.segmentHues = new float[cnt.bodyChunks.Length];
                        for (int s = 0; s < cI.segmentHues.Length; s++)
                        {
                            cI.segmentHues[s] =
                                Mathf.Lerp(cg.hue, cg.hue + 50 / 360f, (s % 20 < 10 ? Mathf.InverseLerp(0, 10, s) : Mathf.InverseLerp(20, 10, s)));
                        }
                    }
                    if (cI.segmentGradientDirections is null)
                    {
                        cI.segmentGradientDirections = new bool[cnt.bodyChunks.Length];
                    }
                }
            }
            else if (cI.BabyAquapede)
            {
                cg.hue = (260 / 360f) - Mathf.Abs(Custom.WrappedRandomVariation(80 / 360f, 80 / 360f, 0.33f) - 80 / 360f);
                cg.saturation = 1;
            }
            else if (cnt.CentiState is ChillipedeState cS)
            {
                cg.hue = Random.Range(180 / 360f, 240 / 360f);
                cg.saturation = 1;
                cS.scaleColor = Custom.HSL2RGB(cg.hue, cg.saturation, 0.7f + (cg.hue / 5f));
                cS.accentColor = Custom.HSL2RGB(cg.hue + (30 / 360f), 0.65f, 0.45f + (cg.hue / 4f));
            }
            else if (IsIncanStory(cg.centipede.room?.game))
            {
                float range = Mathf.Lerp(0.16f, 0.12f, Mathf.InverseLerp(0f, 1f, cnt.size));
                float skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0f, 1f, cnt.size));

                if (cnt.Template.type == CreatureTemplate.Type.Centiwing ||
                (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe))
                {
                    range = Mathf.Lerp(0, 20f, Mathf.InverseLerp(0.4f, 0.8f, cnt.size));
                    skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.4f, 0.8f, cnt.size));
                    cg.hue = Mathf.Lerp((80 + range) / 360f, (160 + range) / 360f, Mathf.Pow(Random.value, skew));
                    if (!cnt.Small)
                    {
                        cg.saturation = Mathf.Clamp(cnt.size, 0, 1);
                    }
                }
                else if (cnt.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
                {
                    range = Mathf.Lerp(0, 20f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
                    skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
                    cg.hue = Mathf.Lerp((240 - range) / 360f, (200 - range) / 360f, Mathf.Pow(Random.value, skew));
                }
                else if (cnt.Template.type == CreatureTemplate.Type.RedCentipede)
                {
                    skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.9f, 1.1f, cnt.size));
                    cg.hue = Mathf.Lerp(-0.06f, 0.03f, Mathf.Pow(Random.value, skew));
                }
                else if (cnt.Template.type == CreatureTemplate.Type.Centipede ||
                    cnt.Template.type == CreatureTemplate.Type.SmallCentipede)
                {
                    cg.hue = Mathf.Lerp(0.04f, range, Mathf.Pow(Random.value, skew));
                }
            }
            Random.state = state;
        }
    }
    public static void WintercentiColorableAntennae(On.CentipedeGraphics.orig_InitiateSprites orig, CentipedeGraphics cg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(cg, sLeaser, rCam);
        if (cg?.centipede is not null && CentiData.TryGetValue(cg.centipede, out CentiInfo cI))
        {
            if (cg.centipede.CentiState is ChillipedeState cS)
            {
                cS.StartOfNewSprites = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + cg.owner.bodyChunks.Length);
                for (int i = 0; i < cg.owner.bodyChunks.Length; i++)
                {
                    int armorStage =
                        cS.ScaleRegenTime[i] <= 0 ? 0 :
                        cS.ScaleRegenTime[i] <= 2400 ? 1 : 2;
                    sLeaser.sprites[cg.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("ChillipedeChunk");
                    sLeaser.sprites[cg.ShellSprite(i, 0)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + cS.ScaleSprites[i][0] + "." + armorStage);
                    sLeaser.sprites[cS.StartOfNewSprites + i] = new("ChillipedeBottomShell" + cS.ScaleSprites[i][1] + "." + armorStage);
                }
                cg.AddToContainer(sLeaser, rCam, null);
            }

            if (!cI.Cyanwing && !cI.Chillipede) return;

            for (int side = 0; side < 2; side++)
            {
                for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
                {
                    for (int whisker = 0; whisker < 2; whisker++)
                    {
                        TriangleMesh whiskerMesh = sLeaser.sprites[cg.WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                        whiskerMesh.customColor = true;
                        whiskerMesh.verticeColors = new Color[15];
                        if (whisker == 0 && whiskerMesh.isVisible) whiskerMesh.isVisible = false;
                    }
                }
            }
        }
    }
    public static void WintercentiPalette(On.CentipedeGraphics.orig_ApplyPalette orig, CentipedeGraphics cg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(cg, sLeaser, rCam, palette);
        if (cg?.centipede is not null && CentiData.TryGetValue(cg.centipede, out CentiInfo cI) && (cI.Cyanwing || cI.Chillipede))
        {
            cg.blackColor = cI.Chillipede ?
                Custom.HSL2RGB(cg.hue, 0.1f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = cg.blackColor;
            }
            for (int legPair = 0; legPair < cg.owner.bodyChunks.Length; legPair++)
            {
                for (int leg = 0; leg < 2; leg++)
                {
                    VertexColorSprite legSprite = sLeaser.sprites[cg.LegSprite(legPair, leg, 1)] as VertexColorSprite;
                    if (cI.Cyanwing)
                    {
                        legSprite.verticeColors[0] = cI.offcolor ? cg.SecondaryShellColor : Color.Lerp(Custom.HSL2RGB(cg.hue + 50 / 360f, cg.saturation, 0.3f), cg.blackColor, 0.3f + 0.7f * cg.darkness);
                        legSprite.verticeColors[1] = cI.offcolor ? cg.SecondaryShellColor : Color.Lerp(Custom.HSL2RGB(cg.hue + 50 / 360f, cg.saturation, 0.3f), cg.blackColor, 0.3f + 0.7f * cg.darkness);
                        legSprite.verticeColors[2] = cg.blackColor;
                        legSprite.verticeColors[3] = cg.blackColor;
                    }
                    else if (cg.centipede.CentiState is ChillipedeState cS)
                    {
                        legSprite.verticeColors[0] = cS.scaleColor;
                        legSprite.verticeColors[1] = cS.scaleColor;
                        legSprite.verticeColors[2] = cS.accentColor;
                        legSprite.verticeColors[3] = cg.blackColor;
                        for (int v = 0; v < legSprite.verticeColors.Length; v++)
                        {
                            legSprite.verticeColors[v] = Color.Lerp(legSprite.verticeColors[v], cg.blackColor, cg.darkness);
                        }
                    }
                }
            }
            for (int side = 0; side < 2; side++)
            {
                for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
                {
                    for (int whisker = 0; whisker < 2; whisker++)
                    {
                        TriangleMesh whiskerMesh = sLeaser.sprites[cg.WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                        for (int v = 0; v < whiskerMesh.verticeColors.Length; v++)
                        {
                            if (cI.Cyanwing)
                            {
                                whiskerMesh.verticeColors[v] = v < 5 ? cg.blackColor :
                                    Color.Lerp(cg.blackColor, cI.offcolor ? cg.SecondaryShellColor : Custom.HSL2RGB(0.75f, 0.66f, 0.6f), Mathf.InverseLerp(5, whiskerMesh.verticeColors.Length - 4, v));
                            }
                            else if (cg.centipede.CentiState is ChillipedeState cS && whiskerMesh.isVisible)
                            {
                                whiskerMesh.verticeColors[v] =
                                    v > 11 ? cS.scaleColor :
                                    v > 8 ? Color.Lerp(cS.scaleColor, cS.accentColor, Mathf.InverseLerp(11, 9, v)) :
                                    Color.Lerp(cS.accentColor, cg.blackColor, Mathf.InverseLerp(8, 6, v));
                            }

                            if (v >= 5 && whiskerMesh.isVisible)
                            {
                                whiskerMesh.verticeColors[v] = Color.Lerp(whiskerMesh.verticeColors[v], cg.blackColor, cg.darkness / 2f);
                            }
                        }
                    }
                }
            }

            if (cI.Cyanwing)
            {
                if (cg.centipede.Glower is not null)
                {
                    cg.centipede.Glower.color =
                        Color.Lerp(Custom.RGB2RGBA(palette.waterColor1, 1), Custom.HSL2RGB(cg.hue, cg.saturation, 0.66f), 0.5f);
                }
                for (int j = 0; j < cg.totalSecondarySegments; j++)
                {
                    Mathf.Sin((float)j / (float)(cg.totalSecondarySegments - 1) * Mathf.PI);
                    sLeaser.sprites[cg.SecondarySegmentSprite(j)].color = Color.Lerp(Custom.HSL2RGB(cg.hue, 1f, 0.2f), cg.blackColor, Mathf.Lerp(0.4f, 1f, cg.darkness));
                }
            }
            else if (cg.centipede.CentiState is ChillipedeState cS)
            {
                for (int chunk = 0; chunk < cg.totalSecondarySegments; chunk++)
                {
                    sLeaser.sprites[cg.SecondarySegmentSprite(chunk)].color = cS.accentColor;
                }
            }

        }
    }
    public static void WintercentiColoration(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics cg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(cg, sLeaser, rCam, timeStacker, camPos);

        if (cg?.centipede is not null && CentiData.TryGetValue(cg.centipede, out CentiInfo cI) && cI.isHailstormCenti)
        {
            Centipede cnt = cg.centipede;
            Color chargeCol = Custom.HSL2RGB(cg.hue, cg.saturation, 0.66f);

            for (int side = 0; side < 2; side++)
            {
                for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
                {
                    for (int whisker = 0; whisker < 2; whisker++)
                    {
                        TriangleMesh whiskerMesh = sLeaser.sprites[cg.WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                        if (cI.BabyAquapede)
                        {
                            for (int v = 1; v < whiskerMesh.vertices.Length; v++)
                            {
                                whiskerMesh.vertices[v] = Vector2.Lerp(whiskerMesh.vertices[v], whiskerMesh.vertices[0], 0.4f);
                            }
                        }
                    }
                    for (int wingPair = 0; wingPair < cg.wingPairs; wingPair++)
                    {
                        CustomFSprite wing = sLeaser.sprites[cg.WingSprite(side, wingPair)] as CustomFSprite;
                        if (cI.BabyAquapede)
                        {
                            sLeaser.sprites[cg.WingSprite(side, wingPair)].isVisible = cg.centipede.BitesLeft > wingPair;
                            for (int v = 0; v < wing.vertices.Length; v++)
                            {
                                if (v == 3) continue;
                                wing.vertices[v] = Vector2.Lerp(wing.vertices[v], wing.vertices[3], 0.4f);
                            }
                        }
                        else if (cI.Cyanwing)
                        {
                            Vector2 val16 =
                                (wingPair != 0) ?
                                Custom.DirVec(cg.ChunkDrawPos(wingPair - 1, timeStacker), cg.ChunkDrawPos(wingPair, timeStacker)) :
                                Custom.DirVec(cg.ChunkDrawPos(0, timeStacker), cg.ChunkDrawPos(1, timeStacker));
                            Vector2 val17 = Custom.PerpendicularVector(val16);
                            Vector2 val18 = cg.RotatAtChunk(wingPair, timeStacker);
                            Vector2 val19 = cg.WingPos(side, wingPair, val16, val17, val18, timeStacker);
                            Vector2 val20 = cg.ChunkDrawPos(wingPair, timeStacker) + cnt.bodyChunks[wingPair].rad * ((side == 0) ? (-1f) : 1f) * val17 * val18.y;
                            Vector2 val21 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(val19, val20) + Custom.VecToDeg(val18));
                            float num18 = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(val21, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(val18)), val16));
                            Vector2 val22 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(val20, val19) + Custom.VecToDeg(val18));
                            float num19 = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(val22, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(val18)), -val16));
                            num18 = Mathf.Pow(Mathf.Max(num18, num19), 0.5f);
                            wing.verticeColors[0] = Color.Lerp(Custom.HSL2RGB(0.75f - 0.4f * Mathf.Pow(num18, 2f), 0.66f, 0.5f + 0.5f * num18, 0.5f + 0.5f * num18), chargeCol, 0.5f * num18);
                            wing.verticeColors[1] = Color.Lerp(Custom.HSL2RGB(0.75f - 0.4f * Mathf.Pow(num18, 2f), 0.66f, 0.5f + 0.5f * num18, 0.5f + 0.5f * num18), chargeCol, 0.5f * num18);
                            wing.verticeColors[2] = Color.Lerp(cg.blackColor, chargeCol, 0.5f * num18);
                            wing.verticeColors[3] = Color.Lerp(cg.blackColor, chargeCol, 0.5f * num18);
                        }
                    }
                }
            }
            if (cnt.CentiState is null || (!cI.Chillipede && (!cI.Cyanwing || cI.segmentHues is null || cI.segmentGradientDirections is null)))
            {
                return;
            }
            for (int i = 0; i < cnt.bodyChunks.Length; i++)
            {
                if (cI.Chillipede)
                {
                    sLeaser.sprites[cg.SecondarySegmentSprite(i)].scaleX *= 0.85f;
                    sLeaser.sprites[cg.SegmentSprite(i)].scaleX = Mathf.Lerp(sLeaser.sprites[cg.SegmentSprite(i)].scaleX, sLeaser.sprites[cg.SegmentSprite(0)].scaleX, 0.5f);
                    sLeaser.sprites[cg.SegmentSprite(i)].scale *= 0.8f;
                }
                for (int k = 0; k < 1; k++)
                {

                    if (cI.Cyanwing && !cnt.CentiState.shells[i]) break;

                    float num = (float)i / (float)(cg.owner.bodyChunks.Length - 1);
                    Vector2 val2 = Vector2.Lerp(cnt.bodyChunks[0].lastPos, cnt.bodyChunks[0].pos, timeStacker);
                    val2 += Custom.DirVec(Vector2.Lerp(cnt.bodyChunks[1].lastPos, cnt.bodyChunks[1].pos, timeStacker), val2) * 10f;
                    Vector2 val3 = cg.RotatAtChunk(i, timeStacker);
                    Vector2 normalized = val3.normalized;
                    Vector2 val4 = Vector2.Lerp(cnt.bodyChunks[i].lastPos, cnt.bodyChunks[i].pos, timeStacker);
                    Vector2 val5 = ((i < cnt.bodyChunks.Length - 1) ? Vector2.Lerp(cnt.bodyChunks[i + 1].lastPos, cnt.bodyChunks[i + 1].pos, timeStacker) : (val4 + Custom.DirVec(val2, val4) * 10f));
                    val3 = val2 - val5;
                    float num6 = Mathf.Clamp(Mathf.Sin(num * Mathf.PI), 0f, 1f);
                    num6 *= Mathf.Lerp(1f, 0.5f, cg.centipede.size);
                    float darkFac = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(val3.normalized, Custom.DegToVec(30f) * normalized.x));
                    darkFac *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - normalized.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - normalized.x)));
                    darkFac *= Mathf.Pow(1f - cg.darkness, 2f);
                    if (normalized.y > 0f)
                    {
                        if (cI.Cyanwing)
                        {
                            sLeaser.sprites[cg.ShellSprite(i, k)].color = Color.Lerp(Custom.HSL2RGB(cI.segmentHues[i], cg.saturation, 0.5f + 0.25f * darkFac), cg.blackColor, cg.darkness);
                            sLeaser.sprites[cg.ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("CyanwingBackShell");
                        }
                        else if (cnt.CentiState is ChillipedeState cS && cS.ScaleSprites is not null && cS.ScaleStages is not null)
                        {
                            sLeaser.sprites[cS.StartOfNewSprites + i].isVisible = sLeaser.sprites[cg.ShellSprite(i, 0)].isVisible;
                            if (sLeaser.sprites[cg.ShellSprite(i, 0)].isVisible)
                            {
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleX = Mathf.Lerp(sLeaser.sprites[cg.ShellSprite(i, k)].scaleX, sLeaser.sprites[cg.ShellSprite(0, k)].scaleX, 0.3f);
                                sLeaser.sprites[cg.ShellSprite(i, k)].color = Color.Lerp(cS.accentColor, cg.blackColor, cg.darkness / 2f);
                                sLeaser.sprites[cg.ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + cS.ScaleSprites[i][0] + "." + cS.ScaleStages[i]);
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleX *= 0.55f;
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleY *= 0.55f;
                                sLeaser.sprites[cS.StartOfNewSprites + i].x = sLeaser.sprites[cg.ShellSprite(i, 0)].x;
                                sLeaser.sprites[cS.StartOfNewSprites + i].y = sLeaser.sprites[cg.ShellSprite(i, 0)].y;
                                sLeaser.sprites[cS.StartOfNewSprites + i].color = Color.Lerp(cS.scaleColor, cg.blackColor, cg.darkness / 2f);
                                sLeaser.sprites[cS.StartOfNewSprites + i].element = Futile.atlasManager.GetElementWithName("ChillipedeBottomShell" + cS.ScaleSprites[i][1] + "." + cS.ScaleStages[i]);
                                sLeaser.sprites[cS.StartOfNewSprites + i].scaleX = sLeaser.sprites[cg.ShellSprite(i, 0)].scaleX;
                                sLeaser.sprites[cS.StartOfNewSprites + i].scaleY = sLeaser.sprites[cg.ShellSprite(i, 0)].scaleY;
                                sLeaser.sprites[cS.StartOfNewSprites + i].rotation = sLeaser.sprites[cg.ShellSprite(i, 0)].rotation;
                            }
                        }
                    }
                    else
                    {
                        if (cI.Cyanwing)
                        {
                            sLeaser.sprites[cg.ShellSprite(i, k)].color = Color.Lerp(Custom.HSL2RGB(cI.segmentHues[i], cg.saturation, 0.5f + 0.25f * darkFac), cg.blackColor, 0.5f);
                            sLeaser.sprites[cg.ShellSprite(i, k)].color = Color.Lerp(sLeaser.sprites[cg.ShellSprite(i, k)].color, new Color(0.4392f, 0.0745f, 0f), 0.25f);
                            sLeaser.sprites[cg.ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("CyanwingBellyShell");
                        }
                        else if (cnt.CentiState is ChillipedeState cS && cS.ScaleSprites is not null && cS.ScaleStages is not null)
                        {
                            sLeaser.sprites[cS.StartOfNewSprites + i].isVisible = sLeaser.sprites[cg.ShellSprite(i, 0)].isVisible;
                            if (sLeaser.sprites[cg.ShellSprite(i, 0)].isVisible)
                            {
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleX = cg.owner.bodyChunks[i].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num6) * 0.125f * normalized.y;
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleX = Mathf.Lerp(sLeaser.sprites[cg.ShellSprite(i, k)].scaleX, sLeaser.sprites[cg.ShellSprite(0, k)].scaleX, 0.3f);
                                sLeaser.sprites[cg.ShellSprite(i, k)].color = Color.Lerp(cS.accentColor, cg.blackColor, Mathf.Lerp(0.25f, 0.75f, cg.darkness));
                                sLeaser.sprites[cg.ShellSprite(i, k)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + cS.ScaleSprites[i][0] + "." + cS.ScaleStages[i]);
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleX *= 0.55f;
                                sLeaser.sprites[cg.ShellSprite(i, k)].scaleY *= 0.55f;
                                sLeaser.sprites[cS.StartOfNewSprites + i].x = sLeaser.sprites[cg.ShellSprite(i, 0)].x;
                                sLeaser.sprites[cS.StartOfNewSprites + i].y = sLeaser.sprites[cg.ShellSprite(i, 0)].y;
                                sLeaser.sprites[cS.StartOfNewSprites + i].color = Color.Lerp(cS.scaleColor, cg.blackColor, Mathf.Lerp(0.25f, 0.75f, cg.darkness));
                                sLeaser.sprites[cS.StartOfNewSprites + i].element = Futile.atlasManager.GetElementWithName("ChillipedeBottomShell" + cS.ScaleSprites[i][1] + "." + cS.ScaleStages[i]);
                                sLeaser.sprites[cS.StartOfNewSprites + i].scaleX = sLeaser.sprites[cg.ShellSprite(i, 0)].scaleX;
                                sLeaser.sprites[cS.StartOfNewSprites + i].scaleY = sLeaser.sprites[cg.ShellSprite(i, 0)].scaleY;
                                sLeaser.sprites[cS.StartOfNewSprites + i].rotation = sLeaser.sprites[cg.ShellSprite(i, 0)].rotation;
                            }
                        }
                    }
                }
                
            }
        }
    }
    public static void WintercentiSpriteLayering(On.CentipedeGraphics.orig_AddToContainer orig, CentipedeGraphics cg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(cg, sLeaser, rCam, newContainer);
        if (cg?.centipede is not null && cg.centipede.Template.type == HailstormEnums.Chillipede && sLeaser.sprites.Length > cg.TotalSprites)
        {
            var foregroundContainer = rCam.ReturnFContainer("Foreground");
            var midgroundContainer = rCam.ReturnFContainer("Midground");

            for (int s = cg.TotalSprites; s < sLeaser.sprites.Length; s++)
            {
                foregroundContainer.RemoveChild(sLeaser.sprites[s]);
                midgroundContainer.AddChild(sLeaser.sprites[s]);
            }

            for (int side = 0; side < 2; side++)
            {
                for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
                {
                    for (int whisker = 0; whisker < 2; whisker++)
                    {
                        sLeaser.sprites[cg.WhiskerSprite(side, whiskerPair, whisker)].MoveToBack();
                    }
                }
            }
        }
    }

    public static Color WinterCentiShortcutColors(On.Centipede.orig_ShortCutColor orig, Centipede cnt)
    {
        if (cnt.Template.type == HailstormEnums.Cyanwing)
        {
            return Custom.HSL2RGB(180 / 360f, 0.88f, 0.33f);
        }
        if (cnt.Template.type == HailstormEnums.Chillipede)
        {
            return Custom.hexToColor("7FD8FF");
        }
        return orig(cnt);
    }

    #endregion
}

//------------------------------------------------------------------------

public class CyanwingShell : CosmeticSprite
{
    public Centipede cnt;

    public float rotation;
    public float lastRotation;
    public float rotVel;
    private float zRotation;
    private float lastZRotation;
    private float zRotVel;

    public float lastDarkness = -1f;
    public float darkness;

    private readonly float scaleX;
    private readonly float scaleY;

    private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();

    public int fuseTime;

    public bool Gilded;
    private Color scaleColor;
    private Color blackColor;
    private Color currentShellColor;

    public LightSource shellLight;

    public float Submersion
    {
        get
        {
            if (room is null)
            {
                return 0f;
            }
            if (room.waterInverted)
            {
                return 1f - Mathf.InverseLerp(pos.y - 5, pos.y + 5, room.FloatWaterLevel(pos.x));
            }
            float num = room.FloatWaterLevel(pos.x);
            if (ModManager.MMF && !MMF.cfgVanillaExploits.Value && num > (room.abstractRoom.size.y + 20) * 20f)
            {
                return 1f;
            }
            return Mathf.InverseLerp(pos.y - 5, pos.y + 5, num);
        }
    }

    public CyanwingShell(Centipede cnt, Vector2 pos, Vector2 vel, Color color, float scaleX, float scaleY, int fuseTime)
    {
        this.fuseTime = fuseTime;
        this.cnt = cnt;
        base.pos = pos + vel;
        lastPos = pos;
        base.vel = vel;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        rotation = Random.value * 360f;
        lastRotation = rotation;
        rotVel = Mathf.Lerp(-1f, 1f, Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 5f, 26f);
        zRotation = Random.value * 360f;
        lastZRotation = rotation;
        zRotVel = Mathf.Lerp(-1f, 1f, Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 2f, 16f);
        this.fuseTime = fuseTime;
        scaleColor = color;
    }

    public override void Update(bool eu)
    {
        if (room.PointSubmerged(pos))
        {
            vel *= 0.92f;
            vel.y -= room.gravity * 0.1f;
            rotVel *= 0.965f;
            zRotVel *= 0.965f;
        }
        else
        {
            vel *= 0.999f;
            vel.y -= room.gravity * 0.9f;
        }
        if (Random.value < Mathf.Max(0.1f, Mathf.InverseLerp(80, 0, fuseTime) / 4f))
        {
            room.AddObject(new WaterDrip(Vector2.Lerp(lastPos, pos, Random.value), vel + Custom.RNV() * Random.value * 2f, waterColor: false));
        }
        lastRotation = rotation;
        rotation += rotVel * Vector2.Distance(lastPos, pos);
        lastZRotation = zRotation;
        zRotation += zRotVel * Vector2.Distance(lastPos, pos);
        if (!Custom.DistLess(lastPos, pos, 3f) && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
        {
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
            FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
            pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
            bool flag = false;
            if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
            {
                vel.x = Mathf.Abs(vel.x) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
            {
                vel.x = (0f - Mathf.Abs(vel.x)) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
            {
                vel.y = Mathf.Abs(vel.y) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
            {
                vel.y = (0f - Mathf.Abs(vel.y)) * 0.15f;
                flag = true;
            }
            if (flag)
            {
                rotVel *= 0.8f;
                zRotVel *= 0.8f;
                if (vel.magnitude > 3f)
                {
                    rotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value * Mathf.Abs(rotVel / 15f);
                    zRotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value * Mathf.Abs(rotVel / 15f);
                }
            }
        }
        SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 3f, new IntVector2(0, 0), goThroughFloors: true);
        cd = SharedPhysics.VerticalCollision(room, cd);
        cd = SharedPhysics.HorizontalCollision(room, cd);
        pos = cd.pos;
        vel = cd.vel;
        if (cd.contactPoint.x != 0)
        {
            vel.y *= 0.6f;
        }
        if (cd.contactPoint.y != 0)
        {
            vel.x *= 0.6f;
        }
        if (cd.contactPoint.y < 0)
        {
            rotVel *= 0.7f;
            zRotVel *= 0.7f;
            if (vel.magnitude < 1f)
            {
                fuseTime--;
            }
        }
        if (shellLight is null && !slatedForDeletetion)
        {
            shellLight = new LightSource(pos, false, currentShellColor, this)
            {
                submersible = true,
                affectedByPaletteDarkness = 0
            };
            room.AddObject(shellLight);
        }
        else if (shellLight is not null)
        {
            float radiusLerp =
                fuseTime > 30 ?
                Mathf.InverseLerp((fuseTime % 30 > 15)? 30 : 0, 15, fuseTime % 30) :
                Mathf.InverseLerp(30, 5, fuseTime);

            shellLight.color = currentShellColor;
            shellLight.setPos = new Vector2?(pos);
            shellLight.setRad = new float?(100 * Mathf.Lerp(0.5f, fuseTime > 30? 1.5f : 2.5f, radiusLerp));
            shellLight.setAlpha = new float?(1);
            if (shellLight.slatedForDeletetion || shellLight.room != room || slatedForDeletetion)
            {
                shellLight = null;
            }

        }
        if (fuseTime <= 0 && !slatedForDeletetion)
        {
            Explode();
            Destroy();
        }
        base.Update(eu);
    }

    public void Explode()
    {
        if (this is null || slatedForDeletetion || room is null) return;

        room.AddObject(new ColorableZapFlash(pos, 2.4f, scaleColor));
        room.PlaySound(SoundID.Zapper_Zap, pos, 0.8f, Random.Range(0.5f, 1.5f));
        if (Submersion > 0.5f)
        {
            room.AddObject(new UnderwaterShock(room, null, pos, 10, 450f, 0.25f, cnt ?? null, scaleColor));
        }
        else foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
        {
            if (absCtr.realizedCreature?.bodyChunks is null)
            {
                continue;
            }

            Creature ctr = absCtr.realizedCreature;
            bool hit = false;

            for (int b = ctr.bodyChunks.Length - 1; b >= 0; b--)
            {
                BodyChunk chunk = ctr.bodyChunks[b];
                if (chunk is null || !Custom.DistLess(pos, chunk.pos, 150) || ctr is BigEel || (ctr is Centipede otherCnt && otherCnt.CentiState is not ChillipedeState) || ctr is BigJellyFish || ctr is Inspector)
                {
                    continue;
                }

                ctr.Violence(cnt.mainBodyChunk ?? null, new Vector2(0, 0), chunk, null, Creature.DamageType.Electric, 0.1f, ctr is Player ? 30 : (20f * Mathf.Lerp(ctr.Template.baseStunResistance, 1f, 0.5f)));
                room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));
                if (!hit) hit = true;
            }
            if (hit)
            {
                room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, pos);
                room.AddObject(new Explosion.ExplosionLight(pos, 150f, 1f, 4, scaleColor));
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite("CyanwingBackShell");
        sLeaser.sprites[1] = new FSprite("CyanwingBackShell");
        for (int s = 0; s < sLeaser.sprites.Length; s++)
        {
            sLeaser.sprites[s].scaleY = scaleY;
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        lastDarkness = darkness;
        darkness = rCam.room.Darkness(val);
        darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(val);
        Vector2 Zrotation = Custom.DegToVec(Mathf.Lerp(lastZRotation, zRotation, timeStacker));
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].x = val.x - camPos.x;
            sLeaser.sprites[i].y = val.y - camPos.y;
            sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            if (Mathf.Abs(Zrotation.x) < 0.1f)
            {
                sLeaser.sprites[i].scaleX = 0.1f * Mathf.Sign(Zrotation.x) * scaleX;
            }
            else
            {
                sLeaser.sprites[i].scaleX = Zrotation.x * scaleX;
            }
        }
        sLeaser.sprites[0].x += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).x * 1.5f;
        sLeaser.sprites[0].y += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).y * 1.5f;

        if (Gilded)
        {
            sLeaser.sprites[0].color = Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.7f + 0.3f * darkness), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            if (Zrotation.y > 0f)
            {
                sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            }
            else sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.4f + 0.6f * darkness), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(scaleColor, Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            if (Zrotation.y > 0f)
            {
                sLeaser.sprites[1].color = Color.Lerp(scaleColor, Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            }
            else sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(scaleColor, blackColor, 0.5f), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
        }
        
        if (sLeaser.sprites[0] is not null)
        {
            currentShellColor = sLeaser.sprites[0].color;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

public class CyanwingSpark : CosmeticSprite
{
    private float size;

    private float lastLife;
    private float life;
    private float lifeTime;

    private Color color;

    public CyanwingSpark(Vector2 pos, float size, Color color)
    {
        base.pos = pos;
        lastPos = pos;
        this.size = size;
        life = 1f;
        lastLife = 1f;
        lifeTime = Mathf.Lerp(12f, 16f, size * Random.value);
        this.color = color;
    }

    public override void Update(bool eu)
    {
        room.AddObject(new Spark(pos, Custom.RNV() * 60f * Random.value, color, null, 4, 50));
        if (life <= 0f && lastLife <= 0f)
        {
            Destroy();
            return;
        }
        lastLife = life;
        life = Mathf.Max(0f, life - 1f / lifeTime);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new("Futile_White");
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
        sLeaser.sprites[0].color = color;

        sLeaser.sprites[1] = new("Futile_White");
        sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
        sLeaser.sprites[1].color = color;

        sLeaser.sprites[2] = new("Futile_White");
        sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
        sLeaser.sprites[2].color = color;

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float lifespanFac = Mathf.Lerp(lastLife, life, timeStacker);
        for (int i = 0; i < 3; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
        }
        float sizeFac = Mathf.Lerp(20f, 120f, Mathf.Pow(size, 1.5f));

        sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac * 4f / 8f;
        sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, Random.value) * 0.75f;

        sLeaser.sprites[1].scale = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac * 4f / 8f;
        sLeaser.sprites[1].alpha = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, Random.value) * 0.15f;

        sLeaser.sprites[2].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(lifespanFac * Mathf.PI)) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac / 8f;
        sLeaser.sprites[2].alpha = Mathf.Sin(lifespanFac * Mathf.PI) * Random.value * 0.75f;
    }
}

public class ColorableZapFlash : CosmeticSprite
{
    private LightSource lightsource;

    private float life;
    private float lastLife;
    private float lifeTime;

    private float size;

    private Color color;

    public ColorableZapFlash(Vector2 initPos, float size, Color color)
    {
        this.size = size;
        lifeTime = Mathf.Lerp(1f, 4f, Random.value) + 2f * size;
        life = 1f;
        lastLife = 1f;
        pos = initPos;
        lastPos = initPos;
        this.color = color;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (lightsource is null)
        {
            lightsource = new LightSource(pos, false, color, this);
            room.AddObject(lightsource);
        }
        lastLife = life;
        life -= 1f / lifeTime;
        if (lastLife < 0f)
        {
            if (lightsource is not null)
            {
                lightsource.Destroy();
            }
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("Futile_White");
        sLeaser.sprites[0].color = color;
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];

        sLeaser.sprites[1] = new FSprite("Futile_White");
        sLeaser.sprites[1].color = Color.white;
        sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float lifespanFac = Mathf.Lerp(lastLife, life, timeStacker);
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
            sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        }
        if (lightsource is not null)
        {
            lightsource.HardSetRad(Mathf.Lerp(0.25f, 1f, Random.value * lifespanFac * size) * 2400f);
            lightsource.HardSetAlpha(Mathf.Pow(lifespanFac * Random.value, 0.4f));
            float colorSkew = Mathf.Pow(lifespanFac * Random.value, 4f);
            lightsource.color = Color.Lerp(color, Color.white, colorSkew);
        }
        sLeaser.sprites[0].scale = Mathf.Lerp(0.5f, 1f, Random.value * lifespanFac * size) * 500f / 16f;
        sLeaser.sprites[0].alpha = lifespanFac * Random.value * 0.75f;

        sLeaser.sprites[1].scale = Mathf.Lerp(0.5f, 1f, (0.5f + 0.5f * Random.value) * lifespanFac * size) * 400f / 16f;
        sLeaser.sprites[1].alpha = lifespanFac * Random.value * 0.75f;
    }
}