using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder; // HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Button buyHouseButton, sellHouseButton;
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();
    [SerializeField] GameObject buttonBox;

    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        nodesInSet.AddRange(nodes);
        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);
            ManageCardUI manageCardUI = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);
            manageCardUI.SetCard(nodesInSet[i], owner, this);
        }
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(nodesInSet[0]);
        Debug.Log(allSame + " allsame");
        buyHouseButton.interactable = allSame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHousePriceText.text = "- G" + nodesInSet[0].houseCost;
        sellHousePriceText.text = "+ G" + nodesInSet[0].houseCost / 2;
        if (nodesInSet[0].monopolyNodeType != MonopolyNodeType.Property)
        {
            buttonBox.SetActive(false);
        }
    }

    public void BuyHouseButton()
    {
        if (!CheckIfBuyAllowed())
        {
            // ERROR MESSAGE
            string message = "One or more properties are mortgaged, you can't build a house.";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHouseOrHotelEvenly(nodesInSet);
            //UPDATE MONEY TEXT - IN MANAGE UI
            UpdateHouseVisuals();
            string message = "You have built a house.";
            ManageUI.instance.UpdateSystemMessage(message);
        }
        else
        {
            string message = "You don't have enough money to build a house.";
            ManageUI.instance.UpdateSystemMessage(message);
            //CANT AFFORD HOUSE - SYSTEM MESSAGE FOR PLAYER
        }
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    public void SellHouseButton()
    {
        //CHECK IF THERE AT LEAST 1 HOUSE TO SELL
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE MONEY TEXT - IN MANAGE UI
        UpdateHouseVisuals();
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    bool CheckIfSellAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return true;
        }
        return false;
    }

    bool CheckIfBuyAllowed()
    {
        if (nodesInSet.Any(n => n.IsMortgaged == true))
        {
            return false;
        }
        return true;
    }

    public bool CheckIfMortgageAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return false;
        }
        return true;
    }

    void UpdateHouseVisuals()
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUI>().ShowBuildings();
        }
    }
}
