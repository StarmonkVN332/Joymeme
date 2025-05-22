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
    [SerializeField] private Button selecharacter;
    [SerializeField] private GameObject panelSelecharacter;

    void Start()
    {
        play.onClick.AddListener(PlayButton);
        selecharacter.onClick.AddListener(selecharacterBtn);
        back.onClick.AddListener(BackBtn);
    }

    private void BackBtn()
    {
        panelSelecharacter.SetActive(false);
    }
    private void PlayButton()
    {
        panelCreateAndJoin.SetActive(true);
    }
    private void selecharacterBtn()
    {
        panelSelecharacter.SetActive(true);
    }
}
