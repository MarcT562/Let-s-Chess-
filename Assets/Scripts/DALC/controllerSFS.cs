using UnityEngine;
using System.Collections;
using Sfs2X;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;
using UnityEngine.UI;
using Sfs2X.Entities.Data;
using System.Collections.Generic;
using System;
using Sfs2X.Requests;

public class controllerSFS : MonoBehaviour {
    public const string CMD_MOVE = "move";
    public const string CMD_START_GAME = "startGame";
    public const string CMD_JOIN_GAME = "joinGame";

    private string MOVE_SUCCESS = "M00";
    private string JOIN_SUCCESS = "J00";
    private string CREATE_SUCCESS = "C00";

    public string host = "127.0.0.1";
    public int tcpPort = 9933;
    public int webSocketPort = 8888;
    public string zone = "Let's Chess";
    public string roomName = "General";

    public GameObject objAskGameName;
    public GameObject objGameName;
    public GameObject objAskUserName;
    public GameObject objUserName;

    private SmartFox _sfs;

    private InputField _inputGameName;  //Probably should end up in controller ui...
    private InputField _inputUserName;  //Probably should end up in controller ui...

    private controllerBoard _board;

    private bool _isCreating;
    private string _userName;
    private string _gameName;

    void Awake()
    {
        #if UNITY_WEBPLAYER
		    if (!Security.PrefetchSocketPolicy(Host, TcpPort, 500)) {
			    Debug.LogError("Security Exception. Policy file loading failed!");
		    }
        #endif
        _inputGameName = objGameName.GetComponent<InputField>();
        _inputUserName = objUserName.GetComponent<InputField>();

        _board = gameObject.GetComponent<controllerBoard>();
    }

	void Start ()
    {
        setAskGameNameActive(false);
        setAskUserNameActive(false);
        setWorkingAnimationActive(false);
	}
	
	void Update ()
    {
        if (_sfs != null)
            _sfs.ProcessEvents();
	}

    public void testPressed()
    {
    }

    public void createPressed()
    {
        _isCreating = true;

        connect();
    }

    public void joinPressed()
    {
        _isCreating = false;

        connect();
    }

    public void confirmUserName()
    {
        setAskUserNameActive(false);

        _userName = _inputUserName.text;
        Debug.Log("User Name : " + _userName);

        setWorkingAnimationActive(true);
        login(_userName);
    }

    public void confirmGameName()
    {
        setAskGameNameActive(false);
        end();  //This really needs a better name... because it's not like i'm ending the connection here...

        _gameName = _inputGameName.text;
        Debug.Log("Game Name : " + _gameName);
        if(_isCreating)
        {
            //Create the game
            Debug.Log("Creating Game");
            startNewGame();
        }
        else
        {
            //Join the game
            Debug.Log("Joining Game");
            JoinGame();
        }
    }

