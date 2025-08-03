using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class RhythmManager : MonoBehaviour
{
    public static RhythmManager Instance { get; private set; }

    [Header("State")]
    public bool IsRhythmBattleActive { get; private set; } = false;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip metronomeTick;

    [Header("Timing Windows (in seconds)")]
    [Tooltip("The window for a 'Perfect' hit, on either side of the beat.")]
    public float perfectWindow = 0.08f;
    [Tooltip("The window for a 'Good' hit, on either side of the beat.")]
    public float goodWindow = 0.15f;

    [Header("Beatmap")]
    [Tooltip("A list of beat timestamps in seconds. Can be loaded from a file.")]
    public List<float> beatTimestamps;
    private int beatIndex = 0;

    [Header("Metronome Settings")]
    [Tooltip("Beats per minute for the metronome.")]
    public float bpm = 120f;
    private float beatInterval;

    private float nextBeatTime = 0f;

    // Event to notify other systems (like the UI) when a beat occurs.
    public UnityEvent OnBeat;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Ensure rhythm UI is hidden by default
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.SetRhythmUIVisibility(false);
        }
    }

    void Update()
    {
        if (!IsRhythmBattleActive) return;

        // The core loop that advances the beat.
        if (Time.time >= nextBeatTime)
        {
            if (OnBeat != null)
            {
                OnBeat.Invoke();
            }

            if (beatTimestamps != null && beatTimestamps.Count > 0)
            {
                beatIndex++;
                if (beatIndex < beatTimestamps.Count)
                {
                    nextBeatTime = beatTimestamps[beatIndex];
                }
                else
                {
                    StopRhythmBattle(); // End of song
                }
            }
            else
            {
                nextBeatTime += beatInterval;
            }
        }
    }

    public void StartMetronome()
    {
        beatTimestamps = null;
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval;
        IsRhythmBattleActive = true;
        musicSource.Play();
        if (GameHUD.Instance != null) GameHUD.Instance.SetRhythmUIVisibility(true);
    }

    public void StartSong(AudioClip song, TextAsset beatmapJson)
    {
        BeatmapData beatmap = JsonUtility.FromJson<BeatmapData>(beatmapJson.text);
        beatTimestamps = beatmap.beats;
        
        beatIndex = 0;
        musicSource.clip = song;
        musicSource.Play();
        
        nextBeatTime = beatTimestamps[0];
        IsRhythmBattleActive = true;
        if (GameHUD.Instance != null) GameHUD.Instance.SetRhythmUIVisibility(true);
    }

    public void StopRhythmBattle()
    {
        IsRhythmBattleActive = false;
        musicSource.Stop();
        if (GameHUD.Instance != null) GameHUD.Instance.SetRhythmUIVisibility(false);
    }

    public HitQuality CheckHitQuality()
    {
        // If not in a rhythm battle, every hit is just a standard "Good" hit.
        if (!IsRhythmBattleActive) return HitQuality.Good;

        float timeDifference = Mathf.Abs(Time.time - nextBeatTime);

        if (timeDifference <= perfectWindow) return HitQuality.Perfect;
        if (timeDifference <= goodWindow) return HitQuality.Good;
        
        float earlyHitThreshold = (beatTimestamps != null && beatTimestamps.Count > 0) ? (beatTimestamps[beatIndex] - (beatIndex > 0 ? beatTimestamps[beatIndex-1] : 0))/2f : beatInterval * 0.5f;
        if (Time.time < nextBeatTime && (nextBeatTime - Time.time) < earlyHitThreshold)
        {
             return HitQuality.OffBeat;
        }

        return HitQuality.Miss;
    }
}

// A simple helper class to deserialize our beatmap JSON.
[System.Serializable]
public class BeatmapData
{
    public List<float> beats;
}
