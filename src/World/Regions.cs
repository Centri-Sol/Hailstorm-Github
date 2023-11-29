using System;
using System.Collections.Generic;
using static System.Reflection.BindingFlags;
using UnityEngine;
using On.Menu;
using RWCustom;
using MoreSlugcats;
using Random = UnityEngine.Random;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using static Hailstorm.ObjectChanges;

namespace Hailstorm;

internal class Regions
{
    public static void Hooks()
    {
        // Region changes
        On.Region.ctor += HailstormRegionParams;
        On.Region.GetProperRegionAcronym += Incan_RegionSwaps;
        //On.SlugcatStats.getSlugcatStoryRegions += Incan_StoryRegions;
        On.MoreSlugcats.CollectiblesTracker.ctor += Incan_SleepScreenRegionUnlocks;
        On.Room.SlugcatGamemodeUniqueRoomSettings += Incan_RoomSettings;
        //On.Room.Loaded += IceCrystalSpawnChance;
        On.Region.RegionColor += SubmergedSuperstructureColor;
        new Hook(typeof(RainCycle).GetMethod("get_RegionHidesTimer", Public | NonPublic | Instance), (Func<RainCycle, bool> orig, RainCycle cycle) => IsIncanStory(cycle.world.game) || orig(cycle));
        new Hook(typeof(RainCycle).GetMethod("get_MusicAllowed", Public | NonPublic | Instance), (Func<RainCycle, bool> orig, RainCycle cycle) => IsIncanStory(cycle.world.game) || orig(cycle));
    
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == HSSlugs.Incandescent);
        // ^ Returns true if all of the given conditions are met, or false otherwise.
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static ConditionalWeakTable<Region, RegionInfo> RegionData = new();

