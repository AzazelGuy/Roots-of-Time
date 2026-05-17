using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearDIssapear : MonoBehaviour
{
    public GameObject Hider;
    public void Appear()
    {
        Hider.SetActive(true);
    }

    public void DIssapear()
    {
        Hider.SetActive(false);
    }
}
