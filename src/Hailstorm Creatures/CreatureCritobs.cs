namespace Hailstorm;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

sealed class RavenCritob : Critob
{

    public Color RavenColor = Color.white; // Custom.HSL2RGB(248 / 360f, 0.15f, 0.17f);

    internal RavenCritob() : base(HSEnums.CreatureType.Raven)
    {
        Icon = new SimpleIcon("Kill_Raven", RavenColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.6f, 0.4f);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(10), HSEnums.SandboxUnlock.Raven);
    }
    public override int ExpeditionScore() => 10;

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
            {
                allow = true;
            }

            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
            {
                allow = true;
            }
        }
        else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
        {
            if (map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
            {
                allow = true;
            }

            if (map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
            {
                allow = true;
            }
        }
    }

    public override Color DevtoolsMapColor(AbstractCreature absCnt) => RavenColor;
    public override string DevtoolsMapName(AbstractCreature absCnt) => "Rvn";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.Flying,
            RoomAttractivenessPanel.Category.Dark,
            RoomAttractivenessPanel.Category.LikesInside,
            RoomAttractivenessPanel.Category.LikesOutside
        };
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "raven", "Raven" };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate Raven = new CreatureFormula(CreatureTemplate.Type.Vulture, Type, "Raven")
        {
            TileResistances = new()
            {
                Air = new(1, PathCost.Legality.Allowed),
                OffScreen = new(1, PathCost.Legality.Allowed),
                Floor = new(1, PathCost.Legality.Allowed),
                Climb = new(1, PathCost.Legality.Allowed),
                Wall = new(1, PathCost.Legality.Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new(1, PathCost.Legality.Allowed),
                OutsideRoom = new(1, PathCost.Legality.Allowed),
                SkyHighway = new(1, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed),
                BetweenRooms = new(8, PathCost.Legality.Allowed),
                OpenDiagonal = new(1, PathCost.Legality.Allowed),
                ReachOverGap = new(1, PathCost.Legality.Allowed),
                NPCTransportation = new(25, PathCost.Legality.Allowed),
                ShortCut = new(4, PathCost.Legality.Allowed),
            },
            DamageResistances = new() { Base = 6f },
            StunResistances = new() { Base = 4.25f },
            InstantDeathDamage = 8,
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Vulture),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0)
        }.IntoTemplate();
        Raven.meatPoints = 8;
        Raven.bodySize = 4f;
        Raven.BlizzardAdapted = true;
        Raven.BlizzardWanderer = true;
        Raven.shortcutSegments = 4;
        Raven.dangerousToPlayer = 0.55f;
        Raven.communityInfluence = 0.5f;

        Raven.visualRadius = 10000;
        Raven.movementBasedVision = 0.7f;
        Raven.throughSurfaceVision = 1;
        Raven.waterVision = 1;
        Raven.lungCapacity = 800f;

        Raven.offScreenSpeed = 1;
        Raven.abstractedLaziness = 0;
        Raven.interestInOtherAncestorsCatches = 0;
        Raven.interestInOtherCreaturesCatches = 0.5f;
        Raven.forbidStandardShortcutEntry = false;
        Raven.usesNPCTransportation = true;
        Raven.stowFoodInDen = true;
        return Raven;
    }
    public override void EstablishRelationships()
    {
        // Relationship types that work with Vulture AI:
        // * Eats - Grabs prey and brings it back to its den.
        // * Attacks - Will seek out their target and stay in a room as long as necessary to hunt them down. Will deal enough damage with its bites to kill Slugcats, but will not hold onto their target.
        // Any relationship types not listed are not supported by base-game or DLC code, and will act like Ignores without new code.
        Relationships Raven = new(HSEnums.CreatureType.Raven);

        Raven.IsInPack(HSEnums.CreatureType.Raven, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absVul) => new VultureAI(absVul, absVul.world);
    public override Creature CreateRealizedCreature(AbstractCreature absVul) => new Vulture(absVul, absVul.world);
    public override CreatureState CreateState(AbstractCreature absVul) => new Vulture.VultureState(absVul);
	public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absVul) => new VultureAbstractAI(absVul.world, absVul);
	
    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Vulture;
    #nullable disable
}

//----------------------------------------------------------------------------------

sealed class PeachSpiderCritob : Critob
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

	#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BigSpider;
#nullable disable
}

//----------------------------------------------------------------------------------

sealed class LuminescipedeCritob : Critob
{

