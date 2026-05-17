using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PorEnquantoeso : MonoBehaviour
{
    [SerializeField] AudioClip AudioClip;
    // Start is called before the first frame update
    void Start()
    {
        AudioController.Instance.PlayMusic(AudioClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
