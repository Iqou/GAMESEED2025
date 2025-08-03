using UnityEngine;
using System.Collections;

public class SceneLoadManager : MonoBehaviour
{
    [Header("Loading Screen Overlay")]
    public GameObject loadingScreenCanvas;

    [Header("Settings")]
    [Tooltip("The delay in seconds before hiding the loading screen to allow the scene to render.")]
    public float loadingDelay = 1.5f;

    void Start()
    {
        // Ensure the loading screen is active when the scene starts
        if (loadingScreenCanvas != null)
        {
            loadingScreenCanvas.SetActive(true);
            // Start the coroutine to hide it after a delay
            StartCoroutine(HideLoadingScreen());
        }
        else
        {
            Debug.LogError("Loading Screen Canvas is not assigned in the SceneLoadManager!");
        }
    }

    private IEnumerator HideLoadingScreen()
    {
        Debug.Log("Game scene loaded. Waiting for rendering...");

        // Wait for the specified delay
        yield return new WaitForSeconds(loadingDelay);

        // Hide the loading screen
        loadingScreenCanvas.SetActive(false);

        Debug.Log("Loading overlay hidden. Game is ready.");
    }
}
