using UnityEngine;

public class Meta : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instancia.NivelCompleto();
    }
}