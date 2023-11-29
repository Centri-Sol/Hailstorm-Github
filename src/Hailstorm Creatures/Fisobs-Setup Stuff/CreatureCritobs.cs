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

sealed class InfantAquapedeCritob : Critob
{

    public Color InfantAquapedeColor = Custom.HSL2RGB(240 / 360f, 1, 0.63f);

    internal InfantAquapedeCritob() : base(HailstormEnums.InfantAquapede)
    {
        Icon = new SimpleIcon("Kill_InfantAquapede", InfantAquapedeColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.6f, 0.4f);
        RegisterUnlock(KillScore.Configurable(2), HailstormEnums.InfantAquapedeUnlock);
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

    public override IEnumerable<string> WorldFileAliases() => new[] { "infantaquapede", "InfantAquapede" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.LikesInside
    };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate temp = new CreatureFormula(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, Type, "BabyAquapede")
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
                OpenDiagonal = new(3f, PathCost.Legality.Allowed),
                ReachOverGap = new(3f, PathCost.Legality.Allowed),
                DoubleReachUp = new(2f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2f, PathCost.Legality.Allowed),
                NPCTransportation = new(15f, PathCost.Legality.Allowed),
                OffScreenMovement = new(1f, PathCost.Legality.Allowed),
                BetweenRooms = new(10f, PathCost.Legality.Allowed),
                Slope = new(1.5f, PathCost.Legality.Allowed),
                DropToFloor = new(5f, PathCost.Legality.Allowed),
                DropToWater = new(1f, PathCost.Legality.Allowed),
                DropToClimb = new(5f, PathCost.Legality.Allowed),
                ShortCut = new(1f, PathCost.Legality.Allowed),
                ReachUp = new(1.1f, PathCost.Legality.Allowed),
                ReachDown = new(1.1f, PathCost.Legality.Allowed),
                CeilingSlope = new(2f, PathCost.Legality.Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 1f),
            DamageResistances = new() { Base = 1, Electric = 100 },
            StunResistances = new() { Base = 1, Electric = 100 },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti),
        }.IntoTemplate();
        temp.meatPoints = 3;
        temp.visualRadius = 1400;
        temp.waterVision = 2f;
        temp.stowFoodInDen = true;
        temp.throughSurfaceVision = 0.5f;
        temp.movementBasedVision = 0.95f;
        temp.dangerousToPlayer = 0.3f;
        temp.communityInfluence = 0.4f;
        temp.bodySize = 0.4f;
        temp.BlizzardAdapted = true;
        temp.BlizzardWanderer = true;
        temp.waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
        temp.lungCapacity = 9900f;
        temp.canSwim = true;
        temp.jumpAction = "Swap Heads";
        temp.pickupAction = "Grab/Shock";
        temp.throwAction = "Release";
        temp.shortcutSegments = 2;
        return temp;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.InfantAquapede);

        // "IgnoredBy" makes the given creatures ignore this one. Who would have gueeeessed
        s.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
        s.IsInPack(HailstormEnums.InfantAquapede, 0.85f);

        s.Ignores(HailstormEnums.Cyanwing);

        s.FearedBy(CreatureTemplate.Type.Leech, 1);
        s.FearedBy(CreatureTemplate.Type.SeaLeech, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 1);
        s.FearedBy(CreatureTemplate.Type.Hazer, 1);

        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.33f);
        s.Fears(HailstormEnums.GorditoGreenie, 0.33f);
        s.Fears(HailstormEnums.Chillipede, 0.33f);
        s.Fears(CreatureTemplate.Type.Slugcat, 0.5f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        s.Fears(CreatureTemplate.Type.Salamander, 0.66f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);


        s.MakesUncomfortable(CreatureTemplate.Type.Salamander, 0.6f);

        s.UncomfortableAround(CreatureTemplate.Type.Salamander, 0.6f);

        s.IntimidatedBy(CreatureTemplate.Type.Snail, 1);

        s.EatenBy(CreatureTemplate.Type.JetFish, 0.2f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCnt) => new CentipedeAI(absCnt, absCnt.world);

    public override Creature CreateRealizedCreature(AbstractCreature absCnt) => new Centipede(absCnt, absCnt.world);

    public override CreatureState CreateState(AbstractCreature absCnt) => new HealthState(absCnt);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.SmallCentipede;
#nullable disable
}

