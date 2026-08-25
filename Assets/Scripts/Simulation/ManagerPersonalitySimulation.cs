using UnityEngine;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Dedicated simulation harness for validating manager personalities.
    /// It keeps the manager system separate from the normal formation tests.
    /// </summary>
    public sealed class ManagerPersonalitySimulation : MonoBehaviour
    {
        [SerializeField]
        private int matchesPerPersonality = 10000;

        [ContextMenu("Run Manager Personality Comparison")]
        public void RunManagerPersonalityComparison()
        {
            if (matchesPerPersonality <= 0)
            {
                Debug.LogError("Manager simulation count must be greater than zero.");
                return;
            }

            Debug.Log("========== MANAGER PERSONALITY COMPARISON ==========");

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
                RunPersonality(personality);
        }

        private void RunPersonality(ManagerPersonality personality)
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

            for (int i = 0; i < matchesPerPersonality; i++)
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

                RunMatch(engine, manager);

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
                $"Win Rate:       {result.WinRate:F1}%\n" +
                $"Avg Goals:      {result.AverageGoals:F2}\n" +
                $"Avg Possession: {result.AveragePossession:F1}%\n" +
                $"Avg Shots:      {result.AverageShots:F2}\n" +
                $"Avg xG:          {result.AverageXG:F2}\n" +
                $"Avg Fitness:    {result.AverageFitness:F1}%\n" +
                $"Behaviour | Changes {Average(mentalityChanges + pressingChanges + defensiveLineChanges + formationChanges):F2} | " +
                $"Mentality {Average(mentalityChanges):F2} | " +
                $"Pressing {Average(pressingChanges):F2} | " +
                $"Defensive Line {Average(defensiveLineChanges):F2} | " +
                $"Formation {Average(formationChanges):F2}\n" +
                $"Decisions | Total {Average(decisions):F2} | " +
                $"0-30 {Average(earlyDecisions):F2} | " +
                $"31-60 {Average(middleDecisions):F2} | " +
                $"61-90 {Average(lateDecisions):F2}");
        }

        private static void RunMatch(
            MatchEngine engine,
            ManagerPersonalityController manager)
        {
            int safetyCounter = 0;

            while (engine.State.Minute < 90)
            {
                engine.SimulateMinute();

                // The manager acts after the current minute's match events,
                // allowing the new tactic to influence the following minute.
                manager.Update(engine);

                if (engine.PendingSituation != null)
                    engine.AutoResolvePendingSituation();

                safetyCounter++;

                if (safetyCounter > 200)
                {
                    Debug.LogError("Manager simulation aborted: match failed to reach full time.");
                    return;
                }
            }
        }

        private float Average(int total)
        {
            return total / (float)matchesPerPersonality;
        }
    }
}