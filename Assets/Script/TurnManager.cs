using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public CharMove player; // Reference to Player character
    public AIController ai; // Reference to AI
    private bool isPlayerTurn = true; // Indicates Player's turn

    private void Start()
    {
        StartCoroutine(TurnLoop());
    }

    private IEnumerator TurnLoop()
    {
        while (true)
        {
            if (isPlayerTurn)
            {
                // Wait until Player finishes moving
                yield return StartCoroutine(PlayerTurn());
            }
            else
            {
                // Reset AI's roll flag at the start of its turn
                ai.ResetRollFlag();

                // Wait until AI finishes rolling and moving
                yield return StartCoroutine(AITurn());
            }

            // Switch turns
            isPlayerTurn = !isPlayerTurn;
        }
    }

    private IEnumerator PlayerTurn()
    {
        // Allow player to roll the dice and move
        yield return new WaitUntil(() => player.steps > 0);
        yield return new WaitUntil(() => !player.IsMoving());
    }

    private IEnumerator AITurn()
    {
        // AI rolls the dice
        ai.RollDiceAutomatically(); // AI rolls the dice

        // Wait until AI finishes moving
        yield return new WaitUntil(() => !ai.IsMoving()); // Wait for AI to finish moving
    }
}
