namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class GorditoGraphics : LizardGraphics
{
    public GorditoGreenie liz;

    public Color bodyColor;
    public Color flashColor;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public GorditoGraphics(PhysicalObject owner) : base(owner)
    {
        liz = owner as GorditoGreenie;

        bodyColor = Color.Lerp(Color.gray, liz.effectColor, 0.1f);
        flashColor = Color.Lerp(effectColor, Color.white, 0.6f);
        Random.State state = Random.state;
        Random.InitState(liz.abstractCreature.ID.RandomSeed);

        int cosmeticSprites = startOfExtraSprites + extraSprites;

        cosmeticSprites = AddCosmetic(cosmeticSprites, new SnowAccumulation(this, cosmeticSprites));
        cosmeticSprites = AddCosmetic(cosmeticSprites, new SnowAccumulation(this, cosmeticSprites));

        Random.state = state;

    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (liz.animation == Lizard.Animation.Lounge)
        {
            legsGrabbing = 0;
            frontLegsGrabbing = 0;
            hindLegsGrabbing = 0;
        }

    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        for (int b = SpriteBodyCirclesStart; b < SpriteBodyCirclesEnd; b++)
        {
            sLeaser.sprites[b].element = Futile.atlasManager.GetElementWithName("HailstormCircle40");
        }

    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        for (int b = SpriteBodyCirclesStart; b < SpriteBodyCirclesEnd; b++)
        {
            sLeaser.sprites[b].scale /= 2f;
        }

        // Visuals-related variables
        float headAngleNumber = Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker);
        int headAngle = 3 - (int)(Mathf.Abs(headAngleNumber) * 3.9f);

        // Position-related variables
        Vector2 val6 = Custom.PerpendicularVector(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker) - Vector2.Lerp(head.lastPos, head.pos, timeStacker));

        sLeaser.sprites[SpriteHeadStart].color = GorditoHeadColor(timeStacker, liz.effectColor2);
        sLeaser.sprites[SpriteHeadStart + 3].color = GorditoHeadColor(timeStacker, effectColor);

        /* Sprite Replacements
        // Jaw
        sLeaser.sprites[SpriteHeadStart].element = Futile.atlasManager.GetElementWithName("FreezerJaw0." + headAngle);
        sLeaser.sprites[SpriteHeadStart].color = GorditoHeadColor(timeStacker, liz.effectColor2);

        // Lower Teeth 
        sLeaser.sprites[SpriteHeadStart + 1].element = Futile.atlasManager.GetElementWithName("FreezerLowerTeeth0." + headAngle);

        // Upper Teeth
        sLeaser.sprites[SpriteHeadStart + 2].element = Futile.atlasManager.GetElementWithName("FreezerUpperTeeth0." + headAngle);

        // Head 
        sLeaser.sprites[SpriteHeadStart + 3].element = Futile.atlasManager.GetElementWithName("FreezerHead0." + headAngle);

        // Eyes
        sLeaser.sprites[SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("FreezerEyes0." + headAngle);
        */
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        ColorBody(sLeaser, bodyColor);

    }



    public virtual Color GorditoHeadColor(float timeStacker, Color baseColor)
    {
        float flickerIntensity = 1f - Mathf.Pow(0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(lastBlink, blink, timeStacker) * 2f * Mathf.PI)), 1.5f + (liz.AI.excitement * 1.5f));
        flickerIntensity = Mathf.Lerp(flickerIntensity, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastVoiceVisualization, voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(lastVoiceVisualizationIntensity, voiceVisualizationIntensity, timeStacker));
        return Color.Lerp(flashColor, baseColor, flickerIntensity);
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------