namespace Hailstorm;

public class ChillipedeAI : CentipedeAI, IUseARelationshipTracker
{
    public Chillipede chl;

    public ChillipedeAI(AbstractCreature creature, World world) : base(creature, world)
    {
        chl = creature.realizedCreature as Chillipede;
    }
    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        return relationship.type == CreatureTemplate.Relationship.Type.Afraid ||
            relationship.type == CreatureTemplate.Relationship.Type.StayOutOfWay
            ? threatTracker
            : relationship.type == CreatureTemplate.Relationship.Type.Eats ||
            relationship.type == CreatureTemplate.Relationship.Type.Antagonizes
            ? preyTracker
            : (AIModule)null;
    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        float lastRun = run;
        base.Update();
        run = lastRun;

        TrackerShenanigans();

        AIModule aIModule = utilityComparer.HighestUtilityModule();
        currentUtility = utilityComparer.HighestUtility();
        if (aIModule is not null)
        {
            if (aIModule is ThreatTracker)
            {
                behavior = Behavior.Flee;
            }
            else if (aIModule is RainTracker)
            {
                behavior = Behavior.EscapeRain;
            }
            else if (aIModule is PreyTracker)
            {
                behavior = Behavior.Hunt;
            }
            else if (aIModule is NoiseTracker)
            {
                behavior = Behavior.InvestigateSound;
            }
            else if (aIModule is InjuryTracker)
            {
                behavior = Behavior.Injured;
            }
        }
        if (currentUtility < 0.1f)
        {
            behavior = Behavior.Idle;
        }

