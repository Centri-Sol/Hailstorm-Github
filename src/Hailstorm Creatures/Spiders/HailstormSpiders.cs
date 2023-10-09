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
using System.Text.RegularExpressions;
using UnityEngine.Experimental.GlobalIllumination;
using System.Runtime.ConstrainedExecution;
using static Hailstorm.GlowSpiderState.State;

namespace Hailstorm;

internal class HailstormSpiders
{
    public static void Hooks()
    {
        On.CreatureSymbol.DoesCreatureEarnATrophy += LuminescipedesShowIcons;
        On.AbstractCreature.Update += AbstractLuminescipedeTimer;
        //On.AbstractPhysicalObject.CreatureGripStick.ctor += NoLuminGrabStunning;

        // Baby Spiders
        On.Spider.ctor += WinterCoalescipedes;
        On.Spider.Centipede.ctor += WinterCoalescipedeChains;
        On.Spider.Update += WinterCoalescipedeSaysNoToDens;
        On.Spider.Move_Vector2 += WinterCoalescipedeSpeed;
        On.Spider.ConsiderPrey += WinterCoalescipedeHigherMassLimit;
        On.SpiderGraphics.InitiateSprites += WinterCoalescipedeSpriteSizes;
        On.SpiderGraphics.ApplyPalette += WinterCoalescipedeColors;
        SpiderILHooks();

        // Big Spiders
        On.BigSpider.ctor += WinterSpiders;
        On.BigSpider.Update += WinterMotherSpiderBabySpawn;
        On.BigSpider.Collide += WinterMotherCRONCH;
        On.BigSpider.Violence += WinterMotherHP;
        On.BigSpider.Die += WinterMotherSpiderBabyPuff;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += WinterMotherSpiderHostility;
        On.BigSpiderAI.TryAddReviveBuddy += PeachSpiderShouldntRevive;
        On.BigSpider.Revive += PeachSpiderGetsRevivedForLonger;
        On.BigSpider.ShortCutColor += PeachSpiderShortcutColor;

        On.BigSpiderGraphics.ctor += PeachSpiderScales;
        On.BigSpiderGraphics.ApplyPalette += PeachSpiderColors;
        On.BigSpiderGraphics.InitiateSprites += WinterSpiderSprites;
        On.BigSpiderGraphics.DrawSprites += WinterSpiderVisuals;

    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG is not null && RWG.IsStorySession && RWG.StoryCharacter == HSSlugs.Incandescent);
    }

    public static bool LuminescipedesShowIcons(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureTemplate.Type type)
    {
        if (type == HailstormEnums.Luminescipede)
        {
            return true;
        }
        return orig(type);
    }
    public static void AbstractLuminescipedeTimer(On.AbstractCreature.orig_Update orig, AbstractCreature absCtr, int time)
    {
        orig(absCtr, time);
        if (absCtr?.state is not null && !absCtr.state.dead && absCtr.state is GlowSpiderState gss && gss.seenNoPreyCounter < 1000)
        {
            gss.seenNoPreyCounter++;
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------


    #region Baby Spiders
    public static bool IsWinterCoalescipede(Spider spd)
    {
        return spd?.room?.game is not null && spd.Template.type == CreatureTemplate.Type.Spider &&
            (HSRemix.HailstormBabySpidersEverywhere.Value is true || (IsIncanStory(spd.room.game) && spd.abstractCreature.Winterized));
    }
    public static void WinterCoalescipedes(On.Spider.orig_ctor orig, Spider spd, AbstractCreature absCtr, World world)
    {
        orig(spd, absCtr, world);
        if (spd is not null && IsWinterCoalescipede(spd))
        {
            Random.State state = Random.state;
            Random.InitState(spd.abstractCreature.ID.RandomSeed);
            spd.iVars = new Spider.IndividualVariations(Random.Range(0.4f, 1.2f));
            Random.state = state;

            spd.gravity = 0.85f;
            spd.bounce = 0.15f;
            spd.surfaceFriction = 0.9f;
            if (spd.bodyChunks is not null && spd.bodyChunks[0] is not null)
            {
                spd.bodyChunks[0].rad = Mathf.Lerp(4.4f, 10.8f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                spd.bodyChunks[0].mass = Mathf.Lerp(0.08f, 0.18f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
            }
            spd.denMovement = -1;
        }
    }
    public static void WinterCoalescipedeChains(On.Spider.Centipede.orig_ctor orig, Spider.Centipede cls, Spider origSpd, Room room)
    {
        orig(cls, origSpd, room);
        if (cls is not null && origSpd is not null && IsWinterCoalescipede(origSpd))
        {
            cls.maxSize = Random.Range(20, 31);

        }
    }

    //-----------------------------------------

    public static void WinterCoalescipedeSaysNoToDens(On.Spider.orig_Update orig, Spider spd, bool eu)
    {
        orig(spd, eu);
        if (spd?.room is not null && IsWinterCoalescipede(spd) && spd.denPos is not null)
        {
            spd.denPos = null;
        }
    }
    public static void WinterCoalescipedeSpeed(On.Spider.orig_Move_Vector2 orig, Spider spd, Vector2 dest)
    {
        Vector2? vel = null;
        if (spd is not null && IsWinterCoalescipede(spd))
        {
            vel = spd.mainBodyChunk.vel;
        }
        orig(spd, dest);
        if (vel.HasValue)
        {
            vel = spd.mainBodyChunk.vel - vel;
            if (spd.State is GlowSpiderState gs)
            {
                spd.mainBodyChunk.vel += vel.Value * Mathf.Lerp(-1, 1, gs.ClampedHealth);
            }
            else 
            {
                spd.mainBodyChunk.vel += vel.Value;
            }
        }
    }
    public static bool WinterCoalescipedeHigherMassLimit(On.Spider.orig_ConsiderPrey orig, Spider spd, Creature ctr)
    {
        if (spd is not null && IsWinterCoalescipede(spd) && ctr.TotalMass <= 6.72f && spd.Template.CreatureRelationship(ctr.Template).type == CreatureTemplate.Relationship.Type.Eats && !ctr.leechedOut)
        {
            return true;
        }
        return orig(spd, ctr);
    }

    public static void SpiderILHooks()
    {
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
    }
    public static bool LightAdaptionChanges(Spider.Centipede cls, bool eu)
    {
        if (cls?.FirstSpider is null || !IsWinterCoalescipede(cls.FirstSpider)) return false;

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


    //-----------------------------------------

    public static void WinterCoalescipedeSpriteSizes(On.SpiderGraphics.orig_InitiateSprites orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(sg, sLeaser, rCam);
        if (sg?.spider is not null && IsWinterCoalescipede(sg.spider))
        {
            Spider spd = sg.spider;

            sLeaser.sprites[sg.BodySprite].scale = spd.iVars.size;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[sg.LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                    sLeaser.sprites[sg.LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                }
            }
        }
    }
    public static void WinterCoalescipedeColors(On.SpiderGraphics.orig_ApplyPalette orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(sg, sLeaser, rCam, palette);
        if (sg is not null && IsWinterCoalescipede(sg.spider))
        {
            sg.blackColor = Custom.HSL2RGB(210 / 360f, 0.1f, Mathf.Lerp(0.2f, 0.3f, Mathf.InverseLerp(0.4f, 1.2f, sg.spider.iVars.size)));
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = sg.blackColor;
            }
        }
    }

    #endregion


    //----------------------------------------------------------------------------------


    #region Big Spiders
    public static void WinterSpiders(On.BigSpider.orig_ctor orig, BigSpider bigSpd, AbstractCreature absSpd, World world)
    {
        orig(bigSpd, absSpd, world);
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(world?.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            absSpd.state.meatLeft = 9;
            if (bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.33f;
                    bigSpd.bodyChunks[b].rad *= 1.33f;
                }
            }
            if (CWT.AbsCtrData.TryGetValue(absSpd, out AbsCtrInfo aI))
            {
                aI.functionTimer = 450 + (int)(HSRemix.MotherSpiderEvenMoreSpiders.Value * 10);
            }
        }

        if (bigSpd.Template.type == HailstormEnums.PeachSpider)
        {
            for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
            {
                bigSpd.bodyChunks[b].mass *= 0.2f;
                bigSpd.bodyChunks[b].rad *= 0.8f;
            }
            Random.State state = Random.state;
            Random.InitState(absSpd.ID.RandomSeed);
            float hue =
                (Random.value < 0.8f) ? 209 :
                (Random.value < 0.8f) ? (0.209f + Custom.ClampedRandomVariation(0, 0.151f, 0.33f)) * 1000 : 1;
            hue /= 360f;
            float sat = Mathf.Lerp(0.39f, 0.6f, Mathf.InverseLerp(209/360f, 1, hue));
            float bri = 0.55f + Random.Range(-0.05f, 0.05f);
            bigSpd.yellowCol = Custom.HSL2RGB(hue, sat, bri);
            Random.state = state;
        }
        else if (IsIncanStory(world?.game) || HSRemix.BigSpiderColorsEverywhere.Value is true)
        {
            if (IsIncanStory(world?.game) && bigSpd.Template.type != MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.15f;
                    bigSpd.bodyChunks[b].rad *= 1.15f;
                }
            }
            
            if (bigSpd.Template.type == CreatureTemplate.Type.BigSpider)
            {
                Random.State state = Random.state;
                Random.InitState(absSpd.ID.RandomSeed);
                bigSpd.yellowCol = Color.Lerp(
                    Custom.HSL2RGB(Random.Range(30 / 360f, 70 / 360f), Random.Range(0.5f, 1f), Random.Range(0.3f, 0.5f)),
                    Custom.HSL2RGB(Random.value, Random.value, Random.value),
                    Random.value * 0.2f);
                Random.state = state;
            }
            else if (bigSpd.Template.type == CreatureTemplate.Type.SpitterSpider)
            {
                Random.State state = Random.state;
                Random.InitState(absSpd.ID.RandomSeed);
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
                Random.InitState(absSpd.ID.RandomSeed);
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
        if (bigSpd?.room is null) return;

        if (bigSpd.Template.type == HailstormEnums.PeachSpider)
        {
            if (bigSpd.jumpStamina < 0.2f)
            {
                bigSpd.jumpStamina = 0.2f;
            }
            if (bigSpd.canCling > 0)
            {
                bigSpd.canCling = 0;
            }
        }
        else if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo aI))
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
        if (bigSpd?.AI is not null && bigSpd.Template.type == HailstormEnums.PeachSpider && obj is Creature ctr)
        {
            bigSpd.AI.tracker.SeeCreature(ctr.abstractCreature);
            List<RelationshipTracker.DynamicRelationship> relationships = bigSpd.AI.relationshipTracker.relationships;
            for (int i = 0; i < relationships.Count; i++)
            {
                if (relationships[i].trackerRep.representedCreature == ctr.abstractCreature)
                {
                    (relationships[i].state as BigSpiderAI.SpiderTrackState).accustomed += 1800;
                    break;
                }
            }
        }
        else if (HSRemix.MotherSpiderCRONCH.Value is true &&
            obj is not null &&
            obj is Creature target &&
            !target.dead &&
            target is not Spider &&
            target is not BigSpider &&
            bigSpd?.room is not null &&
            bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider &&
            (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) &&
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
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            dmg /= 2f; // 2x HP
            stun *= 0.4f;
            if (source?.owner is not null && source.owner is Creature ctr && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo abI) && abI.ctrList is not null && !abI.ctrList.Contains(ctr.abstractCreature))
            {
                abI.ctrList.Add(ctr.abstractCreature);
            }
        }
        if (IsIncanStory(bigSpd?.room?.game) && bigSpd.Template.type == CreatureTemplate.Type.BigSpider && bigSpd.abstractCreature.Winterized)
        {
            dmg /= 2f;
        }
        orig(bigSpd, source, dirAndMomentum, hitChunk, hitAppendage, dmgType, dmg, stun);
    }
    public static void WinterMotherSpiderBabyPuff(On.BigSpider.orig_Die orig, BigSpider bigSpd)
    {
        if (bigSpd?.room is not null && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out AbsCtrInfo aI))
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
        if (AI?.bug?.room is not null && AI.bug.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(AI.bug.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(AI.bug.abstractCreature, out AbsCtrInfo aI))
        {
            foreach (AbstractCreature absCtr in aI.ctrList)
            {
                if (absCtr?.realizedCreature is not null && dynamRelat.trackerRep.representedCreature.realizedCreature is not null &&
                    absCtr.realizedCreature == dynamRelat.trackerRep.representedCreature.realizedCreature && !dynamRelat.trackerRep.representedCreature.state.dead)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Attacks, 1.2f);
                }
            }
        }
        return orig(AI, dynamRelat);
    }
    public static void PeachSpiderShouldntRevive(On.BigSpiderAI.orig_TryAddReviveBuddy orig, BigSpiderAI AI, Tracker.CreatureRepresentation candidate)
    {
        orig(AI, candidate);
        if (AI?.bug is not null && AI.bug.Template.type == HailstormEnums.PeachSpider)
        {
            AI.reviveBuddy = null;
        }
    }
    public static void PeachSpiderGetsRevivedForLonger(On.BigSpider.orig_Revive orig, BigSpider bigSpd)
    {
        orig(bigSpd);
        if (bigSpd is not null && bigSpd.Template.type == HailstormEnums.PeachSpider)
        {
            bigSpd.borrowedTime += 500;
        }
    }
    public static Color PeachSpiderShortcutColor(On.BigSpider.orig_ShortCutColor orig, BigSpider bigSpd)
    {
        if (bigSpd.Template.type == HailstormEnums.PeachSpider)
        {
            return bigSpd.yellowCol * 0.75f;
        }
        return orig(bigSpd);
    }

    public static void PeachSpiderScales(On.BigSpiderGraphics.orig_ctor orig, BigSpiderGraphics bsg, PhysicalObject owner)
    {
        orig(bsg, owner);
        if (bsg?.bug is not null && bsg.bug.Template.type == HailstormEnums.PeachSpider)
        {
            Random.State state = Random.state;
            Random.InitState(bsg.bug.abstractCreature.ID.RandomSeed);
            bsg.totalScales = 0;
            bsg.scales = new Vector2[Random.Range(36, 51)][,];
            bsg.scaleStuckPositions = new Vector2[bsg.scales.Length];
            bsg.scaleSpecs = new Vector2[bsg.scales.Length, 2];
            bsg.legsThickness = Mathf.Lerp(0.6f, 0.8f, Random.value);
            bsg.bodyThickness = 0.4f + Mathf.Lerp(0.9f, 1.1f, Random.value);
            int num = 0;
            for (int s = 0; s < bsg.scales.Length; s++)
            {
                bsg.scaleSpecs[s, 0] = new Vector2(Random.value, 5f);
                bsg.scales[s] = new Vector2[Random.Range(5, 8), 4];
                bsg.totalScales += bsg.scales[s].GetLength(0);
                for (int num4 = 0; num4 < bsg.scales[s].GetLength(0); num4++)
                {
                    bsg.scales[s][num4, 3].x = num;
                    num++;
                }
                bsg.scaleStuckPositions[s] = new Vector2(Mathf.Lerp(-0.5f, 0.5f, Random.value), Mathf.Pow(Random.value, Custom.LerpMap(bsg.scales[s].GetLength(0), 2f, 9f, 0.5f, 2f)));
                if (s % 3 == 0 && bsg.scaleStuckPositions[s].y > 0.5f)
                {
                    bsg.scaleStuckPositions[s].y *= 0.5f;
                }
            }
            bsg.Reset();
            Random.state = state;
        }
    }
    public static void PeachSpiderColors(On.BigSpiderGraphics.orig_ApplyPalette orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(bsg, sLeaser, rCam, palette);
        if (bsg?.bug is not null && bsg.bug.Template.type == HailstormEnums.PeachSpider)
        {
            Random.State state = Random.state;
            Random.InitState(bsg.bug.abstractCreature.ID.altSeed);
            if (Random.value < 0.85f)
            {
                bsg.blackColor = bsg.bug.yellowCol;
                bsg.yellowCol = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), bsg.blackColor, 0.07f);
                bsg.blackColor = Color.Lerp(bsg.blackColor, palette.blackColor, bsg.darkness / 2f);
                bsg.yellowCol = Color.Lerp(bsg.yellowCol, palette.fogColor, 0.1f + (bsg.darkness * 0.4f));
            }
            else
            {
                bsg.blackColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), bsg.yellowCol, 0.07f);
                bsg.yellowCol = bsg.bug.yellowCol;
                bsg.blackColor = Color.Lerp(bsg.blackColor, palette.fogColor, 0.1f + (bsg.darkness * 0.4f));
                bsg.yellowCol = Color.Lerp(bsg.yellowCol, palette.blackColor, bsg.darkness / 2f);
            }
            Random.state = state;

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = bsg.blackColor;
            }
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[bsg.MandibleSprite(j, 1)].color = bsg.yellowCol;
                for (int k = 0; k < 3; k++)
                {
                    (sLeaser.sprites[bsg.MandibleSprite(j, 1)] as CustomFSprite).verticeColors[k] = bsg.blackColor;
                }
            }
            for (int side = 0; side < bsg.legs.GetLength(0); side++)
            {
                for (int leg = 0; leg < bsg.legs.GetLength(1); leg++)
                {
                    for (int part = 0; part < 3; part++)
                    {
                        sLeaser.sprites[bsg.LegSprite(side, leg, part)].color = Color.Lerp(bsg.blackColor, palette.fogColor, Mathf.InverseLerp(1, 5, part * (1 + bsg.darkness)));
                    }
                }
            }
            for (int scale = 0; scale < bsg.scales.Length; scale++)
            {
                for (int part = 0; part < bsg.scales[scale].GetLength(0); part++)
                {
                    float colLerp = (Mathf.InverseLerp(0, bsg.scales[scale].GetLength(0) / 2, part) + Mathf.InverseLerp(-bsg.scales.Length / 2, bsg.scales.Length / 2, scale)) / 2f;
                    sLeaser.sprites[bsg.FirstScaleSprite + (int)bsg.scales[scale][part, 3].x].color = Color.Lerp(bsg.blackColor, bsg.yellowCol, colLerp);
                }
            }
            
        }
    }
    public static void WinterSpiderSprites(On.BigSpiderGraphics.orig_InitiateSprites orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(bsg, sLeaser, rCam);
        if (bsg?.bug is not null && (bsg.bug.Template.type == HailstormEnums.PeachSpider || (IsIncanStory(bsg.bug.room?.game) && bsg.bug.Template.type == CreatureTemplate.Type.BigSpider)))
        {
            for (int s = 0; s < sLeaser.sprites.Length; s++)
            {
                if (sLeaser.sprites[s] is not TriangleMesh && sLeaser.sprites[s] is not CustomFSprite)
                {
                    sLeaser.sprites[s].scale *=
                        bsg.bug.Template.type == HailstormEnums.PeachSpider ? 0.2f : 1.15f;
                }
            }
        }
    }
    public static void WinterSpiderVisuals(On.BigSpiderGraphics.orig_DrawSprites orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(bsg, sLeaser, rCam, timeStacker, camPos);
        if (bsg?.bug?.room is not null && (bsg.bug.Template.type == HailstormEnums.PeachSpider || IsIncanStory(bsg.bug.room.game)))
        {
            if (bsg.bug.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider || bsg.bug.Template.type == CreatureTemplate.Type.BigSpider)
            {
                if (sLeaser.sprites[bsg.MeshSprite] is TriangleMesh mesh)
                {
                    for (int v = 0; v < mesh.vertices.Length - 1; v++)
                    {
                        Vector2 distance = mesh.vertices[v] - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                        mesh.vertices[v] += distance * 0.15f;
                    }
                }
                for (int k = 0; k < bsg.mandibles.Length; k++)
                {
                    Vector2 dist1 = sLeaser.sprites[bsg.MandibleSprite(k, 0)].GetPosition() - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                    sLeaser.sprites[bsg.MandibleSprite(k, 0)].SetPosition(sLeaser.sprites[bsg.MandibleSprite(k, 0)].GetPosition() + (dist1 * 0.15f));
                    if (sLeaser.sprites[bsg.MandibleSprite(k, 1)] is CustomFSprite cfs)
                    {
                        for (int v = 0; v < cfs.vertices.Length - 1; v++)
                        {
                            Vector2 distance = cfs.vertices[v] - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                            cfs.vertices[v] += distance * 0.15f;
                        }
                    }
                }
                for (int l = 0; l < bsg.totalScales; l++)
                {
                    Vector2 distance = sLeaser.sprites[bsg.FirstScaleSprite + l].GetPosition() - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                    sLeaser.sprites[bsg.FirstScaleSprite + l].SetPosition(sLeaser.sprites[bsg.FirstScaleSprite + l].GetPosition() + (distance * 0.15f));
                }
            }
            else if (bsg.bug.Template.type == HailstormEnums.PeachSpider)
            {
                for (int s = 0; s < sLeaser.sprites.Length; s++)
                {
                    if (sLeaser.sprites[s] is TriangleMesh mesh)
                    {
                        for (int v = 0; v < mesh.vertices.Length - 1; v++)
                        {
                            mesh.vertices[v] = Vector2.Lerp(mesh.vertices[v], sLeaser.sprites[bsg.HeadSprite].GetPosition(), 0.1f);
                        }
                    }
                    else if (sLeaser.sprites[s] is CustomFSprite cfs)
                    {
                        float mandibleCharge = Mathf.Lerp(bsg.lastMandiblesCharge, bsg.mandiblesCharge, timeStacker);
                        cfs.verticeColors[2] = bsg.yellowCol;
                        cfs.verticeColors[3] = bsg.yellowCol;
                        cfs.verticeColors[0] = Color.Lerp(bsg.blackColor, bsg.yellowCol, 0.6f * (1f - mandibleCharge) + 0.4f * bsg.darkness);
                        cfs.verticeColors[1] = Color.Lerp(bsg.blackColor, bsg.yellowCol, 0.6f * (1f - mandibleCharge) + 0.4f * bsg.darkness);
                        for (int v = 0; v < cfs.vertices.Length - 1; v++)
                        {
                            cfs.vertices[v] = Vector2.Lerp(cfs.vertices[v], sLeaser.sprites[bsg.HeadSprite].GetPosition(), 0.3f);
                        }
                    }
                    else if (s != bsg.HeadSprite)
                    {
                        sLeaser.sprites[s].x = Mathf.Lerp(sLeaser.sprites[s].x, sLeaser.sprites[bsg.HeadSprite].x, 0.8f);
                        sLeaser.sprites[s].y = Mathf.Lerp(sLeaser.sprites[s].y, sLeaser.sprites[bsg.HeadSprite].y, 0.8f);
                    }
                }
            }
        }
    }

    #endregion


}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Luminescipede stuff

