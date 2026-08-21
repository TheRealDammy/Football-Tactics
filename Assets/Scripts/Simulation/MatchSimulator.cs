using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using FootballTactics.Teams;
using FootballTactics.UI;

namespace FootballTactics.Simulation
{
    public class MatchSimulator : MonoBehaviour
    {
        private FootballTacticsInputActions input;

        private MatchEngine matchEngine;

        private Team homeTeam;
        private Team awayTeam;

        public MatchEngine Engine => matchEngine;

        public Team HomeTeam => homeTeam;
        public Team AwayTeam => awayTeam;
        public TacticalSituation PendingSituation => matchEngine?.PendingSituation;

        public bool HasMatch =>
            matchEngine != null;

        private void Awake()
        {
            input = new FootballTacticsInputActions();

            input.Match.AdvanceMinute.performed +=
                OnAdvanceMinute;

            InitializeTeams();
        }

        private void InitializeTeams()
        {
            homeTeam =
                TeamFactory.CreateHomeTeam();

            awayTeam =
                TeamFactory.CreateAwayTeam();
        }

        public void StartConfiguredMatch(
            Formation formation,
            IReadOnlyList<LineupSlotView> slotViews)
        {
            if (homeTeam == null ||
                awayTeam == null)
            {
                InitializeTeams();
            }

            TacticalSettings homeTactics =
                new()
                {
                    Formation = formation,
                    Mentality = Mentality.Balanced,
                    Pressing = Pressing.Medium,
                    DefensiveLine = DefensiveLine.Normal
                };

            TacticalSettings awayTactics =
                new()
                {
                    Formation = Formation.FourFourTwo,
                    Mentality = Mentality.Balanced,
                    Pressing = Pressing.Medium,
                    DefensiveLine = DefensiveLine.Normal
                };

            Lineup homeLineup = LineupBuilder.BuildFromSlotViews(formation,  slotViews);

            if (!homeTeam.ApplyStartingLineup(homeLineup))
            {
                Debug.LogError(
                    "Unable to apply starting lineup.");

                return;
            }

            matchEngine =
                new MatchEngine(
                    homeTeam,
                    awayTeam,
                    homeTactics,
                    awayTactics);

            matchEngine.SetHomeLineup(
                homeLineup);
        }

        private static Player FindPlayerByName(
            Team team,
            string name)
        {
            foreach (Player player in team.Players)
            {
                if (player.Name == name)
                    return player;
            }

            return null;
        }

        private void OnAdvanceMinute( InputAction.CallbackContext context)
        {
            if (matchEngine == null)
                return;

            if (matchEngine.PendingSituation != null)
                return;

            matchEngine.SimulateMinute();

            if (matchEngine.State.Minute >= 90)
            {
                Debug.Log("FULL TIME");
            }
        }

        public void SetMentality(
            Mentality mentality)
        {
            matchEngine?.SetHomeMentality(mentality);
        }

        public void SetPressing(
            Pressing pressing)
        {
            matchEngine?.SetHomePressing(pressing);
        }

        public void SetDefensiveLine(
            DefensiveLine line)
        {
            matchEngine?.SetHomeDefensiveLine(line);
        }

        public void SetFormation(
            Formation formation)
        {
            matchEngine?.ChangeFormation(formation);
        }

        public bool MakeSubstitution(
            string playerOn,
            string playerOff)
        {
            return matchEngine != null &&
                   matchEngine.MakeHomeSubstitution(
                       playerOn,
                       playerOff);
        }

        public bool ResolveSituation( string optionId)
        {
            return matchEngine != null &&
                   matchEngine.ResolveSituation(optionId);
        }

        private void OnEnable()
        {
            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
        }

        private void OnDestroy()
        {
            if (input != null)
            {
                input.Match.AdvanceMinute.performed -=
                    OnAdvanceMinute;

                input.Dispose();
            }
        }
    }
}