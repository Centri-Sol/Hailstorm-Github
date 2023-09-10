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
using System.Security.Policy;

namespace Hailstorm;

internal class HailstormSpiders
{
    public static void Hooks()
    {
        // Baby Spiders
        On.Spider.ctor += WinterBabySpiders;
        On.Spider.Centipede.ctor += WinterCoalescipedes;
        IL.Spider.Centipede.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((Spider.Centipede cls, bool eu) =>
            {
                return LightAdaptionChanges(cls, eu);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
        On.Spider.Update += WinterBabySpiderSaysNoToDens;
        On.Spider.Move_Vector2 += WinterBabySpiderSpeed;
        On.Spider.ConsiderPrey += WinterBabySpider_HigherMassLimit;
        On.Spider.NewRoom += STAYONTHERIGHTLAYER;
        On.Spider.SpiderMass.AddSpider += NoCoalescipedeMixing;
        On.SpiderGraphics.InitiateSprites += LuminescipedeSpriteSizes;
        On.SpiderGraphics.ApplyPalette += WinterBabySpiderColors;
        On.SpiderGraphics.DrawSprites += LuminescipedeGraphics;
        On.SpiderGraphics.AddToContainer += LuminescipedeSpriteContainer;

        // Big Spiders
        On.BigSpider.ctor += WinterMotherSpiders;
        On.BigSpider.Update += WinterMotherSpiderBabySpawn;
        On.BigSpider.Collide += WinterMotherCRONCH;
        On.BigSpider.Violence += WinterMotherHP;
        On.BigSpider.Die += WinterMotherSpiderBabyPuff;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += WinterMotherSpiderHostility;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool IsRWGIncan(RainWorldGame RWG)
    {
        return (RWG is not null && RWG.IsStorySession && RWG.StoryCharacter == HailstormSlugcats.Incandescent);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Baby Spiders
    public static bool IsWinterSpider(Spider spd)
    {
        return (spd is Luminescipede || HSRemix.HailstormBabySpidersEverywhere.Value is true || (IsRWGIncan(spd.room?.game) && spd.abstractCreature.Winterized));
    }
    public static void WinterBabySpiders(On.Spider.orig_ctor orig, Spider spd, AbstractCreature absCtr, World world)
    {
        orig(spd, absCtr, world);
        if (spd is not null && IsWinterSpider(spd))
        {
            bool lumin = spd is Luminescipede;
            Random.State state = Random.state;
            Random.InitState(spd.abstractCreature.ID.RandomSeed);
            spd.iVars = new Spider.IndividualVariations(lumin ? Random.Range(0.8f, 1.2f) : Random.Range(0.4f, 1.2f));
            Random.state = state;

            spd.gravity = 0.85f;
            spd.bounce = 0.15f;
            spd.surfaceFriction = lumin ? 0.95f : 0.9f;
            if (spd.bodyChunks is not null && spd.bodyChunks[0] is not null)
            {
                spd.bodyChunks[0].rad = Mathf.Lerp(4.4f, 10.8f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                spd.bodyChunks[0].mass = lumin ?
                    Mathf.Lerp(0.2f, 0.3f, Mathf.InverseLerp(0.8f, 1.2f, spd.iVars.size)) :
                    Mathf.Lerp(0.08f, 0.18f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
            }
            spd.denMovement = -1;
            if (lumin)
            {
                spd.collisionLayer = 1;
                spd.ChangeCollisionLayer(1);
            }
        }
    }
    public static void WinterCoalescipedes(On.Spider.Centipede.orig_ctor orig, Spider.Centipede cls, Spider origSpd, Room room)
    {
        orig(cls, origSpd, room);
        if (cls is not null && origSpd is not null && IsWinterSpider(origSpd))
        {
            cls.maxSize =
                origSpd is Luminescipede ?
                Random.Range(7, 12) :
                Random.Range(20, 31);

            if (origSpd is Luminescipede)
            {
                cls.lightAdaption = 1;
            }
        }
    }

    //-----------------------------------------

    public static bool LightAdaptionChanges(Spider.Centipede cls, bool eu)
    {
        if (cls?.FirstSpider is null || !IsWinterSpider(cls.FirstSpider)) return false;

        cls.counter++;
        if (cls.counter >= cls.body.Count)
        {
            cls.counter = 0;
            cls.Tighten();
        }

        if (!cls.ShouldIUpdate(eu)) return true;

        for (int num = cls.spiders.Count - 1; num >= 0; num--)
        {
            if (cls.spiders[num].dead || cls.spiders[num].room != cls.room)
            {
                cls.AbandonSpider(num);
            }
        }

        if (cls.FirstSpider is not null && cls.FirstSpider.moving)
        {
            cls.walkCycle -= 2;
        }
        else
        {
            cls.walkCycle--;
        }
        for (int num = cls.body.Count - 1; num >= 0; num--)
        {
            if (cls.body[num].spider.centipede != cls || cls.body[num].separatedCounter > 5)
            {
                if (cls.body[num].spider.centipede == cls)
                {
                    cls.AbandonSpider(cls.body[num].spider);
                }
                cls.body.RemoveAt(num);
            }
        }
        cls.totalMass = 0f;
        for (int i = 0; i < cls.body.Count; i++)
        {
            cls.body[i].Update(i);
            cls.totalMass += cls.body[i].spider.mainBodyChunk.mass;
            cls.body[i].spider.legsPosition = Mathf.Lerp(-1f, 1f, cls.body[i].BodyFac);
            if (i > 0 && cls.body[i].spider.iVars.size > cls.body[i - 1].spider.iVars.size)
            {
                Spider spider = cls.body[i].spider;
                Spider spider2 = cls.body[i - 1].spider;
                cls.body[i].spider = spider2;
                cls.body[i - 1].spider = spider;
                cls.body[i].inRightPlace = false;
                cls.body[i - 1].inRightPlace = false;
                break;
            }
            if (i >= 3 && cls.body[i - 2].inRightPlace && cls.body[i - 1].inRightPlace && cls.body[i].inRightPlace)
            {
                cls.body[i].spider.mainBodyChunk.vel -=
                    Custom.DirVec(cls.body[i].spider.mainBodyChunk.pos, cls.body[i - 2].spider.mainBodyChunk.pos);

                cls.body[i - 2].spider.mainBodyChunk.vel +=
                    Custom.DirVec(cls.body[i].spider.mainBodyChunk.pos, cls.body[i - 2].spider.mainBodyChunk.pos);
            }
        }
        cls.ConsiderCreature();
        if (cls.prey is not null && cls.FirstSpider is not null)
        {
            if (cls.prey.room != cls.FirstSpider.room || cls.totalMass < cls.prey.TotalMass)
            {
                cls.prey = null;
            }
            else if (cls.FirstSpider.VisualContact(cls.prey.mainBodyChunk.pos))
            {
                cls.preyPos = cls.prey.mainBodyChunk.pos;
                cls.preyVisualCounter = 0;
            }
        }
        if (cls.preyVisualCounter < 100 && cls.prey is not null)
        {
            cls.hunt = Mathf.Min(cls.hunt + 0.005f, 1f);
        }
        else
        {
            cls.hunt = Mathf.Max(cls.hunt - 0.005f, 0f);
        }
        cls.preyVisualCounter++;

        return true;
    }
    public static void WinterBabySpiderSaysNoToDens(On.Spider.orig_Update orig, Spider spd, bool eu)
    {
        orig(spd, eu);
        if (spd?.room is not null && IsWinterSpider(spd) && spd.denPos is not null)
        {
            spd.denPos = null;
        }
    }
    public static void WinterBabySpiderSpeed(On.Spider.orig_Move_Vector2 orig, Spider spd, Vector2 dest)
    {
        orig(spd, dest);
        if (spd is not null && IsWinterSpider(spd))
        {
            float num = 1f;
            if (spd.centipede is not null)
            {
                num += spd.centipede.spiders.Count * 0.05f;
            }
            spd.mainBodyChunk.vel +=
                Custom.DirVec(spd.mainBodyChunk.pos, dest) * num * 2f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.8f, 1.2f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size)) * (spd is Luminescipede ? Mathf.Lerp(-0.5f, 0.5f, (spd.State as HealthState).health) : 0.5f);
        }
    }
    public static bool WinterBabySpider_HigherMassLimit(On.Spider.orig_ConsiderPrey orig, Spider spd, Creature ctr)
    {
        if (spd is not null && IsWinterSpider(spd) && (spd is Luminescipede || ctr.TotalMass <= 6.72f) && spd.Template.CreatureRelationship(ctr.Template).type == CreatureTemplate.Relationship.Type.Eats && !ctr.leechedOut)
        {
            return true;
        }
        return orig(spd, ctr);
    }
    public static void STAYONTHERIGHTLAYER(On.Spider.orig_NewRoom orig, Spider spd, Room room)
    {
        orig(spd, room);
        if (spd is not null && spd.Template.type == HailstormEnums.Luminescipede)
        {
            spd.ChangeCollisionLayer(1);
        }
    }
    public static void NoCoalescipedeMixing(On.Spider.SpiderMass.orig_AddSpider orig, Spider.SpiderMass spdMass, Spider otherSpd)
    {
        orig(spdMass, otherSpd);
        if (spdMass?.FirstSpider is not null && spdMass.FirstSpider.Template.type != otherSpd.Template.type)
        {
            for (int i = 0; i < spdMass.spiders.Count; i++)
            {
                if (spdMass.spiders[i] == otherSpd)
                {
                    spdMass.AbandonSpider(i);
                    if (spdMass is Spider.Centipede cls)
                    {
                        otherSpd.bannedCentipede = cls;
                    }
                    break;
                }
            }
        }
    }


    //-----------------------------------------

    public static void LuminescipedeSpriteSizes(On.SpiderGraphics.orig_InitiateSprites orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(sg, sLeaser, rCam);
        if (sg?.spider is null || !IsWinterSpider(sg.spider)) return;
        Spider spd = sg.spider;

        sLeaser.sprites[sg.BodySprite].scale = spd.iVars.size;

        if (spd.Template.type == CreatureTemplate.Type.Spider)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[sg.LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                    sLeaser.sprites[sg.LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                }
            }
        }
        else if (spd is Luminescipede lmn)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[sg.LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1.25f : -1.25f) * spd.iVars.size;
                    sLeaser.sprites[sg.LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1.25f : -1.25f) * spd.iVars.size;
                }
            }

            lmn.StartOfBodySprites = sLeaser.sprites.Length;
            lmn.DecalSprite = lmn.StartOfBodySprites + 4;
            lmn.StartOfOutlineSprites = lmn.DecalSprite + 1;
            lmn.TotalSprites = lmn.StartOfOutlineSprites + 4;
            Array.Resize(ref sLeaser.sprites, lmn.TotalSprites);

            sLeaser.sprites[sg.BodySprite].SetElementByName("LuminescipedeHead");
            for (int b = lmn.StartOfBodySprites; b < lmn.DecalSprite; b++)
            {
                sLeaser.sprites[b] = new FSprite("LuminescipedeBody" + (b + 1 - lmn.StartOfBodySprites));
            }
            sLeaser.sprites[lmn.DecalSprite] = new FSprite("LuminescipedeDecal");
            for (int b = lmn.StartOfOutlineSprites; b < lmn.TotalSprites; b++)
            {
                sLeaser.sprites[b] = new FSprite("LuminescipedeBody" + (b + 1 - lmn.StartOfOutlineSprites));
                sLeaser.sprites[b].scale *= 1.15f;
            }

            sg.AddToContainer(sLeaser, rCam, null);
        }
    }
    public static void WinterBabySpiderColors(On.SpiderGraphics.orig_ApplyPalette orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(sg, sLeaser, rCam, palette);
        if (sg?.spider is not null && IsWinterSpider(sg.spider))
        {
            if (sg.spider is not Luminescipede)
            {
                sg.blackColor = Custom.HSL2RGB(210 / 360f, 0.1f, 0.2f);
                for (int i = 1; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = sg.blackColor;
                }
            }
            else if (sg.spider is Luminescipede lmn)
            {
                sg.blackColor = Color.Lerp(lmn.baseColor, Color.black, 0.7f);
                for (int i = 1; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color =
                        (i >= lmn.DecalSprite) ? sg.blackColor : lmn.baseColor;
                }
            }
        }
    }
    public static void LuminescipedeGraphics(On.SpiderGraphics.orig_DrawSprites orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(sg, sLeaser, rCam, timeStacker, camPos);
        if (sg?.spider is null || sg.spider is not Luminescipede lmn) return;

        lmn.currentGlowColor = Color.Lerp(sg.blackColor, lmn.glowColor, lmn.chargeFac);
        lmn.glowPos = sLeaser.sprites[sg.BodySprite].GetPosition();

        for (int s = 0; s < sLeaser.sprites.Length; s++)
        {
            sLeaser.sprites[s].color =
                (s >= lmn.DecalSprite) ?
                Color.Lerp(lmn.glowColor, sg.blackColor, lmn.chargeFac) :
                lmn.currentGlowColor;
        }

        for (int n = lmn.StartOfBodySprites; n < sLeaser.sprites.Length; n++)
        {
            sLeaser.sprites[n].x = sLeaser.sprites[sg.BodySprite].x;
            sLeaser.sprites[n].y = sLeaser.sprites[sg.BodySprite].y;
            sLeaser.sprites[n].rotation = sLeaser.sprites[sg.BodySprite].rotation;
        }

        if (lmn.BitesLeft > 1 || sLeaser.sprites[lmn.StartOfOutlineSprites].isVisible)
        {
            sLeaser.sprites[1].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[2].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[5].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[6].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[lmn.StartOfBodySprites].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[lmn.StartOfOutlineSprites].isVisible = lmn.BitesLeft > 1;

            sLeaser.sprites[3].isVisible = lmn.BitesLeft > 2;
            sLeaser.sprites[4].isVisible = lmn.BitesLeft > 2;
            sLeaser.sprites[7].isVisible = lmn.BitesLeft > 2;
            sLeaser.sprites[8].isVisible = lmn.BitesLeft > 2;
            sLeaser.sprites[lmn.StartOfBodySprites + 1].isVisible = lmn.BitesLeft > 2;
            sLeaser.sprites[lmn.StartOfOutlineSprites + 1].isVisible = lmn.BitesLeft > 2;

            sLeaser.sprites[09].isVisible = lmn.BitesLeft > 3;
            sLeaser.sprites[10].isVisible = lmn.BitesLeft > 3;
            sLeaser.sprites[13].isVisible = lmn.BitesLeft > 3;
            sLeaser.sprites[14].isVisible = lmn.BitesLeft > 3;
            sLeaser.sprites[lmn.StartOfBodySprites + 2].isVisible = lmn.BitesLeft > 3;
            sLeaser.sprites[lmn.StartOfOutlineSprites + 2].isVisible = lmn.BitesLeft > 3;

            sLeaser.sprites[11].isVisible = lmn.BitesLeft > 4;
            sLeaser.sprites[12].isVisible = lmn.BitesLeft > 4;
            sLeaser.sprites[15].isVisible = lmn.BitesLeft > 4;
            sLeaser.sprites[16].isVisible = lmn.BitesLeft > 4;
            sLeaser.sprites[lmn.StartOfBodySprites + 3].isVisible = lmn.BitesLeft > 4;
            sLeaser.sprites[lmn.StartOfOutlineSprites + 3].isVisible = lmn.BitesLeft > 4;
        }
        
    }
    public static void LuminescipedeSpriteContainer(On.SpiderGraphics.orig_AddToContainer orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(sg, sLeaser, rCam, newContainer);
        if (sg?.spider is not null && sLeaser.sprites.Length > 17 && sg.spider is Luminescipede lmn)
        {
            var foregroundContainer = rCam.ReturnFContainer("Foreground");
            var midgroundContainer = rCam.ReturnFContainer("Midground");

            for (int s = 17; s < lmn.TotalSprites; s++)
            {
                foregroundContainer.RemoveChild(sLeaser.sprites[s]);
                midgroundContainer.AddChild(sLeaser.sprites[s]);
            }

            for (int o = lmn.StartOfOutlineSprites; o < lmn.TotalSprites; o++)
            {
                sLeaser.sprites[o].MoveToBack();
            }
            sLeaser.sprites[lmn.DecalSprite].MoveToFront();
        }
    }
    #endregion

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Big Spiders
    public static void WinterMotherSpiders(On.BigSpider.orig_ctor orig, BigSpider bigSpd, AbstractCreature absCtr, World world)
    {
        orig(bigSpd, absCtr, world);
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsRWGIncan(world?.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            absCtr.state.meatLeft = 9;
            if (bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.33f;
                    bigSpd.bodyChunks[b].rad *= 1.33f;
                }
            }
            if (CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI))
            {
                aI.functionTimer = 450 + (int)(HSRemix.MotherSpiderEvenMoreSpiders.Value * 10);
            }
        }
        if (IsRWGIncan(world?.game) || HSRemix.BigSpiderColorsEverywhere.Value is true)
        {
            if (IsRWGIncan(world?.game) && bigSpd.Template.type != MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.10f;
                    bigSpd.bodyChunks[b].rad *= 1.15f;
                }
            }

            if (bigSpd.Template.type == CreatureTemplate.Type.BigSpider)
            {
                Random.State state = Random.state;
                Random.InitState(bigSpd.abstractCreature.ID.RandomSeed);
                bigSpd.yellowCol =
                        Color.Lerp(
                            Custom.HSL2RGB(Random.Range(30 / 360f, 70 / 360f), Random.Range(0.5f, 1f), Random.Range(0.3f, 0.5f)),
                            Custom.HSL2RGB(Random.value, Random.value, Random.value),
                            Random.value * 0.2f);
                Random.state = state;
            }
            else if (bigSpd.Template.type == CreatureTemplate.Type.SpitterSpider)
            {
                Random.State state = Random.state;
                Random.InitState(bigSpd.abstractCreature.ID.RandomSeed);
                bigSpd.yellowCol =
                        Color.Lerp(
                            Custom.HSL2RGB(Random.Range(-60 / 360f, 20 / 360f), Random.Range(0.5f, 1f), Random.Range(0.2f, 0.4f)),
                            Custom.HSL2RGB(Random.value, Random.value, Random.value),
                            Random.value * 0.2f);
                Random.state = state;
            }
            else if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
            {
                Random.State state = Random.state;
                Random.InitState(bigSpd.abstractCreature.ID.RandomSeed);
                bigSpd.yellowCol =
                        Color.Lerp(
                            Custom.HSL2RGB(Random.Range(120 / 360f, 160 / 360f), Random.Range(0.5f, 1f), Random.Range(0.4f, 0.6f)),
                            Custom.HSL2RGB(Random.value, Random.value, Random.value),
                            Random.value * 0.2f);
                Random.state = state;
            }
        }
    }

    //-----------------------------------------

    public static void WinterMotherSpiderBabySpawn(On.BigSpider.orig_Update orig, BigSpider bigSpd, bool eu)
    {
        orig(bigSpd, eu);
        if (bigSpd?.room is not null && bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsRWGIncan(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo aI))
        {
            // Partially counteracts the Mother Spider's regeneration, or else it would be WAY too fast thanks to the health increase that Mother Spiders get (it's pretty much percentage-based).
            if (bigSpd.State.health > 0 && bigSpd.State.health < 1)
            {
                bigSpd.State.health -=
                    0.0014705883f * (bigSpd.State.health < 0.5f ? 0.5f : 0.875f);
            }

            // Once a creature is on a Mother Spider's shitlist, the Mother won't be afraid of it anymore. At least, that's what I HOPE this does...
            if (aI.ctrList is not null && aI.ctrList.Count > 0 && bigSpd.AI?.relationshipTracker?.relationships is not null)
            {
                for (int r = 0; r < bigSpd.AI.relationshipTracker.relationships.Count; r++)
                {
                    for (int s = 0; s < aI.ctrList.Count; s++)
                    {
                        RelationshipTracker.DynamicRelationship relationship = bigSpd.AI.relationshipTracker.relationships[r];
                        if (relationship.trackerRep.representedCreature is not null &&
                            relationship.trackerRep.representedCreature == aI.ctrList[s] &&
                            (relationship.state as BigSpiderAI.SpiderTrackState).accustomed < 600)
                        {
                            (relationship.state as BigSpiderAI.SpiderTrackState).accustomed = 1800;
                            break;
                        }
                    }
                }
            }

            // Allows the Mother Spider to spawn little baby spiders while it's in stun.
            if (aI.functionTimer > 0 &&
                !bigSpd.dead &&
                !bigSpd.Consious &&
                !bigSpd.spewBabies &&
                !bigSpd.inShortcut &&
                !bigSpd.slatedForDeletetion &&
                bigSpd.room.world is not null &&
                bigSpd.room.game.cameras[0].room == bigSpd.room)
            {
                aI.functionTimer--;
                if (aI.functionTimer % 10 == 0)
                {
                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < bigSpd.room.updateList.Count; i++)
                    {
                        if (bigSpd.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = bigSpd.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    Vector2 pos = bigSpd.mainBodyChunk.pos;
                    for (int j = 0; j < 5; j++)
                    {
                        SporeCloud sC = new(pos, Custom.RNV() * Random.value * 10f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.4f), 1f, null, j % 20, smallInsects);
                        sC.nonToxic = true;
                        bigSpd.room.AddObject(sC);
                    }

                    SporePuffVisionObscurer sPVO = new(pos);
                    sPVO.doNotCallDeer = true;
                    bigSpd.room.AddObject(sPVO);

                    bigSpd.room.AddObject(new PuffBallSkin(pos, Custom.RNV() * Random.value * 16f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), Color.Lerp(bigSpd.yellowCol, Color.black, 0.6f)));

                    AbstractCreature absSpd = new(bigSpd.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, bigSpd.room.GetWorldCoordinate(pos), bigSpd.room.world.game.GetNewID())
                    {
                        Winterized = bigSpd.abstractCreature.Winterized,
                        ignoreCycle = bigSpd.abstractCreature.ignoreCycle,
                        HypothermiaImmune = bigSpd.abstractCreature.HypothermiaImmune
                    };
                    bigSpd.room.abstractRoom.AddEntity(absSpd);
                    absSpd.RealizeInRoom();
                    (absSpd.realizedCreature as Spider).bloodLust = 1f;
                }
            }
        }
    }
    public static void WinterMotherCRONCH(On.BigSpider.orig_Collide orig, BigSpider bigSpd, PhysicalObject obj, int myChunk, int otherChunk)
    {
        if (HSRemix.MotherSpiderCRONCH.Value is true &&
            obj is not null &&
            obj is Creature target &&
            !target.dead &&
            target is not Spider &&
            target is not BigSpider &&
            bigSpd?.room is not null &&
            bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider &&
            (IsRWGIncan(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) &&
            bigSpd.bodyChunks[myChunk].vel.magnitude >= 12.5f &&
            bigSpd.TotalMass > target.TotalMass &&
            CWT.CreatureData.TryGetValue(bigSpd, out CreatureInfo jI) &&
            CWT.CreatureData.TryGetValue(target, out CreatureInfo vI) &&
            (jI.impactCooldown == 0 || vI.impactCooldown == 0))
        {
            jI.impactCooldown = 40;
            vI.impactCooldown = 40;

            float damage =
                bigSpd.bodyChunks[myChunk].vel.magnitude >= 22.5f ? 1.0f :
                bigSpd.bodyChunks[myChunk].vel.magnitude >= 17.5f ? 0.5f : 0;

            float stun =
                Mathf.Lerp(25, 50, Mathf.InverseLerp(12.5f, 22.5f, bigSpd.bodyChunks[myChunk].vel.magnitude));

            target.Violence(bigSpd.bodyChunks[myChunk], bigSpd.bodyChunks[myChunk].vel, target.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, damage, stun);

            bigSpd.room.PlaySound(SoundID.Cicada_Heavy_Terrain_Impact, bigSpd.bodyChunks[myChunk]);
            bigSpd.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, bigSpd.bodyChunks[myChunk], false, 1.4f, 1.1f);
            bigSpd.room.PlaySound(SoundID.Rock_Hit_Wall, bigSpd.bodyChunks[myChunk], false, 1.5f, Random.Range(0.66f, 0.8f));
            if (target.State is HealthState HS && HS.ClampedHealth == 0 || target.State.dead)
            {
                bigSpd.room.PlaySound(SoundID.Spear_Stick_In_Creature, bigSpd.bodyChunks[myChunk], false, 1.7f, Random.Range(0.6f, 0.8f));
            }
        }
        orig(bigSpd, obj, myChunk, otherChunk);
    }
    public static void WinterMotherHP(On.BigSpider.orig_Violence orig, BigSpider bigSpd, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType dmgType, float dmg, float stun)
    {
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsRWGIncan(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            dmg *= 0.5f; // 2x HP
            stun *= 0.4f;
            if (source?.owner is not null && source.owner is Creature ctr && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo abI) && abI.ctrList is not null && !abI.ctrList.Contains(ctr.abstractCreature))
            {
                abI.ctrList.Add(ctr.abstractCreature);
            }
        }
        orig(bigSpd, source, dirAndMomentum, hitChunk, hitAppendage, dmgType, dmg, stun);
    }
    public static void WinterMotherSpiderBabyPuff(On.BigSpider.orig_Die orig, BigSpider bigSpd)
    {
        if (bigSpd?.room is not null && (IsRWGIncan(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo aI))
        {
            if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
            {
                bigSpd.spewBabies = true;

                if (aI.functionTimer > 0 &&
                    !bigSpd.dead &&
                    !bigSpd.Consious &&
                    !bigSpd.inShortcut &&
                    !bigSpd.slatedForDeletetion &&
                    bigSpd.room.world is not null &&
                    bigSpd.room.game.cameras[0].room == bigSpd.room)
                {
                    int remainingSpiders = aI.functionTimer / 10;
                    aI.functionTimer = 0;

                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < bigSpd.room.updateList.Count; i++)
                    {
                        if (bigSpd.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = bigSpd.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    Vector2 pos = bigSpd.mainBodyChunk.pos;
                    for (int j = 0; j < remainingSpiders * 2; j++)
                    {
                        SporeCloud sC = new(pos, Custom.RNV() * Random.value * 10f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), 1f, null, j % 20, smallInsects);
                        sC.nonToxic = true;
                        bigSpd.room.AddObject(sC);
                    }

                    SporePuffVisionObscurer sPVO = new(pos);
                    sPVO.doNotCallDeer = true;
                    bigSpd.room.AddObject(sPVO);

                    for (int k = 0; k < remainingSpiders * 0.2f; k++)
                    {
                        bigSpd.room.AddObject(new PuffBallSkin(pos, Custom.RNV() * Random.value * 16f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), Color.Lerp(bigSpd.yellowCol, Color.black, 0.6f)));
                    }

                    for (int s = 0; s < remainingSpiders; s++)
                    {
                        AbstractCreature absSpd = new(bigSpd.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, bigSpd.room.GetWorldCoordinate(pos), bigSpd.room.world.game.GetNewID())
                        {
                            Winterized = bigSpd.abstractCreature.Winterized,
                            ignoreCycle = bigSpd.abstractCreature.ignoreCycle,
                            HypothermiaImmune = bigSpd.abstractCreature.HypothermiaImmune
                        };
                        bigSpd.room.abstractRoom.AddEntity(absSpd);
                        absSpd.RealizeInRoom();
                        (absSpd.realizedCreature as Spider).bloodLust = 1f;
                    }
                }
            }
        }
        orig(bigSpd);
    }
    public static CreatureTemplate.Relationship WinterMotherSpiderHostility(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI AI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        if (AI?.bug?.room is not null && AI.bug.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsRWGIncan(AI.bug.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(AI.bug.abstractCreature, out AbsCtrInfo aI))
        {
            foreach (AbstractCreature absCtr in aI.ctrList)
            {
                if (absCtr?.realizedCreature is not null &&
                    dynamRelat.trackerRep.representedCreature.realizedCreature is not null &&
                    absCtr.realizedCreature == dynamRelat.trackerRep.representedCreature.realizedCreature &&
                    !dynamRelat.trackerRep.representedCreature.state.dead)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Attacks, 1.2f);
                }
            }
        }
        return orig(AI, dynamRelat);
    }
    #endregion

}

