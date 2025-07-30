using UnityEngine;

public class RealHoreg : BaseHoreg
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horegName = "Real Horeg";
        cooldownTime = 6.0f;
        weight = 6.0f;
        saweranMultiplier = 1.5f;
        aoeRadius = 1.0f;
        keyTrigger = KeyCode.D;
        color = Color.brown;
        minimumDesibeOutput = 100.0f;
        maximumDesibelOutput = 110.0f;
    }

    protected override void TriggerAOE(bool isOnBeat)
    {
        desibelOutput = Random.Range(minimumDesibeOutput, maximumDesibelOutput);
        base.TriggerAOE(isOnBeat);
        Debug.Log("Real Horeg ke Trigger");
    }

}
