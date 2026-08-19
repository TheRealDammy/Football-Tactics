namespace FootballTactics.Simulation
{
    public enum MatchSituationType
    {
        BuildUp,
        CounterAttack,
        Pressing,
        DefensiveTransition,
        ChanceCreation,
        DefensiveStand,
        Fatigue
    }

    public readonly struct MatchSituation
    {
        public MatchSituationType Type { get; }

        public string Description { get; }

        public bool IsHome { get; }

        public float Impact { get; }

        public MatchSituation(
            MatchSituationType type,
            string description,
            bool isHome,
            float impact)
        {
            Type = type;
            Description = description;
            IsHome = isHome;
            Impact = impact;
        }
    }
}