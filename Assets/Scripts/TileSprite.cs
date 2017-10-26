using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class TileSprite : MonoBehaviour {

    public Texture2D txt;
    public Sprite[] sprite;

    public void SetSpriteArray() {
       string s = AssetDatabase.GetAssetPath(txt);
        sprite = AssetDatabase.LoadAllAssetsAtPath(s).OfType<Sprite>().ToArray();
    }

   
}
