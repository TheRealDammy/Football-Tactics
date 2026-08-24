using System.Collections.Generic;
using UnityEngine;

namespace FootballTactics.Simulation
{
    public static class TacticalSituationGenerator
    {
        // Prevents the same match from generating another player decision immediately
        // after the previous one has been resolved. Without this cooldown the engine's
        // nextSituationMinute could remain in the past, causing decisions at 67, 68, 69...
        private static readonly Dictionary<MatchEngine, int> nextAllowedDecisionMinute =
            new Dictionary<MatchEngine, int>();

        public static TacticalSituation Generate(MatchEngine engine)
        {
            if (engine == null)
                return null;

            if (!DecisionOpportunityManager.IsDecisionWindow(engine))
                return null;

            int minute = engine.State.Minute;

            if (nextAllowedDecisionMinute.TryGetValue(engine, out int nextAllowed) &&
                minute < nextAllowed)
            {
                return null;
            }

            List<TacticalSituation> candidates = new();

            AddIfValid(candidates, TryOppositionPressing(engine));
            AddIfValid(candidates, TryTiredPlayer(engine));
            AddIfValid(candidates, TrySpaceBehindDefence(engine));
            AddIfValid(candidates, TryOpponentDeepBlock(engine));
            AddIfValid(candidates, TryProtectLead(engine));
            AddIfValid(candidates, TryPossessionBattle(engine));
            AddIfValid(candidates, TryWidthBattle(engine));

            if (candidates.Count == 0)
                return null;

            float weight = DecisionOpportunityManager.GetDecisionWeight(engine);

            TacticalSituation selected;

            if (weight > 1.15f && candidates.Count > 1)
            {
                selected = candidates[Random.Range(0, candidates.Count)];
            }
            else
            {
                selected = candidates[Random.Range(0, candidates.Count)];
            }

            // A decision should create breathing room before another decision.
            // 4–7 minutes means decisions can still happen several times in a match,
            // but never repeatedly in consecutive minutes.
            nextAllowedDecisionMinute[engine] = minute + Random.Range(4, 8);

            return selected;
        }

        private static void AddIfValid(List<TacticalSituation> candidates, TacticalSituation situation)
        {
            if (situation != null)
                candidates.Add(situation);
        }

        private static TacticalSituation TryPossessionBattle(MatchEngine engine)
        {
            float possession = engine.State.HomePossession;
            if (possession < 42f || possession > 58f) return null;

            return new TacticalSituation(
                TacticalSituationType.PossessionBattle,
                "MIDFIELD BATTLE",
                "The match is becoming a midfield contest. How do you want to approach it?",
                new List<TacticalSituationOption>
                {
                    new("control", "CONTROL", "Commit more players to circulation.", 1.07f, 0.96f, 0.95f, 0.92f),
                    new("direct", "PLAY DIRECT", "Look to move the ball forward quickly.", 0.94f, 1.08f, 1.02f, 1.06f),
                    new("balanced", "STAY BALANCED", "Keep the current approach.", 1.00f, 1.00f, 1.00f, 1.00f)
                });
        }

        private static TacticalSituation TryWidthBattle(MatchEngine engine)
        {
            if (engine.State.Minute > 65) return null;

            return new TacticalSituation(
                TacticalSituationType.WidthBattle,
                "SPACE OUT WIDE",
                "There is space developing in wide areas. How do you want to exploit it?",
                new List<TacticalSituationOption>
                {
                    new("use_wings", "USE THE WINGS", "Push attacks towards the wide areas.", 1.01f, 1.10f, 1.05f, 0.98f),
                    new("attack_middle", "ATTACK THROUGH THE MIDDLE", "Keep the attack central.", 0.99f, 1.06f, 1.04f, 1.00f),
                    new("retain_shape", "RETAIN SHAPE", "Don't force the attack.", 1.03f, 0.93f, 0.90f, 0.94f)
                });
        }

