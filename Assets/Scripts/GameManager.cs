using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    public enum Estado { Preparando, Jugando, NivelCompleto, GameOver }
    public Estado estado { get; private set; }

    [Header("Referencias")]
    public Transform jugador;
    public NivelGenerador generador;

    [Header("UI")]
    public TextMeshProUGUI textoNivel;
    public TextMeshProUGUI textoMonedas;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoPuntaje;
    public TextMeshProUGUI textoMensaje;

    [Header("Curva de dificultad")]
    public int nivelMaximo = 10;
    public AnimationCurve curva = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public int vidasIniciales = 3;

    private int nivel, puntaje, mejorPuntaje, semilla;
    private int monedas, vidas;
    private float tiempo;
    private float invulnerableHasta;
    private Vector3 checkpoint;

    void Awake()
    {
        Instancia = this;
        if (generador == null) generador = GetComponent<NivelGenerador>();
    }

    void Start()
    {
        // 1. Cargar progreso guardado
        nivel = SistemaGuardado.Nivel;
        puntaje = SistemaGuardado.Puntaje;
        mejorPuntaje = SistemaGuardado.MejorPuntaje;
        if (!SistemaGuardado.TieneSemilla)
            SistemaGuardado.Semilla = NuevaSemilla();
        semilla = SistemaGuardado.Semilla;

        // 2. Dificultad y generación del nivel
        float d = Dificultad();
        generador.Generar(semilla + nivel, d);
        checkpoint = generador.checkpoints[0];

        vidas = vidasIniciales;
        monedas = 0;
        tiempo = generador.totalModulos * Mathf.Lerp(7f, 4f, d) + 10f;

        // 3. Iniciar el bucle de juego
        ActualizarUI();
        StartCoroutine(Preparar());
    }

    IEnumerator Preparar()
    {
        estado = Estado.Preparando;
        textoMensaje.text = "Nivel " + nivel;
        yield return new WaitForSeconds(1.5f);
        textoMensaje.text = "";
        estado = Estado.Jugando;
    }

    void Update()
    {
        if (estado != Estado.Jugando) return;

        tiempo -= Time.deltaTime;
        if (tiempo <= 0f)
        {
            tiempo = 0f;
            GameOver("¡Se acabó el tiempo!");
            return;
        }

        ActualizarCheckpoint();

        if (jugador.position.y < -5f)   // se cayó al vacío
            PerderVida(true);

        ActualizarUI();
    }

    float Dificultad()
    {
        return curva.Evaluate((nivel - 1f) / (nivelMaximo - 1f));
    }

    void ActualizarCheckpoint()
    {
        for (int i = generador.checkpoints.Count - 1; i >= 0; i--)
        {
            if (jugador.position.z >= generador.checkpoints[i].z)
            {
                checkpoint = generador.checkpoints[i];
                break;
            }
        }
    }

    public void RecogerMoneda()
    {
        if (estado != Estado.Jugando) return;
        monedas++;
        ActualizarUI();
    }

    public void PerderVida(bool ignorarInvulnerabilidad = false)
    {
        if (estado != Estado.Jugando) return;
        if (!ignorarInvulnerabilidad && Time.time < invulnerableHasta) return;

        vidas--;
        invulnerableHasta = Time.time + 1.5f;

        if (vidas <= 0)
        {
            GameOver("¡Sin vidas!");
            return;
        }

        // Reaparecer en el último checkpoint
        Rigidbody rb = jugador.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.position = checkpoint;
        jugador.position = checkpoint;
        ActualizarUI();
    }

    public void NivelCompleto()
    {
        if (estado != Estado.Jugando) return;
        estado = Estado.NivelCompleto;

        puntaje += monedas * 10 + Mathf.CeilToInt(tiempo) * 5 + nivel * 50;
        if (puntaje > mejorPuntaje) mejorPuntaje = puntaje;
        nivel++;

        if (nivel > nivelMaximo)
        {
            textoMensaje.text = "¡Ganaste el juego!\nPuntaje: " + puntaje;
            SistemaGuardado.NivelMasAlto = nivelMaximo;
            nivel = 1;
            puntaje = 0;
            semilla = NuevaSemilla();
        }
        else
        {
            textoMensaje.text = "¡Nivel completado!\nPuntaje: " + puntaje;
        }

        SistemaGuardado.MonedasTotales += monedas;
        Guardar();
        Invoke(nameof(ReiniciarEscena), 2.5f);
    }

    void GameOver(string motivo)
    {
        estado = Estado.GameOver;

        int puntajeFinal = puntaje;
        if (puntajeFinal > mejorPuntaje) mejorPuntaje = puntajeFinal;

        // Reiniciar el flujo completo: nivel 1, puntaje 0, nueva semilla
        nivel = 1;
        puntaje = 0;
        semilla = NuevaSemilla();
        SistemaGuardado.Partidas++;
        Guardar();

        textoMensaje.text = motivo + "\nGAME OVER\nPuntaje final: " + puntajeFinal +
                            "\nReiniciando desde el nivel 1...";
        Invoke(nameof(ReiniciarEscena), 3f);
    }

    int NuevaSemilla()
    {
        return (int)(System.DateTime.Now.Ticks % 100000);
    }

    void Guardar()
    {
        SistemaGuardado.Nivel = nivel;
        SistemaGuardado.Puntaje = puntaje;
        SistemaGuardado.MejorPuntaje = mejorPuntaje;
        SistemaGuardado.Semilla = semilla;
        SistemaGuardado.NivelMasAlto = Mathf.Max(SistemaGuardado.NivelMasAlto, nivel);
        SistemaGuardado.Guardar();
    }

    void ReiniciarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ActualizarUI()
    {
        textoNivel.text = "Nivel: " + nivel;
        textoMonedas.text = "Monedas: " + monedas;
        textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempo);
        textoVidas.text = "Vidas: " + vidas;
        textoPuntaje.text = "Puntaje: " + puntaje + "\nRécord: " + mejorPuntaje +
                            "\nMejor nivel: " + SistemaGuardado.NivelMasAlto;
    }

    [ContextMenu("Borrar progreso guardado")]
    void BorrarProgreso()
    {
        SistemaGuardado.BorrarTodo();
        Debug.Log("Progreso borrado");
    }

    public void NuevaPartida()   // lo usa el botón de la UI
    {
        SistemaGuardado.BorrarTodo();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}