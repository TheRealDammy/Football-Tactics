using UnityEngine;
using FootballTactics.Teams;

namespace FootballTactics.Simulation
{
    public class MatchEngine
    {
        private readonly Team homeTeam;
        private readonly Team awayTeam;

        private readonly TacticalSettings homeTactics;
        private readonly TacticalSettings awayTactics;

        private Lineup homeLineup;
        private Lineup awayLineup;

        private float currentHomePossession = 50f;
        private float homeMomentum = 0f;
        private float awayMomentum = 0f;
        private int lastTacticalChangeMinute = -10;

        public MatchState State { get; }
        public TacticalSettings HomeTactics => homeTactics;
        public TacticalSettings AwayTactics => awayTactics;

        public Team HomeTeam => homeTeam;
        public Team AwayTeam => awayTeam;

        public int HomeSubstitutionsUsed { get; private set; }
        public int AwaySubstitutionsUsed { get; private set; }

        public Lineup HomeLineup => homeLineup;
        public Lineup AwayLineup => awayLineup;

        public MatchEngine(
            Team homeTeam,
            Team awayTeam,
            TacticalSettings homeTactics,
            TacticalSettings awayTactics)
        {
            this.homeTeam = homeTeam;
            this.awayTeam = awayTeam;

            this.homeTactics = homeTactics;
            this.awayTactics = awayTactics;

            homeLineup = LineupBuilder.BuildRecommendedLineup(homeTeam, homeTactics.Formation);

            awayLineup = LineupBuilder.BuildRecommendedLineup(awayTeam, awayTactics.Formation);

            State = new MatchState();
        }

        public void SimulateMinute()
        {
            if (State.Minute >= 90)
                return;

            State.AdvanceMinute();

            UpdateMomentum();
            UpdatePossession();

            SimulateAttacks(State.HomePossession);

            ApplyFatigue();
        }

        public void SetHomeMentality(Mentality mentality)
        {
            homeTactics.Mentality = mentality;

            State.AddEvent(
                $"{homeTeam.Name} changes mentality to {mentality}");
        }

        public void SetHomePressing(Pressing pressing)
        {
            homeTactics.Pressing = pressing;

            State.AddEvent(
                $"{homeTeam.Name} changes pressing to {pressing}");
        }

        public void SetHomeDefensiveLine(DefensiveLine defensiveLine)
        {
            homeTactics.DefensiveLine = defensiveLine;

            State.AddEvent(
                $"{homeTeam.Name} changes defensive line to {defensiveLine}");
        }

        public void SetHomeFormation(Formation formation)
        {
            homeTactics.Formation = formation;

            State.AddEvent(                $"{homeTeam.Name} changes formation to {formation}");
        }

        public void SetHomeLineup(Lineup lineup)
        {
            homeLineup = lineup;

            State.AddEvent(
                $"{homeTeam.Name} lineup updated.");
        }

        public bool MakeHomeSubstitution( string playerOn, string playerOff)
        {
            if (HomeSubstitutionsUsed >= 5)
                return false;

            if (!homeTeam.MakeSubstitution(playerOn, playerOff))
            {
                return false;
            }

            if (State.Minute < 15)
                return false;

            if (State.Minute >= 90)
                return false;

            Player newPlayer =
                homeTeam.Players.Find(
                    p => p.Name == playerOn);

            Player oldPlayer =
                homeTeam.SubstitutedPlayers.Find(
                    p => p.Name == playerOff);

            if (newPlayer == null ||
                oldPlayer == null)
            {
                return false;
            }

            homeLineup.ReplacePlayer(
                oldPlayer,
                newPlayer);

            HomeSubstitutionsUsed++;

            State.AddEvent(
                $"{homeTeam.Name}: " +
                $"{playerOff} OFF, " +
                $"{playerOn} ON");

            return true;
        }

        public bool MakeAwaySubstitution(string playerOn, string playerOff)
        {
            if (AwaySubstitutionsUsed >= 5)
                return false;

            if (!awayTeam.MakeSubstitution(
                playerOn,
                playerOff))
            {
                return false;
            }

            if (State.Minute < 15)
                return false;

            if (State.Minute >= 90)
                return false;

            AwaySubstitutionsUsed++;

            State.AddEvent(
                $"{awayTeam.Name}: {playerOff} OFF, {playerOn} ON");

            return true;
        }
     
        public bool ChangeFormation(Formation formation)
        {
            if (State.Minute >= 90)
                return false;

            if (!CanChangeTactics())
                return false;

            lastTacticalChangeMinute =
                State.Minute;

            homeTactics.Formation = formation;

            RebuildHomeLineup();

            State.AddEvent(
                $"{homeTeam.Name} switches to " +
                $"{formation}");

            return true;
        }
        private bool CanChangeTactics()
        {
            return State.Minute -
                   lastTacticalChangeMinute >= 3;
        }

        private void RebuildHomeLineup()
        {
            homeLineup =
                LineupBuilder.BuildRecommendedLineup(
                    homeTeam,
                    homeTactics.Formation);
        }

