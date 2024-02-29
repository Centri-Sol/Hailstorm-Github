namespace Hailstorm;

public class InfantAquapedeGraphics : CentipedeGraphics
{
    public InfantAquapede ba;
    public InfantAquapedeState AquababyState => ba.AquababyState;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public InfantAquapedeGraphics(PhysicalObject owner) : base(owner)
    {
        ba = owner as InfantAquapede;

        Random.State state = Random.state;
        Random.InitState(ba.abstractCreature.ID.RandomSeed);
        hue = (260 / 360f) - Mathf.Abs(Custom.WrappedRandomVariation(80 / 360f, 80 / 360f, 0.33f) - (80 / 360f));
        saturation = 1;
        Random.state = state;
    }

    //--------------------------------------------------------------------------------

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        for (int side = 0; side < 2; side++)
        {
            for (int antennaePair = 0; antennaePair < 2; antennaePair++)
            {
                for (int antenna = 0; antenna < 2; antenna++)
                {
                    TriangleMesh antennaMesh = sLeaser.sprites[WhiskerSprite(side, antennaePair, antenna)] as TriangleMesh;
                    for (int v = 1; v < antennaMesh.vertices.Length; v++)
                    {
                        antennaMesh.vertices[v] = Vector2.Lerp(antennaMesh.vertices[v], antennaMesh.vertices[0], 0.4f);
                    }
                }
                for (int wingPair = 0; wingPair < wingPairs; wingPair++)
                {
                    CustomFSprite wing = sLeaser.sprites[WingSprite(side, wingPair)] as CustomFSprite;
                    wing.isVisible = ba.BitesLeft > wingPair;
                    for (int v = 0; v < wing.vertices.Length; v++)
                    {
                        if (v != 3)
                        {
                            wing.vertices[v] = Vector2.Lerp(wing.vertices[v], wing.vertices[3], 0.25f);
                        }
                    }
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (ba.Glower is not null)
        {
            ba.Glower.color = Color.Lerp(new Color(palette.waterColor1.r, palette.waterColor1.g, palette.waterColor1.b, 1f), new Color(0.7f, 0.7f, 1f, 1f), 0.25f);
        }
        blackColor = palette.blackColor;
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].color = blackColor;
        }
        for (int j = 0; j < totalSecondarySegments; j++)
        {
            if (ba.abstractCreature.IsVoided())
            {
                sLeaser.sprites[SecondarySegmentSprite(j)].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, Mathf.Lerp(0.4f, 1f, darkness));
            }
            sLeaser.sprites[SecondarySegmentSprite(j)].color = Color.Lerp(Custom.HSL2RGB(hue, 1f, 0.2f), blackColor, Mathf.Lerp(0.4f, 1f, darkness));
        }
        for (int k = 0; k < ba.bodyChunks.Length; k++)
        {
            for (int l = 0; l < 2; l++)
            {
                (sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[0] = SecondaryShellColor;
                (sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[1] = SecondaryShellColor;
                (sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[2] = blackColor;
                (sLeaser.sprites[LegSprite(k, l, 1)] as VertexColorSprite).verticeColors[3] = blackColor;
            }
        }
    }

}