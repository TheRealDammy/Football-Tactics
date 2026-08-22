using System;
using FootballTactics.Simulation;
using FootballTactics.Teams;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    public sealed class SubstitutionPreviewController : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement root;
        private VisualElement substitutionOverlay;
        private ScrollView playerOffList;
        private ScrollView playerOnList;
        private Button confirmButton;
        private Label stateLabel;
        private Label offName;
        private Label offRole;
        private Label onName;
        private Label onRole;
        private string selectedOff;
        private string selectedOn;
        private bool wasVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            foreach (UIDocument uiDocument in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (uiDocument.rootVisualElement?.Q<VisualElement>("substitutionOverlay") == null)
                    continue;

                if (uiDocument.GetComponent<SubstitutionPreviewController>() == null)
                    uiDocument.gameObject.AddComponent<SubstitutionPreviewController>();
            }
        }

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
            substitutionOverlay = root.Q<VisualElement>("substitutionOverlay");
            playerOffList = root.Q<ScrollView>("playerOffList");
            playerOnList = root.Q<ScrollView>("playerOnList");
            confirmButton = root.Q<Button>("confirmSubstitutionButton");
            stateLabel = root.Q<Label>("selectedSubstitutionLabel");
            offName = root.Q<Label>("previewOffName");
            offRole = root.Q<Label>("previewOffRole");
            onName = root.Q<Label>("previewOnName");
            onRole = root.Q<Label>("previewOnRole");

            if (substitutionOverlay == null || playerOffList == null || playerOnList == null)
                return;

            root.RegisterCallback<ClickEvent>(OnRootClick, TrickleDown.TrickleDown);
            root.schedule.Execute(Refresh).Every(100);
        }

        private void OnRootClick(ClickEvent evt)
        {
            if (evt.target is not VisualElement clicked)
                return;

            Button button = FindButton(clicked);
            if (button == null)
                return;

            if (IsInside(button, playerOffList))
            {
                selectedOff = ExtractPlayerName(button.text);
                selectedOn = null;
                UpdatePreview();
            }
            else if (IsInside(button, playerOnList))
            {
                selectedOn = ExtractPlayerName(button.text);
                UpdatePreview();
            }
        }

        private void Refresh()
        {
            if (substitutionOverlay == null)
                return;

            bool visible = substitutionOverlay.resolvedStyle.display != DisplayStyle.None;
            if (visible && !wasVisible)
            {
                selectedOff = null;
                selectedOn = null;
            }

            wasVisible = visible;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Player off = FindHomePlayer(selectedOff);
            Player on = FindHomePlayer(selectedOn);

            if (offName != null)
                offName.text = off != null ? off.Name : "No player selected";
            if (onName != null)
                onName.text = on != null ? on.Name : "No player selected";
            if (offRole != null)
                offRole.text = off != null ? FormatPlayerInfo(off) : "";
            if (onRole != null)
                onRole.text = on != null ? FormatPlayerInfo(on) : "";

            bool complete = off != null && on != null;
            if (stateLabel != null)
            {
                stateLabel.text = complete
                    ? "READY TO CONFIRM — review the player coming OFF and ON."
                    : off != null
                        ? "Now select the replacement coming ON."
                        : "Select a player to come OFF and a player to come ON.";
            }

            if (confirmButton != null)
                confirmButton.SetEnabled(complete);
        }

        private static Player FindHomePlayer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            MatchSimulator simulator = FindFirstObjectByType<MatchSimulator>();
            if (simulator?.Engine?.HomeTeam == null)
                return null;

            foreach (Player player in simulator.Engine.HomeTeam.Players)
                if (player.Name == name)
                    return player;

            return null;
        }

        private static string FormatPlayerInfo(Player player)
        {
            return $"{FormatRole(player.Role)}  •  {player.Fitness}% fitness";
        }

        private static string FormatRole(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.Goalkeeper => "Goalkeeper",
                PlayerRole.Sweeper => "Sweeper",
                PlayerRole.LineHolding => "Line Holding",
                PlayerRole.CentreBack => "Centre Back",
                PlayerRole.FullBack => "Full Back",
                PlayerRole.CentralMidfielder => "Central Midfielder",
                PlayerRole.Playmaker => "Playmaker",
                PlayerRole.DefensiveMidfielder => "Defensive Midfielder",
                PlayerRole.BoxToBox => "Box-to-Box",
                PlayerRole.Striker => "Striker",
                PlayerRole.Winger => "Winger",
                _ => role.ToString()
            };
        }

        private static string ExtractPlayerName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            int separator = text.IndexOf("  •", StringComparison.Ordinal);
            return separator >= 0 ? text.Substring(0, separator).Trim() : text.Trim();
        }

        private static Button FindButton(VisualElement element)
        {
            VisualElement current = element;
            while (current != null)
            {
                if (current is Button button)
                    return button;
                current = current.parent;
            }
            return null;
        }

        private static bool IsInside(VisualElement element, VisualElement container)
        {
            VisualElement current = element;
            while (current != null)
            {
                if (current == container)
                    return true;
                current = current.parent;
            }
            return false;
        }
    }
}
