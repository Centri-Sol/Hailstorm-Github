namespace Hailstorm;

public class IncanFeatures
{

    public static void Hooks()
    {

        On.Player.ctor += PlayerCWT;
        On.SlugcatStats.ctor += IncanStats;

        On.Player.ThrownSpear += IncanSpearThrows;

        // Movement
        On.Player.MovementUpdate += IncanMovementUpdate;
        On.Player.Jump += IncanJumpBoosts;
        On.Player.UpdateAnimation += IncanAnimationUpdate;
        On.Player.UpdateBodyMode += IncanOtherMobility;
        //On.Player.TerrainImpact += TerrainImpact;

        IncanILHooks();
        On.SlugcatStats.NourishmentOfObjectEaten += SaintNoYouCantEatTheseFuckOff_WaitNoPUTAWAYYOURASCENSIONPOWERSDONOTBLOWUPMYMINDLIKEPANCA;
        On.Player.ObjectEaten += IncanFoodEffects;
        On.Player.CanEatMeat += INCANNOSTOPEATINGSLUGCATS;
        On.Player.Update += HailstormPlayerUpdate;
        //On.Player.UpdateAnimation += RollAnimations;

        On.Player.Collide += HailstormPlayerCollision;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == IncanInfo.Incandescent);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void PlayerCWT(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (!IncanInfo.IncanData.TryGetValue(self, out _))
        {
            IncanInfo.IncanData.Add(self, new IncanInfo(self));
        }
    }
    public static void IncanStats(On.SlugcatStats.orig_ctor orig, SlugcatStats stats, SlugcatStats.Name slugcat, bool starving)
    {
        orig(stats, slugcat, starving);
        if (slugcat == IncanInfo.Incandescent)
        {
            stats.generalVisibilityBonus = starving ? 0.25f : 0.5f;
            if (ModManager.Expedition &&
                Custom.rainWorld.ExpeditionMode &&
                Expedition.ExpeditionGame.activeUnlocks.Contains("unl-agility"))
            {
                stats.runspeedFac = starving ? 2.3f : 2;
                stats.poleClimbSpeedFac = starving ? 2.3f : 2;
                stats.corridorClimbSpeedFac = starving ? 2.3f : 2;
            }
        }
    }
    public static bool INCANNOSTOPEATINGSLUGCATS(On.Player.orig_CanEatMeat orig, Player self, Creature ctr)
    {
        if (IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) && player.isIncan)
        {
            if (ctr.Template.type == CreatureTemplate.Type.Slugcat ||
                ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                return false;
            }
        }
        return orig(self, ctr);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanSpearThrows(On.Player.orig_ThrownSpear orig, Player self, Spear spr)
    {
        orig(self, spr);
        if (IncanInfo.IncanData.TryGetValue(self, out IncanInfo incan) && incan.isIncan)
        {
            spr.spearDamageBonus *= HSRemix.IncanSpearDamageMultiplier.Value;
            if (MMF.cfgUpwardsSpearThrow.Value && spr.setRotation.Value.y == 1f && self.bodyMode != Player.BodyModeIndex.ZeroG)
            {
                spr.spearDamageBonus *= 1.25f; // Negates the slight damage penalty that upthrows normally have
                if (spr?.room is not null)
                {
                    spr.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, spr.firstChunk.pos, 0.8f, 2f);
                    spr.room.AddObject(new Explosion.ExplosionLight(spr.firstChunk.pos, 280f, 1f, 7, incan.FireColor));
                    spr.room.AddObject(new FireSpikes(spr.room, spr.firstChunk.pos, 14, 15f, 9f, 5f, 90f, incan.FireColor, self.ShortCutColor()));
                }
            }
            else
            {
                spr.spearDamageBonus *= 0.7f;
            }
            if (self.Malnourished)
            {
                spr.spearDamageBonus *= 0.8f;
                spr.firstChunk.vel.x *= 0.875f;
            }
            if (!spr.bugSpear)
            {
                spr.firstChunk.vel.x *= 0.875f;
            }
        }        
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanMovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        if (player.highJump)
        {
            player.highJump = false;
        }

