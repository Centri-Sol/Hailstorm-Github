using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using static Hailstorm.GlowSpiderState.Role;
using static Hailstorm.GlowSpiderState.Behavior;
using UnityEngine;
using RWCustom;
using System.Security.Cryptography;
using Color = UnityEngine.Color;
using System;
using MoreSlugcats;
using System.IO;
using static MonoMod.InlineRT.MonoModRule;

namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class LuminAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
    public LuminCreature lmn => creature.realizedCreature as LuminCreature;
    public GlowSpiderState GlowState => lmn.State as GlowSpiderState;
    public GlowSpiderState.Role Role => GlowState.role;
    public GlowSpiderState.Behavior Behavior => GlowState.behavior;
    public Vector2 lmnPos => lmn.DangerPos;
    public bool HasValidDen =>
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
    public float MovementDesire 
    {
        get
        {
            if (!lmn.Consious || Behavior == Hide || Behavior == Overloaded || (Behavior == Rush && GlowState.rushPreyCounter < 24))
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

            if (Role == Forager || (Role == Hunter && lmn.Dominant))
            {
                desire *= 1.15f;
            }

            if (lmn.bloodlust > 1)
            {
                desire += (lmn.bloodlust - 1) / 10f;
            }
            if (lmn.CamoFac > 0)
            {
                desire *= Mathf.Max(0, 1 - lmn.CamoFac);
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
        lmn.AI = this;
        AddModule(new StandardPather(this, world, absLmn));
        pathFinder.stepsPerFrame = 5;
        pathFinder.accessibilityStepsPerFrame = 10;
        AddModule(new Tracker(this, 5, 9, 1200, 0.5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 0));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, absLmn));
        AddModule(new StuckTracker(this, true, false));
        AddModule(new UtilityComparer(this));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new RelationshipTracker(this, tracker));
        utilityComparer.AddComparedModule(threatTracker, null, 0, 0);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(stuckTracker, null, 0.4f, 1.1f);
        prevForageSpots = new List<WorldCoordinate>();
    }

    //--------------------------------------------------

    public override void Update() 
    {
        try
        {
            if (lmn?.room is null)
            {
                return;
            }

            base.Update();

            if (lmn.LickedByPlayer is not null)
            {
                tracker.SeeCreature(lmn.LickedByPlayer.abstractCreature);
            }

            if (lmn.flock?.lumins is not null)
            {
                if (tracker.CreaturesCount > 0)
                {
                    Tracker.CreatureRepresentation randomCreature = tracker.GetRep(Random.Range(0, tracker.CreaturesCount));
                    if (randomCreature.representedCreature?.realizedCreature is not null && randomCreature.representedCreature.realizedCreature is LuminCreature rndLmn && !lmn.flock.lumins.Contains(rndLmn))
                    {
                        if (rndLmn.GlowState.alive && lmn.VisualContact(rndLmn.body.pos))
                        {
                            lmn.flock.AddLmn(rndLmn);
                        }
                    }
                }
                for (int l = lmn.flock.lumins.Count - 1; l >= 0; l--)
                {
                    LuminCreature otherLmn = lmn.flock.lumins[l];
                    if (otherLmn is null || otherLmn.dead || otherLmn.abstractCreature.pos.room != creature.pos.room)
                    {
                        lmn.flock.RemoveLmn(otherLmn);
                        continue;
                    }

                    if (!HasValidDen && otherLmn.AI.HasValidDen && otherLmn.GlowState.ivars.dominance >= GlowState.ivars.dominance)
                    {
                        denFinder.denPosition = otherLmn.AI.denFinder.denPosition.Value;
                    }
                }
            }

            pathFinder.walkPastPointOfNoReturn = stranded || !denFinder.denPosition.HasValue || !pathFinder.CoordinatePossibleToGetBackFrom(denFinder.denPosition.Value) || threatTracker.Utility() > 0.96f;

            if (Behavior == Overloaded)
            {
                noiseTracker.hearingSkill = 0;
            }
            else if (Behavior == Aggravated)
            {
                noiseTracker.hearingSkill = 0.3f;
            }
            else
            {
                noiseTracker.hearingSkill = 1f;
            }

            if (Behavior != Overloaded && lmn.Role == Forager)
            {
                noiseTracker.hearingSkill += 0.2f;
            }

            /* Could repurpose this for weighting the threatTracker instead of the preyTracker.
            if (MostAttractivePrey is not null)
            {
                utilityComparer.GetUtilityTracker(preyTracker).weight = Custom.LerpMap(creature.pos.Tile.FloatDist(MostAttractivePrey.BestGuessForPosition().Tile), 15f, 45f, 1f, 0.01f);
            }
            */
            AIModule aIModule = utilityComparer.HighestUtilityModule();
            currentUtility = utilityComparer.HighestUtility();

            if (aIModule is not null && currentUtility >= 0.9f)
            {
                if (aIModule is StuckTracker)
                {
                    GlowState.ChangeBehavior(GetUnstuck, false);
                }
                else if (aIModule is RainTracker)
                {
                    GlowState.ChangeBehavior(EscapeRain, false);
                }
            }

            if (lmn.currentPrey is not null)
            {
                GlowState.ChangeBehavior(Hunt, false);
                PreyPos = lmn.room.GetWorldCoordinate(lmn.currentPrey is Creature prey ? prey.mainBodyChunk.pos : lmn.currentPrey.firstChunk.pos);
            }
            else
            {
                if (lmn.bloodlust <= 0)
                {
                    GlowState.ChangeBehavior(Idle, false);
                }
                if (PreyPos.HasValue)
                {
                    PreyPos = null;
                }
            }

            PathingDestination();

            inAccessibleTerrain = lmn.lungeTimer < 20 && lmn.room.aimap.TileAccessibleToCreature(lmnPos, lmn.Template.preBakedPathingAncestor);

            if (lmn.lunging && lmn.lungeTimer == 0 && inAccessibleTerrain)
            {
                lmn.lunging = false;
            }

            if (lmn.lungeTimer > 0 || Behavior == Hide || Behavior == Overloaded || (Behavior == Rush && GlowState.rushPreyCounter < 24))
            {
                MovementSpeed = 0;
            }
            else if (MovementSpeed != MovementDesire)
            {
                MovementSpeed = Custom.LerpAndTick(MovementSpeed, MovementDesire, 0.01f, 0.01f);
            }

            if (lmn.Consious && lmn.lungeTimer == 0 && !lmn.lunging)
            {
                if (!lmn.safariControlled)
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
        if (lmn.Submersion > 0.3f)
        {
            WaterPathfinding();
            return;
        }

        if (specialMoveCounter > 0)
        {
            specialMoveCounter--;
            MoveTowards(lmn.room.MiddleOfTile(specialMoveDestination));
            travelDir = Vector2.Lerp(travelDir, Custom.DirVec(lmnPos, lmn.room.MiddleOfTile(specialMoveDestination)), 0.4f);
            if (Custom.DistLess(lmnPos, lmn.room.MiddleOfTile(specialMoveDestination), 5))
            {
                specialMoveCounter = 0;
            }
        }
        else
        {
            if (lmn.room.GetWorldCoordinate(lmnPos) == pathFinder.GetDestination && lmn.sourceOfFear is null && !lmn.safariControlled)
            {
                lmn.GoThroughFloors = false;
            }
            else
            {
                MovementConnection movementConnection = (pathFinder as StandardPather).FollowPath(lmn.room.GetWorldCoordinate(lmnPos), actuallyFollowingThisPath: true);
                if (movementConnection is null)
                {
                    movementConnection = (pathFinder as StandardPather).FollowPath(lmn.room.GetWorldCoordinate(lmnPos), actuallyFollowingThisPath: true);
                }
                if (lmn.safariControlled && (movementConnection is null || !lmn.AllowableControlledAIOverride(movementConnection.type)))
                {
                    movementConnection = null;
                    if (lmn.inputWithDiagonals.HasValue && Behavior != Hide)
                    {
                        MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                        if (lmn.shortcutDelay == 0 && lmn.room.GetTile(lmnPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                        {
                            type = MovementConnection.MovementType.ShortCut;
                        }
                        if (lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                        {
                            movementConnection = new MovementConnection(type, lmn.room.GetWorldCoordinate(lmnPos), lmn.room.GetWorldCoordinate(lmnPos + new Vector2(lmn.inputWithDiagonals.Value.x, lmn.inputWithDiagonals.Value.y) * 40f), 2);
                        }
                        if (lmn.inputWithDiagonals.Value.y < 0)
                        {
                            lmn.GoThroughFloors = true;
                        }
                        else
                        {
                            lmn.GoThroughFloors = false;
                        }
                    }
                }
                if (movementConnection is not null)
                {
                    NormalPathfinding(movementConnection);
                }
                else
                {
                    lmn.GoThroughFloors = false;
                }
            }
        }

    }
    public virtual void SafariMovement() 
    {
        if (lmn.inputWithDiagonals.HasValue)
        {
            Vector2 inputAngle = new Vector2(lmn.inputWithDiagonals.Value.x, lmn.inputWithDiagonals.Value.y) * 20;
            List<Vector2> posList = new() { lmnPos, lmnPos + inputAngle };
            for (int p = 0; p < posList.Count; p++)
            {
                if (lmn.shortcutDelay == 0 &&
                    lmn.room.GetTile(posList[p]).Terrain == Room.Tile.TerrainType.ShortcutEntrance && (
                    (lmn.room.ShorcutEntranceHoleDirection(lmn.room.GetTilePosition(posList[p])).x != 0 && -lmn.inputWithDiagonals.Value.x == lmn.room.ShorcutEntranceHoleDirection(lmn.room.GetTilePosition(posList[p])).x) ||
                    (lmn.room.ShorcutEntranceHoleDirection(lmn.room.GetTilePosition(posList[p])).y != 0 && -lmn.inputWithDiagonals.Value.y == lmn.room.ShorcutEntranceHoleDirection(lmn.room.GetTilePosition(posList[p])).y)))
                {
                    lmn.enteringShortCut = lmn.room.GetTilePosition(posList[p]);
                    MovementConnection movementConnection = (pathFinder as StandardPather).FollowPath(lmn.room.GetWorldCoordinate(lmn.enteringShortCut.Value), true);
                    if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                    {
                        bool atWackamoleEntrance = false;
                        List<IntVector2> wackamoleExits = new();
                        ShortcutData[] shortcuts = lmn.room.shortcuts;
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
                            lmn.NPCTransportationDestination = lmn.room.GetWorldCoordinate(wackamoleExits[Random.Range(0, wackamoleExits.Count)]);
                        }
                    }
                    break;
                }
            }
            if (lmn.Submersion > 0 || inAccessibleTerrain)
            {
                if (lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                {
                    MoveTowards(posList[1]);
                }
            }
            else for (int d = 0; d < Custom.fourDirections.Length; d++)
            {
                if (lmn.room.aimap.TileAccessibleToCreature(lmnPos + (inputAngle * Custom.fourDirections[d].ToVector2()), lmn.Template.preBakedPathingAncestor))
                {
                    inAccessibleTerrain = true;
                    if (lmn.inputWithDiagonals.Value.AnyDirectionalInput)
                    {
                        MoveTowards(posList[1]);
                    }
                    break;
                }
            }

        }
        if (lmn.Submersion > 0.3f)
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
        if (lmn.shortcutDelay < 1 &&
            (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation) &&
            (lmn.grasps[0]?.grabbed is null || lmn.grasps[0].grabbed is not Creature carried || carried.dead))
        {
            lmn.enteringShortCut = followingConnection.StartTile;
            if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                lmn.NPCTransportationDestination = followingConnection.destinationCoord;
            }
        }
        else if (
            followingConnection.type == MovementConnection.MovementType.OpenDiagonal ||
            followingConnection.type == MovementConnection.MovementType.ReachOverGap ||
            followingConnection.type == MovementConnection.MovementType.ReachUp ||
            followingConnection.type == MovementConnection.MovementType.ReachDown ||
            followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
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
                if (movementConnection2 is not null)
                {
                    if (movementConnection2.destinationCoord == followingConnection.startCoord)
                    {
                        return;
                    }
                    if (movementConnection2.destinationCoord.TileDefined && lmn.room.aimap.getAItile(movementConnection2.DestTile).acc < AItile.Accessibility.Ceiling)
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
                                if (!lmn.room.aimap.TileAccessibleToCreature(j, k, lmn.Template.preBakedPathingAncestor))
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
            Vector2 destination = lmn.room.MiddleOfTile(movementConnection.DestTile);
            travelDir = Vector2.Lerp(travelDir, Custom.DirVec(lmnPos, destination), 0.4f);
            if (lastFollowedConnection is not null && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
            {
                lmn.body.vel += Custom.DirVec(lmnPos, destination) * 4f;
            }
            if (followingConnection.type != MovementConnection.MovementType.DropToFloor)
            {
                if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                {
                    lmn.body.vel.x += Mathf.Min((destination.x - lmnPos.x) / 8f, 1.2f);
                }
                else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                {
                    lmn.body.vel.y += Mathf.Min((destination.y - lmnPos.y) / 8f, 1.2f);
                }
            }
            if (lastFollowedConnection is not null &&
                (followingConnection.type != MovementConnection.MovementType.DropToFloor || lmn.room.aimap.TileAccessibleToCreature(lmnPos, lmn.Template.preBakedPathingAncestor)) && (
                (followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) ||
                (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
            {
                lmn.body.vel *= 0.7f;
            }
            MoveTowards(destination);
        }
        lastFollowedConnection = followingConnection;
    }
    public virtual void WaterPathfinding() 
    {
        lmn.GoThroughFloors = true;
        lmn.body.vel *= inAccessibleTerrain ? 0.95f : 0.8f;
        lmn.body.vel.y += 0.35f * (2.2f - GlowState.ivars.Size);
        if (lmn.safariControlled)
        {
            return;
        }
        MovementConnection movementDestination = (pathFinder as StandardPather).FollowPath(lmn.room.GetWorldCoordinate(lmnPos), actuallyFollowingThisPath: true);
        if (movementDestination is null && Math.Abs(creature.pos.y - lmn.room.defaultWaterLevel) < 4)
        {
            movementDestination = (pathFinder as StandardPather).FollowPath(new WorldCoordinate(creature.pos.room, creature.pos.x, lmn.room.defaultWaterLevel, creature.pos.abstractNode), actuallyFollowingThisPath: true);
        }
        if (movementDestination is not null)
        {
            if (movementDestination.StartTile.y == movementDestination.DestTile.y && movementDestination.DestTile.y == lmn.room.defaultWaterLevel)
            {
                lmn.body.vel.x -= Mathf.Sign(lmn.room.MiddleOfTile(movementDestination.StartTile).x - lmn.room.MiddleOfTile(movementDestination.DestTile).x) * lmn.Submersion;
                return;
            }
            lmn.body.vel *= 0.8f;
            lmn.body.vel += Custom.DirVec(lmnPos, lmn.room.MiddleOfTile(movementDestination.destinationCoord));
            lmn.body.vel *= Mathf.Lerp(1, 0.65f, lmn.Submersion);
            NormalPathfinding(movementDestination);
            lmn.body.vel.y += 0.35f * (2.2f - GlowState.ivars.Size);
        }
    }
    public virtual void MoveTowards(Vector2 moveTo) 
    {
        if (Random.value > 0.5f + (GlowState.health / 2f))
        {
            return;
        }
        float vel = (GlowState.ivars.Size + 0.2f) * Mathf.Lerp(0.5f, 1, MovementSpeed);
        Vector2 angle = Custom.DirVec(lmnPos, moveTo);
        if (!lmn.safariControlled && lmn.IsTileSolid(0, 0, -1))
        {
            lmn.body.vel.x -= 1.3f * Mathf.Sign(angle.x);
        }
        if (lmn.GrabbingAnything)
        {
            for (int g = 0; g < lmn.grasps.Length; g++)
            {
                if (lmn.grasps[g]?.grabbed is not null)
                {
                    float maxMass = lmn.MassAttackLimit/2f;
                    if (g == 1)
                    {
                        maxMass *= 2f;
                    }
                    vel *= Custom.LerpMap(lmn.grasps[g].grabbed.TotalMass, 0, maxMass, 1, 0.5f);
                }
            }
        }
        lmn.body.vel += vel * angle * 3.75f;
        lmn.GoThroughFloors = moveTo.y < lmnPos.y - 5f;
    }
    public virtual void PathingDestination() 
    {
        if (lmn.safariControlled && lmn.Consious)
        {
            GlowState.ChangeBehavior(Idle, true);
            return;
        }

        Debug.Log("behavior: " + Behavior.value);
        if (lmn.shortcutDelay > 0 && denFinder.denPosition.HasValue && denFinder.denPosition.Value == creature.abstractAI.destination && Custom.ManhattanDistance(creature.pos, lmn.room.LocalCoordinateOfNode(denFinder.denPosition.Value.abstractNode)) < 3)
        {
            int x = creature.pos.x + (Random.Range(2, 5) * (Random.value < 0.5f ? 1 : -1));
            int y = creature.pos.y + (Random.Range(2, 5) * (Random.value < 0.5f ? 1 : -1));
            WorldCoordinate coord = new(lmn.room.abstractRoom.index, x, y, -1);
            if (lmn.room.aimap.TileAccessibleToCreature(lmn.room.MiddleOfTile(coord), lmn.Template.preBakedPathingAncestor) && pathFinder.CoordinateReachableAndGetbackable(coord))
            {
                creature.abstractAI.SetDestination(coord);
            }
            return;
        }

        if (Role == Forager)
        {
            if (Behavior != Idle && Behavior != GetUnstuck)
            {
                forageSpot = creature.pos;
            }
            forageSpotCounter--;
        }

        if (lmn.lungs < 0.33f)
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

        if (Behavior == GetUnstuck)
        {
            if (Random.value < 1f / 150f)
            {
                creature.abstractAI.SetDestination(lmn.room.GetWorldCoordinate(lmnPos + Custom.RNV() * 100f));
            }
        }
        else if (Behavior == Idle)
        {
            if (Role == Protector)
            {
                if (denFinder.denPosition.HasValue && (creature.pos.room != denFinder.denPosition.Value.room || Custom.ManhattanDistance(creature.pos, lmn.room.LocalCoordinateOfNode(denFinder.denPosition.Value.abstractNode)) > 20))
                {
                    creature.abstractAI.SetDestination(denFinder.denPosition.Value);
                }
            }
            else if (Role == Forager)
            {
                if (forageAtPosition.HasValue)
                {
                    creature.abstractAI.SetDestination(forageAtPosition.Value);
                    if (Random.value < 0.002f || Custom.ManhattanDistance(creature.pos, forageAtPosition.Value) < 4)
                    {
                        forageAtPosition = null;
                    }
                }
                else if (!creature.abstractAI.WantToMigrate)
                {
                    WorldCoordinate coord = new WorldCoordinate(lmn.room.abstractRoom.index, Random.Range(0, lmn.room.TileWidth), Random.Range(0, lmn.room.TileHeight), -1);
                    if (pathFinder.CoordinateReachableAndGetbackable(coord) && ForagePosScore(coord) < ForagePosScore(nextForageSpot))
                    {
                        nextForageSpot = coord;
                    }
                    creature.abstractAI.SetDestination(forageSpot);
                    if (Custom.ManhattanDistance(creature.pos, forageSpot) < 3 && (lmn.room.aimap.getAItile(creature.pos).narrowSpace || TileInEnclosedArea(creature.pos.Tile)))
                    {
                        forageSpotCounter -= 4;
                    }
                    if (forageSpotCounter < 1)
                    {
                        forageSpotCounter = Random.Range(300, 900);
                        prevForageSpots.Add(forageSpot);
                        if (prevForageSpots.Count > 7)
                        {
                            prevForageSpots.RemoveAt(0);
                        }
                        forageSpot = nextForageSpot;
                        nextForageSpot = new WorldCoordinate(lmn.room.abstractRoom.index, Random.Range(0, lmn.room.TileWidth), Random.Range(0, lmn.room.TileHeight), -1);
                    }
                }
                else
                {
                    if (migrationTimer < 2400) // Takes 8 seconds at 0 bloodlust.
                    {
                        migrationTimer += 3 - lmn.bloodlust;
                    }
                    else if (Random.value < 0.5f)
                    {
                        creature.abstractAI.RandomMoveToOtherRoom(Random.Range(200, 400));
                        migrationTimer = 0;
                    }
                    else
                    {
                        creature.abstractAI.MoveToMoreAttractiveNeighborRoom();
                        migrationTimer = 0;
                    }
                }
            }
        }
        else if (Behavior == Hunt)
        {
            if (PreyPos.HasValue && lmn.bloodlust >= 1)
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
        }
        else if (Behavior == Aggravated)
        {
            if (PreyPos.HasValue && !Custom.DistLess(lmnPos, lmn.room.MiddleOfTile(PreyPos.Value), lmn.Template.visualRadius * 2 / 3f))
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
        }
        else if (Behavior == Rush)
        {
            if (PreyPos.HasValue && !Custom.DistLess(lmnPos, lmn.room.MiddleOfTile(PreyPos.Value), lmn.Template.visualRadius * 2 / 3f))
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
        }
        else if (Behavior == Flee && lmn.sourceOfFear is not null)
        {
            bool considerLeavingRoom = Custom.DistLess(lmnPos, lmn.sourceOfFear.pos, lmn.fleeRadius / (Role == Forager ? 3f : 4f));
            creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 6, 30, considerLeavingRoom));
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
            if (PreyPos.HasValue && lmn.grasps[0] is null)
            {
                creature.abstractAI.SetDestination(PreyPos.Value);
            }
            if (denFinder.denPosition.HasValue && lmn.grasps[0]?.grabbed is not null &&
                ((lmn.grasps[0].grabbed is Creature ctr && ctr.dead && DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats) ||
                (lmn.grasps[0].grabbed is not Creature && ObjRelationship(lmn.grasps[0].grabbed.abstractPhysicalObject).type == ObjectRelationship.Type.Eats)))
            {
                creature.abstractAI.SetDestination(denFinder.denPosition.Value);
            }
        }
    }

    public override PathCost TravelPreference(MovementConnection connection, PathCost cost) 
    {
        cost.resistance += Mathf.Max(0f, threatTracker.ThreatOfTile(connection.destinationCoord, accountThreatCreatureAccessibility: true) - threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true)) * 40f;
        if (lmn?.room is null)
        {
            cost.resistance += Custom.LerpMap(lmn.room.aimap.getAItile(connection.DestTile).smoothedFloorAltitude, 1f, 7f, 60f, 0f);
            if (lmn.lungs < 0.33f)
            {
                cost.resistance += lmn.room.GetTile(connection.destinationCoord).AnyWater ? 100 : -100f;
            }
        }
        return base.TravelPreference(connection, cost);
    }
    public bool TileInEnclosedArea(IntVector2 tilePos) 
    {
        int numOfSolidSides = 0;
        for (int s = 0; s < 4; s++)
        {
            if (lmn.room.GetTile(tilePos + Custom.fourDirections[s] * 2).Solid)
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
        if (!lmn.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate.preBakedPathingAncestor) || !pathFinder.CoordinateReachableAndGetbackable(coord))
        {
            return float.MaxValue;
        }
        float cost = 1f;
        if (lmn.room.aimap.getAItile(coord).narrowSpace)
        {
            cost += 600f;
        }
        if (TileInEnclosedArea(coord.Tile))
        {
            cost += 400f;
        }
        if (lmn.room.aimap.getAItile(coord).terrainProximity > 1)
        {
            cost += 200f;
        }
        cost += threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: false) * 500f;
        for (int i = 0; i < prevForageSpots.Count; i++)
        {
            cost += Mathf.Pow(Mathf.InverseLerp(80, 5, prevForageSpots[i].Tile.FloatDist(coord.Tile)), 2f) * Custom.LerpMap(i, 0, 8, 70, 15);
        }
        cost += Mathf.Max(0f, creature.pos.Tile.FloatDist(coord.Tile) - 40f) / 20f;
        cost += Mathf.Clamp(Mathf.Abs(800f - lmn.room.aimap.getAItile(coord).visibility), 300f, 1000f) / 30f;
        return cost - Mathf.Max(lmn.room.aimap.getAItile(coord).smoothedFloorAltitude, 6) * 2;
    }


    //--------------------------------------------------

    public void ReactToNoise(NoiseTracker.TheorizedSource source, Noise.InGameNoise noise) 
    {
        if (noiseRectionDelay > 0 || lmn?.room is null)
        {
            return;
        }
        noiseRectionDelay = Role == Forager ? 20 : 100;

        if (Role == Protector && lmn.bloodlust >= 2 && (noise.interesting >= 3 || noise.strength > 900))
        {
            creature.abstractAI.SetDestination(lmn.room.GetWorldCoordinate(noise.pos));
        }
        if (Role == Forager)
        {
            if (lmn.bloodlust < 1.5f && noise.strength > 500 && noise.strength < 800 && noise.interesting < 3)
            {
                creature.abstractAI.SetDestination(lmn.room.GetWorldCoordinate(noise.pos));
            }
            if (noise.strength > 1200)
            {
                lmn.fleeRadius = noise.strength;
                if (Custom.DistLess(lmnPos, noise.pos, lmn.fleeRadius) && source?.creatureRep?.representedCreature?.realizedCreature is not null)
                {
                    lmn.sourceOfFear = source.creatureRep.representedCreature.realizedCreature.mainBodyChunk;
                    GlowState.ChangeBehavior(Flee, Custom.DistLess(lmnPos, noise.pos, lmn.fleeRadius/2f));
                }
            }
        }
    }


    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature) 
    {
        if (otherCreature.creatureTemplate.smallCreature)
        {
            return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0.15f, forgetWhenNotVisible: false);
        }
        return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 0.85f, 3);
    }
    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship) 
    {
        return null;
    }
    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship relationship) 
    {
        if (relationship.trackerRep.representedCreature.realizedCreature is null)
        {
            return StaticRelationship(relationship.trackerRep.representedCreature);
        }
        Creature ctr = relationship.trackerRep.representedCreature.realizedCreature;

        if (ctr.Template.type == CreatureTemplate.Type.Spider && ctr.room is not null)
        {
            float spiderMass = 0;
            foreach (AbstractCreature absCtr in ctr.room.abstractRoom.creatures)
            {
                if (absCtr?.realizedCreature is null || absCtr.creatureTemplate.type != CreatureTemplate.Type.Spider || absCtr.realizedCreature.dead || !Custom.DistLess(lmnPos, absCtr.realizedCreature.DangerPos, 200))
                {
                    continue;
                }
                spiderMass += absCtr.realizedCreature.TotalMass;
            }
            if (spiderMass >= lmn.TotalMass)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, 0.5f);
            }
            else if (spiderMass > 0)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, 1);
            }
        }

        return StaticRelationship(ctr.abstractCreature);
    }
    public override bool TrackerToDiscardDeadCreature(AbstractCreature crit) 
    {
        return false;
    }

    //--------------------------------------------------

    public virtual ObjectRelationship ObjRelationship(AbstractPhysicalObject absObj) 
    {
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Creature)
        {
            return new ObjectRelationship(ObjectRelationship.Type.DoesntTrack, 1);
        }

        if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.GooieDuck ||
                absObj.type == MoreSlugcatsEnums.AbstractObjectType.FireEgg ||
                    absObj.type == HailstormEnums.BurnSpear)
        {
            return new ObjectRelationship(ObjectRelationship.Type.UncomfortableAround, 0.5f);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.SporePlant ||
                absObj.type == MoreSlugcatsEnums.AbstractObjectType.SingularityBomb ||
                    absObj.type == HailstormEnums.IceCrystal)
        {
            return new ObjectRelationship(ObjectRelationship.Type.UncomfortableAround, 1);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
        {
            return new ObjectRelationship(ObjectRelationship.Type.AfraidOf, 0.8f);
        }

        if (Role == Hunter && lmn is not null && lmn.WantToHide)
        {
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl ||
                        absObj.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl ||
                            absObj.type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl ||
                                absObj.type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Likes, 0.6f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Likes, 1);
            }
        }
        if (Role == Forager)
        {
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 0.05f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 0.2f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.JellyFish)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 0.4f);
            }
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.Seed ||
                    absObj.type == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)// ||
                    //absObj.type == HailstormEnums.BezanNut)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 0.6f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.WaterNut ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.SlimeMold ||
                        absObj.type == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 0.8f);
            }
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb ||
                    absObj.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg ||
                        absObj.type == AbstractPhysicalObject.AbstractObjectType.NeedleEgg ||
                            absObj.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Eats, 1);
            }
        }
        else
        {
            if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
            {
                return new ObjectRelationship(ObjectRelationship.Type.Attacks, 0.4f);
            }
        }


        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.Rock ||
                absObj.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
        {
            return new ObjectRelationship(ObjectRelationship.Type.Uses, 0.5f);
        }


        return new ObjectRelationship(ObjectRelationship.Type.Ignores, 0);
    }

    //--------------------------------------------------
    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) 
    {
        return null;
    }

}

