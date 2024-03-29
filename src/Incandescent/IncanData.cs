namespace Hailstorm;

public class IncanInfo
{
    public readonly bool isIncan;

    public static SlugcatStats.Name Incandescent = new("Incandescent");
    public SlugBaseCharacter Incan;

    public int rollExtender;
    public int rollFallExtender;

    public int wallRolling;
    public int wallRollDir;
    public bool wallrollJump;
    private int MaxWallRoll => ReadyToMoveOn ? 150 : 50;
    public float WallRollPower => Mathf.InverseLerp(MaxWallRoll, 0, wallRolling);

    public float BaseGlowRadius => 200;
    public LightSource Glow;
    public float[,] flicker;
    public float HypothermiaResistance;
    public int firefuel;
    public float FirefuelSoftCap => 2400;
    public float FirefuelFac => Mathf.InverseLerp(0, FirefuelSoftCap, firefuel);
    public int waterGlow;

    public int soak;
    public int MaxSoak = 7200;
    public float SoakFac => Mathf.InverseLerp(0, MaxSoak, soak);
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

    public bool ReadyToMoveOn;

    public bool longJumpReady;
    public bool longJumping;
    public bool highJump;
    public bool justLongJumped;

    public int singeFlipTimer;

    public bool inArena;
    public bool currentCampaignBeforeRiv;

