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
            IReadOnlyList<Player> squad = team.GetFullSquad();

            // Fill the most restrictive positions first. This prevents a
            // flexible player (for example a winger in a wide midfield slot)
            // from consuming a player needed for a specialist position later.
            foreach (FormationSlot slot in definition.Slots
                         .OrderBy(GetSlotPriority))
            {
                Player bestPlayer = FindBestPlayerForSlot(
                    squad,
                    slot,
                    usedPlayers);

                if (bestPlayer == null)
                    continue;

                lineup.Assign(slot, bestPlayer);
                usedPlayers.Add(bestPlayer);
            }

            return lineup;
        }

        public static Lineup BuildFromSlotViews(
            Formation formation,
            IReadOnlyList<LineupSlotView> slotViews)
        {
            Lineup lineup = new Lineup(formation);

            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Player == null)
                    continue;

                if (!CanPlayerPlaySlot(slotView.Player, slotView.Slot))
                    continue;

                lineup.Assign(slotView.Slot, slotView.Player);
            }

            return lineup;
        }

        /// <summary>
        /// The single source of truth for whether a player can occupy a
        /// formation slot. UI lineup selection and match substitutions both
        /// use this method.
        /// </summary>
        public static bool CanPlayerPlaySlot(
            Player player,
            FormationSlot slot)
        {
            if (player == null || slot == null)
                return false;

            return slot.Area switch
            {
                FormationArea.Goalkeeper =>
                    player.Position == PlayerPosition.Goalkeeper &&
                    IsAny(player.Role,
                        PlayerRole.Goalkeeper,
                        PlayerRole.Sweeper,
                        PlayerRole.LineHolding),

                FormationArea.LeftBack or
                FormationArea.RightBack =>
                    player.Position == PlayerPosition.Defender &&
                    IsAny(player.Role,
                        PlayerRole.FullBack,
                        PlayerRole.LineHolding),

                FormationArea.CentreBack =>
                    player.Position == PlayerPosition.Defender &&
                    IsAny(player.Role,
                        PlayerRole.CentreBack,
                        PlayerRole.Sweeper,
                        PlayerRole.LineHolding),

                FormationArea.LeftMidfield or
                FormationArea.RightMidfield =>
                    player.Position == PlayerPosition.Midfielder &&
                    IsAny(player.Role,
                        PlayerRole.CentralMidfielder,
                        PlayerRole.Playmaker,
                        PlayerRole.DefensiveMidfielder,
                        PlayerRole.BoxToBox)
                    ||
                    player.Position == PlayerPosition.Attacker &&
                    player.Role == PlayerRole.Winger
                    ||
                    player.Position == PlayerPosition.Defender &&
                    player.Role == PlayerRole.FullBack,

                FormationArea.DefensiveMidfield =>
                    player.Position == PlayerPosition.Midfielder &&
                    IsAny(player.Role,
                        PlayerRole.DefensiveMidfielder,
                        PlayerRole.CentralMidfielder,
                        PlayerRole.BoxToBox),

                FormationArea.CentreMidfield =>
                    player.Position == PlayerPosition.Midfielder &&
                    IsAny(player.Role,
                        PlayerRole.CentralMidfielder,
                        PlayerRole.Playmaker,
                        PlayerRole.DefensiveMidfielder,
                        PlayerRole.BoxToBox),

                FormationArea.AttackingMidfield =>
                    player.Position == PlayerPosition.Midfielder &&
                    IsAny(player.Role,
                        PlayerRole.Playmaker,
                        PlayerRole.CentralMidfielder,
                        PlayerRole.BoxToBox),

                FormationArea.LeftWing or
                FormationArea.RightWing =>
                    player.Position == PlayerPosition.Attacker &&
                    player.Role == PlayerRole.Winger,

                FormationArea.Striker =>
                    player.Position == PlayerPosition.Attacker &&
                    player.Role == PlayerRole.Striker,

                _ => false
            };
        }

        private static bool IsAny(
            PlayerRole role,
            params PlayerRole[] allowed)
        {
            foreach (PlayerRole candidate in allowed)
            {
                if (role == candidate)
                    return true;
            }

            return false;
        }

        private static int GetSlotPriority(FormationSlot slot)
        {
            return slot.Area switch
            {
                FormationArea.Goalkeeper => 0,
                FormationArea.CentreBack => 1,
                FormationArea.LeftBack => 2,
                FormationArea.RightBack => 2,
                FormationArea.DefensiveMidfield => 3,
                FormationArea.CentreMidfield => 4,
                FormationArea.AttackingMidfield => 5,
                FormationArea.LeftWing => 6,
                FormationArea.RightWing => 6,
                FormationArea.Striker => 7,
                FormationArea.LeftMidfield => 8,
                FormationArea.RightMidfield => 8,
                _ => 99
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

                float score = CalculateSlotScore(player, slot);

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
            float score = player.Fitness * 0.15f;

            switch (slot.Area)
            {
                case FormationArea.Goalkeeper:
                    score += player.Defence * 0.80f;
                    if (player.Role == PlayerRole.Goalkeeper)
                        score += 20f;
                    break;

                case FormationArea.LeftBack:
                case FormationArea.RightBack:
                    score += player.Defence * 0.40f;
                    score += player.Pace * 0.30f;
                    score += player.Passing * 0.20f;
                    if (player.Role == PlayerRole.FullBack)
                        score += 20f;
                    break;

                case FormationArea.CentreBack:
                    score += player.Defence * 0.65f;
                    score += player.Pace * 0.10f;
                    score += player.Passing * 0.10f;
                    if (player.Role == PlayerRole.CentreBack)
                        score += 20f;
                    if (player.Role == PlayerRole.Sweeper ||
                        player.Role == PlayerRole.LineHolding)
                        score += 15f;
                    break;

                case FormationArea.LeftMidfield:
                case FormationArea.RightMidfield:
                    score += player.Pace * 0.25f;
                    score += player.Attack * 0.30f;
                    score += player.Passing * 0.25f;
                    if (player.Role == PlayerRole.Winger)
                        score += 25f;
                    if (player.Role == PlayerRole.BoxToBox)
                        score += 18f;
                    if (player.Role == PlayerRole.CentralMidfielder)
                        score += 12f;
                    if (player.Role == PlayerRole.FullBack)
                        score += 8f;
                    break;

                case FormationArea.CentreMidfield:
                    score += player.Passing * 0.40f;
                    score += player.Defence * 0.20f;
                    score += player.Attack * 0.20f;
                    if (player.Role == PlayerRole.CentralMidfielder)
                        score += 18f;
                    if (player.Role == PlayerRole.Playmaker)
                        score += 18f;
                    if (player.Role == PlayerRole.BoxToBox)
                        score += 16f;
                    if (player.Role == PlayerRole.DefensiveMidfielder)
                        score += 12f;
                    break;

                case FormationArea.DefensiveMidfield:
                    score += player.Defence * 0.40f;
                    score += player.Passing * 0.40f;
                    if (player.Role == PlayerRole.DefensiveMidfielder)
                        score += 25f;
                    if (player.Role == PlayerRole.CentralMidfielder)
                        score += 10f;
                    if (player.Role == PlayerRole.BoxToBox)
                        score += 8f;
                    break;

                case FormationArea.AttackingMidfield:
                    score += player.Attack * 0.40f;
                    score += player.Passing * 0.40f;
                    if (player.Role == PlayerRole.Playmaker)
                        score += 25f;
                    if (player.Role == PlayerRole.BoxToBox)
                        score += 12f;
                    break;

                case FormationArea.LeftWing:
                case FormationArea.RightWing:
                    score += player.Attack * 0.45f;
                    score += player.Pace * 0.30f;
                    if (player.Role == PlayerRole.Winger)
                        score += 25f;
                    break;

                case FormationArea.Striker:
                    score += player.Attack * 0.60f;
                    score += player.Pace * 0.20f;
                    if (player.Role == PlayerRole.Striker)
                        score += 25f;
                    break;
            }

            return score;
        }
    }
}