using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TradePlayerButton : MonoBehaviour
{
    Player playerReference;
    [SerializeField] TMP_Text playerNameText;

    public void SetPlayer(Player player)
    {
        playerReference = player;
        playerNameText.text = player.name;
    }

    public void SelectPlayer()
    {
        TradingSystem.instance.ShowRightPlayer(playerReference);
    }
}