    public static void HailstormRegionParams(On.Region.orig_ctor orig, Region region, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name campaign)
    {
        if (campaign is not null &&
            campaign == HSSlugs.Incandescent &&
            !RegionData.TryGetValue(region, out _))
        {
            RegionData.Add(region, new RegionInfo(name, campaign));
        }
        orig(region, name, firstRoomIndex, regionNumber, campaign);
    }
    public static string Incan_RegionSwaps(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name playerChar, string baseAcronym)
    {
        if (playerChar is not null && playerChar.value == HSSlugs.Incandescent.value)
        {
            if (baseAcronym == "DS")
            {
                baseAcronym = "UG"; // Replaces Drainage System with Undergrowth.
            }
            if (baseAcronym == "SH")
            {
                baseAcronym = "CL"; // Replaces Shaded Citadel with Silent Construct.
            }
        }
        return orig(playerChar, baseAcronym);
    }
    public static string[] Incan_StoryRegions(On.SlugcatStats.orig_getSlugcatStoryRegions orig, SlugcatStats.Name saveFile)
    {
        if (saveFile.value == HSSlugs.Incandescent.value)
        {
            return new string[12]
            {
                "MS", "SL", "CL", "GW", "HI", "UG", "SU", "CC", "VS", "SI", "LF", "SB"
            };
        }
        return orig(saveFile);
    }
    public static void Incan_SleepScreenRegionUnlocks(On.MoreSlugcats.CollectiblesTracker.orig_ctor orig, CollectiblesTracker cTracker, Menu.Menu menu, Menu.MenuObject owner, Vector2 pos, FContainer container, SlugcatStats.Name saveSlot)
    {
        orig(cTracker, menu, owner, pos, container, saveSlot);
        if (saveSlot == HSSlugs.Incandescent)
        {
            RainWorld rainWorld = menu.manager.rainWorld;
            cTracker.displayRegions = new List<string>(new string[12] { "MS", "SL", "CL", "GW", "HI", "UG", "SU", "CC", "VS", "SI", "LF", "SB" });
            cTracker.collectionData = cTracker.MineForSaveData(menu.manager, saveSlot);
            string[] slugcatOptionalRegions = SlugcatStats.getSlugcatOptionalRegions(saveSlot);
            if (cTracker.collectionData is null)
            {
                cTracker.collectionData = new ();
                cTracker.collectionData.currentRegion = "??";
                cTracker.collectionData.regionsVisited = new List<string>();
            }
            for (int i = 0; i < cTracker.displayRegions.Count; i++)
            {
                cTracker.displayRegions[i] = cTracker.displayRegions[i].ToLowerInvariant();
            }
            for (int j = 0; j < slugcatOptionalRegions.Length; j++)
            {
                slugcatOptionalRegions[j] = slugcatOptionalRegions[j].ToLowerInvariant();
                for (int k = 0; k < cTracker.collectionData.regionsVisited.Count; k++)
                {
                    if (cTracker.collectionData.regionsVisited[k] == slugcatOptionalRegions[j])
                    {
                        cTracker.displayRegions.Add(slugcatOptionalRegions[j]);
                        break;
                    }
                }
            }
            cTracker.sprites = new Dictionary<string, List<FSprite>>();
            cTracker.spriteColors = new Dictionary<string, List<Color>>();
            cTracker.regionIcons = new FSprite[cTracker.displayRegions.Count];
            for (int l = 0; l < cTracker.displayRegions.Count; l++)
            {
                if (cTracker.collectionData is not null && cTracker.displayRegions[l] == cTracker.collectionData.currentRegion)
                {
                    cTracker.regionIcons[l] = new FSprite("keyShiftB");
                    cTracker.regionIcons[l].rotation = 180f;
                    cTracker.regionIcons[l].scale = 0.5f;
                }
                else
                {
                    cTracker.regionIcons[l] = new FSprite("Circle4");
                }
                cTracker.regionIcons[l].color = Color.Lerp(Region.RegionColor(cTracker.displayRegions[l]), Color.white, 0.25f);
                container.AddChild(cTracker.regionIcons[l]);
                cTracker.sprites[cTracker.displayRegions[l]] = new List<FSprite>();
                cTracker.spriteColors[cTracker.displayRegions[l]] = new List<Color>();
                if (cTracker.collectionData is null || !cTracker.collectionData.regionsVisited.Contains(cTracker.displayRegions[l]))
                {
                    cTracker.regionIcons[l].isVisible = false;
                    cTracker.spriteColors[cTracker.displayRegions[l]].Add(CollectToken.WhiteColor.rgb);
                    cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctNone"));
                }
                else
                {
                    for (int m = 0; m < rainWorld.regionGoldTokens[cTracker.displayRegions[l]].Count; m++)
                    {
                        if (rainWorld.regionGoldTokensAccessibility[cTracker.displayRegions[l]][m].Contains(saveSlot))
                        {
                            cTracker.spriteColors[cTracker.displayRegions[l]].Add(new Color(1f, 0.6f, 0.05f));
                            if (!cTracker.collectionData.unlockedGolds.Contains(rainWorld.regionGoldTokens[cTracker.displayRegions[l]][m]))
                            {
                                cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOff"));
                            }
                            else
                            {
                                cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOn"));
                            }
                        }
                    }
                    for (int n = 0; n < rainWorld.regionBlueTokens[cTracker.displayRegions[l]].Count; n++)
                    {
                        if (rainWorld.regionBlueTokensAccessibility[cTracker.displayRegions[l]][n].Contains(saveSlot))
                        {
                            cTracker.spriteColors[cTracker.displayRegions[l]].Add(RainWorld.AntiGold.rgb);
                            if (!cTracker.collectionData.unlockedBlues.Contains(rainWorld.regionBlueTokens[cTracker.displayRegions[l]][n]))
                            {
                                cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOff"));
                            }
                            else
                            {
                                cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOn"));
                            }
                        }
                    }
                    if (ModManager.MSC)
                    {
                        for (int num = 0; num < rainWorld.regionGreenTokens[cTracker.displayRegions[l]].Count; num++)
                        {
                            if (rainWorld.regionGreenTokensAccessibility[cTracker.displayRegions[l]][num].Contains(saveSlot))
                            {
                                cTracker.spriteColors[cTracker.displayRegions[l]].Add(CollectToken.GreenColor.rgb);
                                if (!cTracker.collectionData.unlockedGreens.Contains(rainWorld.regionGreenTokens[cTracker.displayRegions[l]][num]))
                                {
                                    cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOff"));
                                }
                                else
                                {
                                    cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOn"));
                                }
                            }
                        }
                        for (int num3 = 0; num3 < rainWorld.regionRedTokens[cTracker.displayRegions[l]].Count; num3++)
                        {
                            if (rainWorld.regionRedTokensAccessibility[cTracker.displayRegions[l]][num3].Contains(saveSlot))
                            {
                                cTracker.spriteColors[cTracker.displayRegions[l]].Add(CollectToken.RedColor.rgb);
                                if (!cTracker.collectionData.unlockedReds.Contains(rainWorld.regionRedTokens[cTracker.displayRegions[l]][num3]))
                                {
                                    cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOff"));
                                }
                                else
                                {
                                    cTracker.sprites[cTracker.displayRegions[l]].Add(new FSprite("ctOn"));
                                }
                            }
                        }
                    }
                }
                for (int num4 = 0; num4 < cTracker.sprites[cTracker.displayRegions[l]].Count; num4++)
                {
                    cTracker.sprites[cTracker.displayRegions[l]][num4].color = cTracker.spriteColors[cTracker.displayRegions[l]][num4];
                    container.AddChild(cTracker.sprites[cTracker.displayRegions[l]][num4]);
                }
            }
        }
    }
    public static void Incan_RoomSettings(On.Room.orig_SlugcatGamemodeUniqueRoomSettings orig, Room room, RainWorldGame game)
    {
        orig(room, game);
        if (IsIncanStory(game))
        {
            room.roomSettings.wetTerrain = false;
            room.roomSettings.CeilingDrips = 0f;
            room.roomSettings.WaveAmplitude = 0.0001f;
            room.roomSettings.WaveLength = 1f;
            room.roomSettings.WaveSpeed = 0.51f;
            room.roomSettings.SecondWaveAmplitude = 0.0001f;
            room.roomSettings.SecondWaveLength = 1f;
            room.roomSettings.RandomItemDensity = 0.375f;
            room.roomSettings.RandomItemSpearChance = 0.25f;
            room.roomSettings.Clouds =
                room.world.region.name == "SI" ? 0.75f : 1f;

        }
    }
    public static void IceCrystalSpawnChance(On.Room.orig_Loaded orig, Room room)
    {
        bool firstLoad = room?.abstractRoom is not null && room.abstractRoom.firstTimeRealized;
        orig(room);
        if (room?.abstractRoom is null || room.world is null || !firstLoad)
        {
            return;
        }

        if (IsIncanStory(room.game))
        {

        }
    }
    public static Color SubmergedSuperstructureColor(On.Region.orig_RegionColor orig, string regionName)
    {
        if (RainWorld.lastActiveSaveSlot.value == HSSlugs.Incandescent.value && Region.EquivalentRegion(regionName, "MS"))
        {
            return new Color(0.8f, 0.8f, 1f);
        }
        return orig(regionName);
    }


    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}

