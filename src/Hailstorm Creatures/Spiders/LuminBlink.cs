namespace Hailstorm;

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
        radVel -= Mathf.InverseLerp(0.6f + (0.3f * intensity), 0f, life) * Mathf.Lerp(0.2f, 0.6f, intensity);
        Vector2 val = pos + (Custom.DirVec(pos, aimPos) * 80f * Mathf.Sin(life * Mathf.PI));
        pos = Vector2.Lerp(pos, val, 0.3f * (1f - Mathf.Sin(life * Mathf.PI)));
        lastLife = life;
        life = Mathf.Max(0f, life - (1f / lifeTime));
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
        sLeaser.sprites[0].color = Color.Lerp((num2 > 0.5f) ? color2 : blackCol, Color.Lerp(color2, color1, 0.5f + (0.5f * intensity)), Mathf.Sin(num2 * Mathf.PI));
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