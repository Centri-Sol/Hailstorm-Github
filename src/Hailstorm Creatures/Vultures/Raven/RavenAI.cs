namespace Hailstorm;

public class RavenAI : VultureAI
{
    public Raven Rvn => vulture as Raven;
    public Vulture.VultureState RvnState => creature.state as Vulture.VultureState;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public RavenAI(AbstractCreature absRvn, World world) : base(absRvn, world)
    {

    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (pathFinder is null ||
            pathFinder is not VulturePather pather)
        {
            return;
        }

        MovementConnection connection = pather.FollowPath(Rvn.room.GetWorldCoordinate(Rvn.mainBodyChunk.pos), true);
        if (connection != default)
        {
            PipeTravel(connection);
        }
    }

    public virtual void PipeTravel(MovementConnection connection)
    {
        if (Rvn.shortcutDelay > 0)
        {
            return;
        }

        if (connection.type == MovementConnection.MovementType.ShortCut ||
            connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze ||
            connection.type == MovementConnection.MovementType.NPCTransportation)
        {
            Rvn.enteringShortCut = connection.StartTile;

            if (connection.type == MovementConnection.MovementType.NPCTransportation)
            {
                Rvn.NPCTransportationDestination = connection.destinationCoord;
            }

        }

    }

}
