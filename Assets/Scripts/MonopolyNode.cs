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
    [SerializeField] Image propertyColorField;
    [Header("Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;

    [Header("Property Price")]
    public int price;
    [SerializeField] TMP_Text priceText;

    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal int[] rentWithHouses;
    int numberOfHouses;
    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    public Player owner;

    public Player Owner => owner;

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
                    rentWithHouses = new int[]{
                        baseRent * 5,
                        baseRent * 5 * 3,
                        baseRent * 5 * 9,
                        baseRent * 5 * 16,
                        baseRent * 5 * 25,
                    };
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

        //Check Tipe Node dan lakukan sesuai tipe
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman)//AI player
                {
                    //IF IT OWNED AND WERE NOT THE OWNER AND IS NOT MORTGAGED
                    if (owner.name != "" && owner != currentPlayer && isMortgaged)
                    {
                        //PAY RENT

                        //CALCULATE RENT
                        int rentToPay = CalculatePropertyRent();

                        //PAY RENT TO OWNER

                        //SHOW MESSAGE
                    }
                    else if (owner.name == "" /*&& IF CAN AFFORD */)
                    {
                        //BUY NODE

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

                        //PAY RENT TO OWNER

                        //SHOW MESSAGE
                    }
                    else if (owner.name == "" /*&& IF CAN AFFORD */)
                    {
                        //SHOW BUY INTERFACE FOR PROPERTY
                    }
                    else
                    {
                        //UNOWNED NODE AND CANNOT AFFORD
                    }
                }
                break;
            case MonopolyNodeType.Utility:

                break;
            case MonopolyNodeType.Railroad:

                break;
            case MonopolyNodeType.Tax:

                break;
            case MonopolyNodeType.Chance:

                break;
            case MonopolyNodeType.CommunityChest:

                break;
            case MonopolyNodeType.FreeParking:

                break;
            case MonopolyNodeType.GoToJail:

                break;
        }

        //Lanjut
        if (!playerIsHuman)
        {
            Invoke("ContinueGame", 2f);
        }
        else
        {
            // SHOW UI FOR HUMAN PLAYER
        }
    }

    void ContinueGame()
    {
        // IF THE LAST ROLL WAS A DOUBLE

        // ROLL THE DICE AGAIN

        // NOT A DOUBLE ROLL - SWITCH PLAYER
        GameManager.instance.SwitchPlayer();


    }

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
}

