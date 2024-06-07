namespace Hailstorm;

public class CyanwingAI : CentipedeAI, IUseARelationshipTracker
{
    public Cyanwing cyn;
    public CyanwingState CyanState => cyn?.CyanState;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public CyanwingAI(AbstractCreature absCyn, World world) : base(absCyn, world)
    {
        cyn = creature.realizedCreature as Cyanwing;
    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        float weight = (preyTracker.MostAttractivePrey is not null) ? 0 : 0.1f;
        utilityComparer.GetUtilityTracker(injuryTracker).weight = weight;

        base.Update();

        if (creature.abstractAI?.followCreature is not null)
        {
            creature.abstractAI.AbstractBehavior(1);
        }

    }

    // - - - - - - - - - - - - - - - - - - - -

    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dynamRelat)
    {
        Creature ctr = dynamRelat.trackerRep.representedCreature.realizedCreature;
        CreatureTemplate.Relationship defaultRelation = StaticRelationship(dynamRelat.trackerRep.representedCreature);

        if (defaultRelation.type == CreatureTemplate.Relationship.Type.Ignores)
        {
            return defaultRelation;
        }
        if (ctr is not null)
        {
            if (ctr.dead)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 0);
            }
            if (OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile) >= 1)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
            }

            if (defaultRelation.type == CreatureTemplate.Relationship.Type.Eats &&
                ctr.TotalMass < cyn.TotalMass)
            {
                if (creature.abstractAI?.followCreature is not null &&
                    creature.abstractAI.followCreature == ctr.abstractCreature)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Antagonizes, 1);
                }
                float intensity = Mathf.InverseLerp(0f, cyn.TotalMass, ctr.TotalMass * 1.2f);
                if (CyanState is not null)
                {
                    intensity *= 2 - CyanState.health;
                }
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, intensity * defaultRelation.intensity);
            }
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Afraid, 0.2f + (0.8f * Mathf.InverseLerp(cyn.TotalMass, cyn.TotalMass * 1.5f, ctr.TotalMass)));
        }
        return defaultRelation;
    }

}