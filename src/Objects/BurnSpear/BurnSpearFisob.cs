namespace Hailstorm;

public class BurnSpearFisob : Fisob
{
    internal BurnSpearFisob() : base(HSEnums.AbstractObjectType.BurnSpear)
    {
        Icon = new SimpleIcon("Icon_Burn_Spear", Custom.hexToColor("FF3232"));
        SandboxPerformanceCost = new(0.3f, 0f);
        RegisterUnlock(HSEnums.SandboxUnlock.BurnSpear, parent: HSEnums.SandboxUnlock.Freezer);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        string[] p = entitySaveData.CustomData.Split(';');
        if (p.Length < 16)
        {
            p = new string[16];
        }

        float[] rgb1 = new float[3]
        {
            float.TryParse(p[10], out float r1) ? r1 : 0,
            float.TryParse(p[11], out float g1) ? g1 : 0,
            float.TryParse(p[12], out float b1) ? b1 : 0
        };
        Color spearColor = new(r1, g1, b1, r1 + g1 + b1 > 0 ? 1 : 0);

        float[] rgb2 = new float[3]
        {
            float.TryParse(p[13], out float r2) ? r2 : 0,
            float.TryParse(p[14], out float g2) ? g2 : 0,
            float.TryParse(p[15], out float b2) ? b2 : 0
        };
        Color fireFadeColor = new(r2, g2, b2, r2 + g2 + b2 > 0 ? 1 : 0);

        AbstractBurnSpear burnSpear = new(world, null, entitySaveData.Pos, entitySaveData.ID, false, float.TryParse(p[9], out float heat) ? heat : 1, spearColor, fireFadeColor)
        {
            rgb1 = rgb1,
            rgb2 = rgb2,
        };

        return burnSpear;
    }

    public override void LoadResources(RainWorld rainWorld)
    {
    }

}