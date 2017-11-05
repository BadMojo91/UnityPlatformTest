using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class MeshBuilder : MonoBehaviour{
    public WorldData worldData;
    
    const int CHUNKSIZE = 32;
    const float TILESCALE = 1;
    public Vector2 chunkPosition;
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();
    public SubMeshes[] subMeshes;
    int colCount = 0;
    int tileCount = 0;
    public Tile[,] tiles;
    
    Tile[] ConvertMultiToFlat(Tile[,] t) {
        Tile[] _tile = new Tile[CHUNKSIZE * CHUNKSIZE];
        int i = 0;
        for(int y = 0; y < t.GetLength(1); y++) {
            for(int x = 0; x < t.GetLength(0); x++, i++) {
                _tile[i] = t[x, y];
            }
        }
        return _tile;
    } //converts to single array, x and y must be same size
    Tile[,] ConvertFlatToMulti(Tile[] t) {
        Tile[,] _tiles = new Tile[CHUNKSIZE, CHUNKSIZE];
        foreach(Tile _t in t) {     
            _tiles[_t.x, _t.y] = new Tile(_t.x, _t.y, _t.subMesh);
        }     
        return _tiles;
    } //converts to multidimentional array 

    public void SaveChunk(string path) {
        BinaryFormatter formatter = new BinaryFormatter();
        DirectoryInfo folder = Directory.CreateDirectory(Application.dataPath + "/Chunks/" + path);
        FileStream stream = File.Create(Application.dataPath + "/Chunks/" + path + "/" + gameObject.name + ".chnk");
        Tile[] _tiles = ConvertMultiToFlat(tiles);
        formatter.Serialize(stream, _tiles);
        stream.Close();
    }
    public void LoadChunk(string path) {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = File.Open(Application.dataPath + "/Chunks/" + path + "/" + gameObject.name + ".chnk", FileMode.Open);
        Tile[] _tile = new Tile[CHUNKSIZE*CHUNKSIZE];
        _tile = (Tile[])formatter.Deserialize(stream);
        tiles = ConvertFlatToMulti(_tile);
        stream.Close();
    }

    Reigon ReigonComparitor(Reigon r1, Reigon r2, float amount) {
        Reigon outcome = r1;
        if(amount > 1.0f)
            amount = 1.0f;
        outcome.amplitude = Mathf.Lerp(r1.amplitude, r2.amplitude, amount);
        outcome.frequency = Mathf.Lerp(r1.frequency, r2.frequency, amount);

        return outcome;
    }

    public void InitChunk() {
        ClearMesh();
        Tile[,] _tiles = new Tile[CHUNKSIZE, CHUNKSIZE];
        subMeshes = new SubMeshes[worldData.submeshMaterial.materials.Length];
        for(int sm = 0; sm < subMeshes.Length; sm++) {
            subMeshes[sm].triangles = new List<int>();
        }
        for(int y = 0; y < CHUNKSIZE; y++) {
            for(int x = 0; x < CHUNKSIZE; x++) {
                AddTile(x, y, 0);
            }
        }
    } 

    public void GenerateTerrain(Reigon _reigon) {
        Tile[,] _tiles = new Tile[CHUNKSIZE, CHUNKSIZE];
        float offsetX = transform.position.x / TILESCALE;
        float offsetY = transform.position.y / TILESCALE;
        float frequency = _reigon.frequency;
        float amplitude = _reigon.amplitude;
        int octaves = _reigon.octaves;
        float persistance = _reigon.persistance;
        int i = 0;

        for(int y = 0; y < CHUNKSIZE; y++) {
            for(int x = 0; x < CHUNKSIZE; x++, i++) {

                //float perlinNoise = Mathf.PerlinNoise((offsetX + x) * frequency, (offsetY + y) * frequency) * amplitude + octave;
                float perlinNoise = OctavePerlin(x, y, offsetX, offsetY, frequency, amplitude, octaves, persistance);
               // Debug.Log(perlinNoise);
                if(perlinNoise > 0.4f)
                    _tiles[x, y] = new Tile(x, y, 1);
                else if(perlinNoise > 0.3f)
                    _tiles[x, y] = new Tile(x, y, 2);
                else if(perlinNoise > 0.2f)
                    _tiles[x, y] = new Tile(x, y, 3);
                else
                    _tiles[x, y] = new Tile(x, y, 0);
            }
        }

        tiles = _tiles;
        BuildMesh();
        UpdateMesh();
    }
    public float OctavePerlin(float x, float y, float offsetX, float offsetY, float frequency, float amplitude, int octaves, float persistence) {
        float total = 0;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
        for(int i = 0; i < octaves; i++) {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }
    public int ReturnTileType(int x, int y) {
        int t = tiles[x, y].subMesh;
        return t;
    }
    public void SetTile(int x, int y, int subMeshIndex) { //sets tile at position to submesh material
        if(tiles == null) {
            Debug.LogError("SetTile: Mesh not ready");
            return;
        }
        tiles[x, y].subMesh = subMeshIndex;
        BuildMesh();
        UpdateMesh();
    }
    void BuildMesh(Tile[] tile) {    //build mesh from chunk
        tiles = ConvertFlatToMulti(tile);
        BuildMesh();
    }
    public void BuildMesh() {    //build mesh from tiles
        ClearMesh();
        subMeshes = new SubMeshes[worldData.submeshMaterial.materials.Length];
        for(int sm = 0; sm < subMeshes.Length; sm++) {
            subMeshes[sm].triangles = new List<int>();
        }
        for(int y = 0; y < CHUNKSIZE; y++) {
            for(int x = 0; x < CHUNKSIZE; x++) {
                try {
                    AddTile(x, y, tiles[x, y].subMesh);
                }
                catch { return; }
                }
        }
    }
    void ClearMesh() {
        colVerts.Clear();
        colTri.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        tileCount = 0;
        colCount = 0;
    }

    public void UpdateMesh() {
        
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        mesh.subMeshCount = worldData.submeshMaterial.materials.Length;
        mesh.vertices = vertices.ToArray();
        for(int i = 0; i < mesh.subMeshCount; i++) {
            mesh.SetTriangles(subMeshes[i].triangles, i);
        }
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshRenderer.materials = worldData.submeshMaterial.materials;
        meshFilter.mesh = mesh;

        Mesh colMesh = new Mesh();
        colMesh.vertices = colVerts.ToArray();
        colMesh.triangles = colTri.ToArray();
        meshCollider.sharedMesh = colMesh;
        //SaveChunk(worldData.worldName);
        ClearMesh();
    }

    void AddTile(int x, int y, int subMesh) {
        AddVerts(x, y);
        AddTriangles(subMesh);
        if(subMesh != 0)
            GenerateCollider(x,y);
    }
    void AddVerts(float x, float y) {
        float i = TILESCALE;
        x = x * TILESCALE;
        y = y * TILESCALE;
        vertices.Add(new Vector2(x, y));
        vertices.Add(new Vector2(x + i, y));
        vertices.Add(new Vector2(x + i, y - i));
        vertices.Add(new Vector2(x, y - i));

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
    }
    void AddTriangles(int subMesh) {
        List<int> t = new List<int>();

        t.Add(tileCount * 4);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 3);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 2);
        t.Add((tileCount * 4) + 3);

        triangles.AddRange(t);
        AddSubMeshTriangles(subMesh, t);
        
        tileCount++;
    }
    void AddSubMeshTriangles(int subMesh, List<int> t) {
            subMeshes[subMesh].triangles.AddRange(t);
    }
    private void ColTriangles() {
        colTri.Add(colCount * 4);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 3);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 2);
        colTri.Add((colCount * 4) + 3);
    }
    private void GenerateCollider(int posX, int posY) {

        float i = TILESCALE;
        float x = posX * TILESCALE;
        float y = posY * TILESCALE;
        
        if(posY + 1 >= CHUNKSIZE || tiles[posX, posY + 1].subMesh == 0) { 
            //Top
            colVerts.Add(new Vector3(x, y, i));
            colVerts.Add(new Vector3(x + i, y, i));
            colVerts.Add(new Vector3(x + i, y, 0));
            colVerts.Add(new Vector3(x, y, 0));
            ColTriangles();
            colCount++;
        }
        if(posX - 1 < 0 || tiles[posX - 1, posY].subMesh == 0) {
            //Left
            colVerts.Add(new Vector3(x, y - i, i));
            colVerts.Add(new Vector3(x, y, i));
            colVerts.Add(new Vector3(x, y, 0));
            colVerts.Add(new Vector3(x, y - i, 0));
            ColTriangles();
            colCount++;
        }
        if(posX + 1 >= CHUNKSIZE || tiles[posX + 1, posY].subMesh == 0) {
            //Right
            colVerts.Add(new Vector3(x + i, y, i));
            colVerts.Add(new Vector3(x + i, y - i, i));
            colVerts.Add(new Vector3(x + i, y - i, 0));
            colVerts.Add(new Vector3(x + i, y, 0));
            ColTriangles();
            colCount++;
        }
            if(posY - 1 < 0 || tiles[posX, posY - 1].subMesh == 0) {
                //Bottom
                colVerts.Add(new Vector3(x + i, y - i, i));
                colVerts.Add(new Vector3(x, y - i, i));
                colVerts.Add(new Vector3(x, y - i, 0));
                colVerts.Add(new Vector3(x + i, y - i, 0));
                ColTriangles();
                colCount++;
            }
    }
    [System.Serializable]
    public struct SubMeshes {
        public List<int> triangles;
        public SubMeshes(List<int> t) {
            triangles = t;
        }
    }

}
