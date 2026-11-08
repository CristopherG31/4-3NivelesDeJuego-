using UnityEngine;

public class Moneda : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 120f * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instancia.RecogerMoneda();
            Destroy(gameObject);
        }
    }
}