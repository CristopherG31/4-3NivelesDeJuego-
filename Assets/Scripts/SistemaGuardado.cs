using UnityEngine;

public static class SistemaGuardado
{
    const string K_NIVEL = "Nivel";
    const string K_PUNTAJE = "Puntaje";
    const string K_MEJOR = "MejorPuntaje";
    const string K_SEMILLA = "Semilla";
    const string K_NIVEL_ALTO = "NivelMasAlto";
    const string K_MONEDAS = "MonedasTotales";
    const string K_PARTIDAS = "Partidas";

    // Progreso de la partida actual
    public static int Nivel
    {
        get { return PlayerPrefs.GetInt(K_NIVEL, 1); }
        set { PlayerPrefs.SetInt(K_NIVEL, value); }
    }

    public static int Puntaje
    {
        get { return PlayerPrefs.GetInt(K_PUNTAJE, 0); }
        set { PlayerPrefs.SetInt(K_PUNTAJE, value); }
    }

    public static int Semilla
    {
        get { return PlayerPrefs.GetInt(K_SEMILLA, 0); }
        set { PlayerPrefs.SetInt(K_SEMILLA, value); }
    }

    public static bool TieneSemilla
    {
        get { return PlayerPrefs.HasKey(K_SEMILLA); }
    }

    // Estadísticas permanentes (no se reinician al perder)
    public static int MejorPuntaje
    {
        get { return PlayerPrefs.GetInt(K_MEJOR, 0); }
        set { PlayerPrefs.SetInt(K_MEJOR, value); }
    }

    public static int NivelMasAlto
    {
        get { return PlayerPrefs.GetInt(K_NIVEL_ALTO, 1); }
        set { PlayerPrefs.SetInt(K_NIVEL_ALTO, value); }
    }

    public static int MonedasTotales
    {
        get { return PlayerPrefs.GetInt(K_MONEDAS, 0); }
        set { PlayerPrefs.SetInt(K_MONEDAS, value); }
    }

    public static int Partidas
    {
        get { return PlayerPrefs.GetInt(K_PARTIDAS, 0); }
        set { PlayerPrefs.SetInt(K_PARTIDAS, value); }
    }

    public static void Guardar()
    {
        PlayerPrefs.Save();
    }

    public static void BorrarTodo()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}