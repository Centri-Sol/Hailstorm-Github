using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hailstorm;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

public static class HailstormCreatures
{
    public readonly static List<CreatureTemplate.Type> Types = new()
    {
        InfantAquapede,
        SnowcuttleFemale,
        SnowcuttleMale,
        SnowcuttleLe,
        Raven,
        IcyBlue,
        Freezer,
        PeachSpider,
        Cyanwing,
        GorditoGreenie,
        //BezanBud,
        Luminescipede,
        //Strobelegs,
        Chillipede
    };


    [AllowNull] public static CreatureTemplate.Type InfantAquapede = new("InfantAquapede", true);
    [AllowNull] public static CreatureTemplate.Type SnowcuttleTemplate = new("SnowcuttleTemplate", true);
    [AllowNull] public static CreatureTemplate.Type SnowcuttleFemale = new("SnowcuttleFemale", true);
    [AllowNull] public static CreatureTemplate.Type SnowcuttleMale = new("SnowcuttleMale", true);
    [AllowNull] public static CreatureTemplate.Type SnowcuttleLe = new("SnowcuttleLe", true);
    [AllowNull] public static CreatureTemplate.Type Raven = new("Raven", true);
    [AllowNull] public static CreatureTemplate.Type IcyBlue = new("IcyBlueLizard", true);
    [AllowNull] public static CreatureTemplate.Type Freezer = new("FreezerLizard", true);
    [AllowNull] public static CreatureTemplate.Type PeachSpider = new("PeachSpider", true);
    [AllowNull] public static CreatureTemplate.Type Cyanwing = new("Cyanwing", true);
    [AllowNull] public static CreatureTemplate.Type GorditoGreenie = new("GorditoGreenieLizard", true);
    //[AllowNull] public static CreatureTemplate.Type BezanBud = new("BezanBud", true);
    [AllowNull] public static CreatureTemplate.Type Chillipede = new("Chillipede", true);
    [AllowNull] public static CreatureTemplate.Type Luminescipede = new("Luminescipede", true);
    //[AllowNull] public static CreatureTemplate.Type Strobelegs = new("Strobelegs", true);

    public static void UnregisterValues()
    {

        if (InfantAquapede is not null)
        {
            InfantAquapede.Unregister();
            InfantAquapede = null;
        }
        if (SnowcuttleFemale is not null)
        {
            SnowcuttleFemale.Unregister();
            SnowcuttleFemale = null;
        }
        if (SnowcuttleMale is not null)
        {
            SnowcuttleMale.Unregister();
            SnowcuttleMale = null;
        }
        if (SnowcuttleLe is not null)
        {
            SnowcuttleLe.Unregister();
            SnowcuttleLe = null;
        }
        /*
        if (Raven is not null)
        {
            Raven.Unregister();
            Raven = null;
        }
        */
        if (IcyBlue is not null)
        {
            IcyBlue.Unregister();
            IcyBlue = null;
        }
        if (Freezer is not null)
        {
            Freezer.Unregister();
            Freezer = null;
        }
        if (PeachSpider is not null)
        {
            PeachSpider.Unregister();
            PeachSpider = null;
        }
        if (Cyanwing is not null)
        {
            Cyanwing.Unregister();
            Cyanwing = null;
        }
        if (GorditoGreenie is not null)
        {
            GorditoGreenie.Unregister();
            GorditoGreenie = null;
        }
        /*
        if (BezanBud is not null)
        {
            BezanBud.Unregister();
            BezanBud = null;
        }
        */
        if (Chillipede is not null)
        {
            Chillipede.Unregister();
            Chillipede = null;
        }
        if (Luminescipede is not null)
        {
            Luminescipede.Unregister();
            Luminescipede = null;
        }
        /*
        if (Strobelegs is not null)
        {
            Strobelegs.Unregister();
            Strobelegs = null;
        }
        */

    }

}

//----------------------------------------------------------------------------------