public class Luminescipede : Creature, IPlayerEdible
{
    private int bites = 5;
    public int BitesLeft => bites;
    public int FoodPoints => 1;
    public bool Edible => dead;
    public bool AutomaticPickUp => dead;

    public LuminGraphics graphics;
    public GlowSpiderState GlowState => State as GlowSpiderState;
    public float HP => GlowState.health;
    public float Juice => GlowState.juice;
    public bool WantToHide => GlowState.seenNoPreyCounter == 1000 || GlowState.darknessCounter > 0 || GlowState.state == GlowSpiderState.State.Hide;

    //-----------------------------------------

    public Color baseColor;
    public Color glowColor;
    public Color MainBodyColor
    {
        get
        {
            Color bodyColor = Color.Lerp(Color.Lerp(baseColor, Color.black, 0.5f), glowColor, Juice);
            if (flicker > 0f)
            {
                bodyColor = Color.Lerp(bodyColor, Color.Lerp(baseColor, Color.black, 0.5f), flicker);
            }
            if (GlowState.state == Hide)
            {
                bodyColor = Color.Lerp(Color.Lerp(baseColor, Color.black, 0.5f), bodyColor, flicker);
            }
            return bodyColor;
        }
    }
    public Color OutlineColor
    {
        get
        {
            Color altColor = Color.Lerp(glowColor, baseColor, Juice);
            if (flicker > 0f)
            {
                altColor = Color.Lerp(altColor, glowColor, flicker);
            }
            if (GlowState.state == Hide)
            {
                altColor = Color.Lerp(baseColor * 0.5f, altColor, flicker);
            }
            return altColor;
        }
    }

    public float roomBrightness;
    
    public float lightToMove;
    public float lightExposure;

    public float blinkLoudness;

    //-----------------------------------------

    public int denMovement;

    public bool idle;
    public int idleCounter;

    public Vector2 direction;

    public Vector2 dragPos;
    public WorldCoordinate? denPos;
    public float connectDistance;
    public MovementConnection lastFollowingConnection;
    public MovementConnection followingConnection;
    public MovementConnection lastShortCut;
    private List<MovementConnection> path;
    private List<MovementConnection> scratchPath;
    private int pathCount;
    private int scratchPathCount;
    public bool inAccessibleTerrain;
    public int outsideAccessibleCounter;
    public float movementDesire;

    //-----------------------------------------
    public float MassAttackLimit => TotalMass * 10;
    public Creature currentPrey;
    public int preyVisualCounter;
    public float bloodLust;
    public BodyChunk graphicsAttachedToBodyChunk;
    public int losingInterestInPrey;

    public float fleeRadius;
    public Vector2? fleeFromPos;

    public LuminFlock flock;

    public IndividualVariations iVars;


    public float flickeringFac;
    public float flicker;
    public float flickerDuration;

    public float legsPosition;
    public float deathSpasms = 1f;

    //-----------------------------------------

    public struct IndividualVariations
    {

        public float Size;
        public float Fatness;
        public float Dominance;

