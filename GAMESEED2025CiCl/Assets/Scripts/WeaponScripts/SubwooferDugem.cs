using UnityEngine;

public class SubwooferDugem : BaseHoreg
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horegName = "Subwoofer Dugem";
        cooldownTime = 12.0f;
        weight = 4.0f;
        saweranMultiplier = 2.5f;
        aoeRadius = 1.0f;
        keyTrigger = KeyCode.S;
        color = Color.pink;
        minimumDesibeOutput = 90.0f;
        maximumDesibelOutput = 100.0f;
    }

    protected override void TriggerAOE(bool isOnBeat)
    {
        desibelOutput = Random.Range(minimumDesibeOutput, maximumDesibelOutput);
        base.TriggerAOE(isOnBeat);
        Debug.Log("Subwoofer Dugem ke Trigger");
    }

}
