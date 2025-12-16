namespace Hailstorm;

public class Burn : Debuff
{
    public HailstormFireSmokeCreator smoke;
    public LightSource glow;
    public float[,] flicker;
    public ChunkDynamicSoundLoop BurnLOOPSFX;

    public Burn(AbstractPhysicalObject igniter, int? hitChunk, int debuffDuration, Color baseFireColor, Color fadeFireColor) : base(igniter, hitChunk, debuffDuration, baseFireColor, fadeFireColor)
    {
        flicker = new float[2, 3];
    }

    public override void Update(Creature owner, bool setKillTag)
    {
        if (duration < 1 || owner?.room is null)
        {
            return;
        }

        bool submerged = owner.Submersion > 0.5f;

        if (submerged)
        {
            duration -= 40;
        }

        if (BurnLOOPSFX is null)
        {
            BurnLOOPSFX = new ChunkDynamicSoundLoop(owner.bodyChunks[chunk.Value]);
            BurnLOOPSFX.sound = HSEnums.Sound.FireBurnLOOP;
        }
        BurnLOOPSFX.Update();
        BurnLOOPSFX.Volume = Mathf.InverseLerp(0, submerged ? 240 : 40, duration);

        if (duration % 40 == 0)
        {

            if (owner is Player self)
            {
                float HeatDamageMult = CustomTemplateInfo.DamageResistances.SlugcatDamageMultipliers(self, HSEnums.DamageTypes.Heat);
                self.playerState.permanentDamageTracking += 0.04f * HeatDamageMult;
                if (self.playerState.permanentDamageTracking >= 1)
                {
                    self.Die();
                }
                if (self.Hypothermia > 0.001f)
                {
                    self.Hypothermia -= 0.1f * Mathf.Lerp(HeatDamageMult, 1, 0.5f);
                }
            }
            else if (owner.State is HealthState HP)
            {
                float BurnRes = owner.Template.damageRestistances[HSEnums.DamageTypes.Heat.index, 0];
                float DMG = 0.035f / Mathf.Lerp(owner.Template.baseDamageResistance, 1f, 0.25f) / BurnRes;
                if (owner is Centipede cnt and not Chillipede)
                {
                    DMG *= Mathf.Lerp(1.3f, 0.075f, Mathf.Pow(cnt.size, 0.5f));
                }
                HP.health -= DMG;

                if (owner.Hypothermia > 0.001f)
                {
                    owner.Hypothermia -= 0.1f / Mathf.Lerp(BurnRes, 1, 0.5f);
                }
            }
            else if (Random.value < 0.1f && owner is not Player)
            {
                owner.Die();
            }

            if (owner.State is DaddyLongLegs.DaddyState DS && DS.tentacleHealth is not null && DS.tentacleHealth.Length > 0)
            {
                for (int t = 0; t < DS.tentacleHealth.Length; t++)
                {
                    DS.tentacleHealth[t] -= 0.02f;
                }
            }
        }

        if (source is AbstractBurnSpear brnSpr)
        {
            if (brnSpr.heat <= 0 || brnSpr?.realizedObject is null || brnSpr.realizedObject is not Spear spr || spr.stuckInObject is null || spr.stuckInObject != owner)
            {
                duration--;
            }
            else brnSpr.heat -= 0.00005f;

            if (setKillTag && brnSpr.realizedObject is not null && brnSpr.realizedObject is Spear kSpr && kSpr.thrownBy is not null && kSpr.thrownBy.abstractCreature != owner.killTag)
            {
                owner.SetKillTag(kSpr.thrownBy.abstractCreature);
                owner.killTagCounter = (int)duration + 160;
            }
        }
        else if (setKillTag && source is AbstractCreature killTag && (killTag != owner.killTag || owner.killTagCounter < duration + 159))
        {
            owner.SetKillTag(killTag);
            owner.killTagCounter = Mathf.Max(owner.killTagCounter, (int)duration + 160);
        }

        if (owner.State is GlowSpiderState gs && gs.juice < gs.MaxJuice)
        {
            gs.juice += 0.004f;
        }

    }

