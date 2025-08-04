using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUiManager : MonoBehaviour
{
    [Header("Play Mutiplayer")]
    [SerializeField] private Button play;
    [SerializeField] private GameObject panelCreateAndJoin;

    [Header("More Options")]
    [SerializeField] private Button back;
    [SerializeField] private GameObject panelCreAndJoin;

    void Start()
    {
        play.onClick.AddListener(PlayButton);
        back.onClick.AddListener(BackBtn);
    }

    private void BackBtn()
    {
        panelCreAndJoin.SetActive(false);
    }
    private void PlayButton()
    {
        panelCreateAndJoin.SetActive(true);
    }
}
