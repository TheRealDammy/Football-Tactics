using UnityEngine;

namespace FootballTactics.Simulation
{
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
            if (engine == null) return;

            ManagerProfile profile = ManagerProfile.Create(personality);
            engine.SetHomeMentality(profile.DefaultMentality);
            engine.SetHomePressing(profile.DefaultPressing);
            engine.SetHomeDefensiveLine(profile.DefaultDefensiveLine);

            // Changing the formation through the engine also rebuilds the lineup.
            // This prevents the manager's preferred formation from disagreeing with
            // the actual lineup used by the simulation.
            if (engine.HomeTactics.Formation != profile.PreferredFormation)
                engine.ChangeFormation(profile.PreferredFormation);
        }

        public void ApplyInitialTactics(TacticalSettings tactics)
        {
            if (tactics == null) return;

            ManagerProfile profile = ManagerProfile.Create(personality);
            tactics.Formation = profile.PreferredFormation;
            tactics.Mentality = profile.DefaultMentality;
            tactics.Pressing = profile.DefaultPressing;
            tactics.DefensiveLine = profile.DefaultDefensiveLine;
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
                case ManagerPersonality.Possession:
                    ApplyPossession(engine, diff, fitness);
                    break;
                case ManagerPersonality.Gegenpress:
                    ApplyGegenpress(engine, diff, fitness);
                    break;
                case ManagerPersonality.CounterAttack:
                    ApplyCounterAttack(engine, diff, fitness);
                    break;
                case ManagerPersonality.Pragmatic:
                    ApplyPragmatic(engine, diff, fitness);
                    break;
                case ManagerPersonality.Direct:
                    ApplyDirect(engine, diff, fitness);
                    break;
                default:
                    ApplyBalanced(engine, diff, fitness);
                    break;
            }
        }

        private void ApplyBalanced(MatchEngine e, int d, float f)
        {
            SetFormation(e, d < 0 && e.State.Minute >= 60
                ? Formation.FourThreeThree
                : Formation.FourTwoThreeOne);
            SetMentality(e, d < 0 ? Mentality.Attacking : d > 0 && e.State.Minute >= 70 ? Mentality.Defensive : Mentality.Balanced);
            SetPressing(e, f < 60f ? Pressing.Low : Pressing.Medium);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void ApplyPossession(MatchEngine e, int d, float f)
        {
            // The simulations favour 4-2-3-1 for control. Keep the shape unless
            // chasing the game, when 4-3-3 produces more attacking output.
            SetFormation(e, d < 0 && e.State.Minute >= 60
                ? Formation.FourThreeThree
                : Formation.FourTwoThreeOne);
            SetMentality(e, d < 0 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, f < 58f ? Pressing.Medium : Pressing.High);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void ApplyGegenpress(MatchEngine e, int d, float f)
        {
            // 4-3-3 complements the high-press profile and was the strongest
            // attacking shape in the squad matrix for direct/high-tempo play.
            SetFormation(e, d > 0 && e.State.Minute >= 75
                ? Formation.FourTwoThreeOne
                : Formation.FourThreeThree);

            bool conserve = f < 58f || (d > 0 && e.State.Minute >= 75);
            SetMentality(e, d < 0 || e.State.Minute < 70 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, conserve ? Pressing.Medium : Pressing.High);
            SetDefensiveLine(e, conserve ? DefensiveLine.Normal : DefensiveLine.High);
        }

        private void ApplyCounterAttack(MatchEngine e, int d, float f)
        {
            if (d > 0)
            {
                SetFormation(e, Formation.FourFourTwo);
                SetMentality(e, Mentality.Defensive);
                SetPressing(e, f < 65f ? Pressing.Low : Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
            else if (d < 0)
            {
                SetFormation(e, Formation.FourTwoThreeOne);
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
            else
            {
                SetFormation(e, Formation.FourFourTwo);
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, Pressing.Low);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
        }

        private void ApplyPragmatic(MatchEngine e, int d, float f)
        {
            if (d > 0)
            {
                SetFormation(e, Formation.FourFourTwo);
                SetMentality(e, Mentality.Defensive);
                SetPressing(e, Pressing.Low);
                SetDefensiveLine(e, DefensiveLine.Deep);
            }
            else if (d < 0)
            {
                SetFormation(e, Formation.FourThreeThree);
                SetMentality(e, Mentality.Attacking);
                SetPressing(e, Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
            else
            {
                SetFormation(e, Formation.FourFourTwo);
                SetMentality(e, Mentality.Balanced);
                SetPressing(e, f < 65f ? Pressing.Low : Pressing.Medium);
                SetDefensiveLine(e, DefensiveLine.Normal);
            }
        }

        private void ApplyDirect(MatchEngine e, int d, float f)
        {
            SetFormation(e, d < 0 ? Formation.FourThreeThree : Formation.FourFourTwo);
            SetMentality(e, d <= 0 ? Mentality.Attacking : Mentality.Balanced);
            SetPressing(e, f < 60f ? Pressing.Low : Pressing.Medium);
            SetDefensiveLine(e, d < 0 ? DefensiveLine.High : DefensiveLine.Normal);
        }

        private void SetFormation(MatchEngine e, Formation value)
        {
            if (e.HomeTactics.Formation == value) return;
            if (!e.ChangeFormation(value)) return;
            FormationChanges++;
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
                ManagerPersonality.Gegenpress => Random.Range(7, 11),
                ManagerPersonality.Direct => Random.Range(8, 13),
                ManagerPersonality.Possession => Random.Range(9, 14),
                ManagerPersonality.CounterAttack => Random.Range(10, 15),
                ManagerPersonality.Pragmatic => Random.Range(10, 16),
                _ => Random.Range(9, 15)
            };
        }
    }
}
