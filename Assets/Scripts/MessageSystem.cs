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
       GameManager.OnUpdateMessage += ReciveMessage;
       Player.OnUpdateMessage += ReciveMessage;
       MonopolyNode.OnUpdateMessage += ReciveMessage;
   }

    void OnDisable()
    {
        GameManager.OnUpdateMessage -= ReciveMessage;
        Player.OnUpdateMessage -= ReciveMessage;
        MonopolyNode.OnUpdateMessage -= ReciveMessage;
    }

    void ReciveMessage(string _message)
    {
        messageText.text = _message;
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
