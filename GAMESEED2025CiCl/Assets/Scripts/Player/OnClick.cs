using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OnClick : MonoBehaviour
{
    public KeyCode Arrow;
    public GameObject Health;
    public float maxDistanceToDestroyOnTooEarlyClick = 5f;
    public Sprite defaultSprite;
    public Sprite clickedSprite;
    public float clickAnimationDuration = 0.1f;
    [Tooltip("Jarak maksimum dari target Y untuk mendapatkan 'Perfect'. Sesuaikan di Editor.")]
    public float perfectWindowDistance = 0.3f;
    [Tooltip("Jarak maksimum dari target Y untuk mendapatkan 'Good'. Harus lebih besar dari Perfect Window Distance. Sesuaikan di Editor.")]
    public float goodWindowDistance = 0.7f;
    private bool _isTouching;
    private GameObject _currentTouchingArrow;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;

    public bool Touching
    {
        get { return _isTouching; }
        private set { _isTouching = value; }
    }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name + ". Sprite animation will not work.");
        }
    }

    void Start()
    {
        if (_spriteRenderer != null && defaultSprite != null)
        {
            _spriteRenderer.sprite = defaultSprite;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(Arrow))
        {
            StartCoroutine(PlayClickAnimation());
            if (Touching && _currentTouchingArrow != null)
            {
                float distance = Mathf.Abs(transform.position.y - _currentTouchingArrow.transform.position.y);
                if (distance <= perfectWindowDistance)
                {
                    Debug.Log("<color=green>PERFECT!</color> Panah " + _currentTouchingArrow.name + " dihancurkan.", _currentTouchingArrow);
                    if (Health != null)
                    {
                        // Add 5 health for a perfect hit
                        Health.GetComponent<Health>().ChangeHealth(5);
                    }
                }
                else if (distance <= goodWindowDistance)
                {
                    Debug.Log("<color=yellow>GOOD!</color> Panah " + _currentTouchingArrow.name + " dihancurkan.", _currentTouchingArrow);
                    if (Health != null)
                    {
                        // Add 2 health for a good hit
                        Health.GetComponent<Health>().ChangeHealth(2);
                    }
                }
                else
                {
                    Debug.Log("<color=orange>HIT (Akurasi Rendah)!</color> Panah " + _currentTouchingArrow.name + " dihancurkan, tapi kurang akurat.", _currentTouchingArrow);
                    if (Health != null)
                    {
                        Health.GetComponent<Health>().ChangeHealth(-5);
                    }
                }
                Destroy(_currentTouchingArrow);
                _currentTouchingArrow = null;
                Touching = false;
            }
            else
            {
                Debug.Log("<color=red>Miss! Klik terlalu cepat atau tidak ada panah di area target.</color>");
                if (Health != null)
                {
                    Health.GetComponent<Health>().ChangeHealth(-10);
                }
                else
                {
                    Debug.LogError("Health GameObject tidak diatur di OnClick script.");
                }
                DestroyClosestMovingArrow();
            }
        }
    }

    IEnumerator PlayClickAnimation()
    {
        if (_spriteRenderer != null && clickedSprite != null)
        {
            _spriteRenderer.sprite = clickedSprite;
            yield return new WaitForSeconds(clickAnimationDuration);
            if (_spriteRenderer != null && defaultSprite != null)
            {
                _spriteRenderer.sprite = defaultSprite;
            }
        }
        else if (_spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer tidak ditetapkan untuk animasi klik pada " + gameObject.name);
        }
        else if (clickedSprite == null)
        {
            Debug.LogWarning("Sprite 'Clicked' tidak ditetapkan untuk animasi klik pada " + gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingArrow"))
        {
            Touching = true;
            _currentTouchingArrow = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingArrow"))
        {
            if (_currentTouchingArrow == collision.gameObject)
            {
                Touching = false;
                _currentTouchingArrow = null;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingArrow"))
        {
            if (_currentTouchingArrow == null)
            {
                _currentTouchingArrow = collision.gameObject;
            }
            Touching = true;
        }
    }

    void DestroyClosestMovingArrow()
    {
        GameObject[] movingArrows = GameObject.FindGameObjectsWithTag("MovingArrow");
        GameObject arrowToDestroy = null;
        float minDistance = float.MaxValue;
        Vector2 targetPosition = transform.position;
        foreach (GameObject arrow in movingArrows)
        {
            float distance = Vector2.Distance(targetPosition, arrow.transform.position);
            if (distance < maxDistanceToDestroyOnTooEarlyClick &&
                arrow.transform.position.y < targetPosition.y &&
                arrow != _currentTouchingArrow &&
                arrow.activeInHierarchy)
            {
                if (distance < minDistance)
                {
                    arrowToDestroy = arrow;
                    minDistance = distance;
                }
            }
        }
        if (arrowToDestroy != null)
        {
            Destroy(arrowToDestroy);
            Debug.Log("Panah yang akan datang (" + arrowToDestroy.name + ") dihancurkan karena klik terlalu cepat.", arrowToDestroy);
        }
    }
}