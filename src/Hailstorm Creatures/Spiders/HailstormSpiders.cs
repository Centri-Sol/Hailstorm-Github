namespace Hailstorm;

public class HailstormSpiders
{
    public static void Hooks()
    {

        // Baby Spiders
        On.Spider.ctor += WinterCoalescipedes;
        On.Spider.Centipede.ctor += WinterCoalescipedeChains;
        On.Spider.Update += WinterCoalescipedeSaysNoToDens;
        On.Spider.Move_Vector2 += WinterCoalescipedeSpeed;
        On.Spider.ConsiderPrey += WinterCoalescipedeHigherMassLimit;
        On.SpiderGraphics.InitiateSprites += WinterCoalescipedeSpriteSizes;
        On.SpiderGraphics.ApplyPalette += WinterCoalescipedeColors;
        SpiderILHooks();

        // Big Spiders
        On.BigSpider.ctor += WinterSpiders;
        On.BigSpider.Update += WinterMotherSpiderBabySpawn;
        On.BigSpider.Collide += WinterMotherCRONCH;
        On.BigSpider.Violence += WinterMotherHP;
        On.BigSpider.Die += WinterMotherSpiderBabyPuff;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += WinterMotherSpiderHostility;
        On.BigSpiderAI.TryAddReviveBuddy += PeachSpiderShouldntRevive;
        On.BigSpider.Revive += PeachSpiderGetsRevivedForLonger;
        On.BigSpider.ShortCutColor += PeachSpiderShortcutColor;

        On.BigSpiderGraphics.ctor += PeachSpiderScales;
        On.BigSpiderGraphics.ApplyPalette += PeachSpiderColors;
        On.BigSpiderGraphics.InitiateSprites += WinterSpiderSprites;
        On.BigSpiderGraphics.DrawSprites += WinterSpiderVisuals;

        // Luminescipedes
        On.AbstractCreature.Abstractize += LuminAntiAbstraction;
        On.AbstractCreature.Update += LuminHideTimer;
        On.LizardAI.IUseARelationshipTracker_CreateTrackedCreatureState += LuminsGetTrackedByLizards; // This lets Lumins scare Lizards with Vulture Masks.

    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSEnums.Incandescent;
    }


    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------


