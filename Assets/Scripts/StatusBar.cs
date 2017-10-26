using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StatusBar : MonoBehaviour {

    public Image uiFace;
    public Sprite[] faces;
    const float MAX_WAIT = 5f;
    const float MIN_WAIT = 0.5f;
    Sprite nextFace;
    float faceTimer;
    int faceNum;

    private void Update() {
        RandomFaceLook(faces);
    }

    void RandomFaceLook(Sprite[] f) {

        faceTimer -= Time.deltaTime;

        if(faceTimer < 0) {
            float rand = Random.Range(0, 100);

            if (nextFace == null)
                nextFace = f[0];

            uiFace.sprite = nextFace;

            if (faceNum > 0) {
                nextFace = f[0];
                faceTimer = Random.Range(MIN_WAIT, MAX_WAIT);

            }
            else {
                if(rand < 25) {
                    nextFace = f[2];
                    faceTimer = Random.Range(0.2f, 1f);
                }
                else if(rand < 50) {
                    nextFace = f[1];
                    faceTimer = Random.Range(0.2f, 1f);
                }
                else {
                    nextFace = f[0];
                    faceTimer = Random.Range(MIN_WAIT, MAX_WAIT);
                }
            }

            
        }

    }
}
