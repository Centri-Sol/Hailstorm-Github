namespace Hailstorm;

public class LuminAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction, IUseItemTracker
{
    public Luminescipede Lmn => creature.realizedCreature as Luminescipede;
    public GlowSpiderState GlowState => Lmn.State as GlowSpiderState;
    public Role Role => GlowState.role;
    public Behavior Behavior => GlowState.behavior;
    public virtual Vector2 LmnPos => Lmn.DangerPos;
    public virtual bool HasValidDen =>
        denFinder?.denPosition is not null &&
        denFinder.denPosition.Value.NodeDefined &&
        creature.world.GetAbstractRoom(denFinder.denPosition.Value) is not null &&
        creature.world.GetNode(denFinder.denPosition.Value).type == AbstractRoomNode.Type.Den;

    //----------------------

    public float currentUtility;


    public WorldCoordinate nextForageSpot;
    public WorldCoordinate forageSpot;
    public WorldCoordinate? forageAtPosition;
    private int forageSpotCounter;
    public List<WorldCoordinate> prevForageSpots = new();


    private MovementConnection lastFollowedConnection;
    private Vector2 travelDir;
    public virtual float MovementDesire
    {
        get
        {
            if (!Lmn.Consious || Behavior == Hide || Behavior == Overloaded || Lmn.flashbombTimer > 40 || (Behavior == Rush && GlowState.rushPreyCounter < 24))
            {
                return 0;
            }
            float desire = 1;
            if (Behavior == Idle)
            {
                desire = 0.75f;
            }
            if (Behavior == Aggravated)
            {
                desire = 1.15f;
            }
            if (Behavior == Rush)
            {
                desire = 1.3f;
            }

            if (Role == Forager)
            {
                desire *= 1.2f;
            }

            if (Lmn.bloodlust > 1)
            {
                desire += (Lmn.bloodlust - 1) / 10f;
            }
            if (Lmn.CamoFac > 0)
            {
                desire *= Mathf.Max(0, 1 - Lmn.CamoFac);
            }

            if (Lmn.flashbombTimer > 0)
            {
                desire *= 1f - (Lmn.flashbombTimer / 40f);
            }

            return desire;
        }
    }
    public float speedMult;
    public float MovementSpeed;
    public float migrationTimer;

    private int specialMoveCounter;
    private IntVector2 specialMoveDestination;

    public bool inAccessibleTerrain;

    //----------------------

    public WorldCoordinate? PreyPos;

    public int noiseRectionDelay;

    //----------------------

