using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cmdMovePiece : boardCommand
{
    private componentPiece _actor;
    private int _dX, _dY;
    private int _startX, _startY;
    private bool _resetAllValidMoves;

    public cmdMovePiece(componentPiece actor, int dX, int dY, bool resetAllValidMoves = true)   //Deltas, storing the original start x and y upon execution
    {
        _actor = actor;
        _dX = dX;
        _dY = dY;
        _resetAllValidMoves = resetAllValidMoves;
    }

    override public bool execute(controllerBoard board)
    {
        componentPiece piece = _actor.GetComponent<componentPiece>();
        if(piece == null)
        {
            Debug.Log("Trying to move something that is not a piece");
            return false;
        }

        Vector2 pieceLocation = piece.getLocation();
        _startX = (int)pieceLocation.x;
        _startY = (int)pieceLocation.y;

        int newX = _startX + _dX;
        int newY = _startY + _dY;

        //Validate piece can move to location
        if(newX < 0 || newX >= controllerBoard.BOARD_WIDTH || newY < 0 || newY >= controllerBoard.BOARD_HEIGHT)
        {
            Debug.Log("Attempted to move past edge of board to (" + newX + "," + newY + ")");
            return false;
        }

        if (piece.canMoveTo(newX, newY, board))
        {
            board.movePieceToLocation(_actor, newX, newY);
            if(_resetAllValidMoves) //This is the quick to make way, the longer way involves only changing the affected pieces.
            {
                foreach (componentPiece aPiece in board.getActivePieces())
                {
                    aPiece.clearValidMoves();
                }
            }
            board.changeTurn();
            return true;
        }
        else
        {
            return false;
        }
    }
}
