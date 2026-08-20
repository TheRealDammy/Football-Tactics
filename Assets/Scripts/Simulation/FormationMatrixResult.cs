namespace FootballTactics.Simulation
{
    public sealed class FormationMatrixResult
    {
        public SquadArchetype Squad { get; }

        public Formation Formation { get; }

        public float WinRate { get; }

        public float AverageGoals { get; }

        public float AveragePossession { get; }

        public float AverageShots { get; }

        public float AverageXG { get; }

        public FormationMatrixResult(
            SquadArchetype squad,
            Formation formation,
            SimulationResult result)
        {
            Squad = squad;
            Formation = formation;

            WinRate = result.WinRate;
            AverageGoals = result.AverageGoals;
            AveragePossession = result.AveragePossession;
            AverageShots = result.AverageShots;
            AverageXG = result.AverageXG;
        }
    }
}