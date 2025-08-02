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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextBeatTime = Time.time * beatInterval;
        activeHoregs = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        bool isOnBeat = Mathf.Abs(Time.time - nextBeatTime) <= beatWindow;

        foreach (GameObject horeg in horegPrefabs)
        {
            ToaRW toa = horeg.GetComponent<ToaRW>();
            if (toa != null && Input.GetKeyDown(KeyCode.W))
            {
                toa.Use(transform);
                continue;
            }

            BassKondangan kondangan = horeg.GetComponent<BassKondangan>();
            if (kondangan != null && Input.GetKeyDown(KeyCode.A))
            {
                kondangan.Use(transform);
                continue;
            }

            SubwooferDugem dugem = horeg.GetComponent<SubwooferDugem>();
            if (dugem != null && Input.GetKeyDown(KeyCode.S))
            {
                dugem.Use(transform);
                continue;
            }

            RealHoreg superHoreg = horeg.GetComponent<RealHoreg>();
            if (superHoreg != null && Input.GetKeyDown(KeyCode.D))
            {
                superHoreg.Use(transform);
                continue;
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
