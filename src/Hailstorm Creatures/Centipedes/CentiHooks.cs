namespace Hailstorm;

public class CentiHooks
{
    public static void Apply()
    {
        // Main Centi Functions
        _ = new Hook(typeof(Centipede).GetMethod("get_Centiwing", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.Template.type == HSEnums.CreatureType.Cyanwing || orig(cnt));
        _ = new Hook(typeof(Centipede).GetMethod("get_Small", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.Template.type == HSEnums.CreatureType.InfantAquapede || orig(cnt));
        _ = new Hook(typeof(Centipede).GetMethod("get_AquaCenti", Public | NonPublic | Instance), (Func<Centipede, bool> orig, Centipede cnt) => cnt.Template.type == HSEnums.CreatureType.InfantAquapede || orig(cnt));

        On.Centipede.ctor += WinterCentipedes;

        On.Centipede.Update += HailstormCentiUpdate;
        On.Centipede.Crawl += ModifiedCentiCrawling;
        On.Centipede.Fly += ModifiedCentiSwimmingAndFlying;

        On.Centipede.Violence += DMGvsCentis;
        On.Centipede.Stun += CentiStun;
        On.Centipede.BitByPlayer += ConsumeChild;
        On.Centipede.Shock += ChillipedeZappage;
        On.Centipede.Die += CyanwingAlert;
        On.Centipede.UpdateGrasp += HailstormCentiUpdateGrasp;
        ModifiedCentiStuff();

        // Centi AI
        On.CentipedeAI.ctor += HailstormCentipedeAI;
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += CentiwingBravery;
        On.CentipedeAI.Update += CyanwingBabyUpdate;

        // Centi Graphics
        On.CentipedeGraphics.ctor += WinterCentipedeColors;

    }

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == IncanInfo.Incandescent;
    }

    public static ConditionalWeakTable<Centipede, CWT.CentiInfo> CentiData = new();

    public static void WinterCentipedes(On.Centipede.orig_ctor orig, Centipede cnt, AbstractCreature absCnt, World world)
    {
        bool meatNotSetYet = false;
        if (absCnt.state is Centipede.CentipedeState CS && !CS.meatInitated)
        {
            meatNotSetYet = true;
        }

        orig(cnt, absCnt, world);

        CreatureTemplate.Type type = absCnt.creatureTemplate.type;

        if (type == HSEnums.CreatureType.InfantAquapede)
        {
            if (meatNotSetYet)
            {
                cnt.bites = 7;
                (absCnt.state as InfantAquapedeState).remainingBites = cnt.bites;
            }
            return;
        }
        else if (type == HSEnums.CreatureType.Cyanwing)
        {
            if (meatNotSetYet)
            {
                absCnt.state.meatLeft = 12;
            }
            return;
        }
        else if (type == HSEnums.CreatureType.Chillipede)
        {
            Random.State state = Random.state;
            Random.InitState(absCnt.ID.RandomSeed);
            (cnt as Chillipede).AssignSize(absCnt);
            Random.state = state;
            if (meatNotSetYet)
            {
                (cnt as Chillipede).InitiateFoodPips(absCnt);
            }
            return;
        }

        if (!IsIncanStory(world?.game))
        {
            return;
        }

        if (type == CreatureTemplate.Type.RedCentipede ||
            type == CreatureTemplate.Type.Centiwing ||
            type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
        {
            Random.State state = Random.state;
            Random.InitState(absCnt.ID.RandomSeed);
            float sizeFac = 0.5f;
            if (type == CreatureTemplate.Type.Centiwing)
            {
                cnt.size = Random.Range(0.4f, 0.8f);
                sizeFac = Mathf.InverseLerp(0.4f, 0.8f, cnt.size);
            }
            else
            if (type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
            {
                cnt.size = Random.Range(0.6f, 1.2f);
                sizeFac = Mathf.InverseLerp(0.6f, 1.2f, cnt.size);
            }
            else
            if (type == CreatureTemplate.Type.RedCentipede)
            {
                cnt.size = Random.Range(0.9f, 1.1f);
                sizeFac = Mathf.InverseLerp(0.9f, 1.1f, cnt.size);
            }

            if (absCnt.spawnData is not null &&
                absCnt.spawnData.Length > 2 &&
                (type == CreatureTemplate.Type.Centiwing ||
                type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti))
            {
                string s = absCnt.spawnData.Substring(1, absCnt.spawnData.Length - 2);
                try
                {
                    cnt.size = float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
                    sizeFac =
                        type == CreatureTemplate.Type.Centiwing ?
                        Mathf.InverseLerp(0.4f, 0.8f, cnt.size) :
                        Mathf.InverseLerp(0.6f, 1.2f, cnt.size);
                }
                catch
                {
                    // rip lmao
                }
            }

            Random.state = state;

            if (meatNotSetYet)
            {
                if (type == CreatureTemplate.Type.Centiwing)
                {
                    absCnt.state.meatLeft =
                        Mathf.RoundToInt(Mathf.Lerp(2.3f, 4, sizeFac));
                }
                else if (type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
                {
                    absCnt.state.meatLeft =
                        Mathf.RoundToInt(Mathf.Lerp(2.3f, 8, sizeFac));
                }
            }

            cnt.bodyChunks = new BodyChunk[
                type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti ? (int)Mathf.Lerp(7, 27, sizeFac) :
                type == CreatureTemplate.Type.RedCentipede ? (int)Mathf.Lerp(17.33f, 19.66f, sizeFac) :
                (int)Mathf.Lerp(7, 17, cnt.size)];

            for (int i = 0; i < cnt.bodyChunks.Length; i++)
            {
                float bodyLengthProgress = i / (float)(cnt.bodyChunks.Length - 1);
                float chunkRad =
                    Mathf.Lerp(
                        Mathf.Lerp(2f, 3.5f, cnt.size),
                        Mathf.Lerp(4f, 6.5f, cnt.size),
                        Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * bodyLengthProgress), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, cnt.size)));
                float chunkMass =
                    Mathf.Lerp(3 / 70f, 11 / 34f, Mathf.Pow(cnt.size, 1.4f));

                if (type == CreatureTemplate.Type.RedCentipede)
                {
                    chunkRad += 1.5f;
                    chunkMass += 0.02f + (0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, cnt.bodyChunks.Length - 1, i) * Mathf.PI)));
                }
                else if (type == CreatureTemplate.Type.Centiwing)
                {
                    chunkRad = Mathf.Lerp(chunkRad, Mathf.Lerp(2f, 3.5f, chunkRad), 0.4f);
                }

                cnt.bodyChunks[i] = new(cnt, i, default, chunkRad, chunkMass);

            }

