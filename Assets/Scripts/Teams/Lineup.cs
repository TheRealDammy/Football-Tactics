using System.Collections.Generic;
using FootballTactics.Simulation;

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
    }
}