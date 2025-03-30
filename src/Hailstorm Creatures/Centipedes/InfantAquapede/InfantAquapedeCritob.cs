namespace Hailstorm;

public class InfantAquapedeCritob : Critob
{

    public Color InfantAquapedeColor = Custom.HSL2RGB(240 / 360f, 1, 0.63f);

    internal InfantAquapedeCritob() : base(HSEnums.CreatureType.InfantAquapede)
    {
        Icon = new SimpleIcon("Kill_InfantAquapede", InfantAquapedeColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.6f, 0.4f);
        RegisterUnlock(KillScore.Configurable(2), HSEnums.SandboxUnlock.InfantAquapede);
    }
    public override int ExpeditionScore() => 2;

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
        else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
        {
            if (map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
    }

    public override Color DevtoolsMapColor(AbstractCreature absCnt) => InfantAquapedeColor;
    public override string DevtoolsMapName(AbstractCreature absCnt) => "bA";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.LikesInside
    };
    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "infantaquapede", "InfantAquapede" };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate Aquababy = new CreatureFormula(DLCSharedEnums.CreatureTemplateType.AquaCenti, Type, "Infant Aquapede")
        {
            DamageResistances = new() { Base = 1, Electric = 100 },
            StunResistances = new() { Base = 1, Electric = 100 },
            InstantDeathDamage = 1.1f,
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(DLCSharedEnums.CreatureTemplateType.AquaCenti),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 1f),
        }.IntoTemplate();
        Aquababy.meatPoints = 3;
        Aquababy.bodySize = 0.35f;
        Aquababy.shortcutSegments = 2;
        Aquababy.communityInfluence = 0.2f;
        Aquababy.dangerousToPlayer = 0.15f;
        Aquababy.lungCapacity = 9900f;
        Aquababy.visualRadius = 950;
        return Aquababy;
    }
    public override void EstablishRelationships()
    {
        Relationships s = new(HSEnums.CreatureType.InfantAquapede);

        s.IsInPack(DLCSharedEnums.CreatureTemplateType.AquaCenti, 0.5f);
        s.IsInPack(HSEnums.CreatureType.InfantAquapede, 0.85f);

        s.Ignores(HSEnums.CreatureType.Cyanwing);

        s.FearedBy(CreatureTemplate.Type.Leech, 1);
        s.FearedBy(CreatureTemplate.Type.SeaLeech, 1);
        s.FearedBy(DLCSharedEnums.CreatureTemplateType.JungleLeech, 1);
        s.FearedBy(CreatureTemplate.Type.Hazer, 1);

        s.Fears(DLCSharedEnums.CreatureTemplateType.BigJelly, 0.33f);
        s.Fears(HSEnums.CreatureType.GorditoGreenieLizard, 0.33f);
        s.Fears(HSEnums.CreatureType.Chillipede, 0.33f);
        s.Fears(CreatureTemplate.Type.Slugcat, 0.5f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        s.Fears(CreatureTemplate.Type.Salamander, 0.66f);
        s.Fears(DLCSharedEnums.CreatureTemplateType.EelLizard, 1);


        s.MakesUncomfortable(CreatureTemplate.Type.Salamander, 0.6f);

        s.UncomfortableAround(CreatureTemplate.Type.Salamander, 0.6f);

        s.IntimidatedBy(CreatureTemplate.Type.Snail, 1);

        s.EatenBy(CreatureTemplate.Type.JetFish, 0.2f);
        s.EatenBy(DLCSharedEnums.CreatureTemplateType.EelLizard, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCnt) => new CentipedeAI(absCnt, absCnt.world);
    public override Creature CreateRealizedCreature(AbstractCreature absCnt) => new InfantAquapede(absCnt, absCnt.world);
    public override CreatureState CreateState(AbstractCreature absCnt) => new InfantAquapedeState(absCnt);
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.SmallCentipede;
}