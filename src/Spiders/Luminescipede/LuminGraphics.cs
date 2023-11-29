using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using RWCustom;
using static Hailstorm.GlowSpiderState;

namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class LuminGraphics : GraphicsModule
{
    public LuminCreature lmn => owner as LuminCreature;

    private Limb[,] limbs;
    private bool legsPosition;
    private bool lastLegsPosition;
    public float[,] limbGoalDistances;
    private Vector2[,] deathLegPositions;

    public float walkCycle;

    public bool blackedOut;

    public static float[,] legSpriteSizes = new float[4, 2]
        {
            { 19f, 20f },
            { 26f, 20f },
            { 21f, 23f },
            { 26f, 17f }
        };
    public static float[,] limbLengths = new float[4, 2]
        {
            { 0.85f, 0.5f },
            { 1.00f, 0.6f },
            { 0.95f, 0.5f },
            { 0.9f, 0.65f }
        };
    private float limbLength;

    //-----------------------------------------

    public int OutlinesStart => 0;
    private int HeadSprite => 4;
    private int BodySpritesStart => 5;
    private int LegSpritesStart => 9;
    private int DecalSprite => 25;
    private int TotalSprites => 26;

    private int DecalNum;

    private int LimbSprite(int limb, int side, int segment)
    {
        return LegSpritesStart + limb + segment * 4 + side * 8;
    }

    public float GlowSize => 200 * lmn.Juice * lmn.GlowState.ivars.Size * Mathf.Min(1, lmn.BitesLeft/5f);
    public LightSource light;
    public ChunkDynamicSoundLoop lightNoise;

    //-----------------------------------------

    public LuminGraphics(PhysicalObject ow) : base(ow, internalContainers: false)
    {
        limbs = new Limb[4, 2];
        limbGoalDistances = new float[4, 2];
        deathLegPositions = new Vector2[4, 2];
        limbLength = Custom.LerpMap(lmn.GlowState.ivars.Size, 0.8f, 1.2f, 30f, 40f);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                deathLegPositions[i, j] = Custom.DegToVec(Random.value * 360f);
                limbs[i, j] = new Limb(this, lmn.body, i + j * 4, 1f, 0.5f, 0.98f, 15f, 0.95f);
                limbs[i, j].mode = Limb.Mode.Dangle;
                limbs[i, j].pushOutOfTerrain = false;
            }
        }
        legsPosition = Random.value < 0.5f;
        DecalNum = lmn.Role.Index;
        if (lmn.GlowState.dominant)
        {
            DecalNum += Role.values.Count;
        }
    }
    public override void Reset()
    {
        base.Reset();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                limbs[i, j].Reset(lmn.body.pos);
            }
        }
        lightNoise = new ChunkDynamicSoundLoop(lmn.body);
    }

    //-----------------------------------------

    public override void Update()
    {

        if (lightNoise is null)
        {
            Reset();
        }
        lightNoise.Update();
        if (lmn.Juice >= 1)
        {
            lightNoise.sound = SoundID.Mouse_Light_On_LOOP;
            lightNoise.Volume = 1f - (0.6f * lmn.flicker);
            lightNoise.Pitch = 1f - (0.3f * Mathf.Pow(lmn.flicker, 0.6f));
        }
        else if (lmn.Behavior != Behavior.Overloaded && lmn.Consious)
        {
            lightNoise.sound = SoundID.Mouse_Charge_LOOP;
            lightNoise.Volume = 0.25f + lmn.Juice / 2f;
            lightNoise.Pitch = Custom.LerpMap(lmn.Juice, 0, 0.8f, 0.33f, 1);
        }
        else
        {
            lightNoise.sound = SoundID.None;
            lightNoise.Volume = 0f;
        }

        if (lmn.Behavior != Behavior.Hide && lmn.Juice >= 0.05f && light is null)
        {
            light = new LightSource(lmn.body.pos, false, lmn.MainBodyColor, lmn)
            {
                affectedByPaletteDarkness = 0,
                requireUpKeep = true,
                submersible = true
            };
            lmn.room.AddObject(light);
        }
        else if (light is not null)
        {
            light.stayAlive = true;
            light.setPos = new Vector2?(lmn.body.pos);
            light.setRad = new float?(GlowSize * (1 - lmn.flicker) * (1f - lmn.CamoFac));
            light.setAlpha = new float?(lmn.Juice * (1 - (lmn.flicker * 0.4f)));
            light.color = lmn.MainBodyColor;
            if (lmn.Behavior == Behavior.Hide || lmn.Juice < 0.05f || light.slatedForDeletetion || light.room != lmn.room)
            {
                light = null;
            }
        }

        base.Update();
        
        float magnitude = lmn.body.vel.magnitude;
        if (magnitude > 1f)
        {
            walkCycle += Mathf.Max(0f, (magnitude - 1f) / 30f);
            if (walkCycle > 1f)
            {
                walkCycle -= 1f;
            }
        }
        lastLegsPosition = legsPosition;
        legsPosition = walkCycle > 0.5f;
        Vector2 perpToFacedDirection = Custom.PerpendicularVector(lmn.direction);
        for (int limb = 0; limb < 4; limb++)
        {
            for (int side = 0; side < 2; side++)
            {
                Vector2 bodyAngle = lmn.direction;
                bool legOnAltSide = limb % 2 == side == legsPosition;
                bodyAngle = Custom.DegToVec(Custom.VecToDeg(bodyAngle) + Mathf.Lerp(Mathf.Lerp(30f, 140f, limb * (1 / 3f)) + 20f * lmn.legsPosition + 35f * (legOnAltSide ? -1f : 1f) * Mathf.InverseLerp(0.5f, 5f, magnitude), 180f * (0.5f + lmn.legsPosition / 2f), Mathf.Abs(lmn.legsPosition) * 0.3f) * (-1 + 2 * side));
                float limbLength = limbLengths[limb, 0] * this.limbLength;
                Vector2 limbPosGoal = lmn.body.pos + bodyAngle * limbLength * 0.85f + lmn.body.vel.normalized * limbLength * 0.4f * Mathf.InverseLerp(0.5f, 5f, magnitude);
                if (limb == 0 && !lmn.dead && !lmn.idle)
                {
                    limbs[limb, side].pos += Custom.DegToVec(Random.value * 360f) * Random.value;
                }

                bool noFooting = false;
                if (lmn.Consious)
                {
                    limbs[limb, side].mode = Limb.Mode.HuntAbsolutePosition;

                    if ((lmn.AI is not null && !lmn.AI.inAccessibleTerrain) ||
                        (lmn.followingConnection is not null && lmn.followingConnection.type == MovementConnection.MovementType.DropToFloor))
                    {
                        noFooting = true;
                        limbs[limb, side].mode = Limb.Mode.Dangle;
                        Limb limb2 = limbs[limb, side];
                        limb2.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 3f;
                    }
                    else if (limb == 0 && lmn.heavycarryChunk is not null)
                    {
                        noFooting = true;
                        limbs[limb, side].absoluteHuntPos = lmn.heavycarryChunk.pos + perpToFacedDirection * (-1 + 2 * side) * lmn.heavycarryChunk.rad * 0.25f;
                        limbs[limb, side].pos = limbs[limb, side].absoluteHuntPos;
                    }
                }
                else
                {
                    limbs[limb, side].mode = Limb.Mode.Dangle;
                }

                if (limbs[limb, side].mode == Limb.Mode.HuntAbsolutePosition)
                {
                    if (!noFooting)
                    {
                        if (magnitude < 1f)
                        {
                            if (Random.value < 0.05f && !Custom.DistLess(limbs[limb, side].pos, limbPosGoal, limbLength / 6f))
                            {
                                FindGrip(limb, side, limbPosGoal, limbLength, magnitude);
                            }
                        }
                        else if (legOnAltSide && (lastLegsPosition != legsPosition || limb == 3) && !Custom.DistLess(limbs[limb, side].pos, limbPosGoal, limbLength * 0.5f))
                        {
                            FindGrip(limb, side, limbPosGoal, limbLength, magnitude);
                        }
                    }
                }
                else
                {
                    limbs[limb, side].vel += Custom.RotateAroundOrigo(deathLegPositions[limb, side], Custom.AimFromOneVectorToAnother(-lmn.direction, lmn.direction)) * 0.65f;
                    limbs[limb, side].vel += Custom.DegToVec(Random.value * 360f) * lmn.deathSpasms * 5f;
                    limbs[limb, side].vel += bodyAngle * 0.7f;
                    limbs[limb, side].vel.y -= 0.8f;
                    limbGoalDistances[limb, side] = 0f;
                }
                limbs[limb, side].huntSpeed = 15f * Mathf.InverseLerp(-0.05f, 2f, magnitude);
                limbs[limb, side].Update();
                limbs[limb, side].ConnectToPoint(lmn.body.pos, limbLength, push: false, 0f, lmn.body.vel, 1f, 0.5f);
            }
        }

    }

    private void FindGrip(int l, int s, Vector2 idealPos, float rad, float moveSpeed)
    {
        if (lmn.room.GetTile(idealPos).wallbehind)
        {
            limbs[l, s].absoluteHuntPos = idealPos;
        }
        else
        {
            limbs[l, s].FindGrip(lmn.room, lmn.body.pos, idealPos, rad, idealPos + lmn.direction * Mathf.Lerp(moveSpeed * 2f, rad / 2f, 0.5f), 2, 2, behindWalls: true);
        }
        limbGoalDistances[l, s] = Vector2.Distance(limbs[l, s].pos, limbs[l, s].absoluteHuntPos);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[TotalSprites];
        float bodyScale = lmn.GlowState.ivars.Size - 0.1f;
        float bodyWidth = lmn.GlowState.ivars.Fatness;
        for (int b = OutlinesStart; b < HeadSprite; b++)
        {
            sLeaser.sprites[b] = new FSprite("LuminBody" + (b + 1 - OutlinesStart));
            sLeaser.sprites[b].scale = bodyScale + 0.2f;
            sLeaser.sprites[b].scaleY = bodyWidth;
        }

        sLeaser.sprites[HeadSprite] = new FSprite("LuminHead");
        sLeaser.sprites[HeadSprite].scale = bodyScale + (bodyWidth/10f);

        for (int b = BodySpritesStart; b < LegSpritesStart; b++)
        {
            sLeaser.sprites[b] = new FSprite("LuminBody" + (b + 1 - BodySpritesStart));
            sLeaser.sprites[b].scale = bodyScale;
            sLeaser.sprites[b].scaleY = bodyWidth;
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[LimbSprite(i, j, 0)] = new FSprite("SpiderLeg" + i + "A");
                sLeaser.sprites[LimbSprite(i, j, 0)].anchorY = 1f / legSpriteSizes[i, 0];
                sLeaser.sprites[LimbSprite(i, j, 0)].scaleX = (j == 0 ? 1.25f : -1.25f) * lmn.GlowState.ivars.Size;
                sLeaser.sprites[LimbSprite(i, j, 0)].scaleY = limbLengths[i, 0] * limbLengths[i, 1] * limbLength / legSpriteSizes[i, 0];
                sLeaser.sprites[LimbSprite(i, j, 1)] = new FSprite("SpiderLeg" + i + "B");
                sLeaser.sprites[LimbSprite(i, j, 1)].anchorY = 1f / legSpriteSizes[i, 1];
                sLeaser.sprites[LimbSprite(i, j, 1)].scaleX = (j == 0 ? 1.25f : -1.25f) * lmn.GlowState.ivars.Size;
            }
        }
        sLeaser.sprites[DecalSprite] = new FSprite("LuminDecal" + DecalNum);
        sLeaser.sprites[DecalSprite].scale = Mathf.Lerp(bodyScale + 0.1f, 1, 0.5f);

        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!rCam.PositionCurrentlyVisible(lmn.body.pos, 32f, true))
        {
            if (sLeaser.sprites[0].isVisible)
            {
                for (int j = 0; j < sLeaser.sprites.Length; j++)
                {
                    sLeaser.sprites[j].isVisible = false;
                }
            }
            return;
        }

        Vector2 bodyAngle = Vector3.Slerp(lmn.lastDirection, lmn.direction, timeStacker);
        Vector2 bodyPos = Vector2.Lerp(lmn.body.lastPos, lmn.body.pos, timeStacker);
        Vector2 perpToBodyDir = -Custom.PerpendicularVector(bodyAngle);

        if (lmn.lungeTimer > 0 && lmn.lungeTimer < 20)
        {
            bodyPos += Custom.RNV() * Mathf.InverseLerp(0, 20, lmn.lungeTimer) * 2f;
        }
        else if (lmn.flashbombTimer > 0 && lmn.flashbombTimer < 40)
        {
            bodyPos += Custom.RNV() * Mathf.InverseLerp(0, 35, lmn.flashbombTimer) * 2f;
        }

        for (int p = 0; p < LegSpritesStart; p++)
        {
            sLeaser.sprites[p].x = bodyPos.x - camPos.x;
            sLeaser.sprites[p].y = bodyPos.y - camPos.y;
            sLeaser.sprites[p].rotation = Custom.AimFromOneVectorToAnother(-bodyAngle, bodyAngle);
        }
        sLeaser.sprites[DecalSprite].x = bodyPos.x - camPos.x;
        sLeaser.sprites[DecalSprite].y = bodyPos.y - camPos.y;
        sLeaser.sprites[DecalSprite].rotation = Custom.AimFromOneVectorToAnother(-bodyAngle, bodyAngle);

        for (int limb = 0; limb < 4; limb++)
        {
            for (int side = 0; side < 2; side++)
            {
                Vector2 bodyPos2 = bodyPos;
                //bodyPos2 += (bodyAngle * 7f * lmn.GlowState.ivars.Size);
                bodyPos2 += perpToBodyDir * (3f + (limb * 0.5f) - (limb == 3 ? 5.5f : 0f)) * (-1 + 2 * side) * lmn.GlowState.ivars.Size;
                Vector2 limbPos = Vector2.Lerp(limbs[limb, side].lastPos, limbs[limb, side].pos, timeStacker);
                limbPos = Vector2.Lerp(limbPos, bodyPos2 + bodyAngle * limbLength * 0.1f, Mathf.Sin(Mathf.InverseLerp(0f, limbGoalDistances[limb, side], Vector2.Distance(limbPos, limbs[limb, side].absoluteHuntPos)) * Mathf.PI) * 0.4f);
                float num = limbLengths[limb, 0] * limbLengths[limb, 1] * limbLength;
                float num2 = limbLengths[limb, 0] * (1f - limbLengths[limb, 1]) * limbLength;
                float num3 = Vector2.Distance(bodyPos2, limbPos);
                float num4 = ((limb < 3) ? 1f : (-1f));
                if (limb == 2)
                {
                    num4 *= 0.7f;
                }
                if (lmn.legsPosition != 0f)
                {
                    num4 = 1f - 2f * Mathf.Pow(0.5f + 0.5f * lmn.legsPosition, 0.65f);
                }
                num4 *= -1 + (2 * side);
                float num5 = Mathf.Acos(Mathf.Clamp((num3 * num3 + num * num - num2 * num2) / (2f * num3 * num), 0.2f, 0.98f)) * (180f / Mathf.PI) * num4;
                Vector2 bodyPos3 = bodyPos2 + Custom.DegToVec(Custom.AimFromOneVectorToAnother(bodyPos2, limbPos) + num5) * num;
                sLeaser.sprites[LimbSprite(limb, side, 0)].x = bodyPos2.x - camPos.x;
                sLeaser.sprites[LimbSprite(limb, side, 0)].y = bodyPos2.y - camPos.y;
                sLeaser.sprites[LimbSprite(limb, side, 1)].x = bodyPos3.x - camPos.x;
                sLeaser.sprites[LimbSprite(limb, side, 1)].y = bodyPos3.y - camPos.y;
                sLeaser.sprites[LimbSprite(limb, side, 0)].rotation = Custom.AimFromOneVectorToAnother(bodyPos2, bodyPos3);
                sLeaser.sprites[LimbSprite(limb, side, 1)].rotation = Custom.AimFromOneVectorToAnother(bodyPos3, limbPos);
                sLeaser.sprites[LimbSprite(limb, side, 1)].scaleY = Vector2.Distance(bodyPos3, limbPos);
                sLeaser.sprites[LimbSprite(limb, side, 1)].scaleY = limbLengths[limb, 0] * limbLengths[limb, 1] * limbLength / legSpriteSizes[limb, 1];
                sLeaser.sprites[LimbSprite(limb, side, 0)].scaleX = Mathf.Lerp(lmn.GlowState.ivars.dominance, 0.9f, 0.65f);
                sLeaser.sprites[LimbSprite(limb, side, 1)].scaleX = Mathf.Lerp(lmn.GlowState.ivars.dominance, 0.9f, 0.65f);
            }
        }

        for (int s = 0; s < TotalSprites; s++)
        {
            sLeaser.sprites[s].color = lmn.MainBodyColor;
        }
        sLeaser.sprites[DecalSprite].color = lmn.OutlineColor;
        for (int s = OutlinesStart; s < HeadSprite; s++)
        {
            sLeaser.sprites[s].color = lmn.OutlineColor;
        }

        if (lmn.WantToHide && lmn.Behavior != Behavior.Hide && lmn.camoColor != rCam.PixelColorAtCoordinate(lmn.DangerPos))
        {
            lmn.camoColor = rCam.PixelColorAtCoordinate(lmn.DangerPos);
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[HeadSprite].isVisible = lmn.BitesLeft > 0;
        sLeaser.sprites[DecalSprite].isVisible = lmn.BitesLeft > 0;
        if (lmn.BitesLeft > 1 || sLeaser.sprites[OutlinesStart].isVisible)
        {
            sLeaser.sprites[OutlinesStart].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[BodySpritesStart].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[9].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[10].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[13].isVisible = lmn.BitesLeft > 1;
            sLeaser.sprites[14].isVisible = lmn.BitesLeft > 1;
            if (lmn.BitesLeft > 2 || sLeaser.sprites[OutlinesStart + 1].isVisible)
            {
                sLeaser.sprites[OutlinesStart + 1].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[BodySpritesStart + 1].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[11].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[12].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[15].isVisible = lmn.BitesLeft > 2;
                sLeaser.sprites[16].isVisible = lmn.BitesLeft > 2;
                if (lmn.BitesLeft > 3 || sLeaser.sprites[OutlinesStart + 2].isVisible)
                {
                    sLeaser.sprites[OutlinesStart + 2].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[BodySpritesStart + 2].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[17].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[18].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[21].isVisible = lmn.BitesLeft > 3;
                    sLeaser.sprites[22].isVisible = lmn.BitesLeft > 3;
                    if (lmn.BitesLeft > 4 || sLeaser.sprites[OutlinesStart + 3].isVisible)
                    {
                        sLeaser.sprites[OutlinesStart + 3].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[BodySpritesStart + 3].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[19].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[20].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[23].isVisible = lmn.BitesLeft > 4;
                        sLeaser.sprites[24].isVisible = lmn.BitesLeft > 4;
                    }
                }
            }
        }

    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner is null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------