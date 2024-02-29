namespace Hailstorm;

public class ChillipedeCritob : Critob
{
    public Color ChillipedeColor = new HSLColor(198 / 360f, 1, 0.75f).rgb;

    internal ChillipedeCritob() : base(HSEnums.CreatureType.Chillipede)
    {
        Icon = new SimpleIcon("Kill_Chillipede", ChillipedeColor);
        LoadedPerformanceCost = 15f;
        SandboxPerformanceCost = new(0.9f, 0.75f);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(14), HSEnums.SandboxUnlock.Chillipede);
    }
    public override int ExpeditionScore() => 14;

    public override Color DevtoolsMapColor(AbstractCreature absChl) => ChillipedeColor;

    public override string DevtoolsMapName(AbstractCreature absChl) => "chl";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside };

    public override IEnumerable<string> WorldFileAliases() => new[] { "chillipede", "Chillipede" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate chllpd = new CreatureFormula(CreatureTemplate.Type.Centipede, Type, "Chillipede")
        {
            TileResistances = new()
            {
                OffScreen = new(2, PathCost.Legality.Allowed),
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(25, PathCost.Legality.Allowed),
                Climb = new(1, PathCost.Legality.Allowed),
                Ceiling = new(100, PathCost.Legality.Allowed),
                Wall = new(1, PathCost.Legality.Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new(1, PathCost.Legality.Allowed),
                ReachOverGap = new(3, PathCost.Legality.Allowed),
                ReachUp = new(5, PathCost.Legality.Allowed),
                DoubleReachUp = new(20, PathCost.Legality.Allowed),
                ReachDown = new(1.1f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2, PathCost.Legality.Allowed),
                DropToFloor = new(10, PathCost.Legality.Allowed),
                DropToClimb = new(7, PathCost.Legality.Allowed),
                DropToWater = new(50, PathCost.Legality.Unwanted),
                OpenDiagonal = new(3, PathCost.Legality.Allowed),
                Slope = new(2, PathCost.Legality.Allowed),
                CeilingSlope = new(100, PathCost.Legality.Allowed),
                ShortCut = new(20, PathCost.Legality.Allowed),
                NPCTransportation = new(50, PathCost.Legality.Allowed),
                BigCreatureShortCutSqueeze = new(10, PathCost.Legality.Allowed),
                BetweenRooms = new(10, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed)
            },
            DamageResistances = new() { Base = 5f, Explosion = 2 / 3f, Electric = 0.5f, Water = 4f },
            StunResistances = new() { Base = 3f, Explosion = 2 / 3f, Electric = 0.5f, Water = 4f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Centipede),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1),
        }.IntoTemplate();
        chllpd.meatPoints = 10;
        chllpd.BlizzardAdapted = true;
        chllpd.BlizzardWanderer = true;
        chllpd.bodySize = 7;

        chllpd.visualRadius = 1000;
        chllpd.movementBasedVision = 1f;
        chllpd.throughSurfaceVision = 0.25f;
        chllpd.waterVision = 0.3f;
        chllpd.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        chllpd.lungCapacity = 640;

        chllpd.stowFoodInDen = true;
        chllpd.usesCreatureHoles = false;
        chllpd.dangerousToPlayer = 0.45f;
        chllpd.communityInfluence = 0.1f;
        chllpd.shortcutSegments = 5;
        chllpd.jumpAction = "Swap Heads";
        chllpd.pickupAction = "Grab/Freeze";
        chllpd.throwAction = "Release";
        return chllpd;
    }

    public override void EstablishRelationships()
    {
        Relationships Chl = new(HSEnums.CreatureType.Chillipede);

        Chl.Eats(CreatureTemplate.Type.Spider, 0.4f);
        Chl.Eats(CreatureTemplate.Type.BigSpider, 0.4f);
        Chl.Eats(CreatureTemplate.Type.SpitterSpider, 0.4f);
        Chl.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.4f);
        Chl.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.4f);
        Chl.Eats(CreatureTemplate.Type.SmallCentipede, 0.4f);
        Chl.Eats(CreatureTemplate.Type.Centipede, 0.4f);
        Chl.Eats(CreatureTemplate.Type.Centiwing, 0.4f);
        Chl.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        Chl.Eats(HSEnums.CreatureType.InfantAquapede, 0.4f);

        Chl.Fears(CreatureTemplate.Type.RedCentipede, 1);
        Chl.Fears(HSEnums.CreatureType.Cyanwing, 1);
        Chl.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.8f);

        Chl.Ignores(HSEnums.CreatureType.Chillipede);
        Chl.Ignores(HSEnums.CreatureType.IcyBlueLizard);
        Chl.Ignores(HSEnums.CreatureType.FreezerLizard);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        Chl.FearedBy(CreatureTemplate.Type.LizardTemplate, 0.3f);

        Chl.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.2f);
        Chl.EatenBy(CreatureTemplate.Type.RedLizard, 0.2f);
        Chl.EatenBy(CreatureTemplate.Type.GreenLizard, 0.4f);
        Chl.EatenBy(CreatureTemplate.Type.SmallCentipede, 0.4f);
        Chl.EatenBy(CreatureTemplate.Type.Centipede, 0.4f);
        Chl.EatenBy(CreatureTemplate.Type.RedCentipede, 0.4f);
        Chl.EatenBy(CreatureTemplate.Type.Centiwing, 0.4f);
        Chl.EatenBy(HSEnums.CreatureType.Cyanwing, 0.4f);
        Chl.EatenBy(HSEnums.CreatureType.InfantAquapede, 0.4f);
        Chl.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.6f);
        Chl.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.8f);
        Chl.EatenBy(HSEnums.CreatureType.GorditoGreenieLizard, 1);

        Chl.Intimidates(CreatureTemplate.Type.BigSpider, 0.5f);
        Chl.Intimidates(HSEnums.CreatureType.Luminescipede, 0.75f);

        Chl.FearedBy(CreatureTemplate.Type.Spider, 0.5f);
        Chl.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.7f);

        Chl.IgnoredBy(HSEnums.CreatureType.IcyBlueLizard);
        Chl.IgnoredBy(HSEnums.CreatureType.FreezerLizard);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absChl) => new ChillipedeAI(absChl, absChl.world);

    public override Creature CreateRealizedCreature(AbstractCreature absChl) => new Chillipede(absChl, absChl.world);

    public override CreatureState CreateState(AbstractCreature absChl) => new ChillipedeState(absChl);

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Centipede;
}