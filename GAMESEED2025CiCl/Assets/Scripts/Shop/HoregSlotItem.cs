using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Horeg Slot Item", menuName = "Shop/Horeg Slot Unlock Item")]
public class HoregSlotItem : ShopItem
{
    [Header("Unlock Requirements")]
    public int requiredLevel;
    public int slotIndexToUnlock; // e.g., 2 for the second slot, 3 for the third

    [Header("Horeg Database")]
    public HoregDatabase horegDatabase;

    public override bool Purchase(GameObject buyer)
    {
        PlayerStats playerStats = buyer.GetComponent<PlayerStats>();
        PlayerAttack playerAttack = buyer.GetComponent<PlayerAttack>();

        if (playerStats == null || playerAttack == null || horegDatabase == null)
        {
            Debug.LogError("HoregSlotItem Error: Player is missing PlayerStats or PlayerAttack, or the HoregDatabase is not assigned!", this);
            return false;
        }

        // --- All Purchase Condition Checks ---
        if (playerStats.unlockedHoregSlots >= slotIndexToUnlock) return false;
        if (playerStats.level < requiredLevel) return false;
        
        // --- Logic Execution ---
        // 1. Unlock the slot
        playerStats.UnlockHoregSlot(slotIndexToUnlock);

        // 2. Find an unowned Horeg
        List<GameObject> equippedHoregs = playerAttack.GetEquippedHoregPrefabs();
        List<GameObject> unownedHoregs = horegDatabase.allHoregPrefabs.Except(equippedHoregs).ToList();

        if (unownedHoregs.Count > 0)
        {
            // 3. Pick one randomly and equip it
            GameObject newHoreg = unownedHoregs[Random.Range(0, unownedHoregs.Count)];
            playerAttack.EquipNewHoreg(newHoreg, slotIndexToUnlock);
        }
        else
        {
            Debug.LogWarning("Player already owns all available Horegs. Unlocking slot without adding a new Horeg.");
        }

        return true;
    }

    // Override description to show requirements
    public override string GetDescription(PlayerStats playerStats)
    {
        return $"{description}\n<color=yellow>Requires Level: {requiredLevel}</color>";
    }

    // Override cost check to also include level requirement
    public override bool CanAfford(PlayerStats playerStats)
    {
        return base.CanAfford(playerStats) && playerStats.level >= requiredLevel;
    }
}