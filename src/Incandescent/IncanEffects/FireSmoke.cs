namespace Hailstorm;

public class HailstormFireSmokeCreator : Smoke.FireSmoke
{

    public HailstormFireSmokeCreator(Room room) : base(room)
    {
    }

    public override SmokeSystemParticle CreateParticle()
    {
        return new HailstormFireSmoke();
    }

    public class HailstormFireSmoke : FireSmokeParticle
    {

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

    }
}