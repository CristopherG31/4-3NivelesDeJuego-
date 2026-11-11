
using System.Collections.Generic;
using UnityEngine;

public class NivelGenerador : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefabMoneda;
    public GameObject prefabObstaculo;
    public GameObject prefabPlataforma;

    [Header("Modulos")]
    public float largoModulo = 12f;
    public float anchoModulo = 10f;

    [HideInInspector] public List<Vector3> checkpoints = new List<Vector3>();
    [HideInInspector] public int totalModulos;

    private enum Tipo
    {
        Recto,
        Obstaculos,
        Hueco,
        Plataformas
    }

    private Transform contenedor;

    public void Generar(int semilla, float d)
    {
        Random.InitState(semilla);
        checkpoints.Clear();

        if (contenedor != null)
            Destroy(contenedor.gameObject);

        contenedor = new GameObject("NivelGenerado").transform;

        d = Mathf.Clamp01(d);

        int intermedios = 4 + Mathf.RoundToInt(d * 8);
        float z = -largoModulo / 2f;

        Tipo anterior = Tipo.Recto;

        CrearModulo(Tipo.Recto, z, d, false);
        z += largoModulo;

        for (int i = 0; i < intermedios; i++)
        {
            Tipo tipo = ElegirTipo(d, anterior);

            CrearModulo(tipo, z, d, true);

            anterior = tipo;
            z += largoModulo;
        }

        CrearPiso(z, largoModulo);
        CrearMeta(z + largoModulo - 1.5f);

        totalModulos = intermedios + 2;
    }

    Tipo ElegirTipo(float d, Tipo anterior)
    {
        float pHueco = Mathf.Lerp(0.10f, 0.25f, d);
        float pObst = Mathf.Lerp(0.30f, 0.40f, d);
        float pPlataformas = Mathf.Lerp(0.20f, 0.45f, d);

        float r = Random.value;

        Tipo tipo;

        if (r < pHueco)
            tipo = Tipo.Hueco;
        else if (r < pHueco + pObst)
            tipo = Tipo.Obstaculos;
        else if (r < pHueco + pObst + pPlataformas)
            tipo = Tipo.Plataformas;
        else
            tipo = Tipo.Recto;

        if (tipo == Tipo.Hueco && anterior == Tipo.Hueco)
            tipo = Tipo.Recto;

        return tipo;
    }

    void CrearModulo(Tipo tipo, float z, float d, bool conContenido)
    {
        checkpoints.Add(new Vector3(0f, 1.5f, z + 0.8f));

        switch (tipo)
        {
            case Tipo.Recto:
                CrearPiso(z, largoModulo);

                if (conContenido)
                    CrearMonedas(z, 3);
                break;

            case Tipo.Obstaculos:
                CrearPiso(z, largoModulo);
                CrearObstaculos(z, d);
                break;

            case Tipo.Hueco:
            {
                float hueco = Mathf.Lerp(1.5f, 4f, d);
                float parte = (largoModulo - hueco) / 2f;

                CrearPiso(z, parte);
                CrearPiso(z + parte + hueco, parte);

                CrearMoneda(
                    new Vector3(0f, 2.2f, z + largoModulo / 2f)
                );
                break;
            }

            case Tipo.Plataformas:
                CrearPlataformas(z, d);
                break;
        }
    }

    void CrearPiso(float zInicio, float largo)
    {
        GameObject piso = GameObject.CreatePrimitive(PrimitiveType.Cube);

        piso.name = "Piso";
        piso.transform.SetParent(contenedor);
        piso.transform.position =
            new Vector3(0f, -0.5f, zInicio + largo / 2f);

        piso.transform.localScale =
            new Vector3(anchoModulo, 1f, largo);
    }

    void CrearMonedas(float z, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float zMoneda =
                z + largoModulo * (i + 1) / (cantidad + 1);

            CrearMoneda(
                new Vector3(Random.Range(-3f, 3f), 0.8f, zMoneda)
            );
        }
    }

    void CrearMoneda(Vector3 pos, Transform padre = null)
    {
        if (prefabMoneda == null)
            return;

        Transform destino = padre != null ? padre : contenedor;

        Instantiate(
            prefabMoneda,
            pos,
            Quaternion.identity,
            destino
        );
    }

    void CrearObstaculos(float z, float d)
    {
        if (prefabObstaculo == null)
            return;

        int n = 1 + Mathf.RoundToInt(d * 3);
        float vel = Mathf.Lerp(2f, 7f, d);

        for (int i = 0; i < n; i++)
        {
            float zObs =
                z + largoModulo * (i + 1) / (n + 1);

            bool avanzaEnZ =
                Random.value < 0.15f + d * 0.35f;

            Vector3 eje =
                avanzaEnZ ? Vector3.forward : Vector3.right;

            float x =
                avanzaEnZ ? Random.Range(-3f, 3f) : 0f;

            float recorrido =
                avanzaEnZ ? 1f : Random.Range(2.5f, 3.8f);

            GameObject obj = Instantiate(
                prefabObstaculo,
                new Vector3(x, 1f, zObs),
                Quaternion.identity,
                contenedor
            );

            Obstaculo movimiento = obj.GetComponent<Obstaculo>();

            if (movimiento != null)
                movimiento.Configurar(vel, eje, recorrido);

            CrearMoneda(
                new Vector3(0f, 0.8f, zObs - 1.2f)
            );
        }
    }

    void CrearPlataformas(float z, float d)
    {
        if (prefabPlataforma == null)
        {
            Debug.LogError("Falta asignar el Prefab de plataforma.");
            return;
        }

        int cantidad = 4;

        float separacion = largoModulo / cantidad;
        float probabilidadMovil = Mathf.Lerp(0.15f, 0.55f, d);
        float variacionX = Mathf.Lerp(0.4f, 1.2f, d);

        for (int i = 0; i < cantidad; i++)
        {
            float zPlataforma = z + separacion * (i + 0.5f);

            float x = (i == 0 || i == cantidad - 1)
                ? 0f
                : Random.Range(-variacionX, variacionX);

            Vector3 posicion = new Vector3(
                x,
                1f,
                zPlataforma
            );

            GameObject plataforma = Instantiate(
                prefabPlataforma,
                posicion,
                Quaternion.identity,
                contenedor
            );

            plataforma.name = "Plataforma_Generada_" + i;

            bool movil =
                i != 0 &&
                i != cantidad - 1 &&
                Random.value < probabilidadMovil;

            PlataformaMovil movimiento =
                plataforma.GetComponent<PlataformaMovil>();

            if (movimiento != null && movil)
            {
                float velocidad = Mathf.Lerp(1f, 2.5f, d);
                float recorrido = Mathf.Lerp(0.6f, 1.2f, d);

                movimiento.Configurar(
                    velocidad,
                    Vector3.right,
                    recorrido
                );
            }

            Renderer renderPlataforma =
                plataforma.GetComponentInChildren<Renderer>();

            if (renderPlataforma != null && Random.value < 0.75f)
            {
                float altura =
                    renderPlataforma.bounds.max.y + 0.8f;

                Vector3 posicionMoneda = new Vector3(
                    x,
                    altura,
                    zPlataforma
                );

                CrearMoneda(posicionMoneda, plataforma.transform);
            }
        }
    }

    void CrearMeta(float z)
    {
        GameObject meta = GameObject.CreatePrimitive(PrimitiveType.Cube);

        meta.name = "Meta";
        meta.transform.SetParent(contenedor);

        meta.transform.position = new Vector3(0f, 2f, z);

        meta.transform.localScale =
            new Vector3(anchoModulo, 4f, 0.5f);

        meta.GetComponent<Collider>().isTrigger = true;
        meta.GetComponent<Renderer>().material.color = Color.green;

        meta.AddComponent<Meta>();
    }
}
