using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class LogicBuilder : MonoBehaviour
{
    [Header("Referencias UI")]
    public CanvasGroup panelCanvasGroup;
    public Image fondoDelPanel;
    public TextMeshProUGUI textoScript; // Usando tu variable original de Texto

    [Header("Botones de Comando")]
    public Button btnMover, btnTorpedo, btnPlantarMina, btnCanon, btnIf, btnReparar;

    [Header("Botones de Par�metros")]
    public Button btnNorte, btnSur, btnEste, btnOeste;
    public Button btnSalud, btnRadar;
    public Button btnMenorQue, btnMayorQue, btnIgualA, btnMenos;

    [Header("Botones Num�ricos (0-9)")]
    public Button[] botonesNumeros;

    [Header("Botones de Control")]
    public Button btnBorrar, btnGuardar;

    [Header("Conexi�n al Manager")]
    public ProgrammingManager manager;

    // --- Estado Interno ---
    private PlayerSlot slotEnEdicion;
    private List<ShipCommand> comandosTemporales = new List<ShipCommand>();
    private string lineaEnConstruccion = "";
    private int indiceSeleccionado = -1;
    private string bufferNumerico = "";

    private enum EstadoConstruccion
    {
        Idle,
        EsperandoDireccion,
        EsperandoDirCanon1, EsperandoDirCanon2, EsperandoNumCanon,
        EsperandoCondicionIf,
        EsperandoDirRadar,      // Direcci�n del Radar
        EsperandoOperadorIf,
        EsperandoValorIf,       // Valor gen�rico (Salud)
        EsperandoValorRadar     // NUEVO: Estado especial restrictivo para Radar
    }
    private EstadoConstruccion estadoActual;

    // --- Variables Temporales ---
    private string comandoBase;
    private CondicionTipo ifCondicion;
    private Operador ifOperador;
    private int ifValor;
    private Direccion dirCanon1;
    private Direccion dirCanon2;

    void Start()
    {
        if (panelCanvasGroup == null) panelCanvasGroup = GetComponent<CanvasGroup>();
        DesactivarPanel();
        ConectarBotones();
        estadoActual = EstadoConstruccion.Idle;
    }

    public void SeleccionarFila(int indice)
    {
        if (!string.IsNullOrEmpty(lineaEnConstruccion)) return;
        indiceSeleccionado = (indiceSeleccionado == indice) ? -1 : indice;
        RefrescarTextoScript();
    }

    void ConectarBotones()
    {
        btnMover.onClick.AddListener(OnClick_Mover);
        btnTorpedo.onClick.AddListener(OnClick_Torpedo);
        btnPlantarMina.onClick.AddListener(OnClick_PlantarMina);
        btnCanon.onClick.AddListener(OnClick_Canon);
        btnReparar.onClick.AddListener(() => FinalizarAccionIf(new RepararCommand()));
        btnIf.onClick.AddListener(OnClick_If);

        btnNorte.onClick.AddListener(() => OnClick_Direccion(Direccion.Norte));
        btnSur.onClick.AddListener(() => OnClick_Direccion(Direccion.Sur));
        btnEste.onClick.AddListener(() => OnClick_Direccion(Direccion.Este));
        btnOeste.onClick.AddListener(() => OnClick_Direccion(Direccion.Oeste));

        btnSalud.onClick.AddListener(() => OnClick_Condicion(CondicionTipo.Salud));
        btnRadar.onClick.AddListener(OnClick_RadarInicio);

        btnMenorQue.onClick.AddListener(() => OnClick_Operador(Operador.MenorQue));
        btnMayorQue.onClick.AddListener(() => OnClick_Operador(Operador.MayorQue));
        btnIgualA.onClick.AddListener(() => OnClick_Operador(Operador.IgualA));

        if (btnMenos != null) btnMenos.onClick.AddListener(OnClick_Menos);

        for (int i = 0; i < botonesNumeros.Length; i++)
        {
            int x = i;
            botonesNumeros[i].onClick.AddListener(() => OnClick_Numero(x));
        }

        btnBorrar.onClick.AddListener(OnClick_Borrar);
        btnGuardar.onClick.AddListener(OnClick_Guardar);
    }

    // --- Iniciar Comandos ---
    // En LogicBuilder.cs

    void IniciarComando(string nombreComando, EstadoConstruccion siguienteEstado)
    {
        // CORRECCIÓN AQUÍ:
        // Antes solo revisaba EsperandoValorIf. Ahora revisamos ambos.
        if (estadoActual == EstadoConstruccion.EsperandoValorIf || 
            estadoActual == EstadoConstruccion.EsperandoValorRadar)
        {
            // Si el buffer está vacío o es solo un menos, no dejamos avanzar
            if (string.IsNullOrEmpty(bufferNumerico) || bufferNumerico == "-") return; 
            
            ProcesarBufferNumericoIf();
        }

        // ... (El resto de la función sigue igual) ...
        if (lineaEnConstruccion.StartsWith("IF(")) {
            comandoBase = nombreComando + "_IF";
            lineaEnConstruccion += nombreComando + "(";
        } else {
            comandoBase = nombreComando;
            lineaEnConstruccion = nombreComando + "(";
        }

        estadoActual = siguienteEstado;
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    void OnClick_Mover() => IniciarComando("MOVER", EstadoConstruccion.EsperandoDireccion);
    void OnClick_Torpedo() => IniciarComando("TORPEDO", EstadoConstruccion.EsperandoDireccion);
    void OnClick_PlantarMina() => IniciarComando("PLANTAR_MINA", EstadoConstruccion.EsperandoDireccion);
    void OnClick_Canon() => IniciarComando("CA�ON", EstadoConstruccion.EsperandoDirCanon1);

    void OnClick_If()
    {
        estadoActual = EstadoConstruccion.EsperandoCondicionIf;
        lineaEnConstruccion = "IF(";
        comandoBase = "IF";
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    // --- L�gica IF y Radar ---

    void OnClick_RadarInicio()
    {
        // Solo pide direcci�n, ya no pide rango
        lineaEnConstruccion += "RADAR(";
        estadoActual = EstadoConstruccion.EsperandoDirRadar;
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    void OnClick_Condicion(CondicionTipo tipo)
    {
        ifCondicion = tipo; // Salud
        estadoActual = EstadoConstruccion.EsperandoOperadorIf;
        lineaEnConstruccion += "SALUD";
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    void OnClick_Operador(Operador op)
    {
        ifOperador = op;
        string opStr = (op == Operador.MenorQue) ? "<" : (op == Operador.MayorQue ? ">" : "=");
        lineaEnConstruccion += opStr;

        // DECISI�N: �Qu� sigue?
        if (lineaEnConstruccion.Contains("RADAR"))
        {
            // Si es RADAR, vamos al estado restrictivo (Solo 0)
            estadoActual = EstadoConstruccion.EsperandoValorRadar;
        }
        else
        {
            // Si es SALUD, vamos al estado libre
            bufferNumerico = "";
            estadoActual = EstadoConstruccion.EsperandoValorIf;
        }

        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    void OnClick_Menos()
    {
        // CORRECCIÓN: Ahora verificamos si estamos en CUALQUIERA de los dos estados
        // (Ya sea esperando valor de salud O esperando valor de radar)
        bool esEstadoValido = (estadoActual == EstadoConstruccion.EsperandoValorIf || 
                               estadoActual == EstadoConstruccion.EsperandoValorRadar);

        // Solo escribe el menos si el estado es válido y no hemos escrito nada aún
        if (esEstadoValido && string.IsNullOrEmpty(bufferNumerico))
        {
            bufferNumerico = "-";
            lineaEnConstruccion += "-";
            RefrescarTextoScript();
            ActualizarBotonesUI();
        }
    }

    void OnClick_Numero(int num)
    {
        if (estadoActual == EstadoConstruccion.EsperandoNumCanon)
        {
            FinalizarComandoCanon(num);
        }
       else if (estadoActual == EstadoConstruccion.EsperandoValorIf || 
                 estadoActual == EstadoConstruccion.EsperandoValorRadar)
        {
            if (bufferNumerico.Length < 4) 
            {
                bufferNumerico += num.ToString();
                lineaEnConstruccion += num.ToString();
            }
        }
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    void ProcesarBufferNumericoIf()
    {
        if (!string.IsNullOrEmpty(bufferNumerico) && bufferNumerico != "-")
        {
            ifValor = int.Parse(bufferNumerico);
            bufferNumerico = "";
            lineaEnConstruccion += "): ";
        }
    }

    void OnClick_Direccion(Direccion dir)
    {
        string dirStr = ShipCommand.DirToString(dir);
        switch (estadoActual)
        {
            case EstadoConstruccion.EsperandoDireccion:
                ShipCommand cmd = null;
                if (comandoBase.StartsWith("MOVER")) cmd = new MoverCommand(dir);
                else if (comandoBase.StartsWith("TORPEDO")) cmd = new TorpedoCommand(dir);
                else if (comandoBase.StartsWith("PLANTAR_MINA")) cmd = new PlantarMinaCommand(dir);
                if (comandoBase.Contains("_IF")) FinalizarAccionIf(cmd);
                else FinalizarComandoSimple(cmd, dirStr);
                break;

            case EstadoConstruccion.EsperandoDirCanon1:
                dirCanon1 = dir; lineaEnConstruccion += dirStr + ","; estadoActual = EstadoConstruccion.EsperandoDirCanon2; break;
            case EstadoConstruccion.EsperandoDirCanon2:
                dirCanon2 = dir; lineaEnConstruccion += dirStr + ","; estadoActual = EstadoConstruccion.EsperandoNumCanon; break;

            case EstadoConstruccion.EsperandoDirRadar:
                // Radar Simplificado: Direcci�n -> Operador
                ifCondicion = ObtenerTipoRadar(dir);
                lineaEnConstruccion += dirStr + ")"; // Cerramos par�ntesis aqu�
                estadoActual = EstadoConstruccion.EsperandoOperadorIf;
                break;
        }
        RefrescarTextoScript();
        ActualizarBotonesUI();
    }

    // --- FINALIZADORES ---
    void FinalizarComandoSimple(ShipCommand cmd, string dirStr) { comandosTemporales.Add(cmd); ResetearAIdle(); }
    void FinalizarComandoCanon(int num)
    {
        ShipCommand cmd = new CanonCommand(dirCanon1, dirCanon2, num);
        lineaEnConstruccion += num.ToString() + ")";
        if (comandoBase.Contains("_IF")) FinalizarAccionIf(cmd);
        else { comandosTemporales.Add(cmd); ResetearAIdle(); }
    }
    // En LogicBuilder.cs

    void FinalizarAccionIf(ShipCommand accion)
    {
        // CORRECCIÓN AQUÍ TAMBIÉN:
        // Añadimos la condición "|| estadoActual == EstadoConstruccion.EsperandoValorRadar"
        if (estadoActual == EstadoConstruccion.EsperandoValorIf || 
            estadoActual == EstadoConstruccion.EsperandoValorRadar)
        {
            if (string.IsNullOrEmpty(bufferNumerico) || bufferNumerico == "-") return;
            ProcesarBufferNumericoIf();
        }

        if (!lineaEnConstruccion.EndsWith("REPARAR")) lineaEnConstruccion += ")";
        
        // Pasamos 0 como rango (ya no se usa, o usa el default del Controlador)
        ShipCommand comandoIf = new IfCommand(ifCondicion, ifOperador, ifValor, 0, accion);
        comandosTemporales.Add(comandoIf);
        ResetearAIdle();
    }
    void ResetearAIdle() { estadoActual = EstadoConstruccion.Idle; lineaEnConstruccion = ""; bufferNumerico = ""; RefrescarTextoScript(); ActualizarBotonesUI(); }

    // --- UTILS ---
    CondicionTipo ObtenerTipoRadar(Direccion dir) { switch (dir) { case Direccion.Norte: return CondicionTipo.Radar_N; case Direccion.Sur: return CondicionTipo.Radar_S; case Direccion.Este: return CondicionTipo.Radar_E; default: return CondicionTipo.Radar_O; } }
    void SetGrupoBotones(bool activo, params Button[] botones) { foreach (var b in botones) if (b != null) b.interactable = activo; }

    // --- UI/VISUALES ---
    void RefrescarTextoScript()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < comandosTemporales.Count; i++)
        {
            string linea = (i + 1) + ". " + comandosTemporales[i].ToString();
            if (i == indiceSeleccionado) linea = $"<mark=#FFFF0055>{linea}</mark>";
            linea = $"<link=\"{i}\">{linea}</link>";
            sb.AppendLine(linea);
        }
        sb.Append(lineaEnConstruccion);
        if (textoScript != null) textoScript.text = sb.ToString();
    }
    void OnClick_Borrar()
    {
        if (!string.IsNullOrEmpty(lineaEnConstruccion)) { ResetearAIdle(); return; }
        if (indiceSeleccionado != -1 && indiceSeleccionado < comandosTemporales.Count) { comandosTemporales.RemoveAt(indiceSeleccionado); indiceSeleccionado = -1; RefrescarTextoScript(); return; }
        if (comandosTemporales.Count > 0) { comandosTemporales.RemoveAt(comandosTemporales.Count - 1); RefrescarTextoScript(); }
    }
    void OnClick_Guardar() { if (estadoActual == EstadoConstruccion.Idle) manager.GuardarScript(slotEnEdicion, comandosTemporales); }
    public void ActivarPanel(PlayerSlot slot) { slotEnEdicion = slot; if (fondoDelPanel != null) fondoDelPanel.color = slot.imgFondoCuadrado.color; comandosTemporales = new List<ShipCommand>(slot.scriptGuardado); indiceSeleccionado = -1; panelCanvasGroup.alpha = 1.0f; panelCanvasGroup.interactable = true; panelCanvasGroup.blocksRaycasts = true; ResetearAIdle(); }
    public void DesactivarPanel() { if (fondoDelPanel != null) fondoDelPanel.color = Color.grey; panelCanvasGroup.alpha = 0.5f; panelCanvasGroup.interactable = false; panelCanvasGroup.blocksRaycasts = false; }

    // --- UI STATE MACHINE (BLOQUEOS) ---
    void ActualizarBotonesUI()
    {
        SetGrupoBotones(false, btnMover, btnTorpedo, btnPlantarMina, btnCanon, btnIf, btnReparar);
        SetGrupoBotones(false, btnNorte, btnSur, btnEste, btnOeste);
        SetGrupoBotones(false, btnSalud, btnRadar);
        SetGrupoBotones(false, btnMenorQue, btnMayorQue, btnIgualA);
        SetGrupoBotones(false, botonesNumeros);
        if (btnMenos != null) btnMenos.interactable = false;

        switch (estadoActual)
        {
            case EstadoConstruccion.Idle:
                SetGrupoBotones(true, btnMover, btnTorpedo, btnPlantarMina, btnCanon, btnIf);
                break;
            case EstadoConstruccion.EsperandoDireccion:
            case EstadoConstruccion.EsperandoDirRadar:
                SetGrupoBotones(true, btnNorte, btnSur, btnEste, btnOeste); break;
            case EstadoConstruccion.EsperandoDirCanon1: SetGrupoBotones(true, btnNorte, btnSur); break;
            case EstadoConstruccion.EsperandoDirCanon2: SetGrupoBotones(true, btnEste, btnOeste); break;
            case EstadoConstruccion.EsperandoNumCanon: SetGrupoBotones(true, botonesNumeros); break;
            case EstadoConstruccion.EsperandoCondicionIf: SetGrupoBotones(true, btnSalud, btnRadar); break;
            case EstadoConstruccion.EsperandoOperadorIf: SetGrupoBotones(true, btnMenorQue, btnMayorQue, btnIgualA); break;

            case EstadoConstruccion.EsperandoValorIf:
            case EstadoConstruccion.EsperandoValorRadar: // <--- ¡JUNTAMOS LOS DOS CASOS!
                
                // Permitimos todos los números
                SetGrupoBotones(true, botonesNumeros);

                // Permitimos el botón Menos (si está al inicio)
                if (string.IsNullOrEmpty(bufferNumerico) && btnMenos != null) 
                    btnMenos.interactable = true;

                // Permitimos Acciones si ya hay un número válido escrito
                bool hayNumeroValido = !string.IsNullOrEmpty(bufferNumerico) && bufferNumerico != "-";
                
                if (hayNumeroValido)
                {
                    SetGrupoBotones(true, btnMover, btnTorpedo, btnPlantarMina, btnCanon, btnReparar);
                }
                break;

        }
    }
}