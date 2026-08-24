using System.Collections.Generic;
using FootballTactics.Simulation;
using FootballTactics.Teams;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    /// <summary>
    /// Adds a clearer substitution presentation without changing the existing
    /// substitution state machine. Installed automatically at runtime so no
    /// scene setup is required.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class SubstitutionOverlayEnhancer : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement root;
        private MatchSimulator simulator;

        private ScrollView offList;
        private ScrollView onList;
        private Label previewLabel;
        private Button confirmButton;

        private Player selectedOff;
        private Player selectedOn;

        private readonly HashSet<Button> hookedButtons = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            MatchScreenController controller =
                Object.FindFirstObjectByType<MatchScreenController>();

            if (controller == null)
                return;

            if (!controller.TryGetComponent<SubstitutionOverlayEnhancer>(out _))
                controller.gameObject.AddComponent<SubstitutionOverlayEnhancer>();
        }

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            simulator = FindFirstObjectByType<MatchSimulator>();
        }

        private void Update()
        {
            if (document == null)
                return;

            root ??= document.rootVisualElement;
            simulator ??= FindFirstObjectByType<MatchSimulator>();

            if (root == null || simulator?.Engine == null)
                return;

            offList = root.Q<ScrollView>("playerOffList");
            onList = root.Q<ScrollView>("playerOnList");
            previewLabel = root.Q<Label>("selectedSubstitutionLabel");
            confirmButton = root.Q<Button>("confirmSubstitutionButton");

            if (offList == null || onList == null)
                return;

            RefreshPlayerButtons(offList, true);
            RefreshPlayerButtons(onList, false);
            UpdatePreview();
        }

        private void RefreshPlayerButtons(ScrollView list, bool off)
        {
            foreach (VisualElement child in list.Children())
            {
                if (child is not Button button)
                    continue;

                Player player = FindPlayer(button, off);
                if (player == null)
                    continue;

                button.text = BuildPlayerText(player, off);
                button.AddToClassList(off ? "sub-player-off" : "sub-player-on");

                if (hookedButtons.Add(button))
                {
                    Player captured = player;
                    button.clicked += () =>
                    {
                        if (off)
                            selectedOff = captured;
                        else
                            selectedOn = captured;

                        UpdatePreview();
                    };
                }

                bool selected = off
                    ? selectedOff == player
                    : selectedOn == player;

                if (selected)
                    button.AddToClassList("selected-player");
                else
                    button.RemoveFromClassList("selected-player");
            }
        }

        private Player FindPlayer(Button button, bool off)
        {
            string rawName = button.text;
            int separator = rawName.IndexOf("  • ");
            if (separator >= 0)
                rawName = rawName.Substring(0, separator);

            MatchEngine engine = simulator.Engine;

            if (off)
            {
                foreach (Player player in engine.HomeLineup.Assignments.Values)
                {
                    if (player.Name == rawName)
                        return player;
                }
            }
            else
            {
                foreach (Player player in engine.HomeTeam.Bench)
                {
                    if (player.Name == rawName)
                        return player;
                }
            }

            return null;
        }

        private void UpdatePreview()
        {
            if (previewLabel == null)
                return;

            string offText = selectedOff == null
                ? "— Select player —"
                : $"{selectedOff.Name}  [{FormatRole(selectedOff)}]";

            string onText = selectedOn == null
                ? "— Select player —"
                : $"{selectedOn.Name}  [{FormatRole(selectedOn)}]";

            bool valid = IsValidSubstitution();

            previewLabel.text =
                "SUBSTITUTION PREVIEW\n" +
                $"OFF   {offText}\n" +
                $"ON    {onText}" +
                (selectedOff != null && selectedOn != null
                    ? $"\n\n{(valid ? "✓ READY TO CONFIRM" : "✕ PLAYERS NOT COMPATIBLE")}" 
                    : "\n\nSelect both players before confirming.");

            previewLabel.style.whiteSpace = WhiteSpace.Normal;
            previewLabel.style.fontSize = 13;
            previewLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewLabel.style.paddingLeft = 12;
            previewLabel.style.paddingRight = 12;
            previewLabel.style.paddingTop = 10;
            previewLabel.style.paddingBottom = 10;
            previewLabel.style.marginTop = 12;
            previewLabel.style.marginBottom = 10;
            previewLabel.style.backgroundColor = new Color(0.11f, 0.12f, 0.15f, 1f);
            previewLabel.style.borderTopWidth = 1;
            previewLabel.style.borderBottomWidth = 1;
            previewLabel.style.borderLeftWidth = 1;
            previewLabel.style.borderRightWidth = 1;

            if (confirmButton != null)
                confirmButton.SetEnabled(valid);
        }

        private bool IsValidSubstitution()
        {
            if (selectedOff == null || selectedOn == null || simulator?.Engine == null)
                return false;

            FormationSlot slot = GetSlotForPlayer(
                simulator.Engine.HomeLineup,
                selectedOff);

            return slot != null &&
                   LineupBuilder.CanPlayerPlaySlot(selectedOn, slot);
        }

        private static FormationSlot GetSlotForPlayer(Lineup lineup, Player player)
        {
            foreach (var assignment in lineup.Assignments)
            {
                if (assignment.Value != player)
                    continue;

                foreach (FormationSlot slot in lineup.Formation.GetDefinition().Slots)
                {
                    if (slot.Id == assignment.Key)
                        return slot;
                }
            }

            return null;
        }

        private static string BuildPlayerText(Player player, bool off)
        {
            return $"{player.Name}\n" +
                   $"{FormatRole(player)}  •  " +
                   $"{FormatPosition(player.Position)}  •  " +
                   $"FIT {player.Fitness}%";
        }

        private static string FormatRole(Player player)
        {
            return player.Role.ToString() switch
            {
                "Goalkeeper" => "Goalkeeper",
                "Sweeper" => "Sweeper",
                "LineHolding" => "Line Holding",
                "CentreBack" => "Centre Back",
                "FullBack" => "Full Back",
                "CentralMidfielder" => "Central Midfielder",
                "Playmaker" => "Playmaker",
                "DefensiveMidfielder" => "Defensive Midfielder",
                "BoxToBox" => "Box-to-Box",
                "Striker" => "Striker",
                "Winger" => "Winger",
                _ => player.Role.ToString()
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
                _ => "?"
            };
        }
    }
}