//----------------------------------------------------------------------------------
/*
sealed class RavenCritob : Critob
{

    public Color RavenColor = Custom.HSL2RGB(240 / 360f, 1, 0.63f);

    internal RavenCritob() : base(HailstormEnums.InfantAquapede)
    {
        Icon = new SimpleIcon("Kill_Raven", RavenColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.6f, 0.4f);
        RegisterUnlock(KillScore.Configurable(2), HailstormEnums.InfantAquapedeUnlock);
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

    public override Color DevtoolsMapColor(AbstractCreature absCnt) => RavenColor;

    public override string DevtoolsMapName(AbstractCreature absCnt) => "Rvn";

    public override IEnumerable<string> WorldFileAliases() => new[] { "raven", "Raven" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.LikesInside
    };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate temp = new CreatureFormula(CreatureTemplate.Type.Vulture, Type, "Raven")
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
                OpenDiagonal = new(3f, PathCost.Legality.Allowed),
                ReachOverGap = new(3f, PathCost.Legality.Allowed),
                DoubleReachUp = new(2f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2f, PathCost.Legality.Allowed),
                NPCTransportation = new(15f, PathCost.Legality.Allowed),
                OffScreenMovement = new(1f, PathCost.Legality.Allowed),
                BetweenRooms = new(10f, PathCost.Legality.Allowed),
                Slope = new(1.5f, PathCost.Legality.Allowed),
                DropToFloor = new(5f, PathCost.Legality.Allowed),
                DropToWater = new(1f, PathCost.Legality.Allowed),
                DropToClimb = new(5f, PathCost.Legality.Allowed),
                ShortCut = new(1f, PathCost.Legality.Allowed),
                ReachUp = new(1.1f, PathCost.Legality.Allowed),
                ReachDown = new(1.1f, PathCost.Legality.Allowed),
                CeilingSlope = new(2f, PathCost.Legality.Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            DamageResistances = new() { Base = 1, Electric = 100 },
            StunResistances = new() { Base = 1, Electric = 100 },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Vulture),
        }.IntoTemplate();
        temp.meatPoints = 3;
        temp.visualRadius = 1400;
        temp.waterVision = 2f;
        temp.stowFoodInDen = true;
        temp.throughSurfaceVision = 0.5f;
        temp.movementBasedVision = 0.95f;
        temp.dangerousToPlayer = 0.3f;
        temp.communityInfluence = 0.4f;
        temp.bodySize = 0.4f;
        temp.BlizzardAdapted = true;
        temp.BlizzardWanderer = true;
        temp.waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
        temp.lungCapacity = 9900f;
        temp.canSwim = true;
        temp.jumpAction = "Swap Heads";
        temp.pickupAction = "Grab/Shock";
        temp.throwAction = "Release";
        temp.shortcutSegments = 2;
        return temp;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.InfantAquapede);

        // "IgnoredBy" makes the given creatures ignore this one. Who would have gueeeessed
        s.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
        s.IsInPack(HailstormEnums.InfantAquapede, 0.85f);

        s.Ignores(HailstormEnums.Cyanwing);

        s.FearedBy(CreatureTemplate.Type.Leech, 1);
        s.FearedBy(CreatureTemplate.Type.SeaLeech, 1);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 1);
        s.FearedBy(CreatureTemplate.Type.Hazer, 1);

        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.33f);
        s.Fears(HailstormEnums.GorditoGreenie, 0.33f);
        s.Fears(HailstormEnums.Chillipede, 0.33f);
        s.Fears(CreatureTemplate.Type.Slugcat, 0.5f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        s.Fears(CreatureTemplate.Type.Salamander, 0.66f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);


        s.MakesUncomfortable(CreatureTemplate.Type.Salamander, 0.6f);

        s.UncomfortableAround(CreatureTemplate.Type.Salamander, 0.6f);

        s.IntimidatedBy(CreatureTemplate.Type.Snail, 1);

        s.EatenBy(CreatureTemplate.Type.JetFish, 0.2f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absVul) => new VultureAI(absVul, absVul.world);

    public override Creature CreateRealizedCreature(AbstractCreature absVul) => new Vulture(absVul, absVul.world);

    public override CreatureState CreateState(AbstractCreature absVul) => new Vulture.VultureState(absVul);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Vulture;
#nullable disable
}
*/
//----------------------------------------------------------------------------------

sealed class IcyBlueCritob : Critob
{
    public Color IcyBlueColor = new (138/255f, 151/255f, 193/255f);
    internal IcyBlueCritob() : base(HailstormEnums.IcyBlue)
    {
        Icon = new SimpleIcon("Kill_Icy_Blue_Lizard", IcyBlueColor);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(0.6f, 0.6f);
        RegisterUnlock(KillScore.Configurable(10), HailstormEnums.IcyBlueUnlock);
    }

    public override int ExpeditionScore() => 10;

    public override Color DevtoolsMapColor(AbstractCreature absIcy) => IcyBlueColor;

    public override string DevtoolsMapName(AbstractCreature absIcy) => "Icy";

    public override IEnumerable<string> WorldFileAliases() => new[] { "icy", "icyblue", "icybluelizard", "Icy", "IcyBlue", "IcyBlueLizard" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesInside
    };

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(HailstormEnums.IcyBlue, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);

    // This method sets this creature's relationships with other creatures.
    // I explain what I can, but if some relationship types don't have an explanation, that means I don't know what they do.
    public override void EstablishRelationships()
    {
        Relationships s = new (HailstormEnums.IcyBlue);
        
        // "Rivals" makes this creature actively pick fights with the listed creatures.
        s.Rivals(CreatureTemplate.Type.WhiteLizard, 0.50f);
        s.Rivals(CreatureTemplate.Type.BlackLizard, 0.75f);
        s.Rivals(CreatureTemplate.Type.PinkLizard, 1);

        s.IsInPack(CreatureTemplate.Type.BlueLizard, 0.66f);
        s.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.75f);
        s.IsInPack(HailstormEnums.Freezer, 1);
        s.IsInPack(HailstormEnums.IcyBlue, 1);

