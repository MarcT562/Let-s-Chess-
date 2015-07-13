using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class controllerBoard : MonoBehaviour
{
    //Whee less data driven because it's a prototype
    public static int BOARD_WIDTH = 8;
    public static int BOARD_HEIGHT = 8;
    public static int BOARD_Z = 0;
    public static int TILE_EFFECT_Z = -1;
    public static int PIECE_Z = -5;
    public static int TURN_GAME_OVER = -1;

    //Origin for a chess board is the bottom left with white on the bottom. a-h 1-8, but for my case i'm using 0-based numbers for both because computer, but with the same directionality
    private static float BOARD_OFFSET_X = -2.8f;
    private static float BOARD_OFFSET_Y = -2.8f;
    private static float BOARD_TILE_WIDTH = 0.8f;
    private static float BOARD_TILE_HEIGHT = 0.8f;


    private List<boardCommand> _commandHistory;   //If performance starts becoming an issue for either, alter into a dense array. but for prototyping this is faster.
    private List<componentPiece> _pieces;
    private componentPiece _selectedPiece;
    private List<GameObject> _validMoveTileEffects;

    private bool _mouseDown;

    //This should definitely end up being subclassed into controllerChessBoard... but for the current purposes this will do.
    private List<componentPiece> _captured;
    private chessGameMode _gameMode;
    private int _numTurns;
    private int _currentTurn;
    private chessPiece.TeamColor[] _playerColors;
    private bool[] _playerActive;   //in case there are more than 2 players...
    private bool _inCheck;
    private controllerUI _UI;

    void Start()
    {
        _UI = gameObject.GetComponent<controllerUI>();
        _commandHistory = new List<boardCommand>();
        _pieces = new List<componentPiece>();
        _validMoveTileEffects = new List<GameObject>();

        Vector3 boardPosition = gameObject.transform.position;
        boardPosition.z = BOARD_Z;

        //Quick and Dirty Piece of UI
        GameObject selectedThing = GameObject.Find("Selected Tile");
        selectedThing.transform.SetParent(gameObject.transform, false);
        selectedThing.transform.localPosition = new Vector3(1, 1, TILE_EFFECT_Z);
        selectedThing.renderer.enabled = false;

        //Start a brand new game of chess
        initNewChessGame(chessGameMode.SOLO);

        //TEST SECTION
        //GameObject whitePawn1Object = GameObject.Find("WhitePawn1");
        //componentPiece whitePawn1 = whitePawn1Object.GetComponent<componentPiece>();
        //GameObject whiteRook0Object = GameObject.Find("WhiteRook0");
        //componentPiece whiteRook0 = whiteRook0Object.GetComponent<componentPiece>();
        //GameObject blackPawn1Object = GameObject.Find("BlackPawn1");
        //componentPiece blackPawn1 = blackPawn1Object.GetComponent<componentPiece>();

        //movePiece(whitePawn1, 1, 1);    
        //movePiece(whitePawn1, 0, 2);
        //movePiece(whitePawn1, 2, 0);
        //movePiece(whitePawn1, 0, -1);
        //movePiece(whitePawn1, 0, 2);    
        //movePiece(whitePawn1, 0, 1);
        //movePiece(whitePawn1, 0, 1);
        //movePiece(whitePawn1, 0, 1);
        //movePiece(whitePawn1, 0, 1);
        //movePiece(whitePawn1, 0, 1);
        //movePiece(whitePawn1, 1, 1);
        //movePiece(whitePawn1, -1, 1);
        //movePiece(whiteRook0, 1, 0);
        //movePiece(whiteRook0, 0, 2);
        //movePiece(whiteRook0, 0, 4);


        //selectPieceAtLocation(1, 6);
        //selectPieceAtLocation(1, 0);
        //selectPieceAtLocation(2, 0);
	}
	
	void Update () {
        /* Touch controls to come later
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);    //Only the first touch matters
            if (touch.phase == TouchPhase.Began)
            {    
                //Will fill in later, for now where you stop clicking is where it counts it from.
                //What to fill in here, keep track of the grid location clicked? or something like that, and if it is the same as when it ends, then we are good. so i can do long clicks?
                //Do i want to do long clicks?
            }

            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
            {
                Debug.Log("I have been touched");
                //Vector3 position = Camera.main.ScreenToWorldPoint(touch.position);
            }
        }
        */

        //Being lazy with creating the click.
        
        if(Input.GetMouseButtonDown(0) && _mouseDown == false)
        {
            _mouseDown = true;
        }

        if(Input.GetMouseButtonUp(0) && _mouseDown == true)
        {
            _mouseDown = false;

            
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector2(0,0));
            if(hit != null && hit.collider != null)
            {
                Debug.Log("Raycast hit " + hit.collider.gameObject.name);
                componentPiece pieceHit = hit.collider.gameObject.GetComponent<componentPiece>();
                componentValidMoveTile moveTileHit = hit.collider.gameObject.GetComponent<componentValidMoveTile>();
                if(pieceHit != null)
                {
                    Debug.Log("Raycast hit a piece");

                    if(_pieces.Contains(pieceHit) == false)
                    {
                        return;
                    }

                    if(pieceHit == _selectedPiece)
                    {
                        deselectPiece();
                        return;
                    }
                    else
                    {
                        if (_selectedPiece != null)
                        {
                            bool moveIsValid = false;
                            foreach (Vector2 validMove in _selectedPiece.getValidMoveList(this))
                            {
                                Vector2 pieceHitLocation = pieceHit.getLocation();
                                if (validMove == pieceHitLocation)
                                {
                                    moveSelectedPieceToLocation((int)pieceHitLocation.x, (int)pieceHitLocation.y);
                                    deselectPiece();
                                    moveIsValid = true;
                                    return;
                                }
                            }
                            if (moveIsValid == false)
                            {
                                selectPiece(pieceHit);
                                return;
                            }
                        }
                        else
                        {
                            selectPiece(pieceHit);
                            return;
                        }
                    }
                }
                else if (moveTileHit != null)
                {
                    Vector2 moveTileHitLocation = moveTileHit.getLocation();
                    moveSelectedPieceToLocation((int)moveTileHitLocation.x, (int)moveTileHitLocation.y);
                    deselectPiece();
                    return;
                }
            }
        }
	}

    public void initNewChessGame(chessGameMode mode)
    {
        clear();

        _gameMode = mode;
        _captured = new List<componentPiece>();

        //leaving this out here to make the switch simpler.
        if (_gameMode == chessGameMode.SOLO_SELF_ATTACK || _gameMode == chessGameMode.TWO_PLAYER_SELF_ATTACK)
        {
            chessPiece.canAttackOwnUnits = true;
        }
        else
        {
            chessPiece.canAttackOwnUnits = false;
        }

        //Using a switch because this is a prototype. If it was a final product I would make each game mode derived from a base chessGameMode class
        // That would be so that any game mode wanted could be created, with inheiritance, and have more long-term flexibility.
        switch(_gameMode)
        {
            case chessGameMode.SOLO_SELF_ATTACK:
            case chessGameMode.SOLO:
                _currentTurn = 0;

                _playerColors = new chessPiece.TeamColor[1];
                _playerColors[0] = chessPiece.TeamColor.ALL;

                _playerActive = new bool[1];
                _playerActive[0] = true;
                break;
            case chessGameMode.TWO_PLAYER_SELF_ATTACK:
            case chessGameMode.TWO_PLAYER:
                _currentTurn = 0;

                _playerColors = new chessPiece.TeamColor[2];
                _playerColors[0] = chessPiece.TeamColor.WHITE;
                _playerColors[1] = chessPiece.TeamColor.BLACK;
                
                _playerActive = new bool[2];
                _playerActive[0] = true;
                _playerActive[1] = true;
                break;
            case chessGameMode.TWO_AND_A_DIETY:
                _currentTurn = 0;

                _playerColors = new chessPiece.TeamColor[3];
                _playerColors[0] = chessPiece.TeamColor.WHITE;
                _playerColors[1] = chessPiece.TeamColor.BLACK;
                _playerColors[2] = chessPiece.TeamColor.ALL;

                _playerActive = new bool[3];
                _playerActive[0] = true;
                _playerActive[1] = true;
                _playerActive[2] = true;
                break;
        }

        //Instead of creating sub classes for the pieces I am using the type pattern.

        //Sprites

        Sprite blackPawnSprite = Resources.Load<Sprite>("Pawn_Black");
        Sprite whitePawnSprite = Resources.Load<Sprite>("Pawn_White");

        Sprite blackRookSprite = Resources.Load<Sprite>("Rook_Black");
        Sprite whiteRookSprite = Resources.Load<Sprite>("Rook_White");

        Sprite blackKnightSprite = Resources.Load<Sprite>("Knight_Black");
        Sprite whiteKnightSprite = Resources.Load<Sprite>("Knight_White");

        Sprite blackBishopSprite = Resources.Load<Sprite>("Bishop_Black");
        Sprite whiteBishopSprite = Resources.Load<Sprite>("Bishop_White");

        Sprite blackQueenSprite = Resources.Load<Sprite>("Queen_Black");
        Sprite whiteQueenSprite = Resources.Load<Sprite>("Queen_White");

        Sprite blackKingSprite = Resources.Load<Sprite>("King_Black");
        Sprite whiteKingSprite = Resources.Load<Sprite>("King_White");

        ///////////////////////////// PAWNS ///////////////////////////////
        chessPieceType pawn = new chessPieceType("Pawn");
        pawn.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            Vector2 origin = piece.getLocation();
            int startY = 1;
            int forward = 1;
            if(piece.getTeamColor() == chessPiece.TeamColor.BLACK)
            {
                startY = 6;
                forward = -1;
            }

            List<Vector2> validMoves = new List<Vector2>();

            int tempX, tempY;

            tempX = (int)origin.x;
            tempY = (int)origin.y + (1 * forward);
            if(onBoard(tempX, tempY))   //Regular movement.
            {
                List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                if (piecesOnDest.Count == 0)
                {
                    validMoves.Add(new Vector2(tempX, tempY));
                    if ((int)origin.y == startY)    //You can only move 2 squares if another unit is not directly in front of this one. Also if it is at the origin then there is no need to check if 2 squares forward is off the board.
                    {
                        tempY += (1 * forward);
                        piecesOnDest = getPiecesAtLocation(tempX, tempY);
                        if(piecesOnDest.Count == 0)
                        {
                            validMoves.Add(new Vector2(tempX, tempY));
                        }
                        tempY -= (1 * forward); //because of the attack from start.
                    }
                }
                //This is inside the onboard check because if 1 square foward is off board then a pawn CANNOT attack, because it has to move at least 1 square forward, and the board is square.
                tempX -= 1; //Check left
                if(onBoard(tempX, tempY))
                {
                    piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if(piecesOnDest.Count > 0)
                    {
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                            validMoves.Add(new Vector2(tempX, tempY));
                    }
                }
                tempX += 2; //Switch to check right
                if (onBoard(tempX, tempY))
                {
                    piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count > 0)
                    {
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                            validMoves.Add(new Vector2(tempX, tempY));
                    }
                }
            }

            //DEBUG STUFF
            //foreach (Vector2 validMove in validMoves)
            //{
                //Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            //}

            return validMoves;
        };

        pawn.canAttack = delegate(int x, int y, controllerBoard board, chessPiece piece)
        {
            Vector2 origin = piece.getLocation();
            int forward = 1;
            if (piece.getTeamColor() == chessPiece.TeamColor.BLACK)
            {
                forward = -1;
            }

            int tempX, tempY;

            tempX = (int)origin.x + 1;
            tempY = (int)origin.y + (1 * forward);
            if((tempX == x) & (tempY == y))
                return true;

            tempX -= 2;
            if((tempX == x) & (tempY == y))
                return true;

            return false;
        };


        /////////////////////// ROOKS /////////////////////////
        chessPieceType rook = new chessPieceType("Rook");
        rook.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            List<Vector2> validMoves = new List<Vector2>();

            Vector2 origin = piece.getLocation();
            int tempX, tempY, dX, dY, count;

            count = 0;
            dX = 1;
            dY = 0;
            tempX = (int)origin.x;
            tempY = (int)origin.y;
            while(count < 4)    //1 for each direction
            {
                bool countCompleted = false;
                tempX += dX;
                tempY += dY;
                if (onBoard(tempX, tempY))
                {
                    List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count == 0)
                    {
                        validMoves.Add(new Vector2(tempX, tempY));
                    }
                    else
                    {
                        countCompleted = true;
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                        {
                            validMoves.Add(new Vector2(tempX, tempY));
                        }
                    }
                }
                else
                {
                    countCompleted = true;
                }

                if(countCompleted)
                {
                    tempX = (int)origin.x;
                    tempY = (int)origin.y;
                    count++;
                    switch(count)
                    {
                        case 1:
                            dX = 0;
                            dY = 1;
                            break;
                        case 2:
                            dX = -1;
                            dY = 0;
                            break;
                        case 3:
                            dX = 0;
                            dY = -1;
                            break;
                    }
                }
            }

            //DEBUG STUFF
            foreach (Vector2 validMove in validMoves)
            {
                Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            }

            return validMoves;
        };


        /////////////////////// KNIGHTS /////////////////////////
        chessPieceType knight = new chessPieceType("Knight");
        knight.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            List<Vector2> validMoves = new List<Vector2>();

            Vector2 origin = piece.getLocation();
            int tempX, tempY, dX, dY, count;

            count = 0;
            dX = 2;
            dY = -1;
            tempX = (int)origin.x;
            tempY = (int)origin.y;
            while (count < 8)    //1 for each direction
            {
                tempX += dX;
                tempY += dY;
                if (onBoard(tempX, tempY))
                {
                    List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count == 0)
                    {
                        validMoves.Add(new Vector2(tempX, tempY));
                    }
                    else
                    {
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                        {
                            validMoves.Add(new Vector2(tempX, tempY));
                        }
                    }
                }

                tempX = (int)origin.x;
                tempY = (int)origin.y;
                count++;
                switch (count)
                {
                    case 1:
                        dX = 2;
                        dY = 1;
                        break;
                    case 2:
                        dX = 1;
                        dY = 2;
                        break;
                    case 3:
                        dX = -1;
                        dY = 2;
                        break;
                    case 4:
                        dX = -2;
                        dY = 1;
                        break;
                    case 5:
                        dX = -2;
                        dY = -1;
                        break;
                    case 6:
                        dX = -1;
                        dY = -2;
                        break;
                    case 7:
                        dX = 1;
                        dY = -2;
                        break;
                }
            }

            //DEBUG STUFF
            foreach (Vector2 validMove in validMoves)
            {
                Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            }

            return validMoves;
        };


        /////////////////////// BISHOPS /////////////////////////
        chessPieceType bishop = new chessPieceType("Bishop");
        bishop.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            List<Vector2> validMoves = new List<Vector2>();

            Vector2 origin = piece.getLocation();
            int tempX, tempY, dX, dY, count;

            count = 0;
            dX = 1;
            dY = 1;
            tempX = (int)origin.x;
            tempY = (int)origin.y;
            while (count < 4)    //1 for each direction
            {
                bool countCompleted = false;
                tempX += dX;
                tempY += dY;
                if (onBoard(tempX, tempY))
                {
                    List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count == 0)
                    {
                        validMoves.Add(new Vector2(tempX, tempY));
                    }
                    else
                    {
                        countCompleted = true;
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                        {
                            validMoves.Add(new Vector2(tempX, tempY));
                        }
                    }
                }
                else
                {
                    countCompleted = true;
                }

                if (countCompleted)
                {
                    tempX = (int)origin.x;
                    tempY = (int)origin.y;
                    count++;
                    switch (count)
                    {
                        case 1:
                            dX = -1;
                            dY = 1;
                            break;
                        case 2:
                            dX = -1;
                            dY = -1;
                            break;
                        case 3:
                            dX = 1;
                            dY = -1;
                            break;
                    }
                }
            }

            //DEBUG STUFF
            foreach (Vector2 validMove in validMoves)
            {
                Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            }

            return validMoves;
        };


        /////////////////////// QUEENS /////////////////////////
        chessPieceType queen = new chessPieceType("Queen");
        queen.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            List<Vector2> validMoves = new List<Vector2>();

            Vector2 origin = piece.getLocation();
            int tempX, tempY, dX, dY, count;

            count = 0;
            dX = 1;
            dY = 0;
            tempX = (int)origin.x;
            tempY = (int)origin.y;
            while (count < 8)    //1 for each direction
            {
                bool countCompleted = false;
                tempX += dX;
                tempY += dY;
                if (onBoard(tempX, tempY))
                {
                    List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count == 0)
                    {
                        validMoves.Add(new Vector2(tempX, tempY));
                    }
                    else
                    {
                        countCompleted = true;
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                        {
                            validMoves.Add(new Vector2(tempX, tempY));
                        }
                    }
                }
                else
                {
                    countCompleted = true;
                }

                if (countCompleted)
                {
                    tempX = (int)origin.x;
                    tempY = (int)origin.y;
                    count++;
                    switch (count)
                    {
                        case 1:
                            dX = 1;
                            dY = 1;
                            break;
                        case 2:
                            dX = 0;
                            dY = 1;
                            break;
                        case 3:
                            dX = -1;
                            dY = 1;
                            break;
                        case 4:
                            dX = -1;
                            dY = 0;
                            break;
                        case 5:
                            dX = -1;
                            dY = -1;
                            break;
                        case 6:
                            dX = 0;
                            dY = -1;
                            break;
                        case 7:
                            dX = 1;
                            dY = -1;
                            break;
                    }
                }
            }

            //DEBUG STUFF
            foreach (Vector2 validMove in validMoves)
            {
                Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            }

            return validMoves;
        };
        
        /////////////////////// KINGS /////////////////////////
        chessPieceType king = new chessPieceType("King");
        king.generateMoveList = delegate(controllerBoard board, chessPiece piece, bool attackOwn)
        {
            //TODO: before production at least, is to optimize this. or optimize all other move list generation
            Debug.Log("Creating A New Valid Move List for Piece " + piece.gameObject.name);

            List<Vector2> validMoves = new List<Vector2>();

            Vector2 origin = piece.getLocation();
            int tempX, tempY, dX, dY, count;

            count = 0;
            dX = 1;
            dY = 0;
            tempX = (int)origin.x;
            tempY = (int)origin.y;
            while (count < 8)    //1 for each direction
            {
                tempX += dX;
                tempY += dY;
                if (onBoard(tempX, tempY))
                {
                    bool isValidMove = true;
                    
                    foreach (componentPiece aPiece in _pieces)
                    {
                        chessPiece cPiece = (chessPiece)aPiece;
                        if (cPiece.getTeamColor() != piece.getTeamColor())
                        {
                            if (cPiece.getPieceTypeName().Equals("King"))   //Special case because kings have to caluclate other unit move positions
                            {
                                Vector2 otherKingLocation = cPiece.getLocation();
                                if((otherKingLocation.x >= (tempX - 1)) && (otherKingLocation.x <= (tempX + 1)) && (otherKingLocation.y >= (tempY - 1)) && (otherKingLocation.y <= (tempY +1)))
                                {
                                    isValidMove = false;
                                    break;
                                }
                            }
                            else if (cPiece.canAttack(tempX, tempY, this) == true)
                            {
                                isValidMove = false;
                                break;
                            }
                        }
                    }
                    

                    List<componentPiece> piecesOnDest = getPiecesAtLocation(tempX, tempY);
                    if (piecesOnDest.Count > 0)
                    {
                        chessPiece pieceOnDest = (chessPiece)piecesOnDest[0];
                        if ((pieceOnDest.getTeamColor() != piece.getTeamColor()) || (attackOwn == true))
                        {
                            //Extra layer of checks has to be done to make sure if the unit is taken that it won't place yourself in check.
                            //I haven't put in those extra checks yet.
                        }
                        else
                        {
                            isValidMove = false;
                        }
                    }

                    if (isValidMove == true)
                    {
                        validMoves.Add(new Vector2(tempX, tempY));
                    }
                }

                tempX = (int)origin.x;
                tempY = (int)origin.y;
                count++;
                switch (count)
                {
                    case 1:
                        dX = 1;
                        dY = 1;
                        break;
                    case 2:
                        dX = 0;
                        dY = 1;
                        break;
                    case 3:
                        dX = -1;
                        dY = 1;
                        break;
                    case 4:
                        dX = -1;
                        dY = 0;
                        break;
                    case 5:
                        dX = -1;
                        dY = -1;
                        break;
                    case 6:
                        dX = 0;
                        dY = -1;
                        break;
                    case 7:
                        dX = 1;
                        dY = -1;
                        break;
                }   
            }

            //clear the canAttackCache
            foreach (componentPiece aPiece in _pieces)
            {
                chessPiece cPiece = (chessPiece)aPiece;
                if (cPiece.getTeamColor() != piece.getTeamColor())
                {
                    if (!cPiece.getPieceTypeName().Equals("King"))   //Special case because kings have to caluclate other unit move positions
                    {
                        cPiece.clearCanAttackCache();
                    }
                }
            }

            //DEBUG STUFF
            foreach (Vector2 validMove in validMoves)
            {
                Debug.Log("Valid Move : (" + (int)validMove.x + "," + (int)validMove.y + ")");
            }

            return validMoves;
        };

        //Make Pawns
        for (int x = 0; x < 8; x++)
        {
            createChessPiece("BlackPawn" + x, pawn, blackPawnSprite, x, 6, chessPiece.TeamColor.BLACK);
            createChessPiece("WhitePawn" + x, pawn, whitePawnSprite, x, 1, chessPiece.TeamColor.WHITE);
        }
        //Make Rooks
        for (int x = 0; x < 2; x++)
        {
            createChessPiece("BlackRook" + x, rook, blackRookSprite, (x == 0) ? (0) : (7), 7, chessPiece.TeamColor.BLACK);
            createChessPiece("WhiteRook" + x, rook, whiteRookSprite, (x == 0) ? (0) : (7), 0, chessPiece.TeamColor.WHITE);
        }
        //Make Knights
        for (int x = 0; x < 2; x++)
        {
            createChessPiece("BlackKnight" + x, knight, blackKnightSprite, (x == 0) ? (1) : (6), 7, chessPiece.TeamColor.BLACK);
            createChessPiece("WhiteKnight" + x, knight, whiteKnightSprite, (x == 0) ? (1) : (6), 0, chessPiece.TeamColor.WHITE);
        }
        //Make Bishops
        for (int x = 0; x < 2; x++)
        {
            createChessPiece("BlackBishop" + x, bishop, blackBishopSprite, (x == 0) ? (2) : (5), 7, chessPiece.TeamColor.BLACK);
            createChessPiece("WhiteBishop" + x, bishop, whiteBishopSprite, (x == 0) ? (2) : (5), 0, chessPiece.TeamColor.WHITE);
        }
        //Make Queens
        createChessPiece("BlackQueen", queen, blackQueenSprite, 3, 7, chessPiece.TeamColor.BLACK);
        createChessPiece("WhiteQueen", queen, whiteQueenSprite, 3, 0, chessPiece.TeamColor.WHITE);
        //Make Kings
        createChessPiece("BlackKing", king, blackKingSprite, 4, 7, chessPiece.TeamColor.BLACK);
        createChessPiece("WhiteKing", king, whiteKingSprite, 4, 0, chessPiece.TeamColor.WHITE);

        //Say the right turn.
        setupNewGameUI();
    }

    private chessPiece createChessPiece(string objectName, chessPieceType type, Sprite sprite, int x, int y, chessPiece.TeamColor teamColor)
    {
        GameObject pieceObject = new GameObject(objectName);
        chessPiece piece = pieceObject.AddComponent<chessPiece>();
        piece.init(type, teamColor);
        SpriteRenderer spriteRenderer = pieceObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        pieceObject.AddComponent<CircleCollider2D>();
        addPiece(pieceObject, x, y);
        return piece;
    }


    public bool processCommand(boardCommand cmd)
    {
        if (cmd.execute(this))  //Only add to history if it did not fail.
        {
            _commandHistory.Add(cmd);
            return true;
        }
        return false;
    }

    public bool addPiece(GameObject piece, int x, int y)
    {
        componentPiece cPiece = piece.GetComponent<componentPiece>();
        if (cPiece == null)
        {
            Debug.Log("That GameObject you are trying to add to the piece list it's location is not a piece.");
            return false;
        }

        //Ensure piece does not already exist
        if(_pieces.Contains(cPiece))
        {
            Debug.Log("Piece " + piece.name + " is already in the piece list.");
            return false;
        }

        cmdAddPiece cmd = new cmdAddPiece(cPiece, x, y);
        return processCommand(cmd);
    }

    public bool movePiece(componentPiece piece, int x, int y)   //x and y here refer to their deltas
    {
        cmdMovePiece cmd = new cmdMovePiece(piece, x, y);
        return processCommand(cmd);
    }

    //NOTE: capture piece is specific to chess.
    public bool capturePiece(componentPiece captured, componentPiece capturer)
    {
        cmdCapturePiece cmd = new cmdCapturePiece((chessPiece)captured, (chessPiece)capturer);
        return processCommand(cmd);
    }

    public void addPieceToList(componentPiece piece)
    {
        _pieces.Add(piece);
        Debug.Log("Num Pieces : " + _pieces.Count);
        piece.gameObject.transform.SetParent(gameObject.transform, false);
    }

    public void removePieceFromList(componentPiece piece)
    {
        _pieces.Remove(piece);
    }

    public void addPieceToCapturedList(componentPiece piece)
    {
        _captured.Add(piece);
    }

    public int getNumCapturedChess(chessPiece.TeamColor color)
    {
        int count = 0;
        foreach(componentPiece capturedPiece in _captured)
        {
            chessPiece capturedChessPiece = (chessPiece)capturedPiece;
            if(capturedChessPiece.getTeamColor() == color)
            {
                count++;
            }
        }
        return count;
    }

    private void moveSelectedPieceToLocation(int x, int y)
    {
        Vector2 selectLocation = _selectedPiece.getLocation();
        movePiece(_selectedPiece, x - (int)selectLocation.x, y - (int)selectLocation.y);
    }

    public void movePieceToLocation(componentPiece piece, int x, int y, bool updatePiece = true)
    {
        Debug.Log("Moving piece " + piece.gameObject.name + " to (" + x + "," + y + ")");
        if (updatePiece)
        {
            piece.moveTo(x, y, this);
        }
        piece.gameObject.transform.localPosition = new Vector3(BOARD_OFFSET_X + (x * BOARD_TILE_WIDTH), BOARD_OFFSET_Y + (y * BOARD_TILE_HEIGHT), PIECE_Z);
    }

    internal void setPieceLocation(componentPiece piece, int x, int y)
    {
        piece.setLocation(x, y);
        piece.gameObject.transform.localPosition = new Vector3(BOARD_OFFSET_X + (x * BOARD_TILE_WIDTH), BOARD_OFFSET_Y + (y * BOARD_TILE_HEIGHT), PIECE_Z);
    }

    public List<componentPiece> getPiecesAtLocation(int x, int y)
    {
        List<componentPiece> foundPieces = new List<componentPiece>();

        foreach (componentPiece cPiece in _pieces)
        {
            if (cPiece.isAt(x,y))
            {
                foundPieces.Add(cPiece);
            }
        }

        return foundPieces;
    }

    public bool onBoard(int x, int y)
    {
        if(x < 0 || x >= BOARD_WIDTH || y < 0 || y >= BOARD_HEIGHT)
        {
            return false;
        }
        return true;
    }

    public bool selectPieceAtLocation(int x, int y) //Only selects the first found piece at the location
    {
        List<componentPiece> piecesAtLocation = getPiecesAtLocation(x, y);

        if (piecesAtLocation.Count > 0)
        {
            return selectPiece(piecesAtLocation[0]);
        }

        return false;
    }

    private bool selectPiece(componentPiece piece)
    {
        //Validate the piece can be selected
        if (_currentTurn == TURN_GAME_OVER)
        {
            return false;
        }
        if (_gameMode != chessGameMode.NONE) //If it's a chess game.
        {
            chessPiece cPiece = (chessPiece) piece;
            if((cPiece.getTeamColor() != _playerColors[_currentTurn]) && (_playerColors[_currentTurn] != chessPiece.TeamColor.ALL))
            {
                return false;
            }
        }

        //Deselect last selected piece if there was one
        deselectPiece();

        //Select the new piece
        _selectedPiece = piece;
        Vector2 location = _selectedPiece.getLocation();

        //Move the selected tile object
        GameObject selectedThing = GameObject.Find("Selected Tile");
        selectedThing.renderer.enabled = true;
        //Debug.Log("Location : " + location.x + "," + location.y);
        selectedThing.transform.localPosition = new Vector3(BOARD_OFFSET_X + ((int)location.x * BOARD_TILE_WIDTH), BOARD_OFFSET_Y + ((int)location.y * BOARD_TILE_HEIGHT), TILE_EFFECT_Z);

        //Generate the valid move tile effect objects
        foreach(Vector2 validMove in piece.getValidMoveList(this))
        {
            GameObject moveTileEffect = new GameObject("ValidMoveTile"+_validMoveTileEffects.Count);
            SpriteRenderer spriteRenderer = moveTileEffect.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = selectedThing.GetComponent<SpriteRenderer>().sprite;
            moveTileEffect.AddComponent<BoxCollider2D>();
            componentValidMoveTile compValidMoveTile = moveTileEffect.AddComponent<componentValidMoveTile>();
            compValidMoveTile.setLocation((int)validMove.x, (int)validMove.y);

            moveTileEffect.transform.SetParent(gameObject.transform, false);
            moveTileEffect.transform.localPosition = new Vector3(BOARD_OFFSET_X + ((int)validMove.x * BOARD_TILE_WIDTH), BOARD_OFFSET_Y + ((int)validMove.y * BOARD_TILE_HEIGHT), TILE_EFFECT_Z);

            _validMoveTileEffects.Add(moveTileEffect);
        }

        return true;
    }
    
    private void deselectPiece()
    {
        if(_selectedPiece == null)
        {
            return;
        }

        GameObject selectedThing = GameObject.Find("Selected Tile");
        selectedThing.renderer.enabled = false;

        foreach (GameObject validMoveTileEffect in _validMoveTileEffects)
        {
            Destroy(validMoveTileEffect);
        }

        _validMoveTileEffects.Clear();
        _selectedPiece = null;
    }

    public List<componentPiece> getActivePieces()
    {
        return _pieces;
    }

    public void clear()
    {
        deselectPiece();    //Clear movement tiles

        if (_captured != null)
        {
            foreach (componentPiece piece in _captured) //Clear captured pieces.
            {
                Destroy(piece.gameObject);
            }
            _captured.Clear();
        }

        if (_pieces != null)
        {
            foreach (componentPiece piece in _pieces)   //Clear active pieces.
            {
                Destroy(piece.gameObject);
            }
            _pieces.Clear();
        }

        if (_commandHistory != null)
        {
            _commandHistory.Clear();
        }

        _numTurns = 0;
    }

    public void changeTurn(int numTurns = 1)
    {
        _numTurns += numTurns;
        _currentTurn += numTurns;

        int turnInc = 0;
        while(_playerActive[_currentTurn % _playerColors.Length] == false)
        {
            turnInc++;
            _currentTurn++;

            if(turnInc >= _playerColors.Length)
            {
                //no active players left.
                gameOver(0);
                return;
            }
        }

        if(_currentTurn >= _playerColors.Length)    //Allowing for turn skipping.
        {
            _currentTurn = _currentTurn % _playerColors.Length;
        }


        switch(_gameMode)
        {
            case chessGameMode.TWO_PLAYER:
            case chessGameMode.TWO_PLAYER_SELF_ATTACK:
            case chessGameMode.TWO_AND_A_DIETY:
            case chessGameMode.ASYNCHRONOUS:
                _inCheck = false;

                //Check for kings
                Vector2[] kingLocation = new Vector2[_playerColors.Length];
                bool[] hasKing = new bool[_playerColors.Length];
                int totalKings = 0;
                int activePlayers = getNumActivePlayers();

                for (int i = 0; i < _playerColors.Length; i++)
                {
                    hasKing[i] = false;
                }

                foreach (componentPiece piece in _pieces)   //Make sure both sides still have a king.
                {
                    chessPiece cPiece = (chessPiece)piece;
                    if (cPiece.getPieceTypeName().Equals("King"))
                    {
                        for (int i = 0; i < _playerColors.Length; i++)
                        {
                            if (_playerColors[i] == cPiece.getTeamColor())
                            {
                                totalKings++;
                                kingLocation[i] = cPiece.getLocation();
                                hasKing[i] = true;
                                Debug.Log(_playerColors[i] + " has a king.");
                                break;
                            }
                        }
                        if (totalKings >= activePlayers)
                        {
                            Debug.Log("There is at least 1 king per player.");
                            break;
                        }
                    }
                }

                Debug.Log("Test1.");

                if(totalKings < activePlayers)
                {
                    Debug.Log("There are less kings than active players.");
                    for (int i = 0; i < _playerColors.Length; i++)
                    {
                        Debug.Log("Checking Player Color " + _playerColors[i]);
                        if (hasKing[i] == false)
                        {
                            activePlayers--;
                            if (_playerColors[i] != chessPiece.TeamColor.ALL)
                            {
                                _playerActive[i] = false;
                            }
                        }
                    }

                    turnInc = 0;
                    while (_playerActive[_currentTurn % _playerColors.Length] == false)
                    {
                        turnInc++;
                        _currentTurn++;

                        if (turnInc >= _playerColors.Length)
                        {
                            //no active players left.
                            gameOver(0);
                            return;
                        }
                    }

                    if(activePlayers == 1)
                    {
                        for (int i = 0; i < _playerColors.Length; i++)
                        {
                            if (kingLocation[i] != null)
                            {
                                gameOver(i);
                                return;
                            }
                        }
                    }
                }

                List<chessPiece> checkingKing = new List<chessPiece>();
                foreach (componentPiece piece in _pieces) //Check if current player is in check.
                {
                    chessPiece cPiece = (chessPiece)piece;
                    if(cPiece.getTeamColor() != _playerColors[_currentTurn])
                    {
                        if (cPiece.canAttack((int)kingLocation[_currentTurn].x, (int)kingLocation[_currentTurn].y, this))
                        {
                            _inCheck = true;
                            checkingKing.Add(cPiece);
                        }
                    }
                }

                if(_inCheck == true)
                {
                    Debug.Log(_playerColors[_currentTurn] + " is in Check!");
                    //You are in check. Is it checkmate?
                    //Note: The prototype of this check will take ridiculously long to make and run so i'll just put the pseudocode here.
                    //If the king can move, the king is not in checkmate
                    //If the king cannot move:
                        //Check if any player owned pieces can intercept the checking pieces.
                        //Theoretical method 1: simulate the move and recalculate all opposing pieces for each valid move... (lots of cpu time)
                        //Theoretical Method 2: add in to the move validitiy check to make sure you can't move your own unit to put yourself in check, then do method 1, but then you only need to check if it blocks the checking piece.
                }
                break;
        }

        updateTurnUI();
    }

    private int getNumActivePlayers()
    {
        int numActive = 0;
        for(int i = 0; i < _playerActive.Length; i++)
        {
            if(_playerActive[i] == true)
            {
                numActive++;
            }
        }
        return numActive;
    }

    private void gameOver(int winnerIdx)
    {
        Debug.Log("Game Over");
        Debug.Log("Winner : " + _playerColors[winnerIdx]);
        _currentTurn = TURN_GAME_OVER;

        _UI.victory("" + _playerColors[winnerIdx]);
    }

    private void setupNewGameUI()
    {
        _UI.setupNewGameUI(_gameMode);
        updateTurnUI();
    }

    private void updateTurnUI()
    {
        _UI.setTurnTxt("" + _playerColors[_currentTurn]);   //Hacky way to turn it into a string.
    }
}
