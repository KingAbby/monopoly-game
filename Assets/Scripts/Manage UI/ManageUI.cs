using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel; //SHOW & HIDE
    [SerializeField] Transform propertyGrid;
    [SerializeField] GameObject propertySetPrefab;
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText;
    [SerializeField] TMP_Text systemMessageText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManager() //CALL FROM MANAGE BUTTON
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
        CreateProperties();

        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        ClearProperties();
    }

    void ClearProperties()
    {
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }

    void CreateProperties()
    {
        //GET ALL NODES AS NODE SETS
        List<MonopolyNode> processedSet = null;

        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);

            if (nodeSet != null && list != processedSet)
            {
                //UPDATE PROCESSED
                processedSet = list;

                nodeSet.RemoveAll(n => n.Owner != playerReference);
                //CREATE PREFAB WITH ALL NODES OWNED BY PLAYER
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyGrid, false);
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSet, playerReference);

                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? "<color=green>G " + playerReference.ReadMoney : "<color=red>G " + playerReference.ReadMoney;
        yourMoneyText.text = "<color=black>Your Money: </color>" + showMoney;
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }

    public void AutoHandleFunds()
    {
        if (playerReference.ReadMoney > 0)
        {
            UpdateSystemMessage("You don't need to do that. You have enough money!");
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));
        // UPDATE THE UI
        ClearProperties();
        CreateProperties();
        // UPDATE SYSTEM MESSAGE
        UpdateMoneyText();
    }
}
