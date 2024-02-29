namespace Hailstorm;

public static class HSEnums
{
    public static readonly SlugcatStats.Name Incandescent = new("Incandescent");

    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Sound).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CreatureType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SandboxUnlock).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(Color).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(AbstractObjectType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DamageTypes).TypeHandle);
    }

    public static void UnregisterEnums(Type type)
    {
        IEnumerable<FieldInfo> extEnums = type.GetFields(Static | Public).Where(x => x.FieldType.IsSubclassOf(typeof(ExtEnumBase)));
        foreach ((FieldInfo extEnum, object obj) in from extEnum in extEnums
                                                    let obj = extEnum.GetValue(null)
                                                    where obj != null
                                                    select (extEnum, obj))
        {
            obj.GetType().GetMethod("Unregister")!.Invoke(obj, null);
            extEnum.SetValue(null, null);
        }
    }

    public static void Unregister()
    {
        UnregisterEnums(typeof(Sound));
        UnregisterEnums(typeof(CreatureType));
        UnregisterEnums(typeof(SandboxUnlock));
        UnregisterEnums(typeof(Color));
        UnregisterEnums(typeof(AbstractObjectType));
        UnregisterEnums(typeof(DamageTypes));
    }

    public static class Sound
    {
        public static readonly SoundID CyanwingDeath;
    }

    public static class CreatureType
    {
        public static CreatureTemplate.Type InfantAquapede = new(nameof(InfantAquapede), true);
        public static CreatureTemplate.Type SnowcuttleTemplate = new(nameof(SnowcuttleTemplate), true);
        public static CreatureTemplate.Type SnowcuttleFemale = new(nameof(SnowcuttleFemale), true);
        public static CreatureTemplate.Type SnowcuttleMale = new(nameof(SnowcuttleMale), true);
        public static CreatureTemplate.Type SnowcuttleLe = new(nameof(SnowcuttleLe), true);
        public static CreatureTemplate.Type Raven = new(nameof(Raven), true);
        public static CreatureTemplate.Type IcyBlueLizard = new(nameof(IcyBlueLizard), true);
        public static CreatureTemplate.Type FreezerLizard = new(nameof(FreezerLizard), true);
        public static CreatureTemplate.Type PeachSpider = new(nameof(PeachSpider), true);
        public static CreatureTemplate.Type Cyanwing = new(nameof(Cyanwing), true);
        public static CreatureTemplate.Type GorditoGreenieLizard = new(nameof(GorditoGreenieLizard), true);
        //public static CreatureTemplate.Type BezanBud = new(nameof(BezanBud), true);
        public static CreatureTemplate.Type Chillipede = new(nameof(Chillipede), true);
        public static CreatureTemplate.Type Luminescipede = new(nameof(Luminescipede), true);
        //public static CreatureTemplate.Type Strobelegs = new(nameof(Strobelegs), true);

        public static CreatureTemplate.Type[] GetAllCreatureTypes()
        {
            return new CreatureTemplate.Type[]
            {
                InfantAquapede,
                SnowcuttleTemplate,
                SnowcuttleFemale,
                SnowcuttleMale,
                SnowcuttleLe,
                Raven,
                IcyBlueLizard,
                FreezerLizard,
                PeachSpider,
                Cyanwing,
                GorditoGreenieLizard,
                //BezanBud,
                Chillipede,
                Luminescipede
                //, Strobelegs
            };
        }
    }

    public static class SandboxUnlock
    {
        public static MultiplayerUnlocks.SandboxUnlockID InfantAquapede = new(nameof(InfantAquapede), true);
        public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleFemale = new(nameof(SnowcuttleFemale), true);
        public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleMale = new(nameof(SnowcuttleMale), true);
        public static MultiplayerUnlocks.SandboxUnlockID SnowcuttleLe = new(nameof(SnowcuttleLe), true);
        public static MultiplayerUnlocks.SandboxUnlockID Raven = new(nameof(Raven), true);
        public static MultiplayerUnlocks.SandboxUnlockID IcyBlue = new(nameof(IcyBlue), true);
        public static MultiplayerUnlocks.SandboxUnlockID Freezer = new(nameof(Freezer), true);
        public static MultiplayerUnlocks.SandboxUnlockID PeachSpider = new(nameof(PeachSpider), true);
        public static MultiplayerUnlocks.SandboxUnlockID Cyanwing = new(nameof(Cyanwing), true);
        public static MultiplayerUnlocks.SandboxUnlockID GorditoGreenie = new(nameof(GorditoGreenie), true);
        //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BezanBud = new(nameof(BezanBud), true);
        public static MultiplayerUnlocks.SandboxUnlockID Luminescipede = new(nameof(Luminescipede), true);
        //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID Strobelegs = new(nameof(Strobelegs), true);
        public static MultiplayerUnlocks.SandboxUnlockID Chillipede = new(nameof(Chillipede), true);


        public static MultiplayerUnlocks.SandboxUnlockID IceChunk = new(nameof(IceChunk), true);
        public static MultiplayerUnlocks.SandboxUnlockID FreezerCrystal = new(nameof(FreezerCrystal), true);
        public static MultiplayerUnlocks.SandboxUnlockID BurnSpear = new(nameof(BurnSpear), true);
    }

    public static class Color
    {
    }

    public static class DamageTypes
    {
        public static Creature.DamageType Cold = new(nameof(Cold), true);
        public static Creature.DamageType Heat = new(nameof(Heat), true);
        public static Creature.DamageType Venom = new(nameof(Venom), true);
    }

    public static class AbstractObjectType
    {
        public static AbstractPhysicalObject.AbstractObjectType IceChunk = new(nameof(IceChunk), true);
        public static AbstractPhysicalObject.AbstractObjectType FreezerCrystal = new(nameof(FreezerCrystal), true);
        public static AbstractPhysicalObject.AbstractObjectType BurnSpear = new(nameof(BurnSpear), true);
    }

    public static class PlacedObjectType
    {
    }
}