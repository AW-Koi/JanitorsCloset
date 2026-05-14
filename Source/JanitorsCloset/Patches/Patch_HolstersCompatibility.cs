using System;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using Verse;

namespace JanitorsCloset.Patches
{
    // Holsters (https://github.com/IwoRosiak/rimworld-holsters) draws every equipped or
    // inventoried weapon as a holstered sprite on the pawn when it isn't being carried
    // openly. Janitor cleaning tools are weapons mechanically but we render them ourselves
    // via the cleaning patches, so a holstered broom on a pawn's back looks wrong.
    // There is no opt-out API in Holsters, so we patch its private per-weapon
    // draw method to skip anything carrying our CleaningToolExtension.
    [StaticConstructorOnStartup]
    internal static class Patch_HolstersCompatibility
    {
        static Patch_HolstersCompatibility()
        {
            try
            {
                var handlerType = AccessTools.TypeByName("RimWorldHolsters.Core.WeaponDrawingHandler");
                if (handlerType == null) return; // Holsters not loaded.

                var target = AccessTools.Method(handlerType, "DrawWeapon", new[] { typeof(ThingWithComps) });
                if (target == null)
                {
                    Log.Warning("[Janitor's Closet] Holsters detected but WeaponDrawingHandler.DrawWeapon was not found; cleaning tools may render as holstered.");
                    return;
                }

                var prefix = new HarmonyMethod(typeof(Patch_HolstersCompatibility)
                    .GetMethod(nameof(SkipCleaningTools), BindingFlags.Static | BindingFlags.NonPublic));

                new Harmony("TerraIncognita.JanitorsCloset.Holsters").Patch(target, prefix: prefix);
                Log.Message("[Janitor's Closet] Holsters compatibility patch applied.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[Janitor's Closet] Failed to apply Holsters compatibility patch: {ex}");
            }
        }

        private static bool SkipCleaningTools(ThingWithComps weapon)
        {
            return weapon?.def?.GetModExtension<CleaningToolExtension>() == null;
        }
    }
}
