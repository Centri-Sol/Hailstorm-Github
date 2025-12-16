
namespace Hailstorm;

public class IncanFeatures
{

    public static void Hooks()
    {
        On.Player.ctor += IncanCWT;
        On.SlugcatStats.ctor += IncanStats; 
        On.Player.ThrownSpear += SpearThrows;

        // Movement
        On.Player.TerrainImpact += WallRollFromFlip;
        On.Player.MovementUpdate += MovementUpdate;
        On.Player.Jump += JumpBoosts;
        On.Player.UpdateAnimation += AnimationUpdate;
        On.Player.UpdateBodyMode += OtherMobility;

        IncanILHooks();
        On.SlugcatStats.NourishmentOfObjectEaten += SaintNoYouCantEatTheseFuckOff_WaitNoPUTAWAYYOURASCENSIONPOWERSDONOTBLOWUPMYMINDLIKEPANCA;
        On.Player.ObjectEaten += FoodEffects;
        On.Player.CanEatMeat += INCANNOSTOPEATINGSLUGCATS;
        On.Player.Update += HailstormPlayerUpdate;

        On.Player.Collide += HailstormPlayerCollision;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return
            RWG?.session is not null &&
            RWG.IsStorySession &&
            RWG.StoryCharacter == HSEnums.Incandescent;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanCWT(On.Player.orig_ctor orig, Player self, AbstractCreature absSelf, World world)
    {
        orig(self, absSelf, world);
        if (!IncanExtension._cwtInc.TryGetValue(self, out _))
        {
            IncanExtension._cwtInc.Add(self, new IncanInfo(self));
        }
        if (self.IsIncan(out IncanInfo player))
        {
            player.ReadyToMoveOn = MiscWorldChanges.AllEchoesMet;
        }
    }

    public static void IncanStats(On.SlugcatStats.orig_ctor orig, SlugcatStats stats, SlugcatStats.Name slugcat, bool starving)
    {
        orig(stats, slugcat, starving);
        if (slugcat == HSEnums.Incandescent)
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
        if (self.IsIncan(out _))
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

    public static void SpearThrows(On.Player.orig_ThrownSpear orig, Player self, Spear spr)
    {
        orig(self, spr);
        if (self.IsIncan(out IncanInfo incan))
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

    public static void WallRollFromFlip(On.Player.orig_TerrainImpact orig, Player self, int chunk, IntVector2 dir, float speed, bool firstContact)
    {
        orig(self, chunk, dir, speed, firstContact);
        if (!self.IsIncan(out _))
        {
            return;
        }

        if (firstContact &&
            self.animation == Player.AnimationIndex.Flip &&
            self.input[0].downDiagonal != 0)
        {
            self.animation = Player.AnimationIndex.Roll;
            self.rollDirection = self.input[0].downDiagonal;
        }

    }

    public static void MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (!self.IsIncan(out IncanInfo player))
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

    public static void JumpBoosts(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        if (!self.IsIncan(out IncanInfo player))
        {
            return;
        }

        bool Starving = self.Malnourished;

        if (self.animation == Player.AnimationIndex.Flip)
        {
            self.mainBodyChunk.vel.x *= Starving ? 3f : 2.50f;
            self.mainBodyChunk.vel.y *= Starving ? 2f : 1.66f;
            self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.mainBodyChunk.pos, self.flipFromSlide ? 1f : 0.6f, 1.5f);
        }
        else if (player.wallRolling > 0)
        {
            Vector2 launchForce = new(3f * self.input[0].x, 6f);
            player.wallRolling = 0;
            player.wallrollJump = true;
            if (player.ReadyToMoveOn &&
                self.input[0].x != self.bodyChunks[0].ContactPoint.x)
            {
                self.animation = Player.AnimationIndex.Flip;
                self.room.PlaySound(SoundID.Slugcat_Flip_Jump, self.mainBodyChunk);
                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.mainBodyChunk.pos, 0.9f, 1.25f);
                launchForce *= 4 / 3f;
            }
            if (Starving)
            {
                launchForce *= 1.2f;
            }
            self.bodyChunks[0].vel += launchForce;
            self.bodyChunks[1].vel += launchForce / 2f;
            self.bodyChunks[0].pos += launchForce / 2f;
            self.bodyChunks[1].pos += launchForce / 2f;
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
                self.mainBodyChunk.vel.y += Starving ? 30 : 24;
                self.animation = Player.AnimationIndex.Flip;
            }
            else
            {
                self.bodyChunks[0].vel.x *= Starving ? 2.2f : 1.8f;
                self.bodyChunks[1].vel.x *= Starving ? 1.7f : 1.48f;
                self.bodyChunks[0].vel.y *= Starving ? 1.35f : 1.25f;
            }
        }
        else if (!player.longJumping)
        {
            self.mainBodyChunk.vel.y *= 1.33f;
        }
    }

