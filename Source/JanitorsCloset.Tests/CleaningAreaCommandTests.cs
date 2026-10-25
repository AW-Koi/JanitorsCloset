using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using Verse;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CleaningAreaCommandTests
    {
        [Fact]
        public void ToolsIn_ForAMixedGroup_ReturnsEachSelectedToolOnce()
        {
            var mop = new CompCleaningAssignment();
            var broom = new CompCleaningAssignment();
            var mopCommand = new CleaningAreaCommand(mop, null);
            var broomCommand = new CleaningAreaCommand(broom, null);
            var mopCommandAgain = new CleaningAreaCommand(mop, null);
            var unrelated = new Command_Action();
            var group = new List<Gizmo> { mopCommand, unrelated, broomCommand, mopCommandAgain };

            var tools = mopCommand.ToolsIn(group);

            Assert.Equal(new List<CompCleaningAssignment> { mop, broom }, tools);
        }

        [Fact]
        public void ToolsIn_WhenTheGroupIsEmpty_ReturnsTheCommandsOwnTool()
        {
            var mop = new CompCleaningAssignment();
            var mopCommand = new CleaningAreaCommand(mop, null);

            var tools = mopCommand.ToolsIn(new List<Gizmo>());

            Assert.Equal(new List<CompCleaningAssignment> { mop }, tools);
        }
    }
}
