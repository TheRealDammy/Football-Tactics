using System;
using System.Collections.Generic;
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

        private readonly HashSet<Button> hookedButtons = new();
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
            if (document == null)
                return;

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
            {
                Debug.LogWarning("SubstitutionPreviewController: substitution elements not found.");
                return;
            }

            root.schedule.Execute(Refresh).Every(100);
            ResetPreview();
        }

        private void Refresh()
        {
            if (substitutionOverlay == null)
                return;

            bool visible = substitutionOverlay.resolvedStyle.display != DisplayStyle.None;

            if (visible && !wasVisible)
                ResetPreview();

            wasVisible = visible;

            HookPlayerButtons(playerOffList, true);
            HookPlayerButtons(playerOnList, false);

            UpdatePreview();
        }

        private void HookPlayerButtons(ScrollView list, bool isOffList)
        {
            if (list == null)
                return;

            list.Query<Button>().ForEach(button =>
            {
                if (hookedButtons.Contains(button))
                    return;

                hookedButtons.Add(button);
                button.clicked += () =>
                {
                    if (isOffList)
                    {
                        selectedOff = ExtractPlayerName(button.text);
                        selectedOn = null;
                    }
                    else
                    {
                        selectedOn = ExtractPlayerName(button.text);
                    }

                    UpdatePreview();
                };
            });
        }

        private void ResetPreview()
        {
            selectedOff = null;
            selectedOn = null;
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
            {
                if (string.Equals(player.Name, name, StringComparison.Ordinal))
                    return player;
            }

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

            string[] parts = text.Split('•');
            return parts.Length > 0 ? parts[0].Trim() : text.Trim();
        }
    }
}
