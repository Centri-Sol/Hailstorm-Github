using Menu.Remix.MixedUI;
using UnityEngine;
using RWCustom;

namespace Hailstorm;

public class HSRemix : OptionInterface
{
    private UIelement[] IncandescentTab;
    private UIelement[] CreaturesTab;
    private UIelement[] ColorVariationTab;


    // Incandescent Configs
    public static Configurable<float> IncanSpearDamageMultiplier;
    public static Configurable<float> IncanCollisionDamageMultiplier;
    public static Configurable<bool> IncanArenaDowngradesOutsideOfArena;
    public static Configurable<bool> IncanNoArenaDowngrades;
    public static Configurable<bool> IncanNoFireFuelLimit;
    public static Configurable<bool> IncanWaveringFlame;


    // Stowaway Configs
    public static Configurable<float> StowawayFoodSurvivalBonus;
    public static Configurable<float> StowawayHPMultiplier;
    public static Configurable<bool> StowawayToughSides;
    public static Configurable<bool> HailstormStowawaysEverywhere;

    // Mother Spider Configs
    public static Configurable<float> MotherSpiderEvenMoreSpiders;
    public static Configurable<bool> MotherSpiderCRONCH;
    public static Configurable<bool> HailstormMotherSpidersEverywhere;

    // Baby Spider Configs
    public static Configurable<bool> HailstormBabySpidersEverywhere;

    // Miros Vulture Configs
    public static Configurable<bool> ScissorhawkNoNormalLasers;
    public static Configurable<bool> ScissorhawkEagerBirds;
    public static Configurable<bool> AuroricMirosEverywhere;

    // Cyanwing Configs
    public static Configurable<bool> CyanwingAtomization; // Needs implementation

    // Color Variation Configs
    public static Configurable<bool> PolePlantColorsEverywhere;
    public static Configurable<bool> MonsterKelpColorsEverywhere;
    public static Configurable<bool> BigSpiderColorsEverywhere;
    public static Configurable<bool> YellowLizardColorsEverywhere;
    public static Configurable<bool> EelLizardColorsEverywhere;
    public static Configurable<bool> StrawberryLizardColorsEverywhere;



    //public static ConditionalWeakTable<Player, GlowOptions> PlayerData = new ConditionalWeakTable<Player, GlowOptions>();
    public HSRemix()
    {

        // The Incandescent
        IncanSpearDamageMultiplier = config.Bind("IncanSpearDamageMultiplier", 1f, (ConfigurableInfo)null);
        IncanCollisionDamageMultiplier = config.Bind("IncanCollisionDamageMultiplier", 1f, (ConfigurableInfo)null);
        IncanArenaDowngradesOutsideOfArena = config.Bind("IncanArenaDowngradesOutsideOfArena", false, (ConfigurableInfo)null);
        IncanNoArenaDowngrades = config.Bind("IncanNoArenaDowngrades", false, (ConfigurableInfo)null);
        IncanNoFireFuelLimit = config.Bind("IncanNoFireFuelLimit", false, (ConfigurableInfo)null);
        IncanWaveringFlame = config.Bind("IncanWaveringFlame", false, (ConfigurableInfo)null);

        // Stowaways
        StowawayFoodSurvivalBonus = config.Bind("StowawayFoodSurvivalChanceBonus", 0f, (ConfigurableInfo)null);
        StowawayHPMultiplier = config.Bind("StowawayHPMultiplier", 1f, (ConfigurableInfo)null);
        StowawayToughSides = config.Bind("StowawayToughSides", false, (ConfigurableInfo)null);
        HailstormStowawaysEverywhere = config.Bind("HailstormStowawaysEverywhere", false, (ConfigurableInfo)null);

        // Mother Spiders
        MotherSpiderEvenMoreSpiders = config.Bind("MotherSpiderEvenMoreSpiders", 0f, (ConfigurableInfo)null);
        MotherSpiderCRONCH = config.Bind("MotherSpiderCRONCH", false, (ConfigurableInfo)null);
        HailstormMotherSpidersEverywhere = config.Bind("HailstormMotherSpidersEverywhere", false, (ConfigurableInfo)null);

        // Baby Spiders
        HailstormBabySpidersEverywhere = config.Bind("HailstormBabySpidersEverywhere", false, (ConfigurableInfo)null);

        // Miros Vultures
        ScissorhawkNoNormalLasers = config.Bind("ScissorhawkNoNormalLasers", false, (ConfigurableInfo)null);
        ScissorhawkEagerBirds = config.Bind("ScissorhawkEagerBirds", false, (ConfigurableInfo)null);
        AuroricMirosEverywhere = config.Bind("HailstormScissorhawksEverywhere", false, (ConfigurableInfo)null);

        // Cyanwings
        CyanwingAtomization = config.Bind("CyanwingAtomization", false, (ConfigurableInfo)null);

        // Color Variation
        PolePlantColorsEverywhere = config.Bind("PolePlantColorsEverywhere", false, (ConfigurableInfo)null);
        MonsterKelpColorsEverywhere = config.Bind("MonsterKelpColorsEverywhere", false, (ConfigurableInfo)null);
        BigSpiderColorsEverywhere = config.Bind("BigSpiderColorsEverywhere", false, (ConfigurableInfo)null);
        YellowLizardColorsEverywhere = config.Bind("YellowLizardColorsEverywhere", false, (ConfigurableInfo)null);
        EelLizardColorsEverywhere = config.Bind("EelLizardColorsEverywhere", false, (ConfigurableInfo)null);
        StrawberryLizardColorsEverywhere = config.Bind("StrawberryLizardColorsEverywhere", false, (ConfigurableInfo)null);

    }


