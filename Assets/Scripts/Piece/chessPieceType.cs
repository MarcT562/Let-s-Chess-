using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class chessPieceType
{
    private string _name;

    public chessPieceType(string name)
    {
        _name = name;
        canAttack = delegate(int x, int y, controllerBoard board, chessPiece piece)
        {
            List<Vector2> validMoves = generateMoveList(board, piece, true);
            foreach (Vector2 validMove in validMoves)
            {
                if((x == (int)validMove.x) && (y == (int)validMove.y))
                {
                    return true;
                }
            }
            return false;
        };
    }

    public delegate List<Vector2> delegateGenerateMoveList(controllerBoard board, chessPiece piece, bool attackOwn = false);
    public delegate bool delegateCanAttack(int x, int y, controllerBoard board, chessPiece piece);

    public delegateGenerateMoveList generateMoveList;
    public delegateCanAttack canAttack;

    public string getName() { return _name; }
}