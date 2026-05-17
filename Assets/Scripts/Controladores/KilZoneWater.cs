using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KilZoneWater : MonoBehaviour
{
    [SerializeField] float KillTime = 5f;
    private float KillTimer = 5f;

    private bool PlayerInside = false;
    private void Update()
    {
        if (!PlayerInside)
        {
            KillTimer = KillTime;
        }
        else
        {
            KillTimer -= Time.deltaTime;
        }
        if (KillTimer <= 0)
        {
            GameManager.Instance.PlayerHealth -= 999999;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            PlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            PlayerInside = false;
        }
    }
}