    private void startNewGame()
    {
        _board.setInputAllowed(false);
        setWorkingAnimationActive(true);

        SFSObject obj = new SFSObject();
        obj.PutUtfString("gameName", _gameName);

        _sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, onExtensionResponse);
        _sfs.Send(new ExtensionRequest(CMD_START_GAME, obj));
    }

    private void JoinGame()
    {
        _board.setInputAllowed(false);
        setWorkingAnimationActive(true);

        SFSObject obj = new SFSObject();
        obj.PutUtfString("gameName", _gameName);

        _sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, onExtensionResponse);
        _sfs.Send(new ExtensionRequest(CMD_JOIN_GAME, obj));
    }

    public void move(int x, int y, chessPiece piece)
    {
        Debug.Log("Asynchronous move request for piece " + piece.gameObject.name + " to location (" + x + "," + y + ")");

        _board.setInputAllowed(false);
        setWorkingAnimationActive(true);

        SFSObject obj = new SFSObject();
        obj.PutInt("x", x);
        obj.PutInt("y", y);
        obj.PutUtfString("piece", piece.gameObject.name);
        obj.PutUtfString("gameName", _gameName);

        _sfs.Send(new ExtensionRequest(CMD_MOVE, obj));
    }



    private void connect()
    {
        Debug.Log("Going to connect!");
        _board.setInputAllowed(false);

        //Set connection parameters
        ConfigData cfg = new ConfigData();
        cfg.Host = host;
        #if !UNITY_WEBGL
            cfg.Port = tcpPort;
        #else
		    cfg.Port = WSPort;
        #endif
        cfg.Zone = zone;

        //Initialize SFS2X client
        #if !UNITY_WEBGL
            _sfs = new SmartFox();
        #else
		    _sfs = new SmartFox(UseWebSocket.WS);
        #endif

        //Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        _sfs.ThreadSafeMode = true;

        //Add listeners then connect
        addConnectionListeners();
        setWorkingAnimationActive(true);
        _sfs.Connect(cfg);
    }

    private void login(string username)
    {
        _sfs.Send(new Sfs2X.Requests.LoginRequest(username));
    }

    private void joinTheRoom()
    {
        _sfs.AddEventListener(SFSEvent.ROOM_JOIN, onRoomJoin);
        _sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, onRoomJoinError);
        _sfs.Send(new Sfs2X.Requests.JoinRoomRequest(roomName));
    }

    private void setAskGameNameActive(bool enable)
    {
        objAskGameName.SetActive(enable);
    }

    private void setAskUserNameActive(bool enable)
    {
        objAskUserName.SetActive(enable);
    }

    private void setWorkingAnimationActive(bool enable)
    {
        if(enable)
        {
            Debug.Log("Working...");
        }
        if (!enable)
        {
            Debug.Log("Done Working!");
        }
    }
    

    private void addConnectionListeners()
    {
        _sfs.AddEventListener(SFSEvent.CONNECTION, onConnection);
        _sfs.AddEventListener(SFSEvent.CONNECTION_LOST, onConnectionLost);
    }

    private void resetListeners()
    {
        Debug.Log("Reset Listeners Called!");
        _sfs.RemoveAllEventListeners();
    }

    private void end()  //This could use a better name... but i'm not sure what yet.
    {
        Debug.Log("End Called!");
        resetListeners();
        setWorkingAnimationActive(false);
        _board.setInputAllowed(true);
    }



    private void onConnection(BaseEvent e)
    {
        setWorkingAnimationActive(false);
        if ((bool)e.Params["success"])
        {
            Debug.Log("Connection Successful!");
            resetListeners();
            _sfs.AddEventListener(SFSEvent.LOGIN, onLogin);
            _sfs.AddEventListener(SFSEvent.LOGIN_ERROR, onLoginError);

            setAskUserNameActive(true);
        }
        else
        {
            Debug.Log("Connection Failed!");
            end();
        }
    }

    private void onConnectionLost(BaseEvent e)
    {
        string reason = (string)e.Params["reason"];

        if (reason != ClientDisconnectionReason.MANUAL) //If you didn't do it.. what did?!?
        {
            Debug.Log("The connection was lost becasue  " + reason);
        }

        end();
    }

    private void onLogin(BaseEvent e)
    {
        resetListeners();
        joinTheRoom();
    }

    private void onLoginError(BaseEvent e)
    {
        _sfs.Disconnect();
        end();
        Debug.Log("Login failed: " + (string)e.Params["errorMessage"]);
    }

    private void onRoomJoin(BaseEvent e)
    {
        resetListeners();
        Debug.Log("Room Joined successfully!");
        setWorkingAnimationActive(false);
        setAskGameNameActive(true);
    }

    private void onRoomJoinError(BaseEvent e)
    {
        Debug.Log("Room join failed: " + (string)e.Params["errorMessage"]);
        end();
    }

    public void onExtensionResponse(BaseEvent e)
    {
        string cmd = (string)e.Params["cmd"];
        SFSObject data = (SFSObject)e.Params["params"];
        string response = data.GetUtfString("responseCode");

        switch (cmd)
        {
            case CMD_START_GAME:
            case CMD_JOIN_GAME:
                setWorkingAnimationActive(false);
                _board.setInputAllowed(true);

                if(!(response.Equals(CREATE_SUCCESS) || response.Equals(JOIN_SUCCESS)))
                {
                    Debug.Log("Create or Join Failed with Response Code : " + response);
                    return;
                }

                bool isGameOver = data.GetBool("isGameOver");   //Retrospect i should've just made this another response.
                int winner = data.GetInt("winner");

                //Get the game state
                int currentTurn = data.GetInt("currentTurn");
                chessPiece.TeamColor playerColor = (chessPiece.TeamColor)data.GetInt("playerColor");

                //Get the pieces
                int numPieces = data.GetInt("numPieces");
                string[] names = data.GetUtfStringArray("names");
                string[] types = data.GetUtfStringArray("types");
                int[] colors = data.GetIntArray("colors");
                bool[] alive = data.GetBoolArray("alive");
                int[] xList = data.GetIntArray("x");
                int[] yList = data.GetIntArray("y");

                List<pieceData> pieceSetup = new List<pieceData>();
                for (int i = 0; i < numPieces; i++)
                {
                    chessPiece.TeamColor color = (chessPiece.TeamColor)colors[i];
                    enumChessPieceType pieceType = (enumChessPieceType)Enum.Parse(typeof(enumChessPieceType), types[i], true);
                    pieceData piece = new pieceData(names[i], alive[i], color, pieceType, xList[i], yList[i]);
                    pieceSetup.Add(piece);
                }

                //Setup the board
                _board.initNewChessGame(chessGameMode.ASYNCHRONOUS, pieceSetup, playerColor, currentTurn, isGameOver, winner);
                break;

            case CMD_MOVE:
                setWorkingAnimationActive(false);
                _board.setInputAllowed(true);

                bool success = response.Equals(MOVE_SUCCESS);

                if(success)
                {
                    string pieceName = data.GetUtfString("pieceName");
                    int x = data.GetInt("x");
                    int y = data.GetInt("y");

                    _board.movePieceTo(pieceName, x, y);
                }
                else
                {
                    Debug.Log("Move failed with code " + response);
                }
                break;
        }
    }
}
