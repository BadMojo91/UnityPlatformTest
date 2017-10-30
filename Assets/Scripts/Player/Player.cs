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

    public LevelGrid levelGrid;

    public Vector2 currentBlock;
    public int currentChunkX, currentChunkY;

   
    private void Update() {
        controller.RotateHandsWithMouseUpdate(arms, head, this); //head rotation

        if(Input.GetButtonDown("Fire1"))
            if(TraceLine(transform.position, gun.transform.TransformDirection(Vector3.right)))
              levelGrid.DestroyTileAt(levelGrid.chunks[currentChunkX,currentChunkY].GetComponent<Chunk>(),(int)currentBlock.x, (int)currentBlock.y);

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
        int cX = 0;
        int cY = 0;
        //if(levelGrid.blocks[x + 1, y].subMesh != 0 || levelGrid.blocks[x - 1, y].subMesh != 0 || levelGrid.blocks[x, y + 1].subMesh != 0 || levelGrid.blocks[x, y - 1].subMesh != 0) {
            while(x >= levelGrid.MAX_CHUNK_SIZE) {
                x -= levelGrid.MAX_CHUNK_SIZE;
                cX++;
            }
            while(y >= levelGrid.MAX_CHUNK_SIZE) {
                y -= levelGrid.MAX_CHUNK_SIZE;
                cY++;
            }
            levelGrid.CreateTileAt(levelGrid.chunks[cX,cY].GetComponent<Chunk>(), x, y, tile);
       // }
    }
    public bool TraceLine(Vector3 pos, Vector3 end) {
        RaycastHit hit;
        if(Physics.Raycast(pos, end, out hit)) {
            Vector2 point = new Vector2(hit.point.x, hit.point.y) / levelGrid.TILE_SCALE;
            point += (new Vector2(hit.normal.x, hit.normal.y)) * -0.5f;
            point = new Vector2(Mathf.RoundToInt(point.x - 0.5f), Mathf.RoundToInt(point.y + 0.5f));
            int i = 0;
            int y = 0;
            while(point.x >= levelGrid.MAX_CHUNK_SIZE) {
                point.x -= levelGrid.MAX_CHUNK_SIZE;
                i++;
            }
            while(point.y >= levelGrid.MAX_CHUNK_SIZE) {
                point.y -= levelGrid.MAX_CHUNK_SIZE;
                y++;
            }
            currentBlock = new Vector3((int)point.x, (int)point.y);
            currentChunkX = i;
            currentChunkY = y;
            return true;
        }
        else {
            currentBlock = Vector2.zero;
            return false;
        }
    }

    void FixedUpdate() {
        controller.MoveUpdate(this, legsAnim, transform, gun);
        controller.PhysicsUpdate(transform, GetComponent<Rigidbody>());
    }

    
}
