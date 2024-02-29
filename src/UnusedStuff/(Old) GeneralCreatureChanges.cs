//namespace Hailstorm;

//internal class GeneralCreatureChanges
//{

//    public static void Hooks()
//    {
//        On.Creature.ctor += CreatureCWT;
//        On.AbstractCreature.ctor += AbsCtrCWT;

//        // Cicada hooks
//        On.Cicada.GenerateIVars += CicadaH_iVars;
//        On.Cicada.ctor += CicadaH_ctor;
//        On.Player.GraphicsModuleUpdated += WinterSquitJumpHeight1;
//        On.Player.MovementUpdate += WinterSquitJumpHeight2;
//        On.Cicada.GrabbedByPlayer += CicadaH_Stamina;
//        On.CicadaGraphics.ApplyPalette += CicadaH_Palette;

//        // Dropwig hooks
//        On.DropBug.ctor += WinterwigSetup;
//        On.DropBugGraphics.ApplyPalette += WinterwigColors;

//        // Vulture hooks
//        //On.Vulture.ctor += chickenexterminator;
//        On.Vulture.Violence += VultureViolence;
//        KingtuskDeflector();
//        On.KingTusks.Tusk.ctor += KingtuskCWT;
//        On.KingTusks.Tusk.Update += KingtuskDeflectMomentum;
//        On.Vulture.JawSlamShut += MirosVultureShortcutProtection;
//        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += PleaseThereAreOTHERCREATURESTOEATBESIDESSLUGCATS;
//        On.Vulture.Update += MirosVultureLaserVariation;
//        On.VultureGraphics.DrawSprites += MirosVultureLaserVariationGraphics;

//        // Stowaway hooks
//        On.MoreSlugcats.StowawayBugAI.ctor += WAKETHEFUCKUP;
//        On.MoreSlugcats.StowawayBug.Update += StowawayUpdate;
//        On.MoreSlugcats.StowawayBug.Eat += StowFoodAway;
//        On.MoreSlugcats.StowawayBug.Die += StowawayProvidesFood;
//        On.MoreSlugcats.StowawayBugState.StartDigestion += EATFASTERDAMNIT;
//        On.MoreSlugcats.StowawayBug.Violence += StowawayViolence;
//        On.Creature.SpearStick += StowawayToughSides;

//        // Grabby Plant hooks
//        On.PoleMimic.Act += AngrierPolePlants;
//        On.PoleMimicGraphics.ApplyPalette += WinterPolePlantDifferences;
//        On.PoleMimicGraphics.DrawSprites += PolePlantColors;

//        On.TentaclePlantGraphics.ApplyPalette += MonsterKelpColors;

//        // Big Jellyfish & Slime Mold
//        On.MoreSlugcats.BigJellyFish.Collide += BigJellyCRONCH;
//        On.MoreSlugcats.BigJellyFish.Die += BIGJellyfishMold;

//        // HOLY SHIT IT'S ALEX YEEK
//        On.MoreSlugcats.YeekGraphics.CreateCosmeticAppearance += YeekColors;
//        On.MoreSlugcats.YeekGraphics.DrawSprites += YeekEyesBecauseTheirColorIsStupidlySemiHardcoded;

//        //-------------------------------------
//        // Hooks for creatures in general
//        On.Room.ctor += BootlegIProvideWarmth;
//        On.Room.AddObject += BootlegIPWAdder;
//        On.Room.CleanOutObjectNotInThisRoom += BootlegIPWRemover;

//        On.CreatureTemplate.ctor_Type_CreatureTemplate_List1_List1_Relationship += ElementalResistances;
//        On.Player.CanEatMeat += CreatureEdibility;
//        On.AbstractCreature.Update += HypothermiaSafeAreas;
//        On.Creature.Update += CreatureTimersAndMechanics;
//        On.Creature.HypothermiaBodyContactWarmup += HypothermiaBodyContactWarmupChanges;
//        On.Creature.Violence += MinorViolenceTweaks;
//        On.Creature.Die += CreatureDeathChanges;
//        On.Player.Grabability += GrababilityChanges;
//        On.AbstractCreature.setCustomFlags += ActivateCustomFlags;
//        HailstormCreatureFlagChecks();

//    }

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    public static bool IsRWGIncan(RainWorldGame RWG)
//    {
//        return (RWG is not null && RWG.IsStorySession && RWG.StoryCharacter == HailstormSlugcats.Incandescent);
//    }

//    //-----------------------------------------

//    public static void CreatureCWT(On.Creature.orig_ctor orig, Creature ctr, AbstractCreature absCtr, World world)
//    {
//        orig(ctr, absCtr, world);
//        CWT.CreatureData.Add(ctr, new CreatureInfo(ctr));
//        if (absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly && ctr.grasps is null)
//        {
//            ctr.grasps = new Creature.Grasp[ctr.Template.grasps];
//        }
//        if (IsRWGIncan(world?.game))
//        {
//            if (absCtr.creatureTemplate.type == CreatureTemplate.Type.LanternMouse || absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
//            {
//                absCtr.state.meatLeft = 1;
//            }
//            if (absCtr.creatureTemplate.type == CreatureTemplate.Type.TubeWorm)
//            {
//                absCtr.state.meatLeft = 0;
//            }
//        }

//        try
//        {
//            if (absCtr.spawnData is not null)
//            {
//                string spawnData = "";
//                for (int s = 0; s < absCtr.spawnData.Length; s++)
//                {
//                    spawnData += absCtr.spawnData[s];
//                }
//                Debug.Log(absCtr.creatureTemplate.name + " spawndata: " + spawnData);
//            }
//            else Debug.Log("nope lol");
//        }
//        catch (Exception e) { Debug.Log(e); }
//    }

//    public static void AbsCtrCWT(On.AbstractCreature.orig_ctor orig, AbstractCreature absCtr, World world, CreatureTemplate temp, Creature realizedCtr, WorldCoordinate pos, EntityID ID)
//    {
//        orig(absCtr, world, temp, realizedCtr, pos, ID);
//        CWT.AbsCtrData.Add(absCtr, new AbsCtrInfo(absCtr));
//        if (absCtr is not null && (
//            IsRWGIncan(absCtr.world?.game) ||
//            absCtr.creatureTemplate.type == HailstormEnums.InfantAquapede ||
//            absCtr.creatureTemplate.type == HailstormEnums.IcyBlue ||
//            absCtr.creatureTemplate.type == HailstormEnums.Freezer ||
//            absCtr.creatureTemplate.type == HailstormEnums.Cyanwing ||
//            absCtr.creatureTemplate.type == HailstormEnums.GorditoGreenie ||
//            absCtr.creatureTemplate.type == HailstormEnums.Luminescipede ||
//            absCtr.creatureTemplate.type == HailstormEnums.Chillipede))
//        {
//            CustomFlags(absCtr);
//        }

//    }

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Cicadas

//    public static void CicadaH_iVars(On.Cicada.orig_GenerateIVars orig, Cicada ccd)
//    {
//        if (IsRWGIncan(ccd?.room?.game) && ccd.abstractCreature.Winterized)
//        {
//            Random.State state = Random.state;
//            Random.InitState(ccd.abstractCreature.ID.RandomSeed);
//            HSLColor color = new(Custom.ClampedRandomVariation(220 / 360f, 65 / 360f, 0.5f), 0.75f, 0.75f);
//            float fatness = Custom.ClampedRandomVariation(0.4f, 0.066f, 0.5f);
//            int bustedWing = -1;
//            if (Random.value < 0.175f)
//            {
//                bustedWing = Random.Range(0, 4);
//            }
//            ccd.iVars = new Cicada.IndividualVariations(fatness, 1f / fatness + Mathf.Lerp(0.2f, 0.4f, Random.value), Random.value, Mathf.Lerp(1.2f, 2f, Random.value), Mathf.Lerp(0.6f, 1.4f, Random.value), Mathf.Lerp(1f, 0.4f, Random.value * Random.value), Custom.ClampedRandomVariation(0.5f, 0.2f, 0.33f) * 1.5f, bustedWing, color);
//            Random.state = state;
//        }
//        orig(ccd);
//    }
//    public static void CicadaH_ctor(On.Cicada.orig_ctor orig, Cicada ccd, AbstractCreature absCtr, World world, bool gender)
//    {
//        orig(ccd, absCtr, world, gender);
//        if (IsRWGIncan(world?.game) && absCtr.Winterized)
//        {
//            absCtr.state.meatLeft = 1;
//            ccd.buoyancy += 0.02f;
//            ccd.bounce += 0.1f;
//            ccd.surfaceFriction -= 0.1f;
//            if (ccd.bodyChunks is not null)
//            {
//                for (int i = 0; i < ccd.bodyChunks.Length; i++)
//                {
//                    if (ccd.bodyChunks[i] is not null)
//                    {
//                        ccd.bodyChunks[i].mass *= 0.5f;
//                        ccd.bodyChunks[i].rad *= 0.5f;
//                    }
//                }
//            }
//            if (ccd.bodyChunkConnections[0] is not null)
//            {
//                ccd.bodyChunkConnections[0].distance *= 0.5f;
//            }
//        }
//    }
//    public static void WinterSquitJumpHeight1(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
//    {
//        orig(self, actuallyViewed, eu);
//        if (IsRWGIncan(self?.room?.game))
//        {
//            int squitCount = 0;
//            for (int i = 0; i < 2; i++)
//            {
//                if (self.grasps[i]?.grabbed is null ||
//                    self.HeavyCarry(self.grasps[i].grabbed) ||
//                    self.grasps[i].grabbed is not Cicada ccd ||
//                    !ccd.abstractCreature.Winterized)
//                {
//                    continue;
//                }

//                Vector2 val2 = Custom.DirVec(self.DangerPos, new Vector2(self.DangerPos.x, self.DangerPos.y - 1));
//                float boostMult = squitCount == 0 ? 1 : 0.33f;
//                boostMult *= Mathf.InverseLerp(25f, 15f, self.eatMeat);
//                float num6 = self.grasps[i].grabbedChunk.mass / (self.mainBodyChunk.mass + self.grasps[i].grabbedChunk.mass);
//                if (self.enteringShortCut.HasValue)
//                {
//                    num6 = 0f;
//                }
//                else if (self.grasps[i].grabbed.TotalMass < self.TotalMass)
//                {
//                    num6 /= 2f;
//                }
//                if (!self.enteringShortCut.HasValue || 1 > boostMult)
//                {
//                    self.mainBodyChunk.pos += val2 * num6 * boostMult;
//                    self.mainBodyChunk.vel += val2 * num6 * boostMult;
//                    self.grasps[i].grabbedChunk.pos -= val2 * boostMult * (1f - num6);
//                    self.grasps[i].grabbedChunk.vel -= val2 * boostMult * (1f - num6);
//                }
//                if (self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam && self.animation != Player.AnimationIndex.BeamTip && self.animation != Player.AnimationIndex.StandOnBeam)
//                {
//                    self.grasps[i].grabbedChunk.vel.y += self.grasps[i].grabbed.gravity * (1f - self.grasps[i].grabbedChunk.submersion) * 1.5f;
//                }

//                squitCount++;

//            }
//        }
//    }
//    public static void WinterSquitJumpHeight2(On.Player.orig_MovementUpdate orig, Player self, bool eu)
//    {
//        orig(self, eu);
//        if (IsRWGIncan(self?.room?.game))
//        {
//            int squitCount = 0;
//            for (int i = 0; i < 2; i++)
//            {
//                if (self.grasps[i]?.grabbed is null ||
//                    self.HeavyCarry(self.grasps[i].grabbed) ||
//                    self.grasps[i].grabbed is not Cicada ccd ||
//                    !ccd.abstractCreature.Winterized)
//                {
//                    continue;
//                }

//                float boostMult = squitCount == 0 ? 1 : 0.33f;

//                if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats hS) &&
//                    (
//                    (self.bodyMode == Player.BodyModeIndex.Default && self.animation == Player.AnimationIndex.None) ||
//                    self.animation == Player.AnimationIndex.Flip ||
//                    self.animation == Player.AnimationIndex.RocketJump ||
//                    hS.longJumping
//                    )
//                    )
//                {
//                    float jumpBoost =
//                        self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.RocketJump ? 0.66f : 1f;

//                    self.bodyChunks[0].vel.y += ccd.LiftPlayerPower * boostMult * jumpBoost;
//                    self.bodyChunks[1].vel.y += ccd.LiftPlayerPower * boostMult / 4f * jumpBoost;
//                    if (self.animation == Player.AnimationIndex.Flip || self.animation == Player.AnimationIndex.RocketJump)
//                    {
//                        self.bodyChunks[0].vel.x += ccd.LiftPlayerPower * 0.08f * self.flipDirection;
//                        self.bodyChunks[1].vel.x += ccd.LiftPlayerPower * 0.08f * self.flipDirection;
//                    }
//                    ccd.currentlyLiftingPlayer = true;
//                    if (ccd.LiftPlayerPower > 2f / 3f)
//                    {
//                        self.standing = false;
//                    }
//                }
//                else
//                {
//                    self.mainBodyChunk.vel.y += ccd.LiftPlayerPower * boostMult / 2f;
//                    ccd.currentlyLiftingPlayer = false;
//                }
//                if (self.bodyChunks[1].ContactPoint.y < 0 && self.bodyChunks[1].lastContactPoint.y == 0 && ccd.LiftPlayerPower > 1f / 3f)
//                {
//                    self.standing = true;
//                }

//                squitCount++;
//            }
//        }
//    }
//    public static void CicadaH_Stamina(On.Cicada.orig_GrabbedByPlayer orig, Cicada ccd)
//    {
//        if (IsRWGIncan(ccd?.room?.game) && ccd.abstractCreature.Winterized && ccd.currentlyLiftingPlayer)
//        {
//            ccd.stamina += (1f / (ccd.gender ? 160f : 150f));
//            orig(ccd);
//            ccd.flying = ccd.stamina > 1f / 3f;
//        }
//        else
//        {
//            orig(ccd);
//        }
//    }
//    public static void CicadaH_Palette(On.CicadaGraphics.orig_ApplyPalette orig, CicadaGraphics cGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
//    {
//        orig(cGraphics, sLeaser, rCam, palette);
//        if (IsRWGIncan(cGraphics?.cicada?.room?.game) && cGraphics.cicada.abstractCreature.Winterized)
//        {
//            bool whiteCicada = cGraphics.cicada.gender;

//            Color val = Color.Lerp(HSLColor.Lerp(cGraphics.iVars.color, new HSLColor(cGraphics.iVars.color.hue, 0, 0.4f), 0.9f).rgb, (whiteCicada ? palette.fogColor : palette.blackColor), 0.2f);

