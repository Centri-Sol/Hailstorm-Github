using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Runtime.ConstrainedExecution;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using System.Drawing;
using SlugBase.Features;

namespace Hailstorm;

internal class ObjectChanges
{
    public static void Hooks()
    {
        On.Player.CanBeSwallowed += NewItemsSwallowableSettings;
        JollyCoopFoodFix();

        // Bubblegrass
        On.BubbleGrass.Update += HeatyGrass;

        // Slime Mold
        On.SlimeMold.ctor += BIGMold;
        On.SaveState.AbstractPhysicalObjectFromString += AbstractSlimeMoldData;

        // Spears
        On.Spear.ctor += BurnSpearCtor;
        On.Spear.Update += SpearUpdate;

        // Electric Spears
        On.AbstractSpear.ctor_World_Spear_WorldCoordinate_EntityID_bool_bool += ElectriSpearExtraCharge;
        On.MoreSlugcats.ElectricSpear.Recharge += ElectriSpearExtraRecharge;
        ElectriSpearElectrocutionReplacement();
        On.Spear.LodgeInCreature += WeakerElectricSpearStun;

        // Fire Spears
        On.Spear.InitiateSprites += IncandescentSpearColor;
        On.Spear.ApplyPalette += IncandescentSpearPalette;
        On.Spear.DrawSprites += IncandescentSpearVisuals;
        On.Spear.HitSomething += ElementalDamage;

        // Jellyfish
        On.JellyFish.Collide += WeakerJellyfishStun;

        // Flashbangs
        On.FlareBomb.Update += FlashbangLuminescipedeStun;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsRWGIncan(RainWorldGame RWG)
    {
        return (RWG is not null && RWG.IsStorySession && RWG.StoryCharacter == HailstormSlugcats.Incandescent);
    }

    //--------------------------------------------

    private static bool NewItemsSwallowableSettings(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (obj is IceCrystal) return true;

        //if (obj is BezanNut) return true;

        //if (obj is LargeBezanNut) return false;

        return orig(self, obj);
    }

    public static void JollyCoopFoodFix()
    {
        IL.Player.AddQuarterFood += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Player self) =>
            {
                return IsRWGIncan(self?.room?.game) && AddQuarterBip(self);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
    }
    public static bool AddQuarterBip(Player self)
    {
        if (self.redsIllness is not null)
        {
            self.redsIllness.AddQuarterFood();
        }
        else if (self.FoodInStomach < self.MaxFoodInStomach)
        {
            PlayerState playerState = self.playerState;
            if (ModManager.CoopAvailable && self.abstractCreature.world.game.IsStorySession && self.playerState.playerNumber != 0 && !self.isNPC)
            {
                for (int p = 0; p < self.abstractCreature.world.game.Players.Count; p++)
                {
                    PlayerState plr = self.abstractCreature.world.game.Players[p].state as PlayerState;
                    if (plr.playerNumber != 0) continue;

                    playerState = plr;
                    playerState.quarterFoodPoints++;
                    break;
                }
            }
            else
            {
                playerState.quarterFoodPoints++;
            }

            if (playerState.quarterFoodPoints > 3)
            {
                playerState.quarterFoodPoints -= 4;
                self.AddFood(1);
            }
        }
        return true;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void HeatyGrass(On.BubbleGrass.orig_Update orig, BubbleGrass grs, bool eu)
    {
        orig(grs, eu);
        if (IsRWGIncan(grs?.room?.game) && grs.firstChunk.submersion > 0.9f && grs.room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
        {
            bool flag = true;
            if (grs.grabbedBy.Count > 0 && grs.grabbedBy[0].grabber is Player plr && plr.animation == Player.AnimationIndex.SurfaceSwim && plr.airInLungs > 0.5f)
            {
                flag = false;
            }
            if (flag && Random.value < Mathf.InverseLerp(0f, 0.3f, grs.oxygen))
            {
                Bubble bubble = new(grs.firstChunk.pos + Custom.RNV() * Random.value * 4f, Custom.RNV() * Mathf.Lerp(6f, 16f, Random.value) * Mathf.InverseLerp(0f, 0.45f, grs.oxygen), bottomBubble: false, fakeWaterBubble: false);
                grs.room.AddObject(bubble);
                bubble.age = 600 - Random.Range(20, Random.Range(30, 80));
                for (int i = 0; i < grs.room.abstractRoom.creatures.Count; i++)
                {
                    Creature ctr = grs.room.abstractRoom.creatures[i].realizedCreature;
                    if (ctr is null)
                    {
                        continue;
                    }
                    if (Custom.DistLess(grs.firstChunk.pos, ctr.mainBodyChunk.pos, 40f))
                    {
                        ctr.HypothermiaExposure = Mathf.Min(0.3f, ctr.HypothermiaExposure);
                        if (ctr.Hypothermia >= 0.0005f)
                        {
                            ctr.Hypothermia -= 0.0005f;
                        }
                        if (ctr is Player self && CWT.PlayerData.TryGetValue(self, out HailstormSlugcats hS) && hS.isIncan)
                        {
                            hS.wetness -= self.Submersion >= 1 ? 6 : 3;
                        }
                    }
                }
            }
        }
    }

    //-----------------------------------------

    public static void BIGMold(On.SlimeMold.orig_ctor orig, SlimeMold slm, AbstractPhysicalObject absObj)
    {
        orig(slm, absObj);
        if (IsRWGIncan(absObj?.world?.game) && slm?.AbstrConsumable is not null)
        {
            Debug.Log("Mold size check");
            if (slm.AbstrConsumable is AbstractSlimeMold ASM)
            {
                Debug.Log("Is ASM");
                slm.big = ASM.big;
                if (ASM.big) Debug.Log("ENLARGE");
            }
            else if (!slm.AbstrConsumable.isConsumed && absObj.Room.realizedRoom is not null)
            {
                Debug.Log("heeblebeeble");
                for (int s = 0; s < absObj.Room.realizedRoom.roomSettings.placedObjects.Count; s++)
                {
                    if (absObj.Room.realizedRoom.roomSettings.placedObjects[s].type == PlacedObject.Type.CosmeticSlimeMold2 && Custom.DistLess(absObj.Room.realizedRoom.MiddleOfTile(absObj.pos), absObj.Room.realizedRoom.roomSettings.placedObjects[s].pos, 50))
                    {
                        Debug.Log("NEAR BIG MOLD PATCH; ENLARGE");
                        slm.big = true;
                        slm.AbstrConsumable.unrecognizedAttributes = new string[1] { "big" };
                        break;
                    }
                }
            }
            /*
            else
            {
                int othermolds = 0;
                foreach (AbstractWorldEntity AWE in absObj.Room.entities)
                {
                    if (AWE != absObj &&
                        AWE is AbstractConsumable AC &&
                        AC.realizedObject is not null &&
                        AC.realizedObject is SlimeMold SM &&
                        !SM.JellyfishMode &&
                        Custom.DistLess(absObj.Room.realizedRoom.MiddleOfTile(absObj.pos), absObj.Room.realizedRoom.MiddleOfTile(AC.pos), 25))
                    {
                        othermolds += SM.big? 2 : 1;
                        if (othermolds >= 5)
                        {
                            slm.Destroy();
                            return;
                        }
                    }
                }
                foreach (AbstractCreature ctr in absObj.Room.creatures)
                {
                    if (Random.value < 0.5f - othermolds/10f &&
                        ctr.realizedCreature is not null &&
                        ctr.realizedCreature is BigJellyFish &&
                        Custom.DistLess(absObj.Room.realizedRoom.MiddleOfTile(absObj.pos), absObj.Room.realizedRoom.MiddleOfTile(ctr.pos), 25))
                    {
                        Debug.Log("NEAR JELLY; ENLARGE");
                        slm.big = true;
                        slm.AbstrConsumable.unrecognizedAttributes = new string[1] { "big" };
                        break;
                    }
                }
            }
            */
        }
    }

    public static AbstractPhysicalObject AbstractSlimeMoldData(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        if (IsRWGIncan(world?.game))
        {
            try
            {
                string[] array = Regex.Split(objString, "<oA>");
                EntityID iD = EntityID.FromString(array[0]);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = new(array[1]);
                WorldCoordinate pos = WorldCoordinate.FromString(array[2]);
                AbstractPhysicalObject abstractPhysicalObject = null;
                if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SlimeMold && array.Length >= 6)
                {
                    abstractPhysicalObject = new AbstractSlimeMold(world, abstractObjectType, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, array[5] == "1" || array[5] == "big");
                    abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
                    return abstractPhysicalObject;
                }
            }
            catch (Exception ex)
            {
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("[EXCEPTION - HAILSTORM] AbstractPhysicalObjectFromString: " + objString + " -- " + ex.Message + " -- " + ex.StackTrace);
                }
                return null;
            }
        }
        return orig(world, objString);
    }

    //-----------------------------------------

    #region Spears & Jellies
    public static void BurnSpearCtor(On.Spear.orig_ctor orig, Spear spr, AbstractPhysicalObject absObj, World world)
    {
        orig(spr, absObj, world);
        if (absObj is AbstractBurnSpear absBrnSpr)
        {
            if (absBrnSpr.spearColor == Color.clear)
            {
                absBrnSpr.spearColor = Custom.HSL2RGB(Random.value < 0.02f ? Random.value : Random.Range(0f, 0.11f), 1, Random.Range(0.5f, 0.65f));
                Vector3 col2 = Custom.RGB2HSL(absBrnSpr.spearColor);
                col2.x -= col2.x + Random.Range(-0.15f, 0.15f);
                if (col2.x > 1) col2.x -= 1f;
                if (col2.x < 0) col2.x += 1f;
                absBrnSpr.fireFadeColor = Custom.HSL2RGB(col2.x, col2.y, col2.z - (Random.value < 0.2f? -0.2f : 0.2f));
            }
        }
    }
    public static void SpearUpdate(On.Spear.orig_Update orig, Spear spr, bool eu)
    {
        orig(spr, eu);

        if (spr.abstractSpear is AbstractBurnSpear absBrnSpr)
        {
            if (absBrnSpr.heat > 0)
            {
                absBrnSpr.spearTipPos = spr.firstChunk.pos + (spr.rotation * spr.firstChunk.rad * 4f);

                if (absBrnSpr.chill != 0.001f)
                {
                    absBrnSpr.chill = 0.001f + (0.004f * spr.Submersion);
                }
                if (spr.room is not null)
                {
                    if (spr.Submersion > 0 && spr.room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
                    {
                        absBrnSpr.chill += 0.005f * spr.Submersion;
                    }

                    foreach (UpdatableAndDeletable upDel in spr.room.updateList)
                    {
                        if (upDel is null) return;

                        if (upDel is FreezerMist mist && Custom.DistLess(absBrnSpr.spearTipPos, mist.pos, mist.rad + 100f)) absBrnSpr.chill += 0.005f;
                        else if (upDel is Lizard coldLizzy && HailstormLizards.LizardData.TryGetValue(coldLizzy, out LizardInfo lI) && lI.freezerAura is not null)
                        {
                            absBrnSpr.chill += 0.04f * Mathf.InverseLerp(lI.freezerAura.rad, lI.freezerAura.rad - 120f, Custom.Dist(absBrnSpr.spearTipPos, coldLizzy.DangerPos));
                        }
                    }
                }
                absBrnSpr.heat -= absBrnSpr.chill / 40f;

                if (absBrnSpr.emberTimer > 0 && spr.mode != Weapon.Mode.Thrown)
                {
                    absBrnSpr.emberTimer -= Random.value < 0.75f ? 1 : 2;
                }
                else if (absBrnSpr.emberTimer <= 0 || spr.mode == Weapon.Mode.Thrown)
                {
                    Vector2 emberPos =
                        spr.room.GetTile(spr.firstChunk.pos + (spr.rotation * spr.firstChunk.rad * 4f)).Solid ? spr.firstChunk.pos : absBrnSpr.spearTipPos;
                    spr.room.AddObject(new EmberSprite(emberPos, absBrnSpr.currentColor, Random.Range(0.8f, 1.4f)));
                    absBrnSpr.emberTimer = Random.Range(6, 8);
                }
            }

            if (absBrnSpr.heat > 0 && !absBrnSpr.burning)
            {
                absBrnSpr.burning = true;
            }
            else if (absBrnSpr.heat <= 0 && absBrnSpr.burning)
            {
                spr.room.PlaySound(SoundID.Firecracker_Burn, spr.firstChunk.pos, 1, 1.75f);
                absBrnSpr.burning = false;
            }

            for (int i = 0; i < absBrnSpr.flicker.GetLength(0); i++)
            {
                absBrnSpr.flicker[i, 1] = absBrnSpr.flicker[i, 0];
                absBrnSpr.flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * (Random.value < 0.5f ? -1f : 1f);
                absBrnSpr.flicker[i, 0] = Custom.LerpAndTick(absBrnSpr.flicker[i, 0], absBrnSpr.flicker[i, 2], 0.05f, 1f / 30f);
                if (Random.value < 0.2f)
                {
                    absBrnSpr.flicker[i, 2] = 1f + Mathf.Pow(Random.value, 3f) * 0.2f * (Random.value < 0.5f ? -1f : 1f);
                }
                absBrnSpr.flicker[i, 2] = Mathf.Lerp(absBrnSpr.flicker[i, 2], 1f, 0.01f);
            }

            if (absBrnSpr.glow is null && absBrnSpr.burning)
            {
                absBrnSpr.glow = new LightSource(absBrnSpr.spearTipPos, false, absBrnSpr.currentColor, spr, true);
                absBrnSpr.glow.requireUpKeep = true;
                absBrnSpr.glow.stayAlive = true;
                absBrnSpr.glow.affectedByPaletteDarkness = 0;
                absBrnSpr.glow.setAlpha = new float?(1);
                spr.room.AddObject(absBrnSpr.glow);
            }
            else if (absBrnSpr.glow is not null)
            {
                absBrnSpr.glow.stayAlive = true;
                absBrnSpr.glow.setPos = new Vector2?(absBrnSpr.spearTipPos);
                absBrnSpr.glow.setRad = new float?(Mathf.Lerp(80, 200, absBrnSpr.heat) * absBrnSpr.flicker[0, 0]);
                absBrnSpr.glow.color = absBrnSpr.currentColor;
                if (absBrnSpr.glow.slatedForDeletetion || absBrnSpr.glow.room != spr.room || !absBrnSpr.burning)
                {
                    absBrnSpr.glow = null;
                }
            }

        }


        if (spr.mode == Weapon.Mode.StuckInCreature && spr.stuckInObject is not null && spr.stuckInObject is Lizard liz && liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            spr.deerCounter++;
            if (spr.deerCounter > 40)
            {
                float ang = 0f;
                for (int i = 0; i < spr.abstractPhysicalObject.stuckObjects.Count; i++)
                {
                    if (spr.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick aS && aS.Spear == spr.abstractPhysicalObject)
                    {
                        ang = aS.angle;
                        break;
                    }
                }
                spr.ChangeMode(Weapon.Mode.Free);
                if (spr.room.BeingViewed)
                {
                    spr.firstChunk.vel = Custom.DegToVec(ang) * -10f;
                    spr.SetRandomSpin();
                    for (int j = 0; j < 4; j++)
                    {
                        spr.room.AddObject(new WaterDrip(spr.firstChunk.pos, spr.firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * spr.firstChunk.vel.magnitude * Random.value * 0.5f, waterColor: false));
                    }
                }
                spr.deerCounter = 0;
            }
        }
    }

    //-----------------------------------------

    // Electric Spears
    public static void ElectriSpearExtraCharge(On.AbstractSpear.orig_ctor_World_Spear_WorldCoordinate_EntityID_bool_bool orig, AbstractSpear absSpr, World world, Spear spr, WorldCoordinate pos, EntityID ID, bool explosive, bool electric)
    {
        orig(absSpr, world, spr, pos, ID, explosive, electric);
        if (IsRWGIncan(world?.game) && electric)
        {
            absSpr.electricCharge = 5;
        }
    }
    public static void ElectriSpearExtraRecharge(On.MoreSlugcats.ElectricSpear.orig_Recharge orig, ElectricSpear eSpr)
    {
        if (IsRWGIncan(eSpr?.room?.game) && eSpr.abstractSpear.electricCharge == 0)
        {
            eSpr.abstractSpear.electricCharge = 5;
            eSpr.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, eSpr.firstChunk.pos);
            eSpr.room.AddObject(new Explosion.ExplosionLight(eSpr.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
            eSpr.Spark();
            eSpr.Zap();
            eSpr.room.AddObject(new ZapCoil.ZapFlash(eSpr.sparkPoint, 25f));
        }
        orig(eSpr);
    }
    public static void ElectriSpearElectrocutionReplacement()
    {
        IL.MoreSlugcats.ElectricSpear.Electrocute += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((ElectricSpear eSpr, PhysicalObject obj) =>
            {
                return IsRWGIncan(eSpr?.room?.game);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
    }
    public static void WeakerElectricSpearStun(On.Spear.orig_LodgeInCreature orig, Spear spr, SharedPhysics.CollisionResult result, bool eu)
    {
        if (IsRWGIncan(spr?.room?.game) && spr is ElectricSpear eSpr && result.obj is not null && result.obj is Creature target)
        {
            BodyChunk hitChunk = result.chunk;
            bool ElectricTarget = eSpr.CheckElectricCreature(target);
            if (ElectricTarget && eSpr.abstractSpear.electricCharge == 0)
            {
                eSpr.Recharge();
            }
            else if (result.chunk is not null && eSpr.abstractSpear.electricCharge > 0)
            {
                if (target is not BigEel && !ElectricTarget)
                {
                    target.Violence(eSpr.firstChunk, Custom.DirVec(eSpr.firstChunk.pos, hitChunk.pos) * 5f, hitChunk, null, Creature.DamageType.Electric, ElectricDamage(target), ElectricStun(eSpr, target));
                    eSpr.room.AddObject(new CreatureSpasmer(target, allowDead: false, target.stun));
                }
                bool WaterShockFromOutOfWater = false;
                if (eSpr.Submersion <= 0.5f && target.Submersion > 0.5f)
                {
                    eSpr.room.AddObject(new UnderwaterShock(eSpr.room, null, hitChunk.pos, 10, 800f, 2f, eSpr.thrownBy, new Color(0.8f, 0.8f, 1f)));
                    WaterShockFromOutOfWater = true;
                }
                eSpr.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, eSpr.firstChunk.pos);
                eSpr.room.AddObject(new Explosion.ExplosionLight(eSpr.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
                for (int i = 0; i < 15; i++)
                {
                    Vector2 val = Custom.DegToVec(360f * Random.value);
                    eSpr.room.AddObject(new MouseSpark(eSpr.firstChunk.pos + val * 9f, eSpr.firstChunk.vel + val * 36f * Random.value, 20f, new Color(0.7f, 1f, 1f)));
                }
                if (!ElectricTarget)
                {
                    eSpr.abstractSpear.electricCharge--;
                }
                if (target is Centipede cnt && cnt.size >= 0.9f)
                {
                    eSpr.ExplosiveShortCircuit();
                }
                else if (eSpr.abstractSpear.electricCharge == 0 || WaterShockFromOutOfWater)
                {
                    eSpr.ShortCircuit();
                }
            }
        }
        orig(spr, result, eu);
    }
    public static float ElectricDamage(Creature target)
    {
        return
            target.Template.baseStunResistance >= 6f? 1 :
            target.Template.baseStunResistance >= 4f? 0.75f :
            target.Template.baseStunResistance >= 2f? 0.50f : 0.25f;
    }
    public static float ElectricStun(PhysicalObject source, Creature target)
    {
        float stun = Mathf.Min(240, 140 * Mathf.Lerp(target.Template.baseStunResistance / 2f, 1, source is JellyFish ? 0.5f : 0.75f));
        if (target.State is HealthState HS)
        {
            stun *= Mathf.Lerp(source is JellyFish ? 2f : 1.5f, 1f, HS.ClampedHealth);
        }


        if (target.Template.type == CreatureTemplate.Type.YellowLizard)
        {
            stun /= 2;
        }
        else
        if (target.Template.type == HailstormEnums.IcyBlue ||
            target.Template.type == HailstormEnums.Freezer)
        {
            stun *= 1.5f;
        }
        else
        if (target is Player plr)
        {
            if (plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                plr.SaintStagger(520);
            }
            else
            if (plr.SlugCatClass == SlugcatStats.Name.Yellow ||
                plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                stun *= 0.85f;
            }
            else
            if (plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
                plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                stun *= 1.25f;
            }
        }
        return stun;
    }

    // Burn Spears
    public static void IncandescentSpearColor(On.Spear.orig_InitiateSprites orig, Spear spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(spr, sLeaser, rCam);
        if (spr is not null && spr.abstractSpear is AbstractBurnSpear)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[1] = new FSprite("FireBugSpear");
            sLeaser.sprites[0] = new FSprite("FireBugSpearColor");
            spr.AddToContainer(sLeaser, rCam, null);
        }
    }
    public static void IncandescentSpearPalette(On.Spear.orig_ApplyPalette orig, Spear spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(spr, sLeaser, rCam, palette);
        if (spr is not null && spr.abstractSpear is AbstractBurnSpear absBrnSpr)
        {
            sLeaser.sprites[1].color = spr.color;
            absBrnSpr.currentColor = absBrnSpr.heat > 0 ?
                Color.Lerp(Color.Lerp(spr.color, absBrnSpr.fireFadeColor, Mathf.Lerp(0.5f, 1, absBrnSpr.heat)), absBrnSpr.spearColor, absBrnSpr.heat) : spr.color;
            sLeaser.sprites[0].color = absBrnSpr.currentColor;
        }
    }
    public static void IncandescentSpearVisuals(On.Spear.orig_DrawSprites orig, Spear spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(spr, sLeaser, rCam, timeStacker, camPos);
        if (spr is null || spr.abstractSpear is not AbstractBurnSpear absBrnSpr) return;

        absBrnSpr.currentColor = absBrnSpr.heat > 0 ?
            Color.Lerp(Color.Lerp(spr.color, absBrnSpr.fireFadeColor, Mathf.Lerp(0.5f, 0.95f, absBrnSpr.heat)), absBrnSpr.spearColor, absBrnSpr.heat) : spr.color;

        sLeaser.sprites[0].color =
                (spr.blink > 0 && Random.value < 0.5f) ? spr.blinkColor : absBrnSpr.currentColor;

        Vector2 val = Vector2.Lerp(spr.firstChunk.lastPos, spr.firstChunk.pos, timeStacker);
        if (spr.vibrate > 0)
        {
            val += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        }

        sLeaser.sprites[1].x = val.x - camPos.x;
        sLeaser.sprites[1].y = val.y - camPos.y;
        sLeaser.sprites[1].anchorY = Mathf.Lerp(spr.lastPivotAtTip ? 0.85f : 0.5f, spr.pivotAtTip ? 0.85f : 0.5f, timeStacker);
        sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), Vector3.Slerp(spr.lastRotation, spr.rotation, timeStacker));

    }
    public static bool ElementalDamage(On.Spear.orig_HitSomething orig, Spear spr, SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj is not Creature target || !CWT.CreatureData.TryGetValue(target, out CreatureInfo cI))
        {
            return orig(spr, result, eu);
        }

        if (spr.bugSpear || spr.abstractSpear is AbstractBurnSpear)
        {
            if (target is Player && CWT.PlayerData.TryGetValue(target as Player, out HailstormSlugcats hS))
            {
                spr.spearDamageBonus *= hS.HeatDMGmult;
                cI.heatTimer = (int)(20 * hS.HeatDMGmult);
            }
            else if (target.Template.damageRestistances[HailstormEnums.HeatDamage.index, 0] is float heatRes && heatRes != 1)
            {
                spr.spearDamageBonus /= heatRes;
                cI.heatTimer = (int)(20 / heatRes);
            }
        }
        else if (IsRWGIncan(spr?.room?.game) && spr is ElectricSpear)
        {
            spr.spearDamageBonus *= 0.75f;
        }

        if (spr.abstractSpear is AbstractBurnSpear brnSpr)
        {
            spr.spearDamageBonus *= 0.7f;

            if (brnSpr.heat > 0)
            {
                brnSpr.heat -= 0.04f;
                if (spr.room is not null)
                {
                    spr.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, spr.firstChunk.pos, 1f, Random.Range(1.75f, 2f));
                    spr.room.AddObject(new Explosion.ExplosionLight(spr.firstChunk.pos, 280f, 1f, 7, brnSpr.spearColor));
                    spr.room.AddObject(new FireSpikes(spr.room, spr.firstChunk.pos, 14, 15f, 9f, 5f, 90f, brnSpr.spearColor, brnSpr.fireFadeColor));
                }

                if (target.SpearStick(spr, spr.spearDamageBonus, result.chunk, result.onAppendagePos, spr.firstChunk.vel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out AbsCtrInfo aI))
                {
                    aI.AddBurn(brnSpr, target, result.chunk?.index, 600, brnSpr.spearColor, brnSpr.fireFadeColor);
                }
            }
        }

        return orig(spr, result, eu);
    }


    //-----------------------------------------

    public static void WeakerJellyfishStun(On.JellyFish.orig_Collide orig, JellyFish jelly, PhysicalObject obj, int myChunk, int otherChunk)
    {
        if (IsRWGIncan(jelly?.room?.game) && obj is Creature target && target != jelly.thrownBy && jelly.Electric)
        {
            if (target is not BigEel && target is not Centipede && target is not BigJellyFish && target is not Inspector)
            {
                target.Violence(jelly.firstChunk, Custom.DirVec(jelly.firstChunk.pos, target.bodyChunks[otherChunk].pos) * 5f, target.bodyChunks[otherChunk], null, Creature.DamageType.Electric, 0.05f, ElectricStun(jelly, target));
                target.room.AddObject(new CreatureSpasmer(target, allowDead: false, target.stun));
            }
            target.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, jelly.firstChunk.pos);
            jelly.room.AddObject(new Explosion.ExplosionLight(jelly.firstChunk.pos, 200f, 1f, 4, new Color(0.7f, 1f, 1f)));
            if (jelly.electricCounter > 5)
            {
                for (int i = 0; i < 15; i++)
                {
                    Vector2 val = Custom.DegToVec(360f * Random.value);
                    jelly.room.AddObject(new MouseSpark(jelly.firstChunk.pos + val * 9f, jelly.firstChunk.vel + val * 36f * Random.value, 20f, new Color(0.7f, 1f, 1f)));
                }
            }
            jelly.electricCounter = 0;
        }
        orig(jelly, obj, myChunk, otherChunk);
    }
    #endregion

