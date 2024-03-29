namespace Hailstorm;

public class SnowcuttleMaleCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(320 / 360f, 0.25f, 0.6f);

    internal SnowcuttleMaleCritob() : base(HSEnums.CreatureType.SnowcuttleMale, HSEnums.SandboxUnlock.SnowcuttleMale, HSEnums.SandboxUnlock.SnowcuttleFemale) { }
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlM";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleM" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate snwCtlMale = new CreatureFormula(HSEnums.CreatureType.SnowcuttleTemplate, Type, "Snowcuttle Male").IntoTemplate();
        snwCtlMale.virtualCreature = false;
        snwCtlMale.visualRadius = 1200;
        snwCtlMale.throughSurfaceVision = 0.8f;
        snwCtlMale.waterVision = 0.8f;
        snwCtlMale.movementBasedVision = 0.25f;
        return snwCtlMale;
    }
    public override void EstablishRelationships()
    {
        Relationships scM = new(HSEnums.CreatureType.SnowcuttleMale);

        // Flies away from this creature if they are anywhere nearby, showing active fear.
        scM.Fears(HSEnums.CreatureType.PeachSpider, 0.8f);

    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaB;
}