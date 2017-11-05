using UnityEngine;
using System.Collections;

public class ParallaxBackdrop : MonoBehaviour {
    public float scale;
    public float soften = 1f;
    public float layer = 1f;
    private Transform camPos;
    private Vector3 prevCamPos;

    void Awake() {
        camPos = Camera.main.transform;
    }

    void Start() {
        prevCamPos = camPos.position;
    }

    void Update() {  
        float parallax = (prevCamPos.x - camPos.position.x) * scale;
        float backgroundTargetPosX = transform.position.x + parallax;
        Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, transform.position.y, layer);
        transform.position = Vector3.Lerp(transform.position, backgroundTargetPos, soften * Time.deltaTime);
        prevCamPos = camPos.position;
    }
}