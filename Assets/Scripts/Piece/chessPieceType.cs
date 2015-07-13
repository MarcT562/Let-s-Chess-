using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class chessPieceType
{
    private string _name;
    private List<Vector2> _canAttackCache;  //A little optimization hack to improve performance because it's annoying me. A better one would involve a better caching system.

    public chessPieceType(string name)
    {
        _name = name;
        canAttack = delegate(int x, int y, controllerBoard board, chessPiece piece)
        {
            List<Vector2> validMoves = getCanAttackList(board, piece);
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

    private List<Vector2> getCanAttackList(controllerBoard board, chessPiece piece)
    {
        if (_canAttackCache == null)
        {
            _canAttackCache = generateMoveList(board, piece, true);
        }
        return _canAttackCache;
    }

    public void clearCanAttackCache() { _canAttackCache = null; }

    public delegate List<Vector2> delegateGenerateMoveList(controllerBoard board, chessPiece piece, bool attackOwn = false);
    public delegate bool delegateCanAttack(int x, int y, controllerBoard board, chessPiece piece);

    public delegateGenerateMoveList generateMoveList;
    public delegateCanAttack canAttack;

    public string getName() { return _name; }
}