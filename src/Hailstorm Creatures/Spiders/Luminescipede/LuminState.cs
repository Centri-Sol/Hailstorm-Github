namespace Hailstorm;

public class GlowSpiderState : HealthState
{
    public class Behavior : ExtEnum<Behavior>
    {
        public static readonly Behavior Idle = new("Idle", 0, true);
        public static readonly Behavior Hunt = new("Hunt", 0, true);
        public static readonly Behavior Flee = new("Flee", 0, true);

        public static readonly Behavior Hide = new("Hide", 1, true);
        public static readonly Behavior Rush = new("Rush", 1, true);
        public static readonly Behavior Aggravated = new("Aggravated", 1, true);
        public static readonly Behavior ReturnPrey = new("ReturnPrey", 1, true);
        public static readonly Behavior EscapeRain = new("EscapeRain", 1, true);

        public static readonly Behavior Overloaded = new("Overloaded", 2, true);

        public int Stubborness;
        public Behavior(string value, int stubborness, bool register = false) : base(value, register)
        {
            Stubborness = stubborness;
        }
    }
    public class Role : ExtEnum<Role>
    {
        public static readonly Role Guardian = new("Protector", true);
        public static readonly Role Hunter = new("Hunter", true);
        public static readonly Role Forager = new("Forager", true);
        public Role(string value, bool register = false) : base(value, register)
        {

        }
    }


    public Behavior behavior;
    public Behavior suppressedState;
    public int stateTimeLimit;

    public Role role;

    public float MaxJuice => 1.5f;
    public float juice;
    public bool Stunned;

    public int timeSincePreyLastSeen;
    public int timeToWantToHide;
    public int timeToHide;
    public int darknessCounter;
    public int rushPreyCounter;

    public bool dominant;

    //-----------------------------------------

    public struct IndividualVariations
    {

        public float Size;
        public float Fatness;
        public float dominance;

        public int SparkType;

        public IndividualVariations(float size, float fatness)
        {
            Size = size;
            Fatness = fatness;
            dominance = (size + Random.Range(-0.2f, 0.2f)) * Mathf.Max(0, 1f - Mathf.Abs(fatness - 1));

            SparkType = Random.Range(0, 21);
        }
    }
    public IndividualVariations ivars;

    public GlowSpiderState(AbstractCreature absLmn) : base(absLmn)
    {
        juice = 1;
        behavior = Idle;
        stateTimeLimit = -1;
        Random.State rState = Random.state;
        Random.InitState(absLmn.ID.RandomSeed);
        role = Random.value switch
        {
            < 0.25f => Forager,
            < 0.55f => Hunter,
            _ => Guardian,
        };
        ivars = new IndividualVariations(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));
        dominant = ivars.dominance > 1.2f;
        if (role == Guardian)
        {
            timeToWantToHide = !dominant ? 1000 : 1600;
            timeToHide = 320;
        }
        else if (role == Forager)
        {
            timeToWantToHide = 12000;
            timeToHide = 200;
        }
        role = Forager;
        dominant = true;
        Random.state = rState;
    }

    public virtual void Reset()
    {
        timeSincePreyLastSeen = 0;
        darknessCounter = 0;
        rushPreyCounter = 0;
        behavior = Idle;
        suppressedState = null;
        stateTimeLimit = -1;
    }

    //-----------------------------------------

    public virtual void Update(Luminescipede lmn, bool eu)
    {
        if (Stunned != lmn.Stunned)
        {
            Stunned = lmn.Stunned;
        }

        if (dead)
        {
            if (timeSincePreyLastSeen != 0 || darknessCounter != 0 || rushPreyCounter != 0 || behavior != Idle || suppressedState is not null || stateTimeLimit != -1)
            {
                Reset();
            }
            return;
        }

        if (stateTimeLimit > -1)
        {
            stateTimeLimit--;
            if (stateTimeLimit == 0)
            {
                Behavior newState = Idle;
                if (suppressedState is not null)
                {
                    newState = suppressedState;
                }
                else if (behavior == Aggravated)
                {
                    newState = Hunt;
                }
                ChangeBehavior(newState, 1);
            }
        }

    }
    public virtual void ChangeBehavior(Behavior newState, int stubborness)
    {
        if (newState is null || newState == behavior || (dead && newState != Idle) || (behavior == Overloaded && Stunned))
        {
            return;
        }
        if (!dead && behavior.Stubborness > stubborness)
        {
            suppressedState = newState;
            return;
        }
        behavior = newState;
        if (stateTimeLimit > -1)
        {
            stateTimeLimit = -1;
        }
    }

    //-----------------------------------------

}