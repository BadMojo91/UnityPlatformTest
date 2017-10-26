using UnityEngine;
using System.Collections;
using protagonist;
public class Player : MonoBehaviour {

    public Controller controller;
    public Animator legsAnim;
    public Animator gunAnim;
    public SpriteRenderer headSprite;
    public SpriteRenderer torsoSprite;
    public SpriteRenderer armsSprite;
    public SpriteRenderer gunSprite;
    public SpriteRenderer legsSprite;
    public Transform arms;
    public Transform gun;
    public Transform head;
    public Animator gunFlash;

    public GameObject bullet;

    public SpriteRenderer[] playerSprite;

    public GameObject target;
    LevelGrid levelGrid;

    private void Awake() {
        levelGrid = GameObject.Find("LevelGrid").GetComponent<LevelGrid>();
    }

    private void Update() {
        controller.RotateHandsWithMouseUpdate(arms, head, this);
        if(Input.GetButtonDown("Fire1")) {
            RaycastHit hit;
            if(Physics.Raycast(gun.position, gun.TransformDirection(Vector2.right * 0.5f), out hit)){
                Debug.Log("Ray Test!");
                Vector2 point = new Vector2(hit.point.x, hit.point.y);
                point += (new Vector2(hit.normal.x, hit.normal.y)) * -0.5f;
                Debug.DrawLine(gun.position, new Vector2(Mathf.RoundToInt(point.x - .5f), Mathf.RoundToInt(point.y + .5f)), Color.red, 5);
                levelGrid.DestroyTileAt(Mathf.RoundToInt(point.x - .5f), Mathf.RoundToInt(point.y + .5f));
            }
            else {
                Debug.DrawRay(gun.position, gun.TransformDirection(Vector2.right), Color.blue, 5);
            }
        }
        
    }

    void FixedUpdate() {
        controller.MoveUpdate(this, legsAnim, transform, gun);
        controller.PhysicsUpdate(transform, GetComponent<Rigidbody>());
    }

    
}
