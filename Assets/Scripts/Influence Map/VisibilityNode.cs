using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityNode
{
    public Vector3 worldPosition;
    public bool visible;
    public int gridX;
    public int gridY;

    public VisibilityNode(Vector3 _worldPos, bool _visible, int _gridX, int _gridY)
    {
        worldPosition = _worldPos;
        visible = _visible;
        gridX = _gridX;
        gridY = _gridY;
    }
}
