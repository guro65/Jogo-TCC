using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GerenciadorJogoTCC : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelInicio;
    public GameObject painelDadosIniciais;
    public GameObject painelTopo;
    public GameObject painelDialogo;
    public GameObject painelEscolhas;
    public GameObject painelResultadoFase;
    public GameObject painelFinal;

    [Header("Seleção de fase")]
    public GameObject painelSelecaoFase;
    public TMP_Text textoSelecaoFase;
    [TextArea(2, 4)]
    public string mensagemSelecaoFase = "Escolha qual desafio deseja enfrentar agora. Cada fase representa um nível diferente da carreira em TI.";
    public float velocidadeDigitacaoSelecaoFase = 0.035f;
    public Button botaoFaseFacil;
    public Button botaoFaseMedia;
    public Button botaoFaseDificil;
    public Button botaoRecriarPersonagemSelecao;

    [Header("Feedback da resposta")]
    public GameObject painelFeedback;
    public TMP_Text textoFeedback;
    public Button botaoContinuarFeedback;

    [Header("Animação do topo")]
    public Animator animatorTextoFase;
    public string triggerAnimacaoTextoFase = "MostrarFase";

    [Header("Áudio")]
    public AudioSource fonteAudio;
    public AudioClip musicaInicio;
    public AudioClip musicaFaseFacil;
    public AudioClip musicaFaseMedia;
    public AudioClip musicaFaseDificil;

    [Header("Fundo")]
    public GameObject fundo;
    public Image imagemFundo;
    public Sprite fundoTrabalhoTI;

    [Tooltip("Imagem de fundo usada quando o painel de seleção de fase estiver ativo.")]
    public Sprite fundoSelecaoFase;

    [Header("Transição de fase")]
    public Image imagemTransicaoPreta;
    public float duracaoFadeEntrada = 0.45f;
    public float tempoTelaPreta = 0.35f;
    public float duracaoFadeSaida = 0.75f;
    public bool usarTransicaoAoComecarPrimeiraFase = true;

    [Header("Tela inicial")]
    public TMP_InputField campoNome;
    public TMP_Dropdown dropdownGenero;
    public Button botaoComecar;

    [Header("Fluxo inicial em etapas")]
    public TMP_Text textoInstrucaoInicial;
    public GameObject grupoNome;
    public GameObject grupoGenero;
    public float velocidadeDigitacaoInicial = 0.035f;

    [Header("Topo")]
    public TMP_Text textoFase;
    public Button botaoVoltarSelecaoFase;

    [Header("Medidor de aprovação")]
    public Slider medidorAprovacao;
    public TMP_Text textoMedidorAprovacao;

    [Header("Diálogo")]
    public GameObject caixaNomeNPC;
    public TMP_Text textoNomeNPC;
    public TMP_Text textoFalaNPC;

    public GameObject caixaNomeJogador;
    public TMP_Text textoNomeJogador;
    public TMP_Text textoFalaJogador;

    public Button botaoContinuar;

    [Header("Escolhas")]
    public Button botaoEscolha1;
    public Button botaoEscolha2;
    public Button botaoEscolha3;
    public TMP_Text textoEscolha1;
    public TMP_Text textoEscolha2;
    public TMP_Text textoEscolha3;

    [Header("Resultado da fase")]
    public TMP_Text textoResultadoFase;
    public Button botaoContinuarFase;

    [Tooltip("Botão para reiniciar a fase atual no painel de resultado.")]
    public Button botaoReiniciarFaseResultado;

    [Header("Final")]
    public TMP_Text textoFinal;
    public Button botaoReiniciar;

    [Header("Game Over / Demissão")]
    public GameObject painelGameOver;
    public TMP_Text textoGameOver;
    public Button botaoVoltarCriacaoPersonagem;

    [Tooltip("Botão para reiniciar a fase atual após demissão. O nome antigo foi mantido para não perder a referência no Inspector.")]
    public Button botaoReiniciarFase1GameOver;

    public int quantidadeRespostasRuinsSeguidasParaGameOver = 3;

    [Header("Clima da equipe / Risco de demissão")]
    public TMP_Text textoClimaEquipe;
    public int sequenciaRuimParaMostrarAlerta = 2;

    [Header("Visual da cena")]
    public ControladorCenaVN controladorCena;

    [Header("Aparências do jogador")]
    public List<AparenciaJogador> aparenciasMasculinas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasFemininas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasNaoDefinidas = new List<AparenciaJogador>();

    [Header("3 NPCs da Fase Fácil / Júnior")]
    public List<DadosPersonagem> personagensJunior = new List<DadosPersonagem>();

    [Header("3 NPCs da Fase Média / Pleno")]
    public List<DadosPersonagem> personagensPleno = new List<DadosPersonagem>();

    [Header("3 NPCs da Fase Difícil / Sênior")]
    public List<DadosPersonagem> personagensSenior = new List<DadosPersonagem>();

    [Header("Efeito de digitação")]
    public float velocidadeDigitacao = 0.025f;

    private string nomeJogador = "Jogador";
    private GeneroJogador generoJogador = GeneroJogador.Nada;
    private AparenciaJogador aparenciaAtualJogador;
    private Emocao emocaoAtualJogador = Emocao.Neutro;
    private Emocao ultimaEmocaoPersonagem = Emocao.Neutro;

    private FaseProfissional faseAtual = FaseProfissional.FacilJunior;
    private FaseProfissional faseDoGameOver = FaseProfissional.FacilJunior;
    private FaseProfissional proximaFaseDepoisResultado;

    private int comunicacao;
    private int trabalhoEquipe;
    private int resolucaoProblemas;
    private int adaptabilidade;
    private int empatia;

    private int pontosFaseAtual;
    private int pontosMaximosFase;
    private float porcentagemFase;

    private List<NoDialogoVN> nos = new List<NoDialogoVN>();
    private int indiceNoAtual;

    private string ultimaRespostaJogador = "";
    private string ultimaReacaoNPC = "";
    private TomResposta ultimoTomEscolhido = TomResposta.Neutra;
    private int totalEscolhasBoas;
    private int totalEscolhasMedias;
    private int totalEscolhasRuins;
    private int sequenciaEscolhasRuins;
    private int ruinsComunicacao;
    private int ruinsTrabalhoEquipe;
    private int ruinsResolucaoProblemas;
    private int ruinsAdaptabilidade;
    private int ruinsEmpatia;
    private CategoriaSoftSkill ultimaCategoriaRuim;
    private bool exibindoReacaoEscolha;
    private bool aguardandoResultadoFase;
    private int proximoNoAposReacao;
    private bool finalizarDepoisResultado;
    private bool faseConcluidaPorAcertos;
    private bool exibindoConclusaoFase;

    private bool textoDigitando;
    private Coroutine rotinaDigitacao;
    private string textoCompletoNPC = "";
    private string textoCompletoJogador = "";

    private bool nomeJaConfirmado;
    private Coroutine rotinaDigitacaoInicial;
    private Coroutine rotinaDigitacaoSelecaoFase;

    private OpcaoEscolha opcaoAguardandoFeedback;
    private bool feedbackAguardandoContinuar;

    private const int TOTAL_PERGUNTAS_POR_FASE = 24;

    private class QuestaoTI
    {
        public CategoriaSoftSkill categoria;

        public DadosPersonagem npc;
        public DadosPersonagem esquerda;
        public DadosPersonagem centro;
        public DadosPersonagem direita;

        public Emocao emocaoNPC;
        public Emocao emocaoJogadorAoOuvir;

        public string falaNPC;

        public string botaoBom;
        public string respostaBoa;
        public string reacaoBoa;

        public string botaoMedio;
        public string respostaMedia;
        public string reacaoMedia;

        public string botaoRuim;
        public string respostaRuim;
        public string reacaoRuim;
    }

    void Start()
    {
        AtivarEstadoInicial();

        if (botaoComecar != null) botaoComecar.onClick.AddListener(PrepararJogador);
        if (campoNome != null) campoNome.onValueChanged.AddListener(AtualizarBotaoInicioPorNome);
        if (botaoContinuar != null) botaoContinuar.onClick.AddListener(ContinuarDialogo);
        if (botaoContinuarFase != null) botaoContinuarFase.onClick.AddListener(ContinuarDepoisResultadoFase);
        if (botaoReiniciarFaseResultado != null) botaoReiniciarFaseResultado.onClick.AddListener(ReiniciarFaseAtualPeloResultado);
        if (botaoContinuarFeedback != null) botaoContinuarFeedback.onClick.AddListener(ContinuarDepoisFeedback);
        if (botaoFaseFacil != null) botaoFaseFacil.onClick.AddListener(SelecionarFaseFacil);
        if (botaoFaseMedia != null) botaoFaseMedia.onClick.AddListener(SelecionarFaseMedia);
        if (botaoFaseDificil != null) botaoFaseDificil.onClick.AddListener(SelecionarFaseDificil);
        if (botaoRecriarPersonagemSelecao != null) botaoRecriarPersonagemSelecao.onClick.AddListener(VoltarParaCriacaoPersonagem);
        if (botaoVoltarSelecaoFase != null) botaoVoltarSelecaoFase.onClick.AddListener(VoltarParaSelecaoFase);
        if (botaoReiniciar != null) botaoReiniciar.onClick.AddListener(ReiniciarJogo);
        if (botaoVoltarCriacaoPersonagem != null) botaoVoltarCriacaoPersonagem.onClick.AddListener(VoltarParaCriacaoPersonagem);
        if (botaoReiniciarFase1GameOver != null) botaoReiniciarFase1GameOver.onClick.AddListener(ReiniciarFaseAtualAposGameOver);

        TocarMusica(musicaInicio);
    }
    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (feedbackAguardandoContinuar && painelFeedback != null && painelFeedback.activeSelf)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                ContinuarDepoisFeedback();

            return;
        }

        // Continuar diálogo com E
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (botaoContinuar != null && botaoContinuar.gameObject.activeSelf)
            {
                ContinuarDialogo();
            }
        }

        if (painelEscolhas == null || !painelEscolhas.activeSelf)
            return;

        // Escolha 1
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (botaoEscolha1 != null && botaoEscolha1.gameObject.activeSelf)
            {
                botaoEscolha1.onClick.Invoke();
            }
        }

        // Escolha 2
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (botaoEscolha2 != null && botaoEscolha2.gameObject.activeSelf)
            {
                botaoEscolha2.onClick.Invoke();
            }
        }

        // Escolha 3
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            if (botaoEscolha3 != null && botaoEscolha3.gameObject.activeSelf)
            {
                botaoEscolha3.onClick.Invoke();
            }
        }
    }

    void AtivarEstadoInicial()
    {
        if (painelInicio != null) painelInicio.SetActive(true);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(true);
        if (painelTopo != null) painelTopo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (painelSelecaoFase != null) painelSelecaoFase.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(false);
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (textoClimaEquipe != null) textoClimaEquipe.gameObject.SetActive(false);
        if (botaoVoltarSelecaoFase != null) botaoVoltarSelecaoFase.gameObject.SetActive(false);
        if (fundo != null) fundo.SetActive(false);
        PrepararImagemTransicao(false, 0f);

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        nomeJaConfirmado = false;
        ResetarPontuacaoGeral();

        if (grupoNome != null) grupoNome.SetActive(true);
        if (grupoGenero != null) grupoGenero.SetActive(false);
        if (dropdownGenero != null) dropdownGenero.gameObject.SetActive(false);

        if (campoNome != null)
        {
            campoNome.gameObject.SetActive(true);
            campoNome.interactable = true;
        }

        AtualizarBotaoInicioPorNome(campoNome != null ? campoNome.text : "");
        DigitarInstrucaoInicial("Coloque o seu nome representando seu avatar.");

        AtualizarMedidor();
    }

    void AtualizarBotaoInicioPorNome(string textoDigitado)
    {
        if (botaoComecar == null)
            return;

        if (!nomeJaConfirmado)
            botaoComecar.interactable = !string.IsNullOrWhiteSpace(textoDigitado);
        else
            botaoComecar.interactable = true;
    }

    void DigitarInstrucaoInicial(string texto)
    {
        if (textoInstrucaoInicial == null)
            return;

        if (rotinaDigitacaoInicial != null)
            StopCoroutine(rotinaDigitacaoInicial);

        rotinaDigitacaoInicial = StartCoroutine(DigitarInstrucaoInicialRotina(texto));
    }

    IEnumerator DigitarInstrucaoInicialRotina(string texto)
    {
        textoInstrucaoInicial.text = "";

        for (int i = 0; i <= texto.Length; i++)
        {
            textoInstrucaoInicial.text = texto.Substring(0, i);
            yield return new WaitForSeconds(velocidadeDigitacaoInicial);
        }
    }

    void PrepararJogador()
    {
        if (!nomeJaConfirmado)
        {
            nomeJogador = campoNome != null ? campoNome.text.Trim() : "";

            if (string.IsNullOrWhiteSpace(nomeJogador))
            {
                Debug.LogWarning("Digite um nome antes de continuar.");
                DigitarInstrucaoInicial("Digite um nome para representar seu avatar antes de continuar.");
                return;
            }

            nomeJaConfirmado = true;

            if (campoNome != null) campoNome.interactable = false;
            if (grupoGenero != null) grupoGenero.SetActive(true);
            if (dropdownGenero != null) dropdownGenero.gameObject.SetActive(true);

            DigitarInstrucaoInicial("Selecione o gênero do seu avatar.");
            AtualizarBotaoInicioPorNome(nomeJogador);
            return;
        }

        if (dropdownGenero == null)
        {
            Debug.LogError("Dropdown de gênero não foi ligado no Inspector.");
            return;
        }

        generoJogador = (GeneroJogador)dropdownGenero.value;

        if (generoJogador == GeneroJogador.Nada)
        {
            Debug.LogWarning("Escolha um gênero ou selecione Não definido.");
            DigitarInstrucaoInicial("Selecione o gênero do seu avatar para começar.");
            return;
        }

        aparenciaAtualJogador = SortearAparenciaJogador(generoJogador);
        emocaoAtualJogador = Emocao.Neutro;

        if (!ListasDeNPCsValidas())
            return;

        if (painelInicio != null) painelInicio.SetActive(false);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);

        MostrarSelecaoFase();
    }

    void DigitarTextoSelecaoFase()
    {
        if (textoSelecaoFase == null)
            return;

        if (rotinaDigitacaoSelecaoFase != null)
            StopCoroutine(rotinaDigitacaoSelecaoFase);

        rotinaDigitacaoSelecaoFase = StartCoroutine(DigitarTextoSelecaoFaseRotina(mensagemSelecaoFase));
    }

    IEnumerator DigitarTextoSelecaoFaseRotina(string texto)
    {
        textoSelecaoFase.text = "";

        for (int i = 0; i <= texto.Length; i++)
        {
            textoSelecaoFase.text = texto.Substring(0, i);
            yield return new WaitForSeconds(velocidadeDigitacaoSelecaoFase);
        }
    }

    void MostrarSelecaoFase()
    {
        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        if (rotinaDigitacaoSelecaoFase != null)
            StopCoroutine(rotinaDigitacaoSelecaoFase);

        textoDigitando = false;
        exibindoReacaoEscolha = false;
        aguardandoResultadoFase = false;
        feedbackAguardandoContinuar = false;
        opcaoAguardandoFeedback = null;

        if (painelInicio != null) painelInicio.SetActive(false);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);
        if (painelSelecaoFase != null) painelSelecaoFase.SetActive(true);
        if (painelTopo != null) painelTopo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(false);
        if (textoClimaEquipe != null) textoClimaEquipe.gameObject.SetActive(false);
        if (botaoVoltarSelecaoFase != null) botaoVoltarSelecaoFase.gameObject.SetActive(false);

        if (fundo != null) fundo.SetActive(true);
        if (imagemFundo != null)
        {
            if (fundoSelecaoFase != null)
                imagemFundo.sprite = fundoSelecaoFase;
            else if (fundoTrabalhoTI != null)
                imagemFundo.sprite = fundoTrabalhoTI;
        }

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        TocarMusica(musicaInicio);
        DigitarTextoSelecaoFase();
    }

    void SelecionarFaseFacil()
    {
        IniciarFase(FaseProfissional.FacilJunior);
    }

    void SelecionarFaseMedia()
    {
        IniciarFase(FaseProfissional.MedioPleno);
    }

    void SelecionarFaseDificil()
    {
        IniciarFase(FaseProfissional.DificilSenior);
    }

    void VoltarParaSelecaoFase()
    {
        MostrarSelecaoFase();
    }

    int AcertosNecessarios(FaseProfissional fase)
    {
        switch (fase)
        {
            case FaseProfissional.FacilJunior:
                return 3;

            case FaseProfissional.MedioPleno:
                return 5;

            case FaseProfissional.DificilSenior:
                return 8;

            default:
                return 0;
        }
    }

    bool ListasDeNPCsValidas()
    {
        if (personagensJunior == null || personagensJunior.Count < 3)
        {
            Debug.LogError("A fase Júnior precisa ter 3 NPCs.");
            return false;
        }

        if (personagensPleno == null || personagensPleno.Count < 3)
        {
            Debug.LogError("A fase Pleno precisa ter 3 NPCs.");
            return false;
        }

        if (personagensSenior == null || personagensSenior.Count < 3)
        {
            Debug.LogError("A fase Sênior precisa ter 3 NPCs.");
            return false;
        }

        return true;
    }

    AparenciaJogador SortearAparenciaJogador(GeneroJogador genero)
    {
        List<AparenciaJogador> lista = null;

        switch (genero)
        {
            case GeneroJogador.Masculino:
                lista = aparenciasMasculinas;
                break;

            case GeneroJogador.Feminino:
                lista = aparenciasFemininas;
                break;

            case GeneroJogador.NaoDefinido:
                lista = aparenciasNaoDefinidas;
                break;
        }

        if (lista == null || lista.Count == 0)
            return null;

        return lista[Random.Range(0, lista.Count)];
    }
    void TocarAnimacaoTextoFase()
    {
        if (animatorTextoFase == null)
            return;

        animatorTextoFase.ResetTrigger(triggerAnimacaoTextoFase);
        animatorTextoFase.SetTrigger(triggerAnimacaoTextoFase);
    }

    void IniciarFase(FaseProfissional fase)
    {
        if (rotinaDigitacaoSelecaoFase != null)
            StopCoroutine(rotinaDigitacaoSelecaoFase);

        faseAtual = fase;

        comunicacao = 0;
        trabalhoEquipe = 0;
        resolucaoProblemas = 0;
        adaptabilidade = 0;
        empatia = 0;

        pontosFaseAtual = 0;
        pontosMaximosFase = TOTAL_PERGUNTAS_POR_FASE * 2;
        porcentagemFase = 0;

        ultimaRespostaJogador = "";
        ultimaReacaoNPC = "";
        ultimoTomEscolhido = TomResposta.Neutra;
        totalEscolhasBoas = 0;
        totalEscolhasMedias = 0;
        totalEscolhasRuins = 0;
        sequenciaEscolhasRuins = 0;
        ruinsComunicacao = 0;
        ruinsTrabalhoEquipe = 0;
        ruinsResolucaoProblemas = 0;
        ruinsAdaptabilidade = 0;
        ruinsEmpatia = 0;
        ultimaCategoriaRuim = CategoriaSoftSkill.Comunicacao;
        exibindoReacaoEscolha = false;
        aguardandoResultadoFase = false;
        proximoNoAposReacao = -1;
        finalizarDepoisResultado = false;
        feedbackAguardandoContinuar = false;
        opcaoAguardandoFeedback = null;
        ultimaEmocaoPersonagem = Emocao.Neutro;
        emocaoAtualJogador = Emocao.Neutro;

        if (fundo != null) fundo.SetActive(true);
        if (imagemFundo != null && fundoTrabalhoTI != null) imagemFundo.sprite = fundoTrabalhoTI;

        if (painelSelecaoFase != null) painelSelecaoFase.SetActive(false);
        if (painelTopo != null) painelTopo.SetActive(true);
        if (painelDialogo != null) painelDialogo.SetActive(true);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(false);
        if (textoClimaEquipe != null) textoClimaEquipe.gameObject.SetActive(true);
        if (botaoVoltarSelecaoFase != null) botaoVoltarSelecaoFase.gameObject.SetActive(true);

        AtualizarTextoFase();
        TocarAnimacaoTextoFase();
        AtualizarMedidor();
        AtualizarClimaEquipe();
        TocarMusicaDaFase();

        MontarRoteiroDaFase();

        indiceNoAtual = 0;

        // Agora a transição acontece em TODAS as fases:
        // Fácil, Média e Difícil.
        if (usarTransicaoAoComecarPrimeiraFase)
            StartCoroutine(TransicaoInicioPrimeiraFase());
        else
            MostrarNoAtual();
    }

    IEnumerator TransicaoInicioPrimeiraFase()
    {
        if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);

        PrepararImagemTransicao(true, 0f);

        yield return FadeTransicao(0f, 1f, duracaoFadeEntrada);
        yield return new WaitForSeconds(tempoTelaPreta);

        MostrarNoAtual();

        yield return FadeTransicao(1f, 0f, duracaoFadeSaida);
        PrepararImagemTransicao(false, 0f);
    }

    IEnumerator FadeTransicao(float alphaInicial, float alphaFinal, float duracao)
    {
        if (imagemTransicaoPreta == null)
            yield break;

        if (duracao <= 0f)
        {
            DefinirAlphaTransicao(alphaFinal);
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracao);
            DefinirAlphaTransicao(Mathf.Lerp(alphaInicial, alphaFinal, t));
            yield return null;
        }

        DefinirAlphaTransicao(alphaFinal);
    }

    void PrepararImagemTransicao(bool ativa, float alpha)
    {
        if (imagemTransicaoPreta == null)
            return;

        imagemTransicaoPreta.gameObject.SetActive(ativa);
        DefinirAlphaTransicao(alpha);
    }

    void DefinirAlphaTransicao(float alpha)
    {
        if (imagemTransicaoPreta == null)
            return;

        Color cor = imagemTransicaoPreta.color;
        cor.a = alpha;
        imagemTransicaoPreta.color = cor;
    }

    void AtualizarTextoFase()
    {
        if (textoFase == null)
            return;

        textoFase.text = NomeFase(faseAtual);
    }

    string NomeFase(FaseProfissional fase)
    {
        switch (fase)
        {
            case FaseProfissional.FacilJunior:
                return "1ª Fase - Rápido (Júnior de TI)";

            case FaseProfissional.MedioPleno:
                return "2ª Fase - Intermediário (Pleno de TI)";

            case FaseProfissional.DificilSenior:
                return "3ª Fase - Avançado (Sênior de TI)";

            default:
                return "";
        }
    }

    float PorcentagemNecessaria(FaseProfissional fase)
    {
        switch (fase)
        {
            case FaseProfissional.FacilJunior:
                return 50f;

            case FaseProfissional.MedioPleno:
                return 60f;

            case FaseProfissional.DificilSenior:
                return 70f;

            default:
                return 0f;
        }
    }

    void AtualizarMedidor()
    {
        int necessario = Mathf.Max(1, AcertosNecessarios(faseAtual));
        float valor = Mathf.Clamp01((float)totalEscolhasBoas / necessario);

        if (medidorAprovacao != null)
            medidorAprovacao.value = valor;

        if (textoMedidorAprovacao != null)
            textoMedidorAprovacao.text = "Acertos: " + totalEscolhasBoas + " / " + necessario;
    }


    void AtualizarClimaEquipe()
    {
        if (textoClimaEquipe == null)
            return;

        int total = totalEscolhasBoas + totalEscolhasMedias + totalEscolhasRuins;
        float percentualRuim = total > 0 ? (float)totalEscolhasRuins / total : 0f;
        int limiteGameOver = Mathf.Max(2, quantidadeRespostasRuinsSeguidasParaGameOver);
        int limiteAlerta = Mathf.Clamp(sequenciaRuimParaMostrarAlerta, 1, limiteGameOver - 1);

        string nomeFaseCurta = "Equipe";

        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                nomeFaseCurta = "Equipe júnior";
                break;
            case FaseProfissional.MedioPleno:
                nomeFaseCurta = "Equipe pleno";
                break;
            case FaseProfissional.DificilSenior:
                nomeFaseCurta = "Equipe sênior";
                break;
        }

        if (sequenciaEscolhasRuins >= limiteAlerta || percentualRuim >= 0.45f)
        {
            textoClimaEquipe.text = nomeFaseCurta + ": clima pesado. Os NPCs estão perdendo confiança nas suas decisões.";
            return;
        }

        if (total == 0)
        {
            textoClimaEquipe.text = nomeFaseCurta + ": observando sua postura.";
            return;
        }

        if (totalEscolhasBoas > totalEscolhasRuins && sequenciaEscolhasRuins == 0)
        {
            textoClimaEquipe.text = nomeFaseCurta + ": confiança aumentando. Suas decisões estão ajudando o time.";
            return;
        }

        textoClimaEquipe.text = nomeFaseCurta + ": clima instável. Algumas escolhas ajudaram, mas outras deixaram dúvidas.";
    }

    void TocarMusica(AudioClip musica)
    {
        if (fonteAudio == null || musica == null)
            return;

        if (fonteAudio.clip == musica && fonteAudio.isPlaying)
            return;

        fonteAudio.Stop();
        fonteAudio.clip = musica;
        fonteAudio.Play();
    }

    void TocarMusicaDaFase()
    {
        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                TocarMusica(musicaFaseFacil);
                break;

            case FaseProfissional.MedioPleno:
                TocarMusica(musicaFaseMedia);
                break;

            case FaseProfissional.DificilSenior:
                TocarMusica(musicaFaseDificil);
                break;
        }
    }

    void MontarRoteiroDaFase()
    {
        nos.Clear();

        List<QuestaoTI> questoes = new List<QuestaoTI>();

        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                questoes = CriarQuestoesJunior();
                break;

            case FaseProfissional.MedioPleno:
                questoes = CriarQuestoesPleno();
                break;

            case FaseProfissional.DificilSenior:
                questoes = CriarQuestoesSenior();
                break;
        }

        for (int i = 0; i < questoes.Count; i++)
        {
            QuestaoTI q = questoes[i];

            nos.Add(new NoDialogoVN
            {
                id = i,
                tipoNo = TipoNoDialogo.Escolha,

                personagemFalando = q.npc,

                personagemEsquerda = q.esquerda,
                personagemCentro = q.centro,
                personagemDireita = q.direita,

                emocaoEsquerda = q.npc == q.esquerda ? q.emocaoNPC : Emocao.Neutro,
                emocaoCentro = q.npc == q.centro ? q.emocaoNPC : Emocao.Neutro,
                emocaoDireita = q.npc == q.direita ? q.emocaoNPC : Emocao.Neutro,

                emocaoJogadorDuranteNo = q.emocaoJogadorAoOuvir,

                falasVariaveis = new List<string> { q.falaNPC },

                opcoes = new List<OpcaoEscolha>
                {
                    CriarOpcaoBoa(q.categoria, q.botaoBom, q.respostaBoa, q.reacaoBoa, i + 1),
                    CriarOpcaoNeutra(q.categoria, q.botaoMedio, q.respostaMedia, q.reacaoMedia, i + 1),
                    CriarOpcaoRuim(q.categoria, q.botaoRuim, q.respostaRuim, q.reacaoRuim, i + 1)
                }
            });

            if (i == questoes.Count - 1)
            {
                foreach (OpcaoEscolha opcao in nos[i].opcoes)
                    opcao.proximoNo = -1;
            }
        }
    }

    QuestaoTI Q(
        CategoriaSoftSkill categoria,
        DadosPersonagem npc,
        DadosPersonagem esquerda,
        DadosPersonagem centro,
        DadosPersonagem direita,
        Emocao emocaoNPC,
        Emocao emocaoJogadorAoOuvir,
        string falaNPC,
        string botaoBom,
        string respostaBoa,
        string reacaoBoa,
        string botaoMedio,
        string respostaMedia,
        string reacaoMedia,
        string botaoRuim,
        string respostaRuim,
        string reacaoRuim)
    {
        return new QuestaoTI
        {
            categoria = categoria,
            npc = npc,
            esquerda = esquerda,
            centro = centro,
            direita = direita,
            emocaoNPC = emocaoNPC,
            emocaoJogadorAoOuvir = emocaoJogadorAoOuvir,
            falaNPC = falaNPC,
            botaoBom = botaoBom,
            respostaBoa = respostaBoa,
            reacaoBoa = reacaoBoa,
            botaoMedio = botaoMedio,
            respostaMedia = respostaMedia,
            reacaoMedia = reacaoMedia,
            botaoRuim = botaoRuim,
            respostaRuim = respostaRuim,
            reacaoRuim = reacaoRuim
        };
    }

    List<QuestaoTI> CriarQuestoesJunior()
    {
        return CriarQuestoesHumanizadas(FaseProfissional.FacilJunior, personagensJunior);
    }

    List<QuestaoTI> CriarQuestoesPleno()
    {
        return CriarQuestoesHumanizadas(FaseProfissional.MedioPleno, personagensPleno);
    }

    List<QuestaoTI> CriarQuestoesSenior()
    {
        return CriarQuestoesHumanizadas(FaseProfissional.DificilSenior, personagensSenior);
    }

    List<QuestaoTI> CriarQuestoesHumanizadas(FaseProfissional fase, List<DadosPersonagem> personagens)
    {
        List<QuestaoTI> questoes = new List<QuestaoTI>();

        CategoriaSoftSkill[] categorias =
        {
            CategoriaSoftSkill.Comunicacao,
            CategoriaSoftSkill.TrabalhoEquipe,
            CategoriaSoftSkill.ResolucaoProblemas,
            CategoriaSoftSkill.Adaptabilidade,
            CategoriaSoftSkill.Empatia
        };

        int[] ordemFalantes = CriarOrdemFalantesDaFase();

        for (int i = 0; i < TOTAL_PERGUNTAS_POR_FASE; i++)
        {
            CategoriaSoftSkill categoria = categorias[i % categorias.Length];
            DadosPersonagem npc = personagens[ordemFalantes[i % ordemFalantes.Length]];

            questoes.Add(Q(
                categoria,
                npc,
                personagens[0],
                personagens[1],
                personagens[2],
                EscolherEmocaoNPCDaPergunta(fase, categoria),
                EscolherEmocaoJogadorAoOuvir(fase, categoria),
                CriarFalaNPC(fase, categoria, npc, i),
                CriarTextoBotaoBom(fase, categoria, i),
                CriarRespostaBoa(fase, categoria, i),
                CriarReacaoBoa(fase, categoria, npc, i),
                CriarTextoBotaoMedio(fase, categoria, i),
                CriarRespostaMedia(fase, categoria, i),
                CriarReacaoMedia(fase, categoria, npc, i),
                CriarTextoBotaoRuim(fase, categoria, i),
                CriarRespostaRuim(fase, categoria, i),
                CriarReacaoRuim(fase, categoria, npc, i)
            ));
        }

        return questoes;
    }

    int[] CriarOrdemFalantesDaFase()
    {
        // A ordem cria pequenos blocos de conversa contínua.
        // Exemplo: NPC 0 fala duas vezes, depois NPC 1 continua o problema,
        // depois NPC 2 entra com outra visão. Isso evita parecer uma pergunta isolada.
        return new int[]
        {
            0, 0, 1, 1, 2, 2,
            0, 1, 1, 2, 2, 0,
            0, 1, 2, 2, 1, 0,
            1, 1, 0, 2, 2, 0
        };
    }

    Emocao EscolherEmocaoNPCDaPergunta(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
        {
            if (categoria == CategoriaSoftSkill.TrabalhoEquipe || categoria == CategoriaSoftSkill.Empatia)
                return Emocao.Raiva;

            return Emocao.Neutro;
        }

        if (fase == FaseProfissional.MedioPleno)
        {
            if (categoria == CategoriaSoftSkill.Empatia || categoria == CategoriaSoftSkill.TrabalhoEquipe)
                return Emocao.Raiva;

            return Emocao.Neutro;
        }

        if (categoria == CategoriaSoftSkill.Empatia)
            return Emocao.Raiva;

        return Emocao.Neutro;
    }

    Emocao EscolherEmocaoJogadorAoOuvir(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        return Emocao.Neutro;
    }


    private class DialogoInfo
    {
        public string fala;
        public string softSkills;

        public string botaoBom;
        public string respostaBoa;
        public string reacaoBoa;

        public string botaoMedio;
        public string respostaMedia;
        public string reacaoMedia;

        public string botaoRuim;
        public string respostaRuim;
        public string reacaoRuim;
    }

    DialogoInfo D(
        string fala,
        string softSkills,
        string botaoBom,
        string respostaBoa,
        string reacaoBoa,
        string botaoMedio,
        string respostaMedia,
        string reacaoMedia,
        string botaoRuim,
        string respostaRuim,
        string reacaoRuim)
    {
        return new DialogoInfo
        {
            fala = fala,
            softSkills = softSkills,
            botaoBom = botaoBom,
            respostaBoa = respostaBoa,
            reacaoBoa = reacaoBoa,
            botaoMedio = botaoMedio,
            respostaMedia = respostaMedia,
            reacaoMedia = reacaoMedia,
            botaoRuim = botaoRuim,
            respostaRuim = respostaRuim,
            reacaoRuim = reacaoRuim
        };
    }

    DialogoInfo ObterDialogoInfo(FaseProfissional fase, int indice)
    {
        DialogoInfo[] dialogos;

        if (fase == FaseProfissional.FacilJunior)
            dialogos = DialogosJunior();
        else if (fase == FaseProfissional.MedioPleno)
            dialogos = DialogosPleno();
        else
            dialogos = DialogosSenior();

        if (dialogos == null || dialogos.Length == 0)
            return D("Temos uma situação para resolver.", "Comunicação", "Vou agir com calma.", "Vou analisar a situação com calma.", "Boa postura.", "Vou tentar resolver.", "Vou tentar resolver do jeito possível.", "Resposta parcial.", "Vou fazer do meu jeito.", "Vou fazer do meu jeito.", "Essa postura pode prejudicar a equipe.");

        return dialogos[Mathf.Abs(indice) % dialogos.Length];
    }

    string CriarFalaNPC(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        return ObterDialogoInfo(fase, indice).fala;
    }

    string CriarTextoBotaoBom(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).botaoBom;
    }

    string CriarTextoBotaoMedio(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).botaoMedio;
    }

    string CriarTextoBotaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).botaoRuim;
    }

    string CriarRespostaBoa(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).respostaBoa;
    }

    string CriarRespostaMedia(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).respostaMedia;
    }

    string CriarRespostaRuim(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return ObterDialogoInfo(fase, indice).respostaRuim;
    }

    string CriarReacaoBoa(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        return ObterDialogoInfo(fase, indice).reacaoBoa;
    }

    string CriarReacaoMedia(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        return ObterDialogoInfo(fase, indice).reacaoMedia;
    }

    string CriarReacaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        return ObterDialogoInfo(fase, indice).reacaoRuim;
    }

    string ObterSoftSkillsDialogo(FaseProfissional fase, int indice)
    {
        return ObterDialogoInfo(fase, indice).softSkills;
    }

    DialogoInfo[] DialogosJunior()
    {
        return new DialogoInfo[]
        {
            D("Esse card veio incompleto. Antes de começar a codar, vamos confirmar o que realmente foi pedido.", "Comunicação clara; análise de requisitos", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você trabalhou Comunicação clara; análise de requisitos sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação clara; análise de requisitos ficou sem ser plenamente trabalhada.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação clara; análise de requisitos e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O QA apontou duas divergências em relação ao Jira. Ainda dá para corrigir sem impacto, mas, se deixarmos passar, isso vira retrabalho.", "Atenção aos detalhes; alinhamento", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você trabalhou Atenção aos detalhes; alinhamento sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Atenção aos detalhes; alinhamento ficou sem ser plenamente trabalhada.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Atenção aos detalhes; alinhamento e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Você acabou de entrar, então não precisa fingir que já entendeu tudo. O importante é perguntar cedo e não travar a entrega.", "Segurança para pedir ajuda; autoconfiança", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você trabalhou Segurança para pedir ajuda; autoconfiança sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Segurança para pedir ajuda; autoconfiança ficou sem ser plenamente trabalhada.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Segurança para pedir ajuda; autoconfiança e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O PR voltou com comentários simples, mas alguns mudam o comportamento da tela. A resposta precisa ser técnica e respeitosa, não defensiva.", "Receptividade a feedback; comunicação respeitosa", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você trabalhou Receptividade a feedback; comunicação respeitosa sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Receptividade a feedback; comunicação respeitosa ficou sem ser plenamente trabalhada.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Receptividade a feedback; comunicação respeitosa e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O Produto pediu uma mudança pequena no meio da sprint. No papel parece simples, mas no código ela mexe em mais coisa do que parece.", "Adaptabilidade; visão de impacto", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa escolha. Você trabalhou Adaptabilidade; visão de impacto sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Adaptabilidade; visão de impacto ficou sem ser plenamente trabalhada.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Adaptabilidade; visão de impacto e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem alguém do time querendo ajudar, mas a agenda está cheia. Se for pedir apoio, chega com contexto, não só com 'não funciona'.", "Colaboração; pedir ajuda com contexto", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você trabalhou Colaboração; pedir ajuda com contexto sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Colaboração; pedir ajuda com contexto ficou sem ser plenamente trabalhada.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Colaboração; pedir ajuda com contexto e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Esse bug não derruba o sistema, mas bloqueia o QA. Se a gente tratar como detalhe, todo o fluxo para.", "Priorização; senso de urgência", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você trabalhou Priorização; senso de urgência sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Priorização; senso de urgência ficou sem ser plenamente trabalhada.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Priorização; senso de urgência e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O backend diz que a regra está certa, mas a tela mostra outra coisa. Antes de apontar erro, vamos juntar as peças.", "Escuta ativa; comunicação entre áreas", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você trabalhou Escuta ativa; comunicação entre áreas sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Escuta ativa; comunicação entre áreas ficou sem ser plenamente trabalhada.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Escuta ativa; comunicação entre áreas e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A daily começa já. Se você disser só 'estou fazendo', ninguém vai entender o risco real da entrega.", "Comunicação assertiva; transparência", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você trabalhou Comunicação assertiva; transparência sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação assertiva; transparência ficou sem ser plenamente trabalhada.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação assertiva; transparência e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Dá para resolver sem drama, mas precisamos falar com clareza. O problema é pequeno; o ruído ao redor pode crescer.", "Clareza; gestão de ruído", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa escolha. Você trabalhou Clareza; gestão de ruído sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Clareza; gestão de ruído ficou sem ser plenamente trabalhada.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Clareza; gestão de ruído e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O card foi escrito rápido demais e deixou interpretação para o time. Vamos ajustar isso antes que cada um siga por um caminho.", "Alinhamento de expectativa; organização", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você trabalhou Alinhamento de expectativa; organização sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Alinhamento de expectativa; organização ficou sem ser plenamente trabalhada.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Alinhamento de expectativa; organização e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Vi que você tentou resolver sozinho. A intenção é boa, mas ficar muito tempo em silêncio pode passar a impressão errada.", "Autonomia; comunicação", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você trabalhou Autonomia; comunicação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Autonomia; comunicação ficou sem ser plenamente trabalhada.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Autonomia; comunicação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O comentário no PR não foi bronca; faz parte do processo. A forma como você responde também comunica bastante.", "Receptividade a feedback; maturidade profissional", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você trabalhou Receptividade a feedback; maturidade profissional sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Receptividade a feedback; maturidade profissional ficou sem ser plenamente trabalhada.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Receptividade a feedback; maturidade profissional e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Uma mudança de prioridade está chegando. Não é para abandonar tudo, mas também não dá para agir como se nada tivesse mudado.", "Adaptabilidade; gestão de mudança", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você trabalhou Adaptabilidade; gestão de mudança sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Adaptabilidade; gestão de mudança ficou sem ser plenamente trabalhada.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Adaptabilidade; gestão de mudança e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O QA está pressionado para fechar os testes hoje. Se respondermos de forma dura, a conversa vira conflito.", "Empatia; resolução de conflitos", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa escolha. Você trabalhou Empatia; resolução de conflitos sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Empatia; resolução de conflitos ficou sem ser plenamente trabalhada.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Empatia; resolução de conflitos e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A tarefa parece simples, mas há uma regra de negócio escondida ali. Melhor confirmar agora do que descobrir depois da entrega.", "Validação; pensamento crítico", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você trabalhou Validação; pensamento crítico sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Validação; pensamento crítico ficou sem ser plenamente trabalhada.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Validação; pensamento crítico e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Ainda não está claro se isso é bug ou requisito mal explicado. Sua resposta pode organizar a conversa.", "Mediação; comunicação", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você trabalhou Mediação; comunicação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Mediação; comunicação ficou sem ser plenamente trabalhada.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Mediação; comunicação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Quem abriu o card não está online agora. Mesmo assim, precisamos registrar o que falta para ninguém se perder.", "Documentação; organização", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você trabalhou Documentação; organização sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Documentação; organização ficou sem ser plenamente trabalhada.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Documentação; organização e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Ninguém espera que você resolva tudo sozinho. Esperamos que você saiba apontar a dúvida e o que já tentou.", "Autonomia; comunicação objetiva", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você trabalhou Autonomia; comunicação objetiva sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Autonomia; comunicação objetiva ficou sem ser plenamente trabalhada.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Autonomia; comunicação objetiva e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A alteração parece pequena, mas sem teste pode quebrar outra parte. Vamos pensar antes de correr.", "Cautela; gestão de risco", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa escolha. Você trabalhou Cautela; gestão de risco sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Cautela; gestão de risco ficou sem ser plenamente trabalhada.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Cautela; gestão de risco e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem gente falando por mensagem, no Jira e no PR. Se ninguém organizar, isso vira bagunça.", "Organização; coordenação", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você trabalhou Organização; coordenação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Organização; coordenação ficou sem ser plenamente trabalhada.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Organização; coordenação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O prazo está apertado, mas ainda dá para salvar a entrega. Só não dá para trabalhar no escuro.", "Foco; gestão de prazo", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você trabalhou Foco; gestão de prazo sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Foco; gestão de prazo ficou sem ser plenamente trabalhada.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Foco; gestão de prazo e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Você vai perceber que desenvolvimento não é só código. Boa parte do trabalho é alinhar expectativa.", "Alinhamento de expectativa; visão sistêmica", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você trabalhou Alinhamento de expectativa; visão sistêmica sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Alinhamento de expectativa; visão sistêmica ficou sem ser plenamente trabalhada.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Alinhamento de expectativa; visão sistêmica e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Antes de fechar a task, precisamos garantir que todo mundo está falando da mesma coisa. Senão o erro volta.", "Consistência; checagem de entendimento", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você trabalhou Consistência; checagem de entendimento sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Consistência; checagem de entendimento ficou sem ser plenamente trabalhada.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Consistência; checagem de entendimento e aumenta risco de ruído, retrabalho ou perda de confiança.")
        };
    }

    DialogoInfo[] DialogosPleno()
    {
        return new DialogoInfo[]
        {
            D("A sprint já começou atrasada e agora frontend, backend e QA estão defendendo versões diferentes do mesmo problema. Precisamos colocar ordem nisso.", "Mediação; negociação", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você trabalhou Mediação; negociação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Mediação; negociação ficou sem ser plenamente trabalhada.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Mediação; negociação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Produto mudou o requisito de novo. Entendo a urgência, mas, sem discutir impacto, a sprint quebra de vez.", "Gestão de mudanças; análise de impacto", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você trabalhou Gestão de mudanças; análise de impacto sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Gestão de mudanças; análise de impacto ficou sem ser plenamente trabalhada.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Gestão de mudanças; análise de impacto e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O QA diz que avisou desse risco ontem, e o dev diz que a regra não estava clara. Agora os dois lados estão irritados.", "Comunicação não defensiva; resolução de conflitos", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você trabalhou Comunicação não defensiva; resolução de conflitos sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação não defensiva; resolução de conflitos ficou sem ser plenamente trabalhada.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação não defensiva; resolução de conflitos e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Todo mundo sabe que essa refatoração é necessária, mas ela sempre perde espaço para a urgência. Hoje ela voltou a travar a entrega.", "Priorização; visão de longo prazo", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você trabalhou Priorização; visão de longo prazo sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Priorização; visão de longo prazo ficou sem ser plenamente trabalhada.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Priorização; visão de longo prazo e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Você não está mais só recebendo tarefa. O time espera que você ajude a traduzir o problema entre as áreas.", "Tradução entre áreas; maturidade", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa escolha. Você trabalhou Tradução entre áreas; maturidade sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Tradução entre áreas; maturidade ficou sem ser plenamente trabalhada.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Tradução entre áreas; maturidade e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A discussão começou técnica e já virou pessoal. Se continuarmos assim, ninguém vai ouvir a solução.", "Controle emocional; desescalada", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você trabalhou Controle emocional; desescalada sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Controle emocional; desescalada ficou sem ser plenamente trabalhada.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Controle emocional; desescalada e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A demanda do cliente importa, mas a dívida técnica também cobra juros. Precisamos decidir o que cabe sem criar um problema maior.", "Negociação; priorização técnica", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você trabalhou Negociação; priorização técnica sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Negociação; priorização técnica ficou sem ser plenamente trabalhada.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Negociação; priorização técnica e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O time quer fechar a sprint, mas tem gente seguindo prioridade antiga porque ninguém atualizou o combinado.", "Alinhamento de combinados; responsabilidade coletiva", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você trabalhou Alinhamento de combinados; responsabilidade coletiva sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Alinhamento de combinados; responsabilidade coletiva ficou sem ser plenamente trabalhada.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Alinhamento de combinados; responsabilidade coletiva e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O PR virou debate. Tem comentário útil ali, mas também tem resposta atravessada. Precisamos baixar a temperatura.", "Feedback construtivo; equilíbrio", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você trabalhou Feedback construtivo; equilíbrio sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Feedback construtivo; equilíbrio ficou sem ser plenamente trabalhada.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Feedback construtivo; equilíbrio e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Produto quer resposta rápida, QA quer segurança e desenvolvimento quer tempo. Nenhum lado está totalmente errado.", "Reconhecimento de perspectivas; equilíbrio", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa escolha. Você trabalhou Reconhecimento de perspectivas; equilíbrio sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Reconhecimento de perspectivas; equilíbrio ficou sem ser plenamente trabalhada.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Reconhecimento de perspectivas; equilíbrio e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Se você só executar a tarefa, talvez entregue. Se alinhar o impacto, talvez evite o mesmo problema na semana que vem.", "Pensamento estratégico; visão de impacto", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você trabalhou Pensamento estratégico; visão de impacto sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Pensamento estratégico; visão de impacto ficou sem ser plenamente trabalhada.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Pensamento estratégico; visão de impacto e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A pessoa mais nova do time pegou uma parte difícil e está claramente perdida. Ela não pediu ajuda, mas o atraso já apareceu.", "Apoio ao colega; empatia", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você trabalhou Apoio ao colega; empatia sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Apoio ao colega; empatia ficou sem ser plenamente trabalhada.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Apoio ao colega; empatia e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A reunião está virando disputa de culpa. Quero que alguém traga a conversa de volta para fatos e próximos passos.", "Mediação; foco em fatos", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você trabalhou Mediação; foco em fatos sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Mediação; foco em fatos ficou sem ser plenamente trabalhada.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Mediação; foco em fatos e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A mudança parece pequena para o Produto, mas toca num fluxo antigo. Se dissermos só 'não dá', eles não vão entender.", "Comunicação com stakeholder; clareza", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você trabalhou Comunicação com stakeholder; clareza sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação com stakeholder; clareza ficou sem ser plenamente trabalhada.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação com stakeholder; clareza e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O QA achou um comportamento diferente do esperado, mas o requisito está ambíguo. Aqui não adianta ganhar discussão; precisamos fechar entendimento.", "Fechamento de entendimento; negociação", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa escolha. Você trabalhou Fechamento de entendimento; negociação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Fechamento de entendimento; negociação ficou sem ser plenamente trabalhada.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Fechamento de entendimento; negociação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Você conhece essa parte do sistema melhor do que quase todo mundo. Por isso, sua forma de falar pode acalmar ou incendiar o time.", "Influência; comunicação assertiva", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você trabalhou Influência; comunicação assertiva sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Influência; comunicação assertiva ficou sem ser plenamente trabalhada.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Influência; comunicação assertiva e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A sprint não vai caber do jeito que está. Alguém vai precisar negociar escopo sem transformar isso em guerra.", "Negociação de escopo; colaboração", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você trabalhou Negociação de escopo; colaboração sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Negociação de escopo; colaboração ficou sem ser plenamente trabalhada.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Negociação de escopo; colaboração e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O legado está limitando a entrega, mas mexer nele agora tem risco. A decisão precisa ser madura, não só rápida.", "Tomada de decisão; análise de risco", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você trabalhou Tomada de decisão; análise de risco sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Tomada de decisão; análise de risco ficou sem ser plenamente trabalhada.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Tomada de decisão; análise de risco e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem alguém sobrecarregado cobrindo duas frentes. Se fingirmos que está tudo normal, a qualidade vai cair.", "Empatia; gestão de carga", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você trabalhou Empatia; gestão de carga sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Empatia; gestão de carga ficou sem ser plenamente trabalhada.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Empatia; gestão de carga e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A liderança quer saber o que está travando a entrega. Se a resposta sair mal construída, parece desculpa em vez de diagnóstico.", "Diagnóstico; comunicação clara", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa escolha. Você trabalhou Diagnóstico; comunicação clara sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Diagnóstico; comunicação clara ficou sem ser plenamente trabalhada.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Diagnóstico; comunicação clara e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A equipe precisa de uma decisão, mas uma decisão apressada pode custar caro. Vamos separar urgência de impulso.", "Discernimento; tomada de decisão", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você trabalhou Discernimento; tomada de decisão sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Discernimento; tomada de decisão ficou sem ser plenamente trabalhada.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Discernimento; tomada de decisão e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O conflito entre dev e QA está escondendo o principal: ninguém fechou o critério de aceite.", "Critérios de aceite; resolução de conflitos", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você trabalhou Critérios de aceite; resolução de conflitos sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Critérios de aceite; resolução de conflitos ficou sem ser plenamente trabalhada.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Critérios de aceite; resolução de conflitos e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Agora eu preciso que você pense como pleno: entrega, pessoas e consequência. Não dá para olhar só para a parte técnica.", "Visão plena; responsabilidade", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você trabalhou Visão plena; responsabilidade sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Visão plena; responsabilidade ficou sem ser plenamente trabalhada.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Visão plena; responsabilidade e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Ainda dá para recuperar a sprint, mas não se cada pessoa continuar protegendo só a própria parte.", "Colaboração; trabalho em equipe", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você trabalhou Colaboração; trabalho em equipe sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Colaboração; trabalho em equipe ficou sem ser plenamente trabalhada.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Colaboração; trabalho em equipe e aumenta risco de ruído, retrabalho ou perda de confiança.")
        };
    }

    DialogoInfo[] DialogosSenior()
    {
        return new DialogoInfo[]
        {
            D("O incidente em produção já afetou o cliente e a diretoria quer uma previsão. O time está olhando para você porque alguém precisa organizar a resposta.", "Liderança; comunicação em crise", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você trabalhou Liderança; comunicação em crise sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Liderança; comunicação em crise ficou sem ser plenamente trabalhada.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Liderança; comunicação em crise e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A correção rápida existe, mas pode mascarar a causa real. Se aplicarmos agora, talvez estabilize; se falhar, a confiança cai mais.", "Gestão de risco; análise de causa", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você trabalhou Gestão de risco; análise de causa sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Gestão de risco; análise de causa ficou sem ser plenamente trabalhada.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Gestão de risco; análise de causa e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A war room está aberta há horas. Tem gente cansada, cliente cobrando e liderança pedindo uma explicação que ainda não temos completa.", "Resiliência; comunicação sob pressão", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você trabalhou Resiliência; comunicação sob pressão sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Resiliência; comunicação sob pressão ficou sem ser plenamente trabalhada.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Resiliência; comunicação sob pressão e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Dois especialistas discordam sobre a arquitetura. Os dois têm bons argumentos, mas a decisão não pode virar disputa de ego.", "Debate respeitoso; escuta técnica", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você trabalhou Debate respeitoso; escuta técnica sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Debate respeitoso; escuta técnica ficou sem ser plenamente trabalhada.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Debate respeitoso; escuta técnica e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O cliente quer saber quando volta, a equipe quer tempo para investigar e a diretoria quer uma mensagem segura. Nada disso pode ser tratado separado.", "Gestão de stakeholders; negociação", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa escolha. Você trabalhou Gestão de stakeholders; negociação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Gestão de stakeholders; negociação ficou sem ser plenamente trabalhada.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Gestão de stakeholders; negociação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Alguém deixou passar um alerta importante, mas caçar culpado agora só vai fazer as pessoas esconderem informação. Precisamos resolver e aprender.", "Cultura sem culpa; aprendizado contínuo", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você trabalhou Cultura sem culpa; aprendizado contínuo sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Cultura sem culpa; aprendizado contínuo ficou sem ser plenamente trabalhada.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Cultura sem culpa; aprendizado contínuo e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A estabilidade está no limite. Se mexermos demais, piora; se mexermos de menos, o cliente continua parado.", "Gestão de risco; estabilidade", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você trabalhou Gestão de risco; estabilidade sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Gestão de risco; estabilidade ficou sem ser plenamente trabalhada.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Gestão de risco; estabilidade e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem um dev segurando a bronca desde a madrugada. Ele está exausto e já começou a errar em coisas simples. Isso também é risco técnico.", "Empatia; segurança psicológica", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você trabalhou Empatia; segurança psicológica sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Empatia; segurança psicológica ficou sem ser plenamente trabalhada.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Empatia; segurança psicológica e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A decisão de arquitetura, que parecia distante, virou problema de produção hoje. Agora precisamos escolher um caminho sem romantizar a solução perfeita.", "Pensamento crítico; decisão de arquitetura", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você trabalhou Pensamento crítico; decisão de arquitetura sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Pensamento crítico; decisão de arquitetura ficou sem ser plenamente trabalhada.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Pensamento crítico; decisão de arquitetura e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A comunicação externa precisa ser honesta, mas não pode jogar a equipe na fogueira. O cliente precisa de clareza, não de pânico.", "Comunicação honesta; responsabilidade", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa escolha. Você trabalhou Comunicação honesta; responsabilidade sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação honesta; responsabilidade ficou sem ser plenamente trabalhada.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação honesta; responsabilidade e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O time está esperando uma direção. Se você hesitar demais, cada um vai agir por conta própria.", "Liderança; direcionamento", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você trabalhou Liderança; direcionamento sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Liderança; direcionamento ficou sem ser plenamente trabalhada.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Liderança; direcionamento e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O rollback resolve parte do impacto, mas joga fora trabalho importante. Manter a versão atual exige confiança numa correção que ainda não foi validada.", "Análise de trade-offs; tomada de decisão", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você trabalhou Análise de trade-offs; tomada de decisão sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Análise de trade-offs; tomada de decisão ficou sem ser plenamente trabalhada.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Análise de trade-offs; tomada de decisão e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A liderança quer um responsável pelo incidente. Eu prefiro sair daqui com causa, plano e prevenção, mas a pressão por culpado está crescendo.", "Responsabilidade; foco em causa raiz", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você trabalhou Responsabilidade; foco em causa raiz sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Responsabilidade; foco em causa raiz ficou sem ser plenamente trabalhada.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Responsabilidade; foco em causa raiz e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem gente experiente se atacando porque todo mundo está sob pressão. Se isso continuar, a crise técnica vira crise de equipe.", "Gestão de conflito; maturidade emocional", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você trabalhou Gestão de conflito; maturidade emocional sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Gestão de conflito; maturidade emocional ficou sem ser plenamente trabalhada.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Gestão de conflito; maturidade emocional e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O cliente percebeu inconsistência nos dados. Mesmo que o erro seja pequeno, a confiança já foi afetada.", "Transparência; construção de confiança", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa escolha. Você trabalhou Transparência; construção de confiança sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Transparência; construção de confiança ficou sem ser plenamente trabalhada.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Transparência; construção de confiança e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A equipe precisa saber o que comunicar no próximo status report. Silêncio parece omissão; detalhe demais pode virar alarme.", "Comunicação de crise; prudência", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você trabalhou Comunicação de crise; prudência sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação de crise; prudência ficou sem ser plenamente trabalhada.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação de crise; prudência e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A solução definitiva exige tempo que talvez não tenhamos. O contorno rápido exige risco que talvez não possamos assumir.", "Priorização; gestão de incerteza", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você trabalhou Priorização; gestão de incerteza sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Priorização; gestão de incerteza ficou sem ser plenamente trabalhada.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Priorização; gestão de incerteza e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Alguém reconheceu um erro no privado, mas está com medo de falar na reunião. A verdade importa, mas a forma como a gente recebe isso também.", "Segurança psicológica; confiança", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você trabalhou Segurança psicológica; confiança sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Segurança psicológica; confiança ficou sem ser plenamente trabalhada.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Segurança psicológica; confiança e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A diretoria quer garantia, mas garantia absoluta agora seria mentira.", "Honestidade; ética", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você trabalhou Honestidade; ética sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Honestidade; ética ficou sem ser plenamente trabalhada.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Honestidade; ética e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O cliente quer uma data, a engenharia quer mais diagnóstico e o Produto quer manter o compromisso comercial. Você precisa equilibrar essas forças.", "Equilíbrio de interesses; negociação", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa escolha. Você trabalhou Equilíbrio de interesses; negociação sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Equilíbrio de interesses; negociação ficou sem ser plenamente trabalhada.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Equilíbrio de interesses; negociação e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("O sistema voltou parcialmente, mas ainda está instável. Se comemorarmos cedo demais, podemos perder credibilidade.", "Prudência; gestão de credibilidade", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você trabalhou Prudência; gestão de credibilidade sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Prudência; gestão de credibilidade ficou sem ser plenamente trabalhada.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Prudência; gestão de credibilidade e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A decisão técnica de agora vai virar precedente. O time aprende com o que você tolera em crise.", "Liderança pelo exemplo; cultura", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você trabalhou Liderança pelo exemplo; cultura sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Liderança pelo exemplo; cultura ficou sem ser plenamente trabalhada.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Liderança pelo exemplo; cultura e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("Tem uma reunião com stakeholders em poucos minutos. Precisamos transformar o caos técnico em uma mensagem responsável.", "Comunicação com stakeholders; síntese", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você trabalhou Comunicação com stakeholders; síntese sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Comunicação com stakeholders; síntese ficou sem ser plenamente trabalhada.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Comunicação com stakeholders; síntese e aumenta risco de ruído, retrabalho ou perda de confiança."),
            D("A crise está quase controlada, mas o pós-incidente vai definir se isso vira aprendizado ou só mais uma cicatriz na equipe.", "Aprendizado contínuo; retrospectiva", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você trabalhou Aprendizado contínuo; retrospectiva sem tratar a situação como simples demais. A resposta ajuda a criar clareza e reduz risco para a equipe.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente adequada. Existe um caminho útil na sua decisão, mas parte de Aprendizado contínuo; retrospectiva ficou sem ser plenamente trabalhada.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Resposta inadequada. A decisão até pode parecer prática no curto prazo, mas enfraquece Aprendizado contínuo; retrospectiva e aumenta risco de ruído, retrabalho ou perda de confiança.")
        };
    }

    OpcaoEscolha CriarOpcaoBoa(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Boa,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 2,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? 2 : 1,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? 2 : 1,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? 2 : 1,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? 2 : 1,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? 2 : 1,

            emocaoJogadorAposEscolha = Emocao.Feliz,
            emocaoPersonagemAposEscolha = Emocao.Feliz,

            proximoNo = proximoNo
        };
    }

    OpcaoEscolha CriarOpcaoNeutra(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Neutra,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 1,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? 1 : 0,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? 1 : 0,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? 1 : 0,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? 1 : 0,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? 1 : 0,

            emocaoJogadorAposEscolha = Emocao.Neutro,
            emocaoPersonagemAposEscolha = Emocao.Neutro,

            proximoNo = proximoNo
        };
    }

    OpcaoEscolha CriarOpcaoRuim(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Rude,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 0,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? -1 : 0,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? -1 : 0,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? -1 : 0,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? -1 : 0,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? -1 : 0,

            emocaoJogadorAposEscolha = Emocao.Raiva,
            emocaoPersonagemAposEscolha = Emocao.Raiva,

            proximoNo = proximoNo
        };
    }

    string AplicarConsequenciaDaUltimaEscolha(string falaOriginal, NoDialogoVN noAtual)
    {
        if (indiceNoAtual <= 0 || string.IsNullOrWhiteSpace(falaOriginal))
            return falaOriginal;

        string complemento = "";

        if (ultimoTomEscolhido == TomResposta.Boa)
        {
            if (faseAtual == FaseProfissional.FacilJunior)
                complemento = "\n\nPelo menos o último alinhamento ajudou a evitar um pouco de retrabalho. Agora precisamos manter esse cuidado.";
            else if (faseAtual == FaseProfissional.MedioPleno)
                complemento = "\n\nA forma como você conduziu a conversa anterior acalmou parte do time, então dá pra avançar com menos ruído.";
            else
                complemento = "\n\nSua última decisão deu um pouco mais de confiança pra equipe, mas a crise ainda não acabou.";
        }
        else if (ultimoTomEscolhido == TomResposta.Neutra)
        {
            if (faseAtual == FaseProfissional.FacilJunior)
                complemento = "\n\nA situação anterior não piorou, mas também não ficou totalmente clara. Isso ainda pode voltar pra gente.";
            else if (faseAtual == FaseProfissional.MedioPleno)
                complemento = "\n\nA escolha anterior resolveu parte do incêndio, só que o desalinhamento continua aparecendo nas bordas.";
            else
                complemento = "\n\nO contorno anterior comprou tempo, mas liderança e cliente ainda esperam uma resposta mais firme.";
        }
        else if (ultimoTomEscolhido == TomResposta.Rude)
        {
            if (faseAtual == FaseProfissional.FacilJunior)
                complemento = "\n\nDepois da última resposta, o clima ficou um pouco mais pesado. Agora qualquer fala atravessada pode virar problema.";
            else if (faseAtual == FaseProfissional.MedioPleno)
                complemento = "\n\nA conversa anterior deixou algumas pessoas na defensiva. Antes de resolver o técnico, talvez seja preciso recuperar confiança.";
            else
                complemento = "\n\nA última decisão aumentou a tensão na sala. A equipe está mais calada, e isso pode atrapalhar a crise.";
        }

        if (sequenciaEscolhasRuins >= 2)
        {
            complemento += "\n\nE sendo bem direto: a sequência de decisões está fazendo o time confiar menos na sua condução.";
        }

        return falaOriginal + complemento;
    }

    void MostrarNoAtual()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
        {
            MostrarResultadoFase();
            return;
        }

        NoDialogoVN noAtual = nos[indiceNoAtual];

        string falaNPC;
        string falaJogador;

        if (exibindoReacaoEscolha)
        {
            falaJogador = ultimaRespostaJogador;
            falaNPC = ultimaReacaoNPC;
        }
        else
        {
            falaNPC = EscolherTextoAleatorio(noAtual.falasVariaveis);
            falaNPC = AplicarConsequenciaDaUltimaEscolha(falaNPC, noAtual);
            falaJogador = EscolherTextoAleatorio(noAtual.respostasJogadorVariaveis);
        }

        textoCompletoNPC = falaNPC;
        textoCompletoJogador = falaJogador;

        bool npcTemFala = !string.IsNullOrWhiteSpace(falaNPC);
        bool jogadorTemFala = !string.IsNullOrWhiteSpace(falaJogador);

        if (caixaNomeNPC != null) caixaNomeNPC.SetActive(npcTemFala);
        if (textoFalaNPC != null) textoFalaNPC.gameObject.SetActive(npcTemFala);

        if (caixaNomeJogador != null) caixaNomeJogador.SetActive(jogadorTemFala);
        if (textoFalaJogador != null) textoFalaJogador.gameObject.SetActive(jogadorTemFala);

        if (textoNomeNPC != null && npcTemFala)
            textoNomeNPC.text = noAtual.personagemFalando != null ? noAtual.personagemFalando.nomePersonagem : "NPC";

        if (textoNomeJogador != null && jogadorTemFala)
            textoNomeJogador.text = nomeJogador;

        Emocao emocaoEsquerdaFinal = noAtual.emocaoEsquerda;
        Emocao emocaoCentroFinal = noAtual.emocaoCentro;
        Emocao emocaoDireitaFinal = noAtual.emocaoDireita;

        if (exibindoReacaoEscolha && noAtual.personagemFalando != null)
        {
            if (noAtual.personagemFalando == noAtual.personagemEsquerda)
                emocaoEsquerdaFinal = ultimaEmocaoPersonagem;

            if (noAtual.personagemFalando == noAtual.personagemCentro)
                emocaoCentroFinal = ultimaEmocaoPersonagem;

            if (noAtual.personagemFalando == noAtual.personagemDireita)
                emocaoDireitaFinal = ultimaEmocaoPersonagem;
        }
        else
        {
            emocaoAtualJogador = noAtual.emocaoJogadorDuranteNo;
        }

        if (controladorCena != null)
        {
            controladorCena.AtualizarPersonagem(controladorCena.imagemEsquerda, noAtual.personagemEsquerda, emocaoEsquerdaFinal, noAtual.mostrarEsquerda);
            controladorCena.AtualizarPersonagem(controladorCena.imagemCentro, noAtual.personagemCentro, emocaoCentroFinal, noAtual.mostrarCentro);
            controladorCena.AtualizarPersonagem(controladorCena.imagemDireita, noAtual.personagemDireita, emocaoDireitaFinal, noAtual.mostrarDireita);
            controladorCena.AtualizarJogador(aparenciaAtualJogador, emocaoAtualJogador);

            controladorCena.DestacarFalante(
                noAtual.personagemFalando,
                noAtual.personagemEsquerda,
                noAtual.personagemCentro,
                noAtual.personagemDireita
            );

            if (npcTemFala)
            {
                controladorCena.AnimarFalante(
                    noAtual.personagemFalando,
                    noAtual.personagemEsquerda,
                    noAtual.personagemCentro,
                    noAtual.personagemDireita
                );
            }
        }

        if (painelEscolhas != null) painelEscolhas.SetActive(false);

        if (botaoContinuar != null)
            botaoContinuar.gameObject.SetActive(true);

        IniciarDigitacao(falaJogador, falaNPC, jogadorTemFala, npcTemFala);
    }

    void IniciarDigitacao(string falaJogador, string falaNPC, bool jogadorTemFala, bool npcTemFala)
    {
        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        rotinaDigitacao = StartCoroutine(DigitarTextos(falaJogador, falaNPC, jogadorTemFala, npcTemFala));
    }

    IEnumerator DigitarTextos(string falaJogador, string falaNPC, bool jogadorTemFala, bool npcTemFala)
    {
        textoDigitando = true;

        if (textoFalaJogador != null)
            textoFalaJogador.text = "";

        if (textoFalaNPC != null)
            textoFalaNPC.text = "";

        if (jogadorTemFala)
        {
            for (int i = 0; i <= falaJogador.Length; i++)
            {
                if (textoFalaJogador != null)
                    textoFalaJogador.text = falaJogador.Substring(0, i);

                yield return new WaitForSeconds(velocidadeDigitacao);
            }
        }

        if (npcTemFala)
        {
            for (int i = 0; i <= falaNPC.Length; i++)
            {
                if (textoFalaNPC != null)
                    textoFalaNPC.text = falaNPC.Substring(0, i);

                yield return new WaitForSeconds(velocidadeDigitacao);
            }
        }

        textoDigitando = false;
        FinalizarExibicaoDoNo();
    }

    void FinalizarDigitacaoImediata()
    {
        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        if (textoFalaJogador != null)
            textoFalaJogador.text = textoCompletoJogador;

        if (textoFalaNPC != null)
            textoFalaNPC.text = textoCompletoNPC;

        textoDigitando = false;
        FinalizarExibicaoDoNo();
    }

    void FinalizarExibicaoDoNo()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
            return;

        NoDialogoVN noAtual = nos[indiceNoAtual];

        if (exibindoConclusaoFase)
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(false);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);
            return;
        }

        if (exibindoReacaoEscolha)
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(false);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);
            return;
        }

        if (noAtual.tipoNo == TipoNoDialogo.DialogoSimples)
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(false);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);
        }
        else
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(true);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(false);

            List<OpcaoEscolha> opcoesEmbaralhadas = EmbaralharOpcoes(noAtual.opcoes);

            ConfigurarBotaoEscolha(botaoEscolha1, textoEscolha1, opcoesEmbaralhadas, 0);
            ConfigurarBotaoEscolha(botaoEscolha2, textoEscolha2, opcoesEmbaralhadas, 1);
            ConfigurarBotaoEscolha(botaoEscolha3, textoEscolha3, opcoesEmbaralhadas, 2);
        }
    }

    List<OpcaoEscolha> EmbaralharOpcoes(List<OpcaoEscolha> opcoesOriginais)
    {
        List<OpcaoEscolha> lista = new List<OpcaoEscolha>();

        if (opcoesOriginais != null)
            lista.AddRange(opcoesOriginais);

        for (int i = 0; i < lista.Count; i++)
        {
            int indiceAleatorio = Random.Range(i, lista.Count);
            OpcaoEscolha temporaria = lista[i];
            lista[i] = lista[indiceAleatorio];
            lista[indiceAleatorio] = temporaria;
        }

        return lista;
    }

    void ConfigurarBotaoEscolha(Button botao, TMP_Text texto, List<OpcaoEscolha> opcoes, int indice)
    {
        if (botao == null || texto == null)
            return;

        if (opcoes == null || indice >= opcoes.Count)
        {
            botao.gameObject.SetActive(false);
            return;
        }

        botao.gameObject.SetActive(true);

        // Mostra o número da tecla correspondente ao botão.
        // Como as opções já são embaralhadas antes, o [1], [2] e [3]
        // representam apenas a posição atual do botão, não a qualidade da resposta.
        int numeroBotao = indice + 1;
        texto.text = "<color=#6CC6FF>[" + numeroBotao + "]</color> " + opcoes[indice].textoOpcao;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => EscolherOpcao(opcoes[indice]));
    }

    void ContinuarDialogo()
    {
        if (textoDigitando)
        {
            FinalizarDigitacaoImediata();
            return;
        }

        if (exibindoConclusaoFase)
        {
            exibindoConclusaoFase = false;
            MostrarResultadoFase();
            return;
        }

        if (exibindoReacaoEscolha)
        {
            exibindoReacaoEscolha = false;

            if (faseConcluidaPorAcertos)
            {
                faseConcluidaPorAcertos = false;
                exibindoConclusaoFase = true;
                MostrarConclusaoFasePorAcertos();
                return;
            }

            if (aguardandoResultadoFase)
            {
                aguardandoResultadoFase = false;
                MostrarResultadoFase();
                return;
            }

            indiceNoAtual = proximoNoAposReacao;
            MostrarNoAtual();
            return;
        }

        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
            return;

        indiceNoAtual = nos[indiceNoAtual].proximoNoSimples;
        MostrarNoAtual();
    }

    void AbrirPainelFeedback(OpcaoEscolha opcao)
    {
        opcaoAguardandoFeedback = opcao;
        feedbackAguardandoContinuar = true;

        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(true);

        if (textoFeedback != null)
            textoFeedback.text = GerarTextoFeedback(opcao);
    }

    void ContinuarDepoisFeedback()
    {
        if (!feedbackAguardandoContinuar || opcaoAguardandoFeedback == null)
            return;

        OpcaoEscolha opcao = opcaoAguardandoFeedback;
        opcaoAguardandoFeedback = null;
        feedbackAguardandoContinuar = false;

        if (painelFeedback != null) painelFeedback.SetActive(false);

        if (DeveAtivarGameOver())
        {
            MostrarGameOverDemissao();
            return;
        }

        exibindoReacaoEscolha = true;

        bool venceuFaseAgora = totalEscolhasBoas >= AcertosNecessarios(faseAtual);

        if (venceuFaseAgora)
        {
            faseConcluidaPorAcertos = true;
            aguardandoResultadoFase = false;
            proximoNoAposReacao = -1;
        }
        else if (opcao.proximoNo == -1 || opcao.proximoNo >= nos.Count)
        {
            aguardandoResultadoFase = true;
            proximoNoAposReacao = -1;
        }
        else
        {
            aguardandoResultadoFase = false;
            proximoNoAposReacao = opcao.proximoNo;
        }

        MostrarNoAtual();
    }

    string GerarTextoFeedback(OpcaoEscolha opcao)
    {
        string titulo;

        if (opcao.tomResposta == TomResposta.Boa)
            titulo = "Resposta adequada";
        else if (opcao.tomResposta == TomResposta.Neutra)
            titulo = "Resposta parcialmente adequada";
        else
            titulo = "Resposta inadequada";

        return titulo + "\n\n" +
            opcao.reacaoNPC + "\n\n" +
            ExplicarDesempenhoDaResposta(opcao) + "\n\n" +
            "Competência trabalhada: " + NomeCategoria(opcao.categoria) + ".";
    }

    string ExplicarDesempenhoDaResposta(OpcaoEscolha opcao)
    {
        if (opcao.tomResposta == TomResposta.Boa)
        {
            switch (opcao.categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return "Você acertou porque priorizou clareza, alinhamento e registro das informações antes que o problema virasse retrabalho.";
                case CategoriaSoftSkill.TrabalhoEquipe:
                    return "Você acertou porque tratou a situação como um problema coletivo, sem empurrar culpa para outra pessoa.";
                case CategoriaSoftSkill.ResolucaoProblemas:
                    return "Você acertou porque investigou a causa e reduziu risco antes de tentar uma solução apressada.";
                case CategoriaSoftSkill.Adaptabilidade:
                    return "Você acertou porque aceitou reorganizar o plano sem ignorar impacto, prazo e qualidade.";
                case CategoriaSoftSkill.Empatia:
                    return "Você acertou porque considerou o estado da equipe e respondeu sem aumentar a pressão do ambiente.";
            }
        }

        if (opcao.tomResposta == TomResposta.Neutra)
        {
            switch (opcao.categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return "Sua resposta resolveu parte do momento, mas ainda deixou informações importantes sem alinhamento claro.";
                case CategoriaSoftSkill.TrabalhoEquipe:
                    return "Sua resposta evitou conflito direto, mas não ajudou muito a equipe a destravar o problema em conjunto.";
                case CategoriaSoftSkill.ResolucaoProblemas:
                    return "Sua resposta buscou resolver rápido, mas poderia investigar melhor para evitar que o erro volte depois.";
                case CategoriaSoftSkill.Adaptabilidade:
                    return "Sua resposta aceitou a mudança, mas sem reorganizar completamente as prioridades e consequências.";
                case CategoriaSoftSkill.Empatia:
                    return "Sua resposta manteve certa neutralidade, mas poderia acolher melhor as pessoas envolvidas.";
            }
        }

        switch (opcao.categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "Você errou porque sua resposta gerou ruído, escondeu contexto ou deixou a equipe sem informação suficiente.";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "Você errou porque sua resposta aumentou a distância entre as pessoas e enfraqueceu a colaboração.";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "Você errou porque escolheu agir por impulso, sem análise suficiente do problema e dos riscos.";
            case CategoriaSoftSkill.Adaptabilidade:
                return "Você errou porque resistiu à mudança e dificultou a reorganização necessária para seguir o trabalho.";
            case CategoriaSoftSkill.Empatia:
                return "Você errou porque ignorou o impacto emocional e profissional nas pessoas envolvidas.";
        }

        return "Sua escolha afetou diretamente a confiança dos NPCs na sua postura profissional.";
    }

    string NomeCategoria(CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "Comunicação";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "Trabalho em equipe";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "Resolução de problemas";
            case CategoriaSoftSkill.Adaptabilidade:
                return "Adaptabilidade";
            case CategoriaSoftSkill.Empatia:
                return "Empatia";
            default:
                return "Soft skill";
        }
    }

    void EscolherOpcao(OpcaoEscolha opcao)
    {
        pontosFaseAtual += opcao.pontosAprovacao;

        comunicacao += opcao.deltaComunicacao;
        trabalhoEquipe += opcao.deltaTrabalhoEquipe;
        resolucaoProblemas += opcao.deltaResolucaoProblemas;
        adaptabilidade += opcao.deltaAdaptabilidade;
        empatia += opcao.deltaEmpatia;

        emocaoAtualJogador = opcao.emocaoJogadorAposEscolha;
        ultimaEmocaoPersonagem = opcao.emocaoPersonagemAposEscolha;

        ultimoTomEscolhido = opcao.tomResposta;

        if (opcao.tomResposta == TomResposta.Boa)
        {
            totalEscolhasBoas++;
            sequenciaEscolhasRuins = 0;
        }
        else if (opcao.tomResposta == TomResposta.Neutra)
        {
            totalEscolhasMedias++;
            sequenciaEscolhasRuins = 0;
        }
        else
        {
            totalEscolhasRuins++;
            sequenciaEscolhasRuins++;
            RegistrarRespostaRuim(opcao.categoria);
        }

        AtualizarMedidor();
        AtualizarClimaEquipe();

        ultimaRespostaJogador = opcao.respostaJogador;
        ultimaReacaoNPC = opcao.reacaoNPC;

        AbrirPainelFeedback(opcao);
    }

    void RegistrarRespostaRuim(CategoriaSoftSkill categoria)
    {
        ultimaCategoriaRuim = categoria;

        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                ruinsComunicacao++;
                break;

            case CategoriaSoftSkill.TrabalhoEquipe:
                ruinsTrabalhoEquipe++;
                break;

            case CategoriaSoftSkill.ResolucaoProblemas:
                ruinsResolucaoProblemas++;
                break;

            case CategoriaSoftSkill.Adaptabilidade:
                ruinsAdaptabilidade++;
                break;

            case CategoriaSoftSkill.Empatia:
                ruinsEmpatia++;
                break;
        }
    }

    bool DeveAtivarGameOver()
    {
        int limite = Mathf.Max(2, quantidadeRespostasRuinsSeguidasParaGameOver);
        return sequenciaEscolhasRuins >= limite;
    }

    void MostrarGameOverDemissao()
    {
        faseDoGameOver = faseAtual;

        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        textoDigitando = false;
        exibindoReacaoEscolha = false;
        aguardandoResultadoFase = false;

        if (painelInicio != null) painelInicio.SetActive(false);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);
        if (painelTopo != null) painelTopo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (painelSelecaoFase != null) painelSelecaoFase.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(false);
        if (textoClimaEquipe != null) textoClimaEquipe.gameObject.SetActive(false);
        if (botaoVoltarSelecaoFase != null) botaoVoltarSelecaoFase.gameObject.SetActive(false);
        if (painelGameOver != null) painelGameOver.SetActive(true);

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        if (textoGameOver != null)
        {
            textoGameOver.text =
                "Demissão\n\n" +
                nomeJogador + " foi desligado da equipe.\n\n" +
                GerarMotivoDemissao() + "\n\n" +
                "Última resposta dada:\n\"" + ultimaRespostaJogador + "\"\n\n" +
                "O problema não foi apenas uma escolha ruim isolada, mas uma sequência de decisões que afetou a confiança da equipe.";
        }
    }

    string GerarMotivoDemissao()
    {
        CategoriaSoftSkill motivoPrincipal = ObterCategoriaComMaisFalhas();
        string contextoFase = ContextoDaFaseParaDemissao();

        switch (motivoPrincipal)
        {
            case CategoriaSoftSkill.Comunicacao:
                return contextoFase + " A demissão aconteceu porque sua comunicação começou a gerar ruídos constantes: informações importantes ficaram mal explicadas, dúvidas foram escondidas e a equipe passou a tomar decisões sem clareza.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                return contextoFase + " A demissão aconteceu porque suas escolhas quebraram a colaboração do time. Em vez de ajudar a resolver o problema, suas atitudes aumentaram o atrito e fizeram outras pessoas perderem confiança no trabalho em conjunto.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                return contextoFase + " A demissão aconteceu porque você insistiu em decisões impulsivas diante dos problemas. A equipe precisava de análise, investigação e cuidado técnico, mas suas respostas colocaram a entrega em risco.";

            case CategoriaSoftSkill.Adaptabilidade:
                return contextoFase + " A demissão aconteceu porque você resistiu às mudanças necessárias. A equipe precisava se reorganizar diante da situação, mas suas decisões travaram o andamento e dificultaram a adaptação do projeto.";

            case CategoriaSoftSkill.Empatia:
                return contextoFase + " A demissão aconteceu porque suas respostas ignoraram o impacto nas pessoas. O clima da equipe piorou, colegas se sentiram desrespeitados e a liderança entendeu que sua postura estava prejudicando o ambiente profissional.";
        }

        return contextoFase + " A demissão aconteceu porque suas decisões prejudicaram a confiança da equipe e comprometeram o andamento do projeto.";
    }

    CategoriaSoftSkill ObterCategoriaComMaisFalhas()
    {
        CategoriaSoftSkill categoria = ultimaCategoriaRuim;
        int maior = -1;

        if (ruinsComunicacao > maior)
        {
            maior = ruinsComunicacao;
            categoria = CategoriaSoftSkill.Comunicacao;
        }

        if (ruinsTrabalhoEquipe > maior)
        {
            maior = ruinsTrabalhoEquipe;
            categoria = CategoriaSoftSkill.TrabalhoEquipe;
        }

        if (ruinsResolucaoProblemas > maior)
        {
            maior = ruinsResolucaoProblemas;
            categoria = CategoriaSoftSkill.ResolucaoProblemas;
        }

        if (ruinsAdaptabilidade > maior)
        {
            maior = ruinsAdaptabilidade;
            categoria = CategoriaSoftSkill.Adaptabilidade;
        }

        if (ruinsEmpatia > maior)
        {
            maior = ruinsEmpatia;
            categoria = CategoriaSoftSkill.Empatia;
        }

        return categoria;
    }

    string ContextoDaFaseParaDemissao()
    {
        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                return "Durante a fase Júnior, o time ainda tentava corrigir problemas simples de alinhamento, tarefas incompletas e retrabalho.";

            case FaseProfissional.MedioPleno:
                return "Durante a fase Pleno, a equipe enfrentava pressão de entrega, conflitos entre áreas e decisões que exigiam mais autonomia.";

            case FaseProfissional.DificilSenior:
                return "Durante a fase Sênior, a empresa estava lidando com uma crise séria, cliente impactado e necessidade de liderança madura.";
        }

        return "Durante a simulação, a equipe precisava de uma postura profissional mais consistente.";
    }

    void ResetarPontuacaoGeral()
    {
        comunicacao = 0;
        trabalhoEquipe = 0;
        resolucaoProblemas = 0;
        adaptabilidade = 0;
        empatia = 0;

        pontosFaseAtual = 0;
        pontosMaximosFase = TOTAL_PERGUNTAS_POR_FASE * 2;
        porcentagemFase = 0f;

        totalEscolhasBoas = 0;
        totalEscolhasMedias = 0;
        totalEscolhasRuins = 0;
        sequenciaEscolhasRuins = 0;
        ruinsComunicacao = 0;
        ruinsTrabalhoEquipe = 0;
        ruinsResolucaoProblemas = 0;
        ruinsAdaptabilidade = 0;
        ruinsEmpatia = 0;
        ultimaCategoriaRuim = CategoriaSoftSkill.Comunicacao;
        feedbackAguardandoContinuar = false;
        opcaoAguardandoFeedback = null;
    }

    void VoltarParaCriacaoPersonagem()
    {
        if (campoNome != null)
            campoNome.text = "";

        if (dropdownGenero != null)
            dropdownGenero.value = 0;

        ResetarPontuacaoGeral();
        AtivarEstadoInicial();
        TocarMusica(musicaInicio);
    }

    void ReiniciarFaseAtualAposGameOver()
    {
        // Reinicia exatamente a fase em que o jogador foi demitido.
        // Exemplo: se caiu na fase Pleno, volta para a fase Pleno; se caiu na Sênior, volta para a Sênior.
        IniciarFase(faseDoGameOver);
    }

    void ReiniciarFaseAtualPeloResultado()
    {
        if (painelResultadoFase != null)
            painelResultadoFase.SetActive(false);

        IniciarFase(faseAtual);
    }

    void MostrarConclusaoFasePorAcertos()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
        {
            MostrarResultadoFase();
            return;
        }

        NoDialogoVN noAtual = nos[indiceNoAtual];
        DadosPersonagem npcConclusao = EscolherNpcConclusaoFase(noAtual);
        string falaConclusao = CriarFalaConclusaoFase();

        textoCompletoNPC = falaConclusao;
        textoCompletoJogador = "";

        if (caixaNomeNPC != null) caixaNomeNPC.SetActive(true);
        if (textoFalaNPC != null) textoFalaNPC.gameObject.SetActive(true);

        if (caixaNomeJogador != null) caixaNomeJogador.SetActive(false);
        if (textoFalaJogador != null) textoFalaJogador.gameObject.SetActive(false);

        if (textoNomeNPC != null)
            textoNomeNPC.text = npcConclusao != null ? npcConclusao.nomePersonagem : "NPC";

        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);

        if (controladorCena != null)
        {
            Emocao emocaoEsquerdaFinal = noAtual.personagemEsquerda == npcConclusao ? Emocao.Feliz : Emocao.Neutro;
            Emocao emocaoCentroFinal = noAtual.personagemCentro == npcConclusao ? Emocao.Feliz : Emocao.Neutro;
            Emocao emocaoDireitaFinal = noAtual.personagemDireita == npcConclusao ? Emocao.Feliz : Emocao.Neutro;

            controladorCena.AtualizarPersonagem(controladorCena.imagemEsquerda, noAtual.personagemEsquerda, emocaoEsquerdaFinal, noAtual.mostrarEsquerda);
            controladorCena.AtualizarPersonagem(controladorCena.imagemCentro, noAtual.personagemCentro, emocaoCentroFinal, noAtual.mostrarCentro);
            controladorCena.AtualizarPersonagem(controladorCena.imagemDireita, noAtual.personagemDireita, emocaoDireitaFinal, noAtual.mostrarDireita);
            controladorCena.AtualizarJogador(aparenciaAtualJogador, Emocao.Feliz);

            controladorCena.DestacarFalante(
                npcConclusao,
                noAtual.personagemEsquerda,
                noAtual.personagemCentro,
                noAtual.personagemDireita
            );
        }

        IniciarDigitacao("", falaConclusao, false, true);
    }

    DadosPersonagem EscolherNpcConclusaoFase(NoDialogoVN noAtual)
    {
        if (noAtual == null)
            return null;

        if (noAtual.personagemFalando != noAtual.personagemCentro && noAtual.personagemCentro != null)
            return noAtual.personagemCentro;

        if (noAtual.personagemFalando != noAtual.personagemDireita && noAtual.personagemDireita != null)
            return noAtual.personagemDireita;

        if (noAtual.personagemFalando != noAtual.personagemEsquerda && noAtual.personagemEsquerda != null)
            return noAtual.personagemEsquerda;

        return noAtual.personagemFalando;
    }

    string CriarFalaConclusaoFase()
    {
        int necessario = AcertosNecessarios(faseAtual);

        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                return "Boa. Com essas decisões, conseguimos alinhar o card, destravar o QA e fechar o problema sem virar retrabalho. Você atingiu " + totalEscolhasBoas + " de " + necessario + " boas decisões nesta fase.";

            case FaseProfissional.MedioPleno:
                return "Certo, agora a situação está sob controle. O time conseguiu sair da discussão e transformar o conflito em próximos passos. Você atingiu " + totalEscolhasBoas + " de " + necessario + " boas decisões nesta fase.";

            case FaseProfissional.DificilSenior:
                return "Conseguimos estabilizar a crise e organizar uma resposta responsável para equipe, liderança e cliente. Você atingiu " + totalEscolhasBoas + " de " + necessario + " boas decisões nesta fase.";

            default:
                return "Conseguimos resolver os principais pontos desta fase. Você atingiu " + totalEscolhasBoas + " de " + necessario + " boas decisões.";
        }
    }

    void MostrarResultadoFase()
    {
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelFeedback != null) painelFeedback.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(true);

        int necessario = AcertosNecessarios(faseAtual);
        bool aprovado = totalEscolhasBoas >= necessario;
        string textoAprovacao = aprovado
            ? "Você venceu esta fase. A quantidade mínima de boas decisões foi atingida."
            : "Você ainda não venceu esta fase. Refaça a fase para tentar melhorar suas decisões.";

        finalizarDepoisResultado = false;

        if (textoResultadoFase != null)
        {
            textoResultadoFase.text =
                "Resultado da " + NomeFase(faseAtual) + "\n\n" +
                "Acertos necessários para vencer: " + necessario + "\n" +
                "Suas respostas adequadas: " + totalEscolhasBoas + "\n" +
                "Respostas parciais: " + totalEscolhasMedias + "\n" +
                "Respostas inadequadas: " + totalEscolhasRuins + "\n\n" +
                textoAprovacao + "\n\n" +
                "Comunicação: " + comunicacao + "\n" +
                "Trabalho em Equipe: " + trabalhoEquipe + "\n" +
                "Resolução de Problemas: " + resolucaoProblemas + "\n" +
                "Adaptabilidade: " + adaptabilidade + "\n" +
                "Empatia: " + empatia;
        }

        if (botaoContinuarFase != null)
        {
            botaoContinuarFase.gameObject.SetActive(aprovado);
            botaoContinuarFase.onClick.RemoveAllListeners();

            if (aprovado)
                botaoContinuarFase.onClick.AddListener(ContinuarDepoisResultadoFase);
        }

        if (botaoReiniciarFaseResultado != null)
        {
            botaoReiniciarFaseResultado.gameObject.SetActive(true);
            botaoReiniciarFaseResultado.onClick.RemoveAllListeners();
            botaoReiniciarFaseResultado.onClick.AddListener(ReiniciarFaseAtualPeloResultado);
        }
    }

    void ContinuarDepoisResultadoFase()
    {
        if (painelResultadoFase != null)
            painelResultadoFase.SetActive(false);

        MostrarSelecaoFase();
    }

    void MostrarFinal()
    {
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(true);

        if (textoFinal == null)
            return;

        textoFinal.text =
            "Resultado Final\n\n" +
            "Jogador: " + nomeJogador + "\n" +
            "Simulação concluída: Carreira em TI\n\n" +
            "Comunicação: " + comunicacao + "\n" +
            "Trabalho em Equipe: " + trabalhoEquipe + "\n" +
            "Resolução de Problemas: " + resolucaoProblemas + "\n" +
            "Adaptabilidade: " + adaptabilidade + "\n" +
            "Empatia: " + empatia + "\n\n" +
            "Perfil: " + GerarPerfil() + "\n\n" +
            "Áreas indicadas:\n" + GerarAreas();
    }

    string GerarPerfil()
    {
        if (comunicacao >= 25 && trabalhoEquipe >= 25 && empatia >= 20)
            return "Perfil colaborativo, comunicativo e preparado para atuar bem em equipes de TI.";

        if (resolucaoProblemas >= 25 && adaptabilidade >= 20)
            return "Perfil técnico forte, com boa capacidade de resolver problemas e se adaptar a mudanças.";

        if (trabalhoEquipe >= 25 && comunicacao >= 20)
            return "Perfil com potencial para liderança técnica, coordenação de equipe e mediação de conflitos.";

        if (empatia < 5 || comunicacao < 5)
            return "Perfil que precisa desenvolver melhor escuta, comunicação e inteligência emocional no ambiente profissional.";

        return "Perfil equilibrado, com competências socioemocionais em desenvolvimento.";
    }

    string GerarAreas()
    {
        List<string> areas = new List<string>();

        if (resolucaoProblemas >= 20)
            areas.Add("- Desenvolvimento de Software, Backend, Frontend, Full Stack");

        if (comunicacao >= 20)
            areas.Add("- Product Owner, Scrum Master, Suporte Técnico, Customer Success");

        if (trabalhoEquipe >= 20)
            areas.Add("- Squad de Desenvolvimento, Gestão Ágil, Coordenação de Projetos");

        if (adaptabilidade >= 20)
            areas.Add("- DevOps, SRE, Cloud, Sustentação de Sistemas");

        if (empatia >= 20)
            areas.Add("- Liderança Técnica, Mentoria, People Management, RH Tech");

        if (areas.Count == 0)
            areas.Add("- Áreas iniciais de TI com foco em desenvolvimento gradual de soft skills");

        return string.Join("\n", areas);
    }

    string EscolherTextoAleatorio(List<string> lista)
    {
        if (lista == null || lista.Count == 0)
            return "";

        return lista[Random.Range(0, lista.Count)];
    }

    void ReiniciarJogo()
    {
        if (fundo != null) fundo.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}