public class RegionInfo
{
    public float freezingFogInsteadOfHailChance;
    public float erraticWindChance;
    public float erraticWindDandelionChance;
    public float erraticWindWrongDandelionTypeChance;
    public int lateBlizzardStartTimeAfterCycleEnds = 1000000;

    public RegionInfo(string regionName, SlugcatStats.Name campaign)
    {
        string[] propertiesText = new string[1] { "" };
        string scugName = "-" + campaign.value;
        string[] propertiesDirectory = new string[7] { "World", null, null, null, null, null, null };
        string directorySeparatorChar = Path.DirectorySeparatorChar.ToString();
        propertiesDirectory[1] = directorySeparatorChar;
        propertiesDirectory[2] = regionName;
        propertiesDirectory[3] = directorySeparatorChar;
        propertiesDirectory[4] = "properties";
        propertiesDirectory[5] = scugName;
        propertiesDirectory[6] = ".txt";
        string path = AssetManager.ResolveFilePath(string.Concat(propertiesDirectory));
        if (!File.Exists(path))
        {
            string[] propertiesDirecTry2 = new string[5] { "World", null, null, null, null };
            propertiesDirecTry2[1] = directorySeparatorChar;
            propertiesDirecTry2[2] = regionName;
            propertiesDirecTry2[3] = directorySeparatorChar;
            propertiesDirecTry2[4] = "properties.txt";
            path = AssetManager.ResolveFilePath(string.Concat(propertiesDirecTry2));
        }
        if (File.Exists(path))
        {
            propertiesText = File.ReadAllLines(path);
        }
        for (int i = 0; i < propertiesText.Length; i++)
        {
            string[] property = Regex.Split(Custom.ValidateSpacedDelimiter(propertiesText[i], ":"), ": ");
            if (property.Length < 2)
            {
                continue;
            }
            switch (property[0])
            {
                case "freezingFogInsteadOfHailChance":
                    freezingFogInsteadOfHailChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindChance":
                    erraticWindChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindDandelionChance":
                    erraticWindDandelionChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindWrongDandelionTypeChance":
                    erraticWindWrongDandelionTypeChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "lateBlizzardStartTimeAfterCycleEnds":
                    lateBlizzardStartTimeAfterCycleEnds = int.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
            }
        }
    }
}