public class Luminescipede : Spider, IPlayerEdible
{

    private int bites = 5;
    public int BitesLeft => bites;
    public int FoodPoints => 1;
    public bool Edible => dead;
    public bool AutomaticPickUp => dead;

    //-----------------------------------------

    public Color baseColor;
    public Color glowColor;
    public Color currentGlowColor;
    
    public float juice;
    public bool flicker;
    public LightSource spiderGlow;
    public Vector2 glowPos;
    public float chargeFac => Mathf.InverseLerp(0, 0.8f, juice);
    public bool charged => juice >= 0.8f;
    public bool overloaded;

    public int StartOfBodySprites;
    public int DecalSprite;
    public int StartOfOutlineSprites;
    public int TotalSprites;

    public ChunkDynamicSoundLoop lightNoise;

    public HealthState lmnState => State as HealthState;

    public Luminescipede(AbstractCreature absSpd, World world) : base(absSpd, world)
    {
        Random.State state = Random.state;
        Random.InitState(absSpd.ID.RandomSeed);
        baseColor = Custom.HSL2RGB((Random.value < 0.04f ? Random.value : Custom.WrappedRandomVariation(260 / 360f, 60 / 360f, 0.5f)), 0.4f, Custom.WrappedRandomVariation(0.5f, 0.125f, 0.3f));
        glowColor = Color.Lerp(baseColor, Color.white, 0.7f);
        Random.state = state;
    }

