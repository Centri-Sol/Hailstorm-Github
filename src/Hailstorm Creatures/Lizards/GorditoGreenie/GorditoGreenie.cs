namespace Hailstorm;

public class GorditoGreenie : Lizard
{
    public GorditoGraphics GorditoGraphics => graphicsModule as GorditoGraphics;

    public Color effectColor2;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public int impactCooldown;

    public int jumpWindUpTimer;
    public int jumpWindUpTimerGoal;
    public virtual int MaxJumpWindUpTimer => 220;
    public virtual float JumpPower => Mathf.InverseLerp(0, MaxJumpWindUpTimer, jumpWindUpTimer);

    public int BounceDir;

    public float minBounceVel;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public GorditoGreenie(AbstractCreature absLiz, World world) : base(absLiz, world)
    {
        Random.State state = Random.state;
        Random.InitState(absLiz.ID.RandomSeed);
        HSLColor col = new(Custom.WrappedRandomVariation(140 / 360f, 30 / 360f, 0.2f), Random.Range(0.45f, 0.9f), Custom.WrappedRandomVariation(0.8f, 0.1f, 0.33f));
        Random.state = state;
        effectColor = col.rgb;
        col.saturation += 0.1f;
        col.lightness *= 0.6f;
        effectColor2 = col.rgb;

        for (int b = 1; b < bodyChunks.Length; b++)
        {
            bodyChunks[b].rad *= 2.5f;
        }
        bodyChunkConnections[1].distance *= 0.66f;

        minBounceVel = 14f;
    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new GorditoGraphics(this);
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {

        base.Update(eu);
        if (room is null || slatedForDeletetion)
        {
            return;
        }

        if (impactCooldown > 0)
        {
            impactCooldown--;
        }

        LungeUpdate();

    }
    public virtual void LungeUpdate()
    {

        if (animation == Animation.PrepareToLounge)
        {
            if (JumpPower < 1)
            {
                jumpWindUpTimer++;
                Vector2 vib = Custom.RNV();
                for (int b = 0; b < bodyChunks.Length; b++)
                {
                    bodyChunks[b].pos += vib * Mathf.InverseLerp(-1, bodyChunks.Length, b) * JumpPower;
                }
            }
            else
            {
                EnterAnimation(Animation.Lounge, false);
            }
        }
        else if (jumpWindUpTimer > 0)
        {
            jumpWindUpTimer = 0;
        }

        if (animation == Animation.Lounge)
        {
            foreach (BodyChunk chunk in bodyChunks)
            {
                if (Mathf.Abs(chunk.vel.x) < 4)
                {
                    chunk.vel.x = Custom.LerpMap(chunk.vel.x * BounceDir, 4, 0, 4, 6) * BounceDir;
                }
            }
        }

    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType dmgType, float dmg, float bonusStun)
    {
        if (room is not null &&
            hitChunk is not null)
        {
            room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, hitChunk.pos, 1.2f, 0.6f);
            turnedByRockCounter = 10;
            if (source is not null) // Gorditos will turn to face attackers when hit.
            {
                turnedByRockDirection = (int)Mathf.Sign(source.lastPos.x - source.pos.x);
            }
            if (dmgType == DamageType.Electric)
            {
                dmg += 0.25f;
            }

            if (hitChunk.index == 0)
            {
                stun += 20;
                dmg *= 2f;
                room.PlaySound(SoundID.Spear_Stick_In_Creature, hitChunk, false, 1, 0.7f);
            }

        }
        base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, dmgType, dmg, bonusStun);
    }
    public override void Stun(int stun)
    {
        stun = Mathf.Min(20, stun);
        base.Stun(stun);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override void TerrainImpact(int myChunk, IntVector2 direction, float speed, bool firstContact)
    {

        if (firstContact &&
            speed > 5 &&
            animation == Animation.Lounge)
        {
            if (direction.x == BounceDir)
            {
                float pitch = Random.Range(0.1f, 0.5f);
                room.PlaySound(SoundID.Rock_Hit_Wall, bodyChunks[myChunk], false, 1 + pitch, pitch);
                foreach (BodyChunk chunk in bodyChunks)
                {
                    if (Mathf.Sign(chunk.vel.x) == BounceDir)
                    {
                        chunk.vel.x *= -1;
                    }
                }
                BounceDir *= -1;
                Vector2 sparkPos = bodyChunks[myChunk].pos + (bodyChunks[myChunk].vel.normalized * bodyChunks[myChunk].vel.magnitude);
                int sparkCount = Mathf.Min(10, (int)speed / 2);
                for (int i = sparkCount; i >= 0; i--)
                {
                    EmitSpark(sparkPos + (Custom.RNV() * 20f * Random.value), bodyChunks[myChunk].vel + (Custom.RNV() * speed * 0.25f));
                }

            }
            foreach (BodyChunk chunk in bodyChunks)
            {
                if (direction.y != 0 && Mathf.Abs(chunk.vel.y) < (direction.y > 0 ? 3 : 5)) // Bounces liz off of floors and ceilings
                {
                    chunk.vel.y = 5 * Mathf.Sign(chunk.vel.y);
                }

                if (direction.y == -1)
                {
                    float pitch = Random.Range(0.1f, 0.5f);
                    room.PlaySound(SoundID.Rock_Hit_Wall, chunk, false, 1 + pitch, pitch);

                    chunk.vel.y *= -1.4f;

                    Vector2 sparkPos = chunk.pos + (chunk.vel.normalized * chunk.vel.magnitude);
                    int sparkCount = Mathf.Min(10, (int)speed / 2);
                    for (int i = sparkCount; i >= 0; i--)
                    {
                        EmitSpark(sparkPos + (Custom.RNV() * 20f * Random.value), chunk.vel + (Custom.RNV() * speed * 0.25f));
                    }

                    if (chunk.vel.y < minBounceVel)
                    {
                        chunk.vel.y = minBounceVel * Random.Range(1, 1.4f);
                    }
                }
            }
        }

        base.TerrainImpact(myChunk, direction, speed, firstContact);

    }

    public override void Collide(PhysicalObject victim, int myChunk, int otherChunk)
    {
        if (victim is not null &&
            victim is Creature ctr &&
            CWT.CreatureData.TryGetValue(ctr, out CWT.CreatureInfo vI) &&
            CanCRONCH(ctr, impactCooldown, vI.impactCooldown))
        {
            stun = 20;
            impactCooldown = 40;
            vI.impactCooldown = 40;
            float DAMAGE = Mathf.Max(1, (int)(-mainBodyChunk.vel.y - 5) / 5);
            int STUN = 45 * (int)DAMAGE;
            if (ctr is Player ||
                ctr is Chillipede ||
                ctr.Template.type == CreatureTemplate.Type.RedLizard ||
                ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard ||
                ctr.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard ||
                ctr.TotalMass * 2 > TotalMass)
            {
                DAMAGE /= 2f;
            }

            float vol = ctr.dead ? 0.7f : 1.4f;
            room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, bodyChunks[myChunk], false, vol, 1.1f);
            room.PlaySound(SoundID.Lizard_Heavy_Terrain_Impact, bodyChunks[myChunk], false, vol, 1.1f);
            if (!ctr.State.dead)
            {
                ctr.Violence(bodyChunks[myChunk], bodyChunks[myChunk].vel, ctr.bodyChunks[otherChunk], null, DamageType.Blunt, DAMAGE, STUN);
                room.PlaySound(SoundID.Rock_Hit_Wall, bodyChunks[myChunk], false, 1.5f, 0.5f - (DAMAGE / 12.5f));
                if (ctr.State.dead || (ctr.State is HealthState HS && HS.ClampedHealth == 0))
                {
                    room.PlaySound(SoundID.Spear_Stick_In_Creature, bodyChunks[myChunk], false, 1.7f, 0.85f);
                }
            }

        }
        base.Collide(victim, myChunk, otherChunk);
    }
    public virtual bool CanCRONCH(Creature ctr, float collTimer1, float collTimer2)
    {
        if (ctr is GorditoGreenie)
        {
            return false;
        }
        if (collTimer1 > 0 &&
            collTimer2 > 0)
        {
            return false;
        }
        if (gravity == 0f ||
            Submersion > 0.2f)
        {
            return false;
        }
        bool falling = mainBodyChunk.vel.y <= -10 && bodyChunks is not null;
        if (enteringShortCut.HasValue || !(falling || (animation == Animation.Lounge && Mathf.Abs(mainBodyChunk.vel.x) > 5)))
        {
            if (!falling)
            {
                return false;
            }
            foreach (BodyChunk chunk in bodyChunks)
            {
                if (chunk.contactPoint.y == -1) return false;
            }
        }
        if (bodyChunks[1].vel.magnitude < bodyChunks[0].vel.magnitude)
        {
            bodyChunks[0].vel = bodyChunks[1].vel;
        }
        foreach (Grasp grasp in grabbedBy)
        {
            if (grasp.pacifying ||
                grasp.grabber == ctr)
            {
                return false;
            }
        }
        return true;

    }




    public virtual void EmitSpark(Vector2 pos, Vector2 vel)
    {
        Color col = (Random.value < 1 / 3f) ? effectColor2 : effectColor;
        room.AddObject(new Spark(pos, vel, col, null, 10, 40));
    }




}