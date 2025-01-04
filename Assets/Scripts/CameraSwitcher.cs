using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher instance;
    [SerializeField] CinemachineVirtualCamera topDownCamera;
    [SerializeField] CinemachineVirtualCamera diceCamera;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;

    void Awake()
    {
        instance = this;
    }

    public void SwitchToTopDown()
    {
        topDownCamera.Priority = 2;
        diceCamera.Priority = 0;
        playerFollowCamera.Priority = 0;
    }

    public void SwitchToDice()
    {
        topDownCamera.Priority = 0;
        diceCamera.Priority = 2;
        playerFollowCamera.Priority = 0;
    }

    public void SwitchToPlayer(Transform followTarget)
    {
        topDownCamera.Priority = 0;
        diceCamera.Priority = 0;
        playerFollowCamera.Priority = 2;
        playerFollowCamera.Follow = followTarget;
        playerFollowCamera.LookAt = followTarget;
    }
}
