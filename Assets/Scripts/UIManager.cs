using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public Image StartScreen;
    public Image DownloadScreen;
    public Image ConceptsScreen;
    public Image LoadingScreen;

    [Header("Sliders")]
    public Slider[] ConceptsSlider;
    //public Slider SnowMountainSlider;
    public Slider LoadingScreenSlider;

    [Header("Buttons")]
    public Button StartScreenButton;
    public Button DownloadScreenButton;

    [Header("Concept Screen Buttons")]
    public Button[] concepScreenButtons;

    [Header("Boolean")]
    private bool conceptStatus = false;

    [Header("Concepts Name")]
    public string[] dataFileName = { "scene1-night", "scene2-snow" };
    string TempPath;

    private string SetPath() {
        string tempPath = Path.Combine(Application.persistentDataPath, "AssetData");
        return tempPath;
    }

    private string CombinePath(string path, string item) {
        string tempPath = null;
        tempPath = Path.Combine(path, item + ".unity3d");
        return tempPath;
    }

    private bool CheckFile() {
        string tempPath = null;
        string filePath = SetPath();
        foreach (var item in dataFileName) {
            tempPath = CombinePath(filePath, item);
            if (!System.IO.File.Exists(tempPath)) {
                conceptStatus = false;
                Debug.Log("File " + item + " is not exits");
            } else {
                conceptStatus = true;
                Debug.Log("File " + item + " exits");
            }
        }      
        return conceptStatus;
    }

    //StartScreen Next Button
    public void StartToNext() {
        StartScreen.gameObject.SetActive(false);
        if (!CheckFile()) {
            Debug.Log("Downloading Screen");
            DownloadScreen.gameObject.SetActive(true);
            StartCoroutine(DownloadAsset());
        } else {
            ConceptsScreen.gameObject.SetActive(true);
        }
    }

    //DownloadScreen Next Button
    public void DownloadToNext() {
        DownloadScreen.gameObject.SetActive(false);
        ConceptsScreen.gameObject.SetActive(true);
    }

    //SelectConcept
    public void SelectConcept(string conceptName) {
        LoadingScreen.gameObject.SetActive(true);
        foreach (var button in concepScreenButtons) {
            button.interactable = false;
        }
        StartCoroutine(LoadConceptFromMemory(CombinePath(SetPath(), conceptName)));
    }

    //Load Concept From Asset BUndle
    IEnumerator LoadConceptFromMemory(string path) {
        LoadingScreen.gameObject.SetActive(true);
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle bundle = createRequest.assetBundle;
        string[] scenePath = bundle.GetAllScenePaths();
        Debug.Log(scenePath[0]);

        AsyncOperation async = SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(scenePath[0]));
        while (!async.isDone) {
            int progress = (int)Mathf.Clamp01(async.progress / 0.9f);
            LoadingScreenSlider.value = progress;
            yield return null;
        }
    }

    //Download Asset
    IEnumerator DownloadAsset() {
        Debug.Log("Downloading Assets");
        string[] urls = {
                        "https://drive.google.com/uc?export=download&id=1jkURf6-kehF8M1zaay90AS8hdFJo4g6K",
                        "https://drive.google.com/uc?export=download&id=1W2NLo9FYS5ZLOpaJKu3cM67USSgh0nvO"
                        };
        for (int i = 0; i < urls.Length;i++) {
            Debug.Log("itenation" +i);
            WWW www = new WWW(urls[i]);
            yield return www;
            if (!string.IsNullOrEmpty(www.error)) {
                Debug.Log("Error while Downloading Data: " + www.error);
            } else {
                Debug.Log("Download Stat: " + www.progress);
                ConceptsSlider[i].value = www.progress;
                UnityEngine.Debug.Log("Success");
                //Save
                Save(www.bytes, CombinePath(SetPath(), dataFileName[i]));
            }
        }   
    }

    void Save(byte[] data, string path) {
        //Create the Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path))) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        try {
            File.WriteAllBytes(path, data);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
            //StartCoroutine(LoadObject(path));
        } catch (Exception e) {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }
}
