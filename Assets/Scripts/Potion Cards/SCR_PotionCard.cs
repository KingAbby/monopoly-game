using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Potion Card", menuName = "Monopoly/Cards/Potion")]
public class SCR_PotionCard : ScriptableObject
{
    public string textOnCard; //Description of the card
    public int rewardMoney; //GET reward money
    public int penalityMoney; //PAY money
    public int moveToBoardIndex = -1;
    public bool payToPlayer;
    [Header("MoveToLocations")]
    public bool nextRailRoad;
    public bool nextUtility;
    public int moveStepsBackwards;
    [Header("Jail Content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
