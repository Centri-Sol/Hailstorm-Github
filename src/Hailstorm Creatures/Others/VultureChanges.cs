using MoreSlugcats;
using RWCustom;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using MonoMod.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using System;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Messaging;
using System.Net;
using SlugBase.DataTypes;
using MonoMod;

namespace Hailstorm;

internal class VultureChanges
{

    public static void Hooks()
    {

        On.Vulture.ctor += HailstormVulturesSetup;
        On.Vulture.Violence += VultureViolence;

        On.KingTusks.Tusk.ctor += KingtuskCWT;
        On.KingTusks.Tusk.Update += KingtuskDeflectMomentum;
        On.KingTusks.WantToShoot += ErraticWindTuskWait;
        ReallyDumbVultureHooks();

        On.Vulture.JawSlamShut += AuroricMirosShortcutProtection;
        On.Vulture.Update += AuroricMirosLaser;
        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += AuroricMirosAggro;

        On.VultureGraphics.ctor += HailstormVulFeathers;
        On.VultureGraphics.InitiateSprites += AuroricMirosNeckMesh;
        On.VultureGraphics.BeakGraphic.InitiateSprites += AuroricMirosBeakMesh;
        On.VultureGraphics.AddToContainer += AuroricMirosBeakLayering;
        On.VultureGraphics.ApplyPalette += HailstormVulPalettes;
        On.VultureGraphics.DrawSprites += AuroricMirosSprites;
        On.VultureGraphics.ExitShadowMode += NonMirosColoring;

        On.Vulture.DropMask += HailstormVulMaskColoring;
        On.MoreSlugcats.VultureMaskGraphics.DrawSprites += HailstormVulMaskDrawSprites;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSSlugs.Incandescent);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------
    // General Vulture stuff
    public static ConditionalWeakTable<Vulture, VultureInfo> VulData = new();
    public static void HailstormVulturesSetup(On.Vulture.orig_ctor orig, Vulture vul, AbstractCreature absVul, World world)
    {
        orig(vul, absVul, world);
        if (!VulData.TryGetValue(vul, out _) && (IsIncanStory(world.game) || (vul.IsMiros && HSRemix.AuroricMirosEverywhere.Value)))
        {
            VulData.Add(vul, new VultureInfo(vul));
        }
        if (VulData.TryGetValue(vul, out VultureInfo vI))
        {
            Random.InitState(vul.abstractCreature.ID.RandomSeed);
            Random.State state = Random.state;

            vI.albino = Random.value <
                    (vul.IsKing ? 0.005f : 0.0005f);

            if (vul.IsMiros)
            {
                float hue = Random.Range(200 / 360f, 280 / 360f);
                float bri = !vI.albino ? 0.5f : 0.9f;
                vI.ColorB = new HSLColor(hue, 0.15f, bri);
                hue = vI.ColorB.hue + (Random.value < 0.5f ? -20/360f : 20/360f);
                if (hue < 200/360f) hue += 80/360f;
                if (hue > 280/360f) hue -= 80/360f;
                if (vI.albino) hue = Random.Range(0.9f, 1.1f);
                vI.ColorA = new HSLColor(hue, 0.2f, 0.3f);
                hue = !vI.albino ? Random.Range(0.3f, 0.7f) : vI.ColorB.hue;
                bri = hue/3f;
                vI.eyeCol = new HSLColor(hue, 0.75f - bri, Custom.WrappedRandomVariation(0.55f + bri, 0.1f, 0.5f)).rgb;
            }
            else if (vul.IsKing)
            {
                vI.ColorB = vI.albino ?
                    new HSLColor(0.9f, 1, 0.5f) :
                    new HSLColor(0.5f, Mathf.Lerp(0.8f, 1, 1 - Random.value * Random.value), Mathf.Lerp(0.45f, 1f, Random.value * Random.value));
                vI.ColorA = new HSLColor(vI.ColorB.hue + Random.Range(-0.05f, 0.05f), Random.Range(0.6f, 0.85f), Random.Range(0.6f, 0.7f));
                vI.eyeCol = vI.ColorB.rgb;
            }
            else
            {
                absVul.state.meatLeft = 8;
                vul.neck.idealLength *= 0.7f;
                for (int b = 0; b < vul.bodyChunks.Length; b++)
                {
                    vul.bodyChunks[b].rad *= 0.7f;
                    vul.bodyChunks[b].mass *= 0.7f;
                }
                for (int b = 0; b < vul.bodyChunkConnections.Length; b++)
                {
                    vul.bodyChunkConnections[b].distance *= 0.7f;
                }
            }
            Random.state = state;
        }
    }
    public static void VultureViolence(On.Vulture.orig_Violence orig, Vulture vul, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos appPos, Creature.DamageType dmgType, float dmg, float bonusStun)
    {
        bool MaskGotHit =
            hitChunk is not null && hitChunk.index == 4 &&
            (vul.State as Vulture.VultureState).mask;

        if (vul?.room is not null && !vul.dead)
        {
            if (source?.owner is not null && source.owner is IceCrystal && MaskGotHit && dirAndMomentum.HasValue)
            {
                vul.DropMask(dirAndMomentum.Value);
            }
            if (!vul.IsMiros)
            {
                float antidiscouragement = (dmg * 0.75f) + (bonusStun / 345f);
                if (vul.IsKing)
                {
                    antidiscouragement *= 0.3f;
                }
                if (vul.room.game.IsStorySession && vul.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                {
                    antidiscouragement *= 1.5f;
                }

                vul.AI.disencouraged += antidiscouragement;
            }
        }

        bool ActivateNewLaser = false;

        if (vul is not null && VulData.TryGetValue(vul, out _))
        {
            if (vul.IsMiros)
            {
                dmg *= 2f; // HP: 20 -> 10
                bonusStun /= 3f;
                if (dmgType == Creature.DamageType.Explosion)
                {
                    dmg /= 4f;
                }
                else
                if (dmgType == Creature.DamageType.Electric || dmgType == Creature.DamageType.Water ||
                    dmgType == HailstormEnums.Heat || dmgType == HailstormEnums.Cold)
                {
                    dmg /= 2f;
                }
                ActivateNewLaser = vul.laserCounter < 1;
            }
            else if (!vul.IsKing)
            {
                dmg *= 1.4167f; // HP: 8.5 -> 6 (just about)
                bonusStun *= 0.7f;
                if (dmgType == Creature.DamageType.Electric || dmgType == HailstormEnums.Cold)
                {
                    dmg /= 2f;
                    bonusStun /= 2f;
                }
            }
        }

        orig(vul, source, dirAndMomentum, hitChunk, appPos, dmgType, dmg, bonusStun);

        if (ActivateNewLaser && vul.laserCounter > 0)
        {
            vul.laserCounter = (int)(vul.laserCounter * 0.75f);
        }
    }
    public static void ReallyDumbVultureHooks()
    {
        IL.KingTusks.Tusk.ShootUpdate += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((KingTusks.Tusk tusk, float speed) =>
            {
                return KingtuskTargetIsArmored(tusk, speed);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
        IL.VultureTentacle.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<VultureTentacle>(nameof(VultureTentacle.stun)),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((VultureTentacle wing) => HailstormVultureWingResizing(wing));
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] An IL hook for Auroric Miros Vultures exploded! And by that I mean it stopped working! Tell me about this, please.");
        };
    }
    public static bool HailstormVultureWingResizing(VultureTentacle wing)
    {
        if (wing?.vulture is not null && !wing.vulture.IsKing && VulData.TryGetValue(wing.vulture, out _))
        {
            if (wing.vulture.IsMiros)
            {
                wing.idealLength = Mathf.Lerp(70, 110, wing.flyingMode);
            }
            else
            {
                wing.idealLength = Mathf.Lerp(98, 154, wing.flyingMode);
            }
        }
        return true;
    }
    public static bool KingtuskTargetIsArmored(KingTusks.Tusk tusk, float speed)
    {
        Vector2 val = tusk.chunkPoints[0, 0] + tusk.shootDir * 20f;
        Vector2 pos = tusk.chunkPoints[0, 0] + tusk.shootDir * (20f + speed);
        FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(tusk.room, val, pos);
        SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(tusk, tusk.room, val, ref pos, 5f, 1, tusk.owner.vulture, hitAppendages: false);
        if (TuskData.TryGetValue(tusk, out TuskInfo tI) && !floatRect.HasValue && collisionResult.chunk?.owner is not null && collisionResult.chunk.owner is Lizard liz && liz.LizardState is ColdLizState lS && lS.armored)
        {
            int stun = (liz.Template.type == HailstormEnums.Freezer) ? 0 : 30;
            tusk.mode = KingTusks.Tusk.Mode.Dangling;
            tI.bounceOffSpeed = tusk.shootDir * speed * 0.5f;
            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.Violence(tusk.head, tusk.shootDir, liz.bodyChunks[1], null, Creature.DamageType.Stab, 2, stun);
            return true;
        }
        return false;
    }

    //---------------------------------------
    // King-specific
    public static ConditionalWeakTable<KingTusks.Tusk, TuskInfo> TuskData = new();
    public static void KingtuskCWT(On.KingTusks.Tusk.orig_ctor orig, KingTusks.Tusk tusk, KingTusks owner, int side)
    {
        orig(tusk, owner, side);
        if (!TuskData.TryGetValue(tusk, out _))
        {
            TuskData.Add(tusk, new TuskInfo(tusk));
        }
        
    }
    public static void KingtuskDeflectMomentum(On.KingTusks.Tusk.orig_Update orig, KingTusks.Tusk tusk)
    {
        if (tusk?.owner?.vulture is not null && tusk.owner.vulture.IsKing && VulData.TryGetValue(tusk.owner.vulture, out _))
        {
            if (tusk.mode == KingTusks.Tusk.Mode.StuckInWall && tusk.impaleChunk?.owner is not null && tusk.impaleChunk.owner is Creature victim && !victim.dead)
            {
                tusk.SwitchMode(KingTusks.Tusk.Mode.Retracting);
            }
            else if (tusk.mode == KingTusks.Tusk.Mode.Dangling || tusk.mode == KingTusks.Tusk.Mode.StuckInWall)
            {
                if (tusk.mode == KingTusks.Tusk.Mode.StuckInWall && tusk.room is not null)
                {
                    tusk.room.PlaySound(SoundID.King_Vulture_Tusk_Bounce_Off_Terrain, tusk.chunkPoints[0, 0]);
                }
                tusk.SwitchMode(KingTusks.Tusk.Mode.Retracting);
            }
            else if (tusk.mode == KingTusks.Tusk.Mode.Retracting)
            {
                if (tusk.currWireLength > 0f)
                {
                    tusk.currWireLength = Mathf.Max(0f, tusk.currWireLength - KingTusks.Tusk.maxWireLength/20f);
                }
                else
                {
                    float oldAttachFac = tusk.attached;
                    if (tusk.attached < 1f)
                    {
                        tusk.attached = Mathf.Min(1f, tusk.attached + 0.05f);
                    }
                    if (tusk.room is not null && oldAttachFac < 0.5f && tusk.attached >= 0.5f)
                    {
                        tusk.room.PlaySound(SoundID.King_Vulture_Tusk_Reattach, tusk.chunkPoints[0, 0]);
                    }
                }
            }
        }

        if (tusk is not null && TuskData.TryGetValue(tusk, out TuskInfo tI))
        {
            if (tusk.mode == KingTusks.Tusk.Mode.ShootingOut && tI.bounceOffSpeed != new Vector2(0, 0))
            {
                tI.bounceOffSpeed = new Vector2(0, 0);
            }

            orig(tusk);

            if (tusk.mode == KingTusks.Tusk.Mode.Dangling && tI.bounceOffSpeed != new Vector2(0, 0))
            {
                for (int t = 0; t < tusk.chunkPoints.GetLength(0); t++)
                {
                    tusk.chunkPoints[t, 2] -= tI.bounceOffSpeed;
                    SharedPhysics.TerrainCollisionData cd = tusk.scratchTerrainCollisionData.Set(tusk.chunkPoints[t, 0], tusk.chunkPoints[t, 1], tusk.chunkPoints[t, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
                    cd = SharedPhysics.VerticalCollision(tusk.room, cd);
                    cd = SharedPhysics.HorizontalCollision(tusk.room, cd);
                    tusk.chunkPoints[t, 0] = cd.pos;
                    tusk.chunkPoints[t, 2] = cd.vel;
                    tI.bounceOffSpeed = Vector2.Lerp(tI.bounceOffSpeed, new Vector2(0, 0), 0.066f);
                    if (cd.contactPoint.x != 0f)
                    {
                        tI.bounceOffSpeed.x *= 0.5f;
                    }
                    if (cd.contactPoint.y != 0f)
                    {
                        tI.bounceOffSpeed.y *= 0.5f;
                    }
                }
            }
        }
        else orig(tusk);

        if (Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval] && tusk?.room?.blizzardGraphics is not null && tusk.mode == KingTusks.Tusk.Mode.Dangling)
        {
            Color blizzardPixel;
            Vector2 blizzPush;
            for (int t = 0; t < tusk.chunkPoints.GetLength(0); t++)
            {
                Vector2 tuskPos = tusk.chunkPoints[t, 2];
                blizzardPixel = tusk.room.blizzardGraphics.GetBlizzardPixel((int)(tuskPos.x / 20f), (int)(tuskPos.y / 20f));

                blizzPush = new Vector2(0f - tusk.room.blizzardGraphics.WindAngle, 0.1f) * 0.75f;
                blizzPush *= blizzardPixel.g * (5f * tusk.room.blizzardGraphics.WindStrength);

                if (tusk.room.waterInverted && tuskPos.y > tusk.room.FloatWaterLevel(tuskPos.x))
                {
                    break;
                }
                else if (!MMF.cfgVanillaExploits.Value && tusk.room.FloatWaterLevel(tuskPos.x) > (tusk.room.abstractRoom.size.y + 20) * 20f)
                {
                    break;
                }
                else if (tusk.room.FloatWaterLevel(tuskPos.x) > tuskPos.y)
                {
                    break;
                }

                blizzPush.y *= -0.2f;

                SharedPhysics.TerrainCollisionData cd = tusk.scratchTerrainCollisionData.Set(tusk.chunkPoints[t, 0], tusk.chunkPoints[t, 1], tusk.chunkPoints[t, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
                cd = SharedPhysics.VerticalCollision(tusk.room, cd);
                cd = SharedPhysics.HorizontalCollision(tusk.room, cd);

                if (cd.contactPoint.y != 0)
                {
                    blizzPush.x *= 0.33f;
                }
                else
                {
                    blizzPush.x *= Mathf.Lerp(4f, 2f, Mathf.InverseLerp(0.35f, 1.6f, Weather.WindIntervalIntensities[Weather.WindInterval]));
                }

                tusk.chunkPoints[t, 2] += blizzPush;

            }
        }
    }
    public static bool ErraticWindTuskWait(On.KingTusks.orig_WantToShoot orig, KingTusks kt, bool checkVisualOnAnyTargetChunk, bool checkMinDistance)
    {
        if (Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval])
        {
            return false;
        }
        return orig(kt, checkVisualOnAnyTargetChunk, checkMinDistance);
    }

    //---------------------------------------
    // Miros-specific
    public static void AuroricMirosShortcutProtection(On.Vulture.orig_JawSlamShut orig, Vulture vul)
    {
        if (vul?.room?.abstractRoom?.creatures is not null && vul.bodyChunks is not null && (IsIncanStory(vul.room.game) || HSRemix.AuroricMirosEverywhere.Value is true))
        {
            for (int i = 0; i < vul.room.abstractRoom.creatures.Count; i++)
            {
                if (vul.grasps[0] is not null)
                {
                    break;
                }
                Creature ctr = vul.room.abstractRoom.creatures[i].realizedCreature;
                if (vul.room.abstractRoom.creatures[i] == vul.abstractCreature || !vul.AI.DoIWantToBiteCreature(vul.room.abstractRoom.creatures[i]) || ctr is null || ctr is not Player plr || plr.cantBeGrabbedCounter <= 0 || ctr.enteringShortCut.HasValue || ctr.inShortcut)
                {
                    continue;
                }
                for (int j = 0; j < ctr.bodyChunks.Length; j++)
                {
                    if (vul.grasps[0] is not null)
                    {
                        break;
                    }
                    Vector2 headDirection = Custom.DirVec(vul.neck.Tip.pos, vul.Head().pos);
                    if (!Custom.DistLess(vul.Head().pos + headDirection * 20f, ctr.bodyChunks[j].pos, 20f + ctr.bodyChunks[j].rad) || !vul.room.VisualContact(vul.Head().pos, ctr.bodyChunks[j].pos))
                    {
                        continue;
                    }
                    vul.Grab(ctr, 0, j, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1, overrideEquallyDominant: true, pacifying: false);
                    break;
                }
            }
        }
        orig(vul);
    }
    public static void AuroricMirosLaser(On.Vulture.orig_Update orig, Vulture vul, bool eu)
    {
        orig(vul, eu);
        if (vul?.room is not null && vul.IsMiros && VulData.TryGetValue(vul, out VultureInfo vI))
        {
            if (vul.laserCounter > 0)
            {
                if (vul.graphicsModule is not null && vul.graphicsModule is VultureGraphics vg && vg.soundLoop is not null)
                {
                    vg.soundLoop.Pitch *= 1 + Mathf.InverseLerp(50, 10, vul.laserCounter);
                    vg.soundLoop.Volume *= 1.25f;
                }

                if (!vul.dead && vul.laserCounter == 11)
                {
                    vul.room.AddObject(new Explosion(vul.room, vul, vul.mainBodyChunk.pos, 7, 240, 7, 2.5f, 240, 0, vul, 0, 120, 0.5f));
                    vul.room.AddObject(new Explosion.ExplosionLight(vul.mainBodyChunk.pos, 280, 1, 7, vI.eyeCol));
                    vul.room.AddObject(new Explosion.ExplosionLight(vul.mainBodyChunk.pos, 230, 1, 3, Color.white));
                    vul.room.AddObject(new ShockWave(vul.mainBodyChunk.pos, 360, 0.05f, 5));
                    vul.room.AddObject(new MirosBomb(vul.Head().pos, vI.laserAngle * 25f, vul, vI.eyeCol));
                    if (vul.LaserLight is not null)
                    {
                        vul.LaserLight.Destroy();
                    }
                    vul.laserCounter = 0;
                }
                if (vI.currentPrey is not null)
                {
                    if (vul.LaserLight is null)
                    {
                        vul.LaserLight = new (vI.currentPrey.mainBodyChunk.pos, false, Custom.HSL2RGB(Mathf.Lerp(80 / 360f, 0, vul.LaserLight.alpha), 1, 0.5f), vul);
                        vul.LaserLight.affectedByPaletteDarkness = 0;
                        vul.LaserLight.submersible = true;
                        vul.room.AddObject(vul.LaserLight);
                        vul.LaserLight.HardSetRad(300 - vul.laserCounter);
                        vul.LaserLight.HardSetAlpha(Mathf.InverseLerp(400, 40, vul.laserCounter));
                    }
                    else
                    {
                        vul.LaserLight.HardSetPos(vI.currentPrey.mainBodyChunk.pos);
                        vul.LaserLight.HardSetRad(300 - vul.laserCounter);
                        vul.LaserLight.HardSetAlpha(Mathf.InverseLerp(400, 40, vul.laserCounter));
                        vul.LaserLight.color = Custom.HSL2RGB(Mathf.Lerp(80 / 360f, 0, vul.LaserLight.alpha), 1, 0.5f);
                        if (vul.LaserLight.affectedByPaletteDarkness != 0)
                        {
                            vul.LaserLight.affectedByPaletteDarkness = 0;
                        }
                        if (!vul.LaserLight.submersible)
                        {
                            vul.LaserLight.submersible = true;
                        }
                    }
                }
            }

            if (vul.dead)
            {
                if (vI.currentPrey is not null)
                {
                    vI.currentPrey = null;
                }
                return;
            }

            if (vul.laserCounter > 0 && vul.AI?.preyTracker?.MostAttractivePrey is not null)
            {
                Tracker.CreatureRepresentation mostAttractivePrey = vul.AI.preyTracker.MostAttractivePrey;
                if (mostAttractivePrey.TicksSinceSeen < 40 && mostAttractivePrey.representedCreature?.realizedCreature is not null)
                {
                    vI.currentPrey = mostAttractivePrey.representedCreature.realizedCreature;
                }
                else vI.currentPrey = null;
            }
            else if (vI.currentPrey is not null)
            {
                vI.currentPrey = null;
            }

            if (HSRemix.ScissorhawkEagerBirds.Value && vul.laserCounter == 0 && vul.landingBrake == 1)
            {
                vul.laserCounter = 200;
            }
        }
    }
    public static CreatureTemplate.Relationship AuroricMirosAggro(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI vultureAI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        // Causes Miros Vultures to attack and eat ANYTHING that isn't flagged as inedible to them
        if (vultureAI.vulture.IsMiros && (IsIncanStory(vultureAI.vulture.room.game) || HSRemix.AuroricMirosEverywhere.Value is true))
        {
            CreatureTemplate.Type ctrType = dynamRelat.trackerRep.representedCreature.creatureTemplate.type;

            bool inedibleForMirosVultures =
                ctrType == CreatureTemplate.Type.Overseer ||
                ctrType == CreatureTemplate.Type.Leech ||
                ctrType == CreatureTemplate.Type.SeaLeech ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
                ctrType == CreatureTemplate.Type.Vulture ||
                ctrType == CreatureTemplate.Type.KingVulture ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture ||
                ctrType == CreatureTemplate.Type.Deer ||
                ctrType == CreatureTemplate.Type.GarbageWorm ||
                ctrType == CreatureTemplate.Type.Fly ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
                ctrType == CreatureTemplate.Type.TempleGuard;

            bool dangerousToMirosVultures =
                ctrType == CreatureTemplate.Type.BigEel ||
                ctrType == CreatureTemplate.Type.DaddyLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs;


            if (vultureAI?.vulture?.killTag is not null && vultureAI.vulture.killTag == dynamRelat.trackerRep.representedCreature && !dynamRelat.trackerRep.representedCreature.state.dead && !dangerousToMirosVultures && !inedibleForMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, 1.33f);
            }
            if (ctrType == CreatureTemplate.Type.PoleMimic ||
                ctrType == CreatureTemplate.Type.TentaclePlant)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, 0.5f);
            }
            if (ctrType == CreatureTemplate.Type.BrotherLongLegs)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 1);
            }
            if (dangerousToMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, (ctrType == CreatureTemplate.Type.BigEel) ? 0.33f : 0.5f);
            }
            if (!inedibleForMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, dynamRelat.trackerRep.representedCreature.state.dead ? 1 : 0.9f);
            }
        }
        return orig(vultureAI, dynamRelat);
    }

    //---------------------------------------
    // Graphics
    public static void HailstormVulFeathers(On.VultureGraphics.orig_ctor orig, VultureGraphics vg, Vulture owner)
    {
        orig(vg, owner);
        if (vg?.vulture is null || !VulData.TryGetValue(vg.vulture, out _))
        {
            return;
        }

        Random.InitState(vg.vulture.abstractCreature.ID.RandomSeed);
        Random.State state = Random.state;
        if (vg.IsMiros)
        {
            vg.feathersPerWing = Random.value < 0.15f ? 5 : 4;
            vg.beakFatness = Mathf.Pow(Random.value, 0.5f);
            int sprite = vg.FirstBeakSprite();
            for (int s = 0; s < vg.beak.Length; s++)
            {
                vg.beak[s] = new VultureGraphics.BeakGraphic(vg, s, sprite);
                sprite += vg.beak[s].totalSprites;
            }
            vg.eyeTrail.sprite = vg.EyeTrailSprite();
        }
        else if (vg.IsKing)
        {
            vg.feathersPerWing = Random.Range(12, 20);
        }
        else
        {
            vg.feathersPerWing = Random.Range(8, 12);
        }

        vg.wings = new VultureFeather[vg.vulture.tentacles.Length, vg.feathersPerWing];
        float featherDamageFac = (Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, Random.value);
        float shriveledFeatherFac = (Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, Random.value); // I'll be honest, I don't actually understand what effect these have.
        float brokenColFac = (Random.value < 0.5f) ? 20f : Mathf.Lerp(3f, 6f, Random.value);
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                float featherFac = (f + 0.5f) / vg.feathersPerWing;
                float wingPos = Mathf.Lerp(1f - Mathf.Pow(vg.IsMiros ? 0.95f : 0.89f, f), Mathf.Sqrt(featherFac), 0.5f);
                wingPos = Mathf.InverseLerp(0.1f, 1.1f, wingPos);
                if (vg.IsMiros && f == vg.feathersPerWing - 1)
                {
                    wingPos = 0.8f;
                }
                vg.wings[w, f] = new VultureFeather(vg, vg.vulture.tentacles[w], wingPos,
                    VultureTentacle.FeatherContour(featherFac, vg.IsMiros ? 0.25f : 0) * Mathf.Lerp(vg.IsMiros ? 90f : 50f, vg.IsMiros ? 120f : 75f, Random.value),
                    VultureTentacle.FeatherContour(featherFac, 1) * Mathf.Lerp(vg.IsMiros ? 120f : 65f, vg.IsMiros ? 150f : 75f, Random.value) * (vg.IsKing ? 1.3f : 1f),
                    Mathf.Lerp(vg.IsMiros ? 5f : 3f, vg.IsMiros ? 8f : 6f, VultureTentacle.FeatherWidth(featherFac)));
                bool RNG = Random.value < 0.025f;
                if (Random.value < 1 / featherDamageFac || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].lose = 1f - Random.value * Random.value * Random.value;
                    if (Random.value < 0.4f)
                    {
                        vg.wings[w, f].brokenColor = 1f - Random.value * Random.value;
                    }
                }
                if (Random.value < 1f / shriveledFeatherFac)
                {
                    vg.wings[w, f].extendedLength /= 5f;
                    vg.wings[w, f].contractedLength = vg.wings[w, f].extendedLength;
                    vg.wings[w, f].brokenColor = 1f;
                    vg.wings[w, f].width /= 1.7f;
                }
                if (Random.value < 0.025f || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].contractedLength = vg.wings[w, f].extendedLength * 0.7f;
                }
                if (Random.value < 1f / brokenColFac || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].brokenColor = (Random.value < 0.5f) ? 1f : Random.value;
                }
            }
        }
        

        Random.state = state;

    }
    public static void AuroricMirosNeckMesh(On.VultureGraphics.orig_InitiateSprites orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(vg, sLeaser, rCam);
        if (vg?.vulture is null || !VulData.TryGetValue(vg.vulture, out _))
        {
            return;
        }
        if (!vg.IsMiros && !vg.IsKing)
        {
            sLeaser.sprites[vg.BodySprite].scale = 0.7f;
            for (int t = 0; t < vg.tusks.Length; t++)
            {
                sLeaser.sprites[vg.TuskSprite(t)].scale = 0.7f;
            }
        }

        sLeaser.sprites[vg.NeckSprite] = TriangleMesh.MakeLongMesh(vg.vulture.neck.tChunks.Length, pointyTip: false, customColor: true);
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            sLeaser.sprites[vg.TentacleSprite(w)] = TriangleMesh.MakeLongMesh(vg.vulture.tentacles[w].tChunks.Length, pointyTip: false, customColor: true);
        }
    }
    public static void AuroricMirosBeakMesh(On.VultureGraphics.BeakGraphic.orig_InitiateSprites orig, VultureGraphics.BeakGraphic bg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(bg, sLeaser, rCam);
        if (bg?.owner?.vulture is null || !bg.owner.IsMiros || !VulData.TryGetValue(bg.owner.vulture, out _))
        {
            return;
        }
        TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[11];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
        }
        sLeaser.sprites[bg.firstSprite] = new TriangleMesh("Futile_White", array, customColor: true);
    }
    public static void AuroricMirosBeakLayering(On.VultureGraphics.orig_AddToContainer orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(vg, sLeaser, rCam, newContainer);
        if (vg?.vulture is null || !vg.IsMiros || !VulData.TryGetValue(vg.vulture, out _))
        {
            return;
        }

        for (int b = vg.FirstBeakSprite(); b <= vg.LastBeakSprite(); b++)
        {
            sLeaser.sprites[b].MoveBehindOtherNode(sLeaser.sprites[vg.HeadSprite]);
        }

    }
    public static void HailstormVulPalettes(On.VultureGraphics.orig_ApplyPalette orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(vg, sLeaser, rCam, palette);
        if (vg?.vulture?.room is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vI))
        {
            return;
        }

        Random.InitState(vg.vulture.abstractCreature.ID.RandomSeed);
        Random.State state = Random.state;
        if (vg.IsMiros)
        {
            vI.featherColor1 = vI.eyeCol;

            float hue = !vI.albino ? Random.Range(0.3f, 0.7f) : Random.Range(200 / 360f, 280 / 360f);
            float bri = hue / 3f;
            vI.featherColor2 = new HSLColor(hue, 0.75f - bri, Custom.WrappedRandomVariation(0.55f + bri, 0.1f, 0.5f)).rgb;
            if (Random.value < 0.5f)
            {
                vI.featherColor2 = Color.Lerp(vI.featherColor2, vI.featherColor1, Random.value);
            }

            vI.wingColor = Random.value < 0.5f ?
                new HSLColor(12 / 360f, Random.value * 0.25f, Random.Range(0.1f, 0.25f)).rgb :
                new HSLColor(12 / 360f, Random.value * 0.15f, Random.Range(0.1f, 0.85f)).rgb;
            vI.beakColor = Color.Lerp(vI.wingColor, vg.palette.blackColor, 0.5f);
        }
        else if (!vg.IsKing)
        {
            Color.RGBToHSV(Color.Lerp(vg.palette.blackColor, vg.palette.fogColor, 0.1f), out float h, out float s, out float l);
            vI.ColorA = new HSLColor(h, s, l);
            Color.RGBToHSV(vg.palette.fogColor, out h, out s, out l); //Color.Lerp(vg.palette.blackColor, vg.palette.fogColor, 0.66f)
            vI.ColorB = new HSLColor(h, s, l);
            if (vI.albino)
            {
                HSLColor newB = vI.ColorA;
                vI.ColorA = vI.ColorB;
                vI.ColorB = newB;
            }
            vI.eyeCol = vI.ColorA.rgb;
            vI.featherColor1 = vI.ColorA.rgb;
            float hue = !vI.albino ? Random.Range(240 / 360f, 320 / 360f) : 0;
            float bri = hue / 3f;
            vI.featherColor2 = new HSLColor(hue, 0.6f - bri, 0.7f + bri).rgb;
            if (!vI.albino && Random.value < 0.5f)
            {
                vI.featherColor2 = Color.Lerp(vI.featherColor2, vI.featherColor1, Random.value/2f);
            }
        }
        Random.state = state;

        vg.albino = vI.albino;
        vg.ColorA = vI.ColorA;
        vg.ColorB = vI.ColorB;
        vg.eyeCol = vI.eyeCol;

    }
    public static void AuroricMirosSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(vg, sLeaser, rCam, timeStacker, camPos);
        if (vg?.vulture?.room is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vI))
        {
            return;
        }
        
        if (!vg.IsMiros && !vg.shadowMode)
        {
            if (!vg.IsKing)
            {
                RavenColors(vg, vI, sLeaser);
            }
        }
        else if (vg.IsMiros && !vg.culled)
        {
            if (!vg.shadowMode)
            {
                AuroricMirosColors(vg, vI, sLeaser);
            }

            float alpha = Mathf.InverseLerp(320, 40, vg.vulture.laserCounter);
            sLeaser.sprites[vg.LaserSprite()].isVisible = vg.vulture.laserCounter > 0 && alpha > 0f;

            if (!sLeaser.sprites[vg.LaserSprite()].isVisible)
            {
                return;
            }

            Color laserCol = vI.eyeCol;
            sLeaser.sprites[vg.LaserSprite()].alpha = alpha;
            CustomFSprite Laser = sLeaser.sprites[vg.LaserSprite()] as CustomFSprite;
            Laser.verticeColors[0] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[1] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[2] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[3] = Custom.RGB2RGBA(laserCol, alpha);

            if (vI.currentPrey is null)
            {
                return;
            }

            Vector2 headPos = Vector2.Lerp(vg.vulture.Head().lastPos, vg.vulture.Head().pos, timeStacker);
            Vector2 endOfNeckPos = Custom.DirVec(Vector2.Lerp(vg.vulture.neck.Tip.lastPos, vg.vulture.neck.Tip.pos, timeStacker), headPos);
            vI.laserAngle =
                vI.currentPrey is not null ? Custom.DirVec(headPos, vI.currentPrey.mainBodyChunk.pos) :
                sLeaser.sprites[vg.LaserSprite()].isVisible ? Custom.DirVec((sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).vertices[0], (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).vertices[1]) :
                Custom.DirVec(endOfNeckPos, headPos);

            Laser.MoveVertice(0, headPos - (endOfNeckPos * 4f) + (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(1, headPos - (endOfNeckPos * 4f) - (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(2, vI.currentPrey.mainBodyChunk.pos - (endOfNeckPos * 4f) - (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(3, vI.currentPrey.mainBodyChunk.pos - (endOfNeckPos * 4f) + (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
        }
    }
    public static void RavenColors(VultureGraphics vg, VultureInfo vI, RoomCamera.SpriteLeaser sLeaser)
    {
        float darkness = vg.darkness;
        Color blackColor = vg.palette.blackColor;
        //----Body----//
        for (int b = 0; b < sLeaser.sprites.Length; b++)
        {
            sLeaser.sprites[b].color = Color.Lerp(vI.ColorB.rgb, blackColor, darkness);
        }
        //----Neck and Head----//
        TriangleMesh neck = sLeaser.sprites[vg.NeckSprite] as TriangleMesh;
        for (int n = 0; n < neck.verticeColors.Length; n++)
        {
            float lerp = Mathf.InverseLerp(neck.verticeColors.Length * 0.25f, neck.verticeColors.Length * 0.75f, n);
            neck.verticeColors[n] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, lerp), blackColor, darkness);
        }
        sLeaser.sprites[vg.HeadSprite].color = vI.ColorA.rgb;
        sLeaser.sprites[vg.EyesSprite].color = vg.eyeCol;
        for (int t = 0; t < vg.tusks.Length; t++)
        {
            sLeaser.sprites[vg.TuskSprite(t)].color = vI.ColorA.rgb;
        }
        //----Wings----//
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int t = 0; t < wing.verticeColors.Length; t++)
            {
                float wingColorFac = Mathf.InverseLerp(wing.verticeColors.Length * 0.7f, wing.verticeColors.Length * 0.9f, t);
                wing.verticeColors[t] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, wingColorFac), blackColor, darkness);
            }
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                sLeaser.sprites[vg.FeatherSprite(w, f)].color = Color.Lerp(vI.ColorB.rgb, blackColor, darkness);
                float lerpReversePoint = (vg.feathersPerWing - 1)/3f;
                float featherColorFac = (f < lerpReversePoint) ?
                    Mathf.InverseLerp(0, lerpReversePoint, f) :
                    Mathf.InverseLerp(lerpReversePoint, vg.feathersPerWing - 1, f);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].color = Color.Lerp(vI.featherColor1, vI.featherColor2, featherColorFac);
            }
        }
        //----Back Shields----//
        for (int s = 0; s < 2; s++)
        {
            sLeaser.sprites[vg.BackShieldSprite(s)].color = Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, 0.75f);
            sLeaser.sprites[vg.FrontShieldSprite(s)].color = vI.ColorA.rgb;
        }
        //----Danglies----//
        for (int d = 0; d < 2; d++)
        {
            TriangleMesh dangly = sLeaser.sprites[vg.AppendageSprite(d)] as TriangleMesh;
            for (int k = 0; k < dangly.verticeColors.Length; k++)
            {
                float danglyColorFac = Mathf.InverseLerp(dangly.verticeColors.Length * 0.25f, dangly.verticeColors.Length * 0.75f, k);
                dangly.verticeColors[k] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, danglyColorFac), blackColor, darkness);
            }
        }
    }
    public static void AuroricMirosColors(VultureGraphics vg, VultureInfo vI, RoomCamera.SpriteLeaser sLeaser)
    {
        float darkness = vg.darkness;
        Color blackColor = vg.palette.blackColor;
        //----Body----//
        for (int b = 0; b < sLeaser.sprites.Length; b++)
        {
            if (b != vg.EyeTrailSprite())
            {
                sLeaser.sprites[b].color = Color.Lerp(vI.ColorB.rgb, blackColor, darkness);
            }
        }
        //----Neck and Head----//
        TriangleMesh neck = sLeaser.sprites[vg.NeckSprite] as TriangleMesh;
        for (int n = 0; n < neck.verticeColors.Length; n++)
        {
            float lerp = Mathf.InverseLerp(neck.verticeColors.Length * 0.2f, neck.verticeColors.Length * 0.8f, n);
            neck.verticeColors[n] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, lerp), blackColor, darkness);
        }
        sLeaser.sprites[vg.HeadSprite].color = Color.Lerp(vI.ColorA.rgb, blackColor, darkness);
        sLeaser.sprites[vg.EyesSprite].color = Color.Lerp(vg.eyeCol, vg.palette.fogColor, darkness);
        for (int b = vg.FirstBeakSprite(); b <= vg.LastBeakSprite(); b++)
        {
            sLeaser.sprites[b].color = Color.Lerp(vI.beakColor, blackColor, darkness);
        }
        //----Wings----//
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {

            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int t = 0; t < wing.verticeColors.Length; t++)
            {
                float wingColorFac = Mathf.InverseLerp(0, wing.verticeColors.Length * 0.4f, t);
                wing.verticeColors[t] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.wingColor, wingColorFac), blackColor, darkness);
            }
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                float featherColorFac = Mathf.InverseLerp(0, vg.feathersPerWing - 1, f);
                sLeaser.sprites[vg.FeatherSprite(w, f)].color = vI.wingColor;
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].color = Color.Lerp(vI.featherColor1, vI.featherColor2, featherColorFac);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].isVisible = true;
            }
        }
        //----Back Shields----//
        for (int s = 0; s < 2; s++)
        {
            sLeaser.sprites[vg.BackShieldSprite(s)].color = Color.Lerp(vI.ColorA.rgb, blackColor, darkness);
            sLeaser.sprites[vg.FrontShieldSprite(s)].color = Color.Lerp(vI.ColorA.rgb, blackColor, 0.25f + (darkness * 0.75f));
        }
        //----Danglies----//
        for (int d = 0; d < 2; d++)
        {
            TriangleMesh dangly = sLeaser.sprites[vg.AppendageSprite(d)] as TriangleMesh;
            for (int k = 0; k < dangly.verticeColors.Length; k++)
            {
                float danglyColorFac = Mathf.InverseLerp(0, dangly.verticeColors.Length, k);
                dangly.verticeColors[k] = Color.Lerp(Color.Lerp(vI.ColorB.rgb, vI.ColorA.rgb, danglyColorFac), blackColor, darkness);
            }
        }
    }
    public static void NonMirosColoring(On.VultureGraphics.orig_ExitShadowMode orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
    {
        orig(vg, sLeaser, rCam, changeContainer);
        if (vg?.vulture?.room is null || vg.IsMiros || !VulData.TryGetValue(vg.vulture, out _))
        {
            return;
        }


        if (vg.IsKing)
        {
            Color bodyCol = Color.Lerp(vg.palette.blackColor, vg.palette.fogColor, 0.66f);
            Color whiteCol = Color.white;
            if (vg.albino)
            {
                bodyCol = Color.Lerp(bodyCol, Color.white, 0.86f - vg.palette.darkness / 1.8f);
                whiteCol = Color.Lerp(bodyCol, vg.palette.blackColor, 0.74f);
                bodyCol = Color.Lerp(bodyCol, vg.palette.skyColor, 0.21f);

                HSLColor colorB = vg.ColorB;
                colorB.saturation = Mathf.Lerp(colorB.saturation, 1, 0.15f);
                colorB.hue = 0;
                _ = colorB.rgb;
            }
            float darkness = vg.darkness;
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = bodyCol;
            }
            for (int j = 0; j < 2; j++)
            {
                TriangleMesh wing = sLeaser.sprites[vg.AppendageSprite(j)] as TriangleMesh;
                for (int k = 0; k < wing.verticeColors.Length; k++)
                {
                    float colorFac = k / Math.Max(wing.verticeColors.Length - 1f, 24f);
                    float skewedColorFac = Mathf.Clamp(Mathf.InverseLerp(0.15f, 0.7f, colorFac), 0, 1);
                    skewedColorFac = Mathf.Pow(skewedColorFac, 0.5f);
                    if (vg.albino)
                    {
                        wing.verticeColors[k] = Color.Lerp(Color.Lerp(bodyCol, whiteCol, skewedColorFac), vg.palette.blackColor, darkness);
                    }
                    else
                    {
                        wing.verticeColors[k] = Color.Lerp(Color.Lerp(bodyCol, Color.Lerp(vg.ColorA.rgb, vg.palette.fogColor, 0.75f), skewedColorFac), bodyCol, darkness);
                    }
                }
            }
            Color maskCol = Color.Lerp(vg.ColorA.rgb, Color.white, 0.35f);
            sLeaser.sprites[vg.MaskSprite].color = Color.Lerp(maskCol, vg.palette.blackColor, darkness);
            for (int l = 0; l < 2; l++)
            {
                if (vg.albino)
                {
                    sLeaser.sprites[vg.BackShieldSprite(l)].color = Color.Lerp(Color.Lerp(whiteCol, bodyCol, 0.7f), vg.palette.blackColor, darkness);
                    sLeaser.sprites[vg.FrontShieldSprite(l)].color = Color.Lerp(Color.Lerp(whiteCol, bodyCol, 0.35f), vg.palette.blackColor, darkness);
                }
                else
                {
                    sLeaser.sprites[vg.BackShieldSprite(l)].color = Color.Lerp(Color.Lerp(vg.ColorA.rgb, bodyCol, 0.8f), bodyCol, darkness);
                    sLeaser.sprites[vg.FrontShieldSprite(l)].color = Color.Lerp(Color.Lerp(vg.ColorA.rgb, bodyCol, 0.4f), bodyCol, darkness);
                }
            }

            sLeaser.sprites[vg.EyesSprite].color = Color.Lerp(Color.Lerp(vg.eyeCol, bodyCol, vg.albino ? 0.1f : 0.2f), bodyCol, darkness / 4f);

            if (vg.vulture.kingTusks is not null)
            {
                vg.vulture.kingTusks.ApplyPalette(vg, vg.palette, maskCol, sLeaser, rCam);
                sLeaser.sprites[vg.MaskArrowSprite].color = Color.Lerp(Color.Lerp(HSLColor.Lerp(vg.ColorA, vg.ColorB, 0.5f).rgb, bodyCol, 0.6f), bodyCol, darkness);
            }
        }
    }

    // Mask Colors
    public static void HailstormVulMaskColoring(On.Vulture.orig_DropMask orig, Vulture vul, Vector2 violenceDir)
    {
        if (vul?.room is not null && VulData.TryGetValue(vul, out VultureInfo vI) && vul.State is not null && vul.State is Vulture.VultureState vs && vs.mask)
        {
            vs.mask = false;
            if (vul.killTag is not null)
            {
                SocialMemory.Relationship relationship = vul.State.socialMemory.GetOrInitiateRelationship(vul.killTag.ID);
                relationship.like = -1f;
                relationship.tempLike = -1f;
                relationship.know = 1f;
            }
            AbstractPhysicalObject absMask = new VultureMask.AbstractVultureMask(vul.room.world, null, vul.abstractPhysicalObject.pos, vul.room.game.GetNewID(), vul.abstractCreature.ID.RandomSeed, vul.IsKing);
            vul.room.abstractRoom.AddEntity(absMask);
            absMask.pos = vul.abstractCreature.pos;
            absMask.RealizeInRoom();

            VultureMask mask = absMask.realizedObject as VultureMask;
            mask.firstChunk.HardSetPosition(vul.bodyChunks[4].pos);
            mask.firstChunk.vel = vul.bodyChunks[4].vel + violenceDir;
            mask.maskGfx.ColorA = vI.ColorA;
            mask.maskGfx.ColorB = vI.ColorB;
            mask.fallOffVultureMode = 1f;
        }
        orig(vul, violenceDir);
    }
    public static void HailstormVulMaskDrawSprites(On.MoreSlugcats.VultureMaskGraphics.orig_DrawSprites orig, VultureMaskGraphics mg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(mg, sLeaser, rCam, timeStacker, camPos);
        if (IsIncanStory(mg?.attachedTo?.room?.game))
        {
            Vector2 pos = Vector2.zero;
            if (mg.overrideDrawVector.HasValue)
            {
                pos = mg.overrideDrawVector.Value;
            }
            else if (mg.attachedTo is not null)
            {
                pos = Vector2.Lerp(mg.attachedTo.firstChunk.lastPos, mg.attachedTo.firstChunk.pos, timeStacker);
            }
            float darkness = rCam.room.Darkness(pos) * (1 - rCam.room.LightSourceExposure(pos)) * 0.8f * (1 - mg.fallOffVultureMode);
            Color maskColor = Color.Lerp(Color.Lerp(mg.ColorA.rgb, Color.white, 0.35f * mg.fallOffVultureMode), mg.blackColor, Mathf.Lerp(0.2f, 1, Mathf.Pow(darkness, 2f)));
            Color vulBodyCol = Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, 0.66f);
            sLeaser.sprites[mg.firstSprite].color = maskColor;
            sLeaser.sprites[mg.firstSprite + 1].color = Color.Lerp(maskColor, mg.blackColor, Mathf.Lerp(0.75f, 1, darkness));
            sLeaser.sprites[mg.firstSprite + 2].color = Color.Lerp(maskColor, mg.blackColor, Mathf.Lerp(0.75f, 1, darkness));
            if (mg.King)
            {
                sLeaser.sprites[mg.firstSprite + 3].color = Color.Lerp(Color.Lerp(HSLColor.Lerp(mg.ColorA, mg.ColorB, 0.5f).rgb, vulBodyCol, 0.6f), vulBodyCol, darkness);
            }
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

}

