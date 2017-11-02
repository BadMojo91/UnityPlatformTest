using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


public class WorldData : MonoBehaviour {
    const int CHUNKSIZE = 32;
    const float TILESCALE = 1;
    public SubmeshMaterial submeshMaterial;

    public Reigon reigon;
    public List<Chunk> chunkPool = new List<Chunk>();
    public List<GameObject> chunksLoaded = new List<GameObject>();
    public GameObject player;
    public Vector2 playerWorldPos;
    public Vector2 playerWorldChunkPos;

    public void Awake() {
        for(int x = 0; x < 10; x++) {
            for(int y = 0; y < 10; y++) {
                CreateChunk(x, y);
            }
        }
    }

    public void Update() {
        playerWorldPos.x = Mathf.Round(player.transform.position.x);
        playerWorldPos.y = Mathf.Round(player.transform.position.y);
        playerWorldChunkPos = new Vector2(Mathf.Round(playerWorldPos.x / CHUNKSIZE), Mathf.Round(playerWorldPos.y / CHUNKSIZE));
        UnloadChunks();
    }
    public void ChunkUpdate() {
        
    }
    public void UnloadChunks() {
        int i = 0;
        foreach(GameObject chunk in chunksLoaded) {
        //GameObject chunk = chunksLoaded[1];
            float dist = Vector3.Distance(player.transform.position, chunk.transform.position);
            //if(Input.GetButtonDown("Fire1"))
             //   Debug.Log(dist);
            if(dist > 50) {

                chunk.SetActive(false);
            }
            else {
                chunk.SetActive(true);
               
            }
            i++;
        }
    }


    public void LoadChunk(int x, int y) {
        if(GameObject.Find("Chunk_" + x + "," + y)) {
            GameObject newChunk = GameObject.Find("Chunk_" + x + "," + y);
            newChunk.GetComponent<MeshBuilder>().LoadChunk();
            newChunk.GetComponent<MeshBuilder>().BuildMesh();
            newChunk.GetComponent<MeshBuilder>().UpdateMesh();
        }
        else {
            Debug.LogError("LoadChunk() Cant find chunk:" + x + " " + y);
        }
    }
    public void CreateChunk(int x, int y) {
        GameObject newChunk;
        if(GameObject.Find("Chunk_" + x + "," + y))
            newChunk = GameObject.Find("Chunk_" + x + "," + y);
        else {
            newChunk = new GameObject();
            
            newChunk.transform.SetParent(transform);
            newChunk.name = "Chunk_" + x + "," + y;
            newChunk.AddComponent<MeshRenderer>();
            newChunk.AddComponent<MeshFilter>();
            newChunk.AddComponent<MeshCollider>();
            newChunk.AddComponent<MeshBuilder>();
            newChunk.GetComponent<MeshBuilder>().worldData = this;
            chunksLoaded.Add(newChunk);
        }
        newChunk.transform.position = new Vector3(x*CHUNKSIZE, y*CHUNKSIZE);
        newChunk.GetComponent<MeshBuilder>().BuildNewMesh();
        //newChunk.GetComponent<MeshBuilder>().LoadChunk();
        //newChunk.GetComponent<MeshBuilder>().BuildMesh();
        newChunk.GetComponent<MeshBuilder>().UpdateMesh();
        newChunk.GetComponent<MeshBuilder>().SaveToChunk();

    }

}
[System.Serializable]
public class Reigon {
    public float frequency;
    public float amplitude;
}
