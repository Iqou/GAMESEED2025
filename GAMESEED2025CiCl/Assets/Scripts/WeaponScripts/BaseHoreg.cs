using UnityEngine;

public class BaseHoreg : MonoBehaviour
{
    public GameObject aoePrefab;

    public string horegName;
    public float cooldownTime;
    public float aoeRadius;
    public float weight;
    public float saweranMultiplier;

    public float minimumDesibeOutput;
    public float maximumDesibelOutput;
    public float desibelOutput;

    public Color color;
    public KeyCode keyTrigger;

    private float nextAttack = 0f;

    protected virtual void TriggerAOE(bool isOnBeat)
    {
        // Instanstiate AOE
        GameObject aoeInstance = Instantiate(aoePrefab, transform.position, Quaternion.identity);

        // AOE Radius
        aoeInstance.transform.localScale = new Vector3(aoeRadius, 0.1f, aoeRadius);

        // Kalau On Beat (Dugem)
        Renderer aoeRenderer = aoeInstance.GetComponent<Renderer>();

        if (isOnBeat)
        {
            aoeRenderer.material.color = Color.gold;
            Debug.Log($"{gameObject.name} On Beat Dapat Bonus");
        }

        // Destroy AOE
        Destroy(aoeInstance, cooldownTime);
    }

    public virtual void Attack(float currentTime, bool isOnBeat)
    {
      
    }
}
