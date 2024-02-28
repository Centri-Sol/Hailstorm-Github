namespace Hailstorm;

internal class HailstormVultures
{

    public static void Hooks()
    {

        On.Vulture.ctor += HailstormVulturesSetup;
        On.Vulture.Violence += VultureViolence;

        On.KingTusks.Tusk.ctor += KingtuskCWT;
        On.KingTusks.Tusk.Update += KingtuskDeflectMomentum;
        On.KingTusks.WantToShoot += ErraticWindTuskWait;
        ReallyDumbVultureHooks();

        On.Vulture.JawSlamShut += AuroricMirosShortcutProtection;
        On.Vulture.Update += AuroricMirosLaser;
        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += AuroricMirosAggro;

        On.VultureGraphics.ctor += HailstormVulFeathers;
        On.VultureGraphics.InitiateSprites += HailstormVultureSprites;
        On.VultureGraphics.BeakGraphic.InitiateSprites += AuroricMirosBeakMesh;
        On.VultureGraphics.AddToContainer += AuroricMirosSpriteLayering;
        On.VultureGraphics.ApplyPalette += HailstormVulturePalettes;
        On.VultureGraphics.DrawSprites += HailstormVultureVisuals;
        On.KingTusks.Tusk.DrawSprites += FlurryKingLaserColor;

        On.Vulture.VultureThruster.StartSmoke += HailstormVultureSmokeColoring1;
        On.KingTusks.Tusk.Shoot += HailstormVultureSmokeColoring2;

        On.Vulture.DropMask += HailstormVultureMaskColoring;
        On.MoreSlugcats.VultureMaskGraphics.DrawSprites += HailstormVulMaskDrawSprites;

    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == IncanInfo.Incandescent);
    }

    //---------------------------------------

    public static ConditionalWeakTable<Vulture, VultureInfo> VulData = new();
    public static bool AddVultureToCWT(Vulture vul, AbstractCreature absVul, World world)
    {
        if (vul.Template.type == HailstormCreatures.Raven)
        {
            return true;
        }
        if (IsIncanStory(world.game) && (
                vul.Template.type == CreatureTemplate.Type.KingVulture ||
                vul.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture))
        {
            return true;
        }
        if (vul.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture && HSRemix.AuroricMirosEverywhere.Value)
        {
            return true;
        }
        return false;
    }
    public static void HailstormVulturesSetup(On.Vulture.orig_ctor orig, Vulture vul, AbstractCreature absVul, World world)
    {
        orig(vul, absVul, world);
        if (!VulData.TryGetValue(vul, out _) && AddVultureToCWT(vul, absVul, world))
        {
            VulData.Add(vul, new VultureInfo(vul));
        }
        if (!VulData.TryGetValue(vul, out VultureInfo vi))
        {
            return;
        }

        Random.InitState(vul.abstractCreature.ID.RandomSeed);
        Random.State state = Random.state;

        vi.albino = Random.value < (
            vi.Miros ? 0.001f :
            vi.King  ? 0.003f :
                       0.009f);

        if (world.region is not null &&
            world.region.name == "OE" &&
            vi.Miros)
        {
            vi.albino = !vi.albino;
        }

        if (vi.Raven)
        {
            vul.neck.idealLength *= 0.7f;
            for (int b = 0; b < vul.bodyChunks.Length; b++)
            {
                vul.bodyChunks[b].rad *= 0.7f;
                vul.bodyChunks[b].mass *= 0.7f;
            }
            for (int b = 0; b < vul.bodyChunkConnections.Length; b++)
            {
                vul.bodyChunkConnections[b].distance *= 0.7f;
            }
        }
        else if (vi.Miros)
        {
            float hue = Random.Range(200 / 360f, 280 / 360f);
            float bri = !vi.albino ? 0.5f : 0.9f;
            vi.ColorB = new HSLColor(hue, 0.2f, bri);

            if (!vi.albino)
            {
                hue = vi.ColorB.hue + (Random.value < 0.5f ? -20 / 360f : 20 / 360f);
                if (hue < 200 / 360f)
                {
                    hue += 80 / 360f;
                }
                else if (hue > 280 / 360f)
                {
                    hue -= 80 / 360f;
                }
            }
            else
            {
                hue = Random.Range(0.9f, 1.1f);
            }
            vi.ColorA = new HSLColor(hue, 0.2f, 0.3f);

            if (!vi.albino)
            {
                hue = Random.Range(0.3f, 0.9f);
            }
            bri = hue / (!vi.albino ? 3f : Random.Range(5f, 9f));
            vi.eyeCol = new HSLColor(hue, 0.75f - bri, Custom.WrappedRandomVariation(0.55f + bri, 0.1f, 0.5f));

            vi.smokeCol1 = vi.eyeCol;
            hue = !vi.albino ? Random.Range(0.3f, 0.9f) : vi.smokeCol1.hue;
            bri = hue / 3f;
            vi.smokeCol2 = new HSLColor(hue, 0.75f - bri, Custom.WrappedRandomVariation(0.55f + bri, 0.1f, 0.5f));
        }
        Random.state = state;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------
    // General Vulture stuff
    public static void VultureViolence(On.Vulture.orig_Violence orig, Vulture vul, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos appPos, Creature.DamageType dmgType, float dmg, float bonusStun)
    {

        bool ActivateNewLaser = false;

        if (vul is not null && VulData.TryGetValue(vul, out VultureInfo vi))
        {
            if (vi.Miros)
            {
                dmg *= 2f; // HP: 20 -> 10
                bonusStun /= 3f;
                if (dmgType == Creature.DamageType.Explosion)
                {
                    dmg /= 4f;
                }
                else
                if (dmgType == Creature.DamageType.Electric || dmgType == Creature.DamageType.Water ||
                    dmgType == HailstormDamageTypes.Heat || dmgType == HailstormDamageTypes.Cold)
                {
                    dmg /= 2f;
                }
                ActivateNewLaser = vul.laserCounter < 1;
            }
            else if (vi.King)
            {
                if (dmgType == HailstormDamageTypes.Cold)
                {
                    dmg /= 2f;
                    bonusStun /= 2f;
                }

                float antidiscouragement = ((dmg * 0.75f) + (bonusStun / 345f)) * 0.3f;

                if (vul.room.game.IsStorySession &&
                    vul.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                {
                    antidiscouragement *= 1.5f;
                }

                vul.AI.disencouraged -= antidiscouragement;
            }
        }

        orig(vul, source, dirAndMomentum, hitChunk, appPos, dmgType, dmg, bonusStun);

        if (vul is null || !VulData.TryGetValue(vul, out VultureInfo vI))
        {
            return;
        }

        if (ActivateNewLaser && vul.laserCounter > 0)
        {
            vul.laserCounter = (int)(vul.laserCounter * (vI.albino ? 0.8f : 1.2f));
        }

        if (vI.King && dmg > 0.5f)
        {
            vul.LoseAllGrasps();
        }
    }
    public static void ReallyDumbVultureHooks()
    {
        IL.KingTusks.Tusk.ShootUpdate += IL =>
        {
            ILCursor c = new(IL);
            ILLabel? label = IL.DefineLabel();
            _ = c.Emit(OpCodes.Ldarg_0);
            _ = c.Emit(OpCodes.Ldarg_1);
            _ = c.EmitDelegate((KingTusks.Tusk tusk, float speed) =>
            {
                return KingtuskTargetIsArmored(tusk, speed);
            });
            _ = c.Emit(OpCodes.Brfalse_S, label);
            _ = c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
        IL.VultureTentacle.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<VultureTentacle>(nameof(VultureTentacle.stun)),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out label)))
            {
                _ = c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((VultureTentacle wing) => HailstormVultureWingResizing(wing));
                _ = c.Emit(OpCodes.Brfalse, label);
            }
            else
                Plugin.logger.LogError("[Hailstorm] An IL hook for Auroric Miros Vultures exploded! And by that I mean it stopped working! Tell me about this, please.");
        };
    }
    public static bool HailstormVultureWingResizing(VultureTentacle wing)
    {
        if (wing?.vulture is not null &&
            VulData.TryGetValue(wing.vulture, out VultureInfo vi))
        {
            if (vi.Raven)
            {
                wing.idealLength = Mathf.Lerp(98, 154, wing.flyingMode);
            }
            else if (vi.Miros)
            {
                wing.idealLength = Mathf.Lerp(70, 110, wing.flyingMode);
            }
        }
        return true;
    }
    public static bool KingtuskTargetIsArmored(KingTusks.Tusk tusk, float speed)
    {
        Vector2 val = tusk.chunkPoints[0, 0] + tusk.shootDir * 20f;
        Vector2 pos = tusk.chunkPoints[0, 0] + tusk.shootDir * (20f + speed);
        FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(tusk.room, val, pos);
        SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(tusk, tusk.room, val, ref pos, 5f, 1, tusk.owner.vulture, hitAppendages: false);
        if (TuskData.TryGetValue(tusk, out TuskInfo tI) && !floatRect.HasValue && collisionResult.chunk?.owner is not null && collisionResult.chunk.owner is Lizard liz && liz.LizardState is ColdLizState lS && lS.armored)
        {
            int stun = (liz.Template.type == HailstormCreatures.Freezer) ? 0 : 30;
            tusk.mode = KingTusks.Tusk.Mode.Dangling;
            tI.bounceOffSpeed = tusk.shootDir * speed * 0.5f;
            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.room.PlaySound(SoundID.Spear_Bounce_Off_Wall, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, liz.bodyChunks[1].pos, 1.5f, 0.75f);
            liz.Violence(tusk.head, tusk.shootDir, liz.bodyChunks[1], null, Creature.DamageType.Stab, 2, stun);
            return true;
        }
        return false;
    }

    //---------------------------------------
    // King-specific
    public static ConditionalWeakTable<KingTusks.Tusk, TuskInfo> TuskData = new();
    public static void KingtuskCWT(On.KingTusks.Tusk.orig_ctor orig, KingTusks.Tusk tusk, KingTusks owner, int side)
    {
        orig(tusk, owner, side);
        if (!TuskData.TryGetValue(tusk, out _))
        {
            TuskData.Add(tusk, new TuskInfo(tusk));
        }
        
    }
    public static void KingtuskDeflectMomentum(On.KingTusks.Tusk.orig_Update orig, KingTusks.Tusk tusk)
    {
        if (tusk?.owner?.vulture is not null &&
            tusk.owner.vulture.IsKing &&
            VulData.TryGetValue(tusk.owner.vulture, out _))
        {
            if (tusk.mode == KingTusks.Tusk.Mode.StuckInCreature && tusk.impaleChunk?.owner is not null && tusk.impaleChunk.owner is Creature victim && !victim.dead)
            {
                tusk.SwitchMode(KingTusks.Tusk.Mode.Retracting);
            }
            else if (tusk.mode == KingTusks.Tusk.Mode.Dangling || tusk.mode == KingTusks.Tusk.Mode.StuckInWall)
            {
                if (tusk.mode == KingTusks.Tusk.Mode.StuckInWall && tusk.room is not null)
                {
                    tusk.room.PlaySound(SoundID.King_Vulture_Tusk_Bounce_Off_Terrain, tusk.chunkPoints[0, 0]);
                }
                tusk.SwitchMode(KingTusks.Tusk.Mode.Retracting);
            }
            else if (tusk.mode == KingTusks.Tusk.Mode.Retracting)
            {
                if (tusk.currWireLength > 0f)
                {
                    tusk.currWireLength = Mathf.Max(0f, tusk.currWireLength - KingTusks.Tusk.maxWireLength/45f);
                }
            }
        }

        if (tusk is not null && TuskData.TryGetValue(tusk, out TuskInfo tI))
        {
            if (tusk.mode == KingTusks.Tusk.Mode.ShootingOut && tI.bounceOffSpeed != new Vector2(0, 0))
            {
                tI.bounceOffSpeed = new Vector2(0, 0);
            }

            orig(tusk);

            if (tusk.mode == KingTusks.Tusk.Mode.Dangling && tI.bounceOffSpeed != new Vector2(0, 0))
            {
                for (int t = 0; t < tusk.chunkPoints.GetLength(0); t++)
                {
                    tusk.chunkPoints[t, 2] -= tI.bounceOffSpeed;
                    SharedPhysics.TerrainCollisionData cd = tusk.scratchTerrainCollisionData.Set(tusk.chunkPoints[t, 0], tusk.chunkPoints[t, 1], tusk.chunkPoints[t, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
                    cd = SharedPhysics.VerticalCollision(tusk.room, cd);
                    cd = SharedPhysics.HorizontalCollision(tusk.room, cd);
                    tusk.chunkPoints[t, 0] = cd.pos;
                    tusk.chunkPoints[t, 2] = cd.vel;
                    tI.bounceOffSpeed = Vector2.Lerp(tI.bounceOffSpeed, new Vector2(0, 0), 0.066f);
                    if (cd.contactPoint.x != 0f)
                    {
                        tI.bounceOffSpeed.x *= 0.5f;
                    }
                    if (cd.contactPoint.y != 0f)
                    {
                        tI.bounceOffSpeed.y *= 0.5f;
                    }
                }
            }
        }
        else orig(tusk);

        if (Weather.ErraticWindCycle && Weather.ExtremeWindIntervals[Weather.WindInterval] && tusk?.room?.blizzardGraphics is not null && tusk.mode == KingTusks.Tusk.Mode.Dangling)
        {
            Color blizzardPixel;
            Vector2 blizzPush;
            for (int t = 0; t < tusk.chunkPoints.GetLength(0); t++)
            {
                Vector2 tuskPos = tusk.chunkPoints[t, 2];
                blizzardPixel = tusk.room.blizzardGraphics.GetBlizzardPixel((int)(tuskPos.x / 20f), (int)(tuskPos.y / 20f));

                blizzPush = new Vector2(0f - tusk.room.blizzardGraphics.WindAngle, 0.1f) * 0.75f;
                blizzPush *= blizzardPixel.g * (5f * tusk.room.blizzardGraphics.WindStrength);

                if (tusk.room.waterInverted && tuskPos.y > tusk.room.FloatWaterLevel(tuskPos.x))
                {
                    break;
                }
                else if (!MMF.cfgVanillaExploits.Value && tusk.room.FloatWaterLevel(tuskPos.x) > (tusk.room.abstractRoom.size.y + 20) * 20f)
                {
                    break;
                }
                else if (tusk.room.FloatWaterLevel(tuskPos.x) > tuskPos.y)
                {
                    break;
                }

                blizzPush.y *= -0.2f;

                SharedPhysics.TerrainCollisionData cd = tusk.scratchTerrainCollisionData.Set(tusk.chunkPoints[t, 0], tusk.chunkPoints[t, 1], tusk.chunkPoints[t, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
                cd = SharedPhysics.VerticalCollision(tusk.room, cd);
                cd = SharedPhysics.HorizontalCollision(tusk.room, cd);

                if (cd.contactPoint.y != 0)
                {
                    blizzPush.x *= 0.33f;
                }
                else
                {
                    blizzPush.x *= Mathf.Lerp(4f, 2f, Mathf.InverseLerp(0.35f, 1.6f, Weather.WindIntervalIntensities[Weather.WindInterval]));
                }

                tusk.chunkPoints[t, 2] += blizzPush;

            }
        }
    }
    public static bool ErraticWindTuskWait(On.KingTusks.orig_WantToShoot orig, KingTusks kt, bool checkVisualOnAnyTargetChunk, bool checkMinDistance)
    {
        if (Weather.ErraticWindCycle &&
            Weather.ExtremeWindIntervals[Weather.WindInterval])
        {
            return false;
        }
        return orig(kt, checkVisualOnAnyTargetChunk, checkMinDistance);
    }

    //---------------------------------------
    // Miros-specific
    public static void AuroricMirosLaser(On.Vulture.orig_Update orig, Vulture vul, bool eu)
    {
        orig(vul, eu);
        if (vul?.room is null || !VulData.TryGetValue(vul, out VultureInfo vi))
        {
            return;
        }

        if (vi.King)
        {
            vi.wingGlowFadeTimer += 
               (vul.AI.behavior == VultureAI.Behavior.Hunt ||
                vul.AI.behavior == VultureAI.Behavior.ReturnPrey ||
                vul.AI.behavior == VultureAI.Behavior.EscapeRain) ? 2 : 1;

            if (vi.wingGlowFadeTimer > 180)
            {
                vi.wingGlowFadeTimer = -20;
            }
        }

        if (!vi.Miros)
        {
            return;
        }

        if (vul.laserCounter > 0)
        {
            if (vul.graphicsModule is not null && vul.graphicsModule is VultureGraphics vg && vg.soundLoop is not null)
            {
                vg.soundLoop.Pitch *= 1 + Mathf.InverseLerp(50, 10, vul.laserCounter);
                vg.soundLoop.Volume *= 1.25f;
            }

            if (!vul.dead && vul.laserCounter == 11)
            {
                vul.room.AddObject(new Explosion(
                    room: vul.room,
                    sourceObject: vul, 
                    pos: vul.mainBodyChunk.pos,
                    lifeTime: 7,
                    rad: vi.albino ? 160 : 240,
                    force: 7,
                    damage: vi.albino ? 1.25f : 2.5f,
                    stun: 240,
                    deafen: 0,
                    killTagHolder: vul,
                    killTagHolderDmgFactor: 0,
                    minStun: 120,
                    backgroundNoise: 0.5f));
                vul.room.AddObject(new Explosion.ExplosionLight(vul.mainBodyChunk.pos, 280, 1, 7, vi.eyeCol.rgb));
                vul.room.AddObject(new Explosion.ExplosionLight(vul.mainBodyChunk.pos, 230, 1, 3, Color.white));
                vul.room.AddObject(new ShockWave(vul.mainBodyChunk.pos, 360, 0.05f, 5));
                vul.room.AddObject(new MirosBomb(
                    startingPos: vul.Head().pos,
                    baseVel: vi.laserAngle * (vi.albino ? 26f : 24f),
                    bombCreator: vul,
                    bombColor: vi.eyeCol.rgb));
                vul.room.PlaySound(SoundID.Firecracker_Burn, vul.Head().pos);

                vul.LaserLight?.Destroy();
                vul.laserCounter = 0;
            }
            if (vi.currentPrey is not null)
            {
                if (vul.LaserLight is null)
                {
                    vul.LaserLight = new(vi.currentPrey.mainBodyChunk.pos, false, Custom.HSL2RGB(Mathf.Lerp(80 / 360f, 0, vul.LaserLight.alpha), 1, 0.5f), vul)
                    {
                        affectedByPaletteDarkness = 0,
                        submersible = true
                    };
                    vul.room.AddObject(vul.LaserLight);
                    vul.LaserLight.HardSetRad(300 - vul.laserCounter);
                    vul.LaserLight.HardSetAlpha(Mathf.InverseLerp(400, 40, vul.laserCounter));
                }
                else
                {
                    vul.LaserLight.HardSetPos(vi.currentPrey.mainBodyChunk.pos);
                    vul.LaserLight.HardSetRad(300 - vul.laserCounter);
                    vul.LaserLight.HardSetAlpha(Mathf.InverseLerp(400, 40, vul.laserCounter));
                    vul.LaserLight.color = Custom.HSL2RGB(Mathf.Lerp(80 / 360f, 0, vul.LaserLight.alpha), 1, 0.5f);
                    if (vul.LaserLight.affectedByPaletteDarkness != 0)
                    {
                        vul.LaserLight.affectedByPaletteDarkness = 0;
                    }
                    if (!vul.LaserLight.submersible)
                    {
                        vul.LaserLight.submersible = true;
                    }
                }
            }
        }

        if (vul.dead)
        {
            if (vi.currentPrey is not null)
            {
                vi.currentPrey = null;
            }
            return;
        }

        if (vul.laserCounter > 0 && vul.AI?.preyTracker?.MostAttractivePrey is not null)
        {
            Tracker.CreatureRepresentation mostAttractivePrey = vul.AI.preyTracker.MostAttractivePrey;
            if (mostAttractivePrey.TicksSinceSeen < 40 && mostAttractivePrey.representedCreature?.realizedCreature is not null)
            {
                vi.currentPrey = mostAttractivePrey.representedCreature.realizedCreature;
            }
            else vi.currentPrey = null;
        }
        else if (vi.currentPrey is not null)
        {
            vi.currentPrey = null;
        }

        if (HSRemix.ScissorhawkEagerBirds.Value && vul.laserCounter == 0 && vul.landingBrake == 1)
        {
            vul.laserCounter = 200;
        }
    }
    public static void AuroricMirosShortcutProtection(On.Vulture.orig_JawSlamShut orig, Vulture vul)
    {
        if (vul?.room?.abstractRoom?.creatures is not null &&
            vul.bodyChunks is not null &&
            VulData.TryGetValue(vul, out VultureInfo vi) &
            vi.Miros)
        {
            for (int i = 0; i < vul.room.abstractRoom.creatures.Count; i++)
            {
                if (vul.grasps[0] is not null)
                {
                    break;
                }
                Creature ctr = vul.room.abstractRoom.creatures[i].realizedCreature;
                if (vul.room.abstractRoom.creatures[i] == vul.abstractCreature ||
                    !vul.AI.DoIWantToBiteCreature(vul.room.abstractRoom.creatures[i]) ||
                    ctr is null ||
                    ctr is not Player plr ||
                    plr.cantBeGrabbedCounter <= 0 ||
                    plr.enteringShortCut.HasValue ||
                    plr.inShortcut)
                {
                    continue;
                }
                for (int j = 0; j < ctr.bodyChunks.Length; j++)
                {
                    if (vul.grasps[0] is not null)
                    {
                        break;
                    }
                    Vector2 headDirection = Custom.DirVec(vul.neck.Tip.pos, vul.Head().pos);
                    if (!Custom.DistLess(vul.Head().pos + headDirection * 20f, ctr.bodyChunks[j].pos, 20f + ctr.bodyChunks[j].rad) || !vul.room.VisualContact(vul.Head().pos, ctr.bodyChunks[j].pos))
                    {
                        continue;
                    }
                    _ = vul.Grab(ctr, 0, j, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1, overrideEquallyDominant: true, pacifying: false);
                    break;
                }
            }
        }
        orig(vul);
    }
    public static CreatureTemplate.Relationship AuroricMirosAggro(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI vultureAI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        // Causes Miros Vultures to attack and eat ANYTHING that isn't flagged as inedible to them
        if (vultureAI?.vulture is not null &&
            VulData.TryGetValue(vultureAI.vulture, out VultureInfo vi) &&
            vi.Miros)
        {
            CreatureTemplate.Type ctrType = dynamRelat.trackerRep.representedCreature.creatureTemplate.type;

            bool inedibleForMirosVultures =
                ctrType == CreatureTemplate.Type.Overseer ||
                ctrType == CreatureTemplate.Type.Leech ||
                ctrType == CreatureTemplate.Type.SeaLeech ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.JungleLeech ||
                ctrType == CreatureTemplate.Type.Vulture ||
                ctrType == CreatureTemplate.Type.KingVulture ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture ||
                ctrType == CreatureTemplate.Type.Deer ||
                ctrType == CreatureTemplate.Type.GarbageWorm ||
                ctrType == CreatureTemplate.Type.Fly ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug ||
                ctrType == CreatureTemplate.Type.TempleGuard;

            bool dangerousToMirosVultures =
                ctrType == CreatureTemplate.Type.BigEel ||
                ctrType == CreatureTemplate.Type.DaddyLongLegs ||
                ctrType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs;


            if (vultureAI?.vulture?.killTag is not null && vultureAI.vulture.killTag == dynamRelat.trackerRep.representedCreature && !dynamRelat.trackerRep.representedCreature.state.dead && !dangerousToMirosVultures && !inedibleForMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, 1.33f);
            }
            if (ctrType == CreatureTemplate.Type.PoleMimic ||
                ctrType == CreatureTemplate.Type.TentaclePlant)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Attacks, 0.5f);
            }
            if (ctrType == CreatureTemplate.Type.BrotherLongLegs)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Ignores, 1);
            }
            if (dangerousToMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, (ctrType == CreatureTemplate.Type.BigEel) ? 0.33f : 0.5f);
            }
            if (!inedibleForMirosVultures)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, dynamRelat.trackerRep.representedCreature.state.dead ? 1 : 0.9f);
            }
        }
        return orig(vultureAI, dynamRelat);
    }

    //---------------------------------------
    // Graphics
    public static void HailstormVulFeathers(On.VultureGraphics.orig_ctor orig, VultureGraphics vg, Vulture owner)
    {
        orig(vg, owner);
        if (vg?.vulture is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi))
        {
            return;
        }

        Random.InitState(vg.vulture.abstractCreature.ID.RandomSeed);
        Random.State state = Random.state;
        if (vi.Raven)
        {
            vg.feathersPerWing = Random.Range(8, 12);
        }
        else if (vi.King)
        {
            vg.feathersPerWing = Random.Range(12, 20);
        }
        else if (vi.Miros)
        {
            vg.feathersPerWing = Random.value < 0.15f ? 5 : 4;
            vg.beakFatness = Mathf.Pow(Random.value, 0.5f);
            int sprite = vg.FirstBeakSprite();
            for (int s = 0; s < vg.beak.Length; s++)
            {
                vg.beak[s] = new VultureGraphics.BeakGraphic(vg, s, sprite);
                sprite += vg.beak[s].totalSprites;
            }
            vg.eyeTrail.sprite = vg.EyeTrailSprite();
        }

        vg.wings = new VultureFeather[vg.vulture.tentacles.Length, vg.feathersPerWing];
        float featherDamageFac = (Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, Random.value);
        float shriveledFeatherFac = (Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, Random.value); // I'll be honest, I don't actually understand what effect these have.
        float brokenColFac = (Random.value < 0.5f) ? 20f : Mathf.Lerp(3f, 6f, Random.value);
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                float featherFac = (f + 0.5f) / vg.feathersPerWing;
                float wingPos = Mathf.Lerp(1f - Mathf.Pow(vg.IsMiros ? 0.95f : 0.89f, f), Mathf.Sqrt(featherFac), 0.5f);
                wingPos = Mathf.InverseLerp(0.1f, 1.1f, wingPos);
                if (vg.IsMiros && f == vg.feathersPerWing - 1)
                {
                    wingPos = 0.8f;
                }
                vg.wings[w, f] = new VultureFeather(vg,
                    vg.vulture.tentacles[w],
                    wingPos,
                    VultureTentacle.FeatherContour(featherFac, vg.IsMiros ? 0.25f : 0) * Mathf.Lerp(vg.IsMiros ? 90f : 50f, vg.IsMiros ? 120f : 75f, Random.value),
                    VultureTentacle.FeatherContour(featherFac, 1) * Mathf.Lerp(vg.IsMiros ? 120f : 65f, vg.IsMiros ? 150f : 75f, Random.value) * (vg.IsKing ? 1.3f : 1f),
                    Mathf.Lerp(vg.IsMiros ? 5f : 3f, vg.IsMiros ? 8f : 6f, VultureTentacle.FeatherWidth(featherFac)));
                bool RNG = Random.value < 0.025f;
                if (Random.value < 1 / featherDamageFac || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].lose = 1f - Random.value * Random.value * Random.value;
                    if (Random.value < 0.4f)
                    {
                        vg.wings[w, f].brokenColor = 1f - Random.value * Random.value;
                    }
                }
                if (Random.value < 1f / shriveledFeatherFac)
                {
                    vg.wings[w, f].extendedLength /= 5f;
                    vg.wings[w, f].contractedLength = vg.wings[w, f].extendedLength;
                    vg.wings[w, f].brokenColor = 1f;
                    vg.wings[w, f].width /= 1.7f;
                }
                if (Random.value < 0.025f || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].contractedLength = vg.wings[w, f].extendedLength * 0.7f;
                }
                if (Random.value < 1f / brokenColFac || (RNG && Random.value < 0.5f))
                {
                    vg.wings[w, f].brokenColor = (Random.value < 0.5f) ? 1f : Random.value;
                }
            }
        }
        

        Random.state = state;

    }
    public static void HailstormVultureSprites(On.VultureGraphics.orig_InitiateSprites orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(vg, sLeaser, rCam);
        if (vg?.vulture is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi))
        {
            return;
        }

        if (vi.Raven)
        {
            sLeaser.sprites[vg.BodySprite].scale = 0.7f;
            for (int t = 0; t < vg.tusks.Length; t++)
            {
                sLeaser.sprites[vg.TuskSprite(t)].scale = 0.7f;
            }
        }
        else if (vi.Miros)
        {
            for (int w = 0; w < vg.vulture.tentacles.Length; w++)
            {
                for (int f = 0; f < vg.feathersPerWing; f++)
                {
                    if (f == vg.feathersPerWing - 1)
                    {
                        sLeaser.sprites[vg.FeatherSprite(w, f)].anchorX = 0.15f;
                        sLeaser.sprites[vg.FeatherSprite(w, f)].anchorY = 0.50f;
                    }
                    else
                    {
                        sLeaser.sprites[vg.FeatherSprite(w, f)].anchorY = 0.98f;
                        sLeaser.sprites[vg.FeatherColorSprite(w, f)].anchorY = 1f;
                    }
                }
            }
        }

        sLeaser.sprites[vg.NeckSprite] = TriangleMesh.MakeLongMesh(vg.vulture.neck.tChunks.Length, pointyTip: false, customColor: true);
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            sLeaser.sprites[vg.TentacleSprite(w)] = TriangleMesh.MakeLongMesh(vg.vulture.tentacles[w].tChunks.Length, pointyTip: false, customColor: true);
        }
    }
    public static void AuroricMirosBeakMesh(On.VultureGraphics.BeakGraphic.orig_InitiateSprites orig, VultureGraphics.BeakGraphic bg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(bg, sLeaser, rCam);
        if (bg?.owner?.vulture is null || !VulData.TryGetValue(bg.owner.vulture, out VultureInfo vi) || !vi.Miros)
        {
            return;
        }
        TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[11];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
        }
        sLeaser.sprites[bg.firstSprite] = new TriangleMesh("Futile_White", array, customColor: true);
    }
    public static void AuroricMirosSpriteLayering(On.VultureGraphics.orig_AddToContainer orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(vg, sLeaser, rCam, newContainer);
        if (vg?.vulture is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi) || !vi.Miros)
        {
            return;
        }

        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                sLeaser.sprites[vg.FeatherSprite(w, f)].MoveBehindOtherNode(wing);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].MoveBehindOtherNode(wing);
            }
        }

        for (int b = vg.FirstBeakSprite(); b <= vg.LastBeakSprite(); b++)
        {
            sLeaser.sprites[b].MoveBehindOtherNode(sLeaser.sprites[vg.HeadSprite]);
        }

    }
    public static void HailstormVulturePalettes(On.VultureGraphics.orig_ApplyPalette orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(vg, sLeaser, rCam, palette);
        if (vg?.vulture?.room is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi))
        {
            return;
        }

        Random.InitState(vg.vulture.abstractCreature.ID.RandomSeed);
        Random.State state = Random.state;
        if (vi.Raven)
        {
            Color.RGBToHSV(Color.Lerp(vg.palette.blackColor, vg.palette.fogColor, 0.1f), out float h, out float s, out float l);
            vi.ColorA = new HSLColor(h, s, l);
            Color.RGBToHSV(vg.palette.fogColor, out h, out s, out l);
            vi.ColorB = new HSLColor(h, s, l);
            if (vi.albino)
            {
                (vi.ColorB, vi.ColorA) = (vi.ColorA, vi.ColorB);
            }
            vi.eyeCol = vi.ColorB;
            vi.featherColor1 = vi.ColorA;
            h = !vi.albino ? Random.Range(240 / 360f, 320 / 360f) : 0;
            l = h / 3f;
            vi.featherColor2 = new HSLColor(h, 0.6f - l, 0.7f + l);
            if (!vi.albino && Random.value < 0.5f)
            {
                vi.featherColor2 = HSLColor.Lerp(vi.featherColor2, vi.featherColor1, Random.value / 2f);
            }
            vi.smokeCol1 = vi.featherColor2;
            vi.smokeCol2 = vi.ColorB;
        }
        else if (vi.King)
        {
            Color.RGBToHSV(Color.Lerp(vg.palette.blackColor, vg.palette.fogColor, !vi.albino ? 0.5f : 1f), out float h, out float s, out float l);
            vi.ColorB = new HSLColor(h, s, l);
            vi.ColorA = !vi.albino ?
                new HSLColor(Random.Range(0.475f, 0.525f), Random.Range(0.50f, 0.60f), Random.Range(0.35f, 0.45f)) :
                new HSLColor(Random.Range(0.860f, 0.940f), Random.Range(0.25f, 0.33f), Random.Range(0.60f, 0.70f));
            vi.eyeCol = new HSLColor(vi.ColorA.hue, (!vi.albino ? 1f : 0.6f), (!vi.albino ? 0.5f : 0.75f));
            vi.wingColor = vi.ColorA;
            vi.featherColor1 = vi.eyeCol;
            vi.featherColor2 = new HSLColor(vi.ColorA.hue, 0.75f, 0.75f);
            vi.MiscColor = vi.ColorB;
            vi.smokeCol1 = !vi.albino ?
                    new HSLColor(Random.Range(170 / 360f, 210 / 360f), 0.4f, 0.8f) :
                    new HSLColor(Random.Range(240 / 360f, 280 / 360f), 0.4f, 0.8f);
            float hue = vi.smokeCol1.hue + (Random.value < 0.5f ? -20 / 360f : 20 / 360f);
            if (hue < (!vi.albino ? 170 / 360f : 240 / 360f)) hue += 40 / 360f;
            if (hue > (!vi.albino ? 240 / 360f : 280 / 360f)) hue -= 40 / 360f;
            vi.smokeCol2 = new HSLColor(hue, vi.smokeCol1.saturation + 0.4f, vi.smokeCol1.lightness - 0.4f);
        }
        else if (vi.Miros)
        {
            vi.featherColor1 = vi.eyeCol;

            float h = !vi.albino ? Random.Range(0.3f, 0.7f) : Random.Range(280/360f, 560/360f);
            float l = h / (!vi.albino ? 3f : 10f);
            vi.featherColor2 = new HSLColor(h, 0.75f - l, Custom.WrappedRandomVariation(0.55f + l, 0.1f, 0.5f));
            if (Random.value < 0.5f)
            {
                vi.featherColor2 = HSLColor.Lerp(vi.featherColor2, vi.featherColor1, Random.value);
            }

            vi.wingColor = Random.value < 0.5f ?
                new HSLColor(12 / 360f, Random.value * 0.25f, Random.Range(0.1f, 0.25f)) :
                new HSLColor(12 / 360f, Random.value * 0.15f, Random.Range(0.1f, 0.85f));
            Color.RGBToHSV(Color.Lerp(vi.wingColor.rgb, vg.palette.blackColor, 0.5f), out h, out float s, out l);
            vi.MiscColor = new HSLColor(h, s, l);
        }
        Random.state = state;

        vg.albino = vi.albino;
        vg.ColorA = vi.ColorA;
        vg.ColorB = vi.ColorB;
        vg.eyeCol = vi.eyeCol.rgb;

    }

    public static void HailstormVultureVisuals(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(vg, sLeaser, rCam, timeStacker, camPos);
        if (vg?.vulture?.room is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi))
        {
            return;
        }
        
        if (!vi.Miros && !vg.shadowMode)
        {
            if (vi.King)
            {
                FlurryKingColors(vg, vi, sLeaser, rCam);
            }
            else if (vi.Raven)
            {
                RavenColors(vg, vi, sLeaser);
            }
        }
        else if (vi.Miros && !vg.culled)
        {
            if (!vg.shadowMode)
            {
                AuroricMirosColors(vg, vi, sLeaser);
            }

            float alpha = Mathf.InverseLerp(320, 40, vg.vulture.laserCounter);
            sLeaser.sprites[vg.LaserSprite()].isVisible = vg.vulture.laserCounter > 0 && alpha > 0f;

            if (!sLeaser.sprites[vg.LaserSprite()].isVisible)
            {
                return;
            }

            Color laserCol = vi.eyeCol.rgb;
            sLeaser.sprites[vg.LaserSprite()].alpha = alpha;
            CustomFSprite Laser = sLeaser.sprites[vg.LaserSprite()] as CustomFSprite;
            Laser.verticeColors[0] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[1] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[2] = Custom.RGB2RGBA(laserCol, alpha);
            Laser.verticeColors[3] = Custom.RGB2RGBA(laserCol, alpha);

            if (vi.currentPrey is null)
            {
                return;
            }

            Vector2 headPos = Vector2.Lerp(vg.vulture.Head().lastPos, vg.vulture.Head().pos, timeStacker);
            Vector2 endOfNeckPos = Custom.DirVec(Vector2.Lerp(vg.vulture.neck.Tip.lastPos, vg.vulture.neck.Tip.pos, timeStacker), headPos);
            vi.laserAngle =
                vi.currentPrey is not null ? Custom.DirVec(headPos, vi.currentPrey.mainBodyChunk.pos) :
                sLeaser.sprites[vg.LaserSprite()].isVisible ? Custom.DirVec((sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).vertices[0], (sLeaser.sprites[vg.LaserSprite()] as CustomFSprite).vertices[1]) :
                Custom.DirVec(endOfNeckPos, headPos);

            Laser.MoveVertice(0, headPos - (endOfNeckPos * 4f) + (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(1, headPos - (endOfNeckPos * 4f) - (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(2, vi.currentPrey.mainBodyChunk.pos - (endOfNeckPos * 4f) - (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
            Laser.MoveVertice(3, vi.currentPrey.mainBodyChunk.pos - (endOfNeckPos * 4f) + (Custom.PerpendicularVector(endOfNeckPos) * 0.5f) - camPos);
        }
    }
    public static void RavenColors(VultureGraphics vg, VultureInfo vi, RoomCamera.SpriteLeaser sLeaser)
    {
        float darkness = vg.darkness;
        Color blackColor = vg.palette.blackColor;
        //----Body----//
        for (int b = 0; b < sLeaser.sprites.Length; b++)
        {
            sLeaser.sprites[b].color = Color.Lerp(vi.ColorB.rgb, blackColor, darkness);
        }
        //----Neck and Head----//
        TriangleMesh neck = sLeaser.sprites[vg.NeckSprite] as TriangleMesh;
        for (int n = 0; n < neck.verticeColors.Length; n++)
        {
            float lerp = Mathf.InverseLerp(neck.verticeColors.Length * 0.25f, neck.verticeColors.Length * 0.75f, n);
            neck.verticeColors[n] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, lerp), blackColor, darkness);
        }
        sLeaser.sprites[vg.HeadSprite].color = vi.ColorA.rgb;
        sLeaser.sprites[vg.EyesSprite].color = vg.eyeCol;
        for (int t = 0; t < vg.tusks.Length; t++)
        {
            sLeaser.sprites[vg.TuskSprite(t)].color = vi.ColorA.rgb;
        }
        //----Wings----//
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int t = 0; t < wing.verticeColors.Length; t++)
            {
                float wingColorFac = Mathf.InverseLerp(wing.verticeColors.Length * 0.7f, wing.verticeColors.Length * 0.9f, t);
                wing.verticeColors[t] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, wingColorFac), blackColor, darkness);
            }
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                sLeaser.sprites[vg.FeatherSprite(w, f)].color = Color.Lerp(vi.ColorB.rgb, blackColor, darkness);
                float lerpReversePoint = (vg.feathersPerWing - 1)/3f;
                float featherColorFac = (f < lerpReversePoint) ?
                    Mathf.InverseLerp(0, lerpReversePoint, f) :
                    Mathf.InverseLerp(lerpReversePoint, vg.feathersPerWing - 1, f);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].color = HSLColor.Lerp(vi.featherColor1, vi.featherColor2, featherColorFac).rgb;
            }
        }
        //----Back Shields----//
        for (int s = 0; s < 2; s++)
        {
            sLeaser.sprites[vg.BackShieldSprite(s)].color = Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, 0.75f);
            sLeaser.sprites[vg.FrontShieldSprite(s)].color = vi.ColorA.rgb;
        }
        //----Danglies----//
        for (int d = 0; d < 2; d++)
        {
            TriangleMesh dangly = sLeaser.sprites[vg.AppendageSprite(d)] as TriangleMesh;
            for (int k = 0; k < dangly.verticeColors.Length; k++)
            {
                float danglyColorFac = Mathf.InverseLerp(dangly.verticeColors.Length * 0.25f, dangly.verticeColors.Length * 0.75f, k);
                dangly.verticeColors[k] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, danglyColorFac), blackColor, darkness);
            }
        }
    }
    public static void FlurryKingColors(VultureGraphics vg, VultureInfo vi, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        float darkness = vg.darkness;
        Color blackColor = vg.palette.blackColor;
        //----Body----//
        for (int b = 0; b < sLeaser.sprites.Length; b++)
        {
            sLeaser.sprites[b].color = Color.Lerp(vi.ColorB.rgb, blackColor, darkness);
        }
        //----Head----//
        sLeaser.sprites[vg.EyesSprite].color = vg.eyeCol;
        if (vg.vulture.kingTusks is not null)
        {
            vg.vulture.kingTusks.ApplyPalette(vg, vg.palette, vi.ColorA.rgb, sLeaser, rCam);
            sLeaser.sprites[vg.MaskSprite].color = Color.Lerp(vi.ColorA.rgb, blackColor, darkness);
            sLeaser.sprites[vg.MaskArrowSprite].color = Color.Lerp(vi.MiscColor.rgb, blackColor, darkness);
        }
        //----Wings----//
        float wingGlowFac = vi.wingGlowFadeTimer/160f;
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {
            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int t = 0; t < wing.verticeColors.Length; t++)
            {
                float wingDistFac = Mathf.InverseLerp(0, wing.verticeColors.Length -1, t);
                float glowFac = Mathf.InverseLerp(0.125f, 0, Mathf.Abs(wingDistFac - wingGlowFac));
                wing.verticeColors[t] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, blackColor, darkness), vi.wingColor.rgb, glowFac);
            }
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                float featherDistFac = Mathf.InverseLerp(0, vg.feathersPerWing - 1, f);
                float glowFac = Mathf.InverseLerp(0.125f, 0, Mathf.Abs(featherDistFac - wingGlowFac));
                sLeaser.sprites[vg.FeatherSprite(w, f)].color = Color.Lerp(Color.Lerp(vi.ColorB.rgb, blackColor, darkness), vi.featherColor1.rgb, glowFac);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].color = HSLColor.Lerp(vi.featherColor1, vi.featherColor2, glowFac).rgb;
            }
        }
        //----Back Shields----//
        for (int s = 0; s < 2; s++)
        {
            sLeaser.sprites[vg.BackShieldSprite(s)].color = Color.Lerp(vi.ColorA.rgb, blackColor, darkness/2f);
            sLeaser.sprites[vg.FrontShieldSprite(s)].color = Color.Lerp(vi.eyeCol.rgb, blackColor, darkness/2f);
        }
        //----Danglies----//
        for (int d = 0; d < 2; d++)
        {
            TriangleMesh dangly = sLeaser.sprites[vg.AppendageSprite(d)] as TriangleMesh;
            for (int k = 0; k < dangly.verticeColors.Length; k++)
            {
                float danglyColorFac = Mathf.InverseLerp(dangly.verticeColors.Length * 0.25f, dangly.verticeColors.Length * 0.75f, k);
                dangly.verticeColors[k] = Color.Lerp(HSLColor.Lerp(vi.ColorB, vi.eyeCol, danglyColorFac).rgb, blackColor, darkness);
            }
        }
    }
    public static void FlurryKingLaserColor(On.KingTusks.Tusk.orig_DrawSprites orig, KingTusks.Tusk tusk, VultureGraphics vg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(tusk, vg, sLeaser, rCam, timeStacker, camPos);
        if (vg?.vulture?.room is null || !VulData.TryGetValue(vg.vulture, out VultureInfo vi) || !vi.King)
        {
            return;
        }

        if (sLeaser.sprites[tusk.LaserSprite(vg)].isVisible)
        {
            float alpha = Mathf.Lerp(tusk.lastLaserAlpha, tusk.laserAlpha, timeStacker);
            Color eyeCol = vi.eyeCol.rgb;
            CustomFSprite laser = sLeaser.sprites[tusk.LaserSprite(vg)] as CustomFSprite;
            laser.verticeColors[0] = Custom.RGB2RGBA(eyeCol, alpha);
            laser.verticeColors[1] = Custom.RGB2RGBA(eyeCol, alpha);
            laser.verticeColors[2] = Custom.RGB2RGBA(eyeCol, Mathf.Pow(alpha, 2f) * (tusk.mode == KingTusks.Tusk.Mode.Charging ? 1f : 0.5f));
            laser.verticeColors[3] = Custom.RGB2RGBA(eyeCol, Mathf.Pow(alpha, 2f) * (tusk.mode == KingTusks.Tusk.Mode.Charging ? 1f : 0.5f));
        }
    }
    public static void AuroricMirosColors(VultureGraphics vg, VultureInfo vi, RoomCamera.SpriteLeaser sLeaser)
    {
        float darkness = vg.darkness;
        Color blackColor = vg.palette.blackColor;
        //----Body----//
        for (int b = 0; b < sLeaser.sprites.Length; b++)
        {
            if (b != vg.EyeTrailSprite())
            {
                sLeaser.sprites[b].color = Color.Lerp(vi.ColorB.rgb, blackColor, darkness);
            }
        }
        //----Neck and Head----//
        TriangleMesh neck = sLeaser.sprites[vg.NeckSprite] as TriangleMesh;
        for (int n = 0; n < neck.verticeColors.Length; n++)
        {
            float lerp = Mathf.InverseLerp(neck.verticeColors.Length * 0.2f, neck.verticeColors.Length * 0.8f, n);
            neck.verticeColors[n] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, lerp), blackColor, darkness);
        }
        sLeaser.sprites[vg.HeadSprite].color = Color.Lerp(vi.ColorA.rgb, blackColor, darkness);
        sLeaser.sprites[vg.EyesSprite].color = Color.Lerp(vg.eyeCol, vg.palette.fogColor, darkness);
        for (int b = vg.FirstBeakSprite(); b <= vg.LastBeakSprite(); b++)
        {
            sLeaser.sprites[b].color = Color.Lerp(vi.MiscColor.rgb, blackColor, darkness);
        }
        //----Wings----//
        for (int w = 0; w < vg.vulture.tentacles.Length; w++)
        {

            TriangleMesh wing = sLeaser.sprites[vg.TentacleSprite(w)] as TriangleMesh;
            for (int t = 0; t < wing.verticeColors.Length; t++)
            {
                float wingColorFac = Mathf.InverseLerp(0, wing.verticeColors.Length * 0.4f, t);
                wing.verticeColors[t] = Color.Lerp(HSLColor.Lerp(vi.ColorB, vi.wingColor, wingColorFac).rgb, blackColor, darkness);
            }
            for (int f = 0; f < vg.feathersPerWing; f++)
            {
                float featherColorFac = Mathf.InverseLerp(0, vg.feathersPerWing - 1, f);
                sLeaser.sprites[vg.FeatherSprite(w, f)].color = Color.Lerp(vi.wingColor.rgb, blackColor, darkness);
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].color = HSLColor.Lerp(vi.featherColor1, vi.featherColor2, featherColorFac).rgb;
                sLeaser.sprites[vg.FeatherColorSprite(w, f)].isVisible = true;
            }
        }
        //----Back Shields----//
        for (int s = 0; s < 2; s++)
        {
            sLeaser.sprites[vg.BackShieldSprite(s)].color = Color.Lerp(vi.ColorA.rgb, blackColor, darkness);
            sLeaser.sprites[vg.FrontShieldSprite(s)].color = Color.Lerp(vi.ColorA.rgb, blackColor, 0.25f + (darkness * 0.75f));
        }
        //----Danglies----//
        for (int d = 0; d < 2; d++)
        {
            TriangleMesh dangly = sLeaser.sprites[vg.AppendageSprite(d)] as TriangleMesh;
            for (int k = 0; k < dangly.verticeColors.Length; k++)
            {
                float danglyColorFac = Mathf.InverseLerp(0, dangly.verticeColors.Length, k);
                dangly.verticeColors[k] = Color.Lerp(Color.Lerp(vi.ColorB.rgb, vi.ColorA.rgb, danglyColorFac), blackColor, darkness);
            }
        }
    }

    public static void HailstormVultureSmokeColoring1(On.Vulture.VultureThruster.orig_StartSmoke orig, Vulture.VultureThruster thruster)
    {
        orig(thruster);
        if (thruster?.vulture is null || !VulData.TryGetValue(thruster.vulture, out VultureInfo vi))
        {
            return;
        }
        thruster.smoke = null;
        thruster.smoke = new HailstormVultureSmoke(thruster.vulture.room, thruster.ExhaustPos, thruster.vulture, vi.smokeCol1.rgb, vi.smokeCol2.rgb);
    }
    public static void HailstormVultureSmokeColoring2(On.KingTusks.Tusk.orig_Shoot orig, KingTusks.Tusk tusk, Vector2 tuskHangPos)
    {
        if (tusk?.owner?.vulture is null || !VulData.TryGetValue(tusk.vulture, out VultureInfo vi))
        {
            orig(tusk, tuskHangPos);
            return;
        }

        if (tusk.vulture.room.BeingViewed && !tusk.vulture.room.PointSubmerged(tusk.head.pos) && tusk.owner.smoke is null)
        {
            tusk.owner.smoke = new HailstormVultureSmoke(tusk.vulture.room, tusk.head.pos, tusk.vulture, vi.smokeCol1.rgb, vi.smokeCol2.rgb);
            tusk.room.AddObject(tusk.owner.smoke);
        }
        orig(tusk, tuskHangPos);
    }


    // Mask Colors
    public static void HailstormVultureMaskColoring(On.Vulture.orig_DropMask orig, Vulture vul, Vector2 violenceDir)
    {
        if (vul?.room is not null &&
            VulData.TryGetValue(vul, out VultureInfo vi) &&
            vul.State is not null &&
            vul.State is Vulture.VultureState vs &&
            vs.mask)
        {
            vs.mask = false;
            if (vul.killTag is not null)
            {
                SocialMemory.Relationship relationship = vul.State.socialMemory.GetOrInitiateRelationship(vul.killTag.ID);
                relationship.like = -1f;
                relationship.tempLike = -1f;
                relationship.know = 1f;
            }
            AbstractPhysicalObject absMask = new VultureMask.AbstractVultureMask(vul.room.world, null, vul.abstractPhysicalObject.pos, vul.room.game.GetNewID(), vul.abstractCreature.ID.RandomSeed, vi.King);
            vul.room.abstractRoom.AddEntity(absMask);
            absMask.pos = vul.abstractCreature.pos;
            absMask.RealizeInRoom();

            VultureMask mask = absMask.realizedObject as VultureMask;
            mask.firstChunk.HardSetPosition(vul.bodyChunks[4].pos);
            mask.firstChunk.vel = vul.bodyChunks[4].vel + violenceDir;
            if (vi.King)
            {
                mask.maskGfx.ColorA = vi.ColorA;
                mask.maskGfx.ColorB = vi.MiscColor;
            }
            else if (vi.Raven)
            {
                mask.maskGfx.ColorA = vi.ColorB;
                mask.maskGfx.ColorB = vi.ColorA;
            }
            mask.fallOffVultureMode = 1f;
        }
        orig(vul, violenceDir);
    }
    public static void HailstormVulMaskDrawSprites(On.MoreSlugcats.VultureMaskGraphics.orig_DrawSprites orig, VultureMaskGraphics mg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(mg, sLeaser, rCam, timeStacker, camPos);
        if (mg?.attachedTo?.room is not null &&
            mg.attachedTo is Vulture vul &&
            VulData.TryGetValue(vul, out _))
        {
            Vector2 pos = Vector2.zero;
            if (mg.overrideDrawVector.HasValue)
            {
                pos = mg.overrideDrawVector.Value;
            }
            else
            {
                pos = Vector2.Lerp(mg.attachedTo.firstChunk.lastPos, mg.attachedTo.firstChunk.pos, timeStacker);
            }
            float darkness = rCam.room.Darkness(pos) * (1 - rCam.room.LightSourceExposure(pos)) * 0.8f * (1 - mg.fallOffVultureMode);
            Color maskColor = Color.Lerp(Color.Lerp(mg.ColorA.rgb, Color.white, 0.35f * mg.fallOffVultureMode), mg.blackColor, Mathf.Lerp(0.2f, 1, Mathf.Pow(darkness, 2f)));
            sLeaser.sprites[mg.firstSprite].color = maskColor;
            sLeaser.sprites[mg.firstSprite + 1].color = Color.Lerp(maskColor, mg.blackColor, Mathf.Lerp(0.75f, 1, darkness));
            sLeaser.sprites[mg.firstSprite + 2].color = Color.Lerp(maskColor, mg.blackColor, Mathf.Lerp(0.75f, 1, darkness));
            if (mg.King)
            {
                sLeaser.sprites[mg.firstSprite + 3].color = Color.Lerp(mg.ColorB.rgb, mg.blackColor, darkness);
            }
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

}

public class MirosBomb : UpdatableAndDeletable, IDrawable
{
    //----------------------------------------------------------------------------------

    public Vector2 lastPos;
    public Vector2 pos;
    public Vector2 vel;
    private Color color;
    private Color explodeColor;
    private readonly float rad;
    private Vector2 rotation;
    public float burn;
    public float Submersion
    {
        get
        {
            if (room is null)
            {
                return 0f;
            }
            if (room.waterInverted)
            {
                return 1f - Mathf.InverseLerp(pos.y - rad, pos.y + rad, room.FloatWaterLevel(pos.x));
            }
            float floatWaterLvl = room.FloatWaterLevel(pos.x);
            if (!MMF.cfgVanillaExploits.Value && floatWaterLvl > (room.abstractRoom.size.y + 20) * 20)
            {
                return 1f;
            }
            return Mathf.InverseLerp(pos.y - rad, pos.y + rad, floatWaterLvl);
        }
    }

    public PhysicalObject source;

    public float[] spikes;
    public Smoke.BombSmoke smoke;

    //----------------------------------------------------------------------------------

    public MirosBomb(Vector2 startingPos, Vector2 baseVel, PhysicalObject bombCreator, Color bombColor)
    {
        lastPos = startingPos;
        vel = baseVel;
        pos = startingPos + baseVel;
        source = bombCreator;
        explodeColor = bombColor;
        rotation = Custom.RNV();
        rad = 4;
        spikes = new float[Random.Range(3, 8)];
        for (int i = 0; i < spikes.Length; i++)
        {
            spikes[i] = (i + Random.value) * (360f / (float)spikes.Length);
        }
    }

    //---------------------------------------

    public override void Update(bool eu)
    {
        if (Submersion > 0)
        {
            vel.y = Mathf.Lerp(vel.y, 0, Submersion / 100f);
            vel.x = Mathf.Lerp(vel.x, 0, Submersion / 200f);
        }
        if (vel.y + vel.x < 8 && burn == 0)
        {
            burn += 1/30f;
        }
        int updates = Mathf.Max(1, Mathf.CeilToInt(vel.magnitude / rad));
        for (int m = 0; m < updates; m++)
        {
            lastPos = pos;
            pos += vel / (float)updates;

            Vector2 outerRad = pos + vel.normalized * rad;
            FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, outerRad, pos - vel.normalized * rad);
            Vector2 terrainTraceArea = default;
            if (floatRect.HasValue)
            {
                terrainTraceArea = new(floatRect.Value.left, floatRect.Value.bottom);
            }
            SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos, ref pos, rad, 1, source, false);
            if (floatRect.HasValue && collisionResult.chunk is not null)
            {
                if (Vector2.Distance(outerRad, terrainTraceArea) < Vector2.Distance(outerRad, collisionResult.collisionPoint))
                {
                    collisionResult.chunk = null;
                }
                else
                {
                    floatRect = null;
                }
            }
            if (floatRect.HasValue ||
                (collisionResult.chunk?.owner is not null && (collisionResult.chunk.owner is not Creature ctr || ctr.Template.type != MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)))
            {
                Explode(collisionResult.chunk);
            }
        }

        if (burn > 0f)
        {
            if (Submersion > 0 && !room.waterObject.WaterIsLethal)
            {
                burn = 0f;
            }
            for (int i = 0; i < 3; i++)
            {
                room.AddObject(new Spark(Vector2.Lerp(lastPos, pos, Random.value), vel * 0.1f + Custom.RNV() * 3.2f * Random.value, explodeColor, null, 7, 30));
            }
            if (smoke is null)
            {
                smoke = new Smoke.BombSmoke(room, pos, null, explodeColor);
                room.AddObject(smoke);
            }
        }
        else
        {
            smoke?.Destroy();
            smoke = null;
        }

        if (burn > 0f || !room.IsPositionInsideBoundries(room.GetTilePosition(pos)))
        {
            burn += 1/30f;
            if (burn > 1)
            {
                Explode(null);
            }
        }
        if (burn <= 0f)
        {
            Explode(null);
        }

        base.Update(eu);
    }
    public void Explode(BodyChunk hitChunk)
    {
        if (slatedForDeletetion)
        {
            return;
        }
        Creature killtag = null;
        if (source is not null and Creature)
        {
            killtag = source as Creature;
        }
        room.AddObject(new SootMark(room, pos, 100, bigSprite: true));
        room.AddObject(new Explosion(room, source, pos, 2, 200, 7, 2.5f, 240, 0, killtag, 0, 120, 0.5f));
        room.AddObject(new Explosion.ExplosionLight(pos, 400, 1, 7, explodeColor));
        room.AddObject(new Explosion.ExplosionLight(pos, 400, 1, 3, Color.white));
        room.AddObject(new ExplosionSpikes(room, pos, 16, 32, 12, 8, 200, explodeColor));
        room.AddObject(new ShockWave(pos, 360, 0.05f, 6));
        for (int i = 0; i < 25; i++)
        {
            Vector2 angle = Custom.RNV();
            if (room.GetTile(pos + angle * 20f).Solid)
            {
                angle = (room.GetTile(pos - angle * 20f).Solid ? Custom.RNV() : (angle * -1f));
            }
            for (int j = 0; j < 3; j++)
            {
                room.AddObject(new Spark(pos + angle * Mathf.Lerp(30f, 60f, Random.value), angle * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
            }
            room.AddObject(new Explosion.FlashingSmoke(pos + angle * 40f * Random.value, angle * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
        }
        if (smoke is not null)
        {
            for (int k = 0; k < 8; k++)
            {
                smoke.EmitWithMyLifeTime(pos + Custom.RNV(), Custom.RNV() * Random.value * 17f);
            }
        }
        for (int l = 0; l < 6; l++)
        {
            room.AddObject(new ScavengerBomb.BombFragment(pos, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
        }
        room.ScreenMovement(pos, default, 1.3f);
        room.PlaySound(SoundID.Bomb_Explode, pos);
        room.InGameNoise(new Noise.InGameNoise(pos, 9000, null, 1));
        bool smokeTime = hitChunk is not null;
        for (int n = 0; n < 5; n++)
        {
            if (room.GetTile(pos + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
            {
                smokeTime = true;
                break;
            }
        }
        if (smokeTime)
        {
            if (smoke is null)
            {
                smoke = new Smoke.BombSmoke(room, pos, null, explodeColor);
                room.AddObject(smoke);
            }
            if (hitChunk is not null)
            {
                smoke.chunk = hitChunk;
            }
            else
            {
                smoke.chunk = null;
                smoke.fadeIn = 1f;
            }
            smoke.pos = pos;
            smoke.stationary = true;
            smoke.DisconnectSmoke();
        }
        else
        {
            smoke?.Destroy();
        }
        Destroy();
    }

    //---------------------------------------

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[spikes.Length + 4];
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel")
            {
                scaleX = Mathf.Lerp(1, 2, Mathf.Pow(Random.value, 1.8f)),
                scaleY = Mathf.Lerp(4, 7, Random.value)
            };
        }
        for (int j = 0; j < spikes.Length; j++)
        {
            sLeaser.sprites[2 + j] = new FSprite("pixel")
            {
                scaleX = Mathf.Lerp(2, 3, Random.value),
                scaleY = Mathf.Lerp(5, 7, Random.value),
                anchorY = 0f
            };
        }
        sLeaser.sprites[spikes.Length + 2] = new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
            scale = (rad + 0.75f) / 10f,
            alpha = Mathf.Lerp(0.2f, 0.4f, Random.value)
        };
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
        {
            new(0, 1, 2)
        };
        TriangleMesh triangleMesh = new("Futile_White", tris, customColor: true);
        sLeaser.sprites[spikes.Length + 3] = triangleMesh;
        AddToContainer(sLeaser, rCam, null);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[2].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), rotation);
        sLeaser.sprites[spikes.Length + 2].x = pos.x - camPos.x;
        sLeaser.sprites[spikes.Length + 2].y = pos.y - camPos.y;
        for (int i = 0; i < spikes.Length; i++)
        {
            sLeaser.sprites[2 + i].x = pos.x - camPos.x;
            sLeaser.sprites[2 + i].y = pos.y - camPos.y;
            sLeaser.sprites[2 + i].rotation = Custom.VecToDeg(rotation) + spikes[i];
        }
        Color val3 = Color.Lerp(explodeColor, Color.red, 0.5f + 0.2f * Mathf.Pow(Random.value, 0.2f));
        val3 = Color.Lerp(val3, Color.white, Mathf.Pow(Random.value, 3));
        for (int j = 0; j < 2; j++)
        {
            sLeaser.sprites[j].x = pos.x - camPos.x;
            sLeaser.sprites[j].y = pos.y - camPos.y;
            sLeaser.sprites[j].rotation = Custom.VecToDeg(rotation) + (j * 90);
            sLeaser.sprites[j].color = val3;
        }
        sLeaser.sprites[spikes.Length + 3].isVisible = true;
        Vector2 posDifference = pos - lastPos;
        Vector2 perpAngleIthinkIDK = Custom.PerpendicularVector(posDifference.normalized);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(0, pos + perpAngleIthinkIDK * 2f - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(1, pos - perpAngleIthinkIDK * 2f - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(2, lastPos - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[0] = color;
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[1] = color;
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[2] = explodeColor;

        if (sLeaser.sprites[spikes.Length + 2].color != color)
        {
            UpdateColor(sLeaser, color);
        }
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = palette.blackColor;
        UpdateColor(sLeaser, color);
    }
    public virtual void UpdateColor(RoomCamera.SpriteLeaser sLeaser, Color col)
    {
        sLeaser.sprites[spikes.Length + 2].color = col;
        for (int i = 0; i < spikes.Length; i++)
        {
            sLeaser.sprites[2 + i].color = col;
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Items");
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[i]);
        }
    }

    //----------------------------------------------------------------------------------
}

public class HailstormVultureSmoke : Smoke.NewVultureSmoke
{

    public Color startColor;
    public Color endColor;

    public bool King;
    public bool Miros;

    public HailstormVultureSmoke(Room room, Vector2 pos, Vulture vul, Color smokeColA, Color smokeColB) : base (room, pos, vul)
    {
        startColor = smokeColA;
        endColor = smokeColB;
        King = vul.IsKing;
        Miros = vul.IsMiros;
    }
    
    new public void EmitSmoke(Vector2 vel, float power)
    {
        float lifetime = Mathf.Lerp(120f, 200f, Random.value);
        if (Miros)
        {
            lifetime *= 1.5f;
        }
        else if (!King)
        {
            lifetime *= 3f;
        }
        if (AddParticle(pos, vel * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, Random.value), lifetime)) is HailstormVultureSmokeSegment smoke)
        {
            smoke.power = power;
        }
    }
    public override SmokeSystemParticle CreateParticle()
    {
        return new HailstormVultureSmokeSegment(this);
    }

    public class HailstormVultureSmokeSegment : NewVultureSmokeSegment
    {
        public HailstormVultureSmoke creator;

        public HailstormVultureSmokeSegment(HailstormVultureSmoke creator) : base()
        {
            this.creator = creator;
        }

        public override Color MyColor(float timeStacker)
        {
            float lerp = Mathf.InverseLerp(5, 25 + (10f * power), age + timeStacker);
            return Color.Lerp(creator.startColor, creator.endColor, lerp);
        }

        public override float MyRad(float timeStacker)
        {
            float rad = Mathf.Min(Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) + Mathf.Sin(Mathf.InverseLerp(0.7f, 0f, Mathf.Lerp(lastLife, life, timeStacker)) * Mathf.PI) * 8f, 5f + 25f * power) * (2f - MyOpactiy(timeStacker));
            if (!creator.Miros)
            {
                rad *= 2f;
            }
            return rad;
        }

    }
}
