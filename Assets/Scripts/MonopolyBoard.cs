using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard Instance;
    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }

    public List<NodeSet> nodeSetList = new List<NodeSet>();

    void Awake()
    {
        Instance = this;
    }
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

    public void MovePlayerToken(MonopolyNodeType type, Player player)
    {
        int indexOfNextNodeType = -1; // INDEX TO FIND
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // WHERE THE PLAYER IS
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0;

        while (indexOfNextNodeType == -1 && nodeSearches < route.Count)
        {
            if (route[startSearchIndex].monopolyNodeType == type)
            {
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }
        if (indexOfNextNodeType == -1)
        {
            Debug.LogError("NO NODE FOUND");
            return;
        }
        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken; // FROM PLAYER CLASS
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // FROM PLAYER CLASS
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;

        if (isMovingForward)
        {
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
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // MOVE TOKEN
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null;
                }
                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft < 0)
            {
                indexOnBoard--;
                // IF WE REACH THE END OF THE BOARD
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count - 1;

                }

                // GET START AND END POSITIONS
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // MOVE TOKEN
                while (MoveToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null;
                }
                stepsLeft++;
            }
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

    public (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)
    {
        bool allSame = false;
        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSetList.Contains(node))
            {
                //LINQ
                allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodesInSetList, allSame);

            }
        }
        return (null, allSame);
    }
}
