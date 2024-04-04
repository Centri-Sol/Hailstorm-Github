namespace Hailstorm;


[BepInPlugin(MOD_ID, "The Incandescent", "0.3.0")]
public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "theincandescent";

    public bool IsInit;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += InitiateHailstorm;
        On.RainWorld.PostModsInit += ReorderUnlocks;
        On.RainWorld.OnModsDisabled += DisableCheck;

        RegisterUnlocks();
        HSEnums.Init();
    }

    //----------------------------------------
    //----------------------------------------

    private void InitiateHailstorm(On.RainWorld.orig_OnModsInit orig, RainWorld rw)
    {
        orig(rw);
        try
        {
            if (IsInit) return;
            IsInit = true;

            LoadAtlases();

            IncanFeatures.Hooks();
            IncanVisuals.Hooks();

            Regions.Hooks();
            Weather.Hooks();
            Dialogue.Hooks();
            MiscWorldChanges.Hooks();

            JollyCoopFixes.Hooks();

            ApplyCreatures();
            ApplyItems();

            MachineConnector.SetRegisteredOI("theincandescent", new HSRemix());

            Debug.LogWarning($"Incan's up and runnin'!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogException(ex);
            throw;
        }
    }
    private void LoadAtlases()
    {
        foreach (var file in AssetManager.ListDirectory("hs_atlases"))
        {
            if (".png".Equals(Path.GetExtension(file)))
            {
                if (File.Exists(Path.ChangeExtension(file, ".txt")))
                {
                    Futile.atlasManager.LoadAtlas(Path.ChangeExtension(file, null));
                }
                else
                {
                    Futile.atlasManager.LoadImage(Path.ChangeExtension(file, null));
                }
            }
        }
    }
    private void ApplyCreatures()
    {
        LizardHooks.Hooks();
        CentiHooks.Apply();
        HailstormSpiders.Hooks();
        HailstormVultures.Hooks();
        OtherCreatureChanges.Hooks();
    }
    private void ApplyItems()
    {
        CustomTemplateInfo.ApplyWeatherResistances();
        CustomObjectInfo.AddFreezableObjects();
        ObjectChanges.Hooks();
    }

    // - - - - - - - - - -

    private void ReorderUnlocks(On.RainWorld.orig_PostModsInit orig, RainWorld rw)
    {
        orig(rw);
        OrganizeUnlocks(MoreSlugcatsEnums.SandboxUnlockID.AquaCenti, HSEnums.SandboxUnlock.InfantAquapede);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleFemale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleMale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleLe);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.SmallCentipede, HSEnums.SandboxUnlock.Raven);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.IcyBlue);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.Freezer);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.MirosBird, HSEnums.SandboxUnlock.PeachSpider);
        OrganizeUnlocks(HSEnums.SandboxUnlock.InfantAquapede, HSEnums.SandboxUnlock.Cyanwing);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.GorditoGreenie);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.TubeWorm, HSEnums.SandboxUnlock.Chillipede);
        OrganizeUnlocks(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC, HSEnums.SandboxUnlock.Luminescipede);
    }
    private void OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID moveToBeforeThis, MultiplayerUnlocks.SandboxUnlockID unlockToMove)
    {
        MultiplayerUnlocks.CreatureUnlockList.Remove(unlockToMove);
        MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(moveToBeforeThis), unlockToMove);
    }

    // - - - - - - - - - -

    private void DisableCheck(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (int i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "moreslugcats")
            {
                HSEnums.Unregister();
                break;
            }
        }
    }

    // - - - - - - - - - -

    private void RegisterUnlocks()
    {
        Content.Register(
            new InfantAquapedeCritob(),
            new SnowcuttleTemplate(HSEnums.CreatureType.SnowcuttleTemplate, null, null),
            new SnowcuttleFemaleCritob(),
            new SnowcuttleMaleCritob(),
            new SnowcuttleLeCritob(),
            new RavenCritob(),
            new FreezerCritob(),
            new IcyBlueCritob(),
            new PeachSpiderCritob(),
            new CyanwingCritob(),
            new GorditoGreenieCritob(),
            new ChillipedeCritob(),
            new LuminescipedeCritob());

        Content.Register(
            new IceChunkFisob(),
            new FreezerCrystalFisob(),
            new BurnSpearFisob());
    }

}
//----------------------------------------------------------------------------------------------------------------------------------------------------------------