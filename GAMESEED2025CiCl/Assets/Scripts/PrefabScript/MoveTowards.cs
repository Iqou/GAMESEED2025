using UnityEngine;
using System.Collections.Generic;

public class MoveTowards : MonoBehaviour
{
    public float speed = 40.0f;
    private Vector3 moveDirection = Vector3.right;

    public float areaJangkauan = 5f;
    public float duration = 2f;
    public float minDesibelOutput = 10f;
    public float maxDesibelOutput = 11f;

    public List<AudioSource> dugemSound;

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.Translate(moveDirection * Time.deltaTime * speed, Space.World);
        dugemSoundSystem();
    }

    void dugemSoundSystem()
    {
        int randomIndex = Random.Range(0, dugemSound.Count);
        AudioSource soundTrigger = dugemSound[randomIndex];
        string soundName = dugemSound[randomIndex].name;
        Debug.Log($"SONIC BOOM {soundName}");

        soundTrigger.enabled = true;
        soundTrigger.Play();
    }

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