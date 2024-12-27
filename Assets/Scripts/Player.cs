using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }
    public PlayerType playerType;
    public string name;
    int money;
    MonopolyNode currentNode;
    bool isInJail;
    int numTurnsInJail;
    [SerializeField] GameObject myToken; // CHARACTER TOKEN ON BOARD
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();

    // PLAYER INFO
    PlayerInfo myInfo;

    // AI
    int aiMoneySavety = 500;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentNode;

    public void Initialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentNode = startNode;
        money = startMoney;
        myInfo = info;
        myInfo.SetPlayerNameAndCash(name, money);
        myToken = token;
    }

    public void SetMyCurrentNode(MonopolyNode newNode)
    {
        currentNode = newNode;
        // PLAYER LANDED ON NODE THEN DO SOMETHING
        newNode.PlayerLandedOnNode(this);

        // IF ITS AI PLAYER

        // CHECK IF CAN BUILD HOUSES

        // CHECK FOR UNMORTGAGE PROPERTIES

        // CHECK IF COULD TRADE FOR MISSING PROPERTIES
    }

    public void CollectMoney(int amount)
    {
        money += amount;
        myInfo.SetPlayerCash(money);
    }
}
