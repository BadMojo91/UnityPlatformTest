using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
[System.Serializable]
public class ChunkSerialize{

    const string PATH = "/Saved/";
    const string EXT = ".chnk";
    public LevelData currentLevelData;
    public LevelData LoadChunk() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = File.Open(Application.dataPath + PATH + "chunk" + EXT, FileMode.Open);
        currentLevelData = (LevelData)bf.Deserialize(stream);
        stream.Close();

        return currentLevelData;
    }
    public void SaveChunks(LevelData ld) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = File.Create(Application.dataPath + PATH + "chunk" + EXT);
        bf.Serialize(stream, ld);
        stream.Close();
    }
}
