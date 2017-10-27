using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
[ExecuteInEditMode]
public class LevelGrid : MonoBehaviour
{
    //Grid
    public GameObject[,] chunks;
    public Block[,] blocks;
    public BlockData[] blockData;
    public float TILE_SCALE = 0.32f; //size of grid tiles
    public int MAX_CHUNK_SIZE = 64; //max size of grid chunk


    //Mesh
    private Mesh mesh;
    List<SubMeshes> subMeshes = new List<SubMeshes>();
    List<Vector3> verts = new List<Vector3>();
    List<int> tri = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int colCount; //collider index for triangles
    int tileCount;//mesh index for triangles
    //Collider
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();
    
    //Textures

    //Inspector
    [Header("Mesh")]
    public MeshFilter meshFilter;
    public MeshRenderer meshRend;
    public MeshCollider col;

    [Header("Textures")]
    public Texture2D[] tex;

    [Header("Terrain")]
    public int maxHeight;
    public int seed;
    public float scale;
    public float magnitude;

    public bool clearGrid;
    public void GUIClear() {
        clearGrid = true;
    }
    private void Awake()
    {
        LoadLevelData();
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        
        if(clearGrid)
        {
            StartCoroutine(ClearGrid());
            clearGrid = false;
            
        }
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
    public IEnumerator ClearGrid()
    {
        List<Transform> chunkList = new List<Transform>();
        foreach(Transform o in transform)
        {
            chunkList.Add(o);
            
        }
        for(int i = 0; i < chunkList.Count; i++)
        {     
           DestroyImmediate(chunkList[i].gameObject);
        }
        yield return new WaitForEndOfFrame();
    }
    public void CreateChunks(int size)
    {
        StartCoroutine(ClearGrid());
        chunks = new GameObject[size, maxHeight];
        for(int y = 0; y < maxHeight; y++)
        {
            for(int x = 0; x < size; x++)
            {
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
    public void GenerateTerrain(GameObject chunk)
    {
 
        Block[,] b = new Block[MAX_CHUNK_SIZE, MAX_CHUNK_SIZE];
        float offsetX, offsetY;
        offsetX = chunk.transform.position.x;
        offsetY = chunk.transform.position.y;
        for(int y = 0; y < b.GetLength(1); y++)
        {
            for(int x = 0; x < b.GetLength(0); x++)
            {
                Block block = new Block();
                float perlinNoise = Mathf.PerlinNoise(seed+x*scale, -seed+y*scale) / magnitude * scale;

                if(perlinNoise*60 < y)
                    block.subMesh = 0;
                else if(perlinNoise > 0.2f)
                    block.subMesh = 2;
                else if(perlinNoise > 0.1f)
                    block.subMesh = 3;
                else
                    block.subMesh = 0;

                b[x, y] = block;
            }
        }

        chunk.GetComponent<Chunk>().blocks = b;
    }
    public void DestroyTileAt(Chunk c, int x, int y)
    {
        //Debug.Log(blocks.GetLength(0) + " " + blocks.GetLength(1));
        c.blocks[x, y].subMesh = 0;
        c.BuildMesh();
    }
    public void CreateTileAt(int x, int y, int t, bool build)
    {
        blocks[x, y].subMesh = t;
        SaveLevelData();
        if(build)
        {
           // BuildMesh();
           // UpdateMeshData();
        }
    }
    public void CreateLineAt(Vector2 start, Vector2 end, int t)
    {
        float dist = Vector2.Distance(start, end);
        CreateTileAt((int)start.x, (int)start.y, t, false);
        for(int i = 0; i < dist; i++)
        {
            float frac = i / dist;
            Vector2 pos = Vector2.Lerp(start, end, frac);
           // Debug.Log((int)pos.x + " " + (int)pos.y);
            CreateTileAt((int)pos.x, (int)pos.y, t, false);
        }
        CreateTileAt((int)end.x, (int)end.y, t, true);
    }
    public void DestroyAllTiles()
    {
        for(int y = 0; y < MAX_CHUNK_SIZE; y++)
        {
            for(int x = 0; x < MAX_CHUNK_SIZE; x++)
            {
                foreach(GameObject c in chunks)
                {
                    c.GetComponent<Chunk>().blocks[x, y].subMesh = 0;
                } 
            }
        }
        foreach(GameObject c in chunks)
        {
            c.GetComponent<Chunk>().BuildMesh();
        }
    }
    public void SaveLevelData()
    {
        List<BlockData> bd = new List<BlockData>();
        for(int y = 0; y < blocks.GetLength(1); y++)
        {
            for(int x = 0; x < blocks.GetLength(0); x++)
            {
                bd.Add(new BlockData(x, y, blocks[x, y].subMesh));

            }
        }
        blockData = bd.ToArray();
    }
    public void LoadLevelData()
    {
        Block[,] b = new Block[MAX_CHUNK_SIZE, MAX_CHUNK_SIZE];
        for(int y = 0; y < b.GetLength(1); y++)
        {
            for(int x = 0; x < b.GetLength(0); x++)
            {
                b[x, y] = new Block();
            }
        }

        foreach(BlockData bd in blockData)
        {
            b[bd.x, bd.y].subMesh = bd.subMesh;
        }

        blocks = b;
        SaveLevelData();
    }
}
[System.Serializable]
public class SubMeshes
{
    public List<int> triangles = new List<int>();
}
[System.Serializable]
public class Block
{

    public int subMesh;
    public int[] triangles;

    public Block()
    {
        subMesh = 0;
        triangles = new int[6];
    }
}
[System.Serializable]
public class BlockData
{
    public int x;
    public int y;
    public int subMesh;
    public BlockData(int _x, int _y, int _s)
    {
        x = _x;
        y = _y;
        subMesh = _s;
    }
}