public static class HailstormItems
{
    public readonly static List<AbstractPhysicalObject.AbstractObjectType> Types = new()
    {
        IceChunk,
        FreezerCrystal
    };


    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType IceChunk = new("IceChunk", true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType FreezerCrystal = new("FreezerCrystal", true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType BurnSpear = new("BurnSpear", true);


    public static void UnregisterValues()
    {
        if (IceChunk is not null)
        {
            IceChunk.Unregister();
            IceChunk = null;
        }
        if (FreezerCrystal is not null)
        {
            FreezerCrystal.Unregister();
            FreezerCrystal = null;
        }
        if (BurnSpear is not null)
        {
            BurnSpear.Unregister();
            BurnSpear = null;
        }
    }


}

//----------------------------------------------------------------------------------

public static class HailstormUnlocks
{

    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID InfantAquapede = new("InfantAquapede", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleFemale = new("SnowcuttleFemale", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleMale = new("SnowcuttleMale", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleLe = new("SnowcuttleLe", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Raven = new("Raven", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID IcyBlue = new("IcyBlueLizard", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Freezer = new("FreezerLizard", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID PeachSpider = new("PeachSpider", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Cyanwing = new("Cyanwing", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID GorditoGreenie = new("GorditoGreenieLizard", true);
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BezanBud = new("BezanBud", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Luminescipede = new("Luminescipede", true);
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Strobelegs = new("Strobelegs", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Chillipede = new("Chillipede", true);


    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID IceChunk = new("IceChunk", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID FreezerCrystal = new("FreezerCrystal", true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BurnSpear = new("BurnSpear", true);


    public static void UnregisterValues()
    {
        if (InfantAquapede is not null)
        {
            InfantAquapede.Unregister();
            InfantAquapede = null;
        }
        if (SnowcuttleFemale is not null)
        {
            SnowcuttleFemale.Unregister();
            SnowcuttleFemale = null;
        }
        if (SnowcuttleMale is not null)
        {
            SnowcuttleMale.Unregister();
            SnowcuttleMale = null;
        }
        if (SnowcuttleLe is not null)
        {
            SnowcuttleLe.Unregister();
            SnowcuttleLe = null;
        }
        if (Raven is not null)
        {
            Raven.Unregister();
            Raven = null;
        }
        if (IcyBlue is not null)
        {
            IcyBlue.Unregister();
            IcyBlue = null;
        }
        if (Freezer is not null)
        {
            Freezer.Unregister();
            Freezer = null;
        }
        if (PeachSpider is not null)
        {
            PeachSpider.Unregister();
            PeachSpider = null;
        }
        if (Cyanwing is not null)
        {
            Cyanwing.Unregister();
            Cyanwing = null;
        }
        if (GorditoGreenie is not null)
        {
            GorditoGreenie.Unregister();
            GorditoGreenie = null;
        }
        /*
        if (BezanBudUnlock is not null)
        {
            BezanBudUnlock.Unregister();
            BezanBudUnlock = null;
        }
        */
        if (Chillipede is not null)
        {
            Chillipede.Unregister();
            Chillipede = null;
        }
        if (Luminescipede is not null)
        {
            Luminescipede.Unregister();
            Luminescipede = null;
        }
        /*
        if (StrobelegsUnlock is not null)
        {
            StrobelegsUnlock.Unregister();
            StrobelegsUnlock = null;
        }
        */

        if (IceChunk is not null)
        {
            IceChunk.Unregister();
            IceChunk = null;
        }
        if (FreezerCrystal is not null)
        {
            FreezerCrystal.Unregister();
            FreezerCrystal = null;
        }
        if (BurnSpear is not null)
        {
            BurnSpear.Unregister();
            BurnSpear = null;
        }

    }

}

//----------------------------------------------------------------------------------

public static class HailstormDamageTypes
{

    [AllowNull] public static Creature.DamageType Cold = new("HailstormCold", true);
    [AllowNull] public static Creature.DamageType Heat = new("HailstormHeat", true);
    [AllowNull] public static Creature.DamageType Venom = new("HailstormVenom", true);

    public static void UnregisterValues()
    {
        if (Cold is not null)
        {
            Cold.Unregister();
            Cold = null;
        }
        if (Heat is not null)
        {
            Heat.Unregister();
            Heat = null;
        }
        if (Venom is not null)
        {
            Venom.Unregister();
            Venom = null;
        }
    }

}

//--------------------------------------------------------------------------------------------------------------------------