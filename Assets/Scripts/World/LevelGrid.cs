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
    public int seed;
    public float scale;
    public float magnitude;

    private void Awake()
    {
        LoadLevelData();
    }
    private void Start()
    {
        //BuildMesh();
        //UpdateMeshData();
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
    public void LoadMap()
    {

    }
    

    public void CreateChunks(int size)
    {
        chunks = new GameObject[size, size];
        for(int y = 0; y < size; y++)
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
                float perlinNoise = Mathf.PerlinNoise((x+offsetX)*scale, (y+offsetY)*scale) / magnitude;

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

    public void BuildMesh() //builds mesh based on blocks[,]
    {
        LoadLevelData();
        subMeshes.Clear();
        for(int i = 0; i < tex.Length + 1; i++) {
            SubMeshes sm = new SubMeshes();
            sm.triangles = new List<int>();
            subMeshes.Add(sm);
        }

        for(int y = 0; y < MAX_CHUNK_SIZE; y++){
            for(int x = 0; x < MAX_CHUNK_SIZE; x++){

                if(blocks[x, y].subMesh != 0)
                    GenerateCollider(x, y); //only generate collider on solid tiles

                CreateVerts(x, y); 
                CreateTile(blocks[x, y], x, y);
               
            }
        }
        tileCount = 0;
    }
    private void ColTriangles()
    {
        colTri.Add(colCount * 4);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 3);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 2);
        colTri.Add((colCount * 4) + 3);
    }
    private void GenerateCollider(float x, float y)
    {
        float i = TILE_SCALE;
        x = x * TILE_SCALE;
        y = y * TILE_SCALE;

        //Top
        colVerts.Add(new Vector3(x, y, i));
        colVerts.Add(new Vector3(x + i, y, i));
        colVerts.Add(new Vector3(x + i, y, 0));
        colVerts.Add(new Vector3(x, y, 0));
        ColTriangles();
        colCount++;
        //Left
        colVerts.Add(new Vector3(x, y - i, i));
        colVerts.Add(new Vector3(x, y, i));
        colVerts.Add(new Vector3(x, y, 0));
        colVerts.Add(new Vector3(x, y - i, 0));
        ColTriangles();
        colCount++;
        //Right
        colVerts.Add(new Vector3(x + i, y, i));
        colVerts.Add(new Vector3(x + i, y - i, i));
        colVerts.Add(new Vector3(x + i, y - i, 0));
        colVerts.Add(new Vector3(x + i, y, 0));
        ColTriangles();
        colCount++;
        //Bottom
        colVerts.Add(new Vector3(x + i, y - i, i));
        colVerts.Add(new Vector3(x, y - i, i));
        colVerts.Add(new Vector3(x, y - i, 0));
        colVerts.Add(new Vector3(x + i, y - i, 0));
        ColTriangles();
        colCount++;
    }
    public void UpdateMeshData()
    {
        SaveLevelData();
        mesh = new Mesh();
        mesh.Clear();

        mesh.vertices = verts.ToArray();

        mesh.subMeshCount = tex.Length + 1;
        for(int i = 0; i < subMeshes.Count; i++)
        {
            mesh.SetTriangles(subMeshes[i].triangles, i);

        }

        for(int i = 0; i < mesh.subMeshCount; i++)
        {
            if(mesh.GetTriangles(i).Length < 3)
            {
                mesh.SetTriangles(new int[3] { 0, 0, 0 }, i);
            }
        }

        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;

        Mesh colMesh = new Mesh();
        colMesh.name = "Collider Mesh";
        colMesh.vertices = colVerts.ToArray();
        colMesh.triangles = colTri.ToArray();
        col.sharedMesh = colMesh;

        //Cleanup
        colVerts.Clear();
        colTri.Clear();
        colCount = 0;
        verts.Clear();
        tri.Clear();
        uvs.Clear();

    }
    private void CreateVerts(float x, float y)
    {
        float i = TILE_SCALE;
        x = x * TILE_SCALE;
        y = y * TILE_SCALE;
        verts.Add(new Vector2(x, y));
        verts.Add(new Vector2(x + i, y));
        verts.Add(new Vector2(x + i, y - i));
        verts.Add(new Vector2(x, y - i));

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

    }
    private void CreateTile(Block block, float x, float y)
    {

        List<int> t = new List<int>();

        t.Add(tileCount * 4);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 3);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 2);
        t.Add((tileCount * 4) + 3);


        block.triangles = t.ToArray();

        for(int i = 0; i < 6; i++)
        {
            //Debug.Log(block.subMesh);
            subMeshes[block.subMesh].triangles.Add(t[i]);
        }

        tileCount++;
    }
    public void DestroyTileAt(int x, int y)
    {
        //Debug.Log(blocks.GetLength(0) + " " + blocks.GetLength(1));
        blocks[x, y].subMesh = 0;
        SaveLevelData();
        BuildMesh();
        UpdateMeshData();
    }
    public void CreateTileAt(int x, int y, int t, bool build)
    {
        blocks[x, y].subMesh = t;
        SaveLevelData();
        if(build)
        {
            BuildMesh();
            UpdateMeshData();
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
