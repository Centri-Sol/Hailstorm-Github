namespace Hailstorm;

public class FreezerCritob : Critob
{

    public Color FreezerColor = new(129f / 255f, 200f / 255f, 236f / 255f);

    internal FreezerCritob() : base(HSEnums.CreatureType.FreezerLizard)
    {
        Icon = new SimpleIcon("Kill_Freezer_Lizard", FreezerColor);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(0.7f, 0.7f);
        RegisterUnlock(KillScore.Configurable(25), HSEnums.SandboxUnlock.Freezer);
    }
    public override int ExpeditionScore() => 25;

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.NPCTransportation)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.NPCTransportation)
                allow = true;

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.RegionTransportation)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.RegionTransportation)
                allow = true;

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.RoomExit)
                allow = true;
        }
    }

    public override string DevtoolsMapName(AbstractCreature absFrz) => "Frz";
    public override Color DevtoolsMapColor(AbstractCreature absFrz) => FreezerColor;
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesOutside
    };
    public override IEnumerable<string> WorldFileAliases()
    {
        // The names used in World files when setting creature spawns.
        // You can set up as many as you'd like, but only one is necessary.
        return new[] { "freezer", "Freezer" };
    }

    public override CreatureTemplate CreateTemplate()
    {
        return LizardBreeds.BreedTemplate(HSEnums.CreatureType.FreezerLizard, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
    }
    public override void EstablishRelationships()
    {
        // Relationship types that work with Lizard AI:
        // * Eats - Seeks out prey and brings it back to its den.
        // * Attacks - Fights targets to the death.
        // * Fears ("Afraid" in the game's code) - Actively flees from targets.
        // * Rivals ("AggressiveRival" in the game's code) - May fight targets if they get in the way, though typically not to the death.
        // Any relationship types not listed are not supported by base-game or DLC code, and will act like Ignores without new code.
        Relationships Freezer = new(HSEnums.CreatureType.FreezerLizard);

        Freezer.Eats(CreatureTemplate.Type.Slugcat, 1);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        Freezer.Eats(CreatureTemplate.Type.Scavenger, 1);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        Freezer.Eats(CreatureTemplate.Type.RedCentipede, 1);
        Freezer.Eats(HSEnums.CreatureType.Cyanwing, 1);
        Freezer.Eats(CreatureTemplate.Type.EggBug, 1);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1.1f);
        Freezer.Eats(CreatureTemplate.Type.Centipede, 0.95f);
        Freezer.Eats(CreatureTemplate.Type.LanternMouse, 0.95f);
        Freezer.Eats(CreatureTemplate.Type.Centiwing, 0.95f);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.95f);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.9f);
        Freezer.Eats(CreatureTemplate.Type.CicadaA, 0.85f);
        Freezer.Eats(CreatureTemplate.Type.CicadaB, 0.85f);
        Freezer.Eats(CreatureTemplate.Type.BigSpider, 0.8f);
        Freezer.Eats(CreatureTemplate.Type.SpitterSpider, 0.8f);
        Freezer.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.75f);
        Freezer.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.75f);
        Freezer.Eats(CreatureTemplate.Type.DropBug, 0.75f);
        Freezer.Eats(HSEnums.CreatureType.SnowcuttleTemplate, 0.66f);
        Freezer.Eats(HSEnums.CreatureType.Luminescipede, 0.6f);
        Freezer.Eats(CreatureTemplate.Type.LizardTemplate, 0.5f);
        Freezer.Eats(CreatureTemplate.Type.SmallCentipede, 0.5f);
        Freezer.Eats(CreatureTemplate.Type.VultureGrub, 0.4f);
        Freezer.Eats(CreatureTemplate.Type.JetFish, 0.4f);
        Freezer.Eats(CreatureTemplate.Type.Hazer, 0.35f);
        Freezer.Eats(CreatureTemplate.Type.TubeWorm, 0.2f);
        Freezer.Eats(new("InfantAquapede"), 0.1f);
        Freezer.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.1f);
        Freezer.Attacks(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.5f);

        // Does nothing on its own.
        Freezer.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
        Freezer.IntimidatedBy(CreatureTemplate.Type.Snail, 0.4f);
        Freezer.IntimidatedBy(CreatureTemplate.Type.TentaclePlant, 0.4f);
        Freezer.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 0.4f);

        Freezer.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.7f);
        Freezer.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.6f);
        Freezer.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.5f);
        Freezer.Fears(CreatureTemplate.Type.BigEel, 0.25f);
        Freezer.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.1f);

        // Does nothing on its own.
        Freezer.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.5f);
        Freezer.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);

        // Does nothing on its own.
        Freezer.IsInPack(HSEnums.CreatureType.IcyBlueLizard, 1);
        Freezer.IsInPack(HSEnums.CreatureType.FreezerLizard, 1);
        Freezer.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.75f);
        Freezer.IsInPack(CreatureTemplate.Type.BlueLizard, 0.66f);

        Freezer.Ignores(CreatureTemplate.Type.Spider);
        Freezer.Ignores(CreatureTemplate.Type.PoleMimic);
        Freezer.Ignores(HSEnums.CreatureType.Chillipede);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        Freezer.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.75f);
        Freezer.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.75f);
        Freezer.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.75f);
        Freezer.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
        Freezer.EatenBy(CreatureTemplate.Type.MirosBird, 0.4f);
        Freezer.EatenBy(CreatureTemplate.Type.RedCentipede, 0.25f);
        Freezer.EatenBy(HSEnums.CreatureType.Cyanwing, 0.25f);
        Freezer.EatenBy(CreatureTemplate.Type.KingVulture, 0.25f);
        Freezer.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.25f);

        Freezer.AttackedBy(CreatureTemplate.Type.PinkLizard, 1);
        Freezer.AttackedBy(CreatureTemplate.Type.RedLizard, 1);
        Freezer.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);

        Freezer.FearedBy(CreatureTemplate.Type.CicadaA, 1);
        Freezer.FearedBy(CreatureTemplate.Type.CicadaB, 1);
        Freezer.FearedBy(CreatureTemplate.Type.SmallCentipede, 1);
        Freezer.FearedBy(CreatureTemplate.Type.LanternMouse, 1);
        Freezer.FearedBy(CreatureTemplate.Type.YellowLizard, 1);
        Freezer.FearedBy(CreatureTemplate.Type.CyanLizard, 1);
        Freezer.FearedBy(CreatureTemplate.Type.Slugcat, 0.9f);
        Freezer.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.9f);
        Freezer.FearedBy(CreatureTemplate.Type.Scavenger, 0.9f);
        Freezer.FearedBy(CreatureTemplate.Type.BlackLizard, 0.8f);
        Freezer.FearedBy(CreatureTemplate.Type.WhiteLizard, 0.8f);
        Freezer.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.8f);
        Freezer.FearedBy(CreatureTemplate.Type.Centipede, 0.75f);
        Freezer.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.75f);
        Freezer.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.75f);
        Freezer.FearedBy(CreatureTemplate.Type.BigSpider, 0.7f);
        Freezer.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.6f);
        Freezer.FearedBy(CreatureTemplate.Type.Vulture, 0.4f);
        Freezer.FearedBy(CreatureTemplate.Type.Centiwing, 0.4f);
        Freezer.FearedBy(CreatureTemplate.Type.JetFish, 0.2f);

        Freezer.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        Freezer.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        Freezer.IgnoredBy(HSEnums.CreatureType.Chillipede);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absFrz) => new ColdLizAI(absFrz, absFrz.world);
    public override Creature CreateRealizedCreature(AbstractCreature absFrz) => new ColdLizard(absFrz, absFrz.world);
    public override CreatureState CreateState(AbstractCreature absFrz) => new ColdLizState(absFrz);

    public override CreatureTemplate.Type ArenaFallback() => HSEnums.CreatureType.IcyBlueLizard;
}