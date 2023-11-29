using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using LizardCosmetics;
using System.Linq;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using MonoMod.RuntimeDetour;
using System;
using static System.Reflection.BindingFlags;

namespace Hailstorm;

//------------------------------------------------------------------------

public class ColdLizState : LizardState
{
    public bool IcyBlue;
    public bool Freezer;

    public bool[] crystals;
    public bool armored;
    public int crystalSprite;

    public int spitWindup;
    public BodyChunk spitAimChunk;
    public bool spitUsed;
    public float spitGiveUpChance;
    public int spitCooldown;

    public int iceBreath;
    public Vector2 breathDir;

    public float chillAuraRad;
    public LightSource chillAura;

    public float PackPower;
    public bool NearAFreezer;
    public int PackUpdateTimer;

    public ColdLizState(AbstractCreature absLiz) : base(absLiz)
    {
        IcyBlue = absLiz.creatureTemplate.type == HailstormEnums.IcyBlue;
        Freezer = absLiz.creatureTemplate.type == HailstormEnums.Freezer;
        crystals = new bool[3] { true, true, true };
        armored = !crystals.All(intact => !intact);
        if (Freezer && spitCooldown == 0)
        {
            spitCooldown = Random.Range(320, 480);
        }
    }
}

//------------------------------------------------------------------------

internal class HailstormLizards
{

    public static ConditionalWeakTable<Lizard, LizardInfo> LizardData = new();
    public static CreatureTemplate freezerTemplate;