    public override void Initialize()
    {
        base.Initialize();
        OpTab opTab1 = new OpTab(this, "The Incandescent");
        OpTab opTab2 = new OpTab(this, "Creatures");
        OpTab opTab3 = new OpTab(this, "Color Variation");
        Tabs = new[] { opTab1, opTab2, opTab3 };


        OpContainer containerTab1 = new(new Vector2(0, 0));
        opTab1.AddItems(containerTab1);
        FSprite IncanIcon = new FSprite("HSColoredIcon_Incandescent") { x = 300, y = 500, width = 56, height = 34 };
        containerTab1.container.AddChild(IncanIcon);
        IncandescentTab = new UIelement[]
        {
            //------------------------------------------------------------
            new OpLabel(new Vector2(250f, 560f), new Vector2(100f, 40f), "Hailstorm Remix!", FLabelAlignment.Center, true, null) { color = new(127 / 255f, 212 / 255f, 1) },
            new OpLabel(new Vector2(275f, 540f), new Vector2(50f, 20f), "The Incandescent", FLabelAlignment.Center, true, null) { color = new(232 / 255f, 74 / 255f, 60 / 255f) },

            // The Incandescent
            new OpLabel(5f, 455f, "Spear Damage Multiplier", false),
            new OpFloatSlider(IncanSpearDamageMultiplier, new Vector2(15f, 425f), 100, 1)
            {
                min = 0.25f,
                max = 1f,
                _increment = 5,
                description = "Multiplies damage done with spears. Guess this one was pretty self-explanatory, huh?"
            },
            new OpLabel(5f, 400f, "Collision Damage Multiplier", false),
            new OpFloatSlider(IncanCollisionDamageMultiplier, new Vector2(15f, 370f), 100, 1)
            {
                min = 0.5f,
                max = 1.5f,
                _increment = 5,
                description = "Multiplies damage done via heat attacks. I'll let you go above the default value for this one so you can go ham with the slams.\nActually the end of that sentence kinda sounds weird now that I've typed that out; uuuuuuhhhh"
            },
            new OpLabel(5f, 345f, "Arena Downgrades Everywhere?", false),
            new OpCheckBox(IncanArenaDowngradesOutsideOfArena, new Vector2(15f, 317.5f))
            {
                description = "The Incandescent will lose warmth just for using her heat attacks at all. Significantly less harsh than in Arena."
            },
            new OpLabel(5f, 290f, "No Arena Downgrades?", false),
            new OpCheckBox(IncanNoArenaDowngrades, new Vector2(15f, 262.5f))
            {
                description = "Prevents heat loss from simply using the Incandescent's heat attacks.\nIf you enable this solely to be a jerk in Arena, I will find you."
            },
            new OpLabel(5f, 235f, "No Fire Fuel Limit", false),
            new OpCheckBox(IncanNoFireFuelLimit, new Vector2(15f, 207.5f))
            {
                description = "You know how eating certain foods makes the Incandescent's glow a little bigger for a bit? If not, then uh now you do.\nAnyways, the maximum duration of that effect is usually capped out at 1 minute. Turn this option on to remove that limit."
            },
            new OpLabel(5f, 180f, "Wavering Flame", false),
            new OpCheckBox(IncanWaveringFlame, new Vector2(15f, 152.5f))
            {
                description = "The Incandescent's flame will slowly begin to fade if too much time passes without eating any fire fuel, weakening her hypothermia resistance and collision damage.\nI'm actually not sure how possible this is in every region, so do tell me about your adventures if you try this out!"
            },
            //------------------------------------------------------------
        };
        opTab1.AddItems(IncandescentTab);


        OpContainer containerTab2 = new OpContainer(new Vector2(0, 0));
        opTab2.AddItems(containerTab2);
        FSprite StowawayIcon = new FSprite("HSColoredIcon_Stowaway") { x = 85, y = 520 };
        FSprite MotherSpiderIcon = new FSprite("HSColoredIcon_MotherSpider") { x = 299, y = 520 };
        FSprite MirosVultureIcon = new FSprite("HSColoredIcon_MirosVulture") { x = 510, y = 520 };
        FSprite BabySpiderIcon = new FSprite("HSColoredIcon_BabySpider") { x = 300, y = 270 };

        containerTab2.container.AddChild(StowawayIcon);
        containerTab2.container.AddChild(MotherSpiderIcon);
        containerTab2.container.AddChild(MirosVultureIcon);
        containerTab2.container.AddChild(BabySpiderIcon);

        CreaturesTab = new UIelement[]
        {
            //------------------------------------------------------------
            new OpLabel(new Vector2(250f, 560f), new Vector2(100f, 40f), "Hailstorm Remix!", FLabelAlignment.Center, true, null) {color = new(127/255f, 212/255f, 1)},
            new OpLabel(new Vector2(275f, 540f), new Vector2(50f, 20f), "Creatures", FLabelAlignment.Center, true, null) {color = new(0.85f, 0.85f, 0.85f)},

            // Stowaways
            new OpLabel(30f, 480f, "Stowaways", true) {color = Custom.HSL2RGB(210 / 360f, 0.1f, 0.5f)},

            new OpLabel(5f, 455f, "Deathfood Survival Chance", false),
            new OpFloatSlider(StowawayFoodSurvivalBonus, new Vector2(15f, 425f), 100, 1, false)
            {
                min = 0,
                max = 1,
                _increment = 1, // Increments by 0.01
                description = "If you want, you can give the creatures that Stowaways spit out on death a higher chance of being alive, with higher health on average, to boot."
            },
            new OpLabel(5f, 400f, "Stowaway HP Multiplier", false),
            new OpFloatSlider(StowawayHPMultiplier, new Vector2(15f, 370f), 100, 1)
            {
                min = 1,
                max = 3,
                _increment = 5, // Increments by 0.05
                description = "Do ya think Stowaways die too quickly? Then slide their HP up!"
            },
            new OpLabel(5f, 345f, "Hardened Sides", false),
            new OpCheckBox(StowawayToughSides, new Vector2(15f, 317.5f))
            {
                description = "Okay, but what if attacking Stowaways from the sides just didn't work? Check this off to make spears and stuff bounce off unless they come from below!"
            },
            new OpLabel(5f, 285f, "Hailstorm Stowaways\nEverywhere?", false),
            new OpCheckBox(HailstormStowawaysEverywhere, new Vector2(15f, 247.5f))
            {
                description = "Have you seen what changes I've made to Stowaways in the Incandescent's campaign? Now want 'em in aaaall campaigns?"
            },
            //------------------------------
            // Mother Spiders
            new OpLabel(227f, 480f, "Mother Spiders", true) {color = new(25/255f, 178/255f, 25/255f)},

            new OpLabel(246f, 455f, "Even MORE Spiders", false),
            new OpFloatSlider(MotherSpiderEvenMoreSpiders, new Vector2(250f, 425f), 100, 0, false)
            {
                min = 0,
                max = 45,
                _increment = 100, // Increments by 1.00
                description = "Just in case these didn't already spit out enough spiders, I guess. You can increase the spider count to up to double!\nWARNING: THIS *WILL* START TO LAG YOUR GAME AT HIGHER NUMBERS."
            },
            new OpLabel(233f, 400f, "Mother Spider CRONCH", false),
            new OpCheckBox(MotherSpiderCRONCH, new Vector2(287f, 372.5f))
            {
                description = "Yes they will literally kill you if they slam into you quickly enough. That'll take a LOT of speed, though; they'll usually just stun."
            },
            new OpLabel(new Vector2(290f, 340f), new Vector2(5, 5), "Hailstorm Mother Spiders\nEverywhere?", FLabelAlignment.Center, false, null),
            new OpCheckBox(HailstormMotherSpidersEverywhere, new Vector2(287f, 302.5f))
            {
                description = "Maybe their changes are a little less interesting than the Stowaway's, but I thought I'd at least give you the option."
            },
            //------------------------------
            // Baby Spiders
            new OpLabel(239f, 230f, "Baby Spiders", true),

            new OpLabel(new Vector2(289f, 195f), new Vector2(5, 5), "Hailstorm Baby Spiders\nEverywhere?", FLabelAlignment.Center, false, null),
            new OpCheckBox(HailstormBabySpidersEverywhere, new Vector2(287f, 157.5f))
            {
                description = "*Extra fun* when paired with the Mother Spider's Remix options!"
            },
            //------------------------------
            // Miros Vultures
            new OpLabel(440f, 480f, "Miros Vultures", true) { color = new(230 / 255f, 14 / 255f, 14 / 255f) },

            new OpLabel(500f, 455f, "No Normal Lasers", false),
            new OpCheckBox(ScissorhawkNoNormalLasers, new Vector2(565f, 427.5f))
            {
                description = "Did you ever notice that I gave Miros Vultures new laser types?\nWith *this* checked off you DEFINITELY will!"
            },
            new OpLabel(535f, 400f, "Eager Birds", false),
            new OpCheckBox(ScissorhawkEagerBirds, new Vector2(565f, 372.5f))
            {
                description = "Instead of firing their lasers when hurt, Miros Vultures will just start shooting at you whenever.\nThis will extend their laser timers by a second, to make reacting to the lasers a bit more reasonable."
            },
            new OpLabel(new Vector2(575f, 340f), new Vector2(5, 5), "Hailstorm Miros Vultures\nEverywhere?", FLabelAlignment.Right, false, null),
            new OpCheckBox(AuroricMirosEverywhere, new Vector2(565f, 302.5f))
            {
                description = "For all the people who dislike how Miros Vultures only attack players: this one's for you."
            },
            //------------------------------
            // Cyanwings
            /*new OpLabel(239f, 230f, "Cyanwings", true),

            new OpLabel(new Vector2(289f, 195f), new Vector2(5, 5), "Cyanwing Atomization", FLabelAlignment.Center, false, null),
            new OpCheckBox(CyanwingAtomization, new Vector2(287f, 157.5f))
            {
                description = "Uh, this one's a little, how you say, 'fucked up'? Anything Cyanwings zap will straight-up get atomized. Poof.  G O N E.\nThis one is OFF by default because the fucked-up-ness of it was getting to me during testing."
            },*/
            //------------------------------------------------------------
        };
        opTab2.AddItems(CreaturesTab);

        OpContainer containerTab3 = new OpContainer(new Vector2(0, 0));
        opTab3.AddItems(containerTab3);
        PolePlantIcon = new FSprite("Kill_PoleMimic") { x = 50, y = 485, color = Color.red };
        MonsterKelpIcon = new FSprite("Kill_TentaclePlant") { x = 50, y = 435, color = Color.red };
        BigSpiderIcon = new FSprite("Kill_BigSpider") { x = 50, y = 385, color = Custom.HSL2RGB(42/360f, 0.7f, 0.4f)};
        SpitterSpiderIcon = new FSprite("Kill_BigSpider") { x = 80, y = 385, color = Custom.HSL2RGB(0, 0.7f, 0.4f) };
        MotherSpiderIcon2 = new FSprite("Kill_MotherSpider_CentriSol") { x = 110, y = 385, color = Custom.HSL2RGB(120 / 360f, 0.7f, 0.4f) };
        YellowLizardIcon = new FSprite("Kill_Yellow_Lizard") { x = 50, y = 335, color = new Color(1, 0.6f, 0) };
        EelLizardIcon = new FSprite("Kill_Salamander") { x = 50, y = 285, color = new Color(0f, 0.66f, 0.42f) };
        StrawberryLizardIcon = new FSprite("Kill_White_Lizard") { x = 50, y = 235, color = new Color(0.95f, 0.73f, 0.73f) };
        containerTab3.container.AddChild(PolePlantIcon);
        containerTab3.container.AddChild(MonsterKelpIcon);
        containerTab3.container.AddChild(BigSpiderIcon);
        containerTab3.container.AddChild(SpitterSpiderIcon);
        containerTab3.container.AddChild(MotherSpiderIcon2);
        containerTab3.container.AddChild(YellowLizardIcon);
        containerTab3.container.AddChild(EelLizardIcon);
        containerTab3.container.AddChild(StrawberryLizardIcon);
        ColorVariationTab = new UIelement[]
        {
            //------------------------------------------------------------
            new OpLabel(new Vector2(250f, 560f), new Vector2(100f, 40f), "Hailstorm Remix!", FLabelAlignment.Center, true, null) { color = new(127 / 255f, 212 / 255f, 1) },
            new OpLabel(new Vector2(276f, 540f), new Vector2(50f, 20f), "Color Variation", FLabelAlignment.Center, true, null) { color = new(1, 175 / 255f, 230/255f) },

            new OpCheckBox(PolePlantColorsEverywhere, new Vector2(5f, 475f)),
            new OpCheckBox(MonsterKelpColorsEverywhere, new Vector2(5f, 425f)),
            new OpCheckBox(BigSpiderColorsEverywhere, new Vector2(5f, 375f)),
            new OpCheckBox(YellowLizardColorsEverywhere, new Vector2(5f, 325f)),
            new OpCheckBox(EelLizardColorsEverywhere, new Vector2(5f, 275f)),
            new OpCheckBox(StrawberryLizardColorsEverywhere, new Vector2(5f, 225f)),
            new OpLabel(70, 475f, "Pole Plant Colors Everywhere?", false),
            new OpLabel(70, 425f, "Monster Kelp Colors Everywhere?", false),
            new OpLabel(130, 375f, "Big Spider Colors Everywhere?", false),
            new OpLabel(70, 325f, "Yellow Lizard Colors Everywhere?", false),
            new OpLabel(70, 275f, "Eel Lizard Colors Everywhere?", false),
            new OpLabel(70, 225f, "Strawberry Lizard Colors Everywhere?", false),

            //------------------------------------------------------------
        };
        opTab3.AddItems(ColorVariationTab);

    }

