/*namespace Hailstorm;

sealed class Hooks
{
	internal static void Apply()
	{
        IL.OverseerAbstractAI.HowInterestingIsCreature += il =>
        {
            ILCursor c = new (il);
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("BlackLizard"),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label))
            && label != null)
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == HailstormCreatures.FreezerLizard);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Plugin.logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature!");
        };
        On.LizardLimb.ctor += (orig, self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair) =>
        {
            orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
            if (owner is LizardGraphics l && l.lizard?.Template.type == HailstormCreatures.FreezerLizard)
            {
                self.grabSound = SoundID.Lizard_Green_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_Green_Foot_Release;
            }
        };
        On.LizardVoice.GetMyVoiceTrigger += (orig, self) =>
        {
            SoundID res = orig(self);
            if (self.lizard is Lizard l && l.Template.type == HailstormCreatures.FreezerLizard)
            {
                string[] array = new[] { "A", "B", "C", "D", "E" };
                List<SoundID> list = new ();
                for (int i = 0; i < array.Length; i++)
                {
                    SoundID soundID = SoundID.None;
                    string text2 = "Lizard_Voice_Pink_" + array[i];
                    if (SoundID.values.entries.Contains(text2))
                        soundID = new(text2);
                    if (soundID != SoundID.None && soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            return res;
        };
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += (orig, type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate) =>
        {
            if (type == HailstormCreatures.FreezerLizard)
            {
                CreatureTemplate temp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                LizardBreedParams breedParams = (temp.breedParameters as LizardBreedParams)!;
                // Check LizardBreeds in the game's files if you want to compare to other lizards' stats.
                temp.type = type;
                temp.name = "FreezerLizard"; // Internal lizard name.
                breedParams.baseSpeed = 5f; // Self-explanatory.
                breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f); // Ground movement.
                breedParams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f); // Tunnel movement.
                breedParams.terrainSpeeds[3] = new(1f, 0.8f, 0.4f, 1f); // Pole movement speeds.
                breedParams.terrainSpeeds[4] = new(1f, 0.8f, 0.8f, 1f); // Background movement.
                breedParams.terrainSpeeds[5] = new(0.8f, 1f, 1f, 1f); // Ceiling movement.
                breedParams.standardColor = new(129f / 255f, 40f / 51f, 236f / 255f); // The lizard's base color.
                breedParams.biteDelay = 16; // Delay between wanting to bite and actually biting?
                breedParams.biteInFront = 20f;
                breedParams.biteRadBonus = 10f; // Bonus reach for the actual bite attack.
                breedParams.biteHomingSpeed = 1f; // Head tracking speed while trying to go for a bite.
                breedParams.biteChance = 0.7f; // Chance of going for a bite.
                breedParams.attemptBiteRadius = 55f; // How far it'll try to bite you from.
                breedParams.getFreeBiteChance = 0.9f; // Chance of sneaking in an extra bite?
                breedParams.biteDamage = 3f; // Damage done to other creatures.
                breedParams.biteDamageChance = 0f; // Chance of killing the player on bite.
                temp.baseStunResistance = 2.4f;
                temp.baseDamageResistance = 3f; // HP value.
                temp.damageRestistances[(int)Creature.DamageType.Bite, 0] = 6f; // Divides incoming bite damage by 6.
                temp.damageRestistances[(int)Creature.DamageType.Explosion, 0] = 2f; // Halves incoming explosive damage.
                temp.damageRestistances[(int)Creature.DamageType.Stab, 0] = 1.5f; // Reduces incoming stab damage by a third.
                temp.wormGrassImmune = true; // Determines whether Wormgrass will try to eat this lizard or not.
                breedParams.regainFootingCounter = 7;
                breedParams.bodyMass = 4f; // Weight.
                breedParams.bodySizeFac = 1f; // Size.
                breedParams.bodyRadFac = 3f;
                breedParams.floorLeverage = 4f;
                breedParams.maxMusclePower = 11f;
                breedParams.wiggleSpeed = 0.3f;
                breedParams.wiggleDelay = 20;
                breedParams.bodyStiffnes = 0.35f;
                breedParams.swimSpeed = 0.5f; // How well the lizard can swim.
                breedParams.idleCounterSubtractWhenCloseToIdlePos = 80;
                breedParams.danger = 0.8f; // How threatening the game considers this creature.
                breedParams.aggressionCurveExponent = 0.9f;
                breedParams.headShieldAngle = 120f;
                breedParams.findLoungeDirection = 1f;
                breedParams.preLoungeCrouch = 24; // LUNGE, not  l o u n g e. Wind-up time for this lizard's lunge.
                breedParams.preLoungeCrouchMovement = -0.1f; // Movement speed during lunge.
                breedParams.loungeDistance = 100f; // How far the lizard goes when lunging.
                breedParams.loungeSpeed = 3.75f; // Self-explanatory.
                breedParams.loungeMaximumFrames = 48; // How long this lizard's lunge attack can last.
                breedParams.loungePropulsionFrames = 24; // How long this lizard spends accelerating?
                breedParams.loungeJumpyness = 0.2f; // How much they jump for their lunge.
                breedParams.loungeDelay = 160; // Lunge cooldown.
                breedParams.riskOfDoubleLoungeDelay = 0.8f; // Chance of cooldown being doubled.
                breedParams.postLoungeStun = 80;
                breedParams.loungeTendensy = 1f; // How eager this lizard is to go for a lunge. 0 means "Never", and 1 means "Is 100 meters from your location and approaching rapidly".
                breedParams.canExitLoungeWarmUp = true; // Determines whether this lizard can cancel out of winding up their lunge.
                breedParams.canExitLounge = false; // Determines whether this lizard can cancel out of an actual lunge. This, uh, seems to pretty much negate postLoungeStun.
                temp.visualRadius = 1500f; // How far the lizard can see.
                temp.waterVision = 0.2f; // Vision in water.
                temp.throughSurfaceVision = 0.5f; // How well the lizard can see through the surface of water.
                breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0.33f); // The angle that the lizard can see creatures perfectly within.
                breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 0.75f); // The angle through which the lizard can see creatures at all.
                breedParams.biteDominance = 0.66f;
                breedParams.limbSize = 1.5f; // Length of limbs.
                breedParams.limbThickness = 1.5f; // Chonkiness of limbs.
                breedParams.stepLength = 0.7f;
                breedParams.liftFeet = 0.2f; // How much lizards bring their feet up when crawling around.
                breedParams.feetDown = 0.4f; // How much lizards bring their feet down when crawling around.
                breedParams.noGripSpeed = 0.2f;
                breedParams.limbSpeed = 12f;
                breedParams.limbQuickness = 0.7f;
                breedParams.limbGripDelay = 1;
                breedParams.legPairDisplacement = 0.4f;
                breedParams.walkBob = 3f; // How bumpy a lizard's movement is.
                breedParams.smoothenLegMovement = true;
                breedParams.tailSegments = 3; // Number of tail segments this lizard has.
                breedParams.tailStiffness = 100f;
                breedParams.tailStiffnessDecline = 1f;
                breedParams.tailLengthFactor = 500f; // How long each tail segment is.
                breedParams.tailColorationStart = 2f;
                breedParams.tailColorationExponent = 6f;
                breedParams.headSize = 1.05f; // Scale of head sprites.
                breedParams.neckStiffness = 0.4f; // How resistant the lizard's head is to turning.
                breedParams.jawOpenAngle = 80f; // How wide this lizard will open their mouth.
                breedParams.jawOpenLowerJawFac = 0.66f; // How much the lizard's bottom jaw turns when its mouth opens.
                breedParams.jawOpenMoveJawsApart = 12f; // How far the lizard's bottom jaw lowers when its mouth opens.
                breedParams.headGraphics = new int[5] {2, 2, 2, 2, 2}; // This might be important?
                breedParams.framesBetweenLookFocusChange = 70;
                breedParams.tamingDifficulty = 4f; // How stubborn the lizard is to taming attempts. Higher numbers make them harder to tame.
                temp.waterPathingResistance = 10f; // How much this lizard will try to avoid water.
                temp.dangerousToPlayer = breedParams.danger;
                temp.doPreBakedPathing = false;
                temp.requireAImap = true;
                breedParams.tongue = false; // Does lizor use tongue?
                temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        };
        On.Lizard.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (self.Template.type == HailstormCreatures.FreezerLizard)
            {
                Random.State state = Random.state;
                Random.InitState(abstractCreature.ID.RandomSeed);
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.58f, .08f, .6f), .3f, Custom.ClampedRandomVariation(.8f, .15f, .1f));
                Random.state = state;
            }
        };
        On.LizardGraphics.ctor += (orig, self, ow) =>
        {
            orig(self, ow);
            if (self.lizard.Template.type == HailstormCreatures.FreezerLizard)
            {
                Random.State state = Random.state;
                Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
                int num = self.startOfExtraSprites + self.extraSprites;
                num = self.AddCosmetic(num, new LongHeadScales(self, num));
                if (Random.value < .2f)
                    num = self.AddCosmetic(num, new LongHeadScales(self, num));
                if (Random.value < .3f)
                    num = self.AddCosmetic(num, new TailFin(self, num));
                Random.state = state;
            }
        };
    }
}*/