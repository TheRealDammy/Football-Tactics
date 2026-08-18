namespace FootballTactics.Simulation
{
    public readonly struct MatchEvent
    {
        public int Minute { get; }
        public string Description { get; }

        public MatchEvent(int minute, string description)
        {
            Minute = minute;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Minute}'  {Description}";
        }
    }
}