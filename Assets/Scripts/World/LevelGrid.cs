using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
[ExecuteInEditMode]
public class LevelGrid : MonoBehaviour {

    //LevelData
    public LevelData currentLevelData;

    //Grid
    public GameObject[,] chunks;
    public Block[,] blocks;
    public ChunkData cData;
    public float TILE_SCALE = 0.32f; //size of grid tiles
    public int MAX_CHUNK_SIZE = 64; //max size of grid chunk


    //Mesh
    List<SubMeshes> subMeshes = new List<SubMeshes>();
    List<Vector3> verts = new List<Vector3>();
    List<int> tri = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    //Collider
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();

    //Textures

    //Inspector

    [Header("Textures")]
    public Texture2D[] tex;

    [Header("Terrain")]
    public int maxHeight;
    public int seed;
    [Range(0.001f, 0.999f)]
    public float frequency;
    [Range(0.1f, 10)]
    public float amplitude;
    public float anotherValue;

    public bool clearGrid;
    public void GUIClear() {
        clearGrid = true;
    }
    private void Awake() {
        LoadLevelData();
    }
    private void Start() {
        CreateChunks(5);
    }
    private void Update() {

        if(clearGrid) {
            StartCoroutine(ClearGrid());
            clearGrid = false;

        }
    }
    private void OnValidate() {
        //UpdateChunks();
    }
    //public void ConvertImageToMap(Texture2D image)
    //{

    //    int h = image.height;
    //    int w = image.width;

    //    if (h > MAX_CHUNK_SIZE || w > MAX_CHUNK_SIZE)
    //    {
    //        Debug.LogError("Image size exceeds MAX_CHUNK_SIZE.");
    //        return;
    //    }
    //    else
    //    {
    //        Tile[,] tilePos = new Tile[w, h];

    //        for (int y = 0; y < 32; y++)
    //        {
    //            for (int x = 0; x < 32; x++)
    //            {
    //                tilePos[x, y].colour = image.GetPixel(x, y);
    //                foreach (Tile t in tileArray)
    //                {
    //                    if (t.colour == tilePos[x, y].colour)
    //                        tilePos[x, y].sprite = t.sprite;
    //                }
    //                if (tilePos[x, y].sprite == null)
    //                    tilePos[x, y].sprite = defaultSprite;
    //                //Debug.Log(colorPos[x, y]);
    //            }
    //        }
    //        currentMap.mapName = image.name;
    //        currentMap.tiles = tilePos;
    //        currentMap.width = w;
    //        currentMap.height = h;
    //    }

    //    CreateGrid(currentMap);
    //}
    public IEnumerator ClearGrid() {
        List<Transform> chunkList = new List<Transform>();
        
        foreach(Transform o in transform) {
            chunkList.Add(o);
        }

        for(int i = 0; i < chunkList.Count; i++) {
            DestroyImmediate(chunkList[i].gameObject);
        }

        yield return new WaitForEndOfFrame();
    }

