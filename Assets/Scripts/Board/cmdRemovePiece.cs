using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cmdRemovePiece : boardCommand
{
    override public bool execute(controllerBoard board)
    {
        return true;
    }
}
