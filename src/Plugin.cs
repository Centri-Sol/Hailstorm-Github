using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using DevInterface;
using MoreSlugcats;
using Color = UnityEngine.Color;

namespace Hailstorm;


[BepInPlugin(MOD_ID, "The Incandescent", "0.3.0")]
public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "theincandescent";

    [AllowNull] internal static ManualLogSource logger;

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------
    
    // Add hooks.
    public void OnEnable()
    {
        logger = Logger;
        On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
        On.RainWorld.PostModsInit += ReorderUnlocks;

        On.RainWorld.OnModsDisabled += DisableCheck;

        Content.Register(new InfantAquapedeCritob());
        SnowcuttleTemplate.RegisterSnowcuttles();
        Content.Register(new RavenCritob());
        Content.Register(new FreezerCritob());
        Content.Register(new IcyBlueCritob());
        Content.Register(new PeachSpiderCritob());
        Content.Register(new CyanwingCritob());
        Content.Register(new GorditoGreenieCritob());
        Content.Register(new ChillipedeCritob());
        Content.Register(new LuminescipedeCritob());

        Content.Register(new IceChunkFisob());
        Content.Register(new FreezerCrystalFisob());
        Content.Register(new BurnSpearFisob());

    }
    public void OnDisable() => logger = default;

    // Lets you load stuff like sprites or sounds. A bunch of sprite atlases, in my case.
    // I also use this to call for hooks in other files.
    private void LoadResources(RainWorld rainWorld)
    {
        try
        {
            Futile.atlasManager.LoadAtlas("atlases/icons");
            Futile.atlasManager.LoadAtlas("atlases/coloredIcons");

            Futile.atlasManager.LoadAtlas("atlases/incandescent/sadExpressions");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanCheekfluff");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanWaistband");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanTailflame");

            Futile.atlasManager.LoadAtlas("atlases/creatures/armorIceSpikes");
            Futile.atlasManager.LoadAtlas("atlases/creatures/freezer");
            Futile.atlasManager.LoadAtlas("atlases/creatures/cyanwing");
            Futile.atlasManager.LoadAtlas("atlases/creatures/luminescipede");
            Futile.atlasManager.LoadAtlas("atlases/creatures/chillipede");

            Futile.atlasManager.LoadAtlas("atlases/objects/iceChunks");
            Futile.atlasManager.LoadAtlas("atlases/objects/iceCrystals");

            Futile.atlasManager.LoadAtlas("atlases/miscSprites");

            SoundEffects.RegisterValues();

            IncanFeatures.Hooks();
            IncanVisuals.Hooks();

            Regions.Hooks();
            Weather.Hooks();
            Dialogue.Hooks();
            MiscWorldChanges.Hooks();

            LizardHooks.Hooks();
            CentiHooks.Apply();
            HailstormSpiders.Hooks();
			HailstormVultures.Hooks();
            OtherCreatureChanges.Hooks();

            CustomTemplateInfo.ApplyWeatherResistances();
            CustomObjectInfo.AddFreezableObjects();

            ObjectChanges.Hooks();
            
            JollyCoopFixes.Hooks();
            
            MachineConnector.SetRegisteredOI("theincandescent", new HSRemix());

            

            Logger.LogDebug($"Incan's up and runnin'!");
        }
        catch (Exception ex)
        {
            Logger.LogError("Uh whoops, something's wrong with Hailstorm: " + ex);
            throw;
        }
    }
    public void ReorderUnlocks(On.RainWorld.orig_PostModsInit orig, RainWorld rw)
    {
        orig(rw);
        OrganizeUnlocks(MoreSlugcatsEnums.SandboxUnlockID.AquaCenti, HailstormUnlocks.InfantAquapede);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HailstormUnlocks.SnowcuttleFemale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HailstormUnlocks.SnowcuttleMale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HailstormUnlocks.SnowcuttleLe);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.SmallCentipede, HailstormUnlocks.Raven);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HailstormUnlocks.IcyBlue);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HailstormUnlocks.Freezer);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.MirosBird, HailstormUnlocks.PeachSpider);
        OrganizeUnlocks(HailstormUnlocks.InfantAquapede, HailstormUnlocks.Cyanwing);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HailstormUnlocks.GorditoGreenie);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.TubeWorm, HailstormUnlocks.Chillipede);
        OrganizeUnlocks(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC, HailstormUnlocks.Luminescipede);
    }
    public void OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID moveOursBeforeThis, MultiplayerUnlocks.SandboxUnlockID unlockToMove)
    {
        MultiplayerUnlocks.CreatureUnlockList.Remove(unlockToMove);

        MultiplayerUnlocks.CreatureUnlockList.Insert(
            MultiplayerUnlocks.CreatureUnlockList.IndexOf(moveOursBeforeThis),
            unlockToMove);
    }

    //--------------------------------------------

    private void DisableCheck(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (int i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "moreslugcats")
            {

                // Creatures
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.InfantAquapede))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.InfantAquapede);
                
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.SnowcuttleFemale))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.SnowcuttleFemale);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.SnowcuttleMale))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.SnowcuttleMale);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.SnowcuttleLe))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.SnowcuttleLe);
                
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.Raven))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.Raven);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.IcyBlue))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.IcyBlue);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.Freezer))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.Freezer);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.PeachSpider))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.PeachSpider);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.Cyanwing))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.Cyanwing);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.GorditoGreenie))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.GorditoGreenie);
                
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.Chillipede))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.Chillipede);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.Luminescipede))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.Luminescipede);

                // Items
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.IceChunk))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.IceChunk);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.FreezerCrystal))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.FreezerCrystal);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormUnlocks.BurnSpear))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormUnlocks.BurnSpear);

                HailstormCreatures.UnregisterValues();
                HailstormItems.UnregisterValues();
                HailstormUnlocks.UnregisterValues();
                SoundEffects.UnregisterValues();
                break;
            }
        }
    }

}