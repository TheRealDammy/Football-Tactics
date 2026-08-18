namespace FootballTactics.Teams
{
    public static class RoleCompatibility
    {
        public static float GetSuitability(
            Player player,
            PlayerRole desiredRole)
        {
            if (player.Role == desiredRole)
                return 1.0f;

            return (player.Position, desiredRole) switch
            {
                (
                    PlayerPosition.Defender,
                    PlayerRole.Sweeper
                ) => 0.85f,

                (
                    PlayerPosition.Defender,
                    PlayerRole.LineHolding
                ) => 0.90f,

                (
                    PlayerPosition.Defender,
                    PlayerRole.FullBack
                ) => 0.90f,

                (
                    PlayerPosition.Midfielder,
                    PlayerRole.CentralMidfielder
                ) => 0.90f,

                (
                    PlayerPosition.Midfielder,
                    PlayerRole.Playmaker
                ) => 0.85f,

                (
                    PlayerPosition.Midfielder,
                    PlayerRole.DefensiveMidfielder
                ) => 0.85f,

                (
                    PlayerPosition.Midfielder,
                    PlayerRole.BoxToBox
                ) => 0.90f,

                (
                    PlayerPosition.Attacker,
                    PlayerRole.Winger
                ) => 0.90f,

                (
                    PlayerPosition.Attacker,
                    PlayerRole.Striker
                ) => 0.90f,

                _ => 0f
            };
        }
    }
}