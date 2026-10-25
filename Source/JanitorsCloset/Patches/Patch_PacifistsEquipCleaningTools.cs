using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // Lets pawns incapable of violence equip cleaning tools; vanilla still disables their attacks.
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_PacifistsEquipCleaningTools
    {
        private const int LookAhead = 8;

        private static readonly MethodInfo IsWeaponGetter = AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsWeapon));
        private static readonly MethodInfo WorkTagIsDisabled = AccessTools.Method(typeof(Pawn), nameof(Pawn.WorkTagIsDisabled));
        private static readonly MethodInfo IsViolentWeapon = AccessTools.Method(typeof(CleaningToolViolence), nameof(CleaningToolViolence.IsViolentWeapon));

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(FloatMenuOptionProvider_Equip), "GetSingleOptionFor", new[] { typeof(Thing), typeof(FloatMenuContext) });
            yield return AccessTools.Method(typeof(Building_OutfitStand), "GetFloatMenuOptionToEquipWeapon");
            yield return AccessTools.Method(typeof(JobDriver_UseOutfitStand), "PawnCanWieldWeapon");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            int replaced = 0;
            for (int i = 0; i < code.Count; i++)
            {
                if (!code[i].Calls(IsWeaponGetter) || !FollowedByViolenceCheck(code, i)) continue;
                code[i] = new CodeInstruction(System.Reflection.Emit.OpCodes.Call, IsViolentWeapon).MoveLabelsFrom(code[i]);
                replaced++;
            }
            if (replaced == 0) Log.Warning("[Janitor's Closet] Pacifist equip check not found; pacifists can't equip cleaning tools.");
            return code;
        }

        // The IsWeapon read that pairs with WorkTagIsDisabled(WorkTags.Violent) a few instructions later.
        private static bool FollowedByViolenceCheck(List<CodeInstruction> code, int index)
        {
            bool violentConstant = false;
            for (int j = index + 1; j < code.Count && j <= index + LookAhead; j++)
            {
                if (code[j].LoadsConstant((long)WorkTags.Violent)) violentConstant = true;
                if (code[j].Calls(WorkTagIsDisabled)) return violentConstant;
            }
            return false;
        }
    }
}
