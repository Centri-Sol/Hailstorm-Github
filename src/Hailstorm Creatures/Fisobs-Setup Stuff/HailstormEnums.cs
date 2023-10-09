using System.Diagnostics.CodeAnalysis;

namespace Hailstorm;

public static class HailstormEnums
{

    [AllowNull] public static CreatureTemplate.Type InfantAquapede = new("InfantAquapede", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID InfantAquapedeUnlock = new("InfantAquapede", true);

    [AllowNull] public static CreatureTemplate.Type IcyBlue = new("IcyBlueLizard", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID IcyBlueUnlock = new("IcyBlueLizard", true);

    [AllowNull] public static CreatureTemplate.Type Freezer = new("FreezerLizard", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID FreezerUnlock = new("FreezerLizard", true);

    [AllowNull] public static CreatureTemplate.Type PeachSpider = new("PeachSpider", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID PeachSpiderUnlock = new("PeachSpider", true);

    [AllowNull] public static CreatureTemplate.Type Cyanwing = new("Cyanwing", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID CyanwingUnlock = new("Cyanwing", true);

    [AllowNull] public static CreatureTemplate.Type GorditoGreenie = new("GorditoGreenieLizard", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID GorditoGreenieUnlock = new("GorditoGreenieLizard", true);

    //[AllowNull] public static CreatureTemplate.Type BezanBud = new("BezanBud", true);
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BezanBudUnlock = new("BezanBud", true);

    [AllowNull] public static CreatureTemplate.Type Luminescipede = new("Luminsecipede", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LuminsecipedeUnlock = new("Luminsecipede", true);

    //[AllowNull] public static CreatureTemplate.Type Strobelegs = new("Strobelegs", true);
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID StrobelegsUnlock = new("Strobelegs", true);

    [AllowNull] public static CreatureTemplate.Type Chillipede = new("Chillipede", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID ChillipedeUnlock = new("Chillipede", true);


    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType IceCrystal = new ("IceCrystal", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID IceCrystalUnlock = new("IceCrystal", true);

    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType BurnSpear = new("BurnSpear", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BurnSpearUnlock = new("BurnSpear", true);


    public static Creature.DamageType Cold = new("HailstormCold", true);
    public static Creature.DamageType Heat = new("HailstormHeat", true);


    public static void UnregisterValues()
    {

        if (InfantAquapede is not null)
        {
            InfantAquapede.Unregister();
            InfantAquapede = null;
        }
        if (InfantAquapedeUnlock is not null)
        {
            InfantAquapedeUnlock.Unregister();
            InfantAquapedeUnlock = null;
        }

        if (IcyBlue is not null)
        {
            IcyBlue.Unregister();
            IcyBlue = null;
        }
        if (IcyBlueUnlock is not null)
        {
            IcyBlueUnlock.Unregister();
            IcyBlueUnlock = null;
        }

        if (Freezer is not null)
        {
            Freezer.Unregister();
            Freezer = null;
        }
        if (FreezerUnlock is not null)
        {
            FreezerUnlock.Unregister();
            FreezerUnlock = null;
        }

        if (PeachSpider is not null)
        {
            PeachSpider.Unregister();
            PeachSpider = null;
        }
        if (PeachSpiderUnlock is not null)
        {
            PeachSpiderUnlock.Unregister();
            PeachSpiderUnlock = null;
        }

        if (Cyanwing is not null)
        {
            Cyanwing.Unregister();
            Cyanwing = null;
        }
        if (CyanwingUnlock is not null)
        {
            CyanwingUnlock.Unregister();
            CyanwingUnlock = null;
        }

        /*
        if (BezanBud is not null)
        {
            BezanBud.Unregister();
            BezanBud = null;
        }
        if (BezanBudUnlock is not null)
        {
            BezanBudUnlock.Unregister();
            BezanBudUnlock = null;
        }
        */

        if (GorditoGreenie is not null)
        {
            GorditoGreenie.Unregister();
            GorditoGreenie = null;
        }
        if (GorditoGreenieUnlock is not null)
        {
            GorditoGreenieUnlock.Unregister();
            GorditoGreenieUnlock = null;
        }

        if (Luminescipede is not null)
        {
            Luminescipede.Unregister();
            Luminescipede = null;
        }
        if (LuminsecipedeUnlock is not null)
        {
            LuminsecipedeUnlock.Unregister();
            LuminsecipedeUnlock = null;
        }
        /*
        if (Strobelegs is not null)
        {
            Strobelegs.Unregister();
            Strobelegs = null;
        }
        if (StrobelegsUnlock is not null)
        {
            StrobelegsUnlock.Unregister();
            StrobelegsUnlock = null;
        }
        */

        if (Chillipede is not null)
        {
            Chillipede.Unregister();
            Chillipede = null;
        }
        if (ChillipedeUnlock is not null)
        {
            ChillipedeUnlock.Unregister();
            ChillipedeUnlock = null;
        }



        if (IceCrystal is not null)
        {
            IceCrystal.Unregister();
            IceCrystal = null;
        }
        if (IceCrystalUnlock is not null)
        {
            IceCrystalUnlock.Unregister();
            IceCrystalUnlock = null;
        }

        if (BurnSpear is not null)
        {
            BurnSpear.Unregister();
            BurnSpear = null;
        }
        if (BurnSpearUnlock is not null)
        {
            BurnSpearUnlock.Unregister();
            BurnSpearUnlock = null;
        }
    }
}
