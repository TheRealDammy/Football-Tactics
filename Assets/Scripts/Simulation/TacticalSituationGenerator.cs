using System.Collections.Generic;

namespace FootballTactics.Simulation
{
    public static class TacticalSituationGenerator
    {
        public static TacticalSituation Generate(
            MatchEngine engine)
        {
            TacticalSituation situation;

            situation =
                TryOppositionPressing(engine);

            if (situation != null)
                return situation;

            situation =
                TryTiredPlayer(engine);

            if (situation != null)
                return situation;

            situation =
                TrySpaceBehindDefence(engine);

            if (situation != null)
                return situation;

            situation =
                TryOpponentDeepBlock(engine);

            if (situation != null)
                return situation;

            situation =
                TryProtectLead(engine);

            return situation;
        }

        private static TacticalSituation TryOppositionPressing(
            MatchEngine engine)
        {
            if (engine.AwayTactics.Pressing != Pressing.High)
                return null;

            return new TacticalSituation(
                TacticalSituationType.OppositionPressing,

                "OPPOSITION PRESSING",

                $"{engine.AwayTeam.Name} are pressing " +
                "high and your midfield is struggling " +
                "to progress the ball.",

                new List<TacticalSituationOption>
                {
                    new(
                        "play_through",
                        "PLAY THROUGH",
                        "Take risks to progress through the press.",
                        1.04f,
                        1.08f,
                        1.08f,
                        0.92f),

                    new(
                        "go_long",
                        "GO LONG",
                        "Bypass the midfield and attack space early.",
                        0.93f,
                        1.02f,
                        0.96f,
                        1.10f),

                    new(
                        "slow_tempo",
                        "SLOW TEMPO",
                        "Reduce the intensity and wait for space.",
                        1.02f,
                        0.92f,
                        0.82f,
                        0.95f)
                });
        }

        private static TacticalSituation TryTiredPlayer(
            MatchEngine engine)
        {
            float fitness =
                engine.HomeTeam.GetAverageFitness(
                    engine.HomeLineup);

            if (fitness > 58f)
                return null;

            return new TacticalSituation(
                TacticalSituationType.TiredPlayer,

                "FATIGUE BUILDING",

                "Your team is beginning to tire. " +
                "Your current approach is placing " +
                "a heavy physical demand on the players.",

                new List<TacticalSituationOption>
                {
                    new(
                        "reduce_workload",
                        "REDUCE WORKLOAD",
                        "Lower the physical intensity.",
                        0.98f,
                        0.94f,
                        0.65f,
                        0.96f),

                    new(
                        "protect_possession",
                        "PROTECT POSSESSION",
                        "Keep the ball and reduce unnecessary running.",
                        1.04f,
                        0.90f,
                        0.72f,
                        0.94f),

                    new(
                        "keep_going",
                        "KEEP GOING",
                        "Accept the fatigue and maintain the approach.",
                        1.01f,
                        1.05f,
                        1.20f,
                        1.02f)
                });
        }

        private static TacticalSituation TrySpaceBehindDefence(
            MatchEngine engine)
        {
            if (engine.HomeTactics.DefensiveLine !=
                DefensiveLine.High)
            {
                return null;
            }

            return new TacticalSituation(
                TacticalSituationType.SpaceBehindDefence,

                "SPACE BEHIND",

                $"{engine.AwayTeam.Name} are looking " +
                "to exploit the space behind your " +
                "high defensive line.",

                new List<TacticalSituationOption>
                {
                    new(
                        "hold_line",
                        "HOLD THE LINE",
                        "Trust the defensive structure.",
                        1.03f,
                        1.04f,
                        1.00f,
                        1.15f),

                    new(
                        "drop_deeper",
                        "DROP DEEPER",
                        "Give up territory to protect the space behind.",
                        0.94f,
                        0.90f,
                        0.94f,
                        0.70f),

                    new(
                        "press_harder",
                        "PRESS HARDER",
                        "Try to win the ball before they can counter.",
                        1.05f,
                        1.08f,
                        1.16f,
                        0.84f)
                });
        }

        private static TacticalSituation TryOpponentDeepBlock(
            MatchEngine engine)
        {
            if (engine.AwayTactics.Mentality !=
                Mentality.Defensive)
            {
                return null;
            }

            if (engine.AwayTactics.DefensiveLine !=
                DefensiveLine.Deep)
            {
                return null;
            }

            return new TacticalSituation(
                TacticalSituationType.OpponentDeepBlock,

                "DEFENSIVE BLOCK",

                $"{engine.AwayTeam.Name} have dropped deep " +
                "and are denying space around the box.",

                new List<TacticalSituationOption>
                {
                    new(
                        "use_width",
                        "USE WIDTH",
                        "Stretch the defence and attack from wide areas.",
                        1.02f,
                        1.08f,
                        1.04f,
                        0.96f),

                    new(
                        "play_through",
                        "PLAY THROUGH",
                        "Try to combine through the middle.",
                        0.97f,
                        1.06f,
                        1.08f,
                        0.94f),

                    new(
                        "patient_attack",
                        "BE PATIENT",
                        "Keep possession and wait for an opening.",
                        1.06f,
                        0.96f,
                        0.82f,
                        0.92f)
                });
        }

        private static TacticalSituation TryProtectLead(
            MatchEngine engine)
        {
            if (engine.State.Minute < 70)
                return null;

            if (engine.State.HomeGoals <=
                engine.State.AwayGoals)
            {
                return null;
            }

            return new TacticalSituation(
                TacticalSituationType.ProtectLead,

                "PROTECT THE LEAD",

                $"You are leading " +
                $"{engine.State.HomeGoals}-" +
                $"{engine.State.AwayGoals}. " +
                $"{engine.AwayTeam.Name} are beginning to commit players forward.",

                new List<TacticalSituationOption>
                {
                    new(
                        "protect",
                        "PROTECT",
                        "Reduce risk and defend the advantage.",
                        0.96f,
                        0.84f,
                        0.72f,
                        0.82f),

                    new(
                        "counter",
                        "COUNTER",
                        "Allow them forward and attack the space.",
                        0.98f,
                        1.10f,
                        1.02f,
                        1.18f),

                    new(
                        "keep_attacking",
                        "KEEP ATTACKING",
                        "Continue trying to extend the lead.",
                        1.02f,
                        1.08f,
                        1.10f,
                        1.06f)
                });
        }
    }
}