namespace Hailstorm;

public class RegionInfo
{
    public float freezingFogInsteadOfHailChance;
    public float erraticWindChance;
    public float erraticWindDandelionChance;
    public float erraticWindWrongDandelionTypeChance;
    public int lateBlizzardStartTimeAfterCycleEnds = 1000000;

    public RegionInfo(string regionName, SlugcatStats.Name campaign)
    {
        string[] propertiesText = new string[1] { "" };
        string scugName = "-" + campaign.value;
        string[] propertiesDirectory = new string[7] { "World", null, null, null, null, null, null };
        string directorySeparatorChar = Path.DirectorySeparatorChar.ToString();
        propertiesDirectory[1] = directorySeparatorChar;
        propertiesDirectory[2] = regionName;
        propertiesDirectory[3] = directorySeparatorChar;
        propertiesDirectory[4] = "properties";
        propertiesDirectory[5] = scugName;
        propertiesDirectory[6] = ".txt";
        string path = AssetManager.ResolveFilePath(string.Concat(propertiesDirectory));
        if (!File.Exists(path))
        {
            string[] propertiesDirecTry2 = new string[5] { "World", null, null, null, null };
            propertiesDirecTry2[1] = directorySeparatorChar;
            propertiesDirecTry2[2] = regionName;
            propertiesDirecTry2[3] = directorySeparatorChar;
            propertiesDirecTry2[4] = "properties.txt";
            path = AssetManager.ResolveFilePath(string.Concat(propertiesDirecTry2));
        }
        if (File.Exists(path))
        {
            propertiesText = File.ReadAllLines(path);
        }
        for (int i = 0; i < propertiesText.Length; i++)
        {
            string[] property = Regex.Split(Custom.ValidateSpacedDelimiter(propertiesText[i], ":"), ": ");
            if (property.Length < 2)
            {
                continue;
            }
            switch (property[0])
            {
                case "freezingFogInsteadOfHailChance":
                    freezingFogInsteadOfHailChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindChance":
                    erraticWindChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindDandelionChance":
                    erraticWindDandelionChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "erraticWindWrongDandelionTypeChance":
                    erraticWindWrongDandelionTypeChance = float.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                case "lateBlizzardStartTimeAfterCycleEnds":
                    lateBlizzardStartTimeAfterCycleEnds = int.Parse(property[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                default:
                    break;
            }
        }
    }
}