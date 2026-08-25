using System;
using UnityEngine;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public class SimulationLab : MonoBehaviour
    {
        [SerializeField] private int matchesPerTest = 1000;
        [SerializeField] private bool runOnStart = false;

        private void Start()
        {
            if (runOnStart) RunAllBatchTests();
        }

        [ContextMenu("Run ALL Simulation Tests (Batch)")]
        public void RunAllBatchTests()
        {
            if (!ValidateCount()) return;
            float startTime = Time.realtimeSinceStartup;

            Debug.Log("\n========================================\n===== SIMULATION LAB - FULL BATCH =====\n========================================\n" +
                      $"Matches per test: {matchesPerTest}\n");

            RunFormationComparison();
            RunMentalityComparison();
            RunPressingComparison();
            RunDefensiveLineComparison();
            RunSquadFormationMatrix();
            RunManagerPersonalityComparison();

            Debug.Log("\n========================================\n===== SIMULATION LAB BATCH COMPLETE ====\n========================================\n" +
                      $"Elapsed: {Time.realtimeSinceStartup - startTime:F1}s\n");
        }

        [ContextMenu("Run Formation Comparison")]
        public void RunFormationComparison()
        {
            if (!ValidateCount()) return;
            Debug.Log("\n========== FORMATION COMPARISON ==========");
            RunFormationTest(Formation.FourFourTwo);
            RunFormationTest(Formation.FourThreeThree);
            RunFormationTest(Formation.FourTwoThreeOne);
        }

        private void RunFormationTest(Formation formation)
        {
            SimulationResult result = new();
            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();
                TacticalSettings homeTactics = new() { Formation = formation, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                RunMatchToFullTime(engine);
                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }
            PrintResult(formation, result);
        }

        [ContextMenu("Run Mentality Comparison")]
        public void RunMentalityComparison()
        {
            if (!ValidateCount()) return;
            Debug.Log("\n========== MENTALITY COMPARISON ==========");
            RunTacticalTest(Mentality.Defensive);
            RunTacticalTest(Mentality.Balanced);
            RunTacticalTest(Mentality.Attacking);
        }

        private void RunTacticalTest(Mentality mentality)
        {
            SimulationResult result = new();
            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();
                TacticalSettings homeTactics = new() { Formation = Formation.FourThreeThree, Mentality = mentality, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                RunMatchToFullTime(engine);
                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }
            PrintTacticalResult($"MENTALITY: {mentality}", result);
        }

        [ContextMenu("Run Pressing Comparison")]
        public void RunPressingComparison()
        {
            if (!ValidateCount()) return;
            Debug.Log("\n========== PRESSING COMPARISON ==========");
            RunPressingTest(Pressing.Low);
            RunPressingTest(Pressing.Medium);
            RunPressingTest(Pressing.High);
        }

        private void RunPressingTest(Pressing pressing)
        {
            SimulationResult result = new();
            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();
                TacticalSettings homeTactics = new() { Formation = Formation.FourThreeThree, Mentality = Mentality.Balanced, Pressing = pressing, DefensiveLine = DefensiveLine.Normal };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                RunMatchToFullTime(engine);
                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }
            PrintTacticalResult($"PRESSING: {pressing}", result);
        }

        [ContextMenu("Run Defensive Line Comparison")]
        public void RunDefensiveLineComparison()
        {
            if (!ValidateCount()) return;
            Debug.Log("\n========== DEFENSIVE LINE COMPARISON ==========");
            RunDefensiveLineTest(DefensiveLine.Deep);
            RunDefensiveLineTest(DefensiveLine.Normal);
            RunDefensiveLineTest(DefensiveLine.High);
        }

        private void RunDefensiveLineTest(DefensiveLine defensiveLine)
        {
            SimulationResult result = new();
            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();
                TacticalSettings homeTactics = new() { Formation = Formation.FourThreeThree, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = defensiveLine };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                RunMatchToFullTime(engine);
                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }
            PrintTacticalResult($"DEFENSIVE LINE: {defensiveLine}", result);
        }

        [ContextMenu("Run Squad Formation Matrix")]
        public void RunSquadFormationMatrix()
        {
            if (!ValidateCount()) return;
            SquadArchetype[] squads = { SquadArchetype.Possession, SquadArchetype.WideAttack, SquadArchetype.Direct };
            Formation[] formations = { Formation.FourFourTwo, Formation.FourThreeThree, Formation.FourTwoThreeOne };
            foreach (SquadArchetype squad in squads)
            {
                Debug.Log($"\n========== {squad.DisplayName()} ==========");
                foreach (Formation formation in formations)
                {
                    FormationMatrixResult result = RunMatrixTest(squad, formation);
                    Debug.Log($"{FormatFormation(formation)} | Win {result.WinRate:F1}% | Goals {result.AverageGoals:F2} | Poss {result.AveragePossession:F1}% | Shots {result.AverageShots:F2} | xG {result.AverageXG:F2}");
                }
            }
        }

        private FormationMatrixResult RunMatrixTest(SquadArchetype squad, Formation formation)
        {
            SimulationResult result = new();
            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TestSquadFactory.Create(squad, $"{squad} Home");
                Team awayTeam = TeamFactory.CreateHomeTeam();
                TacticalSettings homeTactics = new() { Formation = formation, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                RunMatchToFullTime(engine);
                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }
            return new FormationMatrixResult(squad, formation, result);
        }

        [ContextMenu("Run Manager Personality Comparison")]
        public void RunManagerPersonalityComparison()
        {
            if (!ValidateCount()) return;
            Debug.Log("\n========== MANAGER PERSONALITY COMPARISON ==========");
            foreach (ManagerPersonality personality in Enum.GetValues(typeof(ManagerPersonality)))
                RunManagerPersonalityTest(personality);
        }

        private void RunManagerPersonalityTest(ManagerPersonality personality)
        {
            SimulationResult result = new();
            int tacticalChanges = 0;
            int mentalityChanges = 0;
            int pressingChanges = 0;
            int defensiveLineChanges = 0;
            int formationChanges = 0;
            int decisions = 0;
            int earlyDecisions = 0;
            int midDecisions = 0;
            int lateDecisions = 0;

            for (int i = 0; i < matchesPerTest; i++)
            {
                Team homeTeam = TeamFactory.CreateHomeTeam();
                Team awayTeam = TeamFactory.CreateAwayTeam();
                TacticalSettings homeTactics = new() { Formation = Formation.FourThreeThree, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                TacticalSettings awayTactics = new() { Formation = Formation.FourFourTwo, Mentality = Mentality.Balanced, Pressing = Pressing.Medium, DefensiveLine = DefensiveLine.Normal };
                MatchEngine engine = new(homeTeam, awayTeam, homeTactics, awayTactics);
                ManagerPersonalityController manager = new(personality);
                manager.ApplyInitialTactics(engine);
                RunMatchToFullTime(engine, manager);

                tacticalChanges += manager.TotalTacticalChanges;
                mentalityChanges += manager.MentalityChanges;
                pressingChanges += manager.PressingChanges;
                defensiveLineChanges += manager.DefensiveLineChanges;
                formationChanges += manager.FormationChanges;
                decisions += manager.Decisions;
                earlyDecisions += manager.EarlyDecisions;
                midDecisions += manager.MidDecisions;
                lateDecisions += manager.LateDecisions;

                result.Record(engine.State, homeTeam.GetAverageFitness(engine.HomeLineup));
            }

            float divisor = matchesPerTest;
            PrintTacticalResult($"MANAGER: {personality}", result);
            Debug.Log(
                $"Behaviour | Changes {tacticalChanges / divisor:F2} | " +
                $"Mentality {mentalityChanges / divisor:F2} | " +
                $"Pressing {pressingChanges / divisor:F2} | " +
                $"Defensive Line {defensiveLineChanges / divisor:F2} | " +
                $"Formation {formationChanges / divisor:F2}\n" +
                $"Decisions | Total {decisions / divisor:F2} | " +
                $"0-30 {earlyDecisions / divisor:F2} | " +
                $"31-60 {midDecisions / divisor:F2} | " +
                $"61-90 {lateDecisions / divisor:F2}");
        }

        private void RunMatchToFullTime(MatchEngine engine, ManagerPersonalityController manager = null)
        {
            int safetyCounter = 0;
            while (engine.State.Minute < 90)
            {
                engine.SimulateMinute();
                if (manager != null) manager.Update(engine);
                if (engine.PendingSituation != null) engine.AutoResolvePendingSituation();
                safetyCounter++;
                if (safetyCounter > 200)
                {
                    Debug.LogError("Simulation aborted: match failed to reach full time.");
                    return;
                }
            }
        }

        private bool ValidateCount()
        {
            if (matchesPerTest > 0) return true;
            Debug.LogError("Simulation count must be greater than zero.");
            return false;
        }

        private void PrintResult(Formation formation, SimulationResult result)
        {
            Debug.Log("\n===== " + FormatFormation(formation) + " =====\n" +
                      $"Matches:        {result.Matches}\nWins:           {result.Wins}\nDraws:          {result.Draws}\nLosses:         {result.Losses}\nWin Rate:       {result.WinRate:F1}%\nAvg Goals:      {result.AverageGoals:F2}\nAvg Possession: {result.AveragePossession:F1}%\nAvg Shots:      {result.AverageShots:F2}\nAvg xG:          {result.AverageXG:F2}\nAvg Fitness:    {result.AverageFitness:F1}%");
        }

        private void PrintTacticalResult(string title, SimulationResult result)
        {
            Debug.Log("\n===== " + title + " =====\n" +
                      $"Matches:        {result.Matches}\nWins:           {result.Wins}\nDraws:          {result.Draws}\nLosses:         {result.Losses}\nWin Rate:       {result.WinRate:F1}%\nAvg Goals:      {result.AverageGoals:F2}\nAvg Possession: {result.AveragePossession:F1}%\nAvg Shots:      {result.AverageShots:F2}\nAvg xG:          {result.AverageXG:F2}\nAvg Fitness:    {result.AverageFitness:F1}%");
        }

        private static string FormatFormation(Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => "4-4-2",
                Formation.FourTwoThreeOne => "4-2-3-1",
                _ => "4-3-3"
            };
        }
    }
}
