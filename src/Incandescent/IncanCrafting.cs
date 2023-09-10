using System.Runtime.CompilerServices;
using System.Globalization;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using Smoke;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using static AbstractPhysicalObject;

namespace Hailstorm;

public class IncanCrafting
{

    private static Dictionary<(AbstractObjectType, AbstractObjectType), AbstractObjectType> IncanCraftingRecipes = new Dictionary<(AbstractObjectType, AbstractObjectType), AbstractObjectType>();

    public static void Hooks()
    {

        IncanCanCraftWithOnlyOneItem();
        On.Player.GraspsCanBeCrafted += IncanItemCraftingCheck;
        On.Player.SpitUpCraftedObject += IncanItemCreation;

        #region Crafting Recipes

        IncanCraftingRecipes.Add((AbstractObjectType.Spear, null), HailstormEnums.BurnSpear); // Burn Spear
        IncanCraftingRecipes.Add((HailstormEnums.BurnSpear, null), HailstormEnums.BurnSpear); // Refresh Burn Spear heat
        IncanCraftingRecipes.Add((AbstractObjectType.Spear, HailstormEnums.IceCrystal), AbstractObjectType.Spear); // Freeze Spear + Extinguish Fire Spears
        IncanCraftingRecipes.Add((HailstormEnums.BurnSpear, HailstormEnums.IceCrystal), AbstractObjectType.Spear); // Extinguish Burn Spears

        #endregion

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanCanCraftWithOnlyOneItem()
    {
        IL.Player.GrabUpdate += IL =>
        {
            ILCursor c = new(IL);
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass)),
                x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Artificer)),
                x => x.MatchCall(out _)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((bool flag, Player self) => flag || self.SlugCatClass == HailstormSlugcats.Incandescent);
            }
            else
                Plugin.logger.LogError("BurnSpearCrafting IL hook isn't working, so Burn Spears can only be made if you're holding another item. You should report this if you're seeing this.");
        };
    }
    public static bool IncanItemCraftingCheck(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        if (self is not null && CWT.PlayerData.TryGetValue(self, out HailstormSlugcats hS) && hS.isIncan)// && hS.readyToMoveOn)
        {
            return AreItemsUsableForCrafting(self, hS) is not null;
        }
        return orig(self);
    }
    public static AbstractObjectType AreItemsUsableForCrafting(Player self, HailstormSlugcats hS)
    {
        if (hS.isIncan)
        {
            foreach (Creature.Grasp grasp in self.grasps)
            {
                if (grasp?.grabbed is null) continue;

                if (grasp?.grabbed is IPlayerEdible food && food.Edible)
                {
                    return null;
                }
                if (grasp?.grabbed is Spear spr && (spr.bugSpear || (spr.abstractSpear is AbstractBurnSpear bs && bs.heat > 0)))
                {
                    foreach (Creature.Grasp grasp2 in self.grasps)
                    {
                        if (grasp2?.grabbed is IceCrystal)
                        {
                            return AbstractObjectType.Spear;
                        }
                    }
                }
            }
            if (self.grasps[0]?.grabbed is Spear spr1 && !(spr1.bugSpear || (spr1.abstractSpear is AbstractBurnSpear bs1 && bs1.heat > 0)))
            {
                return HailstormEnums.BurnSpear;
            }
            if (self.grasps[0]?.grabbed is null && self.grasps[1]?.grabbed is Spear spr2 && !(spr2.bugSpear || (spr2.abstractSpear is AbstractBurnSpear bs2 && bs2.heat > 0)) && self.objectInStomach is null)
            {
                return HailstormEnums.BurnSpear;
            }
        }
        return null;
    }

    //----------------------------------------------------------------------------------

    public static AbstractObjectType IncanRecipes(AbstractPhysicalObject grasp1, AbstractPhysicalObject grasp2)
    {
        if (grasp1?.type == AbstractObjectType.Spear && grasp2?.type is not null && grasp2.type != HailstormEnums.IceCrystal)
        {
            return HailstormEnums.BurnSpear;
        }
        if (IncanCraftingRecipes.TryGetValue((grasp1?.type, grasp2?.type), out AbstractObjectType result) || IncanCraftingRecipes.TryGetValue((grasp2?.type, grasp1?.type), out result))
        {
            return result;
        }
        return null;
    }
    public static void IncanItemCreation(On.Player.orig_SpitUpCraftedObject orig, Player self)
    {
        if (self?.room is null || !CWT.PlayerData.TryGetValue(self, out HailstormSlugcats hS) || !hS.isIncan)// || !hS.readyToMoveOn)
        {
            orig(self);
            return;
        }

        int spearGrasp = -1;
        int otherGrasp = -1;
        for (int g = 0; g < self.grasps.Length; g++)
        {
            if (self.grasps[g]?.grabbed is not null && self.grasps[g].grabbed is Spear)
            {
                spearGrasp = g;
                break;
            }
        }
        if (spearGrasp == -1)
        {
            orig(self);
            return;
        }
        for (int g = 0; g < self.grasps.Length; g++)
        {
            if (self.grasps[g]?.grabbed is not null && self.grasps[g].grabbed is not Spear)
            {
                otherGrasp = g;
                break;
            }
        }

        AbstractPhysicalObject absObjGrasp1 =
            (self.grasps[0]?.grabbed?.abstractPhysicalObject is not null) ? self.grasps[0].grabbed.abstractPhysicalObject : null;
        AbstractPhysicalObject absObjGrasp2 =
            (self.grasps[1]?.grabbed?.abstractPhysicalObject is not null) ? self.grasps[1].grabbed.abstractPhysicalObject : null;

        AbstractObjectType resultItem = IncanRecipes(absObjGrasp1, absObjGrasp2);

        if (AreItemsUsableForCrafting(self, hS) == HailstormEnums.BurnSpear)
        {
            MakeBurnSpear(self, hS, spearGrasp);
        }
        else if (resultItem == AbstractObjectType.Spear)
        {
            SpearCrafting(self, hS, spearGrasp, otherGrasp);
        }
    }

    //-----------------------------------------

    public static void MakeBurnSpear(Player self, HailstormSlugcats hS, int spearGrasp)
    {
        if (spearGrasp == -1) return;

        AbstractSpear absSpr =
            (self.grasps[spearGrasp]?.grabbed is not null) ? self.grasps[spearGrasp].grabbed.abstractPhysicalObject as AbstractSpear : null;

        if (absSpr is null || absSpr.hue != 0f || (absSpr is AbstractBurnSpear brnSpr && brnSpr.heat > 0))
        {
            CraftingFailed(self, 20, hS);
            Debug.Log("[Hailstorm] Spear-crafting attempt didn't work out; this ain't a valid spear type.");
            return;
        }

        if (absSpr.electricCharge > 0 && absSpr.realizedObject is not null && absSpr.realizedObject is ElectricSpear)
        {
            self.room.AddObject(new ZapCoil.ZapFlash(self.firstChunk.pos, 4f));
            self.room.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 1, Random.Range(0.5f, 0.75f));
            self.room.PlaySound(SoundID.Firecracker_Burn, self.bodyChunks[1]);
            for (int s = Random.Range(6, 8); s >= 0; s--)
            {
                self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * Random.Range(5f, 10f), Random.value < 0.5 ? hS.FireColor : hS.FireColorBase, null, 30, 50));
            }
            hS.successfulCraft = true;
            self.Stun(20 * absSpr.electricCharge);
        }
        else if (self.FoodInStomach >= 2 && self.Hypothermia < 1.2f && self.playerState.permanentDamageTracking < 0.8f)
        {
            if (absSpr.realizedObject is not null && absSpr.realizedObject is ExplosiveSpear expSpr)
            {
                self.room.PlaySound(SoundID.Firecracker_Burn, self.bodyChunks[1]);
                for (int s = Random.Range(6, 8); s >= 0; s--)
                {
                    self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * Random.Range(5f, 10f), Random.value < 0.5 ? hS.FireColor : hS.FireColorBase, null, 30, 50));
                }
                hS.successfulCraft = true;
                expSpr.Explode();
            }
            else if (absSpr is AbstractBurnSpear bs)
            {
                bs.heat = 1;
                bs.spearColor = hS.FireColorBase;
                bs.fireFadeColor = self.ShortCutColor();
            }
            else
            {
                self.room.abstractRoom.RemoveEntity(self?.grasps[spearGrasp]?.grabbed?.abstractPhysicalObject);
                if (self.grasps[spearGrasp]?.grabbed is not null)
                {
                    self.grasps[spearGrasp].grabbed.RemoveFromRoom();
                }
                self.ReleaseGrasp(spearGrasp);

                absSpr = new AbstractBurnSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, 1, hS.FireColorBase, self.ShortCutColor())
                {
                    explosive = false,
                    electric = false,
                    electricCharge = 0,
                    hue = 0
                };
                absSpr.RealizeInRoom();
                self.SlugcatGrab(absSpr.realizedObject, spearGrasp);
            }

            self.room.PlaySound(SoundID.Firecracker_Burn, self.bodyChunks[1], false, 1, Random.Range(1.25f, 1.75f));
            for (int s = Random.Range(6, 8); s >= 0; s--)
            {
                self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * Random.Range(5f, 10f), Random.value < 0.5 ? hS.FireColor : hS.FireColorBase, null, 30, 50));
            }
            hS.successfulCraft = true;

            self.SubtractFood(2);
            self.Hypothermia += 0.8f;
            self.playerState.permanentDamageTracking += 0.2f;
            self.exhausted = true;
            self.airInLungs *= 0.2f;
            self.saintWeakness += 160;
            self.aerobicLevel = 1f;
            Debug.Log("Incan made a new Burn Spear!");
        }
        else
        {
            self.room.PlaySound(SoundID.Firecracker_Burn, self.firstChunk, false, 0.85f, Random.Range(0.3f, 0.5f));

            self.SaintStagger(250);
            Debug.Log("Incan didn't have the resources required to make this spear...");
        }
    }
    public static void SpearCrafting(Player self, HailstormSlugcats hS, int spearGrasp, int otherGrasp)
    {
        if (spearGrasp == -1) return;

        AbstractSpear absSpr =
            (self.grasps[spearGrasp]?.grabbed is not null) ? self.grasps[spearGrasp].grabbed.abstractPhysicalObject as AbstractSpear : null;

        AbstractPhysicalObject otherObj =
            otherGrasp == -1 ? null :
            (self.grasps[otherGrasp]?.grabbed is not null) ? self.grasps[otherGrasp].grabbed.abstractPhysicalObject : null;

        if (absSpr is null)
        {
            CraftingFailed(self, 20, hS);
            Debug.Log("[Hailstorm] Spear-crafting attempt didn't work out; this ain't a valid spear type.");
            return;
        }

        if (otherObj?.realizedObject is not null && otherObj.realizedObject is IceCrystal ice)
        {
            if (absSpr.hue != 0f || (absSpr is AbstractBurnSpear brnSpr1 && brnSpr1.heat > 0)) // Turns fiery spears into normal ones.
            {
                self.room.PlaySound(SoundID.Flare_Bomb_Burn, self.firstChunk.pos, 0.9f, 1.2f);
                for (int o = 6; o >= 0; o--)
                {
                    self.room.AddObject(new HailstormSnowflake(self.firstChunk.pos, Custom.RNV() * Random.Range(3f, 5f), ice.color, ice.color2));
                    self.room.AddObject(new WaterDrip(self.firstChunk.pos + (Custom.RNV() * Random.Range(0f, ice.firstChunk.rad)), default, true));
                }

                self.room.abstractRoom.RemoveEntity(absSpr);
                self.room.abstractRoom.RemoveEntity(otherObj);
                if (self.grasps[spearGrasp]?.grabbed is not null)
                {
                    self.grasps[spearGrasp].grabbed.RemoveFromRoom();
                }
                if (self.grasps[otherGrasp]?.grabbed is not null)
                {
                    self.grasps[otherGrasp].grabbed.RemoveFromRoom();
                }
                self.ReleaseGrasp(spearGrasp);
                self.ReleaseGrasp(otherGrasp);

                absSpr = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false);
                absSpr.RealizeInRoom();
                self.SlugcatGrab(absSpr.realizedObject, spearGrasp);
                hS.successfulCraft = true;
                Debug.Log("Incan extinguished a fiery spear!");
            }
            else
            {
                CraftingFailed(self, 20, hS);
                Debug.Log("[Hailstorm] Spear-crafting attempt didn't work out; this ain't a valid spear type.");
            }
        }
        else if (absSpr is AbstractBurnSpear && AreItemsUsableForCrafting(self, hS) == HailstormEnums.BurnSpear)
        {
            MakeBurnSpear(self, hS, spearGrasp);
        }
        else
        {
            CraftingFailed(self, 20, hS);
            Debug.Log("[Hailstorm] Spear-crafting attempt didn't work out; this ain't a valid spear type.");
        }
    }

    public static void CraftingFailed(Player self, int stunTime, HailstormSlugcats hS)
    {
        self.Stun((int)(stunTime * (1 + self.playerState.permanentDamageTracking)));
        self.room.PlaySound(SoundID.Firecracker_Disintegrate, self.firstChunk, false, 0.8f, Random.Range(0.5f, 0.6f));
    }

}//--------------------------------------------------------------------------------------------------------------------------------------------------------------------