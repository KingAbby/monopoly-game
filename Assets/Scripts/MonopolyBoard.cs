using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonopolyBoard : MonoBehaviour
{
    public List<MonopolyNode> route = new List<MonopolyNode>();

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }
    }

    void OnDrawGizmos()
    {
        if (route.Count > 1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i + 1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    public void MovePlayerToken(int steps, Player player)
    {
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken; // FROM PLAYER CLASS
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // FROM PLAYER CLASS
        bool moveOverGo = false;
        while (stepsLeft > 0)
        {
            indexOnBoard++;
            // IF WE REACH THE END OF THE BOARD
            if (indexOnBoard > route.Count - 1)
            {
                indexOnBoard = 0;
                moveOverGo = true;
            }

            // GET START AND END POSITIONS
            Vector3 startPos = tokenToMove.transform.position;
            Vector3 endPos = route[indexOnBoard].transform.position;

            // MOVE TOKEN
            while (MoveToNextNode(tokenToMove, endPos, 20))
            {
                yield return null;
            }
            stepsLeft--;
        }
        // GET GO MONEY
        if (moveOverGo)
        {
            // COLLECT GO MONEY ON THE PLAYER
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        // SET NEW NODE ON THE CURRENT PLAYER
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }
}
