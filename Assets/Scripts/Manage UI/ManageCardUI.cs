using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class ManageCardUI : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] GameObject[] buildings;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;
    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUI propertyReference;

    //Color setColor, int numberOfBuildings, bool isMortgaged, int mortgageValue
    public void SetCard(MonopolyNode node, Player owner, ManagePropertyUI propertySet)
    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;

        //SET COLOR
        if (node.propertyColorField != null)
        {
            colorField.color = node.propertyColorField.color;
        }
        else
        {
            colorField.color = Color.black;
        }

        //SHOW BUILDINGS
        ShowBuildings();
        //SHOW MORTGAGE IMAGE
        mortgageImage.SetActive(node.IsMortgaged);
        //TEXT UPDATE
        mortgageValueText.text = "Mortgage Value <br><b> G " + node.MortgageValue;
        //BUTTONS
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.interactable = node.IsMortgaged;
        // SET ICON
        switch (nodeReference.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                break;
        }
        // SET NAME OF PROPERTY
        propertyNameText.text = nodeReference.name;

    }

    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed())
        {
            // ERROR MESSAGE
            string message = "You have houses on one or more properties, you can't mortgage.";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (nodeReference.IsMortgaged)
        {
            //ERROR MESSAGE
            string message = "It's already mortgaged.";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());
        mortgageImage.SetActive(true);
        mortgageButton.interactable = false;
        unMortgageButton.interactable = true;
        ManageUI.instance.UpdateMoneyText();
    }
    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        {
            //ERROR MESSAGE
            string message = "It's not mortgaged.";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            //ERROR MESSAGE
            string message = "You don't have enough money";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MortgageValue);
        nodeReference.UnMortgageProperty();
        mortgageImage.SetActive(false);
        mortgageButton.interactable = true;
        unMortgageButton.interactable = false;
        ManageUI.instance.UpdateMoneyText();
    }

    public void ShowBuildings()
    {
        // HIDE ALL BUILDINGS FIRST
        foreach (var icon in buildings)
        {
            icon.SetActive(false);
        }

        //SHOW BUILDINGS
        if (nodeReference.NumberOfHouses < 5)
        {
            for (int i = 0; i < nodeReference.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
            }
        }
        else
        {
            buildings[4].SetActive(true);
        }
    }
}