    private FSprite PolePlantIcon = new ("Kill_PoleMimic");
    private FSprite MonsterKelpIcon = new ("Kill_TentaclePlant");
    private FSprite BigSpiderIcon = new ("Kill_BigSpider");
    private FSprite SpitterSpiderIcon = new ("Kill_BigSpider");
    private FSprite MotherSpiderIcon2 = new ("Kill_MotherSpider_CentriSol");
    private FSprite YellowLizardIcon = new ("Kill_Yellow_Lizard");
    private FSprite EelLizardIcon = new ("Kill_Salamander");
    private FSprite StrawberryLizardIcon = new ("Kill_White_Lizard");

    private float ppHue;
    private float ppHueDirection = 0.50f / 360f;

    private float mkHue = -20/360f;
    private float mkHueDirection = 0.50f / 360f;

    private float bigSpdHue = 50/360f;
    private float sptSpdHue = -20/360f;
    private float mthSpdHue = 140/360f;
    private float bigSpdHueDirection = 0.25f / 360f;
    private float sptSpdHueDirection = 0.50f / 360f;
    private float mthSpdHueDirection = 0.25f / 360f;

    private float ylHue = 44/360f;
    private float ylHueDirection = 0.50f / 360f;

    private float eelHue = 130/360f;
    private float eelHueDirection = 0.75f / 360f;

