using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class LoadGame : MonoBehaviour
{
    public Text loadingText;
    public Slider sliderBar;
    public Button btn;
    public string bundlePath;

    public void GameScene() {
        sliderBar.gameObject.SetActive(true);
        //StartCoroutine(LoadFromMemoryAsync("AssetBundles/Android/scene1-night"));

        string dataFileName = "scene1-night";
        string tempPath = Path.Combine(Application.persistentDataPath, "AssetData");
        tempPath = Path.Combine(tempPath, dataFileName + ".unity3d");

        if (!System.IO.File.Exists(tempPath))
        {
            Debug.Log("File is not exits");
            StartCoroutine(downloadAsset());
        }
        else
	    {
            Debug.Log("File exits");
            StartCoroutine(LoadObject(tempPath));
	    }
        
        btn.gameObject.SetActive(false);
    }

    IEnumerator LoadFromMemoryAsync(string path) {
        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle bundle = createRequest.assetBundle;
        string[] scenePath = bundle.GetAllScenePaths();
        Debug.Log(scenePath[0]);

        AsyncOperation async = SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(scenePath[0]));
        while (!async.isDone) {
            int progress = (int)Mathf.Clamp01(async.progress / 0.9f);
            sliderBar.value = progress;
            loadingText.text = progress * 100f + "%";
            yield return null;
        }
    }

    IEnumerator downloadAsset()
    {
        string url = "https://drive.google.com/uc?export=download&id=1jkURf6-kehF8M1zaay90AS8hdFJo4g6K";

        WWW www = new WWW(url);
        yield return www;


       // if (!www.isDone)
        //{

          // Debug.Log("Error while Downloading Data: " + www.error);
        //}

        //else
        {
            Debug.Log("Download Stat: " + www.progress);
            UnityEngine.Debug.Log("Success");

            //handle.data

            //Construct path to save it
            string dataFileName = "scene1-night";
            string tempPath = Path.Combine(Application.persistentDataPath, "AssetData");
            tempPath = Path.Combine(tempPath, dataFileName + ".unity3d");

            //Save
            save(www.bytes, tempPath);
        }
    }

    void save(byte[] data, string path)
    {
        //Create the Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        try
        {
            File.WriteAllBytes(path, data);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
            StartCoroutine(LoadObject(path));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }

    IEnumerator LoadObject(string path)
    {
        /*if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            yield break;
        }*/

        AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return createRequest;
        AssetBundle bundle = createRequest.assetBundle;
        string[] scenePath = bundle.GetAllScenePaths();
        Debug.Log(scenePath[0]);

        AsyncOperation async = SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(scenePath[0]));
        while (!async.isDone)
        {
            int progress = (int)Mathf.Clamp01(async.progress / 0.9f);
            sliderBar.value = progress;
            loadingText.text = progress * 100f + "%";
            yield return null;
        }
    }
}
