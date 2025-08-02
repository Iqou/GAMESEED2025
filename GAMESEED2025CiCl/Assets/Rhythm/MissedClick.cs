using UnityEngine;
using TMPro;
using System.Collections;

public class MissedClick : MonoBehaviour
{
    public GameObject Health;

    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 0.5f;

    private Coroutine _hideTextCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);

        if (Health != null)
        {
            Health.GetComponent<Health>().ChangeHealth(-5);
            
            DisplayFeedbackText("MISS!", Color.red);
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
}