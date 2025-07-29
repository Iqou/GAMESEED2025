using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
