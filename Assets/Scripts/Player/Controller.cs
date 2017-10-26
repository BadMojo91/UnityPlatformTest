using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace protagonist {
    [System.Serializable]
    public class Controller {

        public float walkSpeed;
        public float runSpeed;
        [Range(0, 1)]
        public float friction;
        public float jumpMultiplier;
        public float jumpSpeed;
        public bool canJump;

        public float reach;
        public float reachToFloor;
        public float reachToCeiling;
        float angle;
        bool flipped;

        public void MoveUpdate(Player player, Animator anim, Transform pl, Transform gun) {
            float horizontal = Input.GetAxis("Horizontal") ;
            anim.SetFloat("Move", horizontal);
            horizontal *= Time.deltaTime;
            Vector2 moveDirection = Vector2.zero;

            if(Input.GetButton("Horizontal"))
                anim.SetBool("Moving", true);
            else
                anim.SetBool("Moving", false);




            moveDirection = new Vector2(horizontal, DetectSlope(pl, horizontal));
            moveDirection = pl.TransformDirection(moveDirection) * walkSpeed;

            if((moveDirection.x > 0 && !Physics.Raycast(pl.position + (Vector3.down * 0.5f), Vector2.right, 0.3f)) || (moveDirection.x < 0 && !Physics.Raycast(pl.position + (Vector3.down * 0.5f), Vector2.left, 0.3f)))
                pl.Translate(moveDirection * Time.deltaTime);
        }

        public void FlipX(bool b, Player player) {

            flipped = b;

            foreach(SpriteRenderer s in player.playerSprite)
                s.flipX = b;
        }

        public void PhysicsUpdate(Transform player, Rigidbody rb) {

            if(Input.GetButton("Jump") && canJump)
                rb.AddForce(player.TransformDirection(Vector2.up * jumpMultiplier) * jumpSpeed);

            if(Physics.Raycast(player.position, Vector3.down, reachToFloor)) {
                canJump = true;
            }
            else {
                canJump = false;
            }
        }


        float DetectSlope(Transform player, float hor) {
            RaycastHit2D[] hits = new RaycastHit2D[2];
            int h = Physics2D.RaycastNonAlloc(player.transform.position, -Vector2.up, hits); //cast downwards
            float ySpeed = 0;
            if(h > 1) { //if we hit something do stuff

                angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg); //get angle

                if(angle > 30) {

                    ySpeed = 0;//DoSomething(); //change your animation
                               // Debug.Log(ySpeed);
                }

            }

            return ySpeed;
        }

        public void RotateHandsWithMouseUpdate(Transform hands, Transform head, Player player) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(flipped) {
                hands.rotation = Quaternion.LookRotation(-Vector3.forward, mousePos - hands.position);
                head.rotation = Quaternion.LookRotation(-Vector3.forward, mousePos - head.position);
            }
            else {
                hands.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - hands.position);
                head.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - head.position);
            }
            hands.rotation *= Quaternion.Euler(0, 0, 90);
            head.rotation *= Quaternion.Euler(0, 0, 90);

            if(Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 0.5f) {
                FlipX(true, player);
            }
            else {
                FlipX(false, player);
            }

            //if(hands.eulerAngles.z > 90 || hands.eulerAngles.z < -90)
            //   FlipX(true, player);
            //else
            //   FlipX(false, player);
        }
    }
}