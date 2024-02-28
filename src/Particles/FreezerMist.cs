namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class FreezerMist : CosmeticSprite
{
    public float life;
    private float lastLife;
    public float lifeTime;

    public float rotation;
    public float lastRotation;
    public float rotVel;

    public Vector2 getToPos;

    public float rad;

    private Color lizEffectColor1;
    private Color lizEffectColor2;

    public AbstractCreature killTag;

    public InsectCoordinator smallInsects;

    private float gradient;
    private readonly float gradientSpeed;
    private bool gradientLerpFlip;

    public bool dangerous;

    public FreezerMist(Vector2 pos, Vector2 vel, Color col1, Color col2, float size, AbstractCreature killTag, InsectCoordinator smallInsects, bool dangerous)
    {
        this.dangerous = dangerous;
        life = size;
        lastLife = size;
        lastPos = pos;
        base.vel = vel * 0.9f;
        lizEffectColor1 = col1;
        lizEffectColor2 = col2;
        this.killTag = killTag;
        this.smallInsects = smallInsects;
        getToPos = pos + new Vector2(Mathf.Lerp(-50f, 50f, Random.value), Mathf.Lerp(-100f, 400f, Random.value));
        base.pos = pos + vel.normalized * 60f * Random.value;
        rad = Mathf.Lerp(1.2f, 1.5f, Random.value) * (1 + (size * 0.1f)) * (dangerous? 1f : 0.75f);
        rotation = Random.value * 360f;
        lastRotation = rotation;
        rotVel = Mathf.Lerp(-6f, 6f, Random.value);
        lifeTime = Mathf.Lerp(320f, 400f, Random.value);
        gradientSpeed = Random.Range(3f, 4f);
        if (Random.value < 0.5f)
        {
            gradient = 120;
        }
    }

    public override void Update(bool eu)
    {
        vel *= 0.9f;
        vel += Custom.DirVec(pos, getToPos) * Random.value * 0.025f;
        lastRotation = rotation;
        rotation += rotVel * vel.magnitude;
        lastLife = life;
        life -= 1f / lifeTime;
        if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
        {
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
            FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
            pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
            if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
            {
                vel.x = Mathf.Abs(vel.x);
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
            {
                vel.x = 0f - Mathf.Abs(vel.x);
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
            {
                vel.y = Mathf.Abs(vel.y);
            }
            else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
            {
                vel.y = 0f - Mathf.Abs(vel.y);
            }
        }

        if (lastLife <= 0f) Destroy();

        if (dangerous && life > 0.075f)
        {
            foreach (AbstractCreature absCtr in room.abstractRoom.creatures)
            {
                if (absCtr?.realizedCreature is not null &&
                    (killTag is null || killTag.creatureTemplate.type != absCtr.creatureTemplate.type) &&
                    absCtr.realizedCreature is Chillipede chl)
                {
                    for (int s = 0; s < chl.ChillState.iceShells.Count; s++)
                    {
                        ChillipedeState.Shell shell = chl.ChillState.iceShells[s];
                        if (shell.timeToRefreeze > 0 && Random.value < 0.3f)
                        {
                            shell.timeToRefreeze--;
                        }
                    }
                }
                if (CustomTemplateInfo.IsColdCreature(absCtr.creatureTemplate.type) || (killTag is not null && killTag == absCtr) || absCtr.realizedCreature is null)
                {
                    continue;
                }
                Creature ctr = absCtr.realizedCreature;
                if (room == ctr.room && Custom.DistLess(pos, ctr.firstChunk.pos, 100) && CWT.CreatureData.TryGetValue(ctr, out CreatureInfo cI))
                {
                    cI.freezerChill +=
                        cI.freezerChill == 0 ? 0.04f :
                        (killTag is not null && killTag.creatureTemplate.type == HailstormCreatures.Chillipede) ? 0.002f : 0.001f;

                    if (killTag is not null && ctr.killTag != killTag)
                    {
                        ctr.SetKillTag(killTag);
                    }
                }
            }
            for (int k = 0; k < room.physicalObjects.Length; k++)
            {
                for (int l = 0; l < room.physicalObjects[k].Count; l++)
                {
                    if (room.physicalObjects[k][l] is SporePlant beehive)
                    {
                        beehive.PuffBallSpores(pos, rad); // Neutralizes beehives.
                    }
                    else if (room.physicalObjects[k][l] is SporePlant.AttachedBee bee &&
                        Custom.DistLess(pos, room.physicalObjects[k][l].firstChunk.pos, rad + 20f))
                    {
                        bee.life -= 0.5f; // Kills bees.
                    }
                }
            }
        }
        if (smallInsects is not null)
        {
            for (int m = 0; m < smallInsects.allInsects.Count; m++)
            {
                if (Custom.DistLess(smallInsects.allInsects[m].pos, pos, rad + 70f))
                {
                    smallInsects.allInsects[m].alive = false; // Instakills background bugs, I assume?
                }
            }
        }

        if (gradient >= 120)
        {
            gradientLerpFlip = true;
        }
        else if (gradient <= 0)
        {
            gradientLerpFlip = false;
        }
        gradient += gradientLerpFlip? -gradientSpeed : gradientSpeed;

        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            shader = rCam.room.game.rainWorld.Shaders["Spores"]
        };
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        float num = Mathf.Lerp(lastLife, life, timeStacker);

        sLeaser.sprites[0].x = val.x - camPos.x;
        sLeaser.sprites[0].y = val.y - camPos.y;
        sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
        sLeaser.sprites[0].scale = 7f * rad * ((num > 0.5f) ? Custom.LerpMap(num, 1f, 0.5f, 0.5f, 1f) : Mathf.Sin(num * Mathf.PI));
        sLeaser.sprites[0].alpha = Mathf.Lerp(1, 0, Mathf.InverseLerp(0.2f, 0, lastLife));
        sLeaser.sprites[0].color = Color.Lerp(lizEffectColor2, lizEffectColor1, Mathf.InverseLerp(-20, 140, gradient));        
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

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------