namespace Hailstorm;

public static class IncanExtension
{
    private static readonly ConditionalWeakTable<Player, IncanInfo> _ctwic = new();

    public static IncanInfo Incan(this Player player)
    {
        return player is null ? throw new ArgumentNullException(nameof(player)) : _ctwic.GetValue(player, _ => new IncanInfo(player));
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
