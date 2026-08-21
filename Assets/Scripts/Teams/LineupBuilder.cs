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
            FormationDefinition definition = formation.GetDefinition();
            Lineup lineup = new Lineup(formation);
            HashSet<Player> usedPlayers = new();
            IReadOnlyList<Player> squad = team.GetFullSquad();

            foreach (FormationSlot slot in definition.Slots.OrderBy(GetSlotPriority))
            {
                Player bestPlayer = FindBestPlayerForSlot(
                    squad, slot, usedPlayers);

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

        public static bool CanPlayerPlaySlot(
            Player player,
            FormationSlot slot)
        {
            if (player == null || slot == null)
                return false;

            // 4-4-2 uses LS/RS for the front two. The FormationArea names
            // are shared with 4-3-3, so the slot id disambiguates them.
            if ((slot.Id == "LS" || slot.Id == "RS") &&
                (slot.Area == FormationArea.LeftWing ||
                 slot.Area == FormationArea.RightWing))
            {
                return player.Position == PlayerPosition.Attacker &&
                       IsAny(player.Role,
                           PlayerRole.Striker,
                           PlayerRole.Winger);
            }

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
                    (player.Position == PlayerPosition.Midfielder &&
                     IsAny(player.Role,
                         PlayerRole.CentralMidfielder,
                         PlayerRole.Playmaker,
                         PlayerRole.DefensiveMidfielder,
                         PlayerRole.BoxToBox))
                    ||
                    (player.Position == PlayerPosition.Attacker &&
                     player.Role == PlayerRole.Winger),

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
            // Prioritise the two central midfield slots before wide midfield
            // so a 4-4-2 with three natural midfielders can still use a
            // winger as the fourth midfielder. This prevents LM/RM becoming
            // empty after the CMs consume every midfielder.
            return slot.Id switch
            {
                "GK" => 0,
                "LCB" => 1,
                "RCB" => 1,
                "LB" => 2,
                "RB" => 2,

                "LDM" => 3,
                "RDM" => 3,
                "LCM" => 3,
                "CM" => 3,
                "RCM" => 3,

                "LM" => 4,
                "RM" => 4,

                "CAM" => 5,
                "LW" => 6,
                "RW" => 6,
                "ST" => 6,
                "LS" => 6,
                "RS" => 6,

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
                    if (player.Position == PlayerPosition.Midfielder)
                        score += 20f;
                    if (player.Role == PlayerRole.Winger)
                        score += 15f;
                    if (player.Role == PlayerRole.BoxToBox)
                        score += 18f;
                    if (player.Role == PlayerRole.CentralMidfielder)
                        score += 12f;
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
                    if (player.Role == PlayerRole.Striker)
                        score += 25f;
                    if (player.Role == PlayerRole.Winger)
                        score += 20f;
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