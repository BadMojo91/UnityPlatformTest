using System;
[Serializable]
public class Chunk {
    public Tile[] tile;
    public int x, y;
	public Chunk(Tile[] _tile) {
        tile = _tile;
    }
}
