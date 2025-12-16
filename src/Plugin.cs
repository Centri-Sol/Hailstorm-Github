namespace Hailstorm;


[BepInPlugin(MOD_ID, "Hailstorm", "0.3.0")]
public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "theincandescent";

    public bool IsInit;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += InitiateHailstorm;
        On.RainWorld.PostModsInit += ReorderUnlocks;
        On.RainWorld.OnModsDisabled += DisableCheck;

        Content.Register(
            new RavenCritob(),
            new FreezerCritob(),
            new IcyBlueCritob(),
            new PeachSpiderCritob(),
            new CyanwingCritob(),
            new GorditoGreenieCritob(),
            new ChillipedeCritob(),
            new LuminescipedeCritob(),
            new SnowcuttleTemplate(HSEnums.CreatureType.SnowcuttleTemplate, null, null),
            new SnowcuttleFemaleCritob(),
            new SnowcuttleMaleCritob(),
            new SnowcuttleLeCritob(),

            new IceChunkFisob(),
            new FreezerCrystalFisob()//,
            //new BurnSpearFisob()
            );
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
            LoadSounds();

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
    private void LoadSounds()
    {
        HSEnums.Sound.CyanwingDeath = new SoundID(nameof(HSEnums.Sound.CyanwingDeath), true);
        HSEnums.Sound.FireImpact = new SoundID(nameof(HSEnums.Sound.FireImpact), true);
        HSEnums.Sound.IncanFuel = new SoundID(nameof(HSEnums.Sound.IncanFuel), true);

        HSEnums.Sound.FireBurnLOOP = new SoundID(nameof(HSEnums.Sound.FireBurnLOOP), true);
        HSEnums.Sound.IncanTailFlameLOOP = new SoundID(nameof(HSEnums.Sound.IncanTailFlameLOOP), true);

        HSEnums.Sound.IcyBlueHiss = new SoundID(nameof(HSEnums.Sound.IcyBlueHiss), true);
        HSEnums.Sound.FreezerHiss = new SoundID(nameof(HSEnums.Sound.FreezerHiss), true);
        HSEnums.Sound.FreezerLove = new SoundID(nameof(HSEnums.Sound.FreezerLove), true);
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
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleFemale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleMale);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.Snail, HSEnums.SandboxUnlock.SnowcuttleLe);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.SmallCentipede, HSEnums.SandboxUnlock.Raven);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.IcyBlue);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.Freezer);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.MirosBird, HSEnums.SandboxUnlock.PeachSpider);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.CyanLizard, HSEnums.SandboxUnlock.GorditoGreenie);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.TubeWorm, HSEnums.SandboxUnlock.Chillipede);
        OrganizeUnlocks(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC, HSEnums.SandboxUnlock.Luminescipede);
    }
    private void OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID moveToBeforeThis, MultiplayerUnlocks.SandboxUnlockID unlockToMove)
    {
        if (moveToBeforeThis is not null &&
            unlockToMove is not null &&
            MultiplayerUnlocks.CreatureUnlockList.Contains(unlockToMove))
        {
            MultiplayerUnlocks.CreatureUnlockList.Remove(unlockToMove);
            MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(moveToBeforeThis), unlockToMove);
        }
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

    /// <summary> A debug log option that can be easily disabled from Plugin for mod releases or updates. </summary>
    /// <param name="message"></param>
    public static void TestingLog(object message)
    {
        return;
        Debug.Log(message);
    }
    /// <summary> Generates logs, placing a [Hailstorm] tag before each message. </summary>
    /// <param name="message"></param>
    public static void HailstormLog(object message)
    {
        Debug.Log("[Hailstorm] " + message);
    }

}
//----------------------------------------------------------------------------------------------------------------------------------------------------------------