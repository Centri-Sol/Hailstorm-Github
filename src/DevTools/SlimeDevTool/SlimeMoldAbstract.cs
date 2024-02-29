namespace Hailstorm;

public class AbstractSlimeMold : AbstractConsumable
{
    public bool big;

    public AbstractSlimeMold(World world, AbstractObjectType objType, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData slmData, bool big)
        : base(world, objType, realizedObject, pos, ID, originRoom, placedObjectIndex, slmData)
    {
        this.big = big;
    }

    public string BaseToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, big ? 1 : 0);
    }

    public override string ToString()
    {
        return SaveUtils.AppendUnrecognizedStringAttrs(BaseToString(), "<oA>", unrecognizedAttributes);
    }
}