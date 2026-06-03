using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBoss : MonoBehaviour
{
    public DunkeosteusBoss DunkeosteusBoss;
    public AppearDIssapear appearDIssapear;
    public void ativar()
        {
            DunkeosteusBoss.Active = true;
            appearDIssapear.Appear();
        }
}
