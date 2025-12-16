using System.Security.Policy;

namespace Hailstorm;

public class Raven : Vulture
{

    public VultureAI RvnAI => AI as VultureAI;
    public VultureState RvnState => State as VultureState;
    public VultureGraphics RvnGraphics => graphicsModule as VultureGraphics;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public Raven(AbstractCreature absRvn, World world) : base(absRvn, world)
    {
        
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);

        TerrainSqueeze();
    }

    public virtual void TerrainSqueeze()
    {
        bool squeeze = room.GetTile(Head().pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance;

        foreach (BodyChunk chunk in bodyChunks)
        {
            if (chunk == Head()) continue;

            if (squeeze && chunk.terrainSqueeze > 0.5f)
            {
                chunk.terrainSqueeze -= 0.01f;
            }
            else
            if (!squeeze && chunk.terrainSqueeze < 1)
            {
                chunk.terrainSqueeze += 0.01f;
            }
        }

    }

}
