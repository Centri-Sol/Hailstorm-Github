﻿using System;
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

namespace Hailstorm
{
    internal class StoryChanges
    {
        public static void Hooks()
        {
            // Blizzard changes
            On.RainCycle.ctor += HailstormPreCycles;
            On.MoreSlugcats.BlizzardGraphics.CycleUpdate += Incan_RegionBlizzardProgression;
            BlizzardWindChanges();
            On.MoreSlugcats.BlizzardSound.Update += Incan_ReduceBlizzardVolume;
            new Hook(typeof(RainCycle).GetMethod("get_RainApproaching", Public | NonPublic | Instance), (Func<RainCycle, float> orig, RainCycle cycle) => IsRWGIncan(cycle.world.game) && FoggyCycle ? 1f - (cycle.preTimer / (float)cycle.maxPreTimer) : orig(cycle));


            // Region changes
            On.Region.GetProperRegionAcronym += Incan_RegionSwaps;
            //On.SlugcatStats.getSlugcatStoryRegions += Incan_StoryRegions;
            On.MoreSlugcats.CollectiblesTracker.ctor += Incan_SleepScreenRegionUnlocks;
            On.Room.SlugcatGamemodeUniqueRoomSettings += Incan_RoomSettings;
            //On.Room.Loaded += IceCrystalSpawnChance;
            On.Region.RegionColor += SubmergedSuperstructureColor;
            new Hook(typeof(RainCycle).GetMethod("get_RegionHidesTimer", Public | NonPublic | Instance), (Func<RainCycle, bool> orig, RainCycle cycle) => IsRWGIncan(cycle.world.game) || orig(cycle));
            new Hook(typeof(RainCycle).GetMethod("get_MusicAllowed", Public | NonPublic | Instance), (Func<RainCycle, bool> orig, RainCycle cycle) => IsRWGIncan(cycle.world.game) || orig(cycle));

            // Echo changes
            On.World.CheckForRegionGhost += Incan_AllowAllEchoesToSpawn;
            On.World.LoadWorld += EchoSpawning;
            On.GhostWorldPresence.ctor += EchoSpawnLocationChanges;
            On.GhostWorldPresence.GhostMode_AbstractRoom_Vector2 += EchoAuraLocationChanges;

            // Moon
            On.RainWorldGame.IsMoonHeartActive += Moon_HasCell;
            On.RainWorldGame.IsMoonActive += Moon_IsActive;
            On.SLOrcacleState.ForceResetState += Moon_StartsWith6Neurons;
            On.SLOracleBehavior.Update += Moon_BehaviorUpdate;
            On.SLOracleBehavior.UnconciousUpdate += Moon_GravityWhenDead;

            // Pearls
            On.DataPearl.ApplyPalette += Pearl_ColorFade;
            On.DataPearl.Update += MusicPearl_SavedForCycleWhenGivenToMoon;

            // Miscellaneous hooks
            On.ShelterDoor.ctor += NoPrecyclesOnFirstThreeCycles;
            On.MoreSlugcats.CLOracleBehavior.ctor += Pebbles_IsFuckingDead;
            On.RainCycle.GetDesiredCycleLength += Incan_ShorterCycles;
            SleepAndDeathScreen.GetDataFromGame += Incan_SleepScreenBlizzardSounds;
            On.WinState.CreateAndAddTracker += ChieftainAndPilgrimPassageChanges;
            On.WinState.CycleCompleted += MakeColdLizardKillsCountForDragonslayerSorta;
            new Hook(typeof(CreatureTemplate).GetMethod("get_IsLizard", Public | NonPublic | Instance), (Func<CreatureTemplate, bool> orig, CreatureTemplate temp) => temp.type == HailstormEnums.IcyBlue || temp.type == HailstormEnums.Freezer || orig(temp));
            Incan_PatientShelters();
        
        }

        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------

        private static bool IsRWGIncan(RainWorldGame RWG)
        {
            return (RWG is not null && RWG.IsStorySession && RWG.StoryCharacter == HailstormSlugcats.Incandescent);
            // ^ Returns true if all of the given conditions are met, or false otherwise.
        }

        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------

        #region Blizzard Changes

