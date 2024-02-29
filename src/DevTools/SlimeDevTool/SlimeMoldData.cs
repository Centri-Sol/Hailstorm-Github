namespace Hailstorm;

public class SlimeMoldData : PlacedObject.ConsumableObjectData
{
    public bool big;

    public SlimeMoldData(PlacedObject owner) : base(owner)
    {
    }

    public override void FromString(string s)
    {
        base.FromString(s);
        string[] array = Regex.Split(s, "~");
        if (array.Length >= 5)
        {
            big = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
        }
    }

    public override string ToString()
    {
        return SaveUtils.AppendUnrecognizedStringAttrs(BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}", big ? 1 : 0), "~", unrecognizedAttributes);
    }
}