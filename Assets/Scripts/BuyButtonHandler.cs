using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyButtonHandler : MonoBehaviour
{
    public Button buyButton;
    public Button closeButton;
    public Text playerMoneyText;
    public Text priceText;

    private MonopolyNode currentNode;
    private Player currentPlayer;

    void Start()
    {
        // Pastikan tombol tidak aktif pada awalnya
        buyButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

        // Tambahkan listener untuk tombol
        buyButton.onClick.AddListener(OnBuyButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    public void ShowBuyPanel(MonopolyNode node, Player player)
    {
        currentNode = node;
        currentPlayer = player;

        // Update UI
        priceText.text = $"Price: G {currentNode.price}";
        playerMoneyText.text = $"You have: G {currentPlayer.ReadMoney}";

        // Tampilkan tombol
        buyButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    public void OnBuyButtonClick()
    {
        if (currentPlayer.CanAffordNode(currentNode.price))
        {
            currentPlayer.BuyProperty(currentNode);
            Debug.Log($"{currentPlayer.name} bought {currentNode.name} for {currentNode.price} G.");
        }
        else
        {
            Debug.Log($"{currentPlayer.name} does not have enough money to buy {currentNode.name}.");
        }

        HideBuyPanel();
    }

    public void OnCloseButtonClick()
    {
        HideBuyPanel();
    }

    private void HideBuyPanel()
    {
        buyButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }
}