    public float FoodGainMultiplier => HSRemix.IncanFoodGainMultiplier.Value;
    public float FoodCounter;
    public float floatFood;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public IncanInfo(Player player)
    {

        isIncan = player.slugcatStats.name == Incandescent;

        if (!isIncan)
        {
            return;
        }

        flicker = new float[2, 3];
        for (int i = 0; i < flicker.GetLength(0); i++)
        {
            flicker[i, 0] = 1f;
            flicker[i, 1] = 1f;
            flicker[i, 2] = 1f;
        }


        if (player.room.game.session is not null)
        {
            if (player.room.game.IsArenaSession)
            {
                inArena = true;
            }
            else if (player.room.game.session is StoryGameSession SGS)
            {
                LinkedList<SlugcatStats.Name> slugcatTimelineOrder = SlugcatStats.SlugcatTimelineOrder();
                bool? tooFar = null;
                foreach (var name in slugcatTimelineOrder)
                {
                    if (name == SGS.saveStateNumber)
                    {
                        tooFar = false;
                    }
                    else if (name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
                    {
                        tooFar = true;
                    }

                    if (tooFar.HasValue) break;
                }

                currentCampaignBeforeRiv = tooFar.HasValue && tooFar.Value is false;

            }
        }

    }

    //--------------------------------------------------------------------------------

    public void Update(Player self, bool eu)
    {

        TimerUpdate(self);

        //-------------------------------------------------

        HypothermiaUpdate(self);

        //-------------------------------------------------

        GlowUpdate(self);

        //-------------------------------------------------

        ParticleUpdate(self, eu);

    }

    public virtual void TimerUpdate(Player self)
    {

        // These two timers are used in the FoodEffects method above HailstormPlayerUpdate.
        if (self.objectInStomach is not null)
        {
            if (self.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
            {
                self.Hypothermia -= Mathf.Lerp(-0.0004f, 0, self.HypothermiaExposure); // Weakens the warmth of Lanterns inside the Incandescent's stomach.
            }
            else if (self.objectInStomach is AbstractIceChunk absIce)
            {
                float heat = 0.01f / 40f;
                if (absIce.freshness > 0)
                {
                    heat /= 4f;
                }
                absIce.size -= heat;
            }
        }

        if (firefuel > 0 || HSRemix.IncanWaveringFlame.Value is true)
        {
            firefuel--;
        }
        if (waterGlow > 0)
        {
            waterGlow--;
        }


        if (soak < MaxSoak &&
            self.Submersion > 0.5f) // Being in water will gradually build up a debuff for Incan, which weakens her flame based on remaining duration.
        {
            int soak = self.Submersion >= 1 ? 18 : 9;
            if (waterGlow > 0)
            {
                soak /= 3;
            }
            if (bubbleHeat)
            {
                soak /= 3;
            }
            soak += soak;
        }
        else
        if (soak > 0 &&
            self.Submersion < 0.1f) // When out of water, Incan's flame will gradually dry off if it's wet, though at a much slower rate.
        {
            soak -= 3;
        }

        if (bubbleHeat)
        {
            bubbleHeat = false;
        }

        soak = Mathf.Clamp(soak, 0, MaxSoak); // Prevents wetness from going below 0 or over 2400.
    }

    public virtual void HypothermiaUpdate(Player self)
    {
        bool HeatCosts =
            (inArena && HSRemix.IncanNoArenaDowngrades.Value is false) ||
            (!inArena && HSRemix.IncanArenaDowngradesOutsideOfArena.Value is true);


        if (self.HypothermiaGain > 0) // This is where the Incandescent's cold resistance happens.
        {
            HypothermiaResistance = HeatCosts ? 0.2f : 0.75f;

            if (self.isSlugpup)
            {
                HypothermiaResistance *= 0.85f;
            }
            if (self.Malnourished)
            {
                HypothermiaResistance *= 0.8f;
            }
            if (SoakFac > 0)
            {
                HypothermiaResistance *= Mathf.InverseLerp(1, 1 / 3f, SoakFac);
            }
            if (HSRemix.IncanWaveringFlame.Value is true && firefuel <= -1200)
            {
                HypothermiaResistance *= Mathf.InverseLerp(-8400, -1200, firefuel);
            }

            if (HypothermiaResistance != 0)
            {
                self.Hypothermia -= self.HypothermiaGain * HypothermiaResistance;
            }
        }

        if (HeatCosts)
        {
            // In Arena, the Incandescent's mobility will gradually drain their warmth during use, instead of only when you hit something.
            float HeatDrain = 0;
            if (self.animation == Player.AnimationIndex.Flip ||
                self.animation == Player.AnimationIndex.BellySlide)
            {
                HeatDrain += 0.006f;
            }
            if (self.animation == Player.AnimationIndex.Roll ||
                self.animation == Player.AnimationIndex.RocketJump)
            {
                HeatDrain += 0.0036f;
            }
            if (justLongJumped) // Well, long-jumps just drain a bunch of warmth instantly.
            {
                justLongJumped = false;
                HeatDrain += 0.15f;
            }

            if (!inArena)
            {
                HeatDrain /= 3f;
            }
            if (self.room?.blizzardGraphics is not null)
            {
                HeatDrain /= 3f;
            }

            self.Hypothermia += HeatDrain;
        }

        if (self.room is not null)
        {

            if (self.dead &&
                self.Hypothermia < 2)
            {
                self.Hypothermia += 0.00125f;
            }

            if (!self.dead &&
                currentCampaignBeforeRiv &&
                self.Hypothermia >= 1.5f)
            {
                if (self.graphicsModule is not null)
                {
                    (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (self.Hypothermia * 0.75f);
                }
                self.playerState.permanentDamageTracking += (self.Hypothermia > 1.8f) ? 0.002f : 0.001f;
                if (self.playerState.permanentDamageTracking >= 1)
                {
                    self.Die();
                }
            }

        }

        self.Hypothermia = Mathf.Clamp(self.Hypothermia, 0, 2);
    }

    public virtual void GlowUpdate(Player self)
    {
        // This first part sets up a flickering effect for the light using a float array.
        for (int i = 0; i < flicker.GetLength(0); i++)
        {
            flicker[i, 1] = flicker[i, 0];
            flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * (Random.value < 0.5f ? -1f : 1f);
            flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 1f / 30f);
            if (Random.value < 0.2f)
            {
                flicker[i, 2] = 1f + (Mathf.Pow(Random.value, 3f) * 0.2f * (Random.value < 0.5f ? -1f : 1f));
            }
            flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
        }

        TailGlowRadius(self, out float GlowRadius);

        Vector2 GlowPos;
        PlayerGraphics IncanGraphics = self.graphicsModule as PlayerGraphics;
        GlowPos = IncanGraphics?.tail is not null &&
            IncanGraphics.tail.Length > 0
            ? IncanGraphics.tail[IncanGraphics.tail.Length - 1].pos
            : self.bodyChunks[1].pos;

        if (Glow is null && !self.dead)
        {
            Glow = new LightSource(GlowPos, false, FireColor, self)
            {
                affectedByPaletteDarkness = 0,
                requireUpKeep = true,
                setAlpha = 1
            };
            self.room.AddObject(Glow);
        }
        else if (Glow is not null)
        {
            Glow.submersible = waterGlow > 0;
            Glow.stayAlive = true;
            Glow.setPos = new Vector2?(GlowPos);
            Glow.setRad = new float?(GlowRadius);
            Glow.color = FireColor;

            if (self.dead &&
                self.Hypothermia > 1.9f)
            {
                Glow.Destroy();
            }
            if (Glow.slatedForDeletetion ||
                Glow.room != self.room)
            {
                Glow = null;
            }
        }

    }

    public virtual void ParticleUpdate(Player self, bool eu)
    {
        if (self.room is null)
        {
            return;
        }

        PlayerGraphics IncanGraphics = self.graphicsModule as PlayerGraphics;

        Vector2 TailEndPos = // This complicated glowPos change is only here because of DMS. Good thing it wasn't so bad, or else I wouldn't have bothered.
                IncanGraphics.tail is null ? self.bodyChunks[1].pos :
                IncanGraphics.tail.Length > 3 ? IncanGraphics.tail[Random.Range(IncanGraphics.tail.Length - 1, IncanGraphics.tail.Length / 2)].pos :
                IncanGraphics.tail.Length > 2 ? IncanGraphics.tail[Random.Range(IncanGraphics.tail.Length - 1, IncanGraphics.tail.Length - 2)].pos :
                IncanGraphics.tail.Length > 0 ? IncanGraphics.tail[1].pos : self.bodyChunks[1].pos;

        bool MakeBigEmbers =
            longJumping ||
            self.animation == Player.AnimationIndex.BellySlide ||
            self.animation == Player.AnimationIndex.RocketJump ||
            self.animation == Player.AnimationIndex.Flip ||
            self.animation == Player.AnimationIndex.Roll;


        bool SweemBoosting =
            self.animation == Player.AnimationIndex.DeepSwim &&
            self.waterJumpDelay > 0 &&
            self.Submersion > 0;


        float EmberTimerTick = soak > 0 && self.Submersion <= 0.5f
            ? Mathf.Lerp(0.05f, 0.5f, SoakFac)
            : (MakeBigEmbers ? 0.25f : 0.17f) *
                (self.Malnourished ? 0.7f : 1) *
                (self.bodyMode == Player.BodyModeIndex.Swimming ? 0.7f : 1);
        smallEmberTimer += EmberTimerTick;


        if (smallEmberTimer >= 1)
        {
            if (soak > 0 && self.Submersion <= 0.5f)
            {
                BodyChunk dripChunk = self.bodyChunks[Random.Range(0, self.bodyChunks.Length - 1)];
                self.room.AddObject(new WaterDrip(dripChunk.pos + (Custom.RNV() * dripChunk.rad * Random.value), default, false));
            }
            else if (self.Submersion > 0.5f)
            {
                self.room.AddObject(new Bubble(TailEndPos, new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f)), bottomBubble: false, fakeWaterBubble: false));
            }
            else
            {
                self.room.AddObject(new EmberSprite(TailEndPos, FireColor, 1f));
            }
            smallEmberTimer = 0;
        }


        if (MakeBigEmbers || SweemBoosting)
        {
            bigEmberTimer +=
                (self.Malnourished ? 0.14f : 0.2f) *
                (self.bodyMode == Player.BodyModeIndex.Swimming ? 0.7f : 1);

            if (bigEmberTimer >= 1)
            {
                bigEmberTimer = 0;
                if (self.bodyMode == Player.BodyModeIndex.Swimming)
                {
                    self.room.AddObject(new Bubble(TailEndPos, new Vector2(Random.Range(-2f, 2f), Random.Range(4.5f, 9f)), bottomBubble: false, fakeWaterBubble: false));
                }
                else
                {
                    self.room.AddObject(new EmberSprite(TailEndPos, FireColor, 2f));
                }
            }
        }


        if (MakeBigEmbers &&
            fireSmoke is null)
        {
            fireSmoke = new HailstormFireSmokeCreator(self.room);
        }
        if (fireSmoke is not null)
        {
            fireSmoke.Update(eu);

            float SmokeRadMult = Mathf.Lerp(1, 2 / 3f, SoakFac);
            if (FirefuelFac > 0)
            {
                SmokeRadMult *= Mathf.Lerp(1, 1.5f, FirefuelFac);
            }

            if (self.whiplashJump || self.rocketJumpFromBellySlide)
            {
                for (int f = 0; f < (self.whiplashJump ? 9 : 3); f++)
                {
                    if (self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 350f) &&
                        fireSmoke.AddParticle(TailEndPos, self.bodyChunks[1].vel * 0.4f, 40) is Smoke.FireSmoke.FireSmokeParticle whiplashFireSmoke)
                    {
                        whiplashFireSmoke.colorFadeTime = 35;
                        whiplashFireSmoke.effectColor = FireColor;
                        whiplashFireSmoke.colorA = self.ShortCutColor();
                        whiplashFireSmoke.rad = 2.5f * SmokeRadMult;
                    }
                }
            }

            if (MakeBigEmbers &&
                self.room.ViewedByAnyCamera(self.bodyChunks[1].pos, 350f) &&
                fireSmoke.AddParticle(TailEndPos, self.bodyChunks[1].vel * 0.4f, 40) is Smoke.FireSmoke.FireSmokeParticle fireSmokeParticle)
            {
                fireSmokeParticle.colorFadeTime = 35;
                fireSmokeParticle.effectColor = FireColor;
                fireSmokeParticle.colorA = self.ShortCutColor();
                if (longJumping)
                {
                    SmokeRadMult *= 2.25f;
                }
                else SmokeRadMult = self.animation.value switch
                {
                    "Flip" => 2.25f,
                    _ => 1.75f
                };
                if (ReadyToMoveOn)
                {
                    SmokeRadMult *= 1.15f;
                }
                fireSmokeParticle.rad *= SmokeRadMult;
            }
            else if (!MakeBigEmbers)
            {
                fireSmoke = null;
            }
        }

    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual bool CollisionDamageValues(Player self, out float DMG, out float STUN, out float HEATLOSS, out int BURNTIME)
    {
        DMG = 0;
        STUN = 0;
        HEATLOSS = 0;
        BURNTIME = 0;

        if (self.animation == Player.AnimationIndex.BellySlide)
        {
            DMG = 0.50f;
            STUN = 20f;
            HEATLOSS = 0.06f;
        }
        else
        if (self.animation == Player.AnimationIndex.Flip)
        {
            if (self.flipFromSlide)
            {
                DMG = 2f;
                STUN = 120f;
                HEATLOSS = 0.24f;
                BURNTIME = 500; // +0.50 DMG; 2.50 total
            }
            else if (wallrollJump)
            {
                DMG = 0.75f;
                STUN = 60f;
                HEATLOSS = 0.20f;
                BURNTIME = 500; // +0.50 DMG; 1.25 total
            }
            else if (singeFlipTimer >= 15)
            {
                DMG = 0.50f;
                STUN = 20f;
                HEATLOSS = 0.06f;
            }
            else
            {
                return false;
            }
        }
        else
        if (longJumping)
        {
            DMG = 0.75f;
            STUN = 40f;
            HEATLOSS = 0.12f;
        }
        else
        if (self.animation == Player.AnimationIndex.RocketJump)
        {
            DMG = 1.00f;
            STUN = 75f;
            HEATLOSS = 0.16f;
        }
        else
        if (self.animation == Player.AnimationIndex.Roll)
        {
            DMG = 0.75f;
            STUN = 60f;
            HEATLOSS = 0.20f;
            BURNTIME = 500; // +0.50 DMG; 1.25 total
        }
        else
        {
            return false;
        }

        DMG *= HSRemix.IncanCollisionDamageMultiplier.Value;
        HEATLOSS *= HSRemix.IncanCollisionDamageMultiplier.Value;
        if (self.Malnourished) // Weakens damage and stun if you're starving.
        {
            DMG *= 0.8f;
            STUN *= 0.8f;
            BURNTIME = (int)(BURNTIME * 0.8f);
            HEATLOSS *= 0.6f;
        }
        if (self.isSlugpup) // Slightly weakens damage and stun if you're a slugpup.
        {
            DMG *= 0.85f;
            STUN *= 0.85f;
            BURNTIME = (int)(BURNTIME * 0.85f);
            HEATLOSS *= 0.7f;
        }
        if (HSRemix.IncanWaveringFlame.Value is true && firefuel <= -1200)
        {
            float Mult = Mathf.Lerp(0.5f, 1, Mathf.InverseLerp(-8400, -1200, firefuel));
            DMG *= Mult;
            STUN *= Mult;
            BURNTIME = (int)(BURNTIME * Mult);
            HEATLOSS *= Mult;
        }
        if (ReadyToMoveOn)
        {
            if (BURNTIME > 0)
            {
                BURNTIME += BURNTIME / 2;
            }
            else
            {
                DMG += 0.25f;
            }
            STUN += 20;
        }

        return true;
    }

