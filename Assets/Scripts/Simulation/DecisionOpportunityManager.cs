using UnityEngine;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Controls when player-facing tactical decisions are appropriate.
    /// The match engine remains responsible for generating the actual situation;
    /// this class prevents decisions from becoming evenly/randomly distributed
    /// and gives the match a natural rhythm.
    /// </summary>
    public static class DecisionOpportunityManager
    {
        public static bool IsDecisionWindow(MatchEngine engine)
        {
            if (engine == null || engine.State == null)
                return false;

            int minute = engine.State.Minute;

            if (minute < 8 || minute >= 90)
                return false;

            // Early assessment: occasional decisions, not constant interruptions.
            if (minute >= 8 && minute <= 14)
                return true;

            // First tactical phase.
            if (minute >= 18 && minute <= 27)
                return true;

            // End of first half.
            if (minute >= 32 && minute <= 43)
                return true;

            // Opening of second half.
            if (minute >= 48 && minute <= 58)
                return true;

            // Main substitution/tactical window.
            if (minute >= 62 && minute <= 73)
                return true;

            // Late-game management.
            if (minute >= 76 && minute <= 85)
                return true;

            // Final push/protection phase.
            if (minute >= 86 && minute <= 89)
                return true;

            return false;
        }

        public static float GetDecisionWeight(MatchEngine engine)
        {
            if (engine == null || engine.State == null)
                return 0f;

            int minute = engine.State.Minute;
            int goalDifference =
                engine.State.HomeGoals - engine.State.AwayGoals;

            float weight = 1f;

            // Decisions become more important as the result becomes clearer.
            if (minute >= 62)
                weight += 0.15f;

            if (minute >= 76)
                weight += 0.20f;

            // Losing teams need more intervention opportunities.
            if (goalDifference < 0)
                weight += 0.20f;
            else if (goalDifference > 0 && minute >= 70)
                weight += 0.10f;

            // Tight matches are also tactically valuable.
            if (Mathf.Abs(goalDifference) == 0)
                weight += 0.10f;

            return weight;
        }
    }
}
