using System.Collections.Generic;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public readonly struct FormationFitResult
    {
        public float Score { get; }

        public string Summary { get; }

        public FormationFitResult(
            float score,
            string summary)
        {
            Score = score;
            Summary = summary;
        }
    }

    public static class FormationCompatibility
    {
        public static FormationFitResult Calculate(
            Lineup lineup)
        {
            if (lineup == null ||
                lineup.Assignments.Count == 0)
            {
                return new FormationFitResult(
                    0f,
                    "No lineup selected.");
            }

            float total = 0f;
            int count = 0;

            foreach (var assignment in lineup.Assignments)
            {
                Player player = assignment.Value;

                if (player == null)
                    continue;

                FormationArea area =
                    lineup.GetFormationArea(
                        assignment.Key);

                total +=
                    GetRoleSuitability(
                        area,
                        player.Role);

                count++;
            }

            if (count == 0)
            {
                return new FormationFitResult(
                    0f,
                    "No players selected.");
            }

            float score =
                total / count * 100f;

            string summary;

            if (score >= 88f)
            {
                summary =
                    "Excellent fit for this squad.";
            }
            else if (score >= 76f)
            {
                summary =
                    "Strong fit for this squad.";
            }
            else if (score >= 62f)
            {
                summary =
                    "Reasonable fit with some compromises.";
            }
            else
            {
                summary =
                    "Poor fit for the current squad.";
            }

            return new FormationFitResult(
                score,
                summary);
        }

        private static float GetRoleSuitability(
            FormationArea area,
            PlayerRole role)
        {
            return area switch
            {
                FormationArea.Goalkeeper =>
                    role == PlayerRole.Goalkeeper
                        ? 1.00f
                        : 0.60f,

                FormationArea.LeftBack or
                FormationArea.RightBack =>
                    role switch
                    {
                        PlayerRole.FullBack => 1.00f,
                        PlayerRole.LineHolding => 0.82f,
                        PlayerRole.CentreBack => 0.68f,
                        _ => 0.45f
                    },

                FormationArea.CentreBack =>
                    role switch
                    {
                        PlayerRole.CentreBack => 1.00f,
                        PlayerRole.Sweeper => 0.96f,
                        PlayerRole.LineHolding => 0.96f,
                        _ => 0.45f
                    },

                FormationArea.DefensiveMidfield =>
                    role switch
                    {
                        PlayerRole.DefensiveMidfielder => 1.00f,
                        PlayerRole.CentralMidfielder => 0.82f,
                        PlayerRole.BoxToBox => 0.78f,
                        PlayerRole.Playmaker => 0.65f,
                        _ => 0.35f
                    },

                FormationArea.CentreMidfield =>
                    role switch
                    {
                        PlayerRole.CentralMidfielder => 1.00f,
                        PlayerRole.BoxToBox => 0.96f,
                        PlayerRole.Playmaker => 0.94f,
                        PlayerRole.DefensiveMidfielder => 0.78f,
                        _ => 0.40f
                    },

                FormationArea.AttackingMidfield =>
                    role switch
                    {
                        PlayerRole.Playmaker => 1.00f,
                        PlayerRole.BoxToBox => 0.88f,
                        PlayerRole.CentralMidfielder => 0.78f,
                        _ => 0.40f
                    },

                FormationArea.LeftMidfield or
                FormationArea.RightMidfield or
                FormationArea.LeftWing or
                FormationArea.RightWing =>
                    role switch
                    {
                        PlayerRole.Winger => 1.00f,
                        PlayerRole.BoxToBox => 0.82f,
                        PlayerRole.FullBack => 0.65f,
                        _ => 0.38f
                    },

                FormationArea.Striker =>
                    role switch
                    {
                        PlayerRole.Striker => 1.00f,
                        PlayerRole.Winger => 0.72f,
                        _ => 0.35f
                    },

                _ => 0.50f
            };
        }
    }
}