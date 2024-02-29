namespace Hailstorm;

public class ColdLizAI : LizardAI
{
    public ColdLizard liz;
    public ColdLizState ColdState => liz.State as ColdLizState;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public int PackUpdateTimer;
    public float PackPower;
    public bool NearAFreezer;

    public ColdLizAI(AbstractCreature absLiz, World world) : base(absLiz, world)
    {
        liz = absLiz.realizedCreature as ColdLizard;
        if (liz.IcyBlue)
        {
            pathFinder.stepsPerFrame = 20;
            preyTracker.sureToGetPreyDistance = 5;
            preyTracker.giveUpOnUnreachablePrey = 1100;
            stuckTracker.minStuckCounter = 40;
            stuckTracker.maxStuckCounter = 80;
        }
        else if (liz.Freezer)
        {
            pathFinder.stepsPerFrame = 20;
            preyTracker.sureToGetPreyDistance = 10;
            preyTracker.giveUpOnUnreachablePrey = 1800;
            stuckTracker.minStuckCounter = 20;
            stuckTracker.maxStuckCounter = 40;
        }
    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        UpdatePack();

        if (liz.Freezer)
        {
            noiseTracker.hearingSkill = 1.5f;
            if (Random.value < 0.01f)
            {
                creature.abstractAI.AbstractBehavior(1);
            }
        }

        if (Weather.ErraticWindCycle &&
            Weather.ExtremeWindIntervals[Weather.WindInterval] &&
            liz.room.blizzardGraphics is not null)
        {
            float exposure = (
                liz.room.blizzardGraphics.GetBlizzardPixel((int)liz.bodyChunks[0].pos.x, (int)liz.bodyChunks[0].pos.y).g +
                liz.room.blizzardGraphics.GetBlizzardPixel((int)liz.bodyChunks[liz.bodyChunks.Length - 1].pos.x, (int)liz.bodyChunks[liz.bodyChunks.Length - 1].pos.y).g) / 2f;

            if (exposure >= 0.5f)
            {
                runSpeed = Mathf.Lerp(runSpeed, 0.25f, exposure / 30f);
            }
        }
    }
    public virtual void UpdatePack()
    {
        PackUpdateTimer++;

        if (PackUpdateTimer > 80)
        {
            PackUpdateTimer = 0;
            PackPower = 0;
            NearAFreezer = false;
            foreach (AbstractCreature absCtr in liz.room.abstractRoom.creatures)
            {
                if (DynamicRelationship(absCtr).type == CreatureTemplate.Relationship.Type.Pack &&
                    absCtr.realizedCreature is not null &&
                    Custom.DistLess(liz.DangerPos, absCtr.realizedCreature.DangerPos, 1250))
                {
                    if (PackPower < 1)
                    {
                        PackPower +=
                            absCtr.creatureTemplate.type == HSEnums.CreatureType.FreezerLizard ? 0.2f :
                            absCtr.creatureTemplate.type == HSEnums.CreatureType.IcyBlueLizard ? 0.1f : 0.05f;
                    }

                    if (absCtr.creatureTemplate.type == HSEnums.CreatureType.FreezerLizard && !NearAFreezer)
                    {
                        NearAFreezer = true;
                        if (creature.abstractAI.followCreature is null && preyTracker.MostAttractivePrey is null)
                        {
                            creature.abstractAI.followCreature = absCtr;
                        }
                    }
                }
            }
            if (PackPower > 1)
            {
                PackPower = 1;
            }
        }
    }

}