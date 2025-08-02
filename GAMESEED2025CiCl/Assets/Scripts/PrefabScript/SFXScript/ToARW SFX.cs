using UnityEngine;
using System.Collections.Generic;

public class ToARWSFX : MonoBehaviour
{
    public List<AudioSource> dugemSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int randomIndex = Random.Range(0, dugemSound.Count);
        AudioSource soundTrigger = dugemSound[randomIndex];
        string soundName = dugemSound[randomIndex].name;
        Debug.Log($"ToA Mendugem {soundName}");

        soundTrigger.enabled = true;
        soundTrigger.Play();
    }
}
