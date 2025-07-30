using UnityEngine;

public class BassKondangan : BaseHoreg
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horegName = "Bass Kondangan";
        cooldownTime = 10.0f;
        weight = 6.0f;
        saweranMultiplier = 2.2f;
        aoeRadius = 1.0f;
        keyTrigger = KeyCode.A;
        color = Color.blue;
        minimumDesibeOutput = 90.0f;
        maximumDesibelOutput = 100.0f;
    }

    protected override void TriggerAOE(bool isOnBeat)
    {
        desibelOutput = Random.Range(minimumDesibeOutput, maximumDesibelOutput);
        base.TriggerAOE(isOnBeat);
        Debug.Log("Bass Kondangan ke Trigger");
    }

}
