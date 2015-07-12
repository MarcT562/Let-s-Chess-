using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class componentPiece : MonoBehaviour
{
    protected int _x, _y;
    protected List<Vector2> _validMoves;
    protected List<Vector2> _potentialAttacks;

	void Start () {
	}
	
	void Update () {
	}

    public virtual bool canMoveTo(int x, int y, controllerBoard board) { return false; }

    public virtual void setLocation(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public virtual void moveTo(int x, int y, controllerBoard board)
    {
        _x = x;
        _y = y;
    }

    public Vector2 getLocation()
    {
        return new Vector2(_x, _y);
    }

    public bool isAt(int x, int y)
    {
        return ((_x == x) && (_y == y));
    }

    public virtual List<Vector2> getValidMoveList(controllerBoard board)
    {
        return _validMoves;
    }

    public virtual bool hasValidMoveAt(int x, int y, controllerBoard board)
    {
        Vector2 move = new Vector2(x, y);
        foreach (Vector2 validMove in _validMoves)
        {
            if(move == validMove)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void clearValidMoves()
    {
        _validMoves = null;
    }
}
