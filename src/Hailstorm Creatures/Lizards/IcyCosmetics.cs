namespace Hailstorm;

public class IcyCosmetics
{
    public static void Init()
    {
        On.LizardBubble.DrawSprites += SnowflakeBubbles;
        //------------------------------------------------------
        On.LizardCosmetics.BumpHawk.ctor += BumpHawkNegation;
        On.LizardCosmetics.SpineSpikes.ctor += SpineSpikeNegation;
        On.LizardCosmetics.TailTuft.ctor += TailTuftNegation;

        On.LizardCosmetics.LongHeadScales.DrawSprites += IcyLongHeadScaleColors;

        On.LizardCosmetics.ShortBodyScales.ctor += ShortBodyScaleNegation;
        On.LizardCosmetics.ShortBodyScales.DrawSprites += SwimmyLizShortBodyScales;

        On.LizardCosmetics.LongBodyScales.InitiateSprites += IcyLongBodyScaleSprites;
        On.LizardCosmetics.LongBodyScales.DrawSprites += IcyLongBodyScaleColors;

        On.LizardCosmetics.SnowAccumulation.DrawSprites += GorditoGreenieSnow;
    }

    //-----------------------------------------------------------------------
    //-----------------------------------------------------------------------

    public static void SnowflakeBubbles(On.LizardBubble.orig_DrawSprites orig, LizardBubble bubble, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(bubble, sLeaser, rCam, timeStacker, camPos);
        LizardGraphics liz = bubble.lizardGraphics;
        if (liz.lizard is ColdLizard cLiz)
        {
            Color.RGBToHSV(liz.lizard.effectColor, out float h, out float s, out float v);
            h *= (h * 1.2272f > 0.75f) ? 0.7728f : 1.2272f;
            v -= 0.1f;
            Color secondaryLizColor = Color.HSVToRGB(h, s, v);

            int colorPicker = Random.Range(0, 1);
            Color bubbleColor = colorPicker == 0 ? liz.effectColor : secondaryLizColor;

            int spriteNum = 0;
            if (bubble.hollow)
            {
                spriteNum = Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(bubble.lifeTimeWhenFree, 0f, bubble.life), bubble.hollowNess) * 7f), 1, 7);
            }
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("Snowflake" + Random.Range(0, 1) + "." + spriteNum);

            if (cLiz.Freezer)
            {
                sLeaser.sprites[0].color = Color.Lerp(LizardHooks.FreezerHeadColor(liz, timeStacker, bubbleColor), Color.white, 1f - Mathf.Clamp(Mathf.Lerp(bubble.lastLife, bubble.life, timeStacker) * 2f, 0f, 1f));
                sLeaser.sprites[0].scale = 0.85f + Random.Range(0.01f, 0.50f);
            }
            else if (cLiz.IcyBlue)
            {
                Color hBHC = LizardHooks.IcyBlueHeadColor(liz, timeStacker, bubbleColor);
                sLeaser.sprites[0].color = Color.Lerp(hBHC, hBHC * 1.25f, 1f - Mathf.Clamp(Mathf.Lerp(bubble.lastLife, bubble.life, timeStacker) * 2f, 0f, 1f));
                sLeaser.sprites[0].scale = 0.5f + Random.Range(0.01f, 0.30f);
            }
        }
    }

    //------------------------------------------------------
    public static void BumpHawkNegation(On.LizardCosmetics.BumpHawk.orig_ctor orig, BumpHawk BH, LizardGraphics liz, int startSprite)
    {
        orig(BH, liz, startSprite);
        if (liz.lizard.Template.type == HSEnums.CreatureType.FreezerLizard || liz.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            BH.numberOfSprites = 0;
        }
    }
    public static void SpineSpikeNegation(On.LizardCosmetics.SpineSpikes.orig_ctor orig, SpineSpikes SS, LizardGraphics liz, int startSprite)
    {
        orig(SS, liz, startSprite);
        if (liz.lizard.Template.type == HSEnums.CreatureType.FreezerLizard || liz.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            SS.numberOfSprites = 0;
            SS.bumps = 0;
        }
    }
    public static void TailTuftNegation(On.LizardCosmetics.TailTuft.orig_ctor orig, TailTuft TT, LizardGraphics liz, int startSprite)
    {
        orig(TT, liz, startSprite);
        if (liz.lizard.Template.type == HSEnums.CreatureType.FreezerLizard || liz.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            Array.Resize(ref TT.scaleObjects, TT.scaleObjects.Length - TT.scalesPositions.Length);
            Array.Resize(ref TT.scalesPositions, TT.scalesPositions.Length - (TT.colored ? TT.numberOfSprites / 2 : TT.numberOfSprites));
            TT.numberOfSprites = 0;
        }
    }

    public static void IcyLongHeadScaleColors(On.LizardCosmetics.LongHeadScales.orig_DrawSprites orig, LongHeadScales LHS, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(LHS, sLeaser, rCam, timeStacker, camPos);
        _ = LHS.lGraphics.lizard.abstractCreature.ID.number % 2 == 0;

        if (LHS.lGraphics.lizard.Template.type == HSEnums.CreatureType.FreezerLizard || LHS.lGraphics.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            for (int num = LHS.startSprite + LHS.scalesPositions.Length - 1; num >= LHS.startSprite; num--)
            {
                sLeaser.sprites[num].color =
                    sLeaser.sprites[LHS.lGraphics.SpriteHeadStart + 3].color;
                if (LHS.colored)
                {
                    sLeaser.sprites[num + LHS.scalesPositions.Length].color =
                        sLeaser.sprites[LHS.lGraphics.SpriteHeadStart].color;
                }
            }
        }
    }

    public static void ShortBodyScaleNegation(On.LizardCosmetics.ShortBodyScales.orig_ctor orig, ShortBodyScales SBS, LizardGraphics liz, int startSprite)
    {
        orig(SBS, liz, startSprite);
        if (SBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.FreezerLizard ||
            SBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            Array.Resize(ref SBS.scalesPositions, SBS.scalesPositions.Length - SBS.numberOfSprites);
            SBS.numberOfSprites = 0;
        }
    }
    public static void SwimmyLizShortBodyScales(On.LizardCosmetics.ShortBodyScales.orig_DrawSprites orig, ShortBodyScales SBS, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(SBS, sLeaser, rCam, timeStacker, camPos);
        if (OtherCreatureChanges.IsIncanStory(SBS?.lGraphics?.lizard?.room?.game) && SBS.lGraphics.lizard.abstractCreature.Winterized && (SBS.lGraphics.lizard.Template.type == CreatureTemplate.Type.Salamander || SBS.lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
        {
            for (int num = SBS.startSprite + SBS.scalesPositions.Length - 1; num >= SBS.startSprite; num--)
            {
                sLeaser.sprites[num].color = SwimmyLizShortscaleColors(SBS.lGraphics, timeStacker);
            }
        }
    }

    public static void IcyLongBodyScaleSprites(On.LizardCosmetics.LongBodyScales.orig_InitiateSprites orig, LongBodyScales LBS, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        LBS.spritesOverlap = Template.SpritesOverlap.BehindHead;
        orig(LBS, sLeaser, rCam);

        bool icyCheck =
            LBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.FreezerLizard ||
            (LBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard && Random.value < (0.2f + ((LBS.lGraphics.lizard.TotalMass - 1.4f) * 4)));

        if (icyCheck)
        {
            int graphic = Random.Range(0, 8);
            for (int num = LBS.startSprite + LBS.scalesPositions.Length - 1; num >= LBS.startSprite; num--)
            {
                sLeaser.sprites[num].element = Futile.atlasManager.GetElementWithName("LongHeadIceSpike" + graphic);
                if (icyCheck)
                {
                    sLeaser.sprites[num].scale = 1.5f;
                }

                if (LBS.colored)
                {
                    sLeaser.sprites[num + LBS.scalesPositions.Length].element = Futile.atlasManager.GetElementWithName("LongHeadIceSpikeB" + graphic);
                    if (icyCheck)
                    {
                        sLeaser.sprites[num + LBS.scalesPositions.Length].scale = 1.5f;
                    }
                }
            }
        }
    }
    public static void IcyLongBodyScaleColors(On.LizardCosmetics.LongBodyScales.orig_DrawSprites orig, LongBodyScales LBS, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(LBS, sLeaser, rCam, timeStacker, camPos);
        if (LBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.FreezerLizard || LBS.lGraphics.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            for (int num = LBS.startSprite + LBS.scalesPositions.Length - 1; num >= LBS.startSprite; num--)
            {
                sLeaser.sprites[num].color =
                        (LBS.lGraphics.lizard.abstractCreature.ID.number % 2 == 0) ? sLeaser.sprites[LBS.lGraphics.SpriteHeadStart + 3].color : sLeaser.sprites[LBS.lGraphics.SpriteHeadStart].color;
                if (LBS.colored)
                {
                    sLeaser.sprites[num + LBS.scalesPositions.Length].color =
                       (LBS.lGraphics.lizard.abstractCreature.ID.number % 4 == 0) ? sLeaser.sprites[LBS.lGraphics.SpriteHeadStart].color : sLeaser.sprites[LBS.lGraphics.SpriteHeadStart + 3].color;
                }
            }
        }
    }

    public static void GorditoGreenieSnow(On.LizardCosmetics.SnowAccumulation.orig_DrawSprites orig, SnowAccumulation snow, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(snow, sLeaser, rCam, timeStacker, camPos);
        if (OtherCreatureChanges.IsIncanStory(snow?.lGraphics?.lizard?.room?.game) && snow.lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && snow.lGraphics.lizard.abstractCreature.Winterized)
        {
            for (int i = 0; i < snow.numberOfSprites; i++)
            {
                sLeaser.sprites[snow.startSprite + i].scaleX *= 2.2f;
                sLeaser.sprites[snow.startSprite + i].scaleY *= 2.2f;
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------

    public static Color SwimmyLizShortscaleColors(LizardGraphics liz, float timeStacker)
    {
        if (liz.whiteFlicker > 0 && (liz.whiteFlicker > 15 || liz.everySecondDraw))
        {
            return new Color(1, 1, 1);
        }
        float num = 1f - Mathf.Pow(0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(liz.lastBlink, liz.blink, timeStacker) * 2f * Mathf.PI)), 1.5f + (liz.lizard.AI.excitement * 1.5f));
        if (liz.headColorSetter != 0f)
        {
            num = Mathf.Lerp(num, (liz.headColorSetter > 0f) ? 1 : 0, Mathf.Abs(liz.headColorSetter));
        }
        if (liz.flicker > 10)
        {
            num = liz.flickerColor;
        }
        num = Mathf.Lerp(num, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(liz.lastVoiceVisualization, liz.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(liz.lastVoiceVisualizationIntensity, liz.voiceVisualizationIntensity, timeStacker));

        Color scaleCol = liz.effectColor;
        if (liz.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard && CWT.AbsCtrData.TryGetValue(liz.lizard.abstractCreature, out CWT.AbsCtrInfo aI))
        {
            Vector3 e = Custom.RGB2HSL(scaleCol);
            float skew = aI.functionTimer / 1000f;
            scaleCol = Custom.HSL2RGB(e.x + skew, e.y + skew, e.z + skew);
        }
        if (liz.lizard.Template.type == CreatureTemplate.Type.Salamander)
        {
            scaleCol += new Color(0.1f, 0.1f, 0.1f);
        }
        return Color.Lerp(liz.HeadColor1, scaleCol, num);
    }

}

public class ArmorIceSpikes : Template
{
    private readonly Lizard liz;

    public int backSpikes;

    public float spikeLength;

    public float sizeSkewExponent;

    public float sizeRangeMin;

    public float sizeRangeMax;

    public float scaleFac;

    public int graphic;

    public float scaleX;

    public int dualColored;

    public int endSprite;

    public ArmorIceSpikes(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        liz = lGraphics.owner as Lizard;
        spritesOverlap = SpritesOverlap.BehindHead;
        spikeLength = lGraphics.BodyAndTailLength * (liz.Template.type == HSEnums.CreatureType.FreezerLizard ? 0.3f : 0.5f);

        // Ice spike size range and colors
        scaleFac = 1;
        sizeRangeMin = Mathf.Lerp(0.1f, 0.5f, Mathf.Pow(Random.value, 2f));
        sizeRangeMax = Mathf.Lerp(sizeRangeMin, 1.15f, Random.value);
        if (Random.value > 0.75f)
        {
            sizeRangeMax = 1f;
        }
        if (liz.Template.type == HSEnums.CreatureType.FreezerLizard)
        {
            sizeRangeMin = 0.8f;
            sizeRangeMax = 0.8f;
            dualColored = 1;
        }
        else if (liz.Template.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            float sizeMult = Mathf.Lerp(0.5f, 0.66f, Mathf.InverseLerp(1.4f, 1.6f, liz.TotalMass));

            sizeRangeMin = sizeMult;
            sizeRangeMax = sizeMult;
            dualColored = Random.Range(0, 2);
        }
        else
        {
            dualColored = 0;
        }

        sizeSkewExponent = Mathf.InverseLerp(0.8f, 0.95f, Random.value);
        backSpikes = 3;
        scaleX = 1;

        if (liz is ColdLizard cl && cl.Freezer)
        {
            graphic = cl.crystalSprite;
        }
        numberOfSprites = (dualColored > 0) ? backSpikes * 2 : backSpikes;
        endSprite = this.startSprite + backSpikes - 1;
    }

    public override void Update()
    {
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int num = endSprite; num >= startSprite; num--)
        {
            if (dualColored > 0)
            {
                sLeaser.sprites[num] = new FSprite("ArmorIceSpike" + graphic + "B.0");
                sLeaser.sprites[num + backSpikes] = new FSprite("ArmorIceSpike" + graphic + "A.0");
            }
            else
            {
                sLeaser.sprites[num] = new FSprite("ArmorIceSpike" + graphic + "A.0");
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int num = endSprite; num >= startSprite; num--)
        {
            int spikeDepth = (int)Mathf.Lerp(3.3333f, 0.6666f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation)));
            float anchorY =
                spikeDepth == 3 ? 0.30f :
                spikeDepth != 0 ? 0.25f : 0.20f;

            float spriteProgress = Mathf.InverseLerp(startSprite, endSprite, num);
            float spikeSize = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(spriteProgress, sizeSkewExponent) * Mathf.PI));
            LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(Mathf.Lerp(0.1f, spikeLength / lGraphics.BodyAndTailLength, spriteProgress - 0.1f), timeStacker);

            sLeaser.sprites[num].x = lizardSpineData.outerPos.x - camPos.x;
            sLeaser.sprites[num].y = lizardSpineData.outerPos.y - camPos.y;
            sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(-lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.perp * lizardSpineData.depthRotation);
            sLeaser.sprites[num].scaleX = spikeSize * scaleX * Mathf.Sign(lGraphics.depthRotation) * scaleFac;
            sLeaser.sprites[num].element = Futile.atlasManager.GetElementWithName("ArmorIceSpike" + graphic + "B." + spikeDepth);
            sLeaser.sprites[num].anchorY = anchorY;
            if (dualColored <= 0)
            {
                sLeaser.sprites[num].color = sLeaser.sprites[lGraphics.SpriteHeadStart + 3].color;
            }
            else
            {
                sLeaser.sprites[num].color = sLeaser.sprites[lGraphics.SpriteHeadStart].color;
                sLeaser.sprites[num + backSpikes].color = sLeaser.sprites[lGraphics.SpriteHeadStart + 3].color;
                sLeaser.sprites[num + backSpikes].x = lizardSpineData.outerPos.x - camPos.x;
                sLeaser.sprites[num + backSpikes].y = lizardSpineData.outerPos.y - camPos.y;
                sLeaser.sprites[num + backSpikes].rotation = Custom.AimFromOneVectorToAnother(-lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.perp * lizardSpineData.depthRotation);
                sLeaser.sprites[num + backSpikes].scaleX = spikeSize * scaleX * Mathf.Sign(lGraphics.depthRotation) * scaleFac;
                sLeaser.sprites[num + backSpikes].element = Futile.atlasManager.GetElementWithName("ArmorIceSpike" + graphic + "A." + spikeDepth);
                sLeaser.sprites[num + backSpikes].anchorY = anchorY;
            }
        }

        if (liz is not null and
            ColdLizard cLiz)
        {
            for (int c = 0; c < cLiz.ColdState.crystals.Length; c++)
            {
                sLeaser.sprites[startSprite + c].isVisible = cLiz.ColdState.crystals[c];
                if (dualColored > 0)
                {
                    sLeaser.sprites[startSprite + c + backSpikes].isVisible = cLiz.ColdState.crystals[c];
                }

                if (cLiz.Freezer && !cLiz.ColdState.crystals.All(intact => !intact) && cLiz.ColdState.dead)
                {
                    cLiz.ColdState.armored = false;
                    if (cLiz.slatedForDeletetion)
                    {
                        break;
                    }
                    else if (cLiz.ColdState.crystals[c])
                    {
                        cLiz.ColdState.crystals[c] = false;
                        Vector2 crystalPos = sLeaser.sprites[startSprite + c].GetPosition() + camPos;
                        DropCrystals(cLiz, crystalPos, graphic);
                    }
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public static void DropCrystals(ColdLizard liz, Vector2 crystalPos, int crystalType)
    {
        for (int particle = 0; particle < 13; particle++)
        {
            if (particle % 2 == 0)
            {
                liz.room.AddObject(new HailstormSnowflake(crystalPos, Custom.RNV() * Random.value * 12f, liz.effectColor, liz.effectColor2));
            }
            else
            {
                liz.room.AddObject(new PuffBallSkin(crystalPos, Custom.RNV() * Random.value * 12f, liz.effectColor, liz.effectColor2));
            }
        }

        Vector2 lizHeadAngle = (liz.bodyChunks[1].pos - liz.bodyChunks[0].pos).normalized;
        float rotationSide = Random.Range(1.5f, 3f) * ((lizHeadAngle.x > 0) ? 1 : -1);

        AbstractIceChunk absIce = new(liz.room.world, liz.abstractCreature.pos, liz.room.game.GetNewID(), HSEnums.AbstractObjectType.FreezerCrystal)
        {
            freshness = 2f,
            sprite = crystalType,
            color1 = liz.effectColor,
            color2 = liz.effectColor2
        };
        liz.room.abstractRoom.AddEntity(absIce);
        absIce.RealizeInRoom();

        IceChunk ice = absIce.realizedObject as IceChunk;
        ice.firstChunk.HardSetPosition(crystalPos);
        ice.firstChunk.vel.y = Random.Range(3f, 4.5f);
        ice.firstChunk.vel.x *= rotationSide;
        ice.setRotation = Custom.RNV();
        ice.SetRandomSpin();

        liz.room.PlaySound(SoundID.Coral_Circuit_Break, crystalPos, 1.5f, 0.75f);
    }
}

public class IceSpikeTuft : LongBodyScales
{
    public IceSpikeTuft(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        //------------------------------------------------------
        bool icyBlue = lGraphics.lizard.Template.type == HSEnums.CreatureType.IcyBlueLizard;
        bool freezer = lGraphics.lizard.Template.type == HSEnums.CreatureType.FreezerLizard;
        rigor = 0f;

        if ((freezer && Random.value < 0.85f) || (icyBlue && Random.value < 0.3f) || Random.value < 0.15f)
        {
            TwoLines(0f, freezer ? 1f : 0.7f, 2.4f, Random.value);
        }
        else
        {
            GeneratePatchPattern(0f, Random.Range(3, 9), 1.6f, 1f);
        }

        MoveScalesTowardsTail();
        //------------------------------------------------------
        float num = Mathf.Lerp(1f, 1f / Mathf.Lerp(1f, scalesPositions.Length, Mathf.Pow(Random.value, 2f)), 0.5f);
        if (freezer)
        {
            num = Mathf.Max(num, 0.4f) * 1.1f;
        }
        float num2 = Mathf.Lerp(5f, 10f, Random.value) * num;
        float num3 = Mathf.Lerp(num2, 25f, Mathf.Pow(Random.value, 0.5f)) * num;

        colored = freezer || (icyBlue && Random.value < 0.2f + ((lGraphics.lizard.TotalMass - 1.4f) * 4));
        //------------------------------------------------------
        scaleObjects = new LizardScale[scalesPositions.Length];
        backwardsFactors = new float[scalesPositions.Length];
        float num4 = 0f;
        float num5 = 1f;
        float num6 = Mathf.Lerp(1f, 1.5f, Random.value);

        for (int j = 0; j < scalesPositions.Length; j++)
        {
            if (scalesPositions[j].y > num4)
            {
                num4 = scalesPositions[j].y;
            }
            if (scalesPositions[j].y < num5)
            {
                num5 = scalesPositions[j].y;
            }
        }
        for (int k = 0; k < scalesPositions.Length; k++)
        {
            scaleObjects[k] = new LizardScale(this);
            float num7 = Mathf.InverseLerp(num5, num4, scalesPositions[k].y);
            scaleObjects[k].length = Mathf.Lerp(num2, num3, num7);
            scaleObjects[k].width = Mathf.Lerp(0.8f, 1.2f, num7) * num;
            backwardsFactors[k] = 0.3f + (0.7f * Mathf.InverseLerp(0.75f, 1f, scalesPositions[k].y));
            scalesPositions[k].x *= Mathf.InverseLerp(1.05f, 0.85f, scalesPositions[k].y) * num6;
        }
        numberOfSprites =
            colored ? scalesPositions.Length * 2 : scalesPositions.Length;
        //------------------------------------------------------
    }

    private void TwoLines(float startPoint, float maxLength, float spacingScale, float randomValue)
    {
        float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Random.value);
        float num2 = num * lGraphics.BodyAndTailLength;
        float num3 = Mathf.Lerp(7f, 13f, Random.value);
        if (lGraphics.lizard.abstractCreature.creatureTemplate.type == HSEnums.CreatureType.FreezerLizard)
        {
            num3 = (randomValue < 0.25f) ? 6.25f : 7.5f;
        }
        if (lGraphics.lizard.abstractCreature.creatureTemplate.type == HSEnums.CreatureType.IcyBlueLizard)
        {
            num3 = (randomValue < 0.25f) ? 8.25f : 11.25f;
        }

        num3 *= spacingScale;
        int num4 = Mathf.Max(3, (int)(num2 / num3));

        scalesPositions = new Vector2[num4 * 2];
        for (int i = 0; i < num4; i++)
        {
            float num5 = Mathf.Lerp(0f, num, i / (float)(num4 - 1));
            float num6 = 0.6f + (0.4f * Mathf.Sin(i / (float)(num4 - 1) * Mathf.PI));
            scalesPositions[i * 2] = new Vector2(num6, num5);
            scalesPositions[(i * 2) + 1] = new Vector2(0f - num6, num5);
        }
    }

    private void MoveScalesTowardsTail()
    {
        float num = 0f;
        for (int i = 0; i < scalesPositions.Length; i++)
        {
            if (scalesPositions[i].y > num)
            {
                num = scalesPositions[i].y;
            }
        }
        for (int j = 0; j < scalesPositions.Length; j++)
        {
            scalesPositions[j].y += 0.9f - num;
        }
    }
}

public class IcyRhinestones : BodyScales
{
    private readonly CreatureTemplate.Type lizType;
    public string stoneType = "Diamond";
    public int patternType;
    public float shapeSkew;
    public int[] colorPicker;
    public readonly float RNG;

    public IcyRhinestones(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
    {
        RNG = Random.value;

        lizType = lGraphics.lizard.Template.type;

        if (Random.value < (lizType == HSEnums.CreatureType.FreezerLizard ? 0.6f : 0.4f))
        {
            stoneType = "pixel";
        }

        int num = Random.Range(0, 2);
        if (lizType == HSEnums.CreatureType.FreezerLizard)
        {
            num = Random.Range(0, 4);
        }
        switch (num)
        {
            case 0:
                GenerateRows(0.1f, 0.9f, Random.Range(0.75f, 1.25f));
                break;
            case >= 1:
                GenerateTwoLines(0.1f, 1f, 1.5f, 1.1f);
                break;
            default:
                break;
        }
        numberOfSprites = scalesPositions.Length;

        patternType = num;
        shapeSkew = Random.Range(0, 0.2f);
    }

    public override void Update()
    {
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        float stoneScale =
            stoneType == "pixel" ? 6 : 1;

        for (int i = startSprite + scalesPositions.Length - 1; i >= startSprite; i--)
        {
            sLeaser.sprites[i] = new FSprite(stoneType)
            {
                scaleX = (0.3f + shapeSkew) * stoneScale,
                scaleY = (0.5f - shapeSkew) * stoneScale
            };
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        for (int S = startSprite + scalesPositions.Length - 1; S >= startSprite; S--)
        {
            LizardGraphics.LizardSpineData backPos = GetBackPos(S - startSprite, timeStacker, changeDepthRotation: true);
            sLeaser.sprites[S].x = backPos.outerPos.x - camPos.x;
            sLeaser.sprites[S].y = backPos.outerPos.y - camPos.y;
            sLeaser.sprites[S].rotation = Custom.AimFromOneVectorToAnother(backPos.dir, -backPos.dir);
            switch (patternType)
            {
                case 0:
                    sLeaser.sprites[S].color = (RNG < 0.5f && colorPicker[S - startSprite] == 1) || 0.8333f < RNG
                        ? sLeaser.sprites[lGraphics.SpriteHeadStart].color
                        : sLeaser.sprites[lGraphics.SpriteHeadStart + 3].color;
                    break;

                case >= 1:
                    sLeaser.sprites[S].color = RNG < 0.75f && (S % 4 == 0 || S % 4 == 3)
                        ? sLeaser.sprites[lGraphics.SpriteHeadStart].color
                        : sLeaser.sprites[lGraphics.SpriteHeadStart + 3].color;

                    break;
                default:
                    break;
            }
        }
    }

    protected void GenerateRows(float startPoint, float maxLength, float lengthExponent)
    {
        float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Mathf.Pow(Random.value, lengthExponent));
        float num2 = num * lGraphics.BodyAndTailLength;
        float num3 = Mathf.Lerp(7f, 14f, Random.value);
        int num4 = Mathf.Max(3, (int)(num2 / num3));
        int num5 = Random.Range(1, 5) * 2;
        scalesPositions = new Vector2[num4 * num5];
        colorPicker = new int[num4 * num5];
        bool even = num5 % 2 == 0;
        for (int i = num4 - 1; i >= 0; i--)
        {
            float num6 = Mathf.Lerp(0f, num, i / (float)(num4 - 1f));
            for (int j = 0; j < num5; j++)
            {
                float num7 = 0.6f + (0.6f * Mathf.Sin(i / (float)(num4 - 1) * Mathf.PI));
                num7 *= Mathf.Lerp(-1f, 1f, (float)j / (num5 - 1));
                scalesPositions[(i * num5) + j] = new Vector2(num7, num6);
                colorPicker[(i * num5) + j] = ((RNG < 0.25f ? i : (even && j > num5 / 2) ? j + 1 : j) % 2 == 1) ? 1 : 0;
            }
        }
    }

    protected void GenerateBlanketPattern(float startPoint, int numOfScales, float maxLength, float lengthExponent)
    {
        scalesPositions = new Vector2[numOfScales];
        colorPicker = new int[numOfScales];
        float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Mathf.Pow(Random.value, lengthExponent));
        for (int i = 0; i < scalesPositions.Length; i++)
        {
            Vector2 val = Custom.DegToVec(Random.value * 360f) * Random.value;
            scalesPositions[i].y = Mathf.Lerp(startPoint * lGraphics.bodyLength / lGraphics.BodyAndTailLength, num * lGraphics.bodyLength / lGraphics.BodyAndTailLength, (val.y + 1f) / 2f);
            scalesPositions[i].x = val.x;
            colorPicker[i] = (Random.value < 0.5f) ? 1 : 0;
        }
    }

}