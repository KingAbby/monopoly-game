using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    internal bool CanAffordNode(int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.setOwner(this);
        //UPDATE UI
        myInfo.SetPlayerCash(money);
        //SET OWNERSHIP
        myMonopolyNodes.Add(node);
        //SORT NODES BY PRICE
        SortPropertiesByPrice();
    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        //NOT ENOUGH MONEY
        if (money < rentAmount)
        {
            //HANDLE INSUFFICIENT FUNDS
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        //UPDATE UI
        myInfo.SetPlayerCash(money);
    }
}