//            cGraphics.shieldColor =
//                    Color.Lerp(val, new HSLColor(cGraphics.iVars.color.hue, 0.875f, 0.5f).rgb, 0.85f);

//            sLeaser.sprites[cGraphics.BodySprite].color = val;
//            sLeaser.sprites[cGraphics.HeadSprite].color = val;
//            sLeaser.sprites[cGraphics.HighlightSprite].color = Color.Lerp(val, Color.white, whiteCicada ? 0.4f : 0.25f);
//            sLeaser.sprites[cGraphics.ShieldSprite].color = cGraphics.shieldColor;
//            for (int i = 0; i < 2; i++)
//            {
//                for (int j = 0; j < 2; j++)
//                {
//                    sLeaser.sprites[cGraphics.WingSprite(i, j)].color = Color.Lerp(Color.Lerp(val, cGraphics.shieldColor, 0.3f), (whiteCicada ? Color.white : palette.blackColor), 0.3f);
//                    sLeaser.sprites[cGraphics.TentacleSprite(i, j)].color = val;
//                }
//            }
//            if (whiteCicada)
//            {
//                cGraphics.eyeColor = Color.Lerp(val, palette.blackColor, 0.8f);
//                sLeaser.sprites[cGraphics.EyesASprite].color = cGraphics.eyeColor;
//                sLeaser.sprites[cGraphics.EyesBSprite].color = Color.Lerp(cGraphics.iVars.color.rgb, Color.gray, 0.3f);
//            }
//            else
//            {
//                cGraphics.eyeColor = Color.Lerp(cGraphics.iVars.color.rgb, Color.gray, 0.3f);
//                sLeaser.sprites[cGraphics.EyesASprite].color = cGraphics.eyeColor;
//                sLeaser.sprites[cGraphics.EyesBSprite].color = palette.blackColor;
//            }
//        }
//    }


//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Dropwigs

//    public static void WinterwigSetup(On.DropBug.orig_ctor orig, DropBug wig, AbstractCreature absWig, World world)
//    {
//        orig(wig, absWig, world);
//        if (IsRWGIncan(world?.game) && wig.abstractCreature.Winterized)
//        {
//            if (wig.bodyChunks is not null)
//            {
//                for (int i = 0; i < wig.bodyChunks.Length; i++)
//                {
//                    if (wig.bodyChunks[i] is null) continue;

//                    wig.bodyChunks[i].mass *= 1.5f;
//                    wig.bodyChunks[i].rad *= 1.5f;
//                }
//            }
//        }
//    }
//    public static void WinterwigColors(On.DropBugGraphics.orig_ApplyPalette orig, DropBugGraphics DBG, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
//    {
//        orig(DBG, sLeaser, rCam, palette);
//        if (IsRWGIncan(DBG?.bug?.room?.game) && DBG.bug.abstractCreature.Winterized)
//        {
//            DBG.blackColor = Custom.HSL2RGB(DBG.hue, 0.033f, 0.3f);
//            DBG.shineColor = palette.blackColor;
//            DBG.camoColor = Custom.HSL2RGB(DBG.hue, 0, 0.4f);
//            DBG.RefreshColor(0f, sLeaser);
//        }
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Vultures
//    public static void chickenexterminator(On.Vulture.orig_ctor orig, Vulture vulture, AbstractCreature absCtr, World world)
//    {
//        orig(vulture, absCtr, world);
//        if (IsRWGIncan(world.game) && vulture is not null)
//        {
//            vulture.Destroy();
//        }
//    } // This method helps me preserve my sanity while testing features.

//    public static void VultureViolence(On.Vulture.orig_Violence orig, Vulture vul, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos appPos, Creature.DamageType dmgType, float dmg, float stun)
//    {
//        bool hitMask =
//            vul?.room is not null &&
//            !vul.dead &&
//            hitChunk is not null &&
//            hitChunk.index == 4 &&
//            (vul.State as Vulture.VultureState).mask;

//        if (source?.owner is not null && source.owner is IceCrystal && hitMask && dirAndMomentum.HasValue)
//        {
//            vul.DropMask(dirAndMomentum.Value);
//        }

//        orig(vul, source, dirAndMomentum, hitChunk, appPos, dmgType, dmg, stun);

//        if (vul?.room is not null && vul.IsMiros && (HSRemix.ScissorhawkNoNormalLasers.Value is true || Random.value < Mathf.Max(0.5f, dmg * 0.3f)) && (IsRWGIncan(vul?.room?.game) || HSRemix.HailstormScissorhawksEverywhere.Value is true) && vul.laserCounter == 200 && CWT.CreatureData.TryGetValue(vul, out CreatureInfo cI) && cI.customFunction <= 0)
//        {
//            cI.customFunction =
//                HSRemix.ScissorhawkNoNormalLasers.Value ?
//                (Random.value * 1.15f) + (dmg * 0.33f) :
//                Random.value + (dmg * 0.15f);
//            if (cI.customFunction > 1)
//            {
//                vul.laserCounter += 120;
//            }
//            else
//            {
//                vul.laserCounter +=
//                    (cI.customFunction > 0.5f) ? -50 : 70;
//            }
//        }
//    }

//    //---------------------------------------
//    // King-specific
//    public static ConditionalWeakTable<KingTusks.Tusk, TuskInfo> TuskData = new();
//    public static void KingtuskDeflector()
//    {
//        IL.KingTusks.Tusk.ShootUpdate += IL =>
//        {
//            ILCursor c = new(IL);
//            ILLabel? label = IL.DefineLabel();
//            c.Emit(OpCodes.Ldarg_0);
//            c.Emit(OpCodes.Ldarg_1);
//            c.EmitDelegate((KingTusks.Tusk tusk, float speed) =>
//            {
//                return KingtuskTargetIsArmored(tusk, speed);
//            });
//            c.Emit(OpCodes.Brfalse_S, label);
//            c.Emit(OpCodes.Ret);
//            c.MarkLabel(label);
//        };
//    }
//    public static void KingtuskCWT(On.KingTusks.Tusk.orig_ctor orig, KingTusks.Tusk tusk, KingTusks owner, int side)
//    {
//        orig(tusk, owner, side);
//        TuskData.Add(tusk, new TuskInfo(tusk));
//    }
//    public static void KingtuskDeflectMomentum(On.KingTusks.Tusk.orig_Update orig, KingTusks.Tusk tusk)
//    {
//        if (TuskData.TryGetValue(tusk, out TuskInfo tI))
//        {
//            if (tusk.mode == KingTusks.Tusk.Mode.ShootingOut && tI.bounceOffSpeed != new Vector2(0, 0))
//            {
//                tI.bounceOffSpeed = new Vector2(0, 0);
//            }

//            orig(tusk);

//            if (tusk.mode == KingTusks.Tusk.Mode.Dangling && tI.bounceOffSpeed != new Vector2(0, 0))
//            {
//                for (int j = 0; j < tusk.chunkPoints.GetLength(0); j++)
//                {
//                    tusk.chunkPoints[j, 2] -= tI.bounceOffSpeed;
//                    SharedPhysics.TerrainCollisionData cd = tusk.scratchTerrainCollisionData.Set(tusk.chunkPoints[j, 0], tusk.chunkPoints[j, 1], tusk.chunkPoints[j, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
//                    cd = SharedPhysics.VerticalCollision(tusk.room, cd);
//                    cd = SharedPhysics.HorizontalCollision(tusk.room, cd);
//                    tusk.chunkPoints[j, 0] = cd.pos;
//                    tusk.chunkPoints[j, 2] = cd.vel;
//                    tI.bounceOffSpeed = Vector2.Lerp(tI.bounceOffSpeed, new Vector2(0, 0), 0.066f);
//                    if (cd.contactPoint.x != 0f)
//                    {
//                        tI.bounceOffSpeed.x *= 0.5f;
//                    }
//                    if (cd.contactPoint.y != 0f)
//                    {
//                        tI.bounceOffSpeed.y *= 0.5f;
//                    }
//                }
//            }
//        }
//        else orig(tusk);
//    }

//    public static bool KingtuskTargetIsArmored(KingTusks.Tusk tusk, float speed)
//    {
//        Vector2 val = tusk.chunkPoints[0, 0] + tusk.shootDir * 20f;
//        Vector2 pos = tusk.chunkPoints[0, 0] + tusk.shootDir * (20f + speed);
//        FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(tusk.room, val, pos);
//        SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(tusk, tusk.room, val, ref pos, 5f, 1, tusk.owner.vulture, hitAppendages: false);
//        if (TuskData.TryGetValue(tusk, out TuskInfo tI) && !floatRect.HasValue && collisionResult.chunk?.owner is not null && collisionResult.chunk.owner is Lizard liz && CWT.AbsCtrData.TryGetValue(liz.abstractCreature, out AbsCtrInfo aBI) && aBI.isFreezerOrIcyBlue && aBI.functionTimer < 2)
//        {
//            int stun = (liz.Template.type == HailstormEnums.Freezer) ? 0 : 30;
//            tusk.mode = KingTusks.Tusk.Mode.Dangling;
//            tI.bounceOffSpeed = tusk.shootDir * speed * 0.5f;
//            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, liz.bodyChunks[1].pos, 1.5f, 0.75f);
//            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, liz.bodyChunks[1].pos, 1.5f, 0.75f);
//            liz.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, liz.bodyChunks[1].pos, 1.5f, 0.75f);
//            liz.Violence(tusk.head, tusk.shootDir, liz.bodyChunks[1], null, Creature.DamageType.Stab, 2, stun);
//            return true;
//        }
//        return false;
//    }


//    //---------------------------------------
//    // Miros-specific
//    public static void MirosVultureShortcutProtection(On.Vulture.orig_JawSlamShut orig, Vulture vul)
//    {
//        if (vul?.room?.abstractRoom?.creatures is not null && vul.bodyChunks is not null && (IsRWGIncan(vul.room.game) || HSRemix.HailstormScissorhawksEverywhere.Value is true))
//        {
//            for (int i = 0; i < vul.room.abstractRoom.creatures.Count; i++)
//            {
//                if (vul.grasps[0] is not null)
//                {
//                    break;
//                }
//                Creature ctr = vul.room.abstractRoom.creatures[i].realizedCreature;
//                if (vul.room.abstractRoom.creatures[i] == vul.abstractCreature || !vul.AI.DoIWantToBiteCreature(vul.room.abstractRoom.creatures[i]) || ctr.enteringShortCut.HasValue || ctr.inShortcut || ctr is null || ctr is not Player plr || plr.cantBeGrabbedCounter <= 0)
//                {
//                    continue;
//                }
//                for (int j = 0; j < ctr.bodyChunks.Length; j++)
//                {
//                    if (vul.grasps[0] is not null)
//                    {
//                        break;
//                    }
//                    Vector2 val = Custom.DirVec(vul.neck.Tip.pos, vul.Head().pos);
//                    if (!Custom.DistLess(vul.Head().pos + val * 20f, ctr.bodyChunks[j].pos, 20f + ctr.bodyChunks[j].rad) || !vul.room.VisualContact(vul.Head().pos, ctr.bodyChunks[j].pos))
//                    {
//                        continue;
//                    }
//                    if (ctr is not null)
//                    {
//                        vul.Grab(ctr, 0, j, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1, overrideEquallyDominant: true, pacifying: false);
//                    }
//                    break;
//                }
//            }
//        }
//        orig(vul);
//    }
//    public static CreatureTemplate.Relationship PleaseThereAreOTHERCREATURESTOEATBESIDESSLUGCATS(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI vultureAI, RelationshipTracker.DynamicRelationship dynamRelat)
//    {
//        // Causes Miros Vultures to attack and eat ANYTHING that isn't flagged as inedible to them
//        if (vultureAI.vulture.IsMiros && (IsRWGIncan(vultureAI.vulture.room.game) || HSRemix.HailstormScissorhawksEverywhere.Value is true))
//        {
//            CreatureTemplate.Type ctrType = dynamRelat.trackerRep.representedCreature.creatureTemplate.type;

//            bool inedibleForMirosVultures =
//                ctrType == CreatureTemplate.Type.Leech ||
//                ctrType == CreatureTemplate.Type.SeaLeech ||
//                ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
//                ctrType == CreatureTemplate.Type.Vulture ||
//                ctrType == CreatureTemplate.Type.KingVulture ||
//                ctrType == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture ||
//                ctrType == CreatureTemplate.Type.Deer ||
//                ctrType == CreatureTemplate.Type.GarbageWorm ||
//                ctrType == CreatureTemplate.Type.Fly ||
//                ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug;

//            bool dangerousToMirosVultures =
//                ctrType == CreatureTemplate.Type.BigEel ||
//                ctrType == CreatureTemplate.Type.DaddyLongLegs ||
//                ctrType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs;


//            if (vultureAI?.vulture?.killTag is not null && vultureAI.vulture.killTag == dynamRelat.trackerRep.representedCreature && !dynamRelat.trackerRep.representedCreature.state.dead && !dangerousToMirosVultures && !inedibleForMirosVultures)
//            {
//                return new CreatureTemplate.Relationship
//                    (CreatureTemplate.Relationship.Type.Attacks, 1.33f);
//            }
//            if (ctrType == CreatureTemplate.Type.PoleMimic ||
//                ctrType == CreatureTemplate.Type.TentaclePlant)
//            {
//                return new CreatureTemplate.Relationship
//                    (CreatureTemplate.Relationship.Type.Attacks, 0.5f);
//            }
//            if (ctrType == CreatureTemplate.Type.BrotherLongLegs)
//            {
//                return new CreatureTemplate.Relationship
//                    (CreatureTemplate.Relationship.Type.Ignores, 1);
//            }
//            if (dangerousToMirosVultures)
//            {
//                return new CreatureTemplate.Relationship
//                    (CreatureTemplate.Relationship.Type.Afraid, (ctrType == CreatureTemplate.Type.BigEel) ? 0.33f : 0.5f);
//            }
//            if (!inedibleForMirosVultures)
//            {
//                return new CreatureTemplate.Relationship
//                    (CreatureTemplate.Relationship.Type.Eats, dynamRelat.trackerRep.representedCreature.state.dead ? 1 : 0.9f);
//            }
//        }
//        return orig(vultureAI, dynamRelat);
//    }
//    public static void MirosVultureLaserVariation(On.Vulture.orig_Update orig, Vulture vul, bool eu)
//    {
//        // Gives Miros Vultures new laser speeds, and then one rare laser type because I'm a fucking lunatic I guess.
//        // I thought this would make combat with them more unpredictable and interesting, but I got a bit carried away.

