namespace Hailstorm;

public abstract class LuminMass
{
    public List<Luminescipede> lumins;
    public bool lastEu;
    public Room room;
    public Color color = Custom.HSL2RGB(Random.value, 1f, 0.5f);

    public virtual Luminescipede FirstLumin => lumins.Count == 0 ? null : lumins[0];

    public LuminMass(Luminescipede firstLumin, Room room)
    {
        this.room = room;
        lumins = new List<Luminescipede> { firstLumin };
    }

    public virtual void Update(bool eu)
    {
        for (int l = lumins.Count - 1; l >= 0; l--)
        {
            if (lumins[l].dead ||
                lumins[l].room != room)
            {
                RemoveLmnAt(l);
            }
        }
    }
    public bool ShouldIUpdate(bool eu)
    {
        if (eu == lastEu)
        {
            return false;
        }
        lastEu = eu;
        return true;
    }

    public void AddLmn(Luminescipede lmn)
    {
        if (lumins.IndexOf(lmn) == -1)
        {
            lumins.Add(lmn);
        }
        if (this is LuminFlock)
        {
            lmn.flock = this as LuminFlock;
            lmn.flock.lumins = new();
        }
    }
    public void RemoveLmn(Luminescipede lmn)
    {
        for (int i = 0; i < lumins.Count; i++)
        {
            if (lumins[i] == lmn)
            {
                RemoveLmnAt(i);
                break;
            }
        }
    }
    private void RemoveLmnAt(int i)
    {
        if (this is LuminFlock &&
            lumins[i].flock == (this as LuminFlock))
        {
            lumins[i].flock = null;
        }
        lumins.RemoveAt(i);
    }
    public void Merge(LuminMass otherFlock)
    {
        if (otherFlock == this)
        {
            return;
        }
        for (int i = 0; i < otherFlock.lumins.Count; i++)
        {
            if (lumins.IndexOf(otherFlock.lumins[i]) == -1)
            {
                lumins.Add(otherFlock.lumins[i]);
                if (this is LuminFlock)
                {
                    otherFlock.lumins[i].flock = this as LuminFlock;
                }
            }
        }
        otherFlock.lumins.Clear();
    }
}