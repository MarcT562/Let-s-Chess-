using UnityEngine;
using System.Collections;

public class boardCommand
{
    //Utilizing the command pattern in order to set up for synchronous gameplay (if desired) as well as replays and other nifty dealie-ma-bobs
    virtual public bool execute(controllerBoard board) { return false; }
}
