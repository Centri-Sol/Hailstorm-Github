namespace Hailstorm;

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
            return !MMF.cfgVanillaExploits.Value && floatWaterLvl > (room.abstractRoom.size.y + 20) * 20
                ? 1f
                : Mathf.InverseLerp(pos.y - rad, pos.y + rad, floatWaterLvl);
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
            spikes[i] = (i + Random.value) * (360f / spikes.Length);
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
            burn += 1 / 30f;
        }
        int updates = Mathf.Max(1, Mathf.CeilToInt(vel.magnitude / rad));
        for (int m = 0; m < updates; m++)
        {
            lastPos = pos;
            pos += vel / updates;

            Vector2 outerRad = pos + (vel.normalized * rad);
            FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, outerRad, pos - (vel.normalized * rad));
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
                room.AddObject(new Spark(Vector2.Lerp(lastPos, pos, Random.value), (vel * 0.1f) + (Custom.RNV() * 3.2f * Random.value), explodeColor, null, 7, 30));
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
            burn += 1 / 30f;
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
            if (room.GetTile(pos + (angle * 20f)).Solid)
            {
                angle = room.GetTile(pos - (angle * 20f)).Solid ? Custom.RNV() : (angle * -1f);
            }
            for (int j = 0; j < 3; j++)
            {
                room.AddObject(new Spark(pos + (angle * Mathf.Lerp(30f, 60f, Random.value)), (angle * Mathf.Lerp(7f, 38f, Random.value)) + (Custom.RNV() * 20f * Random.value), Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
            }
            room.AddObject(new Explosion.FlashingSmoke(pos + (angle * 40f * Random.value), angle * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + (0.05f * Random.value), new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
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
            if (room.GetTile(pos + (Custom.fourDirectionsAndZero[n].ToVector2() * 20f)).Solid)
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
        Color val3 = Color.Lerp(explodeColor, Color.red, 0.5f + (0.2f * Mathf.Pow(Random.value, 0.2f)));
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
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(0, pos + (perpAngleIthinkIDK * 2f) - camPos);
        (sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(1, pos - (perpAngleIthinkIDK * 2f) - camPos);
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