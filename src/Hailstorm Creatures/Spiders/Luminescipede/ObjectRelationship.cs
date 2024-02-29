namespace Hailstorm;

public struct ObjectRelationship
{
    public class Type : ExtEnum<Type>
    {
        public static readonly Type DoesntTrack = new("DoesntTrack", register: true);
        public static readonly Type Ignores = new("Ignores", register: true);
        public static readonly Type Eats = new("Eats", register: true);
        public static readonly Type Uses = new("Uses", register: true);
        public static readonly Type Likes = new("Likes", register: true);
        public static readonly Type Attacks = new("Attacks", register: true);
        public static readonly Type UncomfortableAround = new("UncomfortableAround", register: true);
        public static readonly Type Avoids = new("Avoids", register: true);
        public static readonly Type AfraidOf = new("AfraidOf", register: true);
        public static readonly Type PlaysWith = new("PlaysWith", register: true);

        public Type(string value, bool register = false)
            : base(value, register)
        {
        }
    }

    public Type type;

    public float intensity;

    public ObjectRelationship(Type type, float intensity)
    {
        this.type = type;
        this.intensity = intensity;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is not null && obj is ObjectRelationship relationship && Equals(relationship);
    }

    public readonly bool Equals(ObjectRelationship relationship)
    {
        return type == relationship.type && intensity == relationship.intensity;
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(ObjectRelationship a, ObjectRelationship b)
    {
        return a.type == b.type && a.intensity == b.intensity;
    }

    public static bool operator !=(ObjectRelationship a, ObjectRelationship b)
    {
        return !(a == b);
    }

    public readonly ObjectRelationship Duplicate()
    {
        return new ObjectRelationship(type, intensity);
    }

    public override readonly string ToString()
    {
        return type.ToString() + " " + intensity;
    }
}