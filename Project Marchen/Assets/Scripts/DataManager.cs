using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public bool aliceStageClear;
    public bool desertStageClear;
}

public class DataManager : MonoBehaviour
{
    /* ------------ 저장 위치 변수 ------------- */
    private string path;

    /* -------------- 이벤트 함수 -------------- */
    void Start()
    {
        path = Path.Combine(Application.dataPath, "SaveData.json");
        Load();
    }

    /* --------------- 기능 함수 --------------- */
    public void Load()
    {
        SaveData saveData = new SaveData();

        if (!File.Exists(path))
        {
            GameManager.instance.AliceStageClear = false;
            GameManager.instance.DesertStageClear = false;
            Save();
        }
        else
        {
            string loadJson = File.ReadAllText(path);
            saveData = JsonUtility.FromJson<SaveData>(loadJson);

            if (saveData != null)
            {
                GameManager.instance.AliceStageClear = saveData.aliceStageClear;
                GameManager.instance.DesertStageClear = saveData.desertStageClear;
            }
        }
    }

    public void Save()
    {
        SaveData saveData = new SaveData();

        saveData.aliceStageClear = GameManager.instance.AliceStageClear;
        saveData.desertStageClear = GameManager.instance.DesertStageClear;

        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(path, json);
    }
}

