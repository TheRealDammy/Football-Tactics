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

            // Build a varied match rhythm. We deliberately guarantee an
            // early/mid-game opportunity and only allow one or two late ones.
            int count = Random.Range(3, 6); // 3-5 decisions per match.

            List<int> candidates = new()
            {
                Random.Range(12, 20), // early
                Random.Range(23, 32), // first main phase
                Random.Range(35, 44), // end of first half
                Random.Range(50, 59), // start of second half
                Random.Range(63, 73), // main tactical/substitution phase
                Random.Range(77, 87)  // late game
            };

            Shuffle(candidates);

            // Always include at least one opportunity before 60'.
            int earlyCount = Random.Range(2, 4);
            for (int i = 0; i < earlyCount && plannedMinutes.Count < count; i++)
            {
                int index = i % 4;
                plannedMinutes.Enqueue(candidates[index]);
            }

            // Fill remaining slots, but cap late-game decisions at two.
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

            // Prevent decisions from stacking even if the match advances
            // through several planned windows quickly.
            cooldownUntil = minute + Random.Range(5, 9);

            return true;
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