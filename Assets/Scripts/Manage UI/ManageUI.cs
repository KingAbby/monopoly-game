using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageUI : MonoBehaviour
{
    [SerializeField] GameObject managePanel; //SHOW & HIDE
    [SerializeField] Transform propertyGrid;
    [SerializeField] GameObject propertySetPrefab;
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();

    void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManager() //CALL FROM MANAGE BUTTON
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
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
        managePanel.SetActive(true);
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }
}
