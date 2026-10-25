using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JanitorsCloset.Cleaning;
using JanitorsCloset.Patches;
using RimWorld;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class PacifistCleaningToolTests
    {
        [Fact]
        public void IsViolentWeapon_ForACleaningTool_IsFalse()
        {
            var mop = Weapon(new CleaningToolExtension());

            Assert.False(CleaningToolViolence.IsViolentWeapon(mop));
        }

        [Fact]
        public void IsViolentWeapon_ForAnOrdinaryWeapon_IsTrue()
        {
            var club = Weapon();

            Assert.True(CleaningToolViolence.IsViolentWeapon(club));
        }

        [Fact]
        public void IsViolentWeapon_ForANonWeapon_IsFalse()
        {
            var wood = new ThingDef { category = ThingCategory.Item };

            Assert.False(CleaningToolViolence.IsViolentWeapon(wood));
        }

        [Fact]
        public void EquipTranspiler_OnEveryTargetMethod_ReplacesOnlyTheViolenceCheck()
        {
            foreach (var method in Patch_PacifistsEquipCleaningTools.TargetMethods())
            {
                AssertReplacesOnlyTheViolenceCheck(method);
            }
        }

        private static void AssertReplacesOnlyTheViolenceCheck(MethodBase method)
        {
            var original = PatchProcessor.GetOriginalInstructions(method);

            var patched = Patch_PacifistsEquipCleaningTools.Transpiler(original).ToList();

            Assert.Equal(1, patched.Count(i => i.Calls(IsViolentWeaponMethod)));
            Assert.Equal(CountIsWeaponCalls(original) - 1, CountIsWeaponCalls(patched));
        }

        [Fact]
        public void GeneTranspiler_OnAddGene_RoutesEveryPrimaryReadThroughTheHelper()
        {
            var addGene = AccessTools.Method(typeof(Pawn_GeneTracker), "AddGene", new[] { typeof(Gene), typeof(bool) });
            var original = PatchProcessor.GetOriginalInstructions(addGene);

            var patched = Patch_GeneKeepsCleaningTool.Transpiler(original).ToList();

            Assert.Equal(0, patched.Count(i => i.Calls(PrimaryGetter)));
            Assert.Equal(original.Count(i => i.Calls(PrimaryGetter)), patched.Count(i => i.Calls(ViolentPrimaryMethod)));
        }

        private static readonly MethodInfo IsViolentWeaponMethod = AccessTools.Method(typeof(CleaningToolViolence), nameof(CleaningToolViolence.IsViolentWeapon));
        private static readonly MethodInfo ViolentPrimaryMethod = AccessTools.Method(typeof(CleaningToolViolence), nameof(CleaningToolViolence.ViolentPrimary));
        private static readonly MethodInfo IsWeaponGetter = AccessTools.PropertyGetter(typeof(ThingDef), nameof(ThingDef.IsWeapon));
        private static readonly MethodInfo PrimaryGetter = AccessTools.PropertyGetter(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Primary));

        private static int CountIsWeaponCalls(IEnumerable<CodeInstruction> code) => code.Count(i => i.Calls(IsWeaponGetter));

        private static ThingDef Weapon(params DefModExtension[] extensions)
        {
            return new ThingDef
            {
                category = ThingCategory.Item,
                tools = new List<Tool> { new Tool() },
                modExtensions = extensions.Length == 0 ? null : extensions.ToList(),
            };
        }
    }
}
