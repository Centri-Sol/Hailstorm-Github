using UnityEngine.UI;

namespace Hailstorm;

public class OtherCreatureChanges
{

    public static void Hooks()
    {

        On.Creature.ctor += CreatureCWT;
        On.AbstractCreature.ctor += AbsCtrCWT;

        // Cicada hooks
        On.Cicada.GenerateIVars += CicadaH_iVars;
        On.Cicada.ctor += CicadaH_ctor;
        On.Player.GraphicsModuleUpdated += WinterSquitJumpHeight1;
        On.Player.MovementUpdate += WinterSquitJumpHeight2;
        On.Cicada.GrabbedByPlayer += CicadaH_Stamina;
        On.CicadaGraphics.ApplyPalette += CicadaH_Palette;
        On.CicadaGraphics.InitiateSprites += WinterSquitSpriteSize;
        On.CicadaGraphics.DrawSprites += WinterSquitSpriteCondense;

        // Dropwig hooks
        On.DropBug.ctor += WinterwigSetup;
        On.DropBugGraphics.ApplyPalette += WinterwigColors;
        On.DropBugGraphics.InitiateSprites += WinterwigSize1;
        On.DropBugGraphics.DrawSprites += WinterwigSize2;

        // Stowaway hooks
        On.MoreSlugcats.StowawayBugAI.ctor += WAKETHEFUCKUP;
        On.MoreSlugcats.StowawayBug.Update += StowawayUpdate;
        On.MoreSlugcats.StowawayBug.Eat += StowFoodAway;
        On.MoreSlugcats.StowawayBug.Die += StowawayProvidesFood;
        On.MoreSlugcats.StowawayBugState.StartDigestion += EATFASTERDAMNIT;
        On.MoreSlugcats.StowawayBug.Violence += StowawayViolence;
        On.Creature.SpearStick += StowawayToughSides;

        // Grabby Plant hooks
        On.PoleMimic.Update += ErraticWindPoleHide;
        On.PoleMimic.Act += AngrierPolePlants;
        On.PoleMimicGraphics.ApplyPalette += WinterPolePlantDifferences;
        On.PoleMimicGraphics.DrawSprites += PolePlantColors;

        On.TentaclePlant.Update += ErraticWindKelpHide;
        On.TentaclePlantGraphics.ApplyPalette += MonsterKelpColors;

        // Big Jellyfish & Slime Mold
        On.MoreSlugcats.BigJellyFish.Collide += BigJellyCRONCH;
        On.MoreSlugcats.BigJellyFish.Die += BIGJellyfishMold;

        // Scavengers
        On.ScavengerAI.DecideBehavior += ErraticWindScavHide;

        // HOLY SHIT IT'S ALEX YEEK
        On.MoreSlugcats.YeekGraphics.CreateCosmeticAppearance += YeekColors;
        On.MoreSlugcats.YeekGraphics.DrawSprites += YeekEyesBecauseTheirColorIsStupidlySemiHardcoded;

        //-------------------------------------
        // Hooks for creatures in general
        On.Room.ctor += BootlegIProvideWarmth;
        On.Room.AddObject += BootlegIPWAdder;
        On.Room.CleanOutObjectNotInThisRoom += BootlegIPWRemover;

        On.CreatureTemplate.ctor_Type_CreatureTemplate_List1_List1_Relationship += NewDamageTypeResistances;
        On.Player.EatMeatOmnivoreGreenList += CreatureEdibility;
        On.AbstractCreature.Update += HypothermiaSafeAreas;
        On.Creature.Update += CreatureTimersAndMechanics;
        On.Creature.HypothermiaBodyContactWarmup += HypothermiaBodyContactChanges;
        On.Creature.Violence += MinorViolenceTweaks;
        On.Creature.Die += CreatureDeathChanges;
        On.Player.Grabability += GrababilityChanges;
        On.AbstractCreature.WantToStayInDenUntilEndOfCycle += LuminDenUpdate;
        On.AbstractCreature.setCustomFlags += ActivateCustomFlags;
        HailstormCreatureFlagChecks();

        On.OverseerAbstractAI.HowInterestingIsCreature += OverseerHailstormCreatureInterest;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSEnums.Incandescent;
    }

    //-----------------------------------------

    public static void CreatureCWT(On.Creature.orig_ctor orig, Creature ctr, AbstractCreature absCtr, World world)
    {
        orig(ctr, absCtr, world);

        if (!CWT.CreatureData.TryGetValue(ctr, out _))
        {
            CWT.CreatureData.Add(ctr, new CWT.CreatureInfo(ctr));
        }

        if (absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly && ctr.grasps is null)
        {
            ctr.grasps = new Creature.Grasp[ctr.Template.grasps];
        }
        if (IsIncanStory(world?.game))
        {
            if (absCtr.creatureTemplate.type == CreatureTemplate.Type.LanternMouse || absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
            {
                absCtr.state.meatLeft = 1;
            }
            else if (absCtr.creatureTemplate.type == CreatureTemplate.Type.TubeWorm)
            {
                absCtr.state.meatLeft = 0;
            }
        }

        if (absCtr.spawnData is not null)
        {
            string spawnData = "";
            for (int s = 0; s < absCtr.spawnData.Length; s++)
            {
                spawnData += absCtr.spawnData[s];
            }
            Debug.Log(absCtr.creatureTemplate.name + " spawndata: " + spawnData);
        }
        else
        {
            Debug.Log("nope lol"); // this is for testing
        }
    }

    public static void AbsCtrCWT(On.AbstractCreature.orig_ctor orig, AbstractCreature absCtr, World world, CreatureTemplate temp, Creature realizedCtr, WorldCoordinate pos, EntityID ID)
    {
        orig(absCtr, world, temp, realizedCtr, pos, ID);

        if (!CWT.AbsCtrData.TryGetValue(absCtr, out _))
        {
            CWT.AbsCtrData.Add(absCtr, new CWT.AbsCtrInfo(absCtr));
        }

        if (absCtr is not null &&
            (IsIncanStory(absCtr.world?.game) || HSEnums.CreatureType.GetAllCreatureTypes().Contains(temp.type)))
        {
            CustomFlags(absCtr);
        }

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Cicadas

    public static void CicadaH_iVars(On.Cicada.orig_GenerateIVars orig, Cicada sqt)
    {
        orig(sqt);
        if (sqt is not null && sqt.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate)
        {
            Random.State state = Random.state;
            Random.InitState(sqt.abstractCreature.ID.RandomSeed);
            HSLColor color = new(Custom.ClampedRandomVariation(220 / 360f, 65 / 360f, 0.5f), 0.75f, 0.75f);
            float fatness = Custom.WrappedRandomVariation(0.475f, 0.125f, 0.33f) * 2f;
            int bustedWing = -1;
            if (Random.value < 0.175f)
            {
                bustedWing = Random.Range(0, 4);
            }
            sqt.iVars = new Cicada.IndividualVariations(fatness, fatness * Random.Range(0.8f, 1.2f), Random.value, Mathf.Lerp(1.66f, 1.8f, Random.value), Mathf.Lerp(1.66f, 1.8f, Random.value), Mathf.Lerp(1.5f, 0.8f, Random.value * Random.value), Custom.ClampedRandomVariation(0.5f, 0.2f, 0.33f) * 2f, bustedWing, color);
            Random.state = state;
        }
    }
    public static void CicadaH_ctor(On.Cicada.orig_ctor orig, Cicada sqt, AbstractCreature absSqt, World world, bool gender)
    {
        orig(sqt, absSqt, world, gender);
        if (sqt.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate)
        {
            absSqt.state.meatLeft = 1;
            sqt.buoyancy += 0.02f;
            sqt.bounce += 0.1f;
            sqt.surfaceFriction -= 0.1f;
            if (sqt.bodyChunks is not null)
            {
                for (int i = 0; i < sqt.bodyChunks.Length; i++)
                {
                    if (sqt.bodyChunks[i] is not null)
                    {
                        sqt.bodyChunks[i].mass *= 0.5f;
                        sqt.bodyChunks[i].rad *= 0.5f;
                    }
                }
            }
            if (sqt.bodyChunkConnections[0] is not null)
            {
                sqt.bodyChunkConnections[0].distance *= 0.5f;
            }
        }
    }
    public static void WinterSquitJumpHeight1(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    {
        orig(self, actuallyViewed, eu);
        if (self is null)
        {
            return;
        }

        int squitCount = 0;
        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i]?.grabbed is null ||
                self.HeavyCarry(self.grasps[i].grabbed) ||
                self.grasps[i].grabbed is not Cicada sqt ||
                sqt.Template.ancestor.type != HSEnums.CreatureType.SnowcuttleTemplate)
            {
                continue;
            }

            Vector2 val2 = Custom.DirVec(self.DangerPos, new Vector2(self.DangerPos.x, self.DangerPos.y - 1));
            float boostMult = squitCount == 0 ? 1 : 0.33f;
            boostMult *= Mathf.InverseLerp(25f, 15f, self.eatMeat);
            float num6 = self.grasps[i].grabbedChunk.mass / (self.mainBodyChunk.mass + self.grasps[i].grabbedChunk.mass);
            if (self.enteringShortCut.HasValue)
            {
                num6 = 0f;
            }
            else if (self.grasps[i].grabbed.TotalMass < self.TotalMass)
            {
                num6 /= 2f;
            }
            if (!self.enteringShortCut.HasValue || 1 > boostMult)
            {
                self.mainBodyChunk.pos += val2 * num6 * boostMult;
                self.mainBodyChunk.vel += val2 * num6 * boostMult;
                self.grasps[i].grabbedChunk.pos -= val2 * boostMult * (1f - num6);
                self.grasps[i].grabbedChunk.vel -= val2 * boostMult * (1f - num6);
            }
            if (self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam && self.animation != Player.AnimationIndex.BeamTip && self.animation != Player.AnimationIndex.StandOnBeam)
            {
                self.grasps[i].grabbedChunk.vel.y += self.grasps[i].grabbed.gravity * (1f - self.grasps[i].grabbedChunk.submersion) * 1.5f;
            }

            squitCount++;

        }
    }
    public static void WinterSquitJumpHeight2(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self is null)
        {
            return;
        }

