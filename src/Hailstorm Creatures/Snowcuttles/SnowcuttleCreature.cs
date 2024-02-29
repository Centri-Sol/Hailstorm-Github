namespace Hailstorm;

public class SnowcuttleCreature : InsectoidCreature
{
    public readonly bool? gender;
    public CicadaAI AI;
    public struct IndividualVariations
    {
        public SnowcuttleCreature owner;

        public HSLColor color;
        public float fatness;

        public float tentacleLength;
        public float tentacleThickness;

        public float wingThickness;
        public float wingLength;
        public float wingSoundPitch;
        public float defaultWingDeployment;
        public int bustedWing;

        public IndividualVariations(SnowcuttleCreature Snwctl, float defaultWingDeployment, float tentacleLength, float tentacleThickness, float wingThickness, float wingLength, int bustedWing)
        {
            GenerateColor(Snwctl);
            GenerateFatness(Snwctl);

            this.tentacleLength = tentacleLength;
            this.tentacleThickness = tentacleThickness;

            this.wingThickness = wingThickness;
            this.wingLength = wingLength;
            GenerateWingPitch(Snwctl);
            this.defaultWingDeployment = defaultWingDeployment;
            this.bustedWing = bustedWing;
        }

        public void GenerateColor(SnowcuttleCreature Snwctl)
        {
            float hue;
            switch (Snwctl.gender)
            {
                case null: // Le
                    hue = Custom.WrappedRandomVariation(260 / 360f, 15 / 360f, 0.3f);
                    break;
                case true: // Female
                    hue = Custom.WrappedRandomVariation(320 / 360f, 15 / 360f, 0.3f);
                    break;
                case false: // Male
                    hue = Custom.WrappedRandomVariation(200 / 360f, 15 / 360f, 0.3f);
                    break;
            }

            color = new HSLColor(hue, Custom.WrappedRandomVariation(0.7f, 0.15f, 0.3f), Random.Range(0.5f, 0.6f));
        }
        public void GenerateFatness(SnowcuttleCreature Snwctl)
        {
            switch (Snwctl.gender)
            {
                case null: // Le
                    fatness = Custom.WrappedRandomVariation(0.45f, 0.15f, 0.5f) * 2f;
                    break;
                case true: // Female
                    fatness = Custom.WrappedRandomVariation(0.475f, 0.1f, 0.33f) * 2f;
                    break;
                case false: // Male
                    fatness = Custom.WrappedRandomVariation(0.425f, 0.075f, 0.25f) * 2f;
                    break;
            }
        }
        public void GenerateWingPitch(SnowcuttleCreature Snwctl)
        {
            wingSoundPitch = 2f - fatness;
        }
    }

    // - - - - - - - - - - - - - - - - - - - -

    public float sinCounter;

    public bool flying;

    public int waitToFlyCounter;

    public float flyingPower;

    private int flipH;

    public int chargeCounter;

    public Vector2 chargeDir;

    public BodyChunk stickyCling;

    public int noStickyCounter;

    public int cantPickUpCounter;

    public Player cantPickUpPlayer;

    public IndividualVariations iVars;

    public IntVector2 sitDirection;

    public float stamina = 1f;

    public float struggleAgainstPlayer;

    public bool currentlyLiftingPlayer;

    public float playerJumpBoost;

    private bool WantToSitDownAtDestination => AI.behavior == CicadaAI.Behavior.Idle && AI.pathFinder.GetDestination.room == room.abstractRoom.index
&& Climbable(AI.pathFinder.GetDestination.Tile);

    public bool AtSitDestination => WantToSitDownAtDestination && Custom.ManhattanDistance(abstractCreature.pos, AI.pathFinder.GetDestination) < 2
&& Climbable(AI.pathFinder.GetDestination.Tile);

    public bool Charging => chargeCounter > 21;

    public float LiftPlayerPower => Custom.SCurve(stamina, 0.15f) * (0.4f + (playerJumpBoost * 0.6f));

    private void GenerateIVars(bool? gender)
    {
        if (gender is null)
        {
            throw new ArgumentNullException(nameof(gender));
        }

        Random.State state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        int bustedWing = -1;
        if (Random.value < 0.125f)
        {
            bustedWing = Random.Range(0, 4);
        }
        iVars = new IndividualVariations(this, Random.value, Mathf.Lerp(0.6f, 1.4f, Random.value), Mathf.Lerp(0.6f, 1.4f, Random.value), Mathf.Lerp(1f, 0.4f, Random.value * Random.value), Custom.ClampedRandomVariation(0.66667f, 0.3f, 0.2f) * 1.5f, bustedWing);
        Random.state = state;
    }

