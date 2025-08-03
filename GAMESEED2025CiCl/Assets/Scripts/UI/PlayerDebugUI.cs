using UnityEngine;
using System.Text;

public class PlayerDebugUI : MonoBehaviour
{
    public PlayerStats playerStats;
    private bool showDebugUI = true;

    private StringBuilder sb = new StringBuilder();

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats != null)
        {
            PlayerStats.OnStatsChanged += UpdateDebugText;
            UpdateDebugText();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerStats != null)
        {
            PlayerStats.OnStatsChanged -= UpdateDebugText;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            showDebugUI = !showDebugUI;
        }
    }

    void UpdateDebugText()
    {
        if (playerStats == null) return;

        sb.Clear();
        sb.AppendLine("--- PLAYER STATS (F5 to toggle) ---");
        sb.AppendLine($"Level: {playerStats.level}");
        sb.AppendLine($"EXP: {playerStats.currentExperience} / {playerStats.maxExperience}");
        sb.AppendLine($"Money: {playerStats.money}");
        sb.AppendLine($"Weapon Slots: {playerStats.unlockedHoregSlots}");
        sb.AppendLine("--------------------");
        sb.AppendLine($"Damage Multiplier: {playerStats.damageMultiplier:F2}");
        sb.AppendLine($"Cooldown Reduction: {playerStats.cooldownReduction:P0}"); // Show as percentage
        sb.AppendLine($"Area of Effect Bonus: {playerStats.areaOfEffectBonus:F2}");
        sb.AppendLine($"Beat Accuracy Bonus: {playerStats.beatAccuracyBonus:F2}");
        sb.AppendLine($"Max Health Bonus: {playerStats.maxHealthBonus}");
        sb.AppendLine("--------------------");
        sb.AppendLine($"Move Speed Multiplier: {playerStats.moveSpeedMultiplier:F2}");
    }

    void OnGUI()
    {
        if (showDebugUI && playerStats != null)
        {
            // Define the box style
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 14;
            boxStyle.normal.textColor = Color.white;

            // Draw the debug box
            GUI.Box(new Rect(10, 10, 300, 250), sb.ToString(), boxStyle);
        }
    }
}