        private void UpdatePossession()
        {
            float homeMidfield =
                homeTeam.GetAverageMidfield(homeLineup) *
                homeTeam.GetRolePossessionImpact(homeLineup) *
                homeTactics.Formation.GetMidfieldModifier() *
                homeTactics.GetPossessionModifier();

            float awayMidfield =
                awayTeam.GetAverageMidfield(awayLineup) *
                awayTeam.GetRolePossessionImpact(awayLineup) *
                awayTactics.Formation.GetMidfieldModifier() *
                awayTactics.GetPossessionModifier();

            float total = homeMidfield + awayMidfield;

            if (total <= 0f)
                return;

            float basePossession =
                homeMidfield / total * 100f;

            float momentumDifference =
                homeMomentum - awayMomentum;

            float fatigueDifference =
                (awayTeam.AverageFitness - homeTeam.AverageFitness) * 0.05f;

            float targetPossession =
                basePossession +
                momentumDifference * 0.8f +
                fatigueDifference +
                2f; // home advantage

            // Move toward the target rather than jumping there.
            currentHomePossession = Mathf.Lerp(
                currentHomePossession,
                targetPossession,
                0.15f
            );

            // Keep possession within sensible football limits.
            currentHomePossession =
                Mathf.Clamp(currentHomePossession, 25f, 75f);

            State.SetPossession(currentHomePossession);
        }

        private void UpdateMomentum()
        {
            // Momentum naturally decays.
            homeMomentum *= 0.92f;
            awayMomentum *= 0.92f;

            // Small match-to-match randomness.
            homeMomentum += Random.Range(-0.4f, 0.4f);
            awayMomentum += Random.Range(-0.4f, 0.4f);

            // Stronger teams have slightly more control.
            homeMomentum += (homeTeam.AverageMidfield - 65f) * 0.01f;
            awayMomentum += (awayTeam.AverageMidfield - 65f) * 0.01f;
        }

        private void SimulateAttacks(float homePossession)
        {
            // More than one attacking sequence can happen in a minute.
            int attackCount = Random.Range(0, 3);

            for (int i = 0; i < attackCount; i++)
            {
                float homeAttackProbability =
                    homePossession / 100f;

                bool homeAttack =
                    Random.value < homeAttackProbability;

                if (homeAttack)
                {
                    SimulateAttack(
                        homeTeam,
                        awayTeam,
                        homeTactics,
                        true);
                }
                else
                {
                    SimulateAttack(
                        awayTeam,
                        homeTeam,
                        awayTactics,
                        false);
                }
            }
        }

        private void SimulateAttack(
            Team attackingTeam,
            Team defendingTeam,
            TacticalSettings attackingTactics,
            bool isHome)
        {
            Lineup attackingLineup =
                attackingTeam == homeTeam
                    ? homeLineup
                    : awayLineup;

            Lineup defendingLineup =
                defendingTeam == homeTeam
                    ? homeLineup
                    : awayLineup;

            float attackingStrength =
                attackingTeam.GetAverageAttack(attackingLineup) *
                attackingTeam.GetRoleAttackImpact(attackingLineup) *
                attackingTactics.Formation.GetAttackModifier() *
                attackingTactics.GetAttackModifier();

            float defendingStrength =
                defendingTeam.GetAverageDefence(defendingLineup) *
                defendingTeam.GetRoleDefenceImpact(defendingLineup) *
                defendingTacticsModifier(defendingTeam);

            float pressingEffect =
                attackingTactics.GetPressingModifier();

            float fitnessEffect =
                attackingTeam
                    .GetAverageFitness(attackingLineup) / 100f;

            // Tired teams become significantly less effective.
            if (attackingTeam.AverageFitness < 70f)
            {
                fitnessEffect *= 0.90f;
            }

            if (attackingTeam.AverageFitness < 55f)
            {
                fitnessEffect *= 0.80f;
            }

            float chanceQuality =
                attackingStrength /
                (attackingStrength + defendingStrength);

            chanceQuality *= pressingEffect;
            chanceQuality *= fitnessEffect;

            float shotProbability =
                0.18f + chanceQuality * 0.20f;

            if (Random.value > shotProbability)
                return;

            float xG =
                Mathf.Clamp(
                    0.05f +
                    chanceQuality * 0.30f,
                    0.03f,
                    0.55f);

            if (isHome)
            {
                State.AddHomeShot(xG);

                State.AddEvent(
                    $"Shot — {homeTeam.Name} ({xG:F2} xG)");
            }
            else
            {
                State.AddAwayShot(xG);

                State.AddEvent(
                    $"Shot — {awayTeam.Name} ({xG:F2} xG)");
            }

            float goalProbability = xG;

            // A high defensive line creates additional risk.
            if (defendingTeam == homeTeam &&
                homeTactics.DefensiveLine == DefensiveLine.High)
            {
                goalProbability +=
                    attackingTeam.AveragePace / 1000f;
            }

            if (defendingTeam == awayTeam &&
                awayTactics.DefensiveLine == DefensiveLine.High)
            {
                goalProbability +=
                    attackingTeam.AveragePace / 1000f;
            }

            if (Random.value < goalProbability)
            {
                if (isHome)
                {
                    State.HomeScores();

                    State.AddEvent(
                        $"GOAL! {homeTeam.Name}");
                }
                else
                {
                    State.AwayScores();

                    State.AddEvent(
                        $"GOAL! {awayTeam.Name}");
                }
            }
        }

        private float defendingTacticsModifier(Team team)
        {
            if (team == homeTeam)
            {
                return homeTactics.GetDefenceModifier() *
                       homeTactics.Formation.GetDefenceModifier();
            }

            return awayTactics.GetDefenceModifier() *
                   awayTactics.Formation.GetDefenceModifier();
        }
        private void ApplyFatigue()
        {
            // High pressing costs more energy.
            float homeDrain =
                homeTactics.GetFitnessDrain();

            float awayDrain =
                awayTactics.GetFitnessDrain();

            if (State.Minute % 5 == 0)
            {
                homeTeam.ReduceFitness(
                    Mathf.CeilToInt(homeDrain * 10f));

                awayTeam.ReduceFitness(
                    Mathf.CeilToInt(awayDrain * 10f));
            }
        }
    }
}