        public static bool FoggyCycle;
        public static void HailstormPreCycles(On.RainCycle.orig_ctor orig, RainCycle rainCycle, World world, float minutes)
        {
            if (FoggyCycle) FoggyCycle = false;

            orig(rainCycle, world, minutes);

            if (IsRWGIncan(world?.game) && rainCycle.maxPreTimer > 0)
            {
                StoryGameSession SGS = world.game.session as StoryGameSession;
                Random.State state = Random.state;
                Random.InitState((SGS.saveState.totTime + SGS.saveState.cycleNumber * SGS.saveStateNumber.Index) * 10000);
                FoggyCycle = Random.value < 0.6f;
                Debug.Log("Hailstorm PreCycle? " + FoggyCycle.ToString());
                Random.state = state;
            }
        }
        public static void Incan_RegionBlizzardProgression(On.MoreSlugcats.BlizzardGraphics.orig_CycleUpdate orig, BlizzardGraphics BG)
        {
            orig(BG);
            if (BG?.room?.world?.region is not null && IsRWGIncan(BG.room.game) && BG.room.world.region.regionParams.glacialWasteland)
            {
                RainCycle rainCycle = BG.room.world.rainCycle;
                string region = BG.room.world.region.name;
                float rainIntensity = BG.room.roomSettings.RainIntensity;

                float CycleProgression = rainCycle.CycleProgression;
                int TimeUntilRain = rainCycle.TimeUntilRain;
                if (BG.room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
                {
                    CycleProgression = Mathf.InverseLerp(0f, 0.75f, rainIntensity);
                    TimeUntilRain = (int)(3000f * Mathf.InverseLerp(1f, 0.5f, rainIntensity));
                }
                float windAngleExtremeness =
                    region == "SI" || region == "SU" ? 3 :
                    region == "HI" || region == "LF" || region == "CC" || region == "CC" ? 2f : 1f;
                float num3 = Mathf.Sin(TimeUntilRain / 900f) * windAngleExtremeness;
                num3 = Mathf.Lerp(num3, Mathf.Sin(TimeUntilRain / 240f), 0.3f + Mathf.Sin(TimeUntilRain / 100f) / 4f);
                float num4 = Mathf.Lerp(BG.WindAngle, num3, 0.1f) * rainIntensity;

                BG.WindAngle =
                    Mathf.Lerp(num4, Mathf.Sign(num3), 0.2f * (0f - Mathf.Abs(num4))) * Mathf.Lerp(0f, 0.75f, CycleProgression * 3f);

                float maxWindStrength =
                    region == "SI" || region == "SU" ? 1.4f :
                    region == "HI" || region == "LF" || region == "CC" ? 1.2f : 1f;                

                if (region == "SU")
                {
                    BG.WindStrength = Mathf.Lerp(0.7f, maxWindStrength, Mathf.InverseLerp(rainCycle.cycleLength, 0, TimeUntilRain)) * rainIntensity;
                    BG.BlizzardIntensity = Mathf.Lerp(0.7f, maxWindStrength, Mathf.InverseLerp(rainCycle.cycleLength, 0, TimeUntilRain)) * rainIntensity;
                    BG.SnowfallIntensity = Mathf.Lerp(0.7f, maxWindStrength, Mathf.Clamp(BG.WindStrength * 6f, 1.5f, 5f)) * rainIntensity;
                    BG.WhiteOut = BG.BlizzardIntensity * rainIntensity;
                }
                else if (region == "HI" || region == "LF" || region == "CC")
                {
                    BG.WindStrength = Mathf.Lerp(maxWindStrength * 0.25f, maxWindStrength, Mathf.InverseLerp(rainCycle.cycleLength * 1.25f, 3000f, TimeUntilRain)) * rainIntensity;
                    BG.BlizzardIntensity = Mathf.Lerp(0.3f, maxWindStrength, Mathf.InverseLerp(rainCycle.cycleLength * 1.25f, 3000f, TimeUntilRain)) * rainIntensity;
                    BG.SnowfallIntensity = Mathf.Lerp(0.3f, maxWindStrength, Mathf.Clamp(BG.WindStrength * 5f, 1f, 4.5f)) * rainIntensity;
                    BG.WhiteOut = Mathf.Lerp(0.3f, maxWindStrength, Mathf.Pow(BG.BlizzardIntensity, 1.4f)) * rainIntensity;
                }
                else
                {
                    BG.WindStrength = Mathf.InverseLerp(rainCycle.cycleLength, 2000f, TimeUntilRain) * rainIntensity;
                    BG.BlizzardIntensity = Mathf.InverseLerp(rainCycle.cycleLength, 2000f, TimeUntilRain) * rainIntensity;
                    BG.SnowfallIntensity = Mathf.Clamp(BG.WindStrength * 5f, 0f, 4f) * rainIntensity;
                    BG.WhiteOut = Mathf.Pow(BG.BlizzardIntensity, 1.3f) * rainIntensity;
                }

                BG.lerpBypass = true;

            }
        }
        public static void BlizzardWindChanges()
        {
            IL.PhysicalObject.WeatherInertia += IL =>
            {
                ILCursor c = new(IL);
                ILLabel? label = IL.DefineLabel();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((PhysicalObject obj) =>
                {
                    return NewBlizzardWindPush(obj);
                });
                c.Emit(OpCodes.Brfalse_S, label);
                c.Emit(OpCodes.Ret);
                c.MarkLabel(label);
            };
        }
        public static bool NewBlizzardWindPush(PhysicalObject obj)
        {
            if (obj?.room?.blizzardGraphics is null || !IsRWGIncan(obj.room.game) || !(obj is Player || Random.value < 0.1f) || !obj.room.blizzard || obj.room.blizzardGraphics.WindStrength <= 0.5f)
            {
                return true;
            }
            if (obj is Creature windImmune && (
                windImmune.Template.type == CreatureTemplate.Type.WhiteLizard ||
                windImmune.Template.type == HailstormEnums.Freezer ||
                windImmune.Template.type == HailstormEnums.GorditoGreenie))
            {
                return true;
            }

            Color blizzardPixel = obj.room.blizzardGraphics.GetBlizzardPixel((int)(obj.firstChunk.pos.x / 20f), (int)(obj.firstChunk.pos.y / 20f));
            Vector2 val;
            foreach (BodyChunk chunk in obj.bodyChunks)
            {
                val = new Vector2(0f - obj.room.blizzardGraphics.WindAngle, 0.1f);
                val *= blizzardPixel.g * (5f * obj.room.blizzardGraphics.WindStrength);

                Vector2 blizzPush =
                    Vector2.Lerp(val, val * 0.08f, obj.Submersion) * Mathf.InverseLerp(40f, 1f, obj.TotalMass);

                if (obj is Player plr)
                {
                    blizzPush *=
                        plr.isGourmand ? 0.075f : 0.1f;
                }
                if (obj is Creature ctr)
                {
                    CreatureTemplate.Type type = ctr.Template.type;

                    if (type == CreatureTemplate.Type.CicadaA ||
                        type == CreatureTemplate.Type.CicadaB ||
                        type == CreatureTemplate.Type.KingVulture)
                    {
                        blizzPush *= 0.75f;
                    }
                    else
                    if (type == CreatureTemplate.Type.Vulture ||
                        type == CreatureTemplate.Type.MirosBird ||
                        type == CreatureTemplate.Type.Centiwing ||
                        type == HailstormEnums.Cyanwing ||
                        type == HailstormEnums.IcyBlue)
                    {
                        blizzPush *= 0.50f;
                    }
                    else
                    if (type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
                        type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
                    {
                        blizzPush *= 0.25f;
                    }
                }
                chunk.vel += blizzPush;
            }

            return true;
        }
        public static void Incan_ReduceBlizzardVolume(On.MoreSlugcats.BlizzardSound.orig_Update orig, BlizzardSound blizzSound, bool eu)
        {
            orig(blizzSound, eu);
            if (IsRWGIncan(blizzSound?.room?.game) && blizzSound.blizzWind is not null)
            {
                blizzSound.blizzWind.Volume *= 0.33f;
            }
        }


        #endregion

        //----------------------------------------------------------------------------------

        #region Region Changes

        public static string Incan_RegionSwaps(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name playerChar, string baseAcronym)
        {
            if (playerChar is not null && playerChar.value == HailstormSlugcats.Incandescent.value)
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
            if (saveFile.value == HailstormSlugcats.Incandescent.value)
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
            if (saveSlot == HailstormSlugcats.Incandescent)
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
            if (IsRWGIncan(game))
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

            if (IsRWGIncan(room.game))
            {

            }
        }
        public static Color SubmergedSuperstructureColor(On.Region.orig_RegionColor orig, string regionName)
        {
            if (RainWorld.lastActiveSaveSlot.value == HailstormSlugcats.Incandescent.value && Region.EquivalentRegion(regionName, "MS"))
            {
                return new Color(0.8f, 0.8f, 1f);
            }
            return orig(regionName);
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Echo Changes

        public static bool AllEchoesMet;
        public static bool Incan_AllowAllEchoesToSpawn(On.World.orig_CheckForRegionGhost orig, SlugcatStats.Name saveFile, string regionString)
        {
            GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
            if (saveFile == HailstormSlugcats.Incandescent && ghostID != GhostWorldPresence.GhostID.NoGhost && ghostID != MoreSlugcatsEnums.GhostID.LC)
            {
                return true;
            }
            return orig(saveFile, regionString);
        }
        public static void EchoSpawning(On.World.orig_LoadWorld orig, World world, SlugcatStats.Name playerChar, List<AbstractRoom> absRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
        {
            orig(world, playerChar, absRoomsList, swarmRooms, shelters, gates);
            if (world.game.setupValues.ghosts >= 0 && world is not null && IsRWGIncan(world.game) && !world.singleRoomWorld && world.game.session is not null && world.game.session is StoryGameSession SGS)
            {
                GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(world.region.name);
                if (world.worldGhost is null && ghostID != GhostWorldPresence.GhostID.NoGhost)
                {
                    int ghostPreviouslyEncountered = 0;
                    if (SGS.saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID))
                    {
                        ghostPreviouslyEncountered = SGS.saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID];
                    }
                    Debug.Log("Save state properly loaded: " + SGS.saveState.loaded);
                    Debug.Log("Found the " + ghostID.ToString() + " Echo. Previously met?: " + ghostPreviouslyEncountered);
                    Debug.Log("Karma: " + SGS.saveState.deathPersistentSaveData.karma);

                    bool canSeeEcho;
                    if (ghostID == MoreSlugcatsEnums.GhostID.MS)
                    {
                        canSeeEcho = SGS.saveState.cycleNumber > 0;
                    }
                    else
                    if (ghostID != MoreSlugcatsEnums.GhostID.SL &&
                        ghostID != MoreSlugcatsEnums.GhostID.CL &&
                        ghostID != MoreSlugcatsEnums.GhostID.UG &&
                        ghostID != GhostWorldPresence.GhostID.CC &&
                        ghostID != GhostWorldPresence.GhostID.SI &&
                        ghostID != GhostWorldPresence.GhostID.LF &&
                        ghostID != GhostWorldPresence.GhostID.SB)
                    {
                        canSeeEcho = false;
                    }
                    else
                    {
                        canSeeEcho =
                            world.game.setupValues.ghosts > 0 ||
                            SGS.saveState.deathPersistentSaveData.karma == SGS.saveState.deathPersistentSaveData.karmaCap;
                    }
                    if (SGS.saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) &&
                        SGS.saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 2)
                    {
                        canSeeEcho = false;
                    }
                    if (canSeeEcho)
                    {
                        world.worldGhost = new GhostWorldPresence(world, ghostID);
                        world.migrationInfluence = world.worldGhost;
                        Debug.Log(ghostID.value + " ghost in region");
                    }
                    else
                    {
                        Debug.Log("No ghost in region");
                    }
                }

                int ghostsMet = 0;
                foreach (KeyValuePair<GhostWorldPresence.GhostID, int> ghost in SGS.saveState.deathPersistentSaveData.ghostsTalkedTo)
                {
                    if (ghost.Value > 1 && (
                        ghostID == MoreSlugcatsEnums.GhostID.MS ||
                        ghostID == MoreSlugcatsEnums.GhostID.SL ||
                        ghostID == MoreSlugcatsEnums.GhostID.CL ||
                        ghostID == MoreSlugcatsEnums.GhostID.UG ||
                        ghostID == GhostWorldPresence.GhostID.CC ||
                        ghostID == GhostWorldPresence.GhostID.SI ||
                        ghostID == GhostWorldPresence.GhostID.LF ||
                        ghostID == GhostWorldPresence.GhostID.SB))
                    {
                        ghostsMet++;
                        if (RainWorld.ShowLogs)
                            Debug.Log(ghost.Key.value + " echo has been met.");
                    }
                }
                Debug.Log("Echoes met: " + ghostsMet + "/8");
                if (ghostsMet >= 8)
                {
                    AllEchoesMet = true;
                }
                // ghostsTalkedTo values:
                // 2 = has been seen      
                // 1 = not seen yet, and karma is high enough to meet
                // 0 = can be seen, but karma is not high enough
            }
        }
        public static void EchoSpawnLocationChanges(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence GWP, World world, GhostWorldPresence.GhostID ghostID)
        {
            orig(GWP, world, ghostID);
            if (IsRWGIncan(world?.game))
            {
                string text = "";
                if (ghostID == MoreSlugcatsEnums.GhostID.SL)
                {
                    text = "SL_B04";
                }
                else if (ghostID == MoreSlugcatsEnums.GhostID.MS && world.game.GetStorySession?.saveState?.denPosition is not null)
                {
                    if (world.game.GetStorySession.saveState.denPosition == "MS_bittershelter")
                    {
                        text = "MS_bitteraerie2";
                    }
                    else if (world.game.GetStorySession.saveState.denPosition == "MS_S07")
                    {
                        text = "MS_WILLSNAGGING01"; // Great room name, guys
                    }
                    else if (world.game.GetStorySession.saveState.denPosition == "MS_S10")
                    {
                        text = "MS_bittersafe";
                    }                    
                }
                if (text != "")
                {
                    GWP.ghostRoom = world.GetAbstractRoom(text);
                    if (GWP.ghostRoom is null && ghostID == GhostWorldPresence.GhostID.LF)
                    {
                        GWP.ghostRoom = world.GetAbstractRoom("LF_B01");
                    }
                }
            }                       
        }
        public static float EchoAuraLocationChanges(On.GhostWorldPresence.orig_GhostMode_AbstractRoom_Vector2 orig, GhostWorldPresence GWP, AbstractRoom testRoom, Vector2 worldPos)
        {
            if (IsRWGIncan(GWP?.world?.game))
            {
                if (testRoom is null || GWP.ghostRoom is null)
                {
                    return 0f;
                }
                if (testRoom.index == GWP.ghostRoom.index)
                {
                    return 1f;
                }
                if (GWP.ghostID == MoreSlugcatsEnums.GhostID.SL && GWP.ghostRoom.name == "SL_B04")
                {
                    if (testRoom.name == "SL_C03")
                    {
                        float y = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).y;
                        return Mathf.Lerp(0.8f, 0.9f, Mathf.InverseLerp(y, y + (testRoom.size.y * 0.8f), worldPos.y));
                    }
                    if (testRoom.name == "SL_E01")
                    {
                        float y = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).y;
                        return Mathf.Lerp(0.6f, 0.9f, Mathf.InverseLerp(y, y + (testRoom.size.y * 0.8f), worldPos.y));
                    }
                    if (testRoom.name == "SL_A11")
                    {
                        return 0.7f;
                    }
                    if (testRoom.name == "SL_A13" || testRoom.name == "SL_A05" || testRoom.name == "SL_A07")
                    {
                        return 0.6f;
                    }
                    if (testRoom.name == "SL_A12" || testRoom.name == "SL_A04" || testRoom.name == "SL_ECNIUS02")
                    {
                        return 0.5f;
                    }
                    if (testRoom.name == "SL_D03")
                    {
                        Vector2 v = GWP.world.RoomToWorldPos(testRoom.size.ToVector2(), testRoom.index);
                        return Mathf.Lerp(0.3f, 0.55f, (Mathf.InverseLerp(v.x * 0.25f, v.x * 0.75f, worldPos.x) + Mathf.InverseLerp(v.y * 0.25f, v.y * 0.75f, worldPos.y))/2f);
                    }
                    if (testRoom.name == "SL_C01" || testRoom.name == "SL_S03")
                    {
                        return 0.4f;
                    }
                    if (testRoom.name == "SL_ECNIUS03")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0.25f, 0.45f, Mathf.InverseLerp(x + (testRoom.size.x * 0.2f), x + (testRoom.size.x * 0.8f), worldPos.x));
                    }
                    if (testRoom.name == "SL_A02" || testRoom.name == "SL_A17" || testRoom.name == "SL_D04")
                    {
                        return 0.3f;
                    }
                    if (testRoom.name == "SL_E02")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0.15f, 0.5f, Mathf.InverseLerp(x + (testRoom.size.x * 0.75f), x + (testRoom.size.x * 0.20f), worldPos.x));
                    }
                    if (testRoom.name == "SL_D02")
                    {
                        float y = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).y;
                        return Mathf.Lerp(0.15f, 0.25f, Mathf.InverseLerp(y, y + (testRoom.size.y * 0.25f), worldPos.y));
                    }
                    if (testRoom.name == "SL_F02")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0.15f, 0.25f, Mathf.InverseLerp(x + (testRoom.size.x * 0.33f), x + (testRoom.size.x * 0.66f), worldPos.x));
                    }
                    if (testRoom.name == "SL_B01")
                    {
                        return 0.25f;
                    }
                    if (testRoom.name == "SL_A17" || testRoom.name == "SL_ECNIUS01")
                    {
                        return 0.2f;
                    }
                    if (testRoom.name == "SL_D06")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0f, 0.2f, Mathf.InverseLerp(x + (testRoom.size.x * 0.25f), x + (testRoom.size.x * 0.75f), worldPos.x));
                    }
                    if (testRoom.name == "SL_BO2SAINT")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0f, 0.25f, Mathf.InverseLerp(x + (testRoom.size.x), x + (testRoom.size.x * 0.25f), worldPos.x));
                    }
                    if (testRoom.name == "SL_H02" || testRoom.name == "SL_WALL06")
                    {
                        return 0.15f;
                    }
                    if (testRoom.name == "SL_A14" || testRoom.name == "SL_S08" || testRoom.name == "SL_D01" || testRoom.name == "SL_TUNNELA" || testRoom.name == "SL_A10" || testRoom.name == "SL_S05" || testRoom.name == "SL_C07")
                    {
                        return 0.1f;
                    }
                    if (testRoom.name == "SL_H03")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0f, 0.1f, Mathf.InverseLerp(x + (testRoom.size.x * 0.4f), x + (testRoom.size.x * 0.2f), worldPos.x));
                    }
                    if (testRoom.name == "SL_C02")
                    {
                        float x = GWP.world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
                        return Mathf.Lerp(0f, 0.1f, Mathf.InverseLerp(x + (testRoom.size.x * 0.33f), x + (testRoom.size.x * 0.66f), worldPos.x));
                    }
                    return 0;
                }
                if (GWP.ghostID == MoreSlugcatsEnums.GhostID.MS)
                {
                    if (GWP.ghostRoom.name == "MS_bitteraerie2")
                    {
                        if (testRoom.name == "MS_bittershelter")
                        {
                            return 0.5f; // For some reason, anything higher than this seems to break the shelter's door entirely, preventing it from ever opening and causing you to get trapped.
                        }
                        if (testRoom.name == "MS_bitterunderground" || testRoom.name == "MS_bitteraerie3")
                        {
                            return 0.85f;
                        }
                        if (testRoom.name == "MS_bitterentrance" || testRoom.name == "MS_bitteraeriedown" || testRoom.name == "MS_bitteraerie5" || testRoom.name == "MS_bitteraerie4")
                        {
                            return 0.7f;
                        }
                        if (testRoom.name == "MS_bitteraerie1" || testRoom.name == "MS_bittermironest" || testRoom.name == "MS_bitteredge" || testRoom.name == "MS_COMMS" || testRoom.name == "MS_pumps")
                        {
                            return 0.55f;
                        }
                        if (testRoom.name == "MS_bitterstart" || testRoom.name == "MS_Jtrap" || testRoom.name == "MS_bitteraeriepipeeu")
                        {
                            return 0.4f;
                        }
                        if (testRoom.name == "MS_bittersafe" || testRoom.name == "MS_splitsewers" || testRoom.name == "MS_scavtrader")
                        {
                            return 0.35f;
                        }
                        if (testRoom.name == "MS_X02")
                        {
                            return 0.3f;
                        }
                        if (testRoom.name == "MS_S10" || testRoom.name == "MS_bittervents")
                        {
                            return 0.25f;
                        }
                        if (testRoom.name == "MS_bitteraccess" || testRoom.name == "MS_sewerbridge" || testRoom.name == "MS_bitterpipe" || testRoom.name == "MS_startsewers")
                        {
                            return 0.125f;
                        }
                        return 0;
                    }
                    if (GWP.ghostRoom.name == "MS_WILLSNAGGING01")
                    {
                        if (testRoom.name == "MS_S07")
                        {
                            return 0.5f;
                        }
                        if (testRoom.name == "GATE_SL_MS")
                        {
                            return 0.9f;
                        }
                        if (testRoom.name == "MS_aeriestart")
                        {
                            return 0.8f;
                        }
                        if (testRoom.name == "MS_startsewers")
                        {
                            return 0.65f;
                        }
                        if (testRoom.name == "MS_bittervents" || testRoom.name == "MS_bitteraccess")
                        {
                            return 0.55f;
                        }
                        if (testRoom.name == "MS_splitsewers")
                        {
                            return 0.4f;
                        }
                        if (testRoom.name == "MS_sewerbridge")
                        {
                            return 0.3f;
                        }
                        if (testRoom.name == "MS_bitterpipe" || testRoom.name == "MS_pumps" || testRoom.name == "MS_scavtrader")
                        {
                            return 0.2f;
                        }
                        if (testRoom.name == "MS_bitteraerie1")
                        {
                            return 0.15f;
                        }
                        if (testRoom.name == "MS_bittermironest")
                        {
                            return 0.1f;
                        }
                        if (testRoom.name == "MS_bitteraerie4" || testRoom.name == "MS_Jtrap")
                        {
                            return 0.05f;
                        }
                        return 0;
                    }
                    if (GWP.ghostRoom.name == "MS_bittersafe")
                    {
                        if (testRoom.name == "MS_S10" || testRoom.name == "MS_X02")
                        {
                            return 0.5f;
                        }
                        if (testRoom.name == "MS_bitteredge")
                        {
                            return 0.8f;
                        }
                        if (testRoom.name == "MS_bitterstart" || testRoom.name == "MS_bitterstart" || testRoom.name == "MS_bitterunderground")
                        {
                            return 0.7f;
                        }
                        if (testRoom.name == "MS_bitterentrance" || testRoom.name == "MS_bitteraeriedown" || testRoom.name == "MS_Jtrap" || testRoom.name == "MS_bitteraerie2")
                        {
                            return 0.6f;
                        }
                        if (testRoom.name == "MS_pumps" || testRoom.name == "MS_bitteraeriepipeu")
                        {
                            return 0.5f;
                        }
                        if (testRoom.name == "MS_scavtrader" || testRoom.name == "MS_bittershelter" || testRoom.name == "MS_bitteraerie3" || testRoom.name == "MS_bitteraerie1" || testRoom.name == "MS_bitterpipe")
                        {
                            return 0.4f;
                        }
                        if (testRoom.name == "MS_splitsewers" || testRoom.name == "MS_bitteraerie4")
                        {
                            return 0.3f;
                        }
                        if (testRoom.name == "MS_COMMS" || testRoom.name == "MS_bitteraerie5" || testRoom.name == "MS_sewerbridge" || testRoom.name == "MS_bittermironest")
                        {
                            return 0.2f;
                        }
                        if (testRoom.name == "MS_startsewers" || testRoom.name == "MS_bittervents" || testRoom.name == "MS_bitteraccess")
                        {
                            return 0.1f;
                        }
                        return 0;
                    }
                }
            }            
            return orig(GWP, testRoom, worldPos);
        }
        
        #endregion

        //----------------------------------------------------------------------------------

        #region Moon

        public static bool Moon_HasCell(On.RainWorldGame.orig_IsMoonHeartActive orig, RainWorldGame RWG)
        {
            if (IsRWGIncan(RWG)) return true;
            else return orig(RWG);
        }
        public static bool Moon_IsActive(On.RainWorldGame.orig_IsMoonActive orig, RainWorldGame RWG)
        {
            if (IsRWGIncan(RWG) && RWG.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0)
            {
                return true;
            }
            else return orig(RWG);
        }
        public static void Moon_StartsWith6Neurons(On.SLOrcacleState.orig_ForceResetState orig, SLOrcacleState moon, SlugcatStats.Name name)
        {
            orig(moon, name);
            if (name is not null && name.value == HailstormSlugcats.Incandescent.value)
            {
                moon.neuronsLeft = 6;
            }
        }
        public static void Moon_BehaviorUpdate(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior moon, bool eu)
        {
            orig(moon, eu);
            if (IsRWGIncan(moon.oracle.room.game) && !moon.hasNoticedPlayer)
            {
                moon.setMovementBehavior(SLOracleBehavior.MovementBehavior.Idle);
            }
        }
        public static void Moon_GravityWhenDead(On.SLOracleBehavior.orig_UnconciousUpdate orig, SLOracleBehavior Moon)
        { // Completely disables the gravity fluctuations in Moon's structure when she's dead, and makes sure that her puppet is disabled.
            orig(Moon);
            Oracle moon = Moon.oracle;
            if (IsRWGIncan(moon.room.game))
            {
                moon.SetLocalGravity(1f);
                moon.room.world.rainCycle.brokenAntiGrav.on = false;
                moon.room.world.rainCycle.brokenAntiGrav.counter = int.MaxValue;
                moon.arm.isActive = false;
                Moon.moonActive = false;
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Future Pearls

        public static void Pearl_ColorFade(On.DataPearl.orig_ApplyPalette orig, DataPearl pearl, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        { // Gives pearls in the Incandescent's campaign the faded colors that they have in the Saint's story.
            orig(pearl, sLeaser, rCam, palette);
            bool excludedPearl =
                pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.CL ||
                pearl.AbstractPearl.dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.LF_west;

            if (IsRWGIncan(rCam.room.game) && !excludedPearl)
            {
                pearl.color = Color.Lerp(pearl.color, new Color(0.7f, 0.7f, 0.7f), 0.76f);
                pearl.highlightColor = new Color(1f, 1f, 1f);
            }
        }
        public static void MusicPearl_SavedForCycleWhenGivenToMoon(On.DataPearl.orig_Update orig, DataPearl pearl, bool eu)
        {
            orig(pearl, eu);
            if (IsRWGIncan(pearl.room.game) && pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.RM && (!ModManager.MMF || !MMF.cfgKeyItemTracking.Value) && pearl.room.game.session is StoryGameSession SGS && Dialogue.firstCycleSeeingMusicPearl > 0)
            {
                SGS.AddNewPersistentTracker(pearl.AbstractPearl);               
            }
        }
        
        #endregion

        //----------------------------------------------------------------------------------

        #region Miscellaneous Hooks

        public static void NoPrecyclesOnFirstThreeCycles(On.ShelterDoor.orig_ctor orig, ShelterDoor door, Room room)
        {
            if (IsRWGIncan(room?.game) && door?.rainCycle is not null && door.rainCycle.preTimer > 0 && room.world.GetAbstractRoom(room.world.game.startingRoom) == room.abstractRoom)
            {
                door.rainCycle.preTimer = 0;
                door.rainCycle.maxPreTimer = 0;
            }
            orig(door, room);
        }
        public static void Pebbles_IsFuckingDead(On.MoreSlugcats.CLOracleBehavior.orig_ctor orig, CLOracleBehavior rBehavior, Oracle rubbles)
        {
            orig(rBehavior, rubbles);
            if (IsRWGIncan(rubbles.room.game))
            {
                if (rubbles.health != 0) rubbles.health = 0;
                if (!rubbles.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles)
                {
                    rubbles.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles = true;
                }
            }
        }
        public static int Incan_ShorterCycles(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle cycle)
        {
            if (IsRWGIncan(cycle.world.game))
            {
                cycle.baseCycleLength = (int)(cycle.baseCycleLength * 0.4f);
                cycle.sunDownStartTime = (int)Random.Range(cycle.baseCycleLength * 0.9f, cycle.baseCycleLength * 1.1f);
            }
            return orig(cycle);           
        }
        public static void Incan_SleepScreenBlizzardSounds(SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen postCycleScreen, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        { // Plays the blizzard background noise when going to sleep in the Incandescent's campaign.
            orig(postCycleScreen, package);
            if (postCycleScreen.IsSleepScreen && package.characterStats.name.value == HailstormSlugcats.Incandescent.value)
            {
                if (postCycleScreen.soundLoop is not null)
                {
                    postCycleScreen.soundLoop.Destroy();
                }
                postCycleScreen.mySoundLoopID = MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
            }
        }
        public static WinState.EndgameTracker ChieftainAndPilgrimPassageChanges(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
        {
            if (RainWorld.lastActiveSaveSlot.value == HailstormSlugcats.Incandescent.value)
            {
                WinState.EndgameTracker tracker;
                if (ID == WinState.EndgameID.Chieftain)
                {
                    tracker = new WinState.FloatTracker(ID, 0f, -0.33f, -0.33f, 1f);
                    return tracker;
                }
                if (ID == MoreSlugcatsEnums.EndgameID.Pilgrim)
                {
                    int echoCount = 0;
                    string[] slugcatStoryRegions = SlugcatStats.getSlugcatStoryRegions(RainWorld.lastActiveSaveSlot);
                    for (int i = 0; i < slugcatStoryRegions.Length; i++)
                    {
                        if (World.CheckForRegionGhost(RainWorld.lastActiveSaveSlot, slugcatStoryRegions[i]))
                        {
                            echoCount++;
                        }
                    }
                    tracker = new WinState.BoolArrayTracker(ID, echoCount);

                    if (tracker is not null && endgameTrackers is not null)
                    {
                        bool flag = false;
                        for (int j = 0; j < endgameTrackers.Count; j++)
                        {
                            if (endgameTrackers[j].ID == ID)
                            {
                                flag = true;
                                endgameTrackers[j] = tracker;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            endgameTrackers.Add(tracker);
                        }
                    }
                    return tracker;
                }
            }
            return orig(ID, endgameTrackers);
        }
        public static void MakeColdLizardKillsCountForDragonslayerSorta(On.WinState.orig_CycleCompleted orig, WinState wState, RainWorldGame RWG)
        {
            if (IsRWGIncan(RWG))
            {
                List<int> lizList = new ();
                for (int i = 0; i < RWG.GetStorySession.playerSessionRecords.Length; i++)
                {
                    if (RWG.GetStorySession.playerSessionRecords[i] is null)
                    {
                        continue;
                    }
                    PlayerSessionRecord playerSessionRecord = RWG.GetStorySession.playerSessionRecords[i];                    
                    for (int j = 0; j < playerSessionRecord.kills.Count; j++)
                    {
                        if (!playerSessionRecord.kills[j].lizard)
                        {
                            continue;
                        }
                        if (playerSessionRecord.kills[j].symbolData.critType == HailstormEnums.IcyBlue || playerSessionRecord.kills[j].symbolData.critType == HailstormEnums.Freezer)
                        {
                            if (!lizList.Contains(2))
                            {
                                lizList.Add(2);
                            }
                            break;
                        }
                    }
                }
                if (lizList.Count > 0)
                {
                    WinState.ListTracker listTracker = wState.GetTracker(WinState.EndgameID.DragonSlayer, addIfMissing: true) as WinState.ListTracker;
                    foreach (int item2 in lizList)
                    {
                        listTracker.AddItemToList(item2);
                    }
                }
            }
            orig(wState, RWG);
        }

        public static void Incan_PatientShelters()
        {
            IL.Player.Update += IL =>
            {
                ILCursor c = new(IL);
                if (c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass)),
                    x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)),
                    x => x.MatchCall(out _)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate((bool flag, Player self) => flag && self.SlugCatClass != HailstormSlugcats.Incandescent);
                }
                else
                    Plugin.logger.LogError("Shelter IL hook is borked; they will not be patient. Report this if you see it, please!");
            };
        }        
        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}