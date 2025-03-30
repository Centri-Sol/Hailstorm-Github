namespace Hailstorm;

public class SnowcuttleLeCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(270 / 360f, 0.25f, 0.6f);

    internal SnowcuttleLeCritob() : base(HSEnums.CreatureType.SnowcuttleLe, HSEnums.SandboxUnlock.SnowcuttleLe, HSEnums.SandboxUnlock.SnowcuttleFemale) { }
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlL";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleL" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.LikesOutside,
            RoomAttractivenessPanel.Category.Flying,
            RoomAttractivenessPanel.Category.Dark,
            RoomAttractivenessPanel.Category.Lizards
        };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate snwCtlLe = new CreatureFormula(HSEnums.CreatureType.SnowcuttleTemplate, Type, "Snowcuttle Le").IntoTemplate();
        snwCtlLe.virtualCreature = false;
        return snwCtlLe;
    }
    public override void EstablishRelationships()
    {
        Relationships scL = new(HSEnums.CreatureType.SnowcuttleLe);

        // Will snatch up these creatures and fly back to its den.
        scL.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.9f);
        scL.Eats(CreatureTemplate.Type.VultureGrub, 0.8f);
        scL.Eats(CreatureTemplate.Type.SeaLeech, 0.8f);
        scL.Eats(CreatureTemplate.Type.Leech, 0.6f);
        scL.Eats(CreatureTemplate.Type.Hazer, 0.5f);
        scL.Eats(DLCSharedEnums.CreatureTemplateType.JungleLeech, 0.4f);
        scL.Eats(CreatureTemplate.Type.Fly, 0.2f);

        // Will bash and nibble at these creatures until it dies.
        scL.Attacks(CreatureTemplate.Type.BigNeedleWorm, 0.6f);
        scL.Attacks(CreatureTemplate.Type.EggBug, 0.6f);
        scL.Attacks(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.6f);
        scL.Attacks(HSEnums.CreatureType.Luminescipede, 0.6f);
        //scL.Attacks(HailstormEnums.Strobelegs, 0.6f);

        // Hangs out near these creatures when it has nothing else to do, and flies to them for protection if nearby and in danger.
        scL.IsInPack(CreatureTemplate.Type.CicadaA, 0.6f);
        scL.IsInPack(CreatureTemplate.Type.CicadaB, 0.6f);
        scL.IsInPack(HSEnums.CreatureType.SnowcuttleTemplate, 0.6f);

        // Flies away from these creatures if they are anywhere nearby, showing active fear.
        scL.Fears(CreatureTemplate.Type.DropBug, 1);
        scL.Fears(CreatureTemplate.Type.BigSpider, 1);
        scL.Fears(CreatureTemplate.Type.SpitterSpider, 1);
        //scL.Fears(HailstormEnums.BezanBud, 1);
        scL.Fears(CreatureTemplate.Type.Vulture, 0.8f);
        scL.Fears(CreatureTemplate.Type.KingVulture, 0.8f);
        scL.Fears(CreatureTemplate.Type.MirosBird, 0.8f);
        scL.Fears(DLCSharedEnums.CreatureTemplateType.MirosVulture, 0.8f);
        scL.Fears(DLCSharedEnums.CreatureTemplateType.StowawayBug, 0.7f);
        scL.Fears(CreatureTemplate.Type.Snail, 0.2f);
        scL.Fears(DLCSharedEnums.CreatureTemplateType.Yeek, 0.2f);

        scL.Ignores(CreatureTemplate.Type.PoleMimic);
        scL.Ignores(CreatureTemplate.Type.TentaclePlant);
        scL.Ignores(DLCSharedEnums.CreatureTemplateType.AquaCenti);
        scL.Ignores(HSEnums.CreatureType.InfantAquapede);

    }
    public override CreatureTemplate.Type ArenaFallback() => (Random.value < 0.5f) ? CreatureTemplate.Type.CicadaB : CreatureTemplate.Type.CicadaA;
}