using UnityEngine;

public class CellGrid
{
    public Vector2 pos;
    public bool isObstacle;
    
    public CellGrid(Vector2 pos, bool isObstacle = false)
    {
        this.pos = pos;
        this.isObstacle = isObstacle;
    }
}