        if (self.superLaunchJump >= 20)
        {
            if (!player.longJumpReady)
            {
                player.longJumpReady = true;
            }

            if (self.input[0].y < 0 &&
                player.ReadyToMoveOn)
            {
                player.highJump = true;
                self.killSuperLaunchJumpCounter = 15;
            }
        }
        else if (player.longJumpReady)
        {
            player.longJumpReady = false;
        }
    }

    public static void IncanJumpBoosts(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        bool starving = self.Malnourished;

        if (self.animation == Player.AnimationIndex.Flip)
        {
            self.mainBodyChunk.vel.x *= starving ? 3f : 2.50f;
            self.mainBodyChunk.vel.y *= starving ? 2f : 1.66f;
            self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.mainBodyChunk.pos, self.flipFromSlide ? 1f : 0.6f, 1.5f);
        }
        else if (player.longJumpReady)
        {
            player.longJumpReady = false;
            player.longJumping = true;
            player.justLongJumped = true;
            if (player.highJump)
            {
                player.highJump = false;
                self.bodyChunks[0].vel.x *= 0;
                self.bodyChunks[1].vel.x *= 0;
                self.mainBodyChunk.vel.y += starving ? 30 : 24;
                self.animation = Player.AnimationIndex.Flip;
            }
            else
            {
                self.bodyChunks[0].vel.x *= starving ? 2.2f  : 1.8f;
                self.bodyChunks[1].vel.x *= starving ? 1.7f  : 1.48f;
                self.bodyChunks[0].vel.y *= starving ? 1.35f  : 1.25f;
            }
        }
        else if (!player.longJumping)
        {
            self.mainBodyChunk.vel.y *= 1.33f;
        }
    }

    public static void IncanAnimationUpdate(On.Player.orig_UpdateAnimation orig, Player self)
    {
        bool canBoost =
            self is not null &&
            self.waterJumpDelay == 0;

        orig(self);
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        if (canBoost &&
            self.waterJumpDelay > 0 &&
            self.animation == Player.AnimationIndex.DeepSwim)
        {
            Vector2 direction = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
            if (!ModManager.MMF ||
                !MMF.cfgFreeSwimBoosts.Value)
            {
                self.airInLungs += 0.005f;
            }
            self.bodyChunks[0].vel += direction * 1.5f;
        }
        if (self.animation == Player.AnimationIndex.DeepSwim &&
            (self.input[0].ZeroGGamePadIntVec.x != 0 || self.input[0].ZeroGGamePadIntVec.y != 0))
        {
            if (self.swimCycle > 0)
            {
                self.bodyChunks[0].vel += Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 0.7f * Mathf.Lerp(self.swimForce, 1f, 0.5f) * self.bodyChunks[0].submersion * 0.125f;
            }
        }

        if (self.animation == Player.AnimationIndex.Flip)
        {
            if (!self.flipFromSlide &&
                player.singeFlipTimer < 15)
            {
                player.singeFlipTimer++;
                if (player.singeFlipTimer > 3)
                {
                    FlipSingeCollisionCheck(self, player);
                }
            }
        }
        else if (player.singeFlipTimer != 0)
        {
            player.singeFlipTimer = 0;
        }

        if (self.animation == Player.AnimationIndex.Roll && player.ReadyToMoveOn)
        {
            FlipSingeCollisionCheck(self, player);
        }
    }
    
    public static void IncanOtherMobility(On.Player.orig_UpdateBodyMode orig, Player self)
    {
        orig(self);
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        bool starving = self.Malnourished;

        if (self.animation == Player.AnimationIndex.BellySlide)
        {
            self.mainBodyChunk.vel.x *= starving ? 1.4f : 1.35f;
            self.mainBodyChunk.vel.y *= 1.05f;
        }
        else if (self.animation == Player.AnimationIndex.RocketJump)
        {
            if (starving)
            {
                self.mainBodyChunk.vel.x *= 1.02f;
            }
            self.mainBodyChunk.vel.y *= starving ? 1.06f : 1.05f;
        }
        else if (self.animation == Player.AnimationIndex.Roll)
        {
            self.bodyChunks[0].vel *= starving? 1.11f : 1.1f;
            self.bodyChunks[1].vel *= starving? 1.11f : 1.1f;
            if (self.rollCounter > 15 && player.rollExtender < (starving? 75 : 60) && self.input[0].downDiagonal != 0) // Extends roll duration.
            {
                player.rollExtender++;
                self.rollCounter--;
            }
            if (self.stopRollingCounter > 3 && player.rollFallExtender < 12) // Extends how long you can fall mid-roll without your roll being canceled.
            {
                player.rollFallExtender++;
                self.stopRollingCounter--;
            }
        }

        if (player.longJumping && (
                self.bodyChunks[0].contactPoint != default ||
                self.bodyChunks[1].contactPoint != default ||
                self.Submersion > 0 ||
                self.Stunned ||
                self.dead ||
                self.bodyMode == Player.BodyModeIndex.Swimming ||
                self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ||
                self.bodyMode == Player.BodyModeIndex.ZeroG ||
                self.animation == Player.AnimationIndex.AntlerClimb ||
                self.animation == Player.AnimationIndex.GrapplingSwing ||
                self.animation == Player.AnimationIndex.VineGrab ||
                (self.grasps[0]?.grabbed is not null && self.HeavyCarry(self.grasps[0].grabbed)) ||
                (self.grasps[1]?.grabbed is not null && self.HeavyCarry(self.grasps[1].grabbed))))
        {
            player.longJumping = false;
        }


        if (self.animation != Player.AnimationIndex.Roll)
        {
            player.rollExtender = 0;
            player.rollFallExtender = 0;
        }

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    /* Wall/ceiling-rolling code
    private static void TerrainImpact(On.Player.orig_TerrainImpact orig, Player self, int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        orig(self, chunk, direction, speed, firstContact);
        if (CWT.PlayerData.TryGetValue(self, out IncanPlayer player) && player.isIncan)
        {
            // This is literally just the code for activating a Roll, but the speed requirements are lowered.
            // This exists alongside the usual Roll-activation code, so I have to make sure both can't activate at once.
            if (self.input[0].downDiagonal != 0 && self.animation != Player.AnimationIndex.Roll && (speed > 10f || self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.RocketJump || self.animation == IncanPlayer.WallRoll || self.animation == IncanPlayer.CeilingRoll) && direction.y < 0 && self.allowRoll > 0 && self.consistentDownDiagonal > ((speed > 16f) ? 1 : 6))
            {
                self.animation = Player.AnimationIndex.Roll;
                if (speed > 10 && speed <= 12)
                {
                    if (self.animation == Player.AnimationIndex.RocketJump && self.rocketJumpFromBellySlide)
                    {
                        self.bodyChunks[1].vel.y += 3f;
                        self.bodyChunks[1].pos.y += 3f;
                        self.bodyChunks[0].vel.y -= 3f;
                        self.bodyChunks[0].pos.y -= 3f;
                    }
                    self.rollDirection = self.input[0].downDiagonal;
                    self.rollCounter = 0;
                    self.bodyChunks[0].vel.x = Mathf.Lerp(self.bodyChunks[0].vel.x, 9f * self.input[0].x, 0.7f);
                    self.bodyChunks[1].vel.x = Mathf.Lerp(self.bodyChunks[1].vel.x, 9f * self.input[0].x, 0.7f);
                    self.standing = false;
                }
            }

            // A check for activating a Wall Roll.
            if (self.input[0].downDiagonal != 0 &&
                self.allowRoll > 0 &&
                (self.IsTileSolid(0, self.input[0].x, 0) ||
                self.IsTileSolid(1, self.input[0].x, 0)) &&
                (self.animation == Player.AnimationIndex.Roll ||
                self.animation == Player.AnimationIndex.Flip ||
                self.animation == Player.AnimationIndex.RocketJump ||
                self.bodyMode == Player.BodyModeIndex.WallClimb))
            {
                self.room.PlaySound(SoundID.Slugcat_Roll_Init, self.mainBodyChunk.pos, 1f, 1f);
                self.animation = IncanPlayer.WallRoll;
                player.wallRollDirection = player.wallRollingFromCeiling? -1 : 1;
                self.rollCounter = 0;
                self.stopRollingCounter = 0;
                player.rollExtender -= 10;
                self.bodyChunks[0].vel.y = Mathf.Lerp(self.bodyChunks[0].vel.y, 9f * -self.input[0].y, 0.7f) * player.wallRollDirection;
                self.bodyChunks[1].vel.y = Mathf.Lerp(self.bodyChunks[1].vel.y, 9f * -self.input[0].y, 0.7f) * player.wallRollDirection;
                self.standing = false;
            }

            // A check for activating a Ceiling Roll.
            if (self.input[0].downDiagonal != 0 &&
                self.allowRoll > 0 &&
                !self.IsTileSolid(0, 0, -1) &&
                !self.IsTileSolid(1, 0, -1) &&
                (self.IsTileSolid(0, 0, 1) ||
                self.IsTileSolid(1, 0, 1)) &&
                (self.animation == IncanPlayer.WallRoll || self.animation == Player.AnimationIndex.Flip))
            {
                self.room.PlaySound(SoundID.Slugcat_Roll_Init, self.mainBodyChunk.pos, 1f, 1f);
                self.animation = IncanPlayer.CeilingRoll;
                self.rollDirection = -self.input[0].downDiagonal;
                self.rollCounter = 0;
                self.stopRollingCounter = 0;
                player.rollExtender -= 10;
                self.bodyChunks[0].vel.x = Mathf.Lerp(self.bodyChunks[0].vel.x, 9f * -self.input[0].x, 0.8f) * -1;
                self.bodyChunks[1].vel.x = Mathf.Lerp(self.bodyChunks[1].vel.x, 9f * -self.input[0].x, 0.8f) * -1;
                self.standing = false;
            }
        }
    }*/

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // This allows certain foods to temporarily buff the Incandescent's glow.
    // The actual buffing is done near the bottom of the HailstormPlayerUpdate method.
    public static void IncanFoodEffects(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible food)
    {
        orig(self, food);
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player))
        {
            return;
        }

        if (IsIncanStory(self.room?.game) && food is GlowWeed && self.airInLungs < 1)
        {
            self.airInLungs = Mathf.Min(1, self.airInLungs + (2 * self.slugcatStats.lungsFac));
        }

        if (!player.isIncan)
        {
            return;
        }

        if (food is Luminescipede)
        {
            player.firefuel += 1200;
        }
        else if (food is FireEgg)
        {
            player.firefuel += 1200;
            self.Hypothermia -= 0.3f;
        }
        else if (food is SLOracleSwarmer || food is SSOracleSwarmer)
        {
            player.firefuel += 4800;
        }
        else if (food is KarmaFlower)
        {
            player.firefuel += 7200;
            player.waterGlow += 7200;
        }
        else if (food is SwollenWaterNut)
        {
            player.soak += 1800;
            self.room.PlaySound(SoundID.Medium_Object_Into_Water_Slow, self.bodyChunks[1].pos, 0.9f, Random.Range(1.4f, 1.6f));
            self.room.PlaySound(SoundID.Firecracker_Burn, self.bodyChunks[1].pos, 0.75f, Random.Range(1.75f, 2f));
        }
        else // All food types below this point cannot increase glow radius past a certain point.                 
        {    // The foods *above* this point have no such restriction.

            int fuel = 0;
            if (food is JellyFish || (food is Centipede cnt2 && cnt2.Template.type == CreatureTemplate.Type.SmallCentipede))
            {
                fuel = 1600;
            }
            else if (food is GlowWeed || (food is Centipede cnt1 && cnt1.Template.type == HailstormCreatures.InfantAquapede))
            {
                fuel = 1200;
                player.waterGlow += 1200;
            }
            else if (food is SlimeMold SM)
            {
                if (SM.big)
                {
                    fuel = 2400;
                    player.soak -= 2400;
                }
                else
                {
                    fuel = 1200;
                    player.soak -= 1200;
                }
            }
            else if (food is LillyPuck)
            {
                fuel = 800;
                player.waterGlow += 4800;
            }
            else if (food is Mushroom)
            {
                fuel = 2400;
            }

            if (HSRemix.IncanNoFireFuelLimit.Value is false && player.firefuel + fuel > 2400) // Prevents fireFuel from going over 2400.
            {
                fuel = Mathf.Max(2400 - player.firefuel, 0);
            }
            player.firefuel += fuel;
        }
    }
    public static int SaintNoYouCantEatTheseFuckOff_WaitNoPUTAWAYYOURASCENSIONPOWERSDONOTBLOWUPMYMINDLIKEPANCA(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcat, IPlayerEdible eatenobject)
    {
        return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint && (eatenobject is Luminescipede || eatenobject is PeachSpiderCritob)
            ? -1
            : orig(slugcat, eatenobject);
    }

    public static void IncanILHooks()
    {

        IL.Player.GrabUpdate += IL => // Makes Glowweed edible underwater in Incan's campaign.
        {
            ILLabel? label = null;
            ILCursor c = new(IL);
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_mainBodyChunk"),
                x => x.MatchCallvirt<BodyChunk>("get_submersion"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchBlt(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Player self) =>
                {
                    if (IsIncanStory(self?.room?.game) && self.grasps is not null)
                    {
                        if (self.grasps[0]?.grabbed is not null &&
                            self.grasps[0].grabbed is GlowWeed)
                        {
                            return true;
                        }
                        else
                        if ((self.grasps[0]?.grabbed is null || self.grasps[0].grabbed is not IPlayerEdible) &&
                            self.grasps[1]?.grabbed is not null &&
                            self.grasps[1].grabbed is GlowWeed)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                );
                c.Emit(OpCodes.Brtrue, label);
            }
            else Plugin.logger.LogError("[Hailstorm] An IL hook for eating Glowweed underwater broke! Report this, would ya?");
        };

        IL.Player.Update += IL => // Prevents Incan from getting exhausted from starvation 
        {
            ILCursor c = new(IL);
            if (c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<Player>("get_slugcatStats"),
                    x => x.MatchLdfld<SlugcatStats>(nameof(SlugcatStats.malnourished))
                    )
                )
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((bool Malnourished, Player self) => Malnourished && (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo Incan) || !Incan.isIncan));
            }
            else Plugin.logger.LogError("[Hailstorm] AN IL hook for Incan starve exhaustion broke! Lemme know about this!");
        };

    }
    public static void UpdateFood(Player self, IncanInfo Incan, bool wasMalnourished)
    {
        self.playerState.foodInStomach = Custom.IntClamp(Mathf.FloorToInt(Incan.floatFood), 0, self.slugcatStats.maxFood);
        if (self.playerState.foodInStomach >= self.slugcatStats.maxFood)
        {
            self.playerState.quarterFoodPoints = 0;
        }
        else
        {
            self.playerState.quarterFoodPoints = Mathf.FloorToInt(Custom.Decimal(Incan.floatFood) * 4f);
        }
        while (Incan.FoodCounter >= 1f)
        {
            Incan.FoodCounter -= 1f;
            self.abstractCreature.world.game.GetStorySession.saveState.totFood++;
        }
        if (wasMalnourished &&
            !self.Malnourished &&
            self.FoodInStomach < self.MaxFoodInStomach)
        {
            self.SetMalnourished(self.FoodInStomach < self.MaxFoodInStomach);
        }
    }

    //--------------------------------------------
    public static void HailstormPlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    { // Manages everything that needs to be constantly updated for players, including things that are exclusive to the Incandescent.
        orig(self, eu);
        if (self is null)
        {
            return;
        }

        //--------------------------------------------------------------------------------------------------

        IncanStoryTutorialText(self);

        //--------------------------------------------------------------------------------------------------

        NewHypothermiaMechanics(self);

        //--------------------------------------------------------------------------------------------------

        if (IncanInfo.IncanData.TryGetValue(self, out IncanInfo Incan) &&
            Incan.isIncan)
        {
            Incan.Update(self, eu);
        }
    }
    public static void IncanStoryTutorialText(Player self)
    {
        if (Dialogue.IsIncanStory(self?.room?.game) &&
            !self.room.game.rainWorld.ExpeditionMode &&
            self.room.game.GetStorySession.saveState.cycleNumber <= 2 &&
            self.room.game.cameras[0]?.hud?.textPrompt is not null &&
            ((self.room.world.rainCycle.maxPreTimer > 0 && self.room.world.rainCycle.preTimer == self.room.world.rainCycle.maxPreTimer - 360) || (self.room.world.rainCycle.maxPreTimer <= 0 && self.room.world.rainCycle.timer == 360)) &&
            (self.playerState.playerNumber == 0 || self.playerState.playerNumber == -1))
        {
            if (self.room.game.GetStorySession.saveState.cycleNumber == 0)
            {
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Your flaming tail requires a great amount of energy to sustain.", 200, 360, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("It will keep you warm as you venture out to find food.", 0, 300, darken: false, hideHud: true);
            }
            if (self.room.game.GetStorySession.saveState.cycleNumber == 1 &&
                self.room.game.GetStorySession.saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(MoreSlugcatsEnums.GhostID.MS) &&
                self.room.game.GetStorySession.saveState.deathPersistentSaveData.ghostsTalkedTo[MoreSlugcatsEnums.GhostID.MS] == 2)
            {
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Thanks to your tail, you possess increased mobility and a heightened jump.", 200, 400, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("With the heat from your flame, your mobility can be used to attack other creatures.", 0, 420, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Be wary, though; with every heat attack performed, your flame will lose some of its warmth.", 0, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Don't get too careless! Keep an eye on your glow to see how much warmth you still have.", 0, 440, darken: false, hideHud: true);
            }
            if (self.room.game.GetStorySession.saveState.cycleNumber == 2)
            {
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Water will dampen your flame, weakening its glow and making you more vulnerable to the cold.", 200, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("The longer you stay in water, the longer the effect will last, and the stronger it will be.", 0, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Eating certain things may help your tail resist these effects... or boost it in other ways.", 0, 440, darken: false, hideHud: true);
            }
        }
    }
    public static void NewHypothermiaMechanics(Player self)
    {
        if (self.isSlugpup)
        {
            self.Hypothermia -= 0.00015f;
        }

        if (self.room is not null)
        {
            if (self?.grasps is not null)
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i]?.grabbed is not null &&
                        self.grasps[i].grabbed is IceChunk ice)
                    {
                        self.Hypothermia -= 0.0003f * ice.Chill;
                    }
                }
            }
            if (self.objectInStomach is not null)
            {
                float stomachHeat = 0;
                if (self.Hypothermia > 0.001f)
                {
                    if (self.objectInStomach.realizedObject is Lantern && !self.room.blizzard)
                    {
                        stomachHeat += Mathf.Lerp(0.0005f, 0f, self.HypothermiaExposure);
                    }
                    if (self.objectInStomach.realizedObject is FireEgg)
                    {
                        stomachHeat += 0.0007f;
                    }
                    if (stomachHeat != 0 &&
                        IncanInfo.IncanData.TryGetValue(self, out IncanInfo Incan) &&
                        Incan.isIncan)
                    {
                        stomachHeat /= 3;
                    }
                }
                if (self.objectInStomach.realizedObject is IceChunk ice)
                {
                    stomachHeat += (self.room.world.game.IsArenaSession ? -0.00075f : -0.0003f) * ice.Chill;
                }
                self.Hypothermia -= stomachHeat;
            }

            if (self.room.game.IsArenaSession) // Sets up Hypothermia death mechanics for non-cold Arenas.
            {
                if (!self.dead &&
                    self.graphicsModule is not null &&
                    self.Hypothermia >= 1.5f)
                {
                    (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (self.Hypothermia * 0.75f); // Head shivers
                }

                if (self.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
                {
                    self.Hypothermia -= 0.00015f;
                    if (!self.dead && self.Hypothermia >= 1.5f)
                    {

                        self.playerState.permanentDamageTracking += (self.Hypothermia > 1.8f) ? 0.002f : 0.001f; // Damages you if you're too cold.

                        if (self.playerState.permanentDamageTracking >= 1)
                        {
                            self.Die();
                        }
                    } // ^ Makes sure to kill you if your health is drained via Hypothermia, since that doesn't happen automatically.
                }
            }
        }

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------        

    /* More wall/ceiling-rolling code
    public static void RollAnimations(On.Player.orig_UpdateAnimation orig, Player self)
    {
        orig(self);
        if (!CWT.PlayerData.TryGetValue(self, out IncanPlayer player) || !player.isIncan)
        {
            return;
        }

        // Movement stuff for wall rolling.
        if (self.animation == IncanPlayer.WallRoll && self.input[0].downDiagonal != 0)
        {
            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = IncanPlayer.WallRoll;
            bool noWallContact = false;
            int wallDirection = self.input[0].x;
            Vector2 perpVector = Custom.PerpendicularVector(self.bodyChunks[0].pos, self.bodyChunks[1].pos);

            self.bodyChunks[0].vel *= 0.8f;
            self.bodyChunks[1].vel *= 0.8f;
            self.bodyChunks[0].vel += perpVector * 2f * player.wallRollDirection;
            self.bodyChunks[1].vel -= perpVector * 2f * player.wallRollDirection;
            self.mainBodyChunk.vel.x = 0f * wallDirection;
            self.AerobicIncrease(0.01f);               
            Debug.Log("beep | " + player.wallStopTimer + " | " + self.rollCounter + " | " + (self.IsTileSolid(0, wallDirection, 0) || self.IsTileSolid(1, wallDirection, 0)) + " | " + (self.IsTileSolid(0, wallDirection*2, 0) || self.IsTileSolid(1, wallDirection*2, 0)));
            if (self.bodyChunks[1].onSlope == player.wallRollDirection || self.bodyChunks[0].onSlope == player.wallRollDirection)
            {
                Debug.Log("bap");
                self.bodyChunks[0].pos += perpVector * player.wallRollDirection;
                self.bodyChunks[1].pos -= perpVector * player.wallRollDirection;
            }
            if (!self.IsTileSolid(0, wallDirection, 0) && !self.IsTileSolid(1, wallDirection, 0) && self.bodyChunks[0].ContactPoint.y <= 0 && self.bodyChunks[1].ContactPoint.y <= 0)
            {
                if (self.IsTileSolid(0, wallDirection*2, 0) || self.IsTileSolid(1, wallDirection*2, 0))
                {
                    Debug.Log("bedurp");
                    self.bodyChunks[0].vel *= 0.85f;
                    self.bodyChunks[1].vel *= 0.85f;
                    self.bodyChunks[0].pos.x += 2.5f * wallDirection;
                    self.bodyChunks[1].pos.x += 2.5f * wallDirection;
                }
                else
                {
                    noWallContact = true;
                }
            }
            else
            {
                self.bodyChunks[0].vel.y += 1.1f * player.wallRollDirection;
                self.bodyChunks[1].vel.y += 1.1f * player.wallRollDirection;
                self.canJump = System.Math.Max(self.canJump, 5);
                for (int C = 0; C < 2; C++)
                {
                    if (self.IsTileSolid(C, self.rollDirection, 0) && !self.IsTileSolid(C, self.rollDirection, 1) && !self.IsTileSolid(0, 0, 1) && !self.IsTileSolid(1, 0, 1))
                    {
                        Debug.Log((object)"roll up ledge");

                        self.bodyChunks[0].vel *= 0.7f;
                        self.bodyChunks[1].vel *= 0.7f;
                        self.bodyChunks[0].pos.y += 2.5f;
                        self.bodyChunks[1].pos.y += 2.5f;
                        self.bodyChunks[0].vel.y -= self.gravity;
                        self.bodyChunks[1].vel.y -= self.gravity;
                        self.animation = Player.AnimationIndex.Roll;
                        break;
                    }
                }
            }
            if (noWallContact)
            {
                player.wallStopTimer++;
            } else
            {
                player.wallStopTimer = 0;
            }
            if ((((self.rollCounter > 15 && self.input[0].x == 0 && self.input[0].downDiagonal == 0) || (self.rollCounter > 30f + 80f * self.Adrenaline * (self.isSlugpup ? 0.5f : 1f) && (!self.isGourmand || self.gourmandExhausted)) || self.input[0].x != wallDirection) && self.bodyChunks[0].pos.y > self.bodyChunks[1].pos.y) ||
                (self.rollCounter > 60f + 80f * self.Adrenaline * (self.isSlugpup ? 0.5f : 1f) && (!self.isGourmand || self.gourmandExhausted)) ||
                player.wallStopTimer > 6)
            {
                player.wallRollDirection = 0;
                self.mainBodyChunk.vel *= 0.2f;
                self.room.PlaySound(SoundID.Slugcat_Roll_Finish, self.mainBodyChunk.pos, 1f, 1f);
                self.animation = Player.AnimationIndex.None;
                self.standing = self.input[0].y > -1;
            }
        }


        // Movement stuff for ceiling rolling.
        if (self.animation == IncanPlayer.CeilingRoll && self.input[0].downDiagonal != 0)
        {
            self.bodyMode = Player.BodyModeIndex.Default;
            bool noCeilingContact = false;                
            Vector2 perpVector = Custom.PerpendicularVector(self.bodyChunks[0].pos, self.bodyChunks[1].pos);

            self.bodyChunks[0].vel *= 0.9f;
            self.bodyChunks[1].vel *= 0.9f;
            self.bodyChunks[0].vel += perpVector * 2f * self.rollDirection;
            self.bodyChunks[1].vel -= perpVector * 2f * self.rollDirection;
            self.mainBodyChunk.vel.y = 0.2f;
            self.AerobicIncrease(0.01f);                
            Debug.Log("beep | " + player.ceilingStopTimer + " | " + self.rollCounter + " | " + (self.IsTileSolid(0, 0, 2) || self.IsTileSolid(1, 0, 2)) + " | " + (self.IsTileSolid(0, 0, 3) || self.IsTileSolid(1, 0, 3)));
            if (self.bodyChunks[1].onSlope == self.rollDirection || self.bodyChunks[0].onSlope == self.rollDirection)
            {
                Debug.Log("bap");
                self.bodyChunks[0].pos += perpVector * self.rollDirection;
                self.bodyChunks[1].pos -= perpVector * self.rollDirection;
            }
            if (!self.IsTileSolid(0, 0, 1) && !self.IsTileSolid(1, 0, 1) && self.bodyChunks[0].ContactPoint.y <= 0 && self.bodyChunks[1].ContactPoint.y <= 0)
            {
                if (self.IsTileSolid(0, 0, 2) || self.IsTileSolid(1, 0, 2))
                {
                    Debug.Log("bedurp");
                    self.bodyChunks[0].vel *= 0.7f;
                    self.bodyChunks[1].vel *= 0.7f;
                    self.bodyChunks[0].pos.y += 2.5f;
                    self.bodyChunks[1].pos.y += 2.5f;
                }
                else
                {
                    noCeilingContact = true;
                }
            }
            else
            {
                self.bodyChunks[0].vel.x += 1.1f * self.rollDirection;
                self.bodyChunks[1].vel.x += 1.1f * self.rollDirection;
                self.canJump = System.Math.Max(self.canJump, 5);                    
            }
            if (noCeilingContact)
            {
                player.ceilingStopTimer++;
            }
            else
            {
                player.ceilingStopTimer = 0;
            }
            if ((((self.rollCounter > 15 && self.input[0].y > -1 && self.input[0].downDiagonal == 0) || (self.rollCounter > 30f + 80f * self.Adrenaline * (self.isSlugpup ? 0.5f : 1f) && (!self.isGourmand || self.gourmandExhausted)) || self.input[0].x != -self.rollDirection) && self.bodyChunks[0].pos.y < self.bodyChunks[1].pos.y) ||
                (self.rollCounter > 60f + 80f * self.Adrenaline * (self.isSlugpup ? 0.5f : 1f) && (!self.isGourmand || self.gourmandExhausted)) ||
                player.ceilingStopTimer > 6)
            {
                self.rollDirection = 0;
                self.mainBodyChunk.vel *= 0.2f;
                self.bodyChunks[0].vel *= 0.2f;
                self.bodyChunks[1].vel *= 0.2f;
                self.room.PlaySound(SoundID.Slugcat_Roll_Finish, self.mainBodyChunk.pos, 1f, 1f);
                self.animation = Player.AnimationIndex.None;
                self.standing = self.input[0].y > -1;
            }
        }
    }*/

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // The following variables and method implement damage-on-collision for the Incandescent's many movement tricks.
    // This method is unfortunately HUGE, so be prepared for a great wall of code.
    public static void HailstormPlayerCollision(On.Player.orig_Collide orig, Player self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (otherObject is not Creature target)
        {
            orig(self, otherObject, myChunk, otherChunk);
            return;
        }

        bool hitSmallCreature =
            target.abstractCreature.creatureTemplate.smallCreature;

        bool canHarm =
            !target.dead &&
            !(ModManager.CoopAvailable && target is Player && !Custom.rainWorld.options.friendlyFire) &&
            target.abstractCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;

        //--------------------------------------
        // Going to deal with Gourmand stuff first, and then we'll move on to Incandescent-exclusive stuff.
        // This is so Gourmand's movement attacks properly trigger code for the body armor of Freezer Lizards.
        // ...I had to put this BEFORE orig because the gourmandRoll check ends up always being false if put after.
        // This method ends up being messier because of it.
        //
        #region Gourmand Collision

        bool gourmandRoll =
            self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand &&
            self.animation == Player.AnimationIndex.Roll &&
            self.gourmandAttackNegateTime <= 0;

        if (gourmandRoll)
        {
            if (canHarm)
            {
                if (RainWorld.ShowLogs) Debug.Log("SLUGROLLED! Stun: " + 120f + " | Damage: " + 1);
                
                self.room.ScreenMovement(self.bodyChunks[0].pos, self.mainBodyChunk.vel * self.bodyChunks[0].mass * 5f * 0.1f, Mathf.Max((self.bodyChunks[0].mass - 30f) / 50f, 0f));
                self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                target.SetKillTag(self.abstractCreature);
                target.Violence(self.mainBodyChunk, (Vector2?)new Vector2(self.mainBodyChunk.vel.x * 5f, self.mainBodyChunk.vel.y), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, 1, 120);
                self.animation = Player.AnimationIndex.None;
                self.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
                self.rollDirection = 0;
                if ((target.State is HealthState targetHS && targetHS.ClampedHealth == 0f) || target.State.dead)
                    self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
                
                else self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.mainBodyChunk, loop: false, 1.2f, 1f);

                self.gourmandAttackNegateTime = 20;
            }
        }
        else if (self.SlugSlamConditions(otherObject))
        {
            float dmg = 4f;
            float stn = 120;
            if (self.animation == Player.AnimationIndex.BellySlide || self.animation == Player.AnimationIndex.RocketJump)
            {
                dmg = 0.25f;
                stn = 50;
            }
            else
            {
                float lastFallPos = self.lastGroundY - self.firstChunk.pos.y;
                stn *= Mathf.Floor(Mathf.Abs(self.mainBodyChunk.vel.magnitude) / 7f);
                dmg =
                    lastFallPos < 100f? dmg * 0.5f :
                        lastFallPos < 200f? dmg :
                            lastFallPos < 320f? dmg * 2f :
                                lastFallPos < 600f? dmg * 3f : dmg * 5f;
            }
            if (stn > 240) stn = 240;
            
            if (dmg < 0f) dmg = 0f;
            
            if (stn < 25) stn = 0;
            
            if (dmg != 0f || stn != 0)
            {
                if (!target.dead)
                {
                    if (RainWorld.ShowLogs)
                        Debug.Log("SLUGSMASH! Slide: " + (self.animation == Player.AnimationIndex.BellySlide || self.animation == Player.AnimationIndex.RocketJump) + " | Incoming speed: " + Mathf.Max(self.mainBodyChunk.vel.y, self.mainBodyChunk.vel.magnitude) + " | Dist: " + (self.lastGroundY - self.firstChunk.pos.y) + " | Damage: " + dmg + " | Stun: " + stn/40 + "s");
                    
                    self.room.ScreenMovement(self.bodyChunks[0].pos, self.mainBodyChunk.vel * dmg * self.bodyChunks[0].mass * 0.1f, Mathf.Max((dmg * self.bodyChunks[0].mass - 30f) / 50f, 0f));
                    self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                    target.SetKillTag(self.abstractCreature);
                    target.Violence(self.mainBodyChunk, (Vector2?)new Vector2(self.mainBodyChunk.vel.x, self.mainBodyChunk.vel.y), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, dmg, (int)stn);
                    if (otherObject is BigJellyFish bJelly)
                    {
                        hitSmallCreature = true;
                        bJelly.Die();
                    }
                    if (target.State is HealthState targHS2 && targHS2.ClampedHealth == 0f || target.State.dead)
                        self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
                    
                    else self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.mainBodyChunk, loop: false, 1.2f, 1f);                    
                }
                else self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                
                if (self.mainBodyChunk.vel.magnitude < 40f && !hitSmallCreature)
                {
                    self.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
                    self.bodyChunks[0].vel = self.mainBodyChunk.vel;
                    self.bodyChunks[1].vel = self.mainBodyChunk.vel;
                }
                else self.mainBodyChunk.vel.Scale(new Vector2(0.9f, 0.9f));                
            }
            if (self.animation == Player.AnimationIndex.BellySlide)
            {
                self.mainBodyChunk.vel.x /= 3f;
                self.rollCounter = 99;
            }
        }
        #endregion

        //--------------------------------------------------------------

        orig(self, otherObject, myChunk, otherChunk);

        if (CWT.CreatureData.TryGetValue(target, out CreatureInfo cI) &&
            IncanInfo.IncanData.TryGetValue(self, out IncanInfo Incan) && Incan.isIncan)
        {
            if (canHarm && (cI.impactCooldown == 0 || Incan.impactCooldown == 0))
            {
                IncanCollision(self, Incan, target, cI, myChunk, otherChunk, hitSmallCreature);
            }
            else
            {
                self.room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, self.mainBodyChunk);
            }
        }

    }
    public static void IncanCollision(Player self, IncanInfo Incan, Creature target, CreatureInfo cI, int myChunk, int otherChunk, bool hitSmallCreature)
    {
        if (!Incan.CollisionDamageValues(self, out float DMG, out float STUN, out float HEATLOSS, out int BURNTIME))
        {
            return;
        }
        if (target is Player) // Making sure we don't get Artificer 2 in Arena Mode.
        {
            DMG /= 2f;
            STUN /= 2f;
            BURNTIME /= 2;
            HEATLOSS *= 2f;
        }


        bool HitFly = target is Fly;

        if (HitFly)
        {
            HEATLOSS = 0.1f;
        }
        else
        {
            cI.impactCooldown = 20;
            Incan.impactCooldown = 20;
        }

        
        self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
        target.SetKillTag(self.abstractCreature);


        Creature.DamageType DMGTYPE;
        if (BURNTIME > 0)
        {
            DMGTYPE = HailstormDamageTypes.Heat;

            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 35f, Incan.FireColor, self.ShortCutColor()));
            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 45f, Incan.FireColor, self.ShortCutColor()));

            if (target.SpearStick(null, DMG, target.bodyChunks[otherChunk], null, self.mainBodyChunk.vel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out AbsCtrInfo aI))
            {
                aI.AddBurn(self.abstractCreature, target, otherChunk, BURNTIME, Incan.FireColor, self.ShortCutColor());
            }

            if (RainWorld.ShowLogs)
            {
                Debug.Log("Player " + (self.playerState.playerNumber + 1) + " burned something! | Damage: " + DMG + " | Burn Time: " + BURNTIME / 40f + "s | Stun: " + STUN / 40f + "s");
            }
        }
        else
        {
            DMGTYPE = Creature.DamageType.Blunt;

            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 20f, Incan.FireColor, self.ShortCutColor()));
            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 30f, Incan.FireColor, self.ShortCutColor()));

            if (RainWorld.ShowLogs)
            {
                Debug.Log("Player " + (self.playerState.playerNumber + 1) + " BONKED something! | Damage: " + DMG + " | Stun: " + STUN / 40f + "s | Impact velocity: " + Mathf.Max(self.mainBodyChunk.vel.y, self.mainBodyChunk.vel.magnitude) + " | Distance: " + (self.lastGroundY - self.firstChunk.pos.y));
            }
        }

        Incan.fireSmoke ??= new HailstormFireSmokeCreator(self.room);
        for (int s = 0; s < 10; s++)
        {
            Vector2 SmokeVel;
            if (DMGTYPE == Creature.DamageType.Blunt)
            {
                SmokeVel = Custom.DegToVec(Custom.VecToDeg(self.mainBodyChunk.vel) + Random.Range(-15, 15));
            }
            else
            {
                SmokeVel = Custom.RNV() * Random.Range(8f, 12f);
            }

            if (Incan.Glow is not null &&
                self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 300f) &&
                Incan.fireSmoke.AddParticle(Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), SmokeVel, 40) is Smoke.FireSmoke.FireSmokeParticle bonkFireSmoke)
            {
                bonkFireSmoke.colorFadeTime = 35;
                bonkFireSmoke.effectColor = Incan.FireColor;
                bonkFireSmoke.colorA = self.ShortCutColor();
                bonkFireSmoke.rad *= 5f * Mathf.InverseLerp(0, 400, Incan.Glow.rad);
            }
        }


        // Damages target. Quick note: Slugcats don't have actual HP without the DLC. As long as attacks don't deal at least 1 damage to them, they're effectively invincible.
        target.Violence(self.bodyChunks[myChunk], self.mainBodyChunk.vel, target.bodyChunks[otherChunk], null, DMGTYPE, DMG, STUN);
        if (target is Player player) // WITH the DLC, though, that's not an issue. You've just gotta make sure to address Slugcat HP separately from Violence, since it... wasn't integrated directly into Violence, for some reason.
        {
            player.playerState.permanentDamageTracking += DMG;
            if (player.playerState.permanentDamageTracking >= 1)
            {
                player.Die();
            }
        }
        if (HitFly && Incan.longJumping)
        {
            self.Stun(15);
        }

        // Heat transfer
        self.Hypothermia += HEATLOSS;
        target.Hypothermia -= HEATLOSS * 0.75f;

        // Damage sounds
        if (target.State.dead || (target.State is HealthState targetHP && targetHP.ClampedHealth == 0f))
        {
            self.room.PlaySound(SoundID.Rock_Hit_Wall, self.mainBodyChunk, loop: false, 1.2f, 1f);
            self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
        }
        else
        {
            self.room.PlaySound(SoundID.Rock_Hit_Wall, self.mainBodyChunk, loop: false, 1.7f, 1f);
        }

        if (HitFly)
        {
            return;
        }

        if (self.animation == Player.AnimationIndex.Roll ||
                (self.animation == Player.AnimationIndex.Flip && self.flipFromSlide)) // Stops movement if rolling or whiplashing. Flips keep going.
        {
            self.animation = Player.AnimationIndex.None;
            self.rollDirection = 0;
            self.bodyChunks[0].vel *= 0f;
            self.bodyChunks[1].vel *= 0f;
        }
        else
            if (self.animation == Player.AnimationIndex.RocketJump ||
                Incan.longJumping) // Bounces you backward if rocket-jumping or long-jumping. Slides keep going.
        {
            self.animation = Player.AnimationIndex.None;
            Incan.longJumping = false;
            if (self.mainBodyChunk.vel.magnitude < 40f && !hitSmallCreature)
            {
                self.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
                self.bodyChunks[0].vel = self.mainBodyChunk.vel;
                self.bodyChunks[1].vel = self.mainBodyChunk.vel;
            }
            else
            {
                self.mainBodyChunk.vel.Scale(new Vector2(0.9f, 0.9f));
            }
        }

    }
    public static void FlipSingeCollisionCheck(Player self, IncanInfo Incan)
    {
        if (self?.room is null || !Incan.isIncan)
        {
            return;
        }

        PlayerGraphics selfGraphics = self.graphicsModule as PlayerGraphics;
        TailSegment tailEnd = selfGraphics.tail[selfGraphics.tail.Length - 1];
        bool powerFlameWheel = Incan.ReadyToMoveOn && self.animation == Player.AnimationIndex.Roll;

        foreach (AbstractCreature absCtr in self.room.abstractRoom.creatures)
        {
            if (absCtr?.realizedCreature is null || absCtr.realizedCreature == self)
            {
                continue;
            }
            Creature target = absCtr.realizedCreature;
            bool smallCreature = target.abstractCreature.creatureTemplate.smallCreature;
            if (target.dead ||
                target.bodyChunks.Length < 1 ||
                (powerFlameWheel && !smallCreature) || (!Incan.ReadyToMoveOn && smallCreature) ||
                (ModManager.CoopAvailable && target is Player && !Custom.rainWorld.options.friendlyFire) ||
                target.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                !CWT.CreatureData.TryGetValue(target, out CreatureInfo cI) ||
                !(cI.impactCooldown == 0 || Incan.impactCooldown == 0))
            {
                continue;
            }
            if (self.grasps is not null)
            {
                bool grabbedByPlayer = false;
                for (int g = 0; g < self.grasps.Length; g++)
                {
                    if (self.grasps[g]?.grabbed is not null && self.grasps[g]?.grabbed == target)
                    {
                        grabbedByPlayer = true;
                        break;
                    }
                }
                if (grabbedByPlayer)
                {
                    continue;
                }

            }

            foreach (BodyChunk chunk in target.bodyChunks)
            {
                if (!Custom.DistLess(tailEnd.pos, chunk.pos, tailEnd.rad + chunk.rad + 40f))
                {
                    continue;
                }

                cI.impactCooldown = 20;
                Incan.impactCooldown = 20;

                float DAMAGE = 0.25f;
                float STUN = 60;
                int BURNTIME = 250;
                float HEATLOSS = 0.06f;
                if (target is Player)
                {
                    DAMAGE /= 2f;
                    STUN /= 2f;
                    BURNTIME /= 2;
                    HEATLOSS *= 2f;
                }
                if (Incan.inArena)
                {
                    HEATLOSS *= 2f;
                }

                Vector2 hitVel = Custom.DirVec(tailEnd.pos, chunk.pos) * 2f;
                target.SetKillTag(self.abstractCreature);
                target.Violence(self.mainBodyChunk, new Vector2?(hitVel), chunk, null, HailstormDamageTypes.Heat, DAMAGE, STUN);
                if (target is Player plr)
                {
                    plr.playerState.permanentDamageTracking += 0.25f;
                    if (plr.playerState.permanentDamageTracking >= 1)
                    {
                        plr.Die();
                    }
                }
                if (target is Lizard liz &&
                    liz.Template.type != CreatureTemplate.Type.RedLizard &&
                    liz.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
                {
                    liz.turnedByRockCounter =
                        (liz.Template.type == HailstormCreatures.Freezer ||
                        liz.Template.type == CreatureTemplate.Type.GreenLizard ||
                        liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard) ? 20 : 40;
                    liz.turnedByRockDirection = (int)Mathf.Sign(chunk.pos.x - tailEnd.pos.x);
                }
                if (target.SpearStick(null, 0.25f, chunk, null, hitVel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out AbsCtrInfo aI))
                {
                    aI.AddBurn(self.abstractCreature, target, chunk.index, BURNTIME, Incan.FireColor, self.ShortCutColor());
                }

                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 35f, Incan.FireColor, self.ShortCutColor()));
                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 45f, Incan.FireColor, self.ShortCutColor()));
                self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk.pos, 1.2f, 1);
                if (target.State is HealthState targetHP && targetHP.ClampedHealth == 0f || target.State.dead)
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.DangerPos, 0.75f, Random.Range(1.75f, 2));
                    self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk.pos, 1.7f, 1f);
                }
                else
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.DangerPos, 1f, Random.Range(1.75f, 2));
                }

                if (RainWorld.ShowLogs)
                    Debug.Log("Player " + (self.playerState.playerNumber + 1) + " burned something! | Damage: 0.25 | Burn Time: 12.5s | Stun: 1.25s");

                self.Hypothermia += HEATLOSS;
                target.Hypothermia -= HEATLOSS * 0.75f;

                Incan.fireSmoke ??= new HailstormFireSmokeCreator(self.room);
                for (int f = 0; f < 10; f++)
                {
                    if (Incan.Glow is not null && self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 300f) && Incan.fireSmoke.AddParticle(Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Custom.RNV() * Random.Range(8f, 12f), 40) is Smoke.FireSmoke.FireSmokeParticle bonkFireSmoke)
                    {
                        bonkFireSmoke.colorFadeTime = 35;
                        bonkFireSmoke.effectColor = Incan.FireColor;
                        bonkFireSmoke.colorA = self.ShortCutColor();
                        bonkFireSmoke.rad *= 5f * Mathf.InverseLerp(0, 400, Incan.Glow.rad);
                    }
                }

                break;
            }
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IncanHypothermiaBodyContact(On.Creature.orig_HypothermiaBodyContactWarmup orig, Creature no, Creature self, Creature other)
    {
        if (self is null || other is null ||
            other.Hypothermia >= self.Hypothermia ||
            (CustomTemplateInfo.IsFireCreature(self) && CustomTemplateInfo.IsFireCreature(other)) ||
            (CustomTemplateInfo.IsColdCreature(self.Template.type) && CustomTemplateInfo.IsColdCreature(other.Template.type)))
        {
            return orig(no, self, other);
        }
        if (!CustomTemplateInfo.IsFireCreature(self) &&
            !CustomTemplateInfo.IsColdCreature(self.Template.type) &&
            !CustomTemplateInfo.IsFireCreature(other))
        {
            return orig(no, self, other);
        }

        float selfHypo = self.Hypothermia;
        float otherHypo = other.Hypothermia;

        orig(no, self, other);

        self.Hypothermia = selfHypo;
        other.Hypothermia = otherHypo;
        selfHypo = 0;
        otherHypo = 0;

        if (CustomTemplateInfo.IsFireCreature(self))
        {
            otherHypo += Mathf.Lerp(other.Hypothermia, self.Hypothermia, 0.012f) - other.Hypothermia;
            if (CustomTemplateInfo.IsColdCreature(other.Template.type))
            {
                otherHypo *= 1.25f;
                selfHypo = 0;
            }
            else if (other.Template.BlizzardAdapted)
            {
                selfHypo = Mathf.Lerp(self.Hypothermia, 0, 0.001f) - self.Hypothermia;
            }
            else
            {
                selfHypo = Mathf.Lerp(self.Hypothermia, other.Hypothermia, 0.001f) - self.Hypothermia;
            }

            if (self.room?.blizzardGraphics is not null)
            {
                selfHypo *= Mathf.InverseLerp(Weather.LateBlizzardTime(self.room.world), 0, self.room.world.rainCycle.timer);
            }
        }
        else if (CustomTemplateInfo.IsColdCreature(self.Template.type))
        {
            if (self is ColdLizard)
            {
                otherHypo += Mathf.Lerp(0.0018f, 0.0036f, Mathf.InverseLerp(1.4f, 1.8f, other.TotalMass));
            }
            else if (self is Chillipede)
            {
                otherHypo += 0.0036f;
            }
            selfHypo += Mathf.Lerp(self.Hypothermia, other.Hypothermia, 0.004f) - self.Hypothermia;
            if (CustomTemplateInfo.IsFireCreature(other))
            {
                selfHypo *= 1.25f;
                otherHypo *= 1.25f;
                if (self.State is HealthState hp1)
                {
                    hp1.health -= 0.00125f / self.Template.baseDamageResistance / self.Template.damageRestistances[HailstormDamageTypes.Heat.index, 0];
                }
                if (other.State is HealthState hp2)
                {
                    hp2.health -= 0.00125f / other.Template.baseDamageResistance / other.Template.damageRestistances[HailstormDamageTypes.Cold.index, 0];
                }
            }
        }
        else if (CustomTemplateInfo.IsFireCreature(other))
        {
            selfHypo += Mathf.Lerp(self.Hypothermia, other.Hypothermia, 0.006f) - self.Hypothermia;
            otherHypo += Mathf.Lerp(other.Hypothermia, self.Hypothermia, 0.012f) - other.Hypothermia;
        }

        self.Hypothermia += selfHypo;
        other.Hypothermia += otherHypo;

        return true;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

}

