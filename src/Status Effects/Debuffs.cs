namespace Hailstorm;

public abstract class Debuff
{
    public AbstractPhysicalObject source;
    public int? chunk;
    public Creature appendagePos;
    public float duration;

    public Color mainColor;
    public Color secondColor;

    public Debuff(AbstractPhysicalObject inflicter, int? hitChunk, int debuffDuration, Color mainColor, Color secondColor)
    {
        source = inflicter;
        chunk = hitChunk;
        duration = debuffDuration;
        this.mainColor = mainColor;
        this.secondColor = secondColor;
    }

    public virtual void DebuffUpdate(Creature victim, bool setKillTag)
    {

    }

    public virtual void DebuffVisuals(Creature victim, bool eu)
    {

    }
}