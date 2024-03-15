namespace Hailstorm;

public class JollyCoopFixes
{
    public static void Hooks()
    {
        On.PlayerGraphics.JollyBodyColorMenu += DefaultAndAutoIncanBodyColors;
        On.PlayerGraphics.JollyFaceColorMenu += DefaultAndAutoIncanFaceColors;
        On.PlayerGraphics.JollyUniqueColorMenu += DefaultAndAutoIncanFireColors;
        On.PlayerGraphics.PopulateJollyColorArray += WaistbandJollyColorArray;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += IncanJollyCoopPupButton;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.ctor += IncanJollyWaistbandSprite;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += IncanJollyFireSprite;
        On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.LoadIcon += LoadIcon;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += WaistbandColorUpdate;
        On.JollyCoop.JollyMenu.JollyPlayerSelector.GrafUpdate += WaistbandDarken;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static ConditionalWeakTable<SymbolButtonTogglePupButton, CWT.PupButtonInfo> PupButtonData = new();

    //--------------------------------------------

    public static Color DefaultAndAutoIncanBodyColors(On.PlayerGraphics.orig_JollyBodyColorMenu orig, SlugcatStats.Name slugcat, SlugcatStats.Name reference)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
        {
            if (PlayerGraphics.jollyColors is not null)
            {
                Color? val = PlayerGraphics.jollyColors[0][0];
                if (val.HasValue && (!val.HasValue || val.GetValueOrDefault() == PlayerGraphics.DefaultSlugcatColor(slugcat)))
                {
                    goto IL_0064;
                }
            }
            PlayerGraphics.PopulateJollyColorArray(slugcat);
        }
        goto IL_0064;
    IL_0064:
        if (slugcat == JollyEnums.Name.JollyPlayer1 && Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
        {
            return PlayerGraphics.SlugcatColor(slugcat);
        }
        return orig(slugcat, reference);
    }
    public static Color DefaultAndAutoIncanFaceColors(On.PlayerGraphics.orig_JollyFaceColorMenu orig, SlugcatStats.Name slugcat, SlugcatStats.Name reference, int playerNumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
        {
            if (PlayerGraphics.jollyColors is not null)
            {
                Color? val = PlayerGraphics.jollyColors[0][0];
                if (val.HasValue && (!val.HasValue || val.GetValueOrDefault() == PlayerGraphics.DefaultSlugcatColor(reference)))
                {
                    goto IL_0064;
                }
            }
            PlayerGraphics.PopulateJollyColorArray(reference);
        }
        goto IL_0064;
    IL_0064:
        if (slugcat == HSEnums.Incandescent && (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT || (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)))
        {
            return Custom.hexToColor("4F0900");
        }
        return orig(slugcat, reference, playerNumber);
    }
    public static Color DefaultAndAutoIncanFireColors(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugcat, SlugcatStats.Name reference, int playerNumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
        {
            if (PlayerGraphics.jollyColors is not null)
            {
                Color? val = PlayerGraphics.jollyColors[0][0];
                if (val.HasValue && (!val.HasValue || val.GetValueOrDefault() == PlayerGraphics.DefaultSlugcatColor(reference)))
                {
                    goto IL_0064;
                }
            }
            PlayerGraphics.PopulateJollyColorArray(reference);
        }
        goto IL_0064;
    IL_0064:
        if (slugcat == HSEnums.Incandescent && (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.DEFAULT || (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)))
        {
            return Custom.hexToColor("FB8602");
        }
        return orig(slugcat, reference, playerNumber);
    }
    public static void WaistbandJollyColorArray(On.PlayerGraphics.orig_PopulateJollyColorArray orig, SlugcatStats.Name slugcat)
    {
        orig(slugcat);
        if (slugcat != HSEnums.Incandescent)
        {
            return;
        }

        for (int i = 0; i < PlayerGraphics.jollyColors.Length; i++)
        {
            if (PlayerGraphics.jollyColors[i].Length < 4)
            {
                Array.Resize(ref PlayerGraphics.jollyColors[i], 4);
            }

            if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)
            {
                JollyCustom.Log("Generating AUTO colors for Incandescent (Player " + (i + 1) + ")");
                if (i == 0)
                {
                    PlayerGraphics.jollyColors[0][3] = Custom.hexToColor("281714");
                }
                else if (i == 1)
                {
                    PlayerGraphics.jollyColors[1][0] = Custom.hexToColor("EDF3F2");
                    PlayerGraphics.jollyColors[1][1] = Custom.hexToColor("0C663D");
                    PlayerGraphics.jollyColors[1][2] = Custom.hexToColor("08CA98");
                    PlayerGraphics.jollyColors[1][3] = Custom.hexToColor("28A0CC");
                }
                else if (i == 2)
                {
                    PlayerGraphics.jollyColors[2][0] = Custom.hexToColor("FFDE93");
                    PlayerGraphics.jollyColors[2][1] = Custom.hexToColor("AF424B");
                    PlayerGraphics.jollyColors[2][2] = Custom.hexToColor("FF7FB4");
                    PlayerGraphics.jollyColors[2][3] = Custom.hexToColor("E0824E");

                }
                else if (i == 3)
                {
                    PlayerGraphics.jollyColors[3][0] = Custom.hexToColor("6864BA");
                    PlayerGraphics.jollyColors[3][1] = Custom.hexToColor("000638");
                    PlayerGraphics.jollyColors[3][2] = Custom.hexToColor("BDDBFF");
                    PlayerGraphics.jollyColors[3][3] = Custom.hexToColor("D8EFFF");
                }
                else
                {
                    HSLColor fireColor = JollyCustom.RGB2HSL((Color)PlayerGraphics.jollyColors[i][2]);
                    fireColor.hue -= 0.15f;
                    fireColor.saturation -= 0.2f;
                    fireColor.lightness =
                        i % 2 != 1 ?
                        Mathf.Lerp(fireColor.lightness, 0, 0.66f) :
                        Mathf.Lerp(fireColor.lightness, 1, 0.33f);

                    PlayerGraphics.jollyColors[i][3] = fireColor.rgb;

                    JollyCustom.Log("Generating auto fourth color for player " + i);
                }
            }
        }
    }

