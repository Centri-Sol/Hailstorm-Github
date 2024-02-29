namespace Hailstorm;

public class AbstractBurnSpear : AbstractSpear
{

    public Color spearColor;
    public float[] rgb1;
    public Color fireFadeColor;
    public float[] rgb2;
    public Color currentColor;

    public float heat = 1;
    public LightSource glow;
    public float[,] flicker = new float[2, 3];
    public bool burning;
    public float chill;
    public int emberTimer;

    public Vector2 spearTipPos;

    public AbstractBurnSpear(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, float heat, Color spearColor, Color fireFadeColor) : base(world, realizedObject, pos, ID, explosive)
    {
        type = HSEnums.AbstractObjectType.BurnSpear;
        this.spearColor = spearColor;
        this.fireFadeColor = fireFadeColor;
        this.heat = heat;
        rgb1 = new float[3]
        {
            this.spearColor.r,
            this.spearColor.g,
            this.spearColor.b
        };
        rgb2 = new float[3]
        {
            this.fireFadeColor.r,
            this.fireFadeColor.g,
            this.fireFadeColor.b
        };
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject is null && type == HSEnums.AbstractObjectType.BurnSpear)
        {
            realizedObject = new Spear(this, world);
        }
    }

    public override string ToString()
    {
        string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}<oA>{7}<oA>{8}", ID.ToString(), type.ToString(), pos.SaveToString(), stuckInWallCycles, explosive ? "1" : "0", hue.ToString(), electric ? "1" : "0", electricCharge.ToString(), needle ? "1" : "0");
        text += string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}", heat.ToString(), rgb1[0], rgb1[1], rgb1[2], rgb2[0], rgb2[1], rgb2[2]);

        return this.SaveToString($"{ID};{type};{pos.SaveToString()};{stuckInWallCycles};{0};{hue};{(electric ? 1 : 0)};{electricCharge};{(needle ? 1 : 0)};{heat};{rgb1[0]};{rgb1[1]};{rgb1[2]};{rgb2[0]};{rgb2[1]};{rgb2[2]}") + SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
    }
}