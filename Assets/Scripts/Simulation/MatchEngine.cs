using FootballTactics.Teams;
using System.Linq;
using UnityEngine;

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

        private TacticalSituation pendingSituation;

        private int nextSituationMinute = 5;

        private float temporaryPossessionModifier = 1f;
        private float temporaryChanceModifier = 1f;
        private float temporaryFatigueModifier = 1f;
        private float temporaryCounterModifier = 1f;

        private int temporaryModifierMinutes;

        public MatchState State { get; }
        public TacticalSettings HomeTactics => homeTactics;
        public TacticalSettings AwayTactics => awayTactics;

        public Team HomeTeam => homeTeam;
        public Team AwayTeam => awayTeam;

        public int HomeSubstitutionsUsed { get; private set; }
        public int AwaySubstitutionsUsed { get; private set; }

        public Lineup HomeLineup => homeLineup;
        public Lineup AwayLineup => awayLineup;
        public TacticalSituation PendingSituation => pendingSituation;

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
            if (pendingSituation != null)
                return;

            if (State.Minute >= 90)
                return;

            State.AdvanceMinute();

            UpdateMomentum();
            UpdatePossession();

            SimulateAttacks(State.HomePossession);

            ApplyFatigue();

            ProcessTemporaryModifiers();

            TryGenerateSituation();
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

        public bool ResolveSituation(string optionId)
        {
            if (pendingSituation == null)
                return false;

            TacticalSituationOption selected =
                null;

            foreach (
                TacticalSituationOption option
                in pendingSituation.Options)
            {
                if (option.Id == optionId)
                {
                    selected = option;
                    break;
                }
            }

            if (selected == null)
                return false;

            temporaryPossessionModifier =
                selected.PossessionModifier;

            temporaryChanceModifier =
                selected.ChanceModifier;

            temporaryFatigueModifier =
                selected.FatigueModifier;

            temporaryCounterModifier =
                selected.CounterAttackModifier;

            temporaryModifierMinutes = 5;

            State.AddEvent(
                $"{selected.Title}: " +
                selected.Description);

            pendingSituation = null;

            return true;
        }

        private bool CanChangeTactics()
        {
            return State.Minute -
                   lastTacticalChangeMinute >= 3;
        }

        private void TryGenerateSituation()
        {
            if (pendingSituation != null)
                return;

            if (State.Minute < nextSituationMinute)
                return;

            // Give the game a chance to have a normal
            // stretch of football without an intervention.
            if (Random.value > 0.65f)
            {
                nextSituationMinute =
                    State.Minute + Random.Range(5, 10);

                return;
            }

            TacticalSituation situation =
                TacticalSituationGenerator.Generate(this);

            if (situation == null)
            {
                nextSituationMinute =
                    State.Minute + Random.Range(5, 10);

                return;
            }

            pendingSituation = situation;
        }

        private void ProcessTemporaryModifiers()
        {
            if (temporaryModifierMinutes <= 0)
                return;

            temporaryModifierMinutes--;

            if (temporaryModifierMinutes == 0)
            {
                temporaryPossessionModifier = 1f;
                temporaryChanceModifier = 1f;
                temporaryFatigueModifier = 1f;
                temporaryCounterModifier = 1f;
            }
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
                homeTactics.GetPossessionModifier() *
                homeTactics.GetTerritoryModifier();

            float awayMidfield =
                awayTeam.GetAverageMidfield(awayLineup) *
                awayTeam.GetRolePossessionImpact(awayLineup) *
                awayTactics.Formation.GetMidfieldModifier() *
                awayTactics.GetPossessionModifier() *
                awayTactics.GetTerritoryModifier();

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

            targetPossession *= temporaryPossessionModifier;

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
            int attackCount = Random.Range(0, 3);

            for (int i = 0; i < attackCount; i++)
            {
                bool homeAttack =
                    Random.value <
                    homePossession / 100f;

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

        private void SimulateAttack(Team attackingTeam, Team defendingTeam, TacticalSettings attackingTactics, bool isHome)
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

            attackingStrength *= temporaryChanceModifier;

            float defendingStrength =
                defendingTeam.GetAverageDefence(defendingLineup) *
                defendingTeam.GetRoleDefenceImpact(defendingLineup) *
                defendingTacticsModifier(defendingTeam);

            float pressingEffect =
                attackingTactics.GetPressingModifier();

            float fitnessEffect =
                attackingTeam
                    .GetAverageFitness(attackingLineup) / 100f;

            bool isCounterAttack =
                Random.value < 0.12f;

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

            if (isCounterAttack)
            {
                float counterModifier =
                    CalculateCounterAttackModifier(
                        attackingTeam,
                        defendingTeam);

                chanceQuality *= counterModifier;

                if (counterModifier > 1.05f)
                {
                    State.AddEvent(
                        $"{attackingTeam.Name} break quickly " +
                        "behind the defensive line.");
                }
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

        private float CalculateCounterAttackModifier(Team attackingTeam, Team defendingTeam)
        {
            DefensiveLine defensiveLine =
                defendingTeam == homeTeam
                    ? homeTactics.DefensiveLine
                    : awayTactics.DefensiveLine;

            float vulnerability =
                defendingTeam == homeTeam
                    ? homeTactics.GetCounterAttackVulnerability()
                    : awayTactics.GetCounterAttackVulnerability();

            float pace =
                defendingTeam == homeTeam
                    ? attackingTeam.GetAveragePace(
                        awayLineup)
                    : attackingTeam.GetAveragePace(
                        homeLineup);

            float paceFactor =
                Mathf.Clamp(
                    pace / 100f,
                    0.6f,
                    1.0f);

            return vulnerability * paceFactor;
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
            if (State.Minute % 5 != 0)
                return;

            int homeBaseDrain = 1;
            int awayBaseDrain = 1;

            homeBaseDrain =
                Mathf.CeilToInt(
                    homeBaseDrain *
                    temporaryFatigueModifier);

            homeTeam.ReduceFitness(
                homeBaseDrain,
                homeLineup,
                homeTactics.Pressing);

            awayTeam.ReduceFitness(
                awayBaseDrain,
                awayLineup,
                awayTactics.Pressing);
        }

        private MatchSituation GenerateSituation(
    bool isHome)
        {
            Team team =
                isHome
                    ? homeTeam
                    : awayTeam;

            Lineup lineup =
                isHome
                    ? homeLineup
                    : awayLineup;

            TacticalSettings tactics =
                isHome
                    ? homeTactics
                    : awayTactics;

            float buildUp =
                team.GetBuildUpContribution(lineup);

            float chanceCreation =
                team.GetChanceCreationContribution(lineup);

            float pressing =
                team.GetPressingContribution(lineup);

            float defending =
                team.GetDefensiveContribution(lineup);

            float roll =
                Random.value;

            if (roll < 0.20f)
            {
                return new MatchSituation(
                    MatchSituationType.BuildUp,
                    $"{team.Name} build patiently from the back.",
                    isHome,
                    buildUp);
            }

            if (roll < 0.38f)
            {
                return new MatchSituation(
                    MatchSituationType.Pressing,
                    $"{team.Name} press aggressively to win the ball.",
                    isHome,
                    pressing *
                    tactics.GetPressingModifier());
            }

            if (roll < 0.56f)
            {
                return new MatchSituation(
                    MatchSituationType.ChanceCreation,
                    $"{team.Name} create an attacking opportunity.",
                    isHome,
                    chanceCreation *
                    tactics.GetAttackModifier());
            }

            if (roll < 0.72f)
            {
                return new MatchSituation(
                    MatchSituationType.DefensiveStand,
                    $"{team.Name} hold their defensive shape.",
                    isHome,
                    defending *
                    tactics.GetDefenceModifier());
            }

            if (roll < 0.86f)
            {
                return new MatchSituation(
                    MatchSituationType.CounterAttack,
                    $"{team.Name} launch a quick counterattack.",
                    isHome,
                    tactics.Formation.GetCounterModifier());
            }

            return new MatchSituation(
                MatchSituationType.DefensiveTransition,
                $"{team.Name} recover their shape.",
                isHome,
                defending);
        }

        private void ProcessSituation(MatchSituation situation)
        {
            if (situation.Impact <= 0f)
                return;

            Team team =
                situation.IsHome
                    ? homeTeam
                    : awayTeam;

            Lineup lineup =
                situation.IsHome
                    ? homeLineup
                    : awayLineup;

            float eventChance =
                Mathf.Clamp(
                    situation.Impact * 0.35f,
                    0.02f,
                    0.30f);

            if (Random.value > eventChance)
                return;

            State.AddEvent(
                situation.Description);

            GenerateRoleSpecificEvent(
                team,
                lineup,
                situation);
        }

        private void GenerateRoleSpecificEvent(Team team, Lineup lineup, MatchSituation situation)
        {
            var players =
                team.GetStartingPlayers(lineup)
                    .ToList();

            if (players.Count == 0)
                return;

            PlayerRole targetRole =
                situation.Type switch
                {
                    MatchSituationType.BuildUp =>
                        PlayerRole.Playmaker,

                    MatchSituationType.ChanceCreation =>
                        PlayerRole.Winger,

                    MatchSituationType.Pressing =>
                        PlayerRole.BoxToBox,

                    MatchSituationType.DefensiveStand =>
                        PlayerRole.LineHolding,

                    MatchSituationType.DefensiveTransition =>
                        PlayerRole.Sweeper,

                    _ =>
                        PlayerRole.CentralMidfielder
                };

            Player player =
                players
                    .OrderByDescending(
                        p => p.Role == targetRole)
                    .FirstOrDefault();

            if (player == null)
                return;

            if (player.Role != targetRole)
                return;

            string description =
                situation.Type switch
                {
                    MatchSituationType.BuildUp =>
                        $"{player.Name} helps dictate the build-up.",

                    MatchSituationType.ChanceCreation =>
                        $"{player.Name} finds space to create a chance.",

                    MatchSituationType.Pressing =>
                        $"{player.Name} leads the press.",

                    MatchSituationType.DefensiveStand =>
                        $"{player.Name} helps maintain the defensive shape.",

                    MatchSituationType.DefensiveTransition =>
                        $"{player.Name} provides cover behind the defence.",

                    _ =>
                        $"{player.Name} influences the phase."
                };

            State.AddEvent(description);
        }
    }
}