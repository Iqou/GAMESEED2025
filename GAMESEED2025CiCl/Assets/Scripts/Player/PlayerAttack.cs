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

        // Update HUD
        if (GameHUD.Instance != null)
        {
            // Get the component from the newHoregPrefab
            MonoBehaviour horegComponent = newHoregPrefab.GetComponent<ToaRW>();
            if (horegComponent == null) horegComponent = newHoregPrefab.GetComponent<BassKondangan>();
            if (horegComponent == null) horegComponent = newHoregPrefab.GetComponent<SubwooferDugem>();
            if (horegComponent == null) horegComponent = newHoregPrefab.GetComponent<RealHoreg>();

            

            Texture2D icon = null;
            float currentCooldown = 0f;
            float maxCooldown = 0f; // Default value

            if (horegComponent != null && playerStats != null)
            {
                if (horegComponent is ToaRW toa) { icon = toa.GetIconTexture(); currentCooldown = toa.GetRemainingCooldown(); maxCooldown = toa.GetMaxCooldown(playerStats); }
                else if (horegComponent is BassKondangan bass) { icon = bass.GetIconTexture(); currentCooldown = bass.GetRemainingCooldown(); maxCooldown = bass.GetMaxCooldown(playerStats); }
                else if (horegComponent is SubwooferDugem sub) { icon = sub.GetIconTexture(); currentCooldown = sub.GetRemainingCooldown(); maxCooldown = sub.GetMaxCooldown(playerStats); }
                else if (horegComponent is RealHoreg real) { icon = real.GetIconTexture(); currentCooldown = real.GetRemainingCooldown(); maxCooldown = real.GetMaxCooldown(playerStats); }
            }
            GameHUD.Instance.UpdateWeaponSlot(slotIndex - 1, icon, currentCooldown, maxCooldown);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        // Update UI for all slots continuously
        for (int i = 0; i < horegPrefabs.Count; i++)
        {
            Texture2D icon = null;
            float currentCooldown = 0f;
            float maxCooldown = 0f;

            if (i < playerStats.unlockedHoregSlots)
            {
                MonoBehaviour horegComponent = null;
                if (i == 0 && toa != null) horegComponent = toa;
                else if (i == 1 && kondangan != null) horegComponent = kondangan;
                else if (i == 2 && dugem != null) horegComponent = dugem;
                else if (i == 3 && superHoreg != null) horegComponent = superHoreg;

                if (horegComponent != null && playerStats != null)
                {
                    if (horegComponent is ToaRW toaComp) { icon = toaComp.GetIconTexture(); currentCooldown = toaComp.GetRemainingCooldown(); maxCooldown = toaComp.GetMaxCooldown(playerStats); }
                    else if (horegComponent is BassKondangan bassComp) { icon = bassComp.GetIconTexture(); currentCooldown = bassComp.GetRemainingCooldown(); maxCooldown = bassComp.GetMaxCooldown(playerStats); }
                    else if (horegComponent is SubwooferDugem subComp) { icon = subComp.GetIconTexture(); currentCooldown = subComp.GetRemainingCooldown(); maxCooldown = subComp.GetMaxCooldown(playerStats); }
                    else if (horegComponent is RealHoreg realComp) { icon = realComp.GetIconTexture(); currentCooldown = realComp.GetRemainingCooldown(); maxCooldown = realComp.GetMaxCooldown(playerStats); }
                }
            }
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.UpdateWeaponSlot(i, icon, currentCooldown, maxCooldown);
            }
        }

        // --- RHYTHM-BASED ATTACK LOGIC ---

        // Slot 1 (W)
        if (toa != null && Input.GetKeyDown(KeyCode.W))
        {
            HitQuality quality = RhythmManager.Instance.CheckHitQuality();
            GameHUD.Instance.ShowHitFeedback(quality);
            if (quality != HitQuality.Miss)
            {
                toa.Use(transform, playerStats, quality);
            }
        }

        // Slot 2 (A)
        if (playerStats.unlockedHoregSlots >= 2 && kondangan != null && Input.GetKeyDown(KeyCode.A))
        {
            HitQuality quality = RhythmManager.Instance.CheckHitQuality();
            GameHUD.Instance.ShowHitFeedback(quality);
            if (quality != HitQuality.Miss)
            {
                kondangan.Use(transform, playerStats, quality);
            }
        }

        // Slot 3 (S)
        if (playerStats.unlockedHoregSlots >= 3 && dugem != null && Input.GetKeyDown(KeyCode.S))
        {
            HitQuality quality = RhythmManager.Instance.CheckHitQuality();
            GameHUD.Instance.ShowHitFeedback(quality);
            if (quality != HitQuality.Miss)
            {
                dugem.Use(transform, playerStats, quality);
            }
        }

        // Slot 4 (D)
        if (playerStats.unlockedHoregSlots >= 4 && superHoreg != null && Input.GetKeyDown(KeyCode.D))
        {
            HitQuality quality = RhythmManager.Instance.CheckHitQuality();
            GameHUD.Instance.ShowHitFeedback(quality);
            if (quality != HitQuality.Miss)
            {
                superHoreg.Use(transform, playerStats, quality);
            }
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
