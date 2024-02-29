namespace Hailstorm;

internal class Dialogue
{
    public static void Hooks()
    {
        // Echo hooks
        On.GhostConversation.AddEvents += NewEchoConversations;

        // Moon hooks
        On.SLOracleBehaviorHasMark.ctor += Moon_WontUseHerUsualEndofcycleDialogue;
        On.SLOracleBehaviorHasMark.Update += Moon_NewEndofCycleDialogue;
        On.SLOracleBehaviorHasMark.GrabObject += HasMoonTalkedAboutItemAlready;
        On.SLOracleBehaviorHasMark.WillingToInspectItem += MoonItemInspection;
        On.SLOracleBehaviorHasMark.PlayerInterruptByTakingItem += StealingMusicPearlBack;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += NewMoonConversationTypes;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += NewMoonDialogue;
        On.SLOracleBehaviorHasMark.Pain += Moon_AttackedInIncanStory;

        // Incandescent hooks
        On.Conversation.DialogueEvent.Update += IncanConversationReactions;
    }


    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool IsIncanStory(RainWorldGame RWG)
    {
        return (RWG?.session is not null && RWG.IsStorySession && RWG.StoryCharacter == IncanInfo.Incandescent);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void NewEchoConversations(On.GhostConversation.orig_AddEvents orig, GhostConversation gConv)
    {            
        orig(gConv);
        if (gConv.currentSaveFile == IncanInfo.Incandescent && (gConv.id == MoreSlugcatsEnums.ConversationID.Ghost_MS || gConv.id == Conversation.ID.Ghost_CC || gConv.id == Conversation.ID.Ghost_SI || gConv.id == Conversation.ID.Ghost_LF || gConv.id == Conversation.ID.Ghost_SB))
        {
            RemoveEvents(gConv);

            if (gConv.id == MoreSlugcatsEnums.ConversationID.Ghost_MS) // Submerged Superstructure [Eight Spots on a Blind Eye]
            {
                AddDialogue(gConv, 0, "In all my time spent bound to these peaks, never have I beared witness to a beast such as you.", 20);
                AddDialogue(gConv, 0, "A brilliant flame that pierces the darkness, paired with such great sorrow...<LINE>Little one, a great tragedy has befallen you, hasn't it?", 30);
                AddDialogue(gConv, 0, "After watching you hide away for so many moons in this desolate place, I wish to<LINE>impart a warning: what you are doing will only bring you more pain.", 40);
                AddDialogue(gConv, 0, "Allowing fear and pain to control you will leave you trapped up here,<LINE>where there is nothing to accompany you but vicious birds and a bitter cold.", 40);
                AddDialogue(gConv, 0, "Do not resign yourself to such a fate, young one. Go on, and allow yourself to<LINE>search for greener pastures. I will root for you from this forgotten peak.", 40);
            }
            else if (gConv.id == Conversation.ID.Ghost_CC) // Chimney Canopy [Nineteen Spades, Endless Reflections]
            {
                AddDialogue(gConv, 0, "From my perch, I've overlooked this land for an eternity.", 0);
                AddDialogue(gConv, 0, "Watched as this vast expanse was blanketed into an endless tundra.", 0);
                AddDialogue(gConv, 0, "We remain trapped in place, and yet can never stop moving.", 0);
                AddDialogue(gConv, 0, "Funneled endlessly into an unknown future...", 0);
                AddDialogue(gConv, 0, "To what destination do these memories reach?", 0);
            }
            else if (gConv.id == Conversation.ID.Ghost_SI) // Sky Islands [Droplets upon Five Large Droplets]
            {
                AddDialogue(gConv, 0, "Another presence attempts to commune with mine.", 0);
                AddDialogue(gConv, 0, "Have I perceived your voice before? I have existed long enough to overhear them all.", 10);
                AddDialogue(gConv, 0, "Those who have been, and those who have yet to be.", 0);
                AddDialogue(gConv, 0, "Each serving as a wave propagating throughout the annals of history.", 0);
                AddDialogue(gConv, 0, "Listened as I have for many eons, 'tis true, some of those swells cannot help but mirror back.", 10);
            }
            else if (gConv.id == Conversation.ID.Ghost_LF) // Farm Arrays [A Bell, Eighteen Amber Beads]
            {
                AddDialogue(gConv, 0, "Why is it?", 0);
                AddDialogue(gConv, 0, "This long forgotten place beckons me, drawn by a certain presence...", 0);
                AddDialogue(gConv, 0, "The fields here are not as I remember. Too much has changed.", 0);
                AddDialogue(gConv, 0, "But, by forfeiting familiarities, perhaps something new is gained?", 0);
                AddDialogue(gConv, 0, "Despite all, this site still has not revealed the entirety of its secrets.", 0);
                AddDialogue(gConv, 0, "Perhaps that is the reason for my continued imprisonment.", 0);
            }
            else if (gConv.id == Conversation.ID.Ghost_SB) // Subterranean [Two Sprouts, Twelve Brackets]
            {
                AddDialogue(gConv, 0, "A little beast!?", 0);
                AddDialogue(gConv, 0, "Come to join me in this great undoing.", 0);
                AddDialogue(gConv, 0, "The old world will soon vanish, wiped from history, to pave a path for the dawning of a new era.", 10);
                AddDialogue(gConv, 0, "How many have been consumed so far? Were we the tenth civilization, or the thousandth?", 10);
                AddDialogue(gConv, 0, "Amusingly, they thought their small struggles bore such great significance.", 0);
                AddDialogue(gConv, 0, "All was naught but to serve the void.", 0);
            }
        }
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // Dialogue changes related to Moon in the Incandescent's campaign.
    static int noticeBlizzardDelay;
    static bool blizzardNoticed;
    static int postCycleTime;

    private static readonly SLOracleBehaviorHasMark.MiscItemType IceChunk = new("IceChunk", register: true);
    private static readonly SLOracleBehaviorHasMark.MiscItemType IceChunk_BrokenMidtalk = new("IceChunk_BrokenMidtalk", register: true);
    //private static readonly SLOracleBehaviorHasMark.MiscItemType BezanNut = new("BezanNut", register: true);
    //private static readonly SLOracleBehaviorHasMark.MiscItemType LargeBezanNut = new("LargeBezanNut", register: true);
    private static bool incPresent;
    private static bool alreadyTalkedAbout;
    private static bool hasTalkedAboutFadedPearls;
    public static int firstCycleSeeingMusicPearl;
    private static int musicPearlStolenBackCount;

    public static void Moon_WontUseHerUsualEndofcycleDialogue(On.SLOracleBehaviorHasMark.orig_ctor orig, SLOracleBehaviorHasMark moon, Oracle oracle)
    {
        orig(moon, oracle);
        if (IsIncanStory(oracle.room.game))
        {
            moon.rainInterrupt = true;
        }
    }

    public static void Moon_NewEndofCycleDialogue(On.SLOracleBehaviorHasMark.orig_Update orig, SLOracleBehaviorHasMark moon, bool eu)
    {
        orig(moon, eu);
        if (!IsIncanStory(moon.oracle.room.game))
        {
            return;
        }

        int timeUntilRain = moon.oracle.room.world.rainCycle.TimeUntilRain;

        if (moon.player != null && moon.hasNoticedPlayer)
        {
            // Allows Moon to talk at ANY point during a cycle, including after it ends.
            if (moon.sayHelloDelay < 0)
            {
                moon.sayHelloDelay = 40;
            }
            if (!blizzardNoticed)
            {
                if (timeUntilRain <= 400 && moon.player.room == moon.oracle.room && moon.oracle.room.world.rainCycle.pause < 1 && moon.currentConversation == null)
                {
                    if (noticeBlizzardDelay == 0)
                    {
                        noticeBlizzardDelay = Random.Range(200, 800);
                    }
                    else if (noticeBlizzardDelay == 1)
                    {
                        NoticeBlizzard(moon);
                        blizzardNoticed = true;
                    }

                    if (noticeBlizzardDelay > 0)
                    {
                        noticeBlizzardDelay--;
                    }
                }
                else if (moon.currentConversation != null && noticeBlizzardDelay > 0)
                {
                    noticeBlizzardDelay++; // Delays Moon noticing the blizzard if she's talking.
                }
            }
        }
        // Prevents NoticeBlizzard dialogue from appearing 
        if (timeUntilRain == 1)
        {
            postCycleTime = Random.Range(500, 1200);
        }
        if (!blizzardNoticed && timeUntilRain <= 0)
        {
            if (moon.currentConversation == null)
            {
                postCycleTime--;
            }
            if (postCycleTime == 0)
            {
                blizzardNoticed = true;
            }
        }

        if (moon.currentConversation is not null &&
            moon.currentConversation is SLOracleBehaviorHasMark.MoonConversation mConv &&
            mConv.describeItem == IceChunk &&
            moon.holdingObject is null)
        {
            mConv.describeItem = SLOracleBehaviorHasMark.MiscItemType.NA;
            RemoveEvents(mConv);
            CrystalBrokeMidtalk(mConv, 0, 40);
        }

        if (firstCycleSeeingMusicPearl > 0)
        {

            if (moon.holdingObject is not null && moon.holdingObject is HalcyonPearl && firstCycleSeeingMusicPearl >= 2)
            {
                if (moon.currentConversation != null)
                {
                    moon.currentConversation.Destroy();
                    moon.currentConversation = null;
                }
                if (moon.describeItemCounter > 20)
                {
                    moon.describeItemCounter = 20;
                }
            }
            else
            {
                if (moon.nextPos != moon.oracle.room.MiddleOfTile(77, 18))
                {
                    moon.SetNewDestination(moon.oracle.room.MiddleOfTile(77, 18));
                }
                if (!moon.holdKnees && Custom.DistLess(moon.oracle.firstChunk.pos, moon.oracle.room.MiddleOfTile(77, 18), 75))
                {
                    moon.holdKnees = true;
                }

                if (moon.player != null)
                {
                    if (moon.player.room != moon.oracle.room || moon.player.DangerPos.x < 1016f)
                    {
                        moon.playerLeavingCounter++;
                    }
                    else
                    {
                        moon.playerLeavingCounter = 0;
                    }

                    if (moon.playerLeavingCounter == 11 && moon.player.grasps != null)
                    {
                        for (int i = 0; i < moon.player.grasps.Length; i++)
                        {
                            if (moon.player.grasps[i] == null || moon.player.grasps[i].grabbed is null || moon.player.grasps[i].grabbed is not HalcyonPearl)
                            {
                                continue;
                            }

                            moon.currentConversation ??= new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.None, moon, SLOracleBehaviorHasMark.MiscItemType.NA);

                            if (musicPearlStolenBackCount == 1)
                            {
                                moon.currentConversation.Interrupt("Little flame, this is mean!", 120);
                                moon.State.InfluenceLike(-0.5f);
                            }
                            else if (musicPearlStolenBackCount == 2)
                            {
                                moon.currentConversation.Interrupt("Little one, come back with that!", 130);
                                moon.State.InfluenceLike(-1.1f);
                            }
                            else if (musicPearlStolenBackCount == 3)
                            {
                                moon.currentConversation.Interrupt("How could you be so cruel?! I cannot forgive this.", 140);
                                moon.State.InfluenceLike(-1.4f);
                                moon.NoLongerOnSpeakingTerms();
                            }
                            else if (musicPearlStolenBackCount == 4)
                            {
                                moon.currentConversation.Interrupt("You are a terrible little beast. Do not come back.", 150);
                                moon.State.likesPlayer = -1;
                                moon.NoLongerOnSpeakingTerms();
                            }
                            musicPearlStolenBackCount++;
                            moon.State.increaseLikeOnSave = false;
                            break;
                        }
                    }
                }
            }  
        }
    }

    public static void HasMoonTalkedAboutItemAlready(On.SLOracleBehaviorHasMark.orig_GrabObject orig, SLOracleBehaviorHasMark moon, PhysicalObject obj)
    {
        if (moon.oracle.room.game.StoryCharacter == IncanInfo.Incandescent)
        {
            incPresent = false;
            if (moon?.oracle?.room?.abstractRoom is not null)
            {
                foreach (AbstractCreature absCtr in moon.oracle.room.abstractRoom.creatures)
                {
                    if (absCtr != null && absCtr.realizedCreature is Player plr && IncanInfo.IncanData.TryGetValue(plr, out IncanInfo player) && player.isIncan)
                    {
                        incPresent = true;
                        break;
                    }
                }
            }  

            alreadyTalkedAbout = false;
            if (moon.State.HaveIAlreadyDescribedThisItem(obj.abstractPhysicalObject.ID))
            {
                alreadyTalkedAbout = true;
            }
            if (musicPearlStolenBackCount == Mathf.Clamp(musicPearlStolenBackCount, 1, 3) &&
                moon.State.alreadyTalkedAboutItems.Contains(obj.abstractPhysicalObject.ID) &&
                obj is HalcyonPearl && (moon.holdingObject is null || (moon.holdingObject is not null && moon.holdingObject == obj)))
            {
                moon.State.alreadyTalkedAboutItems.Remove(obj.abstractPhysicalObject.ID);
            }

            hasTalkedAboutFadedPearls = false;
            if (moon.oracle.room.game.session is StoryGameSession SGS && SGS.saveState.miscWorldSaveData.SLOracleState.totalPearlsBrought > 0)
            {
                int readablePearlsRead = 0;
                for (int i = 0; i < SGS.saveState.miscWorldSaveData.SLOracleState.significantPearls.Count; i++)
                {
                    if (SGS.saveState.miscWorldSaveData.SLOracleState.significantPearls[i] == DataPearl.AbstractDataPearl.DataPearlType.LF_west ||
                        SGS.saveState.miscWorldSaveData.SLOracleState.significantPearls[i] == MoreSlugcatsEnums.DataPearlType.CL)
                    {
                        readablePearlsRead++;
                    }
                }
                if (SGS.saveState.miscWorldSaveData.SLOracleState.totalPearlsBrought > readablePearlsRead)
                {
                    hasTalkedAboutFadedPearls = true;
                }                        
            }                         
        }     

        orig(moon, obj);

        if (alreadyTalkedAbout && !moon.State.HaveIAlreadyDescribedThisItem(obj.abstractPhysicalObject.ID))
        {
            moon.State.AddItemToAlreadyTalkedAbout(obj.abstractPhysicalObject.ID);
        }
    }

    public static bool MoonItemInspection(On.SLOracleBehaviorHasMark.orig_WillingToInspectItem orig, SLOracleBehaviorHasMark moon, PhysicalObject item)
    {
        if (firstCycleSeeingMusicPearl > 0)
        {
            if (item is HalcyonPearl && item != moon.holdingObject && musicPearlStolenBackCount <= 3)
            {
                return true;
            }
            return false;
        }
        if (item is Lizard liz && (liz.Template.type == HSEnums.CreatureType.FreezerLizard || liz.Template.type == HSEnums.CreatureType.IcyBlueLizard))
        {
            return false;
        }
        return orig(moon, item);
    }

    public static void StealingMusicPearlBack(On.SLOracleBehaviorHasMark.orig_PlayerInterruptByTakingItem orig, SLOracleBehaviorHasMark moon)
    {
        if (firstCycleSeeingMusicPearl > 0 && moon.State != null)
        {
            if (moon.currentConversation != null)
            {
                moon.currentConversation.Destroy();
                moon.currentConversation = null;
            }

            moon.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.None, moon, SLOracleBehaviorHasMark.MiscItemType.NA);

            if (musicPearlStolenBackCount == 0)
            {
                moon.currentConversation.Interrupt("Wha-", 0);
                AddDialogue(moon.currentConversation, 0, "No, incandescent, please! Bring that back!", 60);                    
            }
            else if (musicPearlStolenBackCount == 1)
            {
                moon.currentConversation.Interrupt("NO, this is not the time to be doing this! Stop at once!", 20);
                AddDialogue(moon.currentConversation, 0, "I will not forgive you if you take this from me now!", 60);
                moon.State.InfluenceLike(-0.2f);
            }
            else if (musicPearlStolenBackCount == 2)
            {
                moon.currentConversation.Interrupt("No, I WILL NOT put up with this. Not now.", 0);
                AddDialogue(moon.currentConversation, 0, "Give that back immediately!", 60);
                moon.State.InfluenceLike(-0.8f);
                moon.State.increaseLikeOnSave = false;
            }
            else if (musicPearlStolenBackCount == 3)
            {
                moon.currentConversation.Interrupt("...", 0);
                AddDialogue(moon.currentConversation, 0, "You know what? Fine. Then stay away. I do not need you toying with my emotions.", 120);
                moon.State.likesPlayer = -1;
                moon.NoLongerOnSpeakingTerms();
                moon.State.increaseLikeOnSave = false;
            }
            else if (musicPearlStolenBackCount == 4)
            {
                moon.currentConversation.Interrupt("Stay AWAY!", 90);
                moon.State.likesPlayer = -1;
                moon.NoLongerOnSpeakingTerms();
                moon.State.increaseLikeOnSave = false;
            }
            musicPearlStolenBackCount++;
            return;
        }
        orig(moon);
    }

