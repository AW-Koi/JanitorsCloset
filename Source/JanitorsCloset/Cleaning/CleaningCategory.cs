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

        // Vanilla's "weather buildup" layers — snow and Odyssey's windblown sand. Named
        // after RimWorld's own WeatherBuildupCategory / WeatherBuildupUtility, which
        // already enumerate the Dusting/Thin/Medium/Thick depth buckets we care about.
        // Cleared by JobDriver_ClearSnowAndSand against GeneralLaborSpeed, not CleaningSpeed.
        // Per-tool weatherBuildupDepthCap caps the depth at which the bonus (and the
        // cleaning anim) apply, so a straw broom can sweep a light dusting but not shovel
        // a blizzard. See StatPart_WeatherBuildupToolBonus.
        WeatherBuildup,
    }
}
