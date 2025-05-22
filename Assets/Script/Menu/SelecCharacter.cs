using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SelecCharacter : MonoBehaviour
{
    public void SelectCharacter(string characterName)
    {
        // Lưu characterName vào Custom Properties của người chơi
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["Character"] = characterName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log("Bạn đã chọn nhân vật: " + characterName);
    }
}
