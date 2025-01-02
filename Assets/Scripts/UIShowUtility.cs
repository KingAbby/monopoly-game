using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject utilityUiPanel;
    [SerializeField] TMP_Text utilityNameText;
    // [SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityPanel;
    }

    void Start()
    {
        utilityUiPanel.SetActive(false);
    }

    void ShowBuyUtilityPanel(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        //TOP PANEL CONTENT
        utilityNameText.text = node.name;
        // colorField.color = node.propertyColorField.color;

        //COST OF BUILDING
        mortgagePriceText.text = "G " + node.MortgageValue;
        //BOTTOM PANEL
        utilityPriceText.text = "Price: G " + node.price;
        playerMoneyText.text = "You have: G " + currentPlayer.ReadMoney;
        //BUY PROPERTY BUTTON
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            buyUtilityButton.interactable = false;
        }
        //SHOW PANEL
        utilityUiPanel.SetActive(true);
    }

    public void BuyUtilityButton() //CALLED FROM BUY BUTTON
    {
        //TELL PLAYER TO PURCHASE THIS PROPERTY
        playerReference.BuyProperty(nodeReference);
        //CLOSE PROPERTY CARD

        //MAKE THE BUTTON NOT INTERACTABLE ANYMORE
        buyUtilityButton.interactable = false;
    }

    public void CloseUtilityButton() //CALLED FROM BUY BUTTON
    {
        //CLOSE PANEL
        utilityUiPanel.SetActive(false);

        //CLEAR NODE REFERENCE
        nodeReference = null;
        playerReference = null;
    }
}