        int squitCount = 0;
        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i]?.grabbed is null ||
                self.HeavyCarry(self.grasps[i].grabbed) ||
                self.grasps[i].grabbed is not Cicada sqt ||
                sqt.Template.ancestor.type != HSEnums.CreatureType.SnowcuttleTemplate)
            {
                continue;
            }

            float boostMult = squitCount == 0 ? 1 : 0.33f;

            if (self.IsIncan( out IncanInfo Incan) && (
                    (self.bodyMode == Player.BodyModeIndex.Default && self.animation == Player.AnimationIndex.None) ||
                    self.animation == Player.AnimationIndex.Flip ||
                    self.animation == Player.AnimationIndex.RocketJump ||
                    Incan.longJumping))
            {
                float jumpBoost =
                    self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.RocketJump ? 0.66f : 1f;

                self.bodyChunks[0].vel.y += sqt.LiftPlayerPower * boostMult * jumpBoost;
                self.bodyChunks[1].vel.y += sqt.LiftPlayerPower * boostMult / 4f * jumpBoost;
                if (self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.RocketJump)
                {
                    self.bodyChunks[0].vel.x += sqt.LiftPlayerPower * 0.08f * self.flipDirection;
                    self.bodyChunks[1].vel.x += sqt.LiftPlayerPower * 0.08f * self.flipDirection;
                }
                sqt.currentlyLiftingPlayer = true;
                if (sqt.LiftPlayerPower > 2f / 3f)
                {
                    self.standing = false;
                }
            }
            else
            {
                self.mainBodyChunk.vel.y += sqt.LiftPlayerPower * boostMult / 2f;
                sqt.currentlyLiftingPlayer = false;
            }
            if (self.bodyChunks[1].ContactPoint.y < 0 && self.bodyChunks[1].lastContactPoint.y == 0 && sqt.LiftPlayerPower > 1f / 3f)
            {
                self.standing = true;
            }

            squitCount++;
        }
    }
    public static void CicadaH_Stamina(On.Cicada.orig_GrabbedByPlayer orig, Cicada sqt)
    {
        if (sqt is not null && sqt.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate && sqt.currentlyLiftingPlayer)
        {
            sqt.stamina += 1f / (sqt.gender ? 160f : 150f);
            orig(sqt);
            sqt.flying = sqt.stamina > 1f / 3f;
        }
        else
        {
            orig(sqt);
        }
    }
    public static void CicadaH_Palette(On.CicadaGraphics.orig_ApplyPalette orig, CicadaGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(sg, sLeaser, rCam, palette);
        if (sg?.cicada is not null && sg.cicada.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate)
        {
            bool whiteCicada = sg.cicada.gender;

            Color val = Color.Lerp(HSLColor.Lerp(sg.iVars.color, new HSLColor(sg.iVars.color.hue, 0, 0.4f), 0.9f).rgb, whiteCicada ? palette.fogColor : palette.blackColor, 0.2f);

            sg.shieldColor =
                    Color.Lerp(val, new HSLColor(sg.iVars.color.hue, 0.875f, 0.5f).rgb, 0.85f);

            sLeaser.sprites[sg.BodySprite].color = val;
            sLeaser.sprites[sg.HeadSprite].color = val;
            sLeaser.sprites[sg.HighlightSprite].color = Color.Lerp(val, Color.white, whiteCicada ? 0.4f : 0.25f);
            sLeaser.sprites[sg.ShieldSprite].color = sg.shieldColor;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 2 - 1; j >= 0; j--)
                {
                    sLeaser.sprites[sg.WingSprite(i, j)].color = Color.Lerp(Color.Lerp(val, sg.shieldColor, 0.3f), whiteCicada ? Color.white : palette.blackColor, 0.3f);
                    sLeaser.sprites[sg.TentacleSprite(i, j)].color = val;
                }
            }
            if (whiteCicada)
            {
                sg.eyeColor = Color.Lerp(val, palette.blackColor, 0.8f);
                sLeaser.sprites[sg.EyesASprite].color = sg.eyeColor;
                sLeaser.sprites[sg.EyesBSprite].color = Color.Lerp(sg.iVars.color.rgb, Color.gray, 0.3f);
            }
            else
            {
                sg.eyeColor = Color.Lerp(sg.iVars.color.rgb, Color.gray, 0.3f);
                sLeaser.sprites[sg.EyesASprite].color = sg.eyeColor;
                sLeaser.sprites[sg.EyesBSprite].color = palette.blackColor;
            }
        }
    }
    public static void WinterSquitSpriteSize(On.CicadaGraphics.orig_InitiateSprites orig, CicadaGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(sg, sLeaser, rCam);
        if (sg?.cicada is not null && sg.cicada.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate)
        {
            for (int s = 0; s < sLeaser.sprites.Length; s++)
            {
                if (sLeaser.sprites[s] is TriangleMesh || s == sg.HeadSprite || s == sg.ShieldSprite)
                {
                    continue;
                }

                if (s != sg.EyesASprite && s != sg.EyesBSprite)
                {
                    if (s == sg.BodySprite)
                    {
                        sLeaser.sprites[s].scale = 0.7f;
                        sLeaser.sprites[s].scaleY *= 1.15f;
                        sLeaser.sprites[s].scaleX *= 0.67f;
                    }
                    else if (s is < 10 or > 13)
                    {
                        sLeaser.sprites[s].scale *= 0.6f;
                    }
                    else
                    {
                        sLeaser.sprites[s].scale *= 1.2f;
                    }
                }
                else
                {
                    sLeaser.sprites[s].scale *= 1.25f;
                }

            }
        }
    }
    public static void WinterSquitSpriteCondense(On.CicadaGraphics.orig_DrawSprites orig, CicadaGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(sg, sLeaser, rCam, timeStacker, camPos);
        if (sg?.cicada is not null && sg.cicada.Template.ancestor.type == HSEnums.CreatureType.SnowcuttleTemplate)
        {
            for (int s = 0; s < sLeaser.sprites.Length; s++)
            {
                bool v = sLeaser.sprites[s] is TriangleMesh;
                if (v)
                {
                    continue;
                }
                else if (s == sg.ShieldSprite || s == sg.EyesASprite || s == sg.EyesBSprite)
                {
                    sLeaser.sprites[s].x = Mathf.Lerp(sLeaser.sprites[s].x, sLeaser.sprites[sg.ShieldSprite].x, s == sg.ShieldSprite ? 0.65f : 0.75f);
                    sLeaser.sprites[s].y = Mathf.Lerp(sLeaser.sprites[s].y, sLeaser.sprites[sg.ShieldSprite].y, s == sg.ShieldSprite ? 0.65f : 0.75f);
                }
                else
                {
                    sLeaser.sprites[s].x = Mathf.Lerp(sLeaser.sprites[s].x, sLeaser.sprites[sg.ShieldSprite].x, 0.33f);
                    sLeaser.sprites[s].y = Mathf.Lerp(sLeaser.sprites[s].y, sLeaser.sprites[sg.ShieldSprite].y, 0.33f);
                }
            }
        }
    }


    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Dropwigs

    public static void WinterwigSetup(On.DropBug.orig_ctor orig, DropBug wig, AbstractCreature absWig, World world)
    {
        orig(wig, absWig, world);
        if (IsIncanStory(world?.game) && wig.abstractCreature.Winterized)
        {
            absWig.state.meatLeft = 4;
            if (wig.bodyChunks is not null)
            {
                for (int i = 0; i < wig.bodyChunks.Length; i++)
                {
                    wig.bodyChunks[i].rad *= 1.33f;
                    wig.bodyChunks[i].mass *= 1.33f;
                }
                for (int i = 0; i < wig.bodyChunkConnections.Length; i++)
                {
                    wig.bodyChunkConnections[i].distance *= 1.33f;
                }
            }
        }
    }
    public static void WinterwigColors(On.DropBugGraphics.orig_ApplyPalette orig, DropBugGraphics DBG, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(DBG, sLeaser, rCam, palette);
        if (IsIncanStory(DBG?.bug?.room?.game) && DBG.bug.abstractCreature.Winterized)
        {
            DBG.blackColor = Custom.HSL2RGB(DBG.hue, 0.033f, 0.4f);
            DBG.shineColor = palette.blackColor;
            DBG.camoColor = Custom.HSL2RGB(DBG.hue, 0, 0.3f);
            DBG.RefreshColor(0f, sLeaser);
        }
    }
    public static void WinterwigSize1(On.DropBugGraphics.orig_InitiateSprites orig, DropBugGraphics DBG, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(DBG, sLeaser, rCam);
        if (IsIncanStory(DBG?.bug?.room?.game) && DBG.bug.abstractCreature.Winterized)
        {
            sLeaser.sprites[DBG.HeadSprite].scale *= 1.33f;
            for (int i = 0; i < 10; i++)
            {
                sLeaser.sprites[DBG.SegmentSprite(i)].scale *= 1.33f;
            }

        }
    }
    public static void WinterwigSize2(On.DropBugGraphics.orig_DrawSprites orig, DropBugGraphics DBG, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(DBG, sLeaser, rCam, timeStacker, camPos);
        if (IsIncanStory(DBG?.bug?.room?.game) && DBG.bug.abstractCreature.Winterized)
        {
            for (int s = 0; s < sLeaser.sprites.Length; s++)
            {
                if (sLeaser.sprites[s] is TriangleMesh mesh)
                {
                    for (int v = 1; v < mesh.vertices.Length; v++)
                    {
                        Vector2 distance = mesh.vertices[v] - Vector2.Lerp(mesh.vertices[0], mesh.vertices[mesh.vertices.Length / 2], 0.5f);
                        mesh.vertices[v] += distance * 0.33f;
                    }
                }
            }
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void GrappleWormsBreatheGooder(On.TubeWorm.orig_Update orig, TubeWorm worm, bool eu)
    {
        orig(worm, eu);
        if (IsIncanStory(worm?.room?.game) && !worm.dead && worm.mainBodyChunk.submersion > 0.5f)
        {
            worm.lungs += Mathf.Min(worm.lungs + (0.85f / 160f), 1f);
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Stowaways

    public static void WAKETHEFUCKUP(On.MoreSlugcats.StowawayBugAI.orig_ctor orig, StowawayBugAI stwAwyAI, AbstractCreature absCtr, World world)
    {
        orig(stwAwyAI, absCtr, world);
        if (stwAwyAI is not null && (IsIncanStory(world?.game) || HSRemix.HailstormStowawaysEverywhere.Value is true))
        {
            if (!stwAwyAI.activeThisCycle)
            {
                stwAwyAI.activeThisCycle = !Weather.FogPrecycle;
                stwAwyAI.behavior = Weather.FogPrecycle ?
                    StowawayBugAI.Behavior.EscapeRain : StowawayBugAI.Behavior.Idle;
            }
        }
    }
    public static void StowawayUpdate(On.MoreSlugcats.StowawayBug.orig_Update orig, StowawayBug stwAwy, bool eu)
    {
        orig(stwAwy, eu);
        if (stwAwy is not null && (IsIncanStory(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out CWT.AbsCtrInfo aI) && aI.ctrList is not null)
        {
            if (aI.ctrList.Count > 4)
            {
                aI.ctrList.RemoveAt(0);
            }
            if (stwAwy.State.dead && CWT.CreatureData.TryGetValue(stwAwy, out CWT.CreatureInfo cI))
            {
                if (aI.functionTimer == -1 && stwAwy.State is StowawayBugState SBS && SBS.digestionLength > 0 && aI.ctrList.Count > 0)
                {
                    aI.functionTimer = 0;
                    SBS.digestionLength = (int)Mathf.Lerp(0, 2400, Mathf.InverseLerp(1, 8, aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.baseDamageResistance));
                }
                if (cI.impactCooldown < -1)
                {
                    cI.impactCooldown++;
                }
                if (cI.impactCooldown == -2 && stwAwy.room.world is not null && stwAwy.room.abstractRoom is not null)
                {
                    int smallCreatures = 0;
                    for (int c = 0; c < aI.ctrList.Count; c++)
                    {
                        if (aI.ctrList[c].creatureTemplate.smallCreature ||
                            aI.ctrList[c].creatureTemplate.type == CreatureTemplate.Type.SeaLeech ||
                            aI.ctrList[c].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
                        {
                            smallCreatures++;
                        }
                    }
                    for (bool addCtr = true; aI.functionTimer != 1; addCtr = Random.value < 1.75f - ((aI.ctrList.Count - smallCreatures) * 0.75f))
                    {
                        if (addCtr)
                        {
                            aI.ctrList.AddRange(StowawayIndigestion(stwAwy));
                        }
                        else
                        {
                            aI.functionTimer = 1;
                        }
                    }
                    if (aI.ctrList.Count > 0 && aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.type is not null)
                    {

                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit, stwAwy.DangerPos, 1.5f, Random.Range(0.25f, 0.4f));
                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit, stwAwy.DangerPos, 1.5f, Random.Range(0.75f, 0.9f));
                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit_Hit_NPC, stwAwy.DangerPos, 1.5f, Random.Range(0.25f, 0.4f));

                        int foodAtOnce = 0;
                        for (int f = aI.ctrList.Count - 1; f >= 0; f--)
                        {
                            if (aI.ctrList[f].creatureTemplate.type == aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.type)
                            {
                                foodAtOnce++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (int j = foodAtOnce; j > 0; j--)
                        {
                            IntVector2 tilePosition = stwAwy.room.GetTilePosition(stwAwy.DangerPos - new Vector2(0, 25f));
                            WorldCoordinate worldCoordinate = stwAwy.room.GetWorldCoordinate(tilePosition);

                            AbstractCreature eatenCtr = aI.ctrList[aI.ctrList.Count - 1];
                            AbstractCreature newCtr = new(stwAwy.room.world, eatenCtr.creatureTemplate, null, worldCoordinate, eatenCtr.ID);

                            bool dead = false;
                            CreatureTemplate.Type type = newCtr.creatureTemplate.type;
                            bool tough =
                                type == CreatureTemplate.Type.SeaLeech ||
                                type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
                                type == CreatureTemplate.Type.DropBug ||
                                type == CreatureTemplate.Type.RedCentipede ||
                                type == CreatureTemplate.Type.RedLizard ||
                                type == HSEnums.CreatureType.FreezerLizard ||
                                type == HSEnums.CreatureType.Cyanwing;
                            if (Random.value < 0.6f)
                            {
                                dead = true;
                            }
                            else if (Random.value < 5 / 8f)
                            {
                                if (newCtr.state is HealthState HS && eatenCtr.state is HealthState HS2)
                                {
                                    float minPossibleHP =
                                        (tough ? -0.20f : -0.50f) + HSRemix.StowawayFoodSurvivalBonus.Value;

                                    HS.health =
                                        Mathf.Min(HS2.health, Random.Range(minPossibleHP, Mathf.Lerp(0.75f, 1, HSRemix.StowawayFoodSurvivalBonus.Value)));
                                }
                                else if (Random.value < 0.5f)
                                {
                                    dead = true;
                                }
                            }

                            if (dead && newCtr.state is not null)
                            {
                                newCtr.state.alive = false;
                                if (newCtr.realizedCreature is not null)
                                {
                                    newCtr.realizedCreature.dead = true;
                                }
                            }

                            stwAwy.room.abstractRoom.AddEntity(newCtr);
                            CustomFlags(newCtr);
                            newCtr.RealizeInRoom();
                            newCtr.realizedCreature.mainBodyChunk.vel.x += Random.Range(-3f, 3f);
                            if (newCtr.state is not null && newCtr.state.alive)
                            {
                                newCtr.realizedCreature.Stun(Random.Range(120, 240));
                            }
                            newCtr.realizedCreature.killTag = stwAwy.abstractCreature;

                            aI.ctrList.RemoveAt(aI.ctrList.Count - 1);
                        }


                        if (aI.functionTimer != 1)
                        {
                            aI.functionTimer = 1;
                        }
                        cI.impactCooldown -= Random.Range(15, 40);

                        /*
                        if (RainWorld.ShowLogs)
                        {
                            Debug.Log(
                                aI.ctrList.Count > 10 ? "[Hailstorm] DEAR LORD THAT STOWAWAY WAS HUNGRY; IT SPAT OUT " + aI.ctrList.Count + " CREATURES!" :
                                aI.ctrList.Count > 05 ? "[Hailstorm] Stowaway spat out a whopping " + aI.ctrList.Count + " creatures!" :
                                "[Hailstorm] Stowaway spat out " + aI.ctrList.Count + " creatures!");
                        }
                        */
                    }
                }
            }
        }
    }
    public static void StowFoodAway(On.MoreSlugcats.StowawayBug.orig_Eat orig, StowawayBug stwAwy, bool eu)
    {
        if (stwAwy?.eatObjects is not null && (IsIncanStory(stwAwy.abstractCreature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out CWT.AbsCtrInfo aI) && aI.ctrList is not null)
        {
            for (int i = stwAwy.eatObjects.Count - 1; i >= 0; i--)
            {
                if (stwAwy.eatObjects[i].progression > 1f && stwAwy.eatObjects[i].chunk.owner is not null && stwAwy.eatObjects[i].chunk.owner is Creature ctr && ctr is not Player)
                {
                    aI.ctrList.Add(ctr.abstractCreature);
                }
            }
        }
        orig(stwAwy, eu);
    }
    public static void StowawayProvidesFood(On.MoreSlugcats.StowawayBug.orig_Die orig, StowawayBug stwAwy)
    {
        if (stwAwy is not null && (IsIncanStory(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.CreatureData.TryGetValue(stwAwy, out CWT.CreatureInfo cI) && cI.impactCooldown >= 0)
        {
            cI.impactCooldown = Random.Range(-130, -40);
        }
        orig(stwAwy);
    }
    public static void EATFASTERDAMNIT(On.MoreSlugcats.StowawayBugState.orig_StartDigestion orig, StowawayBugState sbs, int cycleTime)
    {
        orig(sbs, cycleTime);
        if (sbs?.creature is not null && (IsIncanStory(sbs.creature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(sbs.creature, out CWT.AbsCtrInfo aI))
        {
            aI.functionTimer = -1;
        }
    }
    public static void StowawayViolence(On.MoreSlugcats.StowawayBug.orig_Violence orig, StowawayBug stwAwy, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float dmg, float stun)
    {
        if (stwAwy?.room is not null)
        {
            if (hitAppen is not null && source?.owner is not null && source.owner is Lizard liz && liz.LizardState is ColdLizState)
            {
                dmg = 0.02f; // Gives Stowaways protection against icy lizard bites, and only their bites. (Spit from Freezers bypasses this)
                stun = 0;
            }
            if (CWT.CreatureData.TryGetValue(stwAwy, out CWT.CreatureInfo cI) && (IsIncanStory(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true)) // I know adding "== true" is redundant, but I'm doing it here for clarity's sake.
            {
                dmg /= HSRemix.StowawayHPMultiplier.Value;
                if (hitAppen is null)
                {
                    if (stwAwy.AI.behavior == StowawayBugAI.Behavior.EscapeRain)
                    {
                        dmg *= 0.3f;
                    }
                    else if (source?.owner is not null && source.owner is Weapon && dirAndMomentum.HasValue && Mathf.Abs(dirAndMomentum.Value.y) >= Mathf.Abs(dirAndMomentum.Value.x * 3))
                    {
                        dmg *= 1.4f; // Takes bonus damage from weapons thrown upwards or downwards
                    }
                    else if (!dirAndMomentum.HasValue || Mathf.Abs(dirAndMomentum.Value.y) < Mathf.Abs(dirAndMomentum.Value.x * 3))
                    {
                        cI.hitDeflected = HSRemix.StowawayToughSides.Value;
                        dmg *= cI.hitDeflected ? 0 : 0.75f;
                        if (cI.hitDeflected && source is not null && hitChunk is not null)
                        {
                            for (int num = 10; num > 0; num--)
                            {
                                stwAwy.room.AddObject(new Spark(Vector2.Lerp(hitChunk.pos, source.pos, 0.5f), Custom.RNV(), Color.white, null, 15, 25));
                            }
                            if (source.owner is not null && source.owner is Player plr && plr.animation != Player.AnimationIndex.Flip)
                            {
                                plr.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, source.pos, 1.25f, 0.75f);
                                plr.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, source.pos, 1.5f, 0.75f);
                                if (plr.IsIncan(out IncanInfo Incan) && Incan.isIncan && !Incan.ReadyToMoveOn)
                                {
                                    plr.Stun(Random.Range(20, 30));
                                }
                            }
                        }
                    }
                }

                if (stwAwy.AI.behavior == StowawayBugAI.Behavior.Hidden ||
                    stwAwy.AI.behavior == StowawayBugAI.Behavior.Digesting ||
                    stwAwy.AI.behavior == StowawayBugAI.Behavior.EscapeRain)
                {
                    stwAwy.AI.behavior = StowawayBugAI.Behavior.Attacking;
                    Debug.Log("[Hailstorm] A Stowaway took damage and got aggravated!");
                }
            }
        }
        orig(stwAwy, source, dirAndMomentum, hitChunk, hitAppen, dmgType, dmg, stun);
    }
    public static bool StowawayToughSides(On.Creature.orig_SpearStick orig, Creature victim, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appen, Vector2 direction)
    {
        if (victim?.room is not null && victim is StowawayBug && CWT.CreatureData.TryGetValue(victim, out CWT.CreatureInfo cI) && cI.hitDeflected)
        {
            cI.hitDeflected = false;
            return false;
        }
        return orig(victim, source, dmg, chunk, appen, direction);
    }

    //--------------------------------------
    public static bool StoreCreatureInsteadOfDestroy(StowawayBug stwAwy)
    {
        Vector2 pos = stwAwy.firstChunk.pos;
        if (stwAwy?.eatObjects is not null && (IsIncanStory(stwAwy.abstractCreature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out CWT.AbsCtrInfo aI) && aI.ctrList is not null)
        {
            for (int i = stwAwy.eatObjects.Count - 1; i >= 0; i--)
            {
                if (stwAwy.eatObjects[i].progression > 1f && stwAwy.eatObjects[i].chunk.owner is not null && stwAwy.eatObjects[i].chunk.owner is Creature ctr)
                {
                    if (ctr is Player && ctr.room is not null)
                    {
                        AbstractCreature ctrCopy = new(ctr.room.world, ctr.Template, new Player(ctr.abstractCreature, ctr.room.world), ctr.abstractCreature.pos, ctr.abstractCreature.ID);
                        aI.ctrList.Add(ctrCopy);
                    }
                    else
                    {
                        aI.ctrList.Add(ctr.abstractCreature);
                    }

                    stwAwy.AI.tracker.ForgetCreature(ctr.abstractCreature);
                    ctr.RemoveFromRoom();
                    ctr.abstractPhysicalObject.Room.RemoveEntity(ctr.abstractPhysicalObject);
                    stwAwy.eatObjects.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }
    public static List<AbstractCreature> StowawayIndigestion(StowawayBug stwAwy)
    {
        List<CreatureTemplate.Type> regionSpawns = new();

        if (stwAwy.room.world.spawners is not null && stwAwy.room.world.spawners.Length > 0)
        {
            foreach (World.CreatureSpawner cS in stwAwy.room.world.spawners)
            {
                if (cS is World.SimpleSpawner spawner &&
                    spawner.creatureType is not null)
                {
                    bool smallCreature =
                        StaticWorld.GetCreatureTemplate(spawner.creatureType).smallCreature ||
                        spawner.creatureType == CreatureTemplate.Type.Fly ||
                        spawner.creatureType == CreatureTemplate.Type.Leech ||
                        spawner.creatureType == CreatureTemplate.Type.SeaLeech ||
                        spawner.creatureType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
                        spawner.creatureType == CreatureTemplate.Type.Spider;

                    for (int s = 0; s < (smallCreature ? 1 : Mathf.Max(1, spawner.amount / 3)); s++)
                    {
                        regionSpawns.Add(spawner.creatureType);
                    }
                }
                else if (cS is World.Lineage lineage &&
                    lineage.creatureTypes is not null &&
                    stwAwy.room.game.session is not null &&
                    stwAwy.room.game.session is StoryGameSession SGS &&
                    lineage.CurrentType(SGS.saveState) is not null)
                {
                    regionSpawns.Add(lineage.CurrentType(SGS.saveState));
                }
            }
        }

        CreatureTemplate.Type ctrType = null;
        if (regionSpawns is not null && regionSpawns.Count > 0 && stwAwy.AI is not null)
        {
            int smallCreatures = 0;
            if (CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out CWT.AbsCtrInfo aI) && aI.ctrList is not null)
            {
                for (int i = 0; i < aI.ctrList.Count; i++)
                {
                    if (aI.ctrList[i].creatureTemplate.smallCreature ||
                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Fly ||
                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Leech ||
                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.SeaLeech ||
                        aI.ctrList[i].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Spider)
                    {
                        smallCreatures++;
                    }
                }
            }

            for (int i = regionSpawns.Count - 1; i >= 0; i--)
            {
                if (!stwAwy.AI.WantToEat(regionSpawns[i]) || ctrType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                    (smallCreatures > 3 &&
                    (regionSpawns[i] == CreatureTemplate.Type.Fly ||
                    regionSpawns[i] == CreatureTemplate.Type.Leech ||
                    regionSpawns[i] == CreatureTemplate.Type.SeaLeech ||
                    regionSpawns[i] == CreatureTemplate.Type.Spider ||
                    regionSpawns[i] == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)))
                {
                    regionSpawns.RemoveAt(i);
                }
            }

            int ctrNum =
                Random.Range(0, regionSpawns.Count);

            float strength =
                StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).baseDamageResistance +
                StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).baseStunResistance;

            if (regionSpawns[ctrNum] == CreatureTemplate.Type.RedCentipede ||
                regionSpawns[ctrNum] == HSEnums.CreatureType.Cyanwing)
            {
                strength = 10;
            }

            if (strength >= (StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).ancestor == StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate) ? 6 : 8) &&
                Random.value < 0.2f + (strength / 30f))
            {
                ctrNum = (ctrNum + 1 >= regionSpawns.Count) ? 0 : ctrNum + 1;
            }

            ctrType = regionSpawns[ctrNum];
        }
        else
        {
            switch (Random.value)
            {
                case < 0.100f:
                    ctrType =
                        Random.value > 0.5f ?
                        CreatureTemplate.Type.SmallNeedleWorm :
                        CreatureTemplate.Type.BigNeedleWorm;
                    break;
                case < 0.150f:
                    ctrType = CreatureTemplate.Type.EggBug;
                    break;
                case < 0.225f:
                    ctrType = CreatureTemplate.Type.TubeWorm;
                    break;
                case < 0.325f:
                    ctrType = CreatureTemplate.Type.Hazer;
                    break;
                case < 0.400f:
                    ctrType =
                        Random.value > 0.5f ?
                        CreatureTemplate.Type.Centipede :
                        CreatureTemplate.Type.Centiwing;
                    break;
                case < 0.475f:
                    ctrType = CreatureTemplate.Type.JetFish;
                    break;
                case < 0.550f:
                    ctrType = CreatureTemplate.Type.LanternMouse;
                    break;
                case < 0.650f:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            ctrType = CreatureTemplate.Type.Spider;
                            break;
                        case 1:
                            ctrType = CreatureTemplate.Type.BigSpider;
                            break;
                        case 2:
                            ctrType = CreatureTemplate.Type.SpitterSpider;
                            break;
                        case 3:
                            ctrType = MoreSlugcatsEnums.CreatureTemplateType.MotherSpider;
                            break;
                        default:
                            break;
                    }
                    break;
                case < 0.725f:
                    ctrType = CreatureTemplate.Type.Snail;
                    break;
                case < 0.800f:
                    ctrType =
                        Random.value > 0.5f ?
                        CreatureTemplate.Type.CicadaA :
                        CreatureTemplate.Type.CicadaB;
                    break;
                case < 0.850f:
                    ctrType = MoreSlugcatsEnums.CreatureTemplateType.Yeek;
                    break;
                case < 0.925f:
                    ctrType = CreatureTemplate.Type.DropBug;
                    break;
                case < 1f:
                    ctrType = Random.Range(0, 10) switch
                    {
                        0 => Random.value < 0.75f ? CreatureTemplate.Type.PinkLizard :
                                                        Random.value < 0.75f ? MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard :
                                                        CreatureTemplate.Type.RedLizard,
                        1 => CreatureTemplate.Type.GreenLizard,
                        2 => Random.value < 0.75 ? CreatureTemplate.Type.BlueLizard :
                                                        Random.value < 0.75 ? HSEnums.CreatureType.IcyBlueLizard :
                                                        HSEnums.CreatureType.FreezerLizard,
                        3 => CreatureTemplate.Type.Salamander,
                        4 => MoreSlugcatsEnums.CreatureTemplateType.EelLizard,
                        5 => CreatureTemplate.Type.WhiteLizard,
                        6 => CreatureTemplate.Type.YellowLizard,
                        7 => CreatureTemplate.Type.BlackLizard,
                        8 => MoreSlugcatsEnums.CreatureTemplateType.SpitLizard,
                        9 => CreatureTemplate.Type.CyanLizard,
                        _ => null,
                    };
                    break;
                default:
                    ctrType = null;
                    break;
            }
        }

        if (IsIncanStory(stwAwy.room.game) && stwAwy.room.abstractRoom.name == "GW_A08")
        {
            ctrType =
                (Random.value < 0.50f) ? CreatureTemplate.Type.DropBug :
                (Random.value < 0.50f) ? CreatureTemplate.Type.SeaLeech :
                (Random.value < 0.50f) ? CreatureTemplate.Type.Snail :
                (Random.value < 0.50f) ? CreatureTemplate.Type.Salamander : MoreSlugcatsEnums.CreatureTemplateType.EelLizard;
        }

        List<AbstractCreature> newCtrList = new();
        if (ctrType is not null)
        {
            bool altForm = false;
            if (regionSpawns is null && (ctrType == CreatureTemplate.Type.Centipede || ctrType == CreatureTemplate.Type.Centiwing))
            {
                if (Random.value < 0.65f)
                {
                    altForm = ctrType == CreatureTemplate.Type.Centiwing;
                    ctrType = CreatureTemplate.Type.SmallCentipede;
                }
                else if (Random.value < 0.03f)
                {
                    ctrType =
                        ctrType == CreatureTemplate.Type.Centipede ?
                        CreatureTemplate.Type.RedCentipede :
                        HSEnums.CreatureType.Cyanwing;
                }
            }

            int reps =
                ctrType == CreatureTemplate.Type.Fly ? (Random.value < 0.5f ? 3 : 2) :
                ctrType == CreatureTemplate.Type.Hazer ? (Random.value < 0.25f ? 2 : 1) :
                ctrType == CreatureTemplate.Type.LanternMouse ||
                 ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek ? (Random.value < 0.15f ? 2 : 1) :
                ctrType == CreatureTemplate.Type.Leech ||
                ctrType == CreatureTemplate.Type.SeaLeech ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ? Random.Range(3, 6) :
                ctrType == CreatureTemplate.Type.Spider ? Random.Range(4, 8) : 1;

            for (int i = 0; i < reps; i++)
            {
                IntVector2 tilePosition = stwAwy.room.GetTilePosition(stwAwy.DangerPos - new Vector2(0, 25f));
                WorldCoordinate worldCoordinate = stwAwy.room.GetWorldCoordinate(tilePosition);
                EntityID newID = stwAwy.room.game.GetNewID();
                AbstractCreature newCtr = new(stwAwy.room.world, StaticWorld.GetCreatureTemplate(ctrType), null, worldCoordinate, newID);

                if (ctrType == CreatureTemplate.Type.SmallCentipede && altForm)
                {
                    newCtr.superSizeMe = true;
                }

                newCtrList.Add(newCtr);
            }
        }
        return newCtrList;
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Grabby Plants

    public static void ErraticWindPoleHide(On.PoleMimic.orig_Update orig, PoleMimic pol, bool eu)
    {
        float? extended = null;
        if (Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval] && pol?.room?.blizzardGraphics is not null && CWT.AbsCtrData.TryGetValue(pol.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            if (aI.destinationLocked || pol.room.blizzardGraphics.GetBlizzardPixel((int)(pol.DangerPos.x / 20f), (int)(pol.DangerPos.y / 20f)).g > 0)
            {
                extended = pol.extended - (1f / 60f);
            }
            if (!aI.destinationLocked && extended <= 0.5f)
            {
                aI.destinationLocked = true;
            }
        }
        orig(pol, eu);
        if (extended.HasValue && pol.extended == 1)
        {
            pol.extended = extended.Value;
            if (pol.extended < 0)
            {
                pol.enteringShortCut = pol.shortCutPos;
                pol.abstractCreature.remainInDenCounter++;
            }
        }
    }
    public static void AngrierPolePlants(On.PoleMimic.orig_Act orig, PoleMimic pm)
    {
        orig(pm);
        if (IsIncanStory(pm?.room?.game) && pm.wantToWakeUp && pm.wakeUpCounter < 250)
        {
            pm.wakeUpCounter++;
        }
    }
    public static void WinterPolePlantDifferences(On.PoleMimicGraphics.orig_ApplyPalette orig, PoleMimicGraphics pmg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(pmg, sLeaser, rCam, palette);
        if (pmg?.pole?.room is not null && pmg.pole.abstractCreature.Winterized && (IsIncanStory(pmg.pole.room.game) || HSRemix.PolePlantColorsEverywhere.Value is true) && CWT.CreatureData.TryGetValue(pmg.pole, out CWT.CreatureInfo cI))
        {
            Random.State state = Random.state;
            Random.InitState(pmg.pole.abstractCreature.ID.RandomSeed);
            pmg.mimicColor = Color.Lerp(palette.texture.GetPixel(4, 3), palette.fogColor, palette.fogAmount * (4f / 15f));
            pmg.blackColor = new Color(0.15f, 0.15f, 0.15f);
            cI.customFunction = Random.Range(1.75f, 2.5f);
            Random.state = state;
        }
    }
    public static void PolePlantColors(On.PoleMimicGraphics.orig_DrawSprites orig, PoleMimicGraphics pmg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(pmg, sLeaser, rCam, timeStacker, camPos);
        if (pmg?.pole?.room is not null && (IsIncanStory(pmg.pole.room.game) || HSRemix.PolePlantColorsEverywhere.Value is true) && CWT.CreatureData.TryGetValue(pmg.pole, out CWT.CreatureInfo cI))
        {
            Random.State state = Random.state;
            Random.InitState(pmg.pole.abstractCreature.ID.RandomSeed);
            Color col1 =
                    pmg.pole.abstractCreature.Winterized ?
                    Custom.HSL2RGB(Random.Range(190 / 360f, 250 / 360f), 1, Custom.ClampedRandomVariation(0.9f, 0.10f, 0.4f)) : // Winter colors
                    pmg.pole.abstractCreature.world.region is not null &&
                    pmg.pole.abstractCreature.world.region.name == "OE" ?
                    Random.ColorHSV(40 / 360f, 140 / 360f, 1, 1, 0.5f, 0.6f) : // Outer Expanse colors
                    Custom.HSL2RGB(Random.Range(-40 / 360f, 40 / 360f), 1, Custom.ClampedRandomVariation(0.6f, 0.15f, 0.2f)); // Default colors
            Color col2 =
                    pmg.pole.abstractCreature.Winterized ?
                    Custom.HSL2RGB(Random.Range(190 / 360f, 250 / 360f), 1, Custom.ClampedRandomVariation(0.9f, 0.10f, 0.4f)) : // Winter colors
                    pmg.pole.abstractCreature.world.region is not null &&
                    pmg.pole.abstractCreature.world.region.name == "OE" ?
                    Random.ColorHSV(40 / 360f, 140 / 360f, 1, 1, 0.5f, 0.6f) : // Outer Expanse colors
                    Custom.HSL2RGB(Random.Range(-40 / 360f, 40 / 360f), 1, Custom.ClampedRandomVariation(0.6f, 0.15f, 0.2f)); // Default colors

            float gradientSkew = Random.Range(0, 3) switch
            {
                0 => 0.4f,
                2 => 1.6f,
                _ => 1f,
            };
            Random.state = state;

            Color val = Color.Lerp(pmg.blackColor, pmg.mimicColor, Mathf.Lerp(pmg.lastLookLikeAPole, pmg.lookLikeAPole, timeStacker));
            sLeaser.sprites[0].color = val;

            for (int i = 0; i < pmg.leafPairs; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (i >= pmg.decoratedLeafPairs)
                    {
                        return;
                    }

                    Color leafColor = Color.Lerp(col1, col2, Mathf.Pow(Mathf.InverseLerp(0, pmg.leafPairs / 3f, i), gradientSkew));

                    sLeaser.sprites[pmg.LeafDecorationSprite(i, j)].color =
                        Color.Lerp(leafColor, val, Mathf.Pow(Mathf.InverseLerp(pmg.decoratedLeafPairs / 2f, pmg.decoratedLeafPairs, i), 0.6f));

                    if (pmg.pole.abstractCreature.Winterized)
                    {
                        sLeaser.sprites[pmg.LeafDecorationSprite(i, j)].scaleY *= cI.customFunction;
                    }
                }
            }
        }
    }

    public static void ErraticWindKelpHide(On.TentaclePlant.orig_Update orig, TentaclePlant klp, bool eu)
    {
        float? extended = null;
        if (Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval] && klp?.room?.blizzardGraphics is not null && CWT.AbsCtrData.TryGetValue(klp.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            if (aI.destinationLocked || klp.room.blizzardGraphics.GetBlizzardPixel((int)(klp.DangerPos.x / 20f), (int)(klp.DangerPos.y / 20f)).g > 0)
            {
                extended = klp.extended - (1f / 60f);
            }
            if (!aI.destinationLocked && extended <= 0.5f)
            {
                aI.destinationLocked = true;
            }
        }
        orig(klp, eu);
        if (extended.HasValue)
        {
            klp.extended = extended.Value;
            if (klp.extended <= 0)
            {
                klp.enteringShortCut = klp.shortCutPos;
            }
        }
    }
    public static void MonsterKelpColors(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics mkg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(mkg, sLeaser, rCam, palette);
        if (mkg?.plant?.room is not null && (IsIncanStory(mkg.plant.room.game) || HSRemix.MonsterKelpColorsEverywhere.Value is true))
        {
            Random.State state = Random.state;
            Random.InitState(mkg.plant.abstractCreature.ID.RandomSeed);
            Color col1 =
                mkg.plant.abstractCreature.Winterized ?
                Custom.HSL2RGB(Random.Range(200f, 300f) / 360f, Random.Range(0.825f, 1), Custom.WrappedRandomVariation(0.875f, 0.125f, 0.2f)) : // Winter colors
                mkg.plant.abstractCreature.world.region is not null &&
                mkg.plant.abstractCreature.world.region.name == "OE" ?
                Random.ColorHSV(30 / 360f, 170 / 360f, 1, 1, 0.4f, 0.7f) : // Outer Expanse colors
                Random.ColorHSV(-60 / 360f, 20 / 360f, 1, 1, 0.45f, 0.65f); // Default colors

            Color col2 =
                mkg.plant.abstractCreature.Winterized ?
                Custom.HSL2RGB(Random.Range(200f, 300f) / 360f, Random.Range(0.825f, 1), Custom.WrappedRandomVariation(0.875f, 0.125f, 0.2f)) : // Winter colors
                mkg.plant.abstractCreature.world.region is not null &&
                mkg.plant.abstractCreature.world.region.name == "OE" ?
                Random.ColorHSV(30 / 360f, 170 / 360f, 1, 1, 0.4f, 0.7f) : // Outer Expanse colors
                Random.ColorHSV(-60 / 360f, 20 / 360f, 1, 1, 0.45f, 0.65f); // Default colors

            float gradientSkew = Random.Range(0, 3) switch
            {
                0 => 0.4f,
                2 => 1.6f,
                _ => 1f,
            };
            for (int i = 0; i < mkg.danglers.Length; i++)
            {
                Color finalKelpColors = Color.Lerp(col1, col2, Mathf.Pow(mkg.danglerProps[i, 0], gradientSkew));

                sLeaser.sprites[i + 1].color = Color.Lerp(finalKelpColors, sLeaser.sprites[0].color, rCam.room.Darkness(mkg.plant.rootPos));
            }

            Random.state = state;
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Big Jellyfish

    public static void BigJellyCRONCH(On.MoreSlugcats.BigJellyFish.orig_Collide orig, BigJellyFish bigJelly, PhysicalObject physObj, int myChunk, int otherChunk)
    {
        bool cntConsumed = physObj is not null && physObj is Centipede eated && bigJelly.consumedCreatures.Contains(eated);

        orig(bigJelly, physObj, myChunk, otherChunk);

        if (!cntConsumed &&
            physObj is not null &&
            physObj is Chillipede chl &&
            bigJelly.consumedCreatures.Contains(chl))
        {
            int[] shells = new int[chl.bodyChunks.Length];
            for (int s = 0; s < shells.Length; s++)
            {
                shells[s] = s;
            }
            chl.DamageChillipedeShells(shells, 15, bigJelly.bodyChunks[bigJelly.CoreChunk]);
        }

        if (IsIncanStory(bigJelly?.room?.game) &&
            !bigJelly.dead &&
            bigJelly.bodyChunks[myChunk].vel.y >= 5f &&
            physObj is not null &&
            physObj is Creature victim &&
            victim.bodyChunks[otherChunk].contactPoint.y > 0 &&
            CWT.CreatureData.TryGetValue(bigJelly, out CWT.CreatureInfo jI) &&
            CWT.CreatureData.TryGetValue(victim, out CWT.CreatureInfo vI) &&
            (jI.impactCooldown == 0 || vI.impactCooldown == 0))
        {
            jI.impactCooldown = 40;
            vI.impactCooldown = 40;
            victim.Violence(bigJelly.bodyChunks[myChunk], bigJelly.bodyChunks[myChunk].vel, victim.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, 2f, Random.Range(60, 81));
            bigJelly.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, bigJelly.bodyChunks[myChunk], false, 1.4f, 1.1f);
            float volume = ((victim.State is HealthState HS && HS.ClampedHealth == 0) || victim.State.dead) ? 1.66f : 1f;
            bigJelly.room.PlaySound(SoundID.Spear_Stick_In_Creature, bigJelly.bodyChunks[myChunk], false, volume, Random.Range(0.6f, 0.8f));
        }
    }
    public static void BIGJellyfishMold(On.MoreSlugcats.BigJellyFish.orig_Die orig, BigJellyFish bigJelly)
    {
        if (IsIncanStory(bigJelly?.room?.game) && !bigJelly.dead)
        {
            bigJelly.SMSuckCounter = 100;
            while (bigJelly.grabbedBy.Count > 0)
            {
                bigJelly.grabbedBy[0].Release();
            }
            bigJelly.abstractCreature.LoseAllStuckObjects();
            bigJelly.consumedCreatures.Clear();
            BodyChunk[] array = bigJelly.bodyChunks;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].collideWithObjects = false;
            }

            int num = Random.Range(8, 16);
            for (int j = 0; j < num; j++)
            {
                AbstractConsumable jellyWad = new(bigJelly.room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, bigJelly.abstractCreature.pos, bigJelly.room.game.GetNewID(), -1, -1, null)
                {
                    destroyOnAbstraction = true
                };
                bigJelly.room.abstractRoom.AddEntity(jellyWad);
                jellyWad.RealizeInRoom();
                (jellyWad.realizedObject as SlimeMold).JellyfishMode = true;
                jellyWad.realizedObject.firstChunk.pos += Custom.RNV() * Random.value * 85f;
                jellyWad.realizedObject.firstChunk.vel *= 0f;
            }

            num = Random.Range(3, 5);
            for (int k = 0; k < num; k++)
            {
                bool big = false;
                if (k < num && Random.value < 0.5f)
                {
                    k++;
                    big = true;
                }
                AbstractConsumable slimeMold = new AbstractSlimeMold(bigJelly.room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, bigJelly.abstractCreature.pos, bigJelly.room.game.GetNewID(), -1, -1, null, big);
                bigJelly.room.abstractRoom.AddEntity(slimeMold);
                slimeMold.RealizeInRoom();
                slimeMold.realizedObject.firstChunk.pos = bigJelly.bodyChunks[bigJelly.CoreChunk].pos + (Custom.RNV() * Random.value * 15f);
                slimeMold.realizedObject.firstChunk.vel *= 0f;
            }

            if (!bigJelly.dead)
            {
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("Die! " + bigJelly.Template.name);
                }
                if (ModManager.MSC && bigJelly.room is not null && bigJelly.room.world.game.IsArenaSession && bigJelly.room.world.game.GetArenaGameSession.chMeta is not null && (bigJelly.room.world.game.GetArenaGameSession.chMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT || bigJelly.room.world.game.GetArenaGameSession.chMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT))
                {
                    bool flag = false;
                    if (bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature is null or "")
                    {
                        flag = true;
                    }
                    else if (bigJelly.Template.name.ToLower() == bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower() || bigJelly.abstractCreature.creatureTemplate.type.value.ToLower() == bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower())
                    {
                        flag = true;
                    }
                    if (bigJelly.protectDeathRecursionFlag)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        for (int i = 0; i < bigJelly.room.world.game.Players.Count; i++)
                        {
                            if (bigJelly.room.world.game.Players[i].realizedCreature is not null && !bigJelly.room.world.game.Players[i].realizedCreature.dead)
                            {
                                bigJelly.room.world.game.Players[i].realizedCreature.protectDeathRecursionFlag = true;
                                bigJelly.room.world.game.Players[i].realizedCreature.Die();
                            }
                        }
                    }
                }
                if (bigJelly?.killTag?.realizedCreature is not null)
                {
                    Room realizedRoom = bigJelly.room;
                    realizedRoom ??= bigJelly.abstractCreature.Room.realizedRoom;
                    if (realizedRoom?.socialEventRecognizer is not null)
                    {
                        realizedRoom.socialEventRecognizer.Killing(bigJelly.killTag.realizedCreature, bigJelly);
                    }
                    if (bigJelly.abstractCreature.world.game.IsArenaSession && bigJelly.killTag.realizedCreature is Player)
                    {
                        bigJelly.abstractCreature.world.game.GetArenaGameSession.Killing(bigJelly.killTag.realizedCreature as Player, bigJelly);
                    }
                }
                bigJelly.dead = true;
                bigJelly.LoseAllGrasps();
                bigJelly.abstractCreature.Die();
            }
            bigJelly.Destroy();
            bigJelly.abstractCreature.Destroy();
        }
        orig(bigJelly);
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void ErraticWindScavHide(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI scvAI)
    {
        orig(scvAI);
        if (IsIncanStory(scvAI?.scavenger?.room?.game) && Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval])
        {
            if (scvAI.denFinder.GetDenPosition().HasValue && scvAI.creature.abstractAI.destination != scvAI.denFinder.GetDenPosition().Value)
            {
                scvAI.creature.abstractAI.SetDestination(scvAI.denFinder.GetDenPosition().Value);
                Weather.LockDestination(scvAI);
            }
            scvAI.runSpeedGoal = 0.7f;
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region ALEX YEEK??!??!?!?

    public static void YeekColors(On.MoreSlugcats.YeekGraphics.orig_CreateCosmeticAppearance orig, YeekGraphics yGrph)
    {
        orig(yGrph);
        if (IsIncanStory(yGrph?.myYeek?.room?.game))
        {
            AbstractCreature absYeek = yGrph.myYeek.abstractCreature;
            float groupLeaderPotential = yGrph.myYeek.GroupLeaderPotential;

            Random.State state = Random.state;
            Random.InitState(absYeek.ID.RandomSeed);

            HSLColor accColor =
                absYeek.Winterized ?
                new(Random.Range(160 / 360f, 280 / 360f), Random.value < 0.1 ? 0 : Random.Range(0.75f, 1), Random.value < 0.1f ? Random.Range(0.45f, 0.55f) : Random.Range(0.55f, 0.70f)) : // Winter colors
                absYeek.world.region is not null &&
                absYeek.world.region.name == "OE" ?
                new(Custom.WrappedRandomVariation(30 / 360f, 80 / 360f, 0.5f), Random.Range(0.8f, 1), Custom.WrappedRandomVariation(Random.value < 0.15 ? 0.55f : 0.7f, 0.1f, 0.5f)) : // Outer Expanse colors
                new(Random.value, Random.Range(0.85f, 1), Custom.WrappedRandomVariation(0.66f, 0.11f, 0.1f)); // Default colors

            yGrph.tailHighlightColor = Color.HSVToRGB(accColor.hue, accColor.saturation, accColor.lightness);
            yGrph.featherColor = Color.HSVToRGB(Custom.ClampedRandomVariation(accColor.hue, 20 / 360f, 0.2f), accColor.saturation + 0.075f, accColor.lightness - 0.15f);

            if (Random.value < 0.01f)
            {
                (yGrph.featherColor, yGrph.tailHighlightColor) = (yGrph.tailHighlightColor, yGrph.featherColor);
            }

            yGrph.furColor = Color.Lerp(yGrph.featherColor, Color.HSVToRGB(accColor.hue, accColor.saturation - 0.1f, accColor.lightness - 0.3f), 0.33f + (absYeek.personality.energy * 0.25f));

            Color val =
                Color.Lerp(yGrph.featherColor, new Color(0.33f, 0.33f, 0.33f), 0.33f + (absYeek.personality.aggression * 0.25f));
            val =
                (absYeek.personality.nervous <= absYeek.personality.bravery) ?
                Color.Lerp(val, Color.black, absYeek.personality.bravery * 0.5f) :
                Color.Lerp(val, Color.white, absYeek.personality.nervous * 0.5f);

            yGrph.furColor = Color.Lerp(val, yGrph.furColor, absYeek.personality.sympathy);
            yGrph.furColor = Color.Lerp(yGrph.furColor, Color.white, Random.Range(0.6f, 0.75f) + (absYeek.Winterized ? 0.15f : 0));
            yGrph.HeadfurColor = Color.Lerp(yGrph.furColor + new Color(0.1f, 0.1f, 0.1f), yGrph.furColor + new Color(0.3f, 0.15f, 0.15f), absYeek.personality.bravery);
            yGrph.HeadfurColor = Color.Lerp(yGrph.furColor, yGrph.HeadfurColor, absYeek.personality.dominance);
            yGrph.beakColor = Color.Lerp(yGrph.furColor, new Color(0.81f, 0.53f, 0.34f), 0.6f + (absYeek.personality.dominance / 3f));
            yGrph.featherColor = yGrph.tailHighlightColor;
            yGrph.trueEyeColor = yGrph.featherColor;
            yGrph.plumageGraphic = 2;
            while (yGrph.plumageGraphic is 2 or 1)
            {
                yGrph.plumageGraphic = Random.Range(0, 7);
            }
            Random.Range(0.5f, Mathf.Clamp(groupLeaderPotential * 1.5f, 0.6f, 1.2f));
            int num2 = Random.Range(3, 5);
            for (int num3 = num2; num3 > 0; num3--)
            {
                YeekGraphics.YeekFeather yeekFeather = new(yGrph.myYeek.bodyChunks[0].pos, yGrph, num3, num2)
                {
                    featherScaler = 1.2f
                };
                yGrph.bodyFeathers.Add(yeekFeather);
            }

            Random.state = state;
        }
    }
    public static void YeekEyesBecauseTheirColorIsStupidlySemiHardcoded(On.MoreSlugcats.YeekGraphics.orig_DrawSprites orig, YeekGraphics yGrph, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(yGrph, sLeaser, rCam, timeStacker, camPos);
        if (IsIncanStory(yGrph?.myYeek?.room?.game))
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[yGrph.HeadSpritesStart + 2 + j].color = Color.Lerp(yGrph.eyeColor, yGrph.featherColor, yGrph.darkness);
            }
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region General Creature Stuff

    // The following section manages my own version of the IProvideWarmth interface, which I made to be able to customize and edit heat sources more freely.
    #region WarmthSources
    public static List<UpdatableAndDeletable> tempSources;
    public static void BootlegIProvideWarmth(On.Room.orig_ctor orig, Room room, RainWorldGame game, World world, AbstractRoom abstractRoom)
    {
        orig(room, game, world, abstractRoom);

        tempSources = new List<UpdatableAndDeletable>();
    }
    public static void BootlegIPWAdder(On.Room.orig_AddObject orig, Room room, UpdatableAndDeletable obj)
    {
        orig(room, obj);
        if (room.game is not null && obj is not null)
        {
            if (obj is Creature ctr && (
                (ctr is EggBug egg && !egg.dead && egg.FireBug) ||
                (ctr is Player plr && !plr.dead && plr.IsIncan(out IncanInfo Incan)) ||
                (ctr is ColdLizard)
                ))
            {
                tempSources.Add(ctr);
            }
            else if (obj is Weapon wpn && wpn is Spear spr && (spr.bugSpear || (spr.abstractSpear is AbstractBurnSpear absBrnSpr && absBrnSpr.heat > 0)))
            {
                tempSources.Add(wpn);
            }
            else if (obj is PlayerCarryableItem UaD && (UaD is Lantern || UaD is FireEgg))
            {
                tempSources.Add(UaD);
            }
        }
    }
    public static void BootlegIPWRemover(On.Room.orig_CleanOutObjectNotInThisRoom orig, Room room, UpdatableAndDeletable obj)
    {
        orig(room, obj);
        if (obj is not null)
        {
            if (obj is Creature ctr && (
                (ctr is EggBug egg && egg.FireBug) ||
                (ctr is Player plr && plr.IsIncan(out IncanInfo _)) ||
                (ctr is ColdLizard)
                ))
            {
                tempSources.Remove(ctr);
            }
            else if (obj is Weapon wpn && wpn is Spear spr && (spr.bugSpear || spr.abstractSpear is AbstractBurnSpear))
            {
                tempSources.Remove(wpn);
            }
            else if (obj is PlayerCarryableItem UaD && (UaD is Lantern || UaD is FireEgg))
            {
                tempSources.Remove(UaD);
            }
        }
    }
    #endregion

    //----------------------------------------------------------------------------------

    public static void NewDamageTypeResistances(On.CreatureTemplate.orig_ctor_Type_CreatureTemplate_List1_List1_Relationship orig, CreatureTemplate temp, CreatureTemplate.Type type, CreatureTemplate ancestor, List<TileTypeResistance> tileResistances, List<TileConnectionResistance> connectionResistances, CreatureTemplate.Relationship defaultRelationship)
    {
        orig(temp, type, ancestor, tileResistances, connectionResistances, defaultRelationship);
        CustomTemplateInfo.DamageResistances.AddNewDamageResistances(temp, type);
    }

    public static bool CreatureEdibility(On.Player.orig_EatMeatOmnivoreGreenList orig, Player self, Creature ctr)
    {
        return ctr is Cyanwing ||
            ctr is Chillipede ||
            ctr.Template.type == HSEnums.CreatureType.PeachSpider
|| orig(self, ctr);
    }

    public static void HypothermiaSafeAreas(On.AbstractCreature.orig_Update orig, AbstractCreature absCtr, int time)
    {
        orig(absCtr, time);
        if (absCtr.realizedCreature is null &
            absCtr.Hypothermia < 1f &&
            IsIncanStory(absCtr.world.game) &&
            absCtr.world.region is not null &&
            absCtr.world.region.name != "UG" &&
            absCtr.world.region.name != "OE" &&
            absCtr.world.region.name != "CL" &&
            absCtr.world.region.name != "SB")
        {
            absCtr.Hypothermia = absCtr.InDen || absCtr.HypothermiaImmune
                ? Mathf.Lerp(absCtr.Hypothermia, 0f, 0.04f)
                : Mathf.Lerp(absCtr.Hypothermia, 3f, Mathf.InverseLerp(0f, -600f, absCtr.world.rainCycle.AmountLeft));
        }

    }
    public static void CreatureTimersAndMechanics(On.Creature.orig_Update orig, Creature ctr, bool eu)
    {
        orig(ctr, eu);
        if (ctr is null || !CWT.CreatureData.TryGetValue(ctr, out CWT.CreatureInfo cI))
        {
            return;
        }

        if (cI.impactCooldown > 0)
        {
            cI.impactCooldown--; // Used for the IncanCollision method in IncanFeatures.
        }

        if (cI.chillTimer > 0)
        {
            ctr.Hypothermia += 0.05f;
            cI.chillTimer--;
        }
        if (cI.heatTimer > 0)
        {
            ctr.Hypothermia -= 0.05f;
            cI.heatTimer--;
        } // ^ Used when stabbing something with an Ice Crystal or Fire Spear, in the HitSomething method of IceCrystal.cs and in this file's ElementalDamage method, respectively.


        // This code below establishes new heat and cold sources, using the EXTREMELY convoluted stuff I set up with the BootlegIProvideWarmth stuff.
        // If you're a newer coder, uh, don't expect to understand what's going on here at ALL.
        // For less-newer coders:
        // This effectively lets me assign both new AND pre-existing objects as IProvideWarmth, and with MAXIMUM CUSTOMIZABILITY!!!
        if (ctr.room is not null)
        {
            if (CWT.AbsCtrData.TryGetValue(ctr.abstractCreature, out CWT.AbsCtrInfo aI) && aI.debuffs is not null)
            {
                for (int b = aI.debuffs.Count - 1; b >= 0; b--)
                {
                    Debuff debuff = aI.debuffs[b];
                    if (debuff.duration > 0)
                    {
                        debuff.duration--;
                    }
                    else
                    {
                        aI.debuffs[b] = null;
                        aI.debuffs.Remove(aI.debuffs[b]);
                        continue;
                    }
                    debuff.DebuffUpdate(ctr, b == 0);
                    debuff.DebuffVisuals(ctr, eu);
                }
                if (aI.debuffs.Count < 1)
                {
                    aI.debuffs = null;
                }
            }


            if (tempSources.Contains(ctr) && ctr.dead && ((ctr is Player plr && plr.SlugCatClass == HSEnums.Incandescent) || (ctr is EggBug f && f.FireBug)))
            {
                tempSources.Remove(ctr);
            }

            if (IsIncanStory(ctr.room.game) && ctr.room.blizzardGraphics is not null)
            {
                if (!ctr.dead)
                {
                    Color blizzardPixel = ctr.room.blizzardGraphics.GetBlizzardPixel((int)(ctr.mainBodyChunk.pos.x / 20f), (int)(ctr.mainBodyChunk.pos.y / 20f));
                    ctr.HypothermiaExposure = blizzardPixel.g;
                    if (!ctr.abstractCreature.HypothermiaImmune)
                    {
                        float blizzEnd = ctr.room.world.rainCycle.cycleLength + RainWorldGame.BlizzardHardEndTimer(ctr.room.game.IsStorySession);
                        ctr.HypothermiaGain -= Mathf.Lerp(0f, 50f, Mathf.InverseLerp(blizzEnd, blizzEnd * 5f, ctr.room.world.rainCycle.timer));
                    }
                }
                else
                {
                    ctr.HypothermiaExposure = 1f;
                    ctr.HypothermiaGain = Mathf.Lerp(0f, 4E-05f, Mathf.InverseLerp(0.8f, 1f, ctr.room.world.rainCycle.CycleProgression));
                }
            }

            // Allows Freezer Mist to rapidly chill creatures. To find where the value of freezerChill is changed, go to Hailstorm Creatures/Lizards/FreezerSpit and scroll down.
            if (cI.freezerChill > 0 && ctr.Hypothermia > 0.001f)
            {
                if (ctr.abstractCreature.HypothermiaImmune)
                {
                    cI.freezerChill /= 4f;
                }
                if (ctr.room.game.IsArenaSession)
                {
                    cI.freezerChill *= 2f;
                }

                if (!ctr.abstractCreature.HypothermiaImmune)
                {
                    ctr.Hypothermia += cI.freezerChill / 40f;
                }
                if (ctr is not Player && ctr.State is HealthState hs)
                {
                    float hpDrain = cI.freezerChill / ctr.Template.baseDamageResistance / (ctr.abstractCreature.HypothermiaImmune ? 40f : 10f);
                    if (ctr is Centipede cnt &&
                        cnt is not Chillipede)
                    {
                        hpDrain *= Mathf.Lerp(1.3f, 0.075f, Mathf.Pow(cnt.size, 0.5f));
                    }
                    hs.health -= hpDrain;
                }
                else if (ctr is not Player)
                {
                    ctr.Die();
                }
                cI.freezerChill = 0;
            }

            HailstormHeatsourceUpdate(ctr);

        }
    }

    public static bool HypothermiaBodyContactChanges(On.Creature.orig_HypothermiaBodyContactWarmup orig, Creature no, Creature self, Creature other)
    {
        if (self is null ||
            other is null ||
            (!CustomTemplateInfo.IsColdCreature(self.Template.type) && other.Hypothermia >= self.Hypothermia) ||
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
                otherHypo += Custom.LerpMap(self.TotalMass, 1.4f, 1.8f, 0.0018f, 0.0036f);
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
        Debug.Log("hypothermia body contact is workiiiiiiiiing");
        self.Hypothermia += selfHypo;
        other.Hypothermia += otherHypo;

        return true;
    }

    public static void MinorViolenceTweaks(On.Creature.orig_Violence orig, Creature target, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float dmg, float stun)
    {
        if (target is Chillipede chl)
        {
            dmg /= Mathf.Lerp(1.3f, 0.075f, Mathf.Pow(chl.size, 0.5f));
            // Undoes that hard-coded Centipede damage multiplier for Chillipedes.
        }

        if (target?.room is not null)
        {
            if (source?.owner is not null)
            {
                if (target is Player plr && plr.cantBeGrabbedCounter > 0 && (source.owner is MirosBird || (source.owner is Vulture vul && vul.IsMiros)))
                {
                    dmg *= 0; // Now Miros birds can't insta-gib you the millisecond you exit a pipe. (I hope)
                    stun *= 0;
                }

                if (source.owner is Weapon wpn &&
                    wpn.thrownBy is not null &&
                    wpn.thrownBy is Player inc &&
                    inc.IsIncan(out IncanInfo Incan) &&
                    dirAndMomentum.HasValue &&
                    dirAndMomentum.Value.y > Mathf.Abs(dirAndMomentum.Value.x) * 3)
                // If the source of damage is a weapon thrown upwards by the Incandescent...
                {
                    stun += 30; // ...the target gets almost an extra second of stun.
                }
            }
            if (IsIncanStory(target.room.game))
            {
                if (target.abstractCreature.Winterized)
                {
                    if (target is DropBug)
                    {
                        dmg *= 0.5f; // 2x HP
                        if (dmgType == HSEnums.DamageTypes.Cold ||
                            dmgType == HSEnums.DamageTypes.Heat)
                        {
                            dmg *= 0.6f; // Usually weak to temperamental attacks, but this is flipped into a resistance in the Incandescent's time
                        }
                    }
                    else if (target is PoleMimic)
                    {
                        dmg *= 0.75f; // 1.33x HP
                        if (dmgType == HSEnums.DamageTypes.Cold)
                        {
                            dmg *= 2 / 3f; // Usually weak to cold, but this is negated in the Incandescent's time
                        }
                    }
                    else if (target is TentaclePlant)
                    {
                        if (dmgType == HSEnums.DamageTypes.Cold)
                        {
                            dmg *= 2 / 3f; // Usually weak to cold, but this is negated in the Incandescent's time
                        }
                    }
                }
            }
        }
        orig(target, source, dirAndMomentum, hitChunk, hitAppen, dmgType, dmg, stun);
    }
    public static void CreatureDeathChanges(On.Creature.orig_Die orig, Creature ctr)
    {
        if (ctr is not null &&
            !ctr.dead &&
            (ctr is ColdLizard || (ctr is Lizard liz && liz.Template.type == HSEnums.CreatureType.GorditoGreenieLizard)) &&
            (ctr as Lizard).animation != Lizard.Animation.Standard)
        {
            (ctr as Lizard).EnterAnimation(Lizard.Animation.Standard, forceAnimationChange: true);
        }
        orig(ctr);
    }

    public static Player.ObjectGrabability GrababilityChanges(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        return IsIncanStory(self?.abstractCreature.world?.game) && obj is Cicada ccd && ccd.abstractCreature.Winterized && !ccd.Charging && (ccd.cantPickUpCounter == 0 || ccd.cantPickUpPlayer != self)
            ? Player.ObjectGrabability.OneHand
            : obj is Luminescipede lmn && (lmn.dead || (SlugcatStats.SlugcatCanMaul(self.SlugCatClass) && self.dontGrabStuff < 1 && obj != self && !lmn.Consious))
            ? Player.ObjectGrabability.OneHand
            : orig(self, obj);
    }
    public static bool LuminDenUpdate(On.AbstractCreature.orig_WantToStayInDenUntilEndOfCycle orig, AbstractCreature absCtr)
    {
        return (absCtr is null || absCtr.creatureTemplate.type != HSEnums.CreatureType.Luminescipede || Luminescipede.WantToHideInDen(absCtr))
&& orig(absCtr);
    }
    public static void ActivateCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature absCtr)
    {
        orig(absCtr);
        if (absCtr is not null &&
            (IsIncanStory(absCtr.world?.game) || HSEnums.CreatureType.GetAllCreatureTypes().Contains(absCtr.creatureTemplate.type)))
        {
            CustomFlags(absCtr);
        }
    }
    public static void HailstormCreatureFlagChecks()
    {
        IL.AbstractCreature.InDenUpdate += IL =>
        {
            ILCursor c = new(IL);
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            ILLabel? label = IL.DefineLabel();
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((AbstractCreature absCtr, int time) =>
            {
                return HailstormDenUpdate(absCtr, time);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);

            ILCursor c1 = new(IL);
            if (c1.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractWorldEntity>(nameof(AbstractWorldEntity.world)),
                x => x.MatchLdfld<World>(nameof(World.rainCycle)),
                x => x.MatchLdfld<RainCycle>(nameof(RainCycle.maxPreTimer)),
                x => x.MatchLdcI4(0),
                x => x.MatchCgt()))
            {
                c1.Emit(OpCodes.Ldarg_0);
                c1.EmitDelegate((bool flag, AbstractCreature absCtr) => flag && !(IsIncanStory(absCtr?.world.game) && Weather.FogPrecycle));
                // Prevents normal Precycle creatures from spawning if a special precycle type becomes active in the Incandescent's campaign.
            }


            else
            {
                Debug.LogError("[Hailstorm] Hook to IL.AbstractCreature.InDenUpdate ain't workin'.");
            }
        };

        IL.AbstractCreature.WantToStayInDenUntilEndOfCycle += IL =>
        {
            ILCursor c1 = new(IL);
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            ILLabel? label = null;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            if (c1.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.ignoreCycle)),
                x => x.MatchBrtrue(out label)))
            {
                c1.Emit(OpCodes.Ldarg_0);
                c1.EmitDelegate((AbstractCreature absCtr) => !IsIncanStory(absCtr?.world?.game) || !CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI) || !aI.LateBlizzardRoamer);
                c1.Emit(OpCodes.Brfalse, label);
                // Allows late blizzard roamers to stay outside of their dens in the Incandescent's campaign.
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #1 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c2 = new(IL);
            if (c2.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
                x => x.MatchIsinst<HealthState>(),
                x => x.MatchCallvirt<HealthState>("get_health"),
                x => x.MatchLdcR4(0.6f),
                x => x.MatchBgeUn(out label)))
            {
                c2.Emit(OpCodes.Ldarg_0);
                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type != HSEnums.CreatureType.Cyanwing);
                c2.Emit(OpCodes.Brfalse, label);
                // Cyanwings will not go back to their dens if they're low on health.
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #2 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c3 = new(IL);
            if (c3.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.preCycle))))
            {
                c3.Emit(OpCodes.Ldarg_0);
                c3.EmitDelegate((bool flag, AbstractCreature absCtr) => flag || (IsIncanStory(absCtr?.world.game) && CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI) && aI.FogRoamer));
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #3 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c4 = new(IL);
            if (c4.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractWorldEntity>(nameof(AbstractWorldEntity.world)),
                x => x.MatchLdfld<World>(nameof(World.rainCycle)),
                x => x.MatchLdfld<RainCycle>(nameof(RainCycle.maxPreTimer)),
                x => x.MatchLdcI4(0)//,
                //x => x.MatchBgt(out label)
                ))
            {
                c4.Emit(OpCodes.Ldarg_0);
                c4.EmitDelegate((bool flag, AbstractCreature absCtr) => flag || (IsIncanStory(absCtr?.world.game) && ((absCtr.preCycle && Weather.FogPrecycle) || (CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI) && aI.FogRoamer && !Weather.FogPrecycle))));
                //c4.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #4 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c5 = new(IL);
            if (c5.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
                x => x.MatchCallvirt<CreatureState>("get_dead")))
            {
                c5.Emit(OpCodes.Ldarg_0);
                c5.EmitDelegate((bool flag, AbstractCreature absCtr) => flag || (IsIncanStory(absCtr?.world?.game) && AdverseToCurrentCycle(absCtr)));
                // Keeps creatures inside their dens for the cycle if they're not a fan of the current conditions.
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #5 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }
        };

        IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle += IL =>
        {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            ILLabel? label = null;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

            ILCursor c1 = new(IL);
            if (c1.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreatureAI>(nameof(AbstractCreatureAI.parent)),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.ignoreCycle)),
                x => x.MatchBrtrue(out label)))
            {
                c1.Emit(OpCodes.Ldarg_0);
                c1.EmitDelegate((AbstractCreatureAI absCtrAI) => !(IsIncanStory(absCtrAI?.parent?.world?.game) && CWT.AbsCtrData.TryGetValue(absCtrAI.parent, out CWT.AbsCtrInfo aI) && aI.LateBlizzardRoamer));
                c1.Emit(OpCodes.Brtrue, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #1 to IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c2 = new(IL);
            if (c2.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreatureAI>(nameof(AbstractCreatureAI.parent)),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
                x => x.MatchIsinst(nameof(HealthState)),
                x => x.MatchCallvirt<HealthState>("get_health"),
                x => x.MatchLdcR4(0.75f),
                x => x.MatchBgeUn(out label)))
            {
                c2.Emit(OpCodes.Ldarg_0);
                c2.EmitDelegate((AbstractCreatureAI absCtrAI) => absCtrAI.parent.creatureTemplate.type != HSEnums.CreatureType.Cyanwing);
                c2.Emit(OpCodes.Brfalse, label);
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #2 to IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }

            ILCursor c3 = new(IL);
            if (c3.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AbstractCreatureAI>(nameof(AbstractCreatureAI.parent)),
                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
                x => x.MatchCallvirt<CreatureState>("get_dead")))
            {
                c3.Emit(OpCodes.Ldarg_0);
                _ = c3.EmitDelegate((bool flag, AbstractCreatureAI absCtrAI) => flag || (IsIncanStory(absCtrAI?.parent?.world?.game) && AdverseToCurrentCycle(absCtrAI.parent)));
                // Keeps creatures inside their dens for the cycle if they're not a fan of the current conditions.
            }
            else
            {
                Debug.LogError("[Hailstorm] Hook #3 to IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle is absolutely not working.");
            }
        };
    }


    //--------------------------------------

    public static void CustomFlags(AbstractCreature absCtr)
    {
        if (!CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI))
        {
            return;
        }

        if (absCtr.spawnData is not null && absCtr.spawnData[0] == '{')
        {
            if (aI.HailstormAvoider)
            {
                aI.HailstormAvoider = false;
            }

            if (aI.FogRoamer)
            {
                aI.FogRoamer = false;
            }

            if (aI.FogAvoider)
            {
                aI.FogAvoider = false;
            }

            if (aI.ErraticWindRoamer)
            {
                aI.ErraticWindRoamer = false;
            }

            if (aI.ErraticWindAvoider)
            {
                aI.ErraticWindAvoider = false;
            }

            if (aI.LateBlizzardRoamer)
            {
                aI.LateBlizzardRoamer = false;
            }

            string[] array = absCtr.spawnData.Substring(1, absCtr.spawnData.Length - 2).Split(new char[1] { '-' });
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length < 1)
                {
                    continue;
                }

                switch (array[i].Split(new char[1] { ':' })[0])
                {
                    case "Ignorecycle":
                        absCtr.ignoreCycle = true;
                        break;

                    case "Night":
                        absCtr.nightCreature = true;
                        absCtr.ignoreCycle = false;
                        break;
                    case "LateBlizzardRoamer":
                        aI.LateBlizzardRoamer = true;
                        absCtr.nightCreature = true;
                        absCtr.ignoreCycle = false;
                        break;

                    case "PreCycle":
                        absCtr.preCycle = !aI.HailstormAvoider;
                        break;
                    case "HailstormAvoider":
                        aI.HailstormAvoider = true;
                        absCtr.preCycle = false;
                        break;
                    case "FogRoamer":
                        aI.FogRoamer = !aI.FogAvoider;
                        break;
                    case "FogAvoider":
                        aI.FogAvoider = true;
                        aI.FogRoamer = false;
                        break;

                    case "ErraticWindRoamer":
                        aI.ErraticWindRoamer = !aI.ErraticWindAvoider;
                        break;
                    case "ErraticWindAvoider":
                        aI.ErraticWindAvoider = true;
                        aI.ErraticWindRoamer = false;
                        break;

                    case "AlternateForm":
                        absCtr.superSizeMe = true;
                        break;
                    case "Winter":
                        absCtr.Winterized = true;
                        break;
                    case "Seed":
                        absCtr.ID.setAltSeed(int.Parse(array[i].Split(new char[1] { ':' })[1], NumberStyles.Any, CultureInfo.InvariantCulture));
                        absCtr.personality = new AbstractCreature.Personality(absCtr.ID);
                        break;
                    default:
                        break;
                }
            }
            if (aI.FogRoamer && absCtr.Room is not null && absCtr.Room.shelter)
            {
                if (RainWorld.ShowLogs)
                {
                    Debug.Log("[HAILSTORM] " + absCtr + "'s fog-roamer flag disabled, creature started with player in the shelter!");
                }
                aI.FogRoamer = false;
            }
        }

        CreatureTemplate.Type ctrType = absCtr.creatureTemplate.type;

        string regName = absCtr.world.game.IsStorySession ?
            absCtr.world.region.name : "";

        if (!absCtr.creatureTemplate.BlizzardAdapted && !absCtr.Winterized)
        {
            if ((regName != "OE" &&
                    (ctrType == CreatureTemplate.Type.Spider ||
                    ctrType == CreatureTemplate.Type.BigSpider ||
                    ctrType == CreatureTemplate.Type.SpitterSpider ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
                    ctrType == CreatureTemplate.Type.Scavenger ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing ||
                    ctrType == CreatureTemplate.Type.Vulture ||
                    ctrType == CreatureTemplate.Type.KingVulture)) ||
                    (regName != "UG" && regName != "OE" && (
                    ctrType == CreatureTemplate.Type.CicadaA ||
                    ctrType == CreatureTemplate.Type.CicadaB ||
                    ctrType == CreatureTemplate.Type.JetFish ||
                    ctrType == CreatureTemplate.Type.Hazer ||
                    ctrType == CreatureTemplate.Type.PoleMimic ||
                    ctrType == CreatureTemplate.Type.TentaclePlant ||
                    ctrType == CreatureTemplate.Type.DropBug ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                    (absCtr.creatureTemplate.ancestor == StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate) && ctrType != CreatureTemplate.Type.BlueLizard))))
            {
                absCtr.Winterized = true;
            }
        }

        if (regName == "UG" || absCtr.creatureTemplate.BlizzardAdapted || absCtr.Winterized)
        {
            absCtr.HypothermiaImmune = true;
        }

        if (regName == "SI" && ctrType == CreatureTemplate.Type.SmallCentipede)
        {
            absCtr.superSizeMe = true;
        }

        if (!absCtr.nightCreature &&
                (absCtr.creatureTemplate.BlizzardWanderer ||
                regName == "UG" ||
                ctrType == CreatureTemplate.Type.Vulture ||
                ctrType == CreatureTemplate.Type.KingVulture ||
                    (absCtr.Winterized && (
                    ctrType == CreatureTemplate.Type.CicadaA ||
                    ctrType == CreatureTemplate.Type.CicadaB ||
                    ctrType == CreatureTemplate.Type.GreenLizard ||
                    ctrType == CreatureTemplate.Type.WhiteLizard ||
                    ctrType == CreatureTemplate.Type.BlackLizard ||
                    ctrType == CreatureTemplate.Type.RedLizard ||
                    ctrType == CreatureTemplate.Type.Spider ||
                    ctrType == CreatureTemplate.Type.BigSpider ||
                    ctrType == CreatureTemplate.Type.SpitterSpider ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
                    ctrType == CreatureTemplate.Type.DropBug ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek))))
        {
            absCtr.ignoreCycle = true;
        }


        if (!absCtr.preCycle && (
                ctrType == CreatureTemplate.Type.Scavenger ||
                ctrType == CreatureTemplate.Type.CyanLizard ||
                ctrType == CreatureTemplate.Type.BrotherLongLegs ||
                ctrType == CreatureTemplate.Type.DaddyLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing))
        {
            aI.HailstormAvoider = true;
        }

        if (!aI.FogRoamer && (
                ctrType == CreatureTemplate.Type.Centipede ||
                ctrType == CreatureTemplate.Type.SmallCentipede ||
                ctrType == CreatureTemplate.Type.BrotherLongLegs ||
                ctrType == CreatureTemplate.Type.DaddyLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.EelLizard ||
                ctrType == HSEnums.CreatureType.InfantAquapede))
        {
            aI.FogAvoider = true;
        }

        if (!aI.ErraticWindRoamer && Weather.ErraticWindFearers.Contains(ctrType) && !(
            ctrType == CreatureTemplate.Type.TentaclePlant ||
            ctrType == CreatureTemplate.Type.Scavenger ||
            ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite ||
            ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing))
        {
            aI.ErraticWindAvoider = true;
        }

        if (ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
        {
            absCtr.HypothermiaImmune = false;
        }

        if (aI.HailstormAvoider || aI.FogRoamer || aI.FogAvoider || aI.ErraticWindRoamer || aI.ErraticWindAvoider || aI.LateBlizzardRoamer)
        {
            aI.HasHSCustomFlag = true;
        }

    }

    public static bool HailstormDenUpdate(AbstractCreature absCtr, int time)
    {
        if (IsIncanStory(absCtr?.world?.game) &&
            CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI) &&
            (aI.HasHSCustomFlag || aI.destinationLocked || Weather.ErraticWindFearers.Contains(absCtr.creatureTemplate.type)))
        {
            if (aI.destinationLocked)
            {
                if (absCtr.state.dead)
                {
                    aI.destinationLocked = false;
                }
                if (Weather.ErraticWindCycle && Weather.ErraticWindFearers.Contains(absCtr.creatureTemplate.type))
                {
                    if (Weather.ExtremeWindIntervals[Weather.WindInterval] && absCtr.remainInDenCounter < 600)
                    {
                        absCtr.remainInDenCounter = Random.Range(600, 801);
                    }
                    else if (aI.destinationLocked && absCtr.abstractAI is not null)
                    {
                        aI.destinationLocked = false;
                        absCtr.abstractAI.freezeDestination = false;
                    }
                }
            }
            return !(absCtr.remainInDenCounter > -1 &&
                    (!aI.HailstormAvoider || (aI.HailstormAvoider && Weather.FogPrecycle) || (aI.HailstormAvoider && !Weather.FogPrecycle && absCtr.world.rainCycle.preTimer < 1)) &&
                    (!aI.FogRoamer || (aI.FogRoamer && absCtr.world.rainCycle.maxPreTimer > 0 && Weather.FogPrecycle)) &&
                    (!aI.FogAvoider || (aI.FogAvoider && !Weather.FogPrecycle) || (aI.FogAvoider && Weather.FogPrecycle && absCtr.world.rainCycle.preTimer < 1)) &&
                    (!aI.LateBlizzardRoamer || (aI.LateBlizzardRoamer && absCtr.world.rainCycle.timer >= Weather.LateBlizzardTime(absCtr.world))) &&
                    (!aI.ErraticWindRoamer || (aI.ErraticWindRoamer && Weather.ErraticWindCycle)) &&
                    (!aI.ErraticWindAvoider || (aI.ErraticWindAvoider && !Weather.ErraticWindCycle)) &&
                    (!Weather.ErraticWindFearers.Contains(absCtr.creatureTemplate.type) || !Weather.ErraticWindCycle || !(aI.ErraticWindAvoider || Weather.ExtremeWindIntervals[Weather.WindInterval]))
                    ); // this has gotten so hilariously complicated that it's not even funny; even I find this hard to read.
                       // basically, if this returns true, the creature will be stuck in its den until further notice
        }
        return false;
    }

    public static bool AdverseToCurrentCycle(AbstractCreature absCtr)
    {
        return CWT.AbsCtrData.TryGetValue(absCtr, out CWT.AbsCtrInfo aI) && Weather.ErraticWindCycle && aI.ErraticWindAvoider;
    }

    public static void HailstormHeatsourceUpdate(Creature target)
    {
        float HypoRes = 0;
        foreach (UpdatableAndDeletable tempSource in tempSources)
        {
            if (tempSource is not PhysicalObject obj || obj.room != target.room)
            {
                continue;
            }

            float distance = Vector2.Distance(target.firstChunk.pos, obj.firstChunk.pos);
            bool receiverIsIncan = target is Player incCheck && incCheck.SlugCatClass == HSEnums.Incandescent;

            // Lantern warmth changes
            if (tempSource is Lantern lan)
            {
                if (target.Hypothermia < 2f && lan.room == target.room && distance < 350)
                {
                    float heat = Mathf.Lerp(0.0005f, 0, target.HypothermiaExposure);
                    heat *= Mathf.InverseLerp(350, 70, distance);

                    if (target.room.blizzardGraphics is null || target.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard || target.room.world.rainCycle.CycleProgression <= 0f)
                    {
                        target.Hypothermia -= heat; // Enables the warming capabilities of Lanterns at pretty much all times. 
                    }
                    if (target is Player self && self.IsIncan(out IncanInfo Incan))
                    {
                        target.Hypothermia += heat * 0.8f;
                        if (target.HypothermiaExposure < 1)
                        {
                            Incan.soak--;
                        }
                    }
                }
            }
            else if (tempSource is LanternMouse mse)
            {
                if (target.Hypothermia < 2f && mse.room == target.room && distance < 190)
                {
                    float heat = Mathf.Lerp(0.00075f, 0, target.HypothermiaExposure);
                    heat *= Mathf.InverseLerp(190, 38, distance);
                    heat *= Mathf.InverseLerp(0f, 3500f, mse.State.battery);

                    if (target.room.blizzardGraphics is null || target.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard || target.room.world.rainCycle.CycleProgression <= 0f)
                    {
                        target.Hypothermia -= heat; // Enables the warming capabilities of Lanterns at pretty much all times. 
                    }
                    if (target is Player self && self.IsIncan(out IncanInfo Incan))
                    {
                        target.Hypothermia += heat;
                        Incan.soak--;
                    }
                }
            }

            // Heat sources
            if (target.Hypothermia > 0.001f)
            {
                // Incan heat
                if (tempSource is Player plr &&
                    plr.IsIncan(out IncanInfo Incan) &&
                    Incan.Glow is not null)
                {
                    if (target != plr)
                    {
                        distance = Vector2.Distance(target.firstChunk.pos, Incan.Glow.pos);
                    }
                    if (distance < Incan.Glow.rad * 1.5f)
                    {
                        float heatRad = Incan.Glow.rad * 1.5f;
                        float heatStrength = Mathf.InverseLerp(heatRad, heatRad / 5f, distance);
                        float heat = Mathf.Lerp(0, 0.0005f, (heatRad - 30) / 45);

                        if (receiverIsIncan && !(target == plr && Incan.inArena && target.room.blizzardGraphics is not null)) // Weakens incan heat effectiveness for anyone playing Incan.
                        {
                            heat /= 3f;
                        }
                        if (Incan.firefuel > 0)
                        {
                            heat += 0.0001f;
                        }

                        target.Hypothermia -= heat * heatStrength;
                    }
                }

                // Firebug heat
                else if (tempSource is EggBug bug && bug.FireBug && distance < 150)
                {
                    float heat = Mathf.Lerp(0.0009f, 0.0003f, target.HypothermiaExposure);
                    target.Hypothermia -= heat * Mathf.InverseLerp(150, 30, distance);
                }

                // Fire Spear heat
                else if (tempSource is Spear spr)
                {
                    if (spr.bugSpear && distance < 120)
                    {
                        float heatStrength = Mathf.InverseLerp(120, 24, distance);
                        target.HypothermiaExposure = Mathf.Min(target.HypothermiaExposure, Mathf.Lerp(1, 0.7f, distance));
                        target.Hypothermia -= Mathf.Lerp(0.0003f, 0f, target.HypothermiaExposure) * heatStrength;
                    }
                    else if (spr.abstractSpear is AbstractBurnSpear incSpr && incSpr.heat > 0 && incSpr.glow is not null && distance < incSpr.glow.rad)
                    {
                        float heatStrength = Mathf.InverseLerp(incSpr.glow.rad, incSpr.glow.rad / 5f, distance);
                        target.HypothermiaExposure = Mathf.Min(target.HypothermiaExposure, Mathf.Lerp(1, 0.7f, distance));
                        target.Hypothermia -= Mathf.Lerp(receiverIsIncan ? 0.0001f : 0.0003f, 0f, target.HypothermiaExposure) * heatStrength;
                        if (!receiverIsIncan && target.HypothermiaGain * Mathf.Lerp(0, 0.75f, incSpr.heat) > HypoRes)
                        {
                            HypoRes = target.HypothermiaGain * Mathf.Lerp(0, 0.75f, incSpr.heat);
                        }
                    }
                }

                // Fire Egg heat
                else if (tempSource is FireEgg egg && distance < egg.firstChunk.rad * 3)
                {
                    target.Hypothermia -= 0.0007f;
                }
            }

            // Freezer Lizard chill
            if (tempSource is ColdLizard icy)
            {
                target.Hypothermia -=
                    (target.room.game.IsArenaSession ? -0.0048f : -0.0024f) * Mathf.InverseLerp(icy.chillRadius, icy.chillRadius - 120, distance);
            }
        }
        if (HypoRes > 0)
        {
            target.Hypothermia -= HypoRes;
        }

        if (target.Hypothermia < 0f)
        {
            target.Hypothermia = 0f;
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static float OverseerHailstormCreatureInterest(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI ovrAbsAI, AbstractCreature absCtr)
    {
        if (absCtr is null || absCtr.creatureTemplate.smallCreature || !ovrAbsAI.safariOwner || !HSEnums.CreatureType.GetAllCreatureTypes().Contains(absCtr.creatureTemplate.type))
        {
            return orig(ovrAbsAI, absCtr);
        }
        else
        {
            float num;
            CreatureTemplate.Type ctrType = absCtr.creatureTemplate.type;
            if (ctrType == HSEnums.CreatureType.InfantAquapede)
            {
                return orig(ovrAbsAI, absCtr);
            }
            else
            if (ctrType == HSEnums.CreatureType.PeachSpider ||
                ctrType == HSEnums.CreatureType.Chillipede)
            {
                num = 0.2f;
            }
            /*
            else if (ctrType == HailstormEnums.BezanBud)
            {
                num = 0.4f;
            }
            */
            else
            if (ctrType == HSEnums.CreatureType.IcyBlueLizard ||
                ctrType == HSEnums.CreatureType.GorditoGreenieLizard)
            {
                num = 0.6f;
            }
            else
            if (ctrType == HSEnums.CreatureType.FreezerLizard ||
                ctrType == HSEnums.CreatureType.Cyanwing)
            {
                num = 0.75f;
            }
            else if (absCtr.state is not null and GlowSpiderState gs)
            {
                /*if (ctrType == HailstormEnums.Luminescipede)
                {
                    num = gs.dominant ? 0.3f : 0.2f;
                }
                else if (ctrType == HailstormEnums.Strobelegs)
                {
                    num = gs.dominant ? 0.6f : 0.4f;
                }*/
                num = gs.dominant ? 0.3f : 0.2f;
            }
            else
            {
                num = 0.1f;
            }
            if (absCtr.state.dead)
            {
                num /= 10f;
            }
            num *= absCtr.Room.AttractionValueForCreature(ovrAbsAI.parent.creatureTemplate.type);
            return num * Mathf.Lerp(0.5f, 1.5f, ovrAbsAI.world.game.SeededRandom(ovrAbsAI.parent.ID.RandomSeed + absCtr.ID.RandomSeed));
        }
    }

}