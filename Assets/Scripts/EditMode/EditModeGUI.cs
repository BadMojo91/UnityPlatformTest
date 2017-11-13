using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeGUI : MonoBehaviour {
    public WorldData worldData;
    public float mouseDragSensitivity;
    float screenScrollSpeed;
    public float maxScrollSpeed;
    public Transform mouseSelectionBox;

    
    private void OnGUI() {
        GUI.Box(new Rect(10, 10, 100, 400), "EditMode");
        if(GUI.Button(new Rect(20, 40, 80, 20), "Mat Update")) {
            worldData.submeshMaterial.UpdateMaterials();
        }
        if(GUI.Button(new Rect(20, 60, 80, 20), "New")) {
            worldData.GenerateChunks(-5, 5);
        }
        if(GUI.Button(new Rect(20, 80, 80, 20), "Save")) {
            worldData.SaveChunks();
        }
        if(GUI.Button(new Rect(20, 100, 80, 20), "Load")) {
            worldData.LoadChunks(-5,5);
        }
        if(GUI.Button(new Rect(20, 120, 80, 20), "Generate")) {
            worldData.GenTerrain();
        }
        

        GUI.Box(new Rect(10, Screen.height - 200f, 600,200), "Console");
    }

    private void Update() {

        
        if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt)) {
            transform.position += new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0) * Time.deltaTime * mouseDragSensitivity;
        }
        else if(Input.GetMouseButton(0)) {
            worldData.ReplaceAtMousePos(0);
        }
        if(mouseSelectionBox != null) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos += new Vector3(mousePos.normalized.x, mousePos.normalized.y, 0) * -0.5f;
            mousePos = new Vector3(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y), -1);
            mouseSelectionBox.position = mousePos;
        }
        Camera editCam = GetComponent<Camera>();

        Vector2 mPos = editCam.ScreenToViewportPoint(Input.mousePosition);
        float margain = 0.0f;
        if(mPos.x < margain || mPos.x > 1.0 - margain || mPos.y < margain || mPos.y > 1.0 - margain)
            screenScrollSpeed = maxScrollSpeed;
        else
            screenScrollSpeed = 0;

        //Debug.Log(mPos);
        transform.position = Vector2.MoveTowards(transform.position, editCam.ScreenToWorldPoint(Input.mousePosition), Time.deltaTime * screenScrollSpeed);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        //if(Input.GetAxis("Mouse ScrollWheel") > 0.01f || Input.GetAxis("Mouse ScrollWheel") < -0.01f)
        //    transform.position = mPos;

        editCam.orthographicSize += Input.GetAxis("Mouse ScrollWheel");



    }
}
