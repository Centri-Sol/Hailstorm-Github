using RWCustom;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class FreezerSpit : UpdatableAndDeletable, IDrawable
{
    public Vector2 pos;
    public Vector2 lastPos;
    public Vector2 vel;

    public BodyChunk stickChunk;

    public AbstractCreature killtag;
    public Color color1;
    public Color color2;

    private float massLeft;
    private float gravity;

    public int lifetime;
    public float ExplosionRadius => 120f;

    private float Rad => 4f * massLeft;

    private Vector2[,] slime;
    private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();

    public int JaggedSprite => 0;
    public int DotSprite => 1 + slime.GetLength(0);
    public int TotalSprites => slime.GetLength(0) + 2;
    public int SlimeSprite(int s)
    {
        return 1 + s;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public FreezerSpit(Vector2 startPos, Vector2 startVel, Lizard liz)
    {
        gravity = 0.7f;
        lastPos = startPos;
        vel = startVel * 0.85f;
        pos = startPos + startVel;
        massLeft = 1f;
        slime = new Vector2[(int)Mathf.Lerp(8f, 15f, Random.value), 4];
        for (int i = 0; i < slime.GetLength(0); i++)
        {
            slime[i, 0] = startPos + Custom.RNV() * 4f * Random.value;
            slime[i, 1] = slime[i, 0];
            slime[i, 2] = startVel + Custom.RNV() * 4f * Random.value;
            int num = (i != 0 && Random.value < 0.7f) ? (Random.value < 0.3f ? Random.Range(0, slime.GetLength(0)) : i - 1) : -1;
            slime[i, 3] = new Vector2(num, Mathf.Lerp(3, 8, Random.value));
        }
        killtag = liz.abstractCreature;
        color1 = liz.effectColor;
        if (liz is ColdLizard cLiz)
        {
            color2 = cLiz.effectColor2;
        }
        else
        {
            color2 = new Color(0.1f, 0.1f, 0.1f);
        }
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        lastPos = pos;
        pos += vel;
        vel.y -= gravity;
        for (int i = 0; i < slime.GetLength(0); i++)
        {
            slime[i, 1] = slime[i, 0];
            ref Vector2 reference = ref slime[i, 0];
            reference += slime[i, 2];
            ref Vector2 reference2 = ref slime[i, 2];
            reference2 *= 0.99f;
            slime[i, 2].y -= 0.9f * (ModManager.MMF ? room.gravity : 1f);
            if ((int)slime[i, 3].x < 0 || (int)slime[i, 3].x >= slime.GetLength(0))
            {
                Vector2 travelDir = Custom.DirVec(slime[i, 0], pos);
                float distBetweenPoints = Vector2.Distance(slime[i, 0], pos);
                ref Vector2 reference3 = ref slime[i, 0];
                reference3 -= travelDir * (slime[i, 3].y * massLeft - distBetweenPoints) * 0.9f;
                ref Vector2 reference4 = ref slime[i, 2];
                reference4 -= travelDir * (slime[i, 3].y * massLeft - distBetweenPoints) * 0.9f;
                pos += travelDir * (slime[i, 3].y - distBetweenPoints) * 0.1f;
                vel += travelDir * (slime[i, 3].y - distBetweenPoints) * 0.1f;
            }
            else
            {
                Vector2 val3 = Custom.DirVec(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
                float num2 = Vector2.Distance(slime[i, 0], slime[(int)slime[i, 3].x, 0]);
                ref Vector2 reference5 = ref slime[i, 0];
                reference5 -= val3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
                ref Vector2 reference6 = ref slime[i, 2];
                reference6 -= val3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
                ref Vector2 reference7 = ref slime[(int)slime[i, 3].x, 0];
                reference7 += val3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
                ref Vector2 reference8 = ref slime[(int)slime[i, 3].x, 2];
                reference8 += val3 * (slime[i, 3].y * massLeft - num2) * 0.5f;
            }
        }
        bool collision = false;

        if (!collision)
        {
            SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, Rad, new IntVector2(0, 0), goThroughFloors: true);
            cd = SharedPhysics.VerticalCollision(room, cd);
            cd = SharedPhysics.HorizontalCollision(room, cd);
            cd = SharedPhysics.SlopesVertically(room, cd);
            pos = cd.pos;
            vel = cd.vel;
            if (cd.contactPoint.x != 0)
            {
                vel.x = Mathf.Abs(vel.x) * 0.2f * -cd.contactPoint.x;
                vel.y *= 0.8f;
                collision = true;
            }
            if (cd.contactPoint.y != 0)
            {
                vel.y = Mathf.Abs(vel.y) * 0.2f * -cd.contactPoint.y;
                vel.x *= 0.8f;
                collision = true;
            }
        }
        /*
        SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos, ref pos, Rad, 1, lizard, hitAppendages: false);
        if (collisionResult.chunk is not null)
        {
            pos = collisionResult.collisionPoint;
            stickChunk = collisionResult.chunk;
            vel *= 0.25f;
            if (stickChunk.owner is not Creature)
            {
                stickChunk.vel += vel * 0.6f / Mathf.Max(1f, stickChunk.mass);
                collision = true;
            }
            Explode();
        }
        */
        if (collision &&
            massLeft == 1f)
        {
            massLeft = 0.5f;
            if (stickChunk is null)
            {
                Explode();
            }
        }

        if (lifetime < 80)
        {
            lifetime++;
        }
        else if (lifetime < 83)
        {
            gravity += 0.1f;
        }

        if (stickChunk is not null || massLeft < 1f)
        {
            Explode();
        }

        base.Update(eu);
    }

    private Vector2 StuckPosOfSlime(int s, float timeStacker)
    {
        if ((int)slime[s, 3].x < 0 || (int)slime[s, 3].x >= slime.GetLength(0))
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }
        return Vector2.Lerp(slime[(int)slime[s, 3].x, 1], slime[(int)slime[s, 3].x, 0], timeStacker);
    }

    public void Explode()
    {
        if (slatedForDeletetion)
        {
            return;
        }

        float distFac;
        for (int e = room.abstractRoom.entities.Count - 1; e >= 0; e--)
        {
            if (room.abstractRoom.entities[e] is null ||
                room.abstractRoom.entities[e] is not AbstractPhysicalObject absObj ||
                absObj.realizedObject is null)
            {
                continue;
            }

            if (absObj.realizedObject is Creature ctr)
            {
                if (CustomTemplateInfo.IsColdCreature(ctr.Template.type))
                {
                    continue;
                }

                distFac = Mathf.InverseLerp(ExplosionRadius, ExplosionRadius/5f, Custom.Dist(pos, ctr.DangerPos));
                if (distFac <= 0)
                {
                    continue;
                }

                ctr.Hypothermia += 0.1f * distFac;
                if (killtag is not null)
                {
                    ctr.killTag = killtag;
                    ctr.killTagCounter = (int)(400 * distFac);
                }
            }
            else
            {
                PhysicalObject obj = absObj.realizedObject;

                distFac = Mathf.InverseLerp(ExplosionRadius, ExplosionRadius/5f, Custom.Dist(pos, obj.firstChunk.pos));
                if (distFac <= 0)
                {
                    continue;
                }

                if (obj is IceChunk ice)
                {
                    ice.absIce.size += distFac;
                    ice.absIce.freshness += distFac * 0.5f;
                    ice.absIce.color1 = Color.Lerp(ice.absIce.color1, color1, distFac);
                    ice.absIce.color2 = Color.Lerp(ice.absIce.color2, color2, distFac);
                }
                if (!CustomObjectInfo.FreezableObjects.ContainsKey(obj.abstractPhysicalObject.type))
                {
                    continue;
                }


                AbstractIceChunk newAbsIce = new(obj.abstractPhysicalObject.world, obj.abstractPhysicalObject.pos, obj.abstractPhysicalObject.world.game.GetNewID());
                newAbsIce.frozenObject = new FrozenObject(obj.abstractPhysicalObject);
                newAbsIce.size = distFac;
                newAbsIce.freshness = distFac * 0.5f;
                newAbsIce.color1 = color1;
                newAbsIce.color2 = color2;
                obj.abstractPhysicalObject.Room.AddEntity(newAbsIce);
                newAbsIce.RealizeInRoom();

                IceChunk newIce = newAbsIce.realizedObject as IceChunk;
                newIce.firstChunk.HardSetPosition(obj.firstChunk.pos);
                newIce.firstChunk.vel = obj.firstChunk.vel;
                if (obj.grabbedBy.Count > 0)
                {
                    for (int g = obj.grabbedBy.Count - 1; g >= 0; g--)
                    {
                        Creature.Grasp grasp = obj.grabbedBy[g];
                        if (grasp.grabber is not null)
                        {
                            grasp.grabber.Grab(newIce, grasp.graspUsed, 0, grasp.shareability, grasp.dominance, true, grasp.pacifying);
                        }
                    }
                }
                obj.RemoveFromRoom();
                obj.abstractPhysicalObject.Room.RemoveEntity(obj.abstractPhysicalObject);
            }
        }

        InsectCoordinator smallInsects = null;
        for (int i = 0; i < room.updateList.Count; i++)
        {
            if (room.updateList[i] is InsectCoordinator)
            {
                smallInsects = room.updateList[i] as InsectCoordinator;
                break;
            }
        }
        for (int j = 0; j < 60; j++)
        {
            room.AddObject(new FreezerMist(lastPos, (Custom.RNV() * Random.value * 10f), color1, color2, 1, killtag, smallInsects, true));
            if (j < 12)
            {
                room.AddObject(j % 2 == 1 ? // Creates snowflakes on odd numbers, and "ice shards" on even ones.
                        new HailstormSnowflake(lastPos, Custom.RNV() * Random.value * 16f, color1, color2) :
                        new PuffBallSkin(lastPos, Custom.RNV() * Random.value * 16f, color1, color2));
            }
        }
        room.AddObject(new FreezerMistVisionObscurer(lastPos, ExplosionRadius, ExplosionRadius, 0.8f, 320));

        room.PlaySound(SoundID.Coral_Circuit_Break, lastPos, 1.25f, 1.25f);
        room.PlaySound(SoundID.Coral_Circuit_Break, lastPos, 1.25f, 0.75f);

        Destroy();
    }

    //--------------------------------------------------------------------------------

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[TotalSprites];

        sLeaser.sprites[DotSprite] = new FSprite("Futile_White");
        sLeaser.sprites[DotSprite].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
        sLeaser.sprites[DotSprite].alpha = Random.value * 0.5f;

        sLeaser.sprites[JaggedSprite] = new FSprite("Futile_White");
        sLeaser.sprites[JaggedSprite].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
        sLeaser.sprites[JaggedSprite].alpha = Random.value * 0.5f;

        for (int i = 0; i < slime.GetLength(0); i++)
        {
            sLeaser.sprites[SlimeSprite(i)] = new FSprite("Futile_White");
            sLeaser.sprites[SlimeSprite(i)].anchorY = 0.05f;
            sLeaser.sprites[SlimeSprite(i)].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
            sLeaser.sprites[SlimeSprite(i)].alpha = Random.value;
        }
        AddToContainer(sLeaser, rCam, null);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(lastPos, pos, timeStacker);
        float num = Mathf.InverseLerp(30f, 6f, Vector2.Distance(lastPos, pos));
        float num2 = Mathf.InverseLerp(6f, 30f, Mathf.Lerp(Vector2.Distance(lastPos, pos), Vector2.Distance(val, Vector2.Lerp(slime[0, 1], slime[0, 0], timeStacker)), num));
        Vector2 v = Vector3.Slerp(Custom.DirVec(lastPos, pos), Custom.DirVec(val, Vector2.Lerp(slime[0, 1], slime[0, 0], timeStacker)), num);

        sLeaser.sprites[DotSprite].x = val.x - camPos.x;
        sLeaser.sprites[DotSprite].y = val.y - camPos.y;
        sLeaser.sprites[DotSprite].rotation = Custom.VecToDeg(v);
        sLeaser.sprites[DotSprite].scaleX = Mathf.Lerp(0.4f, 0.2f, num2) * massLeft;
        sLeaser.sprites[DotSprite].scaleY = Mathf.Lerp(0.3f, 0.7f, num2) * massLeft;

        sLeaser.sprites[JaggedSprite].x = val.x - camPos.x;
        sLeaser.sprites[JaggedSprite].y = val.y - camPos.y;
        sLeaser.sprites[JaggedSprite].rotation = Custom.VecToDeg(v);
        sLeaser.sprites[JaggedSprite].scaleX = Mathf.Lerp(0.6f, 0.4f, num2) * massLeft;
        sLeaser.sprites[JaggedSprite].scaleY = Mathf.Lerp(0.5f, 1f, num2) * massLeft;
        for (int i = 0; i < slime.GetLength(0); i++)
        {
            Vector2 val2 = Vector2.Lerp(slime[i, 1], slime[i, 0], timeStacker);
            Vector2 val3 = StuckPosOfSlime(i, timeStacker);
            sLeaser.sprites[SlimeSprite(i)].x = val2.x - camPos.x;
            sLeaser.sprites[SlimeSprite(i)].y = val2.y - camPos.y;
            sLeaser.sprites[SlimeSprite(i)].scaleY = (Vector2.Distance(val2, val3) + 3f) / 16f;
            sLeaser.sprites[SlimeSprite(i)].rotation = Custom.AimFromOneVectorToAnother(val2, val3);
            sLeaser.sprites[SlimeSprite(i)].scaleX = Custom.LerpMap(Vector2.Distance(val2, val3), 0f, slime[i, 3].y * 3.5f, 6f, 2f, 2f) * massLeft / 16f;
        }
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[DotSprite].color = color1;
        sLeaser.sprites[JaggedSprite].color = color2;

        for (int i = 0; i < slime.GetLength(0); i++)
        {
            sLeaser.sprites[SlimeSprite(i)].color = color1;
        }
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[i]);
        }
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------