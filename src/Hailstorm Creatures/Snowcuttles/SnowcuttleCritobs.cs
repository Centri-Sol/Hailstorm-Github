namespace Hailstorm;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

class SnowcuttleTemplate : Critob
{
    public static void RegisterSnowcuttles()
    {
        Content.Register(new SnowcuttleTemplate(HailstormCreatures.SnowcuttleTemplate, null, null));
        Content.Register(new SnowcuttleFemaleCritob());
        Content.Register(new SnowcuttleMaleCritob());
        Content.Register(new SnowcuttleLeCritob());
    }

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
        Relationships scTempRelation = new(HailstormCreatures.SnowcuttleTemplate);

        // Will bash and nibble at these creatures until they die.
        scTempRelation.Attacks(CreatureTemplate.Type.Spider, 1);
        scTempRelation.Attacks(CreatureTemplate.Type.TubeWorm, 0.95f);
        scTempRelation.Attacks(CreatureTemplate.Type.Overseer, 0.01f);

        // Flies away from these creatures if they get too close.
        // Called "StayOutOfWay" in the game's code.
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.Centipede, 1);
        scTempRelation.IntimidatedBy(HailstormCreatures.InfantAquapede, 1);
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, 1);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.LizardTemplate, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.SeaLeech, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.Snail, 0.7f);
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.9f);
        scTempRelation.IntimidatedBy(CreatureTemplate.Type.GarbageWorm, 0.3f);
        scTempRelation.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.3f);

        // Flies away from these creatures if they are anywhere nearby, showing active fear.
        // (Flee radius is much larger than with IntimidatedBy/StayOutOfWay.)
        scTempRelation.Fears(HailstormCreatures.Raven, 1);
        //scTempRelation.Fears(HailstormCreatures.Strobelegs, 1);
        scTempRelation.Fears(HailstormCreatures.Luminescipede, 1);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1);
        scTempRelation.Fears(CreatureTemplate.Type.SpitterSpider, 0.9f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 1);
        scTempRelation.Fears(HailstormCreatures.Freezer, 0.9f);
        scTempRelation.Fears(HailstormCreatures.Cyanwing, 0.9f);
        scTempRelation.Fears(CreatureTemplate.Type.Vulture, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.CyanLizard, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.RedCentipede, 0.8f);
        scTempRelation.Fears(CreatureTemplate.Type.DropBug, 0.7f);
        scTempRelation.Fears(CreatureTemplate.Type.Centiwing, 0.7f);
        scTempRelation.Fears(CreatureTemplate.Type.RedLizard, 0.7f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.6f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.WhiteLizard, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.BigSpider, 0.6f);
        scTempRelation.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.6f);
        scTempRelation.Fears(HailstormCreatures.PeachSpider, 0.9f);
        scTempRelation.Fears(CreatureTemplate.Type.Slugcat, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.MirosBird, 0.5f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.KingVulture, 0.5f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.5f);
        scTempRelation.Fears(CreatureTemplate.Type.Scavenger, 0.4f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.4f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.4f);
        scTempRelation.Fears(HailstormCreatures.IcyBlue, 0.4f);
        scTempRelation.Fears(HailstormCreatures.GorditoGreenie, 0.3f);
        scTempRelation.Fears(CreatureTemplate.Type.BigEel, 0.3f);
        scTempRelation.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 0.3f);
        scTempRelation.Fears(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.1f);

        // Hangs out near these creatures when it has nothing else to do, and flies to them for protection if nearby and in danger.
        //scTemp.IsInPack(HailstormEnums.BezanBud, 1f);
        scTempRelation.IsInPack(CreatureTemplate.Type.TentaclePlant, 0.6f);
        scTempRelation.IsInPack(CreatureTemplate.Type.CicadaA, 0.4f);
        scTempRelation.IsInPack(CreatureTemplate.Type.CicadaB, 0.4f);
        scTempRelation.IsInPack(HailstormCreatures.SnowcuttleTemplate, 0.2f);
        scTempRelation.IsInPack(CreatureTemplate.Type.PoleMimic, 0.1f);

        scTempRelation.Ignores(CreatureTemplate.Type.Fly);
        scTempRelation.Ignores(CreatureTemplate.Type.SmallNeedleWorm);

        //  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        scTempRelation.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.15f);

        scTempRelation.EatenBy(HailstormCreatures.GorditoGreenie, 1);
        scTempRelation.EatenBy(HailstormCreatures.PeachSpider, 0.7f);
        scTempRelation.EatenBy(HailstormCreatures.IcyBlue, 0.66f);
        scTempRelation.EatenBy(HailstormCreatures.Freezer, 0.66f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        scTempRelation.EatenBy(HailstormCreatures.Raven, 0.5f);
        scTempRelation.EatenBy(HailstormCreatures.Luminescipede, 0.5f);
        //scTempRelation.EatenBy(HailstormCreatures.Strobelegs, 0.5f);
        scTempRelation.EatenBy(CreatureTemplate.Type.CyanLizard, 0.3f);
        scTempRelation.EatenBy(CreatureTemplate.Type.WhiteLizard, 0.3f);
        scTempRelation.EatenBy(CreatureTemplate.Type.SpitterSpider, 0.3f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.25f);
        scTempRelation.EatenBy(CreatureTemplate.Type.Leech, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.SeaLeech, 0.2f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.2f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.RedLizard, 0.2f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 0.2f);
        scTempRelation.EatenBy(CreatureTemplate.Type.JetFish, 0.15f);
        scTempRelation.EatenBy(CreatureTemplate.Type.BigSpider, 0.15f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.15f);
        scTempRelation.EatenBy(CreatureTemplate.Type.Vulture, 0.05f);
        scTempRelation.EatenBy(CreatureTemplate.Type.KingVulture, 0.05f);
        scTempRelation.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.05f);

        scTempRelation.FearedBy(CreatureTemplate.Type.Fly, 0.6f);
        scTempRelation.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);

        scTempRelation.IgnoredBy(CreatureTemplate.Type.PoleMimic);
        scTempRelation.IgnoredBy(CreatureTemplate.Type.TentaclePlant);
        scTempRelation.IgnoredBy(CreatureTemplate.Type.LanternMouse);
        scTempRelation.IgnoredBy(MoreSlugcatsEnums.CreatureTemplateType.Yeek);
    }

    public override Creature CreateRealizedCreature(AbstractCreature absCtl) => new Cicada(absCtl, absCtl.world, true);
    public override CreatureState CreateState(AbstractCreature absCtl) => new HealthState(absCtl);
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absCtl) => new CicadaAI(absCtl, absCtl.world);
    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absCtl) => new CicadaAbstractAI(absCtl.world, absCtl);

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaA;
    #nullable disable
}

