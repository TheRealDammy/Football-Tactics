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

            IReadOnlyList<Player> squad =
                team.GetFullSquad();

            foreach (FormationSlot slot in definition.Slots)
            {
                Player bestPlayer =
                    FindBestPlayerForSlot(
                        squad,
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

        public static Lineup BuildFromSlotViews(
            Formation formation,
            IReadOnlyList<LineupSlotView> slotViews)
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

        public static bool CanPlayerPlaySlot(
            Player player,
            FormationSlot slot)
        {
            return slot.Area switch
            {
                FormationArea.Goalkeeper =>
                    player.Position ==
                    PlayerPosition.Goalkeeper,

                FormationArea.LeftBack or
                FormationArea.RightBack or
                FormationArea.CentreBack =>
                    player.Position ==
                    PlayerPosition.Defender,

                // Wide midfielders can be filled by a
                // midfielder or a natural winger.
                FormationArea.LeftMidfield or
                FormationArea.RightMidfield =>
                    player.Position ==
                        PlayerPosition.Midfielder
                    ||
                    (
                        player.Position ==
                            PlayerPosition.Attacker
                        &&
                        player.Role ==
                            PlayerRole.Winger
                    )
                    ||
                    (
                        player.Position ==
                            PlayerPosition.Defender
                        &&
                        player.Role ==
                            PlayerRole.FullBack
                    ),

                FormationArea.DefensiveMidfield or
                FormationArea.CentreMidfield or
                FormationArea.AttackingMidfield =>
                    player.Position ==
                    PlayerPosition.Midfielder,

                FormationArea.LeftWing or
                FormationArea.RightWing =>
                    player.Position ==
                    PlayerPosition.Attacker,

                FormationArea.Striker =>
                    player.Position ==
                    PlayerPosition.Attacker,

                _ => false
            };
        }

        private static Player FindBestPlayerForSlot(
            IEnumerable<Player> squad,
            FormationSlot slot,
            HashSet<Player> usedPlayers)
        {
            Player bestPlayer = null;
            float bestScore = float.MinValue;

            foreach (Player player in squad)
            {
                if (usedPlayers.Contains(player))
                    continue;

                if (!CanPlayerPlaySlot(player, slot))
                    continue;

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

        private static float CalculateSlotScore(
            Player player,
            FormationSlot slot)
        {
            float score =
                player.Fitness * 0.15f;

            switch (slot.Area)
            {
                case FormationArea.Goalkeeper:
                    score += player.Defence * 0.80f;

                    if (player.Role ==
                        PlayerRole.Goalkeeper)
                    {
                        score += 20f;
                    }

                    break;

                case FormationArea.LeftBack:
                case FormationArea.RightBack:
                    score += player.Defence * 0.40f;
                    score += player.Pace * 0.30f;
                    score += player.Passing * 0.20f;

                    if (player.Role ==
                        PlayerRole.FullBack)
                    {
                        score += 20f;
                    }

                    break;

                case FormationArea.CentreBack:
                    score += player.Defence * 0.65f;
                    score += player.Pace * 0.10f;
                    score += player.Passing * 0.10f;

                    if (player.Role ==
                        PlayerRole.CentreBack)
                    {
                        score += 20f;
                    }

                    if (player.Role ==
                        PlayerRole.Sweeper ||
                        player.Role ==
                        PlayerRole.LineHolding)
                    {
                        score += 15f;
                    }

                    break;

                case FormationArea.LeftMidfield:
                case FormationArea.RightMidfield:

                    score += player.Pace * 0.25f;
                    score += player.Attack * 0.30f;
                    score += player.Passing * 0.25f;

                    if (player.Role ==
                        PlayerRole.Winger)
                    {
                        score += 25f;
                    }

                    if (player.Role ==
                        PlayerRole.BoxToBox)
                    {
                        score += 18f;
                    }

                    if (player.Role ==
                        PlayerRole.CentralMidfielder)
                    {
                        score += 12f;
                    }

                    if (player.Role ==
                        PlayerRole.FullBack)
                    {
                        score += 8f;
                    }

                    break;

                case FormationArea.CentreMidfield:
                    score += player.Passing * 0.40f;
                    score += player.Defence * 0.20f;
                    score += player.Attack * 0.20f;

                    if (player.Role ==
                        PlayerRole.CentralMidfielder)
                    {
                        score += 18f;
                    }

                    if (player.Role ==
                        PlayerRole.Playmaker)
                    {
                        score += 18f;
                    }

                    if (player.Role ==
                        PlayerRole.BoxToBox)
                    {
                        score += 16f;
                    }

                    if (player.Role ==
                        PlayerRole.DefensiveMidfielder)
                    {
                        score += 12f;
                    }

                    break;

                case FormationArea.DefensiveMidfield:
                    score += player.Defence * 0.40f;
                    score += player.Passing * 0.40f;

                    if (player.Role ==
                        PlayerRole.DefensiveMidfielder)
                    {
                        score += 25f;
                    }

                    break;

                case FormationArea.AttackingMidfield:
                    score += player.Attack * 0.40f;
                    score += player.Passing * 0.40f;

                    if (player.Role ==
                        PlayerRole.Playmaker)
                    {
                        score += 25f;
                    }

                    if (player.Role ==
                        PlayerRole.BoxToBox)
                    {
                        score += 12f;
                    }

                    break;

                case FormationArea.LeftWing:
                case FormationArea.RightWing:
                    score += player.Attack * 0.45f;
                    score += player.Pace * 0.30f;

                    if (player.Role ==
                        PlayerRole.Winger)
                    {
                        score += 25f;
                    }

                    break;

                case FormationArea.Striker:
                    score += player.Attack * 0.60f;
                    score += player.Pace * 0.20f;

                    if (player.Role ==
                        PlayerRole.Striker)
                    {
                        score += 25f;
                    }

                    break;
            }

            return score;
        }
    }
}