//        orig(vul, eu);
//        if (vul?.room is not null && vul.IsMiros && (IsRWGIncan(vul.room.game) || HSRemix.HailstormScissorhawksEverywhere.Value is true) && CWT.CreatureData.TryGetValue(vul, out CreatureInfo cI))
//        {
//            if (vul.graphicsModule is null || vul.graphicsModule is not VultureGraphics vg)
//            {
//                return;
//            }

//            if (vul.laserCounter <= 0)
//            {
//                cI.customFunction = 0;
//            }
//            else if (cI.customFunction > 0)
//            {
//                // superDuperHyperLaserThatIsDefinitelyExtremelyUnnecessaryButImAlreadyTooDeepIntoThisToBackUpNow
//                bool auroraLaser = cI.customFunction > 1;
//                if (vg.soundLoop is not null && !auroraLaser)
//                {
//                    vg.soundLoop.Pitch *= cI.customFunction > 0.5f ? 1.5f : 0.5f;
//                    vg.soundLoop.Volume *= 1.25f;
//                }
//                if (!vul.dead && vul.MostlyConsious && vul.LaserLight is not null && !vg.shadowMode)
//                {
//                    Vector2 pos = vul.Head().pos;
//                    Vector2 val = Custom.DirVec(vul.neck.Tip.pos, pos);
//                    val *= -1f;
//                    Vector2 corner = Custom.RectCollision(pos, pos - val * 100000f, vul.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
//                    IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(vul.room, pos, corner);
//                    if (intVector.HasValue)
//                    {
//                        Color hyperColor =
//                            (vul.laserCounter % 100f > 50) ?
//                            Color.Lerp(Custom.HSL2RGB(220 / 360f, 0.4f, 0.5f), Custom.HSL2RGB(300 / 360f, 0.4f, 0.5f), Mathf.InverseLerp(100f, 50f, vul.laserCounter % 100f)) :
//                            Color.Lerp(Custom.HSL2RGB(300 / 360f, 0.4f, 0.5f), Custom.HSL2RGB(220 / 360f, 0.4f, 0.5f), Mathf.InverseLerp(50f, 0f, vul.laserCounter % 100f));

//                        vul.LaserLight.color =
//                            auroraLaser ? hyperColor : new Color(Mathf.InverseLerp(200, 0, vul.laserCounter), Mathf.InverseLerp(200, 0, vul.laserCounter), 0.1f);
//                    }
//                }
//                if (!vul.dead && (vul.laserCounter == 11 || (auroraLaser && (vul.laserCounter == 121 || vul.laserCounter == 66))))
//                {
//                    MixupLaserExplosions(vul, cI.customFunction);
//                    if (vul.LaserLight is not null && vul.laserCounter == 11)
//                    {
//                        vul.LaserLight.Destroy();
//                    }
//                    vul.laserCounter--;
//                }
//            }

//            if (HSRemix.ScissorhawkEagerBirds.Value is true && vul.laserCounter == 0 && vul.landingBrake == 1)
//            {
//                vul.laserCounter = 240;
//                if (HSRemix.ScissorhawkNoNormalLasers.Value is true || Random.value < 0.5f)
//                {
//                    cI.customFunction =
//                        Random.value * (HSRemix.ScissorhawkNoNormalLasers.Value is true ? 1.33f : 1.11f);

//                    vul.laserCounter +=
//                            cI.customFunction > 1.0f ? 120 :
//                            cI.customFunction > 0.5f ? -50 : 70;
//                }
//            }
//        }
//    }
//    public static void MirosVultureLaserVariationGraphics(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
//    {
//        orig(vg, sLeaser, rCam, timeStacker, camPos);
//        if (vg?.vulture?.room is not null &&
//            vg.vulture.IsMiros &&
//            !vg.culled &&
//            (IsRWGIncan(vg.vulture.room.game) || HSRemix.HailstormScissorhawksEverywhere.Value is true) &&
//            sLeaser.sprites[vg.LaserSprite()].isVisible &&
//            CWT.CreatureData.TryGetValue(vg.vulture, out CreatureInfo cI) &&
//            cI.customFunction > 0)
//        {
//            bool hyperLaser = cI.customFunction > 1;

//            float num = Mathf.InverseLerp(0.5f, 1, Mathf.Lerp(vg.lastLaserActive, vg.laserActive, timeStacker));
//            Color laserColor =
//                (hyperLaser && vg.vulture.LaserLight != null) ? vg.vulture.LaserLight.color :
//                (cI.customFunction > 0.5f) ? Color.Lerp(Custom.HSL2RGB(-40 / 360f, 1, 0.5f), Custom.HSL2RGB(20 / 360f, 1, 0.5f), timeStacker) : Custom.HSL2RGB(0.5f, 1, 0.5f);

//            (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).verticeColors[0] = Custom.RGB2RGBA(laserColor, cI.customFunction > 0.5f ? 1 : num);
//            (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).verticeColors[1] = Custom.RGB2RGBA(laserColor, cI.customFunction > 0.5f ? 1 : num);
//            (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).verticeColors[2] = Custom.RGB2RGBA(laserColor, cI.customFunction > 0.5f ? 1 : Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, Mathf.Lerp(vg.lastFlash, vg.flash, timeStacker)));
//            (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).verticeColors[3] = Custom.RGB2RGBA(laserColor, cI.customFunction > 0.5f ? 1 : Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, Mathf.Lerp(vg.lastFlash, vg.flash, timeStacker)));
//        }
//    }

//    private static void MixupLaserExplosions(Vulture vul, float laserType)
//    {
//        if (vul?.room is null)
//        {
//            return;
//        }
//        Vector2 pos = vul.Head().pos;
//        Vector2 val = Custom.DirVec(vul.neck.Tip.pos, pos);
//        val *= -1f;
//        Vector2 corner = Custom.RectCollision(pos, pos - val * 100000f, vul.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
//        IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(vul.room, pos, corner);
//        if (!intVector.HasValue)
//        {
//            return;
//        }

//        bool smallBoom = laserType > 0.5f;

//        Color val2 = Color.Lerp(new Color(0.7f, 0.7f, 0.7f), (smallBoom ? Color.red : Custom.HSL2RGB(0.5f, 1, 0.5f)), 0.2f);

//        corner = Custom.RectCollision(corner, pos, vul.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);

//        float boomRadius =
//            smallBoom ? 175f : 325f;
//        float boomForce =
//            smallBoom ? 4.8f : 7.6f;
//        float boomDMG =
//            laserType > 1f ? 0.75f :
//            smallBoom ? 1.25f : 3f;
//        float boomStun =
//            smallBoom ? 200f : 320f;
//        vul.room.AddObject(new Explosion(vul.room, vul, corner, 7, boomRadius, boomForce, boomDMG, boomStun, 0.1f, vul, 0.3f, boomStun * 0.6f, 1f));
//        vul.room.AddObject(new Explosion.ExplosionLight(corner, boomRadius * 1.12f, 1f, 7, val2));
//        vul.room.AddObject(new Explosion.ExplosionLight(corner, boomRadius * 1.08f, 1f, 3, new Color(1f, 1f, 1f)));
//        vul.room.AddObject(new ShockWave(corner, 330f, 0.045f, 5));
//        for (int i = 0; i < 25; i++)
//        {
//            Vector2 val3 = Custom.RNV();
//            if (vul.room.GetTile(corner + val3 * 20f).Solid)
//            {
//                val3 = (vul.room.GetTile(corner - val3 * 20f).Solid ? Custom.RNV() : (val3 * -1f));
//            }
//            for (int j = 0; j < 3; j++)
//            {
//                vul.room.AddObject(new Spark(corner + val3 * Mathf.Lerp(30f, 60f, Random.value), val3 * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(val2, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
//            }
//            vul.room.AddObject(new Explosion.FlashingSmoke(corner + val3 * 40f * Random.value, val3 * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), val2, Random.Range(3, 11)));
//        }
//        for (int k = 0; k < 6; k++)
//        {
//            vul.room.AddObject(new ScavengerBomb.BombFragment(corner, Custom.DegToVec((k + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
//        }
//        vul.room.ScreenMovement(corner, default, smallBoom ? 0.7f : 1);
//        for (int l = 0; l < vul.abstractPhysicalObject.stuckObjects.Count; l++)
//        {
//            vul.abstractPhysicalObject.stuckObjects[l].Deactivate();
//        }
//        vul.room.PlaySound(SoundID.Bomb_Explode, corner);
//        vul.room.InGameNoise(new Noise.InGameNoise(corner, smallBoom ? 6500f : 11000f, vul, 1));
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    public static void GrappleWormsBreatheGooder(On.TubeWorm.orig_Update orig, TubeWorm worm, bool eu)
//    {
//        orig(worm, eu);
//        if (IsRWGIncan(worm?.room?.game) && !worm.dead && worm.mainBodyChunk.submersion > 0.5f)
//        {
//            worm.lungs += Mathf.Min(worm.lungs + (0.85f/160f), 1f);
//        }
//    }

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Stowaways

//    public static void WAKETHEFUCKUP(On.MoreSlugcats.StowawayBugAI.orig_ctor orig, StowawayBugAI stwAwyAI, AbstractCreature absCtr, World world)
//    {
//        orig(stwAwyAI, absCtr, world);
//        if (stwAwyAI is not null && (IsRWGIncan(world?.game) || HSRemix.HailstormStowawaysEverywhere.Value is true))
//        {
//            if (!stwAwyAI.activeThisCycle)
//            {
//                stwAwyAI.activeThisCycle = true;
//                stwAwyAI.behavior = StowawayBugAI.Behavior.Idle;
//                Debug.Log("[Hailstorm] A sleeping Stowaway got WOKED!");
//            }
//        }
//    }
//    public static void StowawayUpdate(On.MoreSlugcats.StowawayBug.orig_Update orig, StowawayBug stwAwy, bool eu)
//    {
//        orig(stwAwy, eu);
//        if (stwAwy is not null && (IsRWGIncan(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out AbsCtrInfo aI) && aI.ctrList is not null)
//        {
//            if (aI.ctrList.Count > 4)
//            {
//                aI.ctrList.RemoveAt(0);
//            }
//            if (stwAwy.State.dead && CWT.CreatureData.TryGetValue(stwAwy, out CreatureInfo cI))
//            {
//                if (aI.functionTimer == -1 && stwAwy.State is StowawayBugState SBS && SBS.digestionLength > 0 && aI.ctrList.Count > 0)
//                {
//                    aI.functionTimer = 0;
//                    SBS.digestionLength = (int)Mathf.Lerp(0, 2400, Mathf.InverseLerp(1, 8, aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.baseDamageResistance));
//                }
//                if (cI.impactCooldown < -1)
//                {
//                    cI.impactCooldown++;
//                }
//                if (cI.impactCooldown == -2 && stwAwy.room.world is not null && stwAwy.room.abstractRoom is not null)
//                {
//                    int smallCreatures = 0;
//                    for (int c = 0; c < aI.ctrList.Count; c++)
//                    {
//                        if (aI.ctrList[c].creatureTemplate.smallCreature ||
//                            aI.ctrList[c].creatureTemplate.type == CreatureTemplate.Type.SeaLeech ||
//                            aI.ctrList[c].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
//                        {
//                            smallCreatures++;
//                        }
//                    }
//                    for (bool addCtr = true; aI.functionTimer != 1; addCtr = Random.value < 1.75f - (aI.ctrList.Count - smallCreatures) * 0.75f)
//                    {
//                        if (addCtr)
//                        {
//                            aI.ctrList.AddRange(StowawayIndigestion(stwAwy));
//                        }
//                        else aI.functionTimer = 1;
//                    }
//                    if (aI.ctrList.Count > 0 && aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.type is not null)
//                    {

//                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit, stwAwy.DangerPos, 1.5f, Random.Range(0.25f, 0.4f));
//                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit, stwAwy.DangerPos, 1.5f, Random.Range(0.75f, 0.9f));
//                        stwAwy.room.PlaySound(SoundID.Red_Lizard_Spit_Hit_NPC, stwAwy.DangerPos, 1.5f, Random.Range(0.25f, 0.4f));

//                        int foodAtOnce = 0;
//                        for (int f = aI.ctrList.Count - 1; f >= 0; f--)
//                        {
//                            if (aI.ctrList[f].creatureTemplate.type == aI.ctrList[aI.ctrList.Count - 1].creatureTemplate.type)
//                            {
//                                foodAtOnce++;
//                            }
//                            else break;
//                        }

//                        for (int j = foodAtOnce; j > 0; j--)
//                        {
//                            IntVector2 tilePosition = stwAwy.room.GetTilePosition(stwAwy.DangerPos - new Vector2(0, 25f));
//                            WorldCoordinate worldCoordinate = stwAwy.room.GetWorldCoordinate(tilePosition);

//                            AbstractCreature eatenCtr = aI.ctrList[aI.ctrList.Count - 1];
//                            AbstractCreature newCtr = new(stwAwy.room.world, eatenCtr.creatureTemplate, null, worldCoordinate, eatenCtr.ID);

//                            bool dead = false;
//                            CreatureTemplate.Type type = newCtr.creatureTemplate.type;
//                            bool tough =
//                                type == CreatureTemplate.Type.SeaLeech ||
//                                type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
//                                type == CreatureTemplate.Type.DropBug ||
//                                type == CreatureTemplate.Type.RedCentipede ||
//                                type == CreatureTemplate.Type.RedLizard ||
//                                type == HailstormEnums.Freezer ||
//                                type == HailstormEnums.Cyanwing;
//                            if (Random.value < 0.6f)
//                            {
//                                dead = true;
//                            }
//                            else if (Random.value < 5 / 8f)
//                            {
//                                if (newCtr.state is HealthState HS && eatenCtr.state is HealthState HS2)
//                                {
//                                    float minPossibleHP =
//                                        (tough ? -0.20f : -0.50f) + HSRemix.StowawayFoodSurvivalBonus.Value;

//                                    HS.health =
//                                        Mathf.Min(HS2.health, Random.Range(minPossibleHP, Mathf.Lerp(0.75f, 1, HSRemix.StowawayFoodSurvivalBonus.Value)));
//                                }
//                                else if (Random.value < 0.5f)
//                                {
//                                    dead = true;
//                                }
//                            }

//                            if (dead && newCtr.state is not null)
//                            {
//                                newCtr.state.alive = false;
//                                if (newCtr.realizedCreature is not null)
//                                {
//                                    newCtr.realizedCreature.dead = true;
//                                }
//                            }

//                            stwAwy.room.abstractRoom.AddEntity(newCtr);
//                            CustomFlags(newCtr);
//                            newCtr.RealizeInRoom();
//                            newCtr.realizedCreature.mainBodyChunk.vel.x += Random.Range(-3f, 3f);
//                            if (newCtr.state is not null && newCtr.state.alive)
//                            {
//                                newCtr.realizedCreature.Stun(Random.Range(120, 240));
//                            }
//                            newCtr.realizedCreature.killTag = stwAwy.abstractCreature;

