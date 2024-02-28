using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hailstorm;

public static class SoundEffects
{

    public static SoundID CyanwingDeath;

    public static void RegisterValues()
    {
        CyanwingDeath = new("CyanwingDeath", true);
    }

    public static void UnregisterValues()
    {
        if (CyanwingDeath is not null)
        {
            CyanwingDeath = null;
        }
    }

}