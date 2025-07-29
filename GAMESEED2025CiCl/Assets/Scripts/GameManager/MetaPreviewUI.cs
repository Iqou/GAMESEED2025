using UnityEngine;
using UnityEngine.UI;

public class MetaPreviewUI : MonoBehaviour
{
    public Text levelText;
    public Text coinText;
    public Text characterText;
    public Text unlockedText;

    void Start()
    {
        levelText.text = "Level Unlocked: " + GameManager.Instance.levelUnlocked;
        coinText.text = "Coins: " + GameManager.Instance.totalCoins;
        characterText.text = "Character: " + GameManager.Instance.lastCharacterUsed;
        unlockedText.text = "Unlocked: " + string.Join(", ", GameManager.Instance.unlockedItems);
    }
}
