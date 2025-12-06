using UnityEngine;

// --- ENUMS GLOBALES ---
public enum Direccion { Norte, Sur, Este, Oeste }
public enum CondicionTipo { Salud, Radar_N, Radar_S, Radar_E, Radar_O }
public enum Operador { MenorQue, MayorQue, IgualA }
// ----------------------

// CLASE BASE
public abstract class ShipCommand
{
    public abstract void Ejecutar(ControladorDeBarco barco);
    public abstract override string ToString();

    public static string DirToString(Direccion dir)
    {
        switch (dir)
        {
            case Direccion.Norte: return "N";
            case Direccion.Sur: return "S";
            case Direccion.Este: return "E";
            case Direccion.Oeste: return "O";
            default: return "?";
        }
    }
}

// --- A PARTIR DE AQUÍ ES LO QUE TE FALTABA ---

// 1. MOVER
public class MoverCommand : ShipCommand
{
    public Direccion direccion;
    public MoverCommand(Direccion dir) { this.direccion = dir; }
    public override void Ejecutar(ControladorDeBarco barco) { barco.MoverHacia(this.direccion); }
    public override string ToString() { return $"MOVER({DirToString(direccion)})"; }
}

// 2. TORPEDO
public class TorpedoCommand : ShipCommand
{
    public Direccion direccion;
    public TorpedoCommand(Direccion dir) { this.direccion = dir; }
    public override void Ejecutar(ControladorDeBarco barco) { barco.LanzarTorpedo(this.direccion); }
    public override string ToString() { return $"TORPEDO({DirToString(direccion)})"; }
}

// 3. PLANTAR MINA
public class PlantarMinaCommand : ShipCommand
{
    public Direccion direccion;
    public PlantarMinaCommand(Direccion dir) { this.direccion = dir; }
    public override void Ejecutar(ControladorDeBarco barco) { barco.PlantarMina(this.direccion); }
    public override string ToString() { return $"PLANTAR_MINA({DirToString(direccion)})"; }
}

// 4. REPARAR
public class RepararCommand : ShipCommand
{
    public RepararCommand() { } // Sin parámetros
    public override void Ejecutar(ControladorDeBarco barco) { barco.Reparar(); }
    public override string ToString() { return "REPARAR"; }
}

// 5. CAÑON
public class CanonCommand : ShipCommand
{
    public Direccion dir1;
    public Direccion dir2;
    public int numero;

    public CanonCommand(Direccion dir1, Direccion dir2, int num)
    {
        this.dir1 = dir1; this.dir2 = dir2; this.numero = num;
    }
    public override void Ejecutar(ControladorDeBarco barco) { barco.DispararCanon(dir1, dir2, numero); }
    public override string ToString() { return $"CAÑON({DirToString(dir1)},{DirToString(dir2)},{numero})"; }
}

// 6. IF (Condicional)
public class IfCommand : ShipCommand
{
    public CondicionTipo tipo;
    public Operador op;
    public int valor;
    public int parametroExtra; // ¡NUEVO! Para el rango del Radar
    public ShipCommand comandoAccion; // Comando anidado

    public IfCommand(CondicionTipo tipo, Operador op, int valor, int paramExtra, ShipCommand accion)
    {
        this.tipo = tipo;
        this.op = op;
        this.valor = valor;
        this.parametroExtra = paramExtra; // Guardamos el rango
        this.comandoAccion = accion;
    }

    public override void Ejecutar(ControladorDeBarco barco)
    {
        // Ahora pasamos también el parametroExtra al barco
        int valorBarco = barco.GetCondicionValor(tipo, parametroExtra);

        // ... (El resto de la lógica de switch/case sigue igual) ...
        bool condicionCumplida = false;
        switch (op)
        {
            case Operador.MenorQue: condicionCumplida = (valorBarco < valor); break;
            case Operador.MayorQue: condicionCumplida = (valorBarco > valor); break;
            case Operador.IgualA: condicionCumplida = (valorBarco == valor); break;
        }

        if (condicionCumplida) comandoAccion.Ejecutar(barco);
    }

    public override string ToString()
    {
        string opStr = "";
        switch (op)
        {
            case Operador.MenorQue: opStr = "<"; break;
            case Operador.MayorQue: opStr = ">"; break;
            case Operador.IgualA: opStr = "="; break;
        }

        string tipoStr = tipo.ToString();

        // --- CAMBIO AQUÍ: Formato simplificado RADAR(N) ---
        if (tipoStr.StartsWith("Radar_"))
        {
            string dir = tipoStr.Substring(6);
            tipoStr = $"RADAR({dir})"; // Ya no mostramos el rango
        }

        return $"IF({tipoStr}{opStr}{valor}): {comandoAccion.ToString()}";
    }
}