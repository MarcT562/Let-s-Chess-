using Sfs2X.Entities.Data;
using Sfs2X.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class pieceData : SerializableSFSType
{
    public string _name;
    public bool _isAlive;
    public chessPiece.TeamColor _color;
    public enumChessPieceType _type;
    public int _x, _y;

    public pieceData() : this("none", false, chessPiece.TeamColor.ALL, enumChessPieceType.PAWN, -1, -1)
    {
    }

    public pieceData(string name, bool isAlive, chessPiece.TeamColor color, enumChessPieceType type, int x, int y)
    {
        _name = name;
        _isAlive = isAlive;
        _color = color;
        _type = type;
        _x = x;
        _y = y;
    }
}
