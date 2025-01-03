using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    CommunityChest,
    Go,
    Jail,
    FreeParking,
    GoToJail
}

public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType monopolyNodeType;
    public Image propertyColorField;
    [Header("Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;

    [Header("Property Price")]
    public int price;
    public int houseCost;
    [SerializeField] TMP_Text priceText;

    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;
    [SerializeField] GameObject[] houses;
    [SerializeField] GameObject hotel;
    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    public Player owner;

    // MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;
    // DRAF SPELLS CARD
    public delegate void DrawSpellsCard(Player player);
    public static DrawSpellsCard OnDrawSpellsCard;
    // DRAG A CHANCE CARD
    public delegate void DrawPotionCard(Player player);
    public static DrawPotionCard OnDrawPotionCard;
    // HUMAN INPUT PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;
    //PROPERTY BUY PANEL
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;
    //RAILROAD BUY PANEL
    public delegate void ShowRailroadBuyPanel(MonopolyNode node, Player player);
    public static ShowRailroadBuyPanel OnShowRailroadBuyPanel;
    //UTILITY BUY PANEL
    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player player);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;

    public Player Owner => owner;
    public void setOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdated();
    }

    void OnValidate()
    {
        if (nameText != null)
        {
            nameText.text = name;
        }

        // CALCULATE RENT
        if (calculateRentAuto)
        {
            if (monopolyNodeType == MonopolyNodeType.Property)
            {
                if (baseRent > 0)
                {
                    price = 3 * (baseRent * 10);
                    // MORTGAGE VALUE
                    mortgageValue = price / 2;
                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5 * 3);
                    rentWithHouses.Add(baseRent * 5 * 9);
                    rentWithHouses.Add(baseRent * 5 * 16);
                    rentWithHouses.Add(baseRent * 5 * 25);
                }
                else if (baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }
            if (monopolyNodeType == MonopolyNodeType.Railroad)
            {
                mortgageValue = price / 2;
            }
        }

        // PROPERTY PRICE
        if (priceText != null)
        {
            priceText.text = "G " + price;
        }

        // UPDATE THE OWNER
        OnOwnerUpdated();
        UnMortgageProperty();
        // isMortgaged = false;
    }

    public void UpdateColorField(Color color)
    {
        if (propertyColorField != null)
        {
            propertyColorField.color = color;
        }
    }
    // PROPERTY, UTILITY, RAILROAD MORTGAGE
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(true);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(false);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(true);
        }
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;

    // UPDATE OWNER
    public void OnOwnerUpdated()
    {
        if (ownerBar != null)
        {
            if (owner.name != "")
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }

    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;

        //CHECK FOR NODE TYPE AND ACT ACCORDINGLY
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman)//AI player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculatePropertyRent();
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Pays Rent</color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAffordNode(price))
                    {
                        //BUY NODE
                        //Debug.Log(currentPlayer.name + " Might Buy Property");
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        // OnOwnerUpdated();

                        //SHOW MESSAGE
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                else//Human Player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculatePropertyRent();
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Pays Rent</color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "")
                    {
                        //SHOW BUY INTERFACE FOR PROPERTY
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman)//AI player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Pays Rent</color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAffordNode(price))
                    {
                        //BUY NODE
                        // Debug.Log(currentPlayer.name + " Might Buy Utility");
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //SHOW MESSAGE
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                else//Human Player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Pays Rent</color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "")
                    {
                        //SHOW BUY INTERFACE FOR UTILITY
                        OnShowUtilityBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                break;
            case MonopolyNodeType.Railroad:
                if (!playerIsHuman)//AI player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red> Pays Rent </color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "" && currentPlayer.CanAffordNode(price))
                    {
                        //BUY NODE
                        // Debug.Log(currentPlayer.name + " Might Buy Railroad Facility ");
                        OnUpdateMessage.Invoke(currentPlayer.name + " buys " + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //SHOW MESSAGE
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                else//Human Player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //PAY RENT TO OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        //SHOW MESSAGE
                        OnUpdateMessage.Invoke(currentPlayer.name + " <color=red> Pays Rent </color> of " + rentToPay + " G Coins to " + owner.name);
                    }
                    else if (owner.name == "")
                    {
                        //SHOW BUY INTERFACE FOR RAILROAD
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                break;
            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //SHOW MESSAGE
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Pays Tax</color> of " + price + " G Coins");
                break;
            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                //SHOW MESSAGE
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=green>Collects</color> " + tax + " G Coins from Free Parking");
                break;
            case MonopolyNodeType.GoToJail:
                int indexOnBoard = MonopolyBoard.Instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                continueTurn = false;
                //SHOW MESSAGE
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Goes to Jail!</color>");
                break;
            case MonopolyNodeType.Chance:
                OnDrawPotionCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
            case MonopolyNodeType.CommunityChest:
                OnDrawSpellsCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }
        //Stop If Needed
        if (!continueTurn)
        {
            return;
        }

        //CONTINUE GAME
        if (!playerIsHuman)
        {
            // Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
            currentPlayer.ChangeState(Player.AIStates.TRADING);
        }
        else
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            // SHOW UI FOR HUMAN PLAYER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn);
        }
    }

    // void ContinueGame()
    // {
    //     // IF THE LAST ROLL WAS A DOUBLE
    //     if (GameManager.instance.RolledADouble)
    //     {
    //         // ROLL THE DICE AGAIN
    //         GameManager.instance.RollDice();
    //     }
    //     else
    //     {
    //         // NOT A DOUBLE ROLL - SWITCH PLAYER
    //         GameManager.instance.SwitchPlayer();
    //     }
    // }

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                //Check if the owner has a Full Set of This Area(Nodes)
                var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);
                if (allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }
                break;
            case 1:
                currentRent = rentWithHouses[0];
                break;
            case 2:
                currentRent = rentWithHouses[1];
                break;
            case 3:
                currentRent = rentWithHouses[2];
                break;
            case 4:
                currentRent = rentWithHouses[3];
                break;
            case 5://HOTEL
                currentRent = rentWithHouses[4];
                break;
        }

        return currentRent;
    }

    int CalculateUtilityRent()
    {
        int[] lastRolledDice = GameManager.instance.LastRolledDice;

        int result = 0;
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);
        if (allSame)
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;
        }

        return result;
    }

    int CalculateRailroadRent()
    {
        int result = 0;
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);

        int amount = 0;
        foreach (var item in list)
        {
            amount += (item.owner == this.owner) ? 1 : 0;
        }
        result = baseRent * (int)Mathf.Pow(2, amount - 1);

        return result;
    }

    void VisualizeHouses()
    {
        switch (numberOfHouses)
        {
            case 0:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1:
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(true);
                break;
        }
    }

    public void BuildHouseOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses++;
            VisualizeHouses();
        }
    }

    public int SellHouseOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses > 0)
        {
            numberOfHouses--;
            VisualizeHouses();
            return houseCost / 2; //ANY NUMBER
        }
        return 0;
    }

    public void ResetNode()
    {
        if (isMortgaged)
        {
            propertyImage.SetActive(true);
            mortgageImage.SetActive(false);
            isMortgaged = false;
        }
        // RESET HOUSES AND HOTEL
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }
        // RESET OWNER

        // REMOVE PROPERTY FROM OWNER
        owner.RemoveProperty(this);
        owner.name = "";
        owner.ActivateSelector(false);
        owner = null;
        // UPDATE UI
        OnOwnerUpdated();
    }

    //-------------------------------TRADING SYSTEM-------------------------------------------------

    //-------------------------------CHANGE NODE OWNER-------------------------------------------------
    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        setOwner(newOwner);
    }
}

