using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInfo : MonoBehaviour {

    public float health;
    float maxHealth;
    public Sprite[] damageTile;
    public SpriteRenderer damageSprite;
    private void Awake() {
        maxHealth = health;
    }

    private void Update() {
        if(health < 0) {
            Destroy(gameObject);
        }
        float hp = health / maxHealth * 100;
        if(hp < 10)
            damageSprite.sprite = damageTile[8];
        else if(hp < 20)
            damageSprite.sprite = damageTile[7];
        else if(hp < 30)
            damageSprite.sprite = damageTile[6];
        else if(hp < 40)
            damageSprite.sprite = damageTile[5];
        else if(hp < 50)
            damageSprite.sprite = damageTile[4];
        else if(hp < 60)
            damageSprite.sprite = damageTile[3];
        else if(hp < 70)
            damageSprite.sprite = damageTile[2];
        else if(hp < 80)
            damageSprite.sprite = damageTile[1];
        else if(hp < 90)
            damageSprite.sprite = damageTile[0];



    }

    private void OnCollisionEnter2D(Collision2D collision) {
        //Debug.Log(collision);
        if(collision.gameObject.tag != "Player") {

            if(collision.gameObject.GetComponent<Projectile>())
                health -= collision.gameObject.GetComponent<Projectile>().damage;

            Destroy(collision.gameObject);
            //Destroy(gameObject);
        }
    }

}