    //-----------------------------------------

    public static void FlashbangLuminescipedeStun(On.FlareBomb.orig_Update orig, FlareBomb flr, bool eu)
    {
        orig(flr, eu);
        if (flr is null || flr.burning <= 0f) return;

        for (int i = 0; i < flr.room.abstractRoom.creatures.Count; i++)
        {
            AbstractCreature absCtr = flr.room.abstractRoom.creatures[i];
            if (absCtr.realizedCreature is null ||
                (!Custom.DistLess(flr.firstChunk.pos, absCtr.realizedCreature.mainBodyChunk.pos, flr.LightIntensity * 800f) && (!Custom.DistLess(flr.firstChunk.pos, absCtr.realizedCreature.mainBodyChunk.pos, flr.LightIntensity * 2000f) || !flr.room.VisualContact(flr.firstChunk.pos, absCtr.realizedCreature.mainBodyChunk.pos))))
            {
                continue;
            }
            if (absCtr.realizedCreature is Luminescipede lmn && !lmn.overloaded)
            {
                if (!lmn.charged || (lmn.dead && lmn.juice < 1))
                {
                    lmn.juice += 0.01f;
                }
                else if (!lmn.dead)
                {
                    absCtr.realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value;
                    absCtr.realizedCreature.Stun((int)Mathf.Lerp(0, 240, lmn.juice));
                    lmn.overloaded = true;
                    lmn.flicker = false;
                }
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}