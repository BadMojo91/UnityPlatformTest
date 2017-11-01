using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


public class MapInfo : MonoBehaviour {

    public Material[] subMeshMaterial;
    Texture2D[] FindTextureAssets() {
        List<Texture2D> tex = new List<Texture2D>();
        foreach(Texture2D t in Resources.LoadAll("Tiles", typeof(Texture2D))) {
            string n = t.name;
            n = n.Substring(n.Length - 2);
            //Debug.Log(n);
            if(n != "_n" && n != "_s" && n != "_h") {
                tex.Add(t);
                Debug.Log("Texture Found: " + t.name);
            }
        }
        return tex.ToArray();
    }
    public void UpdateMaterials() {
        Texture2D[] textures = FindTextureAssets();
        List<Material> materials = new List<Material>();
        for(int i = 0; i < textures.Length + 1; i++) {
            Material mat;
            if(i == 0) {
                mat = new Material(Shader.Find("Sprites/Diffuse"));
                mat.color = new Color(0, 0, 0, 0);
            }
            else {
                mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = textures[i - 1];
                //Debug.Log("Tiles/" + levelGrid.tex[i - 1].name + "_n");
                Texture2D normalMap = null;
                normalMap = (Texture2D)Resources.Load("Tiles/" + textures[i - 1].name + "_n", typeof(Texture2D));
                if(normalMap) {
                    Debug.Log("Normal Map Found: " + normalMap);
                    mat.SetTexture("_BumpMap", normalMap);
                }
            }
            materials.Add(mat);
        }
        subMeshMaterial = materials.ToArray();
    } //Sets subMeshMateral array to textures found in "Resources/Tiles" with normal maps

    public Reigon reigon;

    public GameObject chunks;

    public void LoadChunk(int x, int y) {
        if(GameObject.Find("Chunk_" + x + "," + y)) {
            GameObject newChunk = GameObject.Find("Chunk_" + x + "," + y);
            newChunk.GetComponent<MeshBuilder>().LoadChunk();
            newChunk.GetComponent<MeshBuilder>().BuildMesh();
            newChunk.GetComponent<MeshBuilder>().UpdateMesh();
        }
        else {
            Debug.LogError("LoadChunk() Cant find chunk:" + x + " " + y);
        }
    }

    public void CreateChunk(int x, int y) {
        GameObject newChunk;
        if(GameObject.Find("Chunk_" + x + "," + y))
            newChunk = GameObject.Find("Chunk_" + x + "," + y);
        else {
            newChunk = new GameObject();
            newChunk.transform.position = new Vector3(x * 0.16f * 32, y * 0.16f * 32);
            newChunk.transform.SetParent(transform);
            newChunk.name = "Chunk_" + x + "," + y;
            newChunk.AddComponent<MeshRenderer>();
            newChunk.AddComponent<MeshFilter>();
            newChunk.AddComponent<MeshCollider>();
            newChunk.AddComponent<MeshBuilder>();
            newChunk.GetComponent<MeshBuilder>().mapInfo = this;
        }
        
        newChunk.GetComponent<MeshBuilder>().BuildNewMesh();
        //newChunk.GetComponent<MeshBuilder>().LoadChunk();
        //newChunk.GetComponent<MeshBuilder>().BuildMesh();
        newChunk.GetComponent<MeshBuilder>().UpdateMesh();
        newChunk.GetComponent<MeshBuilder>().SaveToChunk();

    }

}
[System.Serializable]
public class Reigon {
    public float frequency;
    public float amplitude;
}
