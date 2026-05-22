using UnityEngine;

public class HideZone : MonoBehaviour
{
    bool playerInside = false;

    private void Update()
    {
        if (playerInside)
        {
            UIPlayer.Instance.Vignnet.color = Color.Lerp(UIPlayer.Instance.Vignnet.color, new Color(1, 1, 1, 1), 0.1f);

        }
        else
        {

            UIPlayer.Instance.Vignnet.color = Color.Lerp(UIPlayer.Instance.Vignnet.color, new Color(1, 1, 1, 0), 0.1f);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = true;
            playerInside = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = true;
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = false;
            playerInside = false;
        }
    }
}