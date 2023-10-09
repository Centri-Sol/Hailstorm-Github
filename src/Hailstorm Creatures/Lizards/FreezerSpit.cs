using RWCustom;
using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using System.Runtime.ConstrainedExecution;

namespace Hailstorm;

// Unique spit projectiles for the Freezer Lizard, using the code for normal LizardSpit as a base.
public class FreezerSpit : UpdatableAndDeletable, IDrawable
{
    public Vector2 pos;
    public Vector2 lastPos;
    public Vector2 vel;

    public Lizard liz;
    public BodyChunk stickChunk;

    private float massLeft;
    private float gravity;

    public int lifetime;

    private float Rad => 4f * massLeft;

    private BodyChunk myAimChunk;

    private Vector2[,] slime;
    private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();

    public int JaggedSprite => 0;
    public int DotSprite => 1 + slime.GetLength(0);
    public int TotalSprites => slime.GetLength(0) + 2;
    public int SlimeSprite(int s)
    {
        return 1 + s;
    }

    public FreezerSpit(Vector2 startPos, Vector2 startVel, Lizard lizard)
    {
        gravity = 0.7f;
        lastPos = startPos;
        vel = startVel * 0.85f;
        pos = startPos + startVel;
        liz = lizard;
        if (lizard is not null && lizard.LizardState is ColdLizState lS)
        {
            myAimChunk = lS.spitAimChunk;
        }
        massLeft = 1f;        
        slime = new Vector2[(int)Mathf.Lerp(8f, 15f, Random.value), 4];
        for (int i = 0; i < slime.GetLength(0); i++)
        {
            slime[i, 0] = startPos + Custom.RNV() * 4f * Random.value;
            slime[i, 1] = slime[i, 0];
            slime[i, 2] = startVel + Custom.RNV() * 4f * Random.value;
            int num = -1;
            num = ((i != 0 && !(Random.value < 0.3f)) ? ((!(Random.value < 0.7f)) ? Random.Range(0, slime.GetLength(0)) : (i - 1)) : (-1));
            slime[i, 3] = new Vector2((float)num, Mathf.Lerp(3f, 8f, Random.value));
        }
    }

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
                Vector2 val2 = Custom.DirVec(slime[i, 0], pos);
                float num = Vector2.Distance(slime[i, 0], pos);
                ref Vector2 reference3 = ref slime[i, 0];
                reference3 -= val2 * (slime[i, 3].y * massLeft - num) * 0.9f;
                ref Vector2 reference4 = ref slime[i, 2];
                reference4 -= val2 * (slime[i, 3].y * massLeft - num) * 0.9f;
                pos += val2 * (slime[i, 3].y - num) * 0.1f;
                vel += val2 * (slime[i, 3].y - num) * 0.1f;
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
        if (collision)
        {
            myAimChunk = null;
            if (massLeft == 1f)
            {
                massLeft = 0.5f;
                if (stickChunk is null)
                {
                    Explode();
                }
            }
        }

        if (lifetime < 80) lifetime++;
        else if (lifetime < 83) gravity += 0.1f;

        if (stickChunk != null) Explode();
        if (massLeft < 1f) Explode();

        if (myAimChunk != null && ((lastPos.x < myAimChunk.pos.x) != (pos.x < myAimChunk.pos.x)))
        {
            myAimChunk = null;
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
            room.AddObject(new FreezerMist(lastPos, (Custom.RNV() * Random.value * 10f), liz.effectColor, lizEffectColor2, 1, liz.abstractCreature, smallInsects, true));
            if (j < 12)
            {
                room.AddObject(j % 2 == 1 ? // Creates snowflakes on odd numbers, and "ice shards" on even ones.
                        new HailstormSnowflake(lastPos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizEffectColor2) :
                        new PuffBallSkin(lastPos, Custom.RNV() * Random.value * 16f, liz.effectColor, lizEffectColor2));
            }
        }
        room.AddObject(new FreezerMistVisionObscurer(lastPos));

        room.PlaySound(SoundID.Coral_Circuit_Break, lastPos, 1.25f, 1.25f);
        room.PlaySound(SoundID.Coral_Circuit_Break, lastPos, 1.25f, 0.75f);

        Destroy();
    }

    //----------------------------------------------------

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

    Color lizEffectColor2;
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        lizEffectColor2 = HailstormLizards.SecondaryFreezerColor(liz.graphicsModule as LizardGraphics);
        
        if (liz.abstractCreature.creatureTemplate.type == HailstormEnums.Freezer)
        {
            sLeaser.sprites[DotSprite].color = liz.effectColor;
            sLeaser.sprites[JaggedSprite].color = lizEffectColor2;
        }

        for (int i = 0; i < slime.GetLength(0); i++)
        {
            sLeaser.sprites[SlimeSprite(i)].color = liz.effectColor;
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

//----------------------------------------------------------------------
//----------------------------------------------------------------------
//
// Code for the freezing mist that Freezer Spit creates on impact.
//
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
    private float gradientSpeed;
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
                if (absCtr?.realizedCreature is not null && killTag.creatureTemplate.type != absCtr.creatureTemplate.type && absCtr.realizedCreature is Centipede cnt && cnt.CentiState is not null && cnt.CentiState is ChillipedeState cS && cS.ScaleRegenTime is not null)
                {
                    for (int s = 0; s < cS.ScaleRegenTime.Length; s++)
                    {
                        if (cS.ScaleRegenTime[s] > 0 && Random.value < 0.3f) cS.ScaleRegenTime[s]--;
                    }
                }
                if (absCtr.creatureTemplate.type == HailstormEnums.IcyBlue || absCtr.creatureTemplate.type == HailstormEnums.Freezer || absCtr.creatureTemplate.type == HailstormEnums.Chillipede || killTag == absCtr || absCtr.realizedCreature is null) continue;
                Creature ctr = absCtr.realizedCreature;
                if (room == ctr.room && Custom.DistLess(pos, ctr.firstChunk.pos, 100) && CWT.CreatureData.TryGetValue(ctr, out CreatureInfo cI))
                {
                    cI.freezerChill +=
                        cI.freezerChill == 0 ? 0.04f :
                        killTag.creatureTemplate.type == HailstormEnums.Chillipede ? 0.002f : 0.001f;

                    if (ctr.killTag != killTag)
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
        sLeaser.sprites[0] = new FSprite("Futile_White");
        sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Spores"];
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

public class FreezerMistVisionObscurer : VisionObscurer
{
    private float prog;

    public FreezerMistVisionObscurer(Vector2 pos) : base(pos, 160f, 160f, 1f)
    {
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        prog += (1f / 320f);
        obscureFac = Mathf.InverseLerp(1f, 0.3f, prog - 0.5f);
        rad = Mathf.Lerp(70f, 140f, Mathf.Pow(prog, 0.5f));
        if (prog > 1f) Destroy();        
    }
}