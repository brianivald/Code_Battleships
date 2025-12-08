using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScriptDataWrapper
{
    public List<ScriptData> savedScripts = new List<ScriptData>();
}

[Serializable]
public class ScriptData
{
    public string nombre;
    public List<string> comandosEnTexto; // Guardaremos el ToString() o una representación simple

    // Constructor vacío para JSON
    public ScriptData() { }

    public ScriptData(string _nombre, List<ShipCommand> comandos)
    {
        nombre = _nombre;
        comandosEnTexto = new List<string>();

        // Convertimos cada comando a una cadena especial para poder reconstruirlo
        foreach (var cmd in comandos)
        {
            // Usamos un formato especial: "TIPO|PARAMETROS"
            // Ejemplo: "MOVER|N", "TORPEDO|S", "IF|SALUD|<|50|REPARAR"
            comandosEnTexto.Add(SerializarComando(cmd));
        }
    }

    // --- MAGIA DE CONVERSIÓN ---

    public static string SerializarComando(ShipCommand cmd)
    {
        if (cmd is MoverCommand m) return $"MOVER|{ShipCommand.DirToString(m.direccion)}";
        if (cmd is TorpedoCommand t) return $"TORPEDO|{ShipCommand.DirToString(t.direccion)}";
        if (cmd is PlantarMinaCommand p) return $"PLANTAR_MINA|{ShipCommand.DirToString(p.direccion)}";
        if (cmd is RepararCommand) return "REPARAR";
        if (cmd is CanonCommand c) return $"CAÑON|{ShipCommand.DirToString(c.dir1)}|{ShipCommand.DirToString(c.dir2)}|{c.numero}";

        if (cmd is IfCommand i)
        {
            string op = (i.op == Operador.MenorQue) ? "<" : (i.op == Operador.MayorQue ? ">" : "=");
            string accion = SerializarComando(i.comandoAccion);
            return $"IF|{i.tipo}|{op}|{i.valor}|{i.parametroExtra}|{accion}";
        }
        return "";
    }

    public static ShipCommand DeserializarComando(string linea)
    {
        string[] partes = linea.Split('|');
        string tipo = partes[0];

        if (tipo == "MOVER") return new MoverCommand(StringADir(partes[1]));
        if (tipo == "TORPEDO") return new TorpedoCommand(StringADir(partes[1]));
        if (tipo == "PLANTAR_MINA") return new PlantarMinaCommand(StringADir(partes[1]));
        if (tipo == "REPARAR") return new RepararCommand();
        if (tipo == "CAÑON") return new CanonCommand(StringADir(partes[1]), StringADir(partes[2]), int.Parse(partes[3]));

        if (tipo == "IF")
        {
            CondicionTipo cond = (CondicionTipo)Enum.Parse(typeof(CondicionTipo), partes[1]);
            string opStr = partes[2];
            Operador op = (opStr == "<") ? Operador.MenorQue : (opStr == ">") ? Operador.MayorQue : Operador.IgualA;
            int valor = int.Parse(partes[3]);
            int paramExtra = int.Parse(partes[4]);

            // Reconstruir la acción anidada (que está en el resto del string)
            // Esto es un poco truco: unimos el resto por si la acción tenía '|'
            string restoAccion = string.Join("|", partes, 5, partes.Length - 5);
            ShipCommand accion = DeserializarComando(restoAccion);

            return new IfCommand(cond, op, valor, paramExtra, accion);
        }

        return null;
    }

    static Direccion StringADir(string s)
    {
        if (s == "N") return Direccion.Norte;
        if (s == "S") return Direccion.Sur;
        if (s == "E") return Direccion.Este;
        return Direccion.Oeste;
    }
}