//                            aI.ctrList.RemoveAt(aI.ctrList.Count - 1);
//                        }


//                        if (aI.functionTimer != 1)
//                        {
//                            aI.functionTimer = 1;
//                        }
//                        cI.impactCooldown -= Random.Range(15, 40);

//                        /*
//                        if (RainWorld.ShowLogs)
//                        {
//                            Debug.Log(
//                                aI.ctrList.Count > 10 ? "[Hailstorm] DEAR LORD THAT STOWAWAY WAS HUNGRY; IT SPAT OUT " + aI.ctrList.Count + " CREATURES!" :
//                                aI.ctrList.Count > 05 ? "[Hailstorm] Stowaway spat out a whopping " + aI.ctrList.Count + " creatures!" :
//                                "[Hailstorm] Stowaway spat out " + aI.ctrList.Count + " creatures!");
//                        }
//                        */
//                    }
//                }
//            }
//        }
//    }
//    public static void StowFoodAway(On.MoreSlugcats.StowawayBug.orig_Eat orig, StowawayBug stwAwy, bool eu)
//    {
//        if (stwAwy?.eatObjects is not null && (IsRWGIncan(stwAwy.abstractCreature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out AbsCtrInfo aI) && aI.ctrList is not null)
//        {
//            for (int i = stwAwy.eatObjects.Count - 1; i >= 0; i--)
//            {
//                if (stwAwy.eatObjects[i].progression > 1f && stwAwy.eatObjects[i].chunk.owner is not null && stwAwy.eatObjects[i].chunk.owner is Creature ctr && ctr is not Player)
//                {
//                    aI.ctrList.Add(ctr.abstractCreature);
//                }
//            }
//        }
//        orig(stwAwy, eu);
//    }
//    public static void StowawayProvidesFood(On.MoreSlugcats.StowawayBug.orig_Die orig, StowawayBug stwAwy)
//    {
//        if (stwAwy is not null && (IsRWGIncan(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.CreatureData.TryGetValue(stwAwy, out CreatureInfo cI) && cI.impactCooldown >= 0)
//        {
//            cI.impactCooldown = Random.Range(-130, -40);
//        }
//        orig(stwAwy);
//    }
//    public static void EATFASTERDAMNIT(On.MoreSlugcats.StowawayBugState.orig_StartDigestion orig, StowawayBugState sbs, int cycleTime)
//    {
//        orig(sbs, cycleTime);
//        if (sbs?.creature is not null && (IsRWGIncan(sbs.creature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(sbs.creature, out AbsCtrInfo aI))
//        {
//            aI.functionTimer = -1;
//        }
//    }
//    public static void StowawayViolence(On.MoreSlugcats.StowawayBug.orig_Violence orig, StowawayBug stwAwy, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float dmg, float stun)
//    {
//        if (stwAwy?.room is not null)
//        {
//            if (Random.value < 0.925f && hitAppen is not null && source?.owner is not null && source.owner is Lizard liz && (liz.Template.type == HailstormEnums.IcyBlue || liz.Template.type == HailstormEnums.Freezer))
//            {
//                dmg = 0; // Gives Stowaways protection against icy lizard bites, and only their bites. (Spit from Freezers bypasses this)
//                stun = 0;
//            }
//            if (CWT.CreatureData.TryGetValue(stwAwy, out CreatureInfo cI) && (IsRWGIncan(stwAwy.room.game) || HSRemix.HailstormStowawaysEverywhere.Value is true)) // I know adding "== true" is redundant, but I'm doing it here for clarity's sake.
//            {
//                if (hitAppen is null)
//                {
//                    if (source?.owner is not null && source.owner is Weapon && dirAndMomentum.HasValue && Mathf.Abs(dirAndMomentum.Value.y) >= Mathf.Abs(dirAndMomentum.Value.x * 3))
//                    {
//                        dmg *= 1.4f / (HSRemix.StowawayHPMultiplier.Value); // Takes bonus damage from weapons thrown upwards or downwards
//                    }
//                    else if (!dirAndMomentum.HasValue || Mathf.Abs(dirAndMomentum.Value.y) < Mathf.Abs(dirAndMomentum.Value.x * 3))
//                    {
//                        cI.hitDeflected =
//                            HSRemix.StowawayToughSides.Value is true &&
//                            source?.owner is not null &&
//                            source.owner is not IceCrystal;
//                        dmg =
//                            cI.hitDeflected ?  0 : 0.75f / HSRemix.StowawayHPMultiplier.Value;
//                        if (cI.hitDeflected && source is not null && hitChunk is not null)
//                        {
//                            for (int num = 10; num > 0; num--)
//                            {
//                                stwAwy.room.AddObject(new Spark(Vector2.Lerp(hitChunk.pos, source.pos, 0.5f), Custom.RNV(), Color.white, null, 15, 25));
//                            }
//                            if (source.owner is not null && source.owner is Player plr)
//                            {
//                                plr.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, source.pos, 1.25f, 0.75f);
//                                plr.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, source.pos, 1.5f, 0.75f);
//                                if (CWT.PlayerData.TryGetValue(plr, out HailstormSlugcats hs) && hs.isIncan && !hs.readyToMoveOn)
//                                {
//                                    plr.Stun(Random.Range(20, 30));
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        orig(stwAwy, source, dirAndMomentum, hitChunk, hitAppen, dmgType, dmg, stun);
//    }
//    public static bool StowawayToughSides(On.Creature.orig_SpearStick orig, Creature victim, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appen, Vector2 direction)
//    {
//        if (victim?.room is not null && victim is StowawayBug && CWT.CreatureData.TryGetValue(victim, out CreatureInfo cI) && cI.hitDeflected)
//        {
//            cI.hitDeflected = false;
//            return false;
//        }
//        return orig(victim, source, dmg, chunk, appen, direction);
//    }

//    //--------------------------------------
//    public static bool StoreCreatureInsteadOfDestroy(StowawayBug stwAwy)
//    {
//        Vector2 pos = stwAwy.firstChunk.pos;
//        if (stwAwy?.eatObjects is not null && (IsRWGIncan(stwAwy.abstractCreature.world.game) || HSRemix.HailstormStowawaysEverywhere.Value == true) && CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out AbsCtrInfo aI) && aI.ctrList is not null)
//        {
//            for (int i = stwAwy.eatObjects.Count - 1; i >= 0; i--)
//            {
//                if (stwAwy.eatObjects[i].progression > 1f && stwAwy.eatObjects[i].chunk.owner is not null && stwAwy.eatObjects[i].chunk.owner is Creature ctr)
//                {
//                    if (ctr is Player && ctr.room is not null)
//                    {
//                        AbstractCreature ctrCopy = new(ctr.room.world, ctr.Template, new Player(ctr.abstractCreature, ctr.room.world), ctr.abstractCreature.pos, ctr.abstractCreature.ID);
//                        aI.ctrList.Add(ctrCopy);
//                    }
//                    else
//                    {
//                        aI.ctrList.Add(ctr.abstractCreature);
//                    }

//                    stwAwy.AI.tracker.ForgetCreature(ctr.abstractCreature);
//                    ctr.RemoveFromRoom();
//                    ctr.abstractPhysicalObject.Room.RemoveEntity(ctr.abstractPhysicalObject);
//                    stwAwy.eatObjects.RemoveAt(i);
//                    return true;
//                }
//            }
//        }
//        return false;
//    }
//    public static List<AbstractCreature> StowawayIndigestion(StowawayBug stwAwy)
//    {
//        List<CreatureTemplate.Type> regionSpawns = new();

//        if (stwAwy.room.world.spawners is not null && stwAwy.room.world.spawners.Length > 0)
//        {
//            foreach (World.CreatureSpawner cS in stwAwy.room.world.spawners)
//            {
//                if (cS is World.SimpleSpawner spawner &&
//                    spawner.creatureType is not null)
//                {
//                    bool smallCreature =
//                        StaticWorld.GetCreatureTemplate(spawner.creatureType).smallCreature ||
//                        spawner.creatureType == CreatureTemplate.Type.Fly ||
//                        spawner.creatureType == CreatureTemplate.Type.Leech ||
//                        spawner.creatureType == CreatureTemplate.Type.SeaLeech ||
//                        spawner.creatureType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
//                        spawner.creatureType == CreatureTemplate.Type.Spider;

//                    for (int s = 0; s < (smallCreature ? 1 : Mathf.Max(1, spawner.amount / 3)); s++)
//                    {
//                        regionSpawns.Add(spawner.creatureType);
//                    }
//                }
//                else if (cS is World.Lineage lineage &&
//                    lineage.creatureTypes is not null &&
//                    stwAwy.room.game.session is StoryGameSession SGS &&
//                    lineage.CurrentType(SGS.saveState) is not null)
//                {
//                    regionSpawns.Add(lineage.CurrentType(SGS.saveState));
//                }
//            }
//        }

//        CreatureTemplate.Type ctrType = null;
//        if (regionSpawns is not null && regionSpawns.Count > 0 && stwAwy.AI is not null)
//        {
//            int smallCreatures = 0;
//            if (CWT.AbsCtrData.TryGetValue(stwAwy.abstractCreature, out AbsCtrInfo aI) && aI.ctrList is not null)
//            {
//                for (int i = 0; i < aI.ctrList.Count; i++)
//                {
//                    if (aI.ctrList[i].creatureTemplate.smallCreature ||
//                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Fly ||
//                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Leech ||
//                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.SeaLeech ||
//                        aI.ctrList[i].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
//                        aI.ctrList[i].creatureTemplate.type == CreatureTemplate.Type.Spider)
//                    {
//                        smallCreatures++;
//                    }
//                }
//            }

//            for (int i = regionSpawns.Count - 1; i >= 0; i--)
//            {
//                if (!stwAwy.AI.WantToEat(regionSpawns[i]) || ctrType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
//                    (smallCreatures > 3 &&
//                    (regionSpawns[i] == CreatureTemplate.Type.Fly ||
//                    regionSpawns[i] == CreatureTemplate.Type.Leech ||
//                    regionSpawns[i] == CreatureTemplate.Type.SeaLeech ||
//                    regionSpawns[i] == CreatureTemplate.Type.Spider ||
//                    regionSpawns[i] == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)))
//                {
//                    regionSpawns.RemoveAt(i);
//                }
//            }

//            int ctrNum =
//                Random.Range(0, regionSpawns.Count);

//            float strength =
//                StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).baseDamageResistance +
//                StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).baseStunResistance;

//            if (regionSpawns[ctrNum] == CreatureTemplate.Type.RedCentipede ||
//                regionSpawns[ctrNum] == HailstormEnums.Cyanwing)
//            {
//                strength = 10;
//            }

//            if (strength >= (StaticWorld.GetCreatureTemplate(regionSpawns[ctrNum]).ancestor == StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate) ? 6 : 8) &&
//                Random.value < 0.2f + strength / 30f)
//            {
//                ctrNum = (ctrNum + 1 >= regionSpawns.Count) ? 0 : ctrNum + 1;
//            }

//            ctrType = regionSpawns[ctrNum];
//        }
//        else switch (Random.value)
//            {
//                case < 0.100f:
//                    ctrType =
//                        Random.value > 0.5f ?
//                        CreatureTemplate.Type.SmallNeedleWorm :
//                        CreatureTemplate.Type.BigNeedleWorm;
//                    break;
//                case < 0.150f:
//                    ctrType = CreatureTemplate.Type.EggBug;
//                    break;
//                case < 0.225f:
//                    ctrType = CreatureTemplate.Type.TubeWorm;
//                    break;
//                case < 0.325f:
//                    ctrType = CreatureTemplate.Type.Hazer;
//                    break;
//                case < 0.400f:
//                    ctrType =
//                        Random.value > 0.5f ?
//                        CreatureTemplate.Type.Centipede :
//                        CreatureTemplate.Type.Centiwing;
//                    break;
//                case < 0.475f:
//                    ctrType = CreatureTemplate.Type.JetFish;
//                    break;
//                case < 0.550f:
//                    ctrType = CreatureTemplate.Type.LanternMouse;
//                    break;
//                case < 0.650f:
//                    switch (Random.Range(0, 4))
//                    {
//                        case 0:
//                            ctrType = CreatureTemplate.Type.Spider;
//                            break;
//                        case 1:
//                            ctrType = CreatureTemplate.Type.BigSpider;
//                            break;
//                        case 2:
//                            ctrType = CreatureTemplate.Type.SpitterSpider;
//                            break;
//                        case 3:
//                            ctrType = MoreSlugcatsEnums.CreatureTemplateType.MotherSpider;
//                            break;
//                    }
//                    break;
//                case < 0.725f:
//                    ctrType = CreatureTemplate.Type.Snail;
//                    break;
//                case < 0.800f:
//                    ctrType =
//                        Random.value > 0.5f ?
//                        CreatureTemplate.Type.CicadaA :
//                        CreatureTemplate.Type.CicadaB;
//                    break;
//                case < 0.850f:
//                    ctrType = MoreSlugcatsEnums.CreatureTemplateType.Yeek;
//                    break;
//                case < 0.925f:
//                    ctrType = CreatureTemplate.Type.DropBug;
//                    break;
//                case < 1f:
//                    switch (Random.Range(0, 10))
//                    {
//                        case 0:
//                            ctrType =
//                                Random.value < 0.75f ? CreatureTemplate.Type.PinkLizard :
//                                Random.value < 0.75f ? MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard :
//                                CreatureTemplate.Type.RedLizard;
//                            break;
//                        case 1:
//                            ctrType = CreatureTemplate.Type.GreenLizard;
//                            break;
//                        case 2:
//                            ctrType =
//                                Random.value < 0.75? CreatureTemplate.Type.BlueLizard :
//                                Random.value < 0.75? HailstormEnums.IcyBlue :
//                                HailstormEnums.Freezer;
//                            break;
//                        case 3:
//                            ctrType = CreatureTemplate.Type.Salamander;
//                            break;
//                        case 4:
//                            ctrType = MoreSlugcatsEnums.CreatureTemplateType.EelLizard;
//                            break;
//                        case 5:
//                            ctrType = CreatureTemplate.Type.WhiteLizard;
//                            break;
//                        case 6:
//                            ctrType = CreatureTemplate.Type.YellowLizard;
//                            break;
//                        case 7:
//                            ctrType = CreatureTemplate.Type.BlackLizard;
//                            break;
//                        case 8:
//                            ctrType = MoreSlugcatsEnums.CreatureTemplateType.SpitLizard;
//                            break;
//                        case 9:
//                            ctrType = CreatureTemplate.Type.CyanLizard;
//                            break;
//                        default:
//                            ctrType = null;
//                            break;
//                    }
//                    break;
//                default:
//                    ctrType = null;
//                    break;
//            }

