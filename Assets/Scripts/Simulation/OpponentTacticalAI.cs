namespace FootballTactics.Simulation
{
    public static class OpponentTacticalAI
    {
        public static void Update(MatchEngine engine)
        {
            if (engine.State.Minute < 10)
                return;

            if (engine.State.Minute % 5 != 0)
                return;

            int goalDifference =
                engine.State.AwayGoals -
                engine.State.HomeGoals;

            float fitness =
                engine.AwayTeam.GetAverageFitness(
                    engine.AwayLineup);

            // The opposition is losing badly.
            if (goalDifference <= -2)
            {
                ApplyLosingByTwo(
                    engine,
                    fitness);

                return;
            }

            // The opposition is losing.
            if (goalDifference == -1)
            {
                ApplyLosing(
                    engine,
                    fitness);

                return;
            }

            // The opposition is winning comfortably.
            if (goalDifference >= 2 &&
                engine.State.Minute >= 60)
            {
                ApplyWinningByTwo(
                    engine,
                    fitness);

                return;
            }

            // The opposition is winning late.
            if (goalDifference == 1 &&
                engine.State.Minute >= 70)
            {
                ApplyWinningLate(
                    engine,
                    fitness);

                return;
            }

            // Match is level.
            if (goalDifference == 0)
            {
                ApplyDrawing(
                    engine,
                    fitness);
            }
        }

        private static void ApplyLosingByTwo(
            MatchEngine engine,
            float fitness)
        {
            engine.SetAwayMentality(
                Mentality.Attacking,
                false);

            engine.SetAwayPressing(
                fitness > 55f
                    ? Pressing.High
                    : Pressing.Medium,
                false);

            engine.SetAwayDefensiveLine(
                DefensiveLine.High,
                false);

            engine.SetAwayFormation(
                Formation.FourThreeThree,
                false);
        }

        private static void ApplyLosing(
            MatchEngine engine,
            float fitness)
        {
            engine.SetAwayMentality(
                Mentality.Attacking,
                false);

            engine.SetAwayPressing(
                fitness > 55f
                    ? Pressing.High
                    : Pressing.Medium,
                false);

            engine.SetAwayDefensiveLine(
                DefensiveLine.Normal,
                false);
        }

        private static void ApplyWinningByTwo(
            MatchEngine engine,
            float fitness)
        {
            engine.SetAwayMentality(
                Mentality.Defensive,
                false);

            engine.SetAwayPressing(
                Pressing.Low,
                false);

            engine.SetAwayDefensiveLine(
                DefensiveLine.Deep,
                false);

            engine.SetAwayFormation(
                Formation.FourFourTwo,
                false);
        }

        private static void ApplyWinningLate(
            MatchEngine engine,
            float fitness)
        {
            engine.SetAwayMentality(
                Mentality.Defensive,
                false);

            engine.SetAwayPressing(
                fitness > 45f
                    ? Pressing.Medium
                    : Pressing.Low,
                false);

            engine.SetAwayDefensiveLine(
                DefensiveLine.Deep,
                false);
        }

        private static void ApplyDrawing(
            MatchEngine engine,
            float fitness)
        {
            // Don't constantly make changes during a level match.
            if (engine.State.Minute < 45)
                return;

            engine.SetAwayMentality(
                Mentality.Balanced,
                false);

            engine.SetAwayPressing(
                fitness > 55f
                    ? Pressing.Medium
                    : Pressing.Low,
                false);

            engine.SetAwayDefensiveLine(
                DefensiveLine.Normal,
                false);
        }
    }
}