        private static TacticalSituation TryOppositionPressing(MatchEngine engine)
        {
            if (engine.AwayTactics.Pressing != Pressing.High) return null;

            return new TacticalSituation(
                TacticalSituationType.OppositionPressing,
                "OPPOSITION PRESSING",
                $"{engine.AwayTeam.Name} are pressing high and your midfield is struggling to progress the ball.",
                new List<TacticalSituationOption>
                {
                    new("play_through", "PLAY THROUGH", "Take risks to progress through the press.", 1.04f, 1.08f, 1.08f, 0.92f),
                    new("go_long", "GO LONG", "Bypass the midfield and attack space early.", 0.93f, 1.02f, 0.96f, 1.10f),
                    new("slow_tempo", "SLOW TEMPO", "Reduce the intensity and wait for space.", 1.02f, 0.92f, 0.82f, 0.95f)
                });
        }

        private static TacticalSituation TryTiredPlayer(MatchEngine engine)
        {
            float fitness = engine.HomeTeam.GetAverageFitness(engine.HomeLineup);
            if (fitness > 58f) return null;

            return new TacticalSituation(
                TacticalSituationType.TiredPlayer,
                "FATIGUE BUILDING",
                "Your team is beginning to tire. Your current approach is placing a heavy physical demand on the players.",
                new List<TacticalSituationOption>
                {
                    new("reduce_workload", "REDUCE WORKLOAD", "Lower the physical intensity.", 0.98f, 0.94f, 0.65f, 0.96f),
                    new("protect_possession", "PROTECT POSSESSION", "Keep the ball and reduce unnecessary running.", 1.04f, 0.90f, 0.72f, 0.94f),
                    new("keep_going", "KEEP GOING", "Accept the fatigue and maintain the approach.", 1.01f, 1.05f, 1.20f, 1.02f)
                });
        }

        private static TacticalSituation TrySpaceBehindDefence(MatchEngine engine)
        {
            if (engine.HomeTactics.DefensiveLine != DefensiveLine.High) return null;

            return new TacticalSituation(
                TacticalSituationType.SpaceBehindDefence,
                "SPACE BEHIND",
                $"{engine.AwayTeam.Name} are looking to exploit the space behind your high defensive line.",
                new List<TacticalSituationOption>
                {
                    new("hold_line", "HOLD THE LINE", "Trust the defensive structure.", 1.03f, 1.04f, 1.00f, 1.15f),
                    new("drop_deeper", "DROP DEEPER", "Give up territory to protect the space behind.", 0.94f, 0.90f, 0.94f, 0.70f),
                    new("press_harder", "PRESS HARDER", "Try to win the ball before they can counter.", 1.05f, 1.08f, 1.16f, 0.84f)
                });
        }

        private static TacticalSituation TryOpponentDeepBlock(MatchEngine engine)
        {
            if (engine.AwayTactics.Mentality != Mentality.Defensive) return null;
            if (engine.AwayTactics.DefensiveLine != DefensiveLine.Deep) return null;

            return new TacticalSituation(
                TacticalSituationType.OpponentDeepBlock,
                "DEFENSIVE BLOCK",
                $"{engine.AwayTeam.Name} have dropped deep and are denying space around the box.",
                new List<TacticalSituationOption>
                {
                    new("use_width", "USE WIDTH", "Stretch the defence and attack from wide areas.", 1.02f, 1.08f, 1.04f, 0.96f),
                    new("play_through", "PLAY THROUGH", "Try to combine through the middle.", 0.97f, 1.06f, 1.08f, 0.94f),
                    new("patient_attack", "BE PATIENT", "Keep possession and wait for an opening.", 1.06f, 0.96f, 0.82f, 0.92f)
                });
        }

        private static TacticalSituation TryProtectLead(MatchEngine engine)
        {
            if (engine.State.Minute < 70) return null;
            if (engine.State.HomeGoals <= engine.State.AwayGoals) return null;
            if (Random.value > 0.45f) return null;

            return new TacticalSituation(
                TacticalSituationType.ProtectLead,
                "PROTECT THE LEAD",
                $"You are leading {engine.State.HomeGoals}-{engine.State.AwayGoals}. {engine.AwayTeam.Name} are beginning to commit players forward.",
                new List<TacticalSituationOption>
                {
                    new("protect", "PROTECT", "Reduce risk and defend the advantage.", 0.96f, 0.84f, 0.72f, 0.82f),
                    new("counter", "COUNTER", "Allow them forward and attack the space.", 0.98f, 1.10f, 1.02f, 1.18f),
                    new("keep_attacking", "KEEP ATTACKING", "Continue trying to extend the lead.", 1.02f, 1.08f, 1.10f, 1.06f)
                });
        }
    }
}