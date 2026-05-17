using UnityEngine;

public class WaveY : MonoBehaviour
{
    public float magnitude = 0.5f; // The altura do Wave
    public float frequency = 1f; // Velocidade do wave
    public float rotationSpeed = 1f; //Velocidade de rotação

    private Vector3 initialPosition; //Posição Inicial

    void Start()
    {
        // Posição Inicial
        initialPosition = transform.position;
    }

    void Update()
    {
        // Calcula o novo Y no Wave (eu não vou explicar essa conta)
        float yOffset = Mathf.Sin(Time.time * frequency) * magnitude;

        // Mudar posição
        transform.position = initialPosition + new Vector3(0, yOffset, 0);

        //Rodar o Objeto
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
