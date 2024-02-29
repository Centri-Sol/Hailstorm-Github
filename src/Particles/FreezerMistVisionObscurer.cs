namespace Hailstorm;

public class FreezerMistVisionObscurer : VisionObscurer
{
    private float progress;
    public float lifeTime;
    public FreezerMistVisionObscurer(Vector2 pos, float rad, float fullObscureDist, float obscureFac, float lifetime) : base(pos, rad, fullObscureDist, obscureFac)
    {
        lifeTime = lifetime;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        progress += 1 / lifeTime;
        obscureFac = Mathf.InverseLerp(1, 0.3f, progress - 0.5f);
        rad = Mathf.Lerp(70, 140, Mathf.Pow(progress, 0.5f));
        if (progress > 1)
        {
            Destroy();
        }
    }
}