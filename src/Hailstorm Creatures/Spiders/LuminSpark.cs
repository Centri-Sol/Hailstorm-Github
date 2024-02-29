namespace Hailstorm;

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