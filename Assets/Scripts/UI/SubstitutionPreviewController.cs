using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    /// <summary>
    /// Adds a persistent, visual substitution preview to the existing match UI.
    /// This intentionally sits beside MatchScreenController so the preview can
    /// be improved without duplicating or replacing the match simulation logic.
    /// </summary>
    public sealed class SubstitutionPreviewController : MonoBehaviour
    {
        private UIDocument document;
        private VisualElement root;
        private VisualElement substitutionOverlay;
        private ScrollView playerOffList;
        private ScrollView playerOnList;
        private Button confirmButton;

        private VisualElement preview;
        private Label stateLabel;
        private Label offLabel;
        private Label onLabel;

        private string selectedOff;
        private string selectedOn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            foreach (UIDocument uiDocument in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                if (uiDocument.rootVisualElement.Q<VisualElement>("substitutionOverlay") == null)
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

            if (substitutionOverlay == null ||
                playerOffList == null ||
                playerOnList == null)
                return;

            BuildPreview();

            root.RegisterCallback<ClickEvent>(OnRootClick, TrickleDown.TrickleDown);

            root.schedule.Execute(RefreshButtonHooks).Every(100);
            root.schedule.Execute(SyncVisibility).Every(100);
        }

        private void BuildPreview()
        {
            preview = new VisualElement();
            preview.name = "substitutionPreview";
            preview.AddToClassList("substitution-preview");

            preview.style.marginTop = 12;
            preview.style.marginBottom = 12;
            preview.style.paddingTop = 12;
            preview.style.paddingBottom = 12;
            preview.style.paddingLeft = 16;
            preview.style.paddingRight = 16;
            preview.style.borderTopWidth = 1;
            preview.style.borderBottomWidth = 1;
            preview.style.borderLeftWidth = 1;
            preview.style.borderRightWidth = 1;
            preview.style.borderTopLeftRadius = 8;
            preview.style.borderTopRightRadius = 8;
            preview.style.borderBottomLeftRadius = 8;
            preview.style.borderBottomRightRadius = 8;

            Label title = new Label("SUBSTITUTION PREVIEW");
            title.AddToClassList("substitution-preview-title");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16;
            preview.Add(title);

            stateLabel = new Label("Select a player to come OFF and a player to come ON.");
            stateLabel.style.marginTop = 6;
            stateLabel.style.marginBottom = 8;
            preview.Add(stateLabel);

            offLabel = new Label("OFF  —  No player selected");
            offLabel.style.marginBottom = 4;
            preview.Add(offLabel);

            onLabel = new Label("ON   —  No player selected");
            preview.Add(onLabel);

            // Put the preview immediately before the action buttons so the
            // player can see the final decision immediately before confirming.
            VisualElement buttonRow = confirmButton?.parent;
            if (buttonRow != null && buttonRow.parent != null)
                buttonRow.parent.Insert(buttonRow.parent.IndexOf(buttonRow), preview);
            else
                substitutionOverlay.Add(preview);
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
                selectedOff = button.text;
                UpdatePreview();
            }
            else if (IsInside(button, playerOnList))
            {
                selectedOn = button.text;
                UpdatePreview();
            }
        }

        private void RefreshButtonHooks()
        {
            // Player buttons are dynamically recreated by MatchScreenController.
            // The root click listener therefore intentionally handles them via
            // event delegation instead of storing references to stale buttons.
            UpdatePreview();
        }

        private void SyncVisibility()
        {
            if (substitutionOverlay == null || preview == null)
                return;

            preview.style.display =
                substitutionOverlay.resolvedStyle.display == DisplayStyle.None
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }

        private void UpdatePreview()
        {
            if (preview == null)
                return;

            offLabel.text = string.IsNullOrEmpty(selectedOff)
                ? "OFF  —  No player selected"
                : "OFF  →  " + selectedOff;

            onLabel.text = string.IsNullOrEmpty(selectedOn)
                ? "ON   —  No player selected"
                : "ON   →  " + selectedOn;

            bool complete =
                !string.IsNullOrEmpty(selectedOff) &&
                !string.IsNullOrEmpty(selectedOn);

            stateLabel.text = complete
                ? "READY TO CONFIRM — review the player coming OFF and ON."
                : string.IsNullOrEmpty(selectedOff)
                    ? "Select a player to come OFF."
                    : "Now select the replacement coming ON.";

            if (confirmButton != null)
                confirmButton.SetEnabled(complete);
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
            if (element == null || container == null)
                return false;

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
