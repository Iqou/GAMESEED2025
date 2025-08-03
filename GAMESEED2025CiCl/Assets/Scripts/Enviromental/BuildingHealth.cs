using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private Vector2Int coordinate;
    private void Start()
    {
        currentHealth = maxHealth;
        coordinate = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        GetComponent<Collider>().enabled = true; 
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SoundWave"))
        {
            TakeDamage(10); // Adjust damage amount as needed
            Destroy(other.gameObject); // Destroy the sound wave on impact
        }
    }

    private void DestroyBuilding()
    {
        BuildingData toRemove = null;
        foreach (BuildingData building in PathGen.Instance.PlacedBuildings)
        {
            if (building.coordinate == coordinate)
            {
                toRemove = building;
                break;
            }
        }

        if (toRemove != null)
        {
            PathGen.Instance.PlacedBuildings.Remove(toRemove);
        }

        Destroy(gameObject);
    }
}