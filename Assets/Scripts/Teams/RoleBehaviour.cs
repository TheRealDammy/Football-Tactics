using FootballTactics.Simulation;

namespace FootballTactics.Teams
{
    public static class RoleBehaviour
    {
        public static float BuildUpContribution(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Sweeper => 0.10f,
                PlayerRole.LineHolding => 0.04f,
                PlayerRole.CentreBack => 0.06f,
                PlayerRole.FullBack => 0.08f,

                PlayerRole.DefensiveMidfielder => 0.14f,
                PlayerRole.CentralMidfielder => 0.12f,
                PlayerRole.Playmaker => 0.20f,
                PlayerRole.BoxToBox => 0.11f,

                PlayerRole.Winger => 0.08f,
                PlayerRole.Striker => 0.04f,

                _ => 0.05f
            };
        }

        public static float ChanceCreationContribution(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Playmaker => 0.24f,
                PlayerRole.Winger => 0.18f,
                PlayerRole.BoxToBox => 0.12f,
                PlayerRole.CentralMidfielder => 0.10f,
                PlayerRole.Striker => 0.16f,

                PlayerRole.FullBack => 0.08f,

                PlayerRole.DefensiveMidfielder => 0.04f,
                PlayerRole.CentreBack => 0.02f,
                PlayerRole.Sweeper => 0.01f,
                PlayerRole.LineHolding => 0.01f,

                _ => 0.05f
            };
        }

        public static float PressingContribution(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.BoxToBox => 0.18f,
                PlayerRole.Winger => 0.16f,
                PlayerRole.Striker => 0.14f,
                PlayerRole.CentralMidfielder => 0.12f,
                PlayerRole.DefensiveMidfielder => 0.11f,

                PlayerRole.FullBack => 0.08f,
                PlayerRole.Playmaker => 0.07f,

                PlayerRole.CentreBack => 0.05f,
                PlayerRole.Sweeper => 0.04f,
                PlayerRole.LineHolding => 0.02f,

                _ => 0.05f
            };
        }

        public static float DefensiveContribution(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.LineHolding => 0.24f,
                PlayerRole.Sweeper => 0.20f,
                PlayerRole.CentreBack => 0.22f,
                PlayerRole.FullBack => 0.15f,
                PlayerRole.DefensiveMidfielder => 0.20f,

                PlayerRole.BoxToBox => 0.10f,
                PlayerRole.CentralMidfielder => 0.09f,

                PlayerRole.Winger => 0.05f,
                PlayerRole.Playmaker => 0.04f,
                PlayerRole.Striker => 0.03f,

                _ => 0.05f
            };
        }
    }
}