        // "HasDynamicRelationship" allows other creatures to have a changing relationship with this one.
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.5f);

        s.UncomfortableAround(CreatureTemplate.Type.Spider, 0.4f);
        s.UncomfortableAround(CreatureTemplate.Type.Snail, 0.4f);

        s.MakesUncomfortable(CreatureTemplate.Type.DropBug, 0.5f);

        // "IntimidatedBy" makes this creature less willing to attack the given creatures.
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);

        // "Fears" makes this creature actively avoid the listed creatures.
        s.Fears(CreatureTemplate.Type.TentaclePlant, 0.2f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.25f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.4f);
        s.Fears(CreatureTemplate.Type.BigEel, 1);
        s.Fears(CreatureTemplate.Type.BrotherLongLegs, 1);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);

        // "FearedBy" causes other creatures to actively avoid this creature.
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.15f);
        s.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.15f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.3f);
        s.FearedBy(CreatureTemplate.Type.BigSpider, 0.4f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 0.6f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 0.7f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.7f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.7f);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 0.8f);
        s.FearedBy(CreatureTemplate.Type.CicadaB, 0.8f);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.9f);

        // "AttackedBy" determines how much other creatures will hunt this creature down.
        s.AttackedBy(CreatureTemplate.Type.Scavenger, 0.4f);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.6f);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.8f);
        s.AttackedBy(CreatureTemplate.Type.YellowLizard, 1);

        // "Eaten By" makes other creatures prey on this one.
        s.EatenBy(CreatureTemplate.Type.GreenLizard, 0.25f);
        s.EatenBy(CreatureTemplate.Type.Centipede, 0.4f);
        s.EatenBy(CreatureTemplate.Type.Centiwing, 0.4f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
        s.EatenBy(CreatureTemplate.Type.CyanLizard, 0.5f);
        s.EatenBy(CreatureTemplate.Type.Vulture, 0.6f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.6f);
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
        s.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.6f);
        s.EatenBy(CreatureTemplate.Type.RedCentipede, 0.6f);
        s.EatenBy(HailstormEnums.Cyanwing, 0.6f);

        // "Eats" determines how eager this creature is to make a meal out of the given creatures.
        s.Eats(CreatureTemplate.Type.TubeWorm, 0.10f);
        s.Eats(CreatureTemplate.Type.Hazer, 0.25f);
        s.Eats(CreatureTemplate.Type.JetFish, 0.3f);
        s.Eats(CreatureTemplate.Type.VultureGrub, 0.3f);        
        s.Eats(CreatureTemplate.Type.BigSpider, 0.33f);
        s.Eats(CreatureTemplate.Type.SpitterSpider, 0.33f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        s.Eats(HailstormEnums.InfantAquapede, 0.5f);
        s.Eats(HailstormEnums.Luminescipede, 0.5f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.5f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.5f);
        s.Eats(CreatureTemplate.Type.DropBug, 0.5f);
        s.Eats(CreatureTemplate.Type.EggBug, 0.66f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.66f);
        s.Eats(CreatureTemplate.Type.Slugcat, 0.75f);
        s.Eats(CreatureTemplate.Type.CicadaA, 0.85f);
        s.Eats(CreatureTemplate.Type.CicadaB, 0.85f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.85f);
        s.Eats(CreatureTemplate.Type.Centiwing, 0.9f);
        s.Eats(CreatureTemplate.Type.Centipede, 1);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 1);
        s.Eats(CreatureTemplate.Type.LanternMouse, 1);
        s.Eats(CreatureTemplate.Type.Scavenger, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);

        s.Ignores(HailstormEnums.Chillipede);

        s.IgnoredBy(HailstormEnums.Chillipede);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absIcy) => new LizardAI(absIcy, absIcy.world);

    public override Creature CreateRealizedCreature(AbstractCreature absIcy) => new Lizard(absIcy, absIcy.world);

    public override CreatureState CreateState(AbstractCreature absIcy) => new ColdLizState(absIcy);

    public override void LoadResources(RainWorld rainWorld) { }

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BlueLizard;
    #nullable disable
}

//----------------------------------------------------------------------------------

sealed class FreezerCritob : Critob
{

    public Color FreezerColor = new (129f/255f, 200f/255f, 236f/255f);

    internal FreezerCritob() : base(HailstormEnums.Freezer) // You put your new creature's enum here
    {
        Icon = new SimpleIcon("Kill_Freezer_Lizard", FreezerColor); // Your creature type's in-game icon and icon color. You can either use a pre-existing sprite for this or make a custom one.
        LoadedPerformanceCost = 50f; // idk how to gauge what number to set for these performance costs
        SandboxPerformanceCost = new(0.7f, 0.7f);
        RegisterUnlock(KillScore.Configurable(25), HailstormEnums.FreezerUnlock);
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

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.NPCTransportation)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.NPCTransportation)
                allow = true;

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.RegionTransportation)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.RegionTransportation)
                allow = true;

            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.RoomExit)
                allow = true;
        }
    }

    public override string DevtoolsMapName(AbstractCreature absFrz) => "Frz";
    public override Color DevtoolsMapColor(AbstractCreature absFrz) => FreezerColor;


    // The names you set here are what you use when setting creature spawns. You only need to set up 1; I don't really know why I did 6.
    public override IEnumerable<string> WorldFileAliases() => new[] { "frz", "freezer", "freezerlizard", "Frz", "Freezer", "FreezerLizard" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] 
    { 
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesOutside
    };

    // Creates your actual creature template. This works differently with Lizards than it does with other creatures; you'll need to make a hook to LizardBreeds.BreedTemplate to do the rest of this.
    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(HailstormEnums.Freezer, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);

    public override void EstablishRelationships()
    {
        Relationships s = new (HailstormEnums.Freezer);

        // "HasDynamicRelationship" allows other creatures to have a changing relationship with this one.
        // Not sure if this can just be slapped on to any creature without needing to specifically code interactions for it, but it works just fine for Lizards with Slugcats.
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.5f);

        // "Fears" makes this creature actively avoid the listed creatures.
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.1f);
        s.Fears(CreatureTemplate.Type.BigEel, 0.25f);
        s.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.5f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.6f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.7f);

        // "FearedBy" causes other creatures to actively avoid this creature.
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.2f);
        s.FearedBy(CreatureTemplate.Type.Vulture, 0.4f);
        s.FearedBy(CreatureTemplate.Type.Centiwing, 0.4f);
        s.FearedBy(CreatureTemplate.Type.SpitterSpider, 0.6f);
        s.FearedBy(CreatureTemplate.Type.BigSpider, 0.7f);
        s.FearedBy(CreatureTemplate.Type.Centipede, 0.75f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.75f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.75f);
        s.FearedBy(CreatureTemplate.Type.BlackLizard, 0.8f);
        s.FearedBy(CreatureTemplate.Type.WhiteLizard, 0.8f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.8f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 0.9f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.9f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 0.9f);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 1);
        s.FearedBy(CreatureTemplate.Type.CicadaB, 1);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 1);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 1);
        s.FearedBy(CreatureTemplate.Type.YellowLizard, 1);
        s.FearedBy(CreatureTemplate.Type.CyanLizard, 1);

        // "Attacks" makes this creature go out of their way to attack and kill the given creatures.
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.5f);

        // "AttackedBy" determines how much other creatures will hunt this creature down.
        s.AttackedBy(CreatureTemplate.Type.PinkLizard, 1);
        s.AttackedBy(CreatureTemplate.Type.RedLizard, 1);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);

        // "Eaten By" makes other creatures prey on this one.
        s.EatenBy(CreatureTemplate.Type.RedCentipede, 0.25f);
        s.EatenBy(HailstormEnums.Cyanwing, 0.25f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.25f);
        s.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.25f);
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.4f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.75f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.75f);

        // "Eats" determines how eager this creature is to make a meal out of the given creatures.
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.1f);
        s.Eats(HailstormEnums.InfantAquapede, 0.1f);
        s.Eats(CreatureTemplate.Type.TubeWorm, 0.2f);
        s.Eats(CreatureTemplate.Type.Hazer, 0.35f);
        s.Eats(CreatureTemplate.Type.VultureGrub, 0.4f);
        s.Eats(CreatureTemplate.Type.JetFish, 0.4f);
        s.Eats(CreatureTemplate.Type.LizardTemplate, 0.5f);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 0.5f);
        s.Eats(HailstormEnums.Luminescipede, 0.6f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.75f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.75f);
        s.Eats(CreatureTemplate.Type.DropBug, 0.75f);
        s.Eats(CreatureTemplate.Type.BigSpider, 0.8f);        
        s.Eats(CreatureTemplate.Type.SpitterSpider, 0.8f);
        s.Eats(CreatureTemplate.Type.CicadaA, 0.85f);
        s.Eats(CreatureTemplate.Type.CicadaB, 0.85f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.9f);
        s.Eats(CreatureTemplate.Type.Centipede, 0.95f);
        s.Eats(CreatureTemplate.Type.LanternMouse, 0.95f);
        s.Eats(CreatureTemplate.Type.Centiwing, 0.95f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.95f);
        s.Eats(CreatureTemplate.Type.Slugcat, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        s.Eats(CreatureTemplate.Type.Scavenger, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        s.Eats(CreatureTemplate.Type.RedCentipede, 1);
        s.Eats(HailstormEnums.Cyanwing, 1);
        s.Eats(CreatureTemplate.Type.EggBug, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1.1f);

        // I don't think this has any default interactions or anything. Without adding code, this may just act like Ignores.
        s.IsInPack(CreatureTemplate.Type.BlueLizard, 0.66f);
        s.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.75f);
        s.IsInPack(HailstormEnums.IcyBlue, 1);
        s.IsInPack(HailstormEnums.Freezer, 1);

        s.Ignores(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard);
        s.Ignores(CreatureTemplate.Type.Spider);
        s.Ignores(CreatureTemplate.Type.PoleMimic);
        s.Ignores(HailstormEnums.Chillipede);

        s.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        s.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        s.IgnoredBy(HailstormEnums.Chillipede);

        // I think this makes the creature avoid
        s.UncomfortableAround(CreatureTemplate.Type.TentaclePlant, 0.4f);
        s.UncomfortableAround(CreatureTemplate.Type.Snail, 0.4f);

        // "IntimidatedBy" makes this creature less willing to attack the given creatures.
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 0.5f);

        // If multiple relationship types are set with the same creature, the lowest one will overwrite the rest.
        // Here, the Freezer is set to eat all lizard types, BUUUUUUT four lizards are set as its pack afterwards, so it WON'T horribly maul those guys.
        // You can override all of this by hooking DynamicRelationship-related methods in AI code. There's a catch, though: UpdateDynamicRelationship methods only work if the other creature is alive. THESE relationships always take effect.
        // Do what you will with that info.
    }

    // Creates your physical creature in-game, and assign it an AI type and a State type.
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absFrz) => new LizardAI(absFrz, absFrz.world);
    public override Creature CreateRealizedCreature(AbstractCreature absFrz) => new Lizard(absFrz, absFrz.world);
    public override CreatureState CreateState(AbstractCreature absFrz) => new ColdLizState(absFrz);

    public override void LoadResources(RainWorld rainWorld)
    { 
    }

    #nullable enable
    // I think this gives the game a backup creature to use if Arena Mode tries to spawn your creature while it's not unlocked yet.
    public override CreatureTemplate.Type? ArenaFallback() => HailstormEnums.IcyBlue;
    #nullable disable
}

