using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cmdCapturePiece : boardCommand
{
    private chessPiece _captured, _capturer;

    public cmdCapturePiece(chessPiece captured, chessPiece capturer)
    {
        _captured = captured;
        _capturer = capturer;
    }

    override public bool execute(controllerBoard board)
    {
        board.removePieceFromList(_captured);
        board.addPieceToCapturedList(_captured);

        int capturedX = ((_captured.getTeamColor() == chessPiece.TeamColor.BLACK)?(9):(-2));
        int capturedY = board.getNumCapturedChess(_captured.getTeamColor()) - 1;

        if(capturedY >= controllerBoard.BOARD_HEIGHT)
        {
            capturedX += ((_captured.getTeamColor() == chessPiece.TeamColor.BLACK)?(1):(-1));
            capturedY -= controllerBoard.BOARD_HEIGHT;
        }

        board.movePieceToLocation(_captured, capturedX, capturedY, false);

        return true;
    }
}
