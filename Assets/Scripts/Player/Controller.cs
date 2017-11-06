using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public bool flipped;

        public void MoveUpdate(Player player, Animator anim, Transform pl, Transform gun) {
            float horizontal = Input.GetAxis("Horizontal") ;
            anim.SetFloat("Move", horizontal);
            horizontal *= Time.deltaTime;
            Vector2 moveDirection = Vector2.zero;

            if(Input.GetButton("Horizontal"))
                anim.SetBool("Moving", true);
            else
                anim.SetBool("Moving", false);

            moveDirection = new Vector2(horizontal, 0);
            moveDirection = pl.TransformDirection(moveDirection) * walkSpeed;

            if((moveDirection.x > 0 && !Physics.Raycast(pl.position + (Vector3.down * 0.5f), Vector2.right, 0.3f)) || (moveDirection.x < 0 && !Physics.Raycast(pl.position + (Vector3.down * 0.5f), Vector2.left, 0.3f)))
                pl.Translate(moveDirection * Time.deltaTime);
        }
        public void PhysicsUpdate(Transform player, Rigidbody rb) {

            if(Input.GetButton("Jump") && canJump)
                rb.AddForce(player.TransformDirection(Vector2.up * jumpMultiplier) * jumpSpeed);

            if(Physics.Raycast(player.position, Vector3.down + (Vector3.left * 0.25f), reachToFloor) || Physics.Raycast(player.position, Vector3.down + (Vector3.right * 0.25f), reachToFloor)) {
                canJump = true;
            }
            else {
                canJump = false;
            }
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

            //flip player sprite based on mouse position
            if(Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 0.5f)
                flipped = true;
            else
                flipped = false;
        }
    }