    public LuminAI(AbstractCreature absLmn, World world) : base(absLmn, world)
    {
        Lmn.AI = this;
        AddModule(new StandardPather(this, world, absLmn));
        pathFinder.stepsPerFrame = 5;
        pathFinder.accessibilityStepsPerFrame = 10;
        AddModule(new Tracker(this, 5, 10, 1200, 0.5f, 5, 5, 10));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new ItemTracker(this, 5, 10, 600, 50, true));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, absLmn));
        AddModule(new StuckTracker(this, true, false));
        stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(stuckTracker));
        stuckTracker.minStuckCounter = 320;
        stuckTracker.maxStuckCounter = 640;
        stuckTracker.totalTrackedLastPositions = 30;
        stuckTracker.checkPastPositionsFrom = 15;
        stuckTracker.pastStuckPositionsCloseToIncrementStuckCounter = 10;
        AddModule(new NoiseTracker(this, tracker));
        prevForageSpots = new List<WorldCoordinate>();
    }

    //--------------------------------------------------

    public override void Update()
    {
        try
        {
            if (Lmn?.room is null)
            {
                return;
            }

            base.Update();

            if (Lmn.LickedByPlayer is not null)
            {
                tracker.SeeCreature(Lmn.LickedByPlayer.abstractCreature);
            }

            pathFinder.walkPastPointOfNoReturn = stranded || !denFinder.denPosition.HasValue || !pathFinder.CoordinatePossibleToGetBackFrom(denFinder.denPosition.Value) || threatTracker.Utility() > 0.96f;

            noiseTracker.hearingSkill = Behavior == Overloaded ? 0 : Behavior == Aggravated ? 0.3f : 1f;

            if (Behavior != Overloaded && Lmn.Role == Forager)
            {
                noiseTracker.hearingSkill += 0.2f;
            }

            if (rainTracker.Utility() > 0.35f)
            {
                GlowState.ChangeBehavior(EscapeRain, 0);
            }


            if (Lmn.shortcutDelay < 1)
            {
                ConsiderTrackedCreature();
                ConsiderTrackedItem();
            }
            if (Lmn.flock?.lumins is not null &&
                Lmn.flock.lumins.Count > 0)
            {
                for (int l = Lmn.flock.lumins.Count - 1; l >= 0; l--)
                {

                    Luminescipede otherLmn = Lmn.flock.lumins[l];

                    if (otherLmn.dead ||
                        otherLmn.abstractCreature.pos.room != creature.pos.room)
                    {
                        Lmn.flock.RemoveLmn(otherLmn);
                        continue;
                    }

                    if (!HasValidDen &&
                        otherLmn.AI.HasValidDen &&
                        otherLmn.GlowState.ivars.dominance >= GlowState.ivars.dominance)
                    {
                        denFinder.denPosition = otherLmn.AI.denFinder.denPosition.Value;
                    }
                }
            }
            if (Lmn.currentPrey is not null)
            {
                if (Lmn.currentPrey == Lmn)
                {
                    Lmn.currentPrey = null;
                    PreyPos = null;
                }
                else
                {
                    PreyPos = Lmn.room.GetWorldCoordinate(Lmn.MainChunkOfObject(Lmn.currentPrey).pos);
                }
            }
            else if (PreyPos.HasValue)
            {
                PreyPos = null;
            }


            PathingDestination();

            inAccessibleTerrain = Lmn.lungeTimer < 20 && Lmn.room.aimap.TileAccessibleToCreature(LmnPos, Lmn.Template.preBakedPathingAncestor);

            if (Lmn.lunging && Lmn.lungeTimer == 0 && inAccessibleTerrain)
            {
                Lmn.lunging = false;
            }

            if (Lmn.lungeTimer > 0 || Behavior == Hide || Behavior == Overloaded || (Behavior == Rush && GlowState.rushPreyCounter < 24))
            {
                MovementSpeed = 0;
            }
            else if (MovementSpeed != MovementDesire)
            {
                MovementSpeed = Custom.LerpAndTick(MovementSpeed, MovementDesire, 0.01f, 0.01f);
            }

            if (Lmn.Consious && Lmn.lungeTimer == 0 && !Lmn.lunging)
            {
                if (!Lmn.safariControlled)
                {
                    NormalMovement();
                }
                else
                {
                    SafariMovement();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("[Hailstorm] Something is breaking with Luminescipede AI! Report this ASAP: " + e);
        }
    }

    public virtual void NormalMovement()
    {
        if (Lmn.Submersion > 0.3f)
        {
            WaterPathfinding();
            return;
        }

        if (specialMoveCounter > 0)
        {
            specialMoveCounter--;
            MoveTowards(Lmn.room.MiddleOfTile(specialMoveDestination));
            travelDir = Vector2.Lerp(travelDir, Custom.DirVec(LmnPos, Lmn.room.MiddleOfTile(specialMoveDestination)), 0.4f);
            if (Custom.DistLess(LmnPos, Lmn.room.MiddleOfTile(specialMoveDestination), 5))
            {
                specialMoveCounter = 0;
            }
        }
        else
        {
            if (Lmn.room.GetWorldCoordinate(LmnPos) == pathFinder.GetDestination && Lmn.fearSource is null && !Lmn.safariControlled)
            {
                Lmn.GoThroughFloors = false;
            }
            else
            {
                _ = (pathFinder as StandardPather).FollowPath(Lmn.room.GetWorldCoordinate(LmnPos), actuallyFollowingThisPath: true);
                MovementConnection movementConnection = (pathFinder as StandardPather).FollowPath(Lmn.room.GetWorldCoordinate(LmnPos), actuallyFollowingThisPath: true);
                if (Lmn.safariControlled && (movementConnection == default || !Lmn.AllowableControlledAIOverride(movementConnection.type)))
                {
                    movementConnection = default;
                    if (Lmn.inputWithDiagonals.HasValue && Behavior != Hide)
                    {
                        MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                        if (Lmn.shortcutDelay == 0 && Lmn.room.GetTile(LmnPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                        {
                            type = MovementConnection.MovementType.ShortCut;
                        }
                        if (Lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                        {
                            movementConnection = new MovementConnection(type, Lmn.room.GetWorldCoordinate(LmnPos), Lmn.room.GetWorldCoordinate(LmnPos + (new Vector2(Lmn.inputWithDiagonals.Value.x, Lmn.inputWithDiagonals.Value.y) * 40f)), 2);
                        }
                        Lmn.GoThroughFloors = Lmn.inputWithDiagonals.Value.y < 0;
                    }
                }
                if (movementConnection != default)
                {
                    NormalPathfinding(movementConnection);
                }
                else
                {
                    Lmn.GoThroughFloors = false;
                }
            }
        }

    }
    public virtual void SafariMovement()
    {
        if (Lmn.inputWithDiagonals.HasValue)
        {
            Vector2 inputAngle = new Vector2(Lmn.inputWithDiagonals.Value.x, Lmn.inputWithDiagonals.Value.y) * 20;
            List<Vector2> posList = new() { LmnPos, LmnPos + inputAngle };
            for (int p = 0; p < posList.Count; p++)
            {
                if (Lmn.shortcutDelay == 0 &&
                    Lmn.room.GetTile(posList[p]).Terrain == Room.Tile.TerrainType.ShortcutEntrance && (
                    (Lmn.room.ShorcutEntranceHoleDirection(Lmn.room.GetTilePosition(posList[p])).x != 0 && -Lmn.inputWithDiagonals.Value.x == Lmn.room.ShorcutEntranceHoleDirection(Lmn.room.GetTilePosition(posList[p])).x) ||
                    (Lmn.room.ShorcutEntranceHoleDirection(Lmn.room.GetTilePosition(posList[p])).y != 0 && -Lmn.inputWithDiagonals.Value.y == Lmn.room.ShorcutEntranceHoleDirection(Lmn.room.GetTilePosition(posList[p])).y)))
                {
                    Lmn.enteringShortCut = Lmn.room.GetTilePosition(posList[p]);
                    MovementConnection movementConnection = (pathFinder as StandardPather).FollowPath(Lmn.room.GetWorldCoordinate(Lmn.enteringShortCut.Value), true);
                    if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                    {
                        bool atWackamoleEntrance = false;
                        List<IntVector2> wackamoleExits = new();
                        ShortcutData[] shortcuts = Lmn.room.shortcuts;
                        for (int i = 0; i < shortcuts.Length; i++)
                        {
                            ShortcutData shortcutData = shortcuts[i];
                            if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
                            {
                                wackamoleExits.Add(shortcutData.StartTile);
                            }
                            if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
                            {
                                atWackamoleEntrance = true;
                            }
                        }
                        if (atWackamoleEntrance && wackamoleExits.Count > 0)
                        {
                            Lmn.NPCTransportationDestination = Lmn.room.GetWorldCoordinate(wackamoleExits[Random.Range(0, wackamoleExits.Count)]);
                        }
                    }
                    break;
                }
            }
            if (Lmn.Submersion > 0 || inAccessibleTerrain)
            {
                if (Lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                {
                    MoveTowards(posList[1]);
                }
            }
            else
            {
                float tileCheckAngle = Custom.VecToDeg(inputAngle);
                for (int d = 0; d < 4; d++)
                {
                    if (Lmn.room.aimap.TileAccessibleToCreature(LmnPos + (50f * Custom.DegToVec(tileCheckAngle + (90f * d))), Lmn.Template.preBakedPathingAncestor))
                    {
                        inAccessibleTerrain = true;
                        if (Lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                        {
                            MoveTowards(posList[1]);
                        }
                        break;
                    }
                }
            }

        }
        if (Lmn.Submersion > 0.3f)
        {
            WaterPathfinding();
        }

    }
    public virtual void NormalPathfinding(MovementConnection followingConnection)
    {
        if (followingConnection.type == MovementConnection.MovementType.ReachUp)
        {
            (pathFinder as StandardPather).pastConnections.Clear();
        }
        if (Lmn.shortcutDelay < 1 &&
            (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation) &&
            (Lmn.grasps[0]?.grabbed is null || Lmn.grasps[0].grabbed is not Creature carried || carried.dead))
        {
            Lmn.enteringShortCut = followingConnection.StartTile;
            if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                Lmn.NPCTransportationDestination = followingConnection.destinationCoord;
            }
        }
        else if (
            followingConnection.type is MovementConnection.MovementType.OpenDiagonal or
            MovementConnection.MovementType.ReachOverGap or
            MovementConnection.MovementType.ReachUp or
            MovementConnection.MovementType.ReachDown or
            MovementConnection.MovementType.SemiDiagonalReach)
        {
            specialMoveCounter = 30;
            specialMoveDestination = followingConnection.DestTile;
        }
        else
        {
            MovementConnection movementConnection = followingConnection;
            if (stuckTracker.Utility() == 0f)
            {
                MovementConnection movementConnection2 = (pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
                if (movementConnection2 != default)
                {
                    if (movementConnection2.destinationCoord == followingConnection.startCoord)
                    {
                        return;
                    }
                    if (movementConnection2.destinationCoord.TileDefined && Lmn.room.aimap.getAItile(movementConnection2.DestTile).acc < AItile.Accessibility.Ceiling)
                    {
                        bool notAccessible = false;
                        for (int j = Math.Min(followingConnection.StartTile.x, movementConnection2.DestTile.x); j < Math.Max(followingConnection.StartTile.x, movementConnection2.DestTile.x); j++)
                        {
                            if (notAccessible)
                            {
                                break;
                            }
                            for (int k = Math.Min(followingConnection.StartTile.y, movementConnection2.DestTile.y); k < Math.Max(followingConnection.StartTile.y, movementConnection2.DestTile.y); k++)
                            {
                                if (!Lmn.room.aimap.TileAccessibleToCreature(j, k, Lmn.Template.preBakedPathingAncestor))
                                {
                                    notAccessible = true;
                                    break;
                                }
                            }
                        }
                        if (!notAccessible)
                        {
                            movementConnection = movementConnection2;
                        }
                    }
                }
            }
            Vector2 destination = Lmn.room.MiddleOfTile(movementConnection.DestTile);
            travelDir = Vector2.Lerp(travelDir, Custom.DirVec(LmnPos, destination), 0.4f);
            if (lastFollowedConnection != default && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
            {
                Lmn.Body.vel += Custom.DirVec(LmnPos, destination) * 4f;
            }
            if (followingConnection.type != MovementConnection.MovementType.DropToFloor)
            {
                if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                {
                    Lmn.Body.vel.x += Mathf.Min((destination.x - LmnPos.x) / 8f, 1.2f);
                }
                else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                {
                    Lmn.Body.vel.y += Mathf.Min((destination.y - LmnPos.y) / 8f, 1.2f);
                }
            }
            if (lastFollowedConnection != default &&
                (followingConnection.type != MovementConnection.MovementType.DropToFloor || Lmn.room.aimap.TileAccessibleToCreature(LmnPos, Lmn.Template.preBakedPathingAncestor)) && (
                (followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) ||
                (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
            {
                Lmn.Body.vel *= 0.7f;
            }
            MoveTowards(destination);
        }
        lastFollowedConnection = followingConnection;
    }
    public virtual void WaterPathfinding()
    {
        Lmn.GoThroughFloors = true;
        Lmn.Body.vel *= inAccessibleTerrain ? 0.95f : 0.8f;
        Lmn.Body.vel.y += 0.35f * (2.2f - GlowState.ivars.Size);
        if (Lmn.safariControlled)
        {
            return;
        }
        MovementConnection movementDestination = (pathFinder as StandardPather).FollowPath(Lmn.room.GetWorldCoordinate(LmnPos), actuallyFollowingThisPath: true);
        if (movementDestination == default && Math.Abs(creature.pos.y - Lmn.room.defaultWaterLevel) < 4)
        {
            movementDestination = (pathFinder as StandardPather).FollowPath(new WorldCoordinate(creature.pos.room, creature.pos.x, Lmn.room.defaultWaterLevel, creature.pos.abstractNode), actuallyFollowingThisPath: true);
        }
        if (movementDestination != default)
        {
            if (movementDestination.StartTile.y == movementDestination.DestTile.y && movementDestination.DestTile.y == Lmn.room.defaultWaterLevel)
            {
                Lmn.Body.vel.x -= Mathf.Sign(Lmn.room.MiddleOfTile(movementDestination.StartTile).x - Lmn.room.MiddleOfTile(movementDestination.DestTile).x) * Lmn.Submersion;
                return;
            }
            Lmn.Body.vel *= 0.8f;
            Lmn.Body.vel += Custom.DirVec(LmnPos, Lmn.room.MiddleOfTile(movementDestination.destinationCoord));
            Lmn.Body.vel *= Mathf.Lerp(1, 0.65f, Lmn.Submersion);
            NormalPathfinding(movementDestination);
            Lmn.Body.vel.y += 0.35f * (2.2f - GlowState.ivars.Size);
        }
    }
    public virtual void MoveTowards(Vector2 moveTo)
    {
        if (Random.value > 0.5f + (GlowState.health / 2f))
        {
            return;
        }
        float vel = (GlowState.ivars.Size + 0.2f) * Mathf.Lerp(0.5f, 1, MovementSpeed);
        Vector2 angle = Custom.DirVec(LmnPos, moveTo);
        if (!Lmn.safariControlled && Lmn.IsTileSolid(0, 0, -1))
        {
            Lmn.Body.vel.x -= 1.3f * Mathf.Sign(angle.x);
        }
        if (Lmn.GrabbingAnything)
        {
            for (int g = 0; g < Lmn.grasps.Length; g++)
            {
                if (Lmn.grasps[g]?.grabbed is not null)
                {
                    float maxMass = Lmn.AttackMassLimit / 2f;
                    if (g == 1)
                    {
                        maxMass *= 2f;
                    }
                    vel *= Custom.LerpMap(Lmn.grasps[g].grabbed.TotalMass, 0, maxMass, 1, 0.5f);
                }
            }
        }
        Lmn.Body.vel += vel * angle * 3f;
        Lmn.GoThroughFloors = moveTo.y < LmnPos.y - 5f;
    }
    public virtual void PathingDestination()
    {
        if (Lmn.safariControlled && Lmn.Consious)
        {
            GlowState.ChangeBehavior(Idle, 1);
            return;
        }

        Debug.Log("behavior: " + Behavior.value);
        if (Lmn.shortcutDelay > 0 && denFinder.denPosition.HasValue && denFinder.denPosition.Value == creature.abstractAI.destination && Custom.ManhattanDistance(creature.pos, Lmn.room.LocalCoordinateOfNode(denFinder.denPosition.Value.abstractNode)) < 3)
        {
            int x = creature.pos.x + (Random.Range(2, 5) * (Random.value < 0.5f ? 1 : -1));
            int y = creature.pos.y + (Random.Range(2, 5) * (Random.value < 0.5f ? 1 : -1));
            WorldCoordinate coord = new(Lmn.room.abstractRoom.index, x, y, -1);
            if (Lmn.room.aimap.TileAccessibleToCreature(Lmn.room.MiddleOfTile(coord), Lmn.Template.preBakedPathingAncestor) && pathFinder.CoordinateReachableAndGetbackable(coord))
            {
                creature.abstractAI.SetDestination(coord);
            }
            return;
        }

        if (Role == Forager)
        {
            if (Behavior != Idle)
            {
                forageSpot = creature.pos;
            }
            forageSpotCounter--;
        }

        if (Lmn.lungs < 0.33f)
        {
            if (Random.value < 0.2f)
            {
                creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 5, 40, true));
            }
            return;
        }

        if (migrationTimer > 0 && Behavior != Idle)
        {
            migrationTimer = 0;
        }

        if (Behavior == Hide || Behavior == Overloaded)
        {
            return;
        }

        Debug.Log("stuckTracker utility: " + stuckTracker.Utility());
        if (stuckTracker.Utility() == 1)
        {
            if (Random.value < 1f / 120f)
            {
                creature.abstractAI.SetDestination(stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
            }
        }
        else if (Behavior == Idle)
        {
            if (Role == Guardian)
            {
                if (denFinder.denPosition.HasValue && (creature.pos.room != denFinder.denPosition.Value.room || Custom.ManhattanDistance(creature.pos, Lmn.room.LocalCoordinateOfNode(denFinder.denPosition.Value.abstractNode)) > 20))
                {
                    creature.abstractAI.SetDestination(denFinder.denPosition.Value);
                }
            }
            else if (Role == Forager)
            {
                if (forageAtPosition.HasValue)
                {
                    creature.abstractAI.SetDestination(forageAtPosition.Value);
                    if (Random.value < 0.0002f || Custom.ManhattanDistance(creature.pos, forageAtPosition.Value) < 4)
                    {
                        forageAtPosition = null;
                    }
                }
                else if (!creature.abstractAI.WantToMigrate)
                {
                    WorldCoordinate coord = new(Lmn.room.abstractRoom.index, Random.Range(0, Lmn.room.TileWidth), Random.Range(0, Lmn.room.TileHeight), -1);
                    if (pathFinder.CoordinateReachableAndGetbackable(coord) && ForagePosScore(coord) < ForagePosScore(nextForageSpot))
                    {
                        nextForageSpot = coord;
                    }
                    creature.abstractAI.SetDestination(forageSpot);
                    if (Custom.ManhattanDistance(creature.pos, forageSpot) < 3 && (Lmn.room.aimap.getAItile(creature.pos).narrowSpace || TileInEnclosedArea(creature.pos.Tile)))
                    {
                        forageSpotCounter -= 4;
                    }
                    if (forageSpotCounter < 1)
                    {
                        forageSpotCounter = Random.Range(300, 560);
                        prevForageSpots.Add(forageSpot);
                        if (prevForageSpots.Count > 7)
                        {
                            prevForageSpots.RemoveAt(0);
                        }
                        forageSpot = nextForageSpot;
                        nextForageSpot = new WorldCoordinate(Lmn.room.abstractRoom.index, Random.Range(0, Lmn.room.TileWidth), Random.Range(0, Lmn.room.TileHeight), -1);
                    }
                }
                else
                {
                    if (migrationTimer < 2400) // Takes 8 seconds at 0 bloodlust.
                    {
                        migrationTimer += 3 - Lmn.bloodlust;
                    }
                    else
                    {
                        creature.abstractAI.AbstractBehavior(1);
                        if (pathFinder.GetDestination != creature.abstractAI.MigrationDestination)
                        {
                            creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
                        }
                    }
                }
            }
        }
        else if (Behavior == Hunt)
        {
            if ((!PreyPos.HasValue || Lmn.currentPrey is not Creature) && Lmn.useItem is not null && !Lmn.GrabbingItem(Lmn.useItem))
            {
                creature.abstractAI.SetDestination(Lmn.useItem.abstractPhysicalObject.pos);
            }
            else if (PreyPos.HasValue && !Lmn.GrabbingItem(Lmn.currentPrey))
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
            else if (Random.value < 1f / 300f)
            {
                //creature.abstractAI.SetDestination(lmn.room.GetWorldCoordinate(lmnPos + Custom.RNV() * Random.Range(150f, 300f)));
            }
        }
        else if (Behavior == Aggravated)
        {
            if (PreyPos.HasValue && !Custom.DistLess(LmnPos, Lmn.room.MiddleOfTile(PreyPos.Value), Lmn.Template.visualRadius * 2 / 3f))
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
        }
        else if (Behavior == Rush)
        {
            if (PreyPos.HasValue && !Custom.DistLess(LmnPos, Lmn.room.MiddleOfTile(PreyPos.Value), Lmn.Template.visualRadius * 2 / 3f))
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
        }
        else if (Behavior == Flee)
        {
            creature.abstractAI.SetDestination(
                threatTracker.FleeTo(
                    occupyTile: creature.pos,
                    reevalutaions: 5,
                    maximumDistance: 40,
                    considerLeavingRoom: Lmn.FleeLevel > 0,
                    considerGoingHome: Lmn.FleeLevel > 0 && Role == Guardian));
        }
        else if (Behavior == EscapeRain)
        {
            if (denFinder.denPosition.HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.denPosition.Value);
            }
        }
        else if (Behavior == ReturnPrey)
        {
            if (Lmn.grasps[0] is null && PreyPos.HasValue)
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
            else
            if (denFinder.denPosition.HasValue)
            {
                if ((Lmn.grasps.Length < 2 && Lmn.ConsiderPrey(Lmn.grasps[0]?.grabbed)) ||
                    (Lmn.grasps.Length > 1 && Lmn.grasps[0] is not null && Lmn.grasps[1] is not null && (Lmn.ConsiderPrey(Lmn.grasps[0].grabbed) || Lmn.ConsiderPrey(Lmn.grasps[1].grabbed))))
                {
                    creature.abstractAI.SetDestination(denFinder.denPosition.Value);
                }
            }
        }
    }

    public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
    {
        cost.resistance += Mathf.Max(0f, threatTracker.ThreatOfTile(connection.destinationCoord, accountThreatCreatureAccessibility: true) - threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true)) * 40f;
        if (Lmn?.room is null)
        {
            cost.resistance += Custom.LerpMap(Lmn.room.aimap.getAItile(connection.DestTile).smoothedFloorAltitude, 1f, 7f, 60f, 0f);
            if (Lmn.lungs < 0.33f)
            {
                cost.resistance += Lmn.room.GetTile(connection.destinationCoord).AnyWater ? 100 : -100f;
            }
            if (Lmn.HoldingThisItemType(AbstractPhysicalObject.AbstractObjectType.BubbleGrass))
            {
                cost.resistance += Lmn.room.GetTile(connection.destinationCoord).AnyWater ? -25 : 25f;
            }
        }
        return base.TravelPreference(connection, cost);
    }
    public virtual bool TileInEnclosedArea(IntVector2 tilePos)
    {
        int numOfSolidSides = 0;
        for (int s = 0; s < 4; s++)
        {
            if (Lmn.room.GetTile(tilePos + (Custom.fourDirections[s] * 2)).Solid)
            {
                numOfSolidSides++;
                if (numOfSolidSides > 1)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public virtual float ForagePosScore(WorldCoordinate coord)
    {
        if (coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord))
        {
            return 50f;
        }
        if (!Lmn.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate.preBakedPathingAncestor) || !pathFinder.CoordinateReachableAndGetbackable(coord))
        {
            return float.MaxValue;
        }
        float cost = 1f;
        if (Lmn.room.aimap.getAItile(coord).narrowSpace)
        {
            cost += 600f;
        }
        if (TileInEnclosedArea(coord.Tile))
        {
            cost += 400f;
        }
        if (Lmn.room.aimap.getTerrainProximity(coord) > 1)
        {
            cost += 200f;
        }
        cost += threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: false) * 500f;
        for (int i = 0; i < prevForageSpots.Count; i++)
        {
            cost += Mathf.Pow(Mathf.InverseLerp(80, 5, prevForageSpots[i].Tile.FloatDist(coord.Tile)), 2f) * Custom.LerpMap(i, 0, 8, 70, 15);
        }
        cost += Mathf.Max(0f, creature.pos.Tile.FloatDist(coord.Tile) - 40f) / 20f;
        cost += Mathf.Clamp(Mathf.Abs(800f - Lmn.room.aimap.getAItile(coord).visibility), 300f, 1000f) / 30f;
        return cost - (Mathf.Max(Lmn.room.aimap.getAItile(coord).smoothedFloorAltitude, 6) * 2);
    }


    //--------------------------------------------------

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature newCtr)
    {
        return newCtr.realizedCreature is not null &&
            newCtr.realizedCreature is Luminescipede otherLmn &&
           (newCtr.state.dead || (Lmn.flock?.lumins is not null && Lmn.flock.lumins.Contains(otherLmn)))
            ? null
            : (Tracker.CreatureRepresentation)(newCtr.creatureTemplate.smallCreature
            ? new Tracker.SimpleCreatureRepresentation(tracker, newCtr, 0.15f, forgetWhenNotVisible: false)
            : new Tracker.ElaborateCreatureRepresentation(tracker, newCtr, 0.85f, 3));
    }
    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        return relationship.type == CreatureTemplate.Relationship.Type.Afraid ||
            relationship.type == CreatureTemplate.Relationship.Type.StayOutOfWay
            ? threatTracker
            : (AIModule)null;
    }
    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship relationship)
    {
        if (Lmn.dead || relationship.trackerRep.representedCreature is null)
        {
            return StaticRelationship(relationship.trackerRep.representedCreature);
        }

        CreatureTemplate.Relationship newRelat = StaticRelationship(relationship.trackerRep.representedCreature);
        AbstractCreature absCtr = relationship.trackerRep.representedCreature;
        Creature ctr = absCtr.realizedCreature;

        if (absCtr.creatureTemplate.type == CreatureTemplate.Type.Spider &&
            ctr?.room is not null)
        {
            float spiderMass = 0;
            foreach (AbstractCreature roomCtr in absCtr.Room.creatures)
            {
                if (roomCtr?.realizedCreature is null || roomCtr.creatureTemplate.type != CreatureTemplate.Type.Spider || roomCtr.realizedCreature.dead || !Custom.DistLess(LmnPos, roomCtr.realizedCreature.DangerPos, 200))
                {
                    continue;
                }
                spiderMass += roomCtr.realizedCreature.TotalMass;
            }
            if (spiderMass >= Lmn.TotalMass)
            {
                newRelat = new CreatureTemplate.Relationship
                             (CreatureTemplate.Relationship.Type.Afraid, 0.5f);
            }
            else if (spiderMass > 0)
            {
                newRelat = new CreatureTemplate.Relationship
                             (CreatureTemplate.Relationship.Type.Attacks, 1);
            }
        }


        if (Role == Guardian)
        {
            if (ctr is not null &&
                VisualContact(ctr.DangerPos) &&
                denFinder.denPosition.HasValue &&
                creature.pos.room == denFinder.denPosition.Value.room &&
                Custom.ManhattanDistance(absCtr.pos, Lmn.room.LocalCoordinateOfNode(denFinder.denPosition.Value.abstractNode)) < 16)
            {
                if (ctr.TotalMass > Lmn.AttackMassLimit)
                {
                    newRelat = new CreatureTemplate.Relationship
                                 (CreatureTemplate.Relationship.Type.Afraid, 1);
                }
                else
                if (newRelat.type == CreatureTemplate.Relationship.Type.Eats && newRelat.intensity != 1)
                {
                    if (ctr != Lmn.currentPrey)
                    {
                        Lmn.currentPrey = ctr;
                    }
                    newRelat = new CreatureTemplate.Relationship
                                 (CreatureTemplate.Relationship.Type.Eats, 1);
                }
                else
                if (newRelat.type != CreatureTemplate.Relationship.Type.Attacks || newRelat.intensity != 1)
                {
                    if (ctr != Lmn.currentPrey)
                    {
                        Lmn.currentPrey = ctr;
                    }
                    newRelat = new CreatureTemplate.Relationship
                                 (CreatureTemplate.Relationship.Type.Attacks, 1);
                }
            }
        }
        else if (Role == Forager)
        {
            if (Lmn.HoldingThisItemType(AbstractPhysicalObject.AbstractObjectType.FlyLure))
            {
                if (absCtr.creatureTemplate.type == CreatureTemplate.Type.Fly)
                {
                    newRelat = new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Eats, 1);
                }
                else
                if (newRelat.type == CreatureTemplate.Relationship.Type.Eats ||
                    newRelat.type == CreatureTemplate.Relationship.Type.Attacks)
                {
                    newRelat.intensity -= 0.1f;
                }
                // Prioritizes hunting Batflies when holding onto Batnip.
            }

            if (!ctr.dead &&
                Behavior != Rush &&
                Behavior != Aggravated &&
                Lmn.flock.lumins.Count < 3 &&
                Lmn.TotalMass < ctr.TotalMass &&
                (newRelat.type == CreatureTemplate.Relationship.Type.Eats || newRelat.type == CreatureTemplate.Relationship.Type.Attacks))
            {
                newRelat = new CreatureTemplate.Relationship
                             (CreatureTemplate.Relationship.Type.Afraid, 1f - (0.75f * Mathf.Clamp(newRelat.intensity, 0, 1)));
                // Foragers will run from prey that it can't take on.
                // You may notice that this behavior is not being shared by Guardians.
            }
        }


        if (Lmn.HoldingThisItemType(AbstractPhysicalObject.AbstractObjectType.VultureMask) &&
            (newRelat.type == CreatureTemplate.Relationship.Type.Afraid || newRelat.type == CreatureTemplate.Relationship.Type.StayOutOfWay) &&
            ctr is not null &&
            ctr is Lizard liz &&
            liz.AI is not null &&
            (liz.AI.DynamicRelationship(Lmn.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid || liz.AI.DynamicRelationship(Lmn.abstractCreature).type == CreatureTemplate.Relationship.Type.Ignores))
        {
            newRelat.type = CreatureTemplate.Relationship.Type.Ignores;
            // Lumins won't fear lizards that are scared while they hold a Vulture Mask.
        }


        if (Behavior == Aggravated)
        {
            if (newRelat.type == CreatureTemplate.Relationship.Type.Afraid ||
                newRelat.type == CreatureTemplate.Relationship.Type.StayOutOfWay)
            {
                newRelat.intensity -= 0.5f;
                if (newRelat.intensity < 0)
                {
                    newRelat = new CreatureTemplate.Relationship
                                 (CreatureTemplate.Relationship.Type.Attacks, newRelat.intensity * -1f);
                }
            }
            // AGGRAVATED LUMINS FEAR NOTHING
            // WELL OKAY MAYBE A LITTLE STILL, DEPENDING, BUT THEY FEAR *LESS*.
        }

        return newRelat;
    }
    public override bool TrackerToDiscardDeadCreature(AbstractCreature absCtr)
    {
        return absCtr is null || absCtr.InDen;
    }

    public virtual ObjectRelationship ObjRelationship(AbstractPhysicalObject absObj)
    {
        if (absObj is AbstractCreature)
        {
            CreatureTemplate.Relationship ctrRelat =
                DynamicRelationship(absObj as AbstractCreature);

            return ctrRelat.type == CreatureTemplate.Relationship.Type.Eats
                ? new ObjectRelationship(Eats, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.Attacks
                ? new ObjectRelationship(Attacks, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.Afraid
                ? new ObjectRelationship(AfraidOf, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.StayOutOfWay
                ? new ObjectRelationship(Avoids, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.Uncomfortable
                ? new ObjectRelationship(UncomfortableAround, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.PlaysWith
                ? new ObjectRelationship(PlaysWith, ctrRelat.intensity)
                : ctrRelat.type == CreatureTemplate.Relationship.Type.Ignores
                ? new ObjectRelationship(Ignores, ctrRelat.intensity)
                : new ObjectRelationship(DoesntTrack, 1);
        }

        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Rock)
        {
            return new ObjectRelationship(Uses, 1f);
        }

        if (absObj.type == DLCSharedEnums.AbstractObjectType.GooieDuck ||
                absObj.type == MoreSlugcatsEnums.AbstractObjectType.FireEgg ||
                    absObj.type == HSEnums.AbstractObjectType.BurnSpear)
        {
            return new ObjectRelationship(UncomfortableAround, 0.5f);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.SporePlant ||
                absObj.type == DLCSharedEnums.AbstractObjectType.SingularityBomb ||
                    absObj.type == HSEnums.AbstractObjectType.IceChunk)
        {
            return new ObjectRelationship(UncomfortableAround, 1);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
        {
            return new ObjectRelationship(AfraidOf, 0.8f);
        }

        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
        {
            return new ObjectRelationship(Eats, 0.25f);
        }

        if (Role == Hunter)
        {
            if (Lmn is not null && Lmn.WantToHide)
            {
                if (absObj.type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb ||
                        absObj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl ||
                            absObj.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl ||
                                absObj.type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl ||
                                    absObj.type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
                {
                    return new ObjectRelationship(Likes, 0.6f);
                }
                if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                {
                    return new ObjectRelationship(Likes, 1);
                }
            }
        }
        else
        {
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
            {
                return new ObjectRelationship(Uses, 0.85f);
            }
            if (absObj is VultureMask.AbstractVultureMask AVM)
            {
                float like = AVM.king ? 0.9f : 0.7f;
                return new ObjectRelationship(Likes, like);
            }
        }

        if (Role == Forager)
        {
            if (absObj.type == DLCSharedEnums.AbstractObjectType.DandelionPeach)
            {
                return new ObjectRelationship(Eats, 0.05f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
            {
                return new ObjectRelationship(Eats, 0.2f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.JellyFish)
            {
                return new ObjectRelationship(Eats, 0.4f);
            }
            if (absObj.type == DLCSharedEnums.AbstractObjectType.Seed ||
                    absObj.type == DLCSharedEnums.AbstractObjectType.LillyPuck)// ||
                                                                                  //absObj.type == HailstormEnums.BezanNut)
            {
                return new ObjectRelationship(Eats, 0.6f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.WaterNut ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.SlimeMold ||
                        absObj.type == DLCSharedEnums.AbstractObjectType.GlowWeed)
            {
                return new ObjectRelationship(Eats, 0.8f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg ||
                        absObj.type == AbstractPhysicalObject.AbstractObjectType.NeedleEgg ||
                            absObj.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
            {
                return new ObjectRelationship(Eats, 1);
            }

            if (absObj is BubbleGrass.AbstractBubbleGrass ABG)
            {
                float like = (Lmn.room?.water is not null) ? 1f : 0.25f;
                like *= ABG.oxygenLeft;
                return new ObjectRelationship(Likes, like);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
            {
                return new ObjectRelationship(Likes, 0.5f);
            }
        }
        else
        {
            if (absObj.type == DLCSharedEnums.AbstractObjectType.DandelionPeach)
            {
                return new ObjectRelationship(PlaysWith, 0.8f);
            }
        }


        return new ObjectRelationship(Ignores, 0);
    }

    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return null;
    }

    //--------------------------------------------------

    public void ReactToNoise(NoiseTracker.TheorizedSource source, Noise.InGameNoise noise)
    {
        if (noiseRectionDelay > 0 || Lmn?.room is null)
        {
            return;
        }
        noiseRectionDelay = Role == Forager ? 20 : 100;

        if (Role == Guardian && Lmn.bloodlust >= 2 && (noise.interesting >= 3 || noise.strength > 900))
        {
            creature.abstractAI.SetDestination(Lmn.room.GetWorldCoordinate(noise.pos));
        }
        if (Role == Forager)
        {
            if (Lmn.bloodlust < 1.5f && noise.strength > 500 && noise.strength < 800 && noise.interesting < 3)
            {
                creature.abstractAI.SetDestination(Lmn.room.GetWorldCoordinate(noise.pos));
            }
            if (noise.strength > 1200 && source?.creatureRep?.representedCreature?.realizedCreature is not null)
            {
                Lmn.fearSource = source.creatureRep.representedCreature.realizedCreature;
            }
        }
    }

    bool IUseItemTracker.TrackItem(AbstractPhysicalObject absObj)
    {
        return !Lmn.safariControlled &&
            ObjRelationship(absObj).type != Ignores &&
            ObjRelationship(absObj).type != DoesntTrack;
    }
    void IUseItemTracker.SeeThrownWeapon(PhysicalObject obj, Creature thrower)
    {
    }

    //--------------------------------------------------

    public virtual void ConsiderTrackedCreature()
    {
        if (Lmn.safariControlled || tracker.creatures.Count < 1)
        {
            return;
        }

        AbstractCreature absCtr = tracker.creatures[Random.Range(0, tracker.creatures.Count)].representedCreature;

        if (absCtr?.realizedCreature is null || absCtr.InDen)
        {
            return;
        }
        Creature ctr = absCtr.realizedCreature;
        if (ctr.slatedForDeletetion || !CWT.ObjectData.TryGetValue(ctr, out CWT.ObjectInfo oI) || oI.inShortcut)
        {
            return;
        }

        if (DynamicRelationship(absCtr).type == CreatureTemplate.Relationship.Type.Pack)
        {
            if (ctr is Luminescipede otherLmn &&
                otherLmn.GlowState.alive &&
                Lmn.flock?.lumins is not null &&
                !Lmn.flock.lumins.Contains(otherLmn) &&
                VisualContact(otherLmn.Body.pos))
            {
                Lmn.flock.AddLmn(otherLmn);
                tracker.ForgetCreature(otherLmn.abstractCreature);
            }
        }
        else if (Lmn.ConsiderPrey(ctr) && VisualContact(ctr.mainBodyChunk.pos))
        {
            GlowState.timeSincePreyLastSeen = 0;

            if (Lmn.currentPrey is null || (ctr != Lmn.currentPrey && Lmn.WillingToDitchCurrentPrey(Lmn.currentPrey)))
            {
                Lmn.currentPrey = ctr;
            }
        }
        else if (Lmn.ConsiderThreatening(ctr))
        {
            Lmn.fearSource = ctr;
        }
    }
    public virtual void ConsiderTrackedItem()
    {
        if (Lmn.safariControlled || itemTracker.items.Count < 1)
        {
            return;
        }

        for (int i = itemTracker.ItemCount - 1; i >= 0; i--)
        {
            if (itemTracker.items[i].representedItem is null ||
                ObjRelationship(itemTracker.items[i].representedItem).type == Ignores)
            {
                itemTracker.items[i].Destroy();
            }
        }

        AbstractPhysicalObject absObj = itemTracker.items[Random.Range(0, itemTracker.ItemCount)].representedItem;

        if (absObj?.realizedObject is null ||
            absObj.InDen ||
            absObj.realizedObject is not PlayerCarryableItem item ||
            item.slatedForDeletetion ||
            !CWT.ObjectData.TryGetValue(item, out CWT.ObjectInfo oI) || oI.inShortcut)
        {
            return;
        }

        Vector2 itemPos = Lmn.MainChunkOfObject(item).pos;

        if (Lmn.ConsiderPrey(item) && VisualContact(itemPos))
        {
            GlowState.timeSincePreyLastSeen = 0;

            if (item != Lmn.currentPrey && item.TotalMass < Lmn.AttackMassLimit && Lmn.WillingToDitchCurrentPrey(item))
            {
                Lmn.currentPrey = item;
            }
        }
        else if (Lmn.ConsiderUseful(item) && !Lmn.GrabbingItem(item) && Lmn.MoreAppealingThanCurrentItem(item) && VisualContact(itemPos))
        {
            Lmn.useItem = item;
        }
        else if (Lmn.ConsiderThreatening(item))
        {
            Lmn.fearSource = item;
        }
    }


    public new virtual bool VisualContact(Vector2 pos, float bonus = 0)
    {
        if (Lmn.room is null || !Lmn.room.VisualContact(LmnPos, pos))
        {
            return false;
        }
        if (Role == Forager)
        {
            bonus = Lmn.Dominant ? 2 / 3f : 1 / 3f;
        }
        return base.VisualContact(pos, bonus);
    }


}