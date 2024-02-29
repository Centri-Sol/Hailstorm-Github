namespace Hailstorm;

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
        AddedRad /= Obj.bodyChunks.Length;
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