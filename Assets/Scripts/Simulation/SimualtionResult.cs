namespace FootballTactics.Simulation
{
    public sealed class SimulationResult
    {
        public int Matches { get; private set; }

        public int Wins { get; private set; }
        public int Draws { get; private set; }
        public int Losses { get; private set; }

        public float TotalGoals { get; private set; }
        public float TotalPossession { get; private set; }
        public float TotalShots { get; private set; }
        public float TotalXG { get; private set; }
        public float TotalFitness { get; private set; }

        public void Record(
            MatchState state,
            float averageFitness)
        {
            Matches++;

            if (state.HomeGoals > state.AwayGoals)
            {
                Wins++;
            }
            else if (state.HomeGoals == state.AwayGoals)
            {
                Draws++;
            }
            else
            {
                Losses++;
            }

            TotalGoals += state.HomeGoals;

            TotalPossession +=
                state.HomePossession;

            TotalShots +=
                state.HomeShots;

            TotalXG +=
                state.HomeXG;

            TotalFitness +=
                averageFitness;
        }

        public float WinRate =>
            Matches == 0
                ? 0f
                : Wins / (float)Matches * 100f;

        public float DrawRate =>
            Matches == 0
                ? 0f
                : Draws / (float)Matches * 100f;

        public float LossRate =>
            Matches == 0
                ? 0f
                : Losses / (float)Matches * 100f;

        public float AverageGoals =>
            Matches == 0
                ? 0f
                : TotalGoals / Matches;

        public float AveragePossession =>
            Matches == 0
                ? 0f
                : TotalPossession / Matches;

        public float AverageShots =>
            Matches == 0
                ? 0f
                : TotalShots / Matches;

        public float AverageXG =>
            Matches == 0
                ? 0f
                : TotalXG / Matches;

        public float AverageFitness =>
            Matches == 0
                ? 0f
                : TotalFitness / Matches;
    }
}