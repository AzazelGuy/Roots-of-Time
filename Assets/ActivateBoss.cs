using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBoss : MonoBehaviour
{
    public DunkeosteusBoss DunkeosteusBoss;
    public void ativar()
        {
            DunkeosteusBoss.Active = true;
        }
}
