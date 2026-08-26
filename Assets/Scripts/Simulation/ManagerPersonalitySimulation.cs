using UnityEngine;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Dedicated simulation harness for validating manager personalities.
    /// The comparison is deliberately bounded so a context-menu test cannot
    /// accidentally monopolise the Unity editor for several minutes.
    /// </summary>
    public sealed class ManagerPersonalitySimulation : MonoBehaviour
    {
        [SerializeField]
        private int matchesPerPersonality = 1000;

        [SerializeField]
        private int maximumMatchesPerPersonality = 2000;

        [SerializeField]
        private int maximumMinutesPerMatch = 120;

        [ContextMenu("Run Manager Personality Comparison")]
        public void RunManagerPersonalityComparison()
        {
            if (matchesPerPersonality <= 0)
            {
                Debug.LogError("Manager simulation count must be greater than zero.");
                return;
            }

            if (maximumMatchesPerPersonality <= 0)
            {
                Debug.LogError("Maximum matches per personality must be greater than zero.");
                return;
            }

            if (maximumMinutesPerMatch < 90)
            {
                Debug.LogError("Maximum minutes per match must be at least 90.");
                return;
            }

            int matches = Mathf.Min(matchesPerPersonality, maximumMatchesPerPersonality);

            if (matches != matchesPerPersonality)
            {
                Debug.LogWarning(
                    $"Requested {matchesPerPersonality} matches per personality; " +
                    $"capping this run at {matches} to prevent an excessively long editor lock-up.");
            }

            Debug.Log(
                "========== MANAGER PERSONALITY COMPARISON ==========\n" +
                $"Matches per personality: {matches}");

            ManagerPersonality[] personalities =
            {
                ManagerPersonality.Balanced,
                ManagerPersonality.Possession,
                ManagerPersonality.Gegenpress,
                ManagerPersonality.CounterAttack,
                ManagerPersonality.Pragmatic,
                ManagerPersonality.Direct
            };

            foreach (ManagerPersonality personality in personalities)
                RunPersonality(personality, matches);
        }

        private void RunPersonality(ManagerPersonality personality, int matches)
        {
            SimulationResult result = new();

            int decisions = 0;
            int earlyDecisions = 0;
            int middleDecisions = 0;
            int lateDecisions = 0;
            int mentalityChanges = 0;
            int pressingChanges = 0;
            int defensiveLineChanges = 0;
            int formationChanges = 0;
            int abortedMatches = 0;

            for (int i = 0; i < matches; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();

                TacticalSettings homeTactics = new()
                {
                    Formation = Formation.FourThreeThree,
                    Mentality = Mentality.Balanced,
                    Pressing = Pressing.Medium,
                    DefensiveLine = DefensiveLine.Normal
                };

                TacticalSettings awayTactics = new()
                {
                    Formation = Formation.FourFourTwo,
                    Mentality = Mentality.Balanced,
                    Pressing = Pressing.Medium,
                    DefensiveLine = DefensiveLine.Normal
                };

                ManagerPersonalityController manager =
                    new(personality);

                manager.ApplyInitialTactics(homeTactics);

                MatchEngine engine = new(
                    homeTeam,
                    awayTeam,
                    homeTactics,
                    awayTactics);

                if (!RunMatch(engine, manager))
                {
                    abortedMatches++;
                    continue;
                }

                result.Record(
                    engine.State,
                    homeTeam.GetAverageFitness(engine.HomeLineup));

                decisions += manager.TotalDecisions;
                earlyDecisions += manager.EarlyDecisions;
                middleDecisions += manager.MiddleDecisions;
                lateDecisions += manager.LateDecisions;
                mentalityChanges += manager.MentalityChanges;
                pressingChanges += manager.PressingChanges;
                defensiveLineChanges += manager.DefensiveLineChanges;
                formationChanges += manager.FormationChanges;
            }

            Debug.Log(
                $"\n===== MANAGER: {personality} =====\n" +
                $"Matches:        {result.Matches}\n" +
                $"Wins:           {result.Wins}\n" +
                $"Draws:          {result.Draws}\n" +
                $"Losses:         {result.Losses}\n" +
                $"Aborted:        {abortedMatches}\n" +
                $"Win Rate:       {result.WinRate:F1}%\n" +
                $"Avg Goals:      {result.AverageGoals:F2}\n" +
                $"Avg Possession: {result.AveragePossession:F1}%\n" +
                $"Avg Shots:      {result.AverageShots:F2}\n" +
                $"Avg xG:          {result.AverageXG:F2}\n" +
                $"Avg Fitness:    {result.AverageFitness:F1}%\n" +
                $"Behaviour | Changes {Average(mentalityChanges + pressingChanges + defensiveLineChanges + formationChanges, matches):F2} | " +
                $"Mentality {Average(mentalityChanges, matches):F2} | " +
                $"Pressing {Average(pressingChanges, matches):F2} | " +
                $"Defensive Line {Average(defensiveLineChanges, matches):F2} | " +
                $"Formation {Average(formationChanges, matches):F2}\n" +
                $"Decisions | Total {Average(decisions, matches):F2} | " +
                $"0-30 {Average(earlyDecisions, matches):F2} | " +
                $"31-60 {Average(middleDecisions, matches):F2} | " +
                $"61-90 {Average(lateDecisions, matches):F2}");
        }

        private bool RunMatch(
            MatchEngine engine,
            ManagerPersonalityController manager)
        {
            int safetyCounter = 0;

            while (engine.State.Minute < 90)
            {
                if (safetyCounter++ >= maximumMinutesPerMatch)
                {
                    Debug.LogError(
                        $"Manager simulation aborted at minute {engine.State.Minute}: " +
                        "match exceeded the maximum simulation steps.");
                    return false;
                }

                int previousMinute = engine.State.Minute;

                engine.SimulateMinute();

                // The manager acts after the current minute's match events,
                // allowing the new tactic to influence the following minute.
                manager.Update(engine);

                if (engine.PendingSituation != null &&
                    !engine.AutoResolvePendingSituation())
                {
                    Debug.LogError(
                        $"Manager simulation aborted at minute {engine.State.Minute}: " +
                        "pending situation could not be resolved.");
                    return false;
                }

                // SimulateMinute() intentionally does nothing while a situation
                // is pending. The situation is resolved above, so the next loop
                // must advance. This guard catches any future engine regression.
                if (engine.State.Minute == previousMinute &&
                    engine.PendingSituation == null)
                {
                    Debug.LogError(
                        $"Manager simulation aborted at minute {engine.State.Minute}: " +
                        "match engine failed to advance.");
                    return false;
                }
            }

            return true;
        }

        private static float Average(int total, int matches)
        {
            return matches <= 0 ? 0f : total / (float)matches;
        }
    }
}