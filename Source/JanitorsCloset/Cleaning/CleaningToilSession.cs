using System;
using JanitorsCloset.Cleaning.ToolEffects;
using Verse;
using Verse.AI;

namespace JanitorsCloset.Cleaning
{
    /// <summary>Per-job state for a tool holder's cleaning work toil.</summary>
    /// <remarks>Created only when the pawn holds a cleaning tool, so other pawns pay nothing per tick.</remarks>
    public sealed class CleaningToilSession
    {
        public readonly JobDriver Driver;
        public readonly ThingDef ToolDef;
        public readonly CleaningToolExtension Tool;

        public TickInterval SprayInterval;
        public TickInterval FoamInterval;
        public TickInterval GlowInterval;
        public int LastStrokeCycle = int.MinValue;

        public CleaningToilSession(JobDriver driver, ThingDef toolDef, CleaningToolExtension tool)
        {
            Driver = driver;
            ToolDef = toolDef;
            Tool = tool;
        }

        public Pawn Pawn => Driver.pawn;

        public static CleaningToilSession TryStart(JobDriver driver)
        {
            var toolDef = driver?.pawn?.equipment?.Primary?.def;
            var tool = toolDef?.GetModExtension<CleaningToolExtension>();
            return tool == null ? null : new CleaningToilSession(driver, toolDef, tool);
        }

        public static bool IsWorkToil(Toil toil)
        {
            return toil.defaultCompleteMode == ToilCompleteMode.Never && toil.tickIntervalAction != null;
        }

        public void Attach(Toil toil)
        {
            var preTick = toil.preTickActions;
            if (preTick != null)
            {
                for (int i = 0; i < preTick.Count; i++) preTick[i] = Scoped(preTick[i]);
            }

            var originalInterval = toil.tickIntervalAction;
            toil.tickIntervalAction = delta =>
            {
                var previous = ActiveCleaningDriver.Current;
                ActiveCleaningDriver.Current = Driver;
                try
                {
                    originalInterval(delta);
                }
                finally
                {
                    ActiveCleaningDriver.Current = previous;
                }
                CleaningToolEffects.Tick(this, delta);
            };

            if (Tool.clearsFilthStack)
            {
                var originalInit = toil.initAction;
                toil.initAction = () =>
                {
                    GlittervacuumLayerCollapse.Apply(this);
                    originalInit?.Invoke();
                };
            }
        }

        private Action Scoped(Action original)
        {
            return () =>
            {
                var previous = ActiveCleaningDriver.Current;
                ActiveCleaningDriver.Current = Driver;
                try
                {
                    original();
                }
                finally
                {
                    ActiveCleaningDriver.Current = previous;
                }
            };
        }
    }
}
