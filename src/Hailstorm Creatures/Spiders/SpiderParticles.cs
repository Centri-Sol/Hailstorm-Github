namespace Hailstorm;

//----------------------------------------------------------------------------------

public class LuminSpark : CosmeticSprite
{
    private readonly float lifeTime;
    private float life;

    private readonly int graphic;

    private Color startColor;
    private Color fadeColor;
    private float Fade => Mathf.InverseLerp(0.75f, 1, life);

    private float dir;

    public LuminSpark(int sparkType, Vector2 pos, Vector2 vel, float maxLifeTime, Color startColor, Color fadeColor)
    {
        lastPos = pos;
        base.pos = pos;
        base.vel = vel;
        this.startColor = startColor;
        this.fadeColor = fadeColor;
        dir = Custom.AimFromOneVectorToAnother(-vel, vel);
        lifeTime = Mathf.Lerp(5f, maxLifeTime, Random.value);
        graphic = sparkType;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        life += 1f / lifeTime;
        if (life > 1)
        {
            Destroy();
        }
        vel *= 0.7f;
        vel += Custom.DegToVec(dir) * Random.value * 2f;
        dir += Mathf.Lerp(-17f, 17f, Random.value);
        if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
        {
            if (room.GetTile(lastPos).Terrain != Room.Tile.TerrainType.Solid)
            {
                vel *= 0f;
                pos = lastPos;
            }
            else
            {
                Destroy();
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("LuminSpark" + graphic)
        {
            color = startColor
        };
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        sLeaser.sprites[0].color = Color.Lerp(startColor, fadeColor, Fade);
        sLeaser.sprites[0].scale = 1 - Fade;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

//----------------------------------------------------------------------------------

public class LuminBlink : CosmeticSprite
{

    private float rad;

    private float lastRad;

    private float radVel;

    private readonly float initRad;

    private readonly float lifeTime;

    private float lastLife;

    private float life;

    private readonly float intensity;

    private Vector2 aimPos;

    private Color color1;
    private Color color2;
    private Color blackCol;

    public LuminBlink(Vector2 startPos, Vector2 aimPos, Vector2 startVel, float intensity, Color color1, Color color2)
    {
        pos = startPos;
        lastPos = pos;
        vel = startVel;
        this.intensity = intensity;
        this.aimPos = aimPos;
        this.color1 = color1;
        this.color2 = color2;
        radVel = Mathf.Lerp(1.4f, 4.2f, intensity);
        initRad = Mathf.Lerp(8f, 12f, intensity);
        rad = initRad;
        lastRad = initRad;
        life = 1f;
        lastLife = 0f;
        lifeTime = Mathf.Lerp(6f, 30f, Mathf.Pow(intensity, 4f));
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRad = rad;
        rad += radVel;
        radVel *= 0.92f;
        radVel -= Mathf.InverseLerp(0.6f + 0.3f * intensity, 0f, life) * Mathf.Lerp(0.2f, 0.6f, intensity);
        Vector2 val = pos + Custom.DirVec(pos, aimPos) * 80f * Mathf.Sin(life * Mathf.PI);
        pos = Vector2.Lerp(pos, val, 0.3f * (1f - Mathf.Sin(life * Mathf.PI)));
        lastLife = life;
        life = Mathf.Max(0f, life - 1f / lifeTime);
        if (lastLife <= 0f && life <= 0f)
        {
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["VectorCircle"]
        };
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        float num = Mathf.Lerp(lastLife, life, timeStacker);
        float num2 = Mathf.InverseLerp(0f, 0.75f, num);
        sLeaser.sprites[0].color = Color.Lerp((num2 > 0.5f) ? color2 : blackCol, Color.Lerp(color2, color1, 0.5f + 0.5f * intensity), Mathf.Sin(num2 * Mathf.PI));
        float num3 = Mathf.Lerp(lastRad, rad, timeStacker);
        sLeaser.sprites[0].scale = num3 / 8f;
        sLeaser.sprites[0].alpha = Mathf.Sin(Mathf.Pow(num, 2f) * Mathf.PI) * 2f / num3;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        blackCol = palette.blackColor;
    }
}

//----------------------------------------------------------------------------------

public class LuminFlash : CosmeticSprite
{
    private readonly BodyChunk followChunk;
    private readonly AbstractCreature killTag;
    private LightSource light;

    private float life;
    private float lastLife;
    private readonly int lifeTime;

    private Color color;
    private readonly bool uncontrolled;

    private Vector2 lastDirection;
    private Vector2 direction;
    private readonly float baseRad;
    private float lastRad;
    public float rad;
    private float lastAlpha;
    private float alpha;
    public float LightIntensity => Mathf.Pow(Mathf.Sin(life * Mathf.PI), 0.4f);

    public LuminFlash(Room room, BodyChunk source, float baseRad, int lifeTime, Color color, float flashPitch, bool uncontrolled) : this(room, new Vector2(0, 0), baseRad, lifeTime, color, flashPitch, uncontrolled)
    {
        followChunk = source;
        if (followChunk?.owner is not null && followChunk.owner is Creature ctr)
        {
            killTag = ctr.abstractCreature;
        }
    }
    public LuminFlash(Room room, Vector2 pos, float baseRad, int lifeTime, Color color, float flashPitch, bool uncontrolled)
    {
        this.room = room;
        base.pos = (followChunk is not null) ? followChunk.pos : pos;
        this.baseRad = baseRad;
        this.lifeTime = lifeTime;
        this.color = color;
        this.room.PlaySound(SoundID.Flare_Bomb_Burn, base.pos, 1.2f, flashPitch);
        this.uncontrolled = uncontrolled;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastLife = life;
        life += 1f / lifeTime;
        if (lastLife > 1)
        {
            Destroy();
            return;
        }
        bool owned = followChunk?.owner is not null;
        if (owned && room != followChunk.owner.room)
        {
            room = followChunk.owner.room;
        }
        if (room is null)
        {
            return;
        }
        lastDirection = direction;
        direction = Custom.DegToVec(Random.value * 360f) * (baseRad/64f) * LightIntensity;
        lastAlpha = alpha;
        alpha = Mathf.Pow(Random.value, 0.3f) * LightIntensity * 0.6f;
        lastRad = rad;
        rad = Mathf.Pow(Random.value, 0.3f) * baseRad * LightIntensity;
        for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
        {
            Creature ctr = room.abstractRoom.creatures[i].realizedCreature;
            if (ctr is null || !Custom.DistLess(pos, ctr.mainBodyChunk.pos, baseRad) || !room.VisualContact(pos, ctr.mainBodyChunk.pos))
            {
                continue;
            }
            if (ctr.Template.type == CreatureTemplate.Type.Spider && !ctr.dead)
            {
                ctr.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                ctr.Die();
            }
            else if (ctr is BigSpider bs && bs.Template.type == CreatureTemplate.Type.BigSpider)
            {
                bs.poison = 1f;
                bs.State.health -= Random.value * 0.2f;
                bs.Stun(Random.Range(10, 20));
                if (killTag is not null)
                {
                    bs.SetKillTag(killTag);
                }
            }
            else if (owned && ctr != followChunk.owner && ctr.State is not null && ctr.State is GlowSpiderState gs && gs.juice < gs.MaxJuice)
            {
                gs.juice += (uncontrolled ? 0.04f : 0.0025f);
            }
            if (ctr.State is null || ctr.State is not GlowSpiderState)
            {
                ctr.Blind((int)Custom.LerpMap(Vector2.Distance(pos, ctr.VisionPoint), baseRad/5f, baseRad, 800f, 200f));
            }
        }

        if (light is null)
        {
            light = new LightSource(pos, false, color, this)
            {
                affectedByPaletteDarkness = 0,
                requireUpKeep = true,
                submersible = true,
                color = color
            };
            room.AddObject(light);
        }
        else
        {
            light.stayAlive = true;
            light.setPos = pos;
            light.setAlpha = Mathf.InverseLerp(lifeTime * 0.75f, 0, Mathf.Abs(life - (lifeTime / 2f)));
            light.setRad = Mathf.Max(rad, 60f + (LightIntensity * 10f));
            if (light.slatedForDeletetion || light.room != room)
            {
                light = null;
            }
        }
    }

    public override void Destroy()
    {
        light?.Destroy();
        base.Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["FlareBomb"],
            scale = 2.5f
        };
        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (followChunk is not null)
        {
            lastPos = pos;
            pos = Vector2.Lerp(followChunk.lastPos, followChunk.pos, timeStacker);
        }
        sLeaser.sprites[0].x = pos.x - camPos.x + Mathf.Lerp(lastDirection.x, direction.x, timeStacker);
        sLeaser.sprites[0].y = pos.y - camPos.y + Mathf.Lerp(lastDirection.y, direction.y, timeStacker);
        sLeaser.sprites[0].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
        sLeaser.sprites[0].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker) * 0.6f;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = color;
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Water");
        newContatiner.AddChild(sLeaser.sprites[0]);
    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------