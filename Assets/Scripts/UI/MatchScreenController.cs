using FootballTactics.Simulation;
using FootballTactics.Teams;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MatchScreenController : MonoBehaviour
    {
        [SerializeField]
        private MatchSimulator matchSimulator;

        private UIDocument document;
        private VisualElement root;
        private VisualElement fullTimeOverlay;

        private Label homeTeamLabel;
        private Label awayTeamLabel;
        private Label scoreLabel;
        private Label clockLabel;

        private Label possessionLabel;
        private Label shotsLabel;
        private Label xgLabel;

        private Label formationLabel;
        private Label mentalityLabel;
        private Label pressingLabel;
        private Label defensiveLineLabel;
        private Label fitnessLabel;
        private Label substitutionCountLabel;

        private ScrollView eventList;
        private Label fullTimeLabel;

        private VisualElement substitutionOverlay;

        private Label substitutionAvailabilityLabel;
        private Label selectedSubstitutionLabel;

        private ScrollView playerOffList;
        private ScrollView playerOnList;

        private Player selectedPlayerOff;
        private Player selectedPlayerOn;

        private int displayedEvents;

        private VisualElement tacticalDecisionOverlay;
        private Label tacticalDecisionTitle;
        private Label tacticalDecisionDescription;
        private VisualElement tacticalDecisionOptions;

        private TacticalSituation displayedSituation;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;

            FindElements();
            RegisterButtons();

            Debug.Log(
            $"UI loaded | " +
            $"Score: {scoreLabel != null} | " +
            $"Events: {eventList != null} | " +
            $"Tactics: {formationLabel != null} | " +
            $"Subs: {substitutionOverlay != null}");
        }

        private void Start()
        {
            UpdateScreen();
        }

        private void Update()
        {
            UpdateScreen();
        }

        private void FindElements()
        {
            homeTeamLabel =
                root.Q<Label>("homeTeamLabel");

            awayTeamLabel =
                root.Q<Label>("awayTeamLabel");

            scoreLabel =
                root.Q<Label>("scoreLabel");

            clockLabel =
                root.Q<Label>("clockLabel");

            possessionLabel =
                root.Q<Label>("possessionLabel");

            shotsLabel =
                root.Q<Label>("shotsLabel");

            xgLabel =
                root.Q<Label>("xgLabel");

            formationLabel =
                root.Q<Label>("formationLabel");

            mentalityLabel =
                root.Q<Label>("mentalityLabel");

            pressingLabel =
                root.Q<Label>("pressingLabel");

            defensiveLineLabel =
                root.Q<Label>("defensiveLineLabel");

            eventList =
                root.Q<ScrollView>("eventList");

            fullTimeOverlay =
                root.Q<VisualElement>("fullTimeOverlay");

            fullTimeLabel =
                root.Q<Label>("fullTimeLabel");

            substitutionOverlay =
                root.Q<VisualElement>("substitutionOverlay");

            substitutionAvailabilityLabel =
                root.Q<Label>("substitutionAvailabilityLabel");

            selectedSubstitutionLabel =
                root.Q<Label>("selectedSubstitutionLabel");

            playerOffList =
                root.Q<ScrollView>("playerOffList");

            playerOnList =
                root.Q<ScrollView>("playerOnList");

            tacticalDecisionOverlay =
                root.Q<VisualElement>(
                            "tacticalDecisionOverlay");

            tacticalDecisionTitle =
                root.Q<Label>(
                    "tacticalDecisionTitle");

            tacticalDecisionDescription =
                root.Q<Label>(
                    "tacticalDecisionDescription");

            tacticalDecisionOptions =
                root.Q<VisualElement>(
                    "tacticalDecisionOptions");

            Debug.Log(
                $"Tactical UI: " +
                $"Overlay={tacticalDecisionOverlay != null}, " +
                $"Title={tacticalDecisionTitle != null}, " +
                $"Description={tacticalDecisionDescription != null}, " +
                $"Options={tacticalDecisionOptions != null}");
        }

        private void RegisterButtons()
        {
            root.Q<Button>("defensiveButton")
                .clicked += () =>
                {
                    matchSimulator.SetMentality(
                        Mentality.Defensive);
                };

            root.Q<Button>("balancedButton")
                .clicked += () =>
                {
                    matchSimulator.SetMentality(
                        Mentality.Balanced);
                };

            root.Q<Button>("attackingButton")
                .clicked += () =>
                {
                    matchSimulator.SetMentality(
                        Mentality.Attacking);
                };

            root.Q<Button>("lowPressButton")
                .clicked += () =>
                {
                    matchSimulator.SetPressing(
                        Pressing.Low);
                };

            root.Q<Button>("mediumPressButton")
                .clicked += () =>
                {
                    matchSimulator.SetPressing(
                        Pressing.Medium);
                };

            root.Q<Button>("highPressButton")
                .clicked += () =>
                {
                    matchSimulator.SetPressing(
                        Pressing.High);
                };

            root.Q<Button>("deepLineButton")
                .clicked += () =>
                {
                    matchSimulator.SetDefensiveLine(
                        DefensiveLine.Deep);
                };

            root.Q<Button>("normalLineButton")
                .clicked += () =>
                {
                    matchSimulator.SetDefensiveLine(
                        DefensiveLine.Normal);
                };

            root.Q<Button>("highLineButton")
                .clicked += () =>
                {
                    matchSimulator.SetDefensiveLine(
                        DefensiveLine.High);
                };

            root.Q<Button>("substitutionButton")
                .clicked += OpenSubstitutionOverlay;

            root.Q<Button>("cancelSubstitutionButton")
                .clicked += CloseSubstitutionOverlay;

            root.Q<Button>("confirmSubstitutionButton")
                .clicked += ConfirmSubstitution;

            root.Q<Button>("fourFourTwoButton")
                .clicked += () =>
                {
                    matchSimulator.SetFormation(
                        Formation.FourFourTwo);
                };

            root.Q<Button>("fourThreeThreeButton")
                .clicked += () =>
                {
                    matchSimulator.SetFormation(
                        Formation.FourThreeThree);
                };

            root.Q<Button>("fourTwoThreeOneButton")
                .clicked += () =>
                {
                    matchSimulator.SetFormation(
                        Formation.FourTwoThreeOne);
                };
        }

        private void UpdateScreen()
        {
            if (matchSimulator == null)
                return;

            if (!matchSimulator.HasMatch)
                return;

            MatchEngine engine = matchSimulator.Engine;

            if (engine == null)
                return;

            MatchState state = engine.State;

            if (state == null)
                return;


            // ---------------------------------------------------------
            // HEADER
            // ---------------------------------------------------------

            if (homeTeamLabel != null)
                homeTeamLabel.text = engine.HomeTeam.Name;

            if (awayTeamLabel != null)
                awayTeamLabel.text = engine.AwayTeam.Name;

            if (scoreLabel != null)
            {
                scoreLabel.text =
                    $"{state.HomeGoals} - {state.AwayGoals}";
            }

            if (clockLabel != null)
            {
                clockLabel.text =
                    $"{state.Minute}:00";
            }


            // ---------------------------------------------------------
            // MATCH STATS
            // ---------------------------------------------------------

            if (possessionLabel != null)
            {
                possessionLabel.text =
                    $"{state.HomePossession:F0}% - " +
                    $"{state.AwayPossession:F0}%";
            }

            if (shotsLabel != null)
            {
                shotsLabel.text =
                    $"{state.HomeShots} - " +
                    $"{state.AwayShots}";
            }

            if (xgLabel != null)
            {
                xgLabel.text =
                    $"{state.HomeXG:F2} - " +
                    $"{state.AwayXG:F2}";
            }


            // ---------------------------------------------------------
            // TACTICS
            // ---------------------------------------------------------

            if (formationLabel != null)
            {
                formationLabel.text =
                    $"Formation: " +
                    FormatEnum(engine.HomeTactics.Formation);
            }

            if (mentalityLabel != null)
            {
                mentalityLabel.text =
                    $"Mentality: " +
                    engine.HomeTactics.Mentality;
            }

            if (pressingLabel != null)
            {
                pressingLabel.text =
                    $"Pressing: " +
                    engine.HomeTactics.Pressing;
            }

            if (defensiveLineLabel != null)
            {
                defensiveLineLabel.text =
                    $"Defensive Line: " +
                    FormatEnum(engine.HomeTactics.DefensiveLine);
            }


            // ---------------------------------------------------------
            // EVENTS
            // ---------------------------------------------------------

            UpdateEvents(state);


            // ---------------------------------------------------------
            // FULL TIME
            // ---------------------------------------------------------

            if (fullTimeOverlay != null)
            {
                if (state.Minute >= 90)
                {
                    if (fullTimeLabel != null)
                        fullTimeLabel.text = "FULL TIME";

                    fullTimeOverlay.style.display =
                        DisplayStyle.Flex;
                }
                else
                {
                    fullTimeOverlay.style.display =
                        DisplayStyle.None;
                }
            }

            UpdateTacticalDecision();
        }

        private void UpdateEvents(MatchState state)
        {
            if (eventList == null)
                return;

            while (displayedEvents < state.Events.Count)
            {
                MatchEvent matchEvent =
                    state.Events[displayedEvents];

                Label label =
                    new(matchEvent.ToString());

                label.style.marginBottom = 9;

                eventList.Add(label);

                displayedEvents++;
            }
        }

        private void OpenSubstitutionOverlay()
        {
            if (!matchSimulator.HasMatch)
                return;

            MatchEngine engine =
                matchSimulator.Engine;

            if (engine.State.Minute < 15 ||
                engine.State.Minute >= 90)
            {
                return;
            }

            if (engine.HomeSubstitutionsUsed >= 3)
            {
                return;
            }

            selectedPlayerOff = null;
            selectedPlayerOn = null;

            substitutionOverlay.style.display =
                DisplayStyle.Flex;

            substitutionAvailabilityLabel.text =
                $"Substitutions remaining: " +
                $"{5 - engine.HomeSubstitutionsUsed}";

            RefreshSubstitutionLists();
            UpdateSelectedSubstitutionText();
        }

        private void RefreshSubstitutionLists()
        {
            playerOffList.Clear();
            playerOnList.Clear();

            MatchEngine engine =
                matchSimulator.Engine;

            foreach (Player player in engine.HomeTeam.Players)
            {
                Button button =
                    CreatePlayerButton(
                        player,
                        true);

                playerOffList.Add(button);
            }

            foreach (Player player in engine.HomeTeam.Bench)
            {
                Button button =
                    CreatePlayerButton(
                        player,
                        false);

                playerOnList.Add(button);
            }
        }

        private Button CreatePlayerButton( Player player, bool playerOff)
        {
            string text =
                $"{player.Name}\n" +
                $"{FormatPosition(player.Position)}    " +
                $"FIT {player.Fitness}%";

            Button button =
                new Button(() => { });

            button.AddToClassList("player-button");

            button.clicked += () =>
            {
                if (playerOff)
                {
                    selectedPlayerOff = player;
                }
                else
                {
                    selectedPlayerOn = player;
                }

                RefreshSelectionVisuals();
                UpdateSelectedSubstitutionText();
            };

            return button;
        }

        private void RefreshSelectionVisuals()
        {
            foreach (VisualElement child in playerOffList.Children())
            {
                child.RemoveFromClassList("selected-player");
            }

            foreach (VisualElement child in playerOnList.Children())
            {
                child.RemoveFromClassList("selected-player");
            }
        }

        private void ConfirmSubstitution()
        {
            if (selectedPlayerOff == null ||
                selectedPlayerOn == null)
            {
                return;
            }

            if (selectedPlayerOff.Position !=
                selectedPlayerOn.Position)
            {
                selectedSubstitutionLabel.text =
                    "Players must play the same position.";

                return;
            }

            bool success =
                matchSimulator.MakeSubstitution(
                    selectedPlayerOn.Name,
                    selectedPlayerOff.Name);

            if (!success)
            {
                selectedSubstitutionLabel.text =
                    "Unable to make substitution.";

                return;
            }

            CloseSubstitutionOverlay();
        }

        private void CloseSubstitutionOverlay()
        {
            substitutionOverlay.style.display =
                DisplayStyle.None;

            selectedPlayerOff = null;
            selectedPlayerOn = null;
        }

        private void UpdateSelectedSubstitutionText()
        {
            if (selectedPlayerOff == null &&
                selectedPlayerOn == null)
            {
                selectedSubstitutionLabel.text =
                    "Select a player to come off and a player to come on.";

                return;
            }

            string off =
                selectedPlayerOff?.Name ?? "-";

            string on =
                selectedPlayerOn?.Name ?? "-";

            selectedSubstitutionLabel.text =
                $"{off}  →  {on}";
        }

        private static string FormatEnum(System.Enum value)
        {
            return value.ToString()
                .Replace("FourThreeThree", "4-3-3")
                .Replace("FourFourTwo", "4-4-2")
                .Replace("FourTwoThreeOne", "4-2-3-1")
                .Replace("Deep", "Deep")
                .Replace("Normal", "Normal")
                .Replace("High", "High");
        }

        private static string FormatPosition(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => "GK",
                PlayerPosition.Defender => "DEF",
                PlayerPosition.Midfielder => "MID",
                PlayerPosition.Attacker => "ATT",
                _ => "?"
            };
        }

        private void UpdateTacticalDecision()
        {
            TacticalSituation situation =
                matchSimulator.PendingSituation;

            if (situation == null)
            {
                displayedSituation = null;

                tacticalDecisionOverlay.style.display =
                    DisplayStyle.None;

                return;
            }

            if (displayedSituation == situation)
                return;

            displayedSituation = situation;

            tacticalDecisionTitle.text =
                situation.Title;

            tacticalDecisionDescription.text =
                situation.Description;

            tacticalDecisionOptions.Clear();

            foreach (
                TacticalSituationOption option
                in situation.Options)
            {
                Button button =
                    new();

                button.text =
                    $"{option.Title}\n\n" +
                    option.Description;

                button.AddToClassList(
                    "decision-button");

                button.clicked += () =>
                {
                    matchSimulator.ResolveSituation(
                        option.Id);

                    displayedSituation = null;

                    UpdateTacticalDecision();
                };

                tacticalDecisionOptions.Add(
                    button);
            }

            tacticalDecisionOverlay.style.display =
                DisplayStyle.Flex;
        }
    }
}