using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder; // HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Button buyHouseButton, sellHouseButton;
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();


    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        nodesInSet.AddRange(nodes);
        for (int i = 0; i < nodes.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);
            ManageCardUI manageCardUI = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);
            manageCardUI.SetCard(nodesInSet[i], owner);
        }
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(nodesInSet[0]);
        Debug.Log(allSame + " allsame");
        buyHouseButton.interactable = allSame;
        sellHouseButton.interactable = allSame;

        buyHousePriceText.text = "- G" + nodesInSet[0].houseCost;
        sellHousePriceText.text = "+ G" + nodesInSet[0].houseCost;
    }

    public void BuyHouseButton()
    {
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHouseOrHotelEvenly(nodesInSet);
            //UPDATE MONEY TEXT - IN MANAGE UI
        }
        else
        {
            //CANT AFFORD HOUSE - SYSTEM MESSAGE FOR PLAYER
        }
    }

    public void SellHouseButton()
    {
        //CHECK IF THERE AT LEAST 1 HOUSE TO SELL
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE MONEY TEXT - IN MANAGE UI

    }
}
