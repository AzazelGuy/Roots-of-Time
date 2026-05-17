using UnityEngine;

public class HideZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = true;
            UIPlayer.Instance.Vignnet.color = Color.Lerp(UIPlayer.Instance.Vignnet.color, new Color(1, 1, 1, 1), 0.1f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerMain"))
        {
            other.GetComponentInParent<PlayerMovement>().isHidden = false;
            UIPlayer.Instance.Vignnet.color = Color.Lerp(UIPlayer.Instance.Vignnet.color, new Color(1, 1, 1, 0), 0.1f);
        }
    }
}