namespace Hailstorm;

//--------------------------------------------------------------------------------

public class Shard : CosmeticSprite
{
    public float scale = 1;
    public float volume = 1;
    public float pitch = 1;
    public Color? color;

    public float rotation;
    public float lastRotation;
    public float rotVel;

    public bool iceShard;

    public Shard(Vector2 pos, Vector2 vel, float scale, float impactSoundVolume, float impactSoundPitch, Color? col = null, bool icy = false)
    {
        base.pos = pos + vel * 2f;
        lastPos = pos;
        base.vel = vel;
        rotation = Random.value * 360f;
        lastRotation = rotation;
        rotVel = Mathf.Lerp(-26f, 26f, Random.value);

        this.scale = scale;
        volume = impactSoundVolume;
        pitch = impactSoundPitch;
        color = col;

        iceShard = icy;
    }

    public override void Update(bool eu)
    {
        vel *= 0.999f;
        vel.y -= room.gravity * 0.9f;
        lastRotation = rotation;
        rotation += rotVel * vel.magnitude;
        if (Vector2.Distance(lastPos, pos) > 18f && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
        {
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
            FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
            pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
            bool hitTerrain = false;
            if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
            {
                vel.x = Mathf.Abs(vel.x) * 0.5f;
                hitTerrain = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
            {
                vel.x = (0f - Mathf.Abs(vel.x)) * 0.5f;
                hitTerrain = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
            {
                vel.y = Mathf.Abs(vel.y) * 0.5f;
                hitTerrain = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
            {
                vel.y = (0f - Mathf.Abs(vel.y)) * 0.5f;
                hitTerrain = true;
            }
            if (hitTerrain)
            {
                if (iceShard)
                {
                    room.PlaySound(SoundID.Coral_Circuit_Break, pos, volume, pitch);
                    InsectCoordinator smallInsects = null;
                    for (int i = 0; i < room.updateList.Count; i++)
                    {
                        if (room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }

                    if (!color.HasValue)
                    {
                        color = new Color(0.1f, 0.1f, 0.1f);
                    }

                    for (int j = 0; j < scale * 4f; j++)
                    {
                        room.AddObject(new FreezerMist(pos, Custom.RNV() * vel.magnitude * Random.value, color.Value, color.Value, 0.2f, null, smallInsects, false));
                    }

                    Destroy();
                }
                else
                {
                    rotVel *= 0.8f;
                    rotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value;
                    room.PlaySound(SoundID.Spear_Fragment_Bounce, pos, volume, pitch);
                }
            }
        }
        if ((room.GetTile(pos).Solid && room.GetTile(lastPos).Solid) || pos.x < -100f)
        {
            Destroy();
        }
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("SpearFragment" + (1 + Random.Range(0, 2)))
        {
            scaleX = (Random.value < 0.5f ? -1f : 1f) * scale,
            scaleY = (Random.value < 0.5f ? -1f : 1f) * scale
        };
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        sLeaser.sprites[0].x = val.x - camPos.x;
        sLeaser.sprites[0].y = val.y - camPos.y;
        sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color ??= palette.blackColor;
        sLeaser.sprites[0].color = color.Value;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}

//--------------------------------------------------------------------------------