//----------------------------------------------------------------------------------

sealed class PeachSpiderCritob : Critob
{

    public Color PeachSpiderColor = Custom.HSL2RGB(209 / 360f, 0.9f, 0.75f);

    internal PeachSpiderCritob() : base(HailstormEnums.PeachSpider)
    {
        Icon = new SimpleIcon("Kill_Peach_Spider", PeachSpiderColor);
        LoadedPerformanceCost = 40f;
        SandboxPerformanceCost = new(0.6f, 0.6f);
        RegisterUnlock(KillScore.Configurable(2), HailstormEnums.PeachSpiderUnlock);
    }

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature absSpd) => PeachSpiderColor;

    public override string DevtoolsMapName(AbstractCreature absSpd) => "Pch";

    public override IEnumerable<string> WorldFileAliases()
    {
        return new[] { "peach", "peachspider", "Peach", "PeachSpider" };
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[] { RoomAttractivenessPanel.Category.LikesOutside };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(CreatureTemplate.Type.BigSpider, Type, "Peach Spider")
        {
            TileResistances = new()
            {
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(1, PathCost.Legality.Allowed),
                Climb = new(2, PathCost.Legality.Allowed),
                Wall = new(3, PathCost.Legality.Allowed),
                Ceiling = new(4, PathCost.Legality.Allowed),
                OffScreen = new(1, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, PathCost.Legality.Allowed),
                OpenDiagonal = new(3, PathCost.Legality.Allowed),
                ReachOverGap = new(3, PathCost.Legality.Allowed),
                ReachUp = new(2, PathCost.Legality.Allowed),
                ReachDown = new(2, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2, PathCost.Legality.Allowed),
                DropToFloor = new(10, PathCost.Legality.Allowed),
                DropToWater = new(10, PathCost.Legality.Allowed),
                DropToClimb = new(5, PathCost.Legality.Allowed),
                ShortCut = new(2, PathCost.Legality.Allowed),
                NPCTransportation = new(2, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed),
                BetweenRooms = new(5, PathCost.Legality.Allowed),
                Slope = new(1.5f, PathCost.Legality.Allowed),
                CeilingSlope = new(1.5f, PathCost.Legality.Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            DamageResistances = new() { Base = 0.6f },
            StunResistances = new() { Base = 2 },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BigSpider),
        }.IntoTemplate();
        cf.canSwim = true;
        cf.meatPoints = 2;
        cf.visualRadius = 1200;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        cf.waterPathingResistance = 4f;
        cf.instantDeathDamageLimit = 1f;
        cf.grasps = 1;
        cf.shortcutSegments = 2;
        cf.requireAImap = true;
        cf.offScreenSpeed = 0.5f;
        cf.abstractedLaziness = 50;
        cf.stowFoodInDen = true;
        cf.dangerousToPlayer = 0.25f;
        cf.communityInfluence = 0.05f;
        cf.bodySize = 0.6f;
        cf.interestInOtherAncestorsCatches = 0f;
        cf.interestInOtherCreaturesCatches = 2f;
        cf.usesNPCTransportation = true;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.jumpAction = "Pounce";
        cf.pickupAction = "Grab";
        cf.throwAction = "Release";
        cf.shortcutSegments = 1;
        return cf;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.PeachSpider);

        s.Fears(CreatureTemplate.Type.BigSpider, 0.5f);
        s.Fears(CreatureTemplate.Type.SpitterSpider, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);
        s.Fears(HailstormEnums.Luminescipede, 1);

        s.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.4f);
        s.EatenBy(CreatureTemplate.Type.Spider, 0.5f);
        s.EatenBy(CreatureTemplate.Type.BigSpider, 0.5f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.6f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);

        s.Eats(CreatureTemplate.Type.Spider, 0.7f);

        s.Ignores(HailstormEnums.PeachSpider);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absSpd) => new BigSpiderAI(absSpd, absSpd.world);

    public override Creature CreateRealizedCreature(AbstractCreature absSpd) => new BigSpider(absSpd, absSpd.world);

    public override CreatureState CreateState(AbstractCreature absSpd) => new HealthState(absSpd);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BigSpider;
