using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;

    [Header("Global Game Settings")]
    [SerializeField] int maxTurnsInJail = 3; // 3 turns in jail
    [SerializeField] int startMoney = 2000;
    [SerializeField] int goMoney = 200;

    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // Panel to hold player info
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    // ABOUT THE ROLLING DICE
    int[] rolledDice;
    bool rolledADouble;
    int doubleRollCount;

    // PASS OVER GO TO GET THE MONEY
    public int GetGoMoney => goMoney;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Initialize();
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }
        else
        {
            // SHOW UI FOR HUMAN PLAYER
        }
    }

    void Initialize()
    {
        // CREATE PLAYERS
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            // RANDOM TOKEN
            int randomIndex = Random.Range(0, playerTokenList.Count);

            // Instatntiate player token
            GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);
            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
    }

    // PRESS BUTTON FROM HUMAN - OR AUTO FROM AI
    public void RollDice()
    {
        // RESET LAST ROLL
        rolledDice = new int[2];

        // ANY ROLL DICE AND STORE THEM
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);
        Debug.Log("Rolled dice are: " + rolledDice[0] + " & " + rolledDice[1]);

        // CHECK FOR DOUBLE
        rolledADouble = rolledDice[0] == rolledDice[1];

        // THROW 3 TIMES IN A ROW -> GO TO JAIL -> END TURN

        // IS IN JAIL ALREADY

        // CAN WE LEAVE JAIL

        // MOVE IF ALLOWED
        StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));

        // SHOW OR HIDE UI
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(2f);
        // IF ALLOWED TO MOVE THEN MOVE
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);

        // ELSE SWITCH PLAYER
    }

    public void SwitchPlayer()
    {
        currentPlayer++;
        // ROLL DOUBLE?

        // OVERFLOW CHECK
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }

        // CHECK IF IN JAIL

        // IF AI PLAYER
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }

        // IF HUMAN - SHOW UI
    }
}
