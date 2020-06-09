using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    private string saveDataFilename = "player.dat";

    // Start is called before the first frame update
    void Awake()
    {
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        } else
        {
            Destroy(this.gameObject);
        }
        Load();
    }

    public void Save()
    {
        // Create a binary formatter which can read binary files.
        BinaryFormatter formatter = new BinaryFormatter();
        // Create a route from the program to the file.
        FileStream file = File.Open(Application.persistentDataPath + saveDataFilename, FileMode.Create);

        // Create a copy of save data.
        SaveData data = new SaveData();
        data = saveData;
        // Save data to file
        formatter.Serialize(file, data);
        // close the data stream
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + saveDataFilename))
        {
            Debug.Log("File Loaded");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + saveDataFilename, FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
        }
    }

    private void Start()
    {
      
    }

    private void OnDisable()
    {
        Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