        ManageDestinationAndExcitement();
        if (behavior != Behavior.Idle)
        {
            run = 200f;
            return;
        }
        run -= 1f;
        if (run < Mathf.Lerp(-40f, -5f, excitement))
        {
            run = Mathf.Lerp(60f, 200f, excitement);
        }
    }
    public virtual void TrackerShenanigans()
    {
        if (noiseTracker is not null)
        {
            noiseTracker.hearingSkill = chl.moving ? 0.5f : 1f;
        }
        if (preyTracker.MostAttractivePrey is not null)
        {
            utilityComparer.GetUtilityTracker(preyTracker).weight = Mathf.InverseLerp(1000, 400, preyTracker.MostAttractivePrey.TicksSinceSeen);
        }
    }
    public virtual void ManageDestinationAndExcitement()
    {
        float excitementGoal = 0f;
        if (behavior == Behavior.Idle)
        {
            WorldCoordinate testPos = creature.pos + new IntVector2(Random.Range(-10, 11), Random.Range(-10, 11));
            if (Random.value < 0.01f)
            {
                testPos = new WorldCoordinate(creature.pos.room, Random.Range(0, chl.room.TileWidth), Random.Range(0, chl.room.TileHeight), -1);
            }

            if (IdleScore(testPos) > IdleScore(tempIdlePos))
            {
                tempIdlePos = testPos;
                idleCounter = 0;
            }
            else
            {
                idleCounter++;
                if (idleCounter > 1400 || chl.outsideLevel)
                {
                    idleCounter = 0;
                    forbiddenIdlePos = tempIdlePos;
                }
            }
            if (tempIdlePos != pathFinder.GetDestination &&
                IdleScore(tempIdlePos) > IdleScore(pathFinder.GetDestination) + 100f)
            {
                creature.abstractAI.SetDestination(tempIdlePos);
            }
        }
        else if (behavior == Behavior.Flee)
        {
            excitementGoal = 0.5f;
            if (threatTracker.mostThreateningCreature is not null &&
                DynamicRelationship(threatTracker.mostThreateningCreature).type == CreatureTemplate.Relationship.Type.Afraid)
            {
                excitementGoal = 1;
            }
            WorldCoordinate destination = threatTracker.FleeTo(creature.pos, 1, (int)(30 * excitementGoal), currentUtility + excitementGoal > 0.8f);
            creature.abstractAI.SetDestination(destination);
        }
        else if (behavior == Behavior.EscapeRain)
        {
            excitementGoal = 0.75f;
            if (denFinder.GetDenPosition().HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
            }
        }
        else if (behavior == Behavior.Injured)
        {
            excitementGoal = 1f;
            if (denFinder.GetDenPosition().HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
            }
        }
        else if (behavior == Behavior.Hunt)
        {
            excitementGoal = 0.25f + (0.75f * DynamicRelationship(preyTracker.MostAttractivePrey).intensity);
            creature.abstractAI.SetDestination(preyTracker.MostAttractivePrey.BestGuessForPosition());
        }
        else if (behavior == Behavior.InvestigateSound)
        {
            excitementGoal = 0.33f;
            creature.abstractAI.SetDestination(noiseTracker.ExaminePos);
        }
        excitement = Mathf.Lerp(excitement, excitementGoal, 0.05f);
    }

    public override float VisualScore(Vector2 lookAtPoint, float bonus)
    {
        Vector2 chunkBehindVisionhead;
        Vector2 currentVisionHead;
        if (chl.visionDirection)
        {
            chunkBehindVisionhead = centipede.bodyChunks[1].pos;
            currentVisionHead = chl.bodyChunks[0].pos;
        }
        else
        {
            chunkBehindVisionhead = chl.bodyChunks[centipede.bodyChunks.Length - 2].pos;
            currentVisionHead = centipede.bodyChunks[centipede.bodyChunks.Length - 1].pos;
        }
        float baseVisualScore = BaseVisualScore(lookAtPoint, bonus);
        Vector2 chunkPosDifference = chunkBehindVisionhead - currentVisionHead;
        Vector2 headDirection = chunkPosDifference.normalized;
        chunkPosDifference = currentVisionHead - lookAtPoint;
        return baseVisualScore - Mathf.InverseLerp(1, 0, Vector2.Dot(headDirection, chunkPosDifference.normalized) + (chl.moving ? -0.5f : 0.25f));
    }
    public virtual float BaseVisualScore(Vector2 lookAtPoint, float bonus)
    {
        try
        {
            if (!Custom.DistLess(chl.VisionPoint, lookAtPoint, creature.creatureTemplate.visualRadius * (1f + bonus)))
            {
                return 0f;
            }
            if (creature.Room.realizedRoom is null)
            {
                return 0f;
            }
            float pointVisibility = Mathf.InverseLerp(creature.creatureTemplate.visualRadius * (1f + bonus), 0, Vector2.Distance(chl.VisionPoint, lookAtPoint));
            if (creature.Room.realizedRoom.water)
            {
                if (creature.Room.realizedRoom.water && creature.Room.realizedRoom.GetTile(creature.realizedCreature.VisionPoint).DeepWater != creature.Room.realizedRoom.GetTile(lookAtPoint).DeepWater)
                {
                    pointVisibility -= 1f - creature.creatureTemplate.throughSurfaceVision;
                }
                if (creature.Room.realizedRoom.GetTile(creature.realizedCreature.VisionPoint).DeepWater || creature.Room.realizedRoom.GetTile(lookAtPoint).DeepWater)
                {
                    pointVisibility -= 1f - creature.creatureTemplate.waterVision;
                }
            }
            if (creature.Room.realizedRoom.aimap.getAItile(lookAtPoint).narrowSpace)
            {
                pointVisibility -= 0.5f;
            }
            for (int o = creature.Room.realizedRoom.visionObscurers.Count - 1; o >= 0; o--)
            {
                pointVisibility = creature.Room.realizedRoom.visionObscurers[o].VisionScore(creature.realizedCreature.VisionPoint, lookAtPoint, pointVisibility);
            }
            return pointVisibility;
        }
        catch (NullReferenceException)
        {
            return 0f;
        }
    }

    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship relat)
    {
        CreatureTemplate.Relationship result = StaticRelationship(relat.trackerRep.representedCreature);
        if (result.type == CreatureTemplate.Relationship.Type.Ignores)
        {
            return result;
        }
        if (relat.trackerRep.representedCreature.realizedCreature is not null)
        {
            Creature target = relat.trackerRep.representedCreature.realizedCreature;

            return target.dead
                ? new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 0)
                : result.type == CreatureTemplate.Relationship.Type.Eats && target.TotalMass < chl.TotalMass
                ? target.Hypothermia >= 2
                    ? new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Ignores, 0)
                    : new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats,
                        MassFac(target) * ColdFac(target) * Mathf.Clamp(result.intensity, 0, 1))
                : new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Afraid,
                    0.2f + (0.8f * Mathf.InverseLerp(chl.TotalMass, chl.TotalMass * 1.5f, target.TotalMass)));
        }
        return result;
    }
    public virtual float MassFac(Creature target)
    {
        return Mathf.InverseLerp(0, chl.TotalMass, target.TotalMass);
    }
    public virtual float ColdFac(Creature target)
    {
        float coldFac = Mathf.InverseLerp(2, 0, target.Hypothermia);
        return coldFac;
    }

}