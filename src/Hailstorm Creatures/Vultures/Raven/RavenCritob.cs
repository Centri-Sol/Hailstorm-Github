namespace Hailstorm;

public class RavenCritob : Critob
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

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absVul) => new RavenAI(absVul, absVul.world);
    public override Creature CreateRealizedCreature(AbstractCreature absVul) => new Raven(absVul, absVul.world);
    public override CreatureState CreateState(AbstractCreature absVul) => new Vulture.VultureState(absVul);
    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absVul) => new VultureAbstractAI(absVul.world, absVul);
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Vulture;
}