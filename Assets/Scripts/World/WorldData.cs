using UnityEngine;
//using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class WorldData : MonoBehaviour {

    public string worldName = "default";
    public bool editMode;
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

    public void Update() {
        if(!editMode)
            ChunkUpdate(1);
        else {
            foreach(Transform _chunk in transform) {
                if(_chunk != null && !_chunk.gameObject.activeSelf)
                _chunk.gameObject.SetActive(true);
            }
        }
        if(clearWorldGrid) {
            ClearWorldGrid();
        }

        if(!editMode && player == null)
            player = GameObject.Find("Player");

        if(!editMode) {
            playerWorldPos.x = Mathf.Round(player.transform.position.x);
            playerWorldPos.y = Mathf.Round(player.transform.position.y);
            playerWorldChunkPos = new Vector2(Mathf.Round(playerWorldPos.x / CHUNKSIZE), Mathf.Round(playerWorldPos.y / CHUNKSIZE));
        }
    }
    public void ChunkUpdate(int radius) {
        int cX = (int)player.GetComponent<Player>().worldPosCurrentChunk.x;
        int cY = (int)player.GetComponent<Player>().worldPosCurrentChunk.y;
        int cXL = cX - radius;
        int cXR = cX + radius;
        int cYU = cY + radius;
        int cYD = cY - radius;

        foreach(GameObject c in chunksLoaded) {
            Vector2 tempPos = c.GetComponent<MeshBuilder>().chunkPosition;
            if(tempPos.x > cXR || tempPos.y > cYU || tempPos.x < cXL || tempPos.y < cYD) {
                c.SetActive(false);
            }
            else {
                c.SetActive(true);
            }
        }
    }

    public void GenerateChunks(int width, int height) {
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                CreateChunk(x, y);
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
    public void ReplaceAtMousePos(int tile) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos += new Vector3(mousePos.normalized.x, mousePos.normalized.y, 0) * -0.5f;
        mousePos = new Vector3(Mathf.RoundToInt(mousePos.x - 0.5f), Mathf.RoundToInt(mousePos.y + 0.5f), 0);

        //Debug.Log(mousePos);
        int x = (int)mousePos.x;
        int y = (int)mousePos.y;
        int cX = 0;
        int cY = 0;
        //if(levelGrid.blocks[x + 1, y].subMesh != 0 || levelGrid.blocks[x - 1, y].subMesh != 0 || levelGrid.blocks[x, y + 1].subMesh != 0 || levelGrid.blocks[x, y - 1].subMesh != 0) {
        while(x >= 32) {
            x -= 32;
            cX++;
        }
        while(y >= 32) {
            y -= 32;
            cY++;
        }
        GameObject c = GameObject.Find("Chunk_" + cX + "," + cY);
        if(c == null) {
            CreateChunk(cX, cY);
            c = GameObject.Find("Chunk_" + cX + "," + cY);
        }

        c.GetComponent<MeshBuilder>().SetTile(x, y, tile);
        // }
    }

}

