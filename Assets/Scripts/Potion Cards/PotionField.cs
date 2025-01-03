using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class PotionField : MonoBehaviour
{
    public static PotionField instance;
    [SerializeField] List<SCR_PotionCard> cards = new List<SCR_PotionCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3; //HIDE CARD AFTER 3 SEC
    [SerializeField] Button closeCardButton;

    List<SCR_PotionCard> cardPool = new List<SCR_PotionCard>();
    List<SCR_PotionCard> usedCardPool = new List<SCR_PotionCard>();

    SCR_PotionCard jailFreeCard;
    //CURRENT CARD & PLAYER
    SCR_PotionCard pickedCard;
    Player currentPlayer;

    // HUMAN INPUT PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    void OnEnable()
    {
        MonopolyNode.OnDrawPotionCard += Drawcard;
    }

    void OnDisable()
    {
        MonopolyNode.OnDrawPotionCard -= Drawcard;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cardHolderBackground.SetActive(false);
        //ADD CARD TO THE POOL
        cardPool.AddRange(cards);
        //SHUFFLE THE CARDS
        ShuffleCards();
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count);
            SCR_PotionCard tempCard = cardPool[index];
            cardPool[index] = cardPool[i];
            cardPool[i] = tempCard;
        }
    }

    void Drawcard(Player cardTaker)
    {
        //DRAW CARD
        pickedCard = cardPool[0];
        cardPool.RemoveAt(0);

        if(pickedCard.jailFreeCard)
        {
            jailFreeCard = pickedCard;
        }   
        else
        {
            usedCardPool.Add(pickedCard);
        }    
        


        if (cardPool.Count == 0)
        {
            //PUT BACK ALL CARDS
            cardPool.AddRange(usedCardPool);
            usedCardPool.Clear();
            ShuffleCards(); //RE-SHUFFLE ALL CARDS BACK

        }
        //WHO IS CURRENT PLAYER
        currentPlayer = cardTaker;

        //SHOW CARD
        cardHolderBackground.SetActive(true);

        //FILL IN THE TEXT
        cardText.text = pickedCard.textOnCard;

        //DEACTIVATE THE BUTTON IF AI PLAYER
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }

    public void ApplyCardEffect() //CLOSE BUTTON OF THE CARD
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penalityMoney != 0 && !pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penalityMoney); //HANDLE INSUFFICIENT MONEY
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            //STEPS TO GOAL

            int currentIndex = MonopolyBoard.Instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoards = MonopolyBoard.Instance.route.Count;
            int stepsToMove = 0;
            if (currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            {
                stepsToMove = lengthOfBoards - currentIndex + pickedCard.moveToBoardIndex;
            }

            //START THE MOVE
            MonopolyBoard.Instance.MovePlayerToken(stepsToMove, currentPlayer);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayers;

            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    //PREVENT BANKRUPT
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penalityMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesandHotels();
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = true;
            currentPlayer.GoToJail(MonopolyBoard.Instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard) //JAIL FREE CARD
        {
            currentPlayer.AddChanceJailFreeCard();
        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = Mathf.Abs(pickedCard.moveStepsBackwards);
            MonopolyBoard.Instance.MovePlayerToken(-steps, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextRailRoad)
        {
            MonopolyBoard.Instance.MovePlayerToken(MonopolyNodeType.Railroad, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextUtility)
        {
            MonopolyBoard.Instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
            isMoving = true;
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }
    void ContinueGame(bool isMoving)
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else //HUMAN INPUT
        {
            if (!isMoving)
            {
                bool jail1 = currentPlayer.HasChanceJailFreeCard;
                bool jail2 = currentPlayer.HasCommunityJailFreeCard;
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, jail1, jail2);
            }
        }
    }

    public void AddBackJailFreeCard()
    {
        usedCardPool.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
