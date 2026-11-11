
using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    private Vector3 posicionInicial;
    private Vector3 direccion;
    private float velocidad;
    private float distancia;
    private float desfase;
    private bool configurada = false;


    public void Configurar(float vel, Vector3 eje, float recorrido)
    {
        posicionInicial = transform.position;
        direccion = eje.normalized;
        velocidad = Mathf.Max(vel, 0f);
        distancia = Mathf.Max(recorrido, 0f);
        desfase = Random.Range(0f, 10f);

        configurada = true;
    }

    void Update()
    {
        if (!configurada) return;

        float movimiento = Mathf.PingPong(
            Time.time * velocidad + desfase,
            distancia * 2f
        ) - distancia;

        transform.position = posicionInicial + direccion * movimiento;
    }
}
