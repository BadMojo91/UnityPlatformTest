using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Chunk : MonoBehaviour {
    public LevelGrid levelGrid;
    //Mesh
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRend;
    MeshCollider meshCollider;
    List<SubMeshes> subMeshes = new List<SubMeshes>();
    List<Vector3> verts = new List<Vector3>();
    List<int> tri = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int colCount; //collider index for triangles
    int tileCount;//mesh index for triangles
    float uvUnit = 0.5f;
    //Collider
    List<Vector3> colVerts = new List<Vector3>();
    List<int> colTri = new List<int>();

    public Block[,] blocks;
    public void UpdateMaterialsList() {
        if(levelGrid == null)
            return;

        List<Material> materials = new List<Material>();
        for(int i = 0; i < levelGrid.tex.Length + 1; i++) {
            Material mat;
            if(i == 0) {
                mat = new Material(Shader.Find("Sprites/Diffuse"));
                mat.color = new Color(0, 0, 0, 0);
            }
            else {
                mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = levelGrid.tex[i - 1];
                //Debug.Log("Tiles/" + levelGrid.tex[i - 1].name + "_n");
                Texture2D normalMap = null;
                normalMap = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Tiles/" + levelGrid.tex[i - 1].name + "_n" + ".png", typeof(Texture2D));
                if(normalMap) {
                    mat.SetTexture("_BumpMap", normalMap);

                }
            }
            materials.Add(mat);
        }
        meshRend = GetComponent<MeshRenderer>();
        meshRend.sharedMaterials = materials.ToArray();
    }
    private void CreateVerts(float x, float y, int uvIndexX, int uvIndexY) {
        if(levelGrid == null)
            return;
        float i = levelGrid.TILE_SCALE;
        x = x * levelGrid.TILE_SCALE;
        y = y * levelGrid.TILE_SCALE;
        verts.Add(new Vector2(x, y));
        verts.Add(new Vector2(x + i, y));
        verts.Add(new Vector2(x + i, y - i));
        verts.Add(new Vector2(x, y - i));


        //uvs.Add(new Vector2(0, 1));
        //uvs.Add(new Vector2(1, 1));
        //uvs.Add(new Vector2(1, 0));
        //uvs.Add(new Vector2(0, 0));

        uvs.Add(new Vector2(uvUnit * uvIndexX, uvUnit * uvIndexY + uvUnit));
        uvs.Add(new Vector2(uvUnit * uvIndexX + uvUnit, uvUnit * uvIndexY + uvUnit));
        uvs.Add(new Vector2(uvUnit * uvIndexX + uvUnit, uvUnit * uvIndexY));
        uvs.Add(new Vector2(uvUnit * uvIndexX, uvUnit * uvIndexY));

    }
    private void CreateTile(Block block, float x, float y) {

        List<int> t = new List<int>();

        t.Add(tileCount * 4);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 3);
        t.Add((tileCount * 4) + 1);
        t.Add((tileCount * 4) + 2);
        t.Add((tileCount * 4) + 3);


        block.triangles = t.ToArray();

        for(int i = 0; i < 6; i++) {
            //Debug.Log(block.subMesh);
            subMeshes[block.subMesh].triangles.Add(t[i]);
        }

        tileCount++;
    }
    private void ColTriangles() {
        colTri.Add(colCount * 4);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 3);
        colTri.Add((colCount * 4) + 1);
        colTri.Add((colCount * 4) + 2);
        colTri.Add((colCount * 4) + 3);
    }
    private void GenerateCollider(float x, float y) {
        if(levelGrid == null)
            return;
        float i = levelGrid.TILE_SCALE;
        x = x * levelGrid.TILE_SCALE;
        y = y * levelGrid.TILE_SCALE;

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
    private void UpdateMeshData() {
        if(levelGrid == null)
            return;
        meshFilter = GetComponent<MeshFilter>();
        meshRend = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh();
        mesh.Clear();

        mesh.vertices = verts.ToArray();

        mesh.subMeshCount = levelGrid.tex.Length + 1;
        for(int i = 0; i < subMeshes.Count; i++) {
            mesh.SetTriangles(subMeshes[i].triangles, i);
        }

        for(int i = 0; i < mesh.subMeshCount; i++) {
            if(mesh.GetTriangles(i).Length < 3) {
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
        meshCollider.sharedMesh = colMesh;

        //Cleanup
        colVerts.Clear();
        colTri.Clear();
        colCount = 0;
        verts.Clear();
        tri.Clear();
        uvs.Clear();

    }
    public void BuildMesh() {
        if(levelGrid == null)
            return;
        UpdateMaterialsList();
        //gather submeshes
        subMeshes.Clear();
        for(int i = 0; i < levelGrid.tex.Length + 1; i++) {
            SubMeshes sm = new SubMeshes();
            sm.triangles = new List<int>();
            subMeshes.Add(sm);
        }
        int uvIndexX = 0;
        int uvIndexY = 1;
        for(int y = 0; y < levelGrid.MAX_CHUNK_SIZE; y++) {
            for(int x = 0; x < levelGrid.MAX_CHUNK_SIZE; x++) {

                if(blocks[x, y].subMesh != 0)
                    GenerateCollider(x, y); //only generate collider on solid tiles

                CreateVerts(x, y, uvIndexX, uvIndexY);
                CreateTile(blocks[x, y], x, y);
                
                uvIndexX++;
                if(uvIndexX > 1)
                    uvIndexX = 0;
            }
            uvIndexY--;
            if(uvIndexY < 0)
                uvIndexY = 1;
        }
        tileCount = 0;

        UpdateMeshData();
    }

}

