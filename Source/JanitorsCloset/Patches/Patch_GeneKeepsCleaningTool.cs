using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace JanitorsCloset.Patches
{
    // A gene that disables violence makes the pawn drop their weapon, but they keep a cleaning tool.
    [HarmonyPatch(typeof(Pawn_GeneTracker), "AddGene", typeof(Gene), typeof(bool))]
    [UsedImplicitly]
    public static class Patch_GeneKeepsCleaningTool
    {
        private static readonly MethodInfo PrimaryGetter = AccessTools.PropertyGetter(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Primary));
        private static readonly MethodInfo ViolentPrimary = AccessTools.Method(typeof(CleaningToolViolence), nameof(CleaningToolViolence.ViolentPrimary));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(PrimaryGetter))
                {
                    yield return new CodeInstruction(OpCodes.Call, ViolentPrimary).MoveLabelsFrom(instruction);
                    continue;
                }
                yield return instruction;
            }
        }
    }
}
