using System.Globalization;
using UnityEngine;
using RWCustom;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using Fisobs.Properties;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class IceFisobTemplate : Fisob
{
    public virtual Color IceColor => Custom.HSL2RGB(211/360f, 1, 0.8f);

    internal IceFisobTemplate(AbstractPhysicalObject.AbstractObjectType objectType, MultiplayerUnlocks.SandboxUnlockID unlockType, MultiplayerUnlocks.SandboxUnlockID parentUnlock) : base(objectType)
    {
        if (Icon is null)
        {
            Icon = new SimpleIcon("Icon_Ice_Chunk", IceColor);
        }
        SandboxPerformanceCost = new(0.3f, 0f);
        RegisterUnlock(unlockType, parent: parentUnlock);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
    {
        string[] saveString = saveData.CustomData.Split(';');
        if (saveString.Length < 10)
        {
            saveString = new string[10];
        }

        AbstractIceChunk absIce = new(world, saveData.Pos, saveData.ID, Type);


        if (float.TryParse(saveString[0], out float size))
        {
            absIce.size = size;
        }
        if (int.TryParse(saveString[1], out int sprite))
        {
            absIce.sprite = sprite;
        }

        Color col = Color.black;
        if (float.TryParse(saveString[2], out col.r) &&
            float.TryParse(saveString[3], out col.g) &&
            float.TryParse(saveString[4], out col.b))
        {
            absIce.color1 = col;
        }
        if (float.TryParse(saveString[5], out col.r) &&
            float.TryParse(saveString[6], out col.g) &&
            float.TryParse(saveString[7], out col.b))
        {
            absIce.color2 = col;
        }

        if (saveString[8] is not null &&
            saveString[8].Length > 0)
        {
            absIce.frozenObject = new FrozenObject(SaveState.AbstractPhysicalObjectFromString(world, saveString[9]));
        }

        return absIce;
    }

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return new IceProperties(forObject);
    }

    public class IceProperties : ItemProperties
    {
        public IceProperties(PhysicalObject forObject)
        {

        }

        public override void Throwable(Player player, ref bool throwable)
        {
            throwable = true;
        }
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
        public override void ScavCollectScore(Scavenger scav, ref int score)
        {
            score = 2;
        }
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType != ScavengerAI.ViolenceType.Lethal)
            {
                score = 0;
            }
            else
            {
                score = 2;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk)
                    {
                        score++;
                    }
                }
            }
        }
        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.NonLethal)
            {
                score = 1;
            }
            else if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
            {
                score = 2;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk)
                    {
                        score++;
                    }
                }
            }
        }
        public override void LethalWeapon(Scavenger scav, ref bool isLethal)
        {
            isLethal = true;
        }
    }

}

//--------------------------------------------------------------------------------

sealed class IceChunkFisob : IceFisobTemplate
{
    public override Color IceColor => Custom.HSL2RGB(180/360f, 0.06f, 0.55f);

    internal IceChunkFisob() : base(HailstormItems.IceChunk, HailstormUnlocks.IceChunk, MultiplayerUnlocks.SandboxUnlockID.Slugcat)
    {
        Icon = new SimpleIcon("Icon_Ice_Chunk", IceColor);
    }

}

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

public class FreezerCrystalFisob : IceFisobTemplate
{
    public override Color IceColor => Custom.HSL2RGB(211 / 360f, 1, 0.8f);

    internal FreezerCrystalFisob() : base(HailstormItems.FreezerCrystal, HailstormUnlocks.FreezerCrystal, HailstormUnlocks.Freezer)
    {
        Icon = new SimpleIcon("Icon_Freezer_Crystal", IceColor);
    }

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        return new FreezerCrystalPropoerties(forObject);
    }

    public class FreezerCrystalPropoerties : ItemProperties
    {
        public FreezerCrystalPropoerties(PhysicalObject forObject)
        {

        }

        public override void Throwable(Player player, ref bool throwable)
        {
            throwable = true;
        }
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
        public override void ScavCollectScore(Scavenger scav, ref int score)
        {
            score = 4;
        }
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType != ScavengerAI.ViolenceType.Lethal)
            {
                score = 0;
            }
            else
            {
                score = 3;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk ice &&
                        ice.FreezerCrystal)
                    {
                        score++;
                    }
                }
            }
        }
        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.NonLethal)
            {
                score = 0;
            }
            else if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
            {
                score = 4;
                for (int i = 0; i < scav.grasps.Length; i++)
                {
                    if (scav.grasps[i]?.grabbed is not null &&
                        scav.grasps[i].grabbed is IceChunk ice &&
                        ice.FreezerCrystal)
                    {
                        score++;
                    }
                }
            }
        }
        public override void LethalWeapon(Scavenger scav, ref bool isLethal)
        {
            isLethal = true;
        }
    }

}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------