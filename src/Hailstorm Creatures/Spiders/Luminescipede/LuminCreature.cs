using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static Hailstorm.GlowSpiderState.Role;
using static Hailstorm.GlowSpiderState.Behavior;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class LuminCreature : InsectoidCreature, IPlayerEdible
{
    //-----------------------------------------

    public LuminFlock flock;
    
    public LuminAI AI;
    public BodyChunk body => firstChunk;
    public LuminGraphics graphics => graphicsModule as LuminGraphics;
    public GlowSpiderState GlowState => State as GlowSpiderState;
    public GlowSpiderState.Behavior Behavior => GlowState.behavior;
    public GlowSpiderState.Role Role => GlowState.role;

    //---- General Stats ----//

    private int bites = 5;
    public int BitesLeft => bites;
    public int FoodPoints => 1;
    public bool Edible => dead;
    public bool AutomaticPickUp => dead;

    public float HP => GlowState.health;
    public float Juice => GlowState.juice;
    public float CamoFac => Mathf.InverseLerp(0, 320, GlowState.darknessCounter);
    public bool WantToHide => Role != Forager && (Behavior == Hide || CamoFac > 0 || GlowState.timeSincePreyLastSeen == GlowState.timeToWantToHide);
    public bool Dominant => GlowState.dominant;

    new public Vector2 VisionPoint;

    public Vector2 dragPos;
    public Vector2 direction;
    public Vector2 lastDirection;

    //---- Grasps ----//

    private int attachCounter;
    private int timeWithoutPreyContact;

    public BodyChunk heavycarryChunk;
    public int losingInterestInGrasp;
    public int cantcarryWaitTime;
    public int disagreementTimer;

    private bool shouldTickGraspDelayCounter;
    private int graspSwapDelay; // Gives the go-ahead to swap grasps once ticked high enough
    private float remainingGraspSwapTime; // Grasps swaps aren't visually instant; this keeps track of how much time is left in a swap, so the held items can be transferred smoothly
    public int grabCooldown; // Limits how fast Lumins can regrab items
    public float EasycarryMassLimit => TotalMass * 2;
    public float BackholdMassLimit => TotalMass * 5;
    public float AttackMassLimit => TotalMass * 20;
    public bool GrabbingAnything
    {
        get
        {
            if (grasps is not null)
            {
                for (int g = 0; g < grasps.Length; g++)
                {
                    if (grasps[g]?.grabbed is not null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    public List<PhysicalObject> GrabbedObjects
    {
        get
        {
            List<PhysicalObject> objects = new();
            if (grasps is not null)
            {
                for (int g = 0; g < grasps.Length; g++)
                {
                    if (grasps[g]?.grabbed is not null)
                    {
                        objects.Add(grasps[g].grabbed);
                    }
                }
            }
            return objects;
        }
    }

    //---- Creature & Object Interactions ----//

    public PhysicalObject currentPrey;
    public float bloodlust;
    public float bloodlustRate;
    public int nullpreyCounter;
    public int lastNullpreyCounter;
    public int preyVisualCounter => GlowState.timeSincePreyLastSeen;
    public virtual float CurrentPreyRelationIntensity
    {
        get
        {
            if (currentPrey?.abstractPhysicalObject is not null)
            {
                if (currentPrey.abstractPhysicalObject is AbstractCreature absCtr)
                {
                    return Mathf.Clamp(AI.DynamicRelationship(absCtr).intensity, 0, 1);
                }
                return Mathf.Clamp(AI.ObjRelationship(currentPrey.abstractPhysicalObject).intensity, 0, 1);
            }
            return 0;
        }
    }


    public PhysicalObject fearSource;
    public BodyChunk closestFearChunk;
    public virtual float SourceOfFearIntensity
    {
        get
        {
            if (fearSource?.abstractPhysicalObject is null)
            {
                return 0;
            }
            float fear;
            if (fearSource.abstractPhysicalObject is AbstractCreature absCtr)
            {
                fear = Mathf.Clamp(AI.DynamicRelationship(absCtr).intensity, 0, 1);
            }
            else fear = Mathf.Clamp(AI.ObjRelationship(fearSource.abstractPhysicalObject).intensity, 0, 1);

            return fear;
        }
    }
    public virtual float FleeRadius => 450f; // Mathf.Lerp(150, 450, SourceOfFearIntensity);
    public virtual float ForcefleeRadius
    {
        get
        {
            if (fearSource?.abstractPhysicalObject is null)
            {
                return 0;
            }
            if (SourceOfFearIntensity >= 0.9f)
            {
                return 0;
            }
            float ForceFleeRad = FleeRadius / 4f;
            if (fearSource.abstractPhysicalObject is AbstractCreature absCtr)
            {
                if (AI.DynamicRelationship(absCtr).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    ForceFleeRad *= 2f;
                }
            }
            else if (AI.ObjRelationship(fearSource.abstractPhysicalObject).type == ObjectRelationship.Type.AfraidOf)
            {
                ForceFleeRad *= 2f;
            }

            return ForceFleeRad;
        }
    }

    public int FleeLevel = -1;


    public PhysicalObject useItem;
    public BodyChunk lookAtChunk;

    //---- Abilities ----//

    public bool lunging;
    public int lungeTimer;
    public int flashbombTimer;
    public int testingTimer;

    //---- Visual Stuff ----//
    public Color baseColor;
    public Color glowColor;
    public Color camoColor;
    public virtual Color MainBodyColor
    {
        get
        {
            Color bodyColor = Color.Lerp(Color.Lerp(baseColor, Color.black, 0.5f), glowColor, Juice);
            if (flicker > 0f)
            {
                bodyColor = Color.Lerp(bodyColor, Color.Lerp(baseColor, Color.black, 0.5f), flicker);
            }
            if (Behavior == Rush)
            {
                if (GlowState.stateTimeLimit > 140)
                {
                    bodyColor = Color.Lerp(camoColor, baseColor, Mathf.InverseLerp(0, 24, GlowState.rushPreyCounter));
                }
                else
                {
                    bodyColor = Color.Lerp(baseColor, bodyColor, Mathf.InverseLerp(140, 0, GlowState.stateTimeLimit));
                }
            }
            else if (Behavior == Hide)
            {
                bodyColor = Color.Lerp(camoColor, bodyColor, flicker);
            }
            else if (CamoFac > 0)
            {
                bodyColor = Color.Lerp(bodyColor, camoColor, CamoFac);
            }
            return bodyColor;
        }
    }
    public virtual Color OutlineColor
    {
        get
        {
            Color altColor = Color.Lerp(glowColor, baseColor, Juice);
            if (flicker > 0f)
            {
                altColor = Color.Lerp(altColor, glowColor, flicker);
            }
            if (Behavior == Rush)
            {
                if (GlowState.stateTimeLimit > 140)
                {
                    altColor = Color.Lerp(camoColor, glowColor, Mathf.InverseLerp(0, 4, GlowState.rushPreyCounter));
                }
                else
                {
                    altColor = Color.Lerp(glowColor, altColor, Mathf.InverseLerp(140, 0, GlowState.stateTimeLimit));
                }
            }
            else if (Behavior == Hide)
            {
                altColor = Color.Lerp(camoColor, altColor, flicker);
            }
            else if (CamoFac > 0)
            {
                altColor = Color.Lerp(altColor, camoColor, CamoFac);
            }
            return altColor;
        }
    }
    public override float VisibilityBonus
    {
        get
        {
            float visBonus = Juice;
            bool inverseFlicker = false;
            if (Behavior == Hide)
            {
                visBonus = -1;
                inverseFlicker = true;
            }
            else if (CamoFac > 0)
            {
                visBonus = Mathf.Lerp(visBonus, -1, CamoFac);
            }
            if (flicker > 0f)
            {
                visBonus = Mathf.Lerp(visBonus, inverseFlicker ? 1 : -1, flicker);
            }
            if (room is not null && visBonus != 0)
            {
                visBonus *= 1 - room.LightSourceExposure(DangerPos);
            }
            return visBonus;
        }
    }
    public virtual float LightExposure
    {
        get
        {
            if (room is not null)
            {
                return Mathf.Min(1, (LuminLightSourceExposure(DangerPos) + (1 - room.Darkness(DangerPos))) / 2f);
            }
            return 0;
        }
    } // Only used to affect Juice regen.

    public float flickeringFac;
    public float flicker;
    public float flickerDuration;

    public float blinkPitch;

    public float legsPosition;
    public float deathSpasms = 1f;

    //---- Idle Crawl ----//

    public int denMovement;

    public bool idle;
    public float idleCounter;

    public float connectDistance;
    public MovementConnection lastFollowingConnection;
    public MovementConnection followingConnection;
    public MovementConnection lastShortCut;
    private List<MovementConnection> path;
    private List<MovementConnection> scratchPath;
    private int pathCount;
    private int scratchPathCount;
    public int footingTimer;



    public LuminCreature(AbstractCreature absLmn, World world) : base(absLmn, world)
    {
        Random.State state = Random.state;
        Random.InitState(absLmn.ID.RandomSeed);
        baseColor = Custom.HSL2RGB((Random.value < 0.04f ? Random.value : Custom.WrappedRandomVariation(260 / 360f, 60 / 360f, 0.5f)), 0.4f, Custom.WrappedRandomVariation(0.5f, 0.125f, 0.3f));
        glowColor = Color.Lerp(baseColor, Color.white, 0.6f);
        Random.state = state;

        float rad = Mathf.Lerp(7.6f, 10.8f, Mathf.InverseLerp(0.8f, 1.2f, GlowState.ivars.Size)) * 2 / 3f;
        float mass = Mathf.Lerp(0.2f, 0.3f, Mathf.InverseLerp(0.8f, 1.2f, GlowState.ivars.Size));
        if (Role == Hunter && Dominant)
        {
            rad *= 1.2f;
            mass *= 1.4f;
        }
        for (int i = 5 - bites; i > 0; i--)
        {
            rad *= 0.85f;
            mass *= 0.85f;
        }
        bodyChunks = new BodyChunk[1] { new(this, 0, new Vector2(0f, 0f), rad, mass) };
        bodyChunkConnections = new BodyChunkConnection[0];
        collisionLayer = 1;
        ChangeCollisionLayer(collisionLayer);
        GoThroughFloors = true;
        gravity = 0.96f;
        bounce = 0.3f;
        surfaceFriction = 0.5f;
        waterFriction = 0.9f;
        airFriction = 0.99f;
        buoyancy = 0.96f;

        direction = Custom.DegToVec(Random.value * 360f);
        connectDistance = rad * 2;

        path = new List<MovementConnection>();
        pathCount = 0;
        scratchPath = new List<MovementConnection>();
        scratchPathCount = 0;
        if (world.rainCycle.CycleStartUp < 0.5f)
        {
            denMovement = -1;
        }
        else if (WantToHideInDen(abstractCreature))
        {
            denMovement = 1;
        }
        bloodlustRate = 1 / 160f;
        if (Role == Guardian)
        {
            bloodlustRate *= Dominant ? 2f : 1.2f;
        }
        else if (Role == Forager)
        {
            bloodlustRate /= 2f;
        }

        grasps = new Grasp[Role == Forager && Dominant ? 2 : 1];

    }
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule is null)
        {
            graphicsModule = new LuminGraphics(this);
        }
    }
    public virtual void Reset()
    {
        currentPrey = null;
        fearSource = null;
        lookAtChunk = null;
        lunging = false;
        lungeTimer = 0;
        bloodlust = 0;
        losingInterestInGrasp = 0;
        if (!Consious || !safariControlled)
        {
            flicker = 0;
            flock = null;
        }
    }


    //-----------------------------------------
    // Update
    public override void Update(bool eu)
    {
        //-----------------------------------------//

                        Refresh(eu);
        
        //-----------------------------------------//
        
        if (AI is null || room is null)
        {
            return;
        }


        GlowUpdate(eu);

        
        if (dead)
        {
            deathSpasms = Mathf.Max(0f, deathSpasms - (1 / Mathf.Lerp(200f, 400f, Random.value)));
        }
        IntVector2 tilePosition = room.GetTilePosition(body.pos);
        tilePosition.x = Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1);
        tilePosition.y = Custom.IntClamp(tilePosition.y, 0, room.TileHeight - 1);

        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            body.vel += Custom.DirVec(DangerPos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
            Stun(12);
        }

        if (timeWithoutPreyContact > 0)
        {
            timeWithoutPreyContact--;
        }
        else if (attachCounter > 0)
        {
            attachCounter--;
        }

        if (!Consious)
        {
            if (flashbombTimer > 0)
            {
                flashbombTimer = 0;
            }
            if (lungeTimer > 0)
            {
                lungeTimer = 0;
            }
            if (lunging)
            {
                lunging = false;
            }
            return;
        }
        legsPosition = 0f;


        if (safariControlled)
        {
            SafariControls(eu);
            return;
        }

        if (Role != Forager)
        {
            CamouflageBehavior();
        }


        lastNullpreyCounter = nullpreyCounter;
        PreyUpdate();
        UseitemUpdate();
        if (nullpreyCounter > 0 &&
            nullpreyCounter <= lastNullpreyCounter)
        {
            nullpreyCounter = 0;
        }
        if (Behavior == Hunt)
        {
            if (currentPrey is null && useItem is null)
            {
                GlowState.ChangeBehavior(Idle, 0);
            }
        }



        if (Behavior == ReturnPrey)
        {
            testingTimer++;

            if (AI.denFinder?.denPosition is null ||
                preyVisualCounter > (currentPrey is null ? 40 : Mathf.Lerp(320, 960, CurrentPreyRelationIntensity)))
            {
                GlowState.ChangeBehavior(Idle, 1);
            }

            if (AI.denFinder?.denPosition is not null &&
                room.abstractRoom.index == AI.denFinder.denPosition.Value.room)
            {
                if (testingTimer % 40 == 0)
                {
                    Vector2 denPos = room.MiddleOfTile(room.LocalCoordinateOfNode(AI.denFinder.denPosition.Value.abstractNode));
                    room.AddObject(new LuminBlink(DangerPos, denPos, default, 3, baseColor, baseColor));
                    room.AddObject(new LuminBlink(denPos, DangerPos, default, 3, baseColor, baseColor));
                }
            }
        }
        else
        {
            if (disagreementTimer > 0)
            {
                disagreementTimer = 0;
            }
        }


        if (fearSource is not null)
        {
            if (Behavior != Flee &&
                closestFearChunk is not null &&
                AI.VisualContact(closestFearChunk.pos))
            {
                GlowState.ChangeBehavior(Flee, FleeLevel);
            }
            if (!ConsiderThreatening(fearSource))
            {
                fearSource = null;
            }
        }
        if (Behavior == Flee)
        {

            testingTimer++;

            if (testingTimer % 20 == 0)
            {
                room.AddObject(new LuminBlink(body.pos + Custom.RNV() * 50f, DangerPos, default, 5, Color.cyan, Color.red));
            }

            if (closestFearChunk is not null && ConsiderUseful(grasps[0]?.grabbed) &&
                AI.ObjRelationship(grasps[0].grabbed.abstractPhysicalObject).type == ObjectRelationship.Type.Uses)
            {
                lookAtChunk = closestFearChunk;
            }

            if (Role == Forager && flashbombTimer == 0 && Juice >= 1 &&
               (FleeRadius < 200 || FleeLevel > 0))
            {
                flashbombTimer++;
            }

            if (GrabbingAnything)
            {
                if (FleeLevel > 0)
                {
                    if (grasps[0]?.grabbed is not null && grasps[0].grabbed.TotalMass > EasycarryMassLimit)
                    {
                        ReleaseGrasp(0);
                    }
                    if (grasps.Length > 1 && grasps[1]?.grabbed is not null && grasps[1].grabbed.TotalMass > BackholdMassLimit - TotalMass)
                    {
                        ReleaseGrasp(1);
                    }
                }
                for (int g = 0; g < grasps.Length; g++)
                {
                    if (grasps[g]?.grabbed is not null && !ConsiderUseful(grasps[g].grabbed) && !ConsiderPrey(grasps[g].grabbed))
                    {
                        ReleaseGrasp(g);
                    }
                }
            }

            if (Random.value < 0.01f && (fearSource is null || FleeLevel < 0))
            {
                GlowState.ChangeBehavior(Idle, 0);
            }
        }
        else if (flashbombTimer > 0 && flashbombTimer < 40)
        {
            flashbombTimer = 0;
        }


        if (flashbombTimer > 0)
        {
            Flashbomb();
        }
        else if (lungeTimer > 0)
        {
            Lunge();
        }


        if (!(room.GetTile(DangerPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && (followingConnection is null || followingConnection.DestTile != tilePosition)) &&
            !room.IsPositionInsideBoundries(tilePosition) && AI.inAccessibleTerrain)
        {
            footingTimer = 5;
            followingConnection = new MovementConnection(MovementConnection.MovementType.Standard, abstractCreature.pos, room.GetWorldCoordinate(tilePosition), 1);
        }

        if (WantToHideInDen(abstractCreature) && denMovement != 1)
        {
            denMovement = 1;
        }
        else if (!WantToHideInDen(abstractCreature) && denMovement != 0)
        {
            denMovement = 0;
        }


        GraspUpdate(eu);


        bool validConnection = followingConnection is null || followingConnection.type != MovementConnection.MovementType.DropToFloor;

        if (validConnection && (Submersion > 0 || AI.inAccessibleTerrain))
        {
            body.vel *= 0.7f;
            body.vel.y += gravity;
        }
        if (grasps[0] is null)
        {
            if (Submersion == 0 && validConnection && (AI.inAccessibleTerrain || footingTimer > 0) && Behavior != Hide && AI.pathFinder.nextDestination is null && abstractCreature.abstractAI.migrationDestination is null)
            {
                IdleCrawl();
            }

            if (!validConnection && !(AI.inAccessibleTerrain || footingTimer > 0))
            {
                followingConnection = null;
                if (pathCount > 0)
                {
                    pathCount = 0;
                }
            }
        }

    }
    public virtual void SafariControls(bool eu)
    {
        if (room.GetTile(coord).Solid && !room.GetTile(lastCoord).Solid)
        {
            Debug.Log("trying to set pos back in-bounds");
            body.HardSetPosition(room.MiddleOfTile(lastCoord));
        }
        if (Submersion > 0.3f || AI.inAccessibleTerrain)
        {
            body.vel *= 0.7f;
            body.vel.y += gravity;
        }
        if (currentPrey is not null && (currentPrey == this || GrabbedObjects.Contains(currentPrey)))
        {
            currentPrey = null;
        }
        if (inputWithDiagonals.HasValue)
        {
            if (inputWithDiagonals.Value.thrw && (!lastInputWithDiagonals.HasValue || !lastInputWithDiagonals.Value.thrw) && GrabbingAnything)
            {
                if (grasps[0] is not null)
                {
                    ThrowObject(eu);
                }
                else if (grasps.Length > 1 && grasps[1] is not null)
                {
                    ReleaseGrasp(1);
                }
            }
            if (grasps.Length > 1 && inputWithDiagonals.Value.pckp && GrabbingAnything &&
                (grasps[0]?.grabbed is null || (grasps[0].grabbed.TotalMass < BackholdMassLimit && (grasps[0].grabbed is not Creature grabbed || grabbed.dead))))
            {
                shouldTickGraspDelayCounter = true;
            }

            /*
            if (!lunging && lungeTimer < 1 && inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
            {
                lungeTimer++;
            }
            */
            if (lungeTimer > 0)
            {
                Lunge();
            }

            if ((!lastInputWithDiagonals.HasValue || !lastInputWithDiagonals.Value.pckp) && inputWithDiagonals.Value.pckp && grasps[0] is null)
            {
                foreach (AbstractWorldEntity absEnt in room.abstractRoom.entities)
                {
                    if (absEnt is AbstractCreature absCtr)
                    {
                        if (absCtr?.realizedCreature?.bodyChunks is null || absCtr.realizedCreature == this || GrabbedObjects.Contains(absCtr.realizedCreature) ||
                        (absCtr.realizedCreature.State is not null && absCtr.realizedCreature.State is GlowSpiderState))
                        {
                            continue;
                        }
                        bool foundPrey = false;
                        foreach (BodyChunk chunk in absCtr.realizedCreature.bodyChunks)
                        {
                            if (chunk is not null && Custom.DistLess(VisionPoint, chunk.pos, body.rad + chunk.rad))
                            {
                                currentPrey = absCtr.realizedCreature;
                                foundPrey = true;
                                break;
                            }
                        }
                        if (foundPrey)
                        {
                            break;
                        }
                    }
                    else if (absEnt is AbstractPhysicalObject absObj)
                    {
                        if (absObj?.realizedObject?.bodyChunks is null || GrabbedObjects.Contains(absObj.realizedObject) || absObj.realizedObject is not PlayerCarryableItem)
                        {
                            continue;
                        }
                        bool foundPrey = false;
                        foreach (BodyChunk chunk in absObj.realizedObject.bodyChunks)
                        {
                            if (chunk is not null && Custom.DistLess(VisionPoint, chunk.pos, body.rad + chunk.rad))
                            {
                                currentPrey = absObj.realizedObject;
                                foundPrey = true;
                                break;
                            }
                        }
                        if (foundPrey)
                        {
                            break;
                        }
                    }
                }
                TryToAttach(currentPrey);

            }
        }

        GraspUpdate(eu);

    }
    public virtual void Refresh(bool eu)
    {
        if (grasps is null)
        {
            grasps = new Grasp[Role == Forager && Dominant ? 2 : 1];
        }
        if (grasps.Length != 2 && Role == Forager && Dominant)
        {
            grasps = new Grasp[2];
        }
        else if (grasps.Length != 1 && !(Role == Forager && Dominant))
        {
            grasps = new Grasp[1];
        }

        if (currentPrey is not null && currentPrey == this)
        {
            currentPrey = null;
        }

        if (shouldTickGraspDelayCounter)
        {
            shouldTickGraspDelayCounter = false;
        }

        if (!dead && flock is null)
        {
            flock = new LuminFlock(this, room);
        }
        if (flock is not null)
        {
            flock.Update(eu);
        }

        if (FleeLevel != -1)
        {
            FleeLevel = -1;
        }
        if (closestFearChunk is not null)
        {
            closestFearChunk = null;
        }
        if (fearSource is not null)
        {
            float distToBeat = Custom.Dist(body.pos, fearSource.firstChunk.pos);
            closestFearChunk = fearSource.firstChunk;
            for (int b = 1; b < fearSource.bodyChunks.Length; b++)
            {
                float chunkDist = Custom.Dist(body.pos, fearSource.bodyChunks[b].pos);
                if (chunkDist < distToBeat)
                {
                    distToBeat = chunkDist;
                    closestFearChunk = fearSource.bodyChunks[b];
                }
            }

            if (distToBeat <= ForcefleeRadius)
            {
                FleeLevel = 1;
            }
            else if (distToBeat <= FleeRadius)
            {
                FleeLevel = 0;
            }
        }

        base.Update(eu);
        AI.Update();
        GlowState.Update(this, eu);

        dragPos = DangerPos + Custom.DirVec(DangerPos, dragPos) * connectDistance;
        VisionPoint = DangerPos + (direction * connectDistance);
        lastDirection = direction;
        if (heavycarryChunk is not null)
        {
            direction = Custom.DirVec(body.pos, heavycarryChunk.pos);
            heavycarryChunk = null;
        }
        else if (lookAtChunk is not null)
        {
            direction += Custom.DirVec(body.pos, lookAtChunk.pos) * 0.1f;
            if (!Consious)
            {
                direction += Custom.DegToVec(Random.value * 360f) * deathSpasms;
            }
            direction = direction.normalized;
            lookAtChunk = null;
        }
        else
        {
            direction -= Custom.DirVec(body.pos, dragPos);
            direction += body.vel * 0.25f;
            if (!Consious)
            {
                direction += Custom.DegToVec(Random.value * 360f) * deathSpasms;
            }
            direction = direction.normalized;
        }


        if (footingTimer > 0)
        {
            footingTimer--;
        }
    }


    //-----------------------------------------
    // Grasps

    public virtual void TryToAttach(PhysicalObject target)
    {
        if (grabCooldown > 0 ||
            (grasps[0] is not null && (grasps[0].grabbed is null || grasps[0].grabbed != useItem || useItem == target)) ||
            (grasps.Length > 1 && grasps[1]?.grabbed == target) ||
            target is null ||
            target == this ||
            target.slatedForDeletetion ||
            (target is not PlayerCarryableItem && target is not Creature) ||
            (currentPrey is not null && target == currentPrey && nullpreyCounter > 0))
        {
            return;
        }
        for (int i = 0; i < target.bodyChunks.Length; i++)
        {

            BodyChunk chunk = target.bodyChunks[i];

            if ((safariControlled ||
                ((attachCounter > 15 || target is not Creature || (target as Creature).Template.smallCreature) && (target is not Creature || (target as Creature).shortcutDelay < 1))) &&
                Custom.DistLess(VisionPoint, chunk.pos, body.rad + chunk.rad))
            {
                Grab(target, 0, i, Grasp.Shareability.NonExclusive, 0.2f, false, false);
                room.PlaySound(SoundID.Big_Spider_Grab_Creature, body.pos, 0.9f, 2.5f - GlowState.ivars.Size);
                grabCooldown = 20;
                return;
            }
            if (Random.value < 0.2f && !safariControlled && Behavior != Hide && Custom.DistLess(DangerPos, chunk.pos, body.rad + chunk.rad + 75))
            {
                body.vel += Custom.DirVec(VisionPoint, chunk.pos) * 5f;
                timeWithoutPreyContact = 15;
                attachCounter++;
                return;
            }
        }
    }
    public virtual void GraspUpdate(bool eu)
    {
        if (remainingGraspSwapTime > 0)
        {
            remainingGraspSwapTime = remainingGraspSwapTime < 0.001f ? 0 : Mathf.Lerp(remainingGraspSwapTime, 0, 0.1f);
        }

        if (GrabbingAnything)
        {
            if (grasps[0]?.grabbed is not null &&
                grasps[0].grabbed is not Creature &&
                grasps[0].grabbed is not PlayerCarryableItem)
            {
                ReleaseGrasp(0);
            }
            if (grasps.Length > 1 &&
                grasps[1]?.grabbed is not null &&
                grasps[1].grabbed is not Creature &&
                grasps[1].grabbed is not PlayerCarryableItem)
            {
                ReleaseGrasp(1);
            }
        }

        if (grasps[0] is not null)
        {
            MainGrasp(eu);
            DenConflictSettler();
            if (!safariControlled)
            {
                if (WantToBackcarry(grasps[0]?.grabbed) && !WantToBackcarry(grasps[1]?.grabbed))
                {
                    shouldTickGraspDelayCounter = true;
                }
                if (Behavior == Flee &&
                    lookAtChunk is not null &&
                    ConsiderUseful(grasps[0]?.grabbed) &&
                    AI.ObjRelationship(grasps[0].grabbed.abstractPhysicalObject).type == ObjectRelationship.Type.Uses &&
                    Mathf.Abs(Custom.VecToDeg(Custom.DirVec(body.pos, lookAtChunk.pos)) - Custom.VecToDeg(direction)) <= 5f)
                {
                    ThrowObject(eu);
                }
            }
        }
        else
        {
            if (losingInterestInGrasp > 0)
            {
                losingInterestInGrasp = 0;
            }
            if (cantcarryWaitTime > 0)
            {
                cantcarryWaitTime = 0;
            }
        }

        if (grasps.Length > 1 && grasps[1]?.grabbed is not null)
        {
            if (grasps[1].grabbed.TotalMass >= BackholdMassLimit)
            {
                ReleaseGrasp(1);
                Stun(40);
            }
            else
            {
                body.vel.x *= 0.4f + (Mathf.InverseLerp(AttackMassLimit, TotalMass, grasps[1].grabbed.TotalMass) * 0.6f);
                body.vel.y -= Mathf.InverseLerp(TotalMass, AttackMassLimit, grasps[1].grabbed.TotalMass);
                Vector2 backCarryPos = DangerPos;
                if (grasps[1].grabbed.bodyChunks.Length == 2)
                {
                    backCarryPos.y += 10f;
                }
                BodyChunk chunk = grasps[1].grabbed.bodyChunks.Length > 2 ? grasps[1].grabbed.bodyChunks[Mathf.FloorToInt(grasps[1].grabbed.bodyChunks.Length / 2)] : grasps[1].grabbedChunk;
                chunk.MoveFromOutsideMyUpdate(eu, (remainingGraspSwapTime > 0) ? Vector2.Lerp(backCarryPos, VisionPoint, remainingGraspSwapTime / 10f) : backCarryPos);
                chunk.vel = body.vel;
                if (WantToBackcarry(grasps[0]?.grabbed) && !WantToBackcarry(grasps[1].grabbed))
                {
                    shouldTickGraspDelayCounter = true;
                }
            }
        }

        if (Submersion <= 0.3f && !AI.inAccessibleTerrain && GrabbingAnything)
        {
            float floatyPower = 0;
            for (int g = 0; g < grasps.Length; g++)
            {
                if (grasps[g]?.grabbed is null)
                {
                    continue;
                }
                if (grasps[g].grabbed is DandelionPeach)
                {
                    floatyPower += 0.2f;
                }
                else if (grasps[g].grabbed is BubbleGrass grs && grs.oxygen > 0)
                {
                    floatyPower += 0.25f * grs.oxygen;
                }
            }
            if (floatyPower > 0)
            {
                if (body.vel.y < 0)
                {
                    body.vel.y *= 1 - floatyPower;
                }
                body.vel.x *= 1 - floatyPower;
                if (inputWithDiagonals.HasValue)
                {
                    body.vel.x += inputWithDiagonals.Value.x * 0.75f;
                }
            }
        }

        if (shouldTickGraspDelayCounter)
        {
            graspSwapDelay++;
            if (graspSwapDelay > 25)
            {
                SwitchGrasps(0, 1);
                remainingGraspSwapTime = 10;
                graspSwapDelay = 0;
            }
        }
        else if (graspSwapDelay > 0)
        {
            graspSwapDelay = 0;
        }

        if (grabCooldown > 0)
        {
            grabCooldown--;
        }


        if (grasps.Length < 2 &&
            ConsiderPrey(grasps[0]?.grabbed) &&
            (grasps[0].grabbed is not Creature || (grasps[0].grabbed as Creature).dead))
        {
            GlowState.ChangeBehavior(ReturnPrey, 0);
        }
        else
        if (grasps.Length > 1 &&
            grasps[0] is not null &&
            grasps[1] is not null &&
            ((ConsiderPrey(grasps[0].grabbed) && (grasps[0].grabbed is not Creature || (grasps[0].grabbed as Creature).dead)) ||
                (ConsiderPrey(grasps[1].grabbed) && (grasps[1].grabbed is not Creature || (grasps[1].grabbed as Creature).dead))))
        {
            GlowState.ChangeBehavior(ReturnPrey, 0);
        }

    }
    public virtual void MainGrasp(bool eu)
    {
        if (grasps[0]?.grabbedChunk?.owner is null)
        {
            return;
        }
        BodyChunk chunk = grasps[0].grabbedChunk;
        if (chunk.owner == this)
        {
            ReleaseGrasp(0);
            losingInterestInGrasp = 0;
            return;
        }

        if ((safariControlled || chunk.owner is not Creature grabbee || (Behavior == ReturnPrey && AI.DynamicRelationship(grabbee.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)) &&
            chunk.owner.TotalMass < EasycarryMassLimit &&
           (chunk.owner is not Creature owner || owner.dead) &&
          !(Submersion <= 0.3f && !AI.inAccessibleTerrain && chunk.owner is DandelionPeach))
        {
            if (losingInterestInGrasp > 800 || (chunk.owner is Creature ctr && ctr.enteringShortCut.HasValue))
            {
                ReleaseGrasp(0);
                losingInterestInGrasp = 0;
            }
            else
            {
                chunk.MoveFromOutsideMyUpdate(eu, enteringShortCut.HasValue ? DangerPos : (remainingGraspSwapTime == 0 ? VisionPoint : Vector2.Lerp(VisionPoint, DangerPos, remainingGraspSwapTime / 10f)));
                chunk.vel = body.vel;
            }
            return;
        }
        heavycarryChunk = chunk;
        Vector2 pushAngle = Custom.DirVec(DangerPos, chunk.pos);
        float chunkDistGap = Custom.Dist(DangerPos, chunk.pos);
        float chunKRadii = body.rad + chunk.rad;
        float massFac = TotalMass / (TotalMass + chunk.mass);
        body.vel += pushAngle * (chunkDistGap - chunKRadii) * (1f - massFac);
        body.pos += pushAngle * (chunkDistGap - chunKRadii) * (1f - massFac);
        chunk.vel -= pushAngle * (chunkDistGap - chunKRadii) * massFac;
        chunk.pos -= pushAngle * (chunkDistGap - chunKRadii) * massFac;

        float TotalLmnMass = 0f;
        int TotalLmnCount = 0;
        int LmnNum = -1;
        for (int g = 0; g < chunk.owner.grabbedBy.Count; g++)
        {
            Creature grabber = chunk.owner.grabbedBy[g].grabber;
            if (grabber is LuminCreature)
            {
                TotalLmnMass += grabber.TotalMass;
                TotalLmnCount++;
                if (grabber == this)
                {
                    LmnNum = g;
                }
            }
        }

        if (chunk.owner is not Creature target)
        {
            if (!safariControlled)
            {
                if (Behavior != ReturnPrey &&
                    AI.denFinder.denPosition.HasValue &&
                    ConsiderPrey(chunk.owner) &&
                    (TotalLmnCount == 1 || TotalLmnMass * 3f < chunk.owner.TotalMass))
                {
                    GlowState.ChangeBehavior(ReturnPrey, 0);
                }
                if (Behavior == ReturnPrey &&
                    TotalLmnCount > 1 &&
                    TotalLmnMass * 2f > chunk.owner.TotalMass)
                {
                    cantcarryWaitTime++;
                    if (cantcarryWaitTime > 320)
                    {
                        cantcarryWaitTime = 0;
                        ReleaseGrasp(0);
                        GlowState.ChangeBehavior(Idle, 1);
                    }
                }
                else if (cantcarryWaitTime > 0)
                {
                    cantcarryWaitTime--;
                }
            }
            return;
        }

        if (!safariControlled)
        {
            if (Behavior != ReturnPrey &&
                AI.denFinder.denPosition.HasValue &&
                target.dead &&
                ConsiderPrey(chunk.owner) &&
                (TotalLmnCount == 1 || TotalLmnMass * 3f < target.TotalMass))
            {
                GlowState.ChangeBehavior(ReturnPrey, 0);
            }
            if (Behavior == ReturnPrey && (!target.dead || (TotalLmnCount > 1 && TotalLmnMass * 2f > target.TotalMass)))
            {
                cantcarryWaitTime++;
                if (cantcarryWaitTime > 320)
                {
                    cantcarryWaitTime = 0;
                    ReleaseGrasp(0);
                    GlowState.ChangeBehavior(Idle, 1);
                }
            }
            else if (cantcarryWaitTime > 0)
            {
                cantcarryWaitTime--;
            }
        }

        if (!target.dead)
        {
            if (!safariControlled && TotalLmnCount == 1)
            {
                losingInterestInGrasp++;
            }
            else
            {
                if (losingInterestInGrasp > 0)
                {
                    losingInterestInGrasp = Mathf.Max(0, losingInterestInGrasp - (TotalLmnCount - 1));
                }
            }

            if (target.State is PlayerState ps)
            {
                if (TotalLmnCount >= 3 || TotalLmnMass >= chunk.owner.TotalMass)
                {
                    float drain = Mathf.Lerp(TotalLmnCount, 1, 0.75f) / 800f;
                    if (Behavior == Aggravated)
                    {
                        drain *= 1.4f;
                    }
                    ps.permanentDamageTracking += drain;
                    if (Random.value < (ps.permanentDamageTracking - 1) / 5f)
                    {
                        target.Die();
                    }
                }
            }
            else if (target.State is HealthState hs)
            {
                if (TotalLmnCount >= 3 || TotalLmnMass >= chunk.owner.TotalMass)
                {
                    float drain = Mathf.Lerp(TotalLmnCount, 1, 0.5f) / 400f / target.Template.baseDamageResistance;
                    if (target.Template.damageRestistances[DamageType.Bite.index, 0] > 0)
                    {
                        drain /= target.Template.damageRestistances[DamageType.Bite.index, 0];
                    }
                    if (Behavior == Aggravated)
                    {
                        drain *= 1.4f;
                    }
                    hs.health -= drain;
                }
            }
            else
            {
                target.Die();
                room.PlaySound(SoundID.Big_Spider_Slash_Creature, body.pos, 0.9f, 2.5f - GlowState.ivars.Size);
            }
        }
        else
        if (!safariControlled && chunk.owner is Creature &&
            (Behavior != ReturnPrey || AI.DynamicRelationship((chunk.owner as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats))
        {
            losingInterestInGrasp += TotalLmnCount - LmnNum + (Behavior == Rush ? 12 : 6);
        }

        if (target.enteringShortCut.HasValue || losingInterestInGrasp > 800)
        {
            ReleaseGrasp(0);
            losingInterestInGrasp = 0;
            return;
        }
    }
    public virtual void ThrowObject(bool eu)
    {
        if (grasps?[0]?.grabbed is null || grasps[0].grabbedChunk is null)
        {
            return;
        }
        Grasp grasp = grasps[0];
        if (grasp.grabbedChunk is not null)
        {
            float weightMult = Mathf.InverseLerp(AttackMassLimit / 2f, 0, grasp.grabbed.TotalMass);
            if (grasp.grabbed is Rock ||
                grasp.grabbed is FirecrackerPlant ||
                grasp.grabbed is ScavengerBomb ||
                grasp.grabbed is SporePlant ||
                grasp.grabbed is LillyPuck)
            {
                IntVector2 throwDir = new(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
                float force = GlowState.health * (grasp.grabbed is LillyPuck ? 0.3f : 0.4f);

                (grasp.grabbed as Weapon).Thrown(this, VisionPoint, VisionPoint + (direction * 20), throwDir, force, eu);
                grasp.grabbedChunk.vel += body.vel * 0.2f * weightMult;
            }
            else
            {
                grasp.grabbedChunk.vel += direction * 9f * weightMult;
            }
            if (grasp.grabbed is JellyFish jelly)
            {
                jelly.Tossed(this);
            }
        }
        ReleaseGrasp(0);
        grabCooldown = 40;
    }
    public virtual void DenConflictSettler()
    {   // If Lumins from different dens try to carry prey together, they may get stuck trying to go in different directions.
        // This should let them agree on a den. One way or another...

        if (Behavior != ReturnPrey || grasps[0]?.grabbed is null || grasps[0].grabbed.grabbedBy.Count < 2)
        {
            return;
        }

        bool disagreement = false;
        for (int g = 0; g < grasps[0].grabbed.grabbedBy.Count; g++)
        {
            Grasp grasp = grasps[0].grabbed.grabbedBy[g];
            if (grasp.grabber is null ||
                grasp.grabber is not LuminCreature otherLmn ||
                otherLmn == this ||
                AI?.denFinder?.denPosition is null ||
                otherLmn.AI?.denFinder?.denPosition is null ||
                AI.denFinder.denPosition == otherLmn.AI.denFinder.denPosition)
            {
                continue;
            }

            if (!disagreement)
            {
                disagreement = true;
            }

            if (disagreementTimer < 240)
            {
                continue;
            }

            // ----DOMINANCE CHECK----
            // If one Lumin is significantly more dominant than another, the other Lumin will give in and switch their den to that of the more dominant Lumin.
            if (GlowState.ivars.dominance - otherLmn.GlowState.ivars.dominance > 0.1f)
            {
                otherLmn.AI.denFinder.denPosition = AI.denFinder.denPosition;
                disagreement = false;
                continue;
            }
            if (GlowState.ivars.dominance - otherLmn.GlowState.ivars.dominance < -0.1f)
            {
                AI.denFinder.denPosition = otherLmn.AI.denFinder.denPosition;
                disagreement = false;
                continue;
            }
            // If two Lumins are close enough in dominance, though... a fight will break out.
            if (disagreementTimer < Mathf.Lerp(320, 640, Mathf.Abs(GlowState.ivars.dominance - otherLmn.GlowState.ivars.dominance)))
            {
                continue;

            }// Well, sometimes. Lumins are smart bugs; they might figure something out before then, even if they end up wasting a little more time.
            else if (Random.value < disagreementTimer / 40000f)
            {
                if (Random.value < 0.4f)
                {
                    otherLmn.AI.denFinder.denPosition = AI.denFinder.denPosition;
                    disagreementTimer -= 80;
                    disagreement = false;
                    continue;
                }
                else
                {
                    AI.denFinder.denPosition = otherLmn.AI.denFinder.denPosition;
                    disagreementTimer -= 80;
                    disagreement = false;
                    continue;
                }
            }

            // If they DO end up fighting, they'll go at it until one of them either loses interest or gets their shit kicked in.

        }

        if (disagreement)
        {
            disagreementTimer++;
        }
        else if (disagreementTimer > 0)
        {
            disagreementTimer = 0;
        }
    }

    public virtual bool GrabbingItem(PhysicalObject item)
    {
        if (item is null)
        {
            return false;
        }

        if (grasps[0]?.grabbed == item)
        {
            return true;
        }
        if (grasps.Length > 1 && grasps[1]?.grabbed == item)
        {
            return true;
        }

        return false;
    }
    public virtual bool WantToBackcarry(PhysicalObject target)
    {
        if (safariControlled || grasps.Length < 2 || target is null || target.TotalMass > BackholdMassLimit)
        {
            return false;
        }

        float HeldMass = (grasps[0]?.grabbed is not null) ? grasps[0].grabbed.TotalMass : -1;
        float BackMass = (grasps[1]?.grabbed is not null) ? grasps[1].grabbed.TotalMass : -1;

        if (ConsiderPrey(target) && (target is not Creature ctr || ctr.dead))
        {
            if (HeldMass > -1 && target == grasps[0].grabbed)
            {
                if (HeldMass > BackMass)
                {
                    return true;
                }
            }
            else
            if (BackMass > -1 && target == grasps[1].grabbed)
            {
                if (BackMass > HeldMass)
                {
                    return true;
                }
            }
        }

        if (ConsiderUseful(target))
        {
            if (AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.Likes)
            {
                return true;
            }
            if (Behavior != Flee &&
                currentPrey is not null &&
                grasps[0]?.grabbed != currentPrey &&
                grasps[1]?.grabbed != currentPrey)
            {
                return true;
            }
        }
        else
        {
            if (HeldMass > -1 && target == grasps[0].grabbed &&
                BackMass > -1 && ConsiderUseful(grasps[1].grabbed) &&
                AI.ObjRelationship(grasps[1].grabbed.abstractPhysicalObject).type == ObjectRelationship.Type.Uses)
            {
                return true;
            }
        }

        return false;
    }


    //-----------------------------------------
    // Creature & Object Interactions

    public virtual void PreyUpdate()
    {
        if (currentPrey is null)
        {
            return;
        }

        if (room.abstractRoom.entities.Contains(currentPrey.abstractPhysicalObject))
        {
            foreach (AbstractWorldEntity ent in room.abstractRoom.entities)
            {
                if (ent is not AbstractPhysicalObject absObj || absObj != currentPrey.abstractPhysicalObject)
                {
                    continue;
                }

                if (absObj.InDen)
                {
                    currentPrey = null;
                    return;
                }

                if (absObj.realizedObject?.room is null ||
                    absObj.Room.realizedRoom != currentPrey.room ||
                    (CWT.ObjectData.TryGetValue(currentPrey, out ObjectInfo oI) && oI.inShortcut))
                {
                    nullpreyCounter++;
                }
                else
                {
                    currentPrey = absObj.realizedObject;
                }

                break;
            }
        }
        else
        {
            nullpreyCounter++;
        }

        if (nullpreyCounter > 320 || !ConsiderPrey(currentPrey) || preyVisualCounter > Mathf.Lerp(320, 960, CurrentPreyRelationIntensity))
        {
            currentPrey = null;
            if (Behavior == ReturnPrey)
            {
                GlowState.ChangeBehavior(Idle, 1);
            }
            return;
        }

        if (Behavior == Flee || currentPrey?.grabbedBy is null)
        {
            currentPrey = null;
            return;
        }

        float TotalLmnMass = 0f;
        int TotalLmnCount = 0;
        bool grabbing = false;
        for (int i = 0; i < currentPrey.grabbedBy.Count; i++)
        {
            Creature grabber = currentPrey.grabbedBy[i].grabber;
            if (grabber is LuminCreature)
            {
                TotalLmnMass += grabber.TotalMass;
                TotalLmnCount++;
                if (grabber == this)
                {
                    grabbing = true;
                }
            }
            else if (grabber is not null)
            {
                currentPrey = null;
                return;
            }
        }
        if ((currentPrey is not Creature prey || prey.dead) &&
            TotalLmnCount > (grabbing ? 1 : 0) &&
            TotalLmnMass * 2f > currentPrey.TotalMass)
        {
            currentPrey = null;
            return;
        }

        if (Random.value < 0.1f) room.AddObject(new LuminBlink(MainChunkOfObject(currentPrey).pos, MainChunkOfObject(currentPrey).pos + (Custom.DirVec(MainChunkOfObject(currentPrey).pos, body.pos) * 100f), default, 3, baseColor, baseColor));

        if (currentPrey is Creature ctr)
        {
            if (!ctr.dead && bloodlust < 3)
            {
                ChangeBloodlust(bloodlustRate * Mathf.Clamp(CurrentPreyRelationIntensity, 0.1f, 1));
            }

            if (AI.VisualContact(ctr.mainBodyChunk.pos))
            {
                GlowState.timeSincePreyLastSeen = 0;

                if (bloodlust >= 1 &&
                    (grasps[0]?.grabbed is null ||
                     grasps[0].grabbed != currentPrey))
                {
                    GlowState.ChangeBehavior(Hunt, 0);
                }
            }

            if (!grabbing &&
                (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && (!ctr.dead || (Behavior == ReturnPrey && (TotalLmnCount == 0 || TotalLmnMass * 3f < ctr.TotalMass)))) ||
                (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Attacks && Behavior != ReturnPrey && !ctr.dead))
            {
                TryToAttach(ctr);
            }
        }
        else
        {
            Vector2 mainItemChunk = currentPrey.bodyChunks[currentPrey.bodyChunks.Length < 3 ? 0 : currentPrey.bodyChunks.Length / 2].pos;
            if (AI.VisualContact(mainItemChunk))
            {
                GlowState.timeSincePreyLastSeen = 0;
                if (grasps[0]?.grabbed is null ||
                    grasps[0].grabbed != currentPrey)
                {
                    GlowState.ChangeBehavior(Hunt, 0);
                }
            }

            if (!grabbing &&
                (TotalLmnCount == 0 || TotalLmnMass * 3f < currentPrey.TotalMass) &&
                AI.ObjRelationship(currentPrey.abstractPhysicalObject).type == ObjectRelationship.Type.Eats)
            {
                TryToAttach(currentPrey);
            }
        }
    }
    public virtual void UseitemUpdate()
    {
        if (useItem?.grabbedBy is null)
        {
            return;
        }
        if (useItem is Creature || !ConsiderUseful(useItem))
        {
            useItem = null;
            return;
        }

        float TotalLmnMass = 0f;
        int TotalLmnCount = 0;
        bool grabbing = false;
        for (int i = 0; i < useItem.grabbedBy.Count; i++)
        {
            Creature grabber = useItem.grabbedBy[i].grabber;
            if (grabber is LuminCreature)
            {
                TotalLmnMass += grabber.TotalMass;
                TotalLmnCount++;
                if (grabber == this)
                {
                    grabbing = true;
                }
            }
            else if (grabber is not null)
            {
                useItem = null;
                return;
            }
        }

        if (TotalLmnMass * 2f > useItem.TotalMass &&
            TotalLmnCount > (grabbing ? 1 : 0))
        {
            useItem = null;
            return;
        }

        if (!grabbing && AI.VisualContact(MainChunkOfObject(useItem).pos))
        {
            if (Behavior != Flee)
            {
                GlowState.ChangeBehavior(Hunt, 0);
            }
            TryToAttach(useItem);
        }

    }
    public virtual void ChangeBloodlust(float change)
    {
        if (change > 0 && bloodlust < 3)
        {
            if (bloodlust >= 2)
            {
                change /= 9f;
            }
            else if (bloodlust >= 1)
            {
                change /= 3f;
            }
        }
        bloodlust = Mathf.Clamp(bloodlust + change, 0, 3);
    }
    public override void Stun(int st)
    {
        blinkPitch = Mathf.InverseLerp(-40, 40, st);
        base.LoseAllGrasps();
        base.Stun(st);
    }
    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float dmg, float stunBonus)
    {
        if (source?.owner is not null)
        {
            if (source.owner is Rock)
            {
                dmg *= 5;
            }
            bool FUCKEMUP = false;
            if (source.owner is Creature ctr)
            {
                if (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    fearSource = ctr;
                }
                else if (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowSpiderState.Behavior reaction = bloodlust >= 2 ? Aggravated : Hunt;
                    GlowState.ChangeBehavior(reaction, 0);
                    if (Behavior == Aggravated)
                    {
                        GlowState.stateTimeLimit = (int)Mathf.Lerp(640, 961, AI.DynamicRelationship(ctr.abstractCreature).intensity);
                    }
                    FUCKEMUP =
                        AI.DynamicRelationship(ctr.abstractCreature).intensity == 1 &&
                        AI.VisualContact(ctr.mainBodyChunk.pos) &&
                        WillingToDitchCurrentPrey(currentPrey);
                }
                else if (ctr.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    if (Behavior != Aggravated && Behavior != Rush && ctr.Template.CreatureRelationship(this).intensity >= 0.9f)
                    {
                        fearSource = ctr;
                    }
                }
                if (FUCKEMUP)
                {
                    currentPrey = ctr;
                }
            }
            else if (source.owner is Weapon wpn && wpn.thrownBy is not null && wpn.thrownBy is Creature thrower)
            {
                if (AI.DynamicRelationship(thrower.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    fearSource = thrower;
                }
                else if (AI.DynamicRelationship(thrower.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    GlowSpiderState.Behavior reaction = bloodlust >= 2 ? Aggravated : Hunt;
                    GlowState.ChangeBehavior(reaction, 0);
                    if (Behavior == Aggravated)
                    {
                        GlowState.stateTimeLimit = (int)Mathf.Lerp(480, 721, AI.DynamicRelationship(thrower.abstractCreature).intensity);
                    }
                    FUCKEMUP =
                        AI.DynamicRelationship(thrower.abstractCreature).intensity == 1 &&
                        AI.VisualContact(thrower.mainBodyChunk.pos) &&
                        WillingToDitchCurrentPrey(currentPrey);
                }
                else if (thrower.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    if (Behavior != Aggravated && Behavior != Rush && thrower.Template.CreatureRelationship(this).intensity >= 0.9f)
                    {
                        fearSource = thrower;
                    }
                }
                if (FUCKEMUP)
                {
                    currentPrey = thrower;
                }
            }
        }

        float dmgFac = Mathf.Clamp(dmg, 0, 5);

        if (type == DamageType.Electric || type == HailstormEnums.Heat)
        {
            GlowState.juice += dmgFac / 4f;
            if (Behavior == Overloaded)
            {
                dmg *= 1.5f;
            }
        }
        else if (type == DamageType.Stab || type == DamageType.Blunt)
        {
            GlowState.juice -= Mathf.Lerp(dmgFac / 5f, 0.2f, 1 / 3f);
        }

        if (room is not null)
        {
            room.PlaySound(SoundID.Snail_Warning_Click, DangerPos, Mathf.Clamp(1.2f + (dmgFac / 4f), 1, 2), Random.Range(0.5f, 1.5f));
            room.AddObject(new LuminBlink(DangerPos, DangerPos, default, 0.5f + (dmgFac / 4f), baseColor, glowColor));
        }

        for (int s = 0; s < dmgFac * 10; s++)
        {
            EmitSparks(1, Custom.RNV() * Random.Range(7f, 12f) * dmgFac, 40f * dmgFac);
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, dmg, stunBonus);
    }
    public virtual void Overload()
    {
        GlowState.ChangeBehavior(Overloaded, 2);
        if (room is not null)
        {
            room.AddObject(new LuminFlash(room, body, 200, 40, Color.Lerp(baseColor, glowColor, 0.5f), Random.Range(1.4f, 1.8f), true));
        }
        flashbombTimer = 0;
        GlowState.health -= 0.2f;
        GlowState.darknessCounter = 0;
        GlowState.timeSincePreyLastSeen = 0;
        int stun = (int)Mathf.Lerp(270, 420, (Mathf.InverseLerp(1.2f, 0.8f, GlowState.ivars.Size) * 2 / 3f) + (Mathf.InverseLerp(1.2f, 0.8f, GlowState.ivars.Fatness) * 1 / 3f));
        Stun(stun);
    }

    public virtual bool ConsiderPrey(PhysicalObject target)
    {
        if (target?.abstractPhysicalObject is null || target.TotalMass > AttackMassLimit || target.abstractPhysicalObject.InDen)
        {
            return false;
        }

        if (target is Creature ctr)
        {
            if (ctr.State is not null && ctr.State is GlowSpiderState)
            {
                return false;
            }
            if (AI.DynamicRelationship(ctr.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats &&
                AI.DynamicRelationship(ctr.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks)
            {
                return false;
            }
            if (ctr.dead &&
                AI.DynamicRelationship(ctr.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats)
            {
                return false;
            }
        }
        else
        {
            if (AI.ObjRelationship(target.abstractPhysicalObject).type != ObjectRelationship.Type.Eats)
            {
                return false;
            }
        }


        if (target.TotalMass > EasycarryMassLimit &&
            (grasps.Length < 2 || target.TotalMass > BackholdMassLimit) &&
            flock?.lumins is not null)
        {
            float TotalFlockMass = 0f;
            float RequiredWeight = target.TotalMass / 3f;
            for (int l = 0; TotalFlockMass < RequiredWeight && l < flock.lumins.Count; l++)
            {
                if (flock.lumins[l] is not null)
                {
                    TotalFlockMass += flock.lumins[l].TotalMass;
                }
            }
            if (TotalFlockMass < RequiredWeight)
            {
                return false;
            }
        }

        if (target.grabbedBy is not null)
        {
            float TotalLmnMass = 0f;
            int TotalLmnCount = 0;
            bool Grabbing = false;
            for (int i = 0; i < target.grabbedBy.Count; i++)
            {
                Creature grabber = target.grabbedBy[i].grabber;
                if (grabber is LuminCreature)
                {
                    TotalLmnMass += grabber.TotalMass;
                    TotalLmnCount++;
                    if (grabber == this)
                    {
                        Grabbing = true;
                    }
                }
                else if (grabber is not null)
                {
                    return false;
                }
            }

            bool TooManyLumins =
                TotalLmnCount > (Grabbing ? 1 : 0) &&
                TotalLmnMass * 2f > target.TotalMass;

            if ((target is not Creature grabbed || grabbed.dead) && TooManyLumins)
            {
                return false;
            }

        }

        return true;
    }
    public virtual bool ConsiderThreatening(PhysicalObject target)
    {
        if (target?.abstractPhysicalObject is null || target.abstractPhysicalObject.InDen)
        {
            return false;
        }

        if (target is Creature ctr)
        {
            if (ctr.State is not null && ctr.State is GlowSpiderState)
            {
                return false;
            }
            if (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.StayOutOfWay ||
                AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
            {
                return true;
            }
            if (ctr.Template.CreatureRelationship(this).type == CreatureTemplate.Relationship.Type.Eats && !ConsiderPrey(ctr))
            {
                return true;
            }
        }
        else
        if (AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.Avoids ||
            AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.AfraidOf)
        {
            return true;
        }

        return false;
    }
    public virtual bool ConsiderUseful(PhysicalObject target)
    {
        if (target?.abstractPhysicalObject is null ||
            target.abstractPhysicalObject.InDen ||
            target is Creature ||
            target.room != room ||
            target.TotalMass > BackholdMassLimit ||
            (CWT.ObjectData.TryGetValue(target, out ObjectInfo oI) && oI.inShortcut))
        {
            return false;
        }

        if (AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.Uses ||
            AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.Likes)
        {
            return true;
        }
        if (AI.ObjRelationship(target.abstractPhysicalObject).type == ObjectRelationship.Type.PlaysWith)
        {
            if (Behavior == Idle && currentPrey is null && fearSource is null)
            {
                return true;
            }
        }

        return false;
    }
    public virtual bool WillingToDitchCurrentPrey(PhysicalObject newTarget)
    {
        if (currentPrey is null)
        {
            return true;
        }
        if (newTarget is null ||
            newTarget == currentPrey ||
           !AI.VisualContact(MainChunkOfObject(newTarget).pos))
        {
            return false;
        }

        if (grasps.Length > 1 &&
            grasps[1]?.grabbed is not null &&
            grasps[1] .grabbed == currentPrey)
        {
            return true;
        }
        if (!Custom.DistLess(body.pos, MainChunkOfObject(newTarget).pos, Custom.Dist(body.pos, MainChunkOfObject(currentPrey).pos) * 1.2f))
        {
            return false;
        }

        if (newTarget is Creature ctr)
        {
            if (Behavior == ReturnPrey)
            {
                if (!ctr.dead && (
                    (AI.DynamicRelationship(ctr.abstractCreature).type ==  CreatureTemplate.Relationship.Type.Eats   && AI.DynamicRelationship(ctr.abstractCreature).intensity >= CurrentPreyRelationIntensity * 2f) ||
                    (AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Attacks && AI.DynamicRelationship(ctr.abstractCreature).intensity >= CurrentPreyRelationIntensity * 3f)))
                {
                    return true;
                }
                if (ctr.dead &&
                    AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats &&
                    AI.DynamicRelationship(ctr.abstractCreature).intensity > CurrentPreyRelationIntensity * 4f)
                {
                    return true;
                }
                if (preyVisualCounter >= Mathf.Lerp(100, 1200, bloodlust / 3f))
                {
                    return true;
                }
            }
            else if (!ctr.dead && preyVisualCounter > (400 * CurrentPreyRelationIntensity) - (160 * AI.DynamicRelationship(ctr.abstractCreature).intensity))
            {
                return true;
            }
        }
        else
        {
            if (Behavior == ReturnPrey)
            {
                if (AI.ObjRelationship(newTarget.abstractPhysicalObject).intensity >= CurrentPreyRelationIntensity * 4f && (
                    AI.ObjRelationship(newTarget.abstractPhysicalObject).type == ObjectRelationship.Type.Eats ||
                    AI.ObjRelationship(newTarget.abstractPhysicalObject).type == ObjectRelationship.Type.Uses))
                {
                    return true;
                }
                if (AI.ObjRelationship(newTarget.abstractPhysicalObject).intensity >= CurrentPreyRelationIntensity * 6f && (
                    AI.ObjRelationship(newTarget.abstractPhysicalObject).type == ObjectRelationship.Type.Likes ||
                    AI.ObjRelationship(newTarget.abstractPhysicalObject).type == ObjectRelationship.Type.PlaysWith))
                {
                    return true;
                }
                if (preyVisualCounter >= Mathf.Lerp(100, 1200, bloodlust / 3f))
                {
                    return true;
                }
            }
            else if (preyVisualCounter > (400 * CurrentPreyRelationIntensity) - (160 * AI.ObjRelationship(newTarget.abstractPhysicalObject).intensity))
            {
                return true;
            }
        }
        return false;
    }
    public virtual bool MoreAppealingThanCurrentItem(PhysicalObject newItem)
    {
        if (newItem is null)
        {
            return false;
        }
        if (useItem is null)
        {
            return true;
        }
        if (newItem == useItem)
        {
            return false;
        }

        float newItemAppeal = AI.ObjRelationship(newItem.abstractPhysicalObject).intensity;
        float oldItemAppeal = AI.ObjRelationship(useItem.abstractPhysicalObject).intensity;
        if (AI.ObjRelationship(useItem.abstractPhysicalObject).type == ObjectRelationship.Type.PlaysWith)
        {
            oldItemAppeal *= 0.75f;
        }
        if (AI.ObjRelationship(newItem.abstractPhysicalObject).type == ObjectRelationship.Type.PlaysWith)
        {
            newItemAppeal *= 0.75f;
        }
        if (!GrabbingItem(useItem))
        {
            oldItemAppeal *= 0.2f + (0.8f * AI.VisualScore(MainChunkOfObject(useItem).pos, 0));
            if (Behavior == Flee)
            {
                oldItemAppeal *= AI.VisualScore(MainChunkOfObject(useItem).pos, 0);
            }
        }
        if (!GrabbingItem(newItem))
        {
            newItemAppeal *= 0.2f + (0.8f * AI.VisualScore(MainChunkOfObject(newItem).pos, 0));
            if (Behavior == Flee)
            {
                newItemAppeal *= AI.VisualScore(MainChunkOfObject(newItem).pos, 0);
            }
        }

        if (newItemAppeal > oldItemAppeal)
        {
            return true;
        }

        return false;
    }
    public virtual BodyChunk MainChunkOfObject(PhysicalObject obj)
    {
        if (obj is Creature ctr)
        {
            return ctr.mainBodyChunk;
        }
        return obj.bodyChunks.Length < 3 ?
            obj.firstChunk :
            obj.bodyChunks[obj.bodyChunks.Length / 2];
    }


    //-----------------------------------------
    // Abilities

    public virtual void CamouflageBehavior()
    {
        if (Behavior == Rush)
        {
            GlowState.rushPreyCounter++;
            if (GlowState.rushPreyCounter > 24)
            {
                if (currentPrey is not null && currentPrey is Creature ctr && AI.VisualContact(ctr.mainBodyChunk.pos))
                {
                    body.vel += Custom.DirVec(body.pos, ctr.mainBodyChunk.pos);
                }
            }
            else if (GlowState.rushPreyCounter == 24)
            {
                room.PlaySound(SoundID.Drop_Bug_Voice, DangerPos, 1, 2.5f - GlowState.ivars.Fatness);
                GlowState.stateTimeLimit = 800;
            }
            else if (GlowState.rushPreyCounter == 6)
            {
                RushAlert(); // ACTUALLY alerts nearby Luminescipedes.
                // "Why is there a 2-frame delay between the visual alert and the functional one?" Uuuuh, good question.
                // This makes it so all Lumins that are spread out in an area don't go alert at EXACTLY the same time.
                // It creates a cool "wave" of them all going "GO TIME, BABY".
            }
            else if (GlowState.rushPreyCounter == 4) // "Alerting" nearby Luminescipedes
            {
                room.PlaySound(SoundID.Snail_Warning_Click, DangerPos, 2.4f, 1.5f);
                room.AddObject(new LuminBlink(DangerPos, DangerPos, default, 1, baseColor, glowColor));
            }
        }
        else if (Behavior != Hide)
        {
            if (GlowState.rushPreyCounter > 0)
            {
                GlowState.rushPreyCounter = 0;
            }

            if (WantToHide && bloodlust < 0.2f && denMovement == 0 && fearSource is null)
            {
                if (CamoFac < 1)
                {
                    GlowState.darknessCounter = Mathf.Min(320, GlowState.darknessCounter + (bloodlust == 0 ? 2 : 1));
                }
                else if (Behavior != Hide)
                {
                    GlowState.ChangeBehavior(Hide, 1);
                }
            }
            else if (denMovement != 0 || fearSource is not null)
            {
                if (CamoFac > 0 && (Random.value < 0.2f * bloodlust || denMovement != 0 || fearSource is not null))
                {
                    GlowState.darknessCounter--;
                }
            }

        }
        else
        {
            if (CamoFac > 0 && (FleeLevel == 1 || (FleeLevel == 0 && Random.value < 0.5f)))
            {
                GlowState.darknessCounter--;
            }
            else if (CamoFac < 1 && Random.value < 0.05f)
            {
                GlowState.darknessCounter++;
            }

            if (CamoFac == 0 && Random.value < 0.1f)
            {
                GlowState.ChangeBehavior(Idle, 1);
            }

            if (currentPrey is not null && currentPrey is Creature ctr && bloodlust > 0 && ((Custom.DistLess(ctr.mainBodyChunk.pos, DangerPos, 200) && AI.VisualContact(ctr.mainBodyChunk.pos)) || FleeLevel > -1))
            {
                GlowState.rushPreyCounter++;
                if ((GlowState.rushPreyCounter > 40 && (Random.value < Mathf.InverseLerp(0, 80, GlowState.rushPreyCounter) / 500f) || bloodlust >= 1) || GlowState.rushPreyCounter == 80)
                {
                    RushPrey(false);
                }
            }
            else if (GlowState.rushPreyCounter > 0)
            {
                GlowState.rushPreyCounter--;
            }
        }
    }
    public virtual void RushPrey(bool angery)
    {
        if (angery)
        {
            GlowState.ChangeBehavior(Aggravated, 2);
            GlowState.stateTimeLimit = Random.Range(640, 961);
            AI.MovementSpeed = 1.25f;
        }
        else
        {
            GlowState.ChangeBehavior(Rush, 1);
            GlowState.darknessCounter = 0;
            GlowState.rushPreyCounter = 0;
            AI.MovementSpeed = 0;
        }
        bloodlust = 3;
        fearSource = null;
    }
    public virtual void RushAlert()
    {
        if (room?.abstractRoom is null)
        {
            return;
        }
        foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is not LuminCreature otherLmn || otherLmn == this || !AI.VisualContact(otherLmn.DangerPos) || otherLmn.Behavior != Hide)
            {
                continue;
            }
            int AlertRange = Role != Hunter ? 250 : (Dominant ? 500 : 375);
            if (otherLmn.Role == Guardian && otherLmn.Dominant)
            {
                AlertRange *= 2;
            }
            if (!Custom.DistLess(DangerPos, otherLmn.DangerPos, AlertRange))
            {
                continue;
            }
            if (currentPrey is not null && otherLmn.currentPrey != currentPrey)
            {
                otherLmn.currentPrey = currentPrey;
            }
            otherLmn.RushPrey(false);
        }
    }

    public virtual void Lunge()
    {
        if (!AI.inAccessibleTerrain)
        {
            lungeTimer = 0;
            return;
        }

        lungeTimer++;
        if (lungeTimer < 20)
        {
            body.vel -= direction * Mathf.InverseLerp(20, 0, lungeTimer) / 2f;
        }
        else if (lungeTimer == 20)
        {
            room.PlaySound(SoundID.Drop_Bug_Voice, DangerPos, 1, 2.5f - GlowState.ivars.Fatness);
            body.vel += direction * 16f;
            lunging = true;
        }
        else if (lungeTimer > 30)
        {
            lungeTimer = 0;
        }
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (Consious)
        {
            if (lunging && otherObject is Creature victim && (victim.State is null || victim.State is not GlowSpiderState))
            {
                lunging = false;
                room.PlaySound(SoundID.Big_Spider_Slash_Creature, body.pos, 0.9f, 2.5f - GlowState.ivars.Size);
                float DMG = victim.State is not null && victim.State is GlowSpiderState ? 0.25f : 0.5f;
                victim.Violence(body, body.vel / 2f, victim.bodyChunks[otherChunk], null, DamageType.Bite, DMG, 60);
                body.vel = Vector2.Lerp(body.vel.normalized, Custom.DirVec(body.pos, victim.bodyChunks[otherChunk].pos), 0.5f) * body.vel.magnitude;
                body.vel *= -1f;
            }
            if (!safariControlled)
            {
                if (otherObject is Creature ctr)
                {
                    if ((currentPrey is not null && currentPrey == ctr) ||
                    AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats ||
                    AI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Attacks)
                    {
                        ChangeBloodlust(Mathf.Lerp(1 / 40f, 1 / 20f, AI.DynamicRelationship(ctr.abstractCreature).intensity));
                        if (Behavior == Hide)
                        {
                            RushPrey(false);
                        }
                    }
                }
            }
        }
        base.Collide(otherObject, myChunk, otherChunk);
    }

    public virtual void Flashbomb()
    {
        if (!Consious)
        {
            return;
        }
        flashbombTimer++;
        if (flashbombTimer == 40)
        {
            room.AddObject(new LuminFlash(room, body, 160, 40, Color.Lerp(baseColor, glowColor, 0.5f), Random.Range(1.4f, 1.8f), false));
            float angle = Custom.VecToDeg(direction);
            for (int f = 0; f < 4; f++)
            {
                room.AddObject(new LuminBlink(body.pos, body.pos + 50f * Custom.DegToVec(angle + (90f * f)), default, 4, glowColor, baseColor));
            }
        }
        if (flashbombTimer >= 40)
        {
            GlowState.juice = Mathf.Lerp(GlowState.juice, 0, 0.05f);
        }
        if (flashbombTimer > 80)
        {
            flashbombTimer = 0;
        }
    }


    //-----------------------------------------
    // Visual Stuff

    public virtual void GlowUpdate(bool eu)
    {
        if (!Consious)
        {
            if (body.contactPoint.y < 0)
            {
                body.vel.x *= 0.95f;
            }
            if (Juice > 0)
            {
                GlowState.juice = Mathf.Max(0, Juice - (0.005f * Mathf.Lerp(1, 0.1f, LightExposure)));
                if (eu && Random.value < Juice)
                {
                    EmitSparks(1, Custom.RNV() * Random.Range(3f, 7f), 40f);
                }
            }
            if (dead)
            {
                if (flock is not null || fearSource is not null || flicker != 0 || bloodlust != 0 || losingInterestInGrasp != 0)
                {
                    Reset();
                }
                return;
            }
            else
            {
                flickeringFac = 1f;
                flickerDuration = Mathf.Lerp(10f, 30f, Random.value);
                if (Random.value < 0.1f)
                {
                    flicker = Mathf.Max(flicker, Random.value);
                }
            }
        }

        if (Behavior == Overloaded)
        {
            if (Juice > 0)
            {
                GlowState.juice = Mathf.Max(0, Juice - 0.025f);
            }
            if (!Stunned)
            {
                RushPrey(true);
            }
        }
        else
        {
            if (Juice > 1.25f)
            {
                Overload();
            }

            if (Consious && Juice < 1 && flashbombTimer < 40)
            {
                GlowState.juice = Mathf.Min(1, Juice + (0.0025f * Mathf.Lerp(0.1f, 1, LightExposure))); // Passively replenishes Juice
                if (Juice == 1)     // This activates when a Lumin's Juice maxes out on its own.
                {
                    room.PlaySound(SoundID.Snail_Warning_Click, DangerPos, Mathf.Lerp(blinkPitch, 1, 0.5f) * 2f, blinkPitch);
                    blinkPitch = 0;
                    flickeringFac = (Random.value < 0.5f) ? 0f : 1f;
                    flickerDuration = Mathf.Lerp(30f, 220f, Random.value);
                    room.AddObject(new LuminBlink(DangerPos, DangerPos, default, 1, baseColor, glowColor));
                    EmitSparks(20, Custom.RNV() * Random.Range(3f, 7f), 40f);
                }
            }
            else
            {
                if (Juice > 1)
                {
                    GlowState.juice = Mathf.Max(1, Juice - 0.0025f);
                }

            }

            if (flickeringFac > 0f)
            {
                flickeringFac = Mathf.Max(0, flickeringFac - 1f / flickerDuration);
                if (Random.value < 1f / 15f && Random.value < flickeringFac)
                {
                    flicker = Mathf.Pow(Random.value, 1f - flickeringFac);
                    room.PlaySound(SoundID.Mouse_Light_Flicker, DangerPos, flicker, 1f + (0.5f - flicker));
                }
            }
            else if (!dead && Random.value < (Behavior == Hide ? 0.0001f : 0.0035f))
            {
                flickeringFac = Random.value;
                flickerDuration = Mathf.Lerp(30f, 120f, Random.value);
            }
            if (flicker > 0f)
            {
                flicker = Mathf.Max(0, flicker - 1 / 15f);
                if (Random.value < flicker / 3f)
                {
                    EmitSparks(1, Custom.RNV() * Random.Range(3f, 7f), 40f);
                }
            }
            else if (Behavior != Hide && Random.value < 0.02f * Juice * (1 - CamoFac))
            {
                int sparks = Random.Range(4, 8);
                if (CamoFac > 0)
                {
                    sparks = Mathf.CeilToInt(sparks * CamoFac);
                }
                EmitSparks(sparks, Custom.RNV() * Random.Range(3f, 7f), 40f);
            }

            if (HP < 0.5f)
            {
                if (Random.value < (1f - GlowState.ClampedHealth) / 50f)
                {
                    int stun = (int)(Mathf.Lerp(15, 0, GlowState.ClampedHealth * 2) * Random.Range(0.5f, 1.5f));
                    Stun(stun);
                }
            }

            if (Consious && Juice >= 1 && HP > 0 && HP < 1)
            {
                GlowState.health = Mathf.Min(1, HP + 0.001f); // Regens 4% HP every second
            }

        }

    }
    public virtual void EmitSparks(int sparks, Vector2 vel, float sparkLife)
    {
        for (int i = 0; i < sparks; i++)
        {
            room.AddObject(new LuminSpark(GlowState.ivars.SparkType, DangerPos, vel, sparkLife, MainBodyColor, OutlineColor));
        }
    }

    public override Color ShortCutColor()
    {
        return MainBodyColor;
    }
    public virtual float LuminLightSourceExposure(Vector2 pos)
    {
        float exp = 0f;
        for (int i = 0; i < room.lightSources.Count; i++)
        {
            if ((room.lightSources[i].tiedToObject is null || room.lightSources[i].tiedToObject is not LuminCreature) && Custom.DistLess(pos, room.lightSources[i].Pos, room.lightSources[i].Rad))
            {
                exp += Custom.SCurve(Mathf.InverseLerp(room.lightSources[i].Rad, 0f, Custom.Dist(pos, room.lightSources[i].Pos)), 0.5f) * room.lightSources[i].Lightness;
            }
        }
        return Mathf.Clamp(exp, 0, 1);
    }


    //-----------------------------------------
    // Idle Crawl

    public virtual float ScoreOfPath(List<MovementConnection> testPath, int testPathCount)
    {
        if (testPathCount == 0)
        {
            return float.MinValue;
        }
        float tileScore = TileScore(testPath[testPathCount - 1].DestTile);
        for (int i = 0; i < pathCount; i++)
        {
            if (path[i] == lastFollowingConnection)
            {
                tileScore -= 1000f;
            }
        }
        return tileScore;
    }
    public virtual float TileScore(IntVector2 tile)
    {
        float TileScore = 0f;
        bool ReturningPrey = GrabbingAnything && Behavior == ReturnPrey;
        if (closestFearChunk is not null)
        {
            TileScore += Custom.Dist(room.MiddleOfTile(tile), closestFearChunk.pos);
        }
        if ((denMovement != 0 || ReturningPrey) && AI.denFinder.denPosition.HasValue && AI.denFinder.denPosition.Value.room == room.abstractRoom.index)
        {
            int distanceToExit = room.aimap.CreatureSpecificAImap(Template.preBakedPathingAncestor).GetDistanceToExit(tile.x, tile.y, room.abstractRoom.CommonToCreatureSpecificNodeIndex(AI.denFinder.denPosition.Value.abstractNode, Template.preBakedPathingAncestor));
            TileScore -= (distanceToExit == -1) ? 100f : distanceToExit * denMovement * (ReturningPrey ? 10000f : 1f);
        }

        for (int i = 0; i < 5; i++)
        {
            if (room.GetTile(tile + Custom.fourDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
            {
                return float.MinValue;
            }
        }
        if (flock is not null)
        {
            for (int l = 0; l < flock.lumins.Count; l++)
            {
                if (flock.lumins[l].GlowState.ivars.dominance > GlowState.ivars.dominance && flock.lumins[l].abstractCreature.pos.Tile == tile)
                {
                    TileScore -= 10f;
                }
            }
        }
        TileScore += room.aimap.getAItile(tile).visibility / 800f;
        if (room.aimap.getAItile(tile).narrowSpace)
        {
            TileScore -= 0.01f;
        }
        TileScore -= room.aimap.getAItile(tile).terrainProximity * 0.01f;
        if (lastShortCut is not null)
        {
            TileScore -= 10f / lastShortCut.StartTile.FloatDist(tile);
            TileScore -= 10f / lastShortCut.DestTile.FloatDist(tile);
        }
        if (bloodlust > 0f && flock is not null && flock.lumins.Count > 0)
        {
            for (int k = 0; k < 10f * bloodlust; k++)
            {
                LuminCreature lmn = flock.lumins[Random.Range(0, flock.lumins.Count)];
                TileScore -=
                    (lmn == this || !Custom.DistLess(DangerPos, lmn.body.pos, 200f)) ?
                    200f * bloodlust : Custom.Dist(DangerPos, lmn.body.pos) * bloodlust;
            }
        }

        if (currentPrey is not null && !ReturningPrey)
        {
            TileScore -= Custom.Dist(DangerPos, Vector2.Lerp(currentPrey.firstChunk.pos, currentPrey.bodyChunks[currentPrey.bodyChunks.Length - 1].pos, 0.5f)) * bloodlust * 4f;
        }
        return TileScore;
    }
    public virtual int CreateRandomPath(ref List<MovementConnection> pth)
    {
        WorldCoordinate worldCoordinate = abstractCreature.pos;
        if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, Template.preBakedPathingAncestor))
        {
            for (int i = 0; i < 4; i++)
            {
                if (room.aimap.TileAccessibleToCreature(worldCoordinate.Tile + Custom.fourDirections[i], Template.preBakedPathingAncestor) && room.GetTile(worldCoordinate.Tile + Custom.fourDirections[i]).Terrain != Room.Tile.TerrainType.Slope)
                {
                    worldCoordinate.Tile += Custom.fourDirections[i];
                    break;
                }
            }
        }
        if (!room.aimap.TileAccessibleToCreature(worldCoordinate.Tile, Template.preBakedPathingAncestor))
        {
            return 0;
        }
        WorldCoordinate worldCoordinate2 = abstractCreature.pos;
        int num = 0;
        for (int j = 0; j < Random.Range(5, 16); j++)
        {
            AItile aItile = room.aimap.getAItile(worldCoordinate);
            int index = Random.Range(0, aItile.outgoingPaths.Count);
            if (!room.aimap.IsConnectionAllowedForCreature(aItile.outgoingPaths[index], Template.preBakedPathingAncestor) || lastShortCut == aItile.outgoingPaths[index] || !(worldCoordinate2 != aItile.outgoingPaths[index].destinationCoord))
            {
                continue;
            }
            bool flag = true;
            for (int k = 0; k < num; k++)
            {
                if (pth[k].startCoord == aItile.outgoingPaths[index].destinationCoord || pth[k].destinationCoord == aItile.outgoingPaths[index].destinationCoord)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                worldCoordinate2 = worldCoordinate;
                if (pth.Count <= num)
                {
                    pth.Add(aItile.outgoingPaths[index]);
                }
                else
                {
                    pth[num] = aItile.outgoingPaths[index];
                }
                num++;
                worldCoordinate = aItile.outgoingPaths[index].destinationCoord;
            }
        }
        return num;
    }
    public virtual void IdleCrawl()
    {
        if (denMovement == 0 && fearSource is null)
        {
            idleCounter += Mathf.Max(1, bloodlust);
            if (!idle && idleCounter > 10)
            {
                idle = true;
            }
        }
        else if (Random.value <= 0.15 && (denMovement != 0 || fearSource is not null))
        {
            idleCounter = 0;
            idle = false;
        }

        if (idle)
        {
            if (followingConnection is not null)
            {
                IdleMove(followingConnection);
                if (room.GetTilePosition(DangerPos) == followingConnection.DestTile)
                {
                    followingConnection = null;
                }
            }
            else if (Random.value < 1f / 12f)
            {
                AItile aItile = room.aimap.getAItile(DangerPos);
                MovementConnection movementConnection = aItile.outgoingPaths[Random.Range(0, aItile.outgoingPaths.Count)];
                if (movementConnection.type != MovementConnection.MovementType.DropToFloor && room.aimap.IsConnectionAllowedForCreature(movementConnection, Template.preBakedPathingAncestor))
                {
                    followingConnection = movementConnection;
                }
            }
            return;
        }

        if (bloodlust > 0 || denMovement != 0 || fearSource is not null)
        {
            scratchPathCount = CreateRandomPath(ref scratchPath);
            if (ScoreOfPath(scratchPath, scratchPathCount) > ScoreOfPath(path, pathCount))
            {
                List<MovementConnection> oldPath = path;
                int oldPathCount = pathCount;
                path = scratchPath;
                pathCount = scratchPathCount;
                scratchPath = oldPath;
                scratchPathCount = oldPathCount;
            }
        }
        if (followingConnection is not null && followingConnection.type != 0)
        {
            if (lastFollowingConnection != followingConnection)
            {
                footingTimer = 20;
            }
            if (followingConnection is not null)
            {
                lastFollowingConnection = followingConnection;
            }
            IdleMove(followingConnection);
            if (room.GetTilePosition(DangerPos) != followingConnection.DestTile)
            {
                return;
            }
        }
        else if (followingConnection is not null)
        {
            lastFollowingConnection = followingConnection;
        }
        if (pathCount > 0)
        {
            followingConnection = null;
            for (int p = pathCount - 1; p >= 0; p--)
            {
                if (abstractCreature.pos.Tile == path[p].StartTile)
                {
                    followingConnection = path[p];
                    break;
                }
            }
            if (followingConnection is null)
            {
                pathCount = 0;
            }
        }
        if (followingConnection is null)
        {
            return;
        }
        if (followingConnection.type == MovementConnection.MovementType.Standard || followingConnection.type == MovementConnection.MovementType.DropToFloor)
        {
            IdleMove(followingConnection);
        }
        else if (shortcutDelay == 0 && (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation))
        {
            enteringShortCut = followingConnection.StartTile;
            if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                NPCTransportationDestination = followingConnection.destinationCoord;
            }
            lastShortCut = followingConnection;
            followingConnection = null;
        }
        return;
    }
    public virtual void IdleMove(MovementConnection con)
    {
        Vector2 dest = room.MiddleOfTile(con.DestTile);
        Vector2 addedVel = (Custom.DirVec(DangerPos, dest) * (GlowState.ivars.Size + 0.2f) * Mathf.Lerp(1.5f, 3, HP) * AI.MovementSpeed) + (Custom.DegToVec(Random.value * 360f) * 2f);

        if (addedVel == default)
        {
            return;
        }

        body.vel += addedVel;
    }


    //-----------------------------------------
    // Miscellaneous
    public void ThrowByPlayer() 
    {
    }
    public void BitByPlayer(Grasp grasp, bool eu) 
    {
        bites--;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            bodyChunks[b].rad *= 0.85f;
            bodyChunks[b].mass *= 0.85f;
        }
        if (State is HealthState HS)
        {
            HS.health -= 0.35f;
            if (HS.health <= 0) Die();
        }
        room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, DangerPos);
        body.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1)
        {
            Player eater = grasp.grabber as Player;
            eater.ObjectEaten(this);
            if (!eater.isNPC)
            {
                if (room.game.session is not null &&
                    room.game.session is StoryGameSession SGS)
                {
                    SGS.saveState.theGlow = true;
                }
            }
            else
            {
                (eater.State as PlayerNPCState).Glowing = true;
            }
            eater.glowing = true;
            grasp.Release();
            Destroy();
        }
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) 
    {
        if (firstContact && speed > 15)
        {
            for (int s = 0; s < Mathf.Lerp(0, 20, Juice); s++)
            {
                Vector2 vel = -new Vector2(
                    mainBodyChunk.vel.x * Random.Range(0.6f, 1.4f),
                    mainBodyChunk.vel.y * Random.Range(0.6f, 1.4f)) + Custom.DegToVec(360f * Random.value) * 7f * Random.value;

                EmitSparks(1, vel, 40f);
            }
        }
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }
    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks) 
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        Vector2 pipeDirection = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        shortcutDelay = 30;
        body.HardSetPosition(newRoom.MiddleOfTile(pos) - pipeDirection * 5f);
        body.vel = pipeDirection * 7.5f;
        if (graphicsModule is not null)
        {
            graphicsModule.Reset();
        }
    }
    public static bool WantToHideInDen(AbstractCreature absLmn) 
    {
        if (absLmn?.Room?.realizedRoom is null)
        {
            return false;
        }
        if (absLmn.state.dead)
        {
            return true;
        }
        if (CWT.AbsCtrData.TryGetValue(absLmn, out AbsCtrInfo aI))
        {
            if (absLmn.world.rainCycle.TimeUntilRain < (absLmn.world.game.session is not null && absLmn.world.game.IsStorySession ? 60 : 15) * 40 && !absLmn.nightCreature && !absLmn.ignoreCycle && !aI.LateBlizzardRoamer)
            {
                return true;
            }
            if (absLmn.preCycle && (Weather.FogPrecycle || absLmn.world.rainCycle.maxPreTimer <= 0))
            {
                return true;
            }
            if (aI.FogRoamer && (!Weather.FogPrecycle || absLmn.world.rainCycle.maxPreTimer <= 0))
            {
                return true;
            }
            if (aI.ErraticWindRoamer && !Weather.ErraticWindCycle)
            {
                return true;
            }
            if (aI.ErraticWindAvoider && Weather.ErraticWindCycle)
            {
                return true;
            }
        }
        if (absLmn.world?.game?.session is not null && absLmn.world.game.IsArenaSession && absLmn.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
        {
            return false;
        }
        if (absLmn.state is GlowSpiderState gs && gs.role != Guardian && gs.health < (gs.role == Forager ? 0.1f : 0.05f))
        {
            return true;
        }
        return false;
    }


}

//-----------------------------------------

public abstract class LuminMass 
{
    public List<LuminCreature> lumins;
    public bool lastEu;
    public Room room;
    public Color color = Custom.HSL2RGB(Random.value, 1f, 0.5f);

    public virtual LuminCreature FirstLumin
    {
        get
        {
            if (lumins.Count == 0)
            {
                return null;
            }
            return lumins[0];
        }
    }

    public LuminMass(LuminCreature firstLumin, Room room)
    {
        this.room = room;
        lumins = new List<LuminCreature> { firstLumin };
    }

    public virtual void Update(bool eu)
    {
        for (int l = lumins.Count - 1; l >= 0; l--)
        {
            if (lumins[l].dead ||
                lumins[l].room != room)
            {
                RemoveLmnAt(l);
            }
        }
    }
    public bool ShouldIUpdate(bool eu)
    {
        if (eu == lastEu)
        {
            return false;
        }
        lastEu = eu;
        return true;
    }

    public void AddLmn(LuminCreature lmn)
    {
        if (lumins.IndexOf(lmn) == -1)
        {
            lumins.Add(lmn);
        }
        if (this is LuminFlock)
        {
            lmn.flock = this as LuminFlock;
            lmn.flock.lumins = new();
        }
    }
    public void RemoveLmn(LuminCreature lmn)
    {
        for (int i = 0; i < lumins.Count; i++)
        {
            if (lumins[i] == lmn)
            {
                RemoveLmnAt(i);
                break;
            }
        }
    }
    private void RemoveLmnAt(int i)
    {
        if (this is LuminFlock &&
            lumins[i].flock == this as LuminFlock)
        {
            lumins[i].flock = null;
        }
        lumins.RemoveAt(i);
    }
    public void Merge(LuminMass otherFlock)
    {
        if (otherFlock == this)
        {
            return;
        }
        for (int i = 0; i < otherFlock.lumins.Count; i++)
        {
            if (lumins.IndexOf(otherFlock.lumins[i]) == -1)
            {
                lumins.Add(otherFlock.lumins[i]);
                if (this is LuminFlock)
                {
                    otherFlock.lumins[i].flock = this as LuminFlock;
                }
            }
        }
        otherFlock.lumins.Clear();
    }
}
public class LuminFlock : LuminMass
{
    public LuminFlock(LuminCreature firstLumin, Room room) : base(firstLumin, room)
    {
    }

    public override void Update(bool eu)
    {
        if (lumins is null)
        {
            lumins = new();
        }
        if (!ShouldIUpdate(eu))
        {
            return;
        }
        base.Update(eu);
        if (room.abstractRoom.creatures.Count == 0)
        {
            return;
        }
        AbstractCreature absCtr = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
        if (absCtr.realizedCreature is not null && absCtr.realizedCreature is LuminCreature lmn && lmn.flock is not null && lmn.flock != this && lmn.flock.FirstLumin is not null)
        {
            if (lumins.Count >= lmn.flock.lumins.Count)
            {
                Merge(lmn.flock);
            }
            else
            {
                lmn.flock.Merge(this);
            }
        }
    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------