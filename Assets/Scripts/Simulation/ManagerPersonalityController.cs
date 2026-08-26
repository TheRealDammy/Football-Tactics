using UnityEngine;

namespace FootballTactics.Simulation
{
    public enum ManagerPersonality
    {
        Balanced,
        Possession,
        Gegenpress,
        CounterAttack,
        Pragmatic,
        Direct
    }

    public sealed class ManagerPersonalityController
    {
        private readonly ManagerPersonality personality;
        private int nextDecisionMinute;
        private int lastDecisionMinute = -10;

        public ManagerPersonality Personality => personality;
        public int Decisions { get; private set; }
        public int TotalDecisions => Decisions;
        public int MentalityChanges { get; private set; }
        public int PressingChanges { get; private set; }
        public int DefensiveLineChanges { get; private set; }
        public int FormationChanges { get; private set; }
        public int EarlyDecisions { get; private set; }
        public int MidDecisions { get; private set; }
        public int MiddleDecisions => MidDecisions;
        public int LateDecisions { get; private set; }
        public int TotalTacticalChanges => MentalityChanges + PressingChanges + DefensiveLineChanges + FormationChanges;
        public float BehaviourChanges => TotalTacticalChanges;

        public ManagerPersonalityController(ManagerPersonality personality, int initialMinute = 0)
        {
            this.personality = personality;
            nextDecisionMinute = initialMinute + Random.Range(8, 15);
        }

        public void ApplyInitialTactics(MatchEngine engine)
        {
            if (engine != null) ApplyInitialTactics(engine.HomeTactics);
        }

        public void ApplyInitialTactics(TacticalSettings tactics)
        {
            if (tactics == null) return;
            switch (personality)
            {
                case ManagerPersonality.Possession:
                    tactics.Mentality = Mentality.Balanced;
                    tactics.Pressing = Pressing.Medium;
                    tactics.DefensiveLine = DefensiveLine.Normal;
                    break;
                case ManagerPersonality.Gegenpress:
                    tactics.Mentality = Mentality.Attacking;
                    tactics.Pressing = Pressing.High;
                    tactics.DefensiveLine = DefensiveLine.High;
                    break;
                case ManagerPersonality.CounterAttack:
                    tactics.Mentality = Mentality.Defensive;
                    tactics.Pressing = Pressing.Low;
                    tactics.DefensiveLine = DefensiveLine.Deep;
                    break;
                case ManagerPersonality.Pragmatic:
                    tactics.Mentality = Mentality.Balanced;
                    tactics.Pressing = Pressing.Low;
                    tactics.DefensiveLine = DefensiveLine.Deep;
                    break;
                case ManagerPersonality.Direct:
                    tactics.Mentality = Mentality.Attacking;
                    tactics.Pressing = Pressing.Medium;
                    tactics.DefensiveLine = DefensiveLine.Normal;
                    break;
                default:
                    tactics.Mentality = Mentality.Balanced;
                    tactics.Pressing = Pressing.Medium;
                    tactics.DefensiveLine = DefensiveLine.Normal;
                    break;
            }
        }

        public void Update(MatchEngine engine)
        {
            if (engine == null || engine.State == null || engine.State.Minute >= 90) return;
            int minute = engine.State.Minute;
            if (minute < nextDecisionMinute || minute - lastDecisionMinute < 5) return;
            lastDecisionMinute = minute;
            nextDecisionMinute = minute + GetDecisionInterval();
            Decisions++;
            if (minute <= 30) EarlyDecisions++;
            else if (minute <= 60) MidDecisions++;
            else LateDecisions++;
            ApplyDecision(engine);
        }

        private void ApplyDecision(MatchEngine engine)
        {
            int diff = engine.State.HomeGoals - engine.State.AwayGoals;
            float fitness = engine.HomeTeam.GetAverageFitness(engine.HomeLineup);
            switch (personality)
            {
                case ManagerPersonality.Possession: ApplyPossession(engine, diff, fitness); break;
                case ManagerPersonality.Gegenpress: ApplyGegenpress(engine, diff, fitness); break;
                case ManagerPersonality.CounterAttack: ApplyCounterAttack(engine, diff, fitness); break;
                case ManagerPersonality.Pragmatic: ApplyPragmatic(engine, diff, fitness); break;
                case ManagerPersonality.Direct: ApplyDirect(engine, diff, fitness); break;
                default: ApplyBalanced(engine, diff, fitness); break;
            }
        }

        private void ApplyBalanced(MatchEngine e, int d, float f)
        {
            SetMentality(e, d < 0 ? Mentality.Attacking : d > 0 && e.State.Minute >= 70 ? Mentality.Defensive : Mentality.Balanced);
            SetPressing(e, f < 60f ? Pressing.Low : Pressing.Medium);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void ApplyPossession(MatchEngine e, int d, float f)
        {
            SetMentality(e, d < 0 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, f < 58f ? Pressing.Medium : Pressing.High);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void ApplyGegenpress(MatchEngine e, int d, float f)
        {
            bool conserve = f < 58f || (d > 0 && e.State.Minute >= 75);
            SetMentality(e, d < 0 || e.State.Minute < 70 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, conserve ? Pressing.Medium : Pressing.High);
            SetDefensiveLine(e, conserve ? DefensiveLine.Normal : DefensiveLine.High);
        }

        private void ApplyCounterAttack(MatchEngine e, int d, float f)
        {
            if (d > 0)
            {
                SetMentality(e, Mentality.Defensive);
                SetPressing(e, f < 65f ? Pressing.Low : Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
            else if (d < 0)
            {
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
            else
            {
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, Pressing.Low);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
        }

        private void ApplyPragmatic(MatchEngine e, int d, float f)
        {
            if (d > 0)
            {
                SetMentality(e, Mentality.Defensive);
                SetPressing(e, Pressing.Low);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
            else if (d < 0)
            {
                SetMentality(e, Mentality.Attacking);
                SetPressing(e, Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
            else
            {
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, f < 65f ? Pressing.Low : Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
        }

        private void ApplyDirect(MatchEngine e, int d, float f)
        {
            SetMentality(e, d <= 0 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, f < 60f ? Pressing.Low : Pressing.Medium);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void SetMentality(MatchEngine e, Mentality value)
        {
            if (e.HomeTactics.Mentality == value) return;
            e.SetHomeMentality(value);
            MentalityChanges++;
        }

        private void SetPressing(MatchEngine e, Pressing value)
        {
            if (e.HomeTactics.Pressing == value) return;
            e.SetHomePressing(value);
            PressingChanges++;
        }

        private void SetDefensiveLine(MatchEngine e, DefensiveLine value)
        {
            if (e.HomeTactics.DefensiveLine == value) return;
            e.SetHomeDefensiveLine(value);
            DefensiveLineChanges++;
        }

        private int GetDecisionInterval()
        {
            return personality switch
            {
                ManagerPersonality.Gegenpress => Random.Range(6, 10),
                ManagerPersonality.Direct => Random.Range(7, 12),
                ManagerPersonality.Possession => Random.Range(8, 13),
                ManagerPersonality.CounterAttack => Random.Range(9, 14),
                ManagerPersonality.Pragmatic => Random.Range(9, 15),
                _ => Random.Range(8, 14)
            };
        }
    }
}
