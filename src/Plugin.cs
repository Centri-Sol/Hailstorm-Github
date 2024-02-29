using System.Linq;

namespace Hailstorm;


[BepInPlugin(MOD_ID, "The Incandescent", "0.3.0")]
public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "theincandescent";

    public bool IsInit;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.OnModsDisabled += DisableCheck;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;
            IsInit = true;

            LoadAtlases();

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

    private void ApplyCreatures()
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
    }

    private void ApplyItems()
    {
        Content.Register(
            new IceChunkFisob(),
            new FreezerCrystalFisob(),
            new BurnSpearFisob());
    }

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

    private void LoadAtlases()
    {
        foreach (string file in from string file in AssetManager.ListDirectory("hs_atlases")
                                where Path.GetExtension(file).Equals(".png")
                                where File.Exists(Path.ChangeExtension(file, ".txt"))
                                select file)
        {
            Futile.atlasManager.LoadAtlas(Path.ChangeExtension(file, null));
        }
    }
}