//----------------------------------------------------------------------------------

sealed class SnowcuttleFemaleCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(220/360f, 0.25f, 0.6f);

    internal SnowcuttleFemaleCritob() : base(HailstormCreatures.SnowcuttleFemale, HailstormUnlocks.SnowcuttleFemale, null) {}
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlF";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleF" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate snwCtlFemale = new CreatureFormula(HailstormCreatures.SnowcuttleTemplate, Type, "Snowcuttle Female").IntoTemplate();
        snwCtlFemale.visualRadius = 900;
        snwCtlFemale.throughSurfaceVision = 0.45f;
        snwCtlFemale.waterVision = 0.4f;
        snwCtlFemale.movementBasedVision = 1.5f;
        return snwCtlFemale;
    }
    public override void EstablishRelationships()
    {
        Relationships scF = new(HailstormCreatures.SnowcuttleFemale);

        // Will snatch up these creatures and fly back to its den.
        scF.Eats(CreatureTemplate.Type.Fly, 0.9f);
        scF.Eats(CreatureTemplate.Type.Leech, 0.9f);
        scF.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.7f);
        scF.Eats(HailstormCreatures.PeachSpider, 0.6f);
        scF.Eats(CreatureTemplate.Type.SeaLeech, 0.6f);
        scF.Eats(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.6f);
        scF.Eats(CreatureTemplate.Type.Hazer, 0.5f);
        scF.Eats(CreatureTemplate.Type.VultureGrub, 0.3f);
        scF.Eats(CreatureTemplate.Type.SmallCentipede, 0.1f);

        // Will bash and nibble at these creatures until it dies.
        scF.Attacks(CreatureTemplate.Type.BigNeedleWorm, 0.8f);
        scF.Attacks(CreatureTemplate.Type.EggBug, 0.7f);
        scF.Attacks(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.7f);

    }

}

