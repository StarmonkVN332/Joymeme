using Photon.Pun;
using UnityEngine;
using Cinemachine;

public class PlayerCameraHandler : MonoBehaviourPun
{
    public CinemachineVirtualCamera virtualCam;

    void Start()
    {
        if (!photonView.IsMine)
        {
            virtualCam.gameObject.SetActive(false);
        }
        else
        {
            virtualCam.gameObject.SetActive(true);
        }
    }
}
