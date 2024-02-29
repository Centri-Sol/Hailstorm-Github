namespace Hailstorm;

public class FreezerCrystalFisob : IceFisobTemplate
{
    public override Color IceColor => Custom.HSL2RGB(211 / 360f, 1, 0.8f);

    internal FreezerCrystalFisob() : base(HSEnums.AbstractObjectType.FreezerCrystal, HSEnums.SandboxUnlock.FreezerCrystal, HSEnums.SandboxUnlock.Freezer)
    {
        Icon = new SimpleIcon("Icon_Freezer_Crystal", IceColor);
    }

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return new FreezerCrystalPropoerties(forObject);
    }

    public class FreezerCrystalPropoerties : ItemProperties
    {
        public FreezerCrystalPropoerties(PhysicalObject forObject)
        {

        }

        public override void Throwable(Player player, ref bool throwable)
        {
            throwable = true;
        }
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
        public override void ScavCollectScore(Scavenger scav, ref int score)
        {
            score = 4;
        }
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType != ScavengerAI.ViolenceType.Lethal)
            {
                score = 0;
            }
            else
            {
                score = 3;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk ice &&
                        ice.FreezerCrystal)
                    {
                        score++;
                    }
                }
            }
        }
        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.NonLethal)
            {
                score = 0;
            }
            else if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
            {
                score = 4;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk ice &&
                        ice.FreezerCrystal)
                    {
                        score++;
                    }
                }
            }
        }
        public override void LethalWeapon(Scavenger scav, ref bool isLethal)
        {
            isLethal = true;
        }
    }

}