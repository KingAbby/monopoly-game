using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManage : MonoBehaviour
{
    [SerializeField] private TMP_Text dice1Text, dice2Text;

    private void OnEnable()
    {
        Dice.OnDiceResult += SetText;
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= SetText;
    }

    private void SetText(int diceIndex, int diceResult)
    {
        if (diceIndex == 0)
        {
            dice1Text.SetText($"Dice One Rolled a {diceResult}");
        }
        else
        {
            dice2Text.SetText($"Dice Two Rolled a {diceResult}");
        }
    }
}
