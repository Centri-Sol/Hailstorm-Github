namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class Chillipede : Centipede
{
    public ChillipedeState ChillState => abstractCreature.state as ChillipedeState;
    public ChillipedeGraphics ChillGraphics => graphicsModule as ChillipedeGraphics;
    public virtual float RegenPerShell => 0.1f;
    public virtual int MaxShellHP => 3;

    public virtual bool SmallSize => bodyChunks.Length <= 3;
    public virtual bool NormalSize => bodyChunks.Length > 3 && bodyChunks.Length < 7;
    public virtual bool LargeSize => bodyChunks.Length >= 7;
    public virtual float DefaultTerrainCollisionMult => 0.9f;

    public virtual float FreezeRadius => 200;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public float MovementSpeed;

    public float freezeCharge;

    public int ChillipedeTileLeniency;

    public Chillipede(AbstractCreature absChl, World world) : base(absChl, world)
    {
        bodyChunks = new BodyChunk[(int)Mathf.Lerp(2, 8.8f, size)];
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            float bodyLengthProgress = i / (float)(bodyChunks.Length - 1);
            float chunkRad =
                    Mathf.Lerp(
                        Mathf.Lerp(6, 10.5f, size),
                        Mathf.Lerp(12, 19.5f, size),
                        Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * bodyLengthProgress), 0, 1), Mathf.Lerp(0.7f, 0.3f, size)));
            float chunkMass =
                    Mathf.Lerp(0.3f, 2.3f, Mathf.Pow(size, 1.3f));

            bodyChunks[i] = new BodyChunk(this, i, default, chunkRad, chunkMass)
            {
                loudness = 0.2f,
                terrainSqueeze = 0.9f
            };
        }
        mainBodyChunkIndex = bodyChunks.Length / 2;
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i].rad = Mathf.Lerp(bodyChunks[i].rad, bodyChunks[mainBodyChunkIndex].rad, 0.5f);
            bodyChunks[i].mass = Mathf.Lerp(bodyChunks[i].mass, bodyChunks[mainBodyChunkIndex].mass, 0.5f);
        }

        if (ChillState is not null && (ChillState.shells is null || ChillState.shells.Length != bodyChunks.Length))
        {
            ChillState.shells = new bool[bodyChunks.Length];
            for (int k = 0; k < ChillState.shells.Length; k++)
            {
                ChillState.shells[k] = true;
            }
        }

        if (ChillState.iceShells is null ||
            ChillState.iceShells.Count != bodyChunks.Length)
        {
            ResetShellData();
        }

        bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
        int bodyConnectionNum = 0;
        for (int c = 0; c < bodyChunks.Length; c++)
        {
            for (int m = c + 1; m < bodyChunks.Length; m++)
            {
                bodyChunkConnections[bodyConnectionNum] = new BodyChunkConnection(bodyChunks[c], bodyChunks[m], (bodyChunks[c].rad + bodyChunks[m].rad) * 1.1f, BodyChunkConnection.Type.Push, 0.7f, -1);
                bodyConnectionNum++;
            }
        }

        surfaceFriction = 0.4f;
        airFriction = 0.9995f;
        buoyancy = 1.2f;

        MovementSpeed = 0.9f;
    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ChillipedeGraphics(this);
    }
    public virtual void ResetShellData()
    {
        List<ChillipedeState.Shell> oldShells = ChillState.iceShells;
        ChillState.iceShells = new(bodyChunks.Length);
        Random.State state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        for (int s = 0; s < ChillState.iceShells.Capacity; s++)
        {
            ChillState.iceShells.Add(new ChillipedeState.Shell(s));
            ChillState.iceShells[s].sprites = new int[2] { Random.Range(0, 3), Random.Range(0, 3) };
            if (oldShells is null)
            {
                ChillState.iceShells[s].health = 3;
            }
            else
            if (oldShells.Count > s &&
                oldShells[s] is not null)
            {
                ChillState.iceShells[s].health = oldShells[s].health;
                ChillState.iceShells[s].timeToRefreeze = oldShells[s].timeToRefreeze;
            }
        }
        Random.state = state;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void AssignSize(AbstractCreature absChl)
    {
        if (Template.type == HailstormCreatures.Chillipede)
        {
            size = 0.5f;
        }
        else
        {
            size = Custom.WrappedRandomVariation(0.5f, 0.5f, 0.2f);
        }
    }
    public virtual void InitiateFoodPips(AbstractCreature absChl)
    {
        if (absChl.creatureTemplate.meatPoints <= 1)
        {
            abstractCreature.state.meatLeft = Mathf.RoundToInt(Mathf.Lerp(2.5f, 13.5f, size));
        }
        else
        {
            abstractCreature.state.meatLeft = Template.meatPoints;
        }
        ChillState.meatInitated = true;
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        BodySqueezeUpdate();

        base.Update(eu);

        if (room is null || 
            graphicsModule is null ||
            graphicsModule is not ChillipedeGraphics)
        {
            return;
        }

        if (ChillipedeTileLeniency > 0)
        {
            ChillipedeTileLeniency--;
        }

        if (changeDirCounter > 0)
        {
            changeDirCounter = 0;
        }

        if (Hypothermia > 0.1f)
        {
            Hypothermia = 0;
        }
        if (HypothermiaExposure > 0.1f)
        {
            HypothermiaExposure = 0;
        }
        if (ChillState.iceShells.Count != bodyChunks.Length)
        {
            ResetShellData();
        }

        ShellUpdate(eu);

        ManageCharge();

        FrictionUpdate();

    }
    public virtual void ShellUpdate(bool eu)
    {
        for (int s = 0; s < ChillState.iceShells.Count; s++)
        {
            ChillipedeState.Shell shell = ChillState.iceShells[s];
            if (shell.timeToRefreeze != 0)
            {
                if (HypothermiaGain > 0 && (!dead || HypothermiaExposure > 0))
                {
                    RefreezeShell(shell);
                }

                if (Random.value < 0.00225f)
                {
                    if (Random.value > 0.5f)
                    {
                        EmitSnowflake(bodyChunks[s].pos, Custom.RNV() * Random.value * 2f);
                    }
                    else
                    {
                        EmitIceflake(bodyChunks[s].pos, Custom.RNV() * Random.value * 2f);
                    }
                }
            }
            else
            {
                if (Random.value < 0.0015f)
                {
                    EmitSnowflake(bodyChunks[s].pos, Custom.RNV() * Random.value * 4f);
                }
            }

            if (UpdateShellStage(shell))
            {
                for (int sn = 8; sn >= 0; sn--)
                {
                    EmitSnowflake(bodyChunks[shell.index].pos, Custom.RNV() * Random.value * 6f);
                }
            }

            if (ChillState.health < 1 &&
                ChillState.health > 0 &&
                shell.health > 0)
            {
                RegenerateHealth(shell);
            }

            if (shell.justBroke)
            {
                shell.justBroke = false;
            }

        }
    }
    public virtual void RefreezeShell(ChillipedeState.Shell shell)
    {
        int RefreezeAmount = 1;
        if (room.blizzardGraphics is not null)
        {
            if (!dead)
            {
                float Refreeze =
                    (1 + (4f * HypothermiaExposure)) *
                    Mathf.Clamp(room.blizzardGraphics.SnowfallIntensity, 0.2f, 1);
                RefreezeAmount = (int)Mathf.Max(1, Refreeze);

            }
            else
            {
                RefreezeAmount = (int)(HypothermiaExposure * 2f);
            }
        }

        shell.timeToRefreeze = Mathf.Max(0, shell.timeToRefreeze - RefreezeAmount);
    }
    public virtual bool UpdateShellStage(ChillipedeState.Shell shell)
    {
        if (shell.health < MaxShellHP &&
            shell.timeToRefreeze < 1)
        {
            shell.health++;
            shell.timeToRefreeze += 2400;
            if (!ChillState.shells[shell.index])
            {
                ChillState.shells[shell.index] = true;
            }
            return true;
        }
        return false;

    }
    public virtual void RegenerateHealth(ChillipedeState.Shell shell)
    {
        float HPgain = (RegenPerShell/40f) * (shell.health / MaxShellHP);
        ChillState.health = Mathf.Min(1, ChillState.health + HPgain);
    }
    public virtual void ManageCharge()
    {
        if (shockCharge > 0)
        {
            freezeCharge += 1 / Mathf.Lerp(100f, 5f, size);
            shockCharge = 0;
        }
        else if (freezeCharge != 0)
        {
            freezeCharge = Mathf.Max(0, freezeCharge - (1 / Mathf.Lerp(100f, 5f, size) / 2f));
        }
    }
    public virtual void FrictionUpdate()
    {
        bool grabbingSomething = false;
        for (int g = 0; g < grasps.Length; g++)
        {
            if (grasps[g] is not null)
            {
                grabbingSomething = true;
                break;
            }
        }
        surfaceFriction = grabbingSomething ? 0.2f : 0.6f;
    }
    public virtual void BodySqueezeUpdate()
    {
        for (int m = 0; m < bodyChunks.Length; m++)
        {
            if (dead)
            {
                // Shrinks bodyChunk terrain collision when the Chillipede is dead, to help prevent it from clogging pipes and tunnels.
                bodyChunks[m].terrainSqueeze = Custom.LerpMap(bodyChunks[m].rad, 6, 18, DefaultTerrainCollisionMult, 0.05f);
                continue;
            }

            bool onVerticalSurface = bodyChunks[m].contactPoint.x != 0;
            if (moving || onVerticalSurface)
            {
                MovementConnection movementConnection = (AI?.pathFinder as CentipedePather)?.FollowPath(room.GetWorldCoordinate(HeadChunk.pos), actuallyFollowingThisPath: false);
                if (onVerticalSurface ||
                    room.aimap.getAItile(room.GetWorldCoordinate(bodyChunks[m].pos)).narrowSpace ||
                    (movementConnection is not null && room.aimap.getAItile(movementConnection.DestTile).narrowSpace))
                {
                    // Shrinks bodyChunk terrain collision if the chunk is on a wall or wanting to move through a narrow space.
                    bodyChunks[m].terrainSqueeze = Mathf.Lerp(bodyChunks[m].terrainSqueeze, Custom.LerpMap(bodyChunks[m].rad, 6, 18, DefaultTerrainCollisionMult, 0.05f), 0.05f);
                }
                else
                if (((bodyDirection && m > 0) || (!bodyDirection && m < bodyChunks.Length - 1)) &&
                    (onVerticalSurface || room.aimap.getAItile(room.GetWorldCoordinate(bodyChunks[m + (bodyDirection ? -1 : 1)].pos)).narrowSpace))
                {
                    // Shrinks bodyChunk terrain collision if the chunk in front of it is on a wall or in a narrow space.
                    bodyChunks[m].terrainSqueeze = Mathf.Lerp(bodyChunks[m].terrainSqueeze, Custom.LerpMap(bodyChunks[m].rad, 6, 18, DefaultTerrainCollisionMult, 0.05f), 0.05f);
                }
                else
                {
                    bodyChunks[m].terrainSqueeze = Mathf.Lerp(bodyChunks[m].terrainSqueeze, DefaultTerrainCollisionMult, 0.1f);
                }
            }
            else
            {
                bodyChunks[m].terrainSqueeze = Mathf.Lerp(bodyChunks[m].terrainSqueeze, DefaultTerrainCollisionMult, 0.1f);
            }
        }
    }

    public virtual void ChillipedeCrawl(Vector2[] bodyChunkVels)
    {
        int segmentsAppliedForceTo = 0;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            BodyChunk chunk = bodyChunks[b];
            chunk.vel = bodyChunkVels[b];
            if (!AccessibleTile(room.GetTilePosition(chunk.pos)))
            {
                continue;
            }
            segmentsAppliedForceTo++;
            chunk.vel *= 0.7f;
            chunk.vel.y += gravity * Mathf.Pow(Mathf.Lerp(ChillState.ClampedHealth, 1, Random.value), 0.25f);
            if (b > 0 &&
                !AccessibleTile(room.GetTilePosition(bodyChunks[b - 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += gravity * Mathf.Pow(Mathf.Lerp(ChillState.ClampedHealth, 1, Random.value), 0.25f);
            }
            if (b < bodyChunks.Length - 1 &&
                !AccessibleTile(room.GetTilePosition(bodyChunks[b + 1].pos)))
            {
                chunk.vel *= 0.3f;
                chunk.vel.y += gravity * Mathf.Pow(Mathf.Lerp(ChillState.ClampedHealth, 1, Random.value), 0.25f);
            }
            if (b == 0 ||
                b == bodyChunks.Length - 1)
            {
                continue;
            }
            if (moving)
            {
                int bodyDir = !bodyDirection ? 1 : -1;
                if (IsAccessibleTile(room.GetTilePosition(bodyChunks[b + bodyDir].pos)) || ChillipedeTileLeniency > 0)
                {
                    // Gets pulled forward if the chunk in front of it is on accessible terrain.
                    chunk.vel +=    Custom.DirVec(chunk.pos, bodyChunks[b + bodyDir].pos) * Mathf.Lerp(0.7f, 1.4f, size * ChillState.ClampedHealth) * MovementSpeed;
                }
                // Gets pushed forward by the chunk behind it.
                chunk.vel -= Custom.DirVec(chunk.pos, bodyChunks[b + (bodyDir * -1)].pos) * Mathf.Lerp(0.9f, 1.2f, size * ChillState.ClampedHealth) * 0.5f * MovementSpeed;
                continue;
            }
            Vector2 chunkDifference = chunk.pos - bodyChunks[b - 1].pos;
            Vector2 dirToCurrentChunk = chunkDifference.normalized;
            chunkDifference = bodyChunks[b + 1].pos - chunk.pos;
            Vector2 chunkDirection = (dirToCurrentChunk + chunkDifference.normalized) / 2f;
            if (Mathf.Abs(chunkDirection.x) > 0.5f)
            {
                chunk.vel.y -= (chunk.pos.y - (room.MiddleOfTile(chunk.pos).y +   VerticalSitSurface(chunk.pos) * (10 - chunk.rad/3f))) * Mathf.Lerp(0.05f, 0.55f, Mathf.Pow(size * ChillState.ClampedHealth, 1.2f));
            }
            if (Mathf.Abs(chunkDirection.y) > 0.5f)
            {
                chunk.vel.x -= (chunk.pos.x - (room.MiddleOfTile(chunk.pos).x + HorizontalSitSurface(chunk.pos) * (10 - chunk.rad/3f))) * Mathf.Lerp(0.05f, 0.55f, Mathf.Pow(size * ChillState.ClampedHealth, 1.2f));
            }
        }

        if (!Custom.DistLess(HeadChunk.pos, moveToPos, 10f))
        {
            if (moving && segmentsAppliedForceTo > 0)
            {
                ChillipedeTileLeniency = 10;
            }
            if (segmentsAppliedForceTo > 0)
            {
                HeadChunk.vel += Custom.DirVec(HeadChunk.pos, moveToPos) * Custom.LerpMap(segmentsAppliedForceTo, 0, bodyChunks.Length, 6, 3) * Mathf.Lerp(0.7f, 1.3f, size * ChillState.ClampedHealth);
            }
        }

    }
    private bool IsAccessibleTile(IntVector2 testPos)
    {
        for (int t = 0; t < Custom.fourDirectionsAndZero.Length; t++)
        {
            IntVector2 newTestPos = testPos + Custom.fourDirectionsAndZero[t];
            if (newTestPos.y != room.defaultWaterLevel &&
                room.aimap.TileAccessibleToCreature(newTestPos, Template.preBakedPathingAncestor))
            {
                return true;
            }
            if (IsClimbableTile(newTestPos))
            {
                return true;
            }
        }
        return false;
    }
    public bool IsClimbableTile(IntVector2 testPos)
    {
        if (room.GetTile(testPos).wallbehind ||
            room.GetTile(testPos).verticalBeam ||
            room.GetTile(testPos).horizontalBeam ||
            room.aimap.getAItile(testPos).terrainProximity < 3)
        {
            return true;
        }
        return false;
    }

    // - - - - - - - - - - - - - - - - - - - -

    public override bool SpearStick(Weapon source, float dmg, BodyChunk hitChunk, Appendage.Pos appPos, Vector2 direction)
    {
        if (ChillState.iceShells is not null &&
            ChillState.iceShells.Count > hitChunk.index &&
            ChillState.iceShells[hitChunk.index] is not null)
        {
            ChillipedeState.Shell shell = ChillState.iceShells[hitChunk.index];

            if (hitChunk is not null &&
                hitChunk.index >= 0 &&
                hitChunk.index < bodyChunks.Length &&
                (shell.health > 0 || shell.justBroke))
            {
                if (LargeSize || (NormalSize && source is null))
                {
                    return false;
                }
            }
        }
        return base.SpearStick(source, dmg, hitChunk, appPos, direction);
    }
    public override void Violence(BodyChunk source, Vector2? dirAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppen, DamageType dmgType, float damage, float bonusStun)
    {
        if (room is not null &&
            hitChunk is not null)
        {
            ChillipedeState.Shell shell = ChillState.iceShells[hitChunk.index];
            int shellDamage;
            int[] shells;

            if (dmgType == DamageType.Electric ||
                dmgType == DamageType.Explosion)
            {
                shellDamage = (int)Mathf.Clamp(4f * damage, 0, 4);
                shells = new int[bodyChunks.Length];
                for (int s = 0; s < shells.Length; s++)
                {
                    shells[s] = s;
                }
                DamageChillipedeShells(shells, shellDamage, source);
            }

            bool attackTooWeak = damage < 0.01f;

            if (shell.health > 1 &&
                dmgType != DamageType.Electric)
            {
                damage *= 1 - (0.25f * (shell.health - 1));
            }

            shellDamage = attackTooWeak ? 0 : 2;

            if (!attackTooWeak)
            {
                if (shell.health > 0)
                {
                    if (dmgType == DamageType.Blunt ||
                        dmgType == HailstormDamageTypes.Heat)
                    {
                        damage *= Mathf.Clamp(3 - shell.health, 0, 1);
                        shellDamage += (int)Mathf.Max(1, 3f * damage);
                    }
                }
                else if (shell.health < 3)
                {
                    if (dmgType == DamageType.Water ||
                        dmgType == HailstormDamageTypes.Cold)
                    {
                        shellDamage -= (int)Mathf.Max(1, 3f * damage);
                    }
                }
            }

            shells = new int[1] { hitChunk.index };
            DamageChillipedeShells(shells, shellDamage, source);

        }

        if (!Small &&
                (hitChunk.index == 0 ||
                 hitChunk.index == bodyChunks.Length - 1))
        {
            damage *= 0.5f;
        }

        // Setting a Chillipede's baseDamageResistance to 1 will allow the damage it takes to vary based on size.
        // This effectively makes Max HP scale with size, going from 1 Max HP to 10.
        if (Template.baseDamageResistance == 1)
        {
            damage /= Mathf.Lerp(1, 10, Mathf.Pow(size, 0.5f));
        }
        // Setting baseStunResistance to 1 will allow the stun to vary, as well.
        // Reduces stun taken by 0% to 80%.
        if (Template.baseStunResistance == 1)
        {
            bonusStun /= Mathf.Lerp(1, 5, Mathf.Pow(size, 0.5f));
        }

        base.Violence(source, dirAndMomentum, hitChunk, hitAppen, dmgType, damage, bonusStun);
    }
    public virtual void DamageChillipedeShells(int[] shellIndexes, int damage, BodyChunk attacker)
    {
        if (room is null || ChillState.shells is null)
        {
            return;
        }

        for (int s = 0; s < shellIndexes.Length; s++)
        {
            if (shellIndexes[s] < 0 ||
                shellIndexes[s] > ChillState.iceShells.Count)
            {
                continue;
            }
            ChillipedeState.Shell shell = ChillState.iceShells[shellIndexes[s]];
            int oldHP = shell.health;
            shell.health = Mathf.Clamp(shell.health - damage, 0, MaxShellHP);
            int HPdifference = oldHP - shell.health;

            if (HPdifference == 0)
            {
                if (shell.health > 0)
                {
                    Vector2 deflectPos = bodyChunks[shell.index].pos;
                    room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, deflectPos, 1.2f, 1);
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 vel = (attacker is not null) ?
                            attacker.vel * Random.Range(-0.2f, -0.4f) + Custom.DegToVec(Custom.VecToDeg(attacker.vel) + Random.Range(-75f, 75f)) * attacker.vel.magnitude * -0.1f :
                            Custom.RNV() * Random.Range(5f, 10f);

                        room.AddObject(new Spark(
                            deflectPos + Custom.DegToVec(Random.value * 360f) * Random.value * 5f,
                            vel, Color.white, null, 15, 120));
                    }
                    room.AddObject(new StationaryEffect(deflectPos, Color.white, null, StationaryEffect.EffectType.FlashingOrb));
                }
                continue;
            }

            shell.timeToRefreeze = 2000;
            shell.justBroke = shell.health == 0 && oldHP > 0;

            int absHPdiff = Mathf.Abs(HPdifference);
            float shatterVolume = 0.6f + (0.2f * absHPdiff);
            room.PlaySound(SoundID.Coral_Circuit_Break, bodyChunks[shell.index].pos, shatterVolume, Random.Range(1.4f, 1.6f));

            for (int p = 0; p < (6 * absHPdiff); p++)
            {
                Vector2 particleVel = Custom.RNV() * Random.value * (14f + (3f * HPdifference));
                switch (p % 3)
                {
                    case 0:
                        EmitIceflake(bodyChunks[shell.index].pos, particleVel * 1.25f);
                        EmitIceshard(bodyChunks[shell.index].pos, particleVel, 1.25f, 0.2f, Random.Range(1.3f, 1.7f));
                        break;
                    default:
                        EmitSnowflake(bodyChunks[shell.index].pos, particleVel * 1.5f);
                        EmitIceshard( bodyChunks[shell.index].pos, particleVel, 0.75f, 0.1f, Random.Range(1.3f, 1.7f));
                        break;
                }
            }
        }
    }
    public override void Stun(int time)
    {
        if (time >= 10)
        {
            LoseAllGrasps();
        }
        base.Stun(time);
    }

    public virtual void Freeze(PhysicalObject victim)
    {
        if (victim is null)
        {
            return;
        }

        room.PlaySound(SoundID.Coral_Circuit_Break, HeadChunk.pos, 1.25f, 1.75f);
        room.PlaySound(SoundID.Coral_Circuit_Break, HeadChunk.pos, 1.25f, 1.00f);
        room.PlaySound(SoundID.Coral_Circuit_Break, HeadChunk.pos, 1.25f, 0.25f);
        InsectCoordinator smallInsects = null;
        for (int i = 0; i < room.updateList.Count; i++)
        {
            if (room.updateList[i] is InsectCoordinator)
            {
                smallInsects = room.updateList[i] as InsectCoordinator;
                break;
            }
        }
        for (int i = 0; i < Random.Range(14, 19); i++)
        {
            EmitSnowflake(HeadChunk.pos, Custom.RNV() * Mathf.Lerp(6, 18, Random.value));
            EmitFrostmist(bodyChunks[Random.Range(0, bodyChunks.Length - 1)].pos, Custom.RNV() * Random.value * 6f, 1.5f, smallInsects, true);
        }


        for (int j = 0; j < bodyChunks.Length; j++)
        {
            bodyChunks[j].vel += Custom.RNV() * 6f * Random.value;
            bodyChunks[j].pos += Custom.RNV() * 6f * Random.value;
        }

        if (victim is Creature)
        {
            FreezeCreature(victim as Creature);
        }
        else
        {
            FreezeObject(victim);
        }

    }
    public virtual void FreezeCreature(Creature victim)
    {
        bool Immune = CustomTemplateInfo.IsColdCreature(victim.Template.type);
        float[] ColdResistances = new float[2];
        ColdResistances[0] = victim.abstractCreature.HypothermiaImmune ? 4 : 1;
        ColdResistances[1] = victim.abstractCreature.HypothermiaImmune ? 4 : 1;

        if (victim is Player)
        {
            ColdResistances[0] /= CustomTemplateInfo.DamageResistances.SlugcatDamageMultipliers(victim as Player, HailstormDamageTypes.Cold);
        }
        else
        {
            if (victim.Template.damageRestistances[HailstormDamageTypes.Cold.index, 0] > 0)
            {
                ColdResistances[0] *= victim.Template.damageRestistances[HailstormDamageTypes.Cold.index, 0];
            }
            if (victim.Template.damageRestistances[HailstormDamageTypes.Cold.index, 1] > 0)
            {
                ColdResistances[1] *= victim.Template.damageRestistances[HailstormDamageTypes.Cold.index, 1];
            }
        }

        if (victim is Player player2 && IncanInfo.IncanData.TryGetValue(player2, out IncanInfo hs2) && hs2.isIncan)
        {
            victim.killTag = abstractCreature;
            victim.Die();
            victim.Hypothermia += 2f / ColdResistances[0];
        }
        else if (!Immune && TotalMass > victim.TotalMass / ColdResistances[0])
        {
            victim.killTag = abstractCreature;
            victim.Die();
            victim.Hypothermia += 2f / ColdResistances[0];
        }
        else
        {
            victim.Stun((int)(200 / ColdResistances[1]));
            victim.LoseAllGrasps();
            victim.Hypothermia += 1f / ColdResistances[0];

            shockGiveUpCounter = Math.Max(shockGiveUpCounter, 30);
            AI.annoyingCollisions = Immune ? 0 : Math.Min(AI.annoyingCollisions / 2, 150);
        }

        if (victim.State is ColdLizState lS && !lS.crystals.All(intact => intact))
        {
            if (victim.Template.type == HailstormCreatures.IcyBlue)
            {
                for (int s = 0; s < lS.crystals.Length; s++)
                {
                    lS.crystals[s] = true;
                }
            }
            else if (victim.Template.type == HailstormCreatures.Freezer)
            {
                for (int s = Random.Range(0, lS.crystals.Length); /**/ ; /**/ )
                {
                    if (lS.crystals[s])
                    {
                        lS.crystals[s] = true;
                        break;
                    }
                    if (s >= lS.crystals.Length) s = 0;
                    else s++;
                }
            }
            lS.armored = true;
        }

        for (int e = room.abstractRoom.entities.Count - 1; e >= 0; e--)
        {
            if (room.abstractRoom.entities[e] is null ||
                room.abstractRoom.entities[e] is not AbstractPhysicalObject absObj ||
                absObj.realizedObject is null ||
                absObj.realizedObject == this ||
                absObj.realizedObject == victim)
            {
                continue;
            }

            if (absObj.realizedObject is Creature)
            {
                FreezeCollateralCreature(absObj.realizedObject as Creature);
            }
            else
            {
                FreezeCollateralObject(absObj.realizedObject);
            }
        }

        Stun(50);

    }
    public virtual void FreezeObject(PhysicalObject victim)
    {
        shockGiveUpCounter = Math.Max(shockGiveUpCounter, 30);
        AI.annoyingCollisions = 0;

        if (victim is IceChunk ice)
        {
            ice.AbsIce.size += 2f;
            ice.AbsIce.freshness += 1f;
            ice.AbsIce.color1 = ChillState.topShellColor;
            ice.AbsIce.color2 = ChillState.bottomShellColor;
        }

        if (CustomObjectInfo.FreezableObjects.ContainsKey(victim.abstractPhysicalObject.type))
        {
            AbstractIceChunk newAbsIce = new(victim.abstractPhysicalObject.world, victim.abstractPhysicalObject.pos, victim.abstractPhysicalObject.world.game.GetNewID())
            {
                frozenObject = new FrozenObject(victim.abstractPhysicalObject),
                size = 2f,
                freshness = 1f,
                color1 = ChillState.topShellColor,
                color2 = ChillState.bottomShellColor
            };
            victim.abstractPhysicalObject.Room.AddEntity(newAbsIce);
            newAbsIce.RealizeInRoom();

            IceChunk newIce = newAbsIce.realizedObject as IceChunk;
            newIce.firstChunk.HardSetPosition(victim.firstChunk.pos);
            newIce.firstChunk.vel = victim.firstChunk.vel;
            if (victim.grabbedBy.Count > 0)
            {
                for (int g = victim.grabbedBy.Count - 1; g >= 0; g--)
                {
                    Grasp grasp = victim.grabbedBy[g];
                    grasp.grabber?.Grab(newIce, grasp.graspUsed, 0, grasp.shareability, grasp.dominance, true, grasp.pacifying);
                }
            }
            victim.RemoveFromRoom();
            victim.abstractPhysicalObject.Room.RemoveEntity(victim.abstractPhysicalObject);
        }


        for (int e = room.abstractRoom.entities.Count - 1; e >= 0; e--)
        {
            if (room.abstractRoom.entities[e] is null ||
                room.abstractRoom.entities[e] is not AbstractPhysicalObject absObj ||
                absObj.realizedObject is null ||
                absObj.realizedObject == victim)
            {
                continue;
            }

            if (absObj.realizedObject is Creature)
            {
                FreezeCollateralCreature(absObj.realizedObject as Creature);
            }
            else
            {
                FreezeCollateralObject(absObj.realizedObject);
            }

        }

        Stun(50);

    }
    public virtual void FreezeCollateralCreature(Creature collateral)
    {
        if (CustomTemplateInfo.IsColdCreature(collateral.Template.type) ||
            !room.VisualContact(HeadChunk.pos, collateral.DangerPos) ||
            !Custom.DistLess(HeadChunk.pos, collateral.DangerPos, FreezeRadius))
        {
            return;
        }

        float ChillResistance = collateral.abstractCreature.HypothermiaImmune ? 4 : 1;
        if (collateral is Player)
        {
            ChillResistance /= CustomTemplateInfo.DamageResistances.SlugcatDamageMultipliers(collateral as Player, HailstormDamageTypes.Cold);
        }
        else if (collateral.Template.damageRestistances[HailstormDamageTypes.Cold.index, 0] > 0)
        {
            ChillResistance *= collateral.Template.damageRestistances[HailstormDamageTypes.Cold.index, 0];
        }
        collateral.Hypothermia += 1 / ChillResistance * Mathf.InverseLerp(FreezeRadius, FreezeRadius/5f, Custom.Dist(HeadChunk.pos, collateral.DangerPos));
        collateral.killTag = abstractCreature;
        collateral.killTagCounter = 400;
    }
    public virtual void FreezeCollateralObject(PhysicalObject collateral)
    {
        if (!room.VisualContact(HeadChunk.pos, collateral.firstChunk.pos) ||
            !Custom.DistLess(HeadChunk.pos, collateral.firstChunk.pos, FreezeRadius))
        {
            return;
        }

        float distFac = Mathf.InverseLerp(FreezeRadius, FreezeRadius/5f, Custom.Dist(HeadChunk.pos, collateral.firstChunk.pos));

        if (collateral is IceChunk ice)
        {
            ice.AbsIce.size += distFac * 2f;
            ice.AbsIce.freshness += distFac;
            ice.AbsIce.color1 = Color.Lerp(ice.AbsIce.color1, ChillState.topShellColor, distFac);
            ice.AbsIce.color2 = Color.Lerp(ice.AbsIce.color2, ChillState.bottomShellColor, distFac);
        }
        if (!CustomObjectInfo.FreezableObjects.ContainsKey(collateral.abstractPhysicalObject.type))
        {
            return;
        }


        AbstractIceChunk newAbsIce = new(collateral.abstractPhysicalObject.world, collateral.abstractPhysicalObject.pos, collateral.abstractPhysicalObject.world.game.GetNewID())
        {
            frozenObject = new FrozenObject(collateral.abstractPhysicalObject),
            size = distFac * 2f,
            freshness = distFac,
            color1 = ChillState.topShellColor,
            color2 = ChillState.bottomShellColor
        };
        collateral.abstractPhysicalObject.Room.AddEntity(newAbsIce);
        newAbsIce.RealizeInRoom();

        IceChunk newIce = newAbsIce.realizedObject as IceChunk;
        newIce.firstChunk.HardSetPosition(collateral.firstChunk.pos);
        newIce.firstChunk.vel = collateral.firstChunk.vel;
        if (collateral.grabbedBy.Count > 0)
        {
            for (int g = collateral.grabbedBy.Count - 1; g >= 0; g--)
            {
                Grasp grasp = collateral.grabbedBy[g];
                grasp.grabber?.Grab(newIce, grasp.graspUsed, 0, grasp.shareability, grasp.dominance, true, grasp.pacifying);
            }
        }
        collateral.RemoveFromRoom();
        collateral.abstractPhysicalObject.Room.RemoveEntity(collateral.abstractPhysicalObject);
    }

    public override Color ShortCutColor()
    {
        if (ChillState is not null)
        {
            return ChillState.topShellColor;
        }
        return base.ShortCutColor();
    }

    // - - - - - - - - - - - - - - - - - - - -

    public virtual void EmitSnowflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new HailstormSnowflake(pos, vel, ChillState.topShellColor, ChillState.bottomShellColor));
    }
    public virtual void EmitIceflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new PuffBallSkin(pos, vel, ChillState.topShellColor, ChillState.bottomShellColor));
    }
    public virtual void EmitIceshard(Vector2 pos, Vector2 vel, float scale, float shardVolume, float shardPitch)
    {
        Color shardColor = Random.value < 2/3f ?
                        ChillState.topShellColor :
                        ChillState.bottomShellColor;
        room.AddObject(new Shard(pos, vel, shardVolume, scale, shardPitch, shardColor, true));
    }

    public virtual void EmitFrostmist(Vector2 pos, Vector2 vel, float size, InsectCoordinator insectCoordinator, bool hasGameplayImpact)
    {
        room.AddObject(new FreezerMist(pos, vel, ChillState.topShellColor, ChillState.bottomShellColor, size, abstractCreature, insectCoordinator, hasGameplayImpact));
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------