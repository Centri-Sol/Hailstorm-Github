using System;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using SlugBase.DataTypes;
using SlugBase.Features;
using SlugBase;
using JollyCoop;

namespace Hailstorm;

public class IncanVisuals
{

    public static void Hooks()
    {
        On.PlayerGraphics.ctor += Ctor;
        On.PlayerGraphics.InitiateSprites += InitiateSprites;
        On.PlayerGraphics.DrawSprites += DrawNewSprites;
        On.PlayerGraphics.AddToContainer += SpriteLayering;
        On.PlayerGraphics.ApplyPalette += ApplyPalette;

        IL.PlayerGraphics.InitiateSprites += InitiateSprites_1;

        On.PlayerGraphics.Update += RollAnimationUpdate;
    }

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public static bool incSad;

    public static bool incLookPointReset;
    public static bool incLookDown;
    public static bool incLookAtMoon;

    //---------------------------------------

    // Creates new tail segments to replace the default ones for the Incandescent. Their size, length, connection points, and friction values can all be messed with!
    public static void Ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig.Invoke(self, ow);
        if (!IncanInfo.IncanData.TryGetValue(self.player, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        if (MiscWorldChanges.AllEchoesMet && !player.ReadyToMoveOn)
        {
            player.ReadyToMoveOn = true;
        }

        if (self.RenderAsPup) // Tail segments for if you're playing as a slugpup.
        {
            self.tail[0] = new TailSegment(self, 5f, 5.25f, null, 0.75f, 0.6f, 1f, true);
            self.tail[1] = new TailSegment(self, 3.75f, 5.25f, self.tail[0], 0.75f, 0.6f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 2.5f, 5.25f, self.tail[1], 0.56f, 0.45f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 1.25f, 7.5f, self.tail[2], 0f, 0f, 0f, true);
        }
        else // Tail segments for if you're playing as a full-size slugcat.
        {
            self.tail[0] = new TailSegment(self, 6f, 8f, null, 0.75f, 0.6f, 1f, true);
            self.tail[1] = new TailSegment(self, 4.5f, 8f, self.tail[0], 0.75f, 0.6f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 3f, 8f, self.tail[1], 0.56f, 0.45f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 1.5f, 11f, self.tail[2], 0f, 0f, 0f, true);
        }
        List<BodyPart> list = self.bodyParts.ToList(); // Feeds the Slugcat's bodyparts to a list.
        list.RemoveAll((x) => x is TailSegment); // Removes all tail segments from the list.
        list.AddRange((IEnumerable<BodyPart>)(object)self.tail); // Adds the new tail segments that were created above into the list.
        self.bodyParts = list.ToArray(); // Makes the Slugcat's bodyparts equal to those in the list, replacing the old tail segments with new ones.  
    }

    // Creates the Incandescent's new sprites, giving her her unique look!
    public static void InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (!IncanInfo.IncanData.TryGetValue(self.player, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        player.cheekFluffSprite = sLeaser.sprites.Length;
        player.waistbandSprite = player.cheekFluffSprite + 1;
        player.tailflameSprite = player.waistbandSprite + 1;

        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 3);

        sLeaser.sprites[player.cheekFluffSprite] = new FSprite("incanCheekfluffHeadA0", true);
        sLeaser.sprites[player.waistbandSprite] = new FSprite("incanWaistband", true);
        sLeaser.sprites[player.tailflameSprite] = new FSprite("incanTailflame", true);


        self.AddToContainer(sLeaser, rCam, null);
    }

    // Applies proper positioning and visuals to each Initiated Sprite.
    public static void DrawNewSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!IncanInfo.IncanData.TryGetValue(self.player, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        if (self.lightSource is not null && self.lightSource.alpha != 0)
        {
            self.lightSource.alpha = 0; // Makes the normal Neuron glow invisible for the Incandescent.
        }  

        Player incan = self.owner as Player;

        FSprite headSprites = sLeaser.sprites[3];
        FSprite faceSprites = sLeaser.sprites[9];
        string headSpriteNames = sLeaser.sprites[3].element.name;
        FSprite bodySprites = sLeaser.sprites[1];
        Color waistbandColor = player.WaistbandColor == Color.clear ? new Color(0.1f, 0.1f, 0.1f) : player.WaistbandColor;

        // List of Slugcat sprites:
        /* 0 - BodyA                    Body
         * 1 - HipsA                    Hips
         * 2 - Futile_White             Tail
         * 3 - HeadA0                   Head
         * 4 - LegsA0                   Legs (both are 1 sprite)
         * 5 - PlayerArm0               Arm (the arms are individual sprites)
         * 6 - PlayerArm0               Arm
         * 7 - OnTopOfTerrainHand       Pole-climbing Hand (the ball hands you see when slugcat is climbing a pole)
         * 8 - OnTopOfTerrainHand       Pole-climbing Hand
         * 9 - FaceA0                   Face
         * 10 - Futile_White            Glow (from Neuron Flies)
         * 11 - pixel                   Mark of Communication (it's literally a single pixel but scaled up a LOT)
         * 12 - MoonCloakTex            Moon's Cloak (from Submerged Superstructure)
         */

        // Sad head and faces.
        if (!headSprites.element.name.Contains("incanSad") && (!player.ReadyToMoveOn || player.ReadyToMoveOn && incSad))
        {
            if (headSprites.element.name.StartsWith("HeadC"))
            {
                headSprites.SetElementByName("incanSad" + headSprites.element.name);
            }
            else if (headSprites.element.name.StartsWith("HeadA"))
            {
                headSprites.SetElementByName("incanSad" + headSprites.element.name);
            }


            if (faceSprites.element.name.StartsWith("PFaceA"))
            {
                faceSprites.SetElementByName("incanSad" + faceSprites.element.name);
            }
            else if (faceSprites.element.name.StartsWith("FaceA"))
            {
                faceSprites.SetElementByName("incanSad" + faceSprites.element.name);
            }
        }

        if (incan is not null)
        {            
            if (incLookDown)
            {
                self.objectLooker.lookAtPoint = !self.objectLooker.lookAtPoint.HasValue ?
                    incan.bodyChunks[0].pos :
                    Vector2.Lerp(self.objectLooker.lookAtPoint.Value, incan.bodyChunks[0].pos + new Vector2(3 + (8 * incan.flipDirection), -20), 0.2f);
            }
            else if (incLookAtMoon && incan.room is not null)
            {
                foreach (AbstractPhysicalObject absObj in incan.room.abstractRoom.creatures)
                {
                    if (absObj.realizedObject is Oracle oracle && Custom.DistLess(incan.firstChunk.pos, oracle.firstChunk.pos, 800))
                    {
                        self.objectLooker.lookAtPoint = oracle.firstChunk.pos;
                    }
                }
            }

            if (incLookDown || incLookAtMoon)
            {
                incLookPointReset = true;
            }
            else if (incLookPointReset)
            {
                incLookPointReset = false;
                self.objectLooker.lookAtPoint = null;
            }
        }       


        // Cheek fluff sprites.
        if (!string.IsNullOrWhiteSpace(headSpriteNames) && headSpriteNames.StartsWith("Head"))
        {
            string headSpriteNumber = headSpriteNames.Substring(5);

            float cheekFluffOffsetX = 0f;
            float cheekFluffOffsetY = 0f;
            switch (headSpriteNumber)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                    cheekFluffOffsetY = 2f;
                    break;
                case "5":
                case "6":
                    cheekFluffOffsetX = -1.5f * Math.Sign(sLeaser.sprites[3].scaleX);
                    break;
                case "7":
                    cheekFluffOffsetY = -3.5f;
                    break;
            }

            Vector2 cheekFluffPos = new (sLeaser.sprites[3].x + cheekFluffOffsetX, sLeaser.sprites[3].y + cheekFluffOffsetY);

            sLeaser.sprites[player.cheekFluffSprite].scaleX = sLeaser.sprites[3].scaleX;
            sLeaser.sprites[player.cheekFluffSprite].scaleY = 1f;
            sLeaser.sprites[player.cheekFluffSprite].rotation = sLeaser.sprites[3].rotation;
            sLeaser.sprites[player.cheekFluffSprite].x = cheekFluffPos.x;
            sLeaser.sprites[player.cheekFluffSprite].y = cheekFluffPos.y;
            sLeaser.sprites[player.cheekFluffSprite].color = player.FireColor;

            if (headSpriteNames.StartsWith("HeadA")) sLeaser.sprites[player.cheekFluffSprite].element = Futile.atlasManager.GetElementWithName("incanCheekfluff" + headSpriteNames);

            else if (headSpriteNames.StartsWith("HeadC")) sLeaser.sprites[player.cheekFluffSprite].element = Futile.atlasManager.GetElementWithName("incanCheekfluff" + headSpriteNames);

            player.lastCheekfluffPos = new Vector2(sLeaser.sprites[player.cheekFluffSprite].x, sLeaser.sprites[player.cheekFluffSprite].y);
        }

        // Waistband sprite.
        if (!string.IsNullOrWhiteSpace(bodySprites.element.name))
        {

            var waistbandOffsetX = 0f;
            var waistbandOffsetY = -1f;

            Vector2 waistbandPos = new (sLeaser.sprites[1].x + waistbandOffsetX, sLeaser.sprites[1].y + waistbandOffsetY);

            sLeaser.sprites[player.waistbandSprite].scaleX = sLeaser.sprites[1].scaleX;
            sLeaser.sprites[player.waistbandSprite].scaleY = 1f;
            sLeaser.sprites[player.waistbandSprite].rotation = sLeaser.sprites[1].rotation;
            sLeaser.sprites[player.waistbandSprite].x = waistbandPos.x;
            sLeaser.sprites[player.waistbandSprite].y = waistbandPos.y;
            sLeaser.sprites[player.waistbandSprite].element = Futile.atlasManager.GetElementWithName("incanWaistband");
            sLeaser.sprites[player.waistbandSprite].color = waistbandColor;

            player.lastWaistbandPos = new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y);
        }
        // Tail flame sprite.
        if (!string.IsNullOrWhiteSpace(bodySprites.element.name))
        {

            float tailflameOffsetX = camPos.x;
            float tailflameOffsetY = camPos.y;

            Vector2 tailflamePos = new (Mathf.Lerp(self.tail[3].pos.x, self.tail[2].pos.x, 0.2f) - tailflameOffsetX,
                                            Mathf.Lerp(self.tail[3].pos.y, self.tail[2].pos.y, 0.2f) - tailflameOffsetY);
            Vector2 tailAngle = (self.tail[3].pos - self.tail[2].pos).normalized;
            int rotationSide = (tailAngle.x > 0) ? 1 : -1;

            sLeaser.sprites[player.tailflameSprite].scaleX = rotationSide * (incan.isSlugpup ? 0.7f : 1f);
            sLeaser.sprites[player.tailflameSprite].scaleY = (incan.animation == Player.AnimationIndex.Roll ? -1 : 1) * (incan.isSlugpup ? 0.7f : 1f);
            sLeaser.sprites[player.tailflameSprite].rotation = Mathf.Rad2Deg * (float)-Math.Atan(tailAngle.y / tailAngle.x);
            sLeaser.sprites[player.tailflameSprite].x = tailflamePos.x;
            sLeaser.sprites[player.tailflameSprite].y = tailflamePos.y;
            sLeaser.sprites[player.tailflameSprite].element = Futile.atlasManager.GetElementWithName("incanTailflame");
            sLeaser.sprites[player.tailflameSprite].color = player.FireColor;

            player.lastTailflamePos = tailflamePos;
        }
        // Tail colors.
        if (sLeaser.sprites[2] is TriangleMesh tail)
        {
            Color baseColor = headSprites.color;
            for (int vertice = 1; vertice < 15; vertice++)
            {
                tail.verticeColors[vertice] = Color.Lerp(baseColor, player.FireColor, 0.1f * vertice);
            }
        }
    }

    public static void SpriteLayering(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (!IncanInfo.IncanData.TryGetValue(self.player, out IncanInfo player) || !player.isIncan)
        {
            return;
        }

        if (sLeaser.sprites.Length > player.cheekFluffSprite + 1)
        {
            FContainer foregroundContainer = rCam.ReturnFContainer("Foreground");
            FContainer midgroundContainer = rCam.ReturnFContainer("Midground");

            /* Any new sprites you create for your Slugcat seem to default to the Foreground layer, making them show... in front of the ground.
             * This code takes your sprites out of the foreground and into the midground, with the rest of Slugcat's sprites.
             * Why are the sprites called CHILDREN? I have NO idea. */
            foregroundContainer.RemoveChild(sLeaser.sprites[player.cheekFluffSprite]);
            foregroundContainer.RemoveChild(sLeaser.sprites[player.waistbandSprite]);
            foregroundContainer.RemoveChild(sLeaser.sprites[player.tailflameSprite]);
            midgroundContainer.AddChild(sLeaser.sprites[player.cheekFluffSprite]);
            midgroundContainer.AddChild(sLeaser.sprites[player.waistbandSprite]);
            midgroundContainer.AddChild(sLeaser.sprites[player.tailflameSprite]);

            /* These lines change how your Slugcat's sprites are layered over each other. The order of these lines is really important, so pay attention to it. */
            /*
            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
            sLeaser.sprites[player.waistbandSprite].MoveToBack();
            sLeaser.sprites[player.waistbandSprite].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
            sLeaser.sprites[player.waistbandSprite].MoveInFrontOfOtherNode(sLeaser.sprites[1]);
            sLeaser.sprites[player.waistbandSprite].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            sLeaser.sprites[player.waistbandSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            sLeaser.sprites[player.tailflameSprite].MoveToBack();
            sLeaser.sprites[player.tailflameSprite].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            */
        }
    }

    // This method is needed if you want any extra colors on your Slugcat to be affected by the Hypothermia mechanic as you get colder.
    // By "extra colors", I mean colors that aren't your Slugcat's Body or Eye colors.
    public static void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (!IncanInfo.IncanData.TryGetValue(self.player, out IncanInfo player) || !player.isIncan)
        {
            return;
        }
        if (self.player.room?.game?.IsArenaSession is null)
        {
            return;
        }

        SetupColors(self.player);

        if (ModManager.CoopAvailable)
        {
            if (Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.DEFAULT)
            {
                if (Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO)
                {
                    PlayerGraphics.PopulateJollyColorArray(self.player.SlugCatClass);
                }
                player.FireColorBase = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
                player.WaistbandColor = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 3);
            }
        }
        if (PlayerGraphics.customColors is not null && !ModManager.CoopAvailable)
        {
            player.FireColorBase = PlayerGraphics.CustomColorSafety(2);
            player.WaistbandColor = PlayerGraphics.CustomColorSafety(3);
        }

        player.FireColor = HypothermiaColorBlendbutforFirebecausetheNormalHypothermiaColorBlenddoesntlookGoodonIt(self, player.FireColorBase);
    }
    public static void SetupColors(Player self)
    {
        //if (ModManager.CoopAvailable && self.IsJollyPlayer) return;
        // Loads default colors from this Slugcat's SlugBase .json file.
        if (!IncanInfo.IncanData.TryGetValue(self, out IncanInfo player) ||
            !SlugBaseCharacter.TryGet(IncanInfo.Incandescent, out player.Incan))
        {
            return;
        }

        if (player.Incan.Features.TryGet(PlayerFeatures.CustomColors, out ColorSlot[] customColors))
        {

            int playerNumber = player.inArena ? self.playerState.playerNumber : -1;


            if (customColors.Length > 3)
            {
                player.FireColorBase = customColors[2].GetColor(playerNumber);
                player.WaistbandColor = customColors[3].GetColor(playerNumber);
            }

            if (player.FireColorBase.Equals(Color.clear))
            {
                if (customColors.Length > 0) player.FireColorBase = customColors[2].GetColor(playerNumber);
                else ColorUtility.TryParseHtmlString("#FB8602", out player.FireColorBase);
            }
            if (player.WaistbandColor.Equals(Color.clear))
            {
                if (customColors.Length > 0) player.WaistbandColor = customColors[3].GetColor(playerNumber);
                else ColorUtility.TryParseHtmlString("#281714", out player.WaistbandColor);
            }
        }

    }
    public static Color HypothermiaColorBlendbutforFirebecausetheNormalHypothermiaColorBlenddoesntlookGoodonIt(PlayerGraphics self, Color oldCol)
    {
        if (!IncanInfo.IncanData.TryGetValue(self.owner as Player, out IncanInfo player) || !player.isIncan)
        {
            return Color.black;
        }

        Player inc = self.owner as Player;
        Color inbetween = Color.Lerp(oldCol * 0.7f, inc.ShortCutColor(), 0.7f);
        Color.RGBToHSV(inc.ShortCutColor(), out float H, out float S, out float V);
        H += (H + 0.15f > 1) ? -0.85f : 0.15f;
        V -= 0.1f;

        Color coldColors =
            inc.Hypothermia < 1f ?
            Color.Lerp(oldCol, inbetween, inc.Hypothermia) :
            Color.Lerp(inbetween, Color.HSVToRGB(H, S, V), (inc.Hypothermia - 1f) * 0.5f);

        return Color.Lerp(oldCol, coldColors, 0.92f);
    }

    /* Okay, this part.
     * There is basically nothing to go off of here to figure out what it does, so I asked someone else about it.
     * Basically, if you recolor your slugcat's tail vertices, like I have, then this method is needed to make it work.
     * I don't know what happens if you exclude it; the game probably just has a stroke and the tail recoloring won't work.
     * HOWEVER, if you don't want to recolor any tail vertices?This is useless for you, so don't worry about it. */
    public static void InitiateSprites_1(ILContext il)
    {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdstr("Futile_White"), i => i.MatchLdloc(0)))
        {
            return;
        }

        cursor.MoveAfterLabels();

        cursor.Remove();
        cursor.Emit(OpCodes.Ldc_I4_1);

    }

    // Makes the Incandescent's tail spin around her when she rolls.
    public static void RollAnimationUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (!IncanInfo.IncanData.TryGetValue(self.owner as Player, out IncanInfo Incan) || !Incan.isIncan)
        {
            return;
        }

        if (self.player.animation == Player.AnimationIndex.Roll)
        {
            for (int i = 1; i < self.tail.Length; i++)
            {
                float startVel = Custom.VecToDeg(Custom.DirVec(self.tail[i].pos, self.tail[i - 1].pos));
                startVel += 45f * -self.player.flipDirection;
                self.tail[i].vel = Custom.DegToVec(startVel) * 15f;
                if (self.player.bodyChunks[0].pos.y >= self.player.bodyChunks[1].pos.y)
                {
                    self.tail[i].vel.x *= 2.20f;
                    self.tail[i].vel.y *= 0.25f;
                }
            }
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------
}