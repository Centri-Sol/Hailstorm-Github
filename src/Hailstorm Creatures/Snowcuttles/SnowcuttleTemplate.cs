namespace Hailstorm;

public class SnowcuttleTemplate : Critob
{
    public virtual Color SnowcuttleColor => new HSLColor(240 / 360f, 0.04f, 0.35f).rgb;

    internal SnowcuttleTemplate(CreatureTemplate.Type snowcuttleType, MultiplayerUnlocks.SandboxUnlockID sandboxUnlock, MultiplayerUnlocks.SandboxUnlockID unlockParent) : base(snowcuttleType)
    {
        Icon = new SimpleIcon("Kill_Snowcuttle", SnowcuttleColor);
        LoadedPerformanceCost = 25f;
        SandboxPerformanceCost = new(0.4f, 0.5f);
        if (sandboxUnlock is not null)
        {
            RegisterUnlock(KillScore.Configurable(2), sandboxUnlock, unlockParent);
        }
    }
    public override int ExpeditionScore() => 2;

    public override string DevtoolsMapName(AbstractCreature absChl) => "ctl";
    public override Color DevtoolsMapColor(AbstractCreature absChl) => SnowcuttleColor;
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.LikesOutside,
            RoomAttractivenessPanel.Category.Flying
        };
    }

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate scTemp = new CreatureFormula(CreatureTemplate.Type.CicadaA, Type, "Snowcuttle Template")
        {
            DamageResistances = new()
            {
                Base = 0.35f,
                Blunt = 0.5f,
                Electric = 0.5f,
            },
            StunResistances = new()
            {
                Base = 1,
                Blunt = 0.5f,
                Electric = 0.5f,
            },
            InstantDeathDamage = 0.5f,
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.6f),

        }.IntoTemplate();
        scTemp.virtualCreature = true;
        scTemp.meatPoints = 1;
        scTemp.BlizzardAdapted = true;
        scTemp.BlizzardWanderer = true;
        scTemp.shortcutSegments = 1;

        scTemp.visualRadius = 50;
        scTemp.throughSurfaceVision = 0.1f;
        scTemp.waterVision = 0f;
        scTemp.movementBasedVision = 0.2f;

        scTemp.bodySize = 0.25f;
        scTemp.offScreenSpeed = 0.6f;
        scTemp.roamInRoomChance = 0.2f;
        scTemp.roamBetweenRoomsChance = 0.2f;
        scTemp.abstractedLaziness = 60;

        scTemp.pickupAction = "Grab";
        //scTemp.communityInfluence = 1f;
        //scTemp.communityID = CreatureCommunities.CommunityID.Cicadas;
        return scTemp;
    }
    public override void EstablishRelationships()
    {
        Relationships scTempRelation = new(HSEnums.CreatureType.SnowcuttleTemplate);

        // Will bash and nibble at these creatures until they die.
        scTempRelation.Attacks(CreatureTemplate.Type.Spider, 1);
        scTempRelation.Attacks(CreatureTemplate.Type.TubeWorm, 0.95f);
        scTempRelation.Attacks(CreatureTemplate.Type.Overseer, 0.01f);

        // Flies away from these creatures if they get too close.
        // Called "StayOutOfWay" in the game's code.
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.Centipede, 1);
        scTempRelation.IntimidatedBy(HSEnums.CreatureType.InfantAquapede, 1);
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 1);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.LizardTemplate, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.SeaLeech, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.Snail, 0.7f);
        scTempRelation.IntimidatedBy(DLCSharedEnums.CreatureTemplateType.Yeek, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.GarbageWorm, 0.3f);
        scTempRelation.IntimidatedBy(DLCSharedEnums.CreatureTemplateType.BigJelly, 0.3f);

        // Flies away from these creatures if they are anywhere nearby, showing active fear.
        // (Flee radius is much larger than with IntimidatedBy/StayOutOfWay.)
        scTempRelation.Fears(HSEnums.CreatureType.Raven, 1);
        //scTempRelation.Fears(HSEnums.CreatureType.Strobelegs, 1);
        scTempRelation.Fears(HSEnums.CreatureType.Luminescipede, 1);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, 1);
        scTempRelation.Fears(CreatureTemplate.Type.SpitterSpider, 0.9f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.ZoopLizard, 1);
        scTempRelation.Fears(HSEnums.CreatureType.FreezerLizard, 0.9f);
        scTempRelation.Fears(HSEnums.CreatureType.Cyanwing, 0.9f);
        scTempRelation.Fears(CreatureTemplate.Type.Vulture, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.CyanLizard, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.RedCentipede, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.DropBug, 0.7f);
        scTempRelation.Fears(CreatureTemplate.Type.Centiwing, 0.7f);
        scTempRelation.Fears(CreatureTemplate.Type.RedLizard, 0.7f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.ScavengerElite, 0.6f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.WhiteLizard, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.BigSpider, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.6f);
        scTempRelation.Fears(HSEnums.CreatureType.PeachSpider, 0.9f);
        scTempRelation.Fears(CreatureTemplate.Type.Slugcat, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.MirosBird, 0.5f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.MirosVulture, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.KingVulture, 0.5f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.SpitLizard, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.Scavenger, 0.4f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.StowawayBug, 0.4f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.4f);
        scTempRelation.Fears(HSEnums.CreatureType.IcyBlueLizard, 0.4f);
        scTempRelation.Fears(HSEnums.CreatureType.GorditoGreenieLizard, 0.3f);
        scTempRelation.Fears(CreatureTemplate.Type.BigEel, 0.3f);
        scTempRelation.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.Inspector, 0.3f);
        scTempRelation.Fears(DLCSharedEnums.CreatureTemplateType.AquaCenti, 0.1f);

        // Hangs out near these creatures when it has nothing else to do, and flies to them for protection if nearby and in danger.
        //scTemp.IsInPack(HailstormEnums.BezanBud, 1f);
        scTempRelation.IsInPack(CreatureTemplate.Type.TentaclePlant, 0.6f);
        scTempRelation.IsInPack(CreatureTemplate.Type.CicadaA, 0.4f);
        scTempRelation.IsInPack(CreatureTemplate.Type.CicadaB, 0.4f);
        scTempRelation.IsInPack(HSEnums.CreatureType.SnowcuttleTemplate, 0.2f);
        scTempRelation.IsInPack(CreatureTemplate.Type.PoleMimic, 0.1f);

        scTempRelation.Ignores(CreatureTemplate.Type.Fly);
        scTempRelation.Ignores(CreatureTemplate.Type.SmallNeedleWorm);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        scTempRelation.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.15f);

        scTempRelation.EatenBy(HSEnums.CreatureType.GorditoGreenieLizard, 1);
        scTempRelation.EatenBy(HSEnums.CreatureType.PeachSpider, 0.7f);
        scTempRelation.EatenBy(HSEnums.CreatureType.IcyBlueLizard, 0.66f);
        scTempRelation.EatenBy(HSEnums.CreatureType.FreezerLizard, 0.66f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        scTempRelation.EatenBy(HSEnums.CreatureType.Raven, 0.5f);
        scTempRelation.EatenBy(HSEnums.CreatureType.Luminescipede, 0.5f);
        //scTempRelation.EatenBy(HSEnums.CreatureType.Strobelegs, 0.5f);
        scTempRelation.EatenBy(CreatureTemplate.Type.CyanLizard, 0.3f);
        scTempRelation.EatenBy(CreatureTemplate.Type.WhiteLizard, 0.3f);
        scTempRelation.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.3f);
        scTempRelation.EatenBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard, 0.25f);
        scTempRelation.EatenBy(CreatureTemplate.Type.Leech, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.SeaLeech, 0.2f);
        scTempRelation.EatenBy(DLCSharedEnums.CreatureTemplateType.JungleLeech, 0.2f);
        scTempRelation.EatenBy(DLCSharedEnums.CreatureTemplateType.SpitLizard, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.RedLizard, 0.2f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.JetFish, 0.15f);
        scTempRelation.EatenBy(CreatureTemplate.Type.BigSpider, 0.15f);
        scTempRelation.EatenBy(DLCSharedEnums.CreatureTemplateType.MotherSpider, 0.15f);
        scTempRelation.EatenBy(CreatureTemplate.Type.Vulture, 0.05f);
        scTempRelation.EatenBy(CreatureTemplate.Type.KingVulture, 0.05f);
        scTempRelation.EatenBy(DLCSharedEnums.CreatureTemplateType.MirosVulture, 0.05f);

        scTempRelation.FearedBy(CreatureTemplate.Type.Fly, 0.6f);
        scTempRelation.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);

        scTempRelation.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        scTempRelation.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        scTempRelation.IgnoredBy(CreatureTemplate.Type.LanternMouse);
        scTempRelation.IgnoredBy(DLCSharedEnums.CreatureTemplateType.Yeek);
    }

    public override Creature CreateRealizedCreature(AbstractCreature absCtl) => new Cicada(absCtl, absCtl.world, true);
    public override CreatureState CreateState(AbstractCreature absCtl) => new HealthState(absCtl);
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCtl) => new CicadaAI(absCtl, absCtl.world);
    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absCtl) => new CicadaAbstractAI(absCtl.world, absCtl);

#nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaA;
}