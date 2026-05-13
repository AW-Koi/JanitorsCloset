namespace JanitorsCloset.Cleaning
{
    public enum CleaningCategory
    {
        Dry,
        Wet,
        // Toxic isn't a filth class — there are no Toxic FilthDefs. It's a flag that the
        // tool speeds up the Biotech "clear pollution" job (JobDriver_ClearPollution),
        // which is terrain work driven by GeneralLaborSpeed, not CleaningSpeed.
        // See StatPart_PollutionToolBonus for the hook.
        Toxic,
    }
}
