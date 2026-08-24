using FootballTactics.Simulation;
using FootballTactics.Teams;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballTactics.UI
{
    public sealed class SubstitutionPreviewFix : MonoBehaviour
    {
        private UIDocument document;
        private Label preview;
        private Button confirmButton;
        private MatchSimulator simulator;
        private string lastRawText;

        private void Awake()
        {
            document = GetComponent<UIDocument>();
            simulator = GetComponent<MatchSimulator>();

            if (simulator == null)
                simulator = FindFirstObjectByType<MatchSimulator>();
        }

        private void OnEnable() => Refresh();

        private void Update()
        {
            if (document == null || document.rootVisualElement == null)
                return;

            if (preview == null || confirmButton == null)
                FindControls();

            if (preview == null)
                return;

            if (preview.text != lastRawText)
                Refresh();
        }

        private void FindControls()
        {
            VisualElement root = document.rootVisualElement;
            preview = root.Q<Label>("selectedSubstitutionLabel");
            confirmButton = root.Q<Button>("confirmSubstitutionButton");

            if (preview != null)
            {
                preview.style.whiteSpace = WhiteSpace.Normal;
                preview.style.unityTextAlign = TextAnchor.MiddleCenter;
                preview.style.minHeight = 86;
                preview.style.marginTop = 8;
                preview.style.marginBottom = 8;
            }

            if (confirmButton != null)
            {
                confirmButton.text = "CONFIRM";
                confirmButton.style.minWidth = 150;
                confirmButton.style.width = 170;
                confirmButton.style.height = 42;
                confirmButton.style.flexShrink = 0;
                confirmButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }

        private void Refresh()
        {
            FindControls();
            if (preview == null)
                return;

            string raw = preview.text ?? string.Empty;
            lastRawText = raw;

            if (simulator == null || !simulator.HasMatch || simulator.Engine == null)
                return;

            MatchEngine engine = simulator.Engine;
            int arrow = raw.IndexOf('→');
            if (arrow < 0)
                return;

            string offName = raw.Substring(0, arrow).Trim().TrimEnd('-').Trim();
            string onName = raw.Substring(arrow + 1).Trim().TrimStart('-').Trim();

            Player off = FindPlayer(engine, offName);
            Player on = FindPlayer(engine, onName);

            if (off == null && on == null)
                return;

            preview.text =
                "OFF\n" + FormatPlayer(off, "No player selected") +
                "\n\n↓\n\n" +
                "ON\n" + FormatPlayer(on, "No player selected") +
                "\n\n" +
                (off != null && on != null ? "READY TO CONFIRM" : "Select both players");

            lastRawText = preview.text;
        }

        private static Player FindPlayer(MatchEngine engine, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "-")
                return null;

            foreach (Player player in engine.HomeLineup.Assignments.Values)
                if (player != null && player.Name == name)
                    return player;

            foreach (Player player in engine.HomeTeam.Bench)
                if (player != null && player.Name == name)
                    return player;

            return null;
        }

        private static string FormatPlayer(Player player, string fallback)
        {
            if (player == null)
                return fallback;

            return $"{player.Name}  •  {FormatRole(player.Role)}  •  {player.Fitness}% fitness";
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            UIDocument[] documents = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

            foreach (UIDocument doc in documents)
            {
                if (doc.GetComponent<SubstitutionPreviewFix>() == null)
                    doc.gameObject.AddComponent<SubstitutionPreviewFix>();
            }
        }
    }
}
