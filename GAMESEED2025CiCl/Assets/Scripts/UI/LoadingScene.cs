using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Image LoadingBarFill;

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        // Jadi sebelum scenenya load, jangan diaktifin dulu
        operation.allowSceneActivation = false; 

        LoadingScreen.SetActive(true);

        // Loadingnya ampe 90% loaded aja
        while (operation.progress < 0.9f)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarFill.fillAmount = progressValue;

            Debug.Log($"Loading... {Mathf.RoundToInt(progressValue * 100)}%");

            yield return null;
        }

        Debug.Log("Loading... 100%");
        LoadingBarFill.fillAmount = 1f; 

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Done loading. Activating scene.");
        operation.allowSceneActivation = true; 
    }
}