            cnt.mainBodyChunkIndex = cnt.bodyChunks.Length / 2;


            if (cnt.CentiState is not null && (cnt.CentiState.shells is null || cnt.CentiState.shells.Length != cnt.bodyChunks.Length))
            {
                cnt.CentiState.shells = new bool[cnt.bodyChunks.Length];
                for (int k = 0; k < cnt.CentiState.shells.Length; k++)
                {
                    cnt.CentiState.shells[k] = Random.value < (cnt.Red ? 0.9f : 0.97f);
                }
            }

            if (cnt.bodyChunkConnections is not null)
            {
                cnt.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[cnt.bodyChunks.Length * (cnt.bodyChunks.Length - 1) / 2];
                int chunkConNum = 0;
                for (int l = 0; l < cnt.bodyChunks.Length; l++)
                {
                    for (int m = l + 1; m < cnt.bodyChunks.Length; m++)
                    {
                        cnt.bodyChunkConnections[chunkConNum] = new(cnt.bodyChunks[l], cnt.bodyChunks[m], (cnt.bodyChunks[l].rad + cnt.bodyChunks[m].rad) * 1.1f, PhysicalObject.BodyChunkConnection.Type.Push, 1f - (cnt.AquaCenti ? 0.7f : 0f), -1f);
                        chunkConNum++;
                    }
                }
            }
        }

        if (!CentiData.TryGetValue(cnt, out _) &&
            cnt is not InfantAquapede &&
            cnt is not Cyanwing &&
            cnt is not Chillipede)
        {
            CentiData.Add(cnt, new CWT.CentiInfo(cnt));
        }

    }


    #region Main Centi Functions
    //----Update----//
    public static void HailstormCentiUpdate(On.Centipede.orig_Update orig, Centipede cnt, bool eu)
    {
        if (cnt is null || !CentiData.TryGetValue(cnt, out CWT.CentiInfo cI))
        {
            orig(cnt, eu);
            return;
        }

        if (cnt.shockCharge > 0 && !cnt.safariControlled)
        {
            cI.Charge = cnt.shockCharge;
            PhysicalObject target = null;
            if (cnt.grabbedBy.Count > 0 && !cnt.dead && cnt.Small)
            {
                cI.Charge += 1 / 60f;
                if (cI.Charge >= 1 && cnt.grabbedBy[0].grabber is not null)
                {
                    target = cnt.grabbedBy[0].grabber;
                }
            }
            if (cI.Charge > 0)
            {
                for (int g = 0; g < cnt.grasps.Length; g++)
                {
                    if (cnt.grasps[g]?.grabbed is null)
                    {
                        continue;
                    }

                    for (int i = 0; i < cnt.grasps[g].grabbed.bodyChunks.Length; i++)
                    {
                        PhysicalObject grabbed = cnt.grasps[g].grabbed;
                        if (grabbed is null)
                        {
                            continue;
                        }
                        cI.Charge += 1 / Mathf.Lerp(100f, 5f, cnt.size);
                        if (cI.Charge >= 1)
                        {
                            target = grabbed;
                        }
                    }
                }
            }
            if (target is not null)
            {
                cnt.shockCharge = 0;
                if (cnt is not InfantAquapede and
                    not Cyanwing and
                    not Chillipede)
                {
                    Fry(cnt, target);
                }
            }
        }

        orig(cnt, eu);

        if (cnt.AquaCenti &&
            cnt.lungs < 0.005f)
        {
            cnt.lungs = 1;
        }

    }
    public static void ModifiedCentiCrawling(On.Centipede.orig_Crawl orig, Centipede cnt)
    {
        if (cnt is null or
            (not Cyanwing and not Chillipede))
        {
            orig(cnt);
            return;
        }

        int flyCounter = cnt.flyModeCounter;
        bool wantToFly = cnt.wantToFly;
        Vector2[] bodyChunkVels = new Vector2[cnt.bodyChunks.Length];
        for (int b = 0; b < cnt.bodyChunks.Length; b++)
        {
            bodyChunkVels[b] = cnt.bodyChunks[b].vel;
        }

        orig(cnt);

        cnt.flyModeCounter = flyCounter;
        cnt.wantToFly = wantToFly;

        if (cnt is Chillipede chl)
        {
            chl.ChillipedeCrawl(bodyChunkVels);
        }
        else if (cnt is Cyanwing cyn)
        {
            cyn.CyanwingCrawl(bodyChunkVels);
        }

    }
    public static void ModifiedCentiSwimmingAndFlying(On.Centipede.orig_Fly orig, Centipede cnt)
    {
        if (cnt is null or
            (not InfantAquapede and not Cyanwing))
        {
            orig(cnt);
            return;
        }

        float bodyWave = cnt.bodyWave;
        Vector2[] bodyChunkVels = new Vector2[cnt.bodyChunks.Length];
        for (int b = 0; b < cnt.bodyChunks.Length; b++)
        {
            bodyChunkVels[b] = cnt.bodyChunks[b].vel;
        }

        orig(cnt);

        cnt.bodyWave = bodyWave;

        if (cnt is InfantAquapede BA)
        {
            BA.InfantAquapedeSwimming(bodyChunkVels);
        }
        else if (cnt is Cyanwing cyn)
        {
            cyn.CyanwingFly(bodyChunkVels);
        }
    }

    //----Centi Functions----//
    public static void DMGvsCentis(On.Centipede.orig_Violence orig, Centipede cnt, BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppen, Creature.DamageType dmgType, float damage, float bonusStun)
    {
        if (CentiData.TryGetValue(cnt, out _) &&
            cnt.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
        {
            damage /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(cnt.Template, dmgType, false);
        }

        if (source is not null && cnt.Template.type == CreatureTemplate.Type.RedCentipede)
        {
            bool hitArmoredSegment =
                hitChunk is not null &&
                cnt.room is not null &&
                hitChunk.index >= 0 &&
                hitChunk.index < cnt.CentiState.shells.Length &&
                cnt.CentiState.shells[hitChunk.index];


            bool hitByIncan =
                source.owner is Player self &&
                IncanInfo.IncanData.TryGetValue(self, out IncanInfo incan) &&
                incan.isIncan &&
                damage < 1;

            bool spearedByIncan =
                source.owner is Spear spr &&
                spr.thrownBy is Player plr &&
                IncanInfo.IncanData.TryGetValue(plr, out incan) &&
                incan.isIncan &&
                damage < 1;

            if (hitArmoredSegment && (hitByIncan || spearedByIncan))
            {
                damage *= 1.34f;
            }
        }

        orig(cnt, source, dirAndMomentum, hitChunk, hitAppen, dmgType, damage, bonusStun);

        if (dmgType == HSEnums.DamageTypes.Cold &&
            cnt is not null &&
            cnt is not Chillipede &&
            !cnt.AquaCenti)
        {
            cnt.stun += 40;
        }
    }
    public static void CentiStun(On.Centipede.orig_Stun orig, Centipede cnt, int stun)
    {
        if (cnt is not null &&
            CentiData.TryGetValue(cnt, out _))
        {
            if (cnt.Centiwing &&
                cnt is not Cyanwing)
            {
                stun = (int)(stun * 0.75f);
            }
            else if (cnt.AquaCenti)
            {
                stun *= (int)Mathf.Lerp(1.1f, 0.66f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
            }
        }
        orig(cnt, stun);
    }
    public static void ConsumeChild(On.Centipede.orig_BitByPlayer orig, Centipede cnt, Creature.Grasp grasp, bool eu)
    {
        if (cnt is not null)
        {
            if (!cnt.dead && cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe && grasp?.grabber is not null)
            {
                cnt.killTag = grasp.grabber.abstractCreature;
            }
        }
        orig(cnt, grasp, eu);
    }
    public static void ChillipedeZappage(On.Centipede.orig_Shock orig, Centipede cnt, PhysicalObject target)
    {
        orig(cnt, target);
        if (target is Chillipede chl)
        {
            int[] shells = new int[chl.bodyChunks.Length];
            for (int s = 0; s < shells.Length; s++)
            {
                shells[s] = s;
            }
            chl.DamageChillipedeShells(shells, (int)(Mathf.InverseLerp(0, chl.TotalMass, cnt.TotalMass) * 3f), cnt.HeadChunk);
        }
    }
    public static void CyanwingAlert(On.Centipede.orig_Die orig, Centipede cnt)
    {
        if (cnt is null || cnt.dead)
        {
            orig(cnt);
            return;
        }

        if (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe &&
            CWT.AbsCtrData.TryGetValue(cnt.abstractCreature, out CWT.AbsCtrInfo aI) &&
            aI.ctrList is not null && aI.ctrList.Count > 0 &&
            aI.ctrList[0]?.abstractAI is not null &&
            cnt.killTag is not null &&
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID) is not null)
        {
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID).like = -1f;
            aI.ctrList[0].state.socialMemory.GetOrInitiateRelationship(cnt.killTag.ID).tempLike = -1f;
            aI.ctrList[0].abstractAI.followCreature = cnt.killTag;
            Debug.Log("[Hailstorm] A Cyanwing's comin' after " + cnt.killTag.ToString() + "!");
        }

        orig(cnt);

    }
    public static void HailstormCentiUpdateGrasp(On.Centipede.orig_UpdateGrasp orig, Centipede cnt, int g)
    {
        orig(cnt, g);

        if (cnt is not null &&
            cnt.grasps[g]?.grabbed is not null &&
            cnt.grasps[1 - g] is null &&
            !cnt.safariControlled)
        {
            if (cnt is Chillipede)
            {
                cnt.bodyChunks[g == 0 ? cnt.bodyChunks.Length - 1 : 0].vel.y += 0.5f;
            }

            BodyChunk OtherHead = cnt.bodyChunks[g == 0 ? (cnt.bodyChunks.Length - 1) : 0];
            for (int i = 0; i < cnt.grasps[g].grabbed.bodyChunks.Length; i++)
            {
                if (Custom.DistLess(OtherHead.pos, cnt.grasps[g].grabbed.bodyChunks[i].pos, OtherHead.rad + cnt.grasps[g].grabbed.bodyChunks[i].rad + 10f))
                {
                    if (!cnt.safariControlled)
                    {
                        if (cnt is InfantAquapede ba && ba.babyCharge >= 1)
                        {
                            ba.BabyShock(ba.grasps[g].grabbed);
                            ba.babyCharge = 0;
                        }
                        if (cnt is Cyanwing cyn && cyn.superCharge >= 1)
                        {
                            cyn.Vaporize(cyn.grasps[g].grabbed);
                            cyn.superCharge = 0;
                        }
                        if (cnt is Chillipede chl && chl.freezeCharge >= 1)
                        {
                            chl.Freeze(chl.grasps[g].grabbed);
                            chl.freezeCharge = 0f;
                        }
                    }
                    break;
                }
            }

        }
    }

    public static void ModifiedCentiStuff()
    {

        IL.Centipede.Stun += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall<Centipede>("get_Centiwing"),
                x => x.MatchBrfalse(out label)))
            {
                c = new(IL);
                if (c.TryGotoNext(
                    MoveType.Before,
                    x => x.MatchCall(typeof(Random), "get_value"),
                    x => x.MatchLdcR4(0.5f),
                    x => x.MatchBlt(out _)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate((Centipede cnt) => cnt.Template.type != HSEnums.CreatureType.Cyanwing && cnt.Template.type != HSEnums.CreatureType.Chillipede);
                    c.Emit(OpCodes.Brfalse, label);
                }
                else
                    Debug.LogError("[Hailstorm] A Cyanwing IL anti-stun hook (part 2) got totally beaned! Report this, would ya?");
            }
            else
                Debug.LogError("[Hailstorm] A Cyanwing IL anti-stun hook (part 1) got totally beaned! Report this, would ya?");
        };

        IL.CentipedeAI.Update += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CentipedeAI>(nameof(CentipedeAI.centipede)),
                x => x.MatchCallvirt<Centipede>("get_Red"),
                x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((CentipedeAI cntAI) => cntAI.centipede.Template.type != HSEnums.CreatureType.Cyanwing);
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Debug.LogError("[Hailstorm] A Cyanwing IL hook for prey-tracking is busted! Tell me about it, please!");
        };

        IL.Centipede.UpdateGrasp += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_safariControlled"),
                x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Centipede cnt) => cnt.Template.type != HSEnums.CreatureType.Cyanwing);
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Debug.LogError("[Hailstorm] A Cyanwing grasp-related IL hook got totally beaned! Report this, would ya?");
        };

        IL.Centipede.Act += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Centipede>(nameof(Centipede.AI)),
                    x => x.MatchCallvirt<ArtificialIntelligence>(nameof(ArtificialIntelligence.Update)))
                &&
                c.TryGotoNext(
                MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Centipede>(nameof(Centipede.flying)),
                    x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Centipede cnt) => cnt.AI.run > 0 && (cnt.Template.type == HSEnums.CreatureType.Cyanwing || cnt.Template.type == HSEnums.CreatureType.Chillipede));
                c.Emit(OpCodes.Brtrue, label);
            }
            else
                Debug.LogError("[Hailstorm] A Cyanwing IL hook for their crawling stopped functioning! Tell me about this, would ya?");
        };

        IL.Centipede.Violence += IL =>
        {
            ILCursor c = new(IL);
            ILLabel label = null;
            if (c.TryGotoNext(
                MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<Centipede>("get_Small"),
                    x => x.MatchBrtrue(out label)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Centipede cnt) => cnt is not Chillipede and not Cyanwing);
                c.Emit(OpCodes.Brfalse, label);
            }
            else
                Debug.LogError("[Hailstorm] A Chillipede Violence IL hook is broken! Tell me about this, please.");
        };

    }

    //----    kill    ----//
    public static void Fry(Centipede cnt, PhysicalObject shockee)
    {
        cnt.room.PlaySound(SoundID.Centipede_Shock, cnt.mainBodyChunk.pos);
        if (cnt.graphicsModule is not null)
        {
            (cnt.graphicsModule as CentipedeGraphics).lightFlash = 1f;
            for (int i = 0; i < (int)Mathf.Lerp(4, 8, cnt.size); i++)
            {
                cnt.room.AddObject(new Spark(cnt.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4, 14, Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
            }
        }
        for (int c = 0; c < cnt.bodyChunks.Length; c++)
        {
            cnt.bodyChunks[c].vel += Custom.RNV() * 6f * Random.value;
            cnt.bodyChunks[c].pos += Custom.RNV() * 6f * Random.value;
        }
        for (int s = 0; s < shockee.bodyChunks.Length; s++)
        {
            shockee.bodyChunks[s].vel += Custom.RNV() * 6f * Random.value;
            shockee.bodyChunks[s].pos += Custom.RNV() * 6f * Random.value;
        }
        if (cnt.AquaCenti)
        {
            if (shockee is Creature aquaCtr)
            {
                if (shockee is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    plr.PyroDeath();
                }
                else
                {
                    float dmg = 2f;
                    int stun = 200;
                    if (IsIncanStory(cnt.room.game))
                    {
                        dmg /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(aquaCtr.Template, Creature.DamageType.Electric, false);
                        stun = (int)(stun / CustomTemplateInfo.DamageResistances.IncanStoryResistances(aquaCtr.Template, Creature.DamageType.Electric, true));
                    }
                    aquaCtr.Violence(cnt.mainBodyChunk, default, aquaCtr.mainBodyChunk, null, Creature.DamageType.Electric, dmg, stun);
                    cnt.room.AddObject(new CreatureSpasmer(aquaCtr, false, aquaCtr.stun));
                    aquaCtr.LoseAllGrasps();
                }
            }
            if (shockee.Submersion > 0f)
            {
                cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.HeadChunk.pos, 14, Mathf.Lerp(50, 100, cnt.size), 1, cnt, new Color(0.7f, 0.7f, 1f)));
            }
            return;
        }
        if (shockee is Creature ctr)
        {
            float ElectricResistance = 1;
            float ElecStunResistance = 1;
            if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0] > 0)
            {
                ElectricResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 0];
            }
            if (ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1] > 0)
            {
                ElecStunResistance *= ctr.Template.damageRestistances[Creature.DamageType.Electric.index, 1];
            }
            if (IsIncanStory(cnt.room.game))
            {
                ElectricResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, Creature.DamageType.Electric, false);
                ElecStunResistance *= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, Creature.DamageType.Electric, true);
            }

            if (cnt.Small)
            {
                ctr.Stun((int)(120 / ElecStunResistance));
                cnt.room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));
                ctr.LoseAllGrasps();
            }
            else if (ctr.TotalMass > shockee.TotalMass * ElectricResistance)
            {
                if (shockee is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    plr.PyroDeath();
                }
                else
                {
                    ctr.Die();
                    cnt.room.AddObject(new CreatureSpasmer(ctr, true, (int)Mathf.Lerp(70, 120, cnt.size)));
                }
            }
            else
            {
                ctr.Stun((int)(Custom.LerpMap(shockee.TotalMass, 0f, cnt.TotalMass * 2f, 300f, 30f) / ElecStunResistance));
                ctr.LoseAllGrasps();
                cnt.room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));

                cnt.Stun(6);
                cnt.shockGiveUpCounter = Math.Max(cnt.shockGiveUpCounter, 30);
                cnt.AI.annoyingCollisions = Math.Min(cnt.AI.annoyingCollisions / 2, 150);
            }


            if (ctr is Chillipede chl)
            {
                int[] shells = new int[chl.bodyChunks.Length];
                for (int s = 0; s < shells.Length; s++)
                {
                    shells[s] = s;
                }
                chl.DamageChillipedeShells(shells, (int)Mathf.Lerp(1, 8, cnt.size), cnt.HeadChunk);
            }

        }

        if (shockee.Submersion > 0f)
        {
            cnt.room.AddObject(new UnderwaterShock(cnt.room, cnt, cnt.HeadChunk.pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, cnt.size), 0.2f + (1.9f * cnt.size), cnt, new Color(0.7f, 0.7f, 1f)));
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Centi AI

    public static void HailstormCentipedeAI(On.CentipedeAI.orig_ctor orig, CentipedeAI cntAI, AbstractCreature absCnt, World world)
    {
        orig(cntAI, absCnt, world);
        if (cntAI?.centipede is null)
        {
            return;
        }

        if (cntAI.centipede is Cyanwing)
        {
            cntAI.pathFinder.stepsPerFrame = 15;
            cntAI.preyTracker.persistanceBias = 4f;
            cntAI.preyTracker.sureToGetPreyDistance = 150f;
            cntAI.preyTracker.sureToLosePreyDistance = 600f;
            cntAI.utilityComparer.GetUtilityTracker(cntAI.preyTracker).weight = 1.5f;
        }

        if (absCnt.creatureTemplate.type == CreatureTemplate.Type.SmallCentipede && absCnt.superSizeMe &&
            CWT.AbsCtrData.TryGetValue(absCnt, out CWT.AbsCtrInfo aI) && (aI.ctrList is null || aI.ctrList.Count < 1))
        {
            FindBabyCentiwingMother(absCnt, world, aI);
        }

        if (cntAI.centipede is Chillipede)
        {
            cntAI.preyTracker.sureToGetPreyDistance = 25f;
            cntAI.preyTracker.sureToLosePreyDistance = 200f;
        }
    }
    public static void CyanwingBabyUpdate(On.CentipedeAI.orig_Update orig, CentipedeAI cntAI)
    {
        if (cntAI?.centipede is null)
        {
            orig(cntAI);
            return;
        }
        Centipede cnt = cntAI.centipede;

        orig(cntAI);

        if (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cntAI.creature.superSizeMe && CWT.AbsCtrData.TryGetValue(cntAI.creature, out CWT.AbsCtrInfo aI) && (aI.ctrList is null || aI.ctrList.Count < 1 || aI.ctrList[0].state.dead))
        {
            FindBabyCentiwingMother(cntAI.creature, cntAI.creature.world, aI);
        }

    }
    public static CreatureTemplate.Relationship CentiwingBravery(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI cntAI, RelationshipTracker.DynamicRelationship dynamRelat)
    {
        if (cntAI?.centipede is not null && CentiData.TryGetValue(cntAI.centipede, out _))
        {
            CreatureTemplate.Relationship defaultRelation = cntAI.StaticRelationship(dynamRelat.trackerRep.representedCreature);
            Creature ctr = dynamRelat.trackerRep.representedCreature.realizedCreature;
            Centipede cnt = cntAI.centipede;
            if (cnt.Centiwing &&
                cnt is not Cyanwing &&
                ctr is not null &&
                !ctr.dead)
            {
                if (defaultRelation.type == CreatureTemplate.Relationship.Type.Eats && ctr.TotalMass < cnt.TotalMass)
                {
                    if (IsIncanStory(cntAI?.centipede?.room?.game) && cnt.Template.type == CreatureTemplate.Type.Centiwing)
                    {
                        float massFac = Mathf.Pow(Mathf.InverseLerp(0f, cnt.TotalMass, ctr.TotalMass), 0.75f);
                        float courageThreshold = Mathf.Lerp(360, 100, Mathf.InverseLerp(0.4f, 1, cnt.size));
                        if (dynamRelat.trackerRep.age < Mathf.Lerp(360, 100, Mathf.InverseLerp(0.4f, 1, cnt.size)))
                        {
                            massFac *= 1f - cntAI.OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                            return new CreatureTemplate.Relationship
                                (CreatureTemplate.Relationship.Type.Afraid, massFac * Mathf.InverseLerp(courageThreshold, 0f, dynamRelat.trackerRep.age));
                        }
                        massFac *= 1f - cntAI.OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                        return new CreatureTemplate.Relationship
                            (CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(courageThreshold, courageThreshold * 2.66f, dynamRelat.trackerRep.age) * massFac);
                    }
                }
            }
        }
        return orig(cntAI, dynamRelat);
    }

    //------------------------------------------

    public static void FindBabyCentiwingMother(AbstractCreature absCnt, World world, CWT.AbsCtrInfo aI)
    {
        if (absCnt is null || world is null) return;

        if (aI.ctrList is null)
        {
            aI.ctrList = new List<AbstractCreature>();
        }
        else if (aI.ctrList.Count > 0 && aI.ctrList[0].state.dead)
        {
            aI.ctrList.Clear();
        }

        if (absCnt.Room is not null)
        {
            foreach (AbstractCreature ctr in absCnt.Room.creatures)
            {
                if (ctr is not null &&
                    ctr.state is CyanwingState &&
                    ctr.state.alive)
                {
                    aI.ctrList.Add(ctr);
                    return;
                }
            }
        }

        foreach (AbstractRoom room in world.abstractRooms)
        {
            if (room is null) continue;

            foreach (AbstractCreature ctr in room.creatures)
            {
                if (ctr is not null &&
                    ctr.state is CyanwingState &&
                    ctr.state.alive)
                {
                    aI.ctrList.Add(ctr);
                }
            }
            foreach (AbstractWorldEntity denEntity in room.entitiesInDens)
            {
                if (denEntity is not null &&
                    denEntity is AbstractCreature denCtr &&
                    denCtr.state is CyanwingState &&
                    denCtr.state.alive)
                {
                    aI.ctrList.Add(denCtr);
                }
            }
        }

        for (; aI.ctrList.Count > 1;)
        {
            if (aI.ctrList[0] is null || aI.ctrList[1] is null) continue;

            if (Custom.WorldCoordFloatDist(aI.ctrList[0].pos, absCnt.pos) > Custom.WorldCoordFloatDist(aI.ctrList[1].pos, absCnt.pos))
            {
                aI.ctrList.RemoveAt(0);
            }
            else if (Custom.WorldCoordFloatDist(aI.ctrList[1].pos, absCnt.pos) > Custom.WorldCoordFloatDist(aI.ctrList[0].pos, absCnt.pos))
            {
                aI.ctrList.RemoveAt(1);
            }
            else
                aI.ctrList.RemoveAt(Random.Range(0, 2));
        }
    }

    #endregion

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Centi Graphics

    public static void WinterCentipedeColors(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics cg, PhysicalObject centi)
    {
        orig(cg, centi);
        if (cg?.centipede is null)
        {
            return;
        }
        Centipede cnt = cg.centipede;

        if (CentiData.TryGetValue(cg.centipede, out _))
        {
            Random.State state = Random.state;
            Random.InitState(cnt.abstractCreature.ID.RandomSeed);
            float range = Mathf.Lerp(0.16f, 0.12f, Mathf.InverseLerp(0f, 1f, cnt.size));
            float skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0f, 1f, cnt.size));

            if (cnt.Template.type == CreatureTemplate.Type.Centiwing ||
                (cnt.Template.type == CreatureTemplate.Type.SmallCentipede && cnt.abstractCreature.superSizeMe))
            {
                range = Mathf.Lerp(0, 20f, Mathf.InverseLerp(0.4f, 0.8f, cnt.size));
                skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.4f, 0.8f, cnt.size));
                cg.hue = Mathf.Lerp((80 + range) / 360f, (160 + range) / 360f, Mathf.Pow(Random.value, skew));
                if (!cnt.Small)
                {
                    cg.saturation = Mathf.Clamp(cnt.size, 0, 1);
                }
            }
            else if (cnt.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
            {
                range = Mathf.Lerp(0, 20f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
                skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.6f, 1.2f, cnt.size));
                cg.hue = Mathf.Lerp((240 - range) / 360f, (200 - range) / 360f, Mathf.Pow(Random.value, skew));
            }
            else if (cnt.Template.type == CreatureTemplate.Type.RedCentipede)
            {
                skew = Mathf.Lerp(1.6f, 0.4f, Mathf.InverseLerp(0.9f, 1.1f, cnt.size));
                cg.hue = Mathf.Lerp(-0.06f, 0.03f, Mathf.Pow(Random.value, skew));
            }
            else if (cnt.Template.type == CreatureTemplate.Type.Centipede ||
                cnt.Template.type == CreatureTemplate.Type.SmallCentipede)
            {
                cg.hue = Mathf.Lerp(0.04f, range, Mathf.Pow(Random.value, skew));
            }
            Random.state = state;
        }
    }

    #endregion
}