        public IndividualVariations(float size, float fatness)
        {
            Size = size;
            Fatness = fatness;
            Dominance = (size + Random.Range(-0.2f, 0.2f)) * Mathf.Max(0, 1f - Mathf.Abs(fatness % 1f));
        }
    }
    public Luminescipede(AbstractCreature absSpd, World world) : base(absSpd, world)
    {
        Random.State state = Random.state;
        Random.InitState(absSpd.ID.RandomSeed);
        iVars = new IndividualVariations(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));
        baseColor = Custom.HSL2RGB((Random.value < 0.04f ? Random.value : Custom.WrappedRandomVariation(260 / 360f, 60 / 360f, 0.5f)), 0.4f, Custom.WrappedRandomVariation(0.5f, 0.125f, 0.3f));
        glowColor = Color.Lerp(baseColor, Color.white, 0.6f);
        Random.state = state;

        float rad = Mathf.Lerp(7.6f, 10.8f, Mathf.InverseLerp(0.8f, 1.2f, iVars.Size));
        float mass = Mathf.Lerp(0.2f, 0.3f, Mathf.InverseLerp(0.8f, 1.2f, iVars.Size));
        for (int i = 5 - bites; i > 0; i--)
        {
            rad *= 0.85f;
            mass *= 0.85f;
        }
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), rad, mass);
        bodyChunkConnections = new BodyChunkConnection[0];
        collisionLayer = 1;
        ChangeCollisionLayer(collisionLayer);
        GoThroughFloors = true;
        gravity = 0.99f;
        bounce = 0.3f;
        surfaceFriction = 0.99f;
        airFriction = 0.99f;
        buoyancy = 0.95f;

        direction = Custom.DegToVec(Random.value * 360f);
        path = new List<MovementConnection>();
        pathCount = 0;
        scratchPath = new List<MovementConnection>();
        scratchPathCount = 0;
        connectDistance = Mathf.Lerp(12f, 24f, iVars.Size);
        lightToMove = Mathf.Lerp(iVars.Size - 0.2f, 1, 0.5f);
        if (absSpd.pos.NodeDefined && world.GetNode(absSpd.pos).type == AbstractRoomNode.Type.Den)
        {
            denPos = absSpd.pos.WashTileData();
        }
        if (world.rainCycle.CycleStartUp < 0.5f)
        {
            denMovement = -1;
        }
        else if (WantToHideInDen(abstractCreature))
        {
            denMovement = 1;
        }
        movementDesire = 1;

    }
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule is null)
        {
            graphicsModule = new LuminGraphics(this);
        }
        graphics = graphicsModule as LuminGraphics;
    }
    public void ResetFlock()
    {
        flock = null;
    }

    //-----------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);
        GlowState.Update(this, eu);

        //-----------------------------------------

        graphicsAttachedToBodyChunk = null;
        dragPos = DangerPos + Custom.DirVec(DangerPos, dragPos) * connectDistance;
        if (outsideAccessibleCounter > 0)
        {
            outsideAccessibleCounter--;
        }
        if (room is null)
        {
            return;
        }

        #region Lumin-specific code

        if (!Consious)
        {
            if (Juice > 0)
            {
                GlowState.juice = Mathf.Max(0, Juice - (0.005f * Mathf.Lerp(1, 0.1f, roomBrightness)));
                if (Random.value < 0.1f)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        room.AddObject(new MouseSpark(DangerPos, Custom.RNV() * Random.Range(3f, 7f), 40f, MainBodyColor));
                    }
                }
            }
            if (dead)
            {
                if (GlowState.state != Idle)
                {
                    GlowState.ChangeBehavior(Idle, true);
                    movementDesire = 0;
                }
                goto CodeSkip;
            }
            else
            {
                flickeringFac = 1f;
                flickerDuration = Mathf.Lerp(10f, 30f, Random.value);
                if (Random.value < 0.1f)
                {
                    flicker = Mathf.Max(flicker, Random.value);
                }
            }
        }

        if (GlowState.state != StubbornState.Overloaded)
        {
            if (Juice > 1.25f)
            {
                Overload();
            }

            if (Juice < 1)
            {
                GlowState.juice = Mathf.Min(1, Juice + (0.0025f * Mathf.Lerp(0.1f, 1, roomBrightness)));
                if (Juice == 1)
                {
                    room.PlaySound(SoundID.Snail_Warning_Click, DangerPos, blinkLoudness, blinkLoudness);
                    blinkLoudness = 0;
                    flickeringFac = (Random.value < 0.5f) ? 0f : 1f;
                    flickerDuration = Mathf.Lerp(30f, 220f, Random.value);
                    room.AddObject(new LuminBlink(DangerPos, DangerPos, default, 1, baseColor, glowColor));
                    for (int i = 0; i < 20; i++)
                    {
                        room.AddObject(new MouseSpark(DangerPos, Custom.RNV() * Random.Range(3f, 7f), 40f, MainBodyColor));
                    }
                }
            }
            else
            {
                if (Juice > 1)
                {
                    GlowState.juice = Mathf.Max(1, Juice - 0.0025f);
                }

            }

        }
        else
        {
            if (Juice > 0)
            {
                GlowState.juice = Mathf.Max(0, Juice - 0.025f);
            }
            if (!Stunned)
            {
                GlowState.ChangeBehavior(StubbornState.Aggravated, true);
                GlowState.stateTimeLimit = Random.Range(640, 961);
                movementDesire = 1.5f;
                bloodLust = 3;
            }
        }

        if (flickeringFac > 0f)
        {
            flickeringFac = Mathf.Max(0, flickeringFac - 1f / flickerDuration);
            if (Random.value < flickeringFac)
            {
                room.AddObject(new MouseSpark(DangerPos, Custom.RNV() * Random.Range(3f, 7f), 40f, MainBodyColor));
            }
            if (Random.value < 1f / 15f && Random.value < flickeringFac)
            {
                flicker = Mathf.Pow(Random.value, 1f - flickeringFac);
                room.PlaySound(SoundID.Mouse_Light_Flicker, DangerPos, flicker, 1f + (0.5f - flicker));
            }
        }
        else if (!dead && Random.value < 0.0033333334f)
        {
            flickeringFac = Random.value;
            flickerDuration = Mathf.Lerp(30f, 120f, Random.value);
        }
        if (flicker > 0f)
        {
            flicker = Mathf.Max(0, flicker - 1/15f);
        }

        if (HP < 0.5f)
        {
            if (Random.value < (1f - GlowState.ClampedHealth) / 50f)
            {
                int stun = (int)(Mathf.Lerp(25, 5, GlowState.ClampedHealth) * Random.Range(0.5f, 1.5f));
                Stun(stun);
            }
        }

        if (Consious && HP > 0 && HP < 1)
        {
            GlowState.health = Mathf.Min(1, HP + 0.00075f);
        }

        if (Random.value < (Mathf.Lerp(0.008f, 0.04f, 1 - HP) * Juice))
        {
            for (int i = 0; i < Random.Range(4, 8); i++)
            {
                room.AddObject(new MouseSpark(DangerPos, Custom.RNV() * Random.Range(3f, 7f), 40f, MainBodyColor));
            }
        }
        #endregion

        CodeSkip:

        #region Normal Spider Stuff
        if (dead)
        {
            deathSpasms = Mathf.Max(0f, deathSpasms - (1 / Mathf.Lerp(200f, 400f, Random.value)));
        }
        IntVector2 tilePosition = room.GetTilePosition(DangerPos);
        tilePosition.x = Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1);
        tilePosition.y = Custom.IntClamp(tilePosition.y, 0, room.TileHeight - 1);
        bool tileAccessible = room.aimap.TileAccessibleToCreature(tilePosition, Template);
        if (Random.value < 0.15f)
        {
            lightExposure = Mathf.Min(1, (LuminLightSourceExposure(DangerPos) + (1 - room.Darkness(DangerPos))) / 2f);
        }
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            firstChunk.vel += Custom.DirVec(DangerPos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
            Stun(12);
        }

        if (!Consious)
        {
            return;
        }

        if (GlowState.state != Hide)
        {
            if (GlowState.rushPreyCounter > 0)
            {
                GlowState.rushPreyCounter = 0;
            }

            if (WantToHide && bloodLust < 0.2f && lightExposure < 0.5f && denMovement == 0 && !fleeFromPos.HasValue)
            {
                if (GlowState.darknessCounter < 400)
                {
                    GlowState.darknessCounter = Mathf.Min(400, GlowState.darknessCounter + (lightExposure == 0 ? 2 : 1));
                }
                else if (GlowState.state != Hide)
                {
                    GlowState.ChangeBehavior(Hide, false);
                    movementDesire = 0;
                }
            }
            else if (lightExposure >= 0.25f * (1 - bloodLust) || denMovement != 0 || fleeFromPos.HasValue)
            {
                if ((Random.value < 0.2f * bloodLust || denMovement != 0 || fleeFromPos.HasValue) && GlowState.darknessCounter > 0)
                {
                    GlowState.darknessCounter--;
                }
            }

        }
        else
        {
            if (lightExposure == 1)
            {
                if (currentPrey is not null)
                {
                    RushPrey();
                }
                else
                {
                    GlowState.ChangeBehavior(Flee, false);
                    GlowState.darknessCounter = 0;
                    fleeFromPos = DangerPos;
                    fleeRadius = 150f;
                    movementDesire = 1;
                }
            }
            else if (GlowState.darknessCounter > 0 && (Random.value < lightExposure / 2f || (fleeFromPos.HasValue && Custom.DistLess(fleeFromPos.Value, DangerPos, 400) && Random.value < Mathf.InverseLerp(400, 0, Custom.Dist(fleeFromPos.Value, DangerPos)))))
            {
                GlowState.darknessCounter--;
            }
            else if (GlowState.darknessCounter < 400 && Random.value < 0.05f)
            {
                GlowState.darknessCounter++;
            }

            if (GlowState.darknessCounter == 0 && Random.value < 0.1f)
            {
                GlowState.ChangeBehavior(Idle, false);
                movementDesire = Random.Range(0.5f, 0.75f);
            }

            if (currentPrey is not null && bloodLust > 0 && ((Custom.DistLess(currentPrey.DangerPos, DangerPos, 150) && VisualContact(currentPrey.DangerPos)) || (fleeFromPos.HasValue && Custom.DistLess(fleeFromPos.Value, firstChunk.pos, 150))))
            {
                GlowState.rushPreyCounter++;
                if ((GlowState.rushPreyCounter > 50 && Random.value < Mathf.InverseLerp(0, 3000, GlowState.rushPreyCounter)) || GlowState.rushPreyCounter == 100 || bloodLust >= 1)
                {
                    RushPrey();
                }
            }
            else if (GlowState.rushPreyCounter > 0)
            {
                GlowState.rushPreyCounter--;
            }
        }

        ConsiderCreature();

        if (GlowState.state == Flee)
        {
            if (currentPrey is not null)
            {
                currentPrey = null;
            }
        }

        if (currentPrey is not null)
        {
            if (currentPrey.room != room || (currentPrey.leechedOut && GlowState.state != StubbornState.ReturnPrey) || currentPrey.TotalMass > MassAttackLimit)
            {
                currentPrey = null;
            }
            else if (VisualContact(currentPrey.firstChunk.pos))
            {
                preyVisualCounter = 0;
            }
            if (GlowState.state != Flee)
            {
                TryToAttach();
            }
            else currentPrey = null;
        }
        if (preyVisualCounter < Mathf.Lerp(100, 600, bloodLust) && currentPrey is not null)
        {
            bloodLust = Mathf.Min(bloodLust + 0.005f, 1);
        }
        else
        {
            bloodLust = Mathf.Max(bloodLust - (GlowState.state == Flee || GlowState.state == Hide? 0.00375f : 0.00125f), 0);
        }
        if (GlowState.state == StubbornState.ReturnPrey && ((currentPrey is null && preyVisualCounter > 40) || preyVisualCounter > 400))
        {
            GlowState.ChangeBehavior(Idle, false);
            movementDesire = 1;
        }
        preyVisualCounter++;

        if ((followingConnection is null || followingConnection.DestTile != tilePosition) && room.GetTile(firstChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
        {
            firstChunk.vel += Custom.IntVector2ToVector2(room.ShorcutEntranceHoleDirection(tilePosition)) * 8f;
        }
        else if (!room.IsPositionInsideBoundries(tilePosition) && tileAccessible)
        {
            outsideAccessibleCounter = 5;
            followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, abstractCreature.pos, room.GetWorldCoordinate(tilePosition), 1);
        }

        if (WantToHideInDen(abstractCreature) && denMovement != 1)
        {
            denMovement = 1;
        }
        if (denMovement != 0)
        {
            if (denMovement == -1 && Random.value < 1f / Mathf.Lerp(1200f, 400f, room.world.rainCycle.CycleStartUp))
            {
                denMovement = 0;
            }
            if (denMovement == 1)
            {
                if (!WantToHideInDen(abstractCreature))
                {
                    denMovement = 0;
                }
                if (denPos.HasValue)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 1; j < 3; j++)
                        {
                            if (room.GetTile(abstractCreature.pos.Tile + Custom.fourDirections[i] * j).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(abstractCreature.pos.Tile + Custom.fourDirections[i] * j).destNode == denPos.Value.abstractNode)
                            {
                                enteringShortCut = abstractCreature.pos.Tile + Custom.fourDirections[i] * j;
                            }
                        }
                    }
                }
            }
            if (denPos.HasValue && denPos.Value.room != room.abstractRoom.index)
            {
                denPos = null;
            }
            if (!denPos.HasValue)
            {
                denMovement = 0;
            }
        }
        if (!denPos.HasValue)
        {
            int num = Random.Range(0, room.abstractRoom.nodes.Length);
            if (room.abstractRoom.nodes[num].type == AbstractRoomNode.Type.Den && room.aimap.CreatureSpecificAImap(Template).GetDistanceToExit(abstractCreature.pos.x, abstractCreature.pos.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(num, Template)) > -1)
            {
                denPos = new WorldCoordinate(room.abstractRoom.index, -1, -1, num);
            }
        }

        if (fleeFromPos.HasValue && (Random.value < 0.0125f || !Custom.DistLess(firstChunk.pos, fleeFromPos.Value, fleeRadius)))
        {
            fleeFromPos = null;
        }

        if (flock is null)
        {
            flock = new LuminFlock(this, room);
        }
        else
        {
            flock.Update(eu);
        }
        inAccessibleTerrain = (followingConnection is null || followingConnection.type != MovementConnection.MovementType.DropToFloor) && (outsideAccessibleCounter > 0 || tileAccessible);
        if (followingConnection is null && !tileAccessible)
        {
            for (int k = 0; k < 4; k++)
            {
                if (room.aimap.TileAccessibleToCreature(tilePosition + Custom.fourDirections[k], Template))
                {
                    firstChunk.vel += Custom.fourDirections[k].ToVector2() * 0.5f;
                    break;
                }
            }
        }
        legsPosition = 0f;
        if (grasps is not null && grasps[0] is not null)
        {
            Attached();
            if (GlowState.state != StubbornState.ReturnPrey)
            {
                return;
            }
        }
        else if (losingInterestInPrey > 0)
        {
            losingInterestInPrey = 0;
        }

        if (Consious && inAccessibleTerrain)
        {
            firstChunk.vel *= 0.7f;
            firstChunk.vel.y += gravity;
            if (GlowState.state != Hide)
            {
                Crawl();
            }
        }
        else
        {
            followingConnection = null;
            if (pathCount > 0)
            {
                pathCount = 0;
            }
        }
        #endregion

    }
    public static bool WantToHideInDen(AbstractCreature absLmn)
    {
        if (absLmn.Room.realizedRoom is null)
        {
            return false;
        }
        if (absLmn.world.game.IsArenaSession && absLmn.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
        {
            return false;
        }
        if (absLmn.state.dead)
        {
            return true;
        }
        if (absLmn.state is HealthState HS && HS.health < 0.25f)
        {
            return true;
        }
        if (CWT.AbsCtrData.TryGetValue(absLmn, out AbsCtrInfo aI))
        {
            if (absLmn.world.rainCycle.TimeUntilRain < (absLmn.world.game.IsStorySession ? 60 : 15) * 40 && !absLmn.nightCreature && !absLmn.ignoreCycle && !aI.LateBlizzardRoamer)
            {
                return true;
            }
            if (absLmn.preCycle && (Weather.FogPrecycle || absLmn.world.rainCycle.maxPreTimer <= 0))
            {
                return true;
            }
            if (aI.FogRoamer && (!Weather.FogPrecycle || absLmn.world.rainCycle.maxPreTimer <= 0))
            {
                return true;
            }
            if (aI.HailstormAvoider && Weather.HailPrecycle && absLmn.world.rainCycle.preTimer > 0)
            {
                return true;
            }
            if (aI.FogAvoider && Weather.FogPrecycle && absLmn.world.rainCycle.preTimer > 0)
            {
                return true;
            }
            if (aI.ErraticWindRoamer && !Weather.ErraticWindCycle)
            {
                return true;
            }
            if (aI.ErraticWindAvoider && Weather.ErraticWindCycle)
            {
                return true;
            }
            if (!aI.ErraticWindAvoider && Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval])
            {
                return true;
            }
        }
        if (absLmn.realizedCreature is not null && absLmn.realizedCreature is Luminescipede lmn && lmn.denMovement == 1)
        {
            return true;
        }
        return false;
    }
    public virtual float LuminLightSourceExposure(Vector2 pos)
    {
        float exp = 0f;
        for (int i = 0; i < room.lightSources.Count; i++)
        {
            if ((room.lightSources[i].tiedToObject is null || room.lightSources[i].tiedToObject is not Luminescipede) && Custom.DistLess(pos, room.lightSources[i].Pos, room.lightSources[i].Rad))
            {
                exp += Custom.SCurve(Mathf.InverseLerp(room.lightSources[i].Rad, 0f, Vector2.Distance(pos, room.lightSources[i].Pos)), 0.5f) * room.lightSources[i].Lightness;
            }
        }
        return Mathf.Clamp(exp, 0, 1);
    }

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            bodyChunks[b].rad *= 0.85f;
            bodyChunks[b].mass *= 0.85f;
        }
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
        return MainBodyColor;
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (firstContact && speed > 15)
        {
            for (int s = 0; s < Mathf.Lerp(0, 20, Juice); s++)
            {
                room.AddObject(new MouseSpark(mainBodyChunk.pos, -new Vector2(mainBodyChunk.vel.x * Random.Range(0.6f, 1.4f), mainBodyChunk.vel.y * Random.Range(0.6f, 1.4f)) + Custom.DegToVec(360f * Random.value) * 7f * Random.value, 40f, MainBodyColor));
            }
        }
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float dmg, float stunBonus)
    {
        if (source?.owner is not null)
        {
            if (source.owner is Rock)
            {
                dmg *= 5;
            }
            if (source.owner is Creature ctr)
            {
                if (Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    GlowState.ChangeBehavior(Flee, GlowState.state != StubbornState.Aggravated && ctr.Template.CreatureRelationship(this).intensity >= 0.5f);
                    movementDesire = 1;
                }
                else if (Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowSpiderState.State reaction = (bloodLust > 1) ?
                        StubbornState.Aggravated : Hunt;
                    GlowState.ChangeBehavior(reaction, false);
                    movementDesire = (reaction == Hunt) ? 1 : 1.5f;
                    if (GlowState.state == StubbornState.Aggravated)
                    {
                        GlowState.stateTimeLimit = (int)Mathf.Lerp(640, 961, Template.CreatureRelationship(ctr).intensity);
                    }
                }
                else if (ctr.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowState.ChangeBehavior(Flee, GlowState.state != StubbornState.Aggravated && ctr.Template.CreatureRelationship(this).intensity >= 0.9f);
                    movementDesire = 1;
                }
            }
            else if (source.owner is Weapon wpn && wpn.thrownBy is not null && wpn.thrownBy is Creature thrower)
            {
                if (Template.CreatureRelationship(thrower).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    GlowState.ChangeBehavior(Flee, thrower.Template.CreatureRelationship(this).intensity >= (GlowState.state == StubbornState.Aggravated ? 0.95f : 0.35f));
                    movementDesire = 1;
                }
                else if (Template.CreatureRelationship(thrower).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowSpiderState.State reaction = (bloodLust > 1) ?
                        StubbornState.Aggravated : Hunt;
                    GlowState.ChangeBehavior(reaction, false);
                    movementDesire = (reaction == Hunt) ? 1 : 1.5f;
                    if (GlowState.state == StubbornState.Aggravated)
                    {
                        GlowState.stateTimeLimit = (int)Mathf.Lerp(480, 721, Template.CreatureRelationship(thrower).intensity);
                    }
                }
                else if (thrower.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowState.ChangeBehavior(Flee, GlowState.state != StubbornState.Aggravated && thrower.Template.CreatureRelationship(this).intensity >= 0.75f);
                    movementDesire = 1;
                }
            }
        }
        if (type == DamageType.Electric || type == HailstormEnums.Heat)
        {
            GlowState.juice += dmg / 4f;
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, dmg, stunBonus);
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (Consious && otherObject is Creature ctr && !ctr.leechedOut)
        {
            if (Template.CreatureRelationship(ctr.Template).type == CreatureTemplate.Relationship.Type.Eats)
            {
                bloodLust += 1 / 20f;
            }
            if (GlowState.state == GlowSpiderState.State.Hide)
            {
                RushPrey();
            }
        }
        base.Collide(otherObject, myChunk, otherChunk);
    }
    public override void Stun(int st)
    {
        blinkLoudness = Mathf.InverseLerp(-40, 40, st);
        base.LoseAllGrasps();
        base.Stun(st);
    }
    public virtual void Overload()
    {
        GlowState.ChangeBehavior(StubbornState.Overloaded, true);
        movementDesire = 0;
        if (room is not null)
        {
            room.AddObject(new LuminFlash(firstChunk, 200, 40, glowColor, 1.5f));
        }
        GlowState.health -= 0.2f;
        GlowState.darknessCounter = 0;
        GlowState.seenNoPreyCounter = 0;
        int stun = (int)Mathf.Lerp(320, 420, Mathf.InverseLerp(1.2f, 0.8f, iVars.Size));
        Stun(stun);
    }

    public virtual void ConsiderCreature()
    {
        if (room.abstractRoom.creatures.Count == 0)
        {
            return;
        }
        AbstractCreature absCtr = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
        if (absCtr.realizedCreature is null || absCtr.slatedForDeletion || absCtr.realizedCreature.inShortcut)
        {
            return;
        }
        Creature ctr = absCtr.realizedCreature;
        if (ConsiderPrey(ctr) && VisualContact(ctr.mainBodyChunk.pos))
        {
            GlowState.ChangeBehavior(Hunt, false);
            movementDesire = Mathf.Lerp(0.5f, 1, Template.CreatureRelationship(ctr).intensity);
            GlowState.seenNoPreyCounter = 0;
            if (bloodLust < 1)
            {
                bloodLust = Mathf.Clamp(bloodLust + Mathf.Lerp(Template.CreatureRelationship(ctr).intensity, 1, 0.1f) / 80f, 0, 1);
            }
            if (ctr == currentPrey)
            {
                preyVisualCounter = 0;
            }
            else if (abstractCreature.realizedCreature.TotalMass < MassAttackLimit && (currentPrey is null || WillingToDitchCurrentPrey(currentPrey)))
            {
                currentPrey = ctr;
                preyVisualCounter = 0;
            }
        }
        else
        {
            if (ctr is Luminescipede otherLmn && Custom.DistLess(DangerPos, otherLmn.DangerPos, Template.visualRadius) && (Custom.DistLess(DangerPos, otherLmn.DangerPos, 300) || VisualContact(otherLmn.DangerPos)) && otherLmn.GlowState.state != StubbornState.ReturnPrey)
            {
                if (otherLmn.bloodLust > bloodLust)
                {
                    bloodLust = Mathf.Lerp(bloodLust, otherLmn.bloodLust, 0.05f);
                }
                if (otherLmn.GlowState.seenNoPreyCounter < GlowState.seenNoPreyCounter)
                {
                    GlowState.seenNoPreyCounter--;
                }
                if (!fleeFromPos.HasValue && otherLmn.fleeFromPos.HasValue && Custom.DistLess(otherLmn.fleeFromPos.Value, DangerPos, 100))
                {
                    fleeFromPos = otherLmn.fleeFromPos.Value;
                    fleeRadius = otherLmn.fleeRadius;
                }
                if (currentPrey is null && otherLmn.currentPrey is not null && ConsiderPrey(otherLmn.currentPrey))
                {
                    currentPrey = otherLmn.currentPrey;
                    GlowState.ChangeBehavior(Hunt, false);
                    movementDesire = 0.8f;
                }
            }
        }
        if (ctr is not Luminescipede)
        {
            if (Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.StayOutOfWay)
            {
                fleeRadius = Template.CreatureRelationship(ctr).intensity * 450;
                if (Custom.DistLess(DangerPos, ctr.DangerPos, fleeRadius))
                {
                    fleeFromPos = ctr.DangerPos;
                }
            }
            else if (Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Afraid)
            {
                fleeRadius = Template.CreatureRelationship(ctr).intensity * 300;
                if (Custom.DistLess(DangerPos, ctr.DangerPos, fleeRadius))
                {
                    fleeFromPos = ctr.DangerPos;
                    GlowState.ChangeBehavior(Flee, Custom.DistLess(DangerPos, ctr.DangerPos, fleeRadius / 2f));
                    movementDesire = Mathf.Lerp(0.75f, 1.25f, Mathf.InverseLerp(fleeRadius, fleeRadius/2f, Custom.Dist(DangerPos, ctr.DangerPos)));
                    if (GlowState.state == Flee && grasps is not null && grasps[0] is not null)
                    {
                        LoseAllGrasps();
                    }
                }
            }
            else if (ctr.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats)
            {
                if ((GlowState.state == StubbornState.Aggravated ||
                    Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Eats ||
                    Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Attacks) &&
                    ConsiderPrey(ctr) && VisualContact(ctr.mainBodyChunk.pos))
                {
                    GlowState.ChangeBehavior(Hunt, false);
                    movementDesire = (GlowState.state == Hunt) ? 1 : 1.5f;
                    GlowState.seenNoPreyCounter = 0;
                    if (bloodLust < 1)
                    {
                        bloodLust = Mathf.Clamp(bloodLust + Mathf.Lerp(Template.CreatureRelationship(ctr).intensity, 1, 0.1f) / 80f, 0, 1);
                    }
                    if (ctr == currentPrey)
                    {
                        preyVisualCounter = 0;
                    }
                    else if (abstractCreature.realizedCreature.TotalMass < MassAttackLimit && (currentPrey is null || WillingToDitchCurrentPrey(currentPrey)))
                    {
                        currentPrey = ctr;
                        preyVisualCounter = 0;
                    }
                }
                else
                {
                    fleeRadius = Template.CreatureRelationship(ctr).intensity * 200;
                    if (Custom.DistLess(DangerPos, ctr.DangerPos, fleeRadius))
                    {
                        fleeFromPos = ctr.DangerPos;
                        GlowState.ChangeBehavior(Flee, Custom.DistLess(DangerPos, ctr.DangerPos, fleeRadius / 2f));
                        movementDesire = Mathf.Lerp(0.5f, 1, Mathf.InverseLerp(fleeRadius, fleeRadius / 2f, Custom.Dist(DangerPos, ctr.DangerPos)));
                        if (GlowState.state == Flee && grasps is not null && grasps[0] is not null)
                        {
                            LoseAllGrasps();
                        }
                    }
                }
            }

        }

    }
    public virtual bool ConsiderPrey(Creature target)
    {
        if (target.TotalMass > MassAttackLimit)
        {
            return false;
        }
        if (Template.CreatureRelationship(target.Template).type != CreatureTemplate.Relationship.Type.Eats && Template.CreatureRelationship(target.Template).type != CreatureTemplate.Relationship.Type.Attacks)
        {
            return false;
        }
        if (target.leechedOut)
        {
            if (target.grabbedBy is not null)
            {
                for (int g = 0; g < target.grabbedBy.Count; g++)
                {
                    if (target.grabbedBy[g]?.grabber?.State is not null && target.grabbedBy[g].grabber != this && target.grabbedBy[g].grabber.State is GlowSpiderState gs && gs.state == StubbornState.ReturnPrey)
                    {
                        if (GlowState.state == StubbornState.ReturnPrey)
                        {
                            GlowState.ChangeBehavior(Idle, false);
                            movementDesire = 1;
                        }
                        return false;
                    }
                }
            }
            GlowState.ChangeBehavior(StubbornState.ReturnPrey, false);
            movementDesire = 1;
        }
        return true;
    }
    public virtual bool VisualContact(Vector2 pos)
    {
        if (!Custom.DistLess(DangerPos, pos, Template.visualRadius))
        {
            return false;
        }
        return room.VisualContact(DangerPos, pos);
    }
    public virtual bool WillingToDitchCurrentPrey(Creature ctr)
    {
        if (!Custom.DistLess(DangerPos, ctr.mainBodyChunk.pos, Vector2.Distance(DangerPos, currentPrey.mainBodyChunk.pos)) || !VisualContact(ctr.mainBodyChunk.pos))
        {
            return false;
        }
        if (GlowState.state == StubbornState.ReturnPrey && Template.CreatureRelationship(ctr).type == CreatureTemplate.Relationship.Type.Eats && Template.CreatureRelationship(ctr).intensity >= Template.CreatureRelationship(currentPrey).intensity * 2f)
        {
            GlowState.ChangeBehavior(Hunt, false);
            movementDesire = 1;
            return true;
        }
        return false;
    }

    public virtual void RushPrey()
    {
        GlowState.ChangeBehavior(Hunt, false);
        GlowState.darknessCounter = 0;
        GlowState.rushPreyCounter = 0;
        bloodLust = 3;
        movementDesire = 2;
        fleeFromPos = null;
    }
    public virtual bool TryToAttach()
    {
        if (currentPrey is not null)
        {
            for (int i = 0; i < currentPrey.bodyChunks.Length; i++)
            {
                if (Random.value < 1f / 30f && grasps is not null && (grasps[0]?.grabbed is null || grasps[0].grabbed != currentPrey) && Custom.DistLess(firstChunk.pos, currentPrey.bodyChunks[i].pos, firstChunk.rad + currentPrey.bodyChunks[i].rad))
                {
                    return Grab(currentPrey, 0, i, Grasp.Shareability.NonExclusive, 0.2f, false, false);
                }
                if (GlowState.state != Hide && GlowState.state != StubbornState.ReturnPrey && grasps[0] is null && Random.value < 0.2f && Custom.DistLess(firstChunk.pos, currentPrey.bodyChunks[i].pos, 60f))
                {
                    firstChunk.vel += Custom.DirVec(DangerPos, currentPrey.bodyChunks[i].pos) * 5f;
                    return false;
                }
            }
        }
        return false;
    }
    public virtual void Attached()
    {
        BodyChunk chunk = graphicsAttachedToBodyChunk = grasps[0].grabbed.bodyChunks[grasps[0].chunkGrabbed];
        if (chunk.owner is not Creature target)
        {
            return;
        }

        float TotalLmnMass = 0f;
        int TotalLmnCount = 0;
        int LmnNum = -1;
        for (int i = 0; i < chunk.owner.grabbedBy.Count; i++)
        {
            Creature grabber = chunk.owner.grabbedBy[i].grabber;
            if (grabber is Luminescipede)
            {
                TotalLmnMass += grabber.TotalMass;
                TotalLmnCount++;
                if (grabber == this)
                {
                    LmnNum = i;
                }
                if (GlowState.state != StubbornState.ReturnPrey && TotalLmnCount == 1 && target.leechedOut)
                {
                    GlowState.ChangeBehavior(StubbornState.ReturnPrey, false);
                    movementDesire = 1;
                }
                if (GlowState.state == StubbornState.ReturnPrey && (TotalLmnCount > 1 || !target.leechedOut))
                {
                    GlowState.ChangeBehavior(Idle, false);
                    movementDesire = 1;
                }
            }
        }

        if (!target.leechedOut)
        {
            if (TotalLmnCount <= 1)
            {
                losingInterestInPrey++;
            }
            else
            {
                if (losingInterestInPrey > 0)
                {
                    losingInterestInPrey = Mathf.Max(0, losingInterestInPrey - (TotalLmnCount - 1));
                }
            }

            if (target.State is not HealthState && target.dead && Random.value < 0.001f)
            {
                target.leechedOut = true;
            }
            else if (target.State is PlayerState ps)
            {
                if (TotalLmnCount >= 3 || TotalLmnMass >= chunk.owner.TotalMass)
                {
                    ps.permanentDamageTracking += Mathf.Lerp(1f / TotalLmnCount, 1, 0.5f) / 400f;
                    if (ps.permanentDamageTracking > 1)
                    {
                        target.Die();
                    }
                }
                if (target.dead && Random.value < ps.permanentDamageTracking / 100f)
                {
                    target.leechedOut = true;
                }
            }
            else if (target.State is HealthState hs)
            {
                if (TotalLmnCount >= 3 || TotalLmnMass >= chunk.owner.TotalMass)
                {
                    hs.health -= Mathf.Lerp(1f / TotalLmnCount, 1, 0.5f) / 400f / target.Template.baseDamageResistance / target.Template.damageRestistances[DamageType.Bite.index, 0];
                }
                if (target.dead && Random.value < -hs.health / 500f)
                {
                    target.leechedOut = true;
                }
            }
        }
        else if (GlowState.state != StubbornState.ReturnPrey)
        {
            losingInterestInPrey += TotalLmnCount - LmnNum + 3;
        }

        Vector2 pushAngle = Custom.DirVec(firstChunk.pos, chunk.pos);
        float chunkDistGap = Vector2.Distance(firstChunk.pos, chunk.pos);
        float chunKRadii = firstChunk.rad + chunk.rad;
        float massFac = firstChunk.mass / (firstChunk.mass + chunk.mass);
        firstChunk.vel += pushAngle * (chunkDistGap - chunKRadii) * (1f - massFac);
        firstChunk.pos += pushAngle * (chunkDistGap - chunKRadii) * (1f - massFac);
        chunk.vel -= pushAngle * (chunkDistGap - chunKRadii) * massFac;
        chunk.pos -= pushAngle * (chunkDistGap - chunKRadii) * massFac;
        

        if (target.enteringShortCut.HasValue || losingInterestInPrey > 800)
        {
            LoseAllGrasps();
            losingInterestInPrey = 0;
        }
    }

    public virtual float ScoreOfPath(List<MovementConnection> testPath, int testPathCount)
    {
        if (testPathCount == 0)
        {
            return float.MinValue;
        }
        float tileScore = TileScore(testPath[testPathCount - 1].DestTile);
        for (int i = 0; i < pathCount; i++)
        {
            if (path[i] == lastFollowingConnection)
            {
                tileScore -= 1000f;
            }
        }
        return tileScore;
    }
    public virtual float TileScore(IntVector2 tile)
    {
        float TileScore = 0f;
        bool returningPrey = GlowState.state == StubbornState.ReturnPrey && grasps is not null && grasps[0] is not null;
        if (fleeFromPos.HasValue)
        {
            TileScore += Vector2.Distance(room.MiddleOfTile(tile), fleeFromPos.Value);
        }
        if ((denMovement != 0 || returningPrey) && denPos.HasValue && denPos.Value.room == room.abstractRoom.index)
        {
            int distanceToExit = room.aimap.CreatureSpecificAImap(Template).GetDistanceToExit(tile.x, tile.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(denPos.Value.abstractNode, Template));
            TileScore -= (distanceToExit == -1) ?
                100f : distanceToExit * denMovement;
            if (returningPrey)
            {
                TileScore -= 10000f;
            }
        }
        if (lightExposure <= lightToMove)
        {
            for (int i = 0; i < 5; i++)
            {
                if (room.GetTile(tile + Custom.fourDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                {
                    return float.MinValue;
                }
            }
            for (int j = 0; j < flock.lumins.Count; j++)
            {
                if (flock.lumins[j].iVars.Dominance > iVars.Dominance && flock.lumins[j].abstractCreature.pos.Tile == tile)
                {
                    TileScore -= 1f;
                }
            }
            TileScore += room.aimap.getAItile(tile).visibility / 800f;
            if (room.aimap.getAItile(tile).narrowSpace)
            {
                TileScore -= 0.01f;
            }
            TileScore -= room.aimap.getAItile(tile).terrainProximity * 0.01f;
            if (lastShortCut is not null)
            {
                TileScore -= 10f / lastShortCut.StartTile.FloatDist(tile);
                TileScore -= 10f / lastShortCut.DestTile.FloatDist(tile);
            }
            if (bloodLust > 0f)
            {
                for (int k = 0; k < 10f * bloodLust; k++)
                {
                    Luminescipede lmn = flock.lumins[Random.Range(0, flock.lumins.Count)];
                    TileScore -=
                        (lmn == this || !Custom.DistLess(firstChunk.pos, lmn.firstChunk.pos, 200f)) ?
                        200f * bloodLust : Vector2.Distance(firstChunk.pos, lmn.firstChunk.pos) * bloodLust;
                }
            }
        }
        else
        {
            TileScore -= lightExposure * 10000f * (1 - lightToMove);
            if (lastShortCut is not null && (Custom.ManhattanDistance(tile, lastShortCut.StartTile) < 2 || Custom.ManhattanDistance(tile, lastShortCut.DestTile) < 2))
            {
                TileScore -= 10000f;
            }
        }
        if (currentPrey is not null && !returningPrey)
        {
            TileScore -= Vector2.Distance(DangerPos, Vector2.Lerp(currentPrey.firstChunk.pos, currentPrey.bodyChunks[currentPrey.bodyChunks.Length - 1].pos, 0.5f)) * bloodLust * 4f;
        }
        return TileScore;
    }
    public virtual int CreateRandomPath(ref List<MovementConnection> pth)
    {
        WorldCoordinate worldCoordinate = abstractCreature.pos;
        if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, Template))
        {
            for (int i = 0; i < 4; i++)
            {
                if (room.aimap.TileAccessibleToCreature(worldCoordinate.Tile + Custom.fourDirections[i], Template) && room.GetTile(worldCoordinate.Tile + Custom.fourDirections[i]).Terrain != Room.Tile.TerrainType.Slope)
                {
                    worldCoordinate.Tile += Custom.fourDirections[i];
                    break;
                }
            }
        }
        if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, Template))
        {
            return 0;
        }
        WorldCoordinate worldCoordinate2 = abstractCreature.pos;
        int num = 0;
        for (int j = 0; j < Random.Range(5, 16); j++)
        {
            AItile aItile = room.aimap.getAItile(worldCoordinate);
            int index = Random.Range(0, aItile.outgoingPaths.Count);
            if (!room.aimap.IsConnectionAllowedForCreature(aItile.outgoingPaths[index], Template) || lastShortCut == aItile.outgoingPaths[index] || !(worldCoordinate2 != aItile.outgoingPaths[index].destinationCoord))
            {
                continue;
            }
            bool flag = true;
            for (int k = 0; k < num; k++)
            {
                if (pth[k].startCoord == aItile.outgoingPaths[index].destinationCoord || pth[k].destinationCoord == aItile.outgoingPaths[index].destinationCoord)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                worldCoordinate2 = worldCoordinate;
                if (pth.Count <= num)
                {
                    pth.Add(aItile.outgoingPaths[index]);
                }
                else
                {
                    pth[num] = aItile.outgoingPaths[index];
                }
                num++;
                worldCoordinate = aItile.outgoingPaths[index].destinationCoord;
            }
        }
        return num;
    }

    public virtual void Crawl()
    {
        if (!room.IsPositionInsideBoundries(room.GetTilePosition(firstChunk.pos)))
        {
            Die();
            return;
        }

        if (lightExposure <= lightToMove && bloodLust == 0 && denMovement == 0 && !fleeFromPos.HasValue)
        {
            idleCounter++;
            if (!idle && idleCounter > 10)
            {
                idle = true;
            }
        }
        else if (Random.value <= 0.15 && (lightExposure > lightToMove || bloodLust > 0 || denMovement != 0 || fleeFromPos.HasValue))
        {
            idleCounter = 0;
            idle = false;
        }
        if (idle)
        {
            if (followingConnection is not null)
            {
                Move(followingConnection);
                if (room.GetTilePosition(mainBodyChunk.pos) == followingConnection.DestTile)
                {
                    followingConnection = null;
                }
            }
            else if (lightExposure <= lightToMove && Random.value < 1f / 12f)
            {
                AItile aItile = room.aimap.getAItile(mainBodyChunk.pos);
                MovementConnection movementConnection = aItile.outgoingPaths[Random.Range(0, aItile.outgoingPaths.Count)];
                if (movementConnection.type != MovementConnection.MovementType.DropToFloor && room.aimap.IsConnectionAllowedForCreature(movementConnection, Template))
                {
                    followingConnection = movementConnection;
                }
            }
            return;
        }

        if (lightExposure > lightToMove || bloodLust > 0 || denMovement != 0 || fleeFromPos.HasValue)
        {
            scratchPathCount = CreateRandomPath(ref scratchPath);
            if (ScoreOfPath(scratchPath, scratchPathCount) > ScoreOfPath(path, pathCount))
            {
                List<MovementConnection> oldPath = path;
                int oldPathCount = pathCount;
                path = scratchPath;
                pathCount = scratchPathCount;
                scratchPath = oldPath;
                scratchPathCount = oldPathCount;
            }
        }
        if (followingConnection is not null && followingConnection.type != 0)
        {
            if (lastFollowingConnection != followingConnection)
            {
                outsideAccessibleCounter = 20;
            }
            if (followingConnection is not null)
            {
                lastFollowingConnection = followingConnection;
            }
            Move(followingConnection);
            if (room.GetTilePosition(mainBodyChunk.pos) != followingConnection.DestTile)
            {
                return;
            }
        }
        else if (followingConnection is not null)
        {
            lastFollowingConnection = followingConnection;
        }
        if (pathCount > 0)
        {
            followingConnection = null;
            for (int p = pathCount - 1; p >= 0; p--)
            {
                if (abstractCreature.pos.Tile == path[p].StartTile)
                {
                    followingConnection = path[p];
                    break;
                }
            }
            if (followingConnection is null)
            {
                pathCount = 0;
            }
        }
        if (followingConnection is null)
        {
            return;
        }
        if (followingConnection.type == MovementConnection.MovementType.Standard || followingConnection.type == MovementConnection.MovementType.DropToFloor)
        {
            Move(followingConnection);
        }
        else if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
        {
            enteringShortCut = followingConnection.StartTile;
            if (safariControlled)
            {
                bool enteringShortcut = false;
                List<IntVector2> npcPipeExits = new List<IntVector2>();
                ShortcutData[] shortcuts = room.shortcuts;
                for (int i = 0; i < shortcuts.Length; i++)
                {
                    ShortcutData shortcutData = shortcuts[i];
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
                    {
                        npcPipeExits.Add(shortcutData.StartTile);
                    }
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
                    {
                        enteringShortcut = true;
                    }
                }
                if (enteringShortcut)
                {
                    if (npcPipeExits.Count > 0)
                    {
                        npcPipeExits.Shuffle();
                        NPCTransportationDestination = room.GetWorldCoordinate(npcPipeExits[0]);
                    }
                    else
                    {
                        NPCTransportationDestination = followingConnection.destinationCoord;
                    }
                }
            }
            else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                NPCTransportationDestination = followingConnection.destinationCoord;
            }
            lastShortCut = followingConnection;
            followingConnection = null;
        }
        return;
    }
    public virtual void Move(MovementConnection con)
    {
        Move(room.MiddleOfTile(con.DestTile));
    }
    public virtual void Move(Vector2 dest)
    {
        Vector2 addedVel = (Custom.DirVec(firstChunk.pos, dest) * 2f) + (Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.8f, 1.2f, iVars.Size) * Mathf.Lerp(1.5f, 3, HP));
        if (GlowState.darknessCounter > 0)
        {
            addedVel *= Mathf.Lerp(400, 0, GlowState.darknessCounter);
        }
        firstChunk.vel += addedVel * movementDesire;
    }
    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        shortcutDelay = 20;
        Vector2 pipeDirection = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        firstChunk.HardSetPosition(newRoom.MiddleOfTile(pos) - pipeDirection * 5f);
        firstChunk.vel = pipeDirection * 5f;
        if (graphicsModule is not null)
        {
            graphicsModule.Reset();
        }
    }


    public abstract class LuminMass 
    {
        public List<Luminescipede> lumins;
        public bool lastEu;
        public Room room;
        public Color color = Custom.HSL2RGB(Random.value, 1f, 0.5f);

        public virtual Luminescipede FirstLumin
        {
            get
            {
                if (lumins.Count == 0)
                {
                    return null;
                }
                return lumins[0];
            }
        }

        public LuminMass(Luminescipede origLmn, Room room)
        {
            this.room = room;
            lumins = new List<Luminescipede> { origLmn };
        }

        public virtual void Update(bool eu)
        {
            for (int num = lumins.Count - 1; num >= 0; num--)
            {
                if (lumins[num].dead || lumins[num].room != room)
                {
                    AbandonSpider(num);
                }
            }
        }

        public bool ShouldIUpdate(bool eu)
        {
            if (eu == lastEu)
            {
                return false;
            }
            lastEu = eu;
            return true;
        }

        public void AddSpider(Luminescipede lmn)
        {
            if (lumins.IndexOf(lmn) == -1)
            {
                lumins.Add(lmn);
            }
            if (this is LuminFlock)
            {
                lmn.flock = this as LuminFlock;
            }
        }
        public void AbandonSpider(Luminescipede lmn)
        {
            for (int i = 0; i < lumins.Count; i++)
            {
                if (lumins[i] == lmn)
                {
                    AbandonSpider(i);
                    break;
                }
            }
        }
        private void AbandonSpider(int i)
        {
            if (this is LuminFlock && lumins[i].flock == this as LuminFlock)
            {
                lumins[i].flock = null;
            }
            lumins.RemoveAt(i);
        }
        public void Merge(LuminMass otherFlock)
        {
            if (otherFlock == this)
            {
                return;
            }
            for (int i = 0; i < otherFlock.lumins.Count; i++)
            {
                if (lumins.IndexOf(otherFlock.lumins[i]) == -1)
                {
                    lumins.Add(otherFlock.lumins[i]);
                    if (this is LuminFlock)
                    {
                        otherFlock.lumins[i].flock = this as LuminFlock;
                    }
                }
            }
            otherFlock.lumins.Clear();
        }
    }
    public class LuminFlock : LuminMass
    {
        public LuminFlock(Luminescipede origLmn, Room room) : base(origLmn, room)
        {
        }

        public override void Update(bool eu)
        {
            if (!ShouldIUpdate(eu))
            {
                return;
            }
            base.Update(eu);
            if (room.abstractRoom.creatures.Count == 0)
            {
                return;
            }
            AbstractCreature absCtr = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
            if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Luminescipede lmn && lmn.flock is not null && lmn.flock != this && lmn.flock.FirstLumin is not null)
            {
                if (lumins.Count >= lmn.flock.lumins.Count)
                {
                    Merge(lmn.flock);
                }
                else
                {
                    lmn.flock.Merge(this);
                }
            }
        }
    }

}

