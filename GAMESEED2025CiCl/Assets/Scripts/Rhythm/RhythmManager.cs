using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class RhythmManager : MonoBehaviour
{
    public TextAsset beatmapJsonFile;
    
    public GameObject leftArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject rightArrowPrefab;

    public Transform leftLaneSpawnPoint;
    public Transform downLaneSpawnPoint;
    public Transform upLaneSpawnPoint;
    public Transform rightLaneSpawnPoint;

    public AudioSource audioSource;
    public TextMeshProUGUI playbackTimerText;
    public TextMeshProUGUI remainingTimeText;
    public float noteApproachTime = 1.5f;

    [Header("Mekanik Boss")]
    [Tooltip("Kekuatan dasar pergerakan bar, 0-100 float")]
    public float beatForce = 35f;
    [Tooltip("Multiplier untuk menambah BeatForce, 1-5 kali float")]
    public float beatMultiplier = 2f;
    [Tooltip("Frekuensi boss menggunakan beat multiplier, 0-5 kali/lagu integer")]
    public int pressure = 3;
    [Tooltip("Batasan waktu beat multiplier aktif, 1-20 detik float")]
    public float beatMultiplierThreshold = 5f;
    [Tooltip("Jumlah kali boss merubah BPM, 1-5 kali integer")]
    public int tempoShiftCount = 4;
    [Tooltip("Text UI untuk menampilkan peringatan BPM")]
    public TextMeshProUGUI bpmWarningText;
    public float bpmWarningDuration = 1.5f;

    private BeatmapData beatmapData;
    private List<HitObject> sortedHitObjects;
    private int nextHitObjectIndex = 0;
    private bool beatMultiplierActive = false;
    private int tempoShiftedCount = 0;
    private float defaultAudioPitch;

    void Start()
    {
        if (beatmapJsonFile == null || audioSource == null ||
            leftArrowPrefab == null || downArrowPrefab == null ||
            upArrowPrefab == null || rightArrowPrefab == null ||
            leftLaneSpawnPoint == null || downLaneSpawnPoint == null ||
            upLaneSpawnPoint == null || rightLaneSpawnPoint == null)
        {
            Debug.LogError("RhythmManager is not fully configured. Disabling script.");
            enabled = false;
            return;
        }

        defaultAudioPitch = audioSource.pitch;
        LoadBeatmap();
        
        if (beatmapData != null && audioSource.clip != null)
        {
            StartCoroutine(SpawnNotesRoutine());
            StartCoroutine(BeatMultiplierRoutine());
            StartCoroutine(TempoShiftRoutine());
        }
        else
        {
            Debug.LogError("Failed to load beatmap data or audio clip. Spawning routine will not start. Disabling script.");
            enabled = false;
        }
    }

    void LoadBeatmap()
    {
        string jsonString = beatmapJsonFile.text;
        try
        {
            beatmapData = JsonUtility.FromJson<BeatmapData>(jsonString);
            
            beatmapData.general.AudioFilename = beatmapData.general.AudioFilename.Trim();
            beatmapData.metadata.Title = beatmapData.metadata.Title.Trim();
            beatmapData.metadata.Artist = beatmapData.metadata.Artist.Trim();
            beatmapData.metadata.Creator = beatmapData.metadata.Creator.Trim();
            beatmapData.metadata.Version = beatmapData.metadata.Version.Trim();
            
            foreach (var ho in beatmapData.hitobjects)
            {
                ho.x = ho.x.Trim();
                ho.y = ho.y.Trim();
                ho.time = ho.time.Trim();
                ho.type = ho.type.Trim();
                ho.hitsound = ho.hitsound.Trim();
                ho.extras = ho.extras.Trim();
            }

            sortedHitObjects = beatmapData.hitobjects
                                .Where(ho => float.TryParse(ho.time, out _))
                                .OrderBy(ho => float.Parse(ho.time))
                                .ToList();

            string audioResourcePath = beatmapData.general.AudioFilename; 
            AudioClip audioClip = Resources.Load<AudioClip>(audioResourcePath);
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                Debug.Log("Audio clip '" + audioResourcePath + "' loaded successfully.");
            }
            else
            {
                Debug.LogError("Audio clip not found in Resources folder: '" + audioResourcePath + "'. Please check filename and placement.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse beatmap JSON: " + e.Message);
            beatmapData = null;
        }
    }

    void Update()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            float currentTime = audioSource.time;
            float duration = audioSource.clip.length;
            if (remainingTimeText != null)
                remainingTimeText.text = "Remaining: " + FormatTime(duration - currentTime);
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    IEnumerator SpawnNotesRoutine()
    {
        float audioLeadIn = 0;
        if (float.TryParse(beatmapData.general.AudioLeadIn, out audioLeadIn))
        {
            yield return new WaitForSeconds(audioLeadIn / 1000f);
        }
        audioSource.Play();
        
        const float timeTolerance = 0.01f; 
        
        while (nextHitObjectIndex < sortedHitObjects.Count)
        {
            HitObject currentNote = sortedHitObjects[nextHitObjectIndex];
            float spawnTimeMs;
            if (!float.TryParse(currentNote.time, out spawnTimeMs))
            {
                Debug.LogWarning("Invalid time value in hitobject. Skipping note.");
                nextHitObjectIndex++;
                continue;
            }

            float targetTime = spawnTimeMs / 1000f;
            while (audioSource.time < targetTime - timeTolerance)
            {
                yield return null;
            }

            SpawnNote(currentNote);
            nextHitObjectIndex++;
        }
        Debug.Log("All notes spawned! Beatmap finished.");
    }

    void SpawnNote(HitObject note)
    {
        GameObject prefabToSpawn = null;
        Transform spawnPoint = null;
        float osuX;
        if (!float.TryParse(note.x, out osuX))
        {
            Debug.LogWarning("Invalid X value in hitobject. Cannot spawn note.");
            return;
        }

        if (osuX < 128)
        {
            prefabToSpawn = leftArrowPrefab;
            spawnPoint = leftLaneSpawnPoint;
        }
        else if (osuX >= 128 && osuX < 288)
        {
            prefabToSpawn = downArrowPrefab;
            spawnPoint = downLaneSpawnPoint;
        }
        else if (osuX >= 288 && osuX < 416)
        {
            prefabToSpawn = upArrowPrefab;
            spawnPoint = upLaneSpawnPoint;
        }
        else
        {
            prefabToSpawn = rightArrowPrefab;
            spawnPoint = rightLaneSpawnPoint;
        }

        if (prefabToSpawn != null && spawnPoint != null)
        {
            GameObject newArrow = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            MoveSpawn moveSpawnScript = newArrow.GetComponent<MoveSpawn>();
            if (moveSpawnScript != null)
            {
                float currentSpeed = beatForce;
                if(beatMultiplierActive)
                {
                    currentSpeed *= beatMultiplier;
                }
                moveSpawnScript.SetSpeed(currentSpeed);
            }
        }
    }
    
    IEnumerator BeatMultiplierRoutine()
    {
        while (pressure > 0)
        {
            yield return new WaitForSeconds(audioSource.clip.length / pressure);
            Debug.Log("Beat Multiplier Activated!");
            beatMultiplierActive = true;
            yield return new WaitForSeconds(beatMultiplierThreshold);
            beatMultiplierActive = false;
            Debug.Log("Beat Multiplier Deactivated.");
            pressure--;
        }
    }
    
    IEnumerator TempoShiftRoutine()
    {
        float totalDuration = audioSource.clip.length;
        float segmentTime = totalDuration / (tempoShiftCount + 1);
        for(int i = 0; i < tempoShiftCount; i++)
        {
            yield return new WaitForSeconds(segmentTime);
            
            audioSource.pitch = defaultAudioPitch * beatMultiplier;
            Debug.Log("Tempo Shift Activated!");
            
            if(bpmWarningText != null)
            {
                bpmWarningText.text = "TEMPO SHIFT!";
                bpmWarningText.color = Color.red;
                yield return new WaitForSeconds(bpmWarningDuration);
                bpmWarningText.text = "";
            }
            
            audioSource.pitch = defaultAudioPitch;
        }
    }
    
    TimingPoint GetActiveTimingPoint(float currentTimeMs) { return null; }
}