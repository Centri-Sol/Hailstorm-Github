namespace Hailstorm;

public class HailstormVultureSmoke : Smoke.NewVultureSmoke
{

    public Color startColor;
    public Color endColor;

    public bool King;
    public bool Miros;

    public HailstormVultureSmoke(Room room, Vector2 pos, Vulture vul, Color smokeColA, Color smokeColB) : base(room, pos, vul)
    {
        startColor = smokeColA;
        endColor = smokeColB;
        King = vul.IsKing;
        Miros = vul.IsMiros;
    }

    public new void EmitSmoke(Vector2 vel, float power)
    {
        float lifetime = Mathf.Lerp(120f, 200f, Random.value);
        if (Miros)
        {
            lifetime *= 1.5f;
        }
        else if (!King)
        {
            lifetime *= 3f;
        }
        if (AddParticle(pos, vel * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, Random.value), lifetime)) is HailstormVultureSmokeSegment smoke)
        {
            smoke.power = power;
        }
    }
    public override SmokeSystemParticle CreateParticle()
    {
        return new HailstormVultureSmokeSegment(this);
    }

    public class HailstormVultureSmokeSegment : NewVultureSmokeSegment
    {
        public HailstormVultureSmoke creator;

        public HailstormVultureSmokeSegment(HailstormVultureSmoke creator) : base()
        {
            this.creator = creator;
        }

        public override Color MyColor(float timeStacker)
        {
            float lerp = Mathf.InverseLerp(5, 25 + (10f * power), age + timeStacker);
            return Color.Lerp(creator.startColor, creator.endColor, lerp);
        }

        public override float MyRad(float timeStacker)
        {
            float rad = Mathf.Min(Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) + (Mathf.Sin(Mathf.InverseLerp(0.7f, 0f, Mathf.Lerp(lastLife, life, timeStacker)) * Mathf.PI) * 8f), 5f + (25f * power)) * (2f - MyOpactiy(timeStacker));
            if (!creator.Miros)
            {
                rad *= 2f;
            }
            return rad;
        }

    }
}