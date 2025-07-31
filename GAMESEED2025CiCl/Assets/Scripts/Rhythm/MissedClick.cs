using Unity.VisualScripting;
using UnityEngine;

public class MissedClick : MonoBehaviour
{
    public GameObject Health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
        Health.GetComponent<Health>().currentHealth -= 10;
    }
}
