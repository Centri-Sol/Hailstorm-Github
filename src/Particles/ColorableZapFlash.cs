namespace Hailstorm;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

public class ColorableZapFlash : CosmeticSprite
{
    private LightSource lightsource;

    private float life;
    private float lastLife;
    private readonly float lifeTime;

    private readonly float size;

    private Color color;

    public ColorableZapFlash(Vector2 initPos, float size, Color color)
    {
        this.size = size;
        lifeTime = Mathf.Lerp(1f, 4f, Random.value) + 2f * size;
        life = 1f;
        lastLife = 1f;
        pos = initPos;
        lastPos = initPos;
        this.color = color;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (lightsource is null)
        {
            lightsource = new LightSource(pos, false, color, this);
            room.AddObject(lightsource);
        }
        lastLife = life;
        life -= 1f / lifeTime;
        if (lastLife < 0f)
        {
            lightsource?.Destroy();
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            color = color,
            shader = rCam.room.game.rainWorld.Shaders["FlareBomb"]
        };

        sLeaser.sprites[1] = new FSprite("Futile_White")
        {
            color = Color.white,
            shader = rCam.room.game.rainWorld.Shaders["FlatLight"]
        };

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float lifespanFac = Mathf.Lerp(lastLife, life, timeStacker);
        for (int i = 0; i < 2; i++)
        {
            sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
            sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        }
        if (lightsource is not null)
        {
            lightsource.HardSetRad(Mathf.Lerp(0.25f, 1f, Random.value * lifespanFac * size) * 2400f);
            lightsource.HardSetAlpha(Mathf.Pow(lifespanFac * Random.value, 0.4f));
            float colorSkew = Mathf.Pow(lifespanFac * Random.value, 4f);
            lightsource.color = Color.Lerp(color, Color.white, colorSkew);
        }
        sLeaser.sprites[0].scale = Mathf.Lerp(0.5f, 1f, Random.value * lifespanFac * size) * 500f / 16f;
        sLeaser.sprites[0].alpha = lifespanFac * Random.value * 0.75f;

        sLeaser.sprites[1].scale = Mathf.Lerp(0.5f, 1f, (0.5f + 0.5f * Random.value) * lifespanFac * size) * 400f / 16f;
        sLeaser.sprites[1].alpha = lifespanFac * Random.value * 0.75f;
    }
}

//----------------------------------------------------------------------------------