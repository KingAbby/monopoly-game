using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonopolyBoard : MonoBehaviour
{
    [SerializeField] List<MonopolyNode> route = new List<MonopolyNode>();

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
}
