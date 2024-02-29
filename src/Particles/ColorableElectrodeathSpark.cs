namespace Hailstorm;

public class ColorableElectrodeathSpark : CosmeticSprite
{
    private readonly float size;

    private float lastLife;
    private float life;
    private readonly float lifeTime;

    private Color color;

    public ColorableElectrodeathSpark(Vector2 pos, float size, Color color)
    {
        base.pos = pos;
        lastPos = pos;
        this.size = size;
        life = 1f;
        lastLife = 1f;
        lifeTime = Mathf.Lerp(12f, 16f, size * Random.value);
        this.color = color;
    }

    public override void Update(bool eu)
    {
        room.AddObject(new Spark(pos, Custom.RNV() * 60f * Random.value, color, null, 4, 50));
        if (life <= 0f && lastLife <= 0f)
        {
            Destroy();
            return;
        }
        lastLife = life;
        life = Mathf.Max(0f, life - (1f / lifeTime));
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["LightSource"],
            color = color
        };

        sLeaser.sprites[1] = new("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["FlatLight"],
            color = color
        };

        sLeaser.sprites[2] = new("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["FlareBomb"],
            color = color
        };

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float lifespanFac = Mathf.Lerp(lastLife, life, timeStacker);
        for (int i = 0; i < 3; i++)
        {
            sLeaser.sprites[i].x = pos.x - camPos.x;
            sLeaser.sprites[i].y = pos.y - camPos.y;
        }
        float sizeFac = Mathf.Lerp(20f, 120f, Mathf.Pow(size, 1.5f));

        sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac * 4f / 8f;
        sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, Random.value) * 0.75f;

        sLeaser.sprites[1].scale = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac * 4f / 8f;
        sLeaser.sprites[1].alpha = Mathf.Pow(Mathf.Sin(lifespanFac * Mathf.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, Random.value) * 0.15f;

        sLeaser.sprites[2].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(lifespanFac * Mathf.PI)) * Mathf.Lerp(0.8f, 1.2f, Random.value) * sizeFac / 8f;
        sLeaser.sprites[2].alpha = Mathf.Sin(lifespanFac * Mathf.PI) * Random.value * 0.75f;
    }
}