using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public float noteApproachTime = 1.5f; 

    private BeatmapData beatmapData;
    private List<HitObject> sortedHitObjects;
    private int nextHitObjectIndex = 0;

    void Start()
    {
        if (beatmapJsonFile == null)
        {
            Debug.LogError("Beatmap JSON file not assigned to RhythmManager! Disabling script.");
            enabled = false;
            return;
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component not found on RhythmManager or assigned! Disabling script.");
                enabled = false;
                return;
            }
        }
        if (leftArrowPrefab == null || downArrowPrefab == null ||
            upArrowPrefab == null || rightArrowPrefab == null)
        {
            Debug.LogError("One or more Arrow Prefabs are not assigned! Disabling script.");
            enabled = false;
            return;
        }
        if (leftLaneSpawnPoint == null || downLaneSpawnPoint == null ||
            upLaneSpawnPoint == null || rightLaneSpawnPoint == null)
        {
            Debug.LogError("One or more Lane Spawn Points are not assigned! Disabling script.");
            enabled = false;
            return;
        }

        LoadBeatmap();
        
        if (beatmapData != null && audioSource.clip != null)
        {
            StartCoroutine(SpawnNotesRoutine());
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
                Debug.LogError("Audio clip not found in Resources folder: '" + audioResourcePath + "'. Please check filename and placement (e.g., Assets/Resources/" + audioResourcePath + ".mp3).");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse beatmap JSON: " + e.Message + "\nJSON Content:\n" + jsonString);
            beatmapData = null;
        }
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
                Debug.LogWarning("Invalid time value in hitobject: " + currentNote.time + ". Skipping note.");
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
            Debug.LogWarning("Invalid X value in hitobject: " + note.x + ". Cannot spawn note.");
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
                float noteTimeMs;
                if (!float.TryParse(note.time, out noteTimeMs))
                {
                    Debug.LogWarning("Could not parse note time for speed calculation.");
                    return;
                }
                
                TimingPoint activeTp = GetActiveTimingPoint(noteTimeMs);

                float millPerBeat;
                if (!float.TryParse(activeTp.millperbeat, out millPerBeat) || millPerBeat == 0)
                {
                    Debug.LogWarning("Invalid millperbeat value for active timing point. Using default speed.");
                    return;
                }

                float distanceToTravel = Mathf.Abs(transform.position.y - spawnPoint.position.y); 

                float calculatedSpeed = distanceToTravel / noteApproachTime;

                moveSpawnScript.SetSpeed(calculatedSpeed);
            }
            else
            {
                Debug.LogWarning("Prefab '" + prefabToSpawn.name + "' does not have a MoveSpawn script. Arrow will not move.");
            }
        }
        else
        {
            Debug.LogWarning("Could not spawn note for X: " + osuX + ". Prefab or Spawn Point not assigned/recognized. Prefab assigned: " + (prefabToSpawn != null) + ", SpawnPoint assigned: " + (spawnPoint != null));
        }
    }

    TimingPoint GetActiveTimingPoint(float currentTimeMs)
    {
        TimingPoint activeTp = beatmapData.timingpoints[0];
        foreach (var tp in beatmapData.timingpoints)
        {
            float offsetMs;
            if (float.TryParse(tp.offset, out offsetMs))
            {
                if (offsetMs <= currentTimeMs)
                {
                    activeTp = tp;
                }
                else
                {
                    break; 
                }
            }
            else
            {
                Debug.LogWarning("Invalid offset value in timingpoint: " + tp.offset);
            }
        }
        return activeTp;
    }
}