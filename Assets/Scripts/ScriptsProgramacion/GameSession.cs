using UnityEngine;
using System.Collections.Generic;

public static class GameSession
{
    // Aquí guardaremos la info de cada jugador listo
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