#nullable disable
}

//----------------------------------------------------------------------------------

sealed class CyanwingCritob : Critob
{

    public Color CyanwingColor = Custom.HSL2RGB(180 / 360f, 0.88f, 0.4f);

    internal CyanwingCritob() : base(HailstormEnums.Cyanwing)
    {
        Icon = new SimpleIcon("Kill_Cyanwing", CyanwingColor);
        LoadedPerformanceCost = 15f;
        SandboxPerformanceCost = new(1.15f, 0.75f);
        RegisterUnlock(KillScore.Configurable(25), HailstormEnums.CyanwingUnlock);
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

    public override IEnumerable<string> WorldFileAliases() => new[] { "cyanwing", "Cyanwing" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.LikesOutside,
        RoomAttractivenessPanel.Category.Flying
    };

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
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            DamageResistances = new() { Base = 1, Explosion = 2, Electric = 100 },
            StunResistances = new() { Base = 1, Electric = 100 },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Centiwing),
        }.IntoTemplate();
        cf.canFly = true;
        cf.meatPoints = 12;
        cf.visualRadius = 600;
        cf.waterVision = 0.3f;
        cf.throughSurfaceVision = 1f;
        cf.movementBasedVision = 0.15f;
        cf.dangerousToPlayer = 0.8f;
        cf.communityInfluence = 0.25f;
        cf.bodySize = 7f;
        cf.socialMemory = true;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.jumpAction = "Swap Heads";
        cf.pickupAction = "Grab/Shock";
        cf.throwAction = "Release";
        cf.shortcutSegments = 5;
        return cf;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.Cyanwing);

        s.IsInPack(CreatureTemplate.Type.Centiwing, 1);

        s.Ignores(CreatureTemplate.Type.TentaclePlant);
        s.Ignores(HailstormEnums.InfantAquapede);

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
        s.FearedBy(HailstormEnums.Luminescipede, 1);
        s.FearedBy(HailstormEnums.Chillipede, 1);

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

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCnt) => new CentipedeAI(absCnt, absCnt.world);

    public override Creature CreateRealizedCreature(AbstractCreature absCnt) => new Centipede(absCnt, absCnt.world);

    public override CreatureState CreateState(AbstractCreature absCnt) => new Centipede.CentipedeState(absCnt);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centiwing;
#nullable disable
}

//----------------------------------------------------------------------------------

sealed class GorditoGreenieCritob : Critob
{

    public Color GorditoGreenieColor = Custom.HSL2RGB(135 / 360f, 0.5f, 0.7f);

    internal GorditoGreenieCritob() : base(HailstormEnums.GorditoGreenie)
    {
        Icon = new SimpleIcon("Kill_Gordito_Greenie_Lizard", GorditoGreenieColor);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(0.7f, 0.6f);
        RegisterUnlock(KillScore.Configurable(25), HailstormEnums.GorditoGreenieUnlock);
    }

    public override int ExpeditionScore() => 25;

    public override Color DevtoolsMapColor(AbstractCreature absLiz) => GorditoGreenieColor;

    public override string DevtoolsMapName(AbstractCreature absLiz) => "GG";

    public override IEnumerable<string> WorldFileAliases() => new[] { "gordito", "gorditogreenie", "Gordito", "GorditoGreenie" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesOutside
    };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate gorditoTemp = LizardBreeds.BreedTemplate(HailstormEnums.GorditoGreenie, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
        CreatureTemplate dummyTemp = new CreatureFormula(CreatureTemplate.Type.GreenLizard, Type, "not a real template!!! i'm using this to set the Gordito's Connection Resistances!")
        {
            ConnectionResistances = new()
            {
                ShortCut = new(1, PathCost.Legality.Unallowed),
                BetweenRooms = new(1, PathCost.Legality.Unallowed),
                NPCTransportation = new(1, PathCost.Legality.Unallowed),
                RegionTransportation = new(1, PathCost.Legality.Unallowed),
                BigCreatureShortCutSqueeze = new(100, PathCost.Legality.Allowed),
                LizardTurn = new(80, PathCost.Legality.Allowed),
                DropToFloor = new(20, PathCost.Legality.Allowed),
            },
        }.IntoTemplate();
        gorditoTemp.pathingPreferencesConnections = dummyTemp.pathingPreferencesConnections;
        return gorditoTemp;
    }

