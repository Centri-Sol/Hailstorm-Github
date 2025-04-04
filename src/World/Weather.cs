﻿namespace Hailstorm;

public class Weather
{
    public static void Hooks()
    {

        On.AbstractCreature.OpportunityToEnterDen += ErraticWindCtrHideInDen;

        On.RainCycle.GetDesiredCycleLength += Incan_ShorterCycles;
        On.RainCycle.ctor += HailstormCycleTypes;
        On.RainCycle.Update += NewCycleTypesUpdate;
        On.MoreSlugcats.BlizzardGraphics.Update += HSStormUpdate;
        On.MoreSlugcats.BlizzardGraphics.CycleUpdate += NewCycleTypesUpdate;

        On.HUD.RainMeter.ctor += ExtremeWindRingsSetup;
        On.HUD.RainMeter.Update += ErraticWindCycleUpdate;
        On.HUD.RainMeter.Draw += ErraticWindCycleMeter;
        IncanCyclesILHooks();

        On.PhysicalObject.WeatherInertia += WindChanges;

        On.Room.Update += ErraticWindRandomEvents;

        On.MoreSlugcats.BlizzardSound.Update += BlizzardPleaseSHUTUPYoureSoLOUD;

        _ = new Hook(typeof(RainCycle).GetMethod("get_RainApproaching", Public | NonPublic | Instance), (Func<RainCycle, float> orig, RainCycle cycle) => IsIncanStory(cycle.world.game) && FogPrecycle ? 1f - (cycle.preTimer / (float)cycle.maxPreTimer) : orig(cycle));


    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSEnums.Incandescent;
        // ^ Returns true if all of the given conditions are met, or false otherwise.
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool HailPrecycle;
    public static readonly Dictionary<CreatureTemplate.Type, float> HailResistantCreatures = new()
    {
        { CreatureTemplate.Type.BigSpider, 0.9f },
        { CreatureTemplate.Type.SpitterSpider, 0.8f },
        { CreatureTemplate.Type.SmallCentipede, 0.75f },
        { CreatureTemplate.Type.Centipede, 0.75f },
        { CreatureTemplate.Type.Centiwing, 0.75f },
        { DLCSharedEnums.CreatureTemplateType.MirosVulture, 0.75f },
        { DLCSharedEnums.CreatureTemplateType.EelLizard, 0.35f },
        { DLCSharedEnums.CreatureTemplateType.MotherSpider, 0.35f },
        { CreatureTemplate.Type.KingVulture, 0.25f },
        { DLCSharedEnums.CreatureTemplateType.StowawayBug, 0.25f },
        { CreatureTemplate.Type.RedCentipede, 0.25f },
        { HSEnums.CreatureType.Cyanwing, 0.25f },
        { CreatureTemplate.Type.Salamander, 0 },
        { CreatureTemplate.Type.Deer, 0 },
        { DLCSharedEnums.CreatureTemplateType.BigJelly, 0 },
        { HSEnums.CreatureType.GorditoGreenieLizard, 0 }
    }; // For each listed creature type, hail damage and stun are multiplied by the given number.

    public static bool FogPrecycle;

    public static bool ErraticWindCycle;
    public static List<int> WindIntervalDurations;
    public static List<float> WindIntervalIntensities;
    public static List<bool> ExtremeWindIntervals;
    public static int WindcycleTimer;
    public static int WindcycleCount;
    public static HUDCircle[] ExtremeWindRings;
    public static float CurrentWindIntensity;
    public static int WindInterval;
    public static bool TimerBlink;
    public static int TimePerPip = 1200;
    // A whole 9 things to set up Erratic Wind cycles. Most of this is necessary to color cycle pips the way I did.

    public static readonly List<CreatureTemplate.Type> ErraticWindFearers = new()
    {
        CreatureTemplate.Type.BlueLizard,
        CreatureTemplate.Type.Vulture,
        CreatureTemplate.Type.Centiwing,
        CreatureTemplate.Type.CicadaA,
        CreatureTemplate.Type.CicadaB,
        CreatureTemplate.Type.PoleMimic,
        CreatureTemplate.Type.TentaclePlant,
        CreatureTemplate.Type.Scavenger,
        DLCSharedEnums.CreatureTemplateType.ScavengerElite,
        MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing,
        DLCSharedEnums.CreatureTemplateType.ZoopLizard,
        DLCSharedEnums.CreatureTemplateType.MirosVulture,
        HSEnums.CreatureType.Luminescipede
    };


    public static int LateBlizzardTime(World world)
    {
        return world.region is not null &&
            Regions.RegionData.TryGetValue(world.region, out RegionInfo rI)
            ? world.rainCycle.cycleLength + rI.lateBlizzardStartTimeAfterCycleEnds
            : RainWorldGame.BlizzardHardEndTimer(world.game.IsStorySession);
    }


    //-----------------------------------------
    public static void LockDestination(ArtificialIntelligence AI)
    {
        if (AI?.creature?.abstractAI is not null && CWT.AbsCtrData.TryGetValue(AI.creature, out CWT.AbsCtrInfo aI))
        {
            aI.destinationLocked = true;
            AI.creature.abstractAI.freezeDestination = true;
        }
    }

    public static void ErraticWindCtrHideInDen(On.AbstractCreature.orig_OpportunityToEnterDen orig, AbstractCreature absLmn, WorldCoordinate den)
    {
        int denTime = 0;
        if (absLmn?.state is not null &&
            absLmn.state is GlowSpiderState gs &&
            gs.behavior == ReturnPrey)
        {
            string itemList = "";
            for (int g = absLmn.stuckObjects.Count - 1; g >= 0; g--)
            {
                if (absLmn.stuckObjects[g] is null ||
                    absLmn.stuckObjects[g] is not AbstractPhysicalObject.CreatureGripStick lmnGrip ||
                    lmnGrip.A != absLmn ||
                    lmnGrip.B is null) // Goes through every object the spider is carrying...
                {
                    continue;
                }

                if (itemList == "")
                {
                    if (lmnGrip.B.realizedObject is not null &&
                        absLmn.realizedObject is Luminescipede lmn)
                    {
                        float massFac = Mathf.InverseLerp(lmn.TotalMass, lmn.BackholdMassLimit, lmnGrip.B.realizedObject.TotalMass);
                        denTime = (int)Mathf.Lerp(60, 560, massFac / lmnGrip.B.stuckObjects.Count);
                    }
                    else
                    {
                        denTime = 200;
                    }

                    absLmn.remainInDenCounter += denTime;
                }
                else
                {
                    itemList += " and ";
                }

                itemList += (lmnGrip.B is AbstractCreature) ?
                    (lmnGrip.B as AbstractCreature).creatureTemplate.name : lmnGrip.B.type.value;

                int itemGrabbersCount = lmnGrip.B.stuckObjects.Count;
                for (int b = lmnGrip.B.stuckObjects.Count - 1; b >= 0; b--)
                {
                    if (lmnGrip.B.stuckObjects[b] is null ||
                        lmnGrip.B.stuckObjects[b] is not AbstractPhysicalObject.CreatureGripStick otherGrip ||
                        otherGrip.A == absLmn ||
                        otherGrip.B != lmnGrip.B)
                    {
                        continue;
                    }
                    AbstractCreature grabber = otherGrip.A as AbstractCreature;
                    if (grabber.state is null ||
                        grabber.state is not GlowSpiderState othergs)
                    {
                        continue;
                    }
                    othergs.ChangeBehavior(Idle, 2);
                    grabber.remainInDenCounter += denTime + (20 * (itemGrabbersCount - b));
                    if (grabber.realizedCreature is not null and
                        Luminescipede otherLmn)
                    {
                        otherLmn.currentPrey = null;
                    }
                    grabber.DropCarriedObject(otherGrip.grasp);  // ...and makes all other grabbers let go of the item.
                    grabber.Move(den);
                    absLmn.Room.MoveEntityToDen(grabber);
                }

                lmnGrip.B.Abstractize(den);

                if (absLmn.realizedObject is Luminescipede itemcarryingLmn &&
                    itemcarryingLmn.ConsiderUseful(lmnGrip.B?.realizedObject))
                {
                    absLmn.DropCarriedObject(lmnGrip.grasp);
                }
                else
                {
                    lmnGrip.B.Destroy();
                }
            }
            if (itemList != "")
            {
                if (absLmn.realizedCreature is not null and
                    Luminescipede lmn)
                {
                    lmn.currentPrey = null;
                    lmn.useItem = null;
                    lmn.fearSource = null;
                }
                gs.ChangeBehavior(Idle, 2);
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("[Hailstorm] Lumin returned " + itemList + " to home den at (" + den.ToString() + ")!");
                }
            }

        }
        orig(absLmn, den);
        if (IsIncanStory(absLmn?.world?.game) && !absLmn.InDen && ErraticWindCycle && ExtremeWindIntervals[WindInterval] && ErraticWindFearers.Contains(absLmn.creatureTemplate.type))
        {
            absLmn.Room.MoveEntityToDen(absLmn);
        }
    }

