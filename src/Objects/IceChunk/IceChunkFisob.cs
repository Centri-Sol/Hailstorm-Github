namespace Hailstorm;

public class IceChunkFisob : IceFisobTemplate
{
    public override Color IceColor => Custom.HSL2RGB(180 / 360f, 0.06f, 0.55f);

    internal IceChunkFisob() : base(HSEnums.AbstractObjectType.IceChunk, HSEnums.SandboxUnlock.IceChunk, MultiplayerUnlocks.SandboxUnlockID.Slugcat)
    {
        Icon = new SimpleIcon("Icon_Ice_Chunk", IceColor);
    }

}