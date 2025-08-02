using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    public float speed = 40.0f;
    private Vector3 moveDirection = Vector3.right;

    public float areaJangkauan = 5f;
    public float duration = 2f;
    public float minDesibelOutput = 10f;
    public float maxDesibelOutput = 11f;


    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.Translate(moveDirection * Time.deltaTime * speed, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            float damage = Random.Range(minDesibelOutput, maxDesibelOutput);
            Debug.Log($"{other.name} Duarr kena damage dari Real Horeg, damage {damage} dB");
        }
    }

}