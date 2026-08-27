namespace FootballTactics.Simulation
{
    /// <summary>
    /// Converts a resolved player decision into a short-lived tactical change
    /// using the same tactical settings consumed by MatchEngine.
    /// This sits at the simulation boundary so the existing decision UI remains unchanged.
    /// </summary>
    public sealed class TacticalDecisionEffectRuntime
    {
        private bool active;
        private int minutesRemaining;

        private Mentality originalMentality;
        private Pressing originalPressing;
        private DefensiveLine originalDefensiveLine;

        private Mentality appliedMentality;
        private Pressing appliedPressing;
        private DefensiveLine appliedDefensiveLine;

        public void Apply(MatchEngine engine, TacticalSituationOption option)
        {
            if (engine == null || option == null)
                return;

            RestoreIfActive(engine);

            originalMentality = engine.HomeTactics.Mentality;
            originalPressing = engine.HomeTactics.Pressing;
            originalDefensiveLine = engine.HomeTactics.DefensiveLine;

            Mentality mentality = originalMentality;
            Pressing pressing = originalPressing;
            DefensiveLine defensiveLine = originalDefensiveLine;

            // Chance creation is represented primarily through mentality.
            if (option.ChanceModifier >= 1.06f)
                mentality = Mentality.Attacking;
            else if (option.ChanceModifier <= 0.94f)
                mentality = Mentality.Defensive;

            // Possession-focused choices favour a controlled approach.
            if (option.PossessionModifier >= 1.04f &&
                option.ChanceModifier <= 1.00f)
            {
                mentality = Mentality.Balanced;
            }

            // High fatigue cost means we should reduce pressing intensity.
            if (option.FatigueModifier >= 1.10f)
                pressing = Pressing.Low;
            else if (option.FatigueModifier <= 0.80f)
                pressing = Pressing.Low;
            else if (option.ChanceModifier >= 1.06f &&
                     option.PossessionModifier <= 0.98f)
                pressing = Pressing.High;

            // A strong counter-attacking choice accepts more space behind the defence.
            if (option.CounterAttackModifier >= 1.10f)
                defensiveLine = DefensiveLine.High;
            else if (option.CounterAttackModifier <= 0.75f)
                defensiveLine = DefensiveLine.Deep;

            engine.SetHomeMentality(mentality);
            engine.SetHomePressing(pressing);
            engine.SetHomeDefensiveLine(defensiveLine);

            appliedMentality = mentality;
            appliedPressing = pressing;
            appliedDefensiveLine = defensiveLine;

            minutesRemaining = 5;
            active = true;

            engine.State.AddEvent(
                $"Tactical effect active for 5 minutes: " +
                $"{mentality}, {pressing}, {defensiveLine}.");
        }

        public void Tick(MatchEngine engine)
        {
            if (!active || engine == null)
                return;

            minutesRemaining--;

            if (minutesRemaining > 0)
                return;

            RestoreIfActive(engine);
        }

        private void RestoreIfActive(MatchEngine engine)
        {
            if (!active || engine == null)
                return;

            // Only restore settings that were not manually changed while the
            // temporary effect was active.
            if (engine.HomeTactics.Mentality == appliedMentality)
                engine.SetHomeMentality(originalMentality);

            if (engine.HomeTactics.Pressing == appliedPressing)
                engine.SetHomePressing(originalPressing);

            if (engine.HomeTactics.DefensiveLine == appliedDefensiveLine)
                engine.SetHomeDefensiveLine(originalDefensiveLine);

            engine.State.AddEvent("Temporary tactical decision effect has ended.");

            active = false;
            minutesRemaining = 0;
        }
    }
}