//        if (IsRWGIncan(stwAwy.room.game) && stwAwy.room.abstractRoom.name == "GW_A08")
//        {
//            ctrType =
//                (Random.value < 0.50f) ? CreatureTemplate.Type.DropBug :
//                (Random.value < 0.50f) ? CreatureTemplate.Type.SeaLeech :
//                (Random.value < 0.50f) ? CreatureTemplate.Type.Snail :
//                (Random.value < 0.50f) ? CreatureTemplate.Type.Salamander : MoreSlugcatsEnums.CreatureTemplateType.EelLizard;
//        }

//        List<AbstractCreature> newCtrList = new();
//        if (ctrType is not null)
//        {
//            bool altForm = false;
//            if (regionSpawns is null && (ctrType == CreatureTemplate.Type.Centipede || ctrType == CreatureTemplate.Type.Centiwing))
//            {
//                if (Random.value < 0.65f)
//                {
//                    altForm = ctrType == CreatureTemplate.Type.Centiwing;
//                    ctrType = CreatureTemplate.Type.SmallCentipede;
//                }
//                else if (Random.value < 0.03f)
//                {
//                    ctrType =
//                        ctrType == CreatureTemplate.Type.Centipede?
//                        CreatureTemplate.Type.RedCentipede :
//                        HailstormEnums.Cyanwing;
//                }
//            }

//            int reps =
//                ctrType == CreatureTemplate.Type.Fly ? (Random.value < 0.5f ? 3 : 2) :
//                ctrType == CreatureTemplate.Type.Hazer ? (Random.value < 0.25f ? 2 : 1) :
//                ctrType == CreatureTemplate.Type.LanternMouse ||
//                 ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek ? (Random.value < 0.15f ? 2 : 1) :
//                ctrType == CreatureTemplate.Type.Leech ||
//                ctrType == CreatureTemplate.Type.SeaLeech ||
//                ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ? Random.Range(3, 6) :
//                ctrType == CreatureTemplate.Type.Spider ? Random.Range(4, 8) : 1;

//            for (int i = 0; i < reps; i++)
//            {
//                IntVector2 tilePosition = stwAwy.room.GetTilePosition(stwAwy.DangerPos - new Vector2(0, 25f));
//                WorldCoordinate worldCoordinate = stwAwy.room.GetWorldCoordinate(tilePosition);
//                EntityID newID = stwAwy.room.game.GetNewID();
//                AbstractCreature newCtr = new(stwAwy.room.world, StaticWorld.GetCreatureTemplate(ctrType), null, worldCoordinate, newID);

//                if (ctrType == CreatureTemplate.Type.SmallCentipede && altForm)
//                {
//                    newCtr.superSizeMe = true;
//                }

//                newCtrList.Add(newCtr);
//            }
//        }
//        return newCtrList;
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Grabby Plants

//    public static void AngrierPolePlants(On.PoleMimic.orig_Act orig, PoleMimic pm)
//    {
//        orig(pm);
//        if (IsRWGIncan(pm?.room?.game) && pm.wantToWakeUp && pm.wakeUpCounter < 250)
//        {
//            pm.wakeUpCounter++;
//        }
//    }
//    public static void WinterPolePlantDifferences(On.PoleMimicGraphics.orig_ApplyPalette orig, PoleMimicGraphics pmg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
//    {
//        orig(pmg, sLeaser, rCam, palette);
//        if (pmg?.pole?.room is not null && pmg.pole.abstractCreature.Winterized && (IsRWGIncan(pmg.pole.room.game) || HSRemix.PolePlantColorsEverywhere.Value is true) && CWT.CreatureData.TryGetValue(pmg.pole, out CreatureInfo cI))
//        {
//            Random.State state = Random.state;
//            Random.InitState(pmg.pole.abstractCreature.ID.RandomSeed);
//            pmg.mimicColor = Color.Lerp(palette.texture.GetPixel(4, 3), palette.fogColor, palette.fogAmount * (4f / 15f));
//            pmg.blackColor = new Color(0.15f, 0.15f, 0.15f);
//            cI.customFunction = Random.Range(1.75f, 2.5f);
//            Random.state = state;
//        }
//    }
//    public static void PolePlantColors(On.PoleMimicGraphics.orig_DrawSprites orig, PoleMimicGraphics pmg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
//    {
//        orig(pmg, sLeaser, rCam, timeStacker, camPos);
//        if (pmg?.pole?.room is not null && (IsRWGIncan(pmg.pole.room.game) || HSRemix.PolePlantColorsEverywhere.Value is true) && CWT.CreatureData.TryGetValue(pmg.pole, out CreatureInfo cI))
//        {
//            Random.State state = Random.state;
//            Random.InitState(pmg.pole.abstractCreature.ID.RandomSeed);
//            Color col1 =
//                    pmg.pole.abstractCreature.Winterized ?
//                    Custom.HSL2RGB(Random.Range(190/360f, 250/360f), 1, Custom.ClampedRandomVariation(0.9f, 0.10f, 0.4f)) : // Winter colors
//                    pmg.pole.abstractCreature.world.region is not null &&
//                    pmg.pole.abstractCreature.world.region.name == "OE" ?
//                    Random.ColorHSV(40 / 360f, 140 / 360f, 1, 1, 0.5f, 0.6f) : // Outer Expanse colors
//                    Custom.HSL2RGB(Random.Range(-40/360f, 40/360f), 1, Custom.ClampedRandomVariation(0.6f, 0.15f, 0.2f)); // Default colors
//            Color col2 =
//                    pmg.pole.abstractCreature.Winterized ?
//                    Custom.HSL2RGB(Random.Range(190/360f, 250/360f), 1, Custom.ClampedRandomVariation(0.9f, 0.10f, 0.4f)) : // Winter colors
//                    pmg.pole.abstractCreature.world.region is not null &&
//                    pmg.pole.abstractCreature.world.region.name == "OE" ?
//                    Random.ColorHSV(40/360f, 140/360f, 1, 1, 0.5f, 0.6f) : // Outer Expanse colors
//                    Custom.HSL2RGB(Random.Range(-40/360f, 40/360f), 1, Custom.ClampedRandomVariation(0.6f, 0.15f, 0.2f)); // Default colors

//            float gradientSkew;
//            switch (Random.Range(0, 3))
//            {
//                case 0:
//                    gradientSkew = 0.4f;
//                    break;
//                case 2:
//                    gradientSkew = 1.6f;
//                    break;
//                default:
//                    gradientSkew = 1f;
//                    break;
//            }

//            Random.state = state;

//            Color val = Color.Lerp(pmg.blackColor, pmg.mimicColor, Mathf.Lerp(pmg.lastLookLikeAPole, pmg.lookLikeAPole, timeStacker));
//            sLeaser.sprites[0].color = val;

//            for (int i = 0; i < pmg.leafPairs; i++)
//            {
//                for (int j = 0; j < 2; j++)
//                {
//                    if (i >= pmg.decoratedLeafPairs) return;

//                    Color leafColor = Color.Lerp(col1, col2, Mathf.Pow(Mathf.InverseLerp(0, pmg.leafPairs/3f, i), gradientSkew));

//                    sLeaser.sprites[pmg.LeafDecorationSprite(i, j)].color =
//                        Color.Lerp(leafColor, val, Mathf.Pow(Mathf.InverseLerp((pmg.decoratedLeafPairs / 2f), pmg.decoratedLeafPairs, i), 0.6f));

//                    if (pmg.pole.abstractCreature.Winterized)
//                    {
//                        sLeaser.sprites[pmg.LeafDecorationSprite(i, j)].scaleY *= cI.customFunction;
//                    }
//                }
//            }
//        }
//    }

//    public static void MonsterKelpColors(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics mkg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
//    {
//        orig(mkg, sLeaser, rCam, palette);
//        if (mkg?.plant?.room is not null && (IsRWGIncan(mkg.plant.room.game) || HSRemix.MonsterKelpColorsEverywhere.Value is true))
//        {
//            Random.State state = Random.state;
//            Random.InitState(mkg.plant.abstractCreature.ID.RandomSeed);
//            Color col1 =
//                mkg.plant.abstractCreature.Winterized ?
//                Custom.HSL2RGB(Random.Range(200f, 300f)/360f, Random.Range(0.825f, 1), Custom.WrappedRandomVariation(0.875f, 0.125f, 0.2f)) : // Winter colors
//                mkg.plant.abstractCreature.world.region is not null &&
//                mkg.plant.abstractCreature.world.region.name == "OE" ?
//                Random.ColorHSV(30/360f, 170/360f, 1, 1, 0.4f, 0.7f) : // Outer Expanse colors
//                Random.ColorHSV(-60/360f, 20/360f, 1, 1, 0.45f, 0.65f); // Default colors

//            Color col2 =
//                mkg.plant.abstractCreature.Winterized ?
//                Custom.HSL2RGB(Random.Range(200f, 300f) / 360f, Random.Range(0.825f, 1), Custom.WrappedRandomVariation(0.875f, 0.125f, 0.2f)) : // Winter colors
//                mkg.plant.abstractCreature.world.region is not null &&
//                mkg.plant.abstractCreature.world.region.name == "OE" ?
//                Random.ColorHSV(30/360f, 170/360f, 1, 1, 0.4f, 0.7f) : // Outer Expanse colors
//                Random.ColorHSV(-60/360f, 20/360f, 1, 1, 0.45f, 0.65f); // Default colors

//            float gradientSkew;
//            switch (Random.Range(0, 3))
//            {
//                case 0:
//                    gradientSkew = 0.4f;
//                    break;
//                case 2:
//                    gradientSkew = 1.6f;
//                    break;
//                default:
//                    gradientSkew = 1f;
//                    break;
//            }

//            for (int i = 0; i < mkg.danglers.Length; i++)
//            {
//                Color finalKelpColors = Color.Lerp(col1, col2, Mathf.Pow(mkg.danglerProps[i, 0], gradientSkew));

//                sLeaser.sprites[i + 1].color = Color.Lerp(finalKelpColors, sLeaser.sprites[0].color, rCam.room.Darkness(mkg.plant.rootPos));
//            }

//            Random.state = state;
//        }
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region Big Jellyfish

//    public static void BigJellyCRONCH(On.MoreSlugcats.BigJellyFish.orig_Collide orig, BigJellyFish bigJelly, PhysicalObject physObj, int myChunk, int otherChunk) 
//    {
//        orig(bigJelly, physObj, myChunk, otherChunk);
//        if (IsRWGIncan(bigJelly?.room?.game) &&
//            !bigJelly.dead &&
//            bigJelly.bodyChunks[myChunk].vel.y >= 5f &&
//            physObj is not null &&
//            physObj is Creature victim &&
//            victim.bodyChunks[otherChunk].contactPoint.y > 0 &&
//            CWT.CreatureData.TryGetValue(bigJelly, out CreatureInfo jI) &&
//            CWT.CreatureData.TryGetValue(victim, out CreatureInfo vI) &&
//            (jI.impactCooldown == 0 || vI.impactCooldown == 0))
//        {
//            jI.impactCooldown = 40;
//            vI.impactCooldown = 40;
//            victim.Violence(bigJelly.bodyChunks[myChunk], bigJelly.bodyChunks[myChunk].vel, victim.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, 2f, Random.Range(60, 81));
//            bigJelly.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, bigJelly.bodyChunks[myChunk], false, 1.4f, 1.1f);
//            float volume = (victim.State is HealthState HS && HS.ClampedHealth == 0 || victim.State.dead) ? 1.66f : 1f;
//            bigJelly.room.PlaySound(SoundID.Spear_Stick_In_Creature, bigJelly.bodyChunks[myChunk], false, volume, Random.Range(0.6f, 0.8f));
//        }
//    }

//    public static void BIGJellyfishMold(On.MoreSlugcats.BigJellyFish.orig_Die orig, BigJellyFish bigJelly)
//    {
//        if (IsRWGIncan(bigJelly?.room?.game) && !bigJelly.dead)
//        {
//            bigJelly.SMSuckCounter = 100;
//            while (bigJelly.grabbedBy.Count > 0)
//            {
//                bigJelly.grabbedBy[0].Release();
//            }
//            bigJelly.abstractCreature.LoseAllStuckObjects();
//            bigJelly.consumedCreatures.Clear();
//            BodyChunk[] array = bigJelly.bodyChunks;
//            for (int i = 0; i < array.Length; i++)
//            {
//                array[i].collideWithObjects = false;
//            }

//            int num = Random.Range(8, 16);
//            for (int j = 0; j < num; j++)
//            {
//                AbstractConsumable jellyWad = new AbstractConsumable(bigJelly.room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, bigJelly.abstractCreature.pos, bigJelly.room.game.GetNewID(), -1, -1, null);
//                jellyWad.destroyOnAbstraction = true;
//                bigJelly.room.abstractRoom.AddEntity(jellyWad);
//                jellyWad.RealizeInRoom();
//                (jellyWad.realizedObject as SlimeMold).JellyfishMode = true;
//                jellyWad.realizedObject.firstChunk.pos += Custom.RNV() * Random.value * 85f;
//                jellyWad.realizedObject.firstChunk.vel *= 0f;
//            }

//            num = Random.Range(3, 5);
//            for (int k = 0; k < num; k++)
//            {
//                bool big = false;
//                if (k < num && Random.value < 0.5f)
//                {
//                    k++;
//                    big = true;
//                }
//                AbstractConsumable slimeMold = new AbstractSlimeMold(bigJelly.room.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, bigJelly.abstractCreature.pos, bigJelly.room.game.GetNewID(), -1, -1, null, big);
//                bigJelly.room.abstractRoom.AddEntity(slimeMold);
//                slimeMold.RealizeInRoom();
//                slimeMold.realizedObject.firstChunk.pos = bigJelly.bodyChunks[bigJelly.CoreChunk].pos + Custom.RNV() * Random.value * 15f;
//                slimeMold.realizedObject.firstChunk.vel *= 0f;
//            }

