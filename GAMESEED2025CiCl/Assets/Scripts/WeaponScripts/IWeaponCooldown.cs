using UnityEngine;

public interface IWeaponCooldown
{
    float LastActiveTime { get; }
    float CurrentCooldown { get; }
    bool IsOnCooldown { get; }
}
