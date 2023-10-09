using System;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.Diagnostics.CodeAnalysis;
using Fisobs.Core;
using DevInterface;
using Color = UnityEngine.Color;


namespace Hailstorm;

[BepInPlugin(MOD_ID, "The Incandescent", "0.3.0")]
public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "theincandescent";

    [AllowNull] internal static ManualLogSource logger;

    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    // Add hooks.
    public void OnEnable()
    {
        logger = Logger;
        On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

        On.RainWorld.OnModsDisabled += DisableCheck;

        Content.Register(new InfantAquapedeCritob());
        Content.Register(new IcyBlueCritob());
        Content.Register(new FreezerCritob());
        Content.Register(new PeachSpiderCritob());
        Content.Register(new GorditoGreenieCritob());
        Content.Register(new CyanwingCritob());
        Content.Register(new LuminescipedeCritob());
        Content.Register(new ChillipedeCritob());

        Content.Register(new IceCrystalFisob());
        Content.Register(new BurnSpearFisob());

    }
    public void OnDisable() => logger = default;

    // Loads any resources, such as sprites or sounds. A bunch of sprite atlases, for me.
    // I also use this to call for hooks in other .cs files.
    private void LoadResources(RainWorld rainWorld)
    {
        try
        {
            Futile.atlasManager.LoadAtlas("atlases/miscSprites");
            Futile.atlasManager.LoadAtlas("atlases/icons");
            Futile.atlasManager.LoadAtlas("atlases/coloredIcons");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/sadExpressions");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanCheekfluff");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanWaistband");
            Futile.atlasManager.LoadAtlas("atlases/incandescent/incanTailflame");
            Futile.atlasManager.LoadAtlas("atlases/creatures/iceCrystals");
            Futile.atlasManager.LoadAtlas("atlases/creatures/armorIceSpikes");
            Futile.atlasManager.LoadAtlas("atlases/creatures/freezer");
            Futile.atlasManager.LoadAtlas("atlases/creatures/cyanwing");
            Futile.atlasManager.LoadAtlas("atlases/creatures/luminescipede");
            Futile.atlasManager.LoadAtlas("atlases/creatures/chillipede");

            IncanFeatures.Hooks();
            IncanCrafting.Hooks();
            IncanVisuals.Hooks();

            WorldChanges.Hooks();
            Weather.Hooks();
            Dialogue.Hooks();

            OtherCreatureChanges.Hooks();
            HailstormLizards.Hooks();
            HailstormCentis.Hooks();
            HailstormSpiders.Hooks();

            ObjectChanges.Hooks();

            JollyCoopFixes.Hooks();

            MachineConnector.SetRegisteredOI("theincandescent", new HSRemix());

            Debug.Log($"Incan's up and runnin'!");
        }
        catch (Exception ex)
        {
            Logger.LogError("Uh whoops, something's wrong with Hailstorm: " + ex);
            throw;
        }
    }

    //--------------------------------------------

    private void DisableCheck(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "moreslugcats")
            {

                // Creatures
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.InfantAquapedeUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.InfantAquapedeUnlock);              

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.IcyBlueUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.IcyBlueUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.FreezerUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.FreezerUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.PeachSpiderUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.PeachSpiderUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.GorditoGreenieUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.GorditoGreenieUnlock);
                
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.CyanwingUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.CyanwingUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.LuminsecipedeUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.LuminsecipedeUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.ChillipedeUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.ChillipedeUnlock);

                // Items
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.IceCrystalUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.IceCrystalUnlock);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(HailstormEnums.BurnSpearUnlock))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(HailstormEnums.BurnSpearUnlock);

                HailstormEnums.UnregisterValues();
                break;
            }
        }
    }


    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------




    private void SlimeMoldDevToolsControlPanel(On.DevInterface.ConsumableRepresentation.orig_ctor orig, DevInterface.ConsumableRepresentation cRep, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, PlacedObject pObj, string name)
    {
        orig(cRep, owner, IDstring, parentNode, pObj, name);
        if (pObj.type == PlacedObject.Type.SlimeMold)
        {
            cRep.controlPanel = new SlimeMoldControlPanel(owner, "Consumable_Panel", cRep, new Vector2(0f, 100f), "Consumable: " + pObj.type.ToString());
        }
        cRep.subNodes.Clear();
        cRep.fSprites.Clear();
        owner.placedObjectsContainer.RemoveAllChildren();

        cRep.subNodes.Add(cRep.controlPanel);
        cRep.fSprites.Add(new FSprite("pixel", true));
        owner.placedObjectsContainer.AddChild(cRep.fSprites[cRep.fSprites.Count - 1]);
        cRep.fSprites[cRep.fSprites.Count - 1].anchorY = 0f;
    }

    public class SlimeMoldControlPanel : ConsumableRepresentation.ConsumableControlPanel, IDevUISignals
    {

        public Button exitButton;

        public FSprite exitSprite;

        public int exitSpriteIndex;

        public SlimeMoldControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
        {
            size.y += 20f;
            exitButton = new Button(owner, "Void_Spawn_Exit_Button", this, new Vector2(5f, 45f), 240f, "Exit: " + ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit.ToString());
            subNodes.Add(exitButton);
            exitSprite = new FSprite("pixel", true);
            fSprites.Add(exitSprite);
            exitSpriteIndex = fSprites.Count;
            owner.placedObjectsContainer.AddChild(exitSprite);
            exitSprite.anchorY = 0f;
            exitSprite.scaleX = 2f;
            exitSprite.color = new Color(1f, 0f, 0f);
        }

        public override void Refresh()
        {
            base.Refresh();
            exitButton.Text = "Exit: " + ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit.ToString();
            if (exitSprite != null)
            {
                int exit = ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit;
                if (exit < 0 || exit >= owner.room.abstractRoom.connections.Length)
                {
                    exitSprite.isVisible = false;
                    return;
                }
                exitSprite.isVisible = true;
                Vector2 absPos = (parentNode as PositionedDevUINode).absPos;
                Vector2 vector = owner.room.MiddleOfTile(owner.room.ShortcutLeadingToNode(exit).startCoord) - owner.room.game.cameras[0].pos;
                exitSprite.x = absPos.x;
                exitSprite.y = absPos.y;
                exitSprite.rotation = RWCustom.Custom.AimFromOneVectorToAnother(absPos, vector);
                exitSprite.scaleY = Vector2.Distance(absPos, vector);
            }
        }

        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            string idstring = sender.IDstring;
            if (idstring != null && idstring == "Void_Spawn_Exit_Button")
            {
                ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit++;
                if (((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit >= owner.room.abstractRoom.connections.Length)
                {
                    ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit = 0;
                }
            }
            Refresh();
        }

    }

}