using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Railroad UI")]
    [SerializeField] GameObject railroadUiPanel;
    [SerializeField] TMP_Text railroadNameText;
    // [SerializeField] Image colorField;
    [Space]

    [SerializeField] TMP_Text oneRailroadRentText;
    [SerializeField] TMP_Text twoRailroadRentText;
    [SerializeField] TMP_Text threeRailroadRentText;
    [SerializeField] TMP_Text fourRailroadRentText;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyRailroadButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadPanelUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadPanelUi;
    }

    void Start()
    {
        railroadUiPanel.SetActive(false);
    }

    void ShowBuyRailroadPanelUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        //TOP PANEL CONTENT
        railroadNameText.text = node.name;
        // colorField.color = node.propertyColorField.color;
        //CENTER OF CARD
        // result = baseRent * (int)Mathf.Pow(2, amount - 1);
        oneRailroadRentText.text = "G " + node.baseRent * (int)Mathf.Pow(2, 1 - 1);
        twoRailroadRentText.text = "G " + node.baseRent * (int)Mathf.Pow(2, 2 - 1);
        threeRailroadRentText.text = "G " + node.baseRent * (int)Mathf.Pow(2, 3 - 1);
        fourRailroadRentText.text = "G " + node.baseRent * (int)Mathf.Pow(2, 4 - 1);

        //COST OF BUILDING
        mortgagePriceText.text = "G " + node.MortgageValue;
        //BOTTOM PANEL
        propertyPriceText.text = "Price: G " + node.price;
        playerMoneyText.text = "You have: G " + currentPlayer.ReadMoney;
        //BUY PROPERTY BUTTON
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            buyRailroadButton.interactable = false;
        }
        //SHOW PANEL
        railroadUiPanel.SetActive(true);
    }

    public void BuyRailroadButton() //CALLED FROM BUY BUTTON
    {
        //TELL PLAYER TO PURCHASE THIS PROPERTY
        playerReference.BuyProperty(nodeReference);
        //CLOSE PROPERTY CARD

        //MAKE THE BUTTON NOT INTERACTABLE ANYMORE
        buyRailroadButton.interactable = false;
    }

    public void CloseRailroadButton() //CALLED FROM BUY BUTTON
    {
        //CLOSE PANEL
        railroadUiPanel.SetActive(false);

        //CLEAR NODE REFERENCE
        nodeReference = null;
        playerReference = null;
    }
}
