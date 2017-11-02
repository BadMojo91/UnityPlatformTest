using UnityEngine;
using System.Collections.Generic;
using System;
[Serializable]
public class SubmeshMaterial{
    public Material[] materials;
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
        List<Material> _materials = new List<Material>();
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
            _materials.Add(mat);
        }
        materials = _materials.ToArray();
    } //Sets subMeshMateral array to textures found in "Resources/Tiles" with normal maps
}