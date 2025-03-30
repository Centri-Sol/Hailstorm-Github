namespace Hailstorm;

public class SnowcuttleFemaleCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(220 / 360f, 0.25f, 0.6f);

    internal SnowcuttleFemaleCritob() : base(HSEnums.CreatureType.SnowcuttleFemale, HSEnums.SandboxUnlock.SnowcuttleFemale, null) { }
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlF";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleF" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate snwCtlFemale = new CreatureFormula(HSEnums.CreatureType.SnowcuttleTemplate, Type, "Snowcuttle Female").IntoTemplate();
        snwCtlFemale.virtualCreature = false;
        snwCtlFemale.visualRadius = 900;
        snwCtlFemale.throughSurfaceVision = 0.45f;
        snwCtlFemale.waterVision = 0.4f;
        snwCtlFemale.movementBasedVision = 1.5f;
        return snwCtlFemale;
    }
    public override void EstablishRelationships()
    {
        Relationships scF = new(HSEnums.CreatureType.SnowcuttleFemale);

        // Will snatch up these creatures and fly back to its den.
        scF.Eats(CreatureTemplate.Type.Fly, 0.9f);
        scF.Eats(CreatureTemplate.Type.Leech, 0.9f);
        scF.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.7f);
        scF.Eats(HSEnums.CreatureType.PeachSpider, 0.6f);
        scF.Eats(CreatureTemplate.Type.SeaLeech, 0.6f);
        scF.Eats(DLCSharedEnums.CreatureTemplateType.JungleLeech, 0.6f);
        scF.Eats(CreatureTemplate.Type.Hazer, 0.5f);
        scF.Eats(CreatureTemplate.Type.VultureGrub, 0.3f);
        scF.Eats(CreatureTemplate.Type.SmallCentipede, 0.1f);

        // Will bash and nibble at these creatures until it dies.
        scF.Attacks(CreatureTemplate.Type.BigNeedleWorm, 0.8f);
        scF.Attacks(CreatureTemplate.Type.EggBug, 0.7f);
        scF.Attacks(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.7f);

    }

}