//            if (!bigJelly.dead)
//            {
//                if (RainWorld.ShowLogs)
//                {
//                    Debug.Log("Die! " + bigJelly.Template.name);
//                }
//                if (ModManager.MSC && bigJelly.room is not null && bigJelly.room.world.game.IsArenaSession && bigJelly.room.world.game.GetArenaGameSession.chMeta is not null && (bigJelly.room.world.game.GetArenaGameSession.chMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT || bigJelly.room.world.game.GetArenaGameSession.chMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT))
//                {
//                    bool flag = false;
//                    if (bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature is null || bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature == "")
//                    {
//                        flag = true;
//                    }
//                    else if (bigJelly.Template.name.ToLower() == bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower() || bigJelly.abstractCreature.creatureTemplate.type.value.ToLower() == bigJelly.room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower())
//                    {
//                        flag = true;
//                    }
//                    if (bigJelly.protectDeathRecursionFlag)
//                    {
//                        flag = false;
//                    }
//                    if (flag)
//                    {
//                        for (int i = 0; i < bigJelly.room.world.game.Players.Count; i++)
//                        {
//                            if (bigJelly.room.world.game.Players[i].realizedCreature is not null && !bigJelly.room.world.game.Players[i].realizedCreature.dead)
//                            {
//                                bigJelly.room.world.game.Players[i].realizedCreature.protectDeathRecursionFlag = true;
//                                bigJelly.room.world.game.Players[i].realizedCreature.Die();
//                            }
//                        }
//                    }
//                }
//                if (bigJelly?.killTag?.realizedCreature is not null)
//                {
//                    Room realizedRoom = bigJelly.room;
//                    if (realizedRoom is null)
//                    {
//                        realizedRoom = bigJelly.abstractCreature.Room.realizedRoom;
//                    }
//                    if (realizedRoom?.socialEventRecognizer is not null)
//                    {
//                        realizedRoom.socialEventRecognizer.Killing(bigJelly.killTag.realizedCreature, bigJelly);
//                    }
//                    if (bigJelly.abstractCreature.world.game.IsArenaSession && bigJelly.killTag.realizedCreature is Player)
//                    {
//                        bigJelly.abstractCreature.world.game.GetArenaGameSession.Killing(bigJelly.killTag.realizedCreature as Player, bigJelly);
//                    }
//                }
//                bigJelly.dead = true;
//                bigJelly.LoseAllGrasps();
//                bigJelly.abstractCreature.Die();
//            }
//            bigJelly.Destroy();
//            bigJelly.abstractCreature.Destroy();
//        }
//        orig(bigJelly);
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region ALEX YEEK??!??!?!?

//    public static void YeekColors(On.MoreSlugcats.YeekGraphics.orig_CreateCosmeticAppearance orig, YeekGraphics yGrph)
//    {
//        orig(yGrph);
//        if (IsRWGIncan(yGrph?.myYeek?.room?.game))
//        {
//            AbstractCreature absYeek = yGrph.myYeek.abstractCreature;
//            float groupLeaderPotential = yGrph.myYeek.GroupLeaderPotential;

//            Random.State state = Random.state;
//            Random.InitState(absYeek.ID.RandomSeed);

//            HSLColor accColor =
//                absYeek.Winterized?
//                new(Random.Range(160/360f, 280/360f), (Random.value < 0.1? 0 : Random.Range(0.75f, 1)), (Random.value < 0.1f ? Random.Range(0.45f, 0.55f) : Random.Range(0.55f, 0.70f))) : // Winter colors
//                absYeek.world.region is not null &&
//                absYeek.world.region.name == "OE" ?
//                new(Custom.WrappedRandomVariation(30 / 360f, 80 / 360f, 0.5f), Random.Range(0.8f, 1), Custom.WrappedRandomVariation((Random.value < 0.15? 0.55f : 0.7f), 0.1f, 0.5f)) : // Outer Expanse colors
//                new(Random.value, Random.Range(0.85f, 1), Custom.WrappedRandomVariation(0.66f, 0.11f, 0.1f)); // Default colors

//            yGrph.tailHighlightColor = Color.HSVToRGB(accColor.hue, accColor.saturation, accColor.lightness);
//            yGrph.featherColor = Color.HSVToRGB(Custom.ClampedRandomVariation(accColor.hue, 20/360f, 0.2f), accColor.saturation + 0.075f, accColor.lightness - 0.15f);

//            if (Random.value < 0.01f)
//            {
//                Color c = yGrph.tailHighlightColor;
//                yGrph.tailHighlightColor = yGrph.featherColor;
//                yGrph.featherColor = c;

//            }

//            yGrph.furColor = Color.Lerp(yGrph.featherColor, Color.HSVToRGB(accColor.hue, accColor.saturation - 0.1f, accColor.lightness - 0.3f), 0.33f + absYeek.personality.energy * 0.25f);

//            Color val =
//                Color.Lerp(yGrph.featherColor, new Color(0.33f, 0.33f, 0.33f), 0.33f + absYeek.personality.aggression * 0.25f);
//            val =
//                (absYeek.personality.nervous <= absYeek.personality.bravery) ?
//                Color.Lerp(val, Color.black, absYeek.personality.bravery * 0.5f) :
//                Color.Lerp(val, Color.white, absYeek.personality.nervous * 0.5f);

//            yGrph.furColor = Color.Lerp(val, yGrph.furColor, absYeek.personality.sympathy);
//            yGrph.furColor = Color.Lerp(yGrph.furColor, Color.white, Random.Range(0.6f, 0.75f) + (absYeek.Winterized? 0.15f : 0));
//            yGrph.HeadfurColor = Color.Lerp(yGrph.furColor + new Color(0.1f, 0.1f, 0.1f), yGrph.furColor + new Color(0.3f, 0.15f, 0.15f), absYeek.personality.bravery);
//            yGrph.HeadfurColor = Color.Lerp(yGrph.furColor, yGrph.HeadfurColor, absYeek.personality.dominance);
//            yGrph.beakColor = Color.Lerp(yGrph.furColor, new Color(0.81f, 0.53f, 0.34f), 0.6f + absYeek.personality.dominance / 3f);
//            yGrph.featherColor = yGrph.tailHighlightColor;
//            yGrph.trueEyeColor = yGrph.featherColor;
//            yGrph.plumageGraphic = 2;
//            while (yGrph.plumageGraphic == 2 || yGrph.plumageGraphic == 1)
//            {
//                yGrph.plumageGraphic = Random.Range(0, 7);
//            }
//            Random.Range(0.5f, Mathf.Clamp(groupLeaderPotential * 1.5f, 0.6f, 1.2f));
//            int num2 = Random.Range(3, 5);
//            for (int num3 = num2; num3 > 0; num3--)
//            {
//                YeekGraphics.YeekFeather yeekFeather = new(yGrph.myYeek.bodyChunks[0].pos, yGrph, num3, num2)
//                {
//                    featherScaler = 1.2f
//                };
//                yGrph.bodyFeathers.Add(yeekFeather);
//            }

//            Random.state = state;
//        }
//    }
//    public static void YeekEyesBecauseTheirColorIsStupidlySemiHardcoded(On.MoreSlugcats.YeekGraphics.orig_DrawSprites orig, YeekGraphics yGrph, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
//    {
//        orig(yGrph, sLeaser, rCam, timeStacker, camPos);
//        if (IsRWGIncan(yGrph?.myYeek?.room?.game))
//        {
//            for (int j = 0; j < 2; j++)
//            {
//                sLeaser.sprites[yGrph.HeadSpritesStart + 2 + j].color = Color.Lerp(yGrph.eyeColor, yGrph.featherColor, yGrph.darkness);
//            }
//        }
//    }

//    #endregion

//    //----------------------------------------------------------------------------------
//    //----------------------------------------------------------------------------------

//    #region General Creature Stuff

//    // The following three methods manage my own version of the IProvideWarmth interface, which I made to be able to customize and edit heat sources more freely.
//    public static List<UpdatableAndDeletable> tempSources;
//    public static void BootlegIProvideWarmth(On.Room.orig_ctor orig, Room room, RainWorldGame game, World world, AbstractRoom abstractRoom)
//    {
//        orig(room, game, world, abstractRoom);

//        tempSources = new List<UpdatableAndDeletable>();
//    }
//    public static void BootlegIPWAdder(On.Room.orig_AddObject orig, Room room, UpdatableAndDeletable obj)
//    {
//        orig(room, obj);
//        if (room.game is not null && obj is not null)
//        {
//            if (obj is Creature ctr && (
//                (ctr is EggBug egg && !egg.dead && egg.FireBug) ||
//                (ctr is Player plr && !plr.dead && plr.SlugCatClass == HailstormSlugcats.Incandescent) ||
//                (ctr is Lizard liz && HailstormLizards.LizardData.TryGetValue(liz, out LizardInfo lI) && lI.isFreezerOrIcyBlue)
//                ))
//            {
//                tempSources.Add(ctr);
//            }
//            else if (obj is Weapon wpn && wpn is Spear spr && (spr.bugSpear || (spr.abstractSpear is AbstractBurnSpear absBrnSpr && absBrnSpr.heat > 0)))
//            {
//                tempSources.Add(wpn);
//            }
//            else if (obj is PlayerCarryableItem UaD && (UaD is Lantern || UaD is FireEgg))
//            {
//                tempSources.Add(UaD);
//            }
//        }
//    }
//    public static void BootlegIPWRemover(On.Room.orig_CleanOutObjectNotInThisRoom orig, Room room, UpdatableAndDeletable obj)
//    {
//        orig(room, obj);
//        if (obj is not null)
//        {
//            if (obj is Creature ctr && (
//                (ctr is EggBug egg && egg.FireBug) ||
//                (ctr is Player plr && plr.SlugCatClass == HailstormSlugcats.Incandescent) ||
//                (ctr is Lizard liz && HailstormLizards.LizardData.TryGetValue(liz, out LizardInfo lI) && lI.isFreezerOrIcyBlue)
//                ))
//            {
//                tempSources.Remove(ctr);
//            }
//            else if (obj is Weapon wpn && wpn is Spear spr && (spr.bugSpear || spr.abstractSpear is AbstractBurnSpear))
//            {
//                tempSources.Remove(wpn);
//            }
//            else if (obj is PlayerCarryableItem UaD && (UaD is Lantern || UaD is FireEgg))
//            {
//                tempSources.Remove(UaD);
//            }
//        }
//    }

//    //----------------------------------------------------------------------------------

//    public static void ElementalResistances(On.CreatureTemplate.orig_ctor_Type_CreatureTemplate_List1_List1_Relationship orig, CreatureTemplate temp, CreatureTemplate.Type type, CreatureTemplate ancestor, List<TileTypeResistance> tileResistances, List<TileConnectionResistance> connectionResistances, CreatureTemplate.Relationship defaultRelationship)
//    {
//        orig(temp, type, ancestor, tileResistances, connectionResistances, defaultRelationship);

//        if (type == CreatureTemplate.Type.EggBug || type == CreatureTemplate.Type.DropBug ||
//            type == CreatureTemplate.Type.Spider || type == CreatureTemplate.Type.BigSpider ||
//            type == CreatureTemplate.Type.PoleMimic || type == CreatureTemplate.Type.TentaclePlant ||
//            type == CreatureTemplate.Type.SmallNeedleWorm || type == CreatureTemplate.Type.BigNeedleWorm ||
//            type == CreatureTemplate.Type.SpitterSpider || type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
//            type == HailstormEnums.Luminescipede)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 0.8f; // 0 is damage resistance
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 0.8f; // 1 is stun resistance
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 0.8f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 0.8f;
//        }
//        else
//        if (type == CreatureTemplate.Type.Snail || type == CreatureTemplate.Type.Hazer ||
//            type == CreatureTemplate.Type.Leech || type == CreatureTemplate.Type.SeaLeech ||
//            type == CreatureTemplate.Type.TubeWorm || type == CreatureTemplate.Type.JetFish ||
//            type == CreatureTemplate.Type.BigEel || type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly ||
//            type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti || type == HailstormEnums.InfantAquapede)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 1.25f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 1.5f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 1.25f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 1.5f;
//        }
//        else if (type == MoreSlugcatsEnums.CreatureTemplateType.FireBug || type == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 0.25f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 0.33f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 5f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 5f;
//        }
//        else if (type == HailstormEnums.Chillipede)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 1.5f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 1.5f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 0.5f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 0.5f;
//        }
//        else if (type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 1f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 1f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 0.7f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 0.7f;

//        }
//        else if (type == CreatureTemplate.Type.BrotherLongLegs || type == CreatureTemplate.Type.DaddyLongLegs || type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 0.25f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 0.50f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 0.25f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 0.50f;

//        }
//        else
//        {
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 0] = 1f;
//            temp.damageRestistances[HailstormEnums.ColdDamage.index, 1] = 1f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 0] = 1f;
//            temp.damageRestistances[HailstormEnums.HeatDamage.index, 1] = 1f;
//        }
//    }

//    public static bool CreatureEdibility(On.Player.orig_CanEatMeat orig, Player self, Creature ctr)
//    {
//        if (IsRWGIncan(self?.room?.game) && ctr is StowawayBug && ctr.dead)
//        {
//            return true;
//        }
//        if (CWT.PlayerData.TryGetValue(self, out HailstormSlugcats player) && player.isIncan)
//        {
//            if (ctr.Template.type == CreatureTemplate.Type.Slugcat || ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
//            {
//                return false;
//            }
//        }
//        return orig(self, ctr);
//    }

//    public static void HypothermiaSafeAreas(On.AbstractCreature.orig_Update orig, AbstractCreature absCtr, int time)
//    {
//        orig(absCtr, time);
//        if (absCtr.realizedCreature is null && absCtr.Hypothermia < 1f && IsRWGIncan(absCtr.world.game) && absCtr.world.region is not null && !(absCtr.world.region.name == "UG") && !(absCtr.world.region.name == "OE") && !(absCtr.world.region.name == "CL") && !(absCtr.world.region.name == "SB"))
//        {
//            if (absCtr.InDen || absCtr.HypothermiaImmune)
//            {
//                absCtr.Hypothermia = Mathf.Lerp(absCtr.Hypothermia, 0f, 0.04f);
//            }
//            else
//            {
//                absCtr.Hypothermia = Mathf.Lerp(absCtr.Hypothermia, 3f, Mathf.InverseLerp(0f, -600f, absCtr.world.rainCycle.AmountLeft));
//            }
//        }
//    }

//    public static void CreatureTimersAndMechanics(On.Creature.orig_Update orig, Creature ctr, bool eu)
//    {
//        orig(ctr, eu);
//        if (ctr is null || !CWT.CreatureData.TryGetValue(ctr, out CreatureInfo cI))
//        {
//            return;
//        }

//        if (cI.impactCooldown > 0)
//        {
//            cI.impactCooldown--; // Used for the IncanCollision method in IncanFeatures.
//        }

//        if (cI.chillTimer > 0)
//        {
//            ctr.Hypothermia += 0.05f;
//            cI.chillTimer--;
//        }
//        if (cI.heatTimer > 0)
//        {
//            ctr.Hypothermia -= 0.05f;
//            cI.heatTimer--;
//        } // ^ Used when stabbing something with an Ice Crystal or Fire Spear, in the HitSomething method of IceCrystal.cs and in this file's ElementalDamage method, respectively.


