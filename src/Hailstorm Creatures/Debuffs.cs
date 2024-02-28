namespace Hailstorm;

public abstract class Debuff
{
    public AbstractPhysicalObject source;
    public int? chunk;
    public Creature appendagePos;
    public float duration;

    public Color mainColor;
    public Color secondColor;

    public Debuff (AbstractPhysicalObject inflicter, int? hitChunk, int debuffDuration, Color mainColor, Color secondColor)
    {
        source = inflicter;
        chunk = hitChunk;
        duration = debuffDuration;
        this.mainColor = mainColor;
        this.secondColor = secondColor;
    }

    public virtual void DebuffUpdate(Creature victim, bool setKillTag)
    {

    }

    public virtual void DebuffVisuals(Creature victim, bool eu)
    {

    }
}

public class Burn : Debuff
{
    public HailstormFireSmokeCreator smoke;
    public LightSource glow;
    public float[,] flicker;

    public Burn(AbstractPhysicalObject igniter, int? hitChunk, int debuffDuration, Color baseFireColor, Color fadeFireColor) : base (igniter, hitChunk, debuffDuration, baseFireColor, fadeFireColor)
    {
        flicker = new float[2, 3];
    }

    public override void DebuffUpdate(Creature victim, bool setKillTag)
    {
        if (duration < 1 || victim?.room is null)
        {
            return;
        }

        if (victim.Submersion > 0.5f)
        {
            duration -= 40;
        }

        if (duration % 40 == 0)
        {

            victim.room.PlaySound(SoundID.Firecracker_Burn, victim.bodyChunks[chunk.Value].pos, 0.3f, Random.Range(0.3f, 0.4f));

            if (victim is Player self)
            {
                float HeatDamageMult = CustomTemplateInfo.DamageResistances.SlugcatDamageMultipliers(self, HailstormDamageTypes.Heat);
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
            else if (victim.State is HealthState HP)
            {
                float BurnRes = victim.Template.damageRestistances[HailstormDamageTypes.Heat.index, 0];
                HP.health -= 0.035f / Mathf.Lerp(victim.Template.baseDamageResistance, 1f, 0.25f) / BurnRes;
                if (victim.Hypothermia > 0.001f) victim.Hypothermia -= 0.1f / Mathf.Lerp(BurnRes, 1, 0.5f);
            }
            else if (Random.value < 0.1f && victim is not Player)
            {
                victim.Die();
            }

            if (victim.State is DaddyLongLegs.DaddyState DS && DS.tentacleHealth is not null && DS.tentacleHealth.Length > 0)
            {
                for (int t = 0; t < DS.tentacleHealth.Length; t++)
                {
                    DS.tentacleHealth[t] -= 0.02f;
                }
            }
        }

        if (source is AbstractBurnSpear brnSpr)
        {
            if (brnSpr.heat <= 0 || brnSpr?.realizedObject is null || brnSpr.realizedObject is not Spear spr || spr.stuckInObject is null || spr.stuckInObject != victim)
            {
                duration--;
            }
            else brnSpr.heat -= 0.00005f;

            if (setKillTag && brnSpr.realizedObject is not null && brnSpr.realizedObject is Spear kSpr && kSpr.thrownBy is not null && kSpr.thrownBy.abstractCreature != victim.killTag)
            {
                victim.SetKillTag(kSpr.thrownBy.abstractCreature);
                victim.killTagCounter = (int)duration + 160;
            }
        }
        else if (setKillTag && source is AbstractCreature killTag && (killTag != victim.killTag || victim.killTagCounter < duration + 159))
        {
            victim.SetKillTag(killTag);
            victim.killTagCounter = Mathf.Max(victim.killTagCounter, (int)duration + 160);
        }

        if (victim.State is GlowSpiderState gs && gs.juice < gs.MaxJuice)
        {
            gs.juice += 0.004f;
        }

    }

    public override void DebuffVisuals(Creature victim, bool eu)
    {
        if (duration < 1 || victim?.room is null)
        {
            if (duration < 1)
            {
                glow = null;
                smoke = null;
                if (victim is not null && victim.Submersion > 0.5f)
                {
                    chunk ??= Random.Range(0, victim.bodyChunks.Length - 1);
                    victim.room.PlaySound(SoundID.Firecracker_Disintegrate, victim.bodyChunks[chunk.Value]);
                }
            }
            return;
        }

        chunk = chunk is not null ? chunk : Random.Range(0, victim.bodyChunks.Length - 1);

        for (int i = 0; i < flicker.GetLength(0); i++)
        {
            flicker[i, 1] = flicker[i, 0];
            flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * (Random.value < 0.5f ? -1f : 1f);
            flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 1f / 30f);
            if (Random.value < 0.2f)
            {
                flicker[i, 2] = 1f + Mathf.Pow(Random.value, 3f) * 0.2f * (Random.value < 0.5f ? -1f : 1f);
            }
            flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
        }

        if (glow is null)
        {
            glow = new LightSource(victim.bodyChunks[chunk.Value].pos, false, mainColor, null, false)
            {
                requireUpKeep = true,
                setAlpha = new float?(1)
            };
            victim.room.AddObject(glow);
        }
        else if (glow is not null)
        {
            glow.stayAlive = true;
            glow.setPos = new Vector2?(victim.bodyChunks[chunk.Value].pos);
            glow.setRad = new float?(Mathf.Lerp(50, 400, Mathf.InverseLerp(0, 1600, duration)));
            glow.color = Color.Lerp(secondColor, mainColor, Mathf.InverseLerp(80, 800, duration));
            glow.affectedByPaletteDarkness = Mathf.InverseLerp(800, 0, duration);
            if (glow.slatedForDeletetion || glow.room != victim.room)
            {
                glow = null;
            }
        }

        int burnCount = 0;
        if (CWT.AbsCtrData.TryGetValue(victim.abstractCreature, out AbsCtrInfo aI) && aI.debuffs is not null)
        {
            foreach (Debuff debuff in aI.debuffs)
            {
                if (debuff is Burn) burnCount++;
            }
        }
        if (duration % (burnCount > 0 ? burnCount : 3) == 0) // Creates smoke at a lower rate the more burns are applied.
        {
            smoke ??= new HailstormFireSmokeCreator(victim.room);
            if (smoke is not null)
            {
                smoke.Update(eu);
                if (victim.room.ViewedByAnyCamera(victim.DangerPos, 350f) && smoke.AddParticle(victim.bodyChunks[Random.Range(0, victim.bodyChunks.Length - 1)].pos, new Vector2(Random.Range(-6f, 6f), Random.Range(4f, 6f)), 35) is Smoke.FireSmoke.FireSmokeParticle fireSmoke)
                {
                    fireSmoke.colorFadeTime = 35;
                    fireSmoke.effectColor = mainColor;
                    fireSmoke.colorA = secondColor;
                    fireSmoke.rad *= Mathf.Lerp(0.75f, 3.75f, Mathf.InverseLerp(20, 1600, duration));
                }
                if (victim.appendages is not null && victim.appendages.Count > 0)
                {
                    foreach (PhysicalObject.Appendage appendage in victim.appendages)
                    {
                        if (victim.room.ViewedByAnyCamera(victim.DangerPos, 350f) && smoke.AddParticle(appendage.segments[Random.Range(0, appendage.segments.Length - 1)], new Vector2(Random.Range(-6f, 6f), Random.Range(4f, 6f)), 35) is Smoke.FireSmoke.FireSmokeParticle appendageFireSmoke)
                        {
                            appendageFireSmoke.colorFadeTime = 35;
                            appendageFireSmoke.effectColor = mainColor;
                            appendageFireSmoke.colorA = secondColor;
                            appendageFireSmoke.rad *= Mathf.Lerp(0.75f, 3.75f, Mathf.InverseLerp(20, 1600, duration));
                        }
                    }
                }
                if (smoke.Dead || smoke.room != victim.room)
                {
                    smoke = null;
                }
            }
        }

    }

    public static bool IsCreatureBurning(Creature ctr)
    {
        if (ctr?.abstractCreature is null) return false;

        if (CWT.AbsCtrData.TryGetValue(ctr.abstractCreature, out AbsCtrInfo aI) && aI.debuffs is not null && aI.debuffs.Count > 0)
        {
            foreach (Debuff debuff in aI.debuffs)
            {
                if (debuff is Burn) return true;
            }
        }
        return false;
    }
}