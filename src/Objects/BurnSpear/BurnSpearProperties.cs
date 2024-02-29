namespace Hailstorm;

public class BurnSpearProperties : ItemProperties
{
    public override void Throwable(Player player, ref bool throwable)
    {
        throwable = true;
    }
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.BigOneHand;
    }
    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = 6;
    }
    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
    {
        score = 4;
    }
    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
    {
        score = 2;
    }
    public override void LethalWeapon(Scavenger scav, ref bool isLethal)
    {
        isLethal = true;
    }
}