global using BepInEx;
global using DevInterface;
global using Fisobs.Core;
global using Fisobs.Creatures;
global using Fisobs.Items;
global using Fisobs.Properties;
global using Fisobs.Sandbox;
global using HUD;
global using JollyCoop;
global using JollyCoop.JollyMenu;
global using LizardCosmetics;
global using Menu.Remix.MixedUI;
global using MonoMod.Cil;
global using MonoMod.RuntimeDetour;
global using MoreSlugcats;
global using On.Menu;
global using RWCustom;
global using SlugBase;
global using SlugBase.DataTypes;
global using SlugBase.Features;
global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Security.Permissions;
global using System.Text.RegularExpressions;
global using UnityEngine;
global using static Hailstorm.GlowSpiderState;
global using static Hailstorm.GlowSpiderState.Behavior;
global using static Hailstorm.GlowSpiderState.Role;
global using static Hailstorm.ObjectRelationship.Type;
global using static System.Reflection.BindingFlags;
global using OpCodes = Mono.Cecil.Cil.OpCodes;
global using Color = UnityEngine.Color;
global using Random = UnityEngine.Random;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete