using FootballTactics.Simulation;
using System.Collections.Generic;
using System.Linq;

namespace FootballTactics.Teams
{
    public sealed class Lineup
    {
        private readonly Dictionary<string, Player> assignments = new();

        public Formation Formation { get; }

        public IReadOnlyDictionary<string, Player> Assignments =>
            assignments;

        public Lineup(Formation formation)
        {
            Formation = formation;
        }

        public void Assign(
            FormationSlot slot,
            Player player)
        {
            assignments[slot.Id] = player;
        }

        public bool IsComplete => assignments.Count == 11;

        public bool HasDuplicatePlayers()
        {
            return assignments.Values
                .GroupBy(player => player)
                .Any(group => group.Count() > 1);
        }

        public bool HasPlayer(Player player)
        {
            foreach (Player assignedPlayer in assignments.Values)
            {
                if (assignedPlayer == player)
                    return true;
            }

            return false;
        }

        public Player GetPlayer(string slotId)
        {
            return assignments.TryGetValue(
                slotId,
                out Player player)
                ? player
                : null;
        }

        public bool ReplacePlayer(Player playerOff, Player playerOn)
        {
            string slotId = null;

            foreach (var assignment in assignments)
            {
                if (assignment.Value == playerOff)
                {
                    slotId = assignment.Key;
                    break;
                }
            }

            if (slotId == null)
                return false;

            assignments[slotId] = playerOn;

            return true;
        }
    }
}