    public Color LuminescipedeColor = Custom.hexToColor("DCCCFF");

    internal LuminescipedeCritob() : base(HSEnums.CreatureType.Luminescipede)
    {
        Icon = new SimpleIcon("Kill_Luminescipede", LuminescipedeColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.4f, 0.6f);
        RegisterUnlock(KillScore.Configurable(2), HSEnums.SandboxUnlock.Luminescipede);
    }
    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature absLmn) => LuminescipedeColor;
    public override string DevtoolsMapName(AbstractCreature absLmn) => "lmn";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[] { RoomAttractivenessPanel.Category.All };
    }
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "lumin", "Lumin", "luminescipede", "Luminescipede" };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(null, Type, "Luminescipede")
        {
            TileResistances = new()
            {
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(20, PathCost.Legality.Unwanted),
                Climb = new(3, PathCost.Legality.Allowed),
                Wall = new(2, PathCost.Legality.Allowed),
                Ceiling = new(5, PathCost.Legality.Allowed),
                OffScreen = new(1, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1, PathCost.Legality.Allowed),
                OpenDiagonal = new(2, PathCost.Legality.Allowed),
                NPCTransportation = new(10, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed),
                BetweenRooms = new(1, PathCost.Legality.Allowed),
                CeilingSlope = new(1, PathCost.Legality.Allowed),
                DropToFloor = new(3, PathCost.Legality.Allowed),
                DropToClimb = new(3, PathCost.Legality.Allowed),
                DropToWater = new(10, PathCost.Legality.Allowed),
                ShortCut = new(2, PathCost.Legality.Allowed),
                Slope = new(1, PathCost.Legality.Allowed)
            },
            DamageResistances = new() { Base = 1, Electric = 5/3f, Blunt = 0.5f },
            StunResistances = new()   { Base = 1, Electric = 5/3f, Blunt = 0.5f },
            InstantDeathDamage = 1.5f,
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Snail),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1)
        }.IntoTemplate();
        cf.meatPoints = 1;
        cf.bodySize = 0.75f;
        cf.grasps = 1;
        cf.countsAsAKill = 2;
        cf.dangerousToPlayer = 0.25f;
        cf.communityID = CreatureCommunities.CommunityID.None;
        cf.communityInfluence = 0.25f;
		cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.shortcutSegments = 2;
        cf.scaryness = 1.5f;
		
        cf.visualRadius = 900;
        cf.throughSurfaceVision = 0.5f;
        cf.waterVision = 0.5f;
        cf.waterPathingResistance = 50f;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        cf.lungCapacity = 1200f;
        cf.canSwim = true;
		
        cf.usesCreatureHoles = true;
        cf.usesNPCTransportation = true;
        cf.requireAImap = true;
        cf.doPreBakedPathing = false;
        cf.offScreenSpeed = 0.75f;
        cf.abstractedLaziness = 50;
        cf.roamInRoomChance = 0.5f;
        cf.roamBetweenRoomsChance = 0.5f;
        cf.interestInOtherAncestorsCatches = 0;
        cf.interestInOtherCreaturesCatches = 0.5f;
        cf.stowFoodInDen = true;
        
        cf.pickupAction = "Grab | Hold - Swap Grasps";
        cf.throwAction = "Release / Throw";
        cf.jumpAction = "Camouflage";

        return cf;
    }
    public override void EstablishRelationships()
    {
        Relationships Lumin = new(HSEnums.CreatureType.Luminescipede);

        // Hunts down and chomps away at these creatures with the help of other Lumins, whittling them down over time.
        // If the creature is dead, the Lumins carry it to their den.
        Lumin.Eats(CreatureTemplate.Type.CicadaA, 0.75f);
        Lumin.Eats(CreatureTemplate.Type.CicadaB, 0.75f);
        Lumin.Eats(CreatureTemplate.Type.Centiwing, 0.75f);
        Lumin.Eats(CreatureTemplate.Type.JetFish, 0.50f);
        Lumin.Eats(HSEnums.CreatureType.SnowcuttleTemplate, 0.50f);
        Lumin.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.25f);
        Lumin.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.25f);
        Lumin.Eats(CreatureTemplate.Type.TubeWorm, 0.25f);
        Lumin.Eats(CreatureTemplate.Type.SeaLeech, 0.25f);
        Lumin.Eats(CreatureTemplate.Type.Leech, 0.2f);
        Lumin.Eats(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.15f);
        Lumin.Eats(CreatureTemplate.Type.Fly, 0.1f);

        // Hunts down and chomps away at these creatures with the help of other Lumins, whittling them down over time.
        // If the creature is dead, the Lumins will leave the corpse be.
        Lumin.Attacks(CreatureTemplate.Type.EggBug, 1);
        Lumin.Attacks(CreatureTemplate.Type.PoleMimic, 1);
        Lumin.Attacks(CreatureTemplate.Type.TentaclePlant, 1);
        Lumin.Attacks(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 1);

        // Moves away from these creatures if they get too close.
        Lumin.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        Lumin.IntimidatedBy(HSEnums.CreatureType.Chillipede, 0.75f);
        Lumin.IntimidatedBy(CreatureTemplate.Type.Snail, 0.50f);

        // Flees from these creatures on sight, dropping anything that will weigh them down and using items to defend themselves.
        Lumin.Fears(CreatureTemplate.Type.BigEel, 1);
        Lumin.Fears(CreatureTemplate.Type.BrotherLongLegs, 1);
        Lumin.Fears(CreatureTemplate.Type.DaddyLongLegs, 1);
        Lumin.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        Lumin.Fears(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 1);
        Lumin.Fears(CreatureTemplate.Type.RedLizard, 1);
        Lumin.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
        Lumin.Fears(HSEnums.CreatureType.GorditoGreenieLizard, 1);
        Lumin.Fears(HSEnums.CreatureType.FreezerLizard, 1);
        Lumin.Fears(CreatureTemplate.Type.MirosBird, 0.5f);

        // Socializes with these creatures, and works with them to hunt down prey.
        Lumin.IsInPack(HSEnums.CreatureType.Luminescipede, 1);
        //s.IsInPack(HailstormEnums.StrobeLegs, 1);

        Lumin.Ignores(CreatureTemplate.Type.Overseer);
        Lumin.Ignores(CreatureTemplate.Type.GarbageWorm);
        Lumin.Ignores(CreatureTemplate.Type.Deer);
        Lumin.Ignores(CreatureTemplate.Type.TempleGuard);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        // May run from these creatures if they don't have the numbers or equipment to fight back.
        Lumin.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.5f);
        Lumin.EatenBy(CreatureTemplate.Type.BlackLizard, 1);
        Lumin.EatenBy(CreatureTemplate.Type.BigEel, 1);
        Lumin.EatenBy(HSEnums.CreatureType.GorditoGreenieLizard, 1);
        Lumin.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.9f);
        Lumin.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.8f);
        Lumin.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.7f);
        Lumin.EatenBy(CreatureTemplate.Type.BigSpider, 0.6f);
        Lumin.EatenBy(HSEnums.CreatureType.FreezerLizard, 0.6f);
        Lumin.EatenBy(HSEnums.CreatureType.Raven, 0.6f);
        Lumin.EatenBy(CreatureTemplate.Type.MirosBird, 0.4f);
        Lumin.EatenBy(CreatureTemplate.Type.GreenLizard, 0.3f);
        Lumin.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.3f);
        Lumin.EatenBy(CreatureTemplate.Type.Spider, 0.3f);
        Lumin.EatenBy(CreatureTemplate.Type.Vulture, 0.2f);
        Lumin.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.15f);
        Lumin.EatenBy(CreatureTemplate.Type.KingVulture, 0.1f);

        Lumin.FearedBy(CreatureTemplate.Type.VultureGrub, 1);
        Lumin.FearedBy(CreatureTemplate.Type.Hazer, 1);
        Lumin.FearedBy(CreatureTemplate.Type.LanternMouse, 0.75f);
        Lumin.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.5f);
        Lumin.FearedBy(CreatureTemplate.Type.Scavenger, 0.5f);
        Lumin.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.5f);
        Lumin.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.5f);
        Lumin.FearedBy(HSEnums.CreatureType.PeachSpider, 0.5f);
        Lumin.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.25f);
        Lumin.FearedBy(HSEnums.CreatureType.InfantAquapede, 0.25f);

        Lumin.IgnoredBy(CreatureTemplate.Type.WhiteLizard);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absLmn) => new LuminAI(absLmn, absLmn.world);
    public override Creature CreateRealizedCreature(AbstractCreature absLmn) => new Luminescipede(absLmn, absLmn.world);
    public override CreatureState CreateState(AbstractCreature absLmn) => new GlowSpiderState(absLmn);

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Spider;
#nullable disable
}

//----------------------------------------------------------------------------------
