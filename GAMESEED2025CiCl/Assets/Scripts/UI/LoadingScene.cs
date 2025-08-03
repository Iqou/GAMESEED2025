using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Image LoadingBarFill;
    public TextMeshProUGUI LoadingPercentageText;

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

IEnumerator LoadSceneAsync(int sceneId)
{
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
    operation.allowSceneActivation = false;

    LoadingScreen.SetActive(true);

    float fakeProgress = 0f;

    while (fakeProgress < 0.9f)
    {
        // Simulate progress
        fakeProgress += Time.deltaTime * 0.5f; // Adjust speed as needed
        float clampedProgress = Mathf.Clamp01(fakeProgress);

        LoadingBarFill.fillAmount = clampedProgress;
        LoadingPercentageText.text = Mathf.RoundToInt(clampedProgress * 100f) + "%";

        yield return null;
    }

    // Hold at 100% briefly
    LoadingBarFill.fillAmount = 1f;
    LoadingPercentageText.text = "100%";
    yield return new WaitForSeconds(0.5f);

    operation.allowSceneActivation = true;
}



}
