using System.Collections.Generic;
using MoreSlugcats;

namespace Hailstorm;

public class CustomObjectInfo
{
    public static Dictionary<AbstractPhysicalObject.AbstractObjectType, bool> FreezableObjects;
    // ^ Anything marked "false" will be destroyed if the ice the item is in is shattered.
    // Those items can still be released safely by letting the ice melt.

    public static void AddFreezableObjects()
    {
        FreezableObjects = new Dictionary<AbstractPhysicalObject.AbstractObjectType, bool>
        {
            { AbstractPhysicalObject.AbstractObjectType.DangleFruit, false },
            { AbstractPhysicalObject.AbstractObjectType.DataPearl, false },
            { AbstractPhysicalObject.AbstractObjectType.EggBugEgg, false },
            { AbstractPhysicalObject.AbstractObjectType.FlareBomb, false },
            { AbstractPhysicalObject.AbstractObjectType.Lantern, false },
            { AbstractPhysicalObject.AbstractObjectType.NeedleEgg, true },
            { AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, true },
            { AbstractPhysicalObject.AbstractObjectType.PuffBall, false },
            { AbstractPhysicalObject.AbstractObjectType.Rock, true },
            { AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, true },
            { AbstractPhysicalObject.AbstractObjectType.WaterNut, true },

            { MoreSlugcatsEnums.AbstractObjectType.GooieDuck, true },
            { MoreSlugcatsEnums.AbstractObjectType.FireEgg, true },
            { MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl, false },
            { MoreSlugcatsEnums.AbstractObjectType.Seed, false },
            { MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, true },
            { MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl, true },

            //{ HailstormItems.BezanNut, true },
        };
    }

}