    public static void AnimationUpdate(On.Player.orig_UpdateAnimation orig, Player self)
    {
        bool canBoost =
            self is not null &&
            self.waterJumpDelay == 0;

        orig(self);
        if (!self.IsIncan(out IncanInfo player))
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

    public static void OtherMobility(On.Player.orig_UpdateBodyMode orig, Player self)
    {
        orig(self);
        if (!self.IsIncan(out IncanInfo player))
        {
            return;
        }

        bool Starving = self.Malnourished;

        if (self.animation == Player.AnimationIndex.BellySlide)
        {
            self.mainBodyChunk.vel.x *= Starving ? 1.4f : 1.35f;
            self.mainBodyChunk.vel.y *= 1.05f;
        }
        else if (self.animation == Player.AnimationIndex.RocketJump && !player.wallrollJump)
        {
            if (Starving)
            {
                self.mainBodyChunk.vel.x *= 1.02f;
            }
            self.mainBodyChunk.vel.y *= Starving ? 1.06f : 1.05f;
        }
        else if (self.animation == Player.AnimationIndex.Roll)
        {
            self.bodyChunks[0].vel *= Starving ? 1.11f : 1.1f;
            self.bodyChunks[1].vel *= Starving ? 1.11f : 1.1f;
            if (self.input[0].downDiagonal != 0 &&
                self.rollCounter > 15 &&
                player.rollExtender < (Starving ? 75 : 60)) // Extends roll duration.
            {
                player.rollExtender++;
                self.rollCounter--;
            }

            bool RollUp = false;
            bool HighWall = false;
            for (int b = 0; b < 2; b++)
            {
                if (!RollUp &&
                    self.IsTileSolid(b, self.rollDirection, 0) &&
                    !self.IsTileSolid(0, 0, 1) &&
                    !self.IsTileSolid(1, 0, 1))
                {
                    RollUp = true;
                    if (self.stopRollingCounter > 3)
                    {
                        self.stopRollingCounter--;
                    }
                    if (self.goIntoCorridorClimb > 0)
                    {
                        self.goIntoCorridorClimb--;
                    }
                    if (player.rollFallExtender > 0)
                    {
                        player.rollFallExtender--;
                    }
                }
                if (!HighWall &&
                    self.IsTileSolid(b, self.rollDirection, 0) &&
                    self.IsTileSolid(b, self.rollDirection, 1))
                {
                    HighWall = true;
                }

            }
            if (RollUp && (HighWall || (
                !self.IsTileSolid(0, 0, -1) && self.bodyChunks[0].ContactPoint.y > -1 &&
                !self.IsTileSolid(1, 0, -1) && self.bodyChunks[1].ContactPoint.y > -1)))
            {
                if (player.WallRollPower > 0)
                {
                    self.canJump = Math.Max(self.canJump, 5);
                    self.bodyChunks[0].pos.y += (self.gravity + 7f) * Mathf.Pow(player.WallRollPower, 0.5f);
                    self.bodyChunks[1].pos.y += (self.gravity + 7f) * Mathf.Pow(player.WallRollPower, 0.5f);
                    player.wallRollDir = self.rollDirection;

                    if (HighWall)
                    {
                        player.wallRolling++;
                    }
                }
            }
            else
            if (self.stopRollingCounter > 3 &&
                player.rollFallExtender < 12) // Extends how long you can fall mid-roll without your roll being canceled.
            {
                player.rollFallExtender++;
                self.stopRollingCounter--;
            }

            if (player.wallRolling > 0 && (!RollUp || !HighWall))
            {
                player.wallRolling = Mathf.Min(5, player.wallRolling - 1);
            }

            if (player.WallRollPower == 0)
            {
                self.animation = Player.AnimationIndex.None;
            }
        }

        if (player.longJumping &&
            player.StopLongJump(self))
        {
            player.longJumping = false;
        }

        if (self.animation != Player.AnimationIndex.Roll)
        {
            if (player.rollExtender > 0)
            {
                player.rollExtender = 0;
            }
            if (player.rollFallExtender > 0)
            {
                player.rollFallExtender = 0;
            }
            if (player.wallRolling > 0)
            {
                player.wallRolling = Mathf.Min(5, player.wallRolling - 1);
            }
        }

        if (player.wallrollJump &&
            self.animation != Player.AnimationIndex.RocketJump &&
            self.animation != Player.AnimationIndex.Flip)
        {
            player.wallrollJump = false;
        }

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // This allows certain foods to temporarily buff the Incandescent's glow.
    // The actual buffing is done near the bottom of the HailstormPlayerUpdate method.
    public static void FoodEffects(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible food)
    {
        orig(self, food);
        if (!self.IsIncan(out IncanInfo player))
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

        int FuelGain = 0;

        if (food is Luminescipede)
        {
            FuelGain += 1200;
        }
        else if (food is FireEgg)
        {
            FuelGain += 1200;
            self.Hypothermia -= 0.3f;
        }
        else if (food is SLOracleSwarmer or SSOracleSwarmer)
        {
            FuelGain += 4800;
        }
        else if (food is KarmaFlower)
        {
            FuelGain += 7200;
            player.waterGlow += 7200;
        }
        else if (food is SwollenWaterNut)
        {
            player.soak += 1800;
            self.room.PlaySound(SoundID.Medium_Object_Into_Water_Slow, self.bodyChunks[1].pos, 0.6f, Random.Range(1.4f, 1.6f));
            self.room.PlaySound(SoundID.Firecracker_Burn, self.bodyChunks[1].pos, 0.6f, Random.Range(1.75f, 2f));
        }
        else // All food types below this point cannot increase glow radius past a certain point.                 
        {    // The foods *above* this point have no such restriction.

            if (food is JellyFish || (food is Centipede cnt2 && cnt2.Template.type == CreatureTemplate.Type.SmallCentipede))
            {
                FuelGain = 1600;
            }
            else if (food is GlowWeed || (food is Centipede cnt1 && cnt1.Template.type == new CreatureTemplate.Type("InfantAquapede")))
            {
                FuelGain = 1200;
                player.waterGlow += 1200;
            }
            else if (food is SlimeMold SM)
            {
                if (SM.big)
                {
                    FuelGain = 2400;
                    player.soak -= 2400;
                }
                else
                {
                    FuelGain = 1200;
                    player.soak -= 1200;
                }
            }
            else if (food is LillyPuck)
            {
                FuelGain = 800;
                player.waterGlow += 4800;
            }
            else if (food is Mushroom)
            {
                FuelGain = 2400;
            }

            if (!HSRemix.IncanNoFireFuelLimit.Value &&
                player.firefuel + FuelGain > 2400) // Prevents fireFuel from going over 2400.
            {
                FuelGain = Mathf.Max(2400 - player.firefuel, 0);
            }
            player.firefuel += FuelGain;
        }

        if (FuelGain > 0)
        {
            self.room.PlaySound(HSEnums.Sound.IncanFuel, self.bodyChunks[1].pos, 1, Custom.LerpMap(FuelGain, 0, 2400, 1.4f, 0.6f));
        }

    }

    public static int SaintNoYouCantEatTheseFuckOff_WaitNoPUTAWAYYOURASCENSIONPOWERSDONOTBLOWUPMYMINDLIKEPANCA(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcat, IPlayerEdible eatenobject)
    {
        return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint && (eatenobject is Luminescipede || eatenobject is PeachSpiderCritob || eatenobject is Snowcuttle)
            ? -1
            : orig(slugcat, eatenobject);
    }

    public static void IncanILHooks()
    {

        IL.Player.GrabUpdate += IL =>
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
            else
            {
                Debug.LogError("[Hailstorm] An IL hook for eating Glowweed underwater broke! Report this, would ya?");
            }
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
                c.EmitDelegate((bool Malnourished, Player self) => Malnourished && (!self.IsIncan(out IncanInfo player)));
            }
            else
            {
                Debug.LogError("[Hailstorm] AN IL hook for Incan starve exhaustion broke! Lemme know about this!");
            }
        };

        IL.Player.MovementUpdate += IL => // Prevents Incan from grabbing onto walls while rolling 
        {
            ILLabel label = null;
            ILCursor c = new(IL);
            if (c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<PhysicalObject>("get_bodyChunks"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchLdelemRef(),
                    x => x.MatchCallvirt<BodyChunk>("get_ContactPoint"),
                    x => x.MatchLdfld<IntVector2>(nameof(IntVector2.x)),
                    x => x.MatchBrfalse(out label)
                    )
                )
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Player self) => CanWallRoll(self));
                c.Emit(OpCodes.Brtrue, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] An IL hook related to Incan and wall-grabbing broke! Not good; tell me about this.");
            }

            c = new(IL);
            if (c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<Player>("get_input"),
                    x => x.MatchLdcI4(0),
                    x => x.MatchLdelema<Player.InputPackage>(),
                    x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.x)),
                    x => x.MatchBrfalse(out label)
                    )
                )
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Player self) => CanWallRoll(self));
                c.Emit(OpCodes.Brtrue, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] An IL hook related to Incan and ledge-crawl prevention broke! Not super major, but I'd still like to know about this.");
            }
        };

    }
    public static bool CanWallRoll(Player self)
    {
        if (self.IsIncan(out _) && (
                self.animation == Player.AnimationIndex.Roll || (
                    self.animation == Player.AnimationIndex.Flip && self.input[0].downDiagonal != 0)))
        {
            return true;
        }
        return false;
    }

    public static void UpdateFood(Player self, IncanInfo Incan, bool wasMalnourished)
    {
        self.playerState.foodInStomach = Custom.IntClamp(Mathf.FloorToInt(Incan.floatFood), 0, self.slugcatStats.maxFood);
        self.playerState.quarterFoodPoints = self.playerState.foodInStomach >= self.slugcatStats.maxFood ? 0 : Mathf.FloorToInt(Custom.Decimal(Incan.floatFood) * 4f);
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

        if (self.IsIncan(out IncanInfo Incan))
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

                    if (self.IsIncan(out _))
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
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("SLUGROLLED! Stun: " + 120f + " | Damage: " + 1);
                }

                self.room.ScreenMovement(self.bodyChunks[0].pos, self.mainBodyChunk.vel * self.bodyChunks[0].mass * 5f * 0.1f, Mathf.Max((self.bodyChunks[0].mass - 30f) / 50f, 0f));
                self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                target.SetKillTag(self.abstractCreature);
                target.Violence(self.mainBodyChunk, (Vector2?)new Vector2(self.mainBodyChunk.vel.x * 5f, self.mainBodyChunk.vel.y), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, 1, 120);
                self.animation = Player.AnimationIndex.None;
                self.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
                self.rollDirection = 0;
                if ((target.State is HealthState targetHS && targetHS.ClampedHealth == 0f) || target.State.dead)
                {
                    self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
                }

                else
                {
                    self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.mainBodyChunk, loop: false, 1.2f, 1f);
                }

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
                    lastFallPos < 100f ? dmg * 0.5f :
                        lastFallPos < 200f ? dmg :
                            lastFallPos < 320f ? dmg * 2f :
                                lastFallPos < 600f ? dmg * 3f : dmg * 5f;
            }
            if (stn > 240)
            {
                stn = 240;
            }

            if (dmg < 0f)
            {
                dmg = 0f;
            }

            if (stn < 25)
            {
                stn = 0;
            }

            if (dmg != 0f || stn != 0)
            {
                if (!target.dead)
                {
                    if (RainWorld.ShowLogs)
                    {
                        Debug.Log("SLUGSMASH! Slide: " + (self.animation == Player.AnimationIndex.BellySlide || self.animation == Player.AnimationIndex.RocketJump) + " | Incoming speed: " + Mathf.Max(self.mainBodyChunk.vel.y, self.mainBodyChunk.vel.magnitude) + " | Dist: " + (self.lastGroundY - self.firstChunk.pos.y) + " | Damage: " + dmg + " | Stun: " + (stn / 40) + "s");
                    }

                    self.room.ScreenMovement(self.bodyChunks[0].pos, self.mainBodyChunk.vel * dmg * self.bodyChunks[0].mass * 0.1f, Mathf.Max(((dmg * self.bodyChunks[0].mass) - 30f) / 50f, 0f));
                    self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                    target.SetKillTag(self.abstractCreature);
                    target.Violence(self.mainBodyChunk, (Vector2?)new Vector2(self.mainBodyChunk.vel.x, self.mainBodyChunk.vel.y), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, dmg, (int)stn);
                    if (otherObject is BigJellyFish bJelly)
                    {
                        hitSmallCreature = true;
                        bJelly.Die();
                    }
                    if ((target.State is HealthState targHS2 && targHS2.ClampedHealth == 0f) || target.State.dead)
                    {
                        self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
                    }

                    else
                    {
                        self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.mainBodyChunk, loop: false, 1.2f, 1f);
                    }
                }
                else
                {
                    self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
                }

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
            if (self.animation == Player.AnimationIndex.BellySlide)
            {
                self.mainBodyChunk.vel.x /= 3f;
                self.rollCounter = 99;
            }
        }
        #endregion

        //--------------------------------------------------------------

        orig(self, otherObject, myChunk, otherChunk);

        if (CWT.CreatureData.TryGetValue(target, out CWT.CreatureInfo cI) &&
            self.IsIncan(out IncanInfo Incan))
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

    public static void IncanCollision(Player self, IncanInfo Incan, Creature target, CWT.CreatureInfo cI, int myChunk, int otherChunk, bool hitSmallCreature)
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
            DMGTYPE = HSEnums.DamageTypes.Heat;

            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 35f, Incan.FireColor, self.ShortCutColor()));
            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 45f, Incan.FireColor, self.ShortCutColor()));

            if (target.SpearStick(null, DMG, target.bodyChunks[otherChunk], null, self.mainBodyChunk.vel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out CWT.AbsCtrInfo aI))
            {
                aI.AddBurn(self.abstractCreature, target, otherChunk, BURNTIME, Incan.FireColor, self.ShortCutColor());
            }

            if (RainWorld.ShowLogs)
            {
                Plugin.HailstormLog("Player " + (self.playerState.playerNumber + 1) + " burned something! | Damage: " + DMG + " | Burn Time: " + (BURNTIME / 40f) + "s | Stun: " + (STUN / 40f) + "s");
            }
        }
        else
        {
            DMGTYPE = Creature.DamageType.Blunt;

            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 20f, Incan.FireColor, self.ShortCutColor()));
            self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 30f, Incan.FireColor, self.ShortCutColor()));

            if (RainWorld.ShowLogs)
            {
                Plugin.HailstormLog("Player " + (self.playerState.playerNumber + 1) + " BONKED something! | Damage: " + DMG + " | Stun: " + (STUN / 40f) + "s | Impact velocity: " + Mathf.Max(self.mainBodyChunk.vel.y, self.mainBodyChunk.vel.magnitude) + " | Distance: " + (self.lastGroundY - self.firstChunk.pos.y));
            }
        }

        Incan.fireSmoke ??= new HailstormFireSmokeCreator(self.room);
        for (int s = 0; s < 10; s++)
        {
            Vector2 SmokeVel = DMGTYPE == Creature.DamageType.Blunt
                ? Custom.DegToVec(Custom.VecToDeg(self.mainBodyChunk.vel) + Random.Range(-15, 15))
                : Custom.RNV() * Random.Range(8f, 12f);
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


        // Damage sounds
        bool KillingBlow = false;
        float Pitch = Custom.WrappedRandomVariation(0.1f, 0.01f, 0.3f) * 10f;
        if (self.isSlugpup)
        {
            Pitch *= 1.2f;
        }

        if (target.State.dead || (target.State is HealthState targetHP && targetHP.ClampedHealth == 0f))
        {
            KillingBlow = true;
            self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk, false, 1.7f, Pitch);
        }

        bool HitArmor = !target.SpearStick(null, DMG, target.bodyChunks[otherChunk], null, self.mainBodyChunk.vel);
        if (HitArmor)
        {
            HEATLOSS /= 4f;
            self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.mainBodyChunk, false, KillingBlow ? 0.8f : 1.2f, Pitch);
        }

        float vol = HitArmor ? 0.5f : 1f;
        if (DMGTYPE == HSEnums.DamageTypes.Heat)
        {
            vol *= KillingBlow ? 0.7f : 1f;
            self.room.PlaySound(HSEnums.Sound.FireImpact, self.mainBodyChunk, false, vol, Mathf.Lerp(2f, 1f, DMG / 2f * Pitch));
        }
        else
        {
            vol *= KillingBlow ? 1.1f : 1.7f;
            self.room.PlaySound(SoundID.Rock_Hit_Wall, self.mainBodyChunk, false, vol, Mathf.Lerp(1.4f, 1f, DMG * Pitch));
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
            if (absCtr?.realizedCreature is null ||
                absCtr.realizedCreature == self)
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
                !CWT.CreatureData.TryGetValue(target, out CWT.CreatureInfo cI) ||
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
                target.Violence(self.mainBodyChunk, new Vector2?(hitVel), chunk, null, HSEnums.DamageTypes.Heat, DAMAGE, STUN);
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
                        (liz.Template.type == HSEnums.CreatureType.FreezerLizard ||
                        liz.Template.type == CreatureTemplate.Type.GreenLizard ||
                        liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard) ? 20 : 40;
                    liz.turnedByRockDirection = (int)Mathf.Sign(chunk.pos.x - tailEnd.pos.x);
                }
                if (target.SpearStick(null, 0.25f, chunk, null, hitVel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out CWT.AbsCtrInfo aI))
                {
                    aI.AddBurn(self.abstractCreature, target, chunk.index, BURNTIME, Incan.FireColor, self.ShortCutColor());
                }

                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 35f, Incan.FireColor, self.ShortCutColor()));
                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 45f, Incan.FireColor, self.ShortCutColor()));
                self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk.pos, 1.2f, 1);
                if ((target.State is HealthState targetHP && targetHP.ClampedHealth == 0f) || target.State.dead)
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.DangerPos, 0.75f, Random.Range(1.75f, 2));
                    self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk.pos, 1.7f, 1f);
                }
                else
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.DangerPos, 1f, Random.Range(1.75f, 2));
                }

                if (RainWorld.ShowLogs)
                {
                    Plugin.HailstormLog("Player " + (self.playerState.playerNumber + 1) + " burned something! | Damage: 0.25 | Burn Time: 12.5s | Stun: 1.25s");
                }

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
            else
            {
                selfHypo = other.Template.BlizzardAdapted
                    ? Mathf.Lerp(self.Hypothermia, 0, 0.001f) - self.Hypothermia
                    : Mathf.Lerp(self.Hypothermia, other.Hypothermia, 0.001f) - self.Hypothermia;
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
                    hp1.health -= 0.00125f / self.Template.baseDamageResistance / self.Template.damageRestistances[HSEnums.DamageTypes.Heat.index, 0];
                }
                if (other.State is HealthState hp2)
                {
                    hp2.health -= 0.00125f / other.Template.baseDamageResistance / other.Template.damageRestistances[HSEnums.DamageTypes.Cold.index, 0];
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