public class MirosBomb : UpdatableAndDeletable, IDrawable
{
    //----------------------------------------------------------------------------------

    public Vector2 lastPos;
    public Vector2 pos;
    public Vector2 vel;
    private Color color;
    private Color explodeColor;
    private float rad;
    private Vector2 rotation;
    public float burn;
    public float submersion
    {
        get
        {
            if (room is null)
            {
                return 0f;
            }
            if (room.waterInverted)
            {
                return 1f - Mathf.InverseLerp(pos.y - rad, pos.y + rad, room.FloatWaterLevel(pos.x));
            }
            float floatWaterLvl = room.FloatWaterLevel(pos.x);
            if (!MMF.cfgVanillaExploits.Value && floatWaterLvl > (room.abstractRoom.size.y + 20) * 20)
            {
                return 1f;
            }
            return Mathf.InverseLerp(pos.y - rad, pos.y + rad, floatWaterLvl);
        }
    }

    public PhysicalObject source;

    public float[] spikes;
    public Smoke.BombSmoke smoke;

    //----------------------------------------------------------------------------------

    public MirosBomb(Vector2 startingPos, Vector2 baseVel, PhysicalObject bombCreator, Color bombColor)
    {
        lastPos = startingPos;
        vel = baseVel;
        pos = startingPos + baseVel;
        source = bombCreator;
        explodeColor = bombColor;
        rotation = Custom.RNV();
        rad = 4;
        spikes = new float[Random.Range(3, 8)];
        for (int i = 0; i < spikes.Length; i++)
        {
            spikes[i] = (i + Random.value) * (360f / (float)spikes.Length);
        }
    }

