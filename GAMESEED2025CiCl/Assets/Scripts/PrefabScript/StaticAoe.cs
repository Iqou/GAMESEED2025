using UnityEngine;

public class StaticAoe : MonoBehaviour
{
    public float areaJangkauan = 5f;
    public float duration = 2f;
    public float minDesibelOutput = 10f;
    public float maxDesibelOutput = 11f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            INPCDamageable damageable = other.GetComponent<INPCDamageable>();
            if (damageable != null)
            {
                float damage = Random.Range(minDesibelOutput, maxDesibelOutput);

                damageable.TakeDamage(damage);

                Debug.Log($"{other.name} Duarr kena damage dari Real Horeg, damage {damage} dB");
            }
        }
    }
}
