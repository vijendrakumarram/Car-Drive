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
    public Image NoInternetScreen;

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

    [Header("URL")]
    public string url;

    public CarDriveDataHolder dataHolder;

    private string FixJson(string value) {
        string newStr1 = value.Substring(1, value.Length - 1);
        newStr1 = newStr1.Substring(0, newStr1.Length - 1);
        return "{\"items\":" + newStr1 + "}";
    }

    [System.Serializable]
    public class CarDriveDataHolder
    {
        public List<Items> items;
    }

    [System.Serializable]
    public class Items
    {
        public int id;
        public string concept;
        public string url;
    }

    private IEnumerator FetchDataFromServer() {
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (www.isDone) {
                dataHolder = JsonUtility.FromJson<CarDriveDataHolder>(FixJson(www.text));
            } else {
                Debug.Log(www.error);
            }
        }     
    }

    bool networkStatus = false;
    private void Start() {
        if (!networkStatus) {
            StartCoroutine(CheckInternetConnection());
            Debug.Log("Call");
        }
    }

    private IEnumerator CheckInternetConnection() {
        #if UNITY_EDITOR
            Debug.Log("Unity Editor");
            networkStatus = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

        #elif UNITY_ANDROID
            Debug.Log("Android");
            networkStatus = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
        #else
            Debug.Log("Any other platform");
        #endif

        yield return new WaitForSeconds(5f);
        if (!networkStatus) {
            NoInternetScreen.gameObject.SetActive(true);           
        } else {
            NoInternetScreen.gameObject.SetActive(false);
            StartCoroutine(FetchDataFromServer());
            StartScreenButton.gameObject.SetActive(true);           
        }
    }

    public void RetryConnectionCheck() {
        Debug.Log("Click");
        NoInternetScreen.gameObject.SetActive(false);
        StartCoroutine(CheckInternetConnection());
    }

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

    int increment = 0;
    //Download Asset
    IEnumerator DownloadAsset() {
                
        Debug.Log("Downloading Assets");
        string[] urls = { dataHolder.items[0].url,dataHolder.items[1].url };
        Debug.Log(urls[0] + "   " + urls[1]);
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
                if (increment == 2) {
                    DownloadScreenButton.gameObject.SetActive(true);
                }
                //Save
                Save(www.bytes, CombinePath(SetPath(), dataFileName[i]));
                increment++;
            }
        }
        
    }

    void  Save(byte[] data, string path) {
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
