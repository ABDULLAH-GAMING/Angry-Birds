using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _idleCamera;
    [SerializeField] private CinemachineVirtualCamera _followCamera;

    private void Awake()
    {
        SwitchtoIdleCam();
    }

    public void SwitchtoIdleCam()
    {
        _idleCamera.enabled = true;
        _followCamera.enabled = false;
    }

    public void SwitchToFollowCam(Transform followTransform)
    {
        _followCamera.Follow= followTransform;

        _followCamera.enabled = true;
        _idleCamera.enabled = false;

    }

}