//        // This code below establishes new heat and cold sources, using the EXTREMELY convoluted stuff I set up with the BootlegIProvideWarmth stuff.
//        // If you're a newer coder, uh, don't expect to understand what's going on here at ALL.
//        // For less-newer coders:
//        // This effectively lets me assign both new AND pre-existing objects as IProvideWarmth, and with MAXIMUM CUSTOMIZABILITY!!!
//        if (ctr.room is not null)
//        {
//            if (CWT.AbsCtrData.TryGetValue(ctr.abstractCreature, out AbsCtrInfo aI) && aI.debuffs is not null)
//            {
//                for (int b = aI.debuffs.Count - 1; b >= 0; b--)
//                {
//                    Debuff debuff = aI.debuffs[b];
//                    if (debuff.duration > 0)
//                    {
//                        debuff.duration--;
//                    }
//                    else
//                    {
//                        aI.debuffs[b] = null;
//                        aI.debuffs.Remove(aI.debuffs[b]);
//                        continue;
//                    }
//                    debuff.DebuffUpdate(ctr, b == 0);
//                    debuff.DebuffVisuals(ctr, eu);
//                }
//                if (aI.debuffs.Count < 1)
//                {
//                    aI.debuffs = null;
//                }
//            }


//            if (tempSources.Contains(ctr) && ctr.dead && ((ctr is Player plr && plr.SlugCatClass == HailstormSlugcats.Incandescent) || (ctr is EggBug f && f.FireBug)))
//            {
//                tempSources.Remove(ctr);
//            }

//            if (ctr.room.blizzardGraphics is not null && cI.isIncanCampaign)
//            {
//                if (!ctr.dead)
//                {
//                    Color blizzardPixel = ctr.room.blizzardGraphics.GetBlizzardPixel((int)(ctr.mainBodyChunk.pos.x / 20f), (int)(ctr.mainBodyChunk.pos.y / 20f));
//                    ctr.HypothermiaExposure = blizzardPixel.g;
//                    if (ctr is Player self && self.SlugCatClass == HailstormSlugcats.Incandescent)
//                    {
//                        float blizzEnd = self.room.world.rainCycle.cycleLength + RainWorldGame.BlizzardHardEndTimer(self.room.game.IsStorySession);
//                        self.HypothermiaGain -= Mathf.Lerp(0f, 50f, Mathf.InverseLerp(blizzEnd, blizzEnd * 5f, self.room.world.rainCycle.timer));
//                    }
//                }
//                else
//                {
//                    ctr.HypothermiaExposure = 1f;
//                    ctr.HypothermiaGain = Mathf.Lerp(0f, 4E-05f, Mathf.InverseLerp(0.8f, 1f, ctr.room.world.rainCycle.CycleProgression));
//                }
//            }

//            // Allows Freezer Mist to rapidly chill creatures. To find where the value of freezerChill is changed, go to Hailstorm Creatures/Lizards/FreezerSpit and scroll down.
//            if (cI.freezerChill > 0 && ctr.Hypothermia > 0.001f)
//            {
//                if (ctr.abstractCreature.HypothermiaImmune)
//                {
//                    cI.freezerChill /= 4f;
//                }
//                if (ctr.room.game.IsArenaSession)
//                {
//                    cI.freezerChill *= 2f;
//                }

//                if (!ctr.abstractCreature.HypothermiaImmune)
//                {
//                    ctr.Hypothermia += cI.freezerChill / 40f;
//                }
//                if (ctr is not Player && ctr.State is HealthState hs)
//                {
//                    hs.health -= cI.freezerChill / ctr.Template.baseDamageResistance / (ctr.abstractCreature.HypothermiaImmune ? 40f : 10f);
//                }
//                else if (ctr is not Player)
//                {
//                    ctr.Die();
//                }
//                cI.freezerChill = 0;
//            }

//            // Here's where all objects that act as heat or cold sources are handled.
//            float HypoRes = 0;
//            foreach (UpdatableAndDeletable tempSource in tempSources)
//            {
//                if (tempSource is not PhysicalObject obj || obj.room != ctr.room)
//                {
//                    continue;
//                }

//                float distance = Vector2.Distance(ctr.firstChunk.pos, obj.firstChunk.pos);
//                bool receiverIsIncan = ctr is Player incCheck && incCheck.SlugCatClass == HailstormSlugcats.Incandescent;

//                // Lantern warmth changes
//                if (tempSource is Lantern lan)
//                {
//                    if (ctr.Hypothermia < 2f && lan.room == ctr.room && distance < 350)
//                    {
//                        float coldStrength = Mathf.InverseLerp(350, 70, distance);

//                        if (ctr.room.blizzardGraphics is null || ctr.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard || ctr.room.world.rainCycle.CycleProgression <= 0f)
//                        {
//                            ctr.Hypothermia -= 0.0005f * coldStrength; // Enables the warming capabilities of Lanterns at pretty much all times. 
//                        }
//                        if (ctr is Player self2 && CWT.PlayerData.TryGetValue(self2, out HailstormSlugcats inc) && inc.isIncan)
//                        {
//                            self2.Hypothermia -= -0.0003f * coldStrength;
//                            inc.lanternDryTimer++;
//                        } // ^ Weakens Lantern heat for the Incandescent, but also makes Lanterns dry them out faster.
//                    }
//                }
//                else if (tempSource is LanternMouse lanMse)
//                {
//                    if (ctr.Hypothermia < 2f && lanMse.room == ctr.room && distance < 190)
//                    {
//                        float coldStrength = Mathf.InverseLerp(190, 38, distance);

//                        if (ctr.room.blizzardGraphics is null || ctr.room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard || ctr.room.world.rainCycle.CycleProgression <= 0f)
//                        {
//                            ctr.Hypothermia -= 0.00075f * Mathf.InverseLerp(0f, 3500f, lanMse.State.battery) * coldStrength; // Enables the warming capabilities of Lanterns at pretty much all times. 
//                        }
//                        if (ctr is Player self2 && CWT.PlayerData.TryGetValue(self2, out HailstormSlugcats inc) && inc.isIncan)
//                        {
//                            self2.Hypothermia -= -0.0004f * Mathf.InverseLerp(0f, 3500f, lanMse.State.battery) * coldStrength;
//                        } // ^ Weakens Lantern Mouse heat for the Incandescent.
//                    }
//                }

//                // Heat sources
//                if (ctr.Hypothermia > 0.001f)
//                {
//                    // Incandescent heat
//                    if (tempSource is Player tempPlr && CWT.PlayerData.TryGetValue(tempPlr, out HailstormSlugcats tIP) && tIP.isIncan && tIP.incanLight is not null && (distance < tIP.incanLight.rad * 1.5f))
//                    {
//                        float heatRad = tIP.incanLight.rad * 1.5f;
//                        float heatStrength = Mathf.InverseLerp(heatRad, heatRad / 5f, distance);
//                        float heat = Mathf.Lerp(0, 0.0005f, (heatRad - 30) / 45);

//                        if (receiverIsIncan) // Weakens the heat's effectiveness for the Incandescent themselves.
//                        {
//                            heat /= 3f;
//                        }
//                        if (tIP.fireFuel > 0)
//                        {
//                            heat += 0.0001f;
//                        }

//                        ctr.Hypothermia -= heat * heatStrength;
//                    }

//                    // Firebug heat
//                    else if (tempSource is EggBug bug && bug.FireBug && distance < 150)
//                    {
//                        float heat = Mathf.Lerp(0.0009f, 0.0003f, ctr.HypothermiaExposure);
//                        float heatStrength = Mathf.InverseLerp(150, 30, distance);
//                        ctr.Hypothermia -= heat * heatStrength;
//                    }

//                    // Fire Spear heat
//                    else if (tempSource is Spear spr)
//                    {
//                        if (spr.bugSpear && distance < 120)
//                        {
//                            float heatStrength = Mathf.InverseLerp(120, 24, distance);
//                            ctr.HypothermiaExposure = Mathf.Min(ctr.HypothermiaExposure, Mathf.Lerp(1, 0.7f, distance));
//                            ctr.Hypothermia -= Mathf.Lerp(0.0003f, 0f, ctr.HypothermiaExposure) * heatStrength;
//                        }
//                        else if (spr.abstractSpear is AbstractBurnSpear incSpr && incSpr.heat > 0 && incSpr.glow is not null && distance < incSpr.glow.rad)
//                        {
//                            float heatStrength = Mathf.InverseLerp(incSpr.glow.rad, incSpr.glow.rad/5f, distance);
//                            ctr.HypothermiaExposure = Mathf.Min(ctr.HypothermiaExposure, Mathf.Lerp(1, 0.7f, distance));
//                            ctr.Hypothermia -= Mathf.Lerp(receiverIsIncan ? 0.0001f : 0.0003f, 0f, ctr.HypothermiaExposure) * heatStrength;
//                            if (!receiverIsIncan && ctr.HypothermiaGain * Mathf.Lerp(0, 0.75f, incSpr.heat) > HypoRes) HypoRes = ctr.HypothermiaGain * Mathf.Lerp(0, 0.75f, incSpr.heat);
//                        }
//                    }

//                    // Fire Egg heat
//                    else if (tempSource is FireEgg egg && distance < egg.firstChunk.rad * 3)
//                    {
//                        ctr.Hypothermia -= 0.0007f;
//                    }
//                }

//                // Freezer Lizard chill
//                if (tempSource is Lizard && HailstormLizards.LizardData.TryGetValue(tempSource as Lizard, out LizardInfo lI) && lI.isFreezerOrIcyBlue && distance < lI.auraRadius)
//                {
//                    ctr.Hypothermia -=
//                        (ctr.room.game.IsArenaSession ? -0.0048f : -0.0024f) * Mathf.InverseLerp(lI.auraRadius, lI.auraRadius - 120, distance);
//                }
//            }
//            if (HypoRes > 0)
//            {
//                ctr.Hypothermia -= HypoRes;
//            }

//            if (ctr.Hypothermia < 0f)
//            {
//                ctr.Hypothermia = 0f;
//            }  
//        }
//    }

//    public static bool HypothermiaBodyContactWarmupChanges(On.Creature.orig_HypothermiaBodyContactWarmup orig, Creature this_doesnt_do_anything, Creature ctr, Creature other)
//    {
//        bool selfIsIncan =
//            ctr is Player self && self.SlugCatClass == HailstormSlugcats.Incandescent;

//        bool otherIsIncan =
//            other is Player self2 && self2.SlugCatClass == HailstormSlugcats.Incandescent;


//        if (other.Template.type == HailstormEnums.Chillipede)
//        {
//            ctr.Hypothermia += (selfIsIncan ? 0.005f : 0.004f);
//            return true;
//        }

//        if (other.Template.type == HailstormEnums.IcyBlue || other.Template.type == HailstormEnums.Freezer)
//        {
//            ctr.Hypothermia += Mathf.Lerp(0.0018f, 0.0036f, Mathf.InverseLerp(1.4f, 1.7f, other.TotalMass)) * (selfIsIncan ? 1.25f : 1);
//            return true;
//        }

//        if ((other.Template.type == HailstormEnums.GorditoGreenie || (IsRWGIncan(other.room.game) && other.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard)) && other.Hypothermia < ctr.Hypothermia)
//        {
//            ctr.Hypothermia = Mathf.Lerp(ctr.Hypothermia, other.Hypothermia, (!other.dead? 0.0005f : 0.0002f) * (selfIsIncan ? 0.5f : 1));                
//            other.Hypothermia = Mathf.Lerp(other.Hypothermia, ctr.Hypothermia, 0.002f);
//            return true;
//        }

//        if (selfIsIncan && !otherIsIncan && (other.Hypothermia < ctr.Hypothermia || other.abstractCreature.creatureTemplate.BlizzardAdapted))
//        {
//            if (!other.abstractCreature.creatureTemplate.BlizzardAdapted)
//            {
//                ctr.Hypothermia = Mathf.Lerp(ctr.Hypothermia, other.Hypothermia, 0.0015f);
//            }
//            else
//            {
//                ctr.Hypothermia = Mathf.Lerp(ctr.Hypothermia, 0f, 0.0015f);
//            }
//            other.Hypothermia = Mathf.Lerp(other.Hypothermia, ctr.Hypothermia, 0.012f);
//            return true;
//        }

//        return orig(this_doesnt_do_anything, ctr, other);
//    }

//    public static void MinorViolenceTweaks(On.Creature.orig_Violence orig, Creature target, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float dmg, float stun)
//    {
//        if (target?.room is not null)
//        {
//            if (source?.owner is not null)
//            {
//                if (target is Player plr && plr.cantBeGrabbedCounter > 0 && (source.owner is MirosBird || (source.owner is Vulture vul && vul.IsMiros)))
//                {
//                    dmg *= 0; // Now Miros birds can't insta-gib you the millisecond you exit a pipe. (I hope)
//                    stun *= 0;
//                }

//                if (source.owner is Weapon wpn &&
//                    wpn.thrownBy is not null &&
//                    wpn.thrownBy is Player inc &&
//                    CWT.PlayerData.TryGetValue(inc, out HailstormSlugcats hs) &&
//                    hs.isIncan &&
//                    dirAndMomentum.HasValue &&
//                    dirAndMomentum.Value.y > Mathf.Abs(dirAndMomentum.Value.x) * 3)
//                    // If the source of damage is a weapon thrown upwards by the Incandescent...
//                {
//                    stun += 30; // ...the target gets almost an extra second of stun.
//                }
//            }
//            if (IsRWGIncan(target.room.game))
//            {
//                if (target.abstractCreature.Winterized)
//                {
//                    if (target is Cicada)
//                    {
//                        dmg *= 1.34f; // ~0.75x HP
//                        stun *= (dmgType == Creature.DamageType.Blunt) ? 1.5f : 1.2f;
//                    }
//                    else if (target is DropBug)
//                    {
//                        dmg *= 0.5f; // 2x HP
//                    }
//                    else if (target is PoleMimic)
//                    {
//                        dmg *= 0.7f; // ~1.43x HP
//                    }
//                    else if (target.Template.type == CreatureTemplate.Type.BigSpider)
//                    {
//                        dmg *= 0.5f; // 1.6x HP
//                    }
//                }
//            }
//        }
//        orig(target, source, dirAndMomentum, hitChunk, hitAppen, dmgType, dmg, stun);
//    }

