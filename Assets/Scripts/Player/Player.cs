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

    public WorldData worldData;

    //World coords
    public Vector2 worldPosCurrentBlock;
    public Vector2 worldPosCurrentChunk;
    public Vector2 chunkPosCurrentBlock;
    //Raycasting
    public GameObject currentChunk; //current chunk traceline
    public Vector2 currentBlock;    //current block in that chunk traceline

    private void Start() {
        for(int y = 0; y < 5; y++) {
            for(int x = 0; x < 5; x++) {
                worldData.CreateChunk(x, y);
            }
        }
        worldData.LoadChunks(5, 5);
    }

    void ChunkLoader() {
        foreach(GameObject o in worldData.chunksLoaded) {
            int x = 0;
            if(o.transform.position.x >= 0) {
                while(o.transform.position.x > 15) {
                    x++;
                }
            }
            else {
                while(o.transform.position.x < -15) {
                    x--;
                }
            }


        }
    }

    void FindWorldPosition() {

        int cx = 0, cy = 0, bx = 0, by = 0;
        Vector2 chunkPos = Vector2.zero;

        bx = (int)Mathf.Round(transform.position.x);
        by = (int)Mathf.Round(transform.position.y);
        cx = bx;
        cy = by;

        if(bx >= 0) {
            while(cx > 32) {
                cx -= 32;
                chunkPos.x++;
            }
        }
        else {
            chunkPos.x--;
            while(cx < -32) {
                cx += 32;
                chunkPos.x++;
            }
        }
        if(by >= 0) {
            while(cy > 32) {
                cy -= 32;
                chunkPos.y++;
            }
        }
        else {
            while(cy < -32) {
                cy += 32;
                chunkPos.y--;
            }
        }

        worldPosCurrentBlock = new Vector2(bx, by);
        chunkPosCurrentBlock = new Vector2(cx, cy);
        worldPosCurrentChunk = chunkPos;

    }

        private void Update() {
        FindWorldPosition();
        controller.RotateHandsWithMouseUpdate(arms, head, this); //head rotation


        if(Input.GetButtonDown("Fire1"))
            if(TraceLine(transform.position, gun.transform.TransformDirection(Vector3.right))) {
                int sm = currentChunk.GetComponent<MeshBuilder>().ReturnTileType((int)currentBlock.x, (int)currentBlock.y);
                if(
                    sm == 1 ||
                    sm == 2
                    )
                    currentChunk.GetComponent<MeshBuilder>().SetTile((int)currentBlock.x, (int)currentBlock.y, 0);

            }
        if(Input.GetButton("Fire2"))
        {
            ReplaceAtMousePos(1);
            
        }


    }
    
    void ReplaceAtMousePos(int tile)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos += new Vector3(mousePos.normalized.x, mousePos.normalized.y, 0) * -0.5f;
        mousePos = new Vector3(Mathf.RoundToInt(mousePos.x - 0.5f), Mathf.RoundToInt(mousePos.y + 0.5f), 0);

        //Debug.Log(mousePos);
        int x = (int)mousePos.x;
        int y = (int)mousePos.y;
        int cX = 0;
        int cY = 0;
        //if(levelGrid.blocks[x + 1, y].subMesh != 0 || levelGrid.blocks[x - 1, y].subMesh != 0 || levelGrid.blocks[x, y + 1].subMesh != 0 || levelGrid.blocks[x, y - 1].subMesh != 0) {
            while(x >= 32) {
                x -= 32;
                cX++;
            }
            while(y >= 32) {
                y -= 32;
                cY++;
            }
        GameObject c = GameObject.Find("Chunk_" + cX + "," + cY);
        if(c == null) {
            worldData.CreateChunk(cX, cY);
            c = GameObject.Find("Chunk_" + cX + "," + cY);
        }
        
            c.GetComponent<MeshBuilder>().SetTile(x, y, tile);
       // }
    }

    public bool TraceLine(Vector3 pos, Vector3 end) {
        RaycastHit hit;
        if(Physics.Raycast(pos, end, out hit)) {
            Vector2 point = new Vector2(hit.point.x, hit.point.y);
            point += (new Vector2(hit.normal.x, hit.normal.y)) * -0.5f;
            point = new Vector2(Mathf.RoundToInt(point.x - 0.5f), Mathf.RoundToInt(point.y + 0.5f));
            int i = 0;
            int y = 0;
            if(hit.point.x >= 0) { 
                while(point.x >= 32) {
                    point.x -= 32;
                    i++;
                }
            }
            else {
                Debug.Log(hit + " " + point);
                i--;
                while(point.x <= -32) {
                    point.x += 32;
                    i--;
                }
            }

            while(point.y >= 32) {
                point.y -= 32;
                y++;
            }
            currentBlock = new Vector3((int)point.x, (int)point.y);
            if(currentBlock.x < 0) {
                currentBlock.x = 32 - (int)Mathf.Abs(currentBlock.x);
            }
            Debug.Log(currentBlock);
            currentChunk = hit.collider.gameObject;
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
