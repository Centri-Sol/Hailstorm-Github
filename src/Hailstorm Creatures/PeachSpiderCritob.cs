namespace Hailstorm;

public class PeachSpiderCritob : Critob
{

    public Color PeachSpiderColor = Custom.HSL2RGB(209 / 360f, 0.9f, 0.75f);

    internal PeachSpiderCritob() : base(HSEnums.CreatureType.PeachSpider)
    {
        Icon = new SimpleIcon("Kill_Peach_Spider", PeachSpiderColor);
        LoadedPerformanceCost = 40f;
        SandboxPerformanceCost = new(0.6f, 0.6f);
        ShelterDanger = ShelterDanger.Hostile;
        RegisterUnlock(KillScore.Configurable(2), HSEnums.SandboxUnlock.PeachSpider);
    }
    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature absSpd) => PeachSpiderColor;
    public override string DevtoolsMapName(AbstractCreature absSpd) => "Pch";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[] { RoomAttractivenessPanel.Category.LikesOutside };
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "peachspider", "PeachSpider" };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(CreatureTemplate.Type.BigSpider, Type, "Peach Spider")
        {
            TileResistances = new()
            {
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(1, PathCost.Legality.Allowed),
                Climb = new(2, PathCost.Legality.Allowed),
                Wall = new(3, PathCost.Legality.Allowed),
                Ceiling = new(4, PathCost.Legality.Allowed),
                OffScreen = new(1, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, PathCost.Legality.Allowed),
                OpenDiagonal = new(3, PathCost.Legality.Allowed),
                ReachOverGap = new(3, PathCost.Legality.Allowed),
                ReachUp = new(2, PathCost.Legality.Allowed),
                ReachDown = new(2, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2, PathCost.Legality.Allowed),
                DropToFloor = new(4, PathCost.Legality.Allowed),
                DropToWater = new(8, PathCost.Legality.Allowed),
                DropToClimb = new(4, PathCost.Legality.Allowed),
                ShortCut = new(2, PathCost.Legality.Allowed),
                NPCTransportation = new(2, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed),
                BetweenRooms = new(5, PathCost.Legality.Allowed),
                Slope = new(1.5f, PathCost.Legality.Allowed),
                CeilingSlope = new(1.5f, PathCost.Legality.Allowed)
            },
            DamageResistances = new() { Base = 0.4f },
            StunResistances = new() { Base = 2 },
            InstantDeathDamage = 0.8f,
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BigSpider),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0),
        }.IntoTemplate();
        cf.meatPoints = 2;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.bodySize = 0.3f;
        cf.shortcutSegments = 1;
        cf.communityInfluence = 0.02f;

        cf.visualRadius = 1200;
        cf.waterPathingResistance = 4f;

        cf.offScreenSpeed = 0.5f;
        cf.abstractedLaziness = 10;
        return cf;
    }
    public override void EstablishRelationships()
    {
        // Relationship types that work with BigSpider AI:
        // * Both Eats and Attacks - Lunges at prey and brings it back to its den.
        // * Afraid - Runs away from the target.
        // Any relationship types not listed are not supported by base-game or DLC code, and will act like Ignores without new code.
        Relationships pchSpd = new(HSEnums.CreatureType.PeachSpider);

        pchSpd.Eats(CreatureTemplate.Type.Spider, 0.7f);
        pchSpd.Eats(CreatureTemplate.Type.Fly, 0.7f);
        pchSpd.Eats(HSEnums.CreatureType.SnowcuttleTemplate, 0.4f);
        pchSpd.Eats(CreatureTemplate.Type.SmallCentipede, 0.4f);
        pchSpd.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.225f);
        pchSpd.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.225f);
        pchSpd.Eats(CreatureTemplate.Type.VultureGrub, 0.2f);
        pchSpd.Eats(CreatureTemplate.Type.Hazer, 0.2f);

        pchSpd.Fears(HSEnums.CreatureType.Raven, 1);
        pchSpd.Fears(HSEnums.CreatureType.Luminescipede, 1);
        pchSpd.Fears(HSEnums.CreatureType.Cyanwing, 1);
        pchSpd.Fears(CreatureTemplate.Type.RedCentipede, 1);
        pchSpd.Fears(CreatureTemplate.Type.SpitterSpider, 1);
        pchSpd.Fears(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 1);
        pchSpd.Fears(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);
        pchSpd.Fears(CreatureTemplate.Type.Centipede, 0.5f);
        pchSpd.Fears(CreatureTemplate.Type.BigSpider, 0.5f);

        pchSpd.Ignores(HSEnums.CreatureType.PeachSpider);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        pchSpd.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.4f);
        pchSpd.EatenBy(CreatureTemplate.Type.Spider, 0.5f);
        pchSpd.EatenBy(CreatureTemplate.Type.BigSpider, 0.5f);
        pchSpd.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.6f);
        pchSpd.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absSpd) => new BigSpiderAI(absSpd, absSpd.world);
    public override Creature CreateRealizedCreature(AbstractCreature absSpd) => new BigSpider(absSpd, absSpd.world);
    public override CreatureState CreateState(AbstractCreature absSpd) => new HealthState(absSpd);
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.BigSpider;
}