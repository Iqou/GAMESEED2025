using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class OnClick : MonoBehaviour
{
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 0.5f;

    public KeyCode Arrow;
    public GameObject Health;
    public float maxDistanceToDestroyOnTooEarlyClick = 6f;
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

    private Coroutine _hideTextCoroutine;

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

        if (feedbackText != null)
        {
            feedbackText.text = "";
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

                Health healthComponent = Health.GetComponent<Health>();

                if (distance <= perfectWindowDistance)
                {
                    DisplayFeedbackText("PERFECT!", Color.green);
                    if (healthComponent != null)
                    {
                        healthComponent.ChangeHealth(5);
                    }
                }
                else if (distance <= goodWindowDistance)
                {
                    DisplayFeedbackText("GOOD!", Color.yellow);
                    if (healthComponent != null)
                    {
                        healthComponent.ChangeHealth(2);
                    }
                }
                else
                {
                    DisplayFeedbackText("HIT!", Color.red);
                    if (healthComponent != null)
                    {
                        healthComponent.ChangeHealth(-1);
                    }
                }
                
                Destroy(_currentTouchingArrow);
                _currentTouchingArrow = null;
                Touching = false;
            }
            else
            {
                DisplayFeedbackText("MISS!", Color.red);
                if (Health != null)
                {
                    Health.GetComponent<Health>().ChangeHealth(-5);
                }
                else
                {
                    Debug.LogError("Health GameObject tidak diatur di OnClick script.");
                }
                DestroyClosestMovingArrow();
            }
        }
    }
    
    void DisplayFeedbackText(string message, Color color)
    {
        if (feedbackText != null)
        {
            if (_hideTextCoroutine != null)
            {
                StopCoroutine(_hideTextCoroutine);
            }
            
            feedbackText.text = message;
            feedbackText.color = color;
            
            _hideTextCoroutine = StartCoroutine(HideFeedbackText());
        }
    }

    IEnumerator HideFeedbackText()
    {
        yield return new WaitForSeconds(feedbackDuration);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        _hideTextCoroutine = null;
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