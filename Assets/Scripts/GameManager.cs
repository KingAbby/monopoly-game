using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;

    [Header("Global Game Settings")]
    [SerializeField] int maxTurnsInJail = 3; // 3 turns in jail
    [SerializeField] int startMoney = 2000;
    [SerializeField] int goMoney = 300;
    [SerializeField] float secondsBetweenTurns = 3;

    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // Panel to hold player info
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    [Header("Game Over/ Win Info")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text winnerNameText;

    [Header("Dice")]
    [SerializeField] Dice _dice1;
    [SerializeField] Dice _dice2;

    // ABOUT THE ROLLING DICE
    List<int> rolledDice = new List<int>();
    bool rolledADouble;
    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;
    int doubleRollCount;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;

    //TAX POOL
    int taxPool = 0;

    // PASS OVER GO TO GET THE MONEY
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];

    // MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // HUMAN INPUT PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    //DEBUG
    // [SerializeField] bool alwaysDoubleRoll = false;
    // [SerializeField] bool forceDiceRolls;
    // [SerializeField] int dice1;
    // [SerializeField] int dice2;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentPlayer = Random.Range(0, playerList.Count);
        gameOverPanel.SetActive(false);
        Initialize();
        CameraSwitcher.instance.SwitchToTopDown();
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            // RollDice();
            RollPhysicalDice();
        }
        else
        {
            // SHOW UI FOR HUMAN PLAYER
            OnShowHumanPanel.Invoke(true, true, false, false, false);
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

            // Instantiate player token
            GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);
            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
        playerList[currentPlayer].ActivateSelector(true);

        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
        else
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(false, false, false, jail1, jail2);
        }

    }

    public void RollPhysicalDice()
    {
        CheckForJailFree();
        rolledDice.Clear();
        _dice1.RollDice();
        _dice2.RollDice();
        CameraSwitcher.instance.SwitchToDice();

        // SHOW OR HIDE UI
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, false, false, jail1, jail2);
        }
    }

    void CheckForJailFree()
    {
        // JAIL FREE CARD
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceJailFreeCard)
            {
                playerList[currentPlayer].UseChanceJailFreeCard();
            }
            else if (playerList[currentPlayer].HasCommunityJailFreeCard)
            {
                playerList[currentPlayer].UseCommunityJailFreeCard();
            }
        }
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count == 2)
        {
            RollDice();
        }
    }

    // PRESS BUTTON FROM HUMAN - OR AUTO FROM AI
    void RollDice()
    {
        bool allowedToMove = true;
        hasRolledDice = true;

        // RESET LAST ROLL
        // rolledDice = new int[2];

        // // ANY ROLL DICE AND STORE THEM
        // rolledDice[0] = Random.Range(1, 7);
        // rolledDice[1] = Random.Range(1, 7);
        Debug.Log("Rolled dice are: " + rolledDice[0] + " & " + rolledDice[1]);

        //DEBUG
        // if (alwaysDoubleRoll)
        // {
        //     rolledDice[0] = 1;
        //     rolledDice[1] = 1;
        // }
        // if (forceDiceRolls)
        // {
        //     rolledDice[0] = dice1;
        //     rolledDice[1] = dice2;
        // }

        // CHECK FOR DOUBLE
        rolledADouble = rolledDice[0] == rolledDice[1];

        // THROW 3 TIMES IN A ROW -> GO TO JAIL -> END TURN

        // IS IN JAIL ALREADY
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();

            if (rolledADouble)
            {
                playerList[currentPlayer].GetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>got out of jail</color> by <b>rolling a double</b>!");
                doubleRollCount++;
                //MOVE PLATER
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                //LONG ENOUGH IN JAIL & ALLOWED TO LEAVE
                playerList[currentPlayer].GetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>got out of jail</color> after <b>3 turns</b>!");
            }
            else
            {
                allowedToMove = false;
            }
        }
        else //NOT IN JAIL
        {
            //RESET DOUBLE ROLL
            if (!rolledADouble)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)
                {
                    //GO TO JAIL
                    int indexOnBoard = MonopolyBoard.Instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=red>went to jail</color> for <b>rolling 3 doubles</b> in a row!");
                    rolledADouble = false;//RESET
                    return;
                }
            }
        }

        // CAN WE LEAVE JAIL
        // MOVE IF ALLOWED
        if (allowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " rolled a " + rolledDice[0] + " & " + rolledDice[1] + "!");
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            //SWITCH PLATER
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=red>can't move</color> - <b>switching player</b>!");
            StartCoroutine(DelayBetweenSwicthPlayer());
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayer(playerList[currentPlayer].MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns);
        // IF ALLOWED TO MOVE THEN MOVE
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        // ELSE SWITCH PLAYER
    }

    IEnumerator DelayBetweenSwicthPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer++;
        //RESET DICE HAS ROLLED
        hasRolledDice = false;

        // ROLL DOUBLE?
        doubleRollCount = 0;

        // OVERFLOW CHECK
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }
        DeactivateStaff();
        playerList[currentPlayer].ActivateSelector(true);
        // CHECK IF IN JAIL

        // IF AI PLAYER
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            // RollDice();
            RollPhysicalDice();

            OnShowHumanPanel.Invoke(false, false, false, false, false);
        }
        else // IF HUMAN - SHOW UI
        {
            bool jail1 = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
    }

    public List<int> LastRolledDice => rolledDice;

    public void AddTaxToPool(int amount)
    {
        taxPool += amount;
    }

    public int GetTaxPool()
    {
        //temp store tax pool
        int currentTaxCollected = taxPool;
        taxPool = 0; //reset tax pool
        //send temp
        return currentTaxCollected;
    }

    //----------------------------------GAMEOVER----------------------------------------------------------------
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        //CHECK FOR GAME OVER
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            //THE WINNER
            Debug.Log(playerList[0].name + " has WON THE GAME!");
            OnUpdateMessage.Invoke(playerList[0].name + " has WON THE GAME!");
            //STOP THE GAME LOOP

            //SHOW UI
            gameOverPanel.SetActive(true);
            winnerNameText.text = playerList[0].name;
        }
    }

    //--------------------------------UI COMMANDS----------------------------------------------------------------
    void DeactivateStaff()
    {
        foreach (var player in playerList)
        {
            player.ActivateSelector(false);
        }
    }

    //--------------------------------CONTINUE GAME STUFF----------------------------------------------------------------

    public void Continue()
    {
        if (playerList.Count > 1)
        {
            Invoke("ContinueGame", SecondsBetweenTurns);
        }
    }

    void ContinueGame()
    {
        // IF THE LAST ROLL WAS A DOUBLE
        if (RolledADouble)
        {
            // ROLL THE DICE AGAIN
            // RollDice();
            RollPhysicalDice();
        }
        else
        {
            // SWITCH PLAYER
            SwitchPlayer();
        }
    }

    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
    }

    public void UseJail1Card()
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }

    public void UseJail2Card()
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }

    bool isHumanPanelVisible = false;
    [Header("Toggle Human Panel")]
    [SerializeField] TMP_Text buttonText;

    public void ToggleHumanPanel()
    {
        isHumanPanelVisible = !isHumanPanelVisible;
        OnShowHumanPanel.Invoke(isHumanPanelVisible, isHumanPanelVisible, false, false, false);
        buttonText.text = isHumanPanelVisible ? "HIDE PANEL" : "SHOW PANEL";
        buttonText.color = isHumanPanelVisible ? Color.red : Color.green;
        // Debug.Log(buttonText); // Assuming you want to log the text, replace this with actual UI text update if needed
    }
}
