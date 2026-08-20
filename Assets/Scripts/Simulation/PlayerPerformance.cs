using FootballTactics.Teams;
using UnityEngine;

namespace FootballTactics.Simulation
{
    public static class PlayerPerformance
    {
        public static float GetAttackRating(
            Player player)
        {
            float fitness =
                player.Fitness / 100f;

            float rating =
                player.Attack;

            // Fitness directly affects performance.
            rating *= Mathf.Lerp(
                0.60f,
                1.00f,
                fitness);

            rating *= GetRoleAttackModifier(
                player.Role);

            return rating;
        }

        public static float GetDefenceRating(
            Player player)
        {
            float fitness =
                player.Fitness / 100f;

            float rating =
                player.Defence;

            rating *= Mathf.Lerp(
                0.65f,
                1.00f,
                fitness);

            rating *= GetRoleDefenceModifier(
                player.Role);

            return rating;
        }

        public static float GetPassingRating(
            Player player)
        {
            float fitness =
                player.Fitness / 100f;

            float rating =
                player.Passing;

            rating *= Mathf.Lerp(
                0.70f,
                1.00f,
                fitness);

            return rating;
        }

        public static float GetPaceRating(
            Player player)
        {
            float fitness =
                player.Fitness / 100f;

            float rating =
                player.Pace;

            // Pace degrades noticeably when tired.
            rating *= Mathf.Lerp(
                0.70f,
                1.00f,
                fitness);

            return rating;
        }

        private static float GetRoleAttackModifier(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Striker => 1.12f,
                PlayerRole.Winger => 1.08f,
                PlayerRole.Playmaker => 1.06f,
                PlayerRole.BoxToBox => 1.02f,

                PlayerRole.CentralMidfielder => 1.00f,
                PlayerRole.DefensiveMidfielder => 0.93f,

                PlayerRole.FullBack => 0.88f,
                PlayerRole.CentreBack => 0.70f,
                PlayerRole.Sweeper => 0.68f,
                PlayerRole.LineHolding => 0.62f,

                PlayerRole.Goalkeeper => 0.15f,

                _ => 1.00f
            };
        }

        private static float GetRoleDefenceModifier(
            PlayerRole role)
        {
            return role switch
            {
                PlayerRole.LineHolding => 1.15f,
                PlayerRole.Sweeper => 1.10f,
                PlayerRole.CentreBack => 1.12f,
                PlayerRole.DefensiveMidfielder => 1.08f,

                PlayerRole.FullBack => 1.04f,
                PlayerRole.BoxToBox => 1.00f,
                PlayerRole.CentralMidfielder => 0.98f,

                PlayerRole.Playmaker => 0.88f,
                PlayerRole.Winger => 0.82f,
                PlayerRole.Striker => 0.65f,

                PlayerRole.Goalkeeper => 1.00f,

                _ => 1.00f
            };
        }
    }
}