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
    public static ConditionalWeakTable<Player, HSSlugs> PlayerData = new();
    public static ConditionalWeakTable<Creature, CreatureInfo> CreatureData = new();
    public static ConditionalWeakTable<AbstractCreature, AbsCtrInfo> AbsCtrData = new();
}


public class HSSlugs // Stores a boatload of information for individual players.
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

    public int soak;
    public int maxSoak = 7200;
    public bool bubbleHeat;
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

    public bool readyToAccept;

    public bool longJumpReady;
    public bool longJumping;
    public bool highJump;

    public int singeFlipTimer;

    public bool inArena;
    public bool currentCampaignBeforeRiv;

    public float ColdDMGmult = 1;
    public float HeatDMGmult = 1;

    public int craftingDelayCounter;
    public bool successfulCraft;

    // Creates a specific instance of IncanPlayer. This stores all values that are important to the Incandescent.
    public HSSlugs(Player self)
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
    public bool Gordito;
    public bool bounceLungeUsed;

    public LizardInfo(Lizard liz)
    {
        Gordito = liz.Template.type == HailstormEnums.GorditoGreenie;
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
            cnt.room.game.StoryCharacter == HSSlugs.Incandescent;

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
            ctr.room.game.StoryCharacter == HSSlugs.Incandescent;

    }
}

public class AbsCtrInfo
{
    public bool isIncanCampaign;

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
        isIncanCampaign =
            absCtr?.world?.game is not null &&
            absCtr.world.game.IsStorySession &&
            absCtr.world.game.StoryCharacter == HSSlugs.Incandescent;

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
        if (burnSource is null || target is null) return;

        if (debuffs is null) debuffs = new();


        if (target is Player self && CWT.PlayerData.TryGetValue(self, out HSSlugs hS))
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
            debuffDuration = (int)(debuffDuration / ctr.Template.damageRestistances[HailstormEnums.Heat.index, 0]);
            if (ctr is Centipede cnt)
            {
                debuffDuration = (int)(debuffDuration * Mathf.Lerp(1.3f, 0.1f, Mathf.Pow((cnt.AquaCenti ? cnt.size / 2 : cnt.size), 0.5f)));
            }
            if (ctr.Template.type == HailstormEnums.Freezer)
            {
                debuffDuration = (int)(debuffDuration * 0.8f);
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