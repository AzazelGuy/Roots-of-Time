using UnityEngine;

public class AxisDebugger : MonoBehaviour
{
    void Update()
    {
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKey("joystick button " + i))
            {
                Debug.Log("Button " + i);
            }
        }
    }
}