public class EmberSprite : CosmeticSprite
{
    public float lifeTime;
    public float life;
    public float lastLife;
    public Color color;
    public float size;


    public EmberSprite(Vector2 pos, Color color, float size)
    {
        base.pos = pos;
        this.color = color;
        this.size = size;
        lastPos = pos;
        vel = Custom.RNV() * 1.5f * Random.value;
        life = 1f;
        lifeTime = Mathf.Lerp(10f, 40f, Random.value);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        vel *= 0.8f;
        vel.y += 0.4f;
        vel += Custom.RNV() * Random.value * 0.5f;
        lastLife = life;
        life -= 1f / lifeTime;
        if (life < 0f) Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("deerEyeB");
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        float lifetimeMult = Mathf.Lerp(lastLife, life, timeStacker);
        sLeaser.sprites[0].scale = size * lifetimeMult;
        sLeaser.sprites[0].color = color;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}

public class FireSpikes : ExplosionSpikes
{
    Color color2;
    public FireSpikes(Room room, Vector2 pos, int spikes, float innerRad, float lifeTime, float width, float length, Color color, Color color2) : base(room, pos, spikes, innerRad, lifeTime, width, length, color)
    {
        base.room = room;
        this.innerRad = innerRad;
        base.pos = pos;
        this.color = color;
        this.color2 = color2;
        this.lifeTime = lifeTime;
        base.spikes = spikes;
        values = new float[spikes, 3];
        dirs = (Vector2[])(object)new Vector2[spikes];
        float num = Random.value * 360f;
        for (int i = 0; i < spikes; i++)
        {
            float num2 = (float)i / (float)spikes * 360f + num;
            dirs[i] = Custom.DegToVec(num2 + Mathf.Lerp(-0.5f, 0.5f, Random.value) * 360f / (float)spikes);
            if (room.GetTile(pos + dirs[i] * (innerRad + length * 0.4f)).Solid)
            {
                values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, Random.value) * 0.5f;
                values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, Random.value) * 0.5f;
            }
            else
            {
                values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, Random.value);
                values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, Random.value);
            }
            values[i, 1] = width * Mathf.Lerp(0.6f, 1.4f, Random.value);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[spikes];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new TriangleMesh.Triangle(i * 3, i * 3 + 1, i * 3 + 2);
        }
        TriangleMesh triangleMesh = new ("Futile_White", array, customColor: true);
        sLeaser.sprites[0] = triangleMesh;
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float num = time + timeStacker;
        TriangleMesh tMesh = sLeaser.sprites[0] as TriangleMesh;
        for (int i = 0; i < spikes; i++)
        {
            float num2 = Mathf.InverseLerp(0f, values[i, 2], num);
            float num3 = ((time == 0) ? timeStacker : Mathf.InverseLerp(values[i, 2], 0f, num));
            float num4 = Mathf.Lerp(values[i, 0] * 0.1f, values[i, 0], Mathf.Pow(num2, 0.45f));
            float num5 = values[i, 1] * (0.5f + 0.5f * Mathf.Sin(num2 * Mathf.PI)) * Mathf.Pow(num3, 0.3f);
            Vector2 val = pos + dirs[i] * (innerRad + num4);
            if (room != null && room.GetTile(val).Solid)
            {
                num3 *= 0.5f;
            }
            Vector2 val2 = pos + dirs[i] * (innerRad + num4 * 0.1f);
            Vector2 val3 = Custom.PerpendicularVector(val, val2);
            tMesh.MoveVertice(i * 3, val - camPos);
            tMesh.MoveVertice(i * 3 + 1, val2 - val3 * num5 * 0.5f - camPos);
            tMesh.MoveVertice(i * 3 + 2, val2 + val3 * num5 * 0.5f - camPos);
            tMesh.verticeColors[i * 3] = Custom.RGB2RGBA(color2, Mathf.Pow(num3, 0.6f));
            tMesh.verticeColors[i * 3 + 1] = Custom.RGB2RGBA(color, 0);
            tMesh.verticeColors[i * 3 + 2] = Custom.RGB2RGBA(color, 0);
        }
    }
}

public class HailstormFireSmokeCreator : Smoke.FireSmoke
{

    public HailstormFireSmokeCreator(Room room) : base(room)
    {
    }

    public override SmokeSystemParticle CreateParticle()
    {
        return new HailstormFireSmoke();
    }

    public class HailstormFireSmoke : FireSmokeParticle
    {

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

    }
}