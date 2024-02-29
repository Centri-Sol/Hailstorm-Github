namespace Hailstorm;

public class InfantAquapede : Centipede, IPlayerEdible
{
    public InfantAquapedeState AquababyState => CentiState as InfantAquapedeState;
    public new int FoodPoints => Template.meatPoints;
    public new bool Edible => true;
    public new bool AutomaticPickUp => false;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public float babyCharge;

    public InfantAquapede(AbstractCreature absBA, World world) : base(absBA, world)
    {
        size = 0.1f;

        bodyChunks = new BodyChunk[7];

        for (int i = 0; i < bodyChunks.Length; i++)
        {
            float bodyLengthProgress = i / (float)(bodyChunks.Length - 1);
            float chunkRad =
                Mathf.Lerp(
                    Mathf.Lerp(2, 3.5f, size),
                    Mathf.Lerp(4, 6.5f, size),
                    Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * bodyLengthProgress), 0, 1), Mathf.Lerp(0.7f, 0.3f, size)));
            float chunkMass = (i >= bodyChunks.Length - AquababyState.remainingBites) ?
                Mathf.Lerp(3 / 70f, 11 / 34f, Mathf.Pow(size, 1.4f)) : 0.01f;

            bodyChunks[i] = new(this, i, default, chunkRad, chunkMass);

        }

        mainBodyChunkIndex = bodyChunks.Length / 2;

        if (AquababyState.shells is null ||
            AquababyState.shells.Length != bodyChunks.Length)
        {
            AquababyState.shells = new bool[bodyChunks.Length];
            for (int k = 0; k < AquababyState.shells.Length; k++)
            {
                AquababyState.shells[k] = Random.value < 0.9925f;
            }
        }

        if (bodyChunkConnections is not null)
        {
            bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
            int chunkConNum = 0;
            for (int l = 0; l < bodyChunks.Length; l++)
            {
                for (int m = l + 1; m < bodyChunks.Length; m++)
                {
                    bodyChunkConnections[chunkConNum] = new(bodyChunks[l], bodyChunks[m], (bodyChunks[l].rad + bodyChunks[m].rad) * 1.1f, BodyChunkConnection.Type.Push, 0.3f, -1f);
                    chunkConNum++;
                }
            }
        }

    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new InfantAquapedeGraphics(this);
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);

        ManageCharge();

        if (!dead)
        {
            HeldUpdate();
        }

        if (lungs < 0.005f)
        {
            lungs = 1;
        }

        HealthDrain();
    }
    public virtual void ManageCharge()
    {
        if (shockCharge > 0)
        {
            babyCharge += shockCharge / 2f;
            shockCharge = 0;
        }
        else if (babyCharge > 0)
        {
            babyCharge -= 1 / 240f;
        }
    }
    public virtual void HeldUpdate()
    {
        if (grabbedBy.Count > 0 &&
            grabbedBy[0].grabber is not null &&
            babyCharge >= 1)
        {
            BabyShock(grabbedBy[0].grabber);
            babyCharge = 0;
        }
    }
    public virtual void HealthDrain()
    {
        if (bites < bodyChunks.Length * 0.8f)
        {
            AquababyState.health -= 0.01f / 2400f * (bodyChunks.Length - bites);
            // Loses 1% HP/min for every segment eaten.
        }
    }

    public virtual void InfantAquapedeSwimming(Vector2[] bodyChunkVels)
    {
        bodyWave += Mathf.Clamp(Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) / 80f, 0.1f, 1);

        for (int i = 0; i < bodyChunks.Length; i++)
        {
            BodyChunk chunk = bodyChunks[i];
            chunk.vel = bodyChunkVels[i];
            float bodylengthProgress = i / (float)(bodyChunks.Length - 1);
            if (!bodyDirection)
            {
                bodylengthProgress = 1f - bodylengthProgress;
            }
            float bodyWaveFac = Mathf.Sin((bodyWave - (bodylengthProgress * Mathf.Lerp(12f, 28f, size))) * Mathf.PI * 0.11f);
            chunk.vel *= 0.9f;
            chunk.vel.y += gravity * wingsStartedUp;
            if (i <= 0 || i >= bodyChunks.Length - 1)
            {
                continue;
            }
            Vector2 dirToNextChunk = Custom.DirVec(chunk.pos, bodyChunks[i + (!bodyDirection ? 1 : -1)].pos);
            Vector2 perpAngle = Custom.PerpendicularVector(dirToNextChunk);
            chunk.vel += dirToNextChunk * 0.5f * Mathf.Lerp(0.5f, 1.5f, size);
            if (AI.behavior == CentipedeAI.Behavior.Idle)
            {
                chunk.vel *= Mathf.Clamp(Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) / 40f, 0.02f, 1f) * 0.7f;
                if (Vector2.Distance(HeadChunk.pos, AI.tempIdlePos.Tile.ToVector2()) < 20f)
                {
                    chunk.vel *= 0.28f * 0.7f;
                }
            }
            chunk.pos += perpAngle * 2.5f * bodyWaveFac;
        }
        if (room.aimap.getAItile(moveToPos).terrainProximity > 2)
        {
            HeadChunk.vel +=
                AquacentiSwim ?
                Custom.DirVec(HeadChunk.pos, moveToPos + (Custom.DegToVec(bodyWave * 5f) * 10f)) * 5f * Mathf.Lerp(0.7f, 1.3f, size) * 0.7f :
                Custom.DirVec(HeadChunk.pos, moveToPos + (Custom.DegToVec(bodyWave * 10f) * 60f)) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
        }
        else
        {
            HeadChunk.vel +=
                AquacentiSwim ?
                Custom.DirVec(HeadChunk.pos, moveToPos) * 1.4f * Mathf.Lerp(0.2f, 0.8f, size) :
                Custom.DirVec(HeadChunk.pos, moveToPos) * 4f * Mathf.Lerp(0.7f, 1.3f, size);
        }
    }

    // - - - - - - - - - - - - - - - - - - - -

    public override void Stun(int st)
    {
        st *= (int)Mathf.Lerp(1.1f, 0.66f, Mathf.InverseLerp(0.6f, 1.2f, size));
        base.Stun(st);
    }

    public virtual void BabyShock(PhysicalObject shockee)
    {
        room.PlaySound(SoundID.Centipede_Shock, mainBodyChunk.pos);
        if (graphicsModule is not null)
        {
            (graphicsModule as CentipedeGraphics).lightFlash = 1f;
            for (int i = 0; i < (int)Mathf.Lerp(4, 8, size); i++)
            {
                room.AddObject(new Spark(HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4, 14, Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
            }
        }
        for (int c = 0; c < bodyChunks.Length; c++)
        {
            bodyChunks[c].vel += Custom.RNV() * 6f * Random.value;
            bodyChunks[c].pos += Custom.RNV() * 6f * Random.value;
        }
        for (int s = 0; s < shockee.bodyChunks.Length; s++)
        {
            shockee.bodyChunks[s].vel += Custom.RNV() * 6f * Random.value;
            shockee.bodyChunks[s].pos += Custom.RNV() * 6f * Random.value;
        }

        if (shockee is Creature ctr)
        {
            if (shockee is Player plr && plr.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                plr.PyroDeath();
            }
            else
            {
                float dmg = 2f;
                int stun = 200;
                if (CentiHooks.IsIncanStory(room.game))
                {
                    dmg /= CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, false);
                    stun = (int)(stun / CustomTemplateInfo.DamageResistances.IncanStoryResistances(ctr.Template, DamageType.Electric, true));
                }
                ctr.Violence(mainBodyChunk, default, ctr.mainBodyChunk, null, DamageType.Electric, dmg, stun);
                room.AddObject(new CreatureSpasmer(ctr, true, ctr.stun));
                ctr.LoseAllGrasps();
            }
        }
        if (shockee.Submersion > 0f)
        {
            room.AddObject(new UnderwaterShock(room, this, HeadChunk.pos, 14, Mathf.Lerp(50, 100, size), 1, this, new Color(0.7f, 0.7f, 1f)));
        }

        Stun(120);

    }

    // - - - - - - - - - - - - - - - - - - - -

    public new void BitByPlayer(Grasp grasp, bool eu)
    {
        killTag = (grasp.grabber as Player).abstractCreature;
        killTagCounter += 240;
        bodyChunks[bodyChunks.Length - AquababyState.remainingBites].mass = 0.01f;
        bites--;
        AquababyState.remainingBites = bites;
        if (!dead)
        {
            AquababyState.health -= 0.5f;
        }
        room.PlaySound(bites == 0 ? SoundID.Slugcat_Eat_Centipede : SoundID.Slugcat_Bite_Centipede, mainBodyChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

}