//-----------------------------------------

public class LuminGraphics : GraphicsModule
{
    public Luminescipede lmn => owner as Luminescipede;
    public Luminescipede behindMeLumin;

    public Vector2 bodyDir;
    private Vector2 lastBodyDir;

    private Limb[,] limbs;
    private bool legsPosition;
    private bool lastLegsPosition;
    public float[,] limbGoalDistances;
    private Vector2[,] deathLegPositions;

    public float walkCycle;

    public bool blackedOut;

    public static float[,] legSpriteSizes = new float[4, 2]
    {
        { 19f, 20f },
        { 26f, 20f },
        { 21f, 23f },
        { 26f, 17f }
    };
    public static float[,] limbLengths = new float[4, 2]
    {
        { 0.85f, 0.5f },
        { 1.00f, 0.6f },
        { 0.95f, 0.5f },
        { 0.9f, 0.65f }
    };
    private float limbLength;

    //-----------------------------------------

    public int OutlinesStart => 0;
    private int HeadSprite => 4;
    private int BodySpritesStart => 5;
    private int LegSpritesStart => 9;
    private int DecalSprite => 25;
    private int TotalSprites => 26;

    private float Radius(float bodyPos)
    {
        return 2f + Mathf.Sin(bodyPos * Mathf.PI);
    }
    private int LimbSprite(int limb, int side, int segment)
    {
        return LegSpritesStart + limb + segment * 4 + side * 8;
    }

