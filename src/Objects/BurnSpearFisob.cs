using System.Globalization;
using UnityEngine;
using RWCustom;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using Fisobs.Properties;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

sealed class BurnSpearFisob : Fisob
{
    internal BurnSpearFisob() : base(HailstormItems.BurnSpear)
    {
        Icon = new SimpleIcon("Icon_Burn_Spear", Custom.hexToColor("FF3232"));
        SandboxPerformanceCost = new(0.3f, 0f);
        RegisterUnlock(HailstormUnlocks.BurnSpear, parent: HailstormUnlocks.Freezer);
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
        Color spearColor = new(r1, g1, b1, (r1 + g1 + b1 > 0 ? 1 : 0));

        float[] rgb2 = new float[3]
        {
            float.TryParse(p[13], out float r2) ? r2 : 0,
            float.TryParse(p[14], out float g2) ? g2 : 0,
            float.TryParse(p[15], out float b2) ? b2 : 0
        };
        Color fireFadeColor = new(r2, g2, b2, (r2 + g2 + b2 > 0 ? 1 : 0));

        AbstractBurnSpear burnSpear = new(world, null, entitySaveData.Pos, entitySaveData.ID, false, (float.TryParse(p[9], out float heat) ? heat : 1), spearColor, fireFadeColor)
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

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

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
        type = HailstormItems.BurnSpear;
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
        if (realizedObject is null && type == HailstormItems.BurnSpear)
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

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

public class BurnSpearProperties : ItemProperties
{
    public override void Throwable(Player player, ref bool throwable)
    {
        throwable = true;
    }
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.BigOneHand;
    }
    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = 6;
    }
    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
    {
        score = 4;
    }
    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
    {
        score = 2;
    }
    public override void LethalWeapon(Scavenger scav, ref bool isLethal)
    {
        isLethal = true;
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------