    // This method sets this creature's relationships with other creatures.
    // I explain what I can, but if some relationship types don't have an explanation, that means I don't know what they do.
    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.GorditoGreenie);

        s.Rivals(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        s.Rivals(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 1);
        s.Rivals(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
        s.Rivals(HailstormEnums.GorditoGreenie, 1);

        // "HasDynamicRelationship" allows other creatures to have a changing relationship with this one.
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.25f);

        s.Ignores(CreatureTemplate.Type.Overseer);
        s.Ignores(CreatureTemplate.Type.GarbageWorm);
        s.Ignores(CreatureTemplate.Type.Fly);
        s.Ignores(CreatureTemplate.Type.Vulture);
        s.Ignores(CreatureTemplate.Type.Spider);
        s.Ignores(CreatureTemplate.Type.Leech);
        s.Ignores(CreatureTemplate.Type.SeaLeech);
        s.Ignores(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech);

        // "IgnoredBy" makes the given creatures ignore this one.
        s.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        s.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        s.IgnoredBy(CreatureTemplate.Type.Spider);
        s.IgnoredBy(CreatureTemplate.Type.Leech);
        s.IgnoredBy(CreatureTemplate.Type.SeaLeech);
        s.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech);

        // "IntimidatedBy" makes this creature less willing to attack the given creatures.
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.25f);
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.5f);

        // "Intimidates" deters other creatures from attacking this one.
        s.Intimidates(CreatureTemplate.Type.DropBug, 0.2f);
        s.Intimidates(CreatureTemplate.Type.Centiwing, 0.3f);
        s.Intimidates(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);
        s.Intimidates(CreatureTemplate.Type.Scavenger, 0.3f);
        s.Intimidates(CreatureTemplate.Type.Centipede, 0.5f);
        s.Intimidates(CreatureTemplate.Type.BigSpider, 0.5f);
        s.Intimidates(CreatureTemplate.Type.SpitterSpider, 0.5f);
        s.Intimidates(CreatureTemplate.Type.TentaclePlant, 0.66f);
        s.Intimidates(CreatureTemplate.Type.PoleMimic, 1f);
        s.Intimidates(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1f);

        // "FearedBy" causes other creatures to actively avoid this creature.
        s.FearedBy(CreatureTemplate.Type.JetFish, 0.33f);
        s.FearedBy(HailstormEnums.InfantAquapede, 0.33f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.5f);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.5f);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 0.5f);
        s.FearedBy(CreatureTemplate.Type.CicadaB, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Slugcat, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        s.FearedBy(CreatureTemplate.Type.EggBug, 1);

        // "Fears" makes this creature actively avoid the listed creatures.
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.66f);
        s.Fears(CreatureTemplate.Type.BigEel, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
     
        // "AttackedBy" determines how much other creatures will hunt this creature down.
        s.AttackedBy(CreatureTemplate.Type.Scavenger, 0.3f);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.5f);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.5f);
        s.AttackedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.75f);

        s.Attacks(CreatureTemplate.Type.BrotherLongLegs, 0.5f);
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 0.5f);
        s.Attacks(CreatureTemplate.Type.MirosBird, 1);
        s.Attacks(CreatureTemplate.Type.KingVulture, 1);
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1);

        // "Eaten By" makes other creatures prey on this one.
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.25f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.25f);
        s.EatenBy(CreatureTemplate.Type.RedCentipede, 0.25f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 0.5f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.5f);

        // "Eats" determines how eager this creature is to make a meal out of the given creatures.
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.5f);
        s.Eats(CreatureTemplate.Type.Slugcat, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        s.Eats(CreatureTemplate.Type.CicadaA, 1);
        s.Eats(CreatureTemplate.Type.CicadaB, 1);
        s.Eats(CreatureTemplate.Type.EggBug, 1);
        s.Eats(CreatureTemplate.Type.VultureGrub, 1);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 1);
        s.Eats(CreatureTemplate.Type.Centipede, 1);
        s.Eats(CreatureTemplate.Type.RedCentipede, 1);
        s.Eats(CreatureTemplate.Type.Centiwing, 1);
        s.Eats(HailstormEnums.Cyanwing, 1);
        s.Eats(HailstormEnums.InfantAquapede, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 1);
        s.Eats(CreatureTemplate.Type.Hazer, 1);
        s.Eats(CreatureTemplate.Type.Snail, 1);
        s.Eats(CreatureTemplate.Type.JetFish, 1);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 1);
        s.Eats(CreatureTemplate.Type.DropBug, 1);
        s.Eats(CreatureTemplate.Type.BigSpider, 1);
        s.Eats(CreatureTemplate.Type.SpitterSpider, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 1);
        s.Eats(HailstormEnums.Luminescipede, 1);
        s.Eats(CreatureTemplate.Type.LanternMouse, 1);
        s.Eats(CreatureTemplate.Type.TubeWorm, 1);
        s.Eats(CreatureTemplate.Type.Scavenger, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        s.Eats(HailstormEnums.Chillipede, 1);
        s.Ignores(HailstormEnums.PeachSpider);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absLiz) => new LizardAI(absLiz, absLiz.world);
    public override Creature CreateRealizedCreature(AbstractCreature absLiz) => new Lizard(absLiz, absLiz.world);
    public override CreatureState CreateState(AbstractCreature absLiz) => new LizardState(absLiz);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.GreenLizard;
#nullable disable
}

//----------------------------------------------------------------------------------

sealed class LuminescipedeCritob : Critob
{

    public Color LuminescipedeColor = Custom.hexToColor("DCCCFF");

