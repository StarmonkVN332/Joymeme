using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClick : MonoBehaviour
{
    [SerializeField] AudioSource audioClick;
    // Update is called once per frame
    void Update()
    {
        AudioClickCard();
    }

    public void AudioClickCard()
    {
        audioClick.Play();
    }
}
