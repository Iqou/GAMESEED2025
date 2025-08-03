using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TapToBegin : MonoBehaviour
{
    // Tambahkan referensi ke MainMenuManager
    public MainMenuManager mainMenuManager;
    
    public float blinkInterval = 0.5f;
    private TextMeshProUGUI _tapText;

    void Start()
    {
        _tapText = GetComponent<TextMeshProUGUI>();
        if (_tapText != null)
        {
            StartCoroutine(BlinkText());
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject! Disabling script.");
            enabled = false;
        }
    }
    
    // Metode ini akan dipanggil oleh Button On Click()
    public void OnTapToBeginClicked()
    {
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenuWithFade();
        }
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            if (_tapText != null)
            {
                _tapText.enabled = true;
                yield return new WaitForSeconds(blinkInterval);
                _tapText.enabled = false;
                yield return new WaitForSeconds(blinkInterval);
            }
            else
            {
                yield break;
            }
        }
    }
}