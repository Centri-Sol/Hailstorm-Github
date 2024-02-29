namespace Hailstorm;

//------------------------------------------------------------------------

internal class LizardHooks
{

    public static ConditionalWeakTable<Lizard, LizardInfo> LizardData = new();

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

        _ = new Hook(typeof(LizardTongue).GetMethod("get_Ready", Public | NonPublic | Instance), (Func<LizardTongue, bool> orig, LizardTongue tongue) => orig(tongue) && !(tongue.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard && tongue.lizard.TotalMass > 1.55f));
        On.LizardTongue.ctor += IcyBlueTongues;
        On.Lizard.HitHeadShield += GorditoSquishyHead1;
        On.Lizard.HitInMouth += GorditoSquishyHead2;
        On.Lizard.Violence += HailstormLizardViolence;
        On.Lizard.Act += GorditoAct;
        On.Lizard.EnterAnimation += GorditoEnterAnimation;

        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += HailstormLizardRelationships;
        On.LizardAI.Update += HailstormLizAIUpdate;
        On.LizardAI.TravelPreference += HailstormLizTravelPrefs;
        LizardAI_ILHooks();

        On.LizardGraphics.GenerateIvars += ColdLizardTailsWillAlwaysBeColored;
        On.LizardGraphics.ctor += HailstormLizardGraphics;
        On.LizardGraphics.DrawSprites += ColdLizardSpriteReplacements;
        On.LizardGraphics.AddToContainer += ColdLizardSpriteLayering;
        On.LizardGraphics.ApplyPalette += ColdLizardBodyColors1;
        On.LizardGraphics.BodyColor += ColdLizardBodyColors2;
        IcyCosmetics.Init();
        _ = new Hook(typeof(LizardGraphics).GetMethod("get_SalamanderColor", Public | NonPublic | Instance), (Func<LizardGraphics, Color> orig, LizardGraphics lG) => !(IsIncanStory(lG?.lizard?.room?.game) && lG.lizard.abstractCreature.Winterized) ? orig(lG) : Color.Lerp(lG.blackSalamander ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.75f, 0.75f, 0.75f), lG.effectColor, 0.08f));

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return !(RWG?.session is null || !RWG.IsStorySession || RWG.StoryCharacter != IncanInfo.Incandescent);
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
                c.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HSEnums.CreatureType.FreezerLizard);
                c.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                Debug.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Freezer Lizards!");
            }

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
                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HSEnums.CreatureType.IcyBlueLizard);
                c2.Emit(OpCodes.Brtrue, label2);
            }
            else
            {
                Debug.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Icy Blue Lizards!");
            }

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
                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type == HSEnums.CreatureType.GorditoGreenieLizard);
                c2.Emit(OpCodes.Brfalse, label3);
            }
            else
            {
                Debug.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature for Gorditio Greenie Lizards!");
            }
        };
    }

    public static void HailstormLizLegSFX(On.LizardLimb.orig_ctor orig, LizardLimb lizLimb, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(lizLimb, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics liz)
        {
            if (liz.lizard?.Template.type == HSEnums.CreatureType.FreezerLizard)
            {
                lizLimb.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                lizLimb.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
            else if (liz.lizard?.Template.type == HSEnums.CreatureType.IcyBlueLizard)
            {
                lizLimb.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                lizLimb.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
            else if (liz.lizard?.Template.type == HSEnums.CreatureType.GorditoGreenieLizard)
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
            if (liz.Template.type == HSEnums.CreatureType.FreezerLizard)
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

                res = list.Count == 0 ? SoundID.None : list[Random.Range(0, list.Count)];
            }
            else if (liz.Template.type == HSEnums.CreatureType.IcyBlueLizard)
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

                res = list.Count == 0 ? SoundID.None : list[Random.Range(0, list.Count)];
            }
            else if (liz.Template.type == HSEnums.CreatureType.GorditoGreenieLizard)
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

                res = list.Count == 0 ? SoundID.None : list[Random.Range(0, list.Count)];
            }
        }
        return res;
    }
    public static void GorditoGreenieVoicePitch(On.LizardVoice.orig_ctor orig, LizardVoice voice, Lizard liz)
    {
        orig(voice, liz);
        if (liz is not null && liz.Template.type == HSEnums.CreatureType.GorditoGreenieLizard)
        {
            voice.myPitch *= 0.33f;
        }
    }
    public static void GorditoGreenieVoiceVolume(On.LizardVoice.orig_Update orig, LizardVoice voice)
    {
        orig(voice);
        if (voice.lizard is not null && voice.lizard.Template.type == HSEnums.CreatureType.GorditoGreenieLizard && voice.articulationIndex > -1 && voice.currentArticulationProgression < 1)
        {
            voice.Volume *= 0.7f / voice.myPitch;
        }
    }

    public static CreatureTemplate HailstormLizTemplates(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type lizardType, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        if (lizardType == HSEnums.CreatureType.IcyBlueLizard)
        {
            CreatureTemplate icyBlueTemp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams icyBlueStats = (icyBlueTemp.breedParameters as LizardBreedParams)!;

            CreatureTemplate frzTemp = StaticWorld.GetCreatureTemplate(HSEnums.CreatureType.FreezerLizard);
            LizardBreedParams frzStats = frzTemp.breedParameters as LizardBreedParams;

            // Icy Blues are unique in that their stats vary based on their mass (I just say "size" because it feels more natural), kinda like Centipedes but more detailed.
            // This variation is now done in Lizard.ctor, but I originally tried doing it in here, hence the leftover size-related code. (It doesn't work if done here)
            // This section is still good for seeing any Icy Blue stats that don't vary.
            float sizeMult = 1.33f;
            float sizeFac = Mathf.InverseLerp(1f, 2f, sizeMult);

            //----Basic Info----//
            icyBlueTemp.type = lizardType;
            icyBlueTemp.name = "Icy Blue Lizard";
            icyBlueTemp.meatPoints = 6;
            icyBlueStats.standardColor = Custom.HSL2RGB(0.625f, 0.7f, 0.7f);
            icyBlueStats.tamingDifficulty = 5.4f;
            icyBlueStats.aggressionCurveExponent = Mathf.Lerp(0.9f, frzStats.aggressionCurveExponent, sizeFac);
            icyBlueStats.danger = Mathf.Lerp(0.45f, frzStats.danger, sizeFac);
            icyBlueTemp.dangerousToPlayer = icyBlueStats.danger;
            icyBlueTemp.hibernateOffScreen = false;
            //----HP and Resistances----//
            icyBlueTemp.baseDamageResistance = 3;
            icyBlueTemp.instantDeathDamageLimit = 3;
            icyBlueTemp.baseStunResistance = 2.4f;
            CustomTemplateInfo.DamageResistances.AddLizardDamageResistances(ref icyBlueTemp, lizardType);
            icyBlueTemp.wormGrassImmune = false;
            icyBlueTemp.BlizzardAdapted = true;
            icyBlueTemp.BlizzardWanderer = true;
            //----Vision----//
            icyBlueTemp.visualRadius = 1250;
            icyBlueTemp.waterVision = Mathf.Lerp(0.4f, frzTemp.waterVision, sizeFac);
            icyBlueTemp.throughSurfaceVision = 0.7f;
            icyBlueStats.perfectVisionAngle = Mathf.Lerp(0.8888f, frzStats.perfectVisionAngle, sizeFac);
            icyBlueStats.periferalVisionAngle = Mathf.Lerp(0.0833f, frzStats.periferalVisionAngle, sizeFac);
            //----Body----//
            icyBlueStats.bodyMass = 1.5f;
            icyBlueStats.bodySizeFac = 0.9f * sizeMult;
            icyBlueStats.bodyRadFac = 1 / Mathf.Pow(sizeMult, 2);
            icyBlueStats.bodyStiffnes = Mathf.Lerp(0, frzStats.bodyStiffnes, sizeFac);
            icyBlueStats.floorLeverage = Mathf.Lerp(1, frzStats.floorLeverage, sizeFac);
            icyBlueStats.maxMusclePower = Mathf.Lerp(2, frzStats.maxMusclePower, sizeFac);
            icyBlueStats.wiggleSpeed = Mathf.Lerp(1, frzStats.wiggleSpeed, sizeFac);
            icyBlueStats.wiggleDelay = (int)Mathf.Lerp(15, frzStats.wiggleDelay, sizeFac);
            //----Movement----//
            icyBlueStats.idleCounterSubtractWhenCloseToIdlePos = 0;
            icyBlueStats.baseSpeed = Mathf.Lerp(3.9f, frzStats.baseSpeed, sizeFac);
            icyBlueTemp.offScreenSpeed = 2f;
            icyBlueStats.terrainSpeeds[3] = new(1f, 0.9f, 0.8f, 1f);
            icyBlueStats.terrainSpeeds[4] = new(0.9f, 0.9f, 0.9f, 1f);
            icyBlueStats.terrainSpeeds[5] = new(0.7f, 1f, 1f, 1f);
            icyBlueStats.swimSpeed = Mathf.Lerp(0.35f, frzStats.swimSpeed, sizeFac);
            icyBlueTemp.waterPathingResistance = Mathf.Lerp(20, frzTemp.waterPathingResistance, sizeFac);
            icyBlueTemp.requireAImap = true;
            icyBlueTemp.doPreBakedPathing = false;
            icyBlueTemp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
            icyBlueTemp.roamBetweenRoomsChance = 0.07f;
            icyBlueTemp.usesCreatureHoles = true;
            icyBlueTemp.usesNPCTransportation = true;
            icyBlueTemp.usesRegionTransportation = false;
            icyBlueTemp.shortcutSegments = 3;
            icyBlueStats.shakePrey = 100;
            //----Bites----//
            icyBlueStats.biteDelay = 15;
            icyBlueStats.biteRadBonus = Mathf.Lerp(0, frzStats.biteRadBonus, sizeFac);
            icyBlueStats.biteHomingSpeed = Mathf.Lerp(1.4f, frzStats.biteHomingSpeed, sizeFac);
            icyBlueStats.biteChance = Mathf.Lerp(0.4f, frzStats.biteChance, sizeFac);
            icyBlueStats.attemptBiteRadius = Mathf.Lerp(90f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.getFreeBiteChance = Mathf.Lerp(0.5f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.biteDamage = Mathf.Lerp(0.7f, frzStats.attemptBiteRadius, sizeFac);
            icyBlueStats.biteDamageChance = 0;
            icyBlueStats.biteDominance = Mathf.Lerp(0.1f, frzStats.attemptBiteRadius, sizeFac);
            //----Lunging----//
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
            //----Tongue----//
            icyBlueStats.tongue = true;
            icyBlueStats.tongueChance = 0.2f;
            icyBlueStats.tongueWarmUp = 15;
            icyBlueStats.tongueSegments = 5;
            icyBlueStats.tongueAttackRange = 140;
            //----Limbs----//
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
            //----Tail----//
            icyBlueStats.tailColorationStart = Mathf.Lerp(0.2f, 0.75f, Mathf.InverseLerp(1, 1.2f, sizeMult));
            icyBlueStats.tailColorationExponent = 0.5f;
            //----Head----//
            icyBlueStats.headShieldAngle = Mathf.Lerp(108, frzStats.headShieldAngle, sizeFac);
            icyBlueStats.headSize = Mathf.Lerp(0.9f, 1.1f, sizeFac);
            icyBlueStats.neckStiffness = Mathf.Lerp(0, frzStats.neckStiffness, sizeFac);
            icyBlueStats.framesBetweenLookFocusChange = (int)Mathf.Lerp(20, frzStats.framesBetweenLookFocusChange, sizeFac);

            return icyBlueTemp;
        }
        if (lizardType == HSEnums.CreatureType.FreezerLizard)
        {
            CreatureTemplate frzTemp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams frzStats = (frzTemp.breedParameters as LizardBreedParams)!;

            // Check LizardBreeds in the game's files if you want to compare to other lizards' stats.
            //----Basic Info----//
            frzTemp.type = lizardType;
            frzTemp.name = "Freezer Lizard"; // Internal lizard name.
            frzTemp.meatPoints = 12;  // How many food pips a creature will give when eaten. Has to be an Int.
            frzStats.standardColor = new(129 / 255f, 200 / 255f, 236 / 255f); // The lizard's base color.
            frzStats.tamingDifficulty = 9f; // How stubborn the lizard is to taming attempts. Higher numbers make them harder to tame.
            frzStats.tongue = false;
            frzStats.aggressionCurveExponent = 0.37f;
            frzStats.danger = 0.8f; // How threatening the game considers this creature.
            frzTemp.dangerousToPlayer = frzStats.danger; // How threatening the game considers this creature for stuff like threat music.
            frzTemp.hibernateOffScreen = false;
            //----HP and Resistances----//
            frzTemp.baseDamageResistance = 4f; // This is HP.
            frzTemp.instantDeathDamageLimit = 4f; // If the lizard takes at least this much damage from a single attack, it will die instantly.
            frzTemp.baseStunResistance = 2.5f; // Stun times will be divided by this amount by default.
            CustomTemplateInfo.DamageResistances.AddLizardDamageResistances(ref frzTemp, lizardType);
            frzTemp.wormGrassImmune = true; // Determines whether Wormgrass will try to eat this lizard or not.
            frzTemp.BlizzardAdapted = true; // If true, makes the lizard functionally immune to the Hypothermia mechaic.
            frzTemp.BlizzardWanderer = true; // If true, the lizard will stay outside after the cycle timer ends.
                                             // This is not tied to the blizzard mechanic at all; it's actually also used for nighttime spawns in Metropolis and the Wall.
                                             //----Vision----//
            frzTemp.visualRadius = 1750f; // How far the lizard can see.
            frzTemp.waterVision = 0.2f; // Vision in water.
            frzTemp.throughSurfaceVision = 0.5f; // How well the lizard can see through the surface of water.
            frzStats.perfectVisionAngle = 0.33f; // Determines how wide the angle of perfect vision the lizard has.
                                                 // - At -1, the lizard has perfect 360-degree vision
                                                 // - At 0, the lizard has perfect vision for up to 90 degrees in either direction (180 total)
                                                 // - At 1, the perfect vision angle is pretty much non-existent.
            frzStats.periferalVisionAngle = -0.5f; // The angle through which the lizard can see creatures at all, even if not well. Functions similarly to perfectVisionAngle.
                                                   //----Body----//
            frzStats.bodyMass = 2.8f; // Weight.
            frzStats.bodySizeFac = 1.2f; // Overall scale of the lizard's body chunks.
            frzStats.bodyRadFac = 0.7f; //  W I D E N E S S
            frzStats.bodyStiffnes = 0.33f;
            frzStats.floorLeverage = 4f;
            frzStats.maxMusclePower = 11f;
            frzStats.wiggleSpeed = 0.7f;
            frzStats.wiggleDelay = 20;
            //----Movement----//
            frzStats.idleCounterSubtractWhenCloseToIdlePos = 0;
            frzStats.baseSpeed = 4f;
            frzStats.terrainSpeeds[1] = new(1f, 1f, 1.2f, 1.2f); // Ground movement.
            frzStats.terrainSpeeds[2] = new(0.9f, 1f, 1f, 1.1f); // Tunnel movement.
            frzStats.terrainSpeeds[3] = new(1f, 0.85f, 0.7f, 1f);// Pole movement speeds.
            frzStats.terrainSpeeds[4] = new(0.8f, 1f, 1f, 1.1f); // Background movement.
            frzStats.terrainSpeeds[5] = new(0.8f, 1f, 1f, 1.2f); // Ceiling movement.
            frzStats.swimSpeed = 0.5f;
            frzTemp.waterPathingResistance = 10f;
            frzTemp.requireAImap = true;
            frzTemp.doPreBakedPathing = false;
            frzTemp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
            frzTemp.offScreenSpeed = 4f;
            frzTemp.roamBetweenRoomsChance = 0.1f;
            frzTemp.usesCreatureHoles = true;
            frzTemp.usesNPCTransportation = true;
            frzTemp.usesRegionTransportation = true;
            frzTemp.shortcutSegments = 4;
            frzStats.shakePrey = 100;
            //----Bites----//
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
            //----Lunging---//
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
            //----Limbs----//
            frzStats.limbSize = 1.2f; // Length of limbs.
            frzStats.limbThickness = 1.35f; // Chonkiness of limbs.
            frzStats.stepLength = 0.75f;
            frzStats.liftFeet = 0.2f; // How much lizards bring their feet up when crawling around.
            frzStats.feetDown = 0.4f; // How much lizards bring their feet down when crawling around.
            frzStats.noGripSpeed = 0.33f;
            frzStats.limbSpeed = 11f;
            frzStats.limbQuickness = 0.8f;
            frzStats.limbGripDelay = 0; // How many frames before the lizard's limbs can get a solid grip on surfaces.
            frzStats.legPairDisplacement = 0;
            frzStats.walkBob = 3.5f; // How bumpy a lizard's movement is.
            frzStats.regainFootingCounter = 2;
            //----Tail----//
            frzStats.tailSegments = 6; // Number of tail segments this lizard has.
            frzStats.tailStiffness = 150f;
            frzStats.tailStiffnessDecline = 0.1f;
            frzStats.tailLengthFactor = 3; // How long each tail segment is.
            frzStats.tailColorationStart = 0;
            frzStats.tailColorationExponent = 0.5f;
            //----Head----//
            frzStats.headShieldAngle = 140f;
            frzStats.headSize = 1f; // Scale of head sprites.
            frzStats.neckStiffness = 0.3f; // How resistant the lizard's head is to turning.
            frzStats.jawOpenAngle = 75f; // How wide this lizard will open their mouth.
            frzStats.jawOpenLowerJawFac = 0.66f; // How much the lizard's bottom jaw turns when its mouth opens.
            frzStats.jawOpenMoveJawsApart = 20f; // How far the lizard's bottom jaw lowers when its mouth opens.
            frzStats.headGraphics = new int[5] { 0, 0, 0, 2, 0 }; // This might be important?
            frzStats.framesBetweenLookFocusChange = 70;
            //----Pathing----//
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

            return frzTemp;
        }
        if (lizardType == HSEnums.CreatureType.GorditoGreenieLizard)
        {
            CreatureTemplate gorditoTemp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            LizardBreedParams gorditoStats = (gorditoTemp.breedParameters as LizardBreedParams)!;

            //----Basic Info----//
            gorditoTemp.type = lizardType;
            gorditoTemp.name = "Gordito Greenie Lizard";
            gorditoTemp.meatPoints = 18;
            gorditoStats.standardColor = new HSLColor(135 / 360f, 0.45f, 0.5f).rgb; // A desaturated minty green.
            gorditoStats.tamingDifficulty = 0;
            gorditoStats.tongue = false;
            gorditoStats.aggressionCurveExponent = 0.1f;
            gorditoStats.danger = 0.8f;
            gorditoTemp.dangerousToPlayer = gorditoStats.danger;
            //----HP and Resistances----//
            gorditoTemp.baseDamageResistance = 30; // Effective HP.
            gorditoTemp.instantDeathDamageLimit = gorditoTemp.baseDamageResistance;
            gorditoTemp.baseStunResistance = 100f;
            CustomTemplateInfo.DamageResistances.AddLizardDamageResistances(ref gorditoTemp, lizardType);
            gorditoTemp.wormGrassImmune = true;
            gorditoTemp.BlizzardAdapted = true;
            gorditoTemp.BlizzardWanderer = true;
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
            gorditoStats.loungeTendensy = 0;
            gorditoStats.findLoungeDirection = 1f;
            gorditoStats.preLoungeCrouch = 220;
            gorditoStats.preLoungeCrouchMovement = -0.06f;
            gorditoStats.loungeDistance = 510f;
            gorditoStats.loungeSpeed = 2f;
            gorditoStats.loungeMaximumFrames = 120;
            gorditoStats.loungePropulsionFrames = 20;
            gorditoStats.loungeJumpyness = 3f;
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
            gorditoStats.limbGripDelay = 3;
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
        if (lizardType is not null)
        {
            CreatureTemplate lizTemp = orig(lizardType, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            CustomTemplateInfo.DamageResistances.AddLizardDamageResistances(ref lizTemp, lizardType);
            return lizTemp;
        }

        return orig(lizardType, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }
    public static CreatureTemplate EelTemperatureResistances(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type lizardType, CreatureTemplate lizardAncestor, CreatureTemplate salamanderTemplate)
    {
        if (lizardType is not null)
        {
            CreatureTemplate lizTemp = orig(lizardType, lizardAncestor, salamanderTemplate);
            CustomTemplateInfo.DamageResistances.AddLizardDamageResistances(ref lizTemp, lizardType);
            return lizTemp;
        }

        return orig(lizardType, lizardAncestor, salamanderTemplate);
    }

    public static void HailstormLizConstructors(On.Lizard.orig_ctor orig, Lizard liz, AbstractCreature absLiz, World world)
    {
        orig(liz, absLiz, world);

        if (!LizardData.TryGetValue(liz, out _))
        {
            LizardData.Add(liz, new LizardInfo(liz));
        }

        if (liz is not null &&
            LizardData.TryGetValue(liz, out _))
        {
            if (liz.Template.type == CreatureTemplate.Type.YellowLizard && (IsIncanStory(world?.game) || HSRemix.YellowLizardColorsEverywhere.Value is true))
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
            else if (liz.Template.type == CreatureTemplate.Type.RedLizard && IsIncanStory(world?.game))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                liz.effectColor = Custom.HSL2RGB(Custom.ClampedRandomVariation(340 / 360f, 20 / 360f, 0.25f), Random.Range(0.7f, 0.8f), 0.4f);
                Random.state = state;
            }
            else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard && (IsIncanStory(world?.game) || HSRemix.EelLizardColorsEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(absLiz, out AbsCtrInfo aI))
            {
                Random.State state = Random.state;
                Random.InitState(absLiz.ID.RandomSeed);
                float winterHue = Custom.WrappedRandomVariation(195 / 360f, 65 / 360f, 0.5f);
                liz.effectColor =
                    absLiz.Winterized && world.region is not null && world.region.name != "CC" ? Custom.HSL2RGB(winterHue, Random.Range(0.6f, 1) - Mathf.InverseLerp(13 / 24f, 13 / 12f, winterHue), Custom.ClampedRandomVariation(Random.value < 0.08f ? 0.3f : 0.7f, 0.05f, 0.2f)) : // Winter colors
                    world.region is not null && world.region.name == "OE" ? Custom.HSL2RGB(Custom.WrappedRandomVariation(225 / 360f, 95 / 360f, 1), Random.Range(0.85f, 1), Random.Range(0.3f, 0.5f)) : // Outer Expanse colors
                    Custom.HSL2RGB(Custom.WrappedRandomVariation(130 / 360f, 65 / 360f, 0.8f), Random.Range(0.7f, 1), Custom.ClampedRandomVariation(0.25f, 0.15f, 0.33f)); // Default colors

                aI.functionTimer = Random.Range(0, 101);
                Random.state = state;
            }
            else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard && (IsIncanStory(world?.game) || HSRemix.StrawberryLizardColorsEverywhere.Value is true))
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
            else if (IsIncanStory(world?.game))
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
                        float hue = Custom.WrappedRandomVariation(210 / 360f, 50 / 360f, 0.6f);
                        liz.effectColor = Custom.HSL2RGB(hue, 1f, Custom.ClampedRandomVariation(hue, 0.2f, 0.3f));
                        Random.state = state;
                    }
                }
            }

            if (liz.Template.type == HSEnums.CreatureType.IcyBlueLizard && liz.lizardParams.tongue)
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
        if (LizardData.TryGetValue(liz, out _))
        {
            if (liz.Template.type == HSEnums.CreatureType.IcyBlueLizard)
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
    public static bool GorditoSquishyHead1(On.Lizard.orig_HitHeadShield orig, Lizard liz, Vector2 direction)
    {
        return !(liz is GorditoGreenie || !orig(liz, direction));
    }
    public static bool GorditoSquishyHead2(On.Lizard.orig_HitInMouth orig, Lizard liz, Vector2 direction)
    {
        return liz is not GorditoGreenie && orig(liz, direction);
    }
    public static void HailstormLizardViolence(On.Lizard.orig_Violence orig, Lizard liz, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType dmgType, float dmg, float bonusStun)
    {
        if (liz?.room is not null &&
            hitChunk is not null &&
            IsIncanStory(liz.room.game))
        {
            IncanHPandResistanceChanges(liz, dmgType, ref dmg, ref bonusStun);
        }
        orig(liz, source, directionAndMomentum, hitChunk, onAppendagePos, dmgType, dmg, bonusStun);
    }
    public static void IncanHPandResistanceChanges(Lizard liz, Creature.DamageType dmgType, ref float dmg, ref float bonusStun)
    {
        if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
        {
            dmg *= 0.615384f; // Base HP: 1.6 -> 2.6
        }
        else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)
        {
            dmg *= 2 / 3f; // Base HP: 5 -> 7.5
            bonusStun *= 2 / 3f;
        }
        else if (liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
        {
            dmg *= 1.5f; // Base HP: 2.4 -> 1.6
        }

        dmg /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(liz.Template, dmgType, false);
        bonusStun /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(liz.Template, dmgType, true);
    }

    public static void GorditoAct(On.Lizard.orig_Act orig, Lizard liz)
    {
        orig(liz);
        if (liz is GorditoGreenie)
        {
            foreach (BodyChunk chunk in liz.bodyChunks)
            {
                chunk.terrainSqueeze = Mathf.Lerp(chunk.terrainSqueeze, 0.9f, 0.8f);
            }

            if (liz.animation == Lizard.Animation.Lounge &&
                liz.bodyChunks is not null)
            {
                foreach (BodyChunk chunk in liz.bodyChunks)
                {
                    if (chunk.contactPoint.y == -1)
                    {
                        return;
                    }
                }
                liz.timeInAnimation--; // This timer usually ticks up when an animation is in progress. This line cancels that out, effectively pausing the timer.
            }
        }
    }
    public static void GorditoEnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard liz, Lizard.Animation anim, bool forceAnimChange)
    {
        orig(liz, anim, forceAnimChange);
        if (liz is GorditoGreenie g && anim == Lizard.Animation.Lounge)
        {
            g.BounceDir = liz.loungeDir.x != 0
                ? liz.loungeDir.x < 0 ? -1 : 1
                : liz.loungeDir.y != 0 ? liz.loungeDir.y < 0 ? -1 : 1 : Random.value < 0.5f ? -1 : 1;

            foreach (BodyChunk chunk in liz.bodyChunks)
            {
                chunk.vel.y = liz.loungeDir.y * 5f;
            }
        }
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

        CreatureTemplate.Relationship relat;
        if (ctr is Luminescipede &&
            dynamRelat.state is not null)
        {
            relat = RelationshipsWithLumins(lizAI, ctr as Luminescipede, dynamRelat);
            if (relat.type != CreatureTemplate.Relationship.Type.DoesntTrack)
            {
                return relat;
            }
        }

        if (IsIncanStory(lizAI.lizard.room.game))
        {
            relat = IncanSpecificRelationshipChanges(lizAI, lizType, ctr);
            if (relat.type != CreatureTemplate.Relationship.Type.DoesntTrack)
            {
                return relat;
            }
        }

        if (lizAI is ColdLizAI cldAI)
        {
            bool Scissorbird =
                ctr is MirosBird || (ctr is Vulture vul && vul.IsMiros);

            bool packMember = ctr is not null && lizAI.DynamicRelationship(ctr.abstractCreature).type == CreatureTemplate.Relationship.Type.Pack;

            if (cldAI.liz.IcyBlue)
            {
                if (ctr is Lizard && !packMember) // Eats other lizards if they're dead, otherwise fighting them to kill.
                {
                    if (otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
                    {
                        return cldAI.PackPower >= 0.6f ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f);
                    }

                    if (ctr?.room is not null &&
                        otherCtrType == CreatureTemplate.Type.YellowLizard)
                    {
                        float yellowCount = 0;
                        foreach (AbstractCreature absCtr in ctr.room.abstractRoom.creatures)
                        {
                            if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Lizard yellow && !yellow.State.dead && yellow.Template.type == CreatureTemplate.Type.YellowLizard)
                            {
                                yellowCount += 0.1f;
                            }
                        }
                        return cldAI.PackPower >= yellowCount || cldAI.NearAFreezer ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.75f) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.Min(1, yellowCount - cldAI.PackPower));
                    }

                    if (otherCtrType == CreatureTemplate.Type.GreenLizard ||
                        otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard ||
                        otherCtrType == CreatureTemplate.Type.CyanLizard ||
                        otherCtrType == CreatureTemplate.Type.RedLizard)
                    {

                        bool cyanLiz = otherCtrType == CreatureTemplate.Type.CyanLizard;

                        return cldAI.PackPower >= (cyanLiz ? 0.25f : 0.5f) ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Min(1, cldAI.PackPower * (cyanLiz ? 2 : 1))) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, Mathf.Min(1, cldAI.PackPower * (cyanLiz ? 1 : 2)));
                    }
                }

                if (ctr is Centipede cnt && (cnt.Red || cnt is Cyanwing))
                {
                    return ctr.dead
                        ? cldAI.NearAFreezer ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.6f) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.Lerp(0.2f, 1, cldAI.PackPower))
                        : cldAI.PackPower >= 0.7f && cldAI.NearAFreezer ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, cldAI.PackPower) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1);
                }

                if (ctr is Vulture or MirosBird) // Attacks and kills both vultures and Miros birds, then eats them.
                {
                    float threshold = Scissorbird ? 1 : 0.5f;
                    if (cldAI.NearAFreezer)
                    {
                        threshold -= 0.2f;
                    }

                    return cldAI.PackPower >= threshold ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, cldAI.PackPower) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(1, !Scissorbird ? 0.4f : 0.8f, cldAI.PackPower));
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
                    if (cldAI.PackPower < scavPackPower)
                    {
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Afraid, Mathf.Min(1, scavPackPower - cldAI.PackPower));
                    }
                }

                if (ctr is Player && lizAI.LikeOfPlayer(dynamRelat.trackerRep) < 0.5f)
                {
                    return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, cldAI.PackPower > 0.1f ? 1 : Mathf.InverseLerp(0, 600, dynamRelat.trackerRep.age));
                }

            }
            if (cldAI.liz.Freezer)
            {
                if (dynamRelat?.state is LizardAI.LizardTrackState lizTrackState &&
                    lizTrackState.vultureMask > 0 &&
                    lizAI.usedToVultureMask < 1200)
                {
                    lizAI.usedToVultureMask = 1200;
                }

                if (ctr is Spider)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Ignores, 1);
                }

                if (ctr is Lizard && !packMember) // Eats other lizards if they're dead, otherwise fighting them to kill.
                {
                    return otherCtrType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard
                        ? cldAI.PackPower >= 0.6f ?
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1) :
                            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f)
                        : !ctr.State.dead ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(otherCtrType == CreatureTemplate.Type.RedLizard ? 0.75f : 0.5f, 1, Mathf.InverseLerp(0.2f, 1, cldAI.PackPower))) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f);
                }

                if (ctr is Centipede cnt && (cnt.Red || cnt is Cyanwing))
                { // Wants to eat mega centipedes, but is only willing to attack from afar if it doesn't have backup.
                    return ctr.dead
                        ? new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, 1)
                        : cldAI.PackPower >= 0.7f || !Custom.DistLess(lizAI.lizard.DangerPos, cnt.DangerPos, 350) ?
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Max(0.5f, Mathf.InverseLerp(350, 50, Custom.Dist(lizAI.lizard.DangerPos, cnt.DangerPos)))) :
                        new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1);
                }

                if (ctr is Vulture or MirosBird) // Attacks and kills both vultures and Miros birds, then eats them.
                {
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(Scissorbird ? 0.8f : 0.6f, 1, Mathf.InverseLerp(0.2f, 1, cldAI.PackPower)));
                }
            }
        }

        if (ctr is ColdLizard)
        {
            relat = OtherLizRelationshipsWithColdLizards(lizType, ctr as ColdLizard);
            if (relat.type != CreatureTemplate.Relationship.Type.DoesntTrack)
            {
                return relat;
            }
        }

        if (dynamRelat is not null &&
            IsIncanStory(lizAI.lizard.room.game) &&
            lizAI.lizard is not ColdLizard &&
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
    public static CreatureTemplate.Relationship RelationshipsWithLumins(LizardAI lizAI, Luminescipede lmn, RelationshipTracker.DynamicRelationship dynamRelat)
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
            lizAI.lizard.Template.type != CreatureTemplate.Type.BlackLizard &&
            lizAI.lizard.Template.type != CreatureTemplate.Type.RedLizard &&
            lizAI.usedToVultureMask < (trackedState.vultureMask == 2 ? 1200 : 700))
        {
            lizAI.usedToVultureMask++;
            if (lizAI.lizard.Template.type == CreatureTemplate.Type.GreenLizard && trackedState.vultureMask < 2)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 0);
            }
            float scareFac = trackedState.vultureMask != 2 ?
                (lizAI.lizard.Template.type == CreatureTemplate.Type.BlueLizard ? 0.8f : 0.6f) :
                (lizAI.lizard.Template.type == CreatureTemplate.Type.GreenLizard ? 0.4f : 0.9f);
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(a: trackedState.vultureMask == 2 ? 1200 : 700, b: 600f, value: lizAI.usedToVultureMask) * scareFac);
        }
        return new CreatureTemplate.Relationship
            (CreatureTemplate.Relationship.Type.DoesntTrack, 0);
    }
    public static CreatureTemplate.Relationship IncanSpecificRelationshipChanges(LizardAI lizAI, CreatureTemplate.Type lizType, Creature target)
    {
        if ((lizType == CreatureTemplate.Type.Salamander || lizType == MoreSlugcatsEnums.CreatureTemplateType.EelLizard) && target is Leech)
        {
            return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Eats, 0.1f);
        }

        if (lizType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
        {
            if (target is Lizard && target.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Pack, 0.5f);
            }
            if (target is TubeWorm && (target.grabbedBy is null || target.grabbedBy.Count == 0))
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, target.dead ? 0.4f : 0.1f);
            }

            if (target is Spider && target?.room is not null && Custom.DistLess(target.DangerPos, lizAI.lizard.DangerPos, 400))
            {
                float spiderMass = 0;
                foreach (AbstractCreature absCtr in target.room.abstractRoom.creatures)
                {
                    if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Spider spd && Custom.DistLess(target.DangerPos, spd.DangerPos, 200))
                    {
                        spiderMass +=
                            spd.TotalMass / (spd.dead ? 3 : 1);
                    }
                }
                return spiderMass > lizAI.lizard.TotalMass * 1.33f
                    ? new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Afraid, spiderMass / 2)
                    : new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, 1 - (spiderMass / 2));
            }
        }
        return new CreatureTemplate.Relationship
            (CreatureTemplate.Relationship.Type.DoesntTrack, 0);
    }
    public static CreatureTemplate.Relationship OtherLizRelationshipsWithColdLizards(CreatureTemplate.Type lizType, ColdLizard icy)
    {
        return lizType == CreatureTemplate.Type.CyanLizard && icy.IcyBlue
            ? (icy.ColdAI.NearAFreezer || icy.ColdAI.PackPower >= 0.5f) ?
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, icy.ColdAI.NearAFreezer ? 1 : Mathf.Lerp(0.25f, 1, Mathf.InverseLerp(0.5f, 1, icy.ColdAI.PackPower))) :
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.5f, 0.25f, Mathf.InverseLerp(0, 0.5f, icy.ColdAI.PackPower)))
            : lizType == CreatureTemplate.Type.GreenLizard ||
            lizType == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard ||
            lizType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard
            ? lizType == CreatureTemplate.Type.GreenLizard &&
                icy.IcyBlue &&
                icy.dead &&
                !icy.ColdAI.NearAFreezer
                ? new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, 0.25f)
                : icy.ColdAI.PackPower >= 0.9f &&
                (icy.Freezer || icy.ColdAI.NearAFreezer)
                ? new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, 1)
                : new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.7f, 1, Mathf.InverseLerp(0, 0.9f, icy.ColdAI.PackPower)))
            : lizType == CreatureTemplate.Type.RedLizard &&
            icy.IcyBlue &&
            icy.ColdAI.PackPower >= 0.5f
            ? new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, Mathf.Lerp(0.7f, 1, Mathf.InverseLerp(0.5f, 1, icy.ColdAI.PackPower)))
            : new CreatureTemplate.Relationship
            (CreatureTemplate.Relationship.Type.DoesntTrack, 0);
    }


    public static void HailstormLizAIUpdate(On.LizardAI.orig_Update orig, LizardAI lizAI)
    {
        orig(lizAI);
        if (lizAI.lizard?.room is not null && LizardData.TryGetValue(lizAI.lizard, out _))
        {
            Lizard liz = lizAI.lizard;

            if (IsIncanStory(liz.room.game))
            {
                if (Weather.ErraticWindCycle &&
                    Weather.ExtremeWindIntervals[Weather.WindInterval] &&
                    liz.lizardParams.bodyMass < 1.6f)
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

            if (liz.Template.type == CreatureTemplate.Type.WhiteLizard &&
                Weather.ErraticWindCycle &&
                Weather.ExtremeWindIntervals[Weather.WindInterval] &&
                liz.room.blizzardGraphics is not null)
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
        if (IsIncanStory(lizAI?.lizard?.room?.game) &&
            (lizAI.lizard.Template.type == CreatureTemplate.Type.Salamander || lizAI.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
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
                _ = c.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] BEEP BOOP! SOMETHING WITH CYAN LIZARDS AND ERRATIC WIND CYCLES BROKE! REPORT THIS!");
            }
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
        if (liz.lizard is ColdLizard frz)
        {
            if (frz.Freezer)
            {
                return new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, bodyFatness, 0.8f);
            }
            if (frz.IcyBlue)
            {
                float massLerp = Mathf.InverseLerp(1.4f, 1.6f, liz.lizard.lizardParams.bodyMass);
                headSize = Mathf.Lerp(0.475f, 0.55f, massLerp) * 2;
                bodyFatness = Custom.ClampedRandomVariation(0.5f, 0.03f, 0.375f) * 2f;
                return new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, bodyFatness, Mathf.Lerp(0.25f, 0.75f, massLerp));
            }
        }
        return liz.lizard is GorditoGreenie
            ? new LizardGraphics.IndividualVariations(headSize, bodyFatness, tailLength, Random.Range(0.875f, 0.925f), 0.5f)
            : orig(liz);
    }
    public static void HailstormLizardGraphics(On.LizardGraphics.orig_ctor orig, LizardGraphics lG, PhysicalObject ow)
    {
        orig(lG, ow);
        if (lG.lizard is not null && LizardData.TryGetValue(lG.lizard, out _))
        {
            if (lG.lizard.Template.type == HSEnums.CreatureType.FreezerLizard)
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
                if (num == 0 || Random.value < (0.5f - (0.125f * num)))
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IceSpikeTuft(lG, cosmeticSprites));
                    num++;
                }
                if (Random.value < (num == 0 ? 0.6f : 0.2f))
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IcyRhinestones(lG, cosmeticSprites));
                }

                _ = lG.AddCosmetic(cosmeticSprites, new ArmorIceSpikes(lG, cosmeticSprites));

                Random.state = state;
            }
            else if (lG.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
            {
                Random.State state = Random.state;
                Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;
                int num = 0;
                bool LSS = false;

                if (Random.value < 0.8888f)
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new IceSpikeTuft(lG, cosmeticSprites));
                }

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
                if ((!LSS && Random.value < (0.7f - (0.2f * num))) || Random.value < 0.04f)
                {
                    cosmeticSprites = lG.AddCosmetic(cosmeticSprites, new LongHeadScales(lG, cosmeticSprites));
                }

                _ = lG.AddCosmetic(cosmeticSprites, new ArmorIceSpikes(lG, cosmeticSprites));

                Random.state = state;
            }
            else if (IsIncanStory(lG.lizard.room?.game) && lG.lizard.abstractCreature.Winterized)
            {
                if (lG.lizard.Template.type == CreatureTemplate.Type.Salamander || lG.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
                {
                    Random.State state = Random.state;
                    Random.InitState(lG.lizard.abstractCreature.ID.RandomSeed);

                    int cosmeticSprites = lG.startOfExtraSprites + lG.extraSprites;

                    _ = lG.AddCosmetic(cosmeticSprites, new ShortBodyScales(lG, cosmeticSprites));

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

                            lG.tail[j].rad = rad * (lG.lizard.Template.type == CreatureTemplate.Type.GreenLizard ? 3f : 1.15f);
                            lG.tail[j].connectionRad = connectionRad * 1.15f;
                        }
                    }

                    Random.state = state;
                }
            }
        }
    }

    public static void ColdLizardSpriteReplacements(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(liz, sLeaser, rCam, timeStacker, camPos);
        if (liz is null)
        {
            return;
        }

        if (liz.lizard is ColdLizard cLiz)
        {
            int RNG = cLiz.abstractCreature.ID.RandomSeed;
            if (cLiz.IcyBlue)
            {
                sLeaser.sprites[liz.SpriteHeadStart].color = IcyBlueHeadColor(liz, timeStacker, cLiz.effectColor2);
                sLeaser.sprites[liz.SpriteHeadStart + 3].color = IcyBlueHeadColor(liz, timeStacker, liz.effectColor);

                if (liz.lizard.TotalMass > 1.55f)
                {
                    float headAngleNumber = Mathf.Lerp(liz.lastHeadDepthRotation, liz.headDepthRotation, timeStacker);
                    int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);
                    sLeaser.sprites[liz.SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("FreezerEyes0." + headAngle);
                }

                int colorSpacing = (int)Mathf.Lerp(-0.25f, 0, Mathf.InverseLerp(1.4f, 1.6f, liz.lizard.TotalMass));
                TriangleMesh tail = sLeaser.sprites?[liz.SpriteTail] as TriangleMesh;
                for (int i = 0; i < tail?.verticeColors?.Length; i++)
                {
                    tail.verticeColors[i] = Color.Lerp(
                        sLeaser.sprites[liz.SpriteBodyCirclesStart].color,
                        sLeaser.sprites[(RNG % 5 == 0) ? liz.SpriteHeadStart : liz.SpriteHeadStart + 3].color,
                        colorSpacing + Mathf.InverseLerp(2, 6, i));
                }
            }
            else if (cLiz.Freezer)
            {
                // Visuals-related variables
                float headAngleNumber = Mathf.Lerp(liz.lastHeadDepthRotation, liz.headDepthRotation, timeStacker);
                int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);

                // Position-related variables
                _ = Custom.PerpendicularVector(Vector2.Lerp(liz.drawPositions[0, 1], liz.drawPositions[0, 0], timeStacker) - Vector2.Lerp(liz.head.lastPos, liz.head.pos, timeStacker));

                /* Sprite Replacements */
                // Jaw
                sLeaser.sprites[liz.SpriteHeadStart].element = Futile.atlasManager.GetElementWithName("FreezerJaw0." + headAngle);
                sLeaser.sprites[liz.SpriteHeadStart].color = FreezerHeadColor(liz, timeStacker, cLiz.effectColor2);

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
                        (RNG % 3 == 0 ? cLiz.effectColor2 : liz.effectColor);
                }

                /* Tail recoloring */
                TriangleMesh tail = sLeaser.sprites?[liz.SpriteTail] as TriangleMesh;
                for (int i = 0; i < tail?.verticeColors?.Length; i++)
                {
                    tail.verticeColors[i] = Color.Lerp(
                        sLeaser.sprites[liz.SpriteBodyCirclesStart].color,
                        sLeaser.sprites[(RNG % 5 == 0) ? liz.SpriteHeadStart : liz.SpriteHeadStart + 3].color,
                        Mathf.InverseLerp(2, 6, i));
                }

            }
        }
    }

    public static void ColdLizardSpriteLayering(On.LizardGraphics.orig_AddToContainer orig, LizardGraphics liz, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(liz, sLeaser, rCam, newContainer);
        if (liz.lizard is ColdLizard cLiz && cLiz.Freezer)
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
        if (liz?.lizard is null)
        {
            return;
        }

        if (liz.lizard is ColdLizard cLiz)
        {
            if (cLiz.IcyBlue)
            {
                liz.ColorBody(sLeaser, IcyBlueBodyColor(liz));
            }
            else if (cLiz.Freezer)
            {
                liz.ColorBody(sLeaser, FreezerBodyColor(liz));
            }
        }
    }

    public static Color ColdLizardBodyColors2(On.LizardGraphics.orig_BodyColor orig, LizardGraphics liz, float f)
    {
        if (liz?.lizard is not null)
        {
            if (liz.lizard is ColdLizard cLiz)
            {
                if (cLiz.IcyBlue)
                {
                    return IcyBlueBodyColor(liz);
                }
                else if (cLiz.Freezer)
                {
                    return FreezerBodyColor(liz);
                }
            }
            if (liz is GorditoGraphics gg)
            {
                return gg.bodyColor;
            }
        }
        return orig(liz, f);
    }

    #endregion

    //----------------------------------------------------------------------------------

    #region Custom Lizard Colors
    public static Color FreezerHeadColor(LizardGraphics liz, float timeStacker, Color baseColor)
    {
        float flickerIntensity = 1f - Mathf.Pow(0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(liz.lastBlink, liz.blink, timeStacker) * 2f * Mathf.PI)), 1.5f + (liz.lizard.AI.excitement * 1.5f));
        flickerIntensity = Mathf.Lerp(flickerIntensity, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(liz.lastVoiceVisualization, liz.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(liz.lastVoiceVisualizationIntensity, liz.voiceVisualizationIntensity, timeStacker));
        Color whiteOut = Color.Lerp(baseColor, Color.white, 0.6f);
        return Color.Lerp(whiteOut, baseColor, flickerIntensity);
    }
    public static Color FreezerBodyColor(LizardGraphics liz)
    {
        return Color.Lerp(liz.effectColor, Color.Lerp(Custom.HSL2RGB(220 / 360f, 0.44f, 0.455f), Custom.HSL2RGB(0, 0, 0.45f), 0.92f), 0.92f);
    }

    public static Color IcyBlueHeadColor(LizardGraphics liz, float timeStacker, Color baseColor)
    {
        float flickerIntensity = 1f - Mathf.Pow(0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(liz.lastBlink, liz.blink, timeStacker) * 2f * Mathf.PI)), 1.5f + (liz.lizard.AI.excitement * 1.5f));
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

}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------