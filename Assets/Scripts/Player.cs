using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.ObjectModel;

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
    int numTurnsInJail = 0;
    [SerializeField] GameObject myToken; // CHARACTER TOKEN ON BOARD
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();

    // PLAYER INFO
    PlayerInfo myInfo;

    // AI
    int aiMoneySavity = 500;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentNode;
    public int ReadMoney => money;

    // MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

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
        if (playerType == PlayerType.AI)
        {
            // CHECK IF PLAYER CAN BUY PROPERTY
            CheckIfPlayerHasASet();

        }

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

    internal void PayMoney(int amount)
    {
        //NOT ENOUGH MONEY
        if (money < amount)
        {
            //HANDLE INSUFFICIENT FUNDS
        }
        money -= amount;
        //UPDATE UI
        myInfo.SetPlayerCash(money);
    }

    //---------------------------------------JAIL SECTION---------------------------------------------------

    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        //Reposition Player
        //myToken.transform.position = MonopolyBoard.Instance.route[10].transform.position;
        //currentNode = MonopolyBoard.Instance.route[10];
        MonopolyBoard.Instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }

    public void GetOutOfJail()
    {
        isInJail = false;
        //RESET TURN IN JAIL
        numTurnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;

        if (indexOnBoard > indexOfJail)
        {
            result = (indexOnBoard - indexOfJail) * -1;
        }
        else
        {
            result = (indexOfJail - indexOnBoard);
        }
        return result;
    }

    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseNumTurnsInJail()
    {
        numTurnsInJail++;
    }

    //-------------------------------STREET REPAIRS-------------------------------------------------
    public int[] CountHousesandHotels()
    {
        int houses = 0; //INDEX 0
        int hotels = 0; //INDEX 1

        foreach (var node in myMonopolyNodes)
        {
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    //-------------------------------CHECK IF PLAYER HAS PROPERTY SET-------------------------------
    void CheckIfPlayerHasASet()
    {
        List<MonopolyNode> processedSet = null;
        foreach (var node in myMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
            {
                continue;
            }
            List<MonopolyNode> nodeSet = list;
            if (nodeSet != null && nodeSet != processedSet)
            {
                bool hasMortgadedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMortgadedNode)
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)
                    {
                        //COULD BUILD A HOUSES ON THE SET
                        BuildHouseOrHotelEvenly(nodeSet);
                        //UPDATE PROCESS SET
                        processedSet = nodeSet;
                    }
                }
            }
        }
    }
    //-------------------------------BUILD HOUSES EVENLY ON NODE SETS-------------------------------------------------
    void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;
        //GET MIN AND MAX HOUSES CURRENTLY ON PROPERTY
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if (numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }
            if (numOfHouses > maxHouses && numOfHouses < 5)
            {
                maxHouses = numOfHouses;
            }
        }

        //BUY HOUSES ON PROPERTY
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);

                break;
            }
        }
    }
    //-------------------------------CAN AFFORD & COUNT HOUSES/HOTELS-------------------------------------------------
    bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)// AI
        {
            return (money - aiMoneySavity) >= price;
        }
        //HUMAN
        return money >= price;
    }
}
