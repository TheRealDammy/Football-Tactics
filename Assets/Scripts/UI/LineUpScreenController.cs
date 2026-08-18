using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FootballTactics.Simulation;
using FootballTactics.Teams;

namespace FootballTactics.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class LineupScreenController : MonoBehaviour
    {
        [SerializeField]
        private MatchSimulator matchSimulator;
        [SerializeField]
        private GameObject lineupScreen;

        [SerializeField]
        private GameObject matchScreen;

        private UIDocument document;
        private VisualElement root;

        private DropdownField formationDropdown;

        private VisualElement lineupContainer;
        private VisualElement playerListContainer;
        private VisualElement benchContainer;

        private Button startMatchButton;

        private readonly List<LineupSlotView> slotViews = new();

        private Formation currentFormation;

        private string selectedSlotId;

        private void Awake()
        {
            document =
                GetComponent<UIDocument>();

            root =
                document.rootVisualElement;

            FindElements();
            RegisterEvents();
        }

        private void Start()
        {
            currentFormation =
                Formation.FourThreeThree;

            BuildLineup();
        }

        private void FindElements()
        {
            formationDropdown =
                root.Q<DropdownField>(
                    "formationDropdown");

            lineupContainer =
                root.Q<VisualElement>(
                    "lineupContainer");

            playerListContainer =
                root.Q<VisualElement>(
                    "playerListContainer");

            startMatchButton =
                root.Q<Button>(
                    "startMatchButton");

            benchContainer =
                root.Q<VisualElement>("benchContainer");
        }

        private void RegisterEvents()
        {
            formationDropdown.RegisterValueChangedCallback(
                OnFormationChanged);

            startMatchButton.clicked +=
                OnStartMatch;
        }

        private void OnFormationChanged(
            ChangeEvent<string> evt)
        {
            currentFormation =
                ParseFormation(evt.newValue);

            BuildLineup();
        }

        private void BuildLineup()
        {
            slotViews.Clear();

            lineupContainer.Clear();

            Lineup lineup =
                LineupBuilder.BuildRecommendedLineup(
                    matchSimulator.HomeTeam,
                    currentFormation);

            FormationDefinition definition =
                currentFormation.GetDefinition();

            foreach (FormationSlot slot in definition.Slots)
            {
                Button button =
                    new();

                button.AddToClassList(
                    "lineup-slot");

                LineupSlotView slotView =
                    new(slot, button);

                Player player =
                    lineup.GetPlayer(slot.Id);

                slotView.SetPlayer(player);

                button.clicked += () =>
                {
                    SelectSlot(slotView);
                };

                PositionSlotButton(button,slot);

                lineupContainer.Add(button);

                slotViews.Add(slotView);
            }

            BuildPlayerList();
            BuildBench();
            UpdateFormationLabel();
        }

        private void BuildBench()
        {
            benchContainer.Clear();

            foreach (Player player in matchSimulator.HomeTeam.Bench)
            {
                Button button =
                    new();

                button.text =
                    $"{player.Name}  •  " +
                    $"{FormatPosition(player.Position)}  •  " +
                    $"{player.Fitness}%";

                button.AddToClassList("bench-player");

                benchContainer.Add(button);
            }
        }

        private void SelectSlot(
            LineupSlotView slotView)
        {
            selectedSlotId =
                slotView.Slot.Id;

            BuildPlayerList();
        }

        private void BuildPlayerList()
        {
            playerListContainer.Clear();

            if (string.IsNullOrEmpty(
                selectedSlotId))
            {
                Label prompt =
                    new("Select a position.");

                prompt.AddToClassList(
                    "player-list-placeholder");

                playerListContainer.Add(prompt);

                return;
            }

            FormationDefinition definition =
                currentFormation.GetDefinition();

            FormationSlot selectedSlot = null;

            foreach (FormationSlot slot in definition.Slots)
            {
                if (slot.Id == selectedSlotId)
                {
                    selectedSlot = slot;
                    break;
                }
            }

            if (selectedSlot == null)
                return;

            foreach (
                Player player
                in matchSimulator.HomeTeam.Players)
            {
                if (player.Position !=
                    selectedSlot.RequiredPosition)
                {
                    continue;
                }

                Button button =
                    CreatePlayerButton(
                        player,
                        selectedSlot);

                playerListContainer.Add(
                    button);
            }
        }

        private void PositionSlotButton(Button button, FormationSlot slot)
        {
            button.style.left =
                Length.Percent(slot.X);

            button.style.top =
                Length.Percent(slot.Y);

            button.style.translate =
                new Translate(
                    Length.Percent(-50),
                    Length.Percent(-50));
        }

        private Button CreatePlayerButton(
            Player player,
            FormationSlot slot)
        {
            Button button =
                new();

            button.text =
                $"{player.Name}  |  " +
                $"{player.Role}  |  " +
                $"FIT {player.Fitness}%";

            button.AddToClassList(
                "player-selection-button");

            button.clicked += () =>
            {
                AssignPlayer(
                    slot,
                    player);
            };

            return button;
        }

        private void AssignPlayer(FormationSlot slot, Player player)
        {
            // Remove this player from any other slot.
            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Player == player &&
                    slotView.Slot.Id != slot.Id)
                {
                    slotView.SetPlayer(null);
                }
            }

            // Assign player to selected slot.
            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Slot.Id == slot.Id)
                {
                    slotView.SetPlayer(player);
                    break;
                }
            }

            BuildPlayerList();
        }

        private void UpdateFormationLabel()
        {
            Label label =
                root.Q<Label>(
                    "formationSummaryLabel");

            if (label != null)
            {
                label.text =
                    $"FORMATION  {FormatFormation(currentFormation)}";
            }
        }

        private void OnStartMatch()
        {
            if (matchSimulator == null)
            {
                Debug.LogError(
                    "LineupScreenController: " +
                    "MatchSimulator is not assigned.");

                return;
            }

            if (slotViews.Count == 0)
            {
                Debug.LogError(
                    "LineupScreenController: " +
                    "No lineup slots exist.");

                return;
            }

            matchSimulator.StartConfiguredMatch(
                currentFormation,
                slotViews);

            if (!matchSimulator.HasMatch)
            {
                Debug.LogError(
                    "Failed to create match.");

                return;
            }

            if (lineupScreen != null)
                lineupScreen.SetActive(false);

            if (matchScreen != null)
                matchScreen.SetActive(true);
        }

        private static Formation ParseFormation(
            string value)
        {
            return value switch
            {
                "4-4-2" =>
                    Formation.FourFourTwo,

                "4-2-3-1" =>
                    Formation.FourTwoThreeOne,

                _ =>
                    Formation.FourThreeThree
            };
        }

        private static string FormatFormation(
            Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo =>
                    "4-4-2",

                Formation.FourTwoThreeOne =>
                    "4-2-3-1",

                _ =>
                    "4-3-3"
            };
        }

        private static string FormatPosition(PlayerPosition position)
        {
            return position switch
            {
                PlayerPosition.Goalkeeper => "GK",
                PlayerPosition.Defender => "DEF",
                PlayerPosition.Midfielder => "MID",
                PlayerPosition.Attacker => "ATT",
                _ => position.ToString()
            };
        }
    }
}