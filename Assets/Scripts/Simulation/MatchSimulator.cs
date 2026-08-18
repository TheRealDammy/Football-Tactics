using FootballTactics.Teams;
using FootballTactics.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballTactics.Simulation
{
    public class MatchSimulator : MonoBehaviour
    {
        private FootballTacticsInputActions input;

        private MatchEngine matchEngine;

        private Team homeTeam;
        private Team awayTeam;

        public MatchEngine Engine => matchEngine;

        public bool HasMatch =>
            matchEngine != null;

        private void Awake()
        {
            input = new FootballTacticsInputActions();

            input.Match.AdvanceMinute.performed += OnAdvanceMinute;
        }

        private void Start()
        {
            // Nothing is started here anymore.
            // The lineup screen starts the match.
        }

        public void StartConfiguredMatch(
            Formation formation,
            IReadOnlyList<LineupSlotView> slotViews)
        {
            homeTeam =
                TeamFactory.CreateHomeTeam();

            awayTeam =
                TeamFactory.CreateAwayTeam();

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

            // Build the lineup using THIS team.
            Lineup homeLineup =
                new(formation);

            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Player == null)
                    continue;

                Player actualPlayer =
                    FindPlayerByName(
                        homeTeam,
                        slotView.Player.Name);

                if (actualPlayer == null)
                    continue;

                homeLineup.Assign(
                    slotView.Slot,
                    actualPlayer);
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

        private void OnAdvanceMinute(
            InputAction.CallbackContext context)
        {
            if (matchEngine == null)
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
            input.Match.AdvanceMinute.performed -=
                OnAdvanceMinute;

            input.Dispose();
        }
    }
}