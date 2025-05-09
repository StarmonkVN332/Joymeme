using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUiManager : MonoBehaviour
{
    [Header("Play Mutiplayer")]
    [SerializeField] private Button play;
    [SerializeField] private GameObject panelCreateAndJoin;

    void Start()
    {
        play.onClick.AddListener(PlayButton);
    }

    private void PlayButton()
    {
        panelCreateAndJoin.SetActive(true);
    }
}
