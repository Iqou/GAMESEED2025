using UnityEngine;

[CreateAssetMenu(fileName = "NewHoregSlot", menuName = "Shop/Horeg Slot Item")]
public class HoregSlotItem : ShopItem
{
    [Header("Horeg Slot Specifics")]
    public int slotIndex; 

    public override bool Purchase(GameObject buyer)
    {
        PlayerAttack playerAttack = buyer.GetComponent<PlayerAttack>();
        if (playerAttack == null)
        {            

            Debug.LogError("PlayerAttack component not found on the buyer!");
            return false;
        }

        // TODO: pengecekan apbila slot sudah terbuka
        // if (playerAttack.IsHoregSlotUnlocked(slotIndex))
        // {
        //     Debug.Log($"Horeg slot {slotIndex} is already unlocked.");
        //     return false;
        // }

        // playerAttack.UnlockHoregSlot(slotIndex);

        Debug.Log($"Purchased and unlocked Horeg Slot {slotIndex}.");
        return true;
    }

    public override int GetCurrentCost()
    {
        return baseCost;
    }
}
