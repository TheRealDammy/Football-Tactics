namespace FootballTactics.Simulation
{
    public enum Mentality
    {
        Defensive,
        Balanced,
        Attacking
    }

    public enum Pressing
    {
        Low,
        Medium,
        High
    }

    public enum DefensiveLine
    {
        Deep,
        Normal,
        High
    }

    public class TacticalSettings
    {
        public Formation Formation { get; set; } = Formation.FourThreeThree;

        public Mentality Mentality { get; set; } = Mentality.Balanced;

        public Pressing Pressing { get; set; } = Pressing.Medium;

        public DefensiveLine DefensiveLine { get; set; } = DefensiveLine.Normal;

        public float GetAttackModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 0.90f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 1.12f,
                _ => 1.00f
            };
        }

        public float GetDefenceModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 1.12f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 0.90f,
                _ => 1.00f
            };
        }

        public float GetPossessionModifier()
        {
            return Mentality switch
            {
                Mentality.Defensive => 0.95f,
                Mentality.Balanced => 1.00f,
                Mentality.Attacking => 1.03f,
                _ => 1.00f
            };
        }

        public float GetPressingModifier()
        {
            return Pressing switch
            {
                Pressing.Low => 0.90f,
                Pressing.Medium => 1.00f,
                Pressing.High => 1.12f,
                _ => 1.00f
            };
        }

        public float GetFitnessDrain()
        {
            return Pressing switch
            {
                Pressing.Low => 0.6f,
                Pressing.Medium => 1.0f,
                Pressing.High => 1.5f,
                _ => 1.0f
            };
        }

        public float GetDefensiveLineRisk()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.85f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.18f,
                _ => 1.00f
            };
        }

        public float GetCounterAttackVulnerability()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.75f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.20f,
                _ => 1.00f
            };
        }

        public float GetTerritoryModifier()
        {
            return DefensiveLine switch
            {
                DefensiveLine.Deep => 0.94f,
                DefensiveLine.Normal => 1.00f,
                DefensiveLine.High => 1.06f,
                _ => 1.00f
            };
        }
    }
}