    public override void Visuals(Creature owner, bool eu)
    {
        if (duration < 1 || owner?.room is null)
        {
            if (duration < 1)
            {
                glow = null;
                smoke = null;
                if (owner is not null && owner.Submersion > 0.5f)
                {
                    chunk ??= Random.Range(0, owner.bodyChunks.Length - 1);
                    owner.room.PlaySound(SoundID.Firecracker_Disintegrate, owner.bodyChunks[chunk.Value]);
                }
            }
            return;
        }

        chunk = chunk is not null ? chunk : Random.Range(0, owner.bodyChunks.Length - 1);

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

        if (glow is null)
        {
            glow = new LightSource(owner.bodyChunks[chunk.Value].pos, false, mainColor, null, false)
            {
                requireUpKeep = true,
                setAlpha = new float?(1)
            };
            owner.room.AddObject(glow);
        }
        else if (glow is not null)
        {
            glow.stayAlive = true;
            glow.setPos = new Vector2?(owner.bodyChunks[chunk.Value].pos);
            glow.setRad = new float?(Mathf.Lerp(50, 400, Mathf.InverseLerp(0, 1600, duration)));
            glow.color = Color.Lerp(secondColor, mainColor, Mathf.InverseLerp(80, 800, duration));
            glow.affectedByPaletteDarkness = Mathf.InverseLerp(800, 0, duration);
            if (glow.slatedForDeletetion || glow.room != owner.room)
            {
                glow = null;
            }
        }

        int burnCount = 0;
        if (CWT.AbsCtrData.TryGetValue(owner.abstractCreature, out CWT.AbsCtrInfo aI) && aI.debuffs is not null)
        {
            foreach (Debuff debuff in aI.debuffs)
            {
                if (debuff is Burn) burnCount++;
            }
        }
        if (duration % (burnCount > 0 ? burnCount : 3) == 0) // Creates smoke at a lower rate the more burns are applied.
        {
            smoke ??= new HailstormFireSmokeCreator(owner.room);
            if (smoke is not null)
            {
                smoke.Update(eu);
                if (owner.room.ViewedByAnyCamera(owner.DangerPos, 350f) && smoke.AddParticle(owner.bodyChunks[Random.Range(0, owner.bodyChunks.Length - 1)].pos, new Vector2(Random.Range(-6f, 6f), Random.Range(4f, 6f)), 35) is Smoke.FireSmoke.FireSmokeParticle fireSmoke)
                {
                    fireSmoke.colorFadeTime = 35;
                    fireSmoke.effectColor = mainColor;
                    fireSmoke.colorA = secondColor;
                    fireSmoke.rad *= Mathf.Lerp(0.75f, 3.75f, Mathf.InverseLerp(20, 1600, duration));
                }
                if (owner.appendages is not null && owner.appendages.Count > 0)
                {
                    foreach (PhysicalObject.Appendage appendage in owner.appendages)
                    {
                        if (owner.room.ViewedByAnyCamera(owner.DangerPos, 350f) && smoke.AddParticle(appendage.segments[Random.Range(0, appendage.segments.Length - 1)], new Vector2(Random.Range(-6f, 6f), Random.Range(4f, 6f)), 35) is Smoke.FireSmoke.FireSmokeParticle appendageFireSmoke)
                        {
                            appendageFireSmoke.colorFadeTime = 35;
                            appendageFireSmoke.effectColor = mainColor;
                            appendageFireSmoke.colorA = secondColor;
                            appendageFireSmoke.rad *= Mathf.Lerp(0.75f, 3.75f, Mathf.InverseLerp(20, 1600, duration));
                        }
                    }
                }
                if (smoke.Dead || smoke.room != owner.room)
                {
                    smoke = null;
                }
            }
        }

    }

}