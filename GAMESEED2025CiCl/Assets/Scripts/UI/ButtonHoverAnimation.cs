using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetBox;
    public TextMeshProUGUI targetText;

    public Vector3 boxHoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    public Vector3 textHoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    
    private Vector3 normalBoxScale;
    private Vector3 normalTextScale;

    public float animationDuration = 0.1f;
    private Coroutine _currentAnimationCoroutine;

    // Tambahkan referensi untuk audio
    public AudioSource audioSource;
    public AudioClip hoverSfx;

    void Start()
    {
        if (targetBox != null)
        {
            normalBoxScale = targetBox.transform.localScale;
        }
        if (targetText != null)
        {
            normalTextScale = targetText.transform.localScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSelect();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnDeselect();
    }
    
    public void OnSelect()
    {
        // Putar efek suara saat tombol dipilih
        if (audioSource != null && hoverSfx != null)
        {
            audioSource.PlayOneShot(hoverSfx);
        }

        if (targetBox != null)
        {
            if (_currentAnimationCoroutine != null) StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = StartCoroutine(AnimateScale(targetBox, boxHoverScale));
        }
        if (targetText != null)
        {
            StartCoroutine(AnimateScale(targetText.gameObject, textHoverScale));
        }
    }
    
    public void OnDeselect()
    {
        if (targetBox != null)
        {
            if (_currentAnimationCoroutine != null) StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = StartCoroutine(AnimateScale(targetBox, normalBoxScale));
        }
        if (targetText != null)
        {
            StartCoroutine(AnimateScale(targetText.gameObject, normalTextScale));
        }
    }
    
    private IEnumerator AnimateScale(GameObject target, Vector3 endScale)
    {
        Vector3 startScale = target.transform.localScale;
        float time = 0;

        while (time < animationDuration)
        {
            target.transform.localScale = Vector3.Lerp(startScale, endScale, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        
        target.transform.localScale = endScale;
    }
}