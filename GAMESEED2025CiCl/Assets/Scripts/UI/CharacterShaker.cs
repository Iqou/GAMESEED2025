using UnityEngine;
using System.Collections;

public class CharacterShaker : MonoBehaviour
{
    // Intensitas getaran (amplitude)
    public float shakeIntensity = 0.5f; 
    // Kecepatan getaran
    public float shakeSpeed = 50f;

    private Vector3 _originalPosition;
    private Coroutine _shakeCoroutine;

    void Start()
    {
        _originalPosition = transform.position;
        // Animasi goyang dimulai
        _shakeCoroutine = StartCoroutine(Shake());
    }

    // Metode yang bisa dipanggil untuk memulai/menghentikan goyangan
    public void StartShaking()
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _originalPosition = transform.position;
        _shakeCoroutine = StartCoroutine(Shake());
    }

    public void StopShaking()
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        transform.position = _originalPosition;
    }

    IEnumerator Shake()
    {
        while (true)
        {
            float xOffset = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2 - 1) * shakeIntensity;
            float yOffset = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2 - 1) * shakeIntensity;

            transform.position = _originalPosition + new Vector3(xOffset, yOffset, 0);

            yield return null;
        }
    }
}