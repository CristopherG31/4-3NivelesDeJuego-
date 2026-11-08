using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 inicio;
    private Vector3 eje;
    private float velocidad;
    private float recorrido;
    private float fase;
    private bool configurado;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Configurar(float vel, Vector3 ejeMovimiento, float distancia)
    {
        inicio = transform.position;
        velocidad = vel;
        eje = ejeMovimiento;
        recorrido = Mathf.Max(distancia, 0.1f);   // nunca 0
        fase = Random.Range(0f, 10f);
        configurado = true;
    }

    void FixedUpdate()
    {
        if (!configurado) return;

        float t = Mathf.PingPong(Time.time * velocidad + fase, recorrido * 2f) - recorrido;
        rb.MovePosition(inicio + eje * t);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
            GameManager.Instancia.PerderVida();
    }
}