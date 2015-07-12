using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class boardPiece
{
    //This entire class might be useless...
    private GameObject _piece;
    private int _x, _y;

    public boardPiece(GameObject piece)
    {
        _piece = piece;
    }

    public void setLocation(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public Vector2 getLocation()
    {
        return new Vector2(_x, _y);
    }

    public GameObject getPiece()
    {
        return _piece;
    }

    internal bool isAt(int x, int y)
    {
        return ((_x == x) && (_y == y));
    }
}
