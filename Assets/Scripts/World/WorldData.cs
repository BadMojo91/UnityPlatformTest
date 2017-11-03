using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class WorldData : MonoBehaviour {

    public string worldName = "default";

    const int CHUNKSIZE = 32;
    const float TILESCALE = 1;
    public SubmeshMaterial submeshMaterial;

    public Reigon[] reigons;
    public List<Chunk> chunkPool = new List<Chunk>();
    public List<GameObject> chunksLoaded = new List<GameObject>();
    public GameObject player;
    public Vector2 playerWorldPos;
    public Vector2 playerWorldChunkPos;
    public bool clearWorldGrid;

    void ClearWorldGrid() {
        RefreshList();
        for(int i = 0; i < chunksLoaded.Count; i++) {
            DestroyImmediate(chunksLoaded[i]);
        }
        if(transform.childCount <= 0) {
            chunksLoaded.Clear();
            clearWorldGrid = false;
        }
    }

    public void Start() {
        RefreshList();

    }
    int testInt = 0;
    public void Update() {

        if(clearWorldGrid) {
            ClearWorldGrid();
        }

        if(player == null)
            player = GameObject.Find("Player");
        if(Input.GetButtonDown("Fire2")) {
            for(int y = 0; y < 10; y++) {      
                CreateChunk(testInt, y);
            }
            testInt--;
            RefreshList();
        }
        playerWorldPos.x = Mathf.Round(player.transform.position.x);
        playerWorldPos.y = Mathf.Round(player.transform.position.y);
        playerWorldChunkPos = new Vector2(Mathf.Round(playerWorldPos.x / CHUNKSIZE), Mathf.Round(playerWorldPos.y / CHUNKSIZE));
        UnloadChunks();
    }
    public void ChunkUpdate() {
        for(int x = 0; x < 10; x++) {
            for(int y = 0; y < 10; y++) {
                CreateChunk(x, y);
            }
        }
    }
    public void UnloadChunks() {
        if(EditorApplication.isPlaying) {
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
    }
    void RefreshList() {
        chunksLoaded.Clear();
        foreach(Transform chunk in transform) {
            if(chunk.GetComponent<MeshBuilder>()) {
                chunk.GetComponent<MeshBuilder>().BuildMesh();
                chunk.GetComponent<MeshBuilder>().UpdateMesh();
                chunksLoaded.Add(chunk.gameObject);
            }
        }
    }
    public void GenTerrain() {
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().GenerateTerrain(reigons[0]);
        }
    }

    public void SaveChunks() {
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().SaveChunk(worldName);
        }
    }

    public void LoadChunks(int width, int height) {
        ClearWorldGrid();
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                CreateChunk(x, y);
            }
        }
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().LoadChunk(worldName);
            chunksLoaded[i].GetComponent<MeshBuilder>().BuildMesh();
            chunksLoaded[i].GetComponent<MeshBuilder>().UpdateMesh();
        }
    }
    public void CreateChunk(int x, int y) { //just create a fresh chunk
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
        newChunk.GetComponent<MeshBuilder>().chunkPosition = new Vector2(x, y);
        newChunk.GetComponent<MeshBuilder>().InitChunk();
        newChunk.GetComponent<MeshBuilder>().BuildMesh();
        newChunk.GetComponent<MeshBuilder>().UpdateMesh();

        bool existsInList = false;
        foreach(GameObject o in chunksLoaded) {
            if(o == null)
                return;

            if(o.name == newChunk.name) {
                existsInList = true;
                return;
            }
        }
        if(!existsInList)
            chunksLoaded.Add(newChunk);

    }

}

