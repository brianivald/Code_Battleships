using UnityEngine;
using System.Collections.Generic;

public class GameSession : MonoBehaviour
{
    // --- CONFIGURACIÓN DE LA PARTIDA (Global) ---
    public static int tamanoTablero = 10; // Por defecto 10x10
    public static int minasMaximas = 3;
    public static int reparacionesMaximas = 3;
    public static int saludMaxima = 100;
    public static int danoCanon = 25;
    public static int danoTorpedo = 40;
    
    // Aqu� guardaremos la info de cada jugador listo
    public class PlayerData
    {
        public int playerID;
        public Color colorBarco;
        public List<ShipCommand> rutinaDeComandos;
        public int vidaMaxima;
        public int numMinas;
        public int numReparaciones;
    }

    // La lista que llenaremos antes de cambiar de escena
    public static List<PlayerData> jugadoresListos = new List<PlayerData>();

    public static void LimpiarDatos()
    {
        jugadoresListos.Clear();
    }
    
}