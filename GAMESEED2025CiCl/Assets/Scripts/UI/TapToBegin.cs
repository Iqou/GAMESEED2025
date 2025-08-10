using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TapToBegin : MonoBehaviour
{
    public MainMenuManager mainMenuManager;
    
    public AudioSource audioSource;
    public AudioClip tapSfx;

    public float pulseSpeed = 1f;
    private TextMeshProUGUI _tapText;
    private Color _originalColor;

    void Start()
    {
        // Gunakan GetComponentInChildren() untuk mencari di objek anak
        _tapText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (_tapText != null)
        {
            _originalColor = _tapText.color;
            StartCoroutine(PulseText());
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in children! Disabling script.");
            enabled = false;
        }
    }
    
    public void OnTapToBeginClicked()
    {
        if (mainMenuManager != null)
        {
            if (audioSource != null && tapSfx != null)
            {
                audioSource.PlayOneShot(tapSfx);
            }
            
            mainMenuManager.ShowMainMenuWithFade();
        }
    }

    IEnumerator PulseText()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            _tapText.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
            yield return null;
        }
    }
}