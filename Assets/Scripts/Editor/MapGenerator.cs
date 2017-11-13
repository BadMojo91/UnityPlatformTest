using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapGenerator : EditorWindow {
    WorldData worldData;
    [MenuItem("Window/BadMojo/Map Editor")]
    static void Init() {
       
        MapGenerator window = (MapGenerator)EditorWindow.GetWindow(typeof(MapGenerator));
        window.Show();
    }

    private void OnGUI() {
        if(worldData == null) {
            worldData = GameObject.Find("World").GetComponent<WorldData>();
        }
        worldData = (WorldData)EditorGUILayout.ObjectField(worldData, typeof(WorldData), true);

        if(worldData.submeshMaterial.materials == null)
            worldData.submeshMaterial.UpdateMaterials();

            
        if(GUILayout.Button("Update Materials List")) {
            worldData.submeshMaterial.UpdateMaterials();
        }
        if(GUILayout.Button("Generate Chunks")) {
            for(int y = 0; y < 5; y++) {
                for(int x = 0; x < 5; x++) {
                    worldData.CreateChunk(x, y);
                }
            }
        }

        if(GUILayout.Button("Generate Terrain")) {
            worldData.GenTerrain();
        }

        if(GUILayout.Button("Save Chunks")) {
            worldData.SaveChunks();
        }

        if(GUILayout.Button("Load Chunks")) {
            worldData.LoadChunks(5, 5);
        }
    }
}
