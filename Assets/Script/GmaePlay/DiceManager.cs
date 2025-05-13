using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DiceManager : MonoBehaviourPunCallbacks
{
    public int myRoll;
    public int opponentRoll;

    private bool hasRolled = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Bắt đầu sau 1–2s để đợi cả hai player vào phòng
            Invoke(nameof(StartDiceRoll), 2f);
        }
    }

    void StartDiceRoll()
    {
        photonView.RPC("RollDice", RpcTarget.All);
    }

    [PunRPC]
    void RollDice()
    {
        myRoll = Random.Range(1, 7); // 1 đến 6
        Debug.Log("Tôi tung được: " + myRoll);
        photonView.RPC("SendRollToOthers", RpcTarget.Others, myRoll);
        hasRolled = true;
    }

    [PunRPC]
    void SendRollToOthers(int rollFromOther)
    {
        opponentRoll = rollFromOther;
        Debug.Log("Đối thủ tung được: " + opponentRoll);

        if (hasRolled)
            DecideTurn();
    }

    void DecideTurn()
    {
        if (myRoll > opponentRoll)
        {
            Debug.Log("Bạn đi trước!");
            // Gọi hàm bắt đầu lượt người chơi này
        }
        else if (myRoll < opponentRoll)
        {
            Debug.Log("Đối thủ đi trước!");
            // Gọi hàm chờ lượt
        }
        else
        {
            Debug.Log("Hòa, tung lại...");
            hasRolled = false;
            opponentRoll = 0;
            myRoll = 0;
            Invoke(nameof(StartDiceRoll), 2f);
        }
    }
}
