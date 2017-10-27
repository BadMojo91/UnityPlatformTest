using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DrawMode : EditorWindow {

    LevelGrid levelGrid;

    [MenuItem("Window/DrawMode")]
    static void Init() {
       
        DrawMode window = (DrawMode)EditorWindow.GetWindow(typeof(DrawMode));
        window.Show();
    }

    private void OnEnable() {
        levelGrid = GameObject.Find("LevelGrid").GetComponent<LevelGrid>();
    }


    void OnGUI() {
        if(GUILayout.Button("Save Chunk Data")) {
            levelGrid.SaveLevelData();
        }
        if(GUILayout.Button("Load Chunk Data")) {
            levelGrid.LoadLevelData();
        }
        if(GUILayout.Button("Load Chunks"))
        {
            levelGrid.CreateChunks(5);
        }

        if(GUILayout.Button("ClearGrid")){
            levelGrid.GUIClear();
        }


    }

    private void OnInspectorUpdate() {
        
    }


    private void OnSelectionChange() {
        
      
        
    }
}
