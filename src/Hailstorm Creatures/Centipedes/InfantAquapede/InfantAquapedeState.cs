using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class InfantAquapedeState : Centipede.CentipedeState
{
    public int remainingBites;

    public InfantAquapedeState(AbstractCreature absCtr) : base(absCtr)
    {
    }
    public override string ToString()
    {
        string saveData = base.ToString() + "<cB>Bites<cC>" + remainingBites;
        return saveData;
    }

    public override void LoadFromString(string[] s)
    {
        base.LoadFromString(s);
        for (int i = 0; i < s.Length; i++)
        {
            if (Regex.Split(s[i], "<cC>")[0] == "Bites")
            {
                int bites = int.Parse(Regex.Split(s[i], "<cC>")[1]);
                remainingBites = bites;
                break;
            }
        }
        unrecognizedSaveStrings.Remove("Bites");
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
