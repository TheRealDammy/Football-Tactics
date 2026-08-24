using FootballTactics.Simulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public List<Player> GetFullSquad()
        {
            return Players
                .Concat(Bench)
                .Concat(SubstitutedPlayers)
                .Distinct()
                .ToList();
        }

        public bool ApplyStartingLineup(Lineup lineup)
        {
            List<Player> squad = GetFullSquad();

            List<Player> starters = lineup.Assignments.Values
                .Distinct()
                .ToList();

            if (starters.Count != 11)
                return false;

            Players.Clear();
            Players.AddRange(starters);

            Bench.Clear();

            foreach (Player player in squad)
            {
                if (starters.Contains(player))
                    continue;

                if (Bench.Count >= 6)
                    break;

                Bench.Add(player);
            }

            SubstitutedPlayers.Clear();
            return true;
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
            (float)Players.Select(p => p.Pace)
                .DefaultIfEmpty()
                .Average();

        public float AverageFitness =>
            (float)Players.Select(p => p.Fitness)
                .DefaultIfEmpty()
                .Average();

        public IEnumerable<Player> GetPlayersOfType(
            PlayerPosition position)
        {
            return Players.Where(p => p.Position == position);
        }

        public bool CanMakeSubstitution =>
            SubstitutedPlayers.Count < 5 &&
            Bench.Count > 0;

        public bool MakeSubstitution(
            string playerOnName,
            string playerOffName)
        {
            if (!CanMakeSubstitution)
                return false;

            Player playerOff = Players.FirstOrDefault(
                p => p.Name == playerOffName);

            Player playerOn = Bench.FirstOrDefault(
                p => p.Name == playerOnName);

            if (playerOff == null || playerOn == null)
                return false;

            // Never mutate the squad for an obviously invalid positional
            // substitution. The formation-specific slot check is performed
            // by MatchEngine before the UI offers the player.
            if (playerOff.Position != playerOn.Position)
                return false;

            Players.Remove(playerOff);
            Bench.Remove(playerOn);

            Players.Add(playerOn);
            SubstitutedPlayers.Add(playerOff);

            return true;
        }

        public void ReduceFitness(
            int baseAmount,
            Lineup lineup,
            Pressing pressing)
        {
            foreach (Player player in GetStartingPlayers(lineup))
            {
                float roleCost = RoleBehaviour.FitnessCost(player.Role);

                float pressingMultiplier = pressing switch
                {
                    Pressing.Low => 0.65f,
                    Pressing.Medium => 1.0f,
                    Pressing.High => 1.40f,
                    _ => 1.0f
                };

                int amount = Mathf.CeilToInt(
                    baseAmount * roleCost * pressingMultiplier);

                player.ReduceFitness(amount);
            }
        }

        public IEnumerable<Player> GetStartingPlayers(Lineup lineup)
        {
            return lineup.Assignments.Values.Distinct();
        }

        public float GetAverageAttack(Lineup lineup)
        {
            return (float)GetStartingPlayers(lineup)
                .Select(p => p.Attack)
                .DefaultIfEmpty()
                .Average();
        }

        public float GetAverageDefence(Lineup lineup)
        {
            return (float)GetStartingPlayers(lineup)
                .Select(p => p.Defence)
                .DefaultIfEmpty()
                .Average();
        }

        public float GetAverageMidfield(Lineup lineup)
        {
            return GetStartingPlayers(lineup)
                .Where(p => p.Position == PlayerPosition.Midfielder)
                .Select(p => (p.Passing + p.Defence) / 2f)
                .DefaultIfEmpty()
                .Average();
        }

        public float GetAveragePace(Lineup lineup)
        {
            return (float)GetStartingPlayers(lineup)
                .Select(p => p.Pace)
                .DefaultIfEmpty()
                .Average();
        }

        public float GetAverageFitness(Lineup lineup)
        {
            return (float)GetStartingPlayers(lineup)
                .Select(p => p.Fitness)
                .DefaultIfEmpty()
                .Average();
        }

        public float GetRoleAttackImpact(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();

            if (players.Count == 0)
                return 1f;

            float total = 0f;

            foreach (Player player in players)
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

            return total / players.Count;
        }

        public float GetRoleDefenceImpact(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();

            if (players.Count == 0)
                return 1f;

            float total = 0f;

            foreach (Player player in players)
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

            return total / players.Count;
        }

        public float GetRolePossessionImpact(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();

            if (players.Count == 0)
                return 1f;

            float total = 0f;

            foreach (Player player in players)
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

            return total / players.Count;
        }

        public float GetBuildUpContribution(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();
            if (players.Count == 0)
                return 0f;

            return players.Sum(p => RoleBehaviour.BuildUpContribution(p.Role));
        }

        public float GetChanceCreationContribution(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();
            if (players.Count == 0)
                return 0f;

            return players.Sum(p => RoleBehaviour.ChanceCreationContribution(p.Role));
        }

        public float GetPressingContribution(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();
            if (players.Count == 0)
                return 0f;

            return players.Sum(p => RoleBehaviour.PressingContribution(p.Role));
        }

        public float GetDefensiveContribution(Lineup lineup)
        {
            var players = GetStartingPlayers(lineup).ToList();
            if (players.Count == 0)
                return 0f;

            return players.Sum(p => RoleBehaviour.DefensiveContribution(p.Role));
        }

        public Player SelectAttackingPlayer(Lineup lineup)
        {
            Player bestPlayer = null;
            float bestScore = float.MinValue;

            foreach (Player player in GetStartingPlayers(lineup))
            {
                float roleWeight = player.Role switch
                {
                    PlayerRole.Striker => 1.30f,
                    PlayerRole.Winger => 1.20f,
                    PlayerRole.Playmaker => 1.05f,
                    PlayerRole.BoxToBox => 0.90f,
                    PlayerRole.CentralMidfielder => 0.80f,
                    PlayerRole.DefensiveMidfielder => 0.55f,
                    PlayerRole.FullBack => 0.45f,
                    PlayerRole.CentreBack => 0.25f,
                    PlayerRole.Sweeper => 0.20f,
                    PlayerRole.LineHolding => 0.15f,
                    _ => 0.50f
                };

                float score = PlayerPerformance.GetAttackRating(player) *
                              roleWeight *
                              UnityEngine.Random.Range(0.85f, 1.15f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlayer = player;
                }
            }

            return bestPlayer;
        }

        public Player SelectDefendingPlayer(Lineup lineup)
        {
            Player bestPlayer = null;
            float bestScore = float.MinValue;

            foreach (Player player in GetStartingPlayers(lineup))
            {
                float roleWeight = player.Role switch
                {
                    PlayerRole.CentreBack => 1.25f,
                    PlayerRole.LineHolding => 1.25f,
                    PlayerRole.Sweeper => 1.15f,
                    PlayerRole.DefensiveMidfielder => 1.10f,
                    PlayerRole.FullBack => 1.05f,
                    PlayerRole.BoxToBox => 0.85f,
                    PlayerRole.CentralMidfielder => 0.80f,
                    PlayerRole.Winger => 0.55f,
                    PlayerRole.Playmaker => 0.50f,
                    PlayerRole.Striker => 0.30f,
                    _ => 0.50f
                };

                float score = PlayerPerformance.GetDefenceRating(player) *
                              roleWeight *
                              UnityEngine.Random.Range(0.85f, 1.15f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlayer = player;
                }
            }

            return bestPlayer;
        }
    }
}