    //-----------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (lightNoise is null)
        {
            lightNoise = new ChunkDynamicSoundLoop(mainBodyChunk);
        }
        if (lightNoise is not null)
        {
            lightNoise.Update();
            if (charged)
            {
                lightNoise.sound = SoundID.Mouse_Light_On_LOOP;
                lightNoise.Volume = 1f - 0.6f * Mathf.Lerp(0.8f, 1, juice);
                lightNoise.Pitch = 1f - 0.3f * Mathf.Pow(Mathf.Lerp(0.8f, 1, juice), 0.6f);
            }
            else if (!overloaded)
            {
                lightNoise.sound = SoundID.Mouse_Charge_LOOP;
                lightNoise.Volume = 0.2f + juice / 2f;
                lightNoise.Pitch = Custom.LerpMap(juice, 0, 0.8f, 0.33f, 1);
            }
            else
            {
                lightNoise.sound = SoundID.None;
                lightNoise.Volume = 0f;
            }
        }

        // From here on is code for these spiders' glows.
        if (dead && juice > 0.005f)
        {
            juice -= Random.Range(0.0005f, 0.001f);
            return;
        }

        if (juice <= 1)
        {
            if (!flicker && Random.value < 0.01f) flicker = true;
            else if (flicker && Random.value < 0.1f) flicker = false;
        }

