using System.Collections.Generic;
using UnityEngine;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class CyanwingState : Centipede.CentipedeState
{
    public List<Shell> superShells;

    public CyanwingState(AbstractCreature absCtr) : base(absCtr)
    {
    }

    public class Shell
    {
        public int index;
        public float hue;
        public bool gradientDirection;

        public Shell(int index)
        {
            this.index = index;
        }
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------