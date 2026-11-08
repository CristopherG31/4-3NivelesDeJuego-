using System.Collections.Generic;
using UnityEngine;

public class NivelGenerador : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefabMoneda;
    public GameObject prefabObstaculo;

    [Header("Módulos")]
    public float largoModulo = 12f;
    public float anchoModulo = 10f;

    [HideInInspector] public List<Vector3> checkpoints = new List<Vector3>();
    [HideInInspector] public int totalModulos;

    private enum Tipo { Recto, Obstaculos, Hueco }
    private Transform contenedor;

    // d = dificultad de 0 a 1
    public void Generar(int semilla, float d)
    {
        Random.InitState(semilla);
        checkpoints.Clear();
        contenedor = new GameObject("NivelGenerado").transform;

        int intermedios = 4 + Mathf.RoundToInt(d * 8);   // 4 a 12 módulos
        float z = -largoModulo / 2f;
        Tipo anterior = Tipo.Recto;

        // Módulo de inicio (seguro y sin contenido)
        CrearModulo(Tipo.Recto, z, d, false);
        z += largoModulo;

        // Módulos intermedios aleatorios
        for (int i = 0; i < intermedios; i++)
        {
            Tipo tipo = ElegirTipo(d, anterior);
            CrearModulo(tipo, z, d, true);
            anterior = tipo;
            z += largoModulo;
        }

        // Módulo final con la meta
        CrearPiso(z, largoModulo);
        CrearMeta(z + largoModulo - 1.5f);

        totalModulos = intermedios + 2;
    }

    Tipo ElegirTipo(float d, Tipo anterior)
    {
        float pHueco = Mathf.Lerp(0.10f, 0.35f, d);
        float pObst = Mathf.Lerp(0.30f, 0.55f, d);
        float r = Random.value;

        Tipo t;
        if (r < pHueco) t = Tipo.Hueco;
        else if (r < pHueco + pObst) t = Tipo.Obstaculos;
        else t = Tipo.Recto;

        if (t == Tipo.Hueco && anterior == Tipo.Hueco) t = Tipo.Recto;
        return t;
    }

    void CrearModulo(Tipo tipo, float z, float d, bool conContenido)
    {
        checkpoints.Add(new Vector3(0f, 1.5f, z + 0.8f));

        switch (tipo)
        {
            case Tipo.Recto:
                CrearPiso(z, largoModulo);
                if (conContenido) CrearMonedas(z, 3);
                break;

            case Tipo.Obstaculos:
                CrearPiso(z, largoModulo);
                CrearObstaculos(z, d);
                CrearMonedas(z, 2);
                break;

            case Tipo.Hueco:
            {
                float hueco = Mathf.Lerp(1.5f, 4f, d);        // metros de vacío
                float parte = (largoModulo - hueco) / 2f;
                CrearPiso(z, parte);
                CrearPiso(z + parte + hueco, parte);
                CrearMoneda(new Vector3(0f, 2.2f, z + largoModulo / 2f)); // moneda en el aire
                break;
            }
        }
    }

    void CrearPiso(float zInicio, float largo)
    {
        GameObject piso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piso.name = "Piso";
        piso.transform.SetParent(contenedor);
        piso.transform.position = new Vector3(0f, -0.5f, zInicio + largo / 2f);
        piso.transform.localScale = new Vector3(anchoModulo, 1f, largo);
    }

    void CrearMonedas(float z, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float zMoneda = z + largoModulo * (i + 1) / (cantidad + 1);
            CrearMoneda(new Vector3(Random.Range(-3f, 3f), 0.8f, zMoneda));
        }
    }

    void CrearMoneda(Vector3 pos)
    {
        Instantiate(prefabMoneda, pos, Quaternion.identity, contenedor);
    }

    void CrearObstaculos(float z, float d)
    {
        int n = 1 + Mathf.RoundToInt(d * 3);          // 1 a 4 obstáculos
        float vel = Mathf.Lerp(2f, 7f, d);            // más rápidos

        for (int i = 0; i < n; i++)
        {
            float zObs = z + largoModulo * (i + 1) / (n + 1);
            bool avanzaEnZ = Random.value < 0.15f + d * 0.35f;

            Vector3 eje = avanzaEnZ ? Vector3.forward : Vector3.right;
            float x = avanzaEnZ ? Random.Range(-3f, 3f) : 0f;
            float recorrido = avanzaEnZ ? 1f : Random.Range(2.5f, 3.8f);

            GameObject obj = Instantiate(prefabObstaculo, new Vector3(x, 1f, zObs), Quaternion.identity, contenedor);
            obj.GetComponent<Obstaculo>().Configurar(vel, eje, recorrido);
        }
    }

    void CrearMeta(float z)
    {
        GameObject meta = GameObject.CreatePrimitive(PrimitiveType.Cube);
        meta.name = "Meta";
        meta.transform.SetParent(contenedor);
        meta.transform.position = new Vector3(0f, 2f, z);
        meta.transform.localScale = new Vector3(anchoModulo, 4f, 0.5f);
        meta.GetComponent<Collider>().isTrigger = true;
        meta.GetComponent<Renderer>().material.color = Color.green;
        meta.AddComponent<Meta>();
    }
}