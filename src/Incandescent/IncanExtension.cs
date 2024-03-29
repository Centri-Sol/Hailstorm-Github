namespace Hailstorm;

public static class IncanExtension
{
    public static readonly ConditionalWeakTable<Player, IncanInfo> _cwtInc = new();

    public static IncanInfo Incan(this Player self)
    {
        if (self is null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        if (_cwtInc.TryGetValue(self, out IncanInfo player))
        {
            return player;
        }

        return _cwtInc.GetValue(self, _ => new IncanInfo(self));
    }

    public static bool IsIncan(this Player player)
    {
        return player.Incan().isIncan;
    }

    public static bool IsIncan(this Player player, out IncanInfo incan)
    {
        incan = player.Incan();
        return incan.isIncan;
    }
}
