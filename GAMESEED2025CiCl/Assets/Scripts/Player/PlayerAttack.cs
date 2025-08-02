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

        foreach (GameObject horeg in horegPrefabs)
        {
            if (horeg.GetComponent<ToaRW>() != null)
            {
                toa = horeg.GetComponent<ToaRW>();
            }
            if (horeg.GetComponent<BassKondangan>() != null)
            {
                kondangan = horeg.GetComponent<BassKondangan>();
            }
            if (horeg.GetComponent<SubwooferDugem>() != null)
            {
                dugem = horeg.GetComponent<SubwooferDugem>();
            }
            if (horeg.GetComponent<RealHoreg>() != null)
            {
                superHoreg = horeg.GetComponent<RealHoreg>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        bool isOnBeat = Mathf.Abs(Time.time - nextBeatTime) <= beatWindow;

        if (toa != null && Input.GetKeyDown(KeyCode.W))
        {
            toa.Use(transform, playerStats);
        }

        if (kondangan != null && Input.GetKeyDown(KeyCode.A))
        {
            kondangan.Use(transform, playerStats);
        }

        if (dugem != null && Input.GetKeyDown(KeyCode.S))
        {
            dugem.Use(transform, playerStats);
        }

        if (superHoreg != null && Input.GetKeyDown(KeyCode.D))
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
