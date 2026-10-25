using Verse.AI;

namespace JanitorsCloset.Cleaning
{
    /// <summary>The cleaning driver whose work toil is running right now, or null.</summary>
    /// <remarks>Set only around a tool holder's work-toil delegates, so code without a pawn can tell who is cleaning.</remarks>
    public static class ActiveCleaningDriver
    {
        public static JobDriver Current;
    }
}
