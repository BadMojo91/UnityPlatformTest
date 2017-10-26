using UnityEngine;
using UnityEditor;
using System.Collections;

public class DrawMode : EditorWindow {

    GameObject levelGrid;

    [MenuItem("Window/DrawMode")]
    static void Init() {
       
        DrawMode window = (DrawMode)EditorWindow.GetWindow(typeof(DrawMode));
        window.Show();
    }

    private void OnEnable() {
        levelGrid = GameObject.Find("LevelGrid");
    }


    void OnGUI() {
       

    }

    private void OnInspectorUpdate() {
        
    }


    private void OnSelectionChange() {
        
      
        
    }
}
