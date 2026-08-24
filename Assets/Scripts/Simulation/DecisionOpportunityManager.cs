using System.Collections.Generic;
using UnityEngine;

namespace FootballTactics.Simulation
{
    /// <summary>
    /// Controls the rhythm of player-facing tactical decisions during a match.
    /// Decisions are deliberately spread across the match instead of being
    /// concentrated in the final 20 minutes.
    /// </summary>
    public class DecisionOpportunityManager
    {
        private readonly Queue<int> plannedMinutes = new();
        private int cooldownUntil;

        public DecisionOpportunityManager()
        {
            GeneratePlan();
        }

        private void GeneratePlan()
        {
            plannedMinutes.Clear();

            int count = Random.Range(3, 6);

            List<int> candidates = new()
            {
                Random.Range(12, 20),
                Random.Range(23, 32),
                Random.Range(35, 44),
                Random.Range(50, 59),
                Random.Range(63, 73),
                Random.Range(77, 87)
            };

            Shuffle(candidates);

            int earlyCount = Random.Range(2, 4);
            for (int i = 0; i < earlyCount && plannedMinutes.Count < count; i++)
            {
                int index = i % 4;
                plannedMinutes.Enqueue(candidates[index]);
            }

            List<int> remaining = new(candidates);
            remaining.RemoveAll(m => plannedMinutes.Contains(m));

            Shuffle(remaining);

            int lateAdded = 0;
            foreach (int minute in remaining)
            {
                if (plannedMinutes.Count >= count)
                    break;

                if (minute >= 75)
                {
                    if (lateAdded >= 1)
                        continue;

                    lateAdded++;
                }

                plannedMinutes.Enqueue(minute);
            }

            List<int> sorted = new(plannedMinutes);
            sorted.Sort();
            plannedMinutes.Clear();

            foreach (int minute in sorted)
                plannedMinutes.Enqueue(minute);
        }

        public bool ShouldOfferDecision(MatchEngine engine)
        {
            if (engine == null || engine.State == null)
                return false;

            int minute = engine.State.Minute;

            if (minute < cooldownUntil)
                return false;

            if (plannedMinutes.Count == 0)
                return false;

            if (minute < plannedMinutes.Peek())
                return false;

            plannedMinutes.Dequeue();
            cooldownUntil = minute + Random.Range(5, 9);

            return true;
        }

        // Compatibility helpers used by TacticalSituationGenerator.
        // The actual scheduling decision is still controlled by the instance
        // manager above; these methods only prevent older generator code from
        // bypassing the new rhythm system at compile/runtime.
        public static bool IsDecisionWindow(MatchEngine engine)
        {
            if (engine == null || engine.State == null)
                return false;

            return engine.State.Minute >= 8 && engine.State.Minute < 90;
        }

        public static float GetDecisionWeight(MatchEngine engine)
        {
            if (engine == null || engine.State == null)
                return 1f;

            int minute = engine.State.Minute;

            if (minute < 20)
                return 0.90f;

            if (minute < 40)
                return 1.00f;

            if (minute < 60)
                return 1.05f;

            if (minute < 75)
                return 1.10f;

            return 1.05f;
        }

        private static void Shuffle(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}