using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class MessageSystem : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    void OnEnable()
    {
        ClearMessage();
        GameManager.OnUpdateMessage += RecieveMessage;
        Player.OnUpdateMessage += RecieveMessage;
        MonopolyNode.OnUpdateMessage += RecieveMessage;
        TradingSystem.OnUpdateMessage += RecieveMessage;
    }

    void OnDisable()
    {
        GameManager.OnUpdateMessage -= RecieveMessage;
        Player.OnUpdateMessage -= RecieveMessage;
        MonopolyNode.OnUpdateMessage -= RecieveMessage;
        TradingSystem.OnUpdateMessage -= RecieveMessage;
    }

    void RecieveMessage(string _message)
    {
        messageText.text = _message;
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
