using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class EditMode : MonoBehaviour {
    WorldData worldData;
    public bool drawTool, moveTool;
    public float moveToolSpeed;
    public Camera editModeCamera;
    public int tileIndex;
    public InputField tileIndexInputField;
    private void Awake() {
        worldData = GetComponent<WorldData>();
        worldData.GenerateChunks(5, 5);
        worldData.LoadChunks(5, 5);
        DisableAll();
        EnableMoveTool();
    }
    private void Start() {
        tileIndexInputField.onValueChanged.AddListener(delegate { SetTileNum(); });
    }
    private void Update() {
        if(moveTool) {
            EditModeCameraUpdate();
        }
        if(drawTool) {
            EditModeDrawToolUpdate();
        }
    }
    void DisableAll() {
        drawTool = false;
        moveTool = false;
    }
    public void EnableDrawTool() {
        DisableAll();
        drawTool = true;
    }
    public void EnableMoveTool() {
        DisableAll();
        moveTool = true;
    }

    public void EditModeCameraUpdate() {
        if(Input.GetMouseButton(0))
           editModeCamera.transform.position += new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0) * Time.deltaTime * moveToolSpeed;
    }

    public void EditModeDrawToolUpdate() {
        if(Input.GetMouseButton(0)) {
            worldData.ReplaceAtMousePos(tileIndex);
        }
    }

    public void SetTileNum() {
        int tileNum;
        int.TryParse(tileIndexInputField.text, out tileNum);
        if(tileNum >= worldData.submeshMaterial.materials.Length)
            tileNum = worldData.submeshMaterial.materials.Length - 1;
        tileIndex = tileNum;
    }
}