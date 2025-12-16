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
        Relationships IcyBlue = new(HSEnums.CreatureType.IcyBlueLizard);
        IcyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        IcyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        IcyBlue.Eats(CreatureTemplate.Type.Scavenger, 1);
        IcyBlue.Eats(CreatureTemplate.Type.LanternMouse, 1);
        IcyBlue.Eats(CreatureTemplate.Type.SmallCentipede, 1);
        IcyBlue.Eats(CreatureTemplate.Type.Centipede, 1);
        IcyBlue.Eats(CreatureTemplate.Type.Centiwing, 0.9f);
        IcyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.85f);
        IcyBlue.Eats(CreatureTemplate.Type.CicadaA, 0.85f);
        IcyBlue.Eats(CreatureTemplate.Type.CicadaB, 0.85f);
        IcyBlue.Eats(HSEnums.CreatureType.SnowcuttleTemplate, 0.66f);
        IcyBlue.Eats(CreatureTemplate.Type.EggBug, 0.66f);
        IcyBlue.Eats(CreatureTemplate.Type.DropBug, 0.5f);
        IcyBlue.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.5f);
        IcyBlue.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.5f);
        IcyBlue.Eats(new("InfantAquapede"), 0.5f);
        IcyBlue.Eats(HSEnums.CreatureType.Luminescipede, 0.5f);
        IcyBlue.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        IcyBlue.Eats(CreatureTemplate.Type.BigSpider, 0.33f);
        IcyBlue.Eats(CreatureTemplate.Type.SpitterSpider, 0.33f);
        IcyBlue.Eats(CreatureTemplate.Type.JetFish, 0.3f);
        IcyBlue.Eats(CreatureTemplate.Type.VultureGrub, 0.3f);
        IcyBlue.Eats(CreatureTemplate.Type.Hazer, 0.25f);
        IcyBlue.Eats(CreatureTemplate.Type.TubeWorm, 0.1f);

        IcyBlue.Rivals(CreatureTemplate.Type.WhiteLizard, 0.50f);
        IcyBlue.Rivals(CreatureTemplate.Type.BlackLizard, 0.75f);
        IcyBlue.Rivals(CreatureTemplate.Type.PinkLizard, 1);

        // Does nothing on its own.
        IcyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        IcyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);
        IcyBlue.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);

        IcyBlue.Fears(CreatureTemplate.Type.BigEel, 1);
        IcyBlue.Fears(CreatureTemplate.Type.BrotherLongLegs, 1);
        IcyBlue.Fears(CreatureTemplate.Type.DaddyLongLegs, 1);
        IcyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        IcyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.4f);
        IcyBlue.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.25f);
        IcyBlue.Fears(CreatureTemplate.Type.TentaclePlant, 0.2f);

        // Does nothing on its own.
        IcyBlue.IsInPack(HSEnums.CreatureType.IcyBlueLizard, 1);
        IcyBlue.IsInPack(HSEnums.CreatureType.FreezerLizard, 1);
        IcyBlue.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.75f);
        IcyBlue.IsInPack(CreatureTemplate.Type.BlueLizard, 0.66f);

        // Does nothing on its own.
        IcyBlue.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.5f);
        IcyBlue.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);

        IcyBlue.Ignores(HSEnums.CreatureType.Chillipede);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        IcyBlue.EatenBy(CreatureTemplate.Type.Vulture, 0.6f);
        IcyBlue.EatenBy(CreatureTemplate.Type.KingVulture, 0.6f);
        IcyBlue.EatenBy(HSEnums.CreatureType.Raven, 0.6f);
        IcyBlue.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
        IcyBlue.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.6f);
        IcyBlue.EatenBy(CreatureTemplate.Type.RedCentipede, 0.6f);
        IcyBlue.EatenBy(HSEnums.CreatureType.Cyanwing, 0.6f);
        IcyBlue.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
        IcyBlue.EatenBy(CreatureTemplate.Type.CyanLizard, 0.5f);
        IcyBlue.EatenBy(CreatureTemplate.Type.Centipede, 0.4f);
        IcyBlue.EatenBy(CreatureTemplate.Type.Centiwing, 0.4f);
        IcyBlue.EatenBy(CreatureTemplate.Type.GreenLizard, 0.25f);

        IcyBlue.AttackedBy(CreatureTemplate.Type.YellowLizard, 1);
        IcyBlue.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.8f);
        IcyBlue.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.6f);
        IcyBlue.AttackedBy(CreatureTemplate.Type.Scavenger, 0.4f);

        IcyBlue.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.9f);
        IcyBlue.FearedBy(CreatureTemplate.Type.CicadaA, 0.8f);
        IcyBlue.FearedBy(CreatureTemplate.Type.CicadaB, 0.8f);
        IcyBlue.FearedBy(CreatureTemplate.Type.Slugcat, 0.7f);
        IcyBlue.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.7f);
        IcyBlue.FearedBy(CreatureTemplate.Type.LanternMouse, 0.7f);
        IcyBlue.FearedBy(CreatureTemplate.Type.Scavenger, 0.6f);
        IcyBlue.FearedBy(CreatureTemplate.Type.BigSpider, 0.4f);
        IcyBlue.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.3f);
        IcyBlue.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.15f);
        IcyBlue.FearedBy(CreatureTemplate.Type.JetFish, 0.15f);

        IcyBlue.IgnoredBy(HSEnums.CreatureType.Chillipede);

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