    public virtual void TailGlowRadius(Player self, out float GlowRadius)
    {
        GlowRadius = BaseGlowRadius * flicker[0, 0];
        if (self.glowing)
        {
            GlowRadius *= 1.5f;
        }
        if (self.isSlugpup)
        {
            GlowRadius *= 0.85f;
        }
        if (self.Malnourished)
        {
            GlowRadius *= 0.8f;
        }
        if (self.Hypothermia > 0)
        {
            GlowRadius *= Mathf.InverseLerp(2, 0, self.Hypothermia);
        }
        if (GlowRadius > 0)
        {
            if (SoakFac > 0)
            {
                GlowRadius *= Mathf.Lerp(1, 2 / 3f, SoakFac);
            }
            if (FirefuelFac > 0)
            {
                GlowRadius *= Mathf.Lerp(1, 1.5f, FirefuelFac);
            }
        }
    }

    public virtual float FoodMultiplier(Player self)
    {
        float Multiplier = FoodGainMultiplier;
        if (self.Malnourished)
        {
            Multiplier *= 0.75f;
        }
        return Multiplier;
    }

    public virtual bool StopLongJump(Player self)
    {
        if (self.bodyChunks[0].contactPoint != default ||
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
            (self.grasps[1]?.grabbed is not null && self.HeavyCarry(self.grasps[1].grabbed)))
        {
            return true;
        }
        return false;
    }

}