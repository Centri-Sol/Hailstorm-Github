namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class IceChunk : Weapon
{
    public AbstractIceChunk AbsIce => abstractPhysicalObject as AbstractIceChunk;
    public virtual bool FreezerCrystal => AbsIce.type == HailstormItems.FreezerCrystal;
    public FrozenObject FrozenObject => AbsIce.frozenObject;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public float Size => AbsIce.size;
    public float BaseMass => Mathf.Lerp(0.04f, 0.2f, Size/2f);
    public float BaseRadius => Mathf.Lerp(2, 12, Size/2f);
    public virtual float BaseDamage
    {
        get
        {
            float dmg = 0.6f * Mathf.Lerp(2/3f, 4/3f, Size/2f);
            if (FreezerCrystal)
            {
                dmg *= 2f;
            }
            return dmg;
        }
    }
    public virtual float Chill
    {
        get
        {
            float chill = 0.25f * Size;
            if (FreezerCrystal)
            {
                chill *= 4f;
            }
            return chill;
        }
    }
    public float VelocityMultiplier
    {
        get
        {
            float velMult = 1 * Mathf.Lerp(1.2f, 0.7f, Size/2f);
            if (FreezerCrystal)
            {
                velMult *= 1.25f;
            }
            return velMult;
        }
    }
    public float Pitch
    {
        get
        {
            return Random.Range(1.3f, 1.7f);
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    public override int DefaultCollLayer => 1;
    public override bool HeavyWeapon => true;

    public LightSource light;

    public int noBreakGracePeriod;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public IceChunk(AbstractIceChunk absIce, World world): base(absIce, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, default, BaseRadius, BaseMass)
        {
            loudness = 10f
        };

        bodyChunkConnections = new BodyChunkConnection[0];

        soundLoop = new ChunkDynamicSoundLoop(firstChunk);
        collisionLayer = 1;

        surfaceFriction = 0.75f;
        waterFriction = 0.99f;
        airFriction = 0.97f;
        buoyancy = 1f;
        gravity = 0.9f;
        bounce = FreezerCrystal ? 2/3f : 1/3f;

        if (FrozenObject.obj is not null)
        {
            bodyChunks[0].mass += FrozenObject.TotalMass;
            bodyChunks[0].rad += FrozenObject.AddedRad;
            waterFriction = Mathf.Lerp(FrozenObject.waterFriction, waterFriction, 0.5f);
            airFriction = Mathf.Lerp(FrozenObject.airFriction, airFriction, 0.5f);
            buoyancy = Mathf.Lerp(FrozenObject.buoyancy, buoyancy, 0.5f);
            bounce = Mathf.Lerp(FrozenObject.bounce, bounce, 0.5f);
        }

        exitThrownModeSpeed = -1;
        noBreakGracePeriod = 5;

    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);

        soundLoop.sound = SoundID.None;
        if (firstChunk.vel.magnitude > 5f)
        {
            if (firstChunk.ContactPoint.y < 0)
            {
                soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
            }
            else
            {
                soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
            }
            soundLoop.Volume = Mathf.InverseLerp(5, 15, firstChunk.vel.magnitude);
            soundLoop.Pitch = Pitch;
        }
        soundLoop.Update();
        if (firstChunk.ContactPoint.y != 0)
        {
            rotationSpeed = (rotationSpeed * 2f + firstChunk.vel.x * 5f) / 3f;
        }

        if (light is null)
        {
            light = new LightSource(firstChunk.pos, environmentalLight: false, color, this)
            {
                affectedByPaletteDarkness = 0.25f
            };
            room.AddObject(light);
        }
        else
        {
            light.setPos = firstChunk.pos;
            light.setRad = 80f;
            light.setAlpha = 0.5f;
            if (light.slatedForDeletetion || light.room != room)
            {
                light = null;
            }
        }

        SizeUpdate();

    }
    public virtual void SizeUpdate()
    {
        if (noBreakGracePeriod > 0)
        {
            noBreakGracePeriod--;
        }

        if (room is not null)
        {
            foreach (IProvideWarmth mscHeatSource in room.blizzardHeatSources)
            {
                float dist = Vector2.Distance(firstChunk.pos, mscHeatSource.Position());
                if (mscHeatSource.loadedRoom == room && dist < mscHeatSource.range)
                {
                    float heatFac = Mathf.InverseLerp(mscHeatSource.range, mscHeatSource.range * 0.2f, dist);
                    float heat = mscHeatSource.warmth * heatFac;
                    if (AbsIce.freshness > 0)
                    {
                        heat *= Mathf.Pow(0.75f, AbsIce.freshness);
                    }
                    AbsIce.size -= heat;
                }
            }
            if (room.blizzardGraphics is not null)
            {
                float cold = 0;
                if (Submersion > 0)
                {
                    cold += Submersion;
                }
                if (Submersion < 1)
                {
                    cold += room.blizzardGraphics.GetBlizzardPixel((int)(firstChunk.pos.x / 20f), (int)(firstChunk.pos.y / 20f)).g * (1 - Submersion);
                }
                cold *= Mathf.Lerp(0.0001f, 0.00002f, Size - 1f);
                AbsIce.size += cold;
            }
            else
            {
                AbsIce.size -= 0.00002f;
            }
        }
        AbsIce.size = Mathf.Clamp(Size, 0, 2);
        if (Size == 0)
        {
            Melt();
        }
        else
        {
            firstChunk.mass = BaseMass;
            firstChunk.rad = BaseRadius;
            if (FrozenObject.obj is not null)
            {
                firstChunk.mass += FrozenObject.TotalMass;
                firstChunk.rad /= 2f;
                firstChunk.rad += FrozenObject.AddedRad;
            }
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override void PickedUp(Creature upPicker)
    {
        ChangeMode(Mode.Carried);
        room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, firstChunk, false, 1.1f, Pitch);
    }
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        room?.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk, false, 1.1f, Pitch);
        firstChunk.vel *= VelocityMultiplier;
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (firstContact)
        {
            if (speed >= 4)
            {
                int sparkAmount = (int)(speed / 5f);
                for (int i = 0; i < sparkAmount; i++)
                {
                    room.AddObject(new Spark(new Vector2(firstChunk.pos.x, room.MiddleOfTile(firstChunk.pos).y + firstChunk.ContactPoint.y * 10f), firstChunk.vel * Random.value + Custom.RNV() * Random.value * 4f - firstChunk.ContactPoint.ToVector2() * 4f * Random.value, Color.white, null, 6, 18));
                }
            }
            
            if (speed >= 16)
            {
                Shatter();
            }
        }
        if (mode == Mode.Thrown && speed < 2.5f)
        {
            mode = Mode.Free;
        }

    }
    public override void HitWall()
    {
        base.HitWall();
        Shatter();
    }
    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        Shatter();
    }
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);
        Shatter();
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj is null)
        {
            return false;
        }

        if (thrownBy is Scavenger scv && scv.AI is not null)
        {
            scv.AI.HitAnObjectWithWeapon(this, result.obj);
        }

        vibrate = 20;
        ChangeMode(Mode.Free);

        if (result.obj is Creature target)
        {
            if (target is EggBug egg && egg.FireBug)
            {
                egg.timeWithoutEggs = 10;
            }
            target.Violence(firstChunk, firstChunk.vel * firstChunk.mass, result.chunk, result.onAppendagePos, HailstormDamageTypes.Cold, BaseDamage, 120);
            target.Hypothermia += Chill;
        }
        else if (result.chunk is not null)
        {
            result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos is not null)
        {
            (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
        }

        firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude;

        if (result.chunk is not null ||
            result.onAppendagePos is not null)
        {
            room?.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            Shatter();
        }

        room?.PlaySound(SoundID.Coral_Circuit_Break, firstChunk.pos, 1.25f, 1.5f);

        return true;
    }
    public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
    {
        if (obj is Creature ctr)
        {
            if (thrownBy is not null)
            {
                ctr.SetKillTag(thrownBy.abstractCreature);
            }

            if (BaseDamage >= ctr.Template.baseDamageResistance)
            {
                ctr.Die();
            }
            else
            {
                ctr.Stun((int)Custom.LerpMap(BaseDamage, ctr.Template.baseDamageResistance * 2f, ctr.Template.baseDamageResistance, 120, 600));
            }
        }
        base.HitSomethingWithoutStopping(obj, chunk, appendage);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void Shatter()
    {
        if (noBreakGracePeriod > 0 ||
            slatedForDeletetion ||
            room is null)
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

        // Adds particle effects for the crystal being smashed.
        int ParticleCount = 4 + (int)(8 * Size);
        for (int j = 0; j < ParticleCount; j++)
        {
            Vector2 vel = Custom.RNV() * Random.value * 12f;
            if (FreezerCrystal)
            {
                if (j % 3 == 0)
                {
                    EmitIceshard(firstChunk.pos, vel, 2 / 3f, 0.2f, Pitch);
                }
                else
                {
                    EmitSnowflake(firstChunk.pos, vel);
                }
                EmitFreezerMist(firstChunk.pos, vel * 0.7f, 0.2f, smallInsects, true);
            }
            else
            {
                if (j % 2 == 0)
                {
                    EmitIceflake(firstChunk.pos, vel);
                }
                if (j % 4 == 0)
                {
                    EmitIceshard(firstChunk.pos, vel, Size, 0.2f * Size, Pitch);
                }
            }
        }
        room.AddObject(new FreezerMistVisionObscurer(firstChunk.pos, 100, 100, 0.8f, 40));
        room.PlaySound(SoundID.Coral_Circuit_Break, firstChunk.pos, 1.25f, 1.5f);

        if (AbsIce.size - 1f > 0)
        {
            AbsIce.size = Mathf.Max(0, AbsIce.size - 1);
            return;
        }

        if (FrozenObject.obj is not null)
        {
            AbstractPhysicalObject absObj = FrozenObject.obj;
            absObj.world = AbsIce.world;
            absObj.pos = AbsIce.pos;
            room.abstractRoom.AddEntity(absObj);
            absObj.RealizeInRoom();

            PhysicalObject obj = absObj.realizedObject;
            obj.firstChunk.HardSetPosition(firstChunk.pos);
            obj.firstChunk.vel = firstChunk.vel;
            if (grabbedBy.Count > 0)
            {
                for (int g = grabbedBy.Count - 1; g >= 0; g--)
                {
                    Creature.Grasp grasp = grabbedBy[g];
                    grasp.grabber?.Grab(obj, grasp.graspUsed, 0, grasp.shareability, grasp.dominance, true, grasp.pacifying);
                }
            }
        }

        Destroy();
    }
    public virtual void Melt()
    {
        if (noBreakGracePeriod > 0)
        {
            return;
        }

        if (FrozenObject.obj is not null)
        {
            AbstractPhysicalObject absObj = FrozenObject.obj;
            absObj.pos = AbsIce.pos;
            room.abstractRoom.AddEntity(absObj);
            absObj.RealizeInRoom();

            PhysicalObject obj = absObj.realizedObject;
            obj.firstChunk.HardSetPosition(firstChunk.pos);
            obj.firstChunk.vel = firstChunk.vel;
            if (grabbedBy.Count > 0)
            {
                for (int g = grabbedBy.Count - 1; g >= 0; g--)
                {
                    Creature.Grasp grasp = grabbedBy[g];
                    grasp.grabber?.Grab(obj, grasp.graspUsed, 0, grasp.shareability, grasp.dominance, true, grasp.pacifying);
                }
            }
        }
        Destroy();
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public int IceSprites;
    public float SpriteScale = 1f;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        string spriteName = FreezerCrystal ? "IceCrystal" : "IceChunk";
        sLeaser.sprites = new FSprite[3];
        sLeaser.sprites[0] = new FSprite(spriteName + AbsIce.sprite + "A");
        sLeaser.sprites[1] = new FSprite(spriteName + AbsIce.sprite + "B");

        IceSprites = sLeaser.sprites.Length - 1;

        TriangleMesh.Triangle[] trailMesh = new TriangleMesh.Triangle[1]
        {
            new(0, 1, 2)
        };
        sLeaser.sprites[sLeaser.sprites.Length - 1] = new TriangleMesh("Futile_White", trailMesh, customColor: true);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector3 rotat = Vector3.Slerp(lastRotation, rotation, timeStacker);      

        if (vibrate > 0)
        {
            pos += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        }

        for (int i = 0; i < IceSprites; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
            sLeaser.sprites[i].scale = SpriteScale * Mathf.Max(firstChunk.rad/10f, 0);
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(default, rotat);
        }
        UpdateIceColors(sLeaser, rCam, timeStacker, camPos);

        if (mode == Mode.Thrown)
        {
            TriangleMesh trail = sLeaser.sprites[sLeaser.sprites.Length - 1] as TriangleMesh;
            trail.isVisible = true;
            Vector2 trailPos = Vector2.Lerp(tailPos, firstChunk.lastPos, timeStacker);
            Vector2 posDifference = pos - trailPos;
            Vector2 trailDir = Custom.PerpendicularVector(posDifference.normalized);
            trail.MoveVertice(0, pos + trailDir * 3f - camPos);
            trail.MoveVertice(1, pos - trailDir * 3f - camPos);
            trail.MoveVertice(2, trailPos - camPos);
            for (int i = 0; i < trail.verticeColors.Length; i++)
            {
                trail.verticeColors[i] = Color.Lerp(AbsIce.color2, Color.clear, Mathf.InverseLerp(0, trail.verticeColors.Length, i));
            }
        }
        else
        {
            sLeaser.sprites[sLeaser.sprites.Length - 1].isVisible = false;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public virtual void UpdateIceColors(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (blink > 0)
        {
            if (blink > 1 && Random.value < 0.5f)
            {
                sLeaser.sprites[0].color = blinkColor;
                sLeaser.sprites[1].color = blinkColor;
            }
            else
            {
                sLeaser.sprites[0].color = AbsIce.color1;
                sLeaser.sprites[1].color = AbsIce.color2;
            }
        }
        else
        {
            if (sLeaser.sprites[0].color != AbsIce.color1)
            {
                sLeaser.sprites[0].color = AbsIce.color1;
            }
            if (sLeaser.sprites[1].color != AbsIce.color2)
            {
                sLeaser.sprites[1].color = AbsIce.color2;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        //color = Color.Lerp(new Color(129f/255f, 200f/255f, 236f/255f), palette.texture.GetPixel(11, 4), 0.5f); // THIS IS HOW THE GAME PULLS COLORS FROM PALETTES! IMPORTANT!!!!!!
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void EmitSnowflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new HailstormSnowflake(pos, vel, AbsIce.color1, AbsIce.color2));
    }
    public virtual void EmitIceflake(Vector2 pos, Vector2 vel)
    {
        room.AddObject(new PuffBallSkin(pos, vel, AbsIce.color1, AbsIce.color2));
    }
    public virtual void EmitIceshard(Vector2 pos, Vector2 vel, float scale, float shardVolume, float shardPitch)
    {
        Color shardColor = (Random.value < 1.3f) ? AbsIce.color2 : AbsIce.color1;
        if (FreezerCrystal)
        {
            scale *= 1.5f;
        }
        room.AddObject(new Shard(pos, vel, shardVolume, scale, shardPitch, shardColor, true));
    }

    public virtual void EmitFreezerMist(Vector2 pos, Vector2 vel, float size, InsectCoordinator insectCoordinator, bool hasGameplayImpact)
    {
        room.AddObject(new FreezerMist(pos, vel, AbsIce.color1, AbsIce.color2, size, thrownBy?.abstractCreature, insectCoordinator, hasGameplayImpact));
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------