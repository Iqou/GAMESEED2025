using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Assign Weapon Prefabs Here")]
    public List<GameObject> horegPrefabs; // Untuk konsistensi, gunakan List<GameObject> untuk menyimpan prefab senjata.

    private List<GameObject> activeHoregs; // Menampilkan speaker yang sedang aktif

    public float beatInterval = 1.0f;
    public float beatWindow = 0.15f;

    private float nextBeatTime = 0.0f;


    private List<IHoregWeapon> equippedWeapons;
    private PlayerStats playerStats;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextBeatTime = Time.time * beatInterval;
        playerStats = GetComponent<PlayerStats>();
        equippedWeapons = new List<IHoregWeapon>();

        // Initial setup from the prefab list
        AssignHoregsFromPrefabs();
    }

    private void AssignHoregsFromPrefabs()
    {
        equippedWeapons.Clear();
        foreach (var prefab in horegPrefabs)
        {
            if (prefab != null)
            {
                IHoregWeapon weapon = prefab.GetComponent<IHoregWeapon>();
                if (weapon != null)
                {
                    equippedWeapons.Add(weapon);
                }
                else
                {
                    equippedWeapons.Add(null);
                    Debug.LogWarning($"Prefab {prefab.name} does not have a component that implements IHoregWeapon.");
                }
            }
            else
            {
                equippedWeapons.Add(null);
            }
        }
    }

    public List<GameObject> GetEquippedHoregPrefabs()
    {
        return new List<GameObject>(horegPrefabs);
    }

    public void EquipNewHoreg(GameObject newHoregPrefab, int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > 4)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}. Must be between 1 and 4.");
            return;
        }

        // Ensure the prefab list is large enough
        while (horegPrefabs.Count < slotIndex)
        {
            horegPrefabs.Add(null);
        }

        horegPrefabs[slotIndex - 1] = newHoregPrefab;
        Debug.Log($"Equipped {newHoregPrefab.name} into slot {slotIndex}.");

        // Re-assign all weapon references
        AssignHoregsFromPrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        bool isOnBeat = Mathf.Abs(Time.time - nextBeatTime) <= beatWindow;

        // Slot 1 (W) is always available
        if (Input.GetKeyDown(KeyCode.W) && equippedWeapons.Count > 0 && equippedWeapons[0] != null)
        {
            equippedWeapons[0].Use(transform, playerStats);
        }

        // Slot 2 (A)
        if (playerStats.unlockedHoregSlots >= 2 && Input.GetKeyDown(KeyCode.A) && equippedWeapons.Count > 1 && equippedWeapons[1] != null)
        {
            equippedWeapons[1].Use(transform, playerStats);
        }

        // Slot 3 (S)
        if (playerStats.unlockedHoregSlots >= 3 && Input.GetKeyDown(KeyCode.S) && equippedWeapons.Count > 2 && equippedWeapons[2] != null)
        {
            equippedWeapons[2].Use(transform, playerStats);
        }

        // Slot 4 (D)
        if (playerStats.unlockedHoregSlots >= 4 && Input.GetKeyDown(KeyCode.D) && equippedWeapons.Count > 3 && equippedWeapons[3] != null)
        {
            equippedWeapons[3].Use(transform, playerStats);
        }
    }

    void Metronome()
    {
        if (Time.time >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
           //Debug.Log($"Waktu jedag-jedug: {nextBeatTime}");
        }
    }
}