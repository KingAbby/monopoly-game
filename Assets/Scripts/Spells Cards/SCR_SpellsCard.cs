using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spells Card", menuName = "Monopoly/Cards/Spells")]
public class SCR_SpellsCard : ScriptableObject
{
    public string textOnCard; //Description of the card
    public int rewardMoney; //GET reward money
    public int penalityMoney; //PAY money
    public int moveToBoardIndex = -1;
    public bool collectFromPlayer;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
