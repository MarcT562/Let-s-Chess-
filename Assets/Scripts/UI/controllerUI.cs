using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class controllerUI : MonoBehaviour {
    public GameObject objCurrentTurn;
    public GameObject objVictory;
    public GameObject objGameMode;
    public GameObject objCanAttackSelf;

    private Text _txtCurrentTurn;
    private Text _txtVictory;
    private Text _txtGameMode;
    private Toggle _cbCanAttackSelf;

    private controllerBoard _board;

	// Use this for initialization
	void Start () {
        showVictory(false);

        _txtCurrentTurn = objCurrentTurn.GetComponent<Text>();
        _txtVictory = objVictory.GetComponent<Text>();
        _txtGameMode = objGameMode.GetComponent<Text>();

        _cbCanAttackSelf = objCanAttackSelf.GetComponent<Toggle>();

        _board = gameObject.GetComponent<controllerBoard>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void setupNewGameUI(chessGameMode mode)
    {
        setGameMode("" + mode);
        showVictory(false);
    }

    public void setTurnTxt(string playerName)
    {
        _txtCurrentTurn.text = "Current Turn : " + playerName;
    }

    public void victory(string playerName)
    {
        showVictory(true);
        _txtVictory.text = playerName + " Won!";
    }

    private void showVictory(bool visible)
    {
        objVictory.SetActive(visible);
    }

    private void setGameMode(string modeName)
    {
        _txtGameMode.text = modeName;
    }

    public void newSoloGame()
    {
        if(_cbCanAttackSelf.isOn == true)
        {
            newGame(chessGameMode.SOLO_SELF_ATTACK);
        }
        else
        {
            newGame(chessGameMode.SOLO);
        }
    }

    public void newTwoPlayerGame()
    {
        if (_cbCanAttackSelf.isOn == true)
        {
            newGame(chessGameMode.TWO_PLAYER_SELF_ATTACK);
        }
        else
        {
            newGame(chessGameMode.TWO_PLAYER);
        }
    }

    public void newTwoAndADietyGame()
    {
        newGame(chessGameMode.TWO_AND_A_DIETY);
    }

    private void newGame(chessGameMode mode)
    {
        _board.initNewChessGame(mode);
    }
}
