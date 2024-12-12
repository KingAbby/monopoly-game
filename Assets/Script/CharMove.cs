using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMove : MonoBehaviour
{
    public Route currentRoute;

    private int routePosition;
<<<<<<< Updated upstream
    private int steps;
=======
    public int steps;
>>>>>>> Stashed changes
    private bool isMoving;

    private void OnEnable()
    {
        Dice.OnDiceResult += HandleDiceResult;
    }

    private void OnDisable()
    {
        Dice.OnDiceResult -= HandleDiceResult;
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
<<<<<<< Updated upstream
        Debug.Log("Dice rolled: " + steps);
=======
        Debug.Log("Player rolled: " + steps);
>>>>>>> Stashed changes
        if (!isMoving)
        {
            StartCoroutine(Move());
        }
    }

<<<<<<< Updated upstream
=======
    public bool IsMoving()
    {
        return isMoving;
    }

>>>>>>> Stashed changes
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
}
