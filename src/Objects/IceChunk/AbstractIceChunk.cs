using UnityEngine;
using RWCustom;
using Fisobs.Core;

namespace Hailstorm;

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

public class AbstractIceChunk : AbstractPhysicalObject
{
    public IceChunk realIce;

    public int sprite;
    public Color color1;
    public Color color2;

    public float size = 1f;
    public float freshness;

    public FrozenObject frozenObject;

    public AbstractIceChunk(World world, WorldCoordinate pos, EntityID ID, AbstractObjectType objType = null) : base(world, objType ?? HailstormItems.IceChunk, null, pos, ID)
    {
        bool isFreezerCrystal =
            objType is not null &&
            objType == HailstormItems.FreezerCrystal;

        Random.State state = Random.state;
        Random.InitState(ID.RandomSeed);
        sprite = Random.Range(0, 6);
        HSLColor col = new
        (
            isFreezerCrystal ? Custom.WrappedRandomVariation(220/360f, 40/360f, 0.35f) : Custom.WrappedRandomVariation(180/360f, 40/360f, 1f),
            isFreezerCrystal ? 0.60f : 0.06f,
            isFreezerCrystal ? Custom.ClampedRandomVariation(0.75f, 0.05f, 0.2f) : 0.55f
        );
        Random.state = state;

        color1 = col.rgb;
        color2 = new HSLColor
        (
            isFreezerCrystal ? col.hue * ((col.hue * 1.2272f > 0.75f) ? 0.7728f : 1.2272f) : col.hue,
            isFreezerCrystal ? col.saturation : col.saturation - 0.02f,
            col.lightness - 0.1f
        ).rgb;
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject is null)
        {
            realizedObject = new IceChunk(this, world);
        }
    }

    //--------------------------------------------------------------------------------

    public override void Update(int time)
    {
        base.Update(time);
        if (freshness != 0)
        {
            freshness = Mathf.Max(freshness - 1/2400f, 0);
            if (realIce is not null & Random.value < 0.03f * Mathf.Min(1, freshness))
            {
                InsectCoordinator smallInsects = null;
                if (realIce.room is not null)
                {
                    Room room = realIce.room;
                    for (int i = 0; i < room.updateList.Count; i++)
                    {
                        if (room.updateList[i] is InsectCoordinator)
                        {
                            smallInsects = room.updateList[i] as InsectCoordinator;
                            break;
                        }
                    }
                }
                realIce.EmitFreezerMist(realIce.firstChunk.pos, Custom.RNV() * 10f * Random.value, Mathf.Lerp(0.5f, 1.25f, freshness), smallInsects, true);
            }
        }
    }

    //--------------------------------------------------------------------------------

    public override string ToString()
    {
        string saveData = $"{size};";
        saveData += $"{sprite};";
        saveData += $"{color1.r};{color1.g};{color1.b};";
        saveData += $"{color2.r};{color2.g};{color2.b};";
        if (frozenObject.obj is not null)
        {
            saveData += $"{frozenObject.obj}";
        }
        return this.SaveToString(saveData);
    }
}

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

public struct FrozenObject
{
    public AbstractPhysicalObject obj;

    public float TotalMass;
    public float AddedRad;

    public float waterFriction;
    public float airFriction;

    public float buoyancy;
    public float bounce;

    public FrozenObject(AbstractPhysicalObject absObj)
    {
        if (absObj is null)
        {
            return;
        }

        obj = absObj;

        bool wasNull = false;
        if (obj.realizedObject is null)
        {
            wasNull = true;
            obj.Realize();
        }
        PhysicalObject Obj = obj.realizedObject;

        TotalMass = Obj.TotalMass;
        for (int i = 0; i < Obj.bodyChunks.Length; i++)
        {
            AddedRad += Obj.bodyChunks[i].rad;
        }
        AddedRad /= (float)Obj.bodyChunks.Length;
        waterFriction = Obj.waterFriction;
        airFriction = Obj.airFriction;
        buoyancy = Obj.buoyancy;
        bounce = Obj.bounce;

        if (wasNull)
        {
            obj.realizedObject = null;
        }

    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------