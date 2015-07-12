using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class chessPiece : componentPiece
{
    public enum TeamColor { BLACK, WHITE };
    
    private chessPieceType _pieceType;
    private TeamColor _color;

    public chessPiece(chessPieceType type, TeamColor color)
    {
        _pieceType = type;
        _color = color;
    }

    public void init(chessPieceType type, TeamColor color)
    {
        _pieceType = type;
        _color = color;
    }

    public override bool canMoveTo(int x, int y, controllerBoard board)
    {
        if(_validMoves == null)
        {
            _validMoves = _pieceType.generateMoveList(board, this);
        }

        foreach (Vector2 validMove in _validMoves)
        {
            if (((int)validMove.x == x) && ((int)validMove.y == y))
                return true;
        }

        return false;
    }

    public override void moveTo(int x, int y, controllerBoard board)
    {
        List<componentPiece> piecesAtDestination = board.getPiecesAtLocation(x, y);

        base.moveTo(x, y, board);
        if (_validMoves != null)
        {
            _validMoves = null;
        }

        if(piecesAtDestination.Count > 0)
        {
            foreach (componentPiece piece in piecesAtDestination)
            {
                board.capturePiece((chessPiece)piece, this);
            }
        }
    }

    public TeamColor getTeamColor()
    {
        return _color;
    }

    public override List<Vector2> getValidMoveList(controllerBoard board)
    {
        if(_validMoves == null)
        {
            _validMoves = _pieceType.generateMoveList(board, this);
        }
        return _validMoves;
    }

    public override bool hasValidMoveAt(int x, int y, controllerBoard board)
    {
        if(_validMoves == null)
        {
            _validMoves = _pieceType.generateMoveList(board, this);
        }
        return base.hasValidMoveAt(x, y, board);
    }

    public string getPieceTypeName() { return _pieceType.getName(); }

    public bool canAttack(int x, int y, controllerBoard board)
    {
        return _pieceType.canAttack(x, y, board, this);
    }
}
