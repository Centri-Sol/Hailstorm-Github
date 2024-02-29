namespace Hailstorm;

public class LuminFlock : LuminMass
{
    public LuminFlock(Luminescipede firstLumin, Room room) : base(firstLumin, room)
    {
    }

    public override void Update(bool eu)
    {
        lumins ??= new();
        if (!ShouldIUpdate(eu))
        {
            return;
        }
        base.Update(eu);
        if (room.abstractRoom.creatures.Count == 0)
        {
            return;
        }
        AbstractCreature absCtr = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)];
        if (absCtr.realizedCreature is not null && absCtr.realizedCreature is Luminescipede lmn && lmn.flock is not null && lmn.flock != this && lmn.flock.FirstLumin is not null)
        {
            if (lumins.Count >= lmn.flock.lumins.Count)
            {
                Merge(lmn.flock);
            }
            else
            {
                lmn.flock.Merge(this);
            }
        }
    }
}