    public float GlowSize => 200 * lmn.Juice * lmn.iVars.Size;
    public LightSource light;
    public ChunkDynamicSoundLoop lightNoise;

    //-----------------------------------------

    public LuminGraphics(PhysicalObject ow) : base(ow, internalContainers: false)
    {
        bodyDir = Custom.DegToVec(Random.value * 360f);
        limbs = new Limb[4, 2];
        limbGoalDistances = new float[4, 2];
        deathLegPositions = new Vector2[4, 2];
        limbLength = Custom.LerpMap(lmn.iVars.Size, 0.8f, 1.2f, 30f, 40f);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                deathLegPositions[i, j] = Custom.DegToVec(Random.value * 360f);
                limbs[i, j] = new Limb(this, lmn.firstChunk, i + j * 4, 1f, 0.5f, 0.98f, 15f, 0.95f);
                limbs[i, j].mode = Limb.Mode.Dangle;
                limbs[i, j].pushOutOfTerrain = false;
            }
        }
        legsPosition = Random.value < 0.5f;
    }
    public override void Reset()
    {
        base.Reset();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                limbs[i, j].Reset(lmn.firstChunk.pos);
            }
        }
        lightNoise = new ChunkDynamicSoundLoop(lmn.firstChunk);
    }

    //-----------------------------------------

    public override void Update()
    {
        if (lmn.roomBrightness != 1 - lmn.room.Darkness(lmn.firstChunk.pos))
        {
            lmn.roomBrightness = 1 - lmn.room.Darkness(lmn.firstChunk.pos);
        }

        if (lightNoise is null)
        {
            Reset();
        }
        lightNoise.Update();
        if (lmn.Juice >= 1)
        {
            lightNoise.sound = SoundID.Mouse_Light_On_LOOP;
            lightNoise.Volume = 1f - (0.6f * lmn.flicker);
            lightNoise.Pitch = 1f - (0.3f * Mathf.Pow(lmn.flicker, 0.6f));
        }
        else if (lmn.GlowState.state != StubbornState.Overloaded && lmn.Consious)
        {
            lightNoise.sound = SoundID.Mouse_Charge_LOOP;
            lightNoise.Volume = 0.25f + lmn.Juice / 2f;
            lightNoise.Pitch = Custom.LerpMap(lmn.Juice, 0, 0.8f, 0.33f, 1);
        }
        else
        {
            lightNoise.sound = SoundID.None;
            lightNoise.Volume = 0f;
        }

        if (lmn.Juice >= 0.05f && light is null)
        {
            light = new LightSource(lmn.firstChunk.pos, false, lmn.MainBodyColor, lmn)
            {
                affectedByPaletteDarkness = 0,
                submersible = true,
                requireUpKeep = true
            };
            lmn.room.AddObject(light);
        }
        else if (light is not null)
        {
            light.stayAlive = true;
            light.setPos = new Vector2?(lmn.firstChunk.pos);
            light.setRad = new float?(GlowSize * (1 - lmn.flicker));
            light.setAlpha = new float?(lmn.Juice * (1 - (lmn.flicker * 0.4f)));
            light.color = lmn.MainBodyColor;
            if (lmn.Juice < 0.05f || light.slatedForDeletetion || light.room != lmn.room)
            {
                light = null;
            }
        }

        if (lmn.room is not null && lmn.flicker > 0 && Random.value < lmn.flicker/3f)
        {
            lmn.room.AddObject(new MouseSpark(lmn.DangerPos, Custom.RNV() * Random.Range(3f, 7f), 40f, lmn.MainBodyColor));
        }

        base.Update();

        lastBodyDir = bodyDir;
        if (lmn.graphicsAttachedToBodyChunk is not null)
        {
            bodyDir = Custom.DirVec(lmn.firstChunk.pos, lmn.graphicsAttachedToBodyChunk.pos);
        }
        else
        {
            bodyDir -= Custom.DirVec(lmn.firstChunk.pos, lmn.dragPos);
            bodyDir += lmn.mainBodyChunk.vel * 0.2f;
            if (!lmn.Consious)
            {
                bodyDir += Custom.DegToVec(Random.value * 360f) * lmn.deathSpasms;
            }
            bodyDir = bodyDir.normalized;
        }
        float magnitude = lmn.firstChunk.vel.magnitude;
        if (magnitude > 1f)
        {
            walkCycle += Mathf.Max(0f, (magnitude - 1f) / 30f);
            if (walkCycle > 1f)
            {
                walkCycle -= 1f;
            }
        }
        lastLegsPosition = legsPosition;
        legsPosition = walkCycle > 0.5f;
        Vector2 val = Custom.PerpendicularVector(bodyDir);
        for (int limb = 0; limb < 4; limb++)
        {
            for (int side = 0; side < 2; side++)
            {
                Vector2 bodyAngle = bodyDir;
                if (behindMeLumin is not null && behindMeLumin.graphicsModule is not null)
                {
                    bodyAngle = Vector3.Slerp(bodyAngle, (behindMeLumin.graphicsModule as LuminGraphics).bodyDir, 0.2f);
                }
                if (lmn.graphicsAttachedToBodyChunk is not null && lmn.graphicsAttachedToBodyChunk.owner is Spider && lmn.graphicsAttachedToBodyChunk.owner.graphicsModule is not null)
                {
                    bodyAngle = Vector3.Slerp(bodyAngle, (lmn.graphicsAttachedToBodyChunk.owner.graphicsModule as LuminGraphics).bodyDir, 0.2f);
                }
                bool legOnAltSide = limb % 2 == side == legsPosition;
                bodyAngle = Custom.DegToVec(Custom.VecToDeg(bodyAngle) + Mathf.Lerp(Mathf.Lerp(30f, 140f, limb * (1 / 3f)) + 20f * lmn.legsPosition + 35f * (legOnAltSide ? -1f : 1f) * Mathf.InverseLerp(0.5f, 5f, magnitude), 180f * (0.5f + lmn.legsPosition / 2f), Mathf.Abs(lmn.legsPosition) * 0.3f) * (-1 + 2 * side));
                float limbLength = limbLengths[limb, 0] * this.limbLength;
                Vector2 limbPosGoal = lmn.firstChunk.pos + bodyAngle * limbLength * 0.85f + lmn.firstChunk.vel.normalized * limbLength * 0.4f * Mathf.InverseLerp(0.5f, 5f, magnitude);
                if (limb == 0 && !lmn.dead && (!lmn.idle || lmn.lightExposure <= lmn.lightToMove))
                {
                    limbs[limb, side].pos += Custom.DegToVec(Random.value * 360f) * Random.value;
                }
                bool noIdeaWhatThisIs = false;
                if (lmn.Consious)
                {
                    limbs[limb, side].mode = Limb.Mode.HuntAbsolutePosition;
                    if ((lmn.followingConnection is not null && lmn.followingConnection.type == MovementConnection.MovementType.DropToFloor) || !lmn.inAccessibleTerrain)
                    {
                        noIdeaWhatThisIs = true;
                        limbs[limb, side].mode = Limb.Mode.Dangle;
                        Limb limb2 = limbs[limb, side];
                        limb2.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 3f;
                    }
                    else if (limb == 0 && lmn.graphicsAttachedToBodyChunk is not null)
                    {
                        noIdeaWhatThisIs = true;
                        limbs[limb, side].absoluteHuntPos = lmn.graphicsAttachedToBodyChunk.pos + val * (float)(-1 + 2 * side) * lmn.graphicsAttachedToBodyChunk.rad * 0.5f;
                        limbs[limb, side].pos = limbs[limb, side].absoluteHuntPos;
                    }
                    else if (limb == 3 && behindMeLumin is not null)
                    {
                        noIdeaWhatThisIs = true;
                        limbs[limb, side].absoluteHuntPos = behindMeLumin.firstChunk.pos + val * (float)(-1 + 2 * side) * behindMeLumin.firstChunk.rad * -0.5f;
                        limbs[limb, side].pos = limbs[limb, side].absoluteHuntPos;
                    }
                }
                else
                {
                    limbs[limb, side].mode = Limb.Mode.Dangle;
                }
                if (limbs[limb, side].mode == Limb.Mode.HuntAbsolutePosition)
                {
                    if (!noIdeaWhatThisIs)
                    {
                        if (magnitude < 1f)
                        {
                            if (Random.value < 0.05f && !Custom.DistLess(limbs[limb, side].pos, limbPosGoal, limbLength / 6f))
                            {
                                FindGrip(limb, side, limbPosGoal, limbLength, magnitude);
                            }
                        }
                        else if (legOnAltSide && (lastLegsPosition != legsPosition || limb == 3) && !Custom.DistLess(limbs[limb, side].pos, limbPosGoal, limbLength * 0.5f))
                        {
                            FindGrip(limb, side, limbPosGoal, limbLength, magnitude);
                        }
                    }
                }
                else
                {
                    limbs[limb, side].vel += Custom.RotateAroundOrigo(deathLegPositions[limb, side], Custom.AimFromOneVectorToAnother(-bodyDir, bodyDir)) * 0.65f;
                    limbs[limb, side].vel += Custom.DegToVec(Random.value * 360f) * lmn.deathSpasms * 5f;
                    limbs[limb, side].vel += bodyAngle * 0.7f;
                    limbs[limb, side].vel.y -= 0.8f;
                    limbGoalDistances[limb, side] = 0f;
                }
                limbs[limb, side].huntSpeed = 15f * Mathf.InverseLerp(-0.05f, 2f, magnitude);
                limbs[limb, side].Update();
                limbs[limb, side].ConnectToPoint(lmn.firstChunk.pos, limbLength, push: false, 0f, lmn.firstChunk.vel, 1f, 0.5f);
            }
        }
        if (lmn.graphicsAttachedToBodyChunk is not null && lmn.graphicsAttachedToBodyChunk.owner is Luminescipede otherLmn && lmn.graphicsAttachedToBodyChunk.owner.graphicsModule is not null)
        {
            (otherLmn.graphicsModule as LuminGraphics).behindMeLumin = lmn;
        }
        behindMeLumin = null;
    }

    private void FindGrip(int l, int s, Vector2 idealPos, float rad, float moveSpeed)
    {
        if (lmn.room.GetTile(idealPos).wallbehind)
        {
            limbs[l, s].absoluteHuntPos = idealPos;
        }
        else
        {
            limbs[l, s].FindGrip(lmn.room, lmn.firstChunk.pos, idealPos, rad, idealPos + bodyDir * Mathf.Lerp(moveSpeed * 2f, rad / 2f, 0.5f), 2, 2, behindWalls: true);
        }
        limbGoalDistances[l, s] = Vector2.Distance(limbs[l, s].pos, limbs[l, s].absoluteHuntPos);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[TotalSprites];
        float bodyScale = lmn.iVars.Size - 0.1f;
        float bodyWidth = lmn.iVars.Fatness;
        for (int b = OutlinesStart; b < HeadSprite; b++)
        {
            sLeaser.sprites[b] = new FSprite("LuminescipedeBody" + (b + 1 - OutlinesStart));
            sLeaser.sprites[b].scale = bodyScale + 0.2f;
            sLeaser.sprites[b].scaleY = bodyWidth;
        }

        sLeaser.sprites[HeadSprite] = new FSprite("LuminescipedeHead");
        sLeaser.sprites[HeadSprite].scale = bodyScale;

        for (int b = BodySpritesStart; b < LegSpritesStart; b++)
        {
            sLeaser.sprites[b] = new FSprite("LuminescipedeBody" + (b + 1 - BodySpritesStart));
            sLeaser.sprites[b].scale = bodyScale;
            sLeaser.sprites[b].scaleY = bodyWidth;
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[LimbSprite(i, j, 0)] = new FSprite("SpiderLeg" + i + "A");
                sLeaser.sprites[LimbSprite(i, j, 0)].anchorY = 1f / legSpriteSizes[i, 0];
                sLeaser.sprites[LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1.25f : -1.25f) * lmn.iVars.Size;
                sLeaser.sprites[LimbSprite(i, j, 0)].scaleY = limbLengths[i, 0] * limbLengths[i, 1] * limbLength / legSpriteSizes[i, 0];
                sLeaser.sprites[LimbSprite(i, j, 1)] = new FSprite("SpiderLeg" + i + "B");
                sLeaser.sprites[LimbSprite(i, j, 1)].anchorY = 1f / legSpriteSizes[i, 1];
                sLeaser.sprites[LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1.25f : -1.25f) * lmn.iVars.Size;
            }
        }
        sLeaser.sprites[DecalSprite] = new FSprite("LuminescipedeDecal");

        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!rCam.PositionCurrentlyVisible(lmn.firstChunk.pos, 32f, true))
        {
            if (sLeaser.sprites[0].isVisible)
            {
                for (int j = 0; j < sLeaser.sprites.Length; j++)
                {
                    sLeaser.sprites[j].isVisible = false;
                }
            }
            return;
        }

        Vector2 bodyPos = Vector2.Lerp(lmn.firstChunk.lastPos, lmn.firstChunk.pos, timeStacker);
        Vector2 bodyAngle = Vector3.Slerp(lastBodyDir, bodyDir, timeStacker);
        Vector2 perpToBodyDir = -Custom.PerpendicularVector(bodyAngle);

        for (int p = 0; p < LegSpritesStart; p++)
        {
            sLeaser.sprites[p].x = bodyPos.x - camPos.x;
            sLeaser.sprites[p].y = bodyPos.y - camPos.y;
            sLeaser.sprites[p].rotation = Custom.AimFromOneVectorToAnother(-bodyAngle, bodyAngle);
        }
        sLeaser.sprites[DecalSprite].x = bodyPos.x - camPos.x;
        sLeaser.sprites[DecalSprite].y = bodyPos.y - camPos.y;
        sLeaser.sprites[DecalSprite].rotation = Custom.AimFromOneVectorToAnother(-bodyAngle, bodyAngle);

        for (int limb = 0; limb < 4; limb++)
        {
            for (int side = 0; side < 2; side++)
            {
                Vector2 bodyPos2 = bodyPos;
                bodyPos2 += bodyAngle * (7f - (limb * 0.5f) - (limb == 3 ? 1.5f : 0f)) * lmn.iVars.Size;
                bodyPos2 += perpToBodyDir * (3f + (limb * 0.5f) - (limb == 3 ? 5.5f : 0f)) * (-1 + 2 * side) * lmn.iVars.Size;
                Vector2 limbPos = Vector2.Lerp(limbs[limb, side].lastPos, limbs[limb, side].pos, timeStacker);
                limbPos = Vector2.Lerp(limbPos, bodyPos2 + bodyAngle * limbLength * 0.1f, Mathf.Sin(Mathf.InverseLerp(0f, limbGoalDistances[limb, side], Vector2.Distance(limbPos, limbs[limb, side].absoluteHuntPos)) * Mathf.PI) * 0.4f);
                float num = limbLengths[limb, 0] * limbLengths[limb, 1] * limbLength;
                float num2 = limbLengths[limb, 0] * (1f - limbLengths[limb, 1]) * limbLength;
                float num3 = Vector2.Distance(bodyPos2, limbPos);
                float num4 = ((limb < 3) ? 1f : (-1f));
                if (limb == 2)
                {
                    num4 *= 0.7f;
                }
                if (lmn.legsPosition != 0f)
                {
                    num4 = 1f - 2f * Mathf.Pow(0.5f + 0.5f * lmn.legsPosition, 0.65f);
                }
                num4 *= -1 + (2 * side);
                float num5 = Mathf.Acos(Mathf.Clamp((num3 * num3 + num * num - num2 * num2) / (2f * num3 * num), 0.2f, 0.98f)) * (180f / Mathf.PI) * num4;
                Vector2 bodyPos3 = bodyPos2 + Custom.DegToVec(Custom.AimFromOneVectorToAnother(bodyPos2, limbPos) + num5) * num;
                sLeaser.sprites[LimbSprite(limb, side, 0)].x = bodyPos2.x - camPos.x;
                sLeaser.sprites[LimbSprite(limb, side, 0)].y = bodyPos2.y - camPos.y;
                sLeaser.sprites[LimbSprite(limb, side, 1)].x = bodyPos3.x - camPos.x;
                sLeaser.sprites[LimbSprite(limb, side, 1)].y = bodyPos3.y - camPos.y;
                sLeaser.sprites[LimbSprite(limb, side, 0)].rotation = Custom.AimFromOneVectorToAnother(bodyPos2, bodyPos3);
                sLeaser.sprites[LimbSprite(limb, side, 1)].rotation = Custom.AimFromOneVectorToAnother(bodyPos3, limbPos);
                sLeaser.sprites[LimbSprite(limb, side, 1)].scaleY = Vector2.Distance(bodyPos3, limbPos);
                sLeaser.sprites[LimbSprite(limb, side, 1)].scaleY = limbLengths[limb, 0] * limbLengths[limb, 1] * limbLength / legSpriteSizes[limb, 1];
            }
        }

        for (int s = 0; s < TotalSprites; s++)
        {
            sLeaser.sprites[s].color = lmn.MainBodyColor;
        }
        sLeaser.sprites[DecalSprite].color = lmn.OutlineColor;
        for (int s = OutlinesStart; s < HeadSprite; s++)
        {
            sLeaser.sprites[s].color = lmn.OutlineColor;
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[HeadSprite].isVisible = lmn.BitesLeft > 0;
        sLeaser.sprites[DecalSprite].isVisible = lmn.BitesLeft > 0;
        if (lmn.BitesLeft > 1 || sLeaser.sprites[OutlinesStart].isVisible)
        {
            sLeaser.sprites[OutlinesStart].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[BodySpritesStart].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[9].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[10].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[13].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[14].isVisible = lmn.BitesLeft > 1;
            if (lmn.BitesLeft > 2 || sLeaser.sprites[OutlinesStart + 1].isVisible)
            {
                sLeaser.sprites[OutlinesStart + 1].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[BodySpritesStart + 1].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[11].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[12].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[15].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[16].isVisible = lmn.BitesLeft > 2;
                if (lmn.BitesLeft > 3 || sLeaser.sprites[OutlinesStart + 2].isVisible)
                {
                    sLeaser.sprites[OutlinesStart + 2].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[BodySpritesStart + 2].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[17].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[18].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[21].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[22].isVisible = lmn.BitesLeft > 3;
                    if (lmn.BitesLeft > 4 || sLeaser.sprites[OutlinesStart + 3].isVisible)
                    {
                        sLeaser.sprites[OutlinesStart + 3].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[BodySpritesStart + 3].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[19].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[20].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[23].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[24].isVisible = lmn.BitesLeft > 4;
                    }
                }
            }
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner is null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

}

//-----------------------------------------

public class GlowSpiderState : HealthState
{
    public float MaxJuice => 1.5f;
    public float juice;

    public int seenNoPreyCounter;
    public int darknessCounter;
    public int rushPreyCounter;

    public State state;
    public State suppressedState;
    public int stateTimeLimit;
    public class State : ExtEnum<State>
    {
        public static readonly State Idle = new("Idle", true);
        public static readonly State Hunt = new("Hunt", true);
        public static readonly State Hide = new("Hide", true);
        public static readonly State Flee = new("Flee", true);
        public static readonly State AboutToRush = new("AboutToRush", true);
        public class StubbornState : State
        {
            public static readonly StubbornState Aggravated = new("Aggravated", true);
            public static readonly StubbornState ReturnPrey = new("ReturnPrey", true);
            public static readonly StubbornState Overloaded = new("Overloaded", true);
            public StubbornState(string value, bool register = false) : base(value, register)
            {

            }
        }
        public State(string value, bool register = false) : base(value, register)
        {
        
        }
    }

    public GlowSpiderState(AbstractCreature absSpd) : base (absSpd)
    {
        juice = 1;
        state = Idle;
        stateTimeLimit = -1;
    }

    public virtual void Update(Luminescipede lmn, bool eu)
    {

        if (stateTimeLimit > -1)
        {
            stateTimeLimit--;
            if (stateTimeLimit == 0)
            {
                State newState = Idle;
                if (suppressedState is not null)
                {
                    newState = suppressedState;
                }
                else if (state == StubbornState.Aggravated)
                {
                    newState = Hunt;
                }
                ChangeBehavior(newState, true);
            }
        }

        if (!lmn.fleeFromPos.HasValue && lmn.currentPrey is null && (state == Hunt || state == Flee))
        {
            ChangeBehavior(Idle, false);
        }

    }

    public virtual void ChangeBehavior(State newState, bool forceChange)
    {
        if (newState == state)
        {
            return;
        }
        if (state is StubbornState && !forceChange)
        {
            suppressedState = newState;
            return;
        }
        state = newState;
        if (stateTimeLimit > -1)
        {
            stateTimeLimit = -1;
        }
    }

    public override string ToString()
    {
        string text = HealthBaseSaveString() + ((juice < 1) ? string.Format(CultureInfo.InvariantCulture, "<cB>Juice<cC>{0}", juice) : "");
        foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
        {
            text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
        }
        return text;
    }

    public override void LoadFromString(string[] s)
    {
        base.LoadFromString(s);
        for (int i = 0; i < s.Length; i++)
        {
            string text = Regex.Split(s[i], "<cC>")[0];
            if (text is not null && text == "Juice")
            {
                juice = float.Parse(Regex.Split(s[i], "<cC>")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }
        unrecognizedSaveStrings.Remove("Juice");
    }

}

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

public class LuminBlink : CosmeticSprite
{

    private float rad;

    private float lastRad;

    private float radVel;

    private float initRad;

    private float lifeTime;

    private float lastLife;

    private float life;

    private float intensity;

    private Vector2 aimPos;

    private Color color1;
    private Color color2;
    private Color blackCol;

    public LuminBlink(Vector2 startPos, Vector2 aimPos, Vector2 startVel, float intensity, Color color1, Color color2)
    {
        pos = startPos;
        lastPos = pos;
        vel = startVel;
        this.intensity = intensity;
        this.aimPos = aimPos;
        this.color1 = color1;
        this.color2 = color2;
        radVel = Mathf.Lerp(1.4f, 4.2f, intensity);
        initRad = Mathf.Lerp(8f, 12f, intensity);
        rad = initRad;
        lastRad = initRad;
        life = 1f;
        lastLife = 0f;
        lifeTime = Mathf.Lerp(6f, 30f, Mathf.Pow(intensity, 4f));
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRad = rad;
        rad += radVel;
        radVel *= 0.92f;
        radVel -= Mathf.InverseLerp(0.6f + 0.3f * intensity, 0f, life) * Mathf.Lerp(0.2f, 0.6f, intensity);
        Vector2 val = pos + Custom.DirVec(pos, aimPos) * 80f * Mathf.Sin(life * Mathf.PI);
        pos = Vector2.Lerp(pos, val, 0.3f * (1f - Mathf.Sin(life * Mathf.PI)));
        lastLife = life;
        life = Mathf.Max(0f, life - 1f / lifeTime);
        if (lastLife <= 0f && life <= 0f)
        {
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White");
        sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        float num = Mathf.Lerp(lastLife, life, timeStacker);
        float num2 = Mathf.InverseLerp(0f, 0.75f, num);
        sLeaser.sprites[0].color = Color.Lerp((num2 > 0.5f) ? color2 : blackCol, Color.Lerp(color2, color1, 0.5f + 0.5f * intensity), Mathf.Sin(num2 * Mathf.PI));
        float num3 = Mathf.Lerp(lastRad, rad, timeStacker);
        sLeaser.sprites[0].scale = num3 / 8f;
        sLeaser.sprites[0].alpha = Mathf.Sin(Mathf.Pow(num, 2f) * Mathf.PI) * 2f / num3;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        blackCol = palette.blackColor;
    }
}

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

public class LuminFlash : CosmeticSprite
{
    private readonly BodyChunk followChunk;
    private readonly AbstractCreature killTag;
    private LightSource light;

    private float life;
    private float lastLife;
    private int lifeTime;

    private Color color;

    private Vector2 lastDirection;
    private Vector2 direction;
    private float baseRad;
    private float lastRad;
    private float rad;
    private float lastAlpha;
    private float alpha;
    public float LightIntensity => Mathf.Pow(Mathf.Sin(lifeTime * Mathf.PI), 0.4f);

    public LuminFlash(BodyChunk source, float baseRad, int lifeTime, Color color, float flashPitch) : this(new Vector2(0, 0), baseRad, lifeTime, color, flashPitch)
    {
        followChunk = source;
        if (followChunk?.owner is not null && followChunk.owner is Creature ctr)
        {
            killTag = ctr.abstractCreature;
        }
    }
    public LuminFlash(Vector2 startPos, float baseRad, int lifeTime, Color color, float flashPitch)
    {
        pos = (followChunk is not null) ? followChunk.pos : startPos;
        this.baseRad = baseRad;
        this.lifeTime = lifeTime;
        this.color = color;
        room.PlaySound(SoundID.Flare_Bomb_Burn, pos, 1, flashPitch);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastLife = life;
        life += 1f / lifeTime;
        if (lastLife > 1)
        {
            Destroy();
            return;
        }
        bool owned = followChunk?.owner is not null;
        if (owned && room != followChunk.owner.room)
        {
            room = followChunk.owner.room;
        }
        if (room is null)
        {
            return;
        }
        lastDirection = direction;
        direction = Custom.DegToVec(Random.value * 360f) * 50f * LightIntensity;
        lastAlpha = alpha;
        alpha = Mathf.Pow(Random.value, 0.3f) * LightIntensity;
        lastRad = rad;
        rad = Mathf.Pow(Random.value, 0.3f) * baseRad * LightIntensity;
        for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
        {
            Creature ctr = room.abstractRoom.creatures[i].realizedCreature;
            if (ctr is null || !Custom.DistLess(pos, ctr.mainBodyChunk.pos, baseRad) || !room.VisualContact(pos, ctr.mainBodyChunk.pos))
            {
                continue;
            }
            if (ctr.Template.type == CreatureTemplate.Type.Spider && !ctr.dead)
            {
                ctr.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                ctr.Die();
            }
            else if (ctr is BigSpider bs && bs.Template.type == CreatureTemplate.Type.BigSpider)
            {
                bs.poison = 1f;
                bs.State.health -= Random.value * 0.2f;
                bs.Stun(Random.Range(10, 20));
                if (killTag is not null)
                {
                    bs.SetKillTag(killTag);
                }
            }
            else if (owned && ctr != followChunk.owner && ctr.State is not null && ctr.State is GlowSpiderState gs && gs.juice < gs.MaxJuice)
            {
                gs.juice += 0.025f;
            }
            ctr.Blind((int)Custom.LerpMap(Vector2.Distance(pos, ctr.VisionPoint), 60f, 600f, 400f, 20f));
        }

        if (light is not null)
        {
            light.stayAlive = true;
            light.setPos = pos;
            light.setAlpha = 1f - (0.6f * LightIntensity);
            light.setRad = Mathf.Max(rad, 1f + (LightIntensity * 10f));
            light.color = color;
            if (light.slatedForDeletetion || light.room != room)
            {
                light = null;
            }
        }
        else if (room.Darkness(pos) > 0f)
        {
            light = new LightSource(pos, environmentalLight: false, color, this);
            light.requireUpKeep = true;
            room.AddObject(light);
        }
    }

    public override void Destroy()
    {
        if (light is not null)
        {
            light.Destroy();
        }
        base.Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["FlareBomb"],
            scale = 2.5f
        };
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (followChunk is not null)
        {
            lastPos = pos;
            pos = Vector2.Lerp(followChunk.lastPos, followChunk.pos, timeStacker);
        }
        sLeaser.sprites[0].x = pos.x - camPos.x + Mathf.Lerp(lastDirection.x, direction.x, timeStacker);
        sLeaser.sprites[0].y = pos.y - camPos.y + Mathf.Lerp(lastDirection.y, direction.y, timeStacker);
        sLeaser.sprites[0].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 16f;
        sLeaser.sprites[0].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = color;
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner is null)
        {
            newContatiner = rCam.ReturnFContainer("Water");
        }
        newContatiner.AddChild(sLeaser.sprites[0]);
    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------