    internal LuminescipedeCritob() : base(HailstormEnums.Luminescipede)
    {
        Icon = new SimpleIcon("Kill_Luminescipede", LuminescipedeColor);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.4f, 0.6f);
        RegisterUnlock(KillScore.Configurable(1), HailstormEnums.LuminescipedeUnlock);
    }

    public override int ExpeditionScore() => 1;

    public override Color DevtoolsMapColor(AbstractCreature absLmn) => LuminescipedeColor;

    public override string DevtoolsMapName(AbstractCreature absLmn) => "lmn";

    public override IEnumerable<string> WorldFileAliases() => new[] { "lumin", "Lumin", "luminescipede", "Luminescipede" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.All };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(null, Type, "Luminescipede")
        {
            TileResistances = new()
            {
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(20, PathCost.Legality.Unwanted),
                Climb = new(3, PathCost.Legality.Allowed),
                Wall = new(2, PathCost.Legality.Allowed),
                Ceiling = new(5, PathCost.Legality.Allowed),
                OffScreen = new(1, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1, PathCost.Legality.Allowed),
                OpenDiagonal = new(2, PathCost.Legality.Allowed),
                NPCTransportation = new(10, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed),
                BetweenRooms = new(1, PathCost.Legality.Allowed),
                CeilingSlope = new(1, PathCost.Legality.Allowed),
                DropToFloor = new(3, PathCost.Legality.Allowed),
                DropToClimb = new(3, PathCost.Legality.Allowed),
                DropToWater = new(10, PathCost.Legality.Allowed),
                ShortCut = new(2, PathCost.Legality.Allowed),
                Slope = new(1, PathCost.Legality.Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1),
            InstantDeathDamage = 1,
            DamageResistances = new() { Base = 1, Electric = 2, Blunt = 0.5f },
            StunResistances = new() { Base = 1, Electric = 2, Blunt = 0.5f },
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Snail),
            HasAI = true
        }.IntoTemplate();
        cf.bodySize = 0.75f;
        cf.grasps = 1;
        cf.countsAsAKill = 2;
        cf.meatPoints = 1;
        cf.dangerousToPlayer = 0.25f;
        cf.communityID = CreatureCommunities.CommunityID.None;
        cf.communityInfluence = 0.25f;
        cf.canSwim = true;
        cf.visualRadius = 900;
        cf.throughSurfaceVision = 0.5f;
        cf.waterVision = 0.5f;
        cf.waterPathingResistance = 50f;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        cf.lungCapacity = 1200f;
        cf.usesCreatureHoles = true;
        cf.usesNPCTransportation = true;
        cf.requireAImap = true;
        cf.doPreBakedPathing = false;
        cf.offScreenSpeed = 0.75f;
        cf.abstractedLaziness = 50;
        cf.roamInRoomChance = 0.5f;
        cf.roamBetweenRoomsChance = 0.5f;
        cf.interestInOtherAncestorsCatches = 0;
        cf.interestInOtherCreaturesCatches = 0.5f;
        cf.stowFoodInDen = true;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.shortcutSegments = 2;
        cf.scaryness = 1.5f;
        cf.pickupAction = "Grab | Hold - Swap Grasps";
        cf.throwAction = "Release / Throw";
        cf.jumpAction = "Camouflage";

        return cf;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.Luminescipede);
        //--------------------
        s.Eats(CreatureTemplate.Type.TubeWorm, 0.25f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.25f);
        s.Eats(CreatureTemplate.Type.BigNeedleWorm, 0.25f);
        s.Eats(CreatureTemplate.Type.TubeWorm, 0.25f);
        s.Eats(CreatureTemplate.Type.JetFish, 0.50f);
        s.Eats(CreatureTemplate.Type.CicadaA, 0.75f);
        s.Eats(CreatureTemplate.Type.CicadaB, 0.75f);
        s.Eats(CreatureTemplate.Type.Centiwing, 0.75f);

        s.EatenBy(CreatureTemplate.Type.Vulture, 0.1f);
        s.EatenBy(CreatureTemplate.Type.KingVulture, 0.15f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.2f);
        s.EatenBy(CreatureTemplate.Type.Spider, 0.3f);
        s.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.5f);
        s.EatenBy(CreatureTemplate.Type.GreenLizard, 0.3f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.3f);
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.4f);
        s.EatenBy(HailstormEnums.Freezer, 0.6f);
        s.EatenBy(CreatureTemplate.Type.BigSpider, 0.6f);
        s.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.7f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.8f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.9f);
        s.EatenBy(CreatureTemplate.Type.BlackLizard, 1);
        s.EatenBy(CreatureTemplate.Type.BigEel, 1);
        s.EatenBy(HailstormEnums.GorditoGreenie, 1);
        //-----
        s.Attacks(CreatureTemplate.Type.Leech, 0.2f);
        s.Attacks(CreatureTemplate.Type.SeaLeech, 0.25f);
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.33f);
        s.Attacks(CreatureTemplate.Type.EggBug, 1);
        s.Attacks(CreatureTemplate.Type.PoleMimic, 1);
        s.Attacks(CreatureTemplate.Type.TentaclePlant, 1);
        s.Attacks(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 1);

        s.AttackedBy(CreatureTemplate.Type.WhiteLizard, 0.3f);
        //-----
        s.IntimidatedBy(CreatureTemplate.Type.Snail, 0.50f);
        s.IntimidatedBy(HailstormEnums.Chillipede, 0.75f);
        s.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
        //-----
        s.Fears(CreatureTemplate.Type.MirosBird, 0.5f);
        s.Fears(CreatureTemplate.Type.BigEel, 1);
        s.Fears(CreatureTemplate.Type.BrotherLongLegs, 1);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 1);
        s.Fears(CreatureTemplate.Type.RedLizard, 1);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
        s.Fears(HailstormEnums.GorditoGreenie, 1);
        s.Fears(HailstormEnums.Freezer, 1);

        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.25f);
        s.FearedBy(HailstormEnums.InfantAquapede, 0.25f);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Scavenger, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.5f);
        s.FearedBy(HailstormEnums.PeachSpider, 0.5f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.75f);
        s.FearedBy(CreatureTemplate.Type.VultureGrub, 1);
        s.FearedBy(CreatureTemplate.Type.Hazer, 1);

        //-----
        s.Ignores(CreatureTemplate.Type.Overseer);
        s.Ignores(CreatureTemplate.Type.GarbageWorm);
        s.Ignores(CreatureTemplate.Type.Deer);
        s.Ignores(CreatureTemplate.Type.TempleGuard);
        //-----
        s.IsInPack(HailstormEnums.Luminescipede, 1);
        //s.IsInPack(HailstormEnums.StrobeLegs, 1);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absLmn) => new LuminAI(absLmn, absLmn.world);
    public override Creature CreateRealizedCreature(AbstractCreature absLmn) => new LuminCreature(absLmn, absLmn.world);
    public override CreatureState CreateState(AbstractCreature absLmn) => new GlowSpiderState(absLmn);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Spider;
