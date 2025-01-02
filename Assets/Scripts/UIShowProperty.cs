using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Property UI")]
    [SerializeField] GameObject propertyUiPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text rentPriceText; //WITHOUT HOUSE
    [SerializeField] TMP_Text oneHouseRentText;
    [SerializeField] TMP_Text twoHouseRentText;
    [SerializeField] TMP_Text threeHouseRentText;
    [SerializeField] TMP_Text fourHouseRentText;
    [SerializeField] TMP_Text hotelRentText;
    [Space]
    [SerializeField] TMP_Text housePriceText;
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyPropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUi;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUi;
    }

    void Start()
    {
        propertyUiPanel.SetActive(false);
    }

    void ShowBuyPropertyUi(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        //TOP PANEL CONTENT
        propertyNameText.text = node.name;
        colorField.color = node.propertyColorField.color;
        //CENTER OF CARD
        rentPriceText.text = "G " + node.baseRent;
        oneHouseRentText.text = "G " + node.rentWithHouses[0];
        twoHouseRentText.text = "G " + node.rentWithHouses[1];
        threeHouseRentText.text = "G " + node.rentWithHouses[2];
        fourHouseRentText.text = "G " + node.rentWithHouses[3];
        hotelRentText.text = "G " + node.rentWithHouses[4];
        //COST OF BUILDING
        housePriceText.text = "G " + node.houseCost;
        mortgagePriceText.text = "G " + node.MortgageValue;
        //BOTTOM PANEL
        propertyPriceText.text = "Price: G " + node.price;
        playerMoneyText.text = "You have: G " + currentPlayer.ReadMoney;
        //BUY PROPERTY BUTTON
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            buyPropertyButton.interactable = false;
        }
        //SHOW PANEL
        propertyUiPanel.SetActive(true);
    }

    public void BuyPropertyButton() //CALLED FROM BUY BUTTON
    {
        //TELL PLAYER TO PURCHASE THIS PROPERTY
        playerReference.BuyProperty(nodeReference);
        //CLOSE PROPERTY CARD

        //MAKE THE BUTTON NOT INTERACTABLE ANYMORE
        buyPropertyButton.interactable = false;
    }

    public void ClosePropertyButton() //CALLED FROM BUY BUTTON
    {
        //CLOSE PANEL
        propertyUiPanel.SetActive(false);

        //CLEAR NODE REFERENCE
        nodeReference = null;
        playerReference = null;
    }
}