    #region Baby Spiders
    public static bool IsWinterCoalescipede(Spider spd)
    {
        return spd?.room?.game is not null && spd.Template.type == CreatureTemplate.Type.Spider &&
            (HSRemix.HailstormBabySpidersEverywhere.Value is true || (IsIncanStory(spd.room.game) && spd.abstractCreature.Winterized));
    }
    public static void WinterCoalescipedes(On.Spider.orig_ctor orig, Spider spd, AbstractCreature absCtr, World world)
    {
        orig(spd, absCtr, world);
        if (spd is not null && IsWinterCoalescipede(spd))
        {
            Random.State state = Random.state;
            Random.InitState(spd.abstractCreature.ID.RandomSeed);
            spd.iVars = new Spider.IndividualVariations(Random.Range(0.4f, 1.2f));
            Random.state = state;

            spd.gravity = 0.85f;
            spd.bounce = 0.15f;
            spd.surfaceFriction = 0.9f;
            if (spd.bodyChunks is not null && spd.bodyChunks[0] is not null)
            {
                spd.bodyChunks[0].rad = Mathf.Lerp(4.4f, 10.8f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                spd.bodyChunks[0].mass = Mathf.Lerp(0.08f, 0.18f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
            }
            spd.denMovement = -1;
        }
    }
    public static void WinterCoalescipedeChains(On.Spider.Centipede.orig_ctor orig, Spider.Centipede cls, Spider origSpd, Room room)
    {
        orig(cls, origSpd, room);
        if (cls is not null && origSpd is not null && IsWinterCoalescipede(origSpd))
        {
            cls.maxSize = Random.Range(20, 31);

        }
    }

    //-----------------------------------------

    public static void WinterCoalescipedeSaysNoToDens(On.Spider.orig_Update orig, Spider spd, bool eu)
    {
        orig(spd, eu);
        if (spd?.room is not null && IsWinterCoalescipede(spd) && spd.denPos is not null)
        {
            spd.denPos = null;
        }
    }
    public static void WinterCoalescipedeSpeed(On.Spider.orig_Move_Vector2 orig, Spider spd, Vector2 dest)
    {
        Vector2? vel = null;
        if (spd is not null && IsWinterCoalescipede(spd))
        {
            vel = spd.mainBodyChunk.vel;
        }
        orig(spd, dest);
        if (vel.HasValue)
        {
            vel = spd.mainBodyChunk.vel - vel;
            spd.mainBodyChunk.vel += vel.Value / 2f;
        }
    }
    public static bool WinterCoalescipedeHigherMassLimit(On.Spider.orig_ConsiderPrey orig, Spider spd, Creature ctr)
    {
        return (spd is not null && IsWinterCoalescipede(spd) && ctr.TotalMass <= 6.72f && spd.Template.CreatureRelationship(ctr.Template).type == CreatureTemplate.Relationship.Type.Eats && !ctr.leechedOut)
|| orig(spd, ctr);
    }

    public static void SpiderILHooks()
    {
        IL.Spider.Centipede.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = IL.DefineLabel();
            _ = c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate((Spider.Centipede cls, bool eu) =>
            {
                return LightAdaptionChanges(cls, eu);
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(label);
        };
    }
    public static bool LightAdaptionChanges(Spider.Centipede cls, bool eu)
    {
        if (cls?.FirstSpider is null || !IsWinterCoalescipede(cls.FirstSpider))
        {
            return false;
        }

        cls.counter++;
        if (cls.counter >= cls.body.Count)
        {
            cls.counter = 0;
            cls.Tighten();
        }

        if (!cls.ShouldIUpdate(eu))
        {
            return true;
        }

        for (int num = cls.spiders.Count - 1; num >= 0; num--)
        {
            if (cls.spiders[num].dead || cls.spiders[num].room != cls.room)
            {
                cls.AbandonSpider(num);
            }
        }

        if (cls.FirstSpider is not null && cls.FirstSpider.moving)
        {
            cls.walkCycle -= 2;
        }
        else
        {
            cls.walkCycle--;
        }
        for (int num = cls.body.Count - 1; num >= 0; num--)
        {
            if (cls.body[num].spider.centipede != cls || cls.body[num].separatedCounter > 5)
            {
                if (cls.body[num].spider.centipede == cls)
                {
                    cls.AbandonSpider(cls.body[num].spider);
                }
                cls.body.RemoveAt(num);
            }
        }
        cls.totalMass = 0f;
        for (int i = 0; i < cls.body.Count; i++)
        {
            cls.body[i].Update(i);
            cls.totalMass += cls.body[i].spider.mainBodyChunk.mass;
            cls.body[i].spider.legsPosition = Mathf.Lerp(-1f, 1f, cls.body[i].BodyFac);
            if (i > 0 && cls.body[i].spider.iVars.size > cls.body[i - 1].spider.iVars.size)
            {
                Spider spider = cls.body[i].spider;
                Spider spider2 = cls.body[i - 1].spider;
                cls.body[i].spider = spider2;
                cls.body[i - 1].spider = spider;
                cls.body[i].inRightPlace = false;
                cls.body[i - 1].inRightPlace = false;
                break;
            }
            if (i >= 3 && cls.body[i - 2].inRightPlace && cls.body[i - 1].inRightPlace && cls.body[i].inRightPlace)
            {
                cls.body[i].spider.mainBodyChunk.vel -=
                    Custom.DirVec(cls.body[i].spider.mainBodyChunk.pos, cls.body[i - 2].spider.mainBodyChunk.pos);

                cls.body[i - 2].spider.mainBodyChunk.vel +=
                    Custom.DirVec(cls.body[i].spider.mainBodyChunk.pos, cls.body[i - 2].spider.mainBodyChunk.pos);
            }
        }
        cls.ConsiderCreature();
        if (cls.prey is not null && cls.FirstSpider is not null)
        {
            if (cls.prey.room != cls.FirstSpider.room || cls.totalMass < cls.prey.TotalMass)
            {
                cls.prey = null;
            }
            else if (cls.FirstSpider.VisualContact(cls.prey.mainBodyChunk.pos))
            {
                cls.preyPos = cls.prey.mainBodyChunk.pos;
                cls.preyVisualCounter = 0;
            }
        }
        cls.hunt = cls.preyVisualCounter < 100 && cls.prey is not null ? Mathf.Min(cls.hunt + 0.005f, 1f) : Mathf.Max(cls.hunt - 0.005f, 0f);
        cls.preyVisualCounter++;

        return true;
    }


    //-----------------------------------------

    public static void WinterCoalescipedeSpriteSizes(On.SpiderGraphics.orig_InitiateSprites orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(sg, sLeaser, rCam);
        if (sg?.spider is not null && IsWinterCoalescipede(sg.spider))
        {
            Spider spd = sg.spider;

            sLeaser.sprites[sg.BodySprite].scale = spd.iVars.size;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[sg.LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                    sLeaser.sprites[sg.LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1f : -1f) * Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.4f, 1.2f, spd.iVars.size));
                }
            }
        }
    }
    public static void WinterCoalescipedeColors(On.SpiderGraphics.orig_ApplyPalette orig, SpiderGraphics sg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(sg, sLeaser, rCam, palette);
        if (sg is not null && IsWinterCoalescipede(sg.spider))
        {
            sg.blackColor = Custom.HSL2RGB(210 / 360f, 0.1f, Mathf.Lerp(0.2f, 0.3f, Mathf.InverseLerp(0.4f, 1.2f, sg.spider.iVars.size)));
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = sg.blackColor;
            }
        }
    }

    #endregion


    //----------------------------------------------------------------------------------


    #region Big Spiders
    public static void WinterSpiders(On.BigSpider.orig_ctor orig, BigSpider bigSpd, AbstractCreature absSpd, World world)
    {
        orig(bigSpd, absSpd, world);
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(world?.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            absSpd.state.meatLeft = 9;
            if (bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.33f;
                    bigSpd.bodyChunks[b].rad *= 1.33f;
                }
            }
            if (CWT.AbsCtrData.TryGetValue(absSpd, out CWT.AbsCtrInfo aI))
            {
                aI.functionTimer = 450 + (int)(HSRemix.MotherSpiderEvenMoreSpiders.Value * 10);
            }
        }

        if (bigSpd.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
            {
                bigSpd.bodyChunks[b].mass *= 0.2f;
                bigSpd.bodyChunks[b].rad *= 0.8f;
            }
            Random.State state = Random.state;
            Random.InitState(absSpd.ID.RandomSeed);
            float hue =
                (Random.value < 0.8f) ? 209 :
                (Random.value < 0.8f) ? (0.209f + Custom.ClampedRandomVariation(0, 0.151f, 0.33f)) * 1000 : 1;
            hue /= 360f;
            float sat = Mathf.Lerp(0.39f, 0.6f, Mathf.InverseLerp(209 / 360f, 1, hue));
            float bri = 0.55f + Random.Range(-0.05f, 0.05f);
            bigSpd.yellowCol = Custom.HSL2RGB(hue, sat, bri);
            Random.state = state;
        }
        else if (IsIncanStory(world?.game) || HSRemix.BigSpiderColorsEverywhere.Value is true)
        {
            if (IsIncanStory(world?.game) && bigSpd.Template.type != MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && bigSpd.bodyChunks is not null)
            {
                for (int b = 0; b < bigSpd.bodyChunks.Length; b++)
                {
                    bigSpd.bodyChunks[b].mass *= 1.15f;
                    bigSpd.bodyChunks[b].rad *= 1.15f;
                }
            }

            if (bigSpd.Template.type == CreatureTemplate.Type.BigSpider)
            {
                Random.State state = Random.state;
                Random.InitState(absSpd.ID.RandomSeed);
                bigSpd.yellowCol = Color.Lerp(
                    Custom.HSL2RGB(Random.Range(30 / 360f, 70 / 360f), Random.Range(0.5f, 1f), Random.Range(0.3f, 0.5f)),
                    Custom.HSL2RGB(Random.value, Random.value, Random.value),
                    Random.value * 0.2f);
                Random.state = state;
            }
            else if (bigSpd.Template.type == CreatureTemplate.Type.SpitterSpider)
            {
                Random.State state = Random.state;
                Random.InitState(absSpd.ID.RandomSeed);
                bigSpd.yellowCol =
                        Color.Lerp(
                            Custom.HSL2RGB(Random.Range(-60 / 360f, 20 / 360f), Random.Range(0.5f, 1f), Random.Range(0.2f, 0.4f)),
                            Custom.HSL2RGB(Random.value, Random.value, Random.value),
                            Random.value * 0.2f);
                Random.state = state;
            }
            else if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
            {
                Random.State state = Random.state;
                Random.InitState(absSpd.ID.RandomSeed);
                bigSpd.yellowCol =
                        Color.Lerp(
                            Custom.HSL2RGB(Random.Range(120 / 360f, 160 / 360f), Random.Range(0.5f, 1f), Random.Range(0.4f, 0.6f)),
                            Custom.HSL2RGB(Random.value, Random.value, Random.value),
                            Random.value * 0.2f);
                Random.state = state;
            }
        }
    }

    //-----------------------------------------

    public static void WinterMotherSpiderBabySpawn(On.BigSpider.orig_Update orig, BigSpider bigSpd, bool eu)
    {
        orig(bigSpd, eu);
        if (bigSpd?.room is null)
        {
            return;
        }

        if (bigSpd.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            if (bigSpd.jumpStamina < 0.2f)
            {
                bigSpd.jumpStamina = 0.2f;
            }
            if (bigSpd.canCling > 0)
            {
                bigSpd.canCling = 0;
            }
        }
        else if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            // Partially counteracts the Mother Spider's regeneration, or else it would be WAY too fast thanks to the health increase that Mother Spiders get (it's pretty much percentage-based).
            if (bigSpd.State.health is > 0 and < 1)
            {
                bigSpd.State.health -=
                    0.0014705883f * (bigSpd.State.health < 0.5f ? 0.5f : 0.875f);
            }

            // Once a creature is on a Mother Spider's shitlist, the Mother won't be afraid of it anymore. At least, that's what I HOPE this does...
            if (aI.ctrList is not null && aI.ctrList.Count > 0 && bigSpd.AI?.relationshipTracker?.relationships is not null)
            {
                for (int r = 0; r < bigSpd.AI.relationshipTracker.relationships.Count; r++)
                {
                    for (int s = 0; s < aI.ctrList.Count; s++)
                    {
                        RelationshipTracker.DynamicRelationship relationship = bigSpd.AI.relationshipTracker.relationships[r];
                        if (relationship.trackerRep.representedCreature is not null &&
                            relationship.trackerRep.representedCreature == aI.ctrList[s] &&
                            (relationship.state as BigSpiderAI.SpiderTrackState).accustomed < 600)
                        {
                            (relationship.state as BigSpiderAI.SpiderTrackState).accustomed = 1800;
                            break;
                        }
                    }
                }
            }

            // Allows the Mother Spider to spawn little baby spiders while it's in stun.
            if (aI.functionTimer > 0 &&
                !bigSpd.dead &&
                !bigSpd.Consious &&
                !bigSpd.spewBabies &&
                !bigSpd.inShortcut &&
                !bigSpd.slatedForDeletetion &&
                bigSpd.room.world is not null &&
                bigSpd.room.game.cameras[0].room == bigSpd.room)
            {
                aI.functionTimer--;
                if (aI.functionTimer % 10 == 0)
                {
                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < bigSpd.room.updateList.Count; i++)
                    {
                        if (bigSpd.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = bigSpd.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    Vector2 pos = bigSpd.mainBodyChunk.pos;
                    for (int j = 0; j < 5; j++)
                    {
                        SporeCloud sC = new(pos, Custom.RNV() * Random.value * 10f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.4f), 1f, null, j % 20, smallInsects)
                        {
                            nonToxic = true
                        };
                        bigSpd.room.AddObject(sC);
                    }

                    SporePuffVisionObscurer sPVO = new(pos)
                    {
                        doNotCallDeer = true
                    };
                    bigSpd.room.AddObject(sPVO);

                    bigSpd.room.AddObject(new PuffBallSkin(pos, Custom.RNV() * Random.value * 16f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), Color.Lerp(bigSpd.yellowCol, Color.black, 0.6f)));

                    AbstractCreature absSpd = new(bigSpd.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, bigSpd.room.GetWorldCoordinate(pos), bigSpd.room.world.game.GetNewID())
                    {
                        Winterized = bigSpd.abstractCreature.Winterized,
                        ignoreCycle = bigSpd.abstractCreature.ignoreCycle,
                        HypothermiaImmune = bigSpd.abstractCreature.HypothermiaImmune
                    };
                    bigSpd.room.abstractRoom.AddEntity(absSpd);
                    absSpd.RealizeInRoom();
                    (absSpd.realizedCreature as Spider).bloodLust = 1f;
                }
            }
        }
    }
    public static void WinterMotherCRONCH(On.BigSpider.orig_Collide orig, BigSpider bigSpd, PhysicalObject obj, int myChunk, int otherChunk)
    {
        if (bigSpd?.AI is not null && bigSpd.Template.type == HSEnums.CreatureType.PeachSpider && obj is Creature ctr)
        {
            bigSpd.AI.tracker.SeeCreature(ctr.abstractCreature);
            List<RelationshipTracker.DynamicRelationship> relationships = bigSpd.AI.relationshipTracker.relationships;
            for (int i = 0; i < relationships.Count; i++)
            {
                if (relationships[i].trackerRep.representedCreature == ctr.abstractCreature)
                {
                    (relationships[i].state as BigSpiderAI.SpiderTrackState).accustomed += 1800;
                    break;
                }
            }
        }
        else if (HSRemix.MotherSpiderCRONCH.Value is true &&
            obj is not null &&
            obj is Creature target &&
            !target.dead &&
            target is not Spider &&
            target is not BigSpider &&
            bigSpd?.room is not null &&
            bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider &&
            (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) &&
            bigSpd.bodyChunks[myChunk].vel.magnitude >= 12.5f &&
            bigSpd.TotalMass > target.TotalMass &&
            CWT.CreatureData.TryGetValue(bigSpd, out CWT.CreatureInfo jI) &&
            CWT.CreatureData.TryGetValue(target, out CWT.CreatureInfo vI) &&
            (jI.impactCooldown == 0 || vI.impactCooldown == 0))
        {
            jI.impactCooldown = 40;
            vI.impactCooldown = 40;

            float damage =
                bigSpd.bodyChunks[myChunk].vel.magnitude >= 22.5f ? 1.0f :
                bigSpd.bodyChunks[myChunk].vel.magnitude >= 17.5f ? 0.5f : 0;

            float stun =
                Mathf.Lerp(25, 50, Mathf.InverseLerp(12.5f, 22.5f, bigSpd.bodyChunks[myChunk].vel.magnitude));

            target.Violence(bigSpd.bodyChunks[myChunk], bigSpd.bodyChunks[myChunk].vel, target.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, damage, stun);

            _ = bigSpd.room.PlaySound(SoundID.Cicada_Heavy_Terrain_Impact, bigSpd.bodyChunks[myChunk]);
            bigSpd.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, bigSpd.bodyChunks[myChunk], false, 1.4f, 1.1f);
            _ = bigSpd.room.PlaySound(SoundID.Rock_Hit_Wall, bigSpd.bodyChunks[myChunk], false, 1.5f, Random.Range(0.66f, 0.8f));
            if ((target.State is HealthState HS && HS.ClampedHealth == 0) || target.State.dead)
            {
                _ = bigSpd.room.PlaySound(SoundID.Spear_Stick_In_Creature, bigSpd.bodyChunks[myChunk], false, 1.7f, Random.Range(0.6f, 0.8f));
            }
        }
        orig(bigSpd, obj, myChunk, otherChunk);
    }
    public static void WinterMotherHP(On.BigSpider.orig_Violence orig, BigSpider bigSpd, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType dmgType, float dmg, float stun)
    {
        if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true))
        {
            dmg /= 2f; // x2 HP
            stun *= 0.4f;
            if (source?.owner is not null && source.owner is Creature ctr && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out CWT.AbsCtrInfo abI) && abI.ctrList is not null && !abI.ctrList.Contains(ctr.abstractCreature))
            {
                abI.ctrList.Add(ctr.abstractCreature);
            }
        }
        if (IsIncanStory(bigSpd?.room?.game) && bigSpd.Template.type == CreatureTemplate.Type.BigSpider && bigSpd.abstractCreature.Winterized)
        {
            dmg /= 2f;
        }
        orig(bigSpd, source, dirAndMomentum, hitChunk, hitAppendage, dmgType, dmg, stun);
    }
    public static void WinterMotherSpiderBabyPuff(On.BigSpider.orig_Die orig, BigSpider bigSpd)
    {
        if (bigSpd?.room is not null && (IsIncanStory(bigSpd.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) && CWT.AbsCtrData.TryGetValue(bigSpd.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            if (bigSpd.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
            {
                bigSpd.spewBabies = true;

                if (aI.functionTimer > 0 &&
                    !bigSpd.dead &&
                    !bigSpd.Consious &&
                    !bigSpd.inShortcut &&
                    !bigSpd.slatedForDeletetion &&
                    bigSpd.room.world is not null &&
                    bigSpd.room.game.cameras[0].room == bigSpd.room)
                {
                    int remainingSpiders = aI.functionTimer / 10;
                    aI.functionTimer = 0;

                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < bigSpd.room.updateList.Count; i++)
                    {
                        if (bigSpd.room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = bigSpd.room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    Vector2 pos = bigSpd.mainBodyChunk.pos;
                    for (int j = 0; j < remainingSpiders * 2; j++)
                    {
                        SporeCloud sC = new(pos, Custom.RNV() * Random.value * 10f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), 1f, null, j % 20, smallInsects)
                        {
                            nonToxic = true
                        };
                        bigSpd.room.AddObject(sC);
                    }

                    SporePuffVisionObscurer sPVO = new(pos)
                    {
                        doNotCallDeer = true
                    };
                    bigSpd.room.AddObject(sPVO);

                    for (int k = 0; k < remainingSpiders * 0.2f; k++)
                    {
                        bigSpd.room.AddObject(new PuffBallSkin(pos, Custom.RNV() * Random.value * 16f, Color.Lerp(bigSpd.yellowCol, Color.black, 0.2f), Color.Lerp(bigSpd.yellowCol, Color.black, 0.6f)));
                    }

                    for (int s = 0; s < remainingSpiders; s++)
                    {
                        AbstractCreature absSpd = new(bigSpd.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Spider), null, bigSpd.room.GetWorldCoordinate(pos), bigSpd.room.world.game.GetNewID())
                        {
                            Winterized = bigSpd.abstractCreature.Winterized,
                            ignoreCycle = bigSpd.abstractCreature.ignoreCycle,
                            HypothermiaImmune = bigSpd.abstractCreature.HypothermiaImmune
                        };
                        bigSpd.room.abstractRoom.AddEntity(absSpd);
                        absSpd.RealizeInRoom();
                        (absSpd.realizedCreature as Spider).bloodLust = 1f;
                    }
                }
            }
        }
        orig(bigSpd);
    }
    public static CreatureTemplate.Relationship WinterMotherSpiderHostility(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI AI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        if (AI?.bug?.room is not null &&
            AI.bug.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider &
            (IsIncanStory(AI.bug.room.game) || HSRemix.HailstormMotherSpidersEverywhere.Value is true) &&
            CWT.AbsCtrData.TryGetValue(AI.bug.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            foreach (AbstractCreature absCtr in aI.ctrList)
            {
                if (absCtr?.realizedCreature is not null &&
                    dynamRelat.trackerRep.representedCreature.realizedCreature is not null &&
                    absCtr.realizedCreature == dynamRelat.trackerRep.representedCreature.realizedCreature &&
                    !dynamRelat.trackerRep.representedCreature.state.dead)
                {
                    return new CreatureTemplate.Relationship
                        (CreatureTemplate.Relationship.Type.Attacks, 1.2f);
                }
            }
        }
        return orig(AI, dynamRelat);
    }
    public static void PeachSpiderShouldntRevive(On.BigSpiderAI.orig_TryAddReviveBuddy orig, BigSpiderAI AI, Tracker.CreatureRepresentation candidate)
    {
        orig(AI, candidate);
        if (AI?.bug is not null && AI.bug.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            AI.reviveBuddy = null;
        }
    }
    public static void PeachSpiderGetsRevivedForLonger(On.BigSpider.orig_Revive orig, BigSpider bigSpd)
    {
        orig(bigSpd);
        if (bigSpd is not null && bigSpd.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            bigSpd.borrowedTime += 500;
        }
    }
    public static Color PeachSpiderShortcutColor(On.BigSpider.orig_ShortCutColor orig, BigSpider bigSpd) => bigSpd.Template.type == HSEnums.CreatureType.PeachSpider ? bigSpd.yellowCol * 0.75f : orig(bigSpd);

    public static void PeachSpiderScales(On.BigSpiderGraphics.orig_ctor orig, BigSpiderGraphics bsg, PhysicalObject owner)
    {
        orig(bsg, owner);
        if (bsg?.bug is not null && bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            Random.State state = Random.state;
            Random.InitState(bsg.bug.abstractCreature.ID.RandomSeed);
            bsg.totalScales = 0;
            bsg.scales = new Vector2[Random.Range(36, 51)][,];
            bsg.scaleStuckPositions = new Vector2[bsg.scales.Length];
            bsg.scaleSpecs = new Vector2[bsg.scales.Length, 2];
            bsg.legsThickness = Mathf.Lerp(0.6f, 0.8f, Random.value);
            bsg.bodyThickness = 0.4f + Mathf.Lerp(0.9f, 1.1f, Random.value);
            int num = 0;
            for (int s = 0; s < bsg.scales.Length; s++)
            {
                bsg.scaleSpecs[s, 0] = new Vector2(Random.value, 5f);
                bsg.scales[s] = new Vector2[Random.Range(5, 8), 4];
                bsg.totalScales += bsg.scales[s].GetLength(0);
                for (int num4 = 0; num4 < bsg.scales[s].GetLength(0); num4++)
                {
                    bsg.scales[s][num4, 3].x = num;
                    num++;
                }
                bsg.scaleStuckPositions[s] = new Vector2(Mathf.Lerp(-0.5f, 0.5f, Random.value), Mathf.Pow(Random.value, Custom.LerpMap(bsg.scales[s].GetLength(0), 2f, 9f, 0.5f, 2f)));
                if (s % 3 == 0 && bsg.scaleStuckPositions[s].y > 0.5f)
                {
                    bsg.scaleStuckPositions[s].y *= 0.5f;
                }
            }
            bsg.Reset();
            Random.state = state;
        }
    }
    public static void PeachSpiderColors(On.BigSpiderGraphics.orig_ApplyPalette orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(bsg, sLeaser, rCam, palette);
        if (bsg?.bug is not null && bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider)
        {
            Random.State state = Random.state;
            Random.InitState(bsg.bug.abstractCreature.ID.altSeed);
            if (Random.value < 0.85f)
            {
                bsg.blackColor = bsg.bug.yellowCol;
                bsg.yellowCol = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), bsg.blackColor, 0.07f);
                bsg.blackColor = Color.Lerp(bsg.blackColor, palette.blackColor, bsg.darkness / 2f);
                bsg.yellowCol = Color.Lerp(bsg.yellowCol, palette.fogColor, 0.1f + (bsg.darkness * 0.4f));
            }
            else
            {
                bsg.blackColor = Color.Lerp(new Color(0.9f, 0.9f, 0.9f), bsg.yellowCol, 0.07f);
                bsg.yellowCol = bsg.bug.yellowCol;
                bsg.blackColor = Color.Lerp(bsg.blackColor, palette.fogColor, 0.1f + (bsg.darkness * 0.4f));
                bsg.yellowCol = Color.Lerp(bsg.yellowCol, palette.blackColor, bsg.darkness / 2f);
            }
            Random.state = state;

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = bsg.blackColor;
            }
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[bsg.MandibleSprite(j, 1)].color = bsg.yellowCol;
                for (int k = 0; k < 3; k++)
                {
                    (sLeaser.sprites[bsg.MandibleSprite(j, 1)] as CustomFSprite).verticeColors[k] = bsg.blackColor;
                }
            }
            for (int side = 0; side < bsg.legs.GetLength(0); side++)
            {
                for (int leg = 0; leg < bsg.legs.GetLength(1); leg++)
                {
                    for (int part = 0; part < 3; part++)
                    {
                        sLeaser.sprites[bsg.LegSprite(side, leg, part)].color = Color.Lerp(bsg.blackColor, palette.fogColor, Mathf.InverseLerp(1, 5, part * (1 + bsg.darkness)));
                    }
                }
            }
            for (int scale = 0; scale < bsg.scales.Length; scale++)
            {
                for (int part = 0; part < bsg.scales[scale].GetLength(0); part++)
                {
                    float colLerp = (Mathf.InverseLerp(0, bsg.scales[scale].GetLength(0) / 2, part) + Mathf.InverseLerp(-bsg.scales.Length / 2, bsg.scales.Length / 2, scale)) / 2f;
                    sLeaser.sprites[bsg.FirstScaleSprite + (int)bsg.scales[scale][part, 3].x].color = Color.Lerp(bsg.blackColor, bsg.yellowCol, colLerp);
                }
            }

        }
    }
    public static void WinterSpiderSprites(On.BigSpiderGraphics.orig_InitiateSprites orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(bsg, sLeaser, rCam);
        if (bsg?.bug is not null && (bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider || (IsIncanStory(bsg.bug.room?.game) && bsg.bug.Template.type == CreatureTemplate.Type.BigSpider)))
        {
            for (int s = 0; s < sLeaser.sprites.Length; s++)
            {
                if (sLeaser.sprites[s] is not TriangleMesh and not CustomFSprite)
                {
                    sLeaser.sprites[s].scale *=
                        bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider ? 0.2f : 1.15f;
                }
            }
        }
    }
    public static void WinterSpiderVisuals(On.BigSpiderGraphics.orig_DrawSprites orig, BigSpiderGraphics bsg, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(bsg, sLeaser, rCam, timeStacker, camPos);
        if (bsg?.bug?.room is not null && (bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider || IsIncanStory(bsg.bug.room.game)))
        {
            if (bsg.bug.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider || bsg.bug.Template.type == CreatureTemplate.Type.BigSpider)
            {
                if (sLeaser.sprites[bsg.MeshSprite] is TriangleMesh mesh)
                {
                    for (int v = 0; v < mesh.vertices.Length - 1; v++)
                    {
                        Vector2 distance = mesh.vertices[v] - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                        mesh.vertices[v] += distance * 0.15f;
                    }
                }
                for (int k = 0; k < bsg.mandibles.Length; k++)
                {
                    Vector2 dist1 = sLeaser.sprites[bsg.MandibleSprite(k, 0)].GetPosition() - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                    sLeaser.sprites[bsg.MandibleSprite(k, 0)].SetPosition(sLeaser.sprites[bsg.MandibleSprite(k, 0)].GetPosition() + (dist1 * 0.15f));
                    if (sLeaser.sprites[bsg.MandibleSprite(k, 1)] is CustomFSprite cfs)
                    {
                        for (int v = 0; v < cfs.vertices.Length - 1; v++)
                        {
                            Vector2 distance = cfs.vertices[v] - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                            cfs.vertices[v] += distance * 0.15f;
                        }
                    }
                }
                for (int l = 0; l < bsg.totalScales; l++)
                {
                    Vector2 distance = sLeaser.sprites[bsg.FirstScaleSprite + l].GetPosition() - sLeaser.sprites[bsg.HeadSprite].GetPosition();
                    sLeaser.sprites[bsg.FirstScaleSprite + l].SetPosition(sLeaser.sprites[bsg.FirstScaleSprite + l].GetPosition() + (distance * 0.15f));
                }
            }
            else if (bsg.bug.Template.type == HSEnums.CreatureType.PeachSpider)
            {
                for (int s = 0; s < sLeaser.sprites.Length; s++)
                {
                    if (sLeaser.sprites[s] is TriangleMesh mesh)
                    {
                        for (int v = 0; v < mesh.vertices.Length - 1; v++)
                        {
                            mesh.vertices[v] = Vector2.Lerp(mesh.vertices[v], sLeaser.sprites[bsg.HeadSprite].GetPosition(), 0.1f);
                        }
                    }
                    else if (sLeaser.sprites[s] is CustomFSprite cfs)
                    {
                        float mandibleCharge = Mathf.Lerp(bsg.lastMandiblesCharge, bsg.mandiblesCharge, timeStacker);
                        cfs.verticeColors[2] = bsg.yellowCol;
                        cfs.verticeColors[3] = bsg.yellowCol;
                        cfs.verticeColors[0] = Color.Lerp(bsg.blackColor, bsg.yellowCol, (0.6f * (1f - mandibleCharge)) + (0.4f * bsg.darkness));
                        cfs.verticeColors[1] = Color.Lerp(bsg.blackColor, bsg.yellowCol, (0.6f * (1f - mandibleCharge)) + (0.4f * bsg.darkness));
                        for (int v = 0; v < cfs.vertices.Length - 1; v++)
                        {
                            cfs.vertices[v] = Vector2.Lerp(cfs.vertices[v], sLeaser.sprites[bsg.HeadSprite].GetPosition(), 0.3f);
                        }
                    }
                    else if (s != bsg.HeadSprite)
                    {
                        sLeaser.sprites[s].x = Mathf.Lerp(sLeaser.sprites[s].x, sLeaser.sprites[bsg.HeadSprite].x, 0.8f);
                        sLeaser.sprites[s].y = Mathf.Lerp(sLeaser.sprites[s].y, sLeaser.sprites[bsg.HeadSprite].y, 0.8f);
                    }
                }
            }
        }
    }

    #endregion


    //----------------------------------------------------------------------------------


    public static void LuminAntiAbstraction(On.AbstractCreature.orig_Abstractize orig, AbstractCreature absCtr, WorldCoordinate coord)
    {
        orig(absCtr, coord);
        if (absCtr?.abstractAI?.RealAI?.pathFinder is not null && absCtr.state is not null && !absCtr.state.dead && absCtr.state is GlowSpiderState gs && gs.behavior == Hide)
        {
            if (absCtr.abstractAI.RealAI.rainTracker is not null && absCtr.abstractAI.RealAI.rainTracker.Utility() >= 1)
            {
                gs.behavior = EscapeRain;
            }
            else
            {
                absCtr.abstractAI.path.Clear();
                absCtr.abstractAI.RealAI.pathFinder.nextDestination = null;
            }
        }
    }
    public static void LuminHideTimer(On.AbstractCreature.orig_Update orig, AbstractCreature absCtr, int time)
    {
        orig(absCtr, time);
        if (absCtr?.state is not null && !absCtr.state.dead && absCtr.state is GlowSpiderState gs && gs.timeSincePreyLastSeen < gs.timeToWantToHide)
        {
            gs.timeSincePreyLastSeen++;
        }
    }

    public static RelationshipTracker.TrackedCreatureState LuminsGetTrackedByLizards(On.LizardAI.orig_IUseARelationshipTracker_CreateTrackedCreatureState orig, LizardAI lizAI, RelationshipTracker.DynamicRelationship rel) // Make sure you set up the code for lizards reacting to this when you add Icy liz reactions towards Lumins!
    {
        return rel.trackerRep.representedCreature.creatureTemplate.type == HSEnums.CreatureType.Luminescipede
            ? new LizardAI.LizardTrackState()
            : orig(lizAI, rel);
    }


}