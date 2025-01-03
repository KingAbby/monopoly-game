using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class TradePropertyCard : MonoBehaviour
{
    MonopolyNode nodeReference;

    [SerializeField] Image colorField;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image typeImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] Toggle toggleButton;

    public void SetTradeCard(MonopolyNode node, ToggleGroup toggleGroup)
    {
        nodeReference = node;
        colorField.color = (node.propertyColorField != null) ? node.propertyColorField.color : Color.black;
        propertyNameText.text = node.name;
        switch(node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                typeImage.sprite = houseSprite;
                typeImage.color = Color.blue;
                break;
            case MonopolyNodeType.Railroad:
                typeImage.sprite = railroadSprite;
                typeImage.color = Color.green;
                break;
            case MonopolyNodeType.Utility:
                typeImage.sprite = utilitySprite;
                typeImage.color = Color.black;
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
        propertyPriceText.text = "G " + node.price;
        toggleButton.isOn = false;
        toggleButton.group = toggleGroup;
    }

    public MonopolyNode Node()
    {
        return nodeReference;
    }
}
