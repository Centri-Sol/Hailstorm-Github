namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ChillipedeGraphics : CentipedeGraphics
{
    public Chillipede chl;
    public ChillipedeState ChillState => chl.ChillState;

    public int StartOfExtraShellSprites;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public ChillipedeGraphics (PhysicalObject owner)  : base(owner)
    {
        chl = owner as Chillipede;

        Random.State state = Random.state;
        Random.InitState(chl.abstractCreature.ID.RandomSeed);
        hue = Random.Range(180 / 360f, 240 / 360f);
        saturation = 1;
        ChillState.topShellColor = Custom.HSL2RGB(hue, saturation, 0.7f + hue / 5f);
        ChillState.bottomShellColor = Custom.HSL2RGB(hue + 30 / 360f, 0.65f, 0.45f + hue / 4f);
        Random.state = state;
    }

    //--------------------------------------------------------------------------------

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        for (int side = 0; side < 2; side++)
        {
            for (int antennaePair = 0; antennaePair < 2; antennaePair++)
            {
                for (int antenna = 0; antenna < 2; antenna++)
                {
                    TriangleMesh antennaMesh = sLeaser.sprites[WhiskerSprite(side, antennaePair, antenna)] as TriangleMesh;
                    antennaMesh.customColor = true;
                    antennaMesh.verticeColors = new Color[15];
                    if (antenna == 0 && antennaMesh.isVisible) antennaMesh.isVisible = false;
                }
            }
        }

        StartOfExtraShellSprites = sLeaser.sprites.Length;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + chl.bodyChunks.Length);
        for (int i = 0; i < ChillState.iceShells.Count; i++)
        {
            ChillipedeState.Shell shell = ChillState.iceShells[i];
            sLeaser.sprites[SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("ChillipedeSegment");
            sLeaser.sprites[ShellSprite(i, 0)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + shell.sprites[0] + ".1");
            sLeaser.sprites[StartOfExtraShellSprites + i] = new("ChillipedeBottomShell" + shell.sprites[1] + ".1");
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        blackColor = Custom.HSL2RGB(hue, 0.1f, 0.2f);

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].color = blackColor;
        }
        for (int legPair = 0; legPair < owner.bodyChunks.Length; legPair++)
        {
            for (int leg = 0; leg < 2; leg++)
            {
                VertexColorSprite legSprite = sLeaser.sprites[LegSprite(legPair, leg, 1)] as VertexColorSprite;
                legSprite.verticeColors[0] = ChillState.topShellColor;
                legSprite.verticeColors[1] = ChillState.topShellColor;
                legSprite.verticeColors[2] = ChillState.bottomShellColor;
                legSprite.verticeColors[3] = blackColor;
                for (int v = 0; v < legSprite.verticeColors.Length; v++)
                {
                    legSprite.verticeColors[v] = Color.Lerp(legSprite.verticeColors[v], blackColor, darkness);
                }
            }
        }
        ColorAntennae(sLeaser);

        for (int s = 0; s < totalSecondarySegments; s++)
        {
            sLeaser.sprites[SecondarySegmentSprite(s)].color = ChillState.bottomShellColor;
        }

    }
    public virtual void ColorAntennae(RoomCamera.SpriteLeaser sLeaser)
    {
        for (int side = 0; side < 2; side++)
        {
            for (int antennaePair = 0; antennaePair < 2; antennaePair++)
            {
                for (int antenna = 0; antenna < 2; antenna++)
                {
                    TriangleMesh antennaMesh = sLeaser.sprites[WhiskerSprite(side, antennaePair, antenna)] as TriangleMesh;
                    for (int v = 0; v < antennaMesh.verticeColors.Length; v++)
                    {
                        if (!antennaMesh.isVisible)
                        {
                            continue;
                        }

                        antennaMesh.verticeColors[v] = v switch
                        {
                            > 11 => ChillState.topShellColor,
                            > 8 => Color.Lerp(ChillState.topShellColor, ChillState.bottomShellColor, Mathf.InverseLerp(11, 9, v)),
                            _ => Color.Lerp(ChillState.bottomShellColor, blackColor, Mathf.InverseLerp(8, 6, v)),
                        };
                        if (v >= 5)
                        {
                            antennaMesh.verticeColors[v] = Color.Lerp(antennaMesh.verticeColors[v], blackColor, darkness / 2f);
                        }
                    }
                }
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        for (int c = 0; c < chl.bodyChunks.Length; c++)
        {
            sLeaser.sprites[SecondarySegmentSprite(c)].scaleX *= 0.85f;
            sLeaser.sprites[SegmentSprite(c)].scaleX = Mathf.Lerp(sLeaser.sprites[SegmentSprite(c)].scaleX, sLeaser.sprites[SegmentSprite(0)].scaleX, 0.5f);
            sLeaser.sprites[SegmentSprite(c)].scale *= 0.8f;

            if (ChillState.iceShells is null)
            {
                continue;
            }

            ChillipedeState.Shell shell = ChillState.iceShells[c];

            for (int s = 0; s < 1; s++)
            {
                bool shellVisible = shell.health > 0;
                sLeaser.sprites[ShellSprite(c, 0)].isVisible = shellVisible;
                sLeaser.sprites[StartOfExtraShellSprites + c].isVisible = shellVisible;

                if (!shellVisible)
                {
                    continue;
                }

                float cntSegmentProgress = c / (float)(chl.bodyChunks.Length - 1);
                Vector2 whoKnows = RotatAtChunk(c, timeStacker);
                Vector2 normalized = whoKnows.normalized;

                float num6 = Mathf.Clamp(Mathf.Sin(cntSegmentProgress * Mathf.PI), 0, 1);
                num6 *= Mathf.Lerp(1f, 0.5f, centipede.size);

                if (normalized.y > 0f)
                {
                    sLeaser.sprites[ShellSprite(c, s)].scaleX = Mathf.Lerp(sLeaser.sprites[ShellSprite(c, s)].scaleX, sLeaser.sprites[ShellSprite(0, s)].scaleX, 0.3f);
                    sLeaser.sprites[ShellSprite(c, s)].scaleX *= 0.55f;
                    sLeaser.sprites[ShellSprite(c, s)].scaleY *= 0.55f;
                    sLeaser.sprites[ShellSprite(c, s)].color = Color.Lerp(ChillState.bottomShellColor, blackColor, darkness / 2f);
                    sLeaser.sprites[ShellSprite(c, s)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + shell.sprites[0] + "." + shell.health);
                    sLeaser.sprites[StartOfExtraShellSprites + c].x = sLeaser.sprites[ShellSprite(c, 0)].x;
                    sLeaser.sprites[StartOfExtraShellSprites + c].y = sLeaser.sprites[ShellSprite(c, 0)].y;
                    sLeaser.sprites[StartOfExtraShellSprites + c].scaleX = sLeaser.sprites[ShellSprite(c, 0)].scaleX;
                    sLeaser.sprites[StartOfExtraShellSprites + c].scaleY = sLeaser.sprites[ShellSprite(c, 0)].scaleY;
                    sLeaser.sprites[StartOfExtraShellSprites + c].rotation = sLeaser.sprites[ShellSprite(c, 0)].rotation;
                    sLeaser.sprites[StartOfExtraShellSprites + c].color = Color.Lerp(ChillState.topShellColor, blackColor, darkness / 2f);
                    sLeaser.sprites[StartOfExtraShellSprites + c].element = Futile.atlasManager.GetElementWithName("ChillipedeBottomShell" + shell.sprites[1] + "." + shell.health);
                }
                else
                {
                    sLeaser.sprites[ShellSprite(c, s)].scaleX = chl.bodyChunks[c].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num6) * 0.125f * normalized.y;
                    sLeaser.sprites[ShellSprite(c, s)].scaleX = Mathf.Lerp(sLeaser.sprites[ShellSprite(c, s)].scaleX, sLeaser.sprites[ShellSprite(0, s)].scaleX, 0.3f);
                    sLeaser.sprites[ShellSprite(c, s)].scaleX *= 0.55f;
                    sLeaser.sprites[ShellSprite(c, s)].scaleY *= 0.55f;
                    sLeaser.sprites[ShellSprite(c, s)].color = Color.Lerp(ChillState.bottomShellColor, blackColor, Mathf.Lerp(0.25f, 0.75f, darkness));
                    sLeaser.sprites[ShellSprite(c, s)].element = Futile.atlasManager.GetElementWithName("ChillipedeTopShell" + shell.sprites[0] + "." + shell.health);
                    sLeaser.sprites[StartOfExtraShellSprites + c].x = sLeaser.sprites[ShellSprite(c, 0)].x;
                    sLeaser.sprites[StartOfExtraShellSprites + c].y = sLeaser.sprites[ShellSprite(c, 0)].y;
                    sLeaser.sprites[StartOfExtraShellSprites + c].scaleX = sLeaser.sprites[ShellSprite(c, 0)].scaleX;
                    sLeaser.sprites[StartOfExtraShellSprites + c].scaleY = sLeaser.sprites[ShellSprite(c, 0)].scaleY;
                    sLeaser.sprites[StartOfExtraShellSprites + c].rotation = sLeaser.sprites[ShellSprite(c, 0)].rotation;
                    sLeaser.sprites[StartOfExtraShellSprites + c].color = Color.Lerp(ChillState.topShellColor, blackColor, Mathf.Lerp(0.25f, 0.75f, darkness));
                    sLeaser.sprites[StartOfExtraShellSprites + c].element = Futile.atlasManager.GetElementWithName("ChillipedeBottomShell" + shell.sprites[1] + "." + shell.health);
                }
            }

        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
        if (sLeaser.sprites.Length > TotalSprites)
        {
            var foregroundContainer = rCam.ReturnFContainer("Foreground");
            var midgroundContainer = rCam.ReturnFContainer("Midground");

            for (int s = TotalSprites; s < sLeaser.sprites.Length; s++)
            {
                foregroundContainer.RemoveChild(sLeaser.sprites[s]);
                midgroundContainer.AddChild(sLeaser.sprites[s]);
            }

            for (int side = 0; side < 2; side++)
            {
                for (int antennaePair = 0; antennaePair < 2; antennaePair++)
                {
                    for (int antennae = 0; antennae < 2; antennae++)
                    {
                        sLeaser.sprites[WhiskerSprite(side, antennaePair, antennae)].MoveToBack();
                    }
                }
            }
        }
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------