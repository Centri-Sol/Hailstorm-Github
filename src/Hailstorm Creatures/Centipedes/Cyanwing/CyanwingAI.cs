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
        Creature target = dynamRelat.trackerRep.representedCreature.realizedCreature;
        CreatureTemplate.Relationship defaultRelation = StaticRelationship(dynamRelat.trackerRep.representedCreature);

        if (defaultRelation.type == CreatureTemplate.Relationship.Type.Ignores)
        {
            return defaultRelation;
        }
        if (target is not null)
        {
            if (target.dead)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 0);
            }
            if (OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile) >= 1)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
            }

            float ElectricResistance = target.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
            if (target is Player)
            {
                ElectricResistance /= CustomTemplateInfo.DamageResistances.SlugcatDamageMultipliers(target as Player, Creature.DamageType.Electric);
            }
            else if (CentiHooks.IsIncanStory(cyn.room?.game))
            {
                ElectricResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(target.Template, Creature.DamageType.Electric, false);
            }

            if (defaultRelation.type == CreatureTemplate.Relationship.Type.Eats &&
                cyn.TotalMass > target.TotalMass * ElectricResistance)
            {
                if (creature.abstractAI?.followCreature is not null &&
                    creature.abstractAI.followCreature == target.abstractCreature)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Antagonizes, 1);
                }
                float intensity = Mathf.InverseLerp(0f, cyn.TotalMass, target.TotalMass * 1.2f);
                if (CyanState is not null)
                {
                    intensity *= 2 - CyanState.health;
                }
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, intensity * defaultRelation.intensity);
            }
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Afraid, 0.2f + (0.8f * Mathf.InverseLerp(cyn.TotalMass, cyn.TotalMass * 1.5f, target.TotalMass)));
        }
        return defaultRelation;
    }

}