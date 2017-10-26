using UnityEngine;
using UnityEditor;
using System.Collections;

public class DrawMode : EditorWindow {

    GameObject levelGrid;
    public bool drawMode;
    public bool autoSelect;

    [MenuItem("Window/DrawMode")]
    static void Init() {
       
        DrawMode window = (DrawMode)EditorWindow.GetWindow(typeof(DrawMode));
        window.Show();
    }

    private void OnEnable() {
        levelGrid = GameObject.Find("LevelGrid");
        //Debug.Log("DrawMode active");

        if(levelGrid != null) {
            //Debug.Log("Level Grid Found: " + levelGrid);
        }
    }


    void OnGUI() {
        GUILayout.Label("Draw Mode", EditorStyles.boldLabel);
        drawMode = EditorGUILayout.Toggle("Enable draw mode", drawMode);
        autoSelect = EditorGUILayout.Toggle("Enable auto select", autoSelect);

    }

    private void OnInspectorUpdate() {
        if(autoSelect)
            Selection.activeGameObject = levelGrid.gameObject;
    }


    private void OnSelectionChange() {
        
        if(Selection.activeGameObject != null && drawMode) {
            if(Selection.activeGameObject.GetComponent<SpriteRenderer>()) {
                if(Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite != levelGrid.GetComponent<LevelGrid>().activeSprite) {
                    Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite = levelGrid.GetComponent<LevelGrid>().activeSprite;
                   // Debug.Log("Selected tile changed to: " + Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite);
                    
                }
            }
        }

        
    }
}
