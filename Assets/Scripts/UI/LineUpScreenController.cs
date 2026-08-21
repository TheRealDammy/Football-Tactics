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
        private Button resetLineupButton;

        private Label formationFitLabel;
        private Label formationFitSummary;

        private readonly List<LineupSlotView> slotViews = new();

        private readonly List<PlayerDragHandler> dragHandlers = new();

        private Formation currentFormation;

        private string selectedSlotId;

        private LineupSlotView selectedSlot;

        private Player selectedStarter;
        private Lineup currentLineup;

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

            resetLineupButton =
                root.Q<Button>("resetLineupButton");
            formationFitLabel =
                root.Q<Label>("formationFitLabel");

            formationFitSummary =
                root.Q<Label>("formationFitSummary");
        }

        private void RegisterEvents()
        {
            formationDropdown.RegisterValueChangedCallback(
                OnFormationChanged);

            startMatchButton.clicked +=
                OnStartMatch;

            resetLineupButton.clicked +=
                ResetLineup;
        }

        private void OnFormationChanged(ChangeEvent<string> evt)
        {
            currentFormation =
                ParseFormation(evt.newValue);

            selectedSlot = null;
            selectedStarter = null;

            BuildLineup();
        }

        private void ResetLineup()
        {
            selectedSlot = null;
            selectedStarter = null;

            BuildLineup();
        }

        private void BuildLineup()
        {
            slotViews.Clear();

            lineupContainer.Clear();

            dragHandlers.Clear();

            Lineup lineup =
                LineupBuilder.BuildRecommendedLineup(
                    matchSimulator.HomeTeam,
                    currentFormation);

            currentLineup =
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

                RegisterPlayerDrag(
                    slotView);

                PositionSlotButton(button,slot);

                lineupContainer.Add(button);

                slotViews.Add(slotView);
            }

            UpdatePlayerPanel();
            BuildBench();
            UpdateFormationLabel();
            UpdateFormationFit();
        }

        private void BuildBench()
        {
            if (benchContainer == null)
                return;

            benchContainer.Clear();

            if (currentLineup == null)
                return;

            foreach (Player player
                in matchSimulator.HomeTeam.GetFullSquad())
            {
                if (currentLineup.HasPlayer(player))
                    continue;

                Button button =
                    new();

                button.text =
                        $"{player.Name}   " +
                        $"{FormatPosition(player.Position)}   " +
                        $"FIT {player.Fitness}%";

                button.AddToClassList(
                    "bench-player");

                benchContainer.Add(button);

                RegisterBenchDrag(
                    button,
                    player);

                button.clicked += () =>
                {
                    if (selectedSlot == null)
                        return;

                    if (!LineupBuilder.CanPlayerPlaySlot(
                            player,
                            selectedSlot.Slot))
                    {
                        return;
                    }

                    AssignPlayer(
                        selectedSlot.Slot,
                        player);
                };
            }
        }

        private void SelectSlot(LineupSlotView slotView)
        {
            ClearSelection();

            selectedSlot = slotView;
            selectedStarter = slotView.Player;

            slotView.SetSelected(true);

            UpdatePlayerPanel();

            HighlightEligibleBenchPlayers();
        }

        private void ClearSelection()
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }

            selectedSlot = null;
            selectedStarter = null;

            foreach (Player player
                in matchSimulator.HomeTeam.Bench)
            {
                // Bench styling is refreshed below.
            }
        }

        private void UpdatePlayerPanel()
        {
            playerListContainer.Clear();

            if (selectedSlot == null ||
                selectedStarter == null)
            {
                Label prompt = new(
                    "Select a player on the pitch.");

                prompt.AddToClassList(
                    "player-list-placeholder");

                playerListContainer.Add(prompt);

                return;
            }

            Label selectedLabel = new(
                $"{selectedStarter.Name}\n" +
                $"{FormatPosition(selectedStarter.Position)}  •  " +
                $"{selectedStarter.Fitness}%");

            selectedLabel.AddToClassList(
                "selected-player-heading");

            playerListContainer.Add(
                selectedLabel);

            Label replacementLabel = new(
                "AVAILABLE REPLACEMENTS");

            replacementLabel.AddToClassList(
                "replacement-heading");

            playerListContainer.Add(
                replacementLabel);

            bool foundReplacement = false;

            foreach (Player player
                in matchSimulator.HomeTeam.Bench)
            {
                if (player.Position !=
                    selectedSlot.Slot.RequiredPosition)
                {
                    continue;
                }

                Button button =
                    CreateReplacementButton(player);

                playerListContainer.Add(button);

                foundReplacement = true;
            }

            if (!foundReplacement)
            {
                Label none = new(
                    "No suitable replacements available.");

                none.AddToClassList(
                    "player-list-placeholder");

                playerListContainer.Add(none);
            }
        }

        private Button CreateReplacementButton(Player player)
        {
            Button button = new();

            button.text =
                $"{player.Name}\n" +
                $"{FormatPosition(player.Position)}  •  " +
                $"{player.Fitness}%";

            button.AddToClassList(
                "player-selection-button");

            button.clicked += () =>
            {
                SubstituteSelectedPlayer(player);
            };

            return button;
        }

        private void SubstituteSelectedPlayer(Player replacement)
        {
            if (selectedSlot == null ||
                selectedStarter == null)
            {
                return;
            }

            Player oldStarter =
                selectedStarter;

            // Update the visible lineup.
            selectedSlot.SetPlayer(
                replacement);

            // Put old starter on the bench.
            ReplaceBenchPlayer(
                oldStarter,
                replacement);

            // Refresh everything.
            BuildBench();
            UpdatePlayerPanel();
            UpdateFormationFit();

            HighlightEligibleBenchPlayers();

            Debug.Log(
                $"Lineup change: " +
                $"{oldStarter.Name} -> " +
                $"{replacement.Name}");
        }

        private void ReplaceBenchPlayer(Player oldStarter, Player replacement)
        {
            Team team =
                matchSimulator.HomeTeam;

            int index =
                team.Bench.IndexOf(replacement);

            if (index < 0)
                return;

            team.Bench[index] =
                oldStarter;
        }

        private void HighlightEligibleBenchPlayers()
        {
            if (benchContainer == null)
                return;

            foreach (VisualElement element
                in benchContainer.Children())
            {
                element.RemoveFromClassList(
                    "eligible-bench-player");
            }

            if (selectedSlot == null)
                return;

            int index = 0;

            foreach (Player player
                in matchSimulator.HomeTeam.Bench)
            {
                if (index >=
                    benchContainer.childCount)
                {
                    break;
                }

                if (player.Position ==
                    selectedSlot.Slot.RequiredPosition)
                {
                    benchContainer[index]
                        .AddToClassList(
                            "eligible-bench-player");
                }

                index++;
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

        private void UpdateFormationFit()
        {
            Lineup lineup =
                LineupBuilder.BuildRecommendedLineup(
                    matchSimulator.HomeTeam,
                    currentFormation);

            FormationFitResult result =
                FormationCompatibility.Calculate(
                    lineup);

            formationFitLabel.text =
                $"{result.Score:F0}%";

            formationFitSummary.text =
                result.Summary;
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

        private void AssignPlayer( FormationSlot slot, Player player)
        {
            if (!LineupBuilder.CanPlayerPlaySlot(
                    player,
                    slot))
            {
                return;
            }

            foreach (LineupSlotView slotView
                in slotViews)
            {
                if (slotView.Player == player &&
                    slotView.Slot.Id != slot.Id)
                {
                    slotView.SetPlayer(null);
                }
            }

            currentLineup.Assign(
                slot,
                player);

            foreach (LineupSlotView slotView
                in slotViews)
            {
                if (slotView.Slot.Id == slot.Id)
                {
                    slotView.SetPlayer(player);
                    break;
                }
            }

            BuildBench();
            UpdateFormationFit();
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
            Lineup lineup =
                LineupBuilder.BuildFromSlotViews(
                    currentFormation,
                    slotViews);

            if (!lineup.IsComplete)
            {
                Debug.LogWarning(
                    "You must select 11 players before starting.");

                ShowLineupError(
                    "Select a player for every position.");

                return;
            }

            if (lineup.HasDuplicatePlayers())
            {
                Debug.LogWarning(
                    "The same player cannot occupy multiple positions.");

                ShowLineupError(
                    "A player can only occupy one position.");

                return;
            }

            matchSimulator.StartConfiguredMatch(
                currentFormation,
                slotViews);

            if (!matchSimulator.HasMatch)
            {
                ShowLineupError(
                    "Unable to start match.");

                return;
            }

            if (lineupScreen != null)
                lineupScreen.SetActive(false);

            if (matchScreen != null)
                matchScreen.SetActive(true);
        }

        private void ShowLineupError(string message)
        {
            Debug.LogWarning(message);
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

        private void RegisterBenchDrag(Button button, Player player)
        {
            PlayerDragHandler handler =
                new(
                    root,
                    button,
                    position =>
                    {
                        HandleBenchDrop(
                            player,
                            position);
                    });

            dragHandlers.Add(handler);
        }

        private void RegisterPlayerDrag( LineupSlotView slotView)
        {
            PlayerDragHandler handler =
                new(
                    root,
                    slotView.Button,
                    position =>
                    {
                        HandlePlayerDrop(
                            slotView,
                            position);
                    });

            dragHandlers.Add(handler);
        }

        private void HandleBenchDrop( Player player, Vector2 position)
        {
            LineupSlotView target =
                FindSlotAtPosition(position);

            if (target == null)
                return;

            if (!LineupBuilder.CanPlayerPlaySlot(
                    player,
                    target.Slot))
            {
                return;
            }

            AssignPlayer(
                target.Slot,
                player);
        }

        private void HandlePlayerDrop( LineupSlotView source, Vector2 position)
        {
            LineupSlotView target =
                FindSlotAtPosition(position);

            if (target == null ||
                target == source)
            {
                return;
            }

            if (source.Player == null ||
                target.Player == null)
            {
                return;
            }

            Player sourcePlayer =
                source.Player;

            Player targetPlayer =
                target.Player;

            if (!LineupBuilder.CanPlayerPlaySlot(
                    sourcePlayer,
                    target.Slot))
            {
                return;
            }

            if (!LineupBuilder.CanPlayerPlaySlot(
                    targetPlayer,
                    source.Slot))
            {
                return;
            }

            source.SetPlayer(targetPlayer);
            target.SetPlayer(sourcePlayer);

            currentLineup.Assign(
                source.Slot,
                targetPlayer);

            currentLineup.Assign(
                target.Slot,
                sourcePlayer);

            BuildBench();

            UpdateFormationFit();
        }

        private LineupSlotView FindSlotAtPosition( Vector2 position)
        {
            foreach (LineupSlotView slotView
                in slotViews)
            {
                Rect worldRect =
                    slotView.Button.worldBound;

                if (worldRect.Contains(position))
                {
                    return slotView;
                }
            }

            return null;
        }
    }
}