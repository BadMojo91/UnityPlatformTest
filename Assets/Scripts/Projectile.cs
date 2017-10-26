using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public float speed;
    public float damage;
    public float dropDist;
    public float timer;
    //Vector2 oldPos;
    Rigidbody rb;
    bool stopMissile = false;
    bool destroyBool = false;
    LevelGrid levelGrid;
    private void Awake() {
        levelGrid = GameObject.Find("LevelGrid").GetComponent<LevelGrid>();
    }
    private void Start() {
        rb = GetComponent<Rigidbody>();
        //oldPos = transform.position;
        
    }
    void FixedUpdate () {

        if(timer < 0 || destroyBool)
            Destroy(transform.gameObject);

        timer -= Time.deltaTime;

        /*
        if(transform.position.x > (oldPos.x + dropDist))
            transform.position += transform.TransformDirection(Vector2.down) * Time.deltaTime * speed;
        transform.position += transform.TransformDirection(Vector2.right) * Time.deltaTime * speed;
        */
        if(!stopMissile)
            rb.AddForce(transform.TransformDirection(Vector2.right) * speed);

	}

    private void OnCollisionEnter(Collision collision) {
        //if(collision.gameObject.tag == "LevelGrid") {
        //Debug.Log(transform.position);
        stopMissile = true;
        rb.velocity = Vector3.zero;

        Vector2 pos = transform.position;
        pos += (new Vector2(pos.normalized.x, pos.normalized.y)) * -0.5f;
        
        levelGrid.DestroyTileAt(Mathf.RoundToInt(pos.x-.5f), Mathf.RoundToInt(pos.y+.5f));

        destroyBool = true;
        //}
    }


}
