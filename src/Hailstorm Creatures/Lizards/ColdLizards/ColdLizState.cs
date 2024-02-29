namespace Hailstorm;

public class ColdLizState : LizardState
{
    public bool[] crystals;
    public bool armored;

    public int spitWindup;
    public BodyChunk spitAimChunk;
    public bool spitUsed;
    public float spitGiveUpChance;
    public int spitCooldown;

    public ColdLizState(AbstractCreature absLiz) : base(absLiz)
    {
        crystals = new bool[3] { true, true, true };
        armored = !crystals.All(intact => !intact);
        if (absLiz.creatureTemplate.type == HSEnums.CreatureType.FreezerLizard && spitCooldown == 0)
        {
            spitCooldown = Random.Range(320, 480);
        }
    }

    public override string ToString()
    {
        string saveData = base.ToString();
        bool anyCrystalsBroken = false;
        if (crystals is not null)
        {
            for (int i = 0; i < crystals.Length; i++)
            {
                if (!crystals[i])
                {
                    anyCrystalsBroken = true;
                    break;
                }
            }
        }
        if (anyCrystalsBroken)
        {
            string armorData = "";
            for (int j = 0; j < crystals.Length; j++)
            {
                armorData += crystals[j] ? "1" : "0";
            }
            saveData = saveData + "<cB>IceArmor<cC>" + armorData;
        }
        return saveData;
    }
    public override void LoadFromString(string[] s)
    {
        base.LoadFromString(s);
        for (int i = 0; i < s.Length; i++)
        {
            switch (Regex.Split(s[i], "<cC>")[0])
            {
                case "IceArmor":
                    {
                        string text = Regex.Split(s[i], "<cC>")[1];
                        crystals = new bool[text.Length];
                        for (int j = 0; j < text.Length && j < crystals.Length; j++)
                        {
                            crystals[j] = text[j] == '1';
                        }
                        break;
                    }

                default:
                    break;
            }
        }
        unrecognizedSaveStrings.Remove("IceArmor");
    }

    public override void CycleTick()
    {
        base.CycleTick();
        if (!alive || crystals is null)
        {
            return;
        }
        for (int i = 0; i < crystals.Length; i++)
        {
            if (!crystals[i])
            {
                crystals[i] = Random.value < 0.2f;
            }
        }
    }
}