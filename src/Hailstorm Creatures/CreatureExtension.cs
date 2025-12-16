namespace Hailstorm;

public static class CreatureExtension
{
    public static bool IsBurning(this Creature ctr)
    {
        if (ctr?.abstractCreature is null)
        {
            throw new ArgumentNullException(nameof(ctr.abstractCreature));
        }

        return ctr.Burns() > 0;
    }
    public static int Burns(this Creature ctr)
    {
        if (ctr?.abstractCreature is null)
        {
            throw new ArgumentNullException(nameof(ctr.abstractCreature));
        }

        if (!CWT.AbsCtrData.TryGetValue(ctr.abstractCreature, out CWT.AbsCtrInfo acI) || acI.debuffs is null)
        {
            return 0;
        }

        int burns = 0;
        for (int d = acI.debuffs.Count - 1; d > 0; d--)
        {
            if (acI.debuffs[d] is Burn)
            {
                burns++;
            }
        }

        return burns;
    }

}