    //---------------------------------------

    public override void Update(bool eu)
    {
        if (submersion > 0)
        {
            vel.y = Mathf.Lerp(vel.y, 0, submersion / 100f);
            vel.x = Mathf.Lerp(vel.x, 0, submersion / 200f);
        }
        if (vel.y + vel.x < 8 && burn == 0)
        {
            burn += 1/30f;
        }
        int updates = Mathf.CeilToInt(vel.magnitude / rad);
        for (int m = 0; m < updates; m++)
        {
            lastPos = pos;
            pos += vel / (float)updates;

            Vector2 outerRad = pos + vel.normalized * rad;
            FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, outerRad, pos - vel.normalized * rad);
            Vector2 terrainTraceArea = default;
            if (floatRect.HasValue)
            {
                terrainTraceArea = new(floatRect.Value.left, floatRect.Value.bottom);
            }
            SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos, ref pos, rad, 2, source, false);
            if (floatRect.HasValue && collisionResult.chunk is not null)
            {
                if (Vector2.Distance(outerRad, terrainTraceArea) < Vector2.Distance(outerRad, collisionResult.collisionPoint))
                {
                    collisionResult.chunk = null;
                }
                else
                {
                    floatRect = null;
                }
            }
            if (floatRect.HasValue ||
                (collisionResult.chunk?.owner is not null && (collisionResult.chunk.owner is not Creature ctr || ctr.Template.type != MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)))
            {
                Explode(collisionResult.chunk);
            }
        }

        if (burn > 0f)
        {
            if (submersion > 0 && !room.waterObject.WaterIsLethal)
            {
                burn = 0f;
            }
            for (int i = 0; i < 3; i++)
            {
                room.AddObject(new Spark(Vector2.Lerp(lastPos, pos, Random.value), vel * 0.1f + Custom.RNV() * 3.2f * Random.value, explodeColor, null, 7, 30));
            }
            if (smoke is null)
            {
                smoke = new Smoke.BombSmoke(room, pos, null, explodeColor);
                room.AddObject(smoke);
            }
        }
        else
        {
            if (smoke is not null)
            {
                smoke.Destroy();
            }
            smoke = null;
        }

        if (burn > 0f || !room.IsPositionInsideBoundries(room.GetTilePosition(pos)))
        {
            burn += 1/30f;
            if (burn > 1)
            {
                Explode(null);
            }
        }
        if (burn <= 0f)
        {
            Explode(null);
        }

        base.Update(eu);
    }
    public void Explode(BodyChunk hitChunk)
    {
        if (slatedForDeletetion)
        {
            return;
        }
        Creature killtag = null;
        if (source is not null && source is Creature)
        {
            killtag = source as Creature;
        }
        room.AddObject(new SootMark(room, pos, 100, bigSprite: true));
        room.AddObject(new Explosion(room, source, pos, 2, 200, 7, 2.5f, 240, 0, killtag, 0, 120, 0.5f));
        room.AddObject(new Explosion.ExplosionLight(pos, 400, 1, 7, explodeColor));
        room.AddObject(new Explosion.ExplosionLight(pos, 400, 1, 3, Color.white));
        room.AddObject(new ExplosionSpikes(room, pos, 16, 32, 12, 8, 200, explodeColor));
        room.AddObject(new ShockWave(pos, 360, 0.05f, 6));
        for (int i = 0; i < 25; i++)
        {
            Vector2 angle = Custom.RNV();
            if (room.GetTile(pos + angle * 20f).Solid)
            {
                angle = (room.GetTile(pos - angle * 20f).Solid ? Custom.RNV() : (angle * -1f));
            }
            for (int j = 0; j < 3; j++)
            {
                room.AddObject(new Spark(pos + angle * Mathf.Lerp(30f, 60f, Random.value), angle * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
            }
            room.AddObject(new Explosion.FlashingSmoke(pos + angle * 40f * Random.value, angle * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
        }
        if (smoke is not null)
        {
            for (int k = 0; k < 8; k++)
            {
                smoke.EmitWithMyLifeTime(pos + Custom.RNV(), Custom.RNV() * Random.value * 17f);
            }
        }
        for (int l = 0; l < 6; l++)
        {
            room.AddObject(new ScavengerBomb.BombFragment(pos, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
        }
        room.ScreenMovement(pos, default, 1.3f);
        room.PlaySound(SoundID.Bomb_Explode, pos);
        room.InGameNoise(new Noise.InGameNoise(pos, 9000, null, 1));
        bool smokeTime = hitChunk is not null;
        for (int n = 0; n < 5; n++)
        {
            if (room.GetTile(pos + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
            {
                smokeTime = true;
                break;
            }
        }
        if (smokeTime)
        {
            if (smoke is null)
            {
                smoke = new Smoke.BombSmoke(room, pos, null, explodeColor);
                room.AddObject(smoke);
            }
            if (hitChunk is not null)
            {
                smoke.chunk = hitChunk;
            }
            else
            {
                smoke.chunk = null;
                smoke.fadeIn = 1f;
            }
            smoke.pos = pos;
            smoke.stationary = true;
            smoke.DisconnectSmoke();
        }
        else if (smoke is not null)
        {
            smoke.Destroy();
        }
        Destroy();
    }

    //---------------------------------------

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[spikes.Length + 4];
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel");
            sLeaser.sprites[i].scaleX = Mathf.Lerp(1, 2, Mathf.Pow(Random.value, 1.8f));
            sLeaser.sprites[i].scaleY = Mathf.Lerp(4, 7, Random.value);
        }
        for (int j = 0; j < spikes.Length; j++)
        {
            sLeaser.sprites[2 + j] = new FSprite("pixel");
            sLeaser.sprites[2 + j].scaleX = Mathf.Lerp(2, 3, Random.value);
            sLeaser.sprites[2 + j].scaleY = Mathf.Lerp(5, 7, Random.value);
            sLeaser.sprites[2 + j].anchorY = 0f;
        }
        sLeaser.sprites[spikes.Length + 2] = new FSprite("Futile_White");
        sLeaser.sprites[spikes.Length + 2].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
        sLeaser.sprites[spikes.Length + 2].scale = (rad + 0.75f) / 10f;
        sLeaser.sprites[spikes.Length + 2].alpha = Mathf.Lerp(0.2f, 0.4f, Random.value);
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
        {
            new TriangleMesh.Triangle(0, 1, 2)
        };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true);
        sLeaser.sprites[spikes.Length + 3] = triangleMesh;
        AddToContainer(sLeaser, rCam, null);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[2].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), rotation);
        sLeaser.sprites[spikes.Length + 2].x = pos.x - camPos.x;
        sLeaser.sprites[spikes.Length + 2].y = pos.y - camPos.y;
        for (int i = 0; i < spikes.Length; i++)
        {
            sLeaser.sprites[2 + i].x = pos.x - camPos.x;
            sLeaser.sprites[2 + i].y = pos.y - camPos.y;
            sLeaser.sprites[2 + i].rotation = Custom.VecToDeg(rotation) + spikes[i];
        }
        Color val3 = Color.Lerp(explodeColor, Color.red, 0.5f + 0.2f * Mathf.Pow(Random.value, 0.2f));
        val3 = Color.Lerp(val3, Color.white, Mathf.Pow(Random.value, 3));
        for (int j = 0; j < 2; j++)
        {
            sLeaser.sprites[j].x = pos.x - camPos.x;
            sLeaser.sprites[j].y = pos.y - camPos.y;
            sLeaser.sprites[j].rotation = Custom.VecToDeg(rotation) + (j * 90);
            sLeaser.sprites[j].color = val3;
        }
        sLeaser.sprites[spikes.Length + 3].isVisible = true;
        Vector2 posDifference = pos - lastPos;
        Vector2 perpAngleIthinkIDK = Custom.PerpendicularVector(posDifference.normalized);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(0, pos + perpAngleIthinkIDK * 2f - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(1, pos - perpAngleIthinkIDK * 2f - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(2, lastPos - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[0] = color;
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[1] = color;
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[2] = explodeColor;

        if (sLeaser.sprites[spikes.Length + 2].color != color)
        {
            UpdateColor(sLeaser, color);
        }
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = palette.blackColor;
        UpdateColor(sLeaser, color);
    }
    public virtual void UpdateColor(RoomCamera.SpriteLeaser sLeaser, Color col)
    {
        sLeaser.sprites[spikes.Length + 2].color = col;
        for (int i = 0; i < spikes.Length; i++)
        {
            sLeaser.sprites[2 + i].color = col;
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[i]);
        }
    }

    //----------------------------------------------------------------------------------
}