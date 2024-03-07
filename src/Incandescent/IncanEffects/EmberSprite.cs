namespace Hailstorm;

public class EmberSprite : CosmeticSprite
{
    public float lifeTime;
    public float life;
    public float lastLife;
    public Color color;
    public float size;


    public EmberSprite(Vector2 pos, Color color, float size)
    {
        base.pos = pos;
        this.color = color;
        this.size = size;
        lastPos = pos;
        vel = Custom.RNV() * 1.5f * Random.value;
        life = 1f;
        lifeTime = Mathf.Lerp(10f, 40f, Random.value);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        vel *= 0.8f;
        vel.y += 0.4f;
        vel += Custom.RNV() * Random.value * 0.5f;
        lastLife = life;
        life -= 1f / lifeTime;
        if (life < 0f)
        {
            Destroy();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("deerEyeB");
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
        sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
        float lifetimeMult = Mathf.Lerp(lastLife, life, timeStacker);
        sLeaser.sprites[0].scale = size * lifetimeMult;
        sLeaser.sprites[0].color = color;
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }
}