//    public static void CreatureDeathChanges(On.Creature.orig_Die orig, Creature ctr)
//    {
//        if (ctr is not null && ctr is Lizard liz && liz.Template.type == HailstormEnums.GorditoGreenie && liz.animation != Lizard.Animation.Standard)
//        {
//            liz.EnterAnimation(Lizard.Animation.Standard, forceAnimationChange: true);
//        }
//        orig(ctr);
//    }

//    public static Player.ObjectGrabability GrababilityChanges(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
//    {
//        if (IsRWGIncan(self?.abstractCreature.world?.game) && obj is Cicada ccd && ccd.abstractCreature.Winterized && !ccd.Charging && (ccd.cantPickUpCounter == 0 || ccd.cantPickUpPlayer != self))
//        {
//            return Player.ObjectGrabability.OneHand;
//        }
//        if (obj is Luminescipede lmn && (lmn.dead || (SlugcatStats.SlugcatCanMaul(self.SlugCatClass) && self.dontGrabStuff < 1 && obj != self && !lmn.Consious)))
//        {
//            return Player.ObjectGrabability.OneHand;
//        }
//        return orig(self, obj);
//    }

//    public static void ActivateCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature absCtr)
//    {
//        orig(absCtr);
//        if (absCtr is not null && (
//            IsRWGIncan(absCtr.world?.game) ||
//            absCtr.creatureTemplate.type == HailstormEnums.InfantAquapede ||
//            absCtr.creatureTemplate.type == HailstormEnums.IcyBlue ||
//            absCtr.creatureTemplate.type == HailstormEnums.Freezer ||
//            absCtr.creatureTemplate.type == HailstormEnums.Cyanwing ||
//            absCtr.creatureTemplate.type == HailstormEnums.GorditoGreenie ||
//            absCtr.creatureTemplate.type == HailstormEnums.Luminescipede ||
//            absCtr.creatureTemplate.type == HailstormEnums.Chillipede))
//        {
//            CustomFlags(absCtr);
//        }
//    }

//    public static void HailstormCreatureFlagChecks()
//    {
//        IL.AbstractCreature.InDenUpdate += IL =>
//        {
//            ILCursor c = new(IL);
//            ILLabel? label = IL.DefineLabel();
//            c.Emit(OpCodes.Ldarg_0);
//            c.Emit(OpCodes.Ldarg_1);
//            c.EmitDelegate((AbstractCreature absCtr, int time) =>
//            {
//                return HailstormDenUpdate(absCtr, time);
//            });
//            c.Emit(OpCodes.Brfalse_S, label);
//            c.Emit(OpCodes.Ret);
//            c.MarkLabel(label);

//            ILCursor c1 = new(IL);
//            if (c1.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractWorldEntity>(nameof(AbstractWorldEntity.world)),
//                x => x.MatchLdfld<World>(nameof(World.rainCycle)),
//                x => x.MatchLdfld<RainCycle>(nameof(RainCycle.maxPreTimer)),
//                x => x.MatchLdcI4(0),
//                x => x.MatchCgt()))
//            {
//                c1.Emit(OpCodes.Ldarg_0);
//                c1.EmitDelegate((bool flag, AbstractCreature absCtr) => flag && !(IsRWGIncan(absCtr?.world.game) && StoryChanges.FoggyCycle));
//                                                                                // Prevents normal Precycle creatures from spawning if a special precycle type becomes active in the Incandescent's campaign.
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook to IL.AbstractCreature.InDenUpdate ain't workin'.");
//        };

//        IL.AbstractCreature.WantToStayInDenUntilEndOfCycle += IL =>
//        {
//            ILCursor c1 = new(IL);
//            if (c1.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.ignoreCycle))))
//            {
//                c1.Emit(OpCodes.Ldarg_0);
//                c1.EmitDelegate((bool flag, AbstractCreature absCtr) => flag && (!IsRWGIncan(absCtr?.world?.game) || !CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI) || !aI.LateBlizzardRoamer));
//                                                                                // Allows late blizzard roamers to stay outside of their dens in the Incandescent's campaign.
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #1 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");

//            ILCursor c2 = new(IL);
//            ILLabel? label = null;
//            if (c2.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
//                x => x.MatchIsinst<HealthState>(),
//                x => x.MatchCallvirt<HealthState>("get_health"),
//                x => x.MatchLdcR4(0.6f),
//                x => x.MatchBgeUn(out label)))
//            {
//                c2.Emit(OpCodes.Ldarg_0);
//                c2.EmitDelegate((AbstractCreature absCtr) => absCtr.creatureTemplate.type != HailstormEnums.Cyanwing);
//                c2.Emit(OpCodes.Brfalse, label);
//                // Cyanwings will not go back to their dens if they're low on health.
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #2 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");

//            ILCursor c3 = new(IL);
//            if (c3.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.preCycle))))
//            {
//                c3.Emit(OpCodes.Ldarg_0);
//                c3.EmitDelegate((bool flag, AbstractCreature absCtr) => flag || (IsRWGIncan(absCtr?.world.game) && CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI) && aI.FogRoamer));
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #3 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");

//            ILCursor c4 = new(IL);
//            if (c4.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractWorldEntity>(nameof(AbstractWorldEntity.world)),
//                x => x.MatchLdfld<World>(nameof(World.rainCycle)),
//                x => x.MatchLdfld<RainCycle>(nameof(RainCycle.maxPreTimer)),
//                x => x.MatchLdcI4(0)//,
//                //x => x.MatchBgt(out label)
//                ))
//            {
//                //Debug.LogError(label.Target.ToString() + " | " + label.Branches.ToString());
//                c4.Emit(OpCodes.Ldarg_0);
//                c4.EmitDelegate((bool flag, AbstractCreature absCtr) => flag || (IsRWGIncan(absCtr?.world.game) && ((absCtr.preCycle && StoryChanges.FoggyCycle) || (CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI) && aI.FogRoamer && !StoryChanges.FoggyCycle))));
//                //c4.Emit(OpCodes.Brfalse, label);
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #4 to IL.AbstractCreature.WantToStayInDenUntilEndOfCycle is absolutely not working.");
//        };

//        IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle += IL =>
//        {
//            ILCursor c1 = new(IL);
//            if (c1.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractCreatureAI>(nameof(AbstractCreatureAI.parent)),
//                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.ignoreCycle))))
//            {
//                c1.Emit(OpCodes.Ldarg_0);
//                c1.EmitDelegate((bool flag, AbstractCreatureAI absCtrAI) => flag && !(IsRWGIncan(absCtrAI?.parent?.world?.game) && CWT.AbsCtrData.TryGetValue(absCtrAI.parent, out AbsCtrInfo aI) && aI.LateBlizzardRoamer));
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #1 to IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle is absolutely not working.");

//            ILCursor c2 = new(IL);
//            ILLabel? label = null;
//            if (c2.TryGotoNext(
//                MoveType.After,
//                x => x.MatchLdarg(0),
//                x => x.MatchLdfld<AbstractCreatureAI>(nameof(AbstractCreatureAI.parent)),
//                x => x.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.state)),
//                x => x.MatchIsinst(nameof(HealthState)),
//                x => x.MatchCallvirt<HealthState>("get_health"),
//                x => x.MatchLdcR4(0.75f),
//                x => x.MatchBgeUn(out label)))
//            {
//                c2.Emit(OpCodes.Ldarg_0);
//                c2.EmitDelegate((AbstractCreatureAI absCtrAI) => absCtrAI.parent.creatureTemplate.type != HailstormEnums.Cyanwing);
//                c2.Emit(OpCodes.Brfalse, label);
//            }
//            else
//                Debug.LogError("[Hailstorm] Hook #2 to IL.AbstractCreatureAI.WantToStayInDenUntilEndOfCycle is absolutely not working.");
//        };
//    }


//    //--------------------------------------

//    public static void CustomFlags(AbstractCreature absCtr)
//    {

//        if (absCtr.spawnData is not null && absCtr.spawnData[0] == '{' && CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI))
//        {
//            if (aI.LateBlizzardRoamer) aI.LateBlizzardRoamer = false;
//            if (aI.FogRoamer) aI.FogRoamer = false;

//            string[] array = absCtr.spawnData.Substring(1, absCtr.spawnData.Length - 2).Split(new char[1] { ',' });
//            for (int i = 0; i < array.Length; i++)
//            {
//                if (array[i].Length < 1) continue;

//                switch (array[i].Split(new char[1] { ':' })[0])
//                {
//                    case "Ignorecycle":
//                        absCtr.ignoreCycle = true;
//                        break;

//                    case "Night":
//                        absCtr.nightCreature = true;
//                        absCtr.ignoreCycle = false;
//                        break;
//                    case "LateBlizzardRoamer":
//                        aI.LateBlizzardRoamer = true;
//                        absCtr.nightCreature = true;
//                        absCtr.ignoreCycle = false;
//                        break;

//                    case "PreCycle":
//                        absCtr.preCycle = true;
//                        break;
//                    case "FogRoamer":
//                        aI.FogRoamer = true;
//                        absCtr.preCycle = false;
//                        break;

//                    case "AlternateForm":
//                        absCtr.superSizeMe = true;
//                        break;
//                    case "Winter":
//                        absCtr.Winterized = true;
//                        break;
//                    case "Seed":
//                        absCtr.ID.setAltSeed(int.Parse(array[i].Split(new char[1] { ':' })[1], NumberStyles.Any, CultureInfo.InvariantCulture));
//                        absCtr.personality = new AbstractCreature.Personality(absCtr.ID);
//                        break;
//                }
//            }
//            if (aI.FogRoamer && absCtr.Room.shelter)
//            {
//                if (RainWorld.ShowLogs)
//                {
//                    Debug.Log("[HAILSTORM] " + absCtr + "'s fog-roamer flag disabled, creature started with player in the shelter!");
//                }
//                aI.FogRoamer = false;
//            }
//        }

//        CreatureTemplate.Type ctrType = absCtr.creatureTemplate.type;

//        string regName =
//            absCtr.world.game.IsStorySession ?
//            absCtr.world.region.name : "";

//        if (!absCtr.creatureTemplate.BlizzardAdapted && !absCtr.Winterized)
//        {
//            if ((regName != "OE" &&
//                    (ctrType == CreatureTemplate.Type.Spider ||
//                    ctrType == CreatureTemplate.Type.BigSpider ||
//                    ctrType == CreatureTemplate.Type.SpitterSpider ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
//                    ctrType == CreatureTemplate.Type.Scavenger ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing ||
//                    ctrType == CreatureTemplate.Type.Vulture ||
//                    ctrType == CreatureTemplate.Type.KingVulture)) ||
//                    (regName != "UG" && regName != "OE" && (
//                    ctrType == CreatureTemplate.Type.CicadaA ||
//                    ctrType == CreatureTemplate.Type.CicadaB ||
//                    ctrType == CreatureTemplate.Type.JetFish ||
//                    ctrType == CreatureTemplate.Type.Hazer ||
//                    ctrType == CreatureTemplate.Type.PoleMimic ||
//                    ctrType == CreatureTemplate.Type.TentaclePlant ||
//                    ctrType == CreatureTemplate.Type.DropBug ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
//                    (absCtr.creatureTemplate.ancestor == StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate) && ctrType != CreatureTemplate.Type.BlueLizard))))
//            {
//                absCtr.Winterized = true;
//            }
//        }

//        if (ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech)
//        {
//            absCtr.HypothermiaImmune = false;
//        }
//        else if (regName == "UG" || absCtr.creatureTemplate.BlizzardAdapted || absCtr.Winterized)
//        {
//            absCtr.HypothermiaImmune = true;
//        }

//        if (regName == "SI" && absCtr.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede)
//        {
//            absCtr.superSizeMe = true;
//        }

//        if (!absCtr.nightCreature &&
//                (absCtr.creatureTemplate.BlizzardWanderer ||
//                regName == "UG" ||
//                ctrType == CreatureTemplate.Type.Vulture ||
//                ctrType == CreatureTemplate.Type.KingVulture ||
//                    (absCtr.Winterized && (
//                    ctrType == CreatureTemplate.Type.CicadaA ||
//                    ctrType == CreatureTemplate.Type.CicadaB ||
//                    ctrType == CreatureTemplate.Type.GreenLizard ||
//                    ctrType == CreatureTemplate.Type.WhiteLizard ||
//                    ctrType == CreatureTemplate.Type.BlackLizard ||
//                    ctrType == CreatureTemplate.Type.RedLizard ||
//                    ctrType == CreatureTemplate.Type.Spider ||
//                    ctrType == CreatureTemplate.Type.BigSpider ||
//                    ctrType == CreatureTemplate.Type.SpitterSpider ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
//                    ctrType == CreatureTemplate.Type.DropBug ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
//                    ctrType == MoreSlugcatsEnums.CreatureTemplateType.Yeek))))
//        {
//            absCtr.ignoreCycle = true;
//        }

//    }

//    public static bool HailstormDenUpdate(AbstractCreature absCtr, int time)
//    {
//        if (IsRWGIncan(absCtr?.world.game) && CWT.AbsCtrData.TryGetValue(absCtr, out AbsCtrInfo aI) && (aI.FogRoamer || aI.LateBlizzardRoamer))
//        {
//            return !(absCtr.remainInDenCounter > -1 && ((aI.FogRoamer && StoryChanges.FoggyCycle && absCtr.world.rainCycle.maxPreTimer > 0) || (aI.LateBlizzardRoamer && absCtr.world.rainCycle.timer >= absCtr.world.rainCycle.baseCycleLength + 24000)));
//        }
//        return false;
//    }


//    #endregion


//}


//#region Slime Mold Devtool Stuff
//public class AbstractSlimeMold : AbstractConsumable
//{
//    public bool big;

//    public AbstractSlimeMold(World world, AbstractObjectType objType, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData slmData, bool big)
//        : base(world, objType, realizedObject, pos, ID, originRoom, placedObjectIndex, slmData)
//    {
//        this.big = big;
//    }

//    public string BaseToString()
//    {
//        return string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, big ? 1 : 0);
//    }

//    public override string ToString()
//    {
//        return SaveUtils.AppendUnrecognizedStringAttrs(BaseToString(), "<oA>", unrecognizedAttributes);
//    }
//}

//public class SlimeMoldData : PlacedObject.ConsumableObjectData
//{
//    public bool big;

//    public SlimeMoldData(PlacedObject owner) : base(owner)
//    {
//    }

//    public override void FromString(string s)
//    {
//        base.FromString(s);
//        string[] array = Regex.Split(s, "~");
//        if (array.Length >= 5)
//        {
//            big = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
//            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
//        }
//    }

//    public override string ToString()
//    {
//        return SaveUtils.AppendUnrecognizedStringAttrs(BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}", big ? 1 : 0), "~", unrecognizedAttributes);
//    }
//}
//#endregion
