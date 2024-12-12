using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Route currentRoute;
    public ThrowerDice throwerDice; // Reference to the ThrowerDice component
    private int routePosition;
    public int steps;
    private bool isMoving;
    private bool hasRolled; // Flag to check if AI has rolled this turn

    private void Start()
    {
        // Subscribe to the dice result event
        Dice.OnDiceResult += HandleDiceResult;
    }

    public void RollDiceAutomatically()
    {
        if (isMoving || hasRolled) return; // Prevent rolling if already moving or has rolled this turn

        // Call the RollDice method from ThrowerDice
        throwerDice.RollDice();
        hasRolled = true; // Set the flag to true after rolling
    }

    public void ResetRollFlag()
    {
        hasRolled = false; // Reset the roll flag for the new turn
    }

    private void HandleDiceResult(int diceIndex, int diceResult)
    {
        if (diceIndex == 0)
        {
            steps = diceResult;
        }
        else
        {
            steps += diceResult;   
        }
        Debug.Log("AI rolled: " + steps);
        if (!isMoving)
        {
            StartCoroutine(Move());
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public int GetSteps()
    {
        return steps; // Method to get the current steps
    }

    IEnumerator Move()
    {
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        while (steps > 0)
        {
            routePosition++;
            routePosition %= currentRoute.childNodeList.Count;

            Vector3 nextPos = currentRoute.childNodeList[routePosition].position;
            while (MoveToNextNode(nextPos)) { yield return null; }

            yield return new WaitForSeconds(0.1f);
            steps--;
        }
        isMoving = false;
    }

    bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 150f * Time.deltaTime));
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= HandleDiceResult; // Unsubscribe to prevent memory leaks
    }
}
