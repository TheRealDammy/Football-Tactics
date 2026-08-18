namespace FootballTactics.Simulation
{
    public class TeamTacticalProfile
    {
        public float Possession;

        public float BuildUp;

        public float ChanceCreation;

        public float DefensiveStability;

        public float CounterAttack;

        public float Width;

        public float PressResistance;

        public TeamTacticalProfile()
        {
            Possession = 50f;
            BuildUp = 50f;
            ChanceCreation = 50f;
            DefensiveStability = 50f;
            CounterAttack = 50f;
            Width = 50f;
            PressResistance = 50f;
        }
    }
}