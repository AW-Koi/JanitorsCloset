using System;
using System.Collections.Generic;
using JanitorsCloset.Cleaning;
using RimWorld;
using Verse;
using Verse.AI;
using Xunit;

namespace JanitorsCloset.Tests
{
    public class CleaningToilSessionTests : IDisposable
    {
        private readonly JobDriver_CleanFilth _driver = new JobDriver_CleanFilth();
        private readonly CleaningToilSession _session;

        public CleaningToilSessionTests()
        {
            var mop = new ThingDef { defName = "TestMop" };
            _session = new CleaningToilSession(_driver, mop, new CleaningToolExtension());
            ActiveCleaningDriver.Current = null;
        }

        public void Dispose()
        {
            ActiveCleaningDriver.Current = null;
        }

        [Fact]
        public void WrappedPreTickAction_WhileRunning_ExposesTheSessionDriver()
        {
            JobDriver seen = null;
            var toil = WorkToil();
            toil.AddPreTickAction(() => seen = ActiveCleaningDriver.Current);
            _session.Attach(toil);

            toil.preTickActions[0]();

            Assert.Same(_driver, seen);
        }

        [Fact]
        public void WrappedTickIntervalAction_WhileRunning_ExposesTheSessionDriver()
        {
            JobDriver seen = null;
            var toil = WorkToil();
            toil.tickIntervalAction = _ => seen = ActiveCleaningDriver.Current;
            _session.Attach(toil);

            toil.tickIntervalAction(1);

            Assert.Same(_driver, seen);
        }

        [Fact]
        public void WrappedPreTickAction_AfterRunning_RestoresThePreviousDriver()
        {
            var outer = new JobDriver_ClearSnowAndSand();
            ActiveCleaningDriver.Current = outer;
            var toil = WorkToil();
            toil.AddPreTickAction(() => { });
            _session.Attach(toil);

            toil.preTickActions[0]();

            Assert.Same(outer, ActiveCleaningDriver.Current);
        }

        [Fact]
        public void WrappedPreTickAction_WhenTheOriginalThrows_RestoresThePreviousDriver()
        {
            var toil = WorkToil();
            toil.AddPreTickAction(() => throw new InvalidOperationException());
            _session.Attach(toil);

            Assert.Throws<InvalidOperationException>(() => toil.preTickActions[0]());
            Assert.Null(ActiveCleaningDriver.Current);
        }

        [Fact]
        public void Attach_WithSeveralPreTickActions_KeepsEveryActionInOrder()
        {
            var calls = new List<int>();
            var toil = WorkToil();
            toil.AddPreTickAction(() => calls.Add(1));
            toil.AddPreTickAction(() => calls.Add(2));
            _session.Attach(toil);

            foreach (var action in toil.preTickActions) action();

            Assert.Equal(new[] { 1, 2 }, calls);
        }

        [Theory]
        [InlineData(ToilCompleteMode.Never, true, true)]
        [InlineData(ToilCompleteMode.PatherArrival, true, false)]
        [InlineData(ToilCompleteMode.Never, false, false)]
        public void IsWorkToil_ByCompleteModeAndTickAction_MatchesOnlyTheWorkToil(
            ToilCompleteMode mode, bool hasTickIntervalAction, bool expected)
        {
            var toil = new Toil { defaultCompleteMode = mode };
            if (hasTickIntervalAction) toil.tickIntervalAction = _ => { };

            Assert.Equal(expected, CleaningToilSession.IsWorkToil(toil));
        }

        private static Toil WorkToil()
        {
            return new Toil { defaultCompleteMode = ToilCompleteMode.Never, tickIntervalAction = _ => { } };
        }
    }
}
