using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LogoBeatAnimation : MonoBehaviour
{
    public AudioSource audioSource;
    public float beatScaleMultiplier = 1.1f;
    public float beatDuration = 0.5f;
    public float smoothUpSpeed = 10f;
    public float smoothDownSpeed = 20f;

    private Vector3 _originalScale;
    private RectTransform _rectTransform;

    void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
        {
            _originalScale = _rectTransform.localScale;
        }
        else
        {
            _originalScale = transform.localScale;
        }

        if (audioSource != null && audioSource.clip != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            StopAllCoroutines(); 
            StartCoroutine(Beat());
        }
    }

    IEnumerator Beat()
    {
        while (true)
        {
            float time = 0;
            Vector3 startScale = (_rectTransform != null) ? _rectTransform.localScale : transform.localScale;
            Vector3 endScale = _originalScale * beatScaleMultiplier;
            while (time < 1f)
            {
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = Vector3.Lerp(startScale, endScale, time);
                }
                else
                {
                    transform.localScale = Vector3.Lerp(startScale, endScale, time);
                }
                time += Time.deltaTime * smoothUpSpeed;
                yield return null;
            }
            if (_rectTransform != null)
            {
                _rectTransform.localScale = endScale;
            }
            else
            {
                transform.localScale = endScale;
            }

            yield return new WaitForSeconds((beatDuration / 2f) - (time / smoothUpSpeed));

            time = 0;
            startScale = (_rectTransform != null) ? _rectTransform.localScale : transform.localScale;
            endScale = _originalScale;
            while (time < 1f)
            {
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = Vector3.Lerp(startScale, endScale, time);
                }
                else
                {
                    transform.localScale = Vector3.Lerp(startScale, endScale, time);
                }
                time += Time.deltaTime * smoothDownSpeed;
                yield return null;
            }
            if (_rectTransform != null)
            {
                _rectTransform.localScale = endScale;
            }
            else
            {
                transform.localScale = endScale;
            }
            
            yield return new WaitForSeconds((beatDuration / 2f) - (time / smoothDownSpeed));
        }
    }
}