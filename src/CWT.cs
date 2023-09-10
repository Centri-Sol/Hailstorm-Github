using System;
using UnityEngine;
using SlugBase;
using MoreSlugcats;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using RWCustom;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class CWT
{
    public static ConditionalWeakTable<Player, HailstormSlugcats> PlayerData = new();
    public static ConditionalWeakTable<Creature, CreatureInfo> CreatureData = new();
    public static ConditionalWeakTable<AbstractCreature, AbsCtrInfo> AbsCtrData = new();
}


public class HailstormSlugcats // Stores a boatload of information for individual players.
{
    public readonly bool isIncan;
    public static SlugcatStats.Name Incandescent = new ("Incandescent", false);
    public SlugBaseCharacter Incan;
    public WeakReference<Player> incanRef;

    public int rollExtender;
    public int rollFallExtender;
    /*public int wallRollDirection;
    public int ceilingRollDirection;
    public bool wallRolling;
    public bool ceilingRolling;
    public bool wallRollingFromCeiling;
    public static readonly Player.AnimationIndex WallRoll = new Player.AnimationIndex("WallRoll", register: true);
    public static readonly Player.AnimationIndex CeilingRoll = new Player.AnimationIndex("CeilingRoll", register: true);
    public int wallStopTimer = 0; 
    public int ceilingStopTimer = 0;*/
    // ^ This commented-out stuff up here is scrapped unless I can figure out how to properly implement it. That'll be a LONG while.

    public LightSource incanLight;
    public float[,] flicker;
    public float hypothermiaResistance;
    public int fireFuel;
    public int waterGlow;

    public int wetness;
    public bool inWater;
    public int lanternDryTimer;
    public int impactCooldown;

    public int tailflameSprite;
    public Vector2 lastTailflamePos;

    public float smallEmberTimer;
    public float bigEmberTimer;
    public HailstormFireSmokeCreator fireSmoke;

    public Color FireColorBase;
    public Color FireColor;
    public int cheekFluffSprite;
    public Vector2 lastCheekfluffPos;

    public Color WaistbandColor;
    public int waistbandSprite;
    public Vector2 lastWaistbandPos;

    public bool readyToMoveOn;

    public bool longJumpReady;
    public bool longJumping;
    public bool highJump;

    public bool inArena;
    public bool currentCampaignBeforeRiv;

    public float ColdDMGmult = 1;
    public float HeatDMGmult = 1;

    public int craftingDelayCounter;
    public bool successfulCraft;

    // Creates a specific instance of IncanPlayer. This stores all values that are important to the Incandescent.
    public HailstormSlugcats(Player self)
    {

        incanRef =
            new WeakReference<Player>(self); // I'm not sure why this is needed, but I'm keeping it just in case.

        if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Incandescent", true, out var extEnum))
        {
            Incandescent = extEnum as SlugcatStats.Name;
        }

        isIncan = self.SlugCatClass == Incandescent;

        if (isIncan)
        {
            ColdDMGmult = 2.00f;
            HeatDMGmult = 0.25f;
        }
        else if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            ColdDMGmult = 0.85f;
            HeatDMGmult = 1.15f;
        }
        else if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
        {
            ColdDMGmult = 1.15f;
            HeatDMGmult = 0.85f;
        }
        else if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
        {
            ColdDMGmult = 1.15f;
            HeatDMGmult = 1.15f;
        }

        if (!isIncan) return;

        /*wallRolling = false;
        ceilingRolling = false;
        wallRollDirection = 0;
        ceilingRollDirection = 0;
        wallRollingFromCeiling = false; */

        flicker = new float[2, 3];
        for (int i = 0; i < flicker.GetLength(0); i++)
        {
            flicker[i, 0] = 1f;
            flicker[i, 1] = 1f;
            flicker[i, 2] = 1f;
        }

        inArena = self.room.world.game.IsArenaSession;

        if (self.room.game.session is StoryGameSession SGS)
        {
            SlugcatStats.Name[] slugcatTimelineOrder = SlugcatStats.getSlugcatTimelineOrder();
            bool? tooFar = null;
            for (int s = 0; s < slugcatTimelineOrder.Length - 1; s++)
            {
                if (slugcatTimelineOrder[s] == SGS.saveStateNumber)
                {
                    tooFar = false;
                }
                else if (slugcatTimelineOrder[s] == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
                {
                    tooFar = true;
                }

                if (tooFar.HasValue) break;
            }

            currentCampaignBeforeRiv = tooFar.HasValue && tooFar.Value is false;

        }
    }
}

//-----------------------------------------

public class LizardInfo
{
    public bool isIncanCampaign;
    public bool isFreezerOrIcyBlue;
    public bool isHailstormLiz;

    public int iceBreath;
    public Vector2 breathDir;

    public int abilityTimer;
    public int abilityCooldown;
    public BodyChunk aimChunk;
    public bool abilityUsed;
    public float giveUpChance;

    public int armorGraphic;
    public LightSource freezerAura;
    public float auraRadius;

    public float packPower;
    public bool nearAFreezer;
    public int packPowerUpdateTimer;