        if (overloaded)
        {
            if (charged)
            {
                juice *= 1.02f;
                if (juice >= 1.5f) juice = 0;
            }

            if (!Stunned)
            {
                overloaded = false;
                bloodLust = 1;
            }
        }
        else
        {
            if (!charged)
            {
                juice += Random.Range(0.002f, 0.003f) * (1 - room.Darkness(mainBodyChunk.pos));
                if (charged)
                {
                    juice += 0.15f;
                    room.PlaySound(SoundID.Mouse_Light_Switch_On, mainBodyChunk);
                    for (int i = 0; i < 20; i++)
                    {
                        room.AddObject(new MouseSpark(mainBodyChunk.pos, Custom.RNV() * Random.Range(3f, 7f), 40f, currentGlowColor));
                    }
                }
            }
            else
            {
                juice =
                    Random.value < 0.3f ? Custom.WrappedRandomVariation(0.9f, 0.1f, 0.3f) : Mathf.Lerp(juice, 0.9f, 0.08f);
            }
        }

        if (lmnState.ClampedHealth < 0.5f)
        {
            if (Random.value < (1f - lmnState.ClampedHealth) / 50f)
            {
                Stun((int)(Mathf.Lerp(25, 5, lmnState.ClampedHealth) * Random.Range(0.5f, 1.5f)));
            }
        }

