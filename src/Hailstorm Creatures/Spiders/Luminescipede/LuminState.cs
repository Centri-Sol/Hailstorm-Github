using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using UnityEngine;
using RWCustom;

namespace Hailstorm;

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class GlowSpiderState : HealthState
{
    public class Behavior : ExtEnum<Behavior>
    {
        public static readonly Behavior Idle = new("Idle", false, true);
        public static readonly Behavior Hunt = new("Hunt", false, true);
        public static readonly Behavior Flee = new("Flee", false, true);

        public static readonly Behavior Hide = new("Hide", true, true);
        public static readonly Behavior Rush = new("Rush", true, true);
        public static readonly Behavior Aggravated = new("Aggravated", true, true);
        public static readonly Behavior ReturnPrey = new("ReturnPrey", true, true);
        public static readonly Behavior Overloaded = new("Overloaded", true, true);
        public static readonly Behavior GetUnstuck = new("GetUnstuck", true, true);
        public static readonly Behavior EscapeRain = new("EscapeRain", true, true);

        public bool Stubborn;
        public Behavior(string value, bool stubborn, bool register = false) : base(value, register)
        {
            Stubborn = stubborn;
        }
    }
    public class Role : ExtEnum<Role>
    {
        public static readonly Role Protector = new("Protector", true);
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

    public GlowSpiderState(AbstractCreature absLmn) : base (absLmn)
    {
        juice = 1;
        behavior = Behavior.Idle;
        stateTimeLimit = -1;
        Random.State rState = Random.state;
        Random.InitState(absLmn.ID.RandomSeed);
        switch (Random.value)
        {
            case < 0.25f:
                role = Role.Forager;
                break;
            case < 0.55f:
                role = Role.Hunter;
                break;
            default:
                role = Role.Protector;
                break;
        }
        ivars = new IndividualVariations(Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));
        dominant = ivars.dominance > 1.2f;
        if (role == Role.Hunter)
        {
            timeToWantToHide = !dominant ? 600 : 400;
            timeToHide = !dominant ? 160 : 40;
        }
        else if (role != Role.Forager)
        {
            timeToWantToHide = !dominant ? 1000 : 1600;
            timeToHide = 320;
        }
        role = Role.Forager;
        dominant = true;
        Random.state = rState;
    }

    public virtual void Reset()
    {
        timeSincePreyLastSeen = 0;
        darknessCounter = 0;
        rushPreyCounter = 0;
        behavior = Behavior.Idle;
        suppressedState = null;
        stateTimeLimit = -1;
    }

    //-----------------------------------------

    public virtual void Update(LuminCreature lmn, bool eu)
    {
        if (Stunned != lmn.Stunned)
        {
            Stunned = lmn.Stunned;
        }

        if (dead)
        {
            if (timeSincePreyLastSeen != 0 || darknessCounter != 0 || rushPreyCounter != 0 || behavior != Behavior.Idle || suppressedState is not null || stateTimeLimit != -1)
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
                Behavior newState = Behavior.Idle;
                if (suppressedState is not null)
                {
                    newState = suppressedState;
                }
                else if (behavior == Behavior.Aggravated)
                {
                    newState = Behavior.Hunt;
                }
                ChangeBehavior(newState, true);
            }
        }

        if (lmn.sourceOfFear is null && lmn.currentPrey is null && (behavior == Behavior.Hunt || behavior == Behavior.Flee))
        {
            ChangeBehavior(Behavior.Idle, false);
        }

    }
    public virtual void ChangeBehavior(Behavior newState, bool forceChange)
    {
        if (newState is null || newState == behavior || (dead && newState != Behavior.Idle) || (behavior == Behavior.Overloaded && Stunned))
        {
            return;
        }
        if (!dead && !forceChange && behavior.Stubborn)
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

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------