    public SnowcuttleCreature(AbstractCreature absSnwctl, World world) : base(absSnwctl, world)
    {
        if (absSnwctl.creatureTemplate.type == HSEnums.CreatureType.SnowcuttleFemale)
        {
            gender = true;
        }
        else if (absSnwctl.creatureTemplate.type == HSEnums.CreatureType.SnowcuttleMale)
        {
            gender = false;
        }
        GenerateIVars(gender);
        float num = gender.HasValue ? 0.65f : 0.55f;
        bodyChunks = new BodyChunk[2];
        bodyChunks[0] = new BodyChunk(this, 0, default, 3.5f, num / 2f);
        bodyChunks[1] = new BodyChunk(this, 1, default, 3.5f, num / 2f);
        bodyChunkConnections = new BodyChunkConnection[1];
        bodyChunkConnections[0] = new BodyChunkConnection(bodyChunks[0], bodyChunks[1], 7f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.1f;
        surfaceFriction = 0.4f;
        collisionLayer = 1;
        waterFriction = 0.96f;
        buoyancy = 0.95f;
        sinCounter = Random.value;
        flipH = Random.value >= 0.5f ? 1 : -1;
        flying = true;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new CicadaGraphics(this);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is null)
        {
            return;
        }
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            mainBodyChunk.vel += Custom.DirVec(mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (noStickyCounter > 0)
        {
            noStickyCounter--;
        }
        if (chargeCounter == 0 && cantPickUpCounter > 0)
        {
            cantPickUpCounter--;
        }
        if ((State as HealthState).health < 0.5f && Random.value > (State as HealthState).health && Random.value < 1f / 3f)
        {
            Stun(4);
            if ((State as HealthState).health <= 0f && Random.value < 0.25f)
            {
                Die();
            }
        }
        if (Consious)
        {
            bool flag = false;
            for (int i = 0; i < grabbedBy.Count; i++)
            {
                if (grabbedBy[i].grabber is Player)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                GrabbedByPlayer();
            }
            else if (Submersion > 0.5f)
            {
                Swim();
            }
            else
            {
                Act(GetBodyChunks());
            }
        }
        else
        {
            stickyCling = null;
            cantPickUpCounter = 0;
            stamina = 0f;
        }
        if (!flying)
        {
            cantPickUpCounter = 0;
        }
        if (grasps[0] != null)
        {
            CarryObject();
        }
    }

    private void Swim()
    {
        mainBodyChunk.vel.y += 0.5f;
    }

    private BodyChunk[] GetBodyChunks()
    {
        return bodyChunks;
    }

    private void Act(BodyChunk[] bodyChunks)
    {
        AI.Update();

        if (grabbedBy.Count == 0 && stickyCling is null)
        {
            stamina = Mathf.Min(stamina + (1f / 70f), 1f);
        }
        MovementConnection movementConnection = null;
        if ((flying || !AtSitDestination) && chargeCounter == 0 && !AI.swooshToPos.HasValue)
        {
            movementConnection = (AI.pathFinder as CicadaPather).FollowPath(room.GetWorldCoordinate(mainBodyChunk.pos), actuallyFollowingThisPath: true);
        }
        if (safariControlled && (movementConnection is not null || !AllowableControlledAIOverride(movementConnection.type)))
        {
            movementConnection = null;
            if (inputWithDiagonals.HasValue)
            {
                MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                if (room.GetTile(mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                {
                    type = MovementConnection.MovementType.ShortCut;
                }
                if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && chargeCounter == 0)
                {
                    movementConnection = new MovementConnection(type, room.GetWorldCoordinate(mainBodyChunk.pos), room.GetWorldCoordinate(mainBodyChunk.pos + (new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f)), 2);
                }
                if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp && chargeCounter == 0)
                {
                    if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
                    {
                        Charge(mainBodyChunk.pos + (new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f));
                    }
                    else
                    {
                        Charge(mainBodyChunk.pos + ((graphicsModule as CicadaGraphics).lookDir * 40f));
                    }
                }
                if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
                {
                    for (int i = 0; i < grasps.Length; i++)
                    {
                        ReleaseGrasp(i);
                    }
                }
                if (inputWithDiagonals.Value.pckp && room is not null && (grasps.Length == 0 || grasps[0] is null || grasps[0].grabbed is null))
                {
                    for (int j = 0; j < room.physicalObjects.Length; j++)
                    {
                        for (int k = 0; k < room.physicalObjects[j].Count; k++)
                        {
                            if ((room.physicalObjects[j][k] is Fly || room.physicalObjects[j][k] is Leech) && Custom.DistLess(mainBodyChunk.pos, (room.physicalObjects[j][k] as Creature).mainBodyChunk.pos, 50f))
                            {
                                TryToGrabPrey(room.physicalObjects[j][k]);
                            }
                        }
                    }
                }
                GoThroughFloors = inputWithDiagonals.Value.y < 0;
            }
        }
        if (flying)
        {
            sinCounter += 1f / Mathf.Lerp(45f, 85f, Random.value);
            if (sinCounter > 1f)
            {
                sinCounter -= 1f;
            }
            mainBodyChunk.vel.y += Mathf.Sin(sinCounter * Mathf.PI * 2f) * 0.05f * flyingPower * stamina;
            bodyChunks[1].vel.y += Mathf.Sin(sinCounter * Mathf.PI * 2f) * 0.05f * flyingPower * stamina;
            mainBodyChunk.vel *= Mathf.Lerp(1f, 0.98f, flyingPower * stamina);
            bodyChunks[1].vel *= Mathf.Lerp(1f, 0.94f, flyingPower * stamina);
            if (!safariControlled)
            {
                bodyChunks[0].vel.y += 0.8f * flyingPower * stamina;
                bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1.2f) * flyingPower * stamina;
            }
            else
            {
                base.bodyChunks[0].vel.y += 0.8f * flyingPower * stamina;
                if (!inputWithDiagonals.HasValue || (inputWithDiagonals.Value.x == 0 && inputWithDiagonals.Value.y == 0))
                {
                    bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1f) * flyingPower * stamina;
                }
                else
                {
                    bodyChunks[1].vel.y += (AtSitDestination ? 0.5f : 1.2f) * flyingPower * stamina;
                }
            }
            bool flag = false;
            if (movementConnection == null || Climbable(movementConnection.DestTile) || Climbable(room.GetTilePosition(mainBodyChunk.pos)))
            {
                if (room.aimap.getAItile(bodyChunks[0].pos).narrowSpace)
                {
                    flag = true;
                }
                else if (room.aimap.getAItile(bodyChunks[0].pos).terrainProximity == 1 && room.aimap.getAItile(bodyChunks[1].pos).terrainProximity == 1 && (movementConnection == null || room.aimap.getAItile(movementConnection.destinationCoord).terrainProximity == 1))
                {
                    flag = true;
                }
                else if (AtSitDestination)
                {
                    flag = true;
                }
            }
            if (safariControlled && (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp))
            {
                flag = false;
            }
            bool flag2 = true;
            if (flag)
            {
                int num = Random.Range(0, 4);
                if (room.GetTile(abstractCreature.pos.Tile + Custom.fourDirections[num]).Solid)
                {
                    mainBodyChunk.vel += Custom.fourDirections[num].ToVector2() * 3f;
                    bodyChunks[1].vel += Custom.fourDirections[num].ToVector2() * 3f;
                    Land();
                }
                else if (room.GetTile(mainBodyChunk.pos).verticalBeam)
                {
                    Land();
                }
                else
                if (movementConnection is not null &&
                    movementConnection.destinationCoord.y < abstractCreature.pos.y)
                {
                    flag2 = false;
                }
            }
            else
            {
                for (int l = 0; l < 2; l++)
                {
                    if (bodyChunks[l].ContactPoint.x != 0 ||
                        bodyChunks[l].ContactPoint.y != 0)
                    {
                        bodyChunks[l].vel -= bodyChunks[l].ContactPoint.ToVector2() * 8f * flyingPower * stamina * Random.value;
                    }
                }
            }
            if (stickyCling is not null)
            {
                bodyChunks[1].vel += chargeDir * 0.4f;
                Vector2 val = Custom.DegToVec(Random.value * 360f) * 1.5f;
                mainBodyChunk.vel += val;
                stickyCling.vel += val;
                if (Custom.DistLess(mainBodyChunk.pos, stickyCling.pos, mainBodyChunk.rad + stickyCling.rad + 35f) && !(stickyCling.owner as Creature).enteringShortCut.HasValue && AI.behavior == CicadaAI.Behavior.Antagonize && stickyCling.owner.room == room && stickyCling.pos.y > -20f && flying && Random.value > 0.004761905f && (grabbedBy.Count == 0 || grabbedBy[0].grabber != stickyCling.owner))
                {
                    float num2 = Vector2.Distance(mainBodyChunk.pos, stickyCling.pos);
                    Vector2 val2 = Custom.DirVec(mainBodyChunk.pos, stickyCling.pos);
                    float num3 = mainBodyChunk.rad + stickyCling.rad + 15f;
                    float num4 = 0.65f;
                    float num5 = stickyCling.mass / (stickyCling.mass + mainBodyChunk.mass);
                    mainBodyChunk.pos -= (num3 - num2) * val2 * num5 * num4;
                    mainBodyChunk.vel -= (num3 - num2) * val2 * num5 * num4;
                    stickyCling.pos += (num3 - num2) * val2 * (1f - num5) * num4;
                    stickyCling.vel += (num3 - num2) * val2 * (1f - num5) * num4;
                    stamina = Mathf.Clamp(stamina - (1f / 120f), 0f, 1f);
                    if (stamina < 0.2f)
                    {
                        stickyCling = null;
                    }
                }
                else
                {
                    if (Custom.DistLess(mainBodyChunk.pos, stickyCling.pos, mainBodyChunk.rad + stickyCling.rad + 45f))
                    {
                        for (int m = 0; m < 2; m++)
                        {
                            BodyChunk obj7 = bodyChunks[m];
                            obj7.vel += Custom.DirVec(stickyCling.pos, bodyChunks[m].pos) * 4f;
                        }
                    }
                    stickyCling = null;
                }
                if (stickyCling is null)
                {
                    room.PlaySound(SoundID.Cicada_Tentacles_Detatch, mainBodyChunk);
                }
            }
            else
            {
                flyingPower = Mathf.Lerp(flyingPower, flag2 ? 1f : 0f, 0.1f);
            }
        }
        else
        {
            flyingPower = Mathf.Lerp(flyingPower, 0f, 0.05f);
            if (Climbable(room.GetTilePosition(mainBodyChunk.pos)))
            {
                mainBodyChunk.vel *= 0.8f;
                bodyChunks[1].vel *= 0.8f;
                mainBodyChunk.vel.y += gravity;
                bodyChunks[1].vel.y += gravity;
            }
            else
            {
                flying = true;
            }
        }
        if (AtSitDestination)
        {
            bodyChunks[1].vel += Vector2.ClampMagnitude(BodySitPosOffset(FindBodySitPos(AI.pathFinder.GetDestination.Tile)) - bodyChunks[1].pos, 10f) / 10f * 0.5f;
            mainBodyChunk.vel += Vector2.ClampMagnitude(BodySitPosOffset(AI.pathFinder.GetDestination.Tile) - mainBodyChunk.pos, 10f) / 10f * 0.5f;
        }
        if (movementConnection is not null)
        {
            if (movementConnection.destinationCoord.x < movementConnection.startCoord.x)
            {
                flipH = -1;
            }
            else if (movementConnection.destinationCoord.x > movementConnection.startCoord.x)
            {
                flipH = 1;
            }
            GoThroughFloors = movementConnection.destinationCoord.y < movementConnection.startCoord.y;
            if (movementConnection.type is MovementConnection.MovementType.ShortCut or MovementConnection.MovementType.NPCTransportation)
            {
                enteringShortCut = movementConnection.StartTile;
                if (safariControlled)
                {
                    bool flag3 = false;
                    List<IntVector2> list = new();
                    ShortcutData[] shortcuts = room.shortcuts;
                    for (int n = 0; n < shortcuts.Length; n++)
                    {
                        ShortcutData shortcutData = shortcuts[n];
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
                        {
                            list.Add(shortcutData.StartTile);
                        }
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
                        {
                            flag3 = true;
                        }
                    }
                    if (flag3)
                    {
                        if (list.Count > 0)
                        {
                            list.Shuffle();
                            NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
                        }
                        else
                        {
                            NPCTransportationDestination = movementConnection.destinationCoord;
                        }
                    }
                }
                else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                {
                    NPCTransportationDestination = movementConnection.destinationCoord;
                }
            }
            else if (flying)
            {
                Vector2 val3 = room.MiddleOfTile(movementConnection.destinationCoord);
                int num6 = 1;
                for (int num7 = 0; num7 < 3; num7++)
                {
                    MovementConnection movementConnection2 = (AI.pathFinder as CicadaPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
                    if (movementConnection2 == null)
                    {
                        break;
                    }
                    val3 += room.MiddleOfTile(movementConnection2.destinationCoord);
                    num6++;
                }
                val3 /= num6;
                float num8 = room.aimap.getAItile(mainBodyChunk.pos).terrainProximity / Mathf.Max(room.aimap.getAItile(mainBodyChunk.pos + (Custom.DirVec(mainBodyChunk.pos, val3) * Mathf.Clamp(mainBodyChunk.vel.magnitude * 5f, 5f, 15f))).terrainProximity, 1f);
                num8 = Mathf.Min(num8, 1f);
                num8 = Mathf.Pow(num8, 3f);
                if (WantToSitDownAtDestination && AI.pathFinder.GetDestination.room == room.abstractRoom.index && Custom.DistLess(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), mainBodyChunk.pos, 200f) && AI.VisualContact(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), 0f))
                {
                    num8 *= Mathf.Lerp(0.2f, 1f, Mathf.InverseLerp(0f, 300f, Vector2.Distance(room.MiddleOfTile(AI.pathFinder.GetDestination.Tile), mainBodyChunk.pos)));
                }
                mainBodyChunk.vel += Vector2.ClampMagnitude(val3 - mainBodyChunk.pos, 40f) / 40f * 1.1f * num8 * flyingPower * stamina;
                bodyChunks[1].vel += Vector2.ClampMagnitude(val3 - mainBodyChunk.pos, 40f) / 40f * 0.65f * num8 * flyingPower * stamina;
            }
            else
            {
                if (!movementConnection.destinationCoord.TileDefined)
                {
                    return;
                }
                if (room.GetTile(movementConnection.DestTile).Terrain == Room.Tile.TerrainType.Slope)
                {
                    TakeOff(Custom.DegToVec(Random.value * 360f));
                }
                if (Climbable(movementConnection.DestTile))
                {
                    mainBodyChunk.vel += Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * Mathf.Lerp(0.4f, 1.8f, AI.stuckTracker.Utility());
                    return;
                }
                waitToFlyCounter++;
                if (waitToFlyCounter > 30)
                {
                    TakeOff(Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)));
                }
            }
        }
        else if (chargeCounter > 0)
        {
            chargeCounter++;
            if (chargeCounter < 21)
            {
                mainBodyChunk.vel *= 0.8f;
                bodyChunks[1].vel *= 0.8f;
                bodyChunks[1].vel -= chargeDir * 0.8f;
            }
            else if (chargeCounter == 21)
            {
                room.PlaySound(SoundID.Cicada_Wings_Start_Bump_Attack, mainBodyChunk.pos);
            }
            else if (chargeCounter > 38)
            {
                chargeCounter = 0;
                mainBodyChunk.vel *= 0.5f;
                bodyChunks[1].vel *= 0.5f;
                room.PlaySound(SoundID.Cicada_Wings_Exit_Bump_Attack, mainBodyChunk.pos);
            }
            else
            {
                mainBodyChunk.vel += chargeDir * 4f;
                if (mainBodyChunk.vel.magnitude > 15f)
                {
                    bodyChunks[1].vel *= 0.8f;
                }
                else
                {
                    bodyChunks[1].vel *= 0.98f;
                }
            }
            if (room.aimap.getAItile(mainBodyChunk.pos).narrowSpace)
            {
                chargeCounter = 0;
            }
            flying = true;
        }
        else if (AI.swooshToPos.HasValue)
        {
            mainBodyChunk.vel += Vector2.ClampMagnitude(AI.swooshToPos.Value - mainBodyChunk.pos, 20f) / 20f * 1.8f * flyingPower * stamina;
            bodyChunks[1].vel += Vector2.ClampMagnitude(AI.swooshToPos.Value - mainBodyChunk.pos, 20f) / 20f * 0.8f * flyingPower * stamina;
            bodyChunks[1].vel *= 0.9f;
            bodyChunks[1].vel.y -= 0.2f;
            flying = true;
        }
    }

    private void GrabbedByPlayer()
    {
        flying = stamina > 1f / 3f;
        stickyCling = null;
        if (currentlyLiftingPlayer)
        {
            //stamina -= 1f / (gender ? 190f : 120f);
        }
        stamina = Mathf.Clamp(stamina, 0f, 1f);
        mainBodyChunk.vel *= Mathf.Lerp(1f, 0.98f, stamina);
        bodyChunks[1].vel *= Mathf.Lerp(1f, 0.94f, stamina);
        mainBodyChunk.vel.y += 1.2f * stamina;
        bodyChunks[1].vel.y += playerJumpBoost + (1.8f * stamina);
        Player player = null;
        for (int i = 0; i < grabbedBy.Count; i++)
        {
            if (grabbedBy[i].grabber is Player)
            {
                player = grabbedBy[i].grabber as Player;
                break;
            }
        }
        if (ModManager.MMF && (room.aimap.getAItile(player.firstChunk.pos).narrowSpace || room.aimap.getAItile(firstChunk.pos).narrowSpace || room.aimap.getAItile(bodyChunks[1].pos).narrowSpace))
        {
            bodyChunks[0].vel.y = bodyChunks[0].vel.y - (stamina * 1.3f);
        }
        SocialMemory.Relationship orInitiateRelationship = abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
        if (orInitiateRelationship.like < 0.9f)
        {
            orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, -1f, 0.00025f);
        }
        if (!currentlyLiftingPlayer)
        {
            if (stamina < 1f / 3f)
            {
                stamina += 1f / ((player.input[0].x == 0 && player.input[0].y == 0) ? 600f : 900f);
            }
            else if (stamina < 2f / 3f)
            {
                stamina += 0.004f;
            }
            else
            {
                stamina += 1f / 30f;
            }
        }
        stamina = Mathf.Clamp(stamina, 0f, 1f);
        if (player.input[0].x == 0 && player.input[0].y == 0 && stamina == 1f)
        {
            struggleAgainstPlayer = Mathf.Min(1f, struggleAgainstPlayer + (Random.value / 30f));
            BodyChunk obj3 = bodyChunks[0];
            obj3.vel += Custom.DegToVec(Random.value * 360f) * Random.value * struggleAgainstPlayer * 6f;
        }
        else
        {
            struggleAgainstPlayer = 0f;
        }
        if (player.input[0].jmp)
        {
            if (currentlyLiftingPlayer)
            {
                playerJumpBoost = Mathf.Max(0f, (playerJumpBoost * 0.9f) - (1f / 30f));
            }
            else if (!player.input[1].jmp)
            {
                playerJumpBoost = 1f;
            }
        }
        else
        {
            playerJumpBoost = 0f;
        }
        if (currentlyLiftingPlayer)
        {
            bodyChunks[0].vel.x += player.input[0].x * stamina * 0.3f;
            bodyChunks[1].vel.x += player.input[0].x * stamina * 0.4f;
        }
        noStickyCounter = 200;
        AI.panicFleeCrit = player;
    }

    public void CarryObject()
    {
        if (grasps[0].grabbed is Creature ctr && Random.value < 0.025f && AI.StaticRelationship(ctr.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats)
        {
            LoseAllGrasps();
            return;
        }
        float num = Vector2.Distance(mainBodyChunk.pos, grasps[0].grabbedChunk.pos);
        if (num > 50f)
        {
            LoseAllGrasps();
            return;
        }
        Vector2 val = Custom.DirVec(mainBodyChunk.pos, grasps[0].grabbedChunk.pos);
        float num2 = mainBodyChunk.rad + grasps[0].grabbedChunk.rad;
        float num3 = 0.95f;
        float num4 = 0f;
        if (grasps[0].grabbed.TotalMass > TotalMass / 3f)
        {
            num4 = (grasps[0].grabbedChunk.mass / grasps[0].grabbedChunk.mass) + mainBodyChunk.mass;
        }
        BodyChunk bodyChunk = mainBodyChunk;
        bodyChunk.pos -= (num2 - num) * val * num4 * num3;
        BodyChunk bodyChunk2 = mainBodyChunk;
        bodyChunk2.vel -= (num2 - num) * val * num4 * num3;
        BodyChunk grabbedChunk = grasps[0].grabbedChunk;
        grabbedChunk.pos += (num2 - num) * val * (1f - num4) * num3;
        BodyChunk grabbedChunk2 = grasps[0].grabbedChunk;
        grabbedChunk2.vel += (num2 - num) * val * (1f - num4) * num3;
    }

    public void Charge(Vector2 pos)
    {
        stickyCling = null;
        noStickyCounter = 140;
        if (chargeCounter <= 0)
        {
            chargeDir = Custom.DirVec(mainBodyChunk.pos, pos);
            chargeCounter = 1;
            room.PlaySound(SoundID.Cicada_Wings_Prepare_Bump_Attack, mainBodyChunk.pos);
        }
    }

    public bool Climbable(IntVector2 tile)
    {
        return safariControlled
            ? (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && room.aimap.getAItile(tile).terrainProximity == 1)
|| room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb
            : room.aimap.getAItile(tile).terrainProximity == 1 || room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb;
    }

    public bool TryToGrabPrey(PhysicalObject prey)
    {
        BodyChunk bodyChunk = null;
        float num = float.MaxValue;
        for (int i = 0; i < prey.bodyChunks.Length; i++)
        {
            if (Custom.DistLess(mainBodyChunk.pos, prey.bodyChunks[i].pos, Mathf.Max(num, prey.bodyChunks[i].rad + mainBodyChunk.rad + 3f)))
            {
                num = Vector2.Distance(mainBodyChunk.pos, prey.bodyChunks[i].pos);
                bodyChunk = prey.bodyChunks[i];
            }
        }
        if (bodyChunk == null)
        {
            return false;
        }
        for (int j = 0; j < 2; j++)
        {
            BodyChunk obj = bodyChunks[j];
            obj.vel *= 0.75f;
            BodyChunk obj2 = bodyChunks[j];
            obj2.vel += Custom.DegToVec(Random.value * 360f) * 6f;
        }
        return Grab(prey, 0, bodyChunk.index, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, prey.TotalMass < TotalMass);
    }

    private void TakeOff(Vector2 dir)
    {
        waitToFlyCounter = 0;
        flying = true;
        int num = 0;
        Vector2 val = default;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int terrainProximity = room.aimap.getAItile(abstractCreature.pos.Tile + (Custom.eightDirections[i] * j)).terrainProximity;
                num += terrainProximity;
                val = Custom.eightDirections[i].ToVector2() * terrainProximity;
            }
        }
        val /= num;
        float value = Random.value;
        Vector2 vel = mainBodyChunk.vel;
        Vector2 val2 = Vector2.Lerp(dir, val, 0.5f);
        mainBodyChunk.vel = vel + (val2.normalized * 9f * value);
        Vector2 vel2 = bodyChunks[1].vel;
        val2 = Vector2.Lerp(dir, val, 0.8f);
        bodyChunks[1].vel = vel2 + (val2.normalized * 7f * value);
        flyingPower = 0.5f;
        _ = room.PlaySound(SoundID.Cicada_Wings_TakeOff, mainBodyChunk, false, 1f, iVars.wingSoundPitch);
    }

    private void Land()
    {
        waitToFlyCounter = 0;
        flying = false;
        _ = room.PlaySound(SoundID.Cicada_Landing, mainBodyChunk);
    }

    private IntVector2 FindBodySitPos(IntVector2 headPos)
    {
        return Climbable(headPos + new IntVector2(0, -1))
            ? headPos + new IntVector2(0, -1)
            : Climbable(headPos + new IntVector2(flipH, 0))
            ? headPos + new IntVector2(flipH, 0)
            : Climbable(headPos + new IntVector2(-flipH, 0)) ? headPos + new IntVector2(-flipH, 0) : headPos + new IntVector2(0, 1);
    }

    private Vector2 BodySitPosOffset(IntVector2 pos)
    {
        if (room.GetTile(pos + new IntVector2(flipH, 0)).Solid)
        {
            sitDirection = new IntVector2(flipH, 0);
            return room.MiddleOfTile(pos) + new Vector2(flipH * -2f, 0f);
        }
        if (room.GetTile(pos + new IntVector2(-flipH, 0)).Solid)
        {
            sitDirection = new IntVector2(-flipH, 0);
            return room.MiddleOfTile(pos) + new Vector2(-flipH * -2f, 0f);
        }
        if (room.GetTile(pos + new IntVector2(0, 1)).Solid)
        {
            sitDirection = new IntVector2(0, 1);
            return room.MiddleOfTile(pos) + new Vector2(0f, -2f);
        }
        if (room.GetTile(pos + new IntVector2(0, -1)).Solid)
        {
            sitDirection = new IntVector2(0, -1);
            return room.MiddleOfTile(pos) + new Vector2(0f, 2f);
        }
        if (room.GetTile(pos).verticalBeam)
        {
            if (!room.GetTile(pos + new IntVector2(flipH, 0)).Solid)
            {
                sitDirection = new IntVector2(-flipH, 0);
                return room.MiddleOfTile(pos) + new Vector2(flipH * 7f, 0f);
            }
            if (!room.GetTile(pos + new IntVector2(-flipH, 0)).Solid)
            {
                sitDirection = new IntVector2(flipH, 0);
                return room.MiddleOfTile(pos) + new Vector2(-flipH * 7f, 0f);
            }
        }
        return room.MiddleOfTile(pos);
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (!Consious)
        {
            return;
        }
        if (Charging)
        {
            Vector2 val = Vector2.Lerp(chargeDir * 6f, bodyChunks[myChunk].vel * 0.5f, 0.5f);
            if (val.y < 0f)
            {
                val.y *= 0.5f;
            }
            if (ModManager.MSC && otherObject is Player && (otherObject as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                BodyChunk obj = otherObject.bodyChunks[otherChunk];
                obj.vel += val / otherObject.bodyChunks[otherChunk].mass * 2f;
            }
            else
            {
                BodyChunk obj2 = otherObject.bodyChunks[otherChunk];
                obj2.vel += val / otherObject.bodyChunks[otherChunk].mass;
            }
            if (otherObject is Cicada)
            {
                chargeCounter = 25;
                chargeDir = Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, bodyChunks[myChunk].pos);
                if (!(otherObject as Cicada).Charging)
                {
                    (otherObject as Creature).Stun(20);
                }
            }
            else
            {
                chargeCounter = 0;
                Stun(10);
                if (otherObject is Creature)
                {
                    if (ModManager.MSC && otherObject is Player && (otherObject as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    {
                        (otherObject as Player).SaintStagger(220);
                    }
                    else
                    {
                        (otherObject as Creature).Stun(4);
                    }
                }
            }
            room.PlaySound((otherObject is Player) ? SoundID.Cicada_Bump_Attack_Hit_Player : SoundID.Cicada_Bump_Attack_Hit_NPC, mainBodyChunk);
        }
        else if (myChunk == 0 && noStickyCounter == 0 && Random.value < 0.5f && stickyCling == null && otherObject is Creature && AI.behavior == CicadaAI.Behavior.Antagonize && AI.preyTracker.MostAttractivePrey.representedCreature == (otherObject as Creature).abstractCreature)
        {
            stickyCling = otherObject.bodyChunks[otherChunk];
            chargeDir = Custom.DegToVec(Mathf.Lerp(-70f, 70f, Random.value));
            room.PlaySound((stickyCling.owner is Player) ? SoundID.Cicada_Tentacles_Grab_Player : SoundID.Cicada_Tentacles_Grab_NPC, mainBodyChunk);
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (Charging)
        {
            if (firstContact)
            {
                room.PlaySound(SoundID.Cicada_Bump_Attack_Hit_Terrain, mainBodyChunk);
            }
            if (speed < 20f)
            {
                if (direction.y < 0)
                {
                    mainBodyChunk.vel.y = Mathf.Abs(mainBodyChunk.vel.y);
                }
                else if (direction.y > 0)
                {
                    mainBodyChunk.vel.y = 0f - Mathf.Abs(mainBodyChunk.vel.y);
                }
                if (direction.x < 0)
                {
                    mainBodyChunk.vel.x = Mathf.Abs(mainBodyChunk.vel.x);
                }
                else if (direction.x > 0)
                {
                    mainBodyChunk.vel.x = 0f - Mathf.Abs(mainBodyChunk.vel.x);
                }
                if (firstContact)
                {
                    chargeCounter = 25;
                    room.ScreenMovement(mainBodyChunk.pos, Vector2.ClampMagnitude(direction.ToVector2() * (speed / 25f), 1.5f), Mathf.Min(speed * 0.01f, 0.3f));
                    mainBodyChunk.vel -= direction.ToVector2();
                }
                chargeDir = mainBodyChunk.vel.normalized;
            }
            else
            {
                Stun(20);
                if (firstContact)
                {
                    room.ScreenMovement(mainBodyChunk.pos, Vector2.ClampMagnitude(direction.ToVector2() * (speed / 25f), 2.5f), Mathf.Min(speed * 0.02f, 0.7f));
                }
            }
        }
        else if (speed > 1.5f && firstContact)
        {
            _ = room.PlaySound((speed < 8f) ? SoundID.Cicada_Light_Terrain_Impact : SoundID.Cicada_Heavy_Terrain_Impact, mainBodyChunk);
        }
    }

    public override void Die()
    {
        base.Die();
    }

    public override Color ShortCutColor()
    {
        return iVars.color.rgb;
    }

    public override void Stun(int st)
    {
        flying = false;
        chargeCounter = 0;
        if (Random.value < 0.5f)
        {
            LoseAllGrasps();
        }
        base.Stun(st);
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        Vector2 spitoutAngle = newRoom.ShorcutEntranceHoleDirection(pos).ToVector2();
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - (spitoutAngle * (-1.5f + i) * 15f);
            bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
            bodyChunks[i].vel = spitoutAngle * 10f;
        }
        graphicsModule?.Reset();
    }

    public override void RecreateSticksFromAbstract()
    {
        for (int i = 0; i < abstractCreature.stuckObjects.Count; i++)
        {
            if (abstractCreature.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick grip &&
                grip.A == abstractCreature &&
                grip.B.realizedObject is not null)
            {
                grasps[grip.grasp] = new Grasp(this, grip.B.realizedObject, grip.grasp, Random.Range(0, grip.B.realizedObject.bodyChunks.Length), Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, grip.B.realizedObject.TotalMass < TotalMass);
                grip.B.realizedObject.Grabbed(grasps[grip.grasp]);
            }
        }
    }
}