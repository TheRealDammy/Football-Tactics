using FootballTactics.Simulation;
using FootballTactics.UI;
using System.Collections.Generic;
using System.Linq;

namespace FootballTactics.Teams
{
    public static class LineupBuilder
    {
        public static Lineup BuildRecommendedLineup(
            Team team,
            Formation formation)
        {
            FormationDefinition definition =
                formation.GetDefinition();

            Lineup lineup =
                new Lineup(formation);

            HashSet<Player> usedPlayers = new();

            foreach (FormationSlot slot in definition.Slots)
            {
                Player bestPlayer =
                    FindBestPlayerForSlot(
                        team,
                        slot,
                        usedPlayers);

                if (bestPlayer == null)
                    continue;

                lineup.Assign(
                    slot,
                    bestPlayer);

                usedPlayers.Add(bestPlayer);
            }

            return lineup;
        }

        private static Player FindBestPlayerForSlot(
            Team team,
            FormationSlot slot,
            HashSet<Player> usedPlayers)
        {
            Player bestPlayer = null;
            float bestScore = float.MinValue;

            foreach (Player player in team.Players)
            {
                if (usedPlayers.Contains(player))
                    continue;

                if (player.Position !=
                    slot.RequiredPosition)
                {
                    continue;
                }

                float score =
                    CalculateSlotScore(
                        player,
                        slot);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlayer = player;
                }
            }

            return bestPlayer;
        }

        public static Lineup BuildFromSlotViews(Formation formation, IReadOnlyList<LineupSlotView> slotViews)
        {
            Lineup lineup =
                new Lineup(formation);

            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Player == null)
                    continue;

                lineup.Assign(
                    slotView.Slot,
                    slotView.Player);
            }

            return lineup;
        }

        private static float CalculateSlotScore(Player player, FormationSlot slot)
        {
            float score = 0f;

            score += player.Fitness * 0.20f;

            switch (slot.Area)
            {
                case FormationArea.Goalkeeper:

                    score +=
                        player.Defence * 0.80f;

                    if (player.Role ==
                        PlayerRole.Goalkeeper)
                    {
                        score += 15f;
                    }

                    break;


                case FormationArea.LeftBack:
                case FormationArea.RightBack:

                    score +=
                        player.Defence * 0.45f;

                    score +=
                        player.Pace * 0.30f;

                    score +=
                        player.Passing * 0.15f;

                    if (player.Role ==
                        PlayerRole.FullBack)
                    {
                        score += 15f;
                    }

                    break;


                case FormationArea.CentreBack:

                    score +=
                        player.Defence * 0.65f;

                    score +=
                        player.Pace * 0.15f;

                    score +=
                        player.Passing * 0.10f;

                    if (player.Role ==
                        PlayerRole.CentreBack)
                    {
                        score += 15f;
                    }

                    if (player.Role ==
                        PlayerRole.Sweeper)
                    {
                        score += 8f;
                    }

                    if (player.Role ==
                        PlayerRole.LineHolding)
                    {
                        score += 8f;
                    }

                    break;


                case FormationArea.DefensiveMidfield:

                    score +=
                        player.Defence * 0.35f;

                    score +=
                        player.Passing * 0.40f;

                    score +=
                        player.Fitness * 0.10f;

                    if (player.Role ==
                        PlayerRole.DefensiveMidfielder)
                    {
                        score += 18f;
                    }

                    break;


                case FormationArea.CentreMidfield:

                    score +=
                        player.Passing * 0.40f;

                    score +=
                        player.Attack * 0.20f;

                    score +=
                        player.Defence * 0.20f;

                    if (player.Role ==
                        PlayerRole.CentralMidfielder)
                    {
                        score += 12f;
                    }

                    if (player.Role ==
                        PlayerRole.Playmaker)
                    {
                        score += 10f;
                    }

                    if (player.Role ==
                        PlayerRole.BoxToBox)
                    {
                        score += 10f;
                    }

                    break;


                case FormationArea.AttackingMidfield:

                    score +=
                        player.Attack * 0.40f;

                    score +=
                        player.Passing * 0.40f;

                    if (player.Role ==
                        PlayerRole.Playmaker)
                    {
                        score += 20f;
                    }

                    if (player.Role ==
                        PlayerRole.BoxToBox)
                    {
                        score += 6f;
                    }

                    break;


                case FormationArea.LeftMidfield:
                case FormationArea.RightMidfield:

                    score +=
                        player.Pace * 0.35f;

                    score +=
                        player.Attack * 0.30f;

                    score +=
                        player.Passing * 0.20f;

                    if (player.Role ==
                        PlayerRole.Winger)
                    {
                        score += 15f;
                    }

                    break;


                case FormationArea.LeftWing:
                case FormationArea.RightWing:

                    score +=
                        player.Pace * 0.30f;

                    score +=
                        player.Attack * 0.45f;

                    score +=
                        player.Passing * 0.15f;

                    if (player.Role ==
                        PlayerRole.Winger)
                    {
                        score += 20f;
                    }

                    break;


                case FormationArea.Striker:

                    score +=
                        player.Attack * 0.65f;

                    score +=
                        player.Pace * 0.20f;

                    if (player.Role ==
                        PlayerRole.Striker)
                    {
                        score += 20f;
                    }

                    break;
            }

            return score;
        }
    }
}