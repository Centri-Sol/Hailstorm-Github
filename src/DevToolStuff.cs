/*
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hailstorm;

internal class DevToolStuff
{
    public static PlacedObject.Type EnlargeableSlimeMold = new("EnlargeableSlimeMold", false);
    public static void BigMold(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject obj)
    {
        orig(obj);
        if (ModManager.MSC && obj.type == EnlargeableSlimeMold)
        {
            obj.data = new SlimeMoldData(obj);
            return;
        }
    }

    public static void SlimeMoldDevToolSetup(On.DevInterface.ConsumableRepresentation.orig_ctor orig, DevInterface.ConsumableRepresentation rep, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, PlacedObject pObj, string name)
    {
        orig(rep, owner, IDstring, parentNode, pObj, name);
        if (pObj.type == EnlargeableSlimeMold)
        {
            rep.subNodes.Clear();
            owner.placedObjectsContainer.RemoveAllChildren();
            rep.fSprites.Clear();
            rep.controlPanel = null;

            rep.controlPanel = new SlimeMoldControlPanel(owner, "Consumable_Panel", rep, new Vector2(0f, 100f), "Consumable: " + pObj.type.ToString());
            rep.subNodes.Add(rep.controlPanel);
            rep.controlPanel.pos = (pObj.data as PlacedObject.ConsumableObjectData).panelPos;
            rep.fSprites.Add(new FSprite("pixel", true));
            owner.placedObjectsContainer.AddChild(rep.fSprites[rep.fSprites.Count - 1]);
            rep.fSprites[rep.fSprites.Count - 1].anchorY = 0f;
        }
    }

    public static void HailstormSlimeMoldRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage objPage, PlacedObject.Type type, PlacedObject pObj)
    {
        orig(objPage, type, pObj);

        DevInterface.PlacedObjectRepresentation placedObjectRepresentation = null;
        if (ModManager.MSC && type == EnlargeableSlimeMold)
        {
            placedObjectRepresentation = new DevInterface.ConsumableRepresentation(objPage.owner, type.ToString() + "_Rep", objPage, pObj, type.ToString());
        }

        if (placedObjectRepresentation is not null)
        {
            objPage.tempNodes.Add(placedObjectRepresentation);
            objPage.subNodes.Add(placedObjectRepresentation);
        }
    }

    public static DevInterface.ObjectsPage.DevObjectCategories HailstormSlimeMoldDevtoolsCategory(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage objPage, PlacedObject.Type type)
    {
        if (ModManager.MSC)
        {
            if (type == EnlargeableSlimeMold)
            {
                return DevInterface.ObjectsPage.DevObjectCategories.Consumable;
            }
        }
        return orig(objPage, type);
    }
}

public class SlimeMoldControlPanel : DevInterface.ConsumableRepresentation.ConsumableControlPanel, DevInterface.IDevUISignals
{
    public DevInterface.Button slmSizeButton;

    public DevInterface.ConsumableRepresentation consumRep;

    public SlimeMoldControlPanel(DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
    {
        consumRep = parentNode as DevInterface.ConsumableRepresentation;
        size.y += 20f;
        slmSizeButton = new DevInterface.Button(owner, "Slime_Mold_Size_Button", this, new Vector2(5f, 45f), 240f, "Size: " + ((consumRep.pObj.data as SlimeMoldData).big ? "Big" : "Normal"));
        subNodes.Add(slmSizeButton);
    }

    public override void Refresh()
    {
        base.Refresh();
        slmSizeButton.Text = "Size: " + ((consumRep.pObj.data as SlimeMoldData).big ? "Big" : "Normal");
    }

    public void Signal(DevInterface.DevUISignalType type, DevInterface.DevUINode sender, string message)
    {
        string iDstring = sender.IDstring;
        if (iDstring != null && iDstring == "Slime_Mold_Size_Button")
        {
            (consumRep.pObj.data as SlimeMoldData).big = !(consumRep.pObj.data as SlimeMoldData).big;
        }
        Refresh();
    }
}
*/