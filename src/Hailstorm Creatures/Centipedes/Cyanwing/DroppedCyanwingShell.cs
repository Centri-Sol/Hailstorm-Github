namespace Hailstorm;

public class DroppedCyanwingShell : CosmeticSprite
{
    public Centipede cnt;

    public float rotation;
    public float lastRotation;
    public float rotVel;
    private float zRotation;
    private float lastZRotation;
    private float zRotVel;

    public float lastDarkness = -1f;
    public float darkness;

    private readonly float scaleX;
    private readonly float scaleY;

    private readonly SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new();

    public int fuseTime;

    public bool Gilded;
    private Color scaleColor;
    private Color blackColor;
    private Color currentShellColor;
    private readonly bool isBackShell;

    public LightSource shellLight;

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
                return 1f - Mathf.InverseLerp(pos.y - 5, pos.y + 5, room.FloatWaterLevel(pos));
            }
            float num = room.FloatWaterLevel(pos);
            return ModManager.MMF && !MMF.cfgVanillaExploits.Value && num > (room.abstractRoom.size.y + 20) * 20f
                ? 1f
                : Mathf.InverseLerp(pos.y - 5, pos.y + 5, num);
        }
    }

    public DroppedCyanwingShell(Centipede cnt, Vector2 pos, Vector2 vel, Color color, float scaleX, float scaleY, int fuseTime, bool topShell)
    {
        this.fuseTime = fuseTime;
        this.cnt = cnt;
        base.pos = pos + vel;
        lastPos = pos;
        base.vel = vel;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        rotation = Random.value * 360f;
        lastRotation = rotation;
        rotVel = Mathf.Lerp(-1f, 1f, Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 5f, 26f);
        zRotation = Random.value * 360f;
        lastZRotation = rotation;
        zRotVel = Mathf.Lerp(-1f, 1f, Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 2f, 16f);
        this.fuseTime = fuseTime;
        scaleColor = color;
        isBackShell = topShell;
    }

    public override void Update(bool eu)
    {
        if (room.PointSubmerged(pos))
        {
            vel *= 0.92f;
            vel.y -= room.gravity * 0.1f;
            rotVel *= 0.965f;
            zRotVel *= 0.965f;
        }
        else
        {
            vel *= 0.999f;
            vel.y -= room.gravity * 0.9f;
        }
        if (Random.value < Mathf.Max(0.1f, Mathf.InverseLerp(80, 0, fuseTime) / 4f))
        {
            room.AddObject(new WaterDrip(Vector2.Lerp(lastPos, pos, Random.value), vel + (Custom.RNV() * Random.value * 2f), waterColor: false));
        }
        lastRotation = rotation;
        rotation += rotVel * Vector2.Distance(lastPos, pos);
        lastZRotation = zRotation;
        zRotation += zRotVel * Vector2.Distance(lastPos, pos);
        if (!Custom.DistLess(lastPos, pos, 3f) && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
        {
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
            FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
            pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
            bool flag = false;
            if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
            {
                vel.x = Mathf.Abs(vel.x) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
            {
                vel.x = (0f - Mathf.Abs(vel.x)) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
            {
                vel.y = Mathf.Abs(vel.y) * 0.15f;
                flag = true;
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
            {
                vel.y = (0f - Mathf.Abs(vel.y)) * 0.15f;
                flag = true;
            }
            if (flag)
            {
                rotVel *= 0.8f;
                zRotVel *= 0.8f;
                if (vel.magnitude > 3f)
                {
                    rotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value * Mathf.Abs(rotVel / 15f);
                    zRotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value * Mathf.Abs(rotVel / 15f);
                }
            }
        }
        SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 3f, new IntVector2(0, 0), goThroughFloors: true);
        cd = SharedPhysics.VerticalCollision(room, cd);
        cd = SharedPhysics.HorizontalCollision(room, cd);
        pos = cd.pos;
        vel = cd.vel;
        if (cd.contactPoint.x != 0)
        {
            vel.y *= 0.6f;
        }
        if (cd.contactPoint.y != 0)
        {
            vel.x *= 0.6f;
        }
        if (cd.contactPoint.y < 0)
        {
            rotVel *= 0.7f;
            zRotVel *= 0.7f;
            if (vel.magnitude < 1f)
            {
                fuseTime--;
            }
        }
        if (shellLight is null && !slatedForDeletetion)
        {
            shellLight = new LightSource(pos, false, currentShellColor, this)
            {
                submersible = true,
                affectedByPaletteDarkness = 0
            };
            room.AddObject(shellLight);
        }
        else if (shellLight is not null)
        {
            float radiusLerp =
                fuseTime > 30 ?
                Mathf.InverseLerp((fuseTime % 30 > 15) ? 30 : 0, 15, fuseTime % 30) :
                Mathf.InverseLerp(30, 5, fuseTime);

            shellLight.color = currentShellColor;
            shellLight.setPos = new Vector2?(pos);
            shellLight.setRad = new float?(100 * Mathf.Lerp(0.5f, fuseTime > 30 ? 1.5f : 2.5f, radiusLerp));
            shellLight.setAlpha = new float?(1);
            if (shellLight.slatedForDeletetion || shellLight.room != room || slatedForDeletetion)
            {
                shellLight = null;
            }

        }
        base.Update(eu);
        if (fuseTime <= 0 && !slatedForDeletetion)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (this is null || slatedForDeletetion || room is null) return;

        room.AddObject(new ColorableZapFlash(pos, 2.4f, scaleColor));
        room.PlaySound(SoundID.Zapper_Zap, pos, 0.8f, Random.Range(0.5f, 1.5f));
        if (Submersion > 0.5f)
        {
            room.AddObject(new UnderwaterShock(room, null, pos, 10, 450f, 0.25f, cnt ?? null, scaleColor));
        }
        else foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
            {
                if (absCtr.realizedCreature?.bodyChunks is null)
                {
                    continue;
                }

                Creature ctr = absCtr.realizedCreature;

                bool hit = false;

                for (int b = ctr.bodyChunks.Length - 1; b >= 0; b--)
                {
                    BodyChunk chunk = ctr.bodyChunks[b];
                    if (chunk is null || !Custom.DistLess(pos, chunk.pos, 150) || !room.VisualContact(pos, chunk.pos) || ctr is BigEel || (ctr is Centipede && ctr is not Chillipede) || ctr is BigJellyFish || ctr is Inspector)
                    {
                        continue;
                    }

                    ctr.Violence(cnt.mainBodyChunk ?? null, new Vector2(0, 0), chunk, null, Creature.DamageType.Electric, 0.1f, ctr is Player ? 30 : (20f * Mathf.Lerp(ctr.Template.baseStunResistance, 1f, 0.5f)));
                    room.AddObject(new CreatureSpasmer(ctr, false, ctr.stun));
                    if (!hit) hit = true;
                }
                if (hit)
                {
                    room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, pos);
                    room.AddObject(new Explosion.ExplosionLight(pos, 150f, 1f, 4, scaleColor));
                }
            }
        Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        string spriteName = isBackShell ? "CyanwingBellyShell" : "CyanwingBackShell";
        sLeaser.sprites = new FSprite[2];
        sLeaser.sprites[0] = new FSprite(spriteName);
        sLeaser.sprites[1] = new FSprite(spriteName);
        for (int s = 0; s < sLeaser.sprites.Length; s++)
        {
            sLeaser.sprites[s].scaleY = scaleY;
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        lastDarkness = darkness;
        darkness = rCam.room.Darkness(val);
        darkness *= 1f - (0.5f * rCam.room.LightSourceExposure(val));
        Vector2 Zrotation = Custom.DegToVec(Mathf.Lerp(lastZRotation, zRotation, timeStacker));
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].x = val.x - camPos.x;
            sLeaser.sprites[i].y = val.y - camPos.y;
            sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[i].scaleX = Mathf.Abs(Zrotation.x) < 0.1f ? 0.1f * Mathf.Sign(Zrotation.x) * scaleX : Zrotation.x * scaleX;
        }
        sLeaser.sprites[0].x += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).x * 1.5f;
        sLeaser.sprites[0].y += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).y * 1.5f;

        if (Gilded)
        {
            sLeaser.sprites[0].color = Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.7f + (0.3f * darkness)), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            sLeaser.sprites[1].color = Zrotation.y > 0f
                ? Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness), Color.white, Mathf.InverseLerp(30, 0, fuseTime))
                : Color.Lerp(Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.4f + (0.6f * darkness)), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
        }
        else
        {
            sLeaser.sprites[0].color = Color.Lerp(scaleColor, Color.white, Mathf.InverseLerp(30, 0, fuseTime));
            sLeaser.sprites[1].color = Zrotation.y > 0f
                ? Color.Lerp(scaleColor, Color.white, Mathf.InverseLerp(30, 0, fuseTime))
                : Color.Lerp(Color.Lerp(scaleColor, blackColor, 0.5f), Color.white, Mathf.InverseLerp(30, 0, fuseTime));
        }

        if (sLeaser.sprites[0] is not null)
        {
            currentShellColor = sLeaser.sprites[0].color;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }
}