    public LizardInfo(Lizard liz)
    {       

        isIncanCampaign =
            liz.room is not null &&
            liz.room.game.IsStorySession &&
            liz.room.game.StoryCharacter == HailstormSlugcats.Incandescent;

        isFreezerOrIcyBlue =
            liz is not null &&
            (liz.Template.type == HailstormEnums.Freezer || liz.Template.type == HailstormEnums.IcyBlue);

        isHailstormLiz =
            liz is not null &&
            (liz.Template.type == HailstormEnums.Freezer || liz.Template.type == HailstormEnums.IcyBlue || liz.Template.type == HailstormEnums.GorditoGreenie);

        if (liz is not null && (liz.Template.type == HailstormEnums.Freezer || liz.Template.type == HailstormEnums.IcyBlue))
        {
            if (liz.Template.type == HailstormEnums.Freezer && abilityCooldown == 0)
            {
                abilityCooldown = Random.Range(320, 480);
            }
        }
    }
}

public class CentiInfo
{
    public bool isIncanCampaign;
    public bool Cyanwing;
    public bool BabyAquapede;
    public bool Chillipede;
    public bool isHailstormCenti;

    public float[] segmentHues;
    public bool[] segmentGradientDirections;

    public bool offcolor;

    public CentiInfo(Centipede cnt)
    {

        isIncanCampaign =
            cnt?.room is not null &&
            cnt.room.game.IsStorySession &&
            cnt.room.game.StoryCharacter == HailstormSlugcats.Incandescent;

        Cyanwing =
            cnt is not null &&
            cnt.Template.type == HailstormEnums.Cyanwing;

        BabyAquapede =
            cnt is not null &&
            cnt.Template.type == HailstormEnums.InfantAquapede;

        Chillipede =
            cnt is not null &&
            cnt.Template.type == HailstormEnums.Chillipede;

        isHailstormCenti =
            Cyanwing || BabyAquapede || Chillipede;

        if (Cyanwing)
        {
            Random.State state = Random.state;
            Random.InitState(cnt.abstractCreature.ID.RandomSeed);
            offcolor = Random.value < 0.1f;
            Random.state = state;
        }

    }
}
public class ChillipedeScaleInfo
{
    public int[] ScaleRefreeze;
    public Color scaleColor;
    public Color accentColor;
    public List<int[]> ScaleSprites;
    public int StartOfNewSprites;
    public int mistTimer = 160;
    public ChillipedeScaleInfo(Centipede.CentipedeState cntState)
    {
        ScaleRefreeze = new int[cntState.shells.Length];
        ScaleSprites = new();
        Random.State state = Random.state;
        Random.InitState(cntState.creature.ID.RandomSeed);
        for (int b = 0; b < cntState.shells.Length; b++)
        {
            ScaleSprites.Add(new int[2] { Random.Range(0, 3), Random.Range(0, 3) });
        }
        Random.state = state;
    }
}

//-----------------------------------------

public class CreatureInfo
{
    public bool isIncanCampaign;
    

    public int impactCooldown;
    public int chillTimer;
    public int heatTimer;

    public float freezerChill;

    public float customFunction;

    public bool hitDeflected;

    public CreatureInfo(Creature ctr)
    {
        isIncanCampaign =
            ctr.room is not null &&
            ctr.room.game.IsStorySession &&
            ctr.room.game.StoryCharacter == HailstormSlugcats.Incandescent;

    }
}

public class AbsCtrInfo
{
    public bool isIncanCampaign;

    public bool LateBlizzardRoamer;
    public bool FogRoamer;

    public bool[] spikeBroken;
    public int functionTimer;

    public bool isFreezerOrIcyBlue;

    public List<AbstractCreature> ctrList;

    public List<Debuff> debuffs;

    public float[] scaleHP;

    public AbsCtrInfo(AbstractCreature absCtr)
    {
        isIncanCampaign =
            absCtr?.world?.game is not null &&
            absCtr.world.game.IsStorySession &&
            absCtr.world.game.StoryCharacter == HailstormSlugcats.Incandescent;

        isFreezerOrIcyBlue =
            absCtr is not null && (absCtr.creatureTemplate.type == HailstormEnums.Freezer || absCtr.creatureTemplate.type == HailstormEnums.IcyBlue);

        if (isFreezerOrIcyBlue)
        {
            if (spikeBroken is null)
            {
                spikeBroken = new bool[3];
            }
        }
        else if (absCtr is not null)
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
        if (burnSource is null || target is null) return;

        if (debuffs is null) debuffs = new();


        if (target is Player self && CWT.PlayerData.TryGetValue(self, out HailstormSlugcats hS))
        {
            if (hS.isIncan)
            {
                return;
            }
            else debuffDuration = (int)(debuffDuration * hS.HeatDMGmult);
        }
        else if (target is Overseer || target is Inspector || (target is EggBug egg && egg.FireBug))
        {
            return;
        }
        else if (target is Creature ctr)
        {
            debuffDuration = (int)(debuffDuration / ctr.Template.damageRestistances[HailstormEnums.HeatDamage.index, 0]);
            if (ctr is Centipede cnt)
            {
                debuffDuration = (int)(debuffDuration * Mathf.Lerp(1.3f, 0.1f, Mathf.Pow((cnt.AquaCenti ? cnt.size / 2 : cnt.size), 0.5f)));
            }
        }

        debuffs.Add(new Burn(burnSource, hitChunk, debuffDuration, baseColor, fadeColor));
    }

}

//-----------------------------------------

public class TuskInfo
{
    public Vector2 bounceOffSpeed;

    public TuskInfo (KingTusks.Tusk tusk)
    {

    }
}

//-----------------------------------------

public class PupButtonInfo
{
    public Menu.MenuIllustration uniqueSymbol2;
    public Color uniqueTintColor2;
    public PupButtonInfo(JollyCoop.JollyMenu.SymbolButtonTogglePupButton pupButton)
    {

    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------