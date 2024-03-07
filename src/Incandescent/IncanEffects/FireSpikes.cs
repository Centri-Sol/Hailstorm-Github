namespace Hailstorm;

public class FireSpikes : ExplosionSpikes
{
    private Color color2;
    public FireSpikes(Room room, Vector2 pos, int spikes, float innerRad, float lifeTime, float width, float length, Color color, Color color2) : base(room, pos, spikes, innerRad, lifeTime, width, length, color)
    {
        base.room = room;
        this.innerRad = innerRad;
        base.pos = pos;
        this.color = color;
        this.color2 = color2;
        this.lifeTime = lifeTime;
        base.spikes = spikes;
        values = new float[spikes, 3];
        dirs = (Vector2[])(object)new Vector2[spikes];
        float num = Random.value * 360f;
        for (int i = 0; i < spikes; i++)
        {
            float num2 = (i / (float)spikes * 360f) + num;
            dirs[i] = Custom.DegToVec(num2 + (Mathf.Lerp(-0.5f, 0.5f, Random.value) * 360f / spikes));
            if (room.GetTile(pos + (dirs[i] * (innerRad + (length * 0.4f)))).Solid)
            {
                values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, Random.value) * 0.5f;
                values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, Random.value) * 0.5f;
            }
            else
            {
                values[i, 2] = lifeTime * Mathf.Lerp(0.5f, 1.5f, Random.value);
                values[i, 0] = length * Mathf.Lerp(0.6f, 1.4f, Random.value);
            }
            values[i, 1] = width * Mathf.Lerp(0.6f, 1.4f, Random.value);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[spikes];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new TriangleMesh.Triangle(i * 3, (i * 3) + 1, (i * 3) + 2);
        }
        TriangleMesh triangleMesh = new("Futile_White", array, customColor: true);
        sLeaser.sprites[0] = triangleMesh;
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float num = time + timeStacker;
        TriangleMesh tMesh = sLeaser.sprites[0] as TriangleMesh;
        for (int i = 0; i < spikes; i++)
        {
            float num2 = Mathf.InverseLerp(0f, values[i, 2], num);
            float num3 = (time == 0) ? timeStacker : Mathf.InverseLerp(values[i, 2], 0f, num);
            float num4 = Mathf.Lerp(values[i, 0] * 0.1f, values[i, 0], Mathf.Pow(num2, 0.45f));
            float num5 = values[i, 1] * (0.5f + (0.5f * Mathf.Sin(num2 * Mathf.PI))) * Mathf.Pow(num3, 0.3f);
            Vector2 val = pos + (dirs[i] * (innerRad + num4));
            if (room != null && room.GetTile(val).Solid)
            {
                num3 *= 0.5f;
            }
            Vector2 val2 = pos + (dirs[i] * (innerRad + (num4 * 0.1f)));
            Vector2 val3 = Custom.PerpendicularVector(val, val2);
            tMesh.MoveVertice(i * 3, val - camPos);
            tMesh.MoveVertice((i * 3) + 1, val2 - (val3 * num5 * 0.5f) - camPos);
            tMesh.MoveVertice((i * 3) + 2, val2 + (val3 * num5 * 0.5f) - camPos);
            tMesh.verticeColors[i * 3] = Custom.RGB2RGBA(color2, Mathf.Pow(num3, 0.6f));
            tMesh.verticeColors[(i * 3) + 1] = Custom.RGB2RGBA(color, 0);
            tMesh.verticeColors[(i * 3) + 2] = Custom.RGB2RGBA(color, 0);
        }
    }
}