    public void CreateChunks(int size) {
        if(!EditorApplication.isPlaying)
        StartCoroutine(ClearGrid());
        else
        foreach(Transform o in transform) {
            Destroy(o.gameObject);
        }

        chunks = new GameObject[size, maxHeight];
        for(int y = 0; y < maxHeight; y++) {
            for(int x = 0; x < size; x++) {
                Vector2 offset;
                GameObject newChunk = new GameObject();
                newChunk.AddComponent<MeshFilter>();
                newChunk.AddComponent<MeshCollider>();
                newChunk.AddComponent<MeshRenderer>();
                newChunk.AddComponent<Chunk>();
               
                newChunk.GetComponent<Chunk>().levelGrid = GetComponent<LevelGrid>();

                offset.x = x * TILE_SCALE * MAX_CHUNK_SIZE;
                offset.y = y * TILE_SCALE * MAX_CHUNK_SIZE;

                newChunk.transform.position = offset;
                newChunk.name = "Chunk " + x + ", " + y;
                newChunk.transform.SetParent(transform);
                GenerateTerrain(newChunk);

                newChunk.GetComponent<Chunk>().BuildMesh();

                chunks[x, y] = newChunk;
            }
        }
    }
    public void UpdateChunks() {
        if(chunks == null)
            return;
        for(int y = 0; y < chunks.GetLength(1); y++) {
            for(int x = 0; x < chunks.GetLength(0); x++) {
                Chunk chunk = chunks[x, y].GetComponent<Chunk>();
               // GenerateTerrain(chunks[x,y]);
                chunk.BuildMesh();
            }
        }
        SaveLevelData();
    }
    public void GenerateTerrain(GameObject chunk) {

        Block[,] b = new Block[MAX_CHUNK_SIZE, MAX_CHUNK_SIZE];
        float offsetX, offsetY;
        offsetX = chunk.transform.position.x;
        offsetY = chunk.transform.position.y;
        for(int y = 0; y < b.GetLength(1); y++) {
            for(int x = 0; x < b.GetLength(0); x++) {
                Block block = new Block();
                float perlinNoise = Mathf.PerlinNoise(((offsetX/TILE_SCALE) + x) * frequency, ((offsetY/TILE_SCALE) + y) * frequency) * amplitude;

                //Debug.Log(perlinNoise);
                if(perlinNoise > 0.5f)
                    block.subMesh = 0;
                else if(perlinNoise > 0.3f)
                    block.subMesh = 2;
                else
                    block.subMesh = 1;

                b[x, y] = block;
            }
        }

        chunk.GetComponent<Chunk>().blocks = b;
       
    }
    public void DestroyTileAt(Chunk c, int x, int y) {
        //Debug.Log(blocks.GetLength(0) + " " + blocks.GetLength(1));
        c.blocks[x, y].subMesh = 0;
        c.BuildMesh();
    }
    public void CreateTileAt(int x, int y, int t, bool build) {
        blocks[x, y].subMesh = t;
        SaveLevelData();
        if(build) {
            // BuildMesh();
            // UpdateMeshData();
        }
    }
    public void CreateLineAt(Vector2 start, Vector2 end, int t) {
        float dist = Vector2.Distance(start, end);
        CreateTileAt((int)start.x, (int)start.y, t, false);
        for(int i = 0; i < dist; i++) {
            float frac = i / dist;
            Vector2 pos = Vector2.Lerp(start, end, frac);
            // Debug.Log((int)pos.x + " " + (int)pos.y);
            CreateTileAt((int)pos.x, (int)pos.y, t, false);
        }
        CreateTileAt((int)end.x, (int)end.y, t, true);
    }
    public void DestroyAllTiles() {
        for(int y = 0; y < MAX_CHUNK_SIZE; y++) {
            for(int x = 0; x < MAX_CHUNK_SIZE; x++) {
                foreach(GameObject c in chunks) {
                    c.GetComponent<Chunk>().blocks[x, y].subMesh = 0;
                }
            }
        }
        foreach(GameObject c in chunks) {
            c.GetComponent<Chunk>().BuildMesh();
        }
    }
    public void SaveLevelData() {

        List<ChunkData> chunkData = new List<ChunkData>(); 
        for(int y = 0; y < chunks.GetLength(1); y++) {
            for(int x = 0; x < chunks.GetLength(0); x++) {
                List<BlockData> blockData = new List<BlockData>();
                ChunkData cd = new ChunkData();
                Chunk c = chunks[x, y].GetComponent<Chunk>();
                cd.x = x;
                cd.y = y;
                for(int y2 = 0; y2 < c.blocks.GetLength(1); y2++) {
                    for(int x2 = 0; x2 < c.blocks.GetLength(0); x2++) {
                        blockData.Add(new BlockData(x2, y2, c.blocks[x2, y2].subMesh));
                    }
                }
                cd.blockData = blockData.ToArray();
                chunkData.Add(cd);
            }
        }
       
        LevelData ld = new LevelData(chunks.GetLength(0), chunks.GetLength(1), chunkData.ToArray());
    }
    public void LoadLevelData() {
        if(currentLevelData == null)
            return;

        int w = currentLevelData.width;
        int h = currentLevelData.height;
 
        foreach(ChunkData chDat in currentLevelData.chunkData) {
            for(int y = 0; y < h; y++) {
                for(int x = 0; x < w; x++) {
                    if(chDat.x == x && chDat.y == y) {
                        Chunk newChunk = new Chunk();
                        Block[,] newBlocks = new Block[MAX_CHUNK_SIZE, MAX_CHUNK_SIZE];
                        foreach(BlockData block in chDat.blockData) {
                            for(int y2 = 0; y2 < MAX_CHUNK_SIZE; y2++) {
                                for(int x2 = 0; x2 < MAX_CHUNK_SIZE; x2++) {
                                    if(x2 == block.x && y2 == block.y) {
                                        Block newBlock = new Block();
                                        newBlock.subMesh = block.subMesh;
                                        newBlocks[x2, y2] = newBlock;
                                    }
                                }
                            }                           
                        }
                        chunks[x, y].GetComponent<Chunk>().blocks = newBlocks;
                        
                      
                    }                 
                }
            }
        }
        UpdateChunks();
        
      
        
       

    }
}
[System.Serializable]
public class SubMeshes {
    public List<int> triangles = new List<int>();
}
[System.Serializable]
public class Block {

    public int subMesh;
    public int[] triangles;

    public Block() {
        subMesh = 0;
        triangles = new int[6];
    }
}
[System.Serializable]
public class BlockData {
    public int x;
    public int y;
    public int subMesh;
    public BlockData(int _x, int _y, int _s) {
        x = _x;
        y = _y;
        subMesh = _s;
    }

    
}
[System.Serializable]
public class ChunkData {
    public int x, y;
    public BlockData[] blockData;
}
[System.Serializable]
public class LevelData {
    public int width;
    public int height;
    public ChunkData[] chunkData;

    public LevelData(int _x, int _y, ChunkData[] cd) {
        width = _x;
        height = _y;
        chunkData = cd;
    }

}
