namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class CWT
{
    public static ConditionalWeakTable<Creature, CreatureInfo> CreatureData = new();
    public static ConditionalWeakTable<AbstractCreature, AbsCtrInfo> AbsCtrData = new();
    public static ConditionalWeakTable<PhysicalObject, ObjectInfo> ObjectData = new();

}

//-----------------------------------------

public class LizardInfo
{

    public LizardInfo(Lizard liz)
    {

    }
}

public class CentiInfo
{

    public float Charge;

    public CentiInfo(Centipede cnt)
    {

    }
}

public class VultureInfo
{
    public readonly bool King;
    public readonly bool Miros;
    public readonly bool Raven;

    public HSLColor ColorA;
    public HSLColor ColorB;
    public bool albino;

    public Creature currentPrey;
    public Vector2 laserAngle;

    public HSLColor eyeCol;
    public HSLColor wingColor;
    public HSLColor featherColor1;
    public HSLColor featherColor2;
    public HSLColor MiscColor;

    public HSLColor smokeCol1;
    public HSLColor smokeCol2;

    public int wingGlowFadeTimer;

    public VultureInfo(Vulture vul)
    {
        if (vul is not null)
        {
            if (vul.Template.type == CreatureTemplate.Type.KingVulture)
            {
                King = true;
            }
            else if (vul.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
            {
                Miros = true;
            }
            else if (vul.Template.type == HailstormCreatures.Raven)
            {
                Raven = true;
            }
        }
    }
}

//-----------------------------------------

public class CreatureInfo
{

    public int impactCooldown;
    public int chillTimer;
    public int heatTimer;

    public float freezerChill;

    public float customFunction;

    public bool hitDeflected;

    public CreatureInfo(Creature ctr)
    {

    }
}

public class AbsCtrInfo
{

    public bool LateBlizzardRoamer;
    public bool HailstormAvoider;
    public bool FogRoamer;
    public bool FogAvoider;
    public bool ErraticWindRoamer;
    public bool ErraticWindAvoider;
    public bool HasHSCustomFlag;

    public bool destinationLocked;

    public int functionTimer;

    public List<AbstractCreature> ctrList;

    public List<Debuff> debuffs;

    public AbsCtrInfo(AbstractCreature absCtr)
    {
        if (absCtr is not null)
        {
            if (ctrList is null && (
                absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
                absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug))
            {
                ctrList = new List<AbstractCreature>();
            }
        }
    }

    public void AddBurn(AbstractPhysicalObject burnSource, PhysicalObject target, int? hitChunk, int debuffDuration, Color baseColor, Color fadeColor)
    {
        if (burnSource is null || target is null)
        {
            return;
        }

        debuffs ??= new();


        if (IsTargetBurnImmune(target))
        {
            return;
        }
        else if (target is Creature ctr)
        {
            debuffDuration = (int)(debuffDuration / ctr.Template.damageRestistances[HailstormDamageTypes.Heat.index, 0]);
        }

        debuffs.Add(new Burn(burnSource, hitChunk, debuffDuration, baseColor, fadeColor));
    }

    public bool IsTargetBurnImmune(PhysicalObject target)
    {
        return target is Overseer ||
            target is Inspector ||
            (target is EggBug egg && egg.FireBug) ||
            (target is Player self && IncanInfo.IncanData.TryGetValue(self, out IncanInfo Incan) && Incan.isIncan);
    }

}

public class ObjectInfo
{
    public bool inShortcut;

    public ObjectInfo(PhysicalObject obj)
    {

    }
}

//-----------------------------------------

public class TuskInfo
{

    public Vector2 bounceOffSpeed;

    public TuskInfo(KingTusks.Tusk tusk)
    {

    }
}

//-----------------------------------------

public class PupButtonInfo
{
    public Menu.MenuIllustration uniqueSymbol2;
    public Color uniqueTintColor2;
    public PupButtonInfo(SymbolButtonTogglePupButton pupButton)
    {

    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------