using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
[ExecuteInEditMode]
public class LevelGrid : MonoBehaviour
{
    public Sprite defaultSprite;
    public Sprite activeSprite;
    public GameObject[,] gridPos = new GameObject[MAX_MAP_SIZE, MAX_MAP_SIZE];
    public bool drawMode;

    public Map currentMap = new Map();
    public Texture2D currentMapImage;
    public Tile[] tileArray;
    public Color[] tileColors;
    public GameObject defaultTile;

    public Texture2D[] tex;
    List<SubMeshes> subMeshes = new List<SubMeshes>();
    public Block[,] blocks;
    [SerializeField]private BlockData[] blockData;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tri = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
   
    private Mesh mesh;
    public MeshFilter meshFilter;
    public MeshRenderer meshRend;
    public MeshCollider col;
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();
    int colCount;

    public const float TILE_SCALE = 0.32f;
    const int MAX_MAP_SIZE = 64;
    private int tileCount;

    public void ModifyTile(SpriteRenderer s, TileSprite t, int index)
    {
        s.sprite = t.sprite[index];
    }
    public void CreateSimpleGrid(int width, int height)
    {

        int i = 0;

        for (int y = height - 1; y > -1; y--)
        {


            for (int x = 0; x < width; x++)
            {

                GameObject tileChunk = new GameObject("TileCunk" + x + " " + y);
                tileChunk.transform.parent = transform;
                for (int a = 3; a > -1; a--)
                {
                    for (int b = 0; b < 4; b++)
                    {
                        Debug.Log(i);
                        GameObject tile = Instantiate(defaultTile, tileChunk.transform);
                        tile.transform.position = (new Vector2(b, a) * (TILE_SCALE / 4));

                        tile.GetComponent<TileSprite>().SetSpriteArray();

                        ModifyTile(tile.GetComponent<SpriteRenderer>(), tile.GetComponent<TileSprite>(), i);

                        tile.transform.localScale = Vector2.one * TILE_SCALE;

                        i++;
                        if (i > 15)
                            i = 0;
                    }

                }

                tileChunk.transform.position = new Vector2(x, y) * TILE_SCALE;


            }


        }
    }
    public void SafeDestroy()
    {
        SafeDestroyAll(gridPos, transform);
    }
    public void SafeDestroyAll(GameObject[,] g, Transform t)
    {
        //Debug.Log(g.GetLength(0) + " " + g.GetLength(1));
        for (int y = 0; y < g.GetLength(1); y++)
        {
            for (int x = 0; x < g.GetLength(0); x++)
            {
                DestroyImmediate(g[x, y]);
                g[x, y] = null;
            }
        }
    }
    public void CreateGrid(Map map)
    {
        SafeDestroyAll(gridPos, this.transform);
        gridPos = new GameObject[map.width, map.height];

        int index = 0;
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {

                GameObject tile = new GameObject("Tile X" + x + " Y" + y);
                tile.transform.SetParent(transform);
                tile.transform.position = new Vector2(x, y);
                tile.AddComponent<SpriteRenderer>();
                //Debug.Log(x + "" + y);

                //tile.GetComponent<SpriteRenderer>().sprite = map.tiles[x,y].sprite;

                gridPos[x, y] = tile;
                index++;
            }
        }
    }
    public void ConvertImageToMap(Texture2D image)
    {


        int h = image.height;
        int w = image.width;

        if (h > MAX_MAP_SIZE || w > MAX_MAP_SIZE)
        {
            Debug.LogError("Image size exceeds MAX_MAP_SIZE.");
            return;
        }
        else
        {
            Tile[,] tilePos = new Tile[w, h];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    tilePos[x, y].colour = image.GetPixel(x, y);
                    foreach (Tile t in tileArray)
                    {
                        if (t.colour == tilePos[x, y].colour)
                            tilePos[x, y].sprite = t.sprite;
                    }
                    if (tilePos[x, y].sprite == null)
                        tilePos[x, y].sprite = defaultSprite;
                    //Debug.Log(colorPos[x, y]);
                }
            }
            currentMap.mapName = image.name;
            currentMap.tiles = tilePos;
            currentMap.width = w;
            currentMap.height = h;
        }

        CreateGrid(currentMap);
    }
    public void LoadMap()
    {

    }

    private void Awake()
    {
        LoadLevelData();
    }

    private void Start()
    {
        // GenerateTerrain();
   
        BuildMesh();
        UpdateMeshData();
    }

    public void UpdateMaterialsList()
    {
        List<Material> materials = new List<Material>();
        for (int i = 0; i < tex.Length + 1; i++)
        {
            Material mat;
            if (i == 0)
            {
                mat = new Material(Shader.Find("Sprites/Diffuse"));
                mat.color = new Color(0, 0, 0, 0);
            }
            else
            {
                mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = tex[i - 1];
                Debug.Log("Tiles/" + tex[i - 1].name + "_n");
                Texture2D normalMap = null;
                normalMap = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Tiles/" + tex[i - 1].name + "_n" + ".png", typeof(Texture2D));
                if (normalMap)
                {
                    mat.SetTexture("_BumpMap", normalMap);
                    
                }
            }
            materials.Add(mat);
        }
        meshRend.sharedMaterials = materials.ToArray();
    }

    public void GenerateTerrain()
    {

        Block[,] b = new Block[MAX_MAP_SIZE, MAX_MAP_SIZE];

        for (int y = 0; y < b.GetLength(1); y++)
        {
            for (int x = 0; x < b.GetLength(0); x++)
            {
                Block block = new Block();
                if (Random.Range(0, 100) < 50)
                    block.subMesh = 0;
                else if (y > 50)
                    block.subMesh = 1;
                else if (y > 45)
                    block.subMesh = 2;
                else if (y > 30)
                    block.subMesh = 3;
                else if (y > 20)
                    block.subMesh = 4;
                else if (y > 15)
                    block.subMesh = 5;
                else
                    block.subMesh = 1;

                b[x, y] = block;
            }
        }

        blocks = b;
        SaveLevelData();
    }

    public void BuildMesh()
    {
        LoadLevelData();
        subMeshes.Clear();
        for (int i = 0; i < tex.Length + 1; i++)
        {
            SubMeshes sm = new SubMeshes();
            sm.triangles = new List<int>();
            subMeshes.Add(sm);
        }


        for (int y = 0; y < MAX_MAP_SIZE; y++)
        {
            for (int x = 0; x < MAX_MAP_SIZE; x++)
            {
                //try
               // {
                    if (blocks[x, y].subMesh != 0)
                    {
                        GenerateCollider(x, y);
                    }

                    CreateVerts(x, y);

                    CreateTile(blocks[x, y], x, y);
              //  }
               // catch(System.NullReferenceException ex)
               // {
               //     Debug.Log("error");
               // }
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
        for (int i = 0; i < subMeshes.Count; i++)
        {
            mesh.SetTriangles(subMeshes[i].triangles, i);

        }

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            if (mesh.GetTriangles(i).Length < 3)
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

        for (int i = 0; i < 6; i++)
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

    public void CreateTileAt(int x, int y, int t)
    {
        blocks[x, y].subMesh = t;
    }

    public void CreateLineAt(Vector2 start, Vector2 end, int t)
    {
        float dist = Vector2.Distance(start, end);
        CreateTileAt((int)start.x, (int)start.y, t);
        for (int i = 0; i < dist; i++)
        {
            float frac = i / dist;
            Vector2 pos = Vector2.Lerp(start, end, frac);
            CreateTileAt((int)pos.x, (int)pos.y, t);
        }
        CreateTileAt((int)end.x, (int)end.y, t);
    }

    public void DestroyAllTiles()
    {
        Block[,] b = new Block[MAX_MAP_SIZE, MAX_MAP_SIZE];

        for (int y = 0; y < b.GetLength(1); y++)
        {
            for (int x = 0; x < b.GetLength(0); x++)
            {
                b[x,y] = new Block();
                b[x,y].subMesh = 0;
            }
        }
        blocks = b;
        BuildMesh();
        UpdateMeshData();
    }

    public void SaveLevelData()
    {
        List<BlockData> bd = new List<BlockData>();
        for (int y = 0; y < blocks.GetLength(1); y++)
        {
            for (int x = 0; x < blocks.GetLength(0); x++)
            {
                bd.Add(new BlockData(x,y,blocks[x,y].subMesh));
                
            }
        }
        blockData = bd.ToArray();
    }

    public void LoadLevelData()
    {
        Block[,] b = new Block[MAX_MAP_SIZE,MAX_MAP_SIZE];
        for (int y = 0; y < b.GetLength(1); y++)
        {
            for (int x = 0; x < b.GetLength(0); x++)
            {
                    b[x,y] = new Block();
            }
        }

        foreach(BlockData bd in blockData)
        {
            b[bd.x, bd.y].subMesh = bd.subMesh;
        }

        blocks = b;
          
    }
}
[System.Serializable]public class SubMeshes
{
    public List<int> triangles = new List<int>();
}
[System.Serializable]public class Block
{

    public int subMesh;
    public int[] triangles;

    public Block()
    {
        subMesh = 0;
        triangles = new int[6];
    }
}
[System.Serializable]public class BlockData
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
