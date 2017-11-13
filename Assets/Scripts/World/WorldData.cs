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
    public int MAXCHUNKHEIGHT = 8;
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
    /// <summary>
    /// Enables chunks around player, disables chunks outside of radius 
    /// </summary>
    /// <param name="radius">How many chunks to load</param>
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

    /// <summary>
    /// Creates initial chunks between xPos1 and xPos2.
    /// </summary>
    /// <param name="xPos1">First position</param>
    /// <param name="xPos2">Second Position</param>
    public void GenerateChunks(int xPos1, int xPos2) {
        for(int y = 0; y < MAXCHUNKHEIGHT; y++) {
            for(int x = xPos1; x < xPos2; x++) {
                CreateChunk(x, y);
            }
        }
    }
    /// <summary>
    /// Clears chunksLoaded list, cycles through each Chunk in World, calls 
    /// BuildMesh and UpdateMesh for each one and then re-adds to the list. 
    /// </summary>
    public void RefreshList() {
        chunksLoaded.Clear();
        foreach(Transform chunk in transform) {
            if(chunk.GetComponent<MeshBuilder>()) {
                chunk.GetComponent<MeshBuilder>().BuildMesh();
                chunk.GetComponent<MeshBuilder>().UpdateMesh();
                chunksLoaded.Add(chunk.gameObject);
            }
        }
    }
    /// <summary>
    /// Generates terrain from loaded chunks.
    /// </summary>
    public void GenTerrain() {
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().GenerateTerrain(reigons[0]);
        }
    }
    /// <summary>
    /// Saves all loaded chunks to current world directory.
    /// </summary>
    public void SaveChunks() {
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().SaveChunk(worldName);
        }
    }
    /// <summary>
    /// Loads chunks between xPos1 and xPos2 from current world directory.
    /// 
    /// </summary>
    /// <param name="xPos1">First position of chunks to load</param>
    /// <param name="xPos2">Second position of chunks to load</param>
    public void LoadChunks(int xPos1, int xPos2) {
        ClearWorldGrid();
        for(int y = 0; y < MAXCHUNKHEIGHT; y++) {
            for(int x = xPos1; x < xPos2; x++) {
                CreateChunk(x, y);
            }
        }
        for(int i = 0; i < chunksLoaded.Count; i++) {
            chunksLoaded[i].GetComponent<MeshBuilder>().LoadChunk(worldName);
            chunksLoaded[i].GetComponent<MeshBuilder>().BuildMesh();
            chunksLoaded[i].GetComponent<MeshBuilder>().UpdateMesh();
        }
    }
    /// <summary>
    /// Creates an air chunk (all tiles submesh 0) at position x and y and adds to chunksLoaded list.
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
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
    /// <summary>
    /// Sets tile at position of mouse.
    /// </summary>
    /// <param name="tile">Submesh number</param>
    public void ReplaceAtMousePos(int tile) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(mousePos);
        mousePos += new Vector3(mousePos.normalized.x, mousePos.normalized.y, 0) * -0.5f;
        //Debug.Log(mousePos);
        mousePos = new Vector3(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y), 0);
        //Debug.Log(mousePos);
        int x = (int)mousePos.x;
        int y = (int)mousePos.y;
        int cX = 0;
        int cY = 0;
        while(x >= 32) {
            x -= 32;
            cX++;
        }
        while(x <= -32) {
            x += 32;
            cX--;
        }
        while(y >= 32) {
            y -= 32;
            cY++;
        }
        if(x < 0) {
            x = 32 - Mathf.Abs(x);
            cX -= 1; //Chunk position correction
        }
        Debug.Log("X: " + x + " Y: " + y + "Chunk: " + cX + "," + cY);
        GameObject c = GameObject.Find("Chunk_" + cX + "," + cY);
        if(c == null) {
            CreateChunk(cX, cY);
            c = GameObject.Find("Chunk_" + cX + "," + cY);
        }

        c.GetComponent<MeshBuilder>().SetTile(x, y, tile);
        // }
    }

}

