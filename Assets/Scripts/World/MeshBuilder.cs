using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class MeshBuilder : MonoBehaviour{
    public MapInfo mapInfo;
    
    const int CHUNKSIZE = 32;
    const float TILESCALE = 0.64f;

    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();
    SubMeshes[] subMeshes;
    int colCount = 0;
    int tileCount = 0;
    Tile[,] tiles;
    
    Tile[] ConvertMultiToFlat(Tile[,] t) {
        Tile[] _tile = new Tile[CHUNKSIZE * CHUNKSIZE];
        int i = 0;
        for(int y = 0; y < t.GetLength(1); y++) {
            for(int x = 0; x < t.GetLength(0); x++, i++) {
                _tile[i] = t[x, y];
            }
        }
        return _tile;
    }
    Tile[,] ConvertFlatToMulti(Tile[] t) {
        Tile[,] _tiles = new Tile[CHUNKSIZE, CHUNKSIZE];
        foreach(Tile _t in t) {     
            _tiles[_t.x, _t.y] = new Tile(_t.x, _t.y, _t.subMesh);
        }     
        return _tiles;
    }

    public void SaveToChunk() {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = File.Create(Application.dataPath + "/Chunks/" + gameObject.name + ".chnk");
        Tile[] _tiles = ConvertMultiToFlat(tiles);
        formatter.Serialize(stream, _tiles);
        stream.Close();
    }

    public void LoadChunk() {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = File.Open(Application.dataPath + "/Chunks/" + gameObject.name + ".chnk", FileMode.Open);
        Tile[] _tile = new Tile[CHUNKSIZE*CHUNKSIZE];
        _tile = (Tile[])formatter.Deserialize(stream);
        tiles = ConvertFlatToMulti(_tile);
        stream.Close();
    }

    public void BuildNewMesh() {
        //Chunk chunk;
        Tile[,] _tiles = new Tile[CHUNKSIZE, CHUNKSIZE];
        float offsetX = transform.position.x / TILESCALE;
        float offsetY = transform.position.y / TILESCALE;
        int i = 0;
        for(int y = 0; y < CHUNKSIZE; y++) {
            for(int x = 0; x < CHUNKSIZE; x++, i++) {
                
                float perlinNoise = Mathf.PerlinNoise((offsetX + x) * mapInfo.reigon.frequency, (offsetY + y) * mapInfo.reigon.frequency) * mapInfo.reigon.amplitude;
                if(perlinNoise > 0.4f)
                    _tiles[x,y] = new Tile(x, y, 1);
                else if(perlinNoise > 0.3f)
                    _tiles[x, y] = new Tile(x, y, 2);
                else if(perlinNoise > 0.2f)
                    _tiles[x, y] = new Tile(x, y, 3);
                else
                    _tiles[x,y] = new Tile(x, y, 0);
            }
        }
        //chunk = new Chunk(_tiles);
        tiles = _tiles;
        BuildMesh();
    }
   
    public void SetTile(int x, int y, int subMeshIndex) {
        if(tiles == null) {
            Debug.LogError("SetTile: Mesh not ready");
            return;
        }
        tiles[x, y].subMesh = subMeshIndex;
        BuildMesh();
        UpdateMesh();
    }
    public void BuildMesh(Tile[] tile) {    //build mesh from chunk
        tiles = ConvertFlatToMulti(tile);
        BuildMesh();
    }
    public void BuildMesh() {    //build mesh from tiles
        ClearMesh();
        subMeshes = new SubMeshes[mapInfo.subMeshMaterial.Length];
        for(int sm = 0; sm < subMeshes.Length; sm++) {
            subMeshes[sm].triangles = new List<int>();
        }
        for(int y = 0; y < CHUNKSIZE; y++) {
            for(int x = 0; x < CHUNKSIZE; x++) {
                try {
                    AddTile(x, y, tiles[x, y].subMesh);
                }
                catch {
                    Debug.LogError("AddTile: " + x + " " + y);
                    return;
                }
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
        mesh.subMeshCount = mapInfo.subMeshMaterial.Length;
        mesh.vertices = vertices.ToArray();
        for(int i = 0; i < mesh.subMeshCount; i++) {
            mesh.SetTriangles(subMeshes[i].triangles, i);
        }
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshRenderer.materials = mapInfo.subMeshMaterial;
        meshFilter.mesh = mesh;

        Mesh colMesh = new Mesh();
        colMesh.vertices = colVerts.ToArray();
        colMesh.triangles = colTri.ToArray();
        meshCollider.sharedMesh = colMesh;

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
    public struct SubMeshes {
        public List<int> triangles;
        public SubMeshes(List<int> t) {
            triangles = t;
        }
    }

}