    public static void Hooks()
    {
        //OverseerILHook();
        On.LizardLimb.ctor += HailstormLizLegSFX;
        On.LizardVoice.GetMyVoiceTrigger += HailstormLizVoiceSFX;
        On.LizardVoice.ctor += GorditoGreenieVoicePitch;
        On.LizardVoice.Update += GorditoGreenieVoiceVolume;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += HailstormLizTemplates;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate += EelTemperatureResistances;
        On.Lizard.ctor += HailstormLizConstructors;

        new Hook(typeof(LizardTongue).GetMethod("get_Ready", Public | NonPublic | Instance), (Func<LizardTongue, bool> orig, LizardTongue tongue) => orig(tongue) && !(tongue.lizard.Template.type == HailstormEnums.IcyBlue && tongue.lizard.TotalMass > 1.55f));
        On.LizardTongue.ctor += IcyBlueTongues;
        On.Lizard.HitHeadShield += GorditoGreenieSquishyHead;
        On.Lizard.SpearStick += ColdLizardArmorSpearDeflect;
        On.Lizard.Violence += ColdLizardArmorFunctionality;
        On.Lizard.Update += HailstormLizardUpdate;
        On.Lizard.Act += GorditoGreenieSlide;
        On.Lizard.EnterAnimation += GorditoEnterAnimation;
        On.Lizard.Collide += GorditoGreenieCollision;
        On.Lizard.TerrainImpact += GorditoGreenieGroundImpact;
        On.Lizard.Stun += HailstormLizStun;

        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += HailstormLizardRelationships;
        On.LizardAI.ctor += ColdLizardAISetup;
        On.LizardAI.Update += ColdLizardAIUpdate;
        On.LizardAI.TravelPreference += HailstormLizTravelPrefs;
        LizardAI_ILHooks();

        On.LizardGraphics.GenerateIvars += ColdLizardTailsWillAlwaysBeColored;
        On.LizardGraphics.ctor += HailstormLizardGraphics;
        On.LizardGraphics.InitiateSprites += GorditoGreenieBodySprites;
        On.LizardGraphics.DrawSprites += ColdLizardSpriteReplacements;
        On.LizardGraphics.AddToContainer += ColdLizardSpriteLayering;
        On.LizardGraphics.ApplyPalette += ColdLizardBodyColors1;
        On.LizardGraphics.BodyColor += ColdLizardBodyColors2;
        On.LizardGraphics.DynamicBodyColor += ColdLizardBodyColors3;
        IcyCosmetics.Init();
        new Hook(typeof(LizardGraphics).GetMethod("get_SalamanderColor", Public | NonPublic | Instance), (Func<LizardGraphics, Color> orig, LizardGraphics lG) => !(OtherCreatureChanges.IsIncanStory(lG?.lizard?.room?.game) && lG.lizard.abstractCreature.Winterized) ? orig(lG) : Color.Lerp((lG.blackSalamander ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.75f, 0.75f, 0.75f)), lG.effectColor, 0.08f));
       
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSSlugs.Incandescent);
        // ^ Returns true if all of the given conditions are met, or false otherwise.
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Basic Lizard Info

    public static void OverseerILHook()
    {
        // I guess this tells Overseers how dangerous these lizards are.        
        IL.OverseerAbstractAI.HowInterestingIsCreature += il =>
        {
            ILCursor c = new(il);
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
                x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
                x => x.MatchLdsfld<CreatureTemplate.Type>(nameof(CreatureTemplate.Type.RedLizard)),
                x => x.MatchCall(out _),
                x => x.MatchBrfalse(out label))
            && label is not null)
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HailstormEnums.Freezer);
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Plugin.logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Freezer Lizards!");

            ILCursor c2 = new(il);
            ILLabel? label2 = null;
            if (c2.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
                x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
                x => x.MatchLdsfld<CreatureTemplate.Type>(nameof(CreatureTemplate.Type.YellowLizard)),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label2))
            && label is not null)
            {
                c2.Emit(OpCodes.Ldarg_1);
                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HailstormEnums.IcyBlue);
                c2.Emit(OpCodes.Brtrue, label2);
            }
            else
                Plugin.logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Icy Blue Lizards!");

            
            ILCursor c3 = new(il);
            ILLabel? label3 = null;
            if (c2.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
                x => x.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
                x => x.MatchLdsfld<CreatureTemplate.Type>(nameof(CreatureTemplate.Type.GreenLizard)),
                x => x.MatchCall(out _),
                x => x.MatchBrfalse(out label3))
            && label is not null)
            {
                c2.Emit(OpCodes.Ldarg_1);
                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HailstormEnums.GorditoGreenie);
                c2.Emit(OpCodes.Brfalse, label3);
            }
            else
                Plugin.logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Gorditio Greenie Lizards!");
            
        };
    }

    public static void HailstormLizLegSFX(On.LizardLimb.orig_ctor orig, LizardLimb lizLimb, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(lizLimb, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics liz)
        {
            if (liz.lizard?.Template.type == HailstormEnums.Freezer)
            {
                lizLimb.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                lizLimb.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
            else if (liz.lizard?.Template.type == HailstormEnums.IcyBlue)
            {
                lizLimb.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                lizLimb.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
            else if (liz.lizard?.Template.type == HailstormEnums.GorditoGreenie)
            {
                lizLimb.grabSound = SoundID.Lizard_Green_Foot_Grab;
                lizLimb.releaseSeound = SoundID.Lizard_Green_Foot_Grab;
            }
        }
    }
    public static SoundID HailstormLizVoiceSFX(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice lizVoice)
    {
        SoundID res = orig(lizVoice);

        if (lizVoice.lizard is Lizard liz)
        {
            string[] voiceClips = new[] { "A", "B", "C", "D", "E" };
            List<SoundID> list = new();
            if (liz.Template.type == HailstormEnums.Freezer)
            {
                for (int i = 0; i < voiceClips.Length; i++)
                {
                    SoundID soundID = SoundID.None;
                    string voiceClip = "Lizard_Voice_Red_" + voiceClips[i];
                    if (SoundID.values.entries.Contains(voiceClip))
                    {
                        soundID = new(voiceClip);
                    }
                    if (soundID != SoundID.None && soundID.Index != -1 && liz.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    {
                        list.Add(soundID);
                    }
                }

                if (list.Count == 0)
                {
                    res = SoundID.None;
                }
                else res = list[Random.Range(0, list.Count)];
            }
            else if (liz.Template.type == HailstormEnums.IcyBlue)
            {
                for (int i = 0; i < voiceClips.Length; i++)
                {
                    SoundID soundID = SoundID.None;
                    string voiceClip = "Lizard_Voice_Blue_" + voiceClips[i];
                    if (SoundID.values.entries.Contains(voiceClip))
                    {
                        soundID = new(voiceClip);
                    }
                    if (soundID != SoundID.None && soundID.Index != -1 && liz.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    {
                        list.Add(soundID);
                    }
                }

                if (list.Count == 0)
                {
                    res = SoundID.None;
                }
                else res = list[Random.Range(0, list.Count)];
            }
            else if (liz.Template.type == HailstormEnums.GorditoGreenie)
            {
                for (int i = 0; i < voiceClips.Length; i++)
                {
                    SoundID soundID = SoundID.None;
                    string voiceClip = "Lizard_Voice_Green_" + voiceClips[i];
                    if (SoundID.values.entries.Contains(voiceClip))
                    {
                        soundID = new(voiceClip);
                    }
                    if (soundID != SoundID.None && soundID.Index != -1 && liz.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    {
                        list.Add(soundID);
                    }
                }

                if (list.Count == 0)
                {
                    res = SoundID.None;
                }
                else res = list[Random.Range(0, list.Count)];
            }
        }
        return res;
    }
    public static void GorditoGreenieVoicePitch(On.LizardVoice.orig_ctor orig, LizardVoice voice, Lizard liz)
    {
        orig(voice, liz);
        if (liz is not null && liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            voice.myPitch *= 0.33f;
        }
    }
    public static void GorditoGreenieVoiceVolume(On.LizardVoice.orig_Update orig, LizardVoice voice)
    {
        orig(voice);
        if (voice.lizard is not null && voice.lizard.Template.type == HailstormEnums.GorditoGreenie && voice.articulationIndex > -1 && voice.currentArticulationProgression < 1)
        {
            voice.Volume *= 0.7f / voice.myPitch;
        }
    }

    public static CreatureTemplate HailstormLizTemplates(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type lizardType, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        if (lizardType == HailstormEnums.Freezer || lizardType == HailstormEnums.IcyBlue)
        {            
            CreatureTemplate frzTemp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams frzStats = (frzTemp.breedParameters as LizardBreedParams)!;

            // Check LizardBreeds in the game's files if you want to compare to other lizards' stats.
            frzTemp.type = lizardType;

            frzTemp.name = "Freezer Lizard"; // Internal lizard name.

            frzStats.standardColor = new(129f / 255f, 200f / 255f, 236f / 255f); // The lizard's base color.

            frzTemp.meatPoints = 10;  // How many food pips a creature will give when eaten. Has to be an Int.

            frzStats.tamingDifficulty = 9f; // How stubborn the lizard is to taming attempts. Higher numbers make them harder to tame.

            frzStats.biteDelay = 16; // Delay between being ready to bite and actually biting.
            frzStats.biteInFront = 20f;
            frzStats.biteRadBonus = 10f; // Bonus reach for the actual bite attack.
            frzStats.biteHomingSpeed = 2f; // Head tracking speed while trying to go for a bite.
            frzStats.biteChance = 0.7f; // Chance of going for a bite.
            frzStats.attemptBiteRadius = 90f; // How far it'll try to bite you from.
            frzStats.getFreeBiteChance = 0.9f; // Chance of biting to escape a grasp.
            frzStats.biteDamage = 4f; // Damage done to other creatures.
            frzStats.biteDamageChance = 0f; // Chance of killing the player on bite.
            frzStats.biteDominance = 0.7f;

            frzTemp.baseDamageResistance = 4f; // This is HP.
            frzTemp.baseStunResistance = 2.5f; // Stun times will be divided by this amount by default.
            frzTemp.instantDeathDamageLimit = 4f;
            frzTemp.damageRestistances[Creature.DamageType.Bite.index, 0] = 6; // Divides incoming bite damage by 6.
            frzTemp.damageRestistances[Creature.DamageType.Explosion.index, 0] = 2.25f; // Divides explosive damage by 2.25.
            frzTemp.damageRestistances[Creature.DamageType.Explosion.index, 1] = 0.70f; // Divides explosive stun by 0.7.
            frzTemp.damageRestistances[Creature.DamageType.Electric.index, 0] = 2f;
            frzTemp.damageRestistances[Creature.DamageType.Electric.index, 1] = 2/3f;
            frzTemp.damageRestistances[HailstormEnums.Cold.index, 0] = 3.50f;
            frzTemp.damageRestistances[HailstormEnums.Cold.index, 1] = 1.50f;
            frzTemp.damageRestistances[HailstormEnums.Heat.index, 0] = 0.50f;
            frzTemp.damageRestistances[HailstormEnums.Heat.index, 1] = 0.75f;
            frzTemp.wormGrassImmune = true; // Determines whether Wormgrass will try to eat this lizard or not.

            frzStats.bodyMass = 2.8f; // Weight.
            frzStats.bodySizeFac = 1.2f; // Overall scale of the lizard's body chunks.
            frzStats.bodyRadFac = 0.7f; //  W I D E N E S S
            frzStats.bodyStiffnes = 0.33f;
            frzStats.floorLeverage = 4f;
            frzStats.maxMusclePower = 11f;
            frzStats.wiggleSpeed = 0.7f;
            frzStats.wiggleDelay = 20;
            frzStats.danger = 0.8f; // How threatening the game considers this creature.
            frzStats.aggressionCurveExponent = 0.37f;

            frzStats.idleCounterSubtractWhenCloseToIdlePos = 0;
            frzStats.baseSpeed = 4f;
            frzStats.terrainSpeeds[1] = new(1f, 1f, 1.2f, 1.2f); // Ground movement.
            frzStats.terrainSpeeds[2] = new(0.9f, 1f, 1f, 1.1f); // Tunnel movement.
            frzStats.terrainSpeeds[3] = new(1f, 0.85f, 0.7f, 1f);// Pole movement speeds.
            frzStats.terrainSpeeds[4] = new(0.8f, 1f, 1f, 1.1f); // Background movement.
            frzStats.terrainSpeeds[5] = new(0.8f, 1f, 1f, 1.2f); // Ceiling movement.
            frzStats.swimSpeed = 0.5f;
            frzTemp.waterPathingResistance = 10f;
            frzTemp.offScreenSpeed = 4f;

            frzStats.loungeTendensy = 0.95f; // LUNGE, not  l o u n g e. Determines how eager this lizard is to go for a lunge. 0 means "Never", and 1 means "Is 100 meters from your location and approaching rapidly".
            frzStats.findLoungeDirection = 1f; // 
            frzStats.preLoungeCrouch = 20; // Wind-up time for this lizard's lunge.
            frzStats.preLoungeCrouchMovement = -0.5f; // Movement speed during lunge.
            frzStats.loungeDistance = 400f; // How far the lizard is willing to lunge from.
            frzStats.loungeSpeed = 3.3f; // Self-explanatory.
            frzStats.loungeMaximumFrames = 20; // How long this lizard's lunge attack can last.
            frzStats.loungePropulsionFrames = 20; // How long this lizard spends accelerating?
            frzStats.loungeJumpyness = 0.5f; // How much they jump for their lunge.
            frzStats.loungeDelay = 240; // Lunge cooldown.
            frzStats.riskOfDoubleLoungeDelay = 0.75f; // Chance of cooldown being doubled.
            frzStats.postLoungeStun = 35;
            frzStats.canExitLoungeWarmUp = true; // Determines whether this lizard can cancel out of winding up their lunge.
            frzStats.canExitLounge = false; // Determines whether this lizard can cancel out of an actual lunge. This, uh, seems to pretty much negate postLoungeStun.

            frzTemp.visualRadius = 1750f; // How far the lizard can see.
            frzTemp.waterVision = 0.2f; // Vision in water.
            frzTemp.throughSurfaceVision = 0.5f; // How well the lizard can see through the surface of water.
            frzStats.perfectVisionAngle = 0.33f; // Determines how wide the angle of perfect vision the lizard has.
                                                 // - At -1, the lizard has perfect 360-degree vision
                                                 // - At 0, the lizard has perfect vision for up to 90 degrees in either direction (180 total)
                                                 // - At 1, the perfect vision angle is pretty much non-existent.
            frzStats.periferalVisionAngle = -0.5f; // The angle through which the lizard can see creatures at all, even if not well. Functions similarly to perfectVisionAngle.

            frzStats.limbSize = 1.2f; // Length of limbs.
            frzStats.limbThickness = 1.35f; // Chonkiness of limbs.
            frzStats.stepLength = 0.75f;
            frzStats.liftFeet = 0.2f; // How much lizards bring their feet up when crawling around.
            frzStats.feetDown = 0.4f; // How much lizards bring their feet down when crawling around.
            frzStats.noGripSpeed = 0.33f;
            frzStats.limbSpeed = 11f;
            frzStats.limbQuickness = 0.8f;
            frzStats.limbGripDelay = 0;
            frzStats.legPairDisplacement = 0;
            frzStats.walkBob = 3.5f; // How bumpy a lizard's movement is.
            frzStats.regainFootingCounter = 2;

            frzStats.tailSegments = 6; // Number of tail segments this lizard has.
            frzStats.tailStiffness = 150f;
            frzStats.tailStiffnessDecline = 0.1f;
            frzStats.tailLengthFactor = 3; // How long each tail segment is.
            frzStats.tailColorationStart = 0;
            frzStats.tailColorationExponent = 0.5f;

            frzStats.headShieldAngle = 140f;
            frzStats.headSize = 1f; // Scale of head sprites.
            frzStats.neckStiffness = 0.3f; // How resistant the lizard's head is to turning.
            frzStats.jawOpenAngle = 75f; // How wide this lizard will open their mouth.
            frzStats.jawOpenLowerJawFac = 0.66f; // How much the lizard's bottom jaw turns when its mouth opens.
            frzStats.jawOpenMoveJawsApart = 20f; // How far the lizard's bottom jaw lowers when its mouth opens.
            frzStats.headGraphics = new int[5] { 0, 0, 0, 2, 0 }; // This might be important?
            frzStats.framesBetweenLookFocusChange = 70;

            frzTemp.dangerousToPlayer = frzStats.danger;
            frzTemp.doPreBakedPathing = false;
            frzTemp.requireAImap = true;
            frzStats.tongue = false;
            frzTemp.BlizzardAdapted = true;
            frzTemp.BlizzardWanderer = true;
            frzTemp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
            frzTemp.hibernateOffScreen = false;
            frzTemp.usesCreatureHoles = true;
            frzTemp.usesNPCTransportation = true;
            frzTemp.usesRegionTransportation = true;
            frzTemp.shortcutSegments = 4;
            frzStats.shakePrey = 100;
            frzTemp.roamBetweenRoomsChance = 0.1f;

            frzTemp.pathingPreferencesConnections[1] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[2] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[4] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[6] = new PathCost(16, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[7] = new PathCost(2, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[8] = new PathCost(20, PathCost.Legality.Unwanted);
            frzTemp.pathingPreferencesConnections[12] = new PathCost(2, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[13] = new PathCost(3, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesConnections[21] = new PathCost(1, PathCost.Legality.Allowed);

            frzTemp.pathingPreferencesTiles[1] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesTiles[2] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesTiles[3] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesTiles[4] = new PathCost(1, PathCost.Legality.Allowed);
            frzTemp.pathingPreferencesTiles[5] = new PathCost(1, PathCost.Legality.Allowed);

            freezerTemplate = frzTemp;

            if (lizardType == HailstormEnums.Freezer) return freezerTemplate;
        }
        if (lizardType == HailstormEnums.IcyBlue)
        {
            CreatureTemplate icyBlueTemp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams icyBlueStats = (icyBlueTemp.breedParameters as LizardBreedParams)!;

            CreatureTemplate frzTemp = freezerTemplate;
            LizardBreedParams frzStats = freezerTemplate.breedParameters as LizardBreedParams;

            icyBlueTemp.type = lizardType;

            float sizeMult = 1.33f;
            float sizeFac = Mathf.InverseLerp(1f, 2f, sizeMult);


            icyBlueTemp.name = "Icy Blue Lizard";

            icyBlueStats.standardColor = Custom.HSL2RGB(0.625f, 0.7f, 0.7f);

            icyBlueTemp.meatPoints = 6;

            icyBlueStats.tamingDifficulty = 5.4f;

            icyBlueTemp.BlizzardAdapted = true;
            icyBlueTemp.BlizzardWanderer = true;
            icyBlueTemp.instantDeathDamageLimit = 3;
            icyBlueTemp.baseDamageResistance = 3;
            icyBlueTemp.baseStunResistance = 2.4f;
            icyBlueTemp.damageRestistances[Creature.DamageType.Bite.index, 0] = 5.5f;
            icyBlueTemp.damageRestistances[Creature.DamageType.Explosion.index, 0] = 1.7f;
            icyBlueTemp.damageRestistances[Creature.DamageType.Explosion.index, 1] = 0.7f;
            icyBlueTemp.damageRestistances[Creature.DamageType.Electric.index, 0] = 2f;
            icyBlueTemp.damageRestistances[Creature.DamageType.Electric.index, 1] = 2/3f;
            icyBlueTemp.damageRestistances[HailstormEnums.Cold.index, 0] = 1.50f;
            icyBlueTemp.damageRestistances[HailstormEnums.Cold.index, 1] = 1.00f;
            icyBlueTemp.damageRestistances[HailstormEnums.Heat.index, 0] = 0.75f;
            icyBlueTemp.damageRestistances[HailstormEnums.Heat.index, 1] = 0.75f;

            icyBlueStats.bodyMass = 1.5f;
            icyBlueStats.bodySizeFac = 0.9f * sizeMult;
            icyBlueStats.bodyRadFac = 1 / Mathf.Pow(sizeMult, 2);
            icyBlueStats.bodyStiffnes = Mathf.Lerp(0, frzStats.bodyStiffnes, sizeFac);
            icyBlueStats.floorLeverage = Mathf.Lerp(1, frzStats.floorLeverage, sizeFac);
            icyBlueStats.maxMusclePower = Mathf.Lerp(2, frzStats.maxMusclePower, sizeFac);
            icyBlueStats.wiggleSpeed = Mathf.Lerp(1, frzStats.wiggleSpeed, sizeFac);
            icyBlueStats.wiggleDelay = (int)Mathf.Lerp(15, frzStats.wiggleDelay, sizeFac);
            icyBlueStats.swimSpeed = Mathf.Lerp(0.35f, frzStats.swimSpeed, sizeFac);
            icyBlueStats.idleCounterSubtractWhenCloseToIdlePos = 0;
            icyBlueStats.danger = Mathf.Lerp(0.45f, frzStats.danger, sizeFac);
            icyBlueStats.aggressionCurveExponent = Mathf.Lerp(0.9f, frzStats.aggressionCurveExponent, sizeFac);

            icyBlueStats.baseSpeed = Mathf.Lerp(3.9f, frzStats.baseSpeed, sizeFac);
            icyBlueTemp.offScreenSpeed = 2f;
            icyBlueStats.terrainSpeeds[3] = new(1f, 0.9f, 0.8f, 1f);
            icyBlueStats.terrainSpeeds[4] = new(0.9f, 0.9f, 0.9f, 1f);
            icyBlueStats.terrainSpeeds[5] = new(0.7f, 1f, 1f, 1f);

            icyBlueStats.biteDelay = 15;
            icyBlueStats.biteRadBonus = Mathf.Lerp(0, frzStats.biteRadBonus, sizeFac);
            icyBlueStats.biteHomingSpeed = Mathf.Lerp(1.4f, frzStats.biteHomingSpeed, sizeFac);
            icyBlueStats.biteChance = Mathf.Lerp(0.4f, frzStats.biteChance, sizeFac);
            icyBlueStats.attemptBiteRadius = Mathf.Lerp(90f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.getFreeBiteChance = Mathf.Lerp(0.5f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.biteDamage = Mathf.Lerp(0.7f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.biteDamageChance = 0;
            icyBlueStats.biteDominance = Mathf.Lerp(0.1f, frzStats.attemptBiteRadius, sizeFac);

            icyBlueStats.canExitLoungeWarmUp = true;
            icyBlueStats.canExitLounge = false;
            icyBlueStats.preLoungeCrouch = (int)Mathf.Lerp(35, frzStats.preLoungeCrouch, sizeFac);
            icyBlueStats.preLoungeCrouchMovement = Mathf.Lerp(-0.3f, frzStats.preLoungeCrouchMovement, sizeFac);
            icyBlueStats.loungeSpeed = Mathf.Lerp(2.75f, frzStats.loungeJumpyness, sizeFac);
            icyBlueStats.loungeMaximumFrames = 20;
            icyBlueStats.loungePropulsionFrames = 20;
            icyBlueStats.loungeJumpyness = Mathf.Lerp(1, frzStats.loungeJumpyness, sizeFac);
            icyBlueStats.loungeDelay = (int)Mathf.Lerp(310, frzStats.loungeDelay, sizeFac);
            icyBlueStats.postLoungeStun = (int)Mathf.Lerp(20, frzStats.postLoungeStun, sizeFac);
            icyBlueStats.loungeTendensy = Mathf.Lerp(0.4f, frzStats.loungeTendensy, sizeFac);

            icyBlueStats.tongue = true;
            icyBlueStats.tongueChance = 0.2f;
            icyBlueStats.tongueWarmUp = 15;
            icyBlueStats.tongueSegments = 5;
            icyBlueStats.tongueAttackRange = 140;

            icyBlueTemp.visualRadius = 1250;
            icyBlueTemp.waterVision = Mathf.Lerp(0.4f, frzTemp.waterVision, sizeFac);
            icyBlueTemp.throughSurfaceVision = 0.7f;
            icyBlueStats.perfectVisionAngle = Mathf.Lerp(0.8888f, frzStats.perfectVisionAngle, sizeFac);
            icyBlueStats.periferalVisionAngle = Mathf.Lerp(0.0833f, frzStats.periferalVisionAngle, sizeFac);

            icyBlueStats.limbSize = Mathf.Lerp(0.9f, frzStats.limbSize, sizeFac);
            icyBlueStats.limbThickness = Mathf.Lerp(1, frzStats.limbThickness, sizeFac);
            icyBlueStats.stepLength = Mathf.Lerp(0.4f, frzStats.stepLength, sizeFac);
            icyBlueStats.liftFeet = Mathf.Lerp(0, frzStats.liftFeet, sizeFac);
            icyBlueStats.feetDown = Mathf.Lerp(0, frzStats.feetDown, sizeFac);
            icyBlueStats.noGripSpeed = Mathf.Lerp(0.2f, frzStats.noGripSpeed, sizeFac);
            icyBlueStats.limbSpeed = Mathf.Lerp(6, frzStats.limbSpeed, sizeFac);
            icyBlueStats.limbQuickness = Mathf.Lerp(0.6f, frzStats.limbQuickness, sizeFac);
            icyBlueStats.walkBob = Mathf.Lerp(0.4f, frzStats.walkBob, sizeFac);
            icyBlueStats.regainFootingCounter = (int)Mathf.Lerp(4, frzStats.regainFootingCounter, sizeFac);

            icyBlueStats.tailColorationStart = Mathf.Lerp(0.2f, 0.75f, Mathf.InverseLerp(1, 1.2f, sizeMult));
            icyBlueStats.tailColorationExponent = 0.5f;

            icyBlueStats.headShieldAngle = Mathf.Lerp(108, frzStats.headShieldAngle, sizeFac);
            icyBlueStats.headSize = Mathf.Lerp(0.9f, 1.1f, sizeFac);
            icyBlueStats.neckStiffness = Mathf.Lerp(0, frzStats.neckStiffness, sizeFac);
            icyBlueStats.framesBetweenLookFocusChange = (int)Mathf.Lerp(20, frzStats.framesBetweenLookFocusChange, sizeFac);

            icyBlueTemp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
            icyBlueTemp.waterPathingResistance = Mathf.Lerp(20, frzTemp.waterPathingResistance, sizeFac);            

            icyBlueTemp.dangerousToPlayer = icyBlueStats.danger;
            icyBlueTemp.doPreBakedPathing = false;
            icyBlueTemp.requireAImap = true;
            icyBlueTemp.wormGrassImmune = false;
            icyBlueTemp.hibernateOffScreen = false;
            icyBlueTemp.usesCreatureHoles = true;
            icyBlueTemp.usesNPCTransportation = true;
            icyBlueTemp.usesRegionTransportation = false;
            icyBlueTemp.shortcutSegments = 3;
            icyBlueTemp.roamBetweenRoomsChance = 0.07f;
            icyBlueStats.shakePrey = 100;


            return icyBlueTemp;
        }
        if (lizardType == HailstormEnums.GorditoGreenie)
        {
            CreatureTemplate gorditoTemp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams gorditoStats = (gorditoTemp.breedParameters as LizardBreedParams)!;

            //----Basic Info----//
            gorditoTemp.type = lizardType;
            gorditoTemp.name = "Gordito Greenie Lizard";
            gorditoTemp.meatPoints = 16;
            gorditoStats.standardColor = new HSLColor(135 / 360f, 0.45f, 0.5f).rgb; // A desaturated minty green.
            gorditoStats.tamingDifficulty = 0;
            gorditoStats.tongue = false;
            gorditoStats.aggressionCurveExponent = 0.1f;
            gorditoStats.danger = 0.8f;
            gorditoTemp.dangerousToPlayer = gorditoStats.danger;
            //----HP and Resistances----//
            gorditoTemp.baseDamageResistance = 30; // Effective HP.
            gorditoTemp.instantDeathDamageLimit = gorditoTemp.baseDamageResistance;
            gorditoTemp.damageRestistances[Creature.DamageType.Bite.index, 0] = 10;
            gorditoTemp.damageRestistances[Creature.DamageType.Explosion.index, 0] = 4;
            gorditoTemp.damageRestistances[Creature.DamageType.Blunt.index, 0] = 3;
            gorditoTemp.baseStunResistance = 100f;
            gorditoTemp.BlizzardAdapted = true;
            gorditoTemp.BlizzardWanderer = true;
            gorditoTemp.wormGrassImmune = true;
            //----Vision----//
            gorditoTemp.visualRadius = 1300f;
            gorditoTemp.waterVision = 0.25f;
            gorditoTemp.throughSurfaceVision = 1f;
            gorditoStats.perfectVisionAngle = 0.7f;
            gorditoStats.periferalVisionAngle = 0.1f;
            //----Body----//
            gorditoTemp.bodySize = 3;
            gorditoStats.bodyMass = 15f;
            gorditoStats.bodySizeFac = 1.8f;
            gorditoStats.bodyRadFac = 1f;
            gorditoStats.bodyStiffnes = 0.75f;
            gorditoStats.floorLeverage = 12f;
            gorditoStats.maxMusclePower = 24f;
            gorditoStats.wiggleSpeed = 0f;
            gorditoStats.wiggleDelay = 70;
            //----Movement----//
            gorditoStats.idleCounterSubtractWhenCloseToIdlePos = 0;
            gorditoStats.baseSpeed = 4f;
            gorditoStats.terrainSpeeds[1] = new(1f, 1f, 0.5f, 3f);
            gorditoStats.terrainSpeeds[2] = new(0.9f, 1f, 1f, 1.1f);
            gorditoStats.swimSpeed = 0.6f;
            gorditoTemp.waterPathingResistance = 10f;
            gorditoTemp.requireAImap = true;
            gorditoTemp.doPreBakedPathing = false;
            gorditoTemp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
            gorditoTemp.offScreenSpeed = 0.1f;
            gorditoTemp.roamInRoomChance = 0.01f;
            gorditoTemp.roamBetweenRoomsChance = -1;
            gorditoTemp.usesNPCTransportation = false;
            gorditoTemp.shortcutSegments = 6;
            //----Bites----//
            gorditoStats.biteDelay = 20;
            gorditoStats.biteInFront = 40f;
            gorditoStats.biteRadBonus = 0;
            gorditoStats.biteHomingSpeed = 0.8f;
            gorditoStats.biteChance = 0.5f;
            gorditoStats.attemptBiteRadius = 120f;
            gorditoStats.getFreeBiteChance = 1;
            gorditoStats.biteDamage = 5;
            gorditoStats.biteDamageChance = 1;
            gorditoStats.biteDominance = 1;
            //----Lunging---//
            gorditoStats.loungeTendensy = 1;
            gorditoStats.findLoungeDirection = 1f;
            gorditoStats.preLoungeCrouch = 50;
            gorditoStats.preLoungeCrouchMovement = -0.33f;
            gorditoStats.loungeDistance = 510f;
            gorditoStats.loungeSpeed = 1f;
            gorditoStats.loungeMaximumFrames = 120;
            gorditoStats.loungePropulsionFrames = 20;
            gorditoStats.loungeJumpyness = 2f;
            gorditoStats.loungeDelay = 320;
            gorditoStats.riskOfDoubleLoungeDelay = 0;
            gorditoStats.postLoungeStun = 80;
            gorditoStats.canExitLoungeWarmUp = false;
            gorditoStats.canExitLounge = false;
            //----Limbs----//
            gorditoStats.limbSize = 0.5f;
            gorditoStats.limbThickness = 2.5f;
            gorditoStats.stepLength = 0.8f;
            gorditoStats.liftFeet = 1.2f;
            gorditoStats.feetDown = 0.6f;
            gorditoStats.noGripSpeed = 0;
            gorditoStats.limbSpeed = 2f;
            gorditoStats.limbQuickness = 0.2f;
            gorditoStats.limbGripDelay = 2;
            gorditoStats.legPairDisplacement = 1f;
            gorditoStats.walkBob = 0.5f;
            gorditoStats.regainFootingCounter = 30;
            //----Tail----//
            gorditoStats.tailSegments = 3;
            gorditoStats.tailStiffness = 400f;
            gorditoStats.tailStiffnessDecline = 0.05f;
            gorditoStats.tailLengthFactor = 1;
            gorditoStats.tailColorationStart = 0.9f;
            gorditoStats.tailColorationExponent = 0.05f;
            //----Head----//
            gorditoStats.headShieldAngle = 200f;
            gorditoStats.headSize = 1.05f;
            gorditoStats.neckStiffness = 1.2f;
            gorditoStats.jawOpenAngle = 25f;
            gorditoStats.jawOpenLowerJawFac = 0.5f;
            gorditoStats.jawOpenMoveJawsApart = 10f;
            gorditoStats.headGraphics = new int[5] { 1, 1, 1, 1, 1 };
            gorditoStats.framesBetweenLookFocusChange = 240;

            return gorditoTemp;
        }
        if (lizardType == CreatureTemplate.Type.Salamander)
        {
            CreatureTemplate lotl = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, null, null);
            lotl.damageRestistances[HailstormEnums.Cold.index, 0] = 1.25f;
            lotl.damageRestistances[HailstormEnums.Cold.index, 1] = 1.5f;
            lotl.damageRestistances[HailstormEnums.Heat.index, 0] = 1.25f;
            lotl.damageRestistances[HailstormEnums.Heat.index, 1] = 1.5f;
            return lotl;
        }

        return orig(lizardType, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }
    public static CreatureTemplate EelTemperatureResistances(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate salamanderTemplate)
    {
        if (type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
        {
            CreatureTemplate eel = orig(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, lizardAncestor, salamanderTemplate);
            eel.damageRestistances[HailstormEnums.Cold.index, 0] = 1.25f;
            eel.damageRestistances[HailstormEnums.Cold.index, 1] = 1.5f;
            eel.damageRestistances[HailstormEnums.Heat.index, 0] = 1.25f;
            eel.damageRestistances[HailstormEnums.Heat.index, 1] = 1.5f;
            return eel;
        }

        return orig(type, lizardAncestor, salamanderTemplate);
    }

    public static void HailstormLizConstructors(On.Lizard.orig_ctor orig, Lizard liz, AbstractCreature absLiz, World world)
    {
        orig(liz, absLiz, world);

        if (!LizardData.TryGetValue(liz, out _))
        {
            LizardData.Add(liz, new LizardInfo(liz));
        }

        if (liz is not null && LizardData.TryGetValue(liz, out LizardInfo lI))
        {
            if (liz.LizardState is ColdLizState lS)
            {
                lS.spitWindup = 0;
                if (lS.Freezer)
                {
                    Random.State state = Random.state;
                    Random.InitState(absLiz.ID.RandomSeed);
                    liz.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(220f / 360f, 40 / 360f, 0.15f), 0.55f, Custom.ClampedRandomVariation(0.75f, 0.05f, 0.2f));
                    lS.crystalSprite = Random.Range(0, 6);
                    lS.chillAuraRad = 150;
                    Random.state = state;

                }
                else if (lS.IcyBlue)
                {
                    Random.State state = Random.state;
                    Random.InitState(absLiz.ID.RandomSeed);

                    LizardBreedParams icyBlueStats = liz.lizardParams;
                    LizardBreedParams frzStats = freezerTemplate.breedParameters as LizardBreedParams;

                    float sizeMult = Random.Range(1, 1.2f);
                    float sizeFac = Mathf.InverseLerp(0.9f, 1.3f, sizeMult);
                    bool menacing = sizeMult > 1.15f;

                    liz.LizardState.meatLeft =
                        sizeMult > 1.15f ? 8 :
                        sizeMult > 1.10f ? 7 :
                        sizeMult > 1.05f ? 6 : 5;

                    icyBlueStats.tamingDifficulty =
                        sizeMult > 1.15f ? 8.00f :
                        sizeMult > 1.10f ? 6.75f :
                        sizeMult > 1.05f ? 5.50f : 4.25f;

                    icyBlueStats.bodyMass = sizeMult + 0.4f;
                    if (liz.bodyChunks is not null)
                    {
                        for (int b = 0; b < liz.bodyChunks.Length; b++)
                        {
                            liz.bodyChunks[b].mass = icyBlueStats.bodyMass / 3f;
                        }
                    }

                    icyBlueStats.bodySizeFac = Mathf.Lerp(0.8f, 1.1f, Mathf.InverseLerp(1, 1.2f, sizeMult));
                    icyBlueStats.bodyRadFac = 1 / Mathf.Pow(sizeMult, 2);
                    icyBlueStats.bodyStiffnes = Mathf.Lerp(0, frzStats.bodyStiffnes, sizeFac);
                    icyBlueStats.floorLeverage = Mathf.Lerp(1, frzStats.floorLeverage, sizeFac);
                    icyBlueStats.maxMusclePower = Mathf.Lerp(2, frzStats.maxMusclePower, sizeFac);
                    icyBlueStats.wiggleSpeed = Mathf.Lerp(1, frzStats.wiggleSpeed, sizeFac);
                    icyBlueStats.wiggleDelay = (int)Mathf.Lerp(15, frzStats.wiggleDelay, sizeFac);
                    icyBlueStats.swimSpeed = Mathf.Lerp(0.35f, frzStats.swimSpeed, sizeFac);
                    icyBlueStats.idleCounterSubtractWhenCloseToIdlePos = 0;
                    icyBlueStats.danger = Mathf.Lerp(0.4f, frzStats.danger, sizeFac);
                    icyBlueStats.aggressionCurveExponent = Mathf.Lerp(0.925f, frzStats.aggressionCurveExponent, sizeFac);

                    icyBlueStats.baseSpeed =
                        menacing ? 3.6f : Mathf.Lerp(3.2f, 3.6f, Mathf.InverseLerp(1, 1.2f, sizeMult));

                    icyBlueStats.biteRadBonus = Mathf.Lerp(0, frzStats.biteRadBonus, sizeFac);
                    icyBlueStats.biteHomingSpeed = Mathf.Lerp(1.4f, frzStats.biteHomingSpeed, sizeFac);
                    icyBlueStats.biteChance = Mathf.Lerp(0.4f, frzStats.biteChance, sizeFac);
                    icyBlueStats.attemptBiteRadius = Mathf.Lerp(90f, frzStats.attemptBiteRadius, sizeFac);
                    icyBlueStats.getFreeBiteChance = Mathf.Lerp(0.5f, frzStats.getFreeBiteChance, sizeFac);
                    icyBlueStats.biteDamage = Mathf.Lerp(0.7f, frzStats.biteDamage, sizeFac);
                    icyBlueStats.biteDominance = Mathf.Lerp(0.1f, frzStats.biteDominance, sizeFac);

                    icyBlueStats.canExitLoungeWarmUp = true;
                    icyBlueStats.canExitLounge = false;
                    icyBlueStats.preLoungeCrouch = (int)Mathf.Lerp(35, frzStats.preLoungeCrouch, sizeFac);
                    icyBlueStats.preLoungeCrouchMovement = Mathf.Lerp(-0.3f, frzStats.preLoungeCrouchMovement, sizeFac);
                    icyBlueStats.loungeSpeed =
                        menacing ? 3 : Mathf.Lerp(2.5f, frzStats.loungeSpeed, sizeFac);
                    icyBlueStats.loungeJumpyness = Mathf.Lerp(1, frzStats.loungeJumpyness, sizeFac);
                    icyBlueStats.loungeDelay = (int)Mathf.Lerp(310, frzStats.loungeDelay, sizeFac);
                    icyBlueStats.postLoungeStun = (int)Mathf.Lerp(20, frzStats.postLoungeStun, sizeFac);
                    icyBlueStats.loungeTendensy =
                        menacing ? 0.9f : Mathf.Lerp(0.4f, frzStats.loungeTendensy, sizeFac);

                    icyBlueStats.perfectVisionAngle = Mathf.Lerp(0.8888f, frzStats.perfectVisionAngle, sizeFac);
                    icyBlueStats.periferalVisionAngle = Mathf.Lerp(0.0833f, frzStats.periferalVisionAngle, sizeFac);

                    icyBlueStats.limbSize = Mathf.Lerp(0.9f, frzStats.limbSize, sizeFac);
                    icyBlueStats.limbThickness = Mathf.Lerp(1, frzStats.limbThickness, sizeFac);
                    icyBlueStats.stepLength = Mathf.Lerp(0.4f, frzStats.stepLength, sizeFac);
                    icyBlueStats.liftFeet = Mathf.Lerp(0, frzStats.liftFeet, sizeFac);
                    icyBlueStats.feetDown = Mathf.Lerp(0, frzStats.feetDown, sizeFac);
                    icyBlueStats.noGripSpeed = Mathf.Lerp(0.2f, frzStats.noGripSpeed, sizeFac);
                    icyBlueStats.limbSpeed = Mathf.Lerp(6, frzStats.limbSpeed, sizeFac);
                    icyBlueStats.limbQuickness = Mathf.Lerp(0.6f, frzStats.limbQuickness, sizeFac);
                    icyBlueStats.walkBob = Mathf.Lerp(0.4f, frzStats.walkBob, sizeFac);
                    icyBlueStats.regainFootingCounter = (int)Mathf.Lerp(4, frzStats.regainFootingCounter, sizeFac);

                    icyBlueStats.tailColorationStart = Mathf.Lerp(0.2f, 0.75f, Mathf.InverseLerp(1, 1.2f, sizeMult));
                    icyBlueStats.tailColorationExponent = 0.5f;

                    icyBlueStats.headShieldAngle = Mathf.Lerp(108, frzStats.headShieldAngle, sizeFac);
                    icyBlueStats.headSize = Mathf.Lerp(0.9f, 1, sizeFac);
                    icyBlueStats.neckStiffness = Mathf.Lerp(0, frzStats.neckStiffness, sizeFac);
                    icyBlueStats.framesBetweenLookFocusChange = (int)Mathf.Lerp(20, frzStats.framesBetweenLookFocusChange, sizeFac);

                    icyBlueStats.terrainSpeeds[3] = new(1f, 0.9f, 0.8f, 1f);
                    icyBlueStats.terrainSpeeds[4] = new(0.9f, 0.9f, 0.9f, 1f);
                    icyBlueStats.terrainSpeeds[5] = new(0.7f, 1f, 1f, 1f);

                    icyBlueStats.tongueChance = Mathf.Lerp(0.2f, 0, Mathf.InverseLerp(1, 1.2f, sizeMult));

                    liz.effectColor =
                        Custom.HSL2RGB(
                            Custom.WrappedRandomVariation(220 / 360f, 40 / 360f, 0.66f), // hue
                            Mathf.Lerp(0.7f, 0.45f, Mathf.InverseLerp(1, 1.2f, sizeMult)), // saturation
                            Mathf.Lerp(0.6f, 0.8f, Mathf.InverseLerp(1, 1.2f, sizeMult))); // lightness

                    lS.crystalSprite = Random.Range(0, 6);
                    lS.chillAuraRad = 30 + (90 * Mathf.InverseLerp(1, 1.2f, sizeMult));
                    Random.state = state;
                }
            }
            else if (lI.Gordito)
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                liz.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(140 / 360f, 30 / 360f, 0.2f), Random.Range(0.45f, 0.9f), Custom.WrappedRandomVariation(0.8f, 0.1f, 0.33f));
                Random.state = state;
                for (int b = 1; b < liz.bodyChunks.Length; b++)
                {
                    liz.bodyChunks[b].rad *= 2.5f;
                }
                liz.bodyChunkConnections[1].distance *= 0.66f;

            }
            else if (liz.Template.type == CreatureTemplate.Type.YellowLizard && (OtherCreatureChanges.IsIncanStory(world?.game) || HSRemix.YellowLizardColorsEverywhere.Value is true))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                float hue =
                    absLiz.Winterized && Random.value < 0.01 ? Custom.WrappedRandomVariation(200 / 360f, 40 / 360f, 0.66f) :
                    Random.value < 0.6f ? Custom.WrappedRandomVariation(36 / 360f, 18 / 360f, 0.66f) :
                    Custom.WrappedRandomVariation(44 / 360f, 36 / 360f, 0.66f);

                liz.effectColor = Custom.HSL2RGB(hue, 1, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                Random.state = state;
            }
            else if (liz.Template.type == CreatureTemplate.Type.RedLizard && OtherCreatureChanges.IsIncanStory(world?.game))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                liz.effectColor = Custom.HSL2RGB(Custom.ClampedRandomVariation(340 / 360f, 20 / 360f, 0.25f), Random.Range(0.7f, 0.8f), 0.4f);
                Random.state = state;
            }
            else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard && (OtherCreatureChanges.IsIncanStory(world?.game) || HSRemix.EelLizardColorsEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(absLiz, out AbsCtrInfo aI))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                float winterHue = Custom.WrappedRandomVariation(195 / 360f, 65 / 360f, 0.5f);
                liz.effectColor =
                    absLiz.Winterized && world.region is not null && world.region.name != "CC" ? Custom.HSL2RGB(winterHue, Random.Range(0.6f, 1) - Mathf.InverseLerp(13 / 24f, 13 / 12f, winterHue), Custom.ClampedRandomVariation((Random.value < 0.08f ? 0.3f : 0.7f), 0.05f, 0.2f)) : // Winter colors
                    world.region is not null && world.region.name == "OE" ? Custom.HSL2RGB(Custom.WrappedRandomVariation(225 / 360f, 95 / 360f, 1), Random.Range(0.85f, 1), Random.Range(0.3f, 0.5f)) : // Outer Expanse colors
                    Custom.HSL2RGB(Custom.WrappedRandomVariation(130 / 360f, 65 / 360f, 0.8f), Random.Range(0.7f, 1), Custom.ClampedRandomVariation(0.25f, 0.15f, 0.33f)); // Default colors

                aI.functionTimer = Random.Range(0, 101);
                Random.state = state;
            }
            else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard && (OtherCreatureChanges.IsIncanStory(world?.game) || HSRemix.StrawberryLizardColorsEverywhere.Value is true))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                float hue =
                    Random.value < 0.05f ?
                    Custom.WrappedRandomVariation(30 / 360f, 20 / 360f, 0.33f) :
                    Custom.WrappedRandomVariation(-10 / 360f, 35 / 360f, 0.25f);

                liz.effectColor = Custom.HSL2RGB(hue, Custom.ClampedRandomVariation(0.525f, 0.1f, 0.2f), Custom.ClampedRandomVariation(0.83f, 0.05f, 0.25f));
                Random.state = state;
            }
            else if (OtherCreatureChanges.IsIncanStory(world?.game))
            {
                if (absLiz.Winterized)
                {
                    if (liz.Template.type != CreatureTemplate.Type.WhiteLizard &&
                    liz.Template.type != CreatureTemplate.Type.YellowLizard &&
                    liz.Template.type != MoreSlugcatsEnums.CreatureTemplateType.EelLizard &&
                    liz.Template.type != MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
                    {
                        for (int b = 0; b < liz.bodyChunks.Length; b++)
                        {
                            liz.bodyChunks[b].rad = liz.lizardParams.bodySizeFac * liz.lizardParams.bodyRadFac * 9f;
                            liz.bodyChunks[b].mass = liz.lizardParams.bodyMass / 2.7f;
                            if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                            {
                                liz.bodyChunkConnections[b].distance =
                                    ((b != 2) ?
                                    liz.lizardParams.bodyLengthFac * ((liz.lizardParams.bodySizeFac + 1f) / 2f) :
                                    liz.lizardParams.bodyLengthFac * ((liz.lizardParams.bodySizeFac + 1f) / 2f) * (1f + liz.lizardParams.bodyStiffnes)) * 18f;
                            }
                        }
                    }

                    if (liz.Template.type == CreatureTemplate.Type.Salamander)
                    {
                        Random.State state = Random.state;
                        Random.InitState(absLiz.ID.RandomSeed);
                        liz.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.65f, 0.2f, 0.6f), 0.9f, Custom.ClampedRandomVariation(0.6f, 0.15f, 0.2f));
                        Random.state = state;
                    }
                    else if (liz.Template.type == CreatureTemplate.Type.CyanLizard)
                    {
                        Random.State state = Random.state;
                        Random.InitState(absLiz.ID.RandomSeed);
                        float hue = Custom.WrappedRandomVariation(210/360f, 50/360f, 0.6f);
                        liz.effectColor = Custom.HSL2RGB(hue, 1f, Custom.ClampedRandomVariation(hue, 0.2f, 0.3f));
                        Random.state = state;
                    }
                }
            }

            if (liz.Template.type == HailstormEnums.IcyBlue && liz.lizardParams.tongue)
            {
                liz.tongue = new LizardTongue(liz);
            }
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Special Lizard Mechanics
    public static void IcyBlueTongues(On.LizardTongue.orig_ctor orig, LizardTongue tng, Lizard liz)
    {
        orig(tng, liz);
        if (LizardData.TryGetValue(liz, out LizardInfo lI))
        {
            if (liz.Template.type == HailstormEnums.IcyBlue)
            {
                float mass1 = Mathf.InverseLerp(1.4f, 1.5f, liz.TotalMass);
                float mass2 = Mathf.InverseLerp(1.5f, 1.6f, liz.TotalMass);

                tng.range =
                    liz.TotalMass < 1.5f ?
                    Mathf.Lerp(150, 300, mass1) :
                    Mathf.Lerp(300, -150, mass2);

                tng.elasticRange =
                    liz.TotalMass < 1.5f ?
                    Mathf.Lerp(0.1f, 0.2f, mass1) :
                    Mathf.Lerp(0.2f, -0.1f, mass2);

                tng.lashOutSpeed =
                    Mathf.Lerp(26, 39, Mathf.InverseLerp(1.4f, 1.6f, liz.TotalMass));

                tng.reelInSpeed = 0f;
                tng.chunkDrag = 0.04f;
                tng.terrainDrag = 0.04f;
                tng.dragElasticity = 0.05f;
                tng.emptyElasticity = 0.07f;
                tng.involuntaryReleaseChance = 0.0033333334f;
                tng.voluntaryReleaseChance = 1f;
                tng.baseDragOnly = true;
                tng.totR = tng.range * 1.1f;
            }
        }
    }
    public static bool GorditoGreenieSquishyHead(On.Lizard.orig_HitHeadShield orig, Lizard liz, Vector2 direction)
    {
        if (liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            return false;
        }
        return orig(liz, direction);
    }
    public static bool ColdLizardArmorSpearDeflect(On.Lizard.orig_SpearStick orig, Lizard liz, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos onAppendagePos, Vector2 direction)
    {
        if (source is IceCrystal)
        {
            return true;
        }
        if (liz.LizardState is ColdLizState lS && lS.armored && (chunk.index == 1 || chunk.index == 2))
        {
            return false;
        }
        return orig(liz, source, dmg, chunk, onAppendagePos, direction);
    }
    public static void ColdLizardArmorFunctionality(On.Lizard.orig_Violence orig, Lizard liz, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType dmgType, float dmg, float bonusStun)
    {
        if (liz?.room is not null && hitChunk is not null)
        {
            if (OtherCreatureChanges.IsIncanStory(liz.room.game))
            {
                if (liz.Template.type == CreatureTemplate.Type.Salamander)
                {
                    if (dmgType == Creature.DamageType.Electric)
                    {
                        dmg *= 1.5f;
                        bonusStun *= 1.5f;
                    }
                }
                else if(liz.Template.type == CreatureTemplate.Type.YellowLizard)
                {
                    if (dmgType == Creature.DamageType.Electric)
                    {
                        dmg /= 2f;
                        bonusStun = 2f;
                    }
                }
                else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    dmg *= 0.615384f; // Base HP: 1.6 -> 2.6
                    if (dmgType == Creature.DamageType.Electric)
                    {
                        dmg /= 2f;
                        bonusStun /= 2f;
                    }
                }
                else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
                {
                    dmg *= 2/3f; // Base HP: 5 -> 7.5
                    bonusStun *= 2/3f;
                }
                else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
                {
                    dmg *= 1.5f; // Base HP: 2.4 -> 1.6
                }
            }

            if (liz.Template.type == HailstormEnums.GorditoGreenie)
            {
                liz.room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, hitChunk.pos, 1.2f, 0.6f);
                liz.turnedByRockCounter = 10;
                if (source is not null)
                {
                    liz.turnedByRockDirection = (int)Mathf.Sign(source.lastPos.x - source.pos.x);
                }
                if (dmgType == Creature.DamageType.Bite)
                {
                    dmg = Mathf.Max(0, dmg - 1);
                }

                if (directionAndMomentum.HasValue && hitChunk.index == 0)
                {
                    liz.stun += 20;
                    dmg *= 1.34f;
                    if (!liz.HitInMouth(directionAndMomentum.Value))
                    {
                        liz.room.PlaySound(SoundID.Spear_Stick_In_Creature, hitChunk.pos, 0.8f, 0.8f);
                        if (Random.value < (liz.LizardState.health >= 0.5f ? liz.LizardState.health - 1 : liz.LizardState.health))
                        {
                            liz.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
                        }
                    }
                    else
                    {
                        liz.room.PlaySound(SoundID.Spear_Stick_In_Creature, hitChunk.pos, 1.2f, 0.6f);
                        liz.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
                    }
                }
            }

            if (source?.owner is not null && source.owner is IceCrystal && hitChunk.index == 0 && directionAndMomentum.HasValue && liz.HitHeadShield(directionAndMomentum.Value) && !liz.HitInMouth(directionAndMomentum.Value))
            {
                bonusStun *= 3f; // I don't know how much this negates the stun reduction but it sure does it well.
                dmg *= 7.5f; // This, for sure, ends in a net 0.75x damage, though.
            }

            if (liz.LizardState is ColdLizState lS && (source?.owner is not null || dmgType == Creature.DamageType.Explosion) && LizardData.TryGetValue(liz, out LizardInfo lI))
            {
                bool protectedFrom =
                    source?.owner is not null &&
                    source.owner is Creature ctr &&
                    (ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard ||
                    ctr.Template.type == HailstormEnums.Freezer ||
                    ctr.Template.type == HailstormEnums.IcyBlue);

                if (hitChunk.index == 0 && directionAndMomentum.HasValue && liz.HitInMouth(directionAndMomentum.Value))
                {
                    lS.iceBreath = (int)(dmg * (liz.dead ? 8 : 24));
                    lS.breathDir = -directionAndMomentum.Value;
                    dmg *= 0.8f; // 1.5x damage -> 1.2x damage
                }
                else if (lS.crystals is not null && !lS.crystals.All(intact => !intact) && hitChunk.index > 0 && !protectedFrom && (dmg >= 0.25f || (source?.owner is not null && source.owner is StowawayBug))) // Allows the back armor of Freezer Lizards to block damage and nullify stun.
                {
                    // Armor damage
                    int breakAmount = (int)Mathf.Max(dmg, 1);
                    if (dmgType == Creature.DamageType.Explosion || (source?.owner is not null && source.owner is ExplosiveSpear))
                    {
                        breakAmount = 3;
                    }

                    if (dmgType == Creature.DamageType.Explosion || (source?.owner is not null && source.owner is not IceCrystal))
                    {
                        dmg *= 0.01f;

                        if (bonusStun > 10)
                        {
                            liz.LoseAllGrasps();
                        }

                        if (source?.owner is not null && source.owner is Player self && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                        {
                            bonusStun *= 0.33f;
                        }
                        else bonusStun *= 0.01f;
                    }

                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < liz.room.updateList.Count; i++)
                    {
                        if (liz.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = liz.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    LizardGraphics lGraphics = liz.graphicsModule as LizardGraphics;
                    if (lGraphics is null)
                    {
                        return;
                    }     

                    if (lS.Freezer)
                    {
                        Color lizColor2 = SecondaryFreezerColor(lGraphics);

                        for (int i = Random.Range(0, lS.crystals.Length - 1); breakAmount > 0 && !lS.crystals.All(intact => !intact);)
                        {
                            if (lS.crystals[i])
                            {
                                lS.crystals[i] = false;
                                breakAmount--;
                                if (Random.value < 0.075f && !(source?.owner is not null && source.owner is Spear spr && spr.bugSpear))
                                {
                                    Vector2 crystalPos = hitChunk.pos + new Vector2(0, 10);
                                    ArmorIceSpikes.DropCrystals(liz, crystalPos, lS.crystalSprite);
                                }
                                else
                                {
                                    liz.room.PlaySound(SoundID.Coral_Circuit_Break, hitChunk.pos, 1.25f, 1.5f);
                                }

                                for (int j = 0; j < 12; j++)
                                {
                                    if (j % 2 == 1)
                                    {
                                        liz.room.AddObject(new HailstormSnowflake(hitChunk.pos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizColor2));
                                        liz.room.AddObject(new FreezerMist(hitChunk.pos, Custom.RNV() * Random.value * 10f, liz.effectColor, lizColor2, 0.2f, null, smallInsects, false));
                                    }
                                    else
                                    {
                                        liz.room.AddObject(new PuffBallSkin(hitChunk.pos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizColor2));
                                    }
                                }
                            }
                            if (breakAmount > 0)
                            {
                                if (i == lS.crystals.Length - 1)
                                {
                                    i -= i;
                                }
                                else i++;
                            }
                        }
                    }
                    else if (lS.IcyBlue)
                    {
                        liz.room.PlaySound(SoundID.Coral_Circuit_Break, hitChunk.pos, 1.25f, 1.5f);

                        Color lizColor2 = SecondaryIcyBlueColor(lGraphics);

                        for (int i = 0; i < lS.crystals.Length; i++)
                        {
                            if (!lS.crystals[i]) continue;
                            lS.crystals[i] = false;

                            for (int j = 0; j < 6; j++)
                            {
                                if (j % 2 == 1)
                                {
                                    liz.room.AddObject(new HailstormSnowflake(hitChunk.pos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizColor2));
                                    liz.room.AddObject(new FreezerMist(hitChunk.pos, Custom.RNV() * Random.value * 10f, liz.effectColor, lizColor2, 0.2f, null, smallInsects, false));
                                }
                                else
                                {
                                    liz.room.AddObject(new PuffBallSkin(hitChunk.pos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizColor2));
                                }
                            }
                        }
                    }
                }
            }
        }
        orig(liz, source, directionAndMomentum, hitChunk, onAppendagePos, dmgType, dmg, bonusStun);
    }

    public static void HailstormLizardUpdate(On.Lizard.orig_Update orig, Lizard liz, bool eu)
    {
        orig(liz, eu);
        if (liz?.room is null || liz.slatedForDeletetion || !LizardData.TryGetValue(liz, out LizardInfo lI))
        {
            return;
        }

        if (!(lI.Gordito || liz.LizardState is ColdLizState))
        {
            return;
        }

        if (lI.Gordito)
        {
            if (liz.animation == Lizard.Animation.Lounge && liz.timeInAnimation > 10 && Mathf.Abs(liz.mainBodyChunk.vel.x) < 3)
            {
                float signFloat =
                    liz.mainBodyChunk.vel.x != 0 ?
                    liz.mainBodyChunk.vel.x : (liz.bodyChunks[0].pos - liz.bodyChunks[1].pos).normalized.x;

                liz.mainBodyChunk.vel.x += 3 * Mathf.Sign(signFloat);
            }
            if (liz.animation != Lizard.Animation.Lounge && lI.bounceLungeUsed)
            {
                lI.bounceLungeUsed = false;
            }
        }
        else if (liz.LizardState is ColdLizState lS)
        {
            if (liz.HypothermiaExposure > 0.1f || liz.Hypothermia > 0.1f)
            {
                liz.HypothermiaExposure = 0;
                liz.Hypothermia = 0;                
            }
            if (liz.grabbedBy.Count > 0)
            {
                foreach (AbstractCreature absCtr in liz.room.abstractRoom.creatures)
                {
                    Creature ctr = absCtr.realizedCreature;
                    if (ctr is null || ctr is Player || ctr == liz || ctr.grasps is null || ctr.Template.type == HailstormEnums.Chillipede)
                    {
                        continue; // Stops the code for the current AbstractCreature, and continues on to the next one.
                    }

                    for (int i = 0; i < ctr.grasps.Length; i++)
                    {
                        Creature.Grasp grasp = ctr.grasps[i];
                        if (grasp is null || grasp.grabbed is null || grasp.grabbed != liz)
                        {
                            continue;
                        }

                        if (ctr is Leech || ctr is Spider)
                        {
                            grasp.Release();
                            ctr.Stun((int)Mathf.Lerp(0, 400, Mathf.InverseLerp(1.2f, 1.7f, liz.TotalMass)));
                        }
                        // ^ Stuns leeches and spiders epically if the lizard is grabbed by them. Stun time varies WILDLY with Icy Blue Lizards, but is always max with Freezers.                                
                        else if ((liz.Template.type == HailstormEnums.Freezer && grasp.chunkGrabbed == 0) || ((grasp.chunkGrabbed == 1 || grasp.chunkGrabbed == 2) && lS.armored))
                        {
                            if (liz.Template.type == HailstormEnums.Freezer && grasp.chunkGrabbed == 0)
                            {
                                liz.room.PlaySound(SoundID.Lizard_Jaws_Bite_Do_Damage, liz.bodyChunks[0].pos, 1.25f, 1);
                            }
                            ctr.Violence(liz.bodyChunks[0], -liz.bodyChunks[0].vel, ctr.firstChunk, null, HailstormEnums.Cold, 0.15f, 25);
                            grasp.Release();
                        } // ^ Briefly stuns grabber and makes them let go if they toucha da lizor's armor.    
                    }
                }
            }

            if (lS.crystals is not null && lS.crystals.All(intact => !intact) && lS.armored)
            {
                lS.armored = false;
            }

            if (liz.grasps is not null && liz.grasps[0]?.grabbed is not null && liz.grasps[0].grabbed is Creature grabbed && grabbed is not Player && grabbed.Template.type != HailstormEnums.Chillipede && grabbed.State is HealthState hs)
            {
                float frostbite = (lS.Freezer ? 0.0045f : 0.002f) / grabbed.Template.baseDamageResistance;
                if (grabbed is Centipede cnt)
                {
                    frostbite *= Mathf.Lerp(1.3f, 0.1f, Mathf.Pow((cnt.AquaCenti? cnt.size/2 : cnt.size), 0.5f));
                }
                hs.health -= frostbite;

                if (grabbed.killTag is null)
                {
                    grabbed.SetKillTag(liz.abstractCreature);
                }
            }

            if (lS.chillAuraRad > 75)
            {
                if (lS.chillAura is null)
                {
                    lS.chillAura = new LightSource(liz.mainBodyChunk.pos, false, liz.effectColor, liz);
                    lS.chillAura.affectedByPaletteDarkness = 0.2f;
                    lS.chillAura.requireUpKeep = true;
                    liz.room.AddObject(lS.chillAura);
                }
                else
                {
                    lS.chillAura.stayAlive = true;
                    lS.chillAura.setPos = liz.mainBodyChunk.pos;
                    lS.chillAura.setRad = lS.chillAuraRad;
                    if (lS.chillAura.slatedForDeletetion || lS.chillAura.room != liz.room)
                    {
                        lS.chillAura = null;
                    }
                }
            }
                   

            if (lS.IcyBlue)
            {
                if (liz.tongue?.attached is not null && liz.tongue.attached.owner is Creature && liz.loungeDelay > 1)
                {
                    liz.loungeDelay--;
                }
            }
            else if (lS.Freezer)
            {
                if (lS.spitCooldown > 0)
                {
                    lS.spitCooldown--;
                }

                try
                {
                    if (lS.spitCooldown == 0)
                    {
                        Tracker.CreatureRepresentation mostAttractivePrey = liz.AI.preyTracker.MostAttractivePrey;
                        if (mostAttractivePrey is not null && WantsToSpit(liz, mostAttractivePrey) && lS.iceBreath == 0 && (mostAttractivePrey.VisualContact || mostAttractivePrey.TicksSinceSeen < 12 || lS.spitWindup >= 80))
                        {
                            ItsSpittinTime(liz, lS, mostAttractivePrey);
                        }
                        else if (lS.spitWindup != 0)
                        {
                            if (Random.value < lS.spitGiveUpChance)
                            {
                                lS.spitGiveUpChance = 0;
                                lS.spitCooldown = Random.Range(280, 360);
                            }
                            else
                            {
                                lS.spitGiveUpChance += 0.15f;
                            }
                            lS.spitWindup = 0;
                            lS.spitAimChunk = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("[Hailstorm] Freezer Lizard spit AI is bugging out; please report this: " + e);
                }

                try
                {
                    if (lS.iceBreath > 0)
                    {
                        lS.iceBreath--;
                        InsectCoordinator smallInsects = null;
                        if (liz.room is not null)
                        {
                            for (int i = 0; i < liz.room.updateList.Count; i++)
                            {
                                if (liz.room.updateList[i] is InsectCoordinator)
                                {
                                    smallInsects = liz.room.updateList[i] as InsectCoordinator;
                                    break;
                                }
                            }
                        }
                        Vector2 lizHeadAngle = (liz.bodyChunks[0].pos - liz.bodyChunks[1].pos).normalized;
                        float breathDir = Custom.VecToDeg(lizHeadAngle);
                        Vector2 finalBreathDir = Custom.DegToVec(breathDir + Random.Range(-15, 15));

                        float breathSize =
                            (liz.dead ? 0.1f : 0.15f) * Mathf.Lerp(1, 2.5f, Mathf.InverseLerp(0, 60, lS.iceBreath));

                        if (lS.iceBreath % 2 != 1)
                        {
                            liz.room.AddObject(new FreezerMist(liz.DangerPos, finalBreathDir * (liz.dead ? Random.Range(15f, 19f) : Random.Range(18f, 23f)), liz.effectColor, SecondaryFreezerColor(liz.graphicsModule as LizardGraphics), breathSize, liz.abstractCreature, smallInsects, true));
                        }                        
                    }
                }
                catch (Exception e) 
                {
                    Debug.Log("[Hailstorm] Freezer Lizard ice breath is screwing up???? Report this, please:" + e);
                }                          
            }                
   
        }
    }
    public static void GorditoGreenieSlide(On.Lizard.orig_Act orig, Lizard liz)
    {
        orig(liz);
        if (liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            if (liz.animation == Lizard.Animation.Lounge && liz.bodyChunks is not null)
            {
                foreach (BodyChunk chunk in liz.bodyChunks)
                {
                    if (chunk.contactPoint.y == -1) return;
                }
                liz.timeInAnimation--; // This timer usually ticks up when an animation is in progress. This line cancels that out, effectively pausing the timer.
            }
        }
    }
    public static void GorditoEnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard liz, Lizard.Animation anim, bool forceAnimChange)
    {
        orig(liz, anim, forceAnimChange);
        if (liz.Template.type == HailstormEnums.GorditoGreenie && LizardData.TryGetValue(liz, out LizardInfo lI))
        {
            if (anim == Lizard.Animation.Lounge)
            {
                if (Random.value < 0.25f)
                {
                    lI.bounceLungeUsed = true;
                    foreach (BodyChunk chunk in liz.bodyChunks)
                    {
                        chunk.vel.y += 15;
                        chunk.vel.x += liz.loungeDir.x * liz.lizardParams.loungeSpeed / (chunk.index + 1) / 2f;
                    }
                }
            }
        }
    }
    public static void GorditoGreenieCollision(On.Lizard.orig_Collide orig, Lizard liz, PhysicalObject victim, int myChunk, int otherChunk)
    {
        if (liz is not null && liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            if (victim is not null &&
                victim is Creature ctr &&
                CWT.CreatureData.TryGetValue(liz, out CreatureInfo lI) &&
                CWT.CreatureData.TryGetValue(ctr, out CreatureInfo vI) &&
                CanCRONCH(liz, ctr, lI.impactCooldown, vI.impactCooldown))
            {
                liz.stun = 20;
                lI.impactCooldown = 40;
                vI.impactCooldown = 40;
                float dmg = 1;
                int stun = 45;
                if (liz.animation != Lizard.Animation.Lounge)
                {
                    if (liz.mainBodyChunk.vel.y <= -30f)
                    {
                        dmg *= 5;
                        stun *= 5;
                    }
                    else if (liz.mainBodyChunk.vel.y <= -25f)
                    {
                        dmg *= 4;
                        stun *= 4;
                    }
                    else if (liz.mainBodyChunk.vel.y <= -20f)
                    {
                        dmg *= 3;
                        stun *= 3;
                    }
                    else if (liz.mainBodyChunk.vel.y <= -15f)
                    {
                        dmg *= 2;
                        stun *= 2;
                    }
                }
                if (ctr is Player ||
                    ctr.Template.type == CreatureTemplate.Type.RedLizard ||
                    ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard ||
                    ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard ||
                    ctr.TotalMass * 2 > liz.TotalMass)
                {
                    dmg /= 2f;
                }

                liz.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, liz.bodyChunks[myChunk], false, 1.4f, 1.1f);
                liz.room.PlaySound(SoundID.Lizard_Heavy_Terrain_Impact, liz.bodyChunks[myChunk], false, 1.4f, 1.1f);
                if (!ctr.State.dead)
                {
                    liz.room.PlaySound(SoundID.Rock_Hit_Wall, liz.bodyChunks[myChunk], false, 1.5f, 0.5f - (dmg / 12.5f));
                    if (ctr.State is HealthState HS && HS.ClampedHealth == 0)
                    {
                        liz.room.PlaySound(SoundID.Spear_Stick_In_Creature, liz.bodyChunks[myChunk], false, 1.7f, 0.85f);
                    }
                }
                ctr.Violence(liz.bodyChunks[myChunk], liz.bodyChunks[myChunk].vel, ctr.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, dmg, stun);

            }
        }
        orig(liz, victim, myChunk, otherChunk);
    }
    public static void GorditoGreenieGroundImpact(On.Lizard.orig_TerrainImpact orig, Lizard liz, int myChunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (firstContact && speed > 5 && liz is not null && liz.Template.type == HailstormEnums.GorditoGreenie && liz.animation == Lizard.Animation.Lounge && LizardData.TryGetValue(liz, out LizardInfo lI))
        {
            foreach (BodyChunk chunk in liz.bodyChunks)
            {
                if (direction.x != 0 && lI.bounceLungeUsed) // Bounces liz off of walls during its extra-bouncy lunges
                {
                    float pitch = Random.Range(0.1f, 0.5f);
                    liz.room.PlaySound(SoundID.Rock_Hit_Wall, chunk, false, 1 + pitch, pitch);
                    chunk.vel.x += -1.2f;
                }
                if (Mathf.Abs(chunk.vel.x) < 5) // Keeps liz moving sideways at all times during lunges
                {
                    chunk.vel.x = 5 * Mathf.Sign(chunk.vel.x);
                }
                
                if (direction.y != 0 && Mathf.Abs(chunk.vel.y) < (direction.y > 0? 3 : 5)) // Bounces liz off of floors and ceilings
                {
                    chunk.vel.y = 5 * Mathf.Sign(chunk.vel.y);
                }
                
                if (direction.y == -1)
                {
                    float pitch = Random.Range(0.1f, 0.5f);
                    liz.room.PlaySound(SoundID.Rock_Hit_Wall, chunk, false, 1 + pitch, pitch);

                    chunk.vel.y *= -1.4f;
                    if (chunk.vel.y > (lI.bounceLungeUsed ? 3 : 10))
                    {
                        chunk.vel.y = 15;
                    }
                }
            }
        }
        orig(liz, myChunk, direction, speed, firstContact);
    }

    public static void HailstormLizStun(On.Lizard.orig_Stun orig, Lizard liz, int stun)
    {
        if (liz.Template.type == HailstormEnums.GorditoGreenie)
        {
            stun = Mathf.Min(20, stun);
        }
        if (liz.Template.type == HailstormEnums.Freezer)
        {
            if (stun > 80) stun = 80;
            if (liz.LizardState.health < 0.5f)
            {
                stun = (int)(stun / (1 + Mathf.InverseLerp(0.75f, 0, liz.LizardState.health) * 1.5f));
            }
        }
        orig(liz, stun);
    }

    //--------------------------------------

    public static bool WantsToSpit(Lizard liz, Tracker.CreatureRepresentation mostAttractivePrey)
    {
        if (liz.dead || liz.Stunned || liz.Submersion > 0 ||
            liz.animation == Lizard.Animation.PrepareToLounge ||
            liz.animation == Lizard.Animation.Lounge ||
            liz.animation == Lizard.Animation.ShakePrey ||
            liz.AI.behavior == LizardAI.Behavior.Flee ||
            liz.AI.behavior == LizardAI.Behavior.EscapeRain ||
            liz.AI.behavior == LizardAI.Behavior.ReturnPrey)
        {
            return false;
        }

        if (mostAttractivePrey?.representedCreature?.Room?.realizedRoom == null ||
            mostAttractivePrey.representedCreature.state.dead ||
            mostAttractivePrey.representedCreature.HypothermiaImmune ||            
            mostAttractivePrey.representedCreature.Room.realizedRoom != liz.room)
        {
            return false;
        }


        Creature target = mostAttractivePrey.representedCreature.realizedCreature;
        float distance = Custom.Dist(liz.DangerPos, target.DangerPos);

        if (target is Player || target is Scavenger || target is BigSpider || target is EggBug || target is DaddyLongLegs || target is MirosBird)
        {
            return true;
        }
        if ((target is Vulture vul && !vul.IsMiros) || (target is Centipede cnt2 && cnt2.size >= 0.65f && !cnt2.AquaCenti) ||
            (target is Lizard liz2 && (liz2.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard || liz2.Template.type == CreatureTemplate.Type.RedLizard)))
        {
            return distance > 100;
        }
        if (target is DropBug || target is StowawayBug || target is Cicada || target is NeedleWorm || target is BigNeedleWorm)
        {
            bool preyHighEnough = Mathf.Abs(target.DangerPos.y - liz.DangerPos.y) > 100;

            return distance > 125 && preyHighEnough;           
        }

        return false;
    }

    public static void ItsSpittinTime(Lizard liz, ColdLizState lS, Tracker.CreatureRepresentation mostAttractivePrey)
    {
        if (mostAttractivePrey.representedCreature.realizedCreature?.bodyChunks is not null && lS.spitAimChunk is null)
        {
            lS.spitAimChunk = mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[Random.Range(0, mostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length - 1)];
        }
        lS.spitWindup++;
        liz.bodyWiggleCounter = lS.spitWindup / 2;
        (liz.graphicsModule as LizardGraphics).head.vel +=
            Custom.RNV() * ((lS.spitWindup < 80)?
            Mathf.InverseLerp(0, 80, lS.spitWindup) :
            Mathf.InverseLerp(100, 80, lS.spitWindup));    
        
        liz.JawOpen =
            (lS.spitWindup < 80) ?
            Mathf.InverseLerp(75, 80, lS.spitWindup) :
            Mathf.InverseLerp(105, 90, lS.spitWindup);

        liz.AI.runSpeed = Mathf.Lerp(liz.AI.runSpeed, 0.5f, 0.05f);        

        if (lS.spitWindup == 80 && lS.spitAimChunk is not null)
        {            
            Vector2 val1 = liz.bodyChunks[0].pos + Custom.DirVec(liz.bodyChunks[1].pos, liz.bodyChunks[0].pos) * 10f;
            Vector2 val2 = Custom.DirVec(val1, lS.spitAimChunk.pos);
            if (Vector2.Dot(val2, Custom.DirVec(liz.bodyChunks[1].pos, liz.bodyChunks[0].pos)) > 0.3f || liz.safariControlled)
            {
                liz.room.PlaySound(SoundID.Red_Lizard_Spit, val1, 1.5f, 1.25f);
                liz.room.AddObject(new FreezerSpit(val1, val2 * Mathf.Lerp(40, 50, Mathf.InverseLerp(500, 900, Custom.Dist(liz.DangerPos, val2))), liz));
                liz.bodyChunks[2].pos -= val2 * 8f;
                liz.bodyChunks[1].pos -= val2 * 4f;
                liz.bodyChunks[2].vel -= val2 * 2f;
                liz.bodyChunks[1].vel -= val2 * 1f;
                lS.spitUsed = true;
            }
        }
        else if (lS.spitWindup >= 80 && !lS.spitUsed)
        {
            lS.spitWindup = 0;
            lS.spitAimChunk = null;
        }
        else if (lS.spitWindup >= 110)
        {
            lS.spitUsed = false;
            lS.spitCooldown = Random.Range(1400, 1800);
            lS.spitWindup = 0;
            lS.spitAimChunk = null;
        }
    }

    public static bool CanCRONCH(Lizard liz, Creature ctr, float collTimer1, float collTimer2)
    {
        if (liz.Template.type != HailstormEnums.GorditoGreenie ||
            ctr.Template.type == HailstormEnums.GorditoGreenie)
        {
            return false;
        }
        if (collTimer1 > 0 && collTimer2 > 0)
        {
            return false;
        }
        if (liz.gravity == 0f || liz.Submersion > 0.2f)
        {
            return false;
        }
        bool falling = liz.mainBodyChunk.vel.y <= -10 && liz.bodyChunks is not null;
        if (liz.enteringShortCut.HasValue || !(falling || (liz.animation == Lizard.Animation.Lounge && Mathf.Abs(liz.mainBodyChunk.vel.x) > 5)))
        {
            if (!falling)
            {
                return false;
            }
            foreach (BodyChunk chunk in liz.bodyChunks)
            {
                if (chunk.contactPoint.y == -1) return false;
            }
        }
        if (liz.bodyChunks[1].vel.magnitude < liz.bodyChunks[0].vel.magnitude)
        {
            liz.bodyChunks[0].vel = liz.bodyChunks[1].vel;
        }
        foreach (Creature.Grasp grasp in liz.grabbedBy)
        {
            if (grasp.pacifying || grasp.grabber == ctr)
            {
                return false;
            }
        }
        return true;

    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Lizard AI

    public static CreatureTemplate.Relationship HailstormLizardRelationships(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI lizAI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        CreatureTemplate.Type lizType = lizAI.lizard.Template.type;
        CreatureTemplate.Type otherCtrType = dynamRelat.trackerRep.representedCreature.creatureTemplate.type;
        Creature ctr = dynamRelat.trackerRep.representedCreature.realizedCreature;



        if (ctr is LuminCreature lmn && dynamRelat.state is not null)
        {
            LizardAI.LizardTrackState trackedState = dynamRelat.state as LizardAI.LizardTrackState;
            if (dynamRelat.trackerRep.VisualContact && lmn is not null)
            {
                trackedState.spear = false;
                trackedState.vultureMask = 0;
                if (lmn.grasps is not null)
                {
                    for (int i = 0; i < lmn.grasps.Length; i++)
                    {
                        if (lmn.grasps[i] is null)
                        {
                            continue;
                        }

                        if (lmn.grasps[i].grabbed is Spear)
                        {
                            trackedState.spear = true;
                        }
                        else if (lmn.grasps[i].grabbed is VultureMask mask)
                        {
                            trackedState.vultureMask = Math.Max(trackedState.vultureMask, !mask.King ? 1 : 2);
                        }
                    }
                }
            }
            if (trackedState.vultureMask > 0 &&
                lizAI.creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard &&
                lizAI.creature.creatureTemplate.type != CreatureTemplate.Type.RedLizard &&
                lizAI.usedToVultureMask < (trackedState.vultureMask == 2 ? 1200 : 700))
            {
                lizAI.usedToVultureMask++;
                if (lizAI.creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard && trackedState.vultureMask < 2)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Ignores, 0f);
                }
                float scareFac = trackedState.vultureMask != 2 ?
                    (lizAI.creature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard ? 0.8f : 0.6f) :
                    (lizAI.creature.creatureTemplate.type == CreatureTemplate.Type.GreenLizard ? 0.4f : 0.9f);
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(a: trackedState.vultureMask == 2 ? 1200 : 700, b: 600f, value: lizAI.usedToVultureMask) * scareFac);
            }
        }


        if (OtherCreatureChanges.IsIncanStory(lizAI.lizard.room.game))
        {
            if ((lizType == CreatureTemplate.Type.Salamander || lizType == MoreSlugcatsEnums.CreatureTemplateType.EelLizard) &&
                (otherCtrType == CreatureTemplate.Type.Leech || otherCtrType == CreatureTemplate.Type.SeaLeech || otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech))
            {
                return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, 0.1f);
            }

            if (lizType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
            {
                if (otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Pack, 0.5f);
                }

                if (ctr is TubeWorm && (ctr.grabbedBy is null || ctr.grabbedBy.Count == 0))
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Eats, ctr.dead ? 0.4f : 0.1f);
                }

                if (ctr is Spider && ctr?.room is not null && Custom.DistLess(ctr.DangerPos, lizAI.lizard.DangerPos, 400))
                {
                    float spiderMass = 0;
                    foreach (AbstractCreature absCtr in ctr.room.abstractRoom.creatures)
                    {
                        if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Spider spd && Custom.DistLess(ctr.DangerPos, spd.DangerPos, 200))
                        {
                            spiderMass +=
                                spd.TotalMass / (spd.dead ? 3 : 1);
                        }
                    }
                    if (spiderMass > lizAI.lizard.TotalMass * 1.33f)
                    {
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Afraid, spiderMass / 2);
                    }
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Eats, 1 - (spiderMass / 2));
                }
            }
        }

        if (lizAI.lizard.LizardState is ColdLizState lS)
        {
            bool Scissorbird =
                ctr is MirosBird ||
                (ctr is Vulture vul && vul.IsMiros);

            bool packMember =
                otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard ||
                otherCtrType == CreatureTemplate.Type.BlueLizard ||
                otherCtrType == HailstormEnums.IcyBlue ||
                otherCtrType == HailstormEnums.Freezer;

            if (lS.IcyBlue)
            {
                if (ctr is Lizard && !packMember) // Eats other lizards if they're dead, otherwise fighting them to kill.
                {
                    if (otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
                    {
                        return lS.PackPower >= 0.6f ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f);
                    }

                    if (ctr?.room is not null && otherCtrType == CreatureTemplate.Type.YellowLizard)
                    {
                        float yellowCount = 0;
                        foreach (AbstractCreature absCtr in ctr.room.abstractRoom.creatures)
                        {
                            if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Lizard yellow && !yellow.State.dead && yellow.Template.type == CreatureTemplate.Type.YellowLizard)
                            {
                                yellowCount += 0.1f;
                            }
                        }
                        return lS.PackPower >= yellowCount || lS.NearAFreezer ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.75f) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.Min(1, yellowCount - lS.PackPower));
                    }

                    if (otherCtrType == CreatureTemplate.Type.GreenLizard ||
                        otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard ||
                        otherCtrType == CreatureTemplate.Type.CyanLizard ||
                        otherCtrType == CreatureTemplate.Type.RedLizard)
                    {

                        bool cyanLiz = otherCtrType == CreatureTemplate.Type.CyanLizard;

                        return lS.PackPower >= (cyanLiz ? 0.25f : 0.5f) ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Min(1, lS.PackPower * (cyanLiz ? 2 : 1))) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, Mathf.Min(1, lS.PackPower * (cyanLiz ? 1 : 2)));
                    }
                }

                if (ctr is Centipede cnt && (cnt.Red || cnt.Template.type == HailstormEnums.Cyanwing))
                {
                    if (ctr.dead)
                    {
                        return
                            lS.NearAFreezer ? 
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.6f) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.Lerp(0.2f, 1, lS.PackPower));
                    }
                    return lS.PackPower >= 0.7f && lS.NearAFreezer ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, lS.PackPower) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1);
                }

                if (ctr is Vulture || ctr is MirosBird) // Attacks and kills both vultures and Miros birds, then eats them.
                {
                    float threshold = Scissorbird ? 1 : 0.5f;
                    if (lS.NearAFreezer) threshold -= 0.2f;
                    return lS.PackPower >= threshold ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, lS.PackPower) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(1, (Scissorbird ? 0.8f : 0.4f), lS.PackPower));
                }

                if (ctr is Scavenger && ctr?.room is not null)
                {
                    float scavPackPower = 0;
                    foreach (AbstractCreature absCtr in ctr.room.abstractRoom.creatures)
                    {
                        if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Scavenger scv2 && !scv2.dead)
                        {
                            scavPackPower +=
                                scv2.Elite || scv2.King ? 0.3f : 0.1f;
                        }
                    }
                    if (lS.PackPower < scavPackPower)
                    {
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Afraid, Mathf.Min(1, scavPackPower - lS.PackPower));
                    }
                }

                if (ctr is Player && lizAI.LikeOfPlayer(dynamRelat.trackerRep) < 0.5f)
                {
                    return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, lS.PackPower > 0.1f ? 1 : Mathf.InverseLerp(0, 600, dynamRelat.trackerRep.age));
                }

            }
            if (lS.Freezer)
            {

                if (ctr is Spider && otherCtrType != HailstormEnums.Luminescipede)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Ignores, 1);
                }

                if (ctr is Lizard && !packMember) // Eats other lizards if they're dead, otherwise fighting them to kill.
                {
                    if (otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
                    {
                        return lS.PackPower >= 0.6f?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f);
                    }
                    return !ctr.State.dead ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(otherCtrType == CreatureTemplate.Type.RedLizard ? 0.75f : 0.5f, 1, Mathf.InverseLerp(0.2f, 1, lS.PackPower))) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f);
                }

                if (dynamRelat?.state is LizardAI.LizardTrackState lizTrackState && lizTrackState.vultureMask > 0 && lizAI.usedToVultureMask < 700)
                { // Freezers are almost unaffected by normal Vulture Masks, and King Vulture masks will only briefly intimidate them.
                    lizAI.usedToVultureMask += 800;
                    if (lizTrackState.vultureMask == 2)
                    {
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.StayOutOfWay, 0.5f);
                    }
                }

                if (ctr is Centipede cnt && (cnt.Red || cnt.Template.type == HailstormEnums.Cyanwing))
                { // Tends to play it safe with mega-Centipedes, but generally wants them dead.
                    if (ctr.dead)
                    {
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, 1);
                    }
                    return lS.PackPower >= 0.7f || !Custom.DistLess(lizAI.lizard.DangerPos, cnt.DangerPos, 350) ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Max(0.5f, Mathf.InverseLerp(350, 50, Custom.Dist(lizAI.lizard.DangerPos, cnt.DangerPos)))) :
                        new CreatureTemplate.Relationship (CreatureTemplate.Relationship.Type.Afraid, 1);
                }

                if (ctr is Vulture || ctr is MirosBird) // Attacks and kills both vultures and Miros birds, then eats them.
                {
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(Scissorbird ? 0.8f : 0.6f, 1, Mathf.InverseLerp(0.2f, 1, lS.PackPower)));
                }
            }
        }

        if (ctr is Lizard icy && icy.LizardState is ColdLizState LS)
        {
            if (lizType == CreatureTemplate.Type.CyanLizard && icy.Template.type == HailstormEnums.IcyBlue)
            {
                return (LS.NearAFreezer || LS.PackPower >= 0.5f) ?
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, LS.NearAFreezer ? 1 : Mathf.Lerp(0.25f, 1, Mathf.InverseLerp(0.5f, 1, LS.PackPower))) :
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.5f, 0.25f, Mathf.InverseLerp(0, 0.5f, LS.PackPower)));
            }
            if (lizType == CreatureTemplate.Type.GreenLizard || lizType == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || lizType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
            {
                if (lizType == CreatureTemplate.Type.GreenLizard && otherCtrType == HailstormEnums.IcyBlue && icy.dead && !LS.NearAFreezer)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Eats, 0.25f);
                }
                if (LS.PackPower >= 0.9f && (otherCtrType == HailstormEnums.Freezer || LS.NearAFreezer))
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Afraid, 1);
                }
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.7f, 1, Mathf.InverseLerp(0, 0.9f, LS.PackPower)));
            }
            if (lizType == CreatureTemplate.Type.RedLizard && otherCtrType == HailstormEnums.IcyBlue && LS.PackPower >= 0.5f)
            {
                return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.7f, 1, Mathf.InverseLerp(0.5f, 1, LS.PackPower)));
            }
        }

        if (dynamRelat is not null && OtherCreatureChanges.IsIncanStory(lizAI.lizard.room.game) &&
            lizType != HailstormEnums.Freezer &&
            lizType != HailstormEnums.IcyBlue &&
            lizType != CreatureTemplate.Type.BlueLizard &&
            lizType != CreatureTemplate.Type.Salamander &&
            lizType != MoreSlugcatsEnums.CreatureTemplateType.EelLizard &&
            dynamRelat.trackerRep.representedCreature?.realizedCreature is not null &&
            dynamRelat.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat &&
            lizAI.friendTracker.giftOfferedToMe is null) // Lizards will be more aggressive towards the player in general... probably?
        {
            float likeOfPlayer = lizAI.LikeOfPlayer(dynamRelat.trackerRep);
            if (ModManager.CoopAvailable && Custom.rainWorld.options.friendlyLizards)
            {
                foreach (AbstractCreature nonPermaDeadPlayer in lizAI.lizard.abstractCreature.world.game.NonPermaDeadPlayers)
                {
                    Tracker.CreatureRepresentation player = lizAI.tracker.RepresentationForCreature(nonPermaDeadPlayer, addIfMissing: false);
                    likeOfPlayer = Mathf.Max(lizAI.LikeOfPlayer(player), likeOfPlayer);
                }
            }
            return (likeOfPlayer >= 0.5f) ?
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f) :
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.Pow(Mathf.InverseLerp(0.8f, -0.5f, likeOfPlayer), Mathf.Pow(lizAI.lizard.lizardParams.aggressionCurveExponent, 1.5f)));
        }

        return orig(lizAI, dynamRelat);
    }

    public static void ColdLizardAISetup(On.LizardAI.orig_ctor orig, LizardAI liz, AbstractCreature absCtr, World world)
    {
        orig(liz, absCtr, world);
        if (liz.lizard is not null && liz.lizard.LizardState is ColdLizState lS)
        {
            if (lS.IcyBlue)
            {
                liz.pathFinder.stepsPerFrame = 20;
                liz.preyTracker.sureToGetPreyDistance = 5;
                liz.preyTracker.giveUpOnUnreachablePrey = 1100;
                liz.stuckTracker.minStuckCounter = 40;
                liz.stuckTracker.maxStuckCounter = 80;
            }
            else if (lS.Freezer)
            {
                liz.pathFinder.stepsPerFrame = 20;
                liz.preyTracker.sureToGetPreyDistance = 10;
                liz.preyTracker.giveUpOnUnreachablePrey = 1800;
                liz.stuckTracker.minStuckCounter = 20;
                liz.stuckTracker.maxStuckCounter = 40;
            }
        }
    }

    public static void ColdLizardAIUpdate(On.LizardAI.orig_Update orig, LizardAI lizAI)
    {
        orig(lizAI);
        if (lizAI.lizard?.room is not null && LizardData.TryGetValue(lizAI.lizard, out _))
        {
            Lizard liz = lizAI.lizard;

            if (liz.LizardState is ColdLizState lS)
            {
                lS.PackUpdateTimer++;

                if (lS.PackUpdateTimer > 80)
                {
                    lS.PackUpdateTimer = 0;
                    lS.PackPower = 0;
                    lS.NearAFreezer = false;
                    foreach (AbstractCreature absCtr in liz.room.abstractRoom.creatures)
                    {
                        if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Lizard otherLiz && !otherLiz.dead && Custom.DistLess(liz.DangerPos, otherLiz.DangerPos, 1250) &&
                            (otherLiz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard || otherLiz.Template.type == HailstormEnums.IcyBlue || otherLiz.Template.type == HailstormEnums.Freezer))
                        {
                            if (lS.PackPower < 1)
                            {
                                lS.PackPower +=
                                    otherLiz.Template.type == HailstormEnums.Freezer ? 0.2f :
                                    otherLiz.Template.type == HailstormEnums.IcyBlue ? 0.1f : 0.05f;
                            }

                            if (otherLiz.Template.type == HailstormEnums.Freezer && !lS.NearAFreezer)
                            {
                                lS.NearAFreezer = true;
                                if (lizAI.creature.abstractAI.followCreature is null && lizAI.preyTracker.MostAttractivePrey is null)
                                {
                                    lizAI.creature.abstractAI.followCreature = otherLiz.abstractCreature;
                                }
                            }
                        }
                    }
                    if (lS.PackPower > 1)
                    {
                        lS.PackPower = 1;
                    }
                }

                if (lS.Freezer)
                {
                    lizAI.noiseTracker.hearingSkill = 1.5f;
                    if (Random.value < 0.01f)
                    {
                        lizAI.creature.abstractAI.AbstractBehavior(1);
                    }
                }

            }
            else if (OtherCreatureChanges.IsIncanStory(liz.room.game))
            {
                if (Weather.ErraticWindCycle && liz.lizardParams.bodyMass < 1.6f && Weather.ExtremeWindIntervals[Weather.WindInterval])
                {
                    if (lizAI.denFinder.GetDenPosition().HasValue && lizAI.creature.abstractAI.destination != lizAI.denFinder.GetDenPosition().Value)
                    {
                        lizAI.creature.abstractAI.SetDestination(lizAI.denFinder.GetDenPosition().Value);
                        Weather.LockDestination(lizAI);
                    }
                    lizAI.runSpeed = Mathf.Lerp(lizAI.runSpeed, 1f, 0.1f);
                }

                if (lizAI.redSpitAI is not null &&
                    (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || liz.Template.type == CreatureTemplate.Type.RedLizard))
                {
                    if (lizAI.redSpitAI.spitting)
                    {
                        lizAI.redSpitAI.spitting = false;
                    }
                    if (lizAI.redSpitAI.spitFromPos != lizAI.creature.pos)
                    {
                        lizAI.redSpitAI.spitFromPos = lizAI.creature.pos;
                    }
                }
            }

            if ((liz.LizardState is ColdLizState || liz.Template.type == CreatureTemplate.Type.WhiteLizard) &&
                Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval] && liz.room.blizzardGraphics is not null)
            {
                float exposure = (
                        liz.room.blizzardGraphics.GetBlizzardPixel((int)liz.bodyChunks[0].pos.x, (int)liz.bodyChunks[0].pos.y).g +
                        liz.room.blizzardGraphics.GetBlizzardPixel((int)liz.bodyChunks[liz.bodyChunks.Length - 1].pos.x, (int)liz.bodyChunks[liz.bodyChunks.Length - 1].pos.y).g) / 2f;

                if (exposure >= 0.5f)
                {
                    lizAI.runSpeed = Mathf.Lerp(lizAI.runSpeed, 0.25f, exposure / 30f);
                }
            }
        }
    }

    public static PathCost HailstormLizTravelPrefs(On.LizardAI.orig_TravelPreference orig, LizardAI lizAI, MovementConnection connection, PathCost cost)
    {
        if (IsIncanStory(lizAI?.lizard?.room?.game) && (lizAI.lizard.Template.type == CreatureTemplate.Type.Salamander || lizAI.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
        {
            if (!lizAI.lizard.room.GetTile(connection.destinationCoord).AnyWater)
            {
                //cost.resistance -= 10f;
            }
        }
        return orig(lizAI, connection, cost);
    }

    public static void LizardAI_ILHooks()
    {
        IL.LizardJumpModule.RunningUpdate += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<LizardJumpModule>(nameof(LizardJumpModule.lizard)),
                x => x.MatchCallvirt<Creature>("get_safariControlled"),
                x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((LizardJumpModule ljm) => !(Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval]));
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] BEEP BOOP! SOMETHING WITH CYAN LIZARDS AND ERRATIC WIND CYCLES BROKE! REPORT THIS!");
        };
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Lizard Graphics

    public static LizardGraphics.IndividualVariations ColdLizardTailsWillAlwaysBeColored(On.LizardGraphics.orig_GenerateIvars orig, LizardGraphics liz)
    {
        float headSize = Custom.ClampedRandomVariation(0.5f, 0.025f, 0.2f) * 2f;
        float bodyFatness = Custom.ClampedRandomVariation(0.5f, 0.05f, 0.25f) * 2f;
        float tailLength = Custom.ClampedRandomVariation(0.5f, 0.05f, 0.25f) * 2f;
        if (liz.lizard.Template.type == HailstormEnums.Freezer)
        {
            return new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, bodyFatness, 0.8f);
        }
        if (liz.lizard.Template.type == HailstormEnums.IcyBlue)
        {
            float massLerp = Mathf.InverseLerp(1.4f, 1.6f, liz.lizard.mainBodyChunk.mass * 3);
            headSize = Mathf.Lerp(0.475f, 0.55f, massLerp) * 2;
            bodyFatness = Custom.ClampedRandomVariation(0.5f, 0.03f, 0.375f) * 2f;
            return new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, bodyFatness, Mathf.Lerp(0.25f, 0.75f, massLerp));
        }
        if (liz.lizard.Template.type == HailstormEnums.GorditoGreenie)
        {
            return new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, Random.Range(0.875f, 0.925f), 0.5f);
        }
        return orig(liz);
    }
    public static void HailstormLizardGraphics(On.LizardGraphics.orig_ctor orig, LizardGraphics lG, PhysicalObject ow)
    {        
        orig(lG, ow);
        if (lG.lizard is not null && LizardData.TryGetValue(lG.lizard, out LizardInfo lI))
        {
            if (lG.lizard.Template.type == HailstormEnums.Freezer)
            {
                Random.State state = Random.state;
                Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;
                int num = 0;

                if (Random.value < 0.66f)
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new LongHeadScales(lG, cosmeticSprites));
                    num++;
                }
                if (num == 0 || Random.value < (0.5f - 0.125f * num))
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IceSpikeTuft(lG, cosmeticSprites));
                    num++;
                }
                if (Random.value < (num == 0 ? 0.6f : 0.2f))
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IcyRhinestones(lG, cosmeticSprites));
                }

                cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new ArmorIceSpikes(lG, cosmeticSprites));

                Random.state = state;
            }
            else if (lG.lizard.Template.type == HailstormEnums.IcyBlue)
            {
                Random.State state = Random.state;
                Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;
                int num = 0;
                bool LSS = false;

                if (Random.value < 0.8888f)
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IceSpikeTuft(lG, cosmeticSprites));

                if (Random.value < 0.125f)
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new LongShoulderScales(lG, cosmeticSprites));
                    num++;
                    LSS = true;
                }
                if (Random.value < (num == 0 ? 0.6666f : 0.3333f))
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IcyRhinestones(lG, cosmeticSprites));
                    num++;
                }
                if ((!LSS && Random.value < (0.7f - 0.2f * num)) || Random.value < 0.04f)
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new LongHeadScales(lG, cosmeticSprites));
                }

                cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new ArmorIceSpikes(lG, cosmeticSprites));

                Random.state = state;
            }
            else if (lG.lizard.Template.type == HailstormEnums.GorditoGreenie)
            {
                Random.State state = Random.state;
                Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;

                cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new SnowAccumulation(lG, cosmeticSprites));
                cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new SnowAccumulation(lG, cosmeticSprites));

                Random.state = state;
            }
            else if (OtherCreatureChanges.IsIncanStory(lG.lizard.room?.game) && lG.lizard.abstractCreature.Winterized)
            {
                if (lG.lizard.Template.type == CreatureTemplate.Type.Salamander || lG.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    Random.State state = Random.state;
                    Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                    int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;

                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new ShortBodyScales(lG, cosmeticSprites));

                    Random.state = state;
                }
                else if (lG.lizard.Template.type != CreatureTemplate.Type.WhiteLizard)
                {
                    Random.State state = Random.state;
                    Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                    lG.head.rad = lG.lizard.lizardParams.headSize * (lG.lizard.Template.type == CreatureTemplate.Type.GreenLizard ? 9f : 7f);
                    if (lG.tail is not null)
                    {
                        for (int j = 0; j < lG.lizard.lizardParams.tailSegments; j++)
                        {
                            float rad =
                                8f * lG.lizard.lizardParams.bodySizeFac * ((lG.lizard.lizardParams.tailSegments - j) / (float)lG.lizard.lizardParams.tailSegments);

                            float connectionRad =
                                ((j > 0 ? 8f : 16f) + rad) / 2f * lG.lizard.lizardParams.tailLengthFactor * lG.iVars.tailLength;

                            lG.tail[j].rad = rad * (lG.lizard.Template.type == CreatureTemplate.Type.GreenLizard? 3f : 1.15f);
                            lG.tail[j].connectionRad = connectionRad * 1.15f;
                        }
                    }

                    Random.state = state;
                }
            }
        }
    }

    public static void GorditoGreenieBodySprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(liz, sLeaser, rCam);
        if (liz is not null && liz.lizard.Template.type == HailstormEnums.GorditoGreenie)
        {
            for (int b = liz.SpriteBodyCirclesStart; b < liz.SpriteBodyCirclesEnd; b++)
            {
                sLeaser.sprites[b].element = Futile.atlasManager.GetElementWithName("HailstormCircle40");
            }
        }
    }
    public static void ColdLizardSpriteReplacements(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(liz, sLeaser, rCam, timeStacker, camPos);
        if (liz is null) return;

        if (liz.lizard.LizardState is ColdLizState lS)
        {
            int RNG = liz.lizard.abstractCreature.ID.RandomSeed;
            if (lS.IcyBlue)
            {
                sLeaser.sprites[liz.SpriteHeadStart].color = IcyBlueHeadColor(liz, timeStacker, SecondaryIcyBlueColor(liz));
                sLeaser.sprites[liz.SpriteHeadStart + 3].color = IcyBlueHeadColor(liz, timeStacker, liz.effectColor);

                if (liz.lizard.TotalMass > 1.55f)
                {
                    float headAngleNumber = Mathf.Lerp(liz.lastHeadDepthRotation, liz.headDepthRotation, timeStacker);
                    int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);
                    sLeaser.sprites[liz.SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("FreezerEyes0." + headAngle);
                }

                int colorSpacing = (int)Mathf.Lerp(-0.25f, 0, Mathf.InverseLerp(1.4f, 1.6f, liz.lizard.TotalMass));
                if (liz.iVars.tailColor > 0f && sLeaser.sprites?[liz.SpriteTail] is TriangleMesh tail)
                {
                    for (int i = 0; i < tail?.verticeColors?.Length; i++)
                    {
                        tail.verticeColors[i] =
                            Color.Lerp(sLeaser.sprites[liz.SpriteBodyCirclesStart].color, sLeaser.sprites[(RNG % 5 == 0) ? liz.SpriteHeadStart : liz.SpriteHeadStart + 3].color, colorSpacing + Mathf.InverseLerp(2, 6, i));
                    }
                }
            }
            else if (lS.Freezer)
            {
                // Visuals-related variables
                float headAngleNumber = Mathf.Lerp(liz.lastHeadDepthRotation, liz.headDepthRotation, timeStacker);
                int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);

                // Position-related variables
                Vector2 val6 = Custom.PerpendicularVector(Vector2.Lerp(liz.drawPositions[0, 1], liz.drawPositions[0, 0], timeStacker) - Vector2.Lerp(liz.head.lastPos, liz.head.pos, timeStacker));

                /* Sprite Replacements */
                // Jaw
                sLeaser.sprites[liz.SpriteHeadStart].element = Futile.atlasManager.GetElementWithName("FreezerJaw0." + headAngle);
                sLeaser.sprites[liz.SpriteHeadStart].color = FreezerHeadColor(liz, timeStacker, SecondaryFreezerColor(liz));

                // Lower Teeth 
                sLeaser.sprites[liz.SpriteHeadStart + 1].element = Futile.atlasManager.GetElementWithName("FreezerLowerTeeth0." + headAngle);

                // Upper Teeth
                sLeaser.sprites[liz.SpriteHeadStart + 2].element = Futile.atlasManager.GetElementWithName("FreezerUpperTeeth0." + headAngle);

                // Head 
                sLeaser.sprites[liz.SpriteHeadStart + 3].element = Futile.atlasManager.GetElementWithName("FreezerHead0." + headAngle);
                sLeaser.sprites[liz.SpriteHeadStart + 3].color = FreezerHeadColor(liz, timeStacker, liz.effectColor);

                // Eyes
                sLeaser.sprites[liz.SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("FreezerEyes0." + headAngle);

                // Leg recoloring               
                for (int l = liz.SpriteLimbsColorStart; l < liz.SpriteLimbsColorEnd; l++)
                {
                    sLeaser.sprites[l].color =
                        (RNG % 5 == 0) ? sLeaser.sprites[liz.SpriteHeadStart + 3].color :
                        (RNG % 4 == 0) ? sLeaser.sprites[liz.SpriteHeadStart].color :
                        (RNG % 3 == 0 ? SecondaryFreezerColor(liz) : liz.effectColor);
                }

                /* Tail recoloring */
                if (liz.iVars.tailColor > 0f && sLeaser.sprites?[liz.SpriteTail] is TriangleMesh tail)
                {
                    for (int i = 0; i < tail?.verticeColors?.Length; i++)
                    {
                        tail.verticeColors[i] =
                            Color.Lerp(sLeaser.sprites[liz.SpriteBodyCirclesStart].color, sLeaser.sprites[(RNG % 5 == 0) ? liz.SpriteHeadStart : liz.SpriteHeadStart + 3].color, Mathf.InverseLerp(2, 6, i));
                    }
                }
            }
        }
        else if (LizardData.TryGetValue(liz.lizard, out LizardInfo lI) && lI.Gordito)
        {
            for (int b = liz.SpriteBodyCirclesStart; b < liz.SpriteBodyCirclesEnd; b++)
            {
                sLeaser.sprites[b].scale /= 2f;
            }
        }
    }

    public static void ColdLizardSpriteLayering(On.LizardGraphics.orig_AddToContainer orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(liz, sLeaser, rCam, newContainer);
        if (liz.lizard.Template.type == HailstormEnums.Freezer)
        {
            sLeaser.sprites[liz.SpriteHeadStart + 1].MoveBehindOtherNode(sLeaser.sprites[liz.SpriteHeadStart]);

            if (sLeaser.sprites[liz.SpriteHeadStart].element == Futile.atlasManager.GetElementWithName("FreezerJaw0.0"))
            {
                sLeaser.sprites[liz.SpriteHeadStart].MoveInFrontOfOtherNode(sLeaser.sprites[liz.SpriteHeadStart + 3]);
            }
            else
            {
                sLeaser.sprites[liz.SpriteHeadStart].MoveBehindOtherNode(sLeaser.sprites[liz.SpriteHeadStart + 3]);
            }
        }
    }

    //--------------------------------------

    public static void ColdLizardBodyColors1(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(liz, sLeaser, rCam, palette);
        if (liz?.lizard is not null && LizardData.TryGetValue(liz.lizard, out LizardInfo lI))
        {
            if (liz.lizard.Template.type == HailstormEnums.Freezer)
            {
                liz.ColorBody(sLeaser, FreezerBodyColor(liz));
            }
            else if (liz.lizard.Template.type == HailstormEnums.IcyBlue)
            {
                liz.ColorBody(sLeaser, IcyBlueBodyColor(liz));
            }
            else if (liz.lizard.Template.type == HailstormEnums.GorditoGreenie)
            {
                liz.ColorBody(sLeaser, Color.Lerp(Color.gray, liz.lizard.effectColor, 0.1f));
            }
        }
    }

    // This applies color to the occasional tail coloration that lizards can have.
    public static Color ColdLizardBodyColors2(On.LizardGraphics.orig_BodyColor orig, LizardGraphics liz, float f)
    {
        if (liz?.lizard is not null && LizardData.TryGetValue(liz.lizard, out LizardInfo lI))
        {
            if (liz.lizard.Template.type == HailstormEnums.Freezer)
            {
                return FreezerBodyColor(liz);
            }
            if (liz.lizard.Template.type == HailstormEnums.IcyBlue)
            {
                return IcyBlueBodyColor(liz);
            }
            if (liz.lizard.Template.type == HailstormEnums.GorditoGreenie)
            {
                return Color.Lerp(Color.gray, liz.lizard.effectColor, 0.1f);
            }
        }
        return orig(liz, f);
    }

    // Dynamic body color is used for lizards with body colors that actively change, specifically the White Lizard, Salamanders, and... winterized Strawberries, apparently???
    public static Color ColdLizardBodyColors3(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics liz, float f)
    {
        if (liz?.lizard is not null && LizardData.TryGetValue(liz.owner as Lizard, out LizardInfo lI))
        {
            if (liz.lizard.Template.type == HailstormEnums.Freezer)
            {
                return FreezerBodyColor(liz);
            }
            if (liz.lizard.Template.type == HailstormEnums.IcyBlue)
            {
                return IcyBlueBodyColor(liz);
            }
            if (liz.lizard.Template.type == HailstormEnums.GorditoGreenie)
            {
                return Color.Lerp(Color.gray, liz.lizard.effectColor, 0.1f);
            }
        }
        return orig(liz, f);
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Custom Lizard Colors
    public static Color SecondaryFreezerColor(LizardGraphics liz)
    {
        Color.RGBToHSV(liz.lizard.effectColor, out float h, out float s, out float v);
        h *= (h * 1.2272f > 0.75f)? 0.7728f : 1.2272f;
        v -= 0.1f;
        return Color.HSVToRGB(h, s, v);
    }    
    public static Color FreezerHeadColor(LizardGraphics liz, float timeStacker, Color baseColor)
    {
        float flickerIntensity = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(liz.lastBlink, liz.blink, timeStacker) * 2f * Mathf.PI), 1.5f + liz.lizard.AI.excitement * 1.5f);
        flickerIntensity = Mathf.Lerp(flickerIntensity, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(liz.lastVoiceVisualization, liz.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(liz.lastVoiceVisualizationIntensity, liz.voiceVisualizationIntensity, timeStacker));
        Color whiteOut = Color.Lerp(baseColor, Color.white, 0.6f);
        return Color.Lerp(whiteOut, baseColor, flickerIntensity);
    }
    public static Color FreezerBodyColor(LizardGraphics liz)
    {
        return Color.Lerp(liz.effectColor, Color.Lerp(new Color(129f / 255f, 164f / 255f, 234f / 255f), new Color(0.45f, 0.45f, 0.45f), 0.92f), 0.92f);
    }

    public static Color SecondaryIcyBlueColor(LizardGraphics liz)
    {
        float mass = liz.lizard.TotalMass;
        Color.RGBToHSV(liz.lizard.effectColor, out float h, out float s, out float v);
        h *= 1 - (Mathf.Lerp(0, 0.1136f, Mathf.InverseLerp(1.35f, 1.65f, mass)) * ((mass % 0.02f == 0) ? 1 : -1));
        v -= mass/16;
        return Color.HSVToRGB(h, s, v);
    }
    public static Color IcyBlueHeadColor(LizardGraphics liz, float timeStacker, Color baseColor)
    {
        float flickerIntensity = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(liz.lastBlink, liz.blink, timeStacker) * 2f * Mathf.PI), 1.5f + liz.lizard.AI.excitement * 1.5f);
        flickerIntensity = Mathf.Lerp(flickerIntensity, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(liz.lastVoiceVisualization, liz.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(liz.lastVoiceVisualizationIntensity, liz.voiceVisualizationIntensity, timeStacker));
        Color whiteOut = Color.Lerp(baseColor, Color.white, 0.55f); 
        return Color.Lerp(whiteOut, baseColor, flickerIntensity);
    }
    public static Color IcyBlueBodyColor(LizardGraphics liz)
    {
        Color bodyDarkness = Color.HSVToRGB(0, 0, Mathf.Lerp(0f, 0.3f, Mathf.InverseLerp(1.4f, 1.6f, liz.lizard.TotalMass)));
        return Color.Lerp(liz.effectColor, bodyDarkness, 0.9f);
    }
    #endregion

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}