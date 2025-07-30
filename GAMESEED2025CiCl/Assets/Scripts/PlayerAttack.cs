using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public GameObject aoePrefab;

    public List<BaseHoreg> horegList;
    public float beatInterval = 1.0f;
    public float beatWindow = 0.15f;

    private float nextBeatTime = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextBeatTime = Time.time * beatInterval;
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        bool isOnBeat = Mathf.Abs(Time.time - nextBeatTime) <= beatWindow;

        foreach (BaseHoreg horeg in horegList)
        {
            horeg.Attack(Time.time, isOnBeat);
        }
    }

    void Metronome()
    {
        if (Time.time >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
            Debug.Log($"Waktu jedag-jedug: {nextBeatTime}");
        }
    }
}