public struct ObjectRelationship 
{
    public class Type : ExtEnum<Type>
    {
        public static readonly Type DoesntTrack = new ("DoesntTrack", register: true);
        public static readonly Type Ignores = new ("Ignores", register: true);
        public static readonly Type Eats = new ("Eats", register: true);
        public static readonly Type Uses = new ("Uses", register: true);
        public static readonly Type Likes = new ("Likes", register: true);
        public static readonly Type Attacks = new ("Attacks", register: true);
        public static readonly Type UncomfortableAround = new ("UncomfortableAround", register: true);
        public static readonly Type AfraidOf = new ("AfraidOf", register: true);
        public static readonly Type PlaysWith = new ("PlaysWith", register: true);

        public Type(string value, bool register = false)
            : base(value, register)
        {
        }
    }

    public Type type;

    public float intensity;

    public ObjectRelationship(Type type, float intensity)
    {
        this.type = type;
        this.intensity = intensity;
    }

    public override bool Equals(object obj)
    {
        if (obj is null || obj is not ObjectRelationship)
        {
            return false;
        }
        return Equals((ObjectRelationship)obj);
    }

    public bool Equals(ObjectRelationship relationship)
    {
        if (type == relationship.type)
        {
            return intensity == relationship.intensity;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(ObjectRelationship a, ObjectRelationship b)
    {
        if (a.type == b.type)
        {
            return a.intensity == b.intensity;
        }
        return false;
    }

    public static bool operator !=(ObjectRelationship a, ObjectRelationship b)
    {
        return !(a == b);
    }

    public ObjectRelationship Duplicate()
    {
        return new ObjectRelationship(type, intensity);
    }

    public override string ToString()
    {
        return type.ToString() + " " + intensity;
    }
}