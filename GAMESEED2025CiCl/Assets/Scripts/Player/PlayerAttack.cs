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


    private ToaRW toa;
    private BassKondangan kondangan;
    private SubwooferDugem dugem;
    private RealHoreg superHoreg;
    private PlayerStats playerStats;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextBeatTime = Time.time * beatInterval;
        activeHoregs = new List<GameObject>();
        playerStats = GetComponent<PlayerStats>();

        // Initial setup from the prefab list
        AssignHoregsFromPrefabs();
    }

    private void AssignHoregsFromPrefabs()
    {
        if (horegPrefabs.Count > 0 && horegPrefabs[0] != null) toa = horegPrefabs[0].GetComponent<ToaRW>();
        if (horegPrefabs.Count > 1 && horegPrefabs[1] != null) kondangan = horegPrefabs[1].GetComponent<BassKondangan>();
        if (horegPrefabs.Count > 2 && horegPrefabs[2] != null) dugem = horegPrefabs[2].GetComponent<SubwooferDugem>();
        if (horegPrefabs.Count > 3 && horegPrefabs[3] != null) superHoreg = horegPrefabs[3].GetComponent<RealHoreg>();
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
        if (toa != null && Input.GetKeyDown(KeyCode.W))
        {
            toa.Use(transform, playerStats);
        }

        // Slot 2 (A)
        if (playerStats.unlockedHoregSlots >= 2 && kondangan != null && Input.GetKeyDown(KeyCode.A))
        {
            kondangan.Use(transform, playerStats);
        }

        // Slot 3 (S)
        if (playerStats.unlockedHoregSlots >= 3 && dugem != null && Input.GetKeyDown(KeyCode.S))
        {
            dugem.Use(transform, playerStats);
        }

        // Slot 4 (D)
        if (playerStats.unlockedHoregSlots >= 4 && superHoreg != null && Input.GetKeyDown(KeyCode.D))
        {
            superHoreg.Use(transform, playerStats);
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