    public static SLOracleBehaviorHasMark.MiscItemType NewMoonConversationTypes(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark moon, PhysicalObject item)
    {         
        if (moon.currentConversation is SLOracleBehaviorHasMark.MoonConversation mConv && mConv.describeItem == IceChunk && item == null)
        {
            return IceChunk_BrokenMidtalk;
        }

        if (item is IceChunk)
        {
            return IceChunk;                    
        }

        return orig(moon, item);
    }

    public static void NewMoonDialogue(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation mConv)
    {
        orig(mConv);

        if (mConv.currentSaveFile == IncanInfo.Incandescent)
        {
            if (mConv.id == Conversation.ID.Moon_Pebbles_Pearl || mConv.id.value.StartsWith("Moon_Pearl"))
            {

                RemoveEvents(mConv);

                if (mConv.id == Conversation.ID.Moon_Pearl_LF_west)
                {
                    if (!alreadyTalkedAbout)
                    {
                        AddDialogue(mConv, 0, "...", 0);
                        AddDialogue(mConv, 0, "A functional pearl? How did you find this?", 20);
                        AddDialogue(mConv, 0, "This is written in plain text.", 20);
                        AddDialogue(mConv, 0, "\"On regards of the (by spiritual splendor eternally graced) people of the Congregation of...\"", 30);
                        AddDialogue(mConv, 0, "I am sorry little flame, I am too amazed about the clarity of this data, despite its age...", 30);
                    }
                    else
                    {
                        AddDialogue(mConv, 0, mConv.myBehavior.AlreadyDiscussedItemString(pearl: true), 10);
                        AddDialogue(mConv, 0, "A pearl written in plain text. I am still curious as to how you found this, little flame.", 25);
                        AddDialogue(mConv, 0, "\"On regards of the (by spiritual splendor eternally graced) people of the Congregation of...\"", 30);
                        AddDialogue(mConv, 0, "It is truly amazing how well the data on this pearl has survived after all this time.", 20);
                    }
                    AddDialogue(mConv, 0, "At this point it's safe to say that this text has been burned into the pearl's structure permanently.<LINE>I don't think I could clear this pearl out for my own purposes even if I wanted to.", 60);
                    AddDialogue(mConv, 0, "What a bizarre thing to have lasted all this time. I wonder how much of my creators will be remembered.", 30);
                }
                else if (mConv.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
                {
                    if (firstCycleSeeingMusicPearl == 0)
                    {
                        if (!alreadyTalkedAbout)
                        {
                            AddDialogue(mConv, 0, "A pearl? It's highly faded, yet a faint amount of data still...", 40);
                            AddDialogue(mConv, 0, "...Oh. Little flame, I believe I know exactly where you found this. Please, return this.", 40);
                            AddDialogue(mConv, 0, "I am sure what little is left of him misses this dearly.", 30);
                            AddDialogue(mConv, 40, "...Little flame?", 40);
                            AddDialogue(mConv, 0, "Little one, what's wrong?", 60);
                            AddDialogue(mConv, 90, "...", 160);
                            AddDialogue(mConv, 0, "...Oh.", 70);
                            AddDialogue(mConv, 60, "I...", 70);
                            AddDialogue(mConv, 10, "I see.", 90);
                            AddDialogue(mConv, 0, "Little one, I...", 20);
                            AddDialogue(mConv, 160, "I apologize, but I need some time alone.", 40);
                            AddDialogue(mConv, 80, "Allow me to hold on to this for this cycle. Please.", 80);
                        }
                        else
                        {

                        }
                    }
                    else if (musicPearlStolenBackCount == 1)
                    {
                        AddDialogue(mConv, 0, "Thank you, little one.", 20);
                        AddDialogue(mConv, 20, "Please, don't do that again. This pearl means much more to me than you might think.", 60);
                        AddDialogue(mConv, 20, "I want some time to think. Allow me that much, at least.", 30);
                    }
                    else if (musicPearlStolenBackCount == 2)
                    {
                        AddDialogue(mConv, 0, "Please... allow me some time to myself.<LINE>At least for this cycle.", 50);
                        AddDialogue(mConv, 10, "I need this.", 40);
                    }
                    else if (musicPearlStolenBackCount == 3)
                    {
                        AddDialogue(mConv, 0, "I am not willing to put up with whatever it is you are trying to do. Leave me be for now.", 40);
                        AddDialogue(mConv, 0, "Your antics are only bringing me more distress.", 30);
                    }

                    if (!blizzardNoticed)
                    {
                        blizzardNoticed = true;
                    }
                }
                else // Unreadable pearls
                {
                    if (!hasTalkedAboutFadedPearls)
                    {
                        AddDialogue(mConv, 0, "It's a pearl! With nothing left inside, unfortunately.", 10);
                        AddDialogue(mConv, 0, "Little flame, I'm sure you know that these are valued greatly by the scavengers for trade and decoration.<LINE>But did you know that pearls were originally made to store information?", 60);
                        AddDialogue(mConv, 0, "Back then, my kind made extensive use of pearls in our search for an answer to a great problem.<LINE>Each of our structures housed hundreds, if not thousands, of them, and they came in a wide variety of colors.", 70);
                        AddDialogue(mConv, 0, "They were quite handy back when they were still functional, but by this point, time has likely taken its toll<LINE>on most pearls. Their colors have faded away, as well as their information.", 60);
                        AddDialogue(mConv, 0, "This pearl is no different. There is not much that I can do with this now, but as I said<LINE>earlier, I am sure the scavengers would still appreciate them!", 60);
                    }
                    else
                    {
                        AddDialogue(mConv, 0, "Oh, little flame, another pearl! I have told you this already, but there is nothing left to these besides their shine.", 40);
                        AddDialogue(mConv, 0, "Pearls are valued greatly by the scavengers for trade and decoration, so if you want to<LINE>make use of them, I'm sure the scavengers would appreciate a gift!", 60);
                        AddDialogue(mConv, 0, "If there were any pearls that could still function after all this time, I may be able to use them, but this one is too damaged.", 50);
                        AddDialogue(mConv, 0, "I still appreciate the gift, though, little one! Thank you.", 20);
                    }
                }
            }
            else if (mConv.describeItem == SLOracleBehaviorHasMark.MiscItemType.Lantern)
            {

                RemoveEvents(mConv);

                if (alreadyTalkedAbout)
                {
                    AddDialogue(mConv, 0, mConv.myBehavior.AlreadyDiscussedItemString(pearl: false), 10);
                }

                AddDialogue(mConv, 0, "It appears to be... a pupa shell, smeared on the inside with a glowing substance? It's been mixed<LINE>with fire powder for warmth. It's not much, but anything to escape the cold is welcome.", 40);
                if (incPresent)
                {
                    AddDialogue(mConv, 0, "Of course, you're already plenty capable of withstanding the cold on your own, little one!", 10);
                    AddDialogue(mConv, 0, "This might still be helpful for you, though. Perhaps it can help dry your tail whenever it gets wet!", 10);
                }
                else
                {
                    AddDialogue(mConv, 0, "The scavengers never cease to adapt, even in this weather!", 10);
                }
            }
            else if (mConv.id == Conversation.ID.MoonRecieveSwarmer)
            {

                RemoveEvents(mConv);

                if (mConv.State.neuronGiveConversationCounter == 0)
                {
                    if (mConv.State.neuronsLeft <= 2)
                    {
                        AddDialogue(mConv, 0, "...", 40);
                        AddDialogue(mConv, 0, "...where did... you get... this?..", 120);
                    }
                    else
                    {
                        AddDialogue(mConv, 0, "...", 40);
                        AddDialogue(mConv, 0, "...Where... did you get this?..", 80);
                    }                   
                }
                else if (mConv.State.neuronGiveConversationCounter >= 1)
                {
                    if (mConv.State.neuronsLeft <= 2)
                    {
                        AddDialogue(mConv, 0, "...", 40);
                        AddDialogue(mConv, 0, "...little... fire...", 100);
                        AddDialogue(mConv, 0, "...where... getting these?..", 100);
                    }
                    else
                    {
                        AddDialogue(mConv, 0, "..I... do not know how I... feel about accepting these.", 80);
                        AddDialogue(mConv, 0, "Little one, where are you getting them from?", 80);
                    }                   
                }
            }
            else if (mConv.describeItem == IceChunk)
            {

                RemoveEvents(mConv);                   

                if (alreadyTalkedAbout)
                {
                    AddDialogue(mConv, 0, mConv.myBehavior.AlreadyDiscussedItemString(pearl: false), 10);
                }

                if (incPresent)
                {
                    AddDialogue(mConv, 0, "This is... really cold! Little incandescent, you must be incredibly careful when holding this!", 0);
                    AddDialogue(mConv, 0, "Besides being dangerously cold, this crystal is incredibly sharp, as well as highly fragile. You might be<LINE>able to use it as a weapon if necessary, although it would most likely shatter on impact.", 40);
                    if (!alreadyTalkedAbout)
                    {
                        AddDialogue(mConv, 0, "I'm quite curious as to where you found this. I will have to send some of<LINE>my Overseers out to find some more! Thank you, little one.", 20);
                    }
                }
                else
                {
                    AddDialogue(mConv, 0, "This is... really cold! Little friend, you must be careful when holding this!", 0);
                    AddDialogue(mConv, 0, "This crystal is incredibly sharp and highly fragile. It could likely be used as a weapon<LINE>if necessary, although I wouldn't expect it to remain intact afterwards.", 30);
                    if (!alreadyTalkedAbout)
                    {
                        AddDialogue(mConv, 0, "What a peculiar crystal. I must send some of my Overseers out to<LINE>study them. Thank you, little one.", 20);
                    }
                }
            }
        }       
        else if (mConv.describeItem == IceChunk)
        {
            RemoveEvents(mConv);

            if (mConv.currentSaveFile != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                AddDialogue(mConv, 0, "This is extremely cold! Take care when holding this, little one.", 0);
                AddDialogue(mConv, 0, "This strange crystal is incredibly sharp, as well as highly fragile. You could likely use it as<LINE>a weapon if necessary, although I wouldn't expect it to remain intact afterwards.", 30);
            }
            else
            {
                AddDialogue(mConv, 0, "This is... quite cold! Be careful when holding this, strange friend.", 0);
                AddDialogue(mConv, 0, "This crystal is incredibly sharp, as well as highly fragile. You could likely use it as a weapon<LINE>if necessary, although I wouldn't expect it to remain intact afterwards.", 30);
                if (!alreadyTalkedAbout)
                {
                    AddDialogue(mConv, 0, "What a peculiar crystal. I must send some of my Overseers out to<LINE>study them. Thank you, friend.", 10);
                }
            }               
        }
    }

    public static void Moon_AttackedInIncanStory(On.SLOracleBehaviorHasMark.orig_Pain orig, SLOracleBehaviorHasMark moon)
    {
        bool RFSN = moon.respondToNeuronFromNoSpeakMode;
        float LP = moon.State.likesPlayer;

        orig(moon);
        if (IsIncanStory(moon.oracle.room.game))
        {

            Conversation mConv = moon.currentConversation;

            mConv ??= new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.None, moon, SLOracleBehaviorHasMark.MiscItemType.NA);

            moon.State.likesPlayer = LP - 0.3f;
            moon.respondToNeuronFromNoSpeakMode = RFSN;            

            if (firstCycleSeeingMusicPearl == 0)
            {                

                bool moonLikes = moon.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;

                switch (Random.Range(0, 2))
                {
                    case 0:
                        mConv.Interrupt((moonLikes? "INCANDESCENT!" : "FIERY ONE!") + " Do not do that again! I will not allow you to treat me like that!", 40);
                        break;
                    case 1:
                        if (moonLikes)
                        {
                            mConv.Interrupt("Stop that! What have I done to deserve this, incandescent one?", 10);
                        }
                        else
                        {
                            mConv.Interrupt("Stop that! What have I done to deserve this, fiery one?", 10);
                        }                       
                        break;
                    case 2:
                        if (moonLikes)
                        {
                            mConv.Interrupt("Little one! Why would you do such a thing?! You know I mean you no harm!", 0);
                        }
                        else
                        {
                            mConv.Interrupt("Little one, what is your explanation for this?! You know I mean you no harm!", 0);
                        }                        
                        break;
                }
            }
        }
    }