        if (Consious && lmnState.health > 0 && lmnState.health < 1)
        {
            lmnState.health = Mathf.Min(1f, lmnState.health + 0.00075f);
        }

        if (Random.value < (Mathf.Lerp(0.008f, 0.04f, 1 - lmnState.health) * juice))
        {
            for (int i = 0; i < Random.Range(4, 8); i++)
            {
                room.AddObject(new MouseSpark(mainBodyChunk.pos, Custom.RNV() * Random.Range(3f, 7f), 40f, currentGlowColor));
            }
        }

        if (juice > 0.05f && spiderGlow is null)
        {
            spiderGlow = new LightSource(glowPos, false, currentGlowColor, this)
            {
                affectedByPaletteDarkness = 0,
                submersible = true,
                requireUpKeep = true
            };
            room.AddObject(spiderGlow);
        }
        else if (spiderGlow is not null)
        {
            spiderGlow.stayAlive = true;
            spiderGlow.setPos = new Vector2?(glowPos);
            spiderGlow.setRad = new float?(Mathf.Lerp(spiderGlow.rad, (flicker ? 40 : 200) * juice * iVars.size, 0.05f));
            spiderGlow.setAlpha = new float?(juice);
            spiderGlow.color = currentGlowColor;
            if (juice <= 0.05f || spiderGlow.slatedForDeletetion || spiderGlow.room != room)
            {
                spiderGlow = null;
            }
        }

    }

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;
        if (State is HealthState HS)
        {
            HS.health -= 0.35f;
            if (HS.health <= 0) Die();
        }
        room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, firstChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1)
        {
            Player eater = grasp.grabber as Player;
            eater.ObjectEaten(this);
            if (!eater.isNPC)
            {
                if (room.game.session is StoryGameSession SGS)
                {
                    SGS.saveState.theGlow = true;
                }
            }
            else
            {
                (eater.State as PlayerNPCState).Glowing = true;
            }
            eater.glowing = true;
            grasp.Release();
            Destroy();
        }
    }

    public void ThrowByPlayer()
    {
    }

    public override Color ShortCutColor()
    {
        return currentGlowColor;
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);

        if (!firstContact) return;

        for (int s = 0; s < Mathf.Lerp(0, 20, juice); s++)
        {
            room.AddObject(new MouseSpark(mainBodyChunk.pos, new Vector2(mainBodyChunk.vel.x * Random.Range(0.6f, 1.4f), mainBodyChunk.vel.y * Random.Range(0.6f, 1.4f)) + Custom.DegToVec(360f * Random.value) * 7f * Random.value, 40f, currentGlowColor));
        }
    }
}