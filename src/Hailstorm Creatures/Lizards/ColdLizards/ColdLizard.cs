namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ColdLizard : Lizard
{
    public ColdLizAI ColdAI => AI as ColdLizAI;
    public ColdLizState ColdState => State as ColdLizState;
    public LizardGraphics LizGraphics => graphicsModule as LizardGraphics;
    public virtual bool IcyBlue => Template.type == HSEnums.CreatureType.IcyBlueLizard;
    public virtual bool Freezer => Template.type == HSEnums.CreatureType.FreezerLizard;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public LightSource chillAura;
    public float chillRadius;

    public Color effectColor2;
    public int crystalSprite;

    public int iceBreathTimer;
    public Vector2 breathDir;

    public ColdLizard(AbstractCreature absLiz, World world) : base(absLiz, world)
    {
        if (IcyBlue)
        {
            Random.State state = Random.state;
            Random.InitState(absLiz.ID.RandomSeed);
            float SizeMult = Random.Range(1, 1.2f);
            crystalSprite = Random.Range(0, 6);
            HSLColor col = new (
                    Custom.WrappedRandomVariation(220/360f, 40/360f, 0.66f), // hue
                    Custom.LerpMap(SizeMult, 1, 1.2f, 0.7f, 0.45f), // saturation
                    Custom.LerpMap(SizeMult, 1, 1.2f, 0.6f, 0.8f)); // lightness
            Random.state = state;

            SetUpIcyBlueStats(absLiz, SizeMult);

            effectColor = col.rgb;
            col.hue *= 1 - (Mathf.Lerp(0, 0.1136f, Mathf.InverseLerp(1.35f, 1.65f, lizardParams.bodyMass)) * ((lizardParams.bodyMass % 0.02f == 0) ? 1 : -1));
            col.lightness -= lizardParams.bodyMass/16f;
            effectColor2 = col.rgb;

        }
        else if (Freezer)
        {
            chillRadius = 150;
            Random.State state = Random.state;
            Random.InitState(absLiz.ID.RandomSeed);
            crystalSprite = Random.Range(0, 6);
            HSLColor col = new (Custom.WrappedRandomVariation(220/360f, 40/360f, 0.15f), 0.55f, Custom.ClampedRandomVariation(0.75f, 0.05f, 0.2f));
            Random.state = state;

            effectColor = col.rgb;
            col.hue *= (col.hue * 1.2272f > 0.75f) ? 0.7728f : 1.2272f;
            col.lightness -= 0.1f;
            effectColor2 = col.rgb;

        }
        ColdState.spitWindup = 0;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void SetUpIcyBlueStats(AbstractCreature absLiz, float SizeMult)
    {
        LizardBreedParams Icyblue = lizardParams;
        LizardBreedParams Freezer = StaticWorld.GetCreatureTemplate(HSEnums.CreatureType.FreezerLizard).breedParameters as LizardBreedParams;

        chillRadius = Custom.LerpMap(SizeMult, 1, 1.2f, 30, 120);

        float sizeFac = Mathf.InverseLerp(0.9f, 1.3f, SizeMult);
        bool Freezing = SizeMult > 1.15f;

        ColdState.meatLeft =
            SizeMult > 1.15f ? 8 :
            SizeMult > 1.10f ? 7 :
            SizeMult > 1.05f ? 6 : 5;

        Icyblue.tamingDifficulty =
            SizeMult > 1.15f ? 8.00f :
            SizeMult > 1.10f ? 6.75f :
            SizeMult > 1.05f ? 5.50f : 4.25f;

        Icyblue.bodyMass = SizeMult + 0.4f;
        if (bodyChunks is not null)
        {
            for (int b = 0; b < bodyChunks.Length; b++)
            {
                bodyChunks[b].mass = Icyblue.bodyMass / 3f;
            }
        }

        Icyblue.bodySizeFac = Mathf.Lerp(0.8f, 1.1f, Mathf.InverseLerp(1, 1.2f, SizeMult));
        Icyblue.bodyRadFac = 1 / Mathf.Pow(SizeMult, 2);
        Icyblue.bodyStiffnes = Mathf.Lerp(0, Freezer.bodyStiffnes, sizeFac);
        Icyblue.floorLeverage = Mathf.Lerp(1, Freezer.floorLeverage, sizeFac);
        Icyblue.maxMusclePower = Mathf.Lerp(2, Freezer.maxMusclePower, sizeFac);
        Icyblue.wiggleSpeed = Mathf.Lerp(1, Freezer.wiggleSpeed, sizeFac);
        Icyblue.wiggleDelay = (int)Mathf.Lerp(15, Freezer.wiggleDelay, sizeFac);
        Icyblue.swimSpeed = Mathf.Lerp(0.35f, Freezer.swimSpeed, sizeFac);
        Icyblue.idleCounterSubtractWhenCloseToIdlePos = 0;
        Icyblue.danger = Mathf.Lerp(0.4f, Freezer.danger, sizeFac);
        Icyblue.aggressionCurveExponent = Mathf.Lerp(0.925f, Freezer.aggressionCurveExponent, sizeFac);

        Icyblue.baseSpeed =
            Freezing ? 3.6f : Mathf.Lerp(3.2f, 3.6f, Mathf.InverseLerp(1, 1.2f, SizeMult));

        Icyblue.biteRadBonus = Mathf.Lerp(0, Freezer.biteRadBonus, sizeFac);
        Icyblue.biteHomingSpeed = Mathf.Lerp(1.4f, Freezer.biteHomingSpeed, sizeFac);
        Icyblue.biteChance = Mathf.Lerp(0.4f, Freezer.biteChance, sizeFac);
        Icyblue.attemptBiteRadius = Mathf.Lerp(90f, Freezer.attemptBiteRadius, sizeFac);
        Icyblue.getFreeBiteChance = Mathf.Lerp(0.5f, Freezer.getFreeBiteChance, sizeFac);
        Icyblue.biteDamage = Mathf.Lerp(0.7f, Freezer.biteDamage, sizeFac);
        Icyblue.biteDominance = Mathf.Lerp(0.1f, Freezer.biteDominance, sizeFac);

        Icyblue.canExitLoungeWarmUp = true;
        Icyblue.canExitLounge = false;
        Icyblue.preLoungeCrouch = (int)Mathf.Lerp(35, Freezer.preLoungeCrouch, sizeFac);
        Icyblue.preLoungeCrouchMovement = Mathf.Lerp(-0.3f, Freezer.preLoungeCrouchMovement, sizeFac);
        Icyblue.loungeSpeed =
            Freezing ? 3 : Mathf.Lerp(2.5f, Freezer.loungeSpeed, sizeFac);
        Icyblue.loungeJumpyness = Mathf.Lerp(1, Freezer.loungeJumpyness, sizeFac);
        Icyblue.loungeDelay = (int)Mathf.Lerp(310, Freezer.loungeDelay, sizeFac);
        Icyblue.postLoungeStun = (int)Mathf.Lerp(20, Freezer.postLoungeStun, sizeFac);
        Icyblue.loungeTendensy =
            Freezing ? 0.9f : Mathf.Lerp(0.4f, Freezer.loungeTendensy, sizeFac);

        Icyblue.perfectVisionAngle = Mathf.Lerp(0.8888f, Freezer.perfectVisionAngle, sizeFac);
        Icyblue.periferalVisionAngle = Mathf.Lerp(0.0833f, Freezer.periferalVisionAngle, sizeFac);

        Icyblue.limbSize = Mathf.Lerp(0.9f, Freezer.limbSize, sizeFac);
        Icyblue.limbThickness = Mathf.Lerp(1, Freezer.limbThickness, sizeFac);
        Icyblue.stepLength = Mathf.Lerp(0.4f, Freezer.stepLength, sizeFac);
        Icyblue.liftFeet = Mathf.Lerp(0, Freezer.liftFeet, sizeFac);
        Icyblue.feetDown = Mathf.Lerp(0, Freezer.feetDown, sizeFac);
        Icyblue.noGripSpeed = Mathf.Lerp(0.2f, Freezer.noGripSpeed, sizeFac);
        Icyblue.limbSpeed = Mathf.Lerp(6, Freezer.limbSpeed, sizeFac);
        Icyblue.limbQuickness = Mathf.Lerp(0.6f, Freezer.limbQuickness, sizeFac);
        Icyblue.walkBob = Mathf.Lerp(0.4f, Freezer.walkBob, sizeFac);
        Icyblue.regainFootingCounter = (int)Mathf.Lerp(4, Freezer.regainFootingCounter, sizeFac);

        Icyblue.tailColorationStart = Mathf.Lerp(0.2f, 0.75f, Mathf.InverseLerp(1, 1.2f, SizeMult));
        Icyblue.tailColorationExponent = 0.5f;

        Icyblue.headShieldAngle = Mathf.Lerp(108, Freezer.headShieldAngle, sizeFac);
        Icyblue.headSize = Mathf.Lerp(0.9f, 1, sizeFac);
        Icyblue.neckStiffness = Mathf.Lerp(0, Freezer.neckStiffness, sizeFac);
        Icyblue.framesBetweenLookFocusChange = (int)Mathf.Lerp(20, Freezer.framesBetweenLookFocusChange, sizeFac);

        Icyblue.terrainSpeeds[3] = new(1f, 0.9f, 0.8f, 1f);
        Icyblue.terrainSpeeds[4] = new(0.9f, 0.9f, 0.9f, 1f);
        Icyblue.terrainSpeeds[5] = new(0.7f, 1f, 1f, 1f);

        Icyblue.tongueChance = Mathf.Lerp(0.2f, 0, Mathf.InverseLerp(1, 1.2f, SizeMult));

    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);

        ColdLizHypothermia();

        if (grabbedBy.Count > 0)
        {
            GrabbedUpdate();
        }

        if (ColdState.armored &&
            ColdState.crystals is not null &&
            ColdState.crystals.All(intact => !intact))
        {
            ColdState.armored = false;
        }

        if (grasps[0]?.grabbed is not null &&
            grasps[0].grabbed is Creature grabbed)
        {
            ChillCreature(grabbed);
        }

        ChillAuraUpdate();

        if (IcyBlue)
        {
            if (tongue?.attached is not null &&
                tongue.attached.owner is Creature &&
                loungeDelay > 1)
            {
                loungeDelay--;
            }
        }
        else if (Freezer)
        {
            try
            {
                SpitUpdate();
            }
            catch (Exception e) { Debug.LogError("[Hailstorm] Freezer Lizard spit AI broke: " + e); }

            try
            {
                BreathUpdate(eu);
            }
            catch (Exception e) { Debug.LogError("[Hailstorm] Freezer Lizard ice breath broke???? Report this, please:" + e); }

        }


    }
    public virtual void ColdLizHypothermia()
    {
        if (HypothermiaExposure > 0.1f)
        {
            HypothermiaExposure = 0;
        }
        if (Hypothermia > 0.1f)
        {
            Hypothermia = 0;
        }
    }
    public virtual void GrabbedUpdate()
    {
        
        foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
        {
            Creature ctr = absCtr.realizedCreature;
            if (ctr is null ||
                ctr == this ||
                ctr is Chillipede ||
                ctr.grasps is null)
            {
                continue;
            }

            for (int m = 0; m < ctr.grasps.Length; m++)
            {
                Grasp grasp = ctr.grasps[m];

                if (grasp?.grabbed is null ||
                    grasp.grabbed != this)
                {
                    continue;
                }

                if (ctr is Leech ||
                    ctr is Spider)
                {
                    grasp.Release();
                    ctr.Stun((int)Mathf.Lerp(0, 400, Mathf.InverseLerp(1.2f, 1.7f, TotalMass)));
                }
                // ^ Stuns leeches and spiders epically if the lizard is grabbed by them. Stun time varies WILDLY with Icy Blue Lizards, but is always max with Freezers.

                else
                if (!dead &&
                    ((Freezer && grasp.chunkGrabbed == 0) ||
                    (ColdState.armored && (grasp.chunkGrabbed == 1 || grasp.chunkGrabbed == 2))))
                {
                    if (Freezer &&
                        grasp.chunkGrabbed == 0)
                    {
                        room.PlaySound(SoundID.Lizard_Jaws_Bite_Do_Damage, bodyChunks[0].pos, 1.25f, 1);
                    }
                    ctr.Violence(bodyChunks[0], -bodyChunks[0].vel, ctr.firstChunk, null, HSEnums.DamageTypes.Cold, 0.15f, 25);
                    grasp.Release();
                }
                // ^ Briefly stuns grabber and makes them let go if they toucha da lizor's armor.    
            }
        }
        
    }
    public virtual void ChillCreature(Creature victim)
    {
        if (victim.State is HealthState hs &&
            victim is not Player &&
            victim is not Chillipede)
        {
            float frostbite = (Freezer ? 0.0045f : 0.002f) / victim.Template.baseDamageResistance / victim.Template.damageRestistances[HSEnums.DamageTypes.Cold.index, 0];
            if (victim is Centipede cnt)
            {
                float sizeFac = cnt.size;
                if (cnt.AquaCenti)
                {
                    sizeFac /= 2f;
                }
                frostbite *= Mathf.Lerp(1.3f, 0.1f, Mathf.Pow(sizeFac, 0.5f));
            }
            if (LizardHooks.IsIncanStory(room?.game))
            {
                frostbite /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(victim.Template, HSEnums.DamageTypes.Cold, false);
            }
            hs.health -= frostbite;

            if (victim.killTag is null ||
                victim.killTag == abstractCreature)
            {
                victim.SetKillTag(abstractCreature);
            }
        }
    }
    public virtual void ChillAuraUpdate()
    {
        if (room is null)
        {
            return;
        }

        if (chillAura is null &&
            chillRadius >= 50)
        {
            chillAura = new LightSource(DangerPos, false, effectColor, this)
            {
                affectedByPaletteDarkness = 0.2f,
                requireUpKeep = true
            };
            room.AddObject(chillAura);
        }
        else if (chillAura is not null)
        {
            chillAura.stayAlive = true;
            chillAura.setPos = DangerPos;
            chillAura.setRad = chillRadius;
            if (chillAura.slatedForDeletetion ||
                chillAura.room != room ||
                chillRadius < 50)
            {
                chillAura = null;
            }
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void SpitUpdate()
    {
        if (ColdState.spitCooldown > 0)
        {
            ColdState.spitCooldown--;
        }

        if (ColdState.spitCooldown == 0)
        {
            Tracker.CreatureRepresentation mostAttractivePrey = AI.preyTracker.MostAttractivePrey;
            if (iceBreathTimer == 0 &&
                mostAttractivePrey is not null &&
                WantsToSpit(mostAttractivePrey) &&
                (mostAttractivePrey.VisualContact || mostAttractivePrey.TicksSinceSeen < 12 || ColdState.spitWindup >= 80))
            {
                ItsSpittinTime(mostAttractivePrey);
            }
            else if (ColdState.spitWindup != 0)
            {
                if (Random.value < ColdState.spitGiveUpChance)
                {
                    ColdState.spitGiveUpChance = 0;
                    ColdState.spitCooldown = Random.Range(280, 360);
                }
                else
                {
                    ColdState.spitGiveUpChance += 0.15f;
                }
                ColdState.spitWindup = 0;
                ColdState.spitAimChunk = null;
            }
        }
    }
    public virtual bool WantsToSpit(Tracker.CreatureRepresentation mostAttractivePrey)
    {
        if (dead || Stunned || Submersion > 0 ||
            animation == Animation.PrepareToLounge ||
            animation == Animation.Lounge ||
            animation == Animation.ShakePrey ||
            AI.behavior == LizardAI.Behavior.Flee ||
            AI.behavior == LizardAI.Behavior.EscapeRain ||
            AI.behavior == LizardAI.Behavior.ReturnPrey)
        {
            return false;
        }

        if (mostAttractivePrey?.representedCreature?.Room?.realizedRoom == null ||
            mostAttractivePrey.representedCreature.state.dead ||
            mostAttractivePrey.representedCreature.HypothermiaImmune ||
            mostAttractivePrey.representedCreature.Room.realizedRoom != room)
        {
            return false;
        }


        Creature target = mostAttractivePrey.representedCreature.realizedCreature;
        float distance = Custom.Dist(DangerPos, target.DangerPos);

        if (CustomTemplateInfo.IsColdCreature(target.Template.type))
        {
            return false;
        }

        if (target is Player ||
            target is EggBug ||
            target is Scavenger ||
            target is BigSpider ||
            target is DaddyLongLegs ||
            target is MirosBird)
        {
            return true;
        }
        if ((target is Vulture vul && !vul.IsMiros) ||
            (target is Centipede cnt2 && cnt2.size >= 0.65f && !cnt2.AquaCenti) ||
            (target is Lizard otherLizard && (otherLizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard || otherLizard.Template.type == CreatureTemplate.Type.RedLizard)))
        {
            return distance > 100;
        }
        if (target is Cicada ||
            target is DropBug ||
            target is NeedleWorm ||
            target is BigNeedleWorm ||
            target is StowawayBug ||
            target is Luminescipede)
        {
            bool preyHighEnough = Mathf.Abs(target.DangerPos.y - DangerPos.y) > 100;

            return distance > 125 && preyHighEnough;
        }

        return false;
    }
    public virtual void ItsSpittinTime(Tracker.CreatureRepresentation mostAttractivePrey)
    {
        if (ColdState.spitAimChunk is null &&
            mostAttractivePrey.representedCreature.realizedCreature is not null)
        {
            ColdState.spitAimChunk = mostAttractivePrey.representedCreature.realizedCreature.bodyChunks[Random.Range(0, mostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length - 1)];
        }
        ColdState.spitWindup++;
        bodyWiggleCounter = ColdState.spitWindup / 2;
        JawOpen =
            (ColdState.spitWindup < 80) ?
            Mathf.InverseLerp(75, 80, ColdState.spitWindup) :
            Mathf.InverseLerp(105, 90, ColdState.spitWindup);
        if (LizGraphics is not null)
        {
            LizGraphics.head.vel +=
                Custom.RNV() * ((ColdState.spitWindup < 80) ?
                Mathf.InverseLerp(0, 80, ColdState.spitWindup) :
                Mathf.InverseLerp(100, 80, ColdState.spitWindup));
        }

        AI.runSpeed = Mathf.Lerp(AI.runSpeed, 0.5f, 0.05f);

        if (ColdState.spitWindup == 80 &&
            ColdState.spitAimChunk is not null)
        {
            Vector2 val1 = bodyChunks[0].pos + Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 10f;
            Vector2 val2 = Custom.DirVec(val1, ColdState.spitAimChunk.pos);
            if (Vector2.Dot(val2, Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos)) > 0.3f || safariControlled)
            {
                room.PlaySound(SoundID.Red_Lizard_Spit, val1, 1.5f, 1.25f);
                room.AddObject(new FreezerSpit(val1, val2 * Mathf.Lerp(40, 50, Mathf.InverseLerp(500, 900, Custom.Dist(DangerPos, val2))), this));
                bodyChunks[2].pos -= val2 * 8f;
                bodyChunks[1].pos -= val2 * 4f;
                bodyChunks[2].vel -= val2 * 2f;
                bodyChunks[1].vel -= val2 * 1f;
                ColdState.spitUsed = true;
            }
        }
        else
        if (ColdState.spitWindup >= 80 &&
            !ColdState.spitUsed)
        {
            ColdState.spitWindup = 0;
            ColdState.spitAimChunk = null;
        }
        else
        if (ColdState.spitWindup >= 110)
        {
            ColdState.spitUsed = false;
            ColdState.spitCooldown = Random.Range(1400, 1800);
            ColdState.spitWindup = 0;
            ColdState.spitAimChunk = null;
        }
    }

    public virtual void BreathUpdate(bool eu)
    {
        if (iceBreathTimer > 0)
        {
            iceBreathTimer--;

            if (eu)
            {
                InsectCoordinator smallInsects = null;
                if (room is not null)
                {
                    for (int i = 0; i < room.updateList.Count; i++)
                    {
                        if (room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }
                }
                Vector2 lizHeadAngle = (bodyChunks[0].pos - bodyChunks[1].pos).normalized;
                float breathDir = Custom.VecToDeg(lizHeadAngle);
                Vector2 finalBreathDir = Custom.DegToVec(breathDir + Random.Range(-15, 15));

                float breathSize =
                    (dead ? 0.1f : 0.15f) * Mathf.Lerp(2f, 2.5f, Mathf.InverseLerp(0, 60, iceBreathTimer));
                
                finalBreathDir *= dead ? Random.Range(15f, 19f) : Random.Range(18f, 23f);

                EmitFreezerMist(DangerPos + finalBreathDir, finalBreathDir, breathSize, smallInsects, true);

            }
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override bool SpearStick(Weapon source, float DMG, BodyChunk hitChunk, Appendage.Pos onAppendagePos, Vector2 direction)
    {
        if (ColdState.armored && (hitChunk.index == 1 || hitChunk.index == 2))
        {
            return false;
        }
        return base.SpearStick(source, DMG, hitChunk, onAppendagePos, direction);
    }
    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType dmgType, float DMG, float XSTUN)
    {
        bool NotNull = source?.owner is not null;
        if (room is not null &&
            hitChunk is not null &&
            (NotNull || dmgType == DamageType.Explosion))
        {
            bool SourceIsPackMember =
                    NotNull &&
                    source.owner is Creature attacker &&
                    AI.DynamicRelationship(attacker.abstractCreature).type == CreatureTemplate.Relationship.Type.Pack;

            if (dmgType == DamageType.Blunt && // Blunt attacks are stronger against the heads and armor of the Ice Lizards.
                (hitChunk.index == 0 || (ColdState.armored && (hitChunk.index == 1 || hitChunk.index == 2))))
            {
                DMG *= 1.5f;
                XSTUN *= 1.5f;
            }

            if (hitChunk.index == 0)
            {
                if (directionAndMomentum.HasValue &&
                    HitInMouth(directionAndMomentum.Value)) // Mouth hits cause Freezers to breathe out frost.
                {
                    iceBreathTimer = (int)(DMG * (dead ? 8 : 24));
                    breathDir = -directionAndMomentum.Value;
                    DMG *= 0.8f; // 1.5x damage -> 1.2x damage
                }
                else
                {
                    DMG *= 0.5f;
                }
            }

            if (ColdState.armored && hitChunk.index != 0)
            {
                if (!SourceIsPackMember && (DMG >= 0.25f || (NotNull && source.owner is StowawayBug)))
                {
                    // Armor damage
                    int CrystalsToBreak = (int)Mathf.Max(DMG, 1);
                    if (dmgType == DamageType.Explosion || (NotNull && source.owner is ExplosiveSpear))
                    {
                        CrystalsToBreak += 3;
                    }

                    if (XSTUN >= 10)
                    {
                        LoseAllGrasps();
                    }

                    BreakIceLizArmor(source, hitChunk, dmgType, CrystalsToBreak);
                }

                DMG *= 0.01f;

                if (source?.owner is not null &&
                    source.owner is Player self &&
                    self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                {
                    XSTUN *= 0.5f;
                }
                else
                {
                    XSTUN *= 0.01f;
                }

            }

            if (dmgType == DamageType.Explosion)
            {
                DMG *= 0.01f;
            }

        }
        base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, dmgType, DMG, XSTUN);
    }
    public virtual void BreakIceLizArmor(BodyChunk source, BodyChunk hitChunk, DamageType dmgType, int CrystalsToBreak)
    {
        if (CrystalsToBreak < 1)
        {
            return;
        }

        InsectCoordinator smallInsects = null;
        for (int i = 0; i < room.updateList.Count; i++)
        {
            if (room.updateList[i] is InsectCoordinator)
            {
                smallInsects = room.updateList[i] as InsectCoordinator;
                break;
            }
        }

        Vector2 particleVel = default;

        if (Freezer)
        {
            bool drop = false;
            for (int i = Random.Range(0, ColdState.crystals.Length - 1); CrystalsToBreak > 0 && !ColdState.crystals.All(intact => !intact); )
            {
                if (ColdState.crystals[i])
                {
                    ColdState.crystals[i] = false;
                    CrystalsToBreak--;
                    drop = Random.value < (dmgType == DamageType.Blunt ? 0.01875f : 0.075f) &&
                        dmgType != HSEnums.DamageTypes.Heat &&
                        !(source?.owner is not null && source.owner is Spear spr && spr.bugSpear);

                    if (drop)
                    {
                        Vector2 crystalPos = hitChunk.pos + new Vector2(0, 15);
                        ArmorIceSpikes.DropCrystals(this, crystalPos, crystalSprite);
                    }
                    else
                    {
                        room.PlaySound(SoundID.Coral_Circuit_Break, hitChunk.pos, 1.25f, 1.5f);
                    }

                    for (int j = 0; j < (drop ? 6 : 12); j++)
                    {
                        particleVel = Custom.RNV() * Random.value * 16f;
                        if (j % 2 == 1)
                        {
                            EmitSnowflake(hitChunk.pos, particleVel);
                            EmitFreezerMist(hitChunk.pos, particleVel * 2/3f, 0.2f, smallInsects, false);
                        }
                        else if (!drop)
                        {
                            EmitIceflake(hitChunk.pos, particleVel);
                            if (j < 6)
                            {
                                EmitIceshard(hitChunk.pos, particleVel, Random.Range(1.25f, 1.75f), 0.2f, Random.Range(1.3f, 1.7f));
                            }
                        }
                    }
                }
                if (CrystalsToBreak > 0)
                {
                    if (i == ColdState.crystals.Length - 1)
                    {
                        i -= i;
                    }
                    else i++;
                }
            }
        }
        else if (IcyBlue)
        {
            room.PlaySound(SoundID.Coral_Circuit_Break, hitChunk.pos, 1.25f, 1.5f);

            for (int i = 0; i < ColdState.crystals.Length; i++)
            {
                if (!ColdState.crystals[i])
                {
                    continue;
                }

                ColdState.crystals[i] = false;

                for (int j = 0; j < 6; j++)
                {
                    particleVel = Custom.RNV() * Random.value * 16f;
                    if (j % 2 == 1)
                    {
                        EmitSnowflake(hitChunk.pos, particleVel);
                        EmitFreezerMist(hitChunk.pos, particleVel * 2/3f, 0.2f, smallInsects, false);
                    }
                    else
                    {
                        EmitIceflake(hitChunk.pos, particleVel);
                        EmitIceshard(hitChunk.pos, particleVel, 1f, 0.1f, Random.Range(1.3f, 1.7f));
                    }
                }
            }
        }

    }
    public override void Stun(int st)
    {
        if (Freezer)
        {
            if (stun > 90)
            {
                stun = 90;
            }
            if (ColdState.health < 0.5f)
            {
                stun = (int)(stun / (1 + Mathf.InverseLerp(0.75f, 0, ColdState.health) * 1.5f));
            }
        }
        base.Stun(st);
    }


    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void EmitSnowflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new HailstormSnowflake(pos, vel, effectColor, effectColor2));
    }
    public virtual void EmitIceflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new PuffBallSkin(pos, vel, effectColor, effectColor2));
    }
    public virtual void EmitIceshard(Vector2 pos, Vector2 vel, float scale, float shardVolume, float shardPitch)
    {
        Color shardColor = Random.value < 2/3f ?
                        effectColor :
                        effectColor2;
        room.AddObject(new Shard(pos, vel, shardVolume, scale, shardPitch, shardColor, true));
    }

    public virtual void EmitFreezerMist(Vector2 pos, Vector2 vel, float size, InsectCoordinator insectCoordinator, bool hasGameplayImpact)
    {
        room.AddObject(new FreezerMist(pos, vel, effectColor, effectColor2, size, abstractCreature, insectCoordinator, hasGameplayImpact));
    }



}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------