    private float sbHue = -10/360f;
    private float sbHueDirection = 0.50f / 360f;
    public override void Update()
    {
        base.Update();
        if (!PolePlantColorsEverywhere.Value && PolePlantIcon.color != Color.red)
        {
            PolePlantIcon.color = Color.red;
            ppHue = 0;
        }
        else if (PolePlantColorsEverywhere.Value)
        {
            if (ppHue >= 40/360f)
            {
                ppHueDirection = -0.5f / 360f;
            }
            else if (ppHue <= -40/360f)
            {
                ppHueDirection = 0.5f / 360f;
            }
            ppHue += ppHueDirection;
            PolePlantIcon.color = Custom.HSL2RGB(ppHue, 1, 0.6f);
        }

        if (!MonsterKelpColorsEverywhere.Value && MonsterKelpIcon.color != Color.red)
        {
            MonsterKelpIcon.color = Color.red;
            mkHue = -20/360f;
        }
        else if (MonsterKelpColorsEverywhere.Value)
        {
            if (mkHue >= 20/360f)
            {
                mkHueDirection = -0.5f / 360f;
            }
            else if (mkHue <= -60/360f)
            {
                mkHueDirection = 0.5f / 360f;
            }
            mkHue += mkHueDirection;
            MonsterKelpIcon.color = Custom.HSL2RGB(mkHue, 1, 0.6f);
        }

        if (!BigSpiderColorsEverywhere.Value && BigSpiderIcon.color != Custom.HSL2RGB(42 / 360f, 0.7f, 0.4f))
        {
            BigSpiderIcon.color = Custom.HSL2RGB(42 / 360f, 0.7f, 0.4f);
            SpitterSpiderIcon.color = Custom.HSL2RGB(0, 0.7f, 0.4f);
            MotherSpiderIcon2.color = Custom.HSL2RGB(120/360f, 0.7f, 0.4f);
            bigSpdHue = 50/360f;
            sptSpdHue = -20/360f;
            mthSpdHue = 140/360f;
        }
        else if (BigSpiderColorsEverywhere.Value)
        {
            if (bigSpdHue >= 70 / 360f)
            {
                bigSpdHueDirection = -0.25f / 360f;
            }
            else if (bigSpdHue <= 30 / 360f)
            {
                bigSpdHueDirection = 0.25f / 360f;
            }
            bigSpdHue += bigSpdHueDirection;
            BigSpiderIcon.color = Custom.HSL2RGB(bigSpdHue, 0.7f, 0.4f);

            if (sptSpdHue >= 20 / 360f)
            {
                sptSpdHueDirection = -0.5f / 360f;
            }
            else if (sptSpdHue <= -60 / 360f)
            {
                sptSpdHueDirection = 0.5f / 360f;
            }
            sptSpdHue += sptSpdHueDirection;
            SpitterSpiderIcon.color = Custom.HSL2RGB(sptSpdHue, 0.7f, 0.4f);

            if (mthSpdHue >= 160 / 360f)
            {
                mthSpdHueDirection = -0.25f / 360f;
            }
            else if (mthSpdHue <= 120 / 360f)
            {
                mthSpdHueDirection = 0.25f / 360f;
            }
            mthSpdHue += mthSpdHueDirection;
            MotherSpiderIcon2.color = Custom.HSL2RGB(mthSpdHue, 0.7f, 0.4f);
        }

        if (!YellowLizardColorsEverywhere.Value && YellowLizardIcon.color != new Color(1, 0.6f, 0))
        {
            YellowLizardIcon.color = new Color(1, 0.6f, 0);
            ylHue = 44/360f;
        }
        else if (YellowLizardColorsEverywhere.Value)
        {
            if (ylHue >= 80 / 360f)
            {
                ylHueDirection = -0.5f / 360f;
            }
            else if (ylHue <= 8 / 360f)
            {
                ylHueDirection = 0.5f / 360f;
            }
            ylHue += ylHueDirection;
            YellowLizardIcon.color = Custom.HSL2RGB(ylHue, 1, 0.6f);
        }

        if (!EelLizardColorsEverywhere.Value && EelLizardIcon.color != new Color(0f, 0.66f, 0.42f))
        {
            EelLizardIcon.color = new Color(0f, 0.66f, 0.42f);
            eelHue = 130/360f;
        }
        else if (EelLizardColorsEverywhere.Value)
        {
            if (eelHue >= 195 / 360f)
            {
                eelHueDirection = -0.75f / 360f;
            }
            else if (eelHue <= 65 / 360f)
            {
                eelHueDirection = 0.75f / 360f;
            }
            eelHue += eelHueDirection;
            EelLizardIcon.color = Custom.HSL2RGB(eelHue, 0.85f, 0.25f);
        }

        if (!StrawberryLizardColorsEverywhere.Value && StrawberryLizardIcon.color != new Color(0.95f, 0.73f, 0.73f))
        {
            StrawberryLizardIcon.color = new Color(0.95f, 0.73f, 0.73f);
            sbHue = -10/360f;
        }
        else if (StrawberryLizardColorsEverywhere.Value)
        {
            if (sbHue >= 25 / 360f)
            {
                sbHueDirection = -0.5f / 360f;
            }
            else if (sbHue <= -45 / 360f)
            {
                sbHueDirection = 0.5f / 360f;
            }
            sbHue += sbHueDirection;
            StrawberryLizardIcon.color = Custom.HSL2RGB(sbHue, 0.525f, 0.83f);
        }
    }
}