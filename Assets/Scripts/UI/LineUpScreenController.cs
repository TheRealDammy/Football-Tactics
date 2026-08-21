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
        [SerializeField] private MatchSimulator matchSimulator;
        [SerializeField] private GameObject lineupScreen;
        [SerializeField] private GameObject matchScreen;

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
        private Lineup currentLineup;
        private LineupSlotView selectedSlot;
        private Player selectedStarter;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
            FindElements();
            RegisterEvents();
        }

        private void Start()
        {
            currentFormation = Formation.FourThreeThree;
            BuildLineup();
        }

        private void FindElements()
        {
            formationDropdown = root.Q<DropdownField>("formationDropdown");
            lineupContainer = root.Q<VisualElement>("lineupContainer");
            playerListContainer = root.Q<VisualElement>("playerListContainer");
            benchContainer = root.Q<VisualElement>("benchContainer");
            startMatchButton = root.Q<Button>("startMatchButton");
            resetLineupButton = root.Q<Button>("resetLineupButton");
            formationFitLabel = root.Q<Label>("formationFitLabel");
            formationFitSummary = root.Q<Label>("formationFitSummary");
        }

        private void RegisterEvents()
        {
            formationDropdown?.RegisterValueChangedCallback(OnFormationChanged);

            if (startMatchButton != null)
                startMatchButton.clicked += OnStartMatch;

            if (resetLineupButton != null)
                resetLineupButton.clicked += ResetLineup;
        }

        private void OnFormationChanged(ChangeEvent<string> evt)
        {
            currentFormation = ParseFormation(evt.newValue);
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
            if (matchSimulator == null || matchSimulator.HomeTeam == null)
                return;

            slotViews.Clear();
            dragHandlers.Clear();
            lineupContainer?.Clear();

            currentLineup = LineupBuilder.BuildRecommendedLineup(
                matchSimulator.HomeTeam,
                currentFormation);

            FormationDefinition definition = currentFormation.GetDefinition();

            foreach (FormationSlot slot in definition.Slots)
            {
                Button button = new();
                button.AddToClassList("lineup-slot");

                LineupSlotView slotView = new(slot, button);
                slotView.SetPlayer(currentLineup.GetPlayer(slot.Id));

                button.clicked += () => SelectSlot(slotView);

                RegisterPlayerDrag(slotView);
                PositionSlotButton(button, slot);

                lineupContainer?.Add(button);
                slotViews.Add(slotView);
            }

            RefreshCurrentLineup();
            RefreshAllUI();
        }

        private void RefreshCurrentLineup()
        {
            currentLineup = LineupBuilder.BuildFromSlotViews(
                currentFormation,
                slotViews);
        }

        private void RefreshAllUI()
        {
            UpdatePlayerPanel();
            BuildBench();
            UpdateFormationLabel();
            UpdateFormationFit();
            HighlightEligibleBenchPlayers();
        }

        private void BuildBench()
        {
            if (benchContainer == null || currentLineup == null ||
                matchSimulator?.HomeTeam == null)
                return;

            benchContainer.Clear();

            foreach (Player player in matchSimulator.HomeTeam.GetFullSquad())
            {
                if (currentLineup.HasPlayer(player))
                    continue;

                Player capturedPlayer = player;
                Button button = new();

                button.text =
                    $"{player.Name}   " +
                    $"{FormatPosition(player.Position)}   " +
                    $"{FormatRole(player.Role)}   " +
                    $"FIT {player.Fitness}%";

                button.AddToClassList("bench-player");

                button.clicked += () =>
                {
                    if (selectedSlot == null ||
                        !LineupBuilder.CanPlayerPlaySlot(
                            capturedPlayer,
                            selectedSlot.Slot))
                        return;

                    AssignPlayer(selectedSlot.Slot, capturedPlayer);
                };

                RegisterBenchDrag(button, capturedPlayer);
                benchContainer.Add(button);
            }
        }

        private void SelectSlot(LineupSlotView slotView)
        {
            ClearSelection();
            selectedSlot = slotView;
            selectedStarter = slotView.Player;
            selectedSlot.SetSelected(true);
            UpdatePlayerPanel();
            HighlightEligibleBenchPlayers();
        }

        private void ClearSelection()
        {
            selectedSlot?.SetSelected(false);
            selectedSlot = null;
            selectedStarter = null;
        }

        private void UpdatePlayerPanel()
        {
            if (playerListContainer == null)
                return;

            playerListContainer.Clear();

            if (selectedSlot == null)
            {
                Label prompt = new("Select a position on the pitch.");
                prompt.AddToClassList("player-list-placeholder");
                playerListContainer.Add(prompt);
                return;
            }

            string starterText = selectedStarter == null
                ? $"{selectedSlot.Slot.Id}\nEMPTY"
                : $"{selectedStarter.Name}\n" +
                  $"{FormatRole(selectedStarter.Role)}  •  " +
                  $"{selectedStarter.Fitness}%";

            Label selectedLabel = new(starterText);
            selectedLabel.AddToClassList("selected-player-heading");
            playerListContainer.Add(selectedLabel);

            Label replacementLabel = new("AVAILABLE REPLACEMENTS");
            replacementLabel.AddToClassList("replacement-heading");
            playerListContainer.Add(replacementLabel);

            bool foundReplacement = false;

            foreach (Player player in matchSimulator.HomeTeam.GetFullSquad())
            {
                if (currentLineup.HasPlayer(player))
                    continue;

                if (!LineupBuilder.CanPlayerPlaySlot(player, selectedSlot.Slot))
                    continue;

                playerListContainer.Add(CreateReplacementButton(player));
                foundReplacement = true;
            }

            if (!foundReplacement)
            {
                Label none = new("No suitable replacements available.");
                none.AddToClassList("player-list-placeholder");
                playerListContainer.Add(none);
            }
        }

        private Button CreateReplacementButton(Player player)
        {
            Button button = new();
            button.text =
                $"{player.Name}\n" +
                $"{FormatRole(player.Role)}  •  {player.Fitness}%";

            button.AddToClassList("player-selection-button");
            button.clicked += () => SubstituteSelectedPlayer(player);
            return button;
        }

        private void SubstituteSelectedPlayer(Player replacement)
        {
            if (selectedSlot == null ||
                !LineupBuilder.CanPlayerPlaySlot(replacement, selectedSlot.Slot))
                return;

            AssignPlayer(selectedSlot.Slot, replacement);
            selectedStarter = replacement;
        }

        private void HighlightEligibleBenchPlayers()
        {
            if (benchContainer == null || selectedSlot == null)
                return;

            foreach (VisualElement element in benchContainer.Children())
                element.RemoveFromClassList("eligible-bench-player");

            foreach (VisualElement element in benchContainer.Children())
            {
                if (element is not Button button)
                    continue;

                Player player = FindBenchPlayer(button);
                if (player != null &&
                    LineupBuilder.CanPlayerPlaySlot(player, selectedSlot.Slot))
                {
                    button.AddToClassList("eligible-bench-player");
                }
            }
        }

        private Player FindBenchPlayer(Button button)
        {
            if (matchSimulator?.HomeTeam == null || currentLineup == null)
                return null;

            foreach (Player player in matchSimulator.HomeTeam.GetFullSquad())
            {
                if (currentLineup.HasPlayer(player))
                    continue;

                string text =
                    $"{player.Name}   " +
                    $"{FormatPosition(player.Position)}   " +
                    $"{FormatRole(player.Role)}   " +
                    $"FIT {player.Fitness}%";

                if (button.text == text)
                    return player;
            }

            return null;
        }

        private void AssignPlayer(FormationSlot slot, Player player)
        {
            if (slot == null || player == null ||
                !LineupBuilder.CanPlayerPlaySlot(player, slot))
                return;

            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Slot.Id != slot.Id && slotView.Player == player)
                    slotView.SetPlayer(null);
            }

            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Slot.Id == slot.Id)
                {
                    slotView.SetPlayer(player);
                    break;
                }
            }

            RefreshCurrentLineup();
            RefreshAllUI();
        }

        private void RemovePlayerFromSlot(LineupSlotView slotView)
        {
            if (slotView == null)
                return;

            slotView.SetPlayer(null);

            if (selectedSlot == slotView)
                selectedStarter = null;

            RefreshCurrentLineup();
            RefreshAllUI();
        }

        private void SwapPlayers(LineupSlotView source, LineupSlotView target)
        {
            if (source == null || target == null || source == target ||
                source.Player == null)
                return;

            Player sourcePlayer = source.Player;
            Player targetPlayer = target.Player;

            if (!LineupBuilder.CanPlayerPlaySlot(sourcePlayer, target.Slot))
                return;

            if (targetPlayer != null &&
                !LineupBuilder.CanPlayerPlaySlot(targetPlayer, source.Slot))
                return;

            source.SetPlayer(targetPlayer);
            target.SetPlayer(sourcePlayer);

            RefreshCurrentLineup();
            RefreshAllUI();
        }

        private void PositionSlotButton(Button button, FormationSlot slot)
        {
            button.style.position = Position.Absolute;
            button.style.left = Length.Percent(slot.X);
            button.style.top = Length.Percent(slot.Y);
            button.style.translate = new Translate(
                Length.Percent(-50),
                Length.Percent(-50));
        }

        private void UpdateFormationFit()
        {
            if (currentLineup == null)
                return;

            FormationFitResult result =
                FormationCompatibility.Calculate(currentLineup);

            if (formationFitLabel != null)
                formationFitLabel.text = $"{result.Score:F0}%";

            if (formationFitSummary != null)
                formationFitSummary.text = result.Summary;
        }

        private void UpdateFormationLabel()
        {
            Label label = root.Q<Label>("formationSummaryLabel");
            if (label != null)
                label.text = $"FORMATION  {FormatFormation(currentFormation)}";
        }

        private void OnStartMatch()
        {
            RefreshCurrentLineup();

            if (!currentLineup.IsComplete)
            {
                ShowLineupError("Select a player for every position.");
                return;
            }

            if (currentLineup.HasDuplicatePlayers())
            {
                ShowLineupError("A player can only occupy one position.");
                return;
            }

            matchSimulator.StartConfiguredMatch(currentFormation, slotViews);

            if (!matchSimulator.HasMatch)
            {
                ShowLineupError("Unable to start match.");
                return;
            }

            lineupScreen?.SetActive(false);
            matchScreen?.SetActive(true);
        }

        private void ShowLineupError(string message)
        {
            Debug.LogWarning(message);
        }

        private void RegisterBenchDrag(Button button, Player player)
        {
            dragHandlers.Add(new PlayerDragHandler(
                root,
                button,
                position => HandleBenchDrop(player, position)));
        }

        private void RegisterPlayerDrag(LineupSlotView slotView)
        {
            dragHandlers.Add(new PlayerDragHandler(
                root,
                slotView.Button,
                position => HandlePlayerDrop(slotView, position)));
        }

        private void HandleBenchDrop(Player player, Vector2 position)
        {
            LineupSlotView target = FindSlotAtPosition(position);
            if (target == null ||
                !LineupBuilder.CanPlayerPlaySlot(player, target.Slot))
                return;

            AssignPlayer(target.Slot, player);
        }

        private void HandlePlayerDrop(LineupSlotView source, Vector2 position)
        {
            if (benchContainer != null &&
                benchContainer.worldBound.Contains(position))
            {
                RemovePlayerFromSlot(source);
                return;
            }

            LineupSlotView target = FindSlotAtPosition(position);
            if (target == null)
                return;

            SwapPlayers(source, target);
        }

        private LineupSlotView FindSlotAtPosition(Vector2 position)
        {
            foreach (LineupSlotView slotView in slotViews)
            {
                if (slotView.Button.worldBound.Contains(position))
                    return slotView;
            }

            return null;
        }

        private static Formation ParseFormation(string value)
        {
            return value switch
            {
                "4-4-2" => Formation.FourFourTwo,
                "4-2-3-1" => Formation.FourTwoThreeOne,
                _ => Formation.FourThreeThree
            };
        }

        private static string FormatFormation(Formation formation)
        {
            return formation switch
            {
                Formation.FourFourTwo => "4-4-2",
                Formation.FourTwoThreeOne => "4-2-3-1",
                _ => "4-3-3"
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

        private static string FormatRole(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Goalkeeper => "GK",
                PlayerRole.Sweeper => "SW",
                PlayerRole.LineHolding => "LH",
                PlayerRole.CentreBack => "CB",
                PlayerRole.FullBack => "FB",
                PlayerRole.CentralMidfielder => "CM",
                PlayerRole.Playmaker => "PM",
                PlayerRole.DefensiveMidfielder => "DM",
                PlayerRole.BoxToBox => "B2B",
                PlayerRole.Striker => "ST",
                PlayerRole.Winger => "WG",
                _ => role.ToString()
            };
        }
    }
}