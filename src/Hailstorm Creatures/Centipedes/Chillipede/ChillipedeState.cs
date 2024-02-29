namespace Hailstorm;

public class ChillipedeState : Centipede.CentipedeState
{
    public List<Shell> iceShells;
    public Color topShellColor;
    public Color bottomShellColor;

    public ChillipedeState(AbstractCreature absCtr) : base(absCtr)
    {
    }

    public class Shell
    {
        public int index;
        public int[] sprites;

        public int health;
        public int timeToRefreeze;
        public bool justBroke;

        public Shell(int index)
        {
            this.index = index;
        }
    }
}