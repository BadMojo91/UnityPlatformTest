using UnityEngine;
using System;

[Serializable]public class Map {
    public string mapName;
    public Tile[,] tiles;
    public int width, height;

    public Map(string n, Tile[,] t, int w, int h) {
        mapName = n;
        tiles = t;
        width = w;
        height = h;
    }

    public Map() {
        mapName = "";
        tiles = new Tile[32, 32];
        width = 32;
        height = 32;
    }
}