    //------------------------------------------


    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static void IncanConversationReactions(On.Conversation.DialogueEvent.orig_Update orig, Conversation.DialogueEvent dEvent)
    { // Delicious hardcoded timings, oh boy.
        if (dEvent.owner.currentSaveFile == IncanInfo.Incandescent && dEvent.owner is not null)
        {
            List<Conversation.DialogueEvent> events = dEvent.owner.events;
            if (dEvent.owner.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_RM)
            {
                if (!alreadyTalkedAbout)
                {
                    if (events.Count == 11 && events[0].age == 120)
                    {
                        IncanVisuals.incSad = true;
                    }
                    else if (events.Count == 10 && events[0].age == 80)
                    {
                        IncanVisuals.incLookDown = true;
                    }
                    else if (events.Count == 8 && events[0].age == 50)
                    {
                        IncanVisuals.incLookDown = false;
                        IncanVisuals.incLookAtMoon = true;
                        firstCycleSeeingMusicPearl++;
                    }
                    else if (events.Count == 1 && events[0].IsOver)
                    {
                        IncanVisuals.incLookAtMoon = false;
                        firstCycleSeeingMusicPearl++;
                    }
                }
            }
        }           
        orig(dEvent);
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // Non-hook methods

    public static void RemoveEvents(Conversation conv)
    {
        for (int i = conv.events.Count - 1; i >= 0; i--)
        {
            conv.events.Remove(conv.events[i]);
        }
    }

    public static void AddDialogue(Conversation conv, int initialWait, string text, int textLinger)
    {
        conv.events.Add(new Conversation.TextEvent(conv, initialWait, text, textLinger));
    }


    public static int AreSlugpupsInRoom(SLOracleBehaviorHasMark moon)
    {
        int slugpupCount = 0;
        foreach (AbstractCreature absCtr in moon.oracle.room.abstractRoom.creatures)
        {
            bool isSlugpup = (absCtr.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && absCtr.state.alive);
            if (isSlugpup) slugpupCount++;
        }
        return slugpupCount;
    }
    public static int AreStrayCreaturesInRoom(SLOracleBehaviorHasMark moon)
    {
        int creatureCount = 0;
        foreach (AbstractCreature absCtr in moon.oracle.room.abstractRoom.creatures)
        {
            bool isValidCreature =
                absCtr.state.alive &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.Slugcat &&
                absCtr.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.TubeWorm &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.VultureGrub &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.Hazer &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.Fly &&
                absCtr.creatureTemplate.type != CreatureTemplate.Type.Overseer;

            if (isValidCreature) creatureCount++;
        }
        return creatureCount;
    }

    //------------------------------------------

    public static void NoticeBlizzard(SLOracleBehaviorHasMark moon)
    {
        DialogBox speak = moon.dialogBox;
        bool company = (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 0);
        bool MoonLikesYou = moon.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes;
        bool MoonIsNeutralToYou = moon.State.GetOpinion == SLOrcacleState.PlayerOpinion.Neutral;
        bool MoonDislikesYou = moon.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes;

        if (!IsIncanStory(moon.oracle.room.game))
        {
            return;
        }

        switch (moon.State.neuronsLeft)
        {
            case >= 6: // Six or more neurons
                if (MoonLikesYou)
                {
                    speak.NewMessage(moon.Translate("Oh, it sounds like the blizzard has arrived!<LINE>Little incandescent, do you still wish to stay?"), (company ? 20 : 40));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("If you do, be sure that your friends don't freeze!"), 20);

                        else if (AreSlugpupsInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("If you do, take care that your little friend doesn't freeze!<LINE>They don't have a fiery tail like you do!"), 20);

                        else if (AreStrayCreaturesInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("If you do, take care that your friend doesn't freeze!<LINE>They don't seem as resistant to the cold as you are!"), 20);
                    }
                    else speak.NewMessage(moon.Translate("If so, make sure to avoid the water until you leave!"), 20);
                }
                if (MoonIsNeutralToYou)
                {
                    speak.NewMessage(moon.Translate("Oh, it sounds like the blizzard has arrived."), 20);
                    speak.NewMessage(moon.Translate("If you wish to stay, then be sure to avoid the water, little incandescent."), (company ? 10 : 20));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("And keep your friends warm!"), 10);

                        else if (AreSlugpupsInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("And keep your little friend warm!"), 10);

                        else if (AreStrayCreaturesInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("And keep your friend warm!"), 10);
                    }
                }
                else if (MoonDislikesYou)
                {
                    speak.NewMessage(moon.Translate("Oh, the blizzard has arrived."), 15);
                    speak.NewMessage(moon.Translate("Not that it matters for you, I suppose..."), 5);
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("For the sake of both your friends and myself, however, I implore that you leave."), 20);

                        else if (AreSlugpupsInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("...However, your young friend is not as fortunate.<LINE>Please leave, for their sake."), 15);

                        else if (AreStrayCreaturesInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("...But it might matter for your friend.<LINE>Please leave, and take them with you."), 15);
                    }
                }
                break;
            case 5: // Five neurons
                if (MoonLikesYou)
                {
                    speak.NewMessage(moon.Translate("Oh, the storm is here!"), 15);
                    speak.NewMessage(moon.Translate("Fiery friend, if you wish to stay, then be careful of the water!"), (company ? 15 : 30));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("Keep your friends warm, too!"), 25);

                        else if (AreSlugpupsInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("Keep your little friend warm, too!"), 25);

                        else if (AreStrayCreaturesInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("Keep your friend warm, too!"), 25);
                    }
                }
                if (MoonIsNeutralToYou)
                {
                    speak.NewMessage(moon.Translate("It sounds like the storm has arrived."), 25);
                    speak.NewMessage(moon.Translate("If you intend to stay, fiery one, be sure to avoid the water."), (company ? 15 : 30));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("And don't forget that your friends are not as protected from the cold as you are!"), 15);

                        else if (AreSlugpupsInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("Although, your little friend is shivering badly.<LINE>Maybe you should leave, for their sake."), 15);

                        else if (AreStrayCreaturesInRoom(moon) == 1)
                            speak.NewMessage(moon.Translate("Though I should mention that your friend doesn't seem to be taking the cold as well as you are..."), 15);
                    }
                }
                else if (MoonDislikesYou)
                {
                    speak.Interrupt(moon.Translate("...Oh, the storm has arrived."), (company ? 10 : 20));
                    if (!company)
                        speak.NewMessage(moon.Translate("You should leave."), 10);
                    else
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("You should leave, for both your friends' sakes and for mine."), 20);

                        else speak.NewMessage(moon.Translate("You should go, so that your friend doesn't freeze."), 20);
                    }
                }
                break;
            case 4: // Four neurons
                if (MoonDislikesYou)
                {
                    speak.Interrupt("...", 5);
                    moon.dialogBox.NewMessage(moon.Translate("The storm is here."), 25);
                    moon.dialogBox.NewMessage(moon.Translate("You... should leave."), (company ? 25 : 45));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("If you value your friends... do not let them freeze."), 25);

                        else speak.NewMessage(moon.Translate("If not for my sake... then for your friend."), 25);
                    }
                }
                else
                {
                    speak.NewMessage(moon.Translate("...The storm is here. Be careful, little flame."), (company ? 25 : 45));
                    if (company)
                    {
                        if (AreSlugpupsInRoom(moon) + AreStrayCreaturesInRoom(moon) > 1)
                            speak.NewMessage(moon.Translate("Your friends... need warmth, too."), 15);

                        else speak.NewMessage(moon.Translate("Protect your friend from the cold..."), 15);
                    }
                }
                break;
            case 3: // Three neurons
                if (MoonDislikesYou)
                {
                    speak.Interrupt("...", 15);
                    moon.dialogBox.NewMessage(moon.Translate("...Cold..."), 35);
                    moon.dialogBox.NewMessage(moon.Translate("...leave... now."), (company ? 35 : 55));
                    if (company)
                        speak.NewMessage(moon.Translate("Take them... with you..."), 30);
                }
                else
                {
                    speak.Interrupt("...", 15);
                    moon.dialogBox.NewMessage(moon.Translate("...Cold..."), 35);
                    moon.dialogBox.NewMessage(moon.Translate("not... safe."), (company ? 35 : 55));
                    if (company)
                        speak.NewMessage(moon.Translate("Protect... friends!"), 35);
                }
                break;
            case 2: // Two neurons
                moon.dialogBox.Interrupt(moon.Translate("...s... storm..."), 60);
                moon.dialogBox.NewMessage(moon.Translate("run"), 80);
                break;
            case 1: // One neuron
                moon.dialogBox.NewMessage(moon.Translate("..."), 180);
                break;
        }
    }

    public static void CrystalBrokeMidtalk(Conversation conv, int initialWait, int textLinger)
    {
        string text;

        switch (Random.Range(0, 4))
        {
            case 1:
                conv.Interrupt("!", 0);
                text = "Little one! You would bring this all the way to me just to smash it in my hand?<LINE>Well, I hope the amusement was worth the effort...";
                break;
            case 2:
                conv.Interrupt("...", 60);
                text = "Nevermind then.";
                break;
            case 3:
                conv.Interrupt("...", 20);
                text = "And there it goes.";
                break;
            case 4:
                conv.Interrupt("Little one! Goodness...", 40);
                text = "Well, all that's left to talk about now are little ice shards, and I do not think<LINE>either of us will find much interest in those.";
                break;
            default:
                conv.Interrupt("...", 80);
                text = "...Little one, why did you do that?";
                break;
        }

        AddDialogue(conv, initialWait, text, textLinger);
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}