    public static string IncanJollyCoopPupButton(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyPlayerSelector JPS)
    {
        SlugcatStats.Name slugName = JPS.JollyOptions(JPS.index).playerClass;
        return slugName is not null && slugName.value.Equals("Incandescent") ? "incandescent_pup_off" : orig(JPS);
    }
    public static void WaistbandColorUpdate(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyPlayerSelector JPS)
    {
        bool dirty = JPS.dirty;
        orig(JPS);
        if (dirty && JPS.pupButton is not null && PupButtonData.TryGetValue(JPS.pupButton, out CWT.PupButtonInfo pbI))
        {
            pbI.uniqueTintColor2 = JollyUnique2ColorMenu(JollyCustom.SlugClassMenu(JPS.index, JPS.dialog.currentSlugcatPageName), JPS.JollyOptions(0).playerClass, JPS.index);
            if (pbI.uniqueSymbol2 is not null)
            {
                pbI.uniqueSymbol2.sprite.color = pbI.uniqueTintColor2;
            }
            JPS.pupButton.LoadIcon();
        }
    }
    public static void WaistbandDarken(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GrafUpdate orig, JollyPlayerSelector JPS, float timeStacker)
    {
        orig(JPS, timeStacker);
        if (JPS.pupButton is not null && PupButtonData.TryGetValue(JPS.pupButton, out CWT.PupButtonInfo pbI) && pbI.uniqueSymbol2 is not null)
        {
            pbI.uniqueTintColor2 = JPS.FadePortraitSprite(pbI.uniqueTintColor2, timeStacker);
            pbI.uniqueSymbol2.sprite.color = pbI.uniqueTintColor2;
        }
    }

    public static void IncanJollyWaistbandSprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_ctor orig, SymbolButtonTogglePupButton pupButton, Menu.Menu menu, Menu.MenuObject owner, string signal, Vector2 pos, Vector2 size, string symbolNameOn, string symbolNameOff, bool isOn, string stringLabelOn = null, string stringLabelOff = null)
    {
        orig(pupButton, menu, owner, signal, pos, size, symbolNameOn, symbolNameOff, isOn, stringLabelOn, stringLabelOff);
        PupButtonData.Add(pupButton, new CWT.PupButtonInfo(pupButton));
        if (pupButton.symbolNameOff == "incandescent_pup_off")
        {
            if (PupButtonData.TryGetValue(pupButton, out CWT.PupButtonInfo pbI) && pupButton.owner is JollyPlayerSelector)
            {
                pbI.uniqueSymbol2 = new Menu.MenuIllustration(pupButton.menu, pupButton, "", "unique2_incandescent_pup_off", pupButton.size / 2f, crispPixels: true, anchorCenter: true);
                pupButton.subObjects.Add(pbI.uniqueSymbol2);
            }
            if (pupButton.symbolNameOn != "incandescent_pup_on")
            {
                pupButton.symbolNameOn = "incandescent_pup_on";
            }
            pupButton.LoadIcon();
        }
    }
    public static bool IncanJollyFireSprite(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_HasUniqueSprite orig, SymbolButtonTogglePupButton pupButton)
    {
        return pupButton.symbolNameOff.Contains("incandescent") || orig(pupButton);
    }
    public static void LoadIcon(On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.orig_LoadIcon orig, SymbolButtonTogglePupButton pupButton)
    {
        orig(pupButton);
        if (pupButton.owner is not JollyPlayerSelector JPS) return;

        if (JPS.slugName == HSEnums.Incandescent && pupButton.symbolNameOn != "incandescent_pup_on")
        {
            pupButton.symbolNameOn = "incandescent_pup_on";
            pupButton.faceSymbol.fileName = "face_" + pupButton.symbolNameOn;
        }
        else
        if (pupButton.symbolNameOn != "pup_on" && (
            JPS.slugName == SlugcatStats.Name.White ||
            JPS.slugName == SlugcatStats.Name.Yellow ||
            JPS.slugName == SlugcatStats.Name.Red ||
            JPS.slugName == SlugcatStats.Name.Night ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Spear ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Artificer ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Gourmand ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Saint ||
            JPS.slugName == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
        {
            pupButton.symbolNameOn = "pup_on";
            pupButton.faceSymbol.fileName = "face_" + pupButton.symbolNameOn;
        }

        if (pupButton.faceSymbol is not null && pupButton.symbol.fileName == "incandescent_pup_on")
        {
            pupButton.faceSymbol.fileName = "face_" + pupButton.symbol.fileName;
            pupButton.faceSymbol.LoadFile();
            pupButton.faceSymbol.sprite.SetElementByName(pupButton.faceSymbol.fileName);
        }
        if (pupButton.uniqueSymbol is not null && pupButton.faceSymbol.fileName == "face_incandescent_pup_on")
        {
            pupButton.uniqueSymbol.fileName = "unique_" + pupButton.symbol.fileName;
            pupButton.uniqueSymbol.LoadFile();
            pupButton.uniqueSymbol.sprite.SetElementByName(pupButton.uniqueSymbol.fileName);
            pupButton.uniqueSymbol.pos.y = pupButton.size.y / 2f;
        }
        if (PupButtonData.TryGetValue(pupButton, out CWT.PupButtonInfo pbI) && pbI.uniqueSymbol2 is not null && pupButton.uniqueSymbol is not null && pupButton.uniqueSymbol.fileName.Contains("unique_incandescent_pup"))
        {
            pbI.uniqueSymbol2.fileName = "unique2_" + pupButton.symbol.fileName;
            pbI.uniqueSymbol2.LoadFile();
            pbI.uniqueSymbol2.sprite.SetElementByName(pbI.uniqueSymbol2.fileName);
        }
    }


    private static Color JollyUnique2ColorMenu(SlugcatStats.Name slugcat, SlugcatStats.Name reference, int playerNumber)
    {
        if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
        {
            if (PlayerGraphics.jollyColors is not null)
            {
                Color? val = PlayerGraphics.jollyColors[0][0];
                Color val2 = PlayerGraphics.DefaultSlugcatColor(reference);
                if (val.HasValue && (!val.HasValue || val.GetValueOrDefault() == val2))
                {
                    goto yourMother;
                }
            }
            PlayerGraphics.PopulateJollyColorArray(reference);
        }
        goto yourMother;
    yourMother:
        Color result = Color.white;
        if (PlayerGraphics.jollyColors is not null && PlayerGraphics.jollyColors[playerNumber].Length > 3 && (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM || (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0)))
        {
            return PlayerGraphics.JollyColor(playerNumber, 3);
        }
        if (slugcat == HSEnums.Incandescent)
        {
            result = Custom.hexToColor("281714");
        }
        return result;
    }
}