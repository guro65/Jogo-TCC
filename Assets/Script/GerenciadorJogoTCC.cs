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

    [Tooltip("Animator do objeto/painel Fundo. Use para ativar a animação no menu e desligar durante a gameplay.")]
    public Animator animatorFundo;

    [Tooltip("Se estiver marcado, o script ativa o Animator do fundo nos menus e desativa durante as fases.")]
    public bool controlarAnimatorFundo = true;

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

        // Detectar tecla ESC para voltar ao menu
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            VoltarParaSelecaoFase();
            return;
        }

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

    Animator ObterAnimatorFundo()
    {
        if (animatorFundo != null)
            return animatorFundo;

        if (fundo != null)
            return fundo.GetComponent<Animator>();

        return null;
    }

    void AtivarFundoAnimadoMenu()
    {
        if (fundo != null)
            fundo.SetActive(true);

        if (!controlarAnimatorFundo)
            return;

        Animator anim = ObterAnimatorFundo();

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play(0, 0, 0f);
        }
    }

    void AtivarFundoGameplay()
    {
        if (fundo != null)
            fundo.SetActive(true);

        if (controlarAnimatorFundo)
        {
            Animator anim = ObterAnimatorFundo();

            if (anim != null)
                anim.enabled = false;
        }

        if (imagemFundo != null && fundoTrabalhoTI != null)
            imagemFundo.sprite = fundoTrabalhoTI;
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
        AtivarFundoAnimadoMenu();
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

        AtivarFundoAnimadoMenu();

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

        AtivarFundoGameplay();

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
                return "1ª Fase - Fácil (Júnior de TI)";

            case FaseProfissional.MedioPleno:
                return "2ª Fase - Média (Pleno de TI)";

            case FaseProfissional.DificilSenior:
                return "3ª Fase - Difícil (Sênior de TI)";

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
            D("Esse card veio incompleto. Antes de começar a codar, vamos confirmar o que realmente foi pedido.", "Comunicação clara; análise de requisitos", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você demonstrou comunicação clara e análise de requisitos. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em comunicação clara e análise de requisitos. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem comunicação clara e análise de requisitos, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O QA apontou duas divergências em relação ao Jira. Ainda dá para corrigir sem impacto, mas, se deixarmos passar, isso vira retrabalho.", "Atenção aos detalhes; alinhamento", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa postura. Você demonstrou atenção aos detalhes e alinhamento. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em atenção aos detalhes e alinhamento. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Escolha inadequada. Ao não trabalhar bem atenção aos detalhes e alinhamento, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Você acabou de entrar, então não precisa fingir que já entendeu tudo. O importante é perguntar cedo e não travar a entrega.", "Segurança para pedir ajuda; autoconfiança", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Resposta adequada. Você demonstrou segurança para pedir ajuda e autoconfiança. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em segurança para pedir ajuda e autoconfiança. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem segurança para pedir ajuda e autoconfiança, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O PR voltou com comentários simples, mas alguns mudam o comportamento da tela. A resposta precisa ser técnica e respeitosa, não defensiva.", "Receptividade a feedback; comunicação respeitosa", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Ótima decisão. Você demonstrou receptividade a feedback e comunicação respeitosa. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em receptividade a feedback e comunicação respeitosa. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem receptividade a feedback e comunicação respeitosa, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O Produto pediu uma mudança pequena no meio da sprint. No papel parece simples, mas no código ela mexe em mais coisa do que parece.", "Adaptabilidade; visão de impacto", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa escolha. Você demonstrou adaptabilidade e visão de impacto. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em adaptabilidade e visão de impacto. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem adaptabilidade e visão de impacto, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Tem alguém do time querendo ajudar, mas a agenda está cheia. Se for pedir apoio, chega com contexto, não só com 'não funciona'.", "Colaboração; pedir ajuda com contexto", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa postura. Você demonstrou colaboração e pedir ajuda com contexto. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em colaboração e pedir ajuda com contexto. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Escolha inadequada. Ao não trabalhar bem colaboração e pedir ajuda com contexto, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Esse bug não derruba o sistema, mas bloqueia o QA. Se a gente tratar como detalhe, todo o fluxo para.", "Priorização; senso de urgência", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Resposta adequada. Você demonstrou priorização e senso de urgência. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em priorização e senso de urgência. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem priorização e senso de urgência, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O backend diz que a regra está certa, mas a tela mostra outra coisa. Antes de apontar erro, vamos juntar as peças.", "Escuta ativa; comunicação entre áreas", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Ótima decisão. Você demonstrou escuta ativa e comunicação entre áreas. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em escuta ativa e comunicação entre áreas. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem escuta ativa e comunicação entre áreas, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("A daily começa já. Se você disser só 'estou fazendo', ninguém vai entender o risco real da entrega.", "Comunicação assertiva; transparência", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa escolha. Você demonstrou comunicação assertiva e transparência. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em comunicação assertiva e transparência. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem comunicação assertiva e transparência, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Dá para resolver sem drama, mas precisamos falar com clareza. O problema é pequeno; o ruído ao redor pode crescer.", "Clareza; gestão de ruído", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Boa postura. Você demonstrou clareza e gestão de ruído. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em clareza e gestão de ruído. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Escolha inadequada. Ao não trabalhar bem clareza e gestão de ruído, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O card foi escrito rápido demais e deixou interpretação para o time. Vamos ajustar isso antes que cada um siga por um caminho.", "Alinhamento de expectativa; organização", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Resposta adequada. Você demonstrou alinhamento de expectativa e organização. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em alinhamento de expectativa e organização. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem alinhamento de expectativa e organização, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Vi que você tentou resolver sozinho. A intenção é boa, mas ficar muito tempo em silêncio pode passar a impressão errada.", "Autonomia; comunicação", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Ótima decisão. Você demonstrou autonomia e comunicação. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em autonomia e comunicação. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem autonomia e comunicação, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O comentário no PR não foi bronca; faz parte do processo. A forma como você responde também comunica bastante.", "Receptividade a feedback; maturidade profissional", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa escolha. Você demonstrou receptividade a feedback e maturidade profissional. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em receptividade a feedback e maturidade profissional. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem receptividade a feedback e maturidade profissional, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Uma mudança de prioridade está chegando. Não é para abandonar tudo, mas também não dá para agir como se nada tivesse mudado.", "Adaptabilidade; gestão de mudança", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Boa postura. Você demonstrou adaptabilidade e gestão de mudança. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em adaptabilidade e gestão de mudança. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Escolha inadequada. Ao não trabalhar bem adaptabilidade e gestão de mudança, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O QA está pressionado para fechar os testes hoje. Se respondermos de forma dura, a conversa vira conflito.", "Empatia; resolução de conflitos", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Resposta adequada. Você demonstrou empatia e resolução de conflitos. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em empatia e resolução de conflitos. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem empatia e resolução de conflitos, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("A tarefa parece simples, mas há uma regra de negócio escondida ali. Melhor confirmar agora do que descobrir depois da entrega.", "Validação; pensamento crítico", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Ótima decisão. Você demonstrou validação e pensamento crítico. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em validação e pensamento crítico. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem validação e pensamento crítico, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Ainda não está claro se isso é bug ou requisito mal explicado. Sua resposta pode organizar a conversa.", "Mediação; comunicação", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa escolha. Você demonstrou mediação e comunicação. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em mediação e comunicação. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem mediação e comunicação, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Quem abriu o card não está online agora. Mesmo assim, precisamos registrar o que falta para ninguém se perder.", "Documentação; organização", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Boa postura. Você demonstrou documentação e organização. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em documentação e organização. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Escolha inadequada. Ao não trabalhar bem documentação e organização, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Ninguém espera que você resolva tudo sozinho. Esperamos que você saiba apontar a dúvida e o que já tentou.", "Autonomia; comunicação objetiva", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Resposta adequada. Você demonstrou autonomia e comunicação objetiva. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em autonomia e comunicação objetiva. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem autonomia e comunicação objetiva, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("A alteração parece pequena, mas sem teste pode quebrar outra parte. Vamos pensar antes de correr.", "Cautela; gestão de risco", "Vejo o impacto antes de mexer.", "Vou entender quais partes podem ser afetadas antes de aceitar a mudança como se fosse simples.", "Ótima decisão. Você demonstrou cautela e gestão de risco. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Encaixo a mudança no fluxo.", "Vou tentar encaixar a mudança no fluxo atual sem mexer em mais partes do que o necessário.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em cautela e gestão de risco. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Mantenho o plano original.", "Como a mudança chegou no meio da sprint, talvez seja melhor manter o plano original por enquanto.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem cautela e gestão de risco, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Tem gente falando por mensagem, no Jira e no PR. Se ninguém organizar, isso vira bagunça.", "Organização; coordenação", "Antes, quero validar o combinado.", "Antes de seguir, eu quero validar o combinado com quem abriu a demanda, porque o risco é implementar algo diferente do esperado.", "Boa escolha. Você demonstrou organização e coordenação. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Vou seguir pelo que está mais claro.", "Vou começar pela parte que parece mais clara e deixar as dúvidas para confirmar quando alguém responder.", "Resposta parcialmente correta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em organização e coordenação. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Dá para decidir pelo card.", "Acho que dá para decidir pelo que já está no card e seguir, mesmo sem confirmar tudo agora.", "Essa atitude pode prejudicar a equipe. Ao não trabalhar bem organização e coordenação, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("O prazo está apertado, mas ainda dá para salvar a entrega. Só não dá para trabalhar no escuro.", "Foco; gestão de prazo", "Vou comparar card e teste.", "Vou comparar o que está no Jira com o que o QA encontrou e alinhar a diferença antes que vire retrabalho.", "Boa postura. Você demonstrou foco e gestão de prazo. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Ajusto os pontos marcados.", "Vou corrigir os pontos que o QA marcou primeiro e depois vejo se ainda ficou algo fora do combinado.", "Caminho razoável. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em foco e gestão de prazo. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Talvez não valha parar por isso.", "Talvez não valha parar a entrega por essas divergências se ainda dá para corrigir depois.", "Escolha inadequada. Ao não trabalhar bem foco e gestão de prazo, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Você vai perceber que desenvolvimento não é só código. Boa parte do trabalho é alinhar expectativa.", "Alinhamento de expectativa; visão sistêmica", "Vou mostrar onde travei.", "Vou mostrar o que entendi, o que tentei e exatamente onde travei, para pedir ajuda sem deixar a pessoa no escuro.", "Resposta adequada. Você demonstrou alinhamento de expectativa e visão sistêmica. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Tento mais um pouco sozinho.", "Vou tentar avançar mais um pouco sozinho para não interromper ninguém agora.", "Decisão aceitável, mas incompleta. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em alinhamento de expectativa e visão sistêmica. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Prefiro não envolver mais gente.", "Prefiro não envolver mais gente agora; posso tentar resolver sem abrir mais conversa.", "Essa decisão enfraquece a condução do problema. Ao não trabalhar bem alinhamento de expectativa e visão sistêmica, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe."),
            D("Antes de fechar a task, precisamos garantir que todo mundo está falando da mesma coisa. Senão o erro volta.", "Consistência; checagem de entendimento", "Respondo explicando o ajuste.", "Vou responder o comentário explicando o que vou ajustar e perguntando se há algum impacto que ainda não percebi.", "Ótima decisão. Você demonstrou consistência e checagem de entendimento. Sua decisão deixou a situação mais clara, reduziu o risco de retrabalho e ajudou o time a seguir com mais segurança.", "Respondo de forma objetiva.", "Vou responder só o necessário no PR e fazer o ajuste principal sem prolongar a conversa.", "Sua resposta teve pontos positivos. Você tentou seguir em frente, mas a resposta ainda deixou lacunas em consistência e checagem de entendimento. A equipe até conseguiria avançar, porém com risco de dúvidas voltarem depois.", "Vou defender minha implementação.", "Vou explicar por que fiz desse jeito, porque talvez o comentário não tenha considerado meu raciocínio.", "Cuidado: essa postura pode gerar consequência negativa. Ao não trabalhar bem consistência e checagem de entendimento, a situação pode virar ruído, atraso ou retrabalho. Mesmo em uma fase simples, ignorar alinhamento prejudica a confiança da equipe.")};
    }

    DialogoInfo[] DialogosPleno()
    {
        return new DialogoInfo[]
        {
            D("A sprint já começou atrasada e agora frontend, backend e QA estão defendendo versões diferentes do mesmo problema. Precisamos colocar ordem nisso.", "Mediação; negociação", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você demonstrou maturidade ao trabalhar mediação e negociação. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente mediação e negociação. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece mediação e negociação. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Produto mudou o requisito de novo. Entendo a urgência, mas, sem discutir impacto, a sprint quebra de vez.", "Gestão de mudanças; análise de impacto", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa postura. Você demonstrou maturidade ao trabalhar gestão de mudanças e análise de impacto. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente gestão de mudanças e análise de impacto. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece gestão de mudanças e análise de impacto. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O QA diz que avisou desse risco ontem, e o dev diz que a regra não estava clara. Agora os dois lados estão irritados.", "Comunicação não defensiva; resolução de conflitos", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Resposta adequada. Você demonstrou maturidade ao trabalhar comunicação não defensiva e resolução de conflitos. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente comunicação não defensiva e resolução de conflitos. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece comunicação não defensiva e resolução de conflitos. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Todo mundo sabe que essa refatoração é necessária, mas ela sempre perde espaço para a urgência. Hoje ela voltou a travar a entrega.", "Priorização; visão de longo prazo", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Ótima decisão. Você demonstrou maturidade ao trabalhar priorização e visão de longo prazo. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente priorização e visão de longo prazo. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece priorização e visão de longo prazo. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Você não está mais só recebendo tarefa. O time espera que você ajude a traduzir o problema entre as áreas.", "Tradução entre áreas; maturidade", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa escolha. Você demonstrou maturidade ao trabalhar tradução entre áreas e maturidade. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente tradução entre áreas e maturidade. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece tradução entre áreas e maturidade. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A discussão começou técnica e já virou pessoal. Se continuarmos assim, ninguém vai ouvir a solução.", "Controle emocional; desescalada", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa postura. Você demonstrou maturidade ao trabalhar controle emocional e desescalada. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente controle emocional e desescalada. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece controle emocional e desescalada. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A demanda do cliente importa, mas a dívida técnica também cobra juros. Precisamos decidir o que cabe sem criar um problema maior.", "Negociação; priorização técnica", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Resposta adequada. Você demonstrou maturidade ao trabalhar negociação e priorização técnica. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente negociação e priorização técnica. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece negociação e priorização técnica. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O time quer fechar a sprint, mas tem gente seguindo prioridade antiga porque ninguém atualizou o combinado.", "Alinhamento de combinados; responsabilidade coletiva", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Ótima decisão. Você demonstrou maturidade ao trabalhar alinhamento de combinados e responsabilidade coletiva. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente alinhamento de combinados e responsabilidade coletiva. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece alinhamento de combinados e responsabilidade coletiva. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O PR virou debate. Tem comentário útil ali, mas também tem resposta atravessada. Precisamos baixar a temperatura.", "Feedback construtivo; equilíbrio", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa escolha. Você demonstrou maturidade ao trabalhar feedback construtivo e equilíbrio. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente feedback construtivo e equilíbrio. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece feedback construtivo e equilíbrio. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Produto quer resposta rápida, QA quer segurança e desenvolvimento quer tempo. Nenhum lado está totalmente errado.", "Reconhecimento de perspectivas; equilíbrio", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Boa postura. Você demonstrou maturidade ao trabalhar reconhecimento de perspectivas e equilíbrio. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente reconhecimento de perspectivas e equilíbrio. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece reconhecimento de perspectivas e equilíbrio. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Se você só executar a tarefa, talvez entregue. Se alinhar o impacto, talvez evite o mesmo problema na semana que vem.", "Pensamento estratégico; visão de impacto", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Resposta adequada. Você demonstrou maturidade ao trabalhar pensamento estratégico e visão de impacto. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente pensamento estratégico e visão de impacto. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece pensamento estratégico e visão de impacto. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A pessoa mais nova do time pegou uma parte difícil e está claramente perdida. Ela não pediu ajuda, mas o atraso já apareceu.", "Apoio ao colega; empatia", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Ótima decisão. Você demonstrou maturidade ao trabalhar apoio ao colega e empatia. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente apoio ao colega e empatia. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece apoio ao colega e empatia. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A reunião está virando disputa de culpa. Quero que alguém traga a conversa de volta para fatos e próximos passos.", "Mediação; foco em fatos", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa escolha. Você demonstrou maturidade ao trabalhar mediação e foco em fatos. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente mediação e foco em fatos. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece mediação e foco em fatos. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A mudança parece pequena para o Produto, mas toca num fluxo antigo. Se dissermos só 'não dá', eles não vão entender.", "Comunicação com stakeholder; clareza", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Boa postura. Você demonstrou maturidade ao trabalhar comunicação com stakeholder e clareza. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente comunicação com stakeholder e clareza. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece comunicação com stakeholder e clareza. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O QA achou um comportamento diferente do esperado, mas o requisito está ambíguo. Aqui não adianta ganhar discussão; precisamos fechar entendimento.", "Fechamento de entendimento; negociação", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Resposta adequada. Você demonstrou maturidade ao trabalhar fechamento de entendimento e negociação. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente fechamento de entendimento e negociação. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece fechamento de entendimento e negociação. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Você conhece essa parte do sistema melhor do que quase todo mundo. Por isso, sua forma de falar pode acalmar ou incendiar o time.", "Influência; comunicação assertiva", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Ótima decisão. Você demonstrou maturidade ao trabalhar influência e comunicação assertiva. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente influência e comunicação assertiva. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece influência e comunicação assertiva. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A sprint não vai caber do jeito que está. Alguém vai precisar negociar escopo sem transformar isso em guerra.", "Negociação de escopo; colaboração", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa escolha. Você demonstrou maturidade ao trabalhar negociação de escopo e colaboração. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente negociação de escopo e colaboração. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece negociação de escopo e colaboração. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O legado está limitando a entrega, mas mexer nele agora tem risco. A decisão precisa ser madura, não só rápida.", "Tomada de decisão; análise de risco", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Boa postura. Você demonstrou maturidade ao trabalhar tomada de decisão e análise de risco. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente tomada de decisão e análise de risco. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece tomada de decisão e análise de risco. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Tem alguém sobrecarregado cobrindo duas frentes. Se fingirmos que está tudo normal, a qualidade vai cair.", "Empatia; gestão de carga", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Resposta adequada. Você demonstrou maturidade ao trabalhar empatia e gestão de carga. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente empatia e gestão de carga. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece empatia e gestão de carga. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A liderança quer saber o que está travando a entrega. Se a resposta sair mal construída, parece desculpa em vez de diagnóstico.", "Diagnóstico; comunicação clara", "Quero olhar carga e entrega juntos.", "Vou olhar a sobrecarga junto com a entrega, porque qualidade também depende de como o time está trabalhando.", "Ótima decisão. Você demonstrou maturidade ao trabalhar diagnóstico e comunicação clara. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Cobro sem expor demais.", "Vou cobrar a entrega com cuidado para não transformar a sobrecarga em desculpa ou exposição.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente diagnóstico e comunicação clara. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Se aceitou a tarefa, entrega.", "Se a pessoa aceitou a tarefa, precisa entregar ou avisar formalmente que não consegue.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece diagnóstico e comunicação clara. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("A equipe precisa de uma decisão, mas uma decisão apressada pode custar caro. Vamos separar urgência de impulso.", "Discernimento; tomada de decisão", "Quero fechar um entendimento comum.", "Vou juntar as áreas para fechar uma versão comum do problema, com impacto, responsável e próximo passo.", "Boa escolha. Você demonstrou maturidade ao trabalhar discernimento e tomada de decisão. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "A gente resolve o mais urgente.", "Vou resolver o que está travando a sprint agora e deixar a discussão mais profunda para depois.", "Resposta parcialmente correta. A decisão resolve parte do problema, mas ainda não trabalha completamente discernimento e tomada de decisão. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Cada área precisa sustentar seu ponto.", "Cada área precisa sustentar o que está dizendo; não dá para o time inteiro parar para conciliar tudo.", "Essa atitude pode prejudicar a equipe. A escolha pode parecer prática no curto prazo, mas enfraquece discernimento e tomada de decisão. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("O conflito entre dev e QA está escondendo o principal: ninguém fechou o critério de aceite.", "Critérios de aceite; resolução de conflitos", "Vamos medir o impacto antes.", "Vou levantar o impacto da mudança antes de aceitar o prazo como se nada tivesse mudado.", "Boa postura. Você demonstrou maturidade ao trabalhar critérios de aceite e resolução de conflitos. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Aceito, mas marco o risco.", "Vou aceitar a mudança, mas registrar que ela pode afetar prazo ou qualidade.", "Caminho razoável. A decisão resolve parte do problema, mas ainda não trabalha completamente critérios de aceite e resolução de conflitos. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Produto precisa assumir o custo.", "Se Produto mudou de novo, eles precisam assumir o custo da mudança e do atraso.", "Escolha inadequada. A escolha pode parecer prática no curto prazo, mas enfraquece critérios de aceite e resolução de conflitos. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Agora eu preciso que você pense como pleno: entrega, pessoas e consequência. Não dá para olhar só para a parte técnica.", "Visão plena; responsabilidade", "Vou separar fato de atrito.", "Vou separar o que é fato, o que é ruído e o que precisa virar critério de aceite.", "Resposta adequada. Você demonstrou maturidade ao trabalhar visão plena e responsabilidade. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Deixo cada lado explicar primeiro.", "Vou ouvir cada lado antes de decidir se realmente existe conflito ou só falta de informação.", "Decisão aceitável, mas incompleta. A decisão resolve parte do problema, mas ainda não trabalha completamente visão plena e responsabilidade. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "Alguém precisa responder por isso.", "Se o risco foi avisado e ignorado, alguém precisa responder antes de seguirmos.", "Essa decisão enfraquece a condução do problema. A escolha pode parecer prática no curto prazo, mas enfraquece visão plena e responsabilidade. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento."),
            D("Ainda dá para recuperar a sprint, mas não se cada pessoa continuar protegendo só a própria parte.", "Colaboração; trabalho em equipe", "Precisamos escolher o menor risco.", "Vou propor uma decisão que resolva o bloqueio sem empurrar uma dívida técnica maior para depois.", "Ótima decisão. Você demonstrou maturidade ao trabalhar colaboração e trabalho em equipe. A resposta ajudou a transformar tensão em alinhamento e mostrou preocupação com impacto, pessoas e entrega.", "Contorno agora, refatora depois.", "Vou contornar o bloqueio agora e deixar a refatoração documentada para uma próxima rodada.", "Sua resposta teve pontos positivos. A decisão resolve parte do problema, mas ainda não trabalha completamente colaboração e trabalho em equipe. Como pleno, era importante conduzir melhor o alinhamento para evitar que o conflito voltasse.", "A sprint precisa fechar agora.", "A sprint precisa fechar agora; se virar dívida técnica, a gente organiza depois.", "Cuidado: essa postura pode gerar consequência negativa. A escolha pode parecer prática no curto prazo, mas enfraquece colaboração e trabalho em equipe. Em uma fase de maior responsabilidade, isso aumenta atrito entre áreas e reduz a confiança no seu julgamento.")};
    }

    DialogoInfo[] DialogosSenior()
    {
        return new DialogoInfo[]
        {
            D("O incidente em produção já afetou o cliente e a diretoria quer uma previsão. O time está olhando para você porque alguém precisa organizar a resposta.", "Liderança; comunicação em crise", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando liderança e comunicação em crise. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de liderança e comunicação em crise sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece liderança e comunicação em crise. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A correção rápida existe, mas pode mascarar a causa real. Se aplicarmos agora, talvez estabilize; se falhar, a confiança cai mais.", "Gestão de risco; análise de causa", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando gestão de risco e análise de causa. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de gestão de risco e análise de causa sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece gestão de risco e análise de causa. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A war room está aberta há horas. Tem gente cansada, cliente cobrando e liderança pedindo uma explicação que ainda não temos completa.", "Resiliência; comunicação sob pressão", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando resiliência e comunicação sob pressão. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de resiliência e comunicação sob pressão sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece resiliência e comunicação sob pressão. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Dois especialistas discordam sobre a arquitetura. Os dois têm bons argumentos, mas a decisão não pode virar disputa de ego.", "Debate respeitoso; escuta técnica", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando debate respeitoso e escuta técnica. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de debate respeitoso e escuta técnica sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece debate respeitoso e escuta técnica. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O cliente quer saber quando volta, a equipe quer tempo para investigar e a diretoria quer uma mensagem segura. Nada disso pode ser tratado separado.", "Gestão de stakeholders; negociação", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando gestão de stakeholders e negociação. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de gestão de stakeholders e negociação sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece gestão de stakeholders e negociação. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Alguém deixou passar um alerta importante, mas caçar culpado agora só vai fazer as pessoas esconderem informação. Precisamos resolver e aprender.", "Cultura sem culpa; aprendizado contínuo", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando cultura sem culpa e aprendizado contínuo. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de cultura sem culpa e aprendizado contínuo sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece cultura sem culpa e aprendizado contínuo. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A estabilidade está no limite. Se mexermos demais, piora; se mexermos de menos, o cliente continua parado.", "Gestão de risco; estabilidade", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando gestão de risco e estabilidade. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de gestão de risco e estabilidade sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece gestão de risco e estabilidade. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Tem um dev segurando a bronca desde a madrugada. Ele está exausto e já começou a errar em coisas simples. Isso também é risco técnico.", "Empatia; segurança psicológica", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando empatia e segurança psicológica. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de empatia e segurança psicológica sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece empatia e segurança psicológica. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A decisão de arquitetura, que parecia distante, virou problema de produção hoje. Agora precisamos escolher um caminho sem romantizar a solução perfeita.", "Pensamento crítico; decisão de arquitetura", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando pensamento crítico e decisão de arquitetura. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de pensamento crítico e decisão de arquitetura sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece pensamento crítico e decisão de arquitetura. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A comunicação externa precisa ser honesta, mas não pode jogar a equipe na fogueira. O cliente precisa de clareza, não de pânico.", "Comunicação honesta; responsabilidade", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando comunicação honesta e responsabilidade. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de comunicação honesta e responsabilidade sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece comunicação honesta e responsabilidade. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O time está esperando uma direção. Se você hesitar demais, cada um vai agir por conta própria.", "Liderança; direcionamento", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando liderança e direcionamento. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de liderança e direcionamento sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece liderança e direcionamento. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O rollback resolve parte do impacto, mas joga fora trabalho importante. Manter a versão atual exige confiança numa correção que ainda não foi validada.", "Análise de trade-offs; tomada de decisão", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando análise de trade-offs e tomada de decisão. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de análise de trade-offs e tomada de decisão sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece análise de trade-offs e tomada de decisão. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A liderança quer um responsável pelo incidente. Eu prefiro sair daqui com causa, plano e prevenção, mas a pressão por culpado está crescendo.", "Responsabilidade; foco em causa raiz", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando responsabilidade e foco em causa raiz. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de responsabilidade e foco em causa raiz sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece responsabilidade e foco em causa raiz. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Tem gente experiente se atacando porque todo mundo está sob pressão. Se isso continuar, a crise técnica vira crise de equipe.", "Gestão de conflito; maturidade emocional", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando gestão de conflito e maturidade emocional. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de gestão de conflito e maturidade emocional sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece gestão de conflito e maturidade emocional. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O cliente percebeu inconsistência nos dados. Mesmo que o erro seja pequeno, a confiança já foi afetada.", "Transparência; construção de confiança", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando transparência e construção de confiança. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de transparência e construção de confiança sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece transparência e construção de confiança. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A equipe precisa saber o que comunicar no próximo status report. Silêncio parece omissão; detalhe demais pode virar alarme.", "Comunicação de crise; prudência", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando comunicação de crise e prudência. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de comunicação de crise e prudência sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece comunicação de crise e prudência. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A solução definitiva exige tempo que talvez não tenhamos. O contorno rápido exige risco que talvez não possamos assumir.", "Priorização; gestão de incerteza", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando priorização e gestão de incerteza. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de priorização e gestão de incerteza sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece priorização e gestão de incerteza. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Alguém reconheceu um erro no privado, mas está com medo de falar na reunião. A verdade importa, mas a forma como a gente recebe isso também.", "Segurança psicológica; confiança", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando segurança psicológica e confiança. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de segurança psicológica e confiança sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece segurança psicológica e confiança. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A diretoria quer garantia, mas garantia absoluta agora seria mentira.", "Honestidade; ética", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando honestidade e ética. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de honestidade e ética sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece honestidade e ética. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O cliente quer uma data, a engenharia quer mais diagnóstico e o Produto quer manter o compromisso comercial. Você precisa equilibrar essas forças.", "Equilíbrio de interesses; negociação", "Equilibro urgência e transparência.", "Vou equilibrar cliente, diretoria e equipe com uma mensagem honesta, sem prometer o que não temos.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando equilíbrio de interesses e negociação. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Seguro parte da mensagem.", "Vou segurar parte da mensagem até termos mais segurança técnica sobre o que aconteceu.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de equilíbrio de interesses e negociação sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Dou uma previsão firme.", "Vou dar uma previsão firme para acalmar o cliente, mesmo que ainda falte diagnóstico.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece equilíbrio de interesses e negociação. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("O sistema voltou parcialmente, mas ainda está instável. Se comemorarmos cedo demais, podemos perder credibilidade.", "Prudência; gestão de credibilidade", "Vamos alinhar uma resposta segura.", "Vou organizar uma resposta segura com o que sabemos, o que não sabemos e quando será o próximo status.", "Boa escolha. Você conduziu a situação com visão de liderança, trabalhando prudência e gestão de credibilidade. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Comunico só o que está fechado.", "Vou comunicar apenas o que está confirmado e evitar detalhes que ainda podem mudar.", "Resposta parcialmente correta. Você tentou controlar o impacto imediato, mas deixou parte de prudência e gestão de credibilidade sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não abro o risco agora.", "Prefiro não abrir o risco agora para evitar pânico enquanto tentamos resolver internamente.", "Essa atitude pode prejudicar a equipe. A resposta aumenta o risco da crise porque enfraquece prudência e gestão de credibilidade. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A decisão técnica de agora vai virar precedente. O time aprende com o que você tolera em crise.", "Liderança pelo exemplo; cultura", "Estabilizar sem perder evidência.", "Vou reduzir o impacto no cliente sem apagar informações importantes para entender a causa real.", "Boa postura. Você conduziu a situação com visão de liderança, trabalhando liderança pelo exemplo e cultura. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Uso o contorno para ganhar tempo.", "Vou aplicar um contorno para reduzir o impacto e ganhar tempo para investigar melhor.", "Caminho razoável. Você tentou controlar o impacto imediato, mas deixou parte de liderança pelo exemplo e cultura sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Mexemos direto para voltar logo.", "Vou mexer direto para fazer o serviço voltar logo; a análise completa pode esperar.", "Escolha inadequada. A resposta aumenta o risco da crise porque enfraquece liderança pelo exemplo e cultura. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("Tem uma reunião com stakeholders em poucos minutos. Precisamos transformar o caos técnico em uma mensagem responsável.", "Comunicação com stakeholders; síntese", "Divido frentes e protejo o time.", "Vou dividir frentes de trabalho, reduzir ruído e proteger a equipe para que ela consiga resolver.", "Resposta adequada. Você conduziu a situação com visão de liderança, trabalhando comunicação com stakeholders e síntese. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Deixo as frentes rodarem.", "Vou deixar as frentes rodarem e interferir só se a sala perder foco ou entrar em conflito.", "Decisão aceitável, mas incompleta. Você tentou controlar o impacto imediato, mas deixou parte de comunicação com stakeholders e síntese sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Pressão faz parte da crise.", "A equipe precisa entender o peso do erro agora; acolhimento pode ficar para o pós-crise.", "Essa decisão enfraquece a condução do problema. A resposta aumenta o risco da crise porque enfraquece comunicação com stakeholders e síntese. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe."),
            D("A crise está quase controlada, mas o pós-incidente vai definir se isso vira aprendizado ou só mais uma cicatriz na equipe.", "Aprendizado contínuo; retrospectiva", "Decido pelo risco, não pelo ego.", "Vou ouvir os argumentos técnicos e decidir pelo risco operacional, não pela autoridade de quem falou.", "Ótima decisão. Você conduziu a situação com visão de liderança, trabalhando aprendizado contínuo e retrospectiva. A decisão reduziu riscos, protegeu a equipe e ajudou a manter uma resposta responsável diante da pressão.", "Escolho o caminho menos instável.", "Vou escolher o caminho que parece menos instável agora, mesmo sem resolver toda a discussão.", "Sua resposta teve pontos positivos. Você tentou controlar o impacto imediato, mas deixou parte de aprendizado contínuo e retrospectiva sem uma condução forte. Em uma crise, respostas parciais podem comprar tempo, mas ainda deixam risco estratégico.", "Não há tempo para debate.", "Não temos tempo para debate técnico longo; vou impor uma direção e seguir.", "Cuidado: essa postura pode gerar consequência negativa. A resposta aumenta o risco da crise porque enfraquece aprendizado contínuo e retrospectiva. Em nível avançado, decisões assim podem gerar perda de confiança, pressão maior da liderança e insegurança na equipe.")};
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
                    return "O ponto forte da sua resposta foi tornar a informação visível e compreensível para todos antes que o problema crescesse.";
                case CategoriaSoftSkill.TrabalhoEquipe:
                    return "O ponto forte da sua resposta foi tratar o problema como algo coletivo, preservando colaboração e responsabilidade compartilhada.";
                case CategoriaSoftSkill.ResolucaoProblemas:
                    return "O ponto forte da sua resposta foi buscar causa, impacto e segurança antes de agir por impulso.";
                case CategoriaSoftSkill.Adaptabilidade:
                    return "O ponto forte da sua resposta foi aceitar a mudança sem perder controle de prazo, qualidade e impacto.";
                case CategoriaSoftSkill.Empatia:
                    return "O ponto forte da sua resposta foi considerar as pessoas envolvidas sem deixar de buscar uma solução profissional.";
            }
        }

        if (opcao.tomResposta == TomResposta.Neutra)
        {
            switch (opcao.categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return "A resposta não é totalmente ruim, mas ainda deixa espaço para dúvidas, desalinhamento ou retrabalho.";
                case CategoriaSoftSkill.TrabalhoEquipe:
                    return "A resposta evita um conflito imediato, mas não contribui o suficiente para destravar o time como equipe.";
                case CategoriaSoftSkill.ResolucaoProblemas:
                    return "A resposta pode resolver algo no curto prazo, mas falta investigação suficiente para evitar que o erro retorne.";
                case CategoriaSoftSkill.Adaptabilidade:
                    return "A resposta aceita parte da mudança, porém ainda não reorganiza prioridades e consequências com clareza.";
                case CategoriaSoftSkill.Empatia:
                    return "A resposta mantém o ambiente sob controle, mas poderia demonstrar mais cuidado com o impacto nas pessoas.";
            }
        }

        switch (opcao.categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "O principal problema foi a falta de clareza. Quando a comunicação falha, a equipe perde contexto e decide pior.";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "O principal problema foi enfraquecer a colaboração. Isso aumenta distância entre as pessoas e dificulta a solução.";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "O principal problema foi agir sem análise suficiente. Soluções apressadas podem esconder a causa real.";
            case CategoriaSoftSkill.Adaptabilidade:
                return "O principal problema foi resistir ou se adaptar mal à mudança, dificultando a reorganização do trabalho.";
            case CategoriaSoftSkill.Empatia:
                return "O principal problema foi ignorar o impacto humano da situação, o que pode reduzir confiança e aumentar tensão.";
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