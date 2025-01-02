using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [Header("LEFT SIDE PANEL")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup; // TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoney;
    [SerializeField] Slider leftMoneySlider;
    List<GameObject> leftCardPrefabList = new List<GameObject>();
    int leftChosenMoneyAmount;
    MonopolyNode leftSelectedNode;
    Player leftPlayerReference;

    [Header("MIDDLE PANEL")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();

    [Header("RIGHT SIDE PANEL")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup; // TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoney;
    [SerializeField] Slider rightMoneySlider;
    List<GameObject> rightCardPrefabList = new List<GameObject>();
    int rightChosenMoneyAmount;
    MonopolyNode rightSelectedNode;
    Player rightPlayerReference;

    // MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        tradePanel.SetActive(false);
    }

    //-------------------------------FIND MISSING PROPERTIES IN SET-------------------------------------------------AI
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;
        MonopolyNode requestedNode = null;
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            // CHECK IF ALL HAVE BEEN PURCHASED
            bool notAllPurchased = list.Any(n => n.Owner == null);
            // AI OWNS THIS FULL SET ALREADY
            if (allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            // FIND THE OWNED BY OTHER PLAYER
            // CHECK IF THE PLAYER HAVE MORE THAN THE AVERAGE
            if (list.Count == 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if (requestedNode != null)
                {
                    // MAKE TRADE OFFER
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if (list.Count >= 3)
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    // MAKE TRADE OFFER
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
        }
    }

    //-------------------------------MAKE TRADE DECISION-------------------------------------------------AI
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        // TRADE WITH MONEY IF POSSIBLE
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
        {
            // TRADE WITH MONEY ONLY

            // MAKE TRADE OFFER
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode), 0);
            return;
        }

        // FIND ALL INCOMPLETE SET AND EXCLUDE THE SET WITH THE REQUESTED NODE
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var checkedSet = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node).list;
            if (checkedSet.Contains(requestedNode))
            {
                // STOP CHECKING HERE
                continue;
            }
            // VALID NODE CHECK
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= requestedNode.price)
                {
                    int difference = CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node);
                    // VALID TRADE POSSIBLE
                    if (difference >= 0)
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0);
                    }
                    else
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference));
                    }
                    // MAKE TRADE OFFER
                    break;
                }
            }
        }
        // FIND OUT IF ONLY ONE NODE OF THE FOUND SET IS OWNED

        // CALCULATE THE VALUE OF THAT NODE AND SEE IF WITH ENOUGH MONEY IT COULD BE AFFORDABLE

        // IF SO... MAKE TRADE OFFER

    }

    //-------------------------------MAKE TRADE OFFER-------------------------------------------------
    void MakeTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (nodeOwner.playerType == Player.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else if (nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            // SHOW UI FOR HUMAN
        }
    }

    //-------------------------------CONSIDER TRADE OFFER-------------------------------------------------AI
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode) + requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);
        // 300 - 600(-300)+0 - 300 = -600
        // (300 + req300) - (600 + 0) = 0
        // (600 + req0) - (300 + offer300)
        // WANT         // GIVE
        // 200 + 200    > 200 + 100

        //  SELL A NODE FOR MONEY ONLY
        if (requestedNode == null && offeredNode != null && requestedMoney <= nodeOwner.ReadMoney / 3)
        {
            // TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            return;
        }

        // NORMAL TRADE
        if (valueOfTheTrade <= 0)
        {
            // TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else
        {
            // DEBUG LINE OR TELL PLAYE THATS REJECTED
            Debug.Log("AI REJECTED TRADE OFFER");
        }
    }

    //-------------------------------CALCULATE THE VALUE OF NODE-------------------------------------------------AI
    int CalculateValueOfNode(MonopolyNode requestedNode)
    {
        int value = 0;
        if (requestedNode != null)
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)
            {
                value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;
            }
            else
            {
                value = requestedNode.price;
            }
            return value;
        }
        return value;
    }

    //-------------------------------TRADE THE NODE-------------------------------------------------
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        // CURRENT PLAYER NEEDS TO
        if (requestedNode != null)
        {
            currentPlayer.PayMoney(offeredMoney);
            requestedNode.ChangeOwner(currentPlayer);
            // NODE OWNER
            nodeOwner.CollectMoney(offeredMoney);
            nodeOwner.PayMoney(requestedMoney);

            if (offeredNode != null)
            {
                offeredNode.ChangeOwner(nodeOwner);
            }
            // SHOW A MESSAGE FOR THE UI
            string offeredNodeName = (offeredNode != null) ? " & " + offeredNode.name : "";
            OnUpdateMessage.Invoke(currentPlayer.name + " traded " + requestedNode.name + " for " + offeredMoney + offeredNodeName + " to " + nodeOwner.name);
        }
        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);
            // SHOW A MESSAGE FOR THE UI
            OnUpdateMessage.Invoke(currentPlayer.name + " sold " + offeredNode.name + " To " + nodeOwner.name + " for " + requestedMoney + " G ");
        }
    }

    //-------------------------------USER INTERFACE CONTENT-------------------------------------------------HUMAN

    //-------------------------------CURRENT PLAYER-------------------------------------------------HUMAN
    public void CreateLeftPanel()
    {
        leftOffererNameText.text = leftPlayerReference.name;

        for (int i = 0; i < leftPlayerReference.GetMonopolyNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);

            // SET UP THE ACTUAL CARD CONTENT
            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = "Your Money: G " + leftPlayerReference.ReadMoney;
        // SET UP MONEY SLIDER AND TEXT
        leftMoneySlider.maxValue = leftPlayerReference.ReadMoney;
        leftMoneySlider.value = 0;
        UpdateLeftSlider(leftMoneySlider.value);

        // RESET OLD CONTENT
        tradePanel.SetActive(true);
    }

    public void UpdateLeftSlider(float value)
    {
        leftOfferMoney.text = "Offer Money: G " + leftMoneySlider.value;
    }

    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
    }

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        CreateLeftPanel();
        CreateMiddleButtons();
    }

    //-------------------------------SELECTED PLAYER-------------------------------------------------HUMAN

    public void ShowRightPlayer(Player player)
    {
        // RESET THE CURRENT CONTENT

        // SHOW RIGHT PLAYER OF ABOVE PLAYER


        // UPDATE THE MONEY AND THE SLIDER
    }

    // SET UP MIDDLE
    void CreateMiddleButtons()
    {
        // CLEAR CONTENT
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        // LOOP THROUGH ALL PLAYER
        List<Player> allPlayers = new List<Player>();
        allPlayers.AddRange(GameManager.instance.GetPlayers);
        allPlayers.Remove(leftPlayerReference);

        // ADD THE BUTTONS FOR THEM
        foreach (var player in allPlayers)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, buttonGrid, false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);

            playerButtonList.Add(newPlayerButton);
        }
    }
}
