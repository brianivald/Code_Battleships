using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;   // ← IMPORTANTE para usar ScrollRect

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI textoDescripcion;

    // ← NUEVO: referencia al ScrollRect del panel donde está el texto
    public ScrollRect scrollRect;

    public void MostrarDescripcion(string instruccion)
    {
        switch (instruccion)
        {
            case "Direccion":
                textoDescripcion.text = 
                    "Cuando una instrucción requiere una Dirección se refiere a alguno de los cuatro puntos cardinales: N, S, E, O";
                break;

            case "Mover":
                textoDescripcion.text =
                    "<b>Instrucción:</b> MOVER(DIRECCION)\n\n" +
                    "<b>Explicación:</b> Desplaza la unidad una casilla en la dirección establecida (si está en el área de movimiento).\n\n" +

                    "Parámetro <b>DIRECCION:</b> N, S, E, O.\n" +
                    "<b>Explicación del parámetro:</b> Valores permitidos para indicar la dirección del submarino:\n" +
                    "• N = Norte\n" +
                    "• S = Sur\n" +
                    "• E = Este\n" +
                    "• O = Oeste\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>MOVER(E)</color> — La unidad intenta moverse una casilla hacia el Este.\n\n" +

                    "<b>Notas:</b> Si la casilla destino está ocupada por otro objeto (jugador o límite del mapa), se considera una colisión que disminuye puntos de vida del o los jugadores involucrados.";
                break;

            case "Plantar mina":
                textoDescripcion.text =
                    "<b>Instrucción:</b> PLANTAR_MINA(DIRECCION_DE_SALIDA)\n\n" +
                    "<b>Explicación:</b> Coloca una mina en la ubicación actual del jugador. " +
                    "Después de plantarla, el submarino del jugador se mueve una casilla en la dirección indicada " +
                    "para evitar entrar en contacto con la mina.\n\n" +

                    "Parámetro <b>DIRECCION_DE_SALIDA:</b> N, S, E, O.\n" +
                    "<b>Explicación del parámetro:</b> Dirección en la cual el submarino se moverá una casilla después de plantar la mina.\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>PLANTAR_MINA(E)</color> — planta una mina e intenta moverse una casilla hacia el Este.\n\n" +

                    "<b>Notas:</b> Si la dirección de movimiento no se efectúa debido a una colisión con un objeto, " +
                    "el submarino no se mueve y recibe el daño de la mina.";
                break;

            case "Cañon":
                textoDescripcion.text =
                    "<b>Instrucción:</b> CAÑON (<DIRECCIÓN, DIRECCIÓN, ALCANCE>)\n\n" +
                    "<b>Explicación:</b> Lanza una bala de cañón a una casilla en específico, si " +
                    "acierta en algún barco el daño que hace es de 35pts de vida, además " +
                    "de que al caer hace daño en área a las 8 casillas vecinas de 15pts de vida.\n\n" +

                    "Parámetro <b><DIRECCION>:</b> N, S.\n" +
                    "<b>Explicación:</b> Dirección vertical del disparo.\n\n" +
                    "Parámetro <b><DIRECCION></b>: E, O.\n" +
                    "<b>Explicación:</b> Dirección horizontal del disparo.\n\n" +
                    "Parámetro <b><ALCANCE></b>: número.\n" +
                    "<b>Explicación:</b> Distancia a la que caerá el proyectil.\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>CAÑON (N, E, 2)</color> — el proyectil caerá 2 casillas al noreste.\n\n" +

                    "<b>Notas:</b> Si impacta un barco, solo se aplica el daño principal. Los vecinos reciben daño en área.";
                break;

            case "Torpedo":
                textoDescripcion.text =
                    "<b>Instrucción:</b> TORPEDO (<DIRECCION>)\n\n" +
                    "<b>Explicación:</b> Lanza un torpedo a lo largo de una fila o columna. " +
                    "Si impacta un barco, este recibe 20 pts de daño.\n\n" +

                    "Parámetro <b><DIRECCION>:</b> N, S, E, O.\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>TORPEDO(N)</color> — recorre todas las casillas al norte.\n\n" +

                    "<b>Notas:</b> El torpedo solo daña al primer barco que encuentre.";
                break;

            case "Salud":
                textoDescripcion.text =
                    "<b>Instrucción:</b> SALUD\n\n" +
                    "<b>Explicación:</b> Devuelve la salud actual del barco. " +
                    "Normalmente se usa dentro de un condicional.\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>IF (SALUD<50): REPARAR</color>";
                break;

            case "Reparar":
                textoDescripcion.text =
                    "<b>Instrucción:</b> REPARAR\n\n" +
                    "<b>Explicación:</b> Aumenta la salud del barco en 25 pts.\n\n" +
                    "<b>Notas:</b> Límite de 3 usos por partida.";
                break;

            case "Radar":
                textoDescripcion.text =
                    "<b>Instrucción:</b> RADAR(<DIRECCION>)\n\n" +
                    "<b>Explicación:</b> Escanea todas las casillas hacia una dirección. " +
                    "Devuelve 1 si detecta barco, -1 si detecta mina o borde.\n\n" +

                    "<b>Ejemplo:</b> <color=#00BFFF>RADAR(N)</color>";
                break;

            case "Condicional":
                textoDescripcion.text =
                    "<b>Instrucción:</b> IF (CONDICIÓN): INSTRUCCIÓN\n\n" +
                    "<b>Explicación:</b> Ejecuta una instrucción si la condición es verdadera.\n\n" +
                    "<b>Ejemplo:</b> <color=#00BFFF>IF (SALUD<50): REPARAR</color>";
                break;

            default:
                textoDescripcion.text = "Selecciona una instrucción para ver su descripción.";
                break;
        }

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f; // Arriba del todo
        }
    }

    public void Regresar()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
