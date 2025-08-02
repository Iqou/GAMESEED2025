// using UnityEngine;
// using System.IO;

// public class SaveManager : MonoBehaviour
// {
//     private static string SavePath => Path.Combine(Application.persistentDataPath, "progression.json");
//     public static LongProgression CurrentProgression = new LongProgression();

//     private static float sessionStartTime;

//     void Awake()
//     {
//         // Load progression at game start
//         LoadProgression();
        
//         // Register application quit event
//         Application.quitting += OnAppQuit;

//         // Start session
//         sessionStartTime = Time.time;
//         CurrentProgression.sessionCount++;
//         Debug.Log("Session started. Total sessions: " + CurrentProgression.sessionCount);
//     }

//     void Update()
//     {
//         // Optional: auto-save every 60 seconds
//         // Could be improved with a coroutine for performance
//     }

//     private void OnAppQuit()
//     {
//         // Calculate playtime for this session
//         int sessionDuration = Mathf.FloorToInt(Time.time - sessionStartTime);
//         CurrentProgression.totalPlaytime += sessionDuration;
//         CurrentProgression.lastPlayTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//         SaveProgression();
//         Debug.Log($"Session ended. Duration: {sessionDuration}s | Total Playtime: {CurrentProgression.totalPlaytime}s");
//     }

//     public static void SaveProgression()
//     {
//         string json = JsonUtility.ToJson(CurrentProgression, true);
//         File.WriteAllText(SavePath, json);
//         Debug.Log("Progression Saved: " + SavePath);
//     }

//     public static void LoadProgression()
//     {
//         if (File.Exists(SavePath))
//         {
//             string json = File.ReadAllText(SavePath);
//             CurrentProgression = JsonUtility.FromJson<LongProgression>(json);
//             Debug.Log("Progression Loaded!");
//         }
//         else
//         {
//             CurrentProgression = new LongProgression();
//             Debug.Log("No save found, creating new progression.");
//         }
//     }

//     public static void ResetProgression()
//     {
//         CurrentProgression = new LongProgression();
//         SaveProgression();
//         Debug.Log("Progression Reset.");
//     }
// }
