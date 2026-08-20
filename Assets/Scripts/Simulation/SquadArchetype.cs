namespace FootballTactics.Simulation
{
    public enum SquadArchetype
    {
        Balanced,
        Possession,
        WideAttack,
        Direct
    }

    public static class SquadArchetypeExtensions
    {
        public static string DisplayName(
            this SquadArchetype archetype)
        {
            return archetype switch
            {
                SquadArchetype.Possession =>
                    "Possession",

                SquadArchetype.WideAttack =>
                    "Wide Attack",

                SquadArchetype.Direct =>
                    "Direct",

                _ =>
                    "Balanced"
            };
        }
    }
}