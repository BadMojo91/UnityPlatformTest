using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapEditor : EditorWindow {
    public GameObject worldData;
    [MenuItem("Window/BadMojo/Map Editor")]
    static void Init() {
        MapEditor window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor));
        window.Show();
    }

    private void OnGUI() {
        worldData = (GameObject)EditorGUILayout.ObjectField(worldData, typeof(GameObject), true);
        if(GUILayout.Button("Update Materials List")) {
            worldData.GetComponent<WorldData>().submeshMaterial.UpdateMaterials();
        }
        if(GUILayout.Button("Generate Chunks")) {
            for(int x = 0; x < 10; x++) {
                for(int y = 0; y < 10; y++) {
                    worldData.GetComponent<WorldData>().CreateChunk(x, y);
                }
            }
            
        }

        if(GUILayout.Button("Load Chunks")) {
            for(int x = 0; x < 10; x++) {
                for(int y = 0; y < 10; y++) {
                    worldData.GetComponent<WorldData>().LoadChunk(x, y);
                }
            }

        }
    }
}