    //-----------------------------------------

    public static int Incan_ShorterCycles(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle rc)
    {
        if (IsIncanStory(rc.world.game))
        {
            rc.cycleLength = Random.Range(6, 11) * 1200;
            rc.baseCycleLength = rc.cycleLength;
            rc.sunDownStartTime = (int)Random.Range(rc.cycleLength * 0.9f, rc.cycleLength * 1.1f);
        }
        return orig(rc);
    }
    public static void HailstormCycleTypes(On.RainCycle.orig_ctor orig, RainCycle rc, World world, float minutes)
    {
        if (HailPrecycle)
        {
            HailPrecycle = false;
        }

        if (FogPrecycle)
        {
            FogPrecycle = false;
        }

        if (ErraticWindCycle)
        {
            ErraticWindCycle = false;
        }

        if (WindIntervalDurations is not null)
        {
            WindIntervalDurations = null;
        }

        if (WindIntervalIntensities is not null)
        {
            WindIntervalIntensities = null;
        }

        if (WindcycleCount != -1)
        {
            WindcycleCount = -1;
        }

        if (WindcycleTimer != 0)
        {
            WindcycleTimer = 0;
        }

        if (CurrentWindIntensity != 0)
        {
            CurrentWindIntensity = 0;
        }

        if (WindInterval != 0)
        {
            WindInterval = 0;
        }

        if (TimerBlink)
        {
            TimerBlink = false;
        }

        orig(rc, world, minutes);

        if (IsIncanStory(world?.game) &&
            world.region is not null &&
            Regions.RegionData.TryGetValue(world.region, out RegionInfo rI))
        {
            StoryGameSession sgs = world.game.session as StoryGameSession;

            if (sgs.saveState.cycleNumber < 3)
            {
                if (rc.maxPreTimer > 0)
                {
                    rc.maxPreTimer = 0;
                    rc.preTimer = 0;
                }
                return;
            }

            Random.State state = Random.state;
            Random.InitState((sgs.saveState.totTime + (sgs.saveState.cycleNumber * sgs.saveStateNumber.Index)) * 10000);
            if (rc.maxPreTimer > 0)
            {
                FogPrecycle = Random.value < rI.freezingFogInsteadOfHailChance;
                HailPrecycle = !FogPrecycle;
            }
            if (Random.value < rI.erraticWindChance && rc.cycleLength >= 7200)
            {
                ErraticWindCycle = true;
            }
            Random.state = state;

            if (ErraticWindCycle)
            {
                SetUpNewWindcycle(rc);
                string logMessage = Random.Range(0, 4) switch
                {
                    0 => "Oh hey the wind's picking up early. This'll be fun.",
                    1 => "WHOA, the wind's going CRAZY this cycle!",
                    2 => "What the heck's going on with the wind this cycle?",
                    _ => "The winds are looking pretty erratic this cycle...",
                };
                Debug.Log("[Hailstorm] " + logMessage);
            }
            if (FogPrecycle)
            {
                string logMessage = Random.Range(0, 4) switch
                {
                    0 => "Is that... fog? That's not good.",
                    1 => "Yeeeesh, what's with all this cold mist?",
                    2 => "It's, uh... a bit CHILLIER this cycle, isn't it? And foggier...",
                    _ => "Lookin' a little foggy this cycle.",
                };
                Debug.Log("[Hailstorm] " + logMessage);
            }
            else if (HailPrecycle)
            {
                string logMessage = Random.Range(0, 4) switch
                {
                    0 => "Do you hear that? That sounds like hail.",
                    1 => "OH, that's not snow outside; that's HAIL!",
                    2 => "Oh hey, it's hailing outside. That stuff looks like it'll HURT!",
                    _ => "There's a hailstorm going on this cycle!",
                };
                Debug.Log("[Hailstorm] " + logMessage);
            }
        }

        if (RainWorld.ShowLogs && (HSRemix.HailstormStowawaysEverywhere.Value is true || IsIncanStory(world?.game)) && !FogPrecycle)
        {
            Debug.Log("[Hailstorm] Stowaways have all been WOKED!");
        }

    }
    public static void NewCycleTypesUpdate(On.RainCycle.orig_Update orig, RainCycle rc)
    {
        orig(rc);

        if (ErraticWindCycle && rc is not null)
        {

            WindcycleTimer = (WindcycleCount < 0) ?
                    rc.preTimer : (rc.cycleLength * (1 + WindcycleCount)) - rc.timer;

            if (WindcycleTimer < 1 ||
                    WindIntervalDurations is null || WindIntervalDurations.Count < 1 ||
                    WindIntervalIntensities is null || WindIntervalIntensities.Count < 1 ||
                    ExtremeWindIntervals is null || ExtremeWindIntervals.Count < 1)
            {
                SetUpNewWindcycle(rc);
            }

        }

    }
    public static void HSStormUpdate(On.MoreSlugcats.BlizzardGraphics.orig_Update orig, BlizzardGraphics BG, bool eu)
    {
        orig(BG, eu);

        if (ErraticWindCycle && IsIncanStory(BG?.room?.game))
        {
            if (BG.updateCount < BG.updateDelay - 1)
            {
                BG.updateCount = BG.updateDelay - 1;
            }
        }

    }
    public static void NewCycleTypesUpdate(On.MoreSlugcats.BlizzardGraphics.orig_CycleUpdate orig, BlizzardGraphics BG)
    {
        orig(BG);
        if (BG?.room?.world?.region is not null && IsIncanStory(BG.room.game))
        {
            RainCycle RainCycle = BG.room.world.rainCycle;
            string region = BG.room.world.region.name;
            float RainIntensity = BG.room.roomSettings.RainIntensity;
            int TimeTilRain = RainCycle.TimeUntilRain;

            if (ErraticWindCycle)
            {
                float WindStrength = 0;
                int WindcycleProgress = WindIntervalDurations.Sum() - WindcycleTimer;
                int WindcycleInterval = 0;
                for (int i = 0; i < WindIntervalDurations.Count; i++)
                {
                    WindcycleInterval += WindIntervalDurations[i];
                    if (WindcycleProgress <= WindcycleInterval)
                    {
                        WindInterval = i;
                        WindStrength = WindIntervalIntensities[i];
                        if (ExtremeWindIntervals[i])
                        {
                            WindStrength *= 2;
                        }
                        break;
                    }
                }

                if (region == "LF")
                {
                    WindStrength *= 1.5f;
                }
                else if (region is "HI" or "SU" or "CC")
                {
                    WindStrength += 0.2f;
                }

                BG.WindStrength = (CurrentWindIntensity < 0.35f) ? WindStrength * RainIntensity : Mathf.Lerp(CurrentWindIntensity, WindStrength * RainIntensity, 0.02f);
                CurrentWindIntensity = BG.WindStrength;
                BG.BlizzardIntensity = BG.WindStrength;
                BG.SnowfallIntensity = Mathf.Clamp(BG.WindStrength * 5f, 0f, 4f) * RainIntensity;
                BG.WhiteOut = Mathf.Pow(BG.WindStrength, 1.3f) * RainIntensity;

                TimeTilRain = -RainCycle.timer;
                if (RainCycle.maxPreTimer > 0)
                {
                    TimeTilRain -= RainCycle.maxPreTimer - RainCycle.preTimer;
                }

                float windAngleExtremeness =
                    region is "SI" or "SU" or "MS" ? 3 :
                    region is "HI" or "LF" or "CC" or "SL" ? 2 : 1;

                float whatEven = Mathf.Sin(TimeTilRain / 900f) * windAngleExtremeness;
                whatEven = Mathf.Lerp(whatEven, Mathf.Sin(TimeTilRain / 240f), 0.3f + (Mathf.Sin(TimeTilRain / 100f) / 4f));
                float lerpedWind = Mathf.Lerp(BG.WindAngle, whatEven, 0.1f) * RainIntensity;

                BG.WindAngle = Mathf.Lerp(lerpedWind, Mathf.Sign(whatEven), 0.2f * (0f - Mathf.Abs(lerpedWind)));
                BG.WindAngle = Mathf.Lerp(BG.WindAngle, Mathf.Sign(BG.WindAngle), 0.05f * Mathf.InverseLerp(0, 0.2f, Mathf.Abs(BG.WindAngle)));
                Debug.Log("Current wind angle1: " + BG.WindAngle);
                if (Mathf.Abs(BG.WindAngle) > 0.8f)
                {
                    BG.WindAngle = Mathf.Lerp(BG.WindAngle, 0.8f * Mathf.Sign(BG.WindAngle), 0.5f);
                    Debug.Log("Current wind angle2: " + BG.WindAngle);
                }

            }
            else if (BG.room.world.region.regionParams.glacialWasteland)
            {
                float CycleProgression = RainCycle.CycleProgression;
                if (BG.room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
                {
                    CycleProgression = Mathf.InverseLerp(0f, 0.75f, RainIntensity);
                    TimeTilRain = (int)(3000f * Mathf.InverseLerp(1f, 0.5f, RainIntensity));
                }
                float windAngleExtremeness =
                    region is "SI" or "SU" or "MS" ? 3 :
                    region is "HI" or "LF" or "CC" or "SL" ? 2f : 1f;
                float num3 = Mathf.Sin(TimeTilRain / 900f) * windAngleExtremeness;
                num3 = Mathf.Lerp(num3, Mathf.Sin(TimeTilRain / 240f), 0.3f + (Mathf.Sin(TimeTilRain / 100f) / 4f));
                float num4 = Mathf.Lerp(BG.WindAngle, num3, 0.1f) * RainIntensity;

                BG.WindAngle =
                    Mathf.Lerp(num4, Mathf.Sign(num3), 0.2f * (0f - Mathf.Abs(num4))) * Mathf.Lerp(0f, 0.75f, CycleProgression * 3f);

                float maxWindStrength =
                    region is "SI" or "SU" ? 1.4f :
                    region is "HI" or "LF" or "CC" ? 1.2f : 1f;

                if (region == "SU")
                {
                    BG.WindStrength = Mathf.Lerp(0.7f, maxWindStrength, Mathf.InverseLerp(RainCycle.cycleLength, 0, TimeTilRain)) * RainIntensity;
                    BG.BlizzardIntensity = Mathf.Lerp(0.7f, maxWindStrength, Mathf.InverseLerp(RainCycle.cycleLength, 0, TimeTilRain)) * RainIntensity;
                    BG.SnowfallIntensity = Mathf.Lerp(0.7f, maxWindStrength, Mathf.Clamp(BG.WindStrength * 6f, 1.5f, 5f)) * RainIntensity;
                    BG.WhiteOut = BG.BlizzardIntensity * RainIntensity;
                }
                else if (region is "HI" or "LF" or "CC")
                {
                    BG.WindStrength = Mathf.Lerp(maxWindStrength * 0.25f, maxWindStrength, Mathf.InverseLerp(RainCycle.cycleLength * 1.25f, 3000f, TimeTilRain)) * RainIntensity;
                    BG.BlizzardIntensity = Mathf.Lerp(0.3f, maxWindStrength, Mathf.InverseLerp(RainCycle.cycleLength * 1.25f, 3000f, TimeTilRain)) * RainIntensity;
                    BG.SnowfallIntensity = Mathf.Lerp(0.3f, maxWindStrength, Mathf.Clamp(BG.WindStrength * 5f, 1f, 4.5f)) * RainIntensity;
                    BG.WhiteOut = Mathf.Lerp(0.3f, maxWindStrength, Mathf.Pow(BG.BlizzardIntensity, 1.4f)) * RainIntensity;
                }
                else
                {
                    BG.WindStrength = Mathf.InverseLerp(RainCycle.cycleLength, 2000f, TimeTilRain) * RainIntensity;
                    BG.BlizzardIntensity = Mathf.InverseLerp(RainCycle.cycleLength, 2000f, TimeTilRain) * RainIntensity;
                    BG.SnowfallIntensity = Mathf.Clamp(BG.WindStrength * 5f, 0f, 4f) * RainIntensity;
                    BG.WhiteOut = Mathf.Pow(BG.BlizzardIntensity, 1.3f) * RainIntensity;
                }
            }

            BG.lerpBypass = true;

        }
    }

    public static void ExtremeWindRingsSetup(On.HUD.RainMeter.orig_ctor orig, RainMeter rm, HUD.HUD HUD, FContainer container)
    {
        if (ExtremeWindRings is not null)
        {
            ExtremeWindRings = null;
        }

        orig(rm, HUD, container);

        if (!ErraticWindCycle || rm.circles is null || rm.circles.Length < 1)
        {
            return;
        }
        ExtremeWindRings = new HUDCircle[rm.circles.Length];
        for (int i = 0; i < ExtremeWindRings.Length; i++)
        {
            ExtremeWindRings[i] = new HUDCircle(HUD, HUDCircle.SnapToGraphic.smallEmptyCircle, container, 255);
        }

        if (HUD.owner is Player owner && IsIncanStory(owner.room?.game))
        {
            rm.halfTimeShown = true;
        }

    }
    public static void ErraticWindCycleUpdate(On.HUD.RainMeter.orig_Update orig, RainMeter rm)
    {
        if (!rm.halfTimeShown &&
            rm.hud?.owner is not null &&
            rm.hud.owner is Player owner &&
            owner?.room is not null &&
            IsIncanStory(owner.room.game))
        {
            rm.halfTimeShown = true;
        }

        orig(rm);

        if (!ErraticWindCycle ||
            ExtremeWindRings is null)
        {
            return;
        }

        if (TimerBlink)
        {
            TimerBlink = false;
            rm.halfTimeBlink = 200;
        }

        if (rm.hud?.owner is null ||
            rm.hud.owner is not Player owner2 ||
            owner2?.room is null)
        {
            return;
        }

        for (int i = 0; i < ExtremeWindRings.Length; i++)
        {
            HUDCircle circle = rm.circles[i];
            ExtremeWindRings[i].Update();
            ExtremeWindRings[i].rad = circle.rad;
            if (rm.fade > 0f || rm.lastFade > 0f)
            {
                ExtremeWindRings[i].thickness = circle.thickness;
                ExtremeWindRings[i].snapRad = circle.snapRad;
                ExtremeWindRings[i].snapThickness = circle.snapThickness;
                ExtremeWindRings[i].pos = circle.pos;
                ExtremeWindRings[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
            }
        }

    }
    public static void ErraticWindCycleMeter(On.HUD.RainMeter.orig_Draw orig, RainMeter rm, float timeStacker)
    {
        orig(rm, timeStacker);
        if (!(ErraticWindCycle || HailPrecycle || FogPrecycle))
        {
            return;
        }

        bool windcycleNoGo =
            !ErraticWindCycle ||
            WindIntervalIntensities is null ||
            WindIntervalDurations is null ||
            ExtremeWindRings is null ||
            ExtremeWindRings.Length < 1;

        for (int c = 0; c < rm.circles.Length; c++)
        {
            HUDCircle circle = rm.circles[c];
            if (circle.rad <= 0 || (circle.fade <= 0f && circle.lastFade <= 0f))
            {
                continue;
            }

            if (circle?.hud?.owner is not null && circle.hud.owner is Player owner && IsIncanStory(owner.room?.game))
            {
                RainCycle rc = owner.room.world.rainCycle;
                float cutOffTime = rc.maxPreTimer / (float)rm.circles.Length * c;
                if (rc.preTimer > cutOffTime)
                {
                    if (HailPrecycle && circle.sprite.element.name != "HSHailPip")
                    {
                        circle.sprite.element = Futile.atlasManager.GetElementWithName("HSHailPip");
                    }
                    else if (FogPrecycle && circle.sprite.element.name != "HSFogPip")
                    {
                        circle.sprite.element = Futile.atlasManager.GetElementWithName("HSFogPip");
                    }

                    if ((HailPrecycle || FogPrecycle) && rc.preTimer <= cutOffTime + 160)
                    {
                        circle.sprite.scale *= Mathf.Lerp(0.6f, 1, Mathf.InverseLerp(cutOffTime, cutOffTime + 160, rc.preTimer));
                    }
                }

            }

        }

        if (windcycleNoGo)
        {
            return;
        }

        for (int c = 0; c < ExtremeWindRings.Length; c++)
        {
            FSprite ring = ExtremeWindRings[c].sprite;
            FSprite circle = rm.circles[c].sprite;

            int pipInterval = (ExtremeWindRings.Length - c) * TimePerPip;
            int WindcycleInterval = 0;
            ring.isVisible = false;
            for (int i = 0; i < WindIntervalDurations.Count; i++)
            {
                WindcycleInterval += WindIntervalDurations[i];
                if (pipInterval <= WindcycleInterval)
                {
                    ring.isVisible = circle.isVisible && ExtremeWindIntervals[i];
                    break;
                }
            }
            if (ring.isVisible)
            {
                ring.x = circle.x;
                ring.y = circle.y;
                ring.scale = circle.scale;
                ring.alpha = circle.alpha;
                circle.color = Custom.hexToColor("32FFBA");
                ring.color = Custom.hexToColor("009993");
                ring.shader = circle.shader;
                string ringSpriteName =
                    circle.element == Futile.atlasManager.GetElementWithName("HSHailPip") ? "HSHailPipOutline" :
                    circle.element == Futile.atlasManager.GetElementWithName("HSFogPip") ? "HSFogPipOutline" : "smallEmptyCircle";
                ring.element = circle.element == Futile.atlasManager.GetElementWithName("Futile_White") ?
                    circle.element : Futile.atlasManager.GetElementWithName(ringSpriteName);
            }
        }
    }
    public static void IncanCyclesILHooks()
    {

        IL.HUD.RainMeter.Update += IL =>
        {
            ILCursor c = new(IL);
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            ILLabel? label = null;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            if (
            c.TryGotoNext(x => x.MatchStfld<RainMeter>(nameof(RainMeter.fRain))) &&
            c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HudPart>(nameof(HudPart.hud)),
                x => x.MatchLdfld<HUD.HUD>(nameof(HUD.HUD.owner)),
                x => x.MatchIsinst<Player>(),
                x => x.MatchLdfld<UpdatableAndDeletable>(nameof(UpdatableAndDeletable.room)),
                x => x.MatchBrfalse(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((RainMeter rm) => IsNotErraticWindCycle(rm));
                c.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] An IL hook for Erratic Wind Cycles isn't working! Report this if you see it!");
            }
        };

    }

    public static void ErraticWindRandomEvents(On.Room.orig_Update orig, Room room)
    {
        orig(room);
        if (IsIncanStory(room?.game) &&
            ErraticWindCycle &&
            ExtremeWindIntervals[WindInterval] &&
            room.world.region is not null &&
            Regions.RegionData.TryGetValue(room.world.region, out RegionInfo rI) &&
            rI.erraticWindChance > 0 &&
            Random.value < rI.erraticWindDandelionChance &&
            room.blizzardGraphics is not null &&
            !room.roomSettings.effects.Contains(room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.BorderPushBack)))
        {
            WorldCoordinate? spawnCoordinate = null;
            int startTile = 0;
            int endTile = room.abstractRoom.size.x;
            if (room.blizzardGraphics.WindAngle != 0)
            {
                if (room.blizzardGraphics.WindAngle > 0)
                {
                    endTile = (int)Mathf.Lerp(endTile, startTile, Mathf.Abs(room.blizzardGraphics.WindAngle));
                }
                else
                {
                    startTile = (int)Mathf.Lerp(startTile, endTile, Mathf.Abs(room.blizzardGraphics.WindAngle));
                }
            }
            if (startTile > 0)
            {
                for (int t = startTile; t <= endTile; t++)
                {
                    if (!room.GetTile(new IntVector2(t, room.abstractRoom.size.y)).Solid && Random.value < Mathf.Lerp(0.2f, 1, Mathf.InverseLerp(startTile, endTile, t)))
                    {
                        spawnCoordinate = room.GetWorldCoordinate(new IntVector2(t, room.abstractRoom.size.y + 10));
                        break;
                    }
                }
            }
            else
            {
                for (int t = endTile; t >= startTile; t--)
                {
                    if (!room.GetTile(new IntVector2(t, room.abstractRoom.size.y)).Solid && Random.value < Mathf.Lerp(0.2f, 1, Mathf.InverseLerp(endTile, startTile, t)))
                    {
                        spawnCoordinate = room.GetWorldCoordinate(new IntVector2(t, room.abstractRoom.size.y + 10));
                        break;
                    }
                }
            }
            if (spawnCoordinate.HasValue)
            {
                if (Random.value < rI.erraticWindWrongDandelionTypeChance)
                {
                    AbstractCreature PeachSpider = new(room.world, StaticWorld.GetCreatureTemplate(HSEnums.CreatureType.PeachSpider), null, spawnCoordinate.Value, room.game.GetNewID());
                    OtherCreatureChanges.CustomFlags(PeachSpider);
                    room.abstractRoom.AddEntity(PeachSpider);
                    PeachSpider.RealizeInRoom();
                    foreach (BodyChunk chunk in PeachSpider.realizedObject.bodyChunks)
                    {
                        chunk.pos.y += 150;
                    }
                }
                else
                {
                    AbstractConsumable DandelionPeach = new(room.world, DLCSharedEnums.AbstractObjectType.DandelionPeach, null, spawnCoordinate.Value, room.game.GetNewID(), room.abstractRoom.index, -1, null);
                    room.abstractRoom.AddEntity(DandelionPeach);
                    DandelionPeach.RealizeInRoom();
                    foreach (BodyChunk chunk in DandelionPeach.realizedObject.bodyChunks)
                    {
                        chunk.pos.y += 100;
                    }
                }
            }
        }
    }

    //-----------------------------------------

    public static void WindChanges(On.PhysicalObject.orig_WeatherInertia orig, PhysicalObject obj)
    {
        List<Vector2> bodyChunkVels = null;
        if (IsIncanStory(obj?.room?.game) &&
            obj.room.blizzard &&
            obj.room.blizzardGraphics is not null &&
            obj.room.blizzardGraphics.WindStrength > 0.5f)
        {
            bodyChunkVels = new();
            for (int b = 0; b < obj.bodyChunks.Length; b++)
            {
                bodyChunkVels.Add(obj.bodyChunks[b].vel);
            }
        }
        orig(obj);
        if (bodyChunkVels is not null)
        {
            for (int b = 0; b < obj.bodyChunks.Length; b++)
            {
                if (obj.bodyChunks[b].vel != bodyChunkVels[b])
                {
                    obj.bodyChunks[b].vel = bodyChunkVels[b];
                }
            }

            if (obj is not Player && Random.value >= 0.1f)
            {
                return;
            }

            if (obj is Creature windImmune && (
                windImmune.Template.type == DLCSharedEnums.CreatureTemplateType.StowawayBug ||
                windImmune.Template.type == HSEnums.CreatureType.GorditoGreenieLizard))
            {
                return;
            }

            Color blizzardPixel;
            Vector2 baseWindPush;
            foreach (BodyChunk chunk in obj.bodyChunks)
            {
                blizzardPixel = obj.room.blizzardGraphics.GetBlizzardPixel((int)(chunk.pos.x / 20f), (int)(chunk.pos.y / 20f));

                baseWindPush = new Vector2(0f - obj.room.blizzardGraphics.WindAngle, 0.1f);
                baseWindPush *= blizzardPixel.g * (5f * obj.room.blizzardGraphics.WindStrength);

                Vector2 blizzPush =
                    Vector2.Lerp(baseWindPush, baseWindPush * 0.08f, obj.Submersion) * Mathf.InverseLerp(40f, 1f, obj.TotalMass);

                if (obj is Spear)
                {
                    Vector2 pushoffofedgePrevention = new(obj.bodyChunks[0].pos.x + (5 * Mathf.Sign(blizzPush.x)), obj.bodyChunks[0].pos.y - 10);
                    if (!obj.room.GetTile(pushoffofedgePrevention).Solid)
                    {
                        return;
                    }
                    else
                    {
                        blizzPush *= 0.05f;
                    }
                }

                blizzPush.y = 0;

                if (ErraticWindCycle)
                {
                    if (ExtremeWindIntervals[WindInterval])
                    {
                        blizzPush.x *= Mathf.Lerp(4f, 2f, Mathf.InverseLerp(0.35f, 1.6f, WindIntervalIntensities[WindInterval]));
                    }
                    else
                    {
                        blizzPush.x *= 2;
                    }
                }

                if (obj is Player plr)
                {
                    blizzPush *= plr.isGourmand ? 0.075f : 0.1f;
                    if (plr.graphicsModule is PlayerGraphics pg && pg.tail is not null)
                    {
                        for (int t = 0; t < pg.tail.Length; t++)
                        {
                            if (Random.value < 0.15f)
                            {
                                pg.tail[t].vel += new Vector2(blizzPush.x, -blizzPush.y) * Random.Range(3f, 4f);
                            }
                        }
                    }
                    if ((plr.animation == Player.AnimationIndex.None && plr.bodyMode == Player.BodyModeIndex.Crawl) || (chunk.index == 0 && (plr.animation == Player.AnimationIndex.HangFromBeam || plr.animation == Player.AnimationIndex.ClimbOnBeam)))
                    {
                        blizzPush /= 10f;
                    }
                }
                else if (obj is Creature ctr)
                {
                    CreatureTemplate.Type type = ctr.Template.type;

                    float windVulnerability = 1;
                    if (CustomTemplateInfo.WeatherResistances.Wind.ContainsKey(type))
                    {
                        windVulnerability = CustomTemplateInfo.WeatherResistances.Wind[type];
                    }

                    if (ctr is Lizard liz &&
                        liz.LegsGripping < 4 - (liz.TotalMass / 3f))
                    {
                        windVulnerability *= liz.LegsGripping / 4f;
                    }

                    if (windVulnerability > 0)
                    {
                        blizzPush *= windVulnerability;
                    }
                    else
                    {
                        return;
                    }
                }

                chunk.vel += blizzPush;

            }

        }
    }

    public static void SetUpNewWindcycle(RainCycle rc)
    {
        if ((rc.maxPreTimer > 0 && rc.preTimer < 1) || (rc.maxPreTimer < 1 && WindcycleCount > -1))
        {
            TimerBlink = true;
        }

        WindcycleTimer = rc.preTimer > 0 ? rc.maxPreTimer : rc.cycleLength >= 7200 ? rc.cycleLength : 7200;

        if (rc.preTimer < 1)
        {
            WindcycleCount++;
        }

        TimePerPip = 1200;
        int pipCount = rc.cycleLength / TimePerPip;
        if (pipCount > 30)
        {
            pipCount = 30;
            TimePerPip = WindcycleTimer / pipCount;
        }
        if (rc.preTimer > 0)
        {
            TimePerPip = WindcycleTimer / pipCount;
        }

        WindIntervalDurations = new List<int>();
        while (WindIntervalDurations.Sum() != WindcycleTimer)
        {
            int remainingTime = WindcycleTimer - WindIntervalDurations.Sum();
            if (remainingTime < TimePerPip)
            {
                if (remainingTime > 0)
                {
                    if (WindIntervalDurations.Count > 0)
                    {
                        WindIntervalDurations[WindIntervalDurations.Count - 1] += remainingTime;
                    }
                    else
                    {
                        WindIntervalDurations.Add(remainingTime);
                    }
                }
                else if (remainingTime < 0 && WindIntervalDurations.Count > 0)
                {
                    WindIntervalDurations[WindIntervalDurations.Count - 1] -= 30;
                }
            }
            else
            {
                int timerPips =
                    Random.value < 0.20f && rc.maxPreTimer - rc.preTimer + rc.timer < 14400 ? 1 :
                    Random.value < 0.33f && rc.maxPreTimer - rc.preTimer + rc.timer >= 9600 ? 3 : 2;

                WindIntervalDurations.Add(Mathf.Min(timerPips * TimePerPip, remainingTime));
            }
        }

        int windIntensityFac = (rc.preTimer > 0) ?
            WindcycleTimer : WindcycleTimer * WindcycleCount;

        WindInterval = WindIntervalDurations.Count - 1;
        WindIntervalIntensities = new List<float>();
        ExtremeWindIntervals = new List<bool>();
        for (int w = 0; w < WindIntervalDurations.Count; w++)
        {
            if (rc.preTimer > 0)
            {
                windIntensityFac -= WindIntervalDurations[w];
                WindIntervalIntensities.Add(Random.Range(0.35f, 0.7f) * (1 + Mathf.InverseLerp(0, WindcycleTimer, windIntensityFac)));
            }
            else
            {
                windIntensityFac += WindIntervalDurations[w];
                WindIntervalIntensities.Add(Random.Range(0.4f, 0.8f) * (1 + Mathf.InverseLerp(2400, WindcycleTimer + 24000, windIntensityFac)));
            }
            ExtremeWindIntervals.Add(false);
        }

        while (ExtremeWindIntervals.Count(extreme => extreme) < Mathf.Max(ExtremeWindIntervals.Count < 5 ? 1 : 2, ExtremeWindIntervals.Count / 3))
        {
            int interval = Random.Range(1, ExtremeWindIntervals.Count);
            if (!ExtremeWindIntervals[interval])
            {
                ExtremeWindIntervals[interval] = !ExtremeWindIntervals[interval - 1] && !(interval < ExtremeWindIntervals.Count - 1 && ExtremeWindIntervals[interval + 1]);
            }
        }
    }
    public static bool IsNotErraticWindCycle(RainMeter rm)
    {
        if (ErraticWindCycle &&
            rm?.hud?.owner is not null &&
            rm.hud.owner is Player owner &&
            owner.room?.world?.rainCycle is not null)
        {
            RainCycle rc = owner.room.world.rainCycle;
            rm.fRain = rc.preTimer > 0 ?
                1 : (float)WindcycleTimer / rc.cycleLength;
            return false;
        }
        return true;
    }

    public static void BlizzardPleaseSHUTUPYoureSoLOUD(On.MoreSlugcats.BlizzardSound.orig_Update orig, BlizzardSound blzSfx, bool eu)
    {
        orig(blzSfx, eu);
        if (IsIncanStory(blzSfx?.room?.game) && blzSfx.blizzWind is not null)
        {
            blzSfx.blizzWind.Volume *= 0.66f;
            blzSfx.blizzHowl.Volume *= 0.66f;
        }
    }
}