using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MetaPreviewUI : MonoBehaviour
{
    public Text soundChipText;
    public Text characterText;
    public Text unlockedHoregsText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            soundChipText.text = "SoundChips: " + GameManager.Instance.soundChips;
            characterText.text = "Character: " + GameManager.Instance.lastCharacterUsed;
            unlockedHoregsText.text = "Unlocked Horegs: " + string.Join(", ", GameManager.Instance.unlockedHoregs);
        }
        else
        {
            soundChipText.text = "SoundChips: N/A";
            characterText.text = "Character: N/A";
            unlockedHoregsText.text = "Unlocked Horegs: N/A";
        }
    }
}
