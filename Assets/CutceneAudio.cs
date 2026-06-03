using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CutceneAudio : MonoBehaviour
{
    public VideoPlayer video;
    // Update is called once per frame
    void Update()
    {
        video.SetDirectAudioVolume(0,AudioController.Instance.SFXVolume); 
    }
}
