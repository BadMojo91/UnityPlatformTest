using System;
[Serializable]
public class Tile {
    public int x, y;
    public int subMesh;
    public Tile(int _x, int _y, int _subMesh) {
        x = _x;
        y = _y;
        subMesh = _subMesh;
    }
}
