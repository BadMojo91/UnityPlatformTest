/*
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGrid))]
public class LevelEditor : Editor {
    public string width = "4";
    public string height = "4";
    public string lineX = "0";
    public string lineY = "0";

    public string lineX2 = "1";
    public string lineY2 = "1";

    public override void OnInspectorGUI() {
        int w,h, x,y, x2,y2;
        DrawDefaultInspector();
        LevelGrid levelGrid = (LevelGrid)target;


       
        GUILayout.BeginHorizontal();
        lineX = GUILayout.TextField(lineX);
        lineY = GUILayout.TextField(lineY);
        lineX2 = GUILayout.TextField(lineX2);
        lineY2 = GUILayout.TextField(lineY2);
        GUILayout.EndHorizontal();

        int.TryParse(width, out w);
        int.TryParse(height, out h);
        int.TryParse(lineX, out x);
        int.TryParse(lineY, out y);
        int.TryParse(lineX2, out x2);
        int.TryParse(lineY2, out y2);

        if(GUILayout.Button("Create Chunks"))
        {
            levelGrid.CreateChunks(5);
        }


        if(GUILayout.Button("Draw Line"))
        {
            levelGrid.CreateLineAt(new Vector2(x, y), new Vector2(x2, y2), 2);
           // levelGrid.BuildMesh();
           // levelGrid.UpdateMeshData();
            lineX = x2.ToString();
            lineY = y2.ToString();
        }

        GUILayout.BeginHorizontal();
        width = GUILayout.TextField(width);
        height = GUILayout.TextField(height);
       
        if(GUILayout.Button("DestroyTileAt")) {
            levelGrid.DestroyTileAt(levelGrid.chunks[0,0].GetComponent<Chunk>(),w,h);
        }
        GUILayout.EndHorizontal();
        
    }
}
*/