using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using RWCustom;
using MoreSlugcats;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class CyanwingGraphics : CentipedeGraphics
{
    public Cyanwing cyn;
    public CyanwingState CyanState => cyn.CyanState;

    public int StartOfExtraShellSprites;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public CyanwingGraphics(PhysicalObject owner)  : base(owner)
    {
        cyn = owner as Cyanwing;

        Random.State state = Random.state;
        Random.InitState(cyn.abstractCreature.ID.RandomSeed);
        hue = cyn.minHue;
        saturation = cyn.saturation;
        Random.state = state;
    }

    //--------------------------------------------------------------------------------

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        for (int side = 0; side < 2; side++)
        {
            for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
            {
                for (int whisker = 0; whisker < 2; whisker++)
                {
                    TriangleMesh whiskerMesh = sLeaser.sprites[WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                    whiskerMesh.customColor = true;
                    whiskerMesh.verticeColors = new Color[15];
                    if (whisker == 0 && whiskerMesh.isVisible)
                    {
                        whiskerMesh.isVisible = false;
                    }
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        blackColor = cyn.bodyColor;

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].color = blackColor;
        }

        for (int legPair = 0; legPair < cyn.bodyChunks.Length; legPair++)
        {
            for (int leg = 0; leg < 2; leg++)
            {
                VertexColorSprite legSprite = sLeaser.sprites[LegSprite(legPair, leg, 1)] as VertexColorSprite;
                legSprite.verticeColors[0] = cyn.offcolor ? SecondaryShellColor : Color.Lerp(Custom.HSL2RGB(cyn.maxHue, cyn.saturation, 0.3f), blackColor, 0.3f + 0.7f * darkness);
                legSprite.verticeColors[1] = cyn.offcolor ? SecondaryShellColor : Color.Lerp(Custom.HSL2RGB(cyn.maxHue, cyn.saturation, 0.3f), blackColor, 0.3f + 0.7f * darkness);
                legSprite.verticeColors[2] = blackColor;
                legSprite.verticeColors[3] = blackColor;
            }
        }

        for (int side = 0; side < 2; side++)
        {
            for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
            {
                for (int whisker = 0; whisker < 2; whisker++)
                {
                    TriangleMesh whiskerMesh = sLeaser.sprites[WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                    for (int v = 0; v < whiskerMesh.verticeColors.Length; v++)
                    {
                        if (v < 5)
                        {
                            whiskerMesh.verticeColors[v] = blackColor;
                        }
                        else
                        {
                            whiskerMesh.verticeColors[v] = Color.Lerp(
                                blackColor,
                                cyn.offcolor ? SecondaryShellColor : Custom.HSL2RGB(0.75f, 0.66f, 0.6f),
                                Mathf.InverseLerp(5, whiskerMesh.verticeColors.Length - 4, v));
                        }

                        if (v >= 5 && whiskerMesh.isVisible)
                        {
                            whiskerMesh.verticeColors[v] = Color.Lerp(whiskerMesh.verticeColors[v], blackColor, darkness * 0.5f);
                        }
                    }
                }
            }
        }

        if (cyn.Glower is not null)
        {
            CyanwingState.Shell headShell = CyanState.superShells[cyn.GlowerHead.index];
            cyn.Glower.color =
                Color.Lerp(
                    Custom.RGB2RGBA(palette.waterColor1, 1),
                    Custom.HSL2RGB(headShell.hue, cyn.saturation, 0.66f),
                    0.5f);
        }
        for (int s = 0; s < totalSecondarySegments; s++)
        {
            float combinedShellHue = (CyanState.superShells[s].hue + CyanState.superShells[s + 1].hue) / 2f;
            sLeaser.sprites[SecondarySegmentSprite(s)].color = Color.Lerp(Custom.HSL2RGB(combinedShellHue, cyn.saturation, 0.2f), blackColor, 0.4f);
        }

    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (CyanState is null ||
            CyanState.shells.Length != cyn.bodyChunks.Length)
        {
            return;
        }

        for (int side = 0; side < 2; side++)
        {
            for (int whiskerPair = 0; whiskerPair < 2; whiskerPair++)
            {
                for (int whisker = 0; whisker < 2; whisker++)
                {
                    TriangleMesh whiskerMesh = sLeaser.sprites[WhiskerSprite(side, whiskerPair, whisker)] as TriangleMesh;
                    for (int v = 1; v < whiskerMesh.vertices.Length; v++)
                    {
                        whiskerMesh.vertices[v] = Vector2.LerpUnclamped(whiskerMesh.vertices[v], whiskerMesh.vertices[0], -0.4f);
                    }
                }
                for (int wingPair = 0; wingPair < wingPairs; wingPair++)
                {
                    CustomFSprite wing = sLeaser.sprites[WingSprite(side, wingPair)] as CustomFSprite;
                    Color chargeCol = Custom.HSL2RGB(CyanState.superShells[wingPair].hue, cyn.saturation, 0.66f);

                    Vector2 val16 =
                            (wingPair != 0) ?
                            Custom.DirVec(ChunkDrawPos(wingPair - 1, timeStacker), ChunkDrawPos(wingPair, timeStacker)) :
                            Custom.DirVec(ChunkDrawPos(0, timeStacker), ChunkDrawPos(1, timeStacker));
                    Vector2 val17 = Custom.PerpendicularVector(val16);
                    Vector2 val18 = RotatAtChunk(wingPair, timeStacker);
                    Vector2 val19 = WingPos(side, wingPair, val16, val17, val18, timeStacker);
                    Vector2 val20 = ChunkDrawPos(wingPair, timeStacker) + cyn.bodyChunks[wingPair].rad * ((side == 0) ? (-1f) : 1f) * val17 * val18.y;
                    Vector2 val21 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(val19, val20) + Custom.VecToDeg(val18));
                    float zRotFac = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(val21, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(val18)), val16));
                    Vector2 val22 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(val20, val19) + Custom.VecToDeg(val18));
                    float num19 = Mathf.InverseLerp(0.85f, 1f, Vector2.Dot(val22, Custom.DegToVec(45f))) * Mathf.Abs(Vector2.Dot(Custom.DegToVec(45f + Custom.VecToDeg(val18)), -val16));
                    zRotFac = Mathf.Pow(Mathf.Max(zRotFac, num19), 0.5f);
                    wing.verticeColors[0] = Color.Lerp(Custom.HSL2RGB(0.75f - 0.4f * Mathf.Pow(zRotFac, 2f), 0.66f, 0.5f + 0.5f * zRotFac, 0.5f + 0.5f * zRotFac), chargeCol, 0.5f * zRotFac);
                    wing.verticeColors[1] = Color.Lerp(Custom.HSL2RGB(0.75f - 0.4f * Mathf.Pow(zRotFac, 2f), 0.66f, 0.5f + 0.5f * zRotFac, 0.5f + 0.5f * zRotFac), chargeCol, 0.5f * zRotFac);
                    wing.verticeColors[2] = Color.Lerp(cyn.bodyColor, chargeCol, 0.5f * zRotFac);
                    wing.verticeColors[3] = Color.Lerp(cyn.bodyColor, chargeCol, 0.5f * zRotFac);
                }
            }
        }

        for (int c = 0; c < cyn.bodyChunks.Length; c++)
        {
            for (int side = 0; side < 1; side++)
            {
                if (!cyn.CentiState.shells[c])
                {
                    break;
                }

                Vector2 firstchunkPos = Vector2.Lerp(cyn.bodyChunks[0].lastPos, cyn.bodyChunks[0].pos, timeStacker);
                firstchunkPos += Custom.DirVec(Vector2.Lerp(cyn.bodyChunks[1].lastPos, cyn.bodyChunks[1].pos, timeStacker), firstchunkPos) * 10f;
                Vector2 chunkZrotation = RotatAtChunk(c, timeStacker);
                Vector2 chunkZrotDir = chunkZrotation.normalized;
                Vector2 chunkPos = Vector2.Lerp(cyn.bodyChunks[c].lastPos, cyn.bodyChunks[c].pos, timeStacker);
                Vector2 nextchunkPos = (c < cyn.bodyChunks.Length - 1) ?
                    Vector2.Lerp(cyn.bodyChunks[c + 1].lastPos, cyn.bodyChunks[c + 1].pos, timeStacker) :
                    (chunkPos + Custom.DirVec(firstchunkPos, chunkPos) * 10f);
                chunkZrotation = firstchunkPos - nextchunkPos;
                float darkFac = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(chunkZrotation.normalized, Custom.DegToVec(30f) * chunkZrotDir.x));
                darkFac *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - chunkZrotDir.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - chunkZrotDir.x)));
                darkFac *= Mathf.Pow(1f - darkness, 2f);

                CyanwingState.Shell shell = CyanState.superShells[c];
                FSprite shellSprite = sLeaser.sprites[ShellSprite(c, side)];

                if (chunkZrotDir.y > 0f)
                {
                    float lightness = 0.75f - (0.25f * darkness * CyanState.ClampedHealth);
                    shellSprite.color = Custom.HSL2RGB(shell.hue, cyn.saturation, lightness);
                    shellSprite.element = Futile.atlasManager.GetElementWithName("CyanwingBackShell");
                }
                else
                {
                    float lightness = Mathf.Lerp(0.75f, 0.5f, CyanState.ClampedHealth) - (0.2f * darkness * CyanState.ClampedHealth);
                    shellSprite.color = Custom.HSL2RGB(shell.hue, cyn.saturation, lightness);
                    shellSprite.element = Futile.atlasManager.GetElementWithName("CyanwingBellyShell");
                }
            }

        }
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------