namespace Hailstorm;

internal class MiscWorldChanges
{
    public static void Hooks()
    {

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
        On.MoreSlugcats.CLOracleBehavior.ctor += Pebbles_IsFuckingDead;
        SleepAndDeathScreen.GetDataFromGame += Incan_SleepScreenBlizzardSounds;
        On.WinState.CreateAndAddTracker += ChieftainAndPilgrimPassageChanges;
        On.WinState.CycleCompleted += MakeColdLizardKillsCountForDragonslayerSorta;
        new Hook(typeof(CreatureTemplate).GetMethod("get_IsLizard", Public | NonPublic | Instance), (Func<CreatureTemplate, bool> orig, CreatureTemplate temp) => temp.type == HSEnums.CreatureType.IcyBlueLizard || temp.type == HSEnums.CreatureType.FreezerLizard || temp.type == HSEnums.CreatureType.GorditoGreenieLizard || orig(temp));
        Incan_PatientShelters();
    
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    private static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == IncanInfo.Incandescent);
        // ^ Returns true if all of the given conditions are met, or false otherwise.
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    #region Echo Changes

    public static bool AllEchoesMet;
    public static bool Incan_AllowAllEchoesToSpawn(On.World.orig_CheckForRegionGhost orig, SlugcatStats.Name saveFile, string regionString)
    {
        GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
        if (saveFile == IncanInfo.Incandescent && ghostID != GhostWorldPresence.GhostID.NoGhost && ghostID != MoreSlugcatsEnums.GhostID.LC)
        {
            return true;
        }
        return orig(saveFile, regionString);
    }
    public static void EchoSpawning(On.World.orig_LoadWorld orig, World world, SlugcatStats.Name playerChar, List<AbstractRoom> absRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
    {
        orig(world, playerChar, absRoomsList, swarmRooms, shelters, gates);
        if (world.game.setupValues.ghosts >= 0 && world is not null && IsIncanStory(world.game) && !world.singleRoomWorld && world.game.session is not null && world.game.session is StoryGameSession SGS)
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
        if (IsIncanStory(world?.game))
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
        if (IsIncanStory(GWP?.world?.game))
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
        if (IsIncanStory(RWG)) return true;
        else return orig(RWG);
    }
    public static bool Moon_IsActive(On.RainWorldGame.orig_IsMoonActive orig, RainWorldGame RWG)
    {
        if (IsIncanStory(RWG) && RWG.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0)
        {
            return true;
        }
        else return orig(RWG);
    }
    public static void Moon_StartsWith6Neurons(On.SLOrcacleState.orig_ForceResetState orig, SLOrcacleState moon, SlugcatStats.Name name)
    {
        orig(moon, name);
        if (name is not null && name.value == IncanInfo.Incandescent.value)
        {
            moon.neuronsLeft = 6;
        }
    }
    public static void Moon_BehaviorUpdate(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior moon, bool eu)
    {
        orig(moon, eu);
        if (IsIncanStory(moon.oracle.room.game) && !moon.hasNoticedPlayer)
        {
            moon.setMovementBehavior(SLOracleBehavior.MovementBehavior.Idle);
        }
    }
    public static void Moon_GravityWhenDead(On.SLOracleBehavior.orig_UnconciousUpdate orig, SLOracleBehavior Moon)
    { // Completely disables the gravity fluctuations in Moon's structure when she's dead, and makes sure that her puppet is disabled.
        orig(Moon);
        Oracle moon = Moon.oracle;
        if (IsIncanStory(moon.room.game))
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

        if (IsIncanStory(rCam.room.game) && !excludedPearl)
        {
            pearl.color = Color.Lerp(pearl.color, new Color(0.7f, 0.7f, 0.7f), 0.76f);
            pearl.highlightColor = new Color(1f, 1f, 1f);
        }
    }
    public static void MusicPearl_SavedForCycleWhenGivenToMoon(On.DataPearl.orig_Update orig, DataPearl pearl, bool eu)
    {
        orig(pearl, eu);
        if (IsIncanStory(pearl.room.game) &&
            pearl.AbstractPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.RM &&
            (!ModManager.MMF || !MMF.cfgKeyItemTracking.Value) &&
            pearl.room.game.session is not null &&
            pearl.room.game.session is StoryGameSession SGS &&
            Dialogue.firstCycleSeeingMusicPearl > 0)
        {
            SGS.AddNewPersistentTracker(pearl.AbstractPearl);               
        }
    }
    
    #endregion

    //----------------------------------------------------------------------------------

    #region Miscellaneous Hooks

    public static void Pebbles_IsFuckingDead(On.MoreSlugcats.CLOracleBehavior.orig_ctor orig, CLOracleBehavior rBehavior, Oracle rubbles)
    {
        orig(rBehavior, rubbles);
        if (IsIncanStory(rubbles.room.game))
        {
            if (rubbles.health != 0) rubbles.health = 0;
            if (!rubbles.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles)
            {
                rubbles.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles = true;
            }
        }
    }
    public static void Incan_SleepScreenBlizzardSounds(SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen postCycleScreen, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
    { // Plays the blizzard background noise when going to sleep in the Incandescent's campaign.
        orig(postCycleScreen, package);
        if (postCycleScreen.IsSleepScreen && package.characterStats.name.value == IncanInfo.Incandescent.value)
        {
            postCycleScreen.soundLoop?.Destroy();
            postCycleScreen.mySoundLoopID = MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
        }
    }
    public static WinState.EndgameTracker ChieftainAndPilgrimPassageChanges(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
    {
        if (RainWorld.lastActiveSaveSlot.value == IncanInfo.Incandescent.value)
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
        if (IsIncanStory(RWG))
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
                    if (playerSessionRecord.kills[j].symbolData.critType == HSEnums.CreatureType.IcyBlueLizard || playerSessionRecord.kills[j].symbolData.critType == HSEnums.CreatureType.FreezerLizard)
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
                c.EmitDelegate((bool NotSaint, Player self) => NotSaint && self.SlugCatClass != IncanInfo.Incandescent);
            }
            else
                Debug.LogError("[Hailstorm] Shelter IL hook is borked; they will not be patient. Report this if you see it, please!");
        };
    }        

    #endregion

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}