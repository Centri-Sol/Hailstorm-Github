namespace Hailstorm;

internal sealed class GorditoGreenieCritob : Critob
{
    public Color GorditoGreenieColor = Custom.HSL2RGB(135 / 360f, 0.5f, 0.7f);

    internal GorditoGreenieCritob() : base(HSEnums.CreatureType.GorditoGreenieLizard)
    {
        Icon = new SimpleIcon("Kill_Gordito_Greenie_Lizard", GorditoGreenieColor);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(0.7f, 0.6f);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(20), HSEnums.SandboxUnlock.GorditoGreenie);
    }
    public override int ExpeditionScore() => 20;

    public override Color DevtoolsMapColor(AbstractCreature absLiz) => GorditoGreenieColor;
    public override string DevtoolsMapName(AbstractCreature absLiz) => "GORDITO";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.Lizards,
            RoomAttractivenessPanel.Category.LikesOutside
        };
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "gorditogeenie", "GorditoGreenie" };
    }
    public override CreatureTemplate CreateTemplate()
    {
        return LizardBreeds.BreedTemplate(HSEnums.CreatureType.GorditoGreenieLizard, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
    }
    public override void EstablishRelationships()
    {
        // Relationship types that work with Lizard AI:
        // * Eats - Seeks out prey and brings it back to its den.
        // * Attacks - Fights targets to the death.
        // * Fears ("Afraid" in the game's code) - Actively flees from targets.
        // * Rivals ("AggressiveRival" in the game's code) - May fight targets if they get in the way, though typically not to the death.
        // Any relationship types not listed are not supported by base-game or DLC code, and will act like Ignores without new code.
        Relationships elGordito = new(HSEnums.CreatureType.GorditoGreenieLizard);

        elGordito.Eats(CreatureTemplate.Type.Slugcat, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        elGordito.Eats(CreatureTemplate.Type.CicadaA, 1);
        elGordito.Eats(CreatureTemplate.Type.CicadaB, 1);
        elGordito.Eats(CreatureTemplate.Type.EggBug, 1);
        elGordito.Eats(CreatureTemplate.Type.VultureGrub, 1);
        elGordito.Eats(CreatureTemplate.Type.SmallCentipede, 1);
        elGordito.Eats(CreatureTemplate.Type.Centipede, 1);
        elGordito.Eats(CreatureTemplate.Type.RedCentipede, 1);
        elGordito.Eats(CreatureTemplate.Type.Centiwing, 1);
        elGordito.Eats(HSEnums.CreatureType.Cyanwing, 1);
        elGordito.Eats(HSEnums.CreatureType.InfantAquapede, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 1);
        elGordito.Eats(CreatureTemplate.Type.Hazer, 1);
        elGordito.Eats(CreatureTemplate.Type.Snail, 1);
        elGordito.Eats(CreatureTemplate.Type.JetFish, 1);
        elGordito.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1);
        elGordito.Eats(CreatureTemplate.Type.BigNeedleWorm, 1);
        elGordito.Eats(CreatureTemplate.Type.DropBug, 1);
        elGordito.Eats(CreatureTemplate.Type.BigSpider, 1);
        elGordito.Eats(CreatureTemplate.Type.SpitterSpider, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 1);
        elGordito.Eats(HSEnums.CreatureType.Luminescipede, 1);
        elGordito.Eats(CreatureTemplate.Type.LanternMouse, 1);
        elGordito.Eats(CreatureTemplate.Type.TubeWorm, 1);
        elGordito.Eats(CreatureTemplate.Type.Scavenger, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        elGordito.Eats(HSEnums.CreatureType.Chillipede, 1);
        elGordito.Eats(CreatureTemplate.Type.BlueLizard, 0.5f);
        elGordito.Eats(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.5f);
        elGordito.Eats(HSEnums.CreatureType.Raven, 0.5f);

        elGordito.Attacks(CreatureTemplate.Type.MirosBird, 1);
        elGordito.Attacks(CreatureTemplate.Type.KingVulture, 1);
        elGordito.Attacks(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1);
        elGordito.Attacks(CreatureTemplate.Type.BrotherLongLegs, 0.5f);
        elGordito.Attacks(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 0.5f);

        elGordito.Rivals(HSEnums.CreatureType.GorditoGreenieLizard, 1);
        elGordito.Rivals(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 1);
        elGordito.Rivals(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
        elGordito.Rivals(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        elGordito.Rivals(CreatureTemplate.Type.GreenLizard, 0.25f);

        // Does nothing on its own.
        elGordito.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.25f);
        elGordito.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.5f);

        elGordito.Fears(CreatureTemplate.Type.BigEel, 1);
        elGordito.Fears(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);
        elGordito.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        elGordito.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.66f);

        elGordito.Ignores(CreatureTemplate.Type.Overseer);
        elGordito.Ignores(CreatureTemplate.Type.GarbageWorm);
        elGordito.Ignores(CreatureTemplate.Type.Fly);
        elGordito.Ignores(CreatureTemplate.Type.Vulture);
        elGordito.Ignores(CreatureTemplate.Type.Spider);
        elGordito.Ignores(CreatureTemplate.Type.Leech);
        elGordito.Ignores(CreatureTemplate.Type.SeaLeech);
        elGordito.Ignores(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech);
        elGordito.Ignores(HSEnums.CreatureType.PeachSpider);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        elGordito.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
        elGordito.EatenBy(CreatureTemplate.Type.KingVulture, 0.4f);
        elGordito.EatenBy(CreatureTemplate.Type.RedCentipede, 0.25f);

        elGordito.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.75f);
        elGordito.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.5f);
        elGordito.AttackedBy(CreatureTemplate.Type.MirosBird, 0.4f);
        elGordito.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.4f);
        elGordito.AttackedBy(CreatureTemplate.Type.Scavenger, 0.3f);

        elGordito.Intimidates(CreatureTemplate.Type.PoleMimic, 1);
        elGordito.Intimidates(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        elGordito.Intimidates(CreatureTemplate.Type.TentaclePlant, 0.7f);
        elGordito.Intimidates(CreatureTemplate.Type.SpitterSpider, 0.5f);
        elGordito.Intimidates(CreatureTemplate.Type.Centipede, 0.5f);
        elGordito.Intimidates(CreatureTemplate.Type.BigSpider, 0.5f);
        elGordito.Intimidates(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);
        elGordito.Intimidates(CreatureTemplate.Type.Scavenger, 0.3f);
        elGordito.Intimidates(CreatureTemplate.Type.Centiwing, 0.3f);

        elGordito.FearedBy(CreatureTemplate.Type.EggBug, 1);
        elGordito.FearedBy(CreatureTemplate.Type.Slugcat, 0.5f);
        elGordito.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.CicadaA, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.CicadaB, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.DropBug, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.LanternMouse, 0.5f);
        elGordito.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        elGordito.FearedBy(CreatureTemplate.Type.JetFish, 0.3f);
        elGordito.FearedBy(CreatureTemplate.Type.BigSpider, 0.3f);
        elGordito.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.1f);

        elGordito.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        elGordito.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        elGordito.IgnoredBy(CreatureTemplate.Type.Spider);
        elGordito.IgnoredBy(CreatureTemplate.Type.Leech);
        elGordito.IgnoredBy(CreatureTemplate.Type.SeaLeech);
        elGordito.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech);

    }

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow)
    {
        if (map.getAItile(tilePos).acc is AItile.Accessibility.Climb or
            AItile.Accessibility.Corridor or
            AItile.Accessibility.Wall)
        {
            allow = false;
        }
        if (map.getTerrainProximity(tilePos) != 0 && map.getAItile(tilePos).narrowSpace)
        {
            allow = false;
        }
    }
    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type is MovementConnection.MovementType.ShortCut or
            MovementConnection.MovementType.BetweenRooms)
        {
            allow = false;
        }
        else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
        {
            allow = true;
        }
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absLiz) => new LizardAI(absLiz, absLiz.world);
    public override Creature CreateRealizedCreature(AbstractCreature absLiz) => new GorditoGreenie(absLiz, absLiz.world);
    public override CreatureState CreateState(AbstractCreature absLiz) => new LizardState(absLiz);
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.GreenLizard;
}