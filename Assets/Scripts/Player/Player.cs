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

    public Vector2 currentBlock;

    private void Awake() {
        levelGrid = GameObject.Find("LevelGrid").GetComponent<LevelGrid>();
    }

    private void Update() {
        controller.RotateHandsWithMouseUpdate(arms, head, this); //head rotation

        if(Input.GetButtonDown("Fire1"))
            if(TraceLine(transform.position, gun.transform.TransformDirection(Vector3.right)))
                levelGrid.DestroyTileAt((int)currentBlock.x, (int)currentBlock.y);

        if(Input.GetButton("Fire2"))
        {
            ReplaceAtMousePos(1);
        }

        
    }

    void ReplaceAtMousePos(int tile)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) / levelGrid.TILE_SCALE;
        mousePos += new Vector3(mousePos.normalized.x, mousePos.normalized.y, 0) * -0.5f;
        mousePos = new Vector3(Mathf.RoundToInt(mousePos.x - 0.5f), Mathf.RoundToInt(mousePos.y + 0.5f), 0);

        //Debug.Log(mousePos);
        int x = (int)mousePos.x;
        int y = (int)mousePos.y;
        if(levelGrid.blocks[x + 1, y].subMesh != 0 || levelGrid.blocks[x - 1, y].subMesh != 0 || levelGrid.blocks[x, y + 1].subMesh != 0 || levelGrid.blocks[x, y - 1].subMesh != 0)
            levelGrid.CreateTileAt((int)mousePos.x, (int)mousePos.y, tile, true);
    }

    bool TraceLine(Vector3 pos, Vector3 end)
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, gun.transform.TransformDirection(Vector3.right), out hit))
        {
            Vector2 point = new Vector2(hit.point.x, hit.point.y) / levelGrid.TILE_SCALE;
            point += (new Vector2(hit.normal.x, hit.normal.y)) * -0.5f;
            currentBlock = new Vector2(Mathf.RoundToInt(point.x - 0.5f), Mathf.RoundToInt(point.y + 0.5f));
            return true;
        }
        else
        {
            currentBlock = Vector2.zero;
            return false;
        }
    }
    void FixedUpdate() {
        controller.MoveUpdate(this, legsAnim, transform, gun);
        controller.PhysicsUpdate(transform, GetComponent<Rigidbody>());
    }

    
}