#nullable disable
}

//----------------------------------------------------------------------------------

sealed class ChillipedeCritob : Critob
{

    public Color ChillipedeColor = Custom.hexToColor("7FD8FF");

    internal ChillipedeCritob() : base(HailstormEnums.Chillipede)
    {
        Icon = new SimpleIcon("Kill_Chillipede", ChillipedeColor);
        LoadedPerformanceCost = 15f;
        SandboxPerformanceCost = new(0.9f, 0.75f);
        RegisterUnlock(KillScore.Configurable(12), HailstormEnums.ChillipedeUnlock);
    }

    public override int ExpeditionScore() => 12;

    public override Color DevtoolsMapColor(AbstractCreature absChl) => ChillipedeColor;

    public override string DevtoolsMapName(AbstractCreature absChl) => "chl";

    public override IEnumerable<string> WorldFileAliases() => new[] { "chillipede", "Chillipede" };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[]
    {
        RoomAttractivenessPanel.Category.LikesInside
    };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(CreatureTemplate.Type.Centipede, Type, "Chillipede")
        {
            TileResistances = new()
            {
                OffScreen = new(2, PathCost.Legality.Allowed),
                Floor = new(1, PathCost.Legality.Allowed),
                Corridor = new(25, PathCost.Legality.Allowed),
                Climb = new(1, PathCost.Legality.Allowed),
                Wall = new(1, PathCost.Legality.Allowed),
                Ceiling = new(1, PathCost.Legality.Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1, PathCost.Legality.Allowed),
                ReachOverGap = new(3, PathCost.Legality.Allowed),
                ReachUp = new(1.1f, PathCost.Legality.Allowed),
                DoubleReachUp = new(3, PathCost.Legality.Allowed),
                ReachDown = new(1.1f, PathCost.Legality.Allowed),
                SemiDiagonalReach = new(2, PathCost.Legality.Allowed),
                DropToFloor = new(10, PathCost.Legality.Allowed),
                DropToClimb = new(7, PathCost.Legality.Allowed),
                DropToWater = new(50, PathCost.Legality.Unwanted),
                OpenDiagonal = new(3, PathCost.Legality.Allowed),
                Slope = new(2, PathCost.Legality.Allowed),
                CeilingSlope = new(2, PathCost.Legality.Allowed),
                ShortCut = new(20, PathCost.Legality.Allowed),
                NPCTransportation = new(50, PathCost.Legality.Allowed),
                BigCreatureShortCutSqueeze = new(10, PathCost.Legality.Allowed),
                BetweenRooms = new(10, PathCost.Legality.Allowed),
                OffScreenMovement = new(1, PathCost.Legality.Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1),
            DamageResistances = new() { Base = 3, Electric = 1 },
            StunResistances = new() { Base = 5, Electric = 0.5f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Centipede),
        }.IntoTemplate();
        cf.meatPoints = 6;
        cf.visualRadius = 1500;
        cf.waterVision = 0.3f;
        cf.stowFoodInDen = true;
        cf.throughSurfaceVision = 0.5f;
        cf.movementBasedVision = 0.5f;
        cf.dangerousToPlayer = 0.45f;
        cf.communityInfluence = 0.1f;
        cf.bodySize = 2;
        cf.usesCreatureHoles = false;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        cf.lungCapacity = 1600;
        cf.jumpAction = "Swap Heads";
        cf.pickupAction = "Grab/Freeze";
        cf.throwAction = "Release";
        cf.shortcutSegments = 3;
        return cf;
    }

    public override void EstablishRelationships()
    {
        Relationships s = new(HailstormEnums.Chillipede);

        s.Eats(CreatureTemplate.Type.Spider, 0.4f);
        s.Eats(CreatureTemplate.Type.BigSpider, 0.4f);
        s.Eats(CreatureTemplate.Type.SpitterSpider, 0.4f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.4f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.4f);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 0.4f);
        s.Eats(CreatureTemplate.Type.Centipede, 0.4f);
        s.Eats(CreatureTemplate.Type.Centiwing, 0.4f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        s.Eats(HailstormEnums.InfantAquapede, 0.4f);

        s.EatenBy(CreatureTemplate.Type.SmallCentipede, 0.4f);
        s.EatenBy(CreatureTemplate.Type.SmallCentipede, 0.4f);
        s.EatenBy(CreatureTemplate.Type.Centipede, 0.4f);
        s.EatenBy(CreatureTemplate.Type.RedCentipede, 0.4f);
        s.EatenBy(CreatureTemplate.Type.Centiwing, 0.4f);
        s.EatenBy(HailstormEnums.Cyanwing, 0.4f);
        s.EatenBy(HailstormEnums.InfantAquapede, 0.4f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.4f);
        s.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.5f);
        s.EatenBy(CreatureTemplate.Type.GreenLizard, 0.6f);
        s.EatenBy(CreatureTemplate.Type.RedLizard, 0.7f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.8f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.8f);
        s.EatenBy(HailstormEnums.GorditoGreenie, 1);

        s.Intimidates(CreatureTemplate.Type.BigSpider, 0.5f);
        s.Intimidates(HailstormEnums.Luminescipede, 0.75f);

        s.Fears(CreatureTemplate.Type.RedCentipede, 1);
        s.Fears(HailstormEnums.Cyanwing, 1);

        s.FearedBy(CreatureTemplate.Type.Spider, 0.5f);
        s.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.4f);

        s.Ignores(HailstormEnums.Chillipede);
        s.Ignores(HailstormEnums.IcyBlue);
        s.Ignores(HailstormEnums.Freezer);
        s.IgnoredBy(HailstormEnums.IcyBlue);
        s.IgnoredBy(HailstormEnums.Freezer);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absChl) => new CentipedeAI(absChl, absChl.world);

    public override Creature CreateRealizedCreature(AbstractCreature absChl) => new Centipede(absChl, absChl.world);

    public override CreatureState CreateState(AbstractCreature absChl) => new ChillipedeState(absChl);

    public override void LoadResources(RainWorld rainWorld)
    {
    }

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centipede;
#nullable disable
}

//----------------------------------------------------------------------------------
