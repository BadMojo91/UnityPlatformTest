using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapEditor : EditorWindow {
    public GameObject mapInfo;
    [MenuItem("Window/BadMojo/Map Editor")]
    static void Init() {
        MapEditor window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor));
        window.Show();
    }

    private void OnGUI() {
        mapInfo = (GameObject)EditorGUILayout.ObjectField(mapInfo, typeof(GameObject), true);
        if(GUILayout.Button("Update Materials List")) {
            mapInfo.GetComponent<MapInfo>().UpdateMaterials();
        }
        if(GUILayout.Button("Generate Chunks")) {
            for(int x = -10; x < 10; x++) {
                for(int y = -10; y < 10; y++) {
                    mapInfo.GetComponent<MapInfo>().CreateChunk(x, y);
                }
            }
            
        }

        if(GUILayout.Button("Load Chunks")) {
            for(int x = -10; x < 10; x++) {
                for(int y = -10; y < 10; y++) {
                    mapInfo.GetComponent<MapInfo>().LoadChunk(x, y);
                }
            }

        }
    }
}
