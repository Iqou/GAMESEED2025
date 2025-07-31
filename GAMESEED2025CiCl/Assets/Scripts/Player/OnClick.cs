using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OnClick : MonoBehaviour
{
    public KeyCode Arrow;
    public GameObject Health;
    public float maxDistanceToDestroyOnTooEarlyClick = 5f;

    private bool _isTouching;
    private GameObject _currentTouchingArrow;

    public bool Touching
    {
        get { return _isTouching; }
        private set { _isTouching = value; }
    }

    void Update()
    {
        if (Input.GetKeyDown(Arrow))
        {
            if (Touching && _currentTouchingArrow != null)
            {
                Debug.Log("HIT! Panah " + _currentTouchingArrow.name + " dihancurkan.", _currentTouchingArrow);
                Destroy(_currentTouchingArrow);

                _currentTouchingArrow = null;
                Touching = false;
            }
            else
            {
                Debug.Log("Miss! Klik terlalu cepat!");

                if (Health != null)
                {
                    Health.GetComponent<Health>().currentHealth -= 10;
                }
                else
                {
                    Debug.LogError("Health GameObject tidak diatur di OnClick script.");
                }

                DestroyClosestMovingArrow();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MovingArrow"))
        {
            Touching = true;
            _currentTouchingArrow = collision.gameObject;
            Debug.Log("Panah masuk area target: " + collision.gameObject.name, collision.gameObject);
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
                Debug.Log("Panah keluar area target: " + collision.gameObject.name, collision.gameObject);
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