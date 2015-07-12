using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cmdAddPiece : boardCommand
{
    private componentPiece _actor;
    private int _x, _y;

    public cmdAddPiece(componentPiece actor, int x, int y)   //Deltas, storing the original start x and y upon execution
    {
        _actor = actor;
        _x = x;
        _y = y;
    }

    override public bool execute(controllerBoard board)
    {
        board.addPieceToList(_actor);
        board.setPieceLocation(_actor, _x, _y);
        return true;
    }
}
