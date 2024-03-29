namespace Hailstorm;

public class IcyBlueCritob : Critob
{
    public Color IcyBlueColor = new(138 / 255f, 151 / 255f, 193 / 255f);

    internal IcyBlueCritob() : base(HSEnums.CreatureType.IcyBlueLizard)
    {
        Icon = new SimpleIcon("Kill_Icy_Blue_Lizard", IcyBlueColor);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(0.6f, 0.6f);
        RegisterUnlock(KillScore.Configurable(12), HSEnums.SandboxUnlock.IcyBlue);
    }
    public override int ExpeditionScore() => 12;

    public override Color DevtoolsMapColor(AbstractCreature absIcy) => IcyBlueColor;
    public override string DevtoolsMapName(AbstractCreature absIcy) => "Icy";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.Lizards,
            RoomAttractivenessPanel.Category.LikesInside
        };
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "icyblue", "IcyBlue" };
    }

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(HSEnums.CreatureType.IcyBlueLizard, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
    public override void EstablishRelationships()
    {
        // Relationship types that work with Lizard AI:
        // * Eats - Seeks out prey and brings it back to its den.
        // * Attacks - Fights targets to the death.
        // * Fears ("Afraid" in the game's code) - Actively flees from targets.
        // * Rivals ("AggressiveRival" in the game's code) - May fight targets if they get in the way, though typically not to the death.
        // Any relationship types not listed are not supported by base-game or DLC code, and will act like Ignores without new code.
        Relationships icyBlue = new(HSEnums.CreatureType.IcyBlueLizard);
        icyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        icyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        icyBlue.Eats(CreatureTemplate.Type.Scavenger, 1);
        icyBlue.Eats(CreatureTemplate.Type.LanternMouse, 1);
        icyBlue.Eats(CreatureTemplate.Type.SmallCentipede, 1);
        icyBlue.Eats(CreatureTemplate.Type.Centipede, 1);
        icyBlue.Eats(CreatureTemplate.Type.Centiwing, 0.9f);
        icyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.85f);
        icyBlue.Eats(CreatureTemplate.Type.CicadaA, 0.85f);
        icyBlue.Eats(CreatureTemplate.Type.CicadaB, 0.85f);
        icyBlue.Eats(HSEnums.CreatureType.SnowcuttleTemplate, 0.66f);
        icyBlue.Eats(CreatureTemplate.Type.EggBug, 0.66f);
        icyBlue.Eats(CreatureTemplate.Type.DropBug, 0.5f);
        icyBlue.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.5f);
        icyBlue.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.5f);
        icyBlue.Eats(HSEnums.CreatureType.InfantAquapede, 0.5f);
        icyBlue.Eats(HSEnums.CreatureType.Luminescipede, 0.5f);
        icyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        icyBlue.Eats(CreatureTemplate.Type.BigSpider, 0.33f);
        icyBlue.Eats(CreatureTemplate.Type.SpitterSpider, 0.33f);
        icyBlue.Eats(CreatureTemplate.Type.JetFish, 0.3f);
        icyBlue.Eats(CreatureTemplate.Type.VultureGrub, 0.3f);
        icyBlue.Eats(CreatureTemplate.Type.Hazer, 0.25f);
        icyBlue.Eats(CreatureTemplate.Type.TubeWorm, 0.1f);

        icyBlue.Rivals(CreatureTemplate.Type.WhiteLizard, 0.50f);
        icyBlue.Rivals(CreatureTemplate.Type.BlackLizard, 0.75f);
        icyBlue.Rivals(CreatureTemplate.Type.PinkLizard, 1);

        // Does nothing on its own.
        icyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        icyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);
        icyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);

        icyBlue.Fears(CreatureTemplate.Type.BigEel, 1);
        icyBlue.Fears(CreatureTemplate.Type.BrotherLongLegs, 1);
        icyBlue.Fears(CreatureTemplate.Type.DaddyLongLegs, 1);
        icyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        icyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.4f);
        icyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.25f);
        icyBlue.Fears(CreatureTemplate.Type.TentaclePlant, 0.2f);

        // Does nothing on its own.
        icyBlue.IsInPack(HSEnums.CreatureType.IcyBlueLizard, 1);
        icyBlue.IsInPack(HSEnums.CreatureType.FreezerLizard, 1);
        icyBlue.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.75f);
        icyBlue.IsInPack(CreatureTemplate.Type.BlueLizard, 0.66f);

        // Does nothing on its own.
        icyBlue.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.5f);
        icyBlue.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);

        icyBlue.Ignores(HSEnums.CreatureType.Chillipede);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        icyBlue.EatenBy(CreatureTemplate.Type.Vulture, 0.6f);
        icyBlue.EatenBy(CreatureTemplate.Type.KingVulture, 0.6f);
        icyBlue.EatenBy(HSEnums.CreatureType.Raven, 0.6f);
        icyBlue.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
        icyBlue.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.6f);
        icyBlue.EatenBy(CreatureTemplate.Type.RedCentipede, 0.6f);
        icyBlue.EatenBy(HSEnums.CreatureType.Cyanwing, 0.6f);
        icyBlue.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
        icyBlue.EatenBy(CreatureTemplate.Type.CyanLizard, 0.5f);
        icyBlue.EatenBy(CreatureTemplate.Type.Centipede, 0.4f);
        icyBlue.EatenBy(CreatureTemplate.Type.Centiwing, 0.4f);
        icyBlue.EatenBy(CreatureTemplate.Type.GreenLizard, 0.25f);

        icyBlue.AttackedBy(CreatureTemplate.Type.YellowLizard, 1);
        icyBlue.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.8f);
        icyBlue.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.6f);
        icyBlue.AttackedBy(CreatureTemplate.Type.Scavenger, 0.4f);

        icyBlue.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.9f);
        icyBlue.FearedBy(CreatureTemplate.Type.CicadaA, 0.8f);
        icyBlue.FearedBy(CreatureTemplate.Type.CicadaB, 0.8f);
        icyBlue.FearedBy(CreatureTemplate.Type.Slugcat, 0.7f);
        icyBlue.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.7f);
        icyBlue.FearedBy(CreatureTemplate.Type.LanternMouse, 0.7f);
        icyBlue.FearedBy(CreatureTemplate.Type.Scavenger, 0.6f);
        icyBlue.FearedBy(CreatureTemplate.Type.BigSpider, 0.4f);
        icyBlue.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.3f);
        icyBlue.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.15f);
        icyBlue.FearedBy(CreatureTemplate.Type.JetFish, 0.15f);

        icyBlue.IgnoredBy(HSEnums.CreatureType.Chillipede);

        // Fun Fact 1: If you set multiple relationship types with the same creature, the last one you set will overwrite the rest.

        // Fun Fact 2: If you set a relationship type with LizardTemplate, your creature will use that relationship with ALL lizards.
        // You can then set different relationships with specific lizard types after that.
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absIcy) => new ColdLizAI(absIcy, absIcy.world);
    public override Creature CreateRealizedCreature(AbstractCreature absIcy) => new ColdLizard(absIcy, absIcy.world);
    public override CreatureState CreateState(AbstractCreature absIcy) => new ColdLizState(absIcy);

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BlueLizard;
#nullable disable
}