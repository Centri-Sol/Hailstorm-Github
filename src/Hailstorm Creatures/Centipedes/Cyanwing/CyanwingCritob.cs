using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using MoreSlugcats;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RWCustom;

namespace Hailstorm;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

sealed class CyanwingCritob : Critob
{

    public Color CyanwingColor = Custom.HSL2RGB(180/360f, 0.88f, 0.4f);

    internal CyanwingCritob() : base(HailstormCreatures.Cyanwing)
    {
        Icon = new SimpleIcon("Kill_Cyanwing", CyanwingColor);
        LoadedPerformanceCost = 15f;
        SandboxPerformanceCost = new(1.15f, 0.75f);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(25), HailstormUnlocks.Cyanwing);
    }
    public override int ExpeditionScore() => 25;

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

    public override Color DevtoolsMapColor(AbstractCreature absCnt) => CyanwingColor;
    public override string DevtoolsMapName(AbstractCreature absCnt) => "CW";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.LikesOutside,
            RoomAttractivenessPanel.Category.Flying
        };
    }
    public override IEnumerable<string> WorldFileAliases() => new[] { "cyanwing", "Cyanwing" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(CreatureTemplate.Type.Centiwing, Type, "Cyanwing")
        {
            TileResistances = new()
            {
                Air = new(1f, PathCost.Legality.Allowed),
                OffScreen = new(1f, PathCost.Legality.Allowed),
                Floor = new(1f, PathCost.Legality.Allowed),
                Corridor = new(1f, PathCost.Legality.Allowed),
                Climb = new(1f, PathCost.Legality.Allowed),
                Wall = new(1f, PathCost.Legality.Allowed),
                Ceiling = new(1f, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, PathCost.Legality.Allowed),
                OpenDiagonal = new(2f, PathCost.Legality.Allowed),
                ReachOverGap = new(2f, PathCost.Legality.Allowed),
                DoubleReachUp = new(1.5f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(1.5f, PathCost.Legality.Allowed),
                NPCTransportation = new(25f, PathCost.Legality.Allowed),
                OffScreenMovement = new(1f, PathCost.Legality.Allowed),
                BetweenRooms = new(8f, PathCost.Legality.Allowed),
                Slope = new(1f, PathCost.Legality.Allowed),
                DropToFloor = new(1f, PathCost.Legality.Allowed),
                DropToClimb = new(1f, PathCost.Legality.Allowed),
                ShortCut = new(10f, PathCost.Legality.Allowed),
                BigCreatureShortCutSqueeze = new(10f, PathCost.Legality.Allowed),
                ReachUp = new(1f, PathCost.Legality.Allowed),
                ReachDown = new(1f, PathCost.Legality.Allowed),
                CeilingSlope = new(2f, PathCost.Legality.Allowed)
            },
            DamageResistances = new() { Base = 1, Electric = 10000, Explosion = 2 },
            StunResistances = new() { Base = 1, Electric = 10000 },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Centiwing),
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
        }.IntoTemplate();
        cf.meatPoints = 12;
        cf.bodySize = 8.5f;
        cf.dangerousToPlayer = 0.7f;
        cf.communityInfluence = 0.5f;
        cf.shortcutSegments = 5;

        cf.visualRadius = 725;
        cf.throughSurfaceVision = 1f;
        cf.waterVision = 0.5f;

        cf.socialMemory = true;
        return cf;
    }
    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormCreatures.Cyanwing);

        s.IsInPack(CreatureTemplate.Type.Centiwing, 1);

        s.Ignores(CreatureTemplate.Type.TentaclePlant);
        s.Ignores(HailstormCreatures.InfantAquapede);

        // "IgnoredBy" makes the given creatures ignore this one.
        s.IgnoredBy(CreatureTemplate.Type.Centipede);
        s.IgnoredBy(CreatureTemplate.Type.RedCentipede);
        s.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti);

        // "FearedBy" causes other creatures to actively avoid this creature.
        s.FearedBy(CreatureTemplate.Type.Slugcat, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        s.FearedBy(CreatureTemplate.Type.Fly, 1);
        s.FearedBy(CreatureTemplate.Type.PinkLizard, 1);
        s.FearedBy(CreatureTemplate.Type.GreenLizard, 1);
        s.FearedBy(CreatureTemplate.Type.BlueLizard, 1);
        s.FearedBy(CreatureTemplate.Type.Salamander, 1);
        s.FearedBy(CreatureTemplate.Type.WhiteLizard, 1);
        s.FearedBy(CreatureTemplate.Type.YellowLizard, 1);
        s.FearedBy(CreatureTemplate.Type.BlackLizard, 1);
        s.FearedBy(CreatureTemplate.Type.CyanLizard, 1);
        s.FearedBy(CreatureTemplate.Type.RedLizard, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 1);
        s.FearedBy(CreatureTemplate.Type.CicadaB, 1);
        s.FearedBy(CreatureTemplate.Type.EggBug, 1);
        s.FearedBy(CreatureTemplate.Type.VultureGrub, 1);
        s.FearedBy(CreatureTemplate.Type.Vulture, 0.5f);
        s.FearedBy(CreatureTemplate.Type.PoleMimic, 1);
        s.FearedBy(CreatureTemplate.Type.TentaclePlant, 1);
        s.FearedBy(CreatureTemplate.Type.Hazer, 1);
        s.FearedBy(CreatureTemplate.Type.Snail, 1);
        s.FearedBy(CreatureTemplate.Type.JetFish, 1);
        s.FearedBy(CreatureTemplate.Type.Leech, 1);
        s.FearedBy(CreatureTemplate.Type.SeaLeech, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 1);
        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1);
        s.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1);
        s.FearedBy(CreatureTemplate.Type.DropBug, 1);
        s.FearedBy(CreatureTemplate.Type.BigSpider, 1);
        s.FearedBy(CreatureTemplate.Type.SpitterSpider, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 1);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 1);
        s.FearedBy(CreatureTemplate.Type.TubeWorm, 1);
        s.FearedBy(CreatureTemplate.Type.Deer, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.75f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.75f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        s.FearedBy(HailstormCreatures.Luminescipede, 1);
        s.FearedBy(HailstormCreatures.Chillipede, 1);

        // "Eaten By" makes other creatures prey on this one.
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.4f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 0.4f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.6f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.6f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.8f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.8f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 1);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCnt) => new CyanwingAI(absCnt, absCnt.world);
    public override Creature CreateRealizedCreature(AbstractCreature absCnt) => new Cyanwing(absCnt, absCnt.world);
    public override CreatureState CreateState(AbstractCreature absCnt) => new CyanwingState(absCnt);

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centiwing;
    #nullable disable
}

//----------------------------------------------------------------------------------