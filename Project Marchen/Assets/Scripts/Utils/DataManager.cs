using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int clearStage;
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

        if (!File.Exists(path)) // 파일 없으면
        {
            GameManager.instance.ClearStage = 0;
            //Save();
        }
        else
        {
            string loadJson = File.ReadAllText(path);
            saveData = JsonUtility.FromJson<SaveData>(loadJson);

            if (saveData != null)
            {
                GameManager.instance.ClearStage = saveData.clearStage;
            }
        }
    }

    public void Save()
    {
        SaveData saveData = new SaveData();
        saveData.clearStage = GameManager.instance.ClearStage;

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
    }
}

