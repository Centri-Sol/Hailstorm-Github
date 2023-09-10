using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using MonoMod.Cil;
using Mono.Cecil.Cil;
namespace Hailstorm;

public class IncanFeatures
{
                                                                                        // Player = the class you want to assign new variables to.
                                                                                        // HailstormSlugcats = the class your new variables are pulled from.
                                                                                        // And then CWT.PlayerData is just the name of the Conditional Weak Table (CWTs).
    /* This CWT is used to give each set of players or creatures their own sets of the same variables or data.
     * If you're having a problem with multiple players sharing the same variables when using your custom slugcat, here's your fix. This should also help if variable values are
     * being carried over between cycles/deaths!
     * Check ConditionalWeakTables.cs to see what these tables are pulling from. */

    public static void Hooks()
    {

        On.Player.ctor += PlayerCWT;

        On.Player.ThrownSpear += IncanSpearThrows;

        On.Player.Jump += IncanJumpBoosts;
        On.Player.UpdateBodyMode += IncanOtherMobility;
        On.Player.UpdateAnimation += IncanSwimming;
        //On.Player.TerrainImpact += TerrainImpact;

        On.Player.ObjectEaten += IncanFoodEffects;
        On.Player.Update += HailstormPlayerUpdate;
        //On.Player.UpdateAnimation += RollAnimations;

        On.Player.Collide += IncanCollision;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void PlayerCWT(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        CWT.PlayerData.Add(self, new HailstormSlugcats(self));
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanSpearThrows(On.Player.orig_ThrownSpear orig, Player self, Spear spr)
    {
        orig(self, spr);
        if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) && player.isIncan)
        {
            spr.spearDamageBonus *= HSRemix.IncanSpearDamageMultiplier.Value;
            if (MMF.cfgUpwardsSpearThrow.Value && spr.setRotation.Value.y == 1f && self.bodyMode != Player.BodyModeIndex.ZeroG)
            {
                spr.spearDamageBonus *= 1.25f; // Negates the slight damage penalty that upthrows normally have
                if (spr?.room is not null)
                {
                    spr.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, spr.firstChunk.pos, 0.75f, Random.Range(1.75f, 2f));
                    spr.room.AddObject(new Explosion.ExplosionLight(spr.firstChunk.pos, 280f, 1f, 7, player.FireColor));
                    spr.room.AddObject(new FireSpikes(spr.room, spr.firstChunk.pos, 14, 15f, 9f, 5f, 90f, player.FireColor, self.ShortCutColor()));
                }
            }
            else
            {
                spr.spearDamageBonus *= 0.7f;
            }
            if (self.Malnourished)
            {
                spr.spearDamageBonus *= 0.8f;
                spr.firstChunk.vel.x *= 0.9f;
            }
            if (!spr.bugSpear)
            {
                spr.firstChunk.vel.x *= 0.85f;
            }
        }        
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // Increases the speed and reach of certain jump-based moves.
    public static void IncanJumpBoosts(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) && player.isIncan)
        {
            if (self.animation == Player.AnimationIndex.Flip)
            {
                self.mainBodyChunk.vel.x *= self.Malnourished? 2f : 2.5f;
                self.mainBodyChunk.vel.y *= self.Malnourished? 1.44f : 1.66f;
            }
            else if (!player.longJumpReady && !player.longJumping)
            {
                self.mainBodyChunk.vel.y *= self.Malnourished? 1.2f : 1.33f;
            }            
        }
    }

    // Increases the speed and reach of other moves.
    public static void IncanOtherMobility(On.Player.orig_UpdateBodyMode orig, Player self)
    {
        orig(self);
        if (!CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) || !player.isIncan) return;

        bool starving = self.Malnourished;
        if (self.animation == Player.AnimationIndex.RocketJump)
        {
            self.mainBodyChunk.vel.y *= starving? 1.033f : 1.05f;
        }

        if (self.animation == Player.AnimationIndex.Roll)
        {
            self.bodyChunks[0].vel *= starving? 1.075f : 1.1f; // Increases rolling speed.
            self.bodyChunks[1].vel *= starving? 1.075f : 1.1f;
            if (self.rollCounter > 15 && player.rollExtender < (starving? 45 : 60) && self.input[0].downDiagonal != 0) // Extends roll duration.
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
        else
        {
            player.rollExtender = 0;
            player.rollFallExtender = 0;
        }

        // Increases how far the Incandescent's slides go by about double.
        if (self.animation == Player.AnimationIndex.BellySlide)
        {
            self.mainBodyChunk.vel.x *= starving ? 1.24f : 1.35f;
            self.mainBodyChunk.vel.y *= 1.05f;
        }

        // Sends the Incandescent WAY farther when long-jumping, and allows her to *high*-jump if you hold Down.
        if (self.superLaunchJump >= 19)
        {
            player.longJumpReady = true;
            if (self.input[0].y < 0 && player.readyToMoveOn)
            {
                player.highJump = true;
                self.killSuperLaunchJumpCounter = 15;
            }
            else if (player.highJump)
            {
                player.highJump = false;
            }
        }
        if (player.longJumpReady && self.superLaunchJump == 0)
        {
            player.longJumpReady = false;
            player.longJumping = true;
            if (!player.highJump)
            {
                self.bodyChunks[0].vel.x *= starving ? 1.6f : 1.8f;
                self.bodyChunks[1].vel.x *= starving ? 1.36f : 1.48f;
                self.bodyChunks[0].vel.y *= starving ? 1.16f : 1.25f;
            }
            else if (player.readyToMoveOn)
            {
                player.highJump = false;
                self.bodyChunks[0].vel.x *= 0;
                self.bodyChunks[1].vel.x *= 0;
                self.mainBodyChunk.vel.y += starving ? 20 : 24;
                self.animation = Player.AnimationIndex.Flip;
            }
        }
        if (player.longJumping && (
            self.bodyChunks[0].contactPoint.x != 0 || self.bodyChunks[0].contactPoint.y != 0 ||
            self.bodyChunks[1].contactPoint.x != 0 || self.bodyChunks[1].contactPoint.y != 0 ||
            self.Submersion > 0 ||
            self.bodyMode == Player.BodyModeIndex.Swimming ||
            self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ||
            self.bodyMode == Player.BodyModeIndex.ZeroG ||
            self.animation == Player.AnimationIndex.AntlerClimb ||
            self.animation == Player.AnimationIndex.GrapplingSwing ||
            self.animation == Player.AnimationIndex.VineGrab ||
            self.Stunned ||
            self.dead
            ))
        {
            player.longJumping = false;
        }
    }

    // Strengthens both normal swimming and swimboosts for the Incandescent.
    public static void IncanSwimming(On.Player.orig_UpdateAnimation orig, Player self)
    {
        bool canBoost = false;
        if (self is not null && self.waterJumpDelay == 0)
        {
            canBoost = true;
        }
        orig(self);
        if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) && player.isIncan)
        {
            if (canBoost && self.waterJumpDelay > 0 && self.animation == Player.AnimationIndex.DeepSwim)
            {
                Vector2 direction = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
                if (!ModManager.MMF || !MMF.cfgFreeSwimBoosts.Value)
                {
                    self.airInLungs += 0.005f;
                }
                self.bodyChunks[0].vel += direction * 1.5f;
            }
            if (self.animation == Player.AnimationIndex.DeepSwim && (self.input[0].ZeroGGamePadIntVec.x != 0 || self.input[0].ZeroGGamePadIntVec.y != 0))
            {
                if (self.swimCycle > 0)
                {
                    self.bodyChunks[0].vel += Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos) * 0.7f * Mathf.Lerp(self.swimForce, 1f, 0.5f) * self.bodyChunks[0].submersion * 0.125f;
                }
            }
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
        if (!CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) || !player.isIncan)
        {
            return;
        }

        if (food is Luminescipede)
        {
            player.fireFuel += 1200;
        }
        else if (food is FireEgg)
        {
            player.fireFuel += 1200;
            self.Hypothermia -= 0.3f;
        }
        else if (food is SLOracleSwarmer || food is SSOracleSwarmer)
        {
            player.fireFuel += 4800;
        }
        else if (food is KarmaFlower)
        {
            player.fireFuel += 7200;
            player.waterGlow += 7200;
        }
        else if (food is SwollenWaterNut)
        {
            player.wetness += 600;
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
            else if (food is GlowWeed || (food is Centipede cnt1 && cnt1.Template.type == HailstormEnums.InfantAquapede))
            {
                fuel = 1200;
                player.waterGlow += 1200;
            }
            else if (food is SlimeMold SM)
            {
                fuel = (SM.big) ? 2400 : 1200;
                player.wetness -= (SM.big) ? 800 : 400;
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

            if (HSRemix.IncanNoFireFuelLimit.Value is false && player.fireFuel + fuel > 2400) // Prevents fireFuel from going over 2400.
            {
                fuel = Mathf.Max(2400 - player.fireFuel, 0);
            }
            player.fireFuel += fuel;
        }
    }

    //--------------------------------------------
    public static void HailstormPlayerUpdate(On.Player.orig_Update orig, Player self, bool eu)
    { // Manages everything that needs to be constantly updated for players, including things that are exclusive to the Incandescent.
        orig(self, eu);

        Player.AnimationIndex anim = self.animation;
        PlayerGraphics selfGraphics = self.graphicsModule as PlayerGraphics;

        //--------------------------------------------------------------------------------------------------

        #region Tutorial Text

        if (Dialogue.IsRWGIncan(self?.room?.game) && !self.room.game.rainWorld.ExpeditionMode && self.room.game.GetStorySession.saveState.cycleNumber <= 2 && self.room.game.cameras[0]?.hud?.textPrompt is not null &&
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
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Thanks to your tail, you possess great mobility and a heightened jump.", 200, 400, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Using the heat from your tail, this mobility can be used to attack other creatures.", 0, 420, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Be wary, though; with every heat attack performed, your flame will lose some of its warmth.", 0, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Don't get too careless! Keep an eye on your glow to see how much warmth you still have.", 0, 440, darken: false, hideHud: true);
            }
            if (self.room.game.GetStorySession.saveState.cycleNumber == 2)
            {
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Water will dampen your flame, weakening its glow and making you more vulnerable to the cold.", 200, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("The longer you stay in water, the longer the effect will last, and the stronger it will be.", 0, 440, darken: false, hideHud: true);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Eating certain things may help your tail resist these effects... and boost it in other ways, too.", 0, 440, darken: false, hideHud: true);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------

        #region Hypothermia mechanics for all Slugcats.

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
                    if (self.grasps[i]?.grabbed is not null && self.grasps[i].grabbed is IceCrystal)
                    {
                        self.Hypothermia -= 0.0003f;
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
                    if (stomachHeat != 0 && self.SlugCatClass == HailstormSlugcats.Incandescent)
                    {
                        stomachHeat /= 3;
                    }
                }
                if (self.objectInStomach.realizedObject is IceCrystal)
                {
                    stomachHeat +=
                        self.room.world.game.IsArenaSession ? -0.00075f : -0.0003f;
                }
                self.Hypothermia -= stomachHeat;
            }

            if (self.room.game.IsArenaSession) // Sets up Hypothermia death mechanics for non-cold Arenas.
            {
                if (!self.dead && self.Hypothermia >= 1.5f)
                {
                    selfGraphics.head.vel += Custom.RNV() * (self.Hypothermia * 0.75f); // Head shivers
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

        #endregion

        //--------------------------------------------------------------------------------------------------

        if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) && player.isIncan)
        { // All features from this point on are exclusive to the Incandescent.

            #region Fire Stats
            /*
            if (self.animation == IncanPlayer.WallRoll || self.animation == IncanPlayer.CeilingRoll)
            {
                self.slideLoopSound = SoundID.Slugcat_Roll_LOOP;
            }
            */

            // These two timers are used in the FoodEffects method, just above this one.

            if (player.craftingDelayCounter < 115 && self.craftingObject && self.swallowAndRegurgitateCounter > 60)
            {
                AbstractPhysicalObject grasp1 = null;
                AbstractPhysicalObject grasp2 = null;
                if (self.grasps is not null && self.grasps.Length > 1)
                {
                    if (self.grasps[0]?.grabbed?.abstractPhysicalObject is not null) grasp1 = self.grasps[0].grabbed.abstractPhysicalObject;
                    if (self.grasps[1]?.grabbed?.abstractPhysicalObject is not null) grasp2 = self.grasps[1].grabbed.abstractPhysicalObject;
                }
                if (IncanCrafting.IncanRecipes(grasp1, grasp2) == HailstormEnums.BurnSpear)
                {
                    player.craftingDelayCounter++;
                    self.swallowAndRegurgitateCounter--;
                }
            }
            else if (!self.craftingObject)
            {
                player.craftingDelayCounter = 0;
            }

            if (self.room is not null && self.objectInStomach is not null && self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
            {
                self.Hypothermia -= Mathf.Lerp(-0.001f/3, 0f, self.HypothermiaExposure); // Weakens the warmth of Lanterns inside the Incandescent's stomach.
            }

            if (player.fireFuel > 0 || HSRemix.IncanWaveringFlame.Value is true)
            {
                player.fireFuel--;
            }
            if (player.waterGlow > 0) // Just two simple timers.
            {
                player.waterGlow--;
            }


            if (player.wetness < 2400 && self.Submersion > 0.5f) // Gradually builds up a debuff on the Incandescent while they are in water.
            {
                player.wetness += player.waterGlow > 0 ?
                    (self.Submersion >= 1 ? 3 : 1) :
                    (self.Submersion >= 1 ? 6 : 3);
            }
            else if (player.wetness > 0 && self.Submersion < 0.1f) // Gets rid of the debuff when outside of water, though at a much slower rate.
            {
                player.wetness--;
                if (player.lanternDryTimer >= 4)
                {
                    player.lanternDryTimer = 0;
                    player.wetness--;
                } //  ^ Lanterns can dry you out a little faster. lanternDryTimer is handled in a completely separate file: 'CreatureChanges.cs'.
            }

            player.wetness = Mathf.Clamp(player.wetness, 0, 2400); // Prevents wetness from going below 0 or over 2400.
            #endregion

            //-------------------------------------------------

            #region Hypothermia-related stuff

            bool arenaPenalties =
                (player.inArena && HSRemix.IncanNoArenaDowngrades.Value is false) ||
                (!player.inArena && HSRemix.IncanArenaDowngradesOutsideOfArena.Value is true);


            if (self.HypothermiaGain > 0) // This is where the Incandescent's cold resistance happens.
            {
                player.hypothermiaResistance = arenaPenalties ?
                    (self.isSlugpup ? 0.17f : 0.20f) :
                    (self.isSlugpup ? 0.66f : 0.75f);

                if (player.wetness > 0)
                {
                    player.hypothermiaResistance -=
                        (arenaPenalties ? 0.40f : 0.75f) * Mathf.InverseLerp(0, 2400, player.wetness);
                }
                if (self.Malnourished)
                {
                    player.hypothermiaResistance -= 0.2f;
                }
                if (HSRemix.IncanWaveringFlame.Value is true && player.fireFuel <= -1200)
                {
                    player.hypothermiaResistance *=
                        Mathf.InverseLerp(-8400, -1200, player.fireFuel);
                }

                if (player.hypothermiaResistance != 0)
                {
                    self.Hypothermia -= self.HypothermiaGain * player.hypothermiaResistance;
                }
            }

            if (arenaPenalties)
            {
                // In Arena, the Incandescent's mobility will gradually drain their warmth during use, instead of only when you hit something.
                if (anim == Player.AnimationIndex.BellySlide || anim == Player.AnimationIndex.Flip)
                {
                    self.Hypothermia -=
                        player.inArena ? -0.006f : -0.002f;
                }
                if (anim == Player.AnimationIndex.Roll || anim == Player.AnimationIndex.RocketJump)
                {
                    self.Hypothermia -=
                        player.inArena? -0.0036f : -0.0012f;
                }
                if (player.longJumpReady && self.superLaunchJump == 0) // Well, long-jumps just drain a bunch of warmth instantly.
                {
                    self.Hypothermia -=
                        player.inArena? -0.15f : -0.0375f;
                }        
            }

            if (self.room is not null)
            {

                if (self.dead && self.Hypothermia < 2)
                {
                    self.Hypothermia += self.room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard ? 0.015f : 0.003f;
                }

                if (player.currentCampaignBeforeRiv && !self.dead && self.Hypothermia >= 1.5f)
                {
                    selfGraphics.head.vel += Custom.RNV() * (self.Hypothermia * 0.75f);
                    self.playerState.permanentDamageTracking += (self.Hypothermia > 1.8f) ? 0.002f : 0.001f;
                    if (self.playerState.permanentDamageTracking >= 1)
                    {
                        self.Die();
                    }
                }

            }

            self.Hypothermia = Mathf.Clamp(self.Hypothermia, 0, 2); // Clamp sets both minimum and maximum values for a variable.
            #endregion

            //-------------------------------------------------

            #region Incan's Fire Glow

            // This first part sets up a flickering effect for the light using a float array.
            for (int i = 0; i < player.flicker.GetLength(0); i++)
            {
                player.flicker[i, 1] = player.flicker[i, 0];
                player.flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * (Random.value < 0.5f ? -1f : 1f);
                player.flicker[i, 0] = Custom.LerpAndTick(player.flicker[i, 0], player.flicker[i, 2], 0.05f, 1f / 30f);
                if (Random.value < 0.2f)
                {
                    player.flicker[i, 2] = 1f + Mathf.Pow(Random.value, 3f) * 0.2f * (Random.value < 0.5f ? -1f : 1f);
                }
                player.flicker[i, 2] = Mathf.Lerp(player.flicker[i, 2], 1f, 0.01f);
            }

            // Sets up multiple variables for dynamically changing the size of the Incandescent's glow.
            float glowMult = self.glowing? 1.5f : 1f;
            float ageMult = self.isSlugpup? 0.8f : 1f;
            float coldMult = Mathf.InverseLerp(1, 0, self.Hypothermia/2);
            float wetMult = Mathf.InverseLerp(7200, 0, player.wetness);
            float starveMult = self.Malnourished? 0.8f : 1f;
            float fuelMult = Mathf.Lerp(1, 1.5f, Mathf.InverseLerp(0, 2400, player.fireFuel));

            Color glowColor = Color.Lerp(player.FireColor, Color.white, glowMult - 1.2f); // ...Aaand the color, too.

            Vector2 glowPos =
                selfGraphics.tail is not null && selfGraphics.tail.Length > 0 ?
                selfGraphics.tail[selfGraphics.tail.Length - 1].pos :
                self.bodyChunks[1].pos;

            if (player.incanLight is null && !self.dead)
            {
                player.incanLight = new LightSource(glowPos, false, glowColor, self);
                player.incanLight.affectedByPaletteDarkness = 0;
                player.incanLight.requireUpKeep = true;
                player.incanLight.setAlpha = new float?(1);
                self.room.AddObject(player.incanLight);
            }
            else if (player.incanLight is not null)
            {
                player.incanLight.submersible = player.waterGlow > 0;
                player.incanLight.stayAlive = true;
                player.incanLight.setPos = new Vector2?(glowPos);
                player.incanLight.setRad = new float?(200 * player.flicker[0, 0] * glowMult * ageMult * coldMult * wetMult * starveMult * fuelMult);
                player.incanLight.color = glowColor;
                if (self.dead && self.Hypothermia > 1.9f)
                {
                    player.incanLight.Destroy();
                }
                if (player.incanLight.slatedForDeletetion ||
                    player.incanLight.room != self.room ||
                    (player.waterGlow > 0 && !player.incanLight.submersible) ||
                    (player.waterGlow <= 0 && player.incanLight.submersible))
                {
                    player.incanLight = null;
                }
            }

            #endregion

            //-------------------------------------------------

            #region Particles
            if (self.room is not null)
            {
                glowPos = // This complicated glowPos change is only here because of DMS. Good thing it wasn't so bad, or else I wouldn't have bothered.
                    selfGraphics.tail is null ? self.bodyChunks[1].pos :
                    selfGraphics.tail.Length > 3 ? selfGraphics.tail[Random.Range(selfGraphics.tail.Length - 1, (int)(selfGraphics.tail.Length / 2f))].pos :
                    selfGraphics.tail.Length > 2 ? selfGraphics.tail[Random.Range(selfGraphics.tail.Length - 1, selfGraphics.tail.Length - 2)].pos :
                    selfGraphics.tail.Length > 0 ? selfGraphics.tail[1].pos : self.bodyChunks[1].pos;

                bool makeBigEmbers =
                    player.longJumping ||
                    self.animation == Player.AnimationIndex.BellySlide ||
                    self.animation == Player.AnimationIndex.RocketJump ||
                    self.animation == Player.AnimationIndex.Flip ||
                    self.animation == Player.AnimationIndex.Roll ||
                    player.successfulCraft;


                bool sweemBoosting =
                    self.animation == Player.AnimationIndex.DeepSwim &&
                    self.waterJumpDelay > 0 &&
                    self.Submersion > 0;

                player.smallEmberTimer +=
                    (player.wetness > 0 && self.Submersion > 0.5f) ?
                    Mathf.Lerp(0.05f, 0.5f, Mathf.InverseLerp(0, 2400, player.wetness)) :
                    (makeBigEmbers ? 0.25f : 0.17f) * (self.Malnourished ? 0.7f : 1) * (self.bodyMode == Player.BodyModeIndex.Swimming? 0.7f : 1);

                if (player.smallEmberTimer >= 1)
                {
                    if (player.wetness > 0 && self.Submersion <= 0.5f)
                    {
                        BodyChunk dripChunk = self.bodyChunks[Random.Range(0, self.bodyChunks.Length - 1)];
                        self.room.AddObject(new WaterDrip(dripChunk.pos + (Custom.RNV() * dripChunk.rad * Random.value), default, false));
                    }
                    else
                    {
                        self.room.AddObject(self.Submersion > 0.5f ?
                            new Bubble(glowPos, new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f)), bottomBubble: false, fakeWaterBubble: false) :
                            new EmberSprite(glowPos, player.FireColor, 1f));
                    }
                    player.smallEmberTimer = 0;
                }

                if (makeBigEmbers || sweemBoosting)
                {
                    player.bigEmberTimer += (self.Malnourished ? 0.14f : 0.2f) * (self.bodyMode == Player.BodyModeIndex.Swimming ? 0.7f : 1);
                    if (player.bigEmberTimer >= 1)
                    {
                        player.bigEmberTimer = 0;
                        self.room.AddObject(self.bodyMode == Player.BodyModeIndex.Swimming ?
                            new Bubble(glowPos, new Vector2(Random.Range(-2f, 2f), Random.Range(4.5f, 9f)), bottomBubble: false, fakeWaterBubble: false) :
                            new EmberSprite(glowPos, player.FireColor, 2f));
                    }
                }

                if (player.fireSmoke is null && makeBigEmbers)
                {
                    player.fireSmoke = new HailstormFireSmokeCreator(self.room);
                }
                if (player.fireSmoke is not null)
                {
                    player.fireSmoke.Update(eu);
                    if (player.successfulCraft)
                    {
                        for (int f = 0; f < Random.Range(7, 9); f++)
                        {
                            if (self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 350f) && player.fireSmoke.AddParticle(glowPos, Custom.RNV() * Random.Range(3, 6), 40) is Smoke.FireSmoke.FireSmokeParticle craftFireSmoke)
                            {
                                craftFireSmoke.colorFadeTime = 35;
                                craftFireSmoke.effectColor = player.FireColor;
                                craftFireSmoke.colorA = self.ShortCutColor();
                                craftFireSmoke.rad = 2.5f * wetMult * fuelMult;
                            }
                        }
                        player.successfulCraft = false;
                    }
                    if (self.whiplashJump || self.rocketJumpFromBellySlide)
                    {
                        for (int f = 0; f < (self.whiplashJump ? 9 : 3); f++)
                        {
                            if (self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 350f) && player.fireSmoke.AddParticle(glowPos, self.bodyChunks[1].vel * 0.4f, 40) is Smoke.FireSmoke.FireSmokeParticle whiplashFireSmoke)
                            {
                                whiplashFireSmoke.colorFadeTime = 35;
                                whiplashFireSmoke.effectColor = player.FireColor;
                                whiplashFireSmoke.colorA = self.ShortCutColor();
                                whiplashFireSmoke.rad = 2.5f * wetMult * fuelMult;
                            }
                        }
                    }
                    if (makeBigEmbers && self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 350f) && player.fireSmoke.AddParticle(glowPos, self.bodyChunks[1].vel * 0.4f, 40) is Smoke.FireSmoke.FireSmokeParticle fireSmokeParticle)
                    {
                        fireSmokeParticle.colorFadeTime = 35;
                        fireSmokeParticle.effectColor = player.FireColor;
                        fireSmokeParticle.colorA = self.ShortCutColor();
                        fireSmokeParticle.rad *=
                            (self.flipFromSlide ? 2.25f : self.animation == Player.AnimationIndex.RocketJump || self.animation == Player.AnimationIndex.BellySlide ? 1.75f : 1.25f) * wetMult * fuelMult;
                    }
                    else if (!makeBigEmbers)
                    {
                        player.fireSmoke = null;
                    }
                }

            }
            #endregion
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
    public static void IncanCollision(On.Player.orig_Collide orig, Player self, PhysicalObject otherObject, int myChunk, int otherChunk)
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
                stn = stn * Mathf.Floor(Mathf.Abs(self.mainBodyChunk.vel.magnitude) / 7f);
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
        if (!CWT.CreatureData.TryGetValue(target, out CreatureInfo cI))
        {
            return;
        }

        //--------------------------------------
        // Okay that's all. Now, here we are at the Incandescent stuff.
        //
        #region Incandescent Collision

        if (!CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) || !player.isIncan)
        {
            return;
        }

        // This part here lists all the important values for each movement attack the Incandescent has.
        // Sorry if it's hard to read; I figured it'd be more convenient to have all the damage values together in one place, as opposed to... not that, like how I had it before.

        #region Attack Stats
        float DMG;
        float STUN;
        float HEATLOSS;

        if (self.animation == Player.AnimationIndex.BellySlide || (self.animation == Player.AnimationIndex.Flip && !self.flipFromSlide))
        {
            DMG = 0.5f;
            STUN = 20f;
            HEATLOSS = self.animation == Player.AnimationIndex.Flip? 0.12f : 0.06f;
        }
        else if (player.longJumping)
        {
            DMG = 0.75f;
            STUN = 40f;
            HEATLOSS = 0.12f;
        }
        else if (self.animation == Player.AnimationIndex.Roll || self.animation == Player.AnimationIndex.RocketJump)
        {
            DMG = 1.25f;
            STUN = 60f;
            HEATLOSS = 0.16f;
        }
        else if (self.animation == Player.AnimationIndex.Flip && self.flipFromSlide)
        {
            DMG = 2.25f;
            STUN = 120f;
            HEATLOSS = 0.24f;
        }
        else return;

        DMG *= HSRemix.IncanCollisionDamageMultiplier.Value;
        if (self.Malnourished) // Weakens damage and stun if you're starving.
        {
            HEATLOSS *= 0.75f;
        }
        if (self.isSlugpup) // Slightly weakens damage and stun if you're a slugpup.
        {
            DMG *= 0.8f;
            STUN *= 0.8f;
            HEATLOSS *= 0.66f;
        }
        if (target is Player) // Making sure we don't get Artificer 2 in Arena Mode.
        {
            DMG /= 2f;
            STUN /= 2f;
            HEATLOSS *= 2f;
        }
        if (HSRemix.IncanWaveringFlame.Value is true && player.fireFuel <= -1200)
        {
            float multiplier = Mathf.Lerp(0.5f, 1, Mathf.InverseLerp(-8400, -1200, player.fireFuel));
            DMG *= multiplier;
            STUN *= multiplier;
            HEATLOSS *= multiplier;
        }

        #endregion

        if (canHarm && (cI.impactCooldown == 0 || player.impactCooldown == 0)) // What happens on impact.
        {
            bool notAFly = otherObject is not Fly;
            if (notAFly)
            {
                cI.impactCooldown = 20;
                player.impactCooldown = 20;
            }
            else HEATLOSS = 0.1f;

            Vector2 scrnBumpFac =
                self.mainBodyChunk.vel * self.mainBodyChunk.mass * DMG *
                (self.animation == Player.AnimationIndex.Roll ? 0.4f :
                self.animation == Player.AnimationIndex.Flip ? 0.2f :
                self.animation == Player.AnimationIndex.BellySlide ? 0.1f : 0.3f);

            self.room.ScreenMovement(self.mainBodyChunk.pos, scrnBumpFac, Mathf.Max((self.mainBodyChunk.mass - 30f) / 50f, 0f));
            self.room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, self.mainBodyChunk);
            target.SetKillTag(self.abstractCreature);

            Creature.DamageType dmgType;
            if (self.animation == Player.AnimationIndex.Roll || self.animation == Player.AnimationIndex.Flip)
            {
                dmgType = HailstormEnums.HeatDamage;

                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 35f, player.FireColor, self.ShortCutColor()));
                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10f, 10f, 45f, player.FireColor, self.ShortCutColor()));

                if (!player.readyToMoveOn && target.SpearStick(null, DMG, target.bodyChunks[otherChunk], null, self.mainBodyChunk.vel) && CWT.AbsCtrData.TryGetValue(target.abstractCreature, out AbsCtrInfo aI))
                {
                    aI.AddBurn(self.abstractCreature, target, otherChunk, 600, player.FireColor, self.ShortCutColor());
                }

                if (RainWorld.ShowLogs)
                    Debug.Log("Player " + (self.playerState.playerNumber + 1) + " burned something! | Damage: " + DMG + " | Stun: " + STUN / 40f + "s");
            }
            else
            {
                dmgType = Creature.DamageType.Blunt;

                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 20f, player.FireColor, self.ShortCutColor()));
                self.room.AddObject(new FireSpikes(self.room, Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), Random.Range(4, 6), 2f, 10, 20f, 30f, player.FireColor, self.ShortCutColor()));

                if (player.readyToMoveOn) DMG += 0.25f;

                if (RainWorld.ShowLogs)
                    Debug.Log("Player " + (self.playerState.playerNumber + 1) + " BONKED something! | Damage: " + DMG + " | Stun: " + STUN / 40f + "s | Incoming speed: " + Mathf.Max(self.mainBodyChunk.vel.y, self.mainBodyChunk.vel.magnitude) + " | Distance: " + (self.lastGroundY - self.firstChunk.pos.y));
            }

            if (player.fireSmoke is null)
            {
                player.fireSmoke = new HailstormFireSmokeCreator(self.room);
            }
            for (int f = 0; f < 10; f++)
            {
                Vector2 vel =
                    dmgType == Creature.DamageType.Blunt ?
                    Custom.DegToVec(Custom.VecToDeg(self.mainBodyChunk.vel) + Random.Range(-15, 15)) :
                    Custom.RNV() * Random.Range(8f, 10f);

                if (player.incanLight is not null && self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 300f) && player.fireSmoke.AddParticle(Vector2.Lerp(self.mainBodyChunk.pos, target.mainBodyChunk.pos, 0.5f), vel, 40) is Smoke.FireSmoke.FireSmokeParticle bonkFireSmoke)
                {
                    bonkFireSmoke.colorFadeTime = 35;
                    bonkFireSmoke.effectColor = player.FireColor;
                    bonkFireSmoke.colorA = self.ShortCutColor();
                    bonkFireSmoke.rad *= 5f * Mathf.InverseLerp(0, 400, player.incanLight.rad);
                }
            }


            // Damages target. Quick note: Slugcats don't have actual HP without the DLC. As long as attacks don't deal at least 1 damage to them, they're effectively invincible.
            target.Violence(self.mainBodyChunk, new Vector2?(self.mainBodyChunk.vel), target.bodyChunks[otherChunk], null, dmgType, DMG, STUN);
            if (target is Player plr) // WITH the DLC, though, that's not an issue. You've just gotta make sure to address Slugcat HP separately from Violence, since it... wasn't integrated directly into Violence, for some reason.
            {
                plr.playerState.permanentDamageTracking += DMG;
                if (plr.playerState.permanentDamageTracking >= 1)
                {
                    plr.Die();
                }
            }
            if (self.animation == Player.AnimationIndex.Flip && !self.flipFromSlide && target is Lizard liz && (liz.Template.type != CreatureTemplate.Type.RedLizard || liz.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TrainLizard))
            {
                if (self.animation == Player.AnimationIndex.Flip && !self.flipFromSlide)
                {
                    liz.turnedByRockCounter =
                        (liz.Template.type == HailstormEnums.Freezer) ? 75 :
                        (liz.Template.type == CreatureTemplate.Type.GreenLizard || liz.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard) ? 25 : 50;

                }
            }
            if (notAFly && player.longJumping)
            {
                self.Stun(15);
            }

            // Heat transfer
            self.Hypothermia += HEATLOSS;
            target.Hypothermia -= HEATLOSS * 0.75f;

            // Damage sounds
            if (target.State is HealthState targetHP && targetHP.ClampedHealth == 0f || target.State.dead)
            {
                self.room.PlaySound(SoundID.Rock_Hit_Wall, self.mainBodyChunk, loop: false, 1.2f, 1f);
                self.room.PlaySound(SoundID.Rock_Hit_Creature, self.mainBodyChunk, loop: false, 1.7f, 1f);
            }
            else self.room.PlaySound(SoundID.Rock_Hit_Wall, self.mainBodyChunk, loop: false, 1.7f, 1f);

            if (notAFly)
            {
                if (self.animation == Player.AnimationIndex.Roll || (self.animation == Player.AnimationIndex.Flip && self.flipFromSlide)) // Stops movement if rolling or whiplashing. Flips keep going.
                {
                    self.animation = Player.AnimationIndex.None;
                    self.rollDirection = 0;
                    self.bodyChunks[0].vel *= 0f;
                    self.bodyChunks[1].vel *= 0f;
                }
                else if (self.animation == Player.AnimationIndex.RocketJump || player.longJumping) // Bounces you backward if rocket-jumping or long-jumping. Slides keep going.
                {
                    self.animation = Player.AnimationIndex.None;
                    player.longJumping = false;
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

        }
        else if (target.State.dead)
        {
            self.room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, self.mainBodyChunk);
        }

        #endregion
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