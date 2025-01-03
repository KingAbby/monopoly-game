using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Xml.Serialization;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultMessageText;
    [Header("LEFT SIDE PANEL")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup; // TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoney;
    [SerializeField] Slider leftMoneySlider;
    List<GameObject> leftCardPrefabList = new List<GameObject>();

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
    Player rightPlayerReference;

    [Header("TRADE OFFER PANEL")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text leftMessageText, rightMessageText, leftPropertyNameText, rightPropertyNameText, leftMoneyText, rightMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] Image leftPropImage, rightPropImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    // STORE THE OFFER FOR HUMAN
    Player currentPlayer, nodeOwner;
    MonopolyNode requestedNode, offeredNode;
    int requestedMoney, offeredMoney;

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
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
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

        // CONTINUE IF NOTHING WAS FOUND
        if (requestedNode == null)
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
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

        bool foundDecision = false;

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
                    foundDecision = true;
                    break;
                }
            }
        }
        // NO VALID TRADE FOUND THEN EXIT
        if (!foundDecision)
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
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
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }

    //-------------------------------CONSIDER TRADE OFFER-------------------------------------------------AI
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode) + requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);

        //  SELL A NODE FOR MONEY ONLY
        if (requestedNode == null && offeredNode != null && requestedMoney <= nodeOwner.ReadMoney / 3 && !MonopolyBoard.Instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            // TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
            return;
        }

        // NORMAL TRADE
        if (valueOfTheTrade <= 0 && !MonopolyBoard.Instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            // TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
        }
        else
        {
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(false);
            }
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

        //HIDE UI FOR HUMAN
        CloseTradePanel();
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //-------------------------------USER INTERFACE CONTENT-------------------------------------------------HUMAN

    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);

        ClearAll();
    }

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "Select Player";
        CreateLeftPanel();
        CreateMiddleButtons();
    }

    //-------------------------------CURRENT PLAYER-------------------------------------------------HUMAN
    public void CreateLeftPanel()
    {
        leftOffererNameText.text = leftPlayerReference.name;

        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);
            // SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], leftToggleGroup);

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

    //-------------------------------SELECTED PLAYER-------------------------------------------------HUMAN

    public void ShowRightPlayer(Player player)
    {
        rightPlayerReference = player;
        // RESET THE CURRENT CONTENT
        ClearRightPanel();
        // SHOW RIGHT PLAYER OF ABOVE PLAYER
        rightOffererNameText.text = rightPlayerReference.name;

        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);
            // SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);

            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = "Your Money: G " + rightPlayerReference.ReadMoney;
        // SET UP MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        // UPDATE THE MONEY AND THE SLIDER
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoney.text = "Requested Money: G " + rightMoneySlider.value;
    }

    //-------------------------------MIDLLE CONTENT-------------------------------------------------HUMAN

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

    void ClearAll() // IF WE OPEN OR CLOSE THE PANEL
    {
        rightOffererNameText.text = "Select Player";
        rightYourMoneyText.text = "Your Money: G 0";
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        // CLEAR MIDDLE BUTTONS
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        // CLEAR LEFT CARD CONTENT
        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        // CLEAR RIGHT CARD CONTENT
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }

    void ClearRightPanel()
    {
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
        // RESET THE SLIDER
        // SET UP MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
    }

    //-------------------------------MAKE OFFER-------------------------------------------------HUMAN

    public void MakeOfferButton() // HUMAN INPUT BUTTON
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;

        if (rightPlayerReference == null)
        {
            // ERROR MESSAGE

            return;
        }

        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode, (int)leftMoneySlider.value, (int)rightMoneySlider.value);
    }

    //------------------------------------TRADE RESULT-----------------------------------------

    void TradeResult(bool accepted)
    {
        if (accepted)
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=blue> Accepted </color=blue></b>" + "The Trade Offer";
        }
        else
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=red> Rejected </color=red></b>" + "The Trade Offer";
        }

        resultPanel.SetActive(true);
    }

    //------------------------------------TRADE OFFER PANEL-----------------------------------------HUMAN

    void ShowTradeOfferPanel(Player _currentPlayer, Player _nodeOwner, MonopolyNode _requestedNode, MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        // FILL THE ACTUAL OFFER CONTENT
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        requestedNode = _requestedNode;
        offeredNode = _offeredNode;
        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;

        // SHOW PANEL CONTENT
        tradeOfferPanel.SetActive(true);
        leftMessageText.text = currentPlayer.name + " Offers: ";
        rightMessageText.text = "For " + nodeOwner.name + "'s: ";
        leftMoneyText.text = "+G " + offeredMoney;
        rightMoneyText.text = "+G " + requestedMoney;

        leftCard.SetActive(offeredNode != null ? true : false);
        rightCard.SetActive(requestedNode != null ? true : false);

        if (leftCard.activeInHierarchy)
        {
            leftColorField.color = (offeredNode.propertyColorField != null) ? offeredNode.propertyColorField.color : Color.black;
            leftPropertyNameText.text = offeredNode.name;
            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftPropImage.sprite = houseSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropImage.sprite = railroadSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    leftPropImage.sprite = utilitySprite;
                    leftPropImage.color = Color.white;
                    break;
            }
        }

        if (rightCard.activeInHierarchy)
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;
            rightPropertyNameText.text = requestedNode.name;
            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightPropImage.sprite = houseSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropImage.sprite = railroadSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    rightPropImage.sprite = utilitySprite;
                    rightPropImage.color = Color.white;
                    break;
            }
        }
    }

    public void AcceptOffer()
    {
        Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        ResetOffer();
    }

    public void RejectOffer()
    {
        currentPlayer.ChangeState(Player.AIStates.IDLE);
        ResetOffer();
    }

    void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null;
        requestedNode = null;
        offeredNode = null;
        requestedMoney = 0;
        offeredMoney = 0;
    }
}
