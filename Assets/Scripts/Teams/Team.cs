using System.Collections.Generic;
using System.Linq;

namespace FootballTactics.Teams
{
    public class Team
    {
        public string Name { get; }

        // Players currently on the pitch.
        public List<Player> Players { get; }

        // Players available from the bench.
        public List<Player> Bench { get; }

        // Players who have been substituted off.
        public List<Player> SubstitutedPlayers { get; }

        public Team(
            string name,
            List<Player> players,
            List<Player> bench)
        {
            Name = name;

            Players = players;
            Bench = bench;

            SubstitutedPlayers = new List<Player>();
        }

        public float AverageAttack =>
            (float)GetPlayersOfType(PlayerPosition.Attacker)
                .Select(p => p.Attack)
                .DefaultIfEmpty()
                .Average();

        public float AverageDefence =>
            (float)GetPlayersOfType(PlayerPosition.Defender)
                .Select(p => p.Defence)
                .DefaultIfEmpty()
                .Average();

        public float AverageMidfield =>
            GetPlayersOfType(PlayerPosition.Midfielder)
                .Select(p => (p.Passing + p.Defence) / 2f)
                .DefaultIfEmpty()
                .Average();

        public float AveragePace =>
            (float)Players
                .Select(p => p.Pace)
                .DefaultIfEmpty()
                .Average();

        public float AverageFitness =>
            (float)Players
                .Select(p => p.Fitness)
                .DefaultIfEmpty()
                .Average();

        public IEnumerable<Player> GetPlayersOfType(
            PlayerPosition position)
        {
            return Players.Where(
                p => p.Position == position);
        }

        public bool CanMakeSubstitution =>
            SubstitutedPlayers.Count < 3 &&
            Bench.Count > 0;

        public bool MakeSubstitution(
            string playerOnName,
            string playerOffName)
        {
            if (!CanMakeSubstitution)
                return false;

            Player playerOff =
                Players.FirstOrDefault(
                    p => p.Name == playerOffName);

            Player playerOn =
                Bench.FirstOrDefault(
                    p => p.Name == playerOnName);

            if (playerOff == null || playerOn == null)
                return false;

            // Basic position validation.
            if (playerOff.Position != playerOn.Position)
                return false;

            Players.Remove(playerOff);
            Bench.Remove(playerOn);

            Players.Add(playerOn);
            SubstitutedPlayers.Add(playerOff);

            return true;
        }

        public void ReduceFitness(int amount)
        {
            foreach (Player player in Players)
            {
                player.ReduceFitness(amount);
            }
        }

        public float GetRoleAttackImpact()
        {
            float total = 0f;

            foreach (Player player in Players)
            {
                total += player.Role switch
                {
                    PlayerRole.Striker => 1.25f,
                    PlayerRole.Winger => 1.15f,
                    PlayerRole.Playmaker => 1.10f,
                    PlayerRole.BoxToBox => 1.05f,

                    PlayerRole.CentralMidfielder => 1.00f,
                    PlayerRole.DefensiveMidfielder => 0.90f,

                    PlayerRole.CentreBack => 0.45f,
                    PlayerRole.FullBack => 0.65f,

                    PlayerRole.Sweeper => 0.50f,
                    PlayerRole.LineHolding => 0.40f,

                    PlayerRole.Goalkeeper => 0.05f,

                    _ => 1.00f
                };
            }

            return total / Players.Count;
        }

        public float GetRoleDefenceImpact()
        {
            float total = 0f;

            foreach (Player player in Players)
            {
                total += player.Role switch
                {
                    PlayerRole.CentreBack => 1.20f,
                    PlayerRole.LineHolding => 1.25f,
                    PlayerRole.Sweeper => 1.10f,
                    PlayerRole.DefensiveMidfielder => 1.10f,
                    PlayerRole.FullBack => 1.05f,

                    PlayerRole.CentralMidfielder => 0.90f,
                    PlayerRole.BoxToBox => 0.85f,

                    PlayerRole.Playmaker => 0.75f,
                    PlayerRole.Winger => 0.70f,
                    PlayerRole.Striker => 0.45f,

                    PlayerRole.Goalkeeper => 1.00f,

                    _ => 1.00f
                };
            }

            return total / Players.Count;
        }

        public float GetRolePossessionImpact()
        {
            float total = 0f;

            foreach (Player player in Players)
            {
                total += player.Role switch
                {
                    PlayerRole.Playmaker => 1.20f,
                    PlayerRole.CentralMidfielder => 1.10f,
                    PlayerRole.BoxToBox => 1.05f,
                    PlayerRole.DefensiveMidfielder => 1.10f,

                    PlayerRole.Winger => 0.95f,
                    PlayerRole.Striker => 0.90f,

                    PlayerRole.CentreBack => 0.90f,
                    PlayerRole.FullBack => 0.98f,
                    PlayerRole.Sweeper => 0.88f,
                    PlayerRole.LineHolding => 0.92f,

                    _ => 1.00f
                };
            }

            return total / Players.Count;
        }
    }
}