sealed class SnowcuttleMaleCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(320/360f, 0.25f, 0.6f);

    internal SnowcuttleMaleCritob() : base (HailstormCreatures.SnowcuttleMale, HailstormUnlocks.SnowcuttleMale, HailstormUnlocks.SnowcuttleFemale) {}
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlM";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleM" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate snwCtlMale = new CreatureFormula(HailstormCreatures.SnowcuttleTemplate, Type, "Snowcuttle Male").IntoTemplate();
        snwCtlMale.visualRadius = 1200;
        snwCtlMale.throughSurfaceVision = 0.8f;
        snwCtlMale.waterVision = 0.8f;
        snwCtlMale.movementBasedVision = 0.25f;
        return snwCtlMale;
    }
    public override void EstablishRelationships()
    {
        Relationships scM = new(HailstormCreatures.SnowcuttleMale);

        // Flies away from this creature if they are anywhere nearby, showing active fear.
        scM.Fears(HailstormCreatures.PeachSpider, 0.8f);

    }

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaB;
    #nullable disable
}

sealed class SnowcuttleLeCritob : SnowcuttleTemplate
{

    public override Color SnowcuttleColor => Custom.HSL2RGB(270 / 360f, 0.25f, 0.6f);

    internal SnowcuttleLeCritob() : base (HailstormCreatures.SnowcuttleLe, HailstormUnlocks.SnowcuttleLe, HailstormUnlocks.SnowcuttleFemale) {}
    public override string DevtoolsMapName(AbstractCreature absCtl) => "ctlL";
    public override IEnumerable<string> WorldFileAliases() => new[] { "SnowcuttleL" };
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.LikesOutside,
            RoomAttractivenessPanel.Category.Flying,
            RoomAttractivenessPanel.Category.Dark,
            RoomAttractivenessPanel.Category.Lizards
        };
    }

    public override CreatureTemplate CreateTemplate()
    {
        return new CreatureFormula(HailstormCreatures.SnowcuttleTemplate, Type, "Snowcuttle Le").IntoTemplate();
    }
    public override void EstablishRelationships()
    {
        Relationships scL = new(HailstormCreatures.SnowcuttleLe);

        // Will snatch up these creatures and fly back to its den.
        scL.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.9f);
        scL.Eats(CreatureTemplate.Type.VultureGrub, 0.8f);
        scL.Eats(CreatureTemplate.Type.SeaLeech, 0.8f);
        scL.Eats(CreatureTemplate.Type.Leech, 0.6f);
        scL.Eats(CreatureTemplate.Type.Hazer, 0.5f);
        scL.Eats(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.4f);
        scL.Eats(CreatureTemplate.Type.Fly, 0.2f);

        // Will bash and nibble at these creatures until it dies.
        scL.Attacks(CreatureTemplate.Type.BigNeedleWorm, 0.6f);
        scL.Attacks(CreatureTemplate.Type.EggBug, 0.6f);
        scL.Attacks(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.6f);
        scL.Attacks(HailstormCreatures.Luminescipede, 0.6f);
        //scL.Attacks(HailstormEnums.Strobelegs, 0.6f);

        // Hangs out near these creatures when it has nothing else to do, and flies to them for protection if nearby and in danger.
        scL.IsInPack(CreatureTemplate.Type.CicadaA, 0.6f);
        scL.IsInPack(CreatureTemplate.Type.CicadaB, 0.6f);
        scL.IsInPack(HailstormCreatures.SnowcuttleTemplate, 0.6f);

        // Flies away from these creatures if they are anywhere nearby, showing active fear.
        scL.Fears(CreatureTemplate.Type.DropBug, 1);
        scL.Fears(CreatureTemplate.Type.BigSpider, 1);
        scL.Fears(CreatureTemplate.Type.SpitterSpider, 1);
        //scL.Fears(HailstormEnums.BezanBud, 1);
        scL.Fears(CreatureTemplate.Type.Vulture, 0.8f);
        scL.Fears(CreatureTemplate.Type.KingVulture, 0.8f);
        scL.Fears(CreatureTemplate.Type.MirosBird, 0.8f);
        scL.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.8f);
        scL.Fears(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.7f);
        scL.Fears(CreatureTemplate.Type.Snail, 0.2f);
        scL.Fears(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.2f);

        scL.Ignores(CreatureTemplate.Type.PoleMimic);
        scL.Ignores(CreatureTemplate.Type.TentaclePlant);
        scL.Ignores(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti);
        scL.Ignores(HailstormCreatures.InfantAquapede);

    }

    #nullable enable
    public override CreatureTemplate.Type? ArenaFallback() => (UnityEngine.Random.value < 0.5f) ? CreatureTemplate.Type.CicadaB : CreatureTemplate.Type.CicadaA;
    #nullable disable
}

//----------------------------------------------------------------------------------