using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Final")]
    public TMP_Text textoFinal;
    public Button botaoReiniciar;

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
    private bool exibindoReacaoEscolha;
    private bool aguardandoResultadoFase;
    private int proximoNoAposReacao;
    private bool finalizarDepoisResultado;

    private bool textoDigitando;
    private Coroutine rotinaDigitacao;
    private string textoCompletoNPC = "";
    private string textoCompletoJogador = "";

    private bool nomeJaConfirmado;
    private Coroutine rotinaDigitacaoInicial;

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
        if (botaoReiniciar != null) botaoReiniciar.onClick.AddListener(ReiniciarJogo);

        TocarMusica(musicaInicio);
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
        if (fundo != null) fundo.SetActive(false);
        PrepararImagemTransicao(false, 0f);

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        nomeJaConfirmado = false;

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

        faseAtual = FaseProfissional.FacilJunior;
        IniciarFase(faseAtual);
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

    void IniciarFase(FaseProfissional fase)
    {
        faseAtual = fase;

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
        exibindoReacaoEscolha = false;
        aguardandoResultadoFase = false;
        proximoNoAposReacao = -1;
        finalizarDepoisResultado = false;
        ultimaEmocaoPersonagem = Emocao.Neutro;
        emocaoAtualJogador = Emocao.Neutro;

        if (fundo != null) fundo.SetActive(true);
        if (imagemFundo != null && fundoTrabalhoTI != null) imagemFundo.sprite = fundoTrabalhoTI;

        if (painelTopo != null) painelTopo.SetActive(true);
        if (painelDialogo != null) painelDialogo.SetActive(true);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);

        AtualizarTextoFase();
        AtualizarMedidor();
        TocarMusicaDaFase();

        MontarRoteiroDaFase();

        indiceNoAtual = 0;

        if (usarTransicaoAoComecarPrimeiraFase && fase == FaseProfissional.FacilJunior)
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
        float valor = 0f;

        if (pontosMaximosFase > 0)
            valor = (float)pontosFaseAtual / pontosMaximosFase;

        if (medidorAprovacao != null)
            medidorAprovacao.value = valor;

        if (textoMedidorAprovacao != null)
            textoMedidorAprovacao.text = "Aprovação: " + Mathf.RoundToInt(valor * 100f) + "% / Necessário: " + PorcentagemNecessaria(faseAtual).ToString("F0") + "%";
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

    string CriarFalaNPC(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        string[] falas;

        if (fase == FaseProfissional.FacilJunior)
        {
            falas = new string[]
            {
                "A task que chegou pra você está com informações faltando. Eu percebi isso agora olhando o card. Antes de sair codando, precisamos entender o que realmente foi pedido.",
                "O QA já marcou dois pontos que não batem com o que está no Jira. Não é grave ainda, mas se a gente deixar passar, vira retrabalho no fim do dia.",
                "Eu sei que você acabou de entrar, então não precisa fingir que entendeu tudo. O importante agora é mostrar como você tira dúvida sem travar a entrega.",
                "O pull request voltou com comentários simples, mas alguns deles mudam o comportamento da tela. A gente precisa responder sem parecer que está só se defendendo.",
                "Produto pediu uma alteração pequena agora no meio da sprint. Pequena no papel, porque no código ela mexe em mais coisa do que parece.",
                "Tem uma pessoa do time tentando te ajudar, mas ela também está cheia de tarefa. Se você pedir ajuda, precisa chegar com contexto, não só com 'não funciona'.",
                "Esse bug não derruba o sistema, mas bloqueia o teste do QA. Se você ignorar porque parece pequeno, todo mundo fica parado esperando.",
                "O backend disse que a regra está certa, mas o comportamento na tela está diferente. Antes de apontar erro, melhor juntar as informações.",
                "A daily começa em alguns minutos. Se você falar só 'estou fazendo', ninguém vai saber que existe risco nessa entrega.",
                "A gente pode resolver isso sem drama, mas precisa de clareza. O problema é pequeno; o ruído em volta dele é que pode crescer.",
                "O card foi escrito rápido demais e agora sobrou pra equipe interpretar. Vamos arrumar isso antes que cada pessoa siga por um caminho diferente.",
                "Eu vi que você tentou corrigir sozinho. A intenção é boa, mas ficar muito tempo calado pode passar a impressão errada.",
                "O comentário no pull request não foi uma bronca. É só parte do processo. A forma como você responde também conta.",
                "Tem uma mudança de prioridade chegando. Não é pra largar tudo de qualquer jeito, mas também não dá pra fingir que nada mudou.",
                "O QA está pressionado porque precisa fechar os testes ainda hoje. Se a gente responder mal, a conversa vira conflito em vez de solução.",
                "A tarefa parece simples, mas tem uma regra de negócio escondida nela. É melhor confirmar agora do que descobrir depois da entrega.",
                "O time está tentando entender se isso é bug ou requisito mal explicado. Sua resposta pode ajudar a organizar a conversa.",
                "A pessoa que abriu o card não está online agora. Mesmo assim, precisamos deixar registrado o que falta pra ninguém se perder.",
                "Eu não espero que você resolva tudo sozinho. Eu espero que você saiba mostrar onde está a dúvida e o que já tentou.",
                "A alteração parece pequena, mas se entrar sem teste pode quebrar outra parte. Vamos pensar antes de correr.",
                "Tem gente falando por mensagem, gente comentando no Jira e gente discutindo no PR. Se ninguém organizar, isso vira bagunça.",
                "O prazo está apertado, mas ainda dá pra salvar a entrega. Só não dá pra trabalhar no escuro.",
                "Você vai perceber que desenvolvimento não é só escrever código. Metade do problema aqui é alinhar expectativa.",
                "Antes de fechar essa task, precisamos ter certeza de que todo mundo está falando da mesma coisa. Senão o erro volta pra você depois."
            };
        }
        else if (fase == FaseProfissional.MedioPleno)
        {
            falas = new string[]
            {
                "A sprint já começou atrasada e agora frontend, backend e QA estão defendendo versões diferentes do mesmo problema. A gente precisa colocar ordem nisso.",
                "Produto mudou o requisito de novo. Eu entendo a urgência, mas se aceitarmos tudo sem discutir impacto, a sprint quebra de vez.",
                "O QA está dizendo que avisou sobre esse risco ontem. O dev respondeu que a regra não estava clara. Agora os dois lados estão irritados.",
                "Tem uma refatoração que todo mundo sabe que precisa acontecer, mas sempre perde espaço pra demanda urgente. Hoje ela voltou a bloquear uma entrega.",
                "Você já não está mais numa posição de só receber tarefa. O time espera que você ajude a traduzir o problema entre as áreas.",
                "A discussão começou técnica, mas já virou pessoal. Se a gente deixar continuar assim, ninguém vai ouvir a solução de ninguém.",
                "A demanda do cliente é importante, mas a dívida técnica está cobrando juros. Precisamos decidir o que dá pra entregar sem criar uma bomba maior.",
                "O time está tentando fechar a sprint, mas tem gente trabalhando em prioridade antiga porque ninguém atualizou o combinado.",
                "O pull request virou debate. Tem comentário útil ali, mas também tem resposta atravessada. Precisamos baixar a temperatura.",
                "Produto quer resposta rápida. QA quer segurança. Desenvolvimento quer tempo. Nenhum lado está completamente errado.",
                "Se você só executar a tarefa, talvez entregue. Se você alinhar o impacto, talvez evite que o problema volte semana que vem.",
                "A pessoa mais nova do time assumiu uma parte difícil e agora está claramente perdida. Ela não pediu ajuda, mas o atraso já apareceu.",
                "A reunião está começando a virar disputa de culpa. Eu preciso que alguém traga a conversa de volta para fatos e próximos passos.",
                "A mudança parece pequena para produto, mas tecnicamente toca em fluxo antigo. Se dissermos só 'não dá', eles não vão entender.",
                "O QA encontrou um comportamento diferente do esperado, mas o requisito realmente está ambíguo. Aqui não adianta vencer discussão; tem que fechar entendimento.",
                "Você conhece essa parte do sistema melhor que quase todo mundo. Por isso sua forma de falar pode acalmar ou incendiar o time.",
                "A sprint não vai caber do jeito que está. Alguém vai precisar negociar escopo sem transformar isso em guerra.",
                "O legado está limitando a entrega, mas mexer nele agora tem risco. A decisão precisa ser madura, não só rápida.",
                "Tem uma pessoa sobrecarregada cobrindo duas frentes. Se a gente fingir que está tudo normal, a qualidade vai cair.",
                "A liderança quer saber o que está impedindo a entrega. Se a resposta sair mal construída, parece desculpa em vez de diagnóstico.",
                "A equipe precisa de uma decisão, mas uma decisão apressada pode custar caro. Vamos separar urgência de impulso.",
                "O conflito entre dev e QA está escondendo o ponto principal: ninguém fechou critério de aceite direito.",
                "Eu preciso que você pense como pleno agora: entrega, pessoas e consequência. Não dá pra olhar só pro pedaço técnico.",
                "Ainda dá pra recuperar a sprint, mas não se cada pessoa continuar protegendo só a própria parte."
            };
        }
        else
        {
            falas = new string[]
            {
                "O incidente em produção já impactou cliente e a diretoria quer uma previsão. O time está olhando pra você porque alguém precisa organizar a resposta.",
                "A correção rápida existe, mas pode mascarar a causa real. Se aplicarmos agora, talvez estabilize; se der errado, a confiança cai mais ainda.",
                "A war room está aberta há horas. Tem gente cansada, tem cliente cobrando e tem liderança pedindo uma explicação que ainda não temos completa.",
                "Dois especialistas discordam sobre a arquitetura. Os dois têm bons argumentos, mas a decisão não pode virar disputa de ego.",
                "O cliente quer saber quando volta. A equipe quer tempo pra investigar. A diretoria quer uma mensagem segura. Nada disso pode ser tratado isolado.",
                "Alguém deixou passar um alerta importante, mas agora caçar culpado só vai fazer as pessoas esconderem informação. Precisamos resolver e aprender.",
                "A estabilidade está no limite. Se mexermos demais, podemos piorar. Se mexermos de menos, o cliente continua parado.",
                "Tem um dev segurando a bronca desde madrugada. Ele está exausto e começou a cometer erro bobo. Isso também é risco técnico.",
                "A decisão de arquitetura que parecia distante virou problema de produção hoje. Agora precisamos escolher um caminho sem romantizar solução perfeita.",
                "A comunicação externa precisa ser honesta, mas não pode jogar a equipe no fogo. O cliente precisa de clareza, não de pânico.",
                "O time está esperando uma direção. Se você hesitar demais, cada um vai agir por conta própria.",
                "O rollback resolve parte do impacto, mas joga fora trabalho importante. Manter a versão atual exige confiança numa correção que ainda não foi validada.",
                "A liderança quer um responsável pelo incidente. Eu prefiro sair daqui com causa, plano e prevenção. Mas a pressão por culpado está crescendo.",
                "Tem gente experiente se atacando porque todo mundo está sob pressão. Se isso continuar, a crise técnica vira crise de equipe.",
                "O cliente percebeu inconsistência nos dados. Mesmo que a falha seja pequena, a confiança foi atingida.",
                "A equipe precisa saber o que comunicar no próximo status report. Silêncio agora parece omissão; excesso de detalhe pode gerar alarme.",
                "A solução definitiva exige tempo que talvez não tenhamos. O contorno rápido exige risco que talvez não possamos assumir.",
                "Uma pessoa assumiu um erro no privado, mas tem medo de falar na reunião. A verdade importa, mas a forma como lidamos com ela também.",
                "A diretoria está pressionando por garantia. Só que garantia absoluta, nesse momento, seria mentira.",
                "O cliente quer uma data. A engenharia quer mais diagnóstico. Produto quer manter compromisso comercial. Você precisa equilibrar essas forças.",
                "O sistema voltou parcialmente, mas ainda instável. Se comemorarmos cedo demais, podemos perder credibilidade.",
                "A decisão técnica de agora vai virar precedente. O time vai aprender com o que você tolera em crise.",
                "Tem uma reunião com stakeholders em poucos minutos. Precisamos transformar caos técnico em uma mensagem responsável.",
                "A crise está quase controlada, mas o pós-incidente vai definir se isso vira aprendizado ou só mais uma cicatriz na equipe."
            };
        }

        return AjustarFalaPelaPersonalidade(npc, falas[indice % falas.Length]);
    }

    string AjustarFalaPelaPersonalidade(DadosPersonagem npc, string falaBase)
    {
        if (npc == null)
            return falaBase;

        switch (npc.personalidade)
        {
            case PersonalidadePersonagem.Irritado:
                return falaBase + " E eu prefiro resolver isso agora, antes que vire mais uma reunião sem fim.";

            case PersonalidadePersonagem.Calmo:
                return falaBase + " Vamos respirar e organizar isso por partes.";

            case PersonalidadePersonagem.Gentil:
                return falaBase + " Ninguém precisa resolver sozinho, mas todo mundo precisa ser claro.";

            case PersonalidadePersonagem.Competitivo:
                return falaBase + " Outros times não esperariam isso sair do controle.";

            case PersonalidadePersonagem.Inseguro:
                return falaBase + " Eu só não quero que isso estoure maior do que já está.";

            case PersonalidadePersonagem.Engracado:
                return falaBase + " Se o sistema queria testar nossa paciência, conseguiu.";

            case PersonalidadePersonagem.Serio:
                return falaBase + " Preciso de uma resposta objetiva e responsável.";

            case PersonalidadePersonagem.Extrovertido:
                return falaBase + " Vamos colocar todo mundo na mesma página antes que isso vire telefone sem fio.";

            case PersonalidadePersonagem.Timido:
                return falaBase + " Talvez seja melhor alinhar com cuidado antes de falar com o grupo todo.";
        }

        return falaBase;
    }

    string CriarTextoBotaoBom(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return EscolherMiniResposta(fase, categoria, TomResposta.Boa, indice);
    }

    string CriarTextoBotaoMedio(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return EscolherMiniResposta(fase, categoria, TomResposta.Neutra, indice);
    }

    string CriarTextoBotaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        return EscolherMiniResposta(fase, categoria, TomResposta.Rude, indice);
    }

    string EscolherMiniResposta(FaseProfissional fase, CategoriaSoftSkill categoria, TomResposta tom, int indice)
    {
        string[] opcoes = ObterBancoMiniRespostas(fase, categoria, tom);

        if (opcoes == null || opcoes.Length == 0)
            return "Vou responder.";

        int deslocamentoFase = 0;

        if (fase == FaseProfissional.MedioPleno)
            deslocamentoFase = 2;
        else if (fase == FaseProfissional.DificilSenior)
            deslocamentoFase = 4;

        int deslocamentoCategoria = ((int)categoria * 3);
        int indiceFinal = Mathf.Abs(indice + deslocamentoFase + deslocamentoCategoria) % opcoes.Length;
        return opcoes[indiceFinal];
    }

    string[] ObterBancoMiniRespostas(FaseProfissional fase, CategoriaSoftSkill categoria, TomResposta tom)
    {
        if (fase == FaseProfissional.FacilJunior)
            return ObterMiniRespostasJunior(categoria, tom);

        if (fase == FaseProfissional.MedioPleno)
            return ObterMiniRespostasPleno(categoria, tom);

        return ObterMiniRespostasSenior(categoria, tom);
    }

    string[] ObterMiniRespostasJunior(CategoriaSoftSkill categoria, TomResposta tom)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (tom == TomResposta.Boa) return new string[] { "Vou confirmar antes.", "Melhor alinhar o card.", "Vou perguntar o que falta.", "Deixo a dúvida registrada.", "Vou validar com o QA.", "Antes de codar, confirmo." };
                if (tom == TomResposta.Neutra) return new string[] { "Tento seguir assim.", "Vou avançar e ajustar.", "Se travar, eu pergunto.", "Acho que entendi o suficiente.", "Começo pelo que está claro.", "Vou ver no caminho." };
                return new string[] { "Faço do jeito que der.", "O card veio assim.", "Não vou parar por isso.", "Se der errado, corrigem.", "Vou interpretar sozinho.", "Não dá pra esperar." };

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (tom == TomResposta.Boa) return new string[] { "Chego com contexto.", "Peço ajuda do jeito certo.", "Vamos dividir melhor.", "Mostro o que tentei.", "Alinho com quem depende disso.", "Chamo alguém pra destravar." };
                if (tom == TomResposta.Neutra) return new string[] { "Faço minha parte.", "Ajudo se pedirem.", "Tento não atrapalhar.", "Vou resolver meu lado.", "Deixo o time seguir.", "Pergunto só se precisar." };
                return new string[] { "Isso não é comigo.", "Cada um resolve o seu.", "Não vou puxar problema.", "Quem abriu que arrume.", "Eu só sigo minha tarefa.", "Não vou me envolver." };

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (tom == TomResposta.Boa) return new string[] { "Vou reproduzir primeiro.", "Testo antes de mexer.", "Procuro a causa real.", "Vou isolar o erro.", "Faço uma correção segura.", "Investigo passo a passo." };
                if (tom == TomResposta.Neutra) return new string[] { "Tento uma solução rápida.", "Corrijo o mais visível.", "Faço um teste simples.", "Vou pelo caminho curto.", "Se passar, seguimos.", "Ajusto o básico agora." };
                return new string[] { "Chuto uma correção.", "Mudo e vejo se passa.", "Apago esse trecho.", "Deve ser coisa simples.", "Mexendo eu descubro.", "Vou direto no código." };

            case CategoriaSoftSkill.Adaptabilidade:
                if (tom == TomResposta.Boa) return new string[] { "Reorganizo a tarefa.", "Confirmo o novo combinado.", "Ajusto antes de seguir.", "Vejo o que mudou de verdade.", "Priorizo o que importa.", "Atualizo o plano." };
                if (tom == TomResposta.Neutra) return new string[] { "Vou tentar encaixar.", "Termino o que comecei.", "Mudo se for necessário.", "Ajusto depois dessa parte.", "Dá pra adaptar no caminho.", "Sigo até avisarem melhor." };
                return new string[] { "Avisaram tarde demais.", "Agora não dá mais.", "Vou manter como estava.", "Mudaram de novo?", "Não vou refazer tudo.", "Isso atrasa por culpa deles." };

            case CategoriaSoftSkill.Empatia:
                if (tom == TomResposta.Boa) return new string[] { "Vou ouvir primeiro.", "Pergunto como posso ajudar.", "Respondo sem pressionar.", "Tento aliviar a conversa.", "Entendo o lado deles.", "Falo com calma." };
                if (tom == TomResposta.Neutra) return new string[] { "Deixo a pessoa respirar.", "Não vou entrar nisso agora.", "Mantenho a conversa objetiva.", "Espero ela pedir ajuda.", "Falo só do trabalho.", "Evito aumentar o clima." };
                return new string[] { "Cada um com sua pressão.", "Ela devia ter avisado.", "Não é problema meu.", "Pressão todo mundo tem.", "Agora não é hora disso.", "Se errou, precisa ouvir." };
        }

        return new string[] { "Vou responder." };
    }

    string[] ObterMiniRespostasPleno(CategoriaSoftSkill categoria, TomResposta tom)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (tom == TomResposta.Boa) return new string[] { "Vamos fechar entendimento.", "Preciso alinhar o impacto.", "Vou colocar todos na mesma página.", "Antes de prometer, valido.", "Traduzo isso pro time.", "Organizo os pontos abertos." };
                if (tom == TomResposta.Neutra) return new string[] { "Aviso depois do urgente.", "Resolvo e comunico depois.", "Passo só o essencial.", "Vou falar quando tiver certeza.", "Alinho se perguntarem.", "Deixo a conversa pra depois." };
                return new string[] { "Produto precisa decidir.", "QA que prove o ponto.", "Não dá pra explicar tudo.", "Quem pediu que detalhe.", "Vou responder direto.", "Não vou justificar atraso." };

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (tom == TomResposta.Boa) return new string[] { "Junto os envolvidos.", "Vamos tirar isso do pessoal.", "Separar fato de culpa.", "Alinho a decisão com todos.", "Trago o foco pro próximo passo.", "Faço a ponte entre áreas." };
                if (tom == TomResposta.Neutra) return new string[] { "Ajudo só no meu ponto.", "Não entro no conflito.", "Deixo eles se resolverem.", "Foco na minha entrega.", "Entro se travar mais.", "Evito tomar partido." };
                return new string[] { "Quem atrasou resolve.", "Alguém precisa assumir.", "Isso é culpa de uma área.", "Não vou carregar o time.", "Cada lado que se vire.", "Eu avisei que daria ruim." };

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (tom == TomResposta.Boa) return new string[] { "Resolvo sem criar bomba.", "Ataco causa e impacto.", "Faço o mínimo seguro.", "Proponho uma saída sustentável.", "Contorno com plano depois.", "Evito gambiarra escondida." };
                if (tom == TomResposta.Neutra) return new string[] { "Corrijo o urgente.", "Depois vemos a causa.", "Faço passar na sprint.", "Entrego o possível agora.", "Ajusto só o bloqueio.", "Não mexo além disso." };
                return new string[] { "Troco e vejo se passa.", "A sprint precisa fechar.", "Depois alguém refatora.", "Corto esse fluxo agora.", "O legado que aguente.", "Faço rápido e pronto." };

            case CategoriaSoftSkill.Adaptabilidade:
                if (tom == TomResposta.Boa) return new string[] { "Renegocio prioridade.", "Replanejo com o time.", "Mostro o custo da mudança.", "Ajusto sem esconder risco.", "Corto escopo com critério.", "Faço o combinado caber." };
                if (tom == TomResposta.Neutra) return new string[] { "Aceito, mas vai apertar.", "Tento encaixar na sprint.", "Dá pra mudar um pouco.", "Seguimos e vemos depois.", "Faço o possível.", "Ajusto sem prometer muito." };
                return new string[] { "Continuo o plano antigo.", "Isso quebra a sprint.", "Produto mudou tarde.", "Não vou refazer prioridade.", "Agora eles que esperem.", "Isso não cabe mais." };

            case CategoriaSoftSkill.Empatia:
                if (tom == TomResposta.Boa) return new string[] { "Converso sem expor.", "Vejo quem está sobrecarregado.", "Redistribuo sem culpar.", "Chamo no privado.", "Protejo o clima do time.", "Entendo antes de cobrar." };
                if (tom == TomResposta.Neutra) return new string[] { "Mantenho profissional.", "Falo só da entrega.", "Não entro no lado pessoal.", "Deixo isso pra liderança.", "Evito aumentar o conflito.", "Cobro com cuidado." };
                return new string[] { "Ela devia ter pedido ajuda.", "Todo mundo está cansado.", "Não dá pra passar pano.", "Se atrasou, precisa ouvir.", "O time não pode parar por isso.", "Erro tem consequência." };
        }

        return new string[] { "Vou responder." };
    }

    string[] ObterMiniRespostasSenior(CategoriaSoftSkill categoria, TomResposta tom)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (tom == TomResposta.Boa) return new string[] { "Passo risco e próximo status.", "Comunico sem gerar pânico.", "Alinho cliente e liderança.", "Falo o que sabemos agora.", "Assumo a comunicação da crise.", "Organizo uma atualização clara." };
                if (tom == TomResposta.Neutra) return new string[] { "Falo só o confirmado.", "Atualizo quando fechar causa.", "Seguro a comunicação por enquanto.", "Passo uma previsão curta.", "Evito detalhes agora.", "Comunico o mínimo necessário." };
                return new string[] { "Melhor não abrir o risco.", "Digo que já está resolvendo.", "A diretoria não precisa saber tudo.", "Seguro isso internamente.", "Não dá pra expor o time.", "Falo depois que estabilizar." };

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (tom == TomResposta.Boa) return new string[] { "Divido as frentes da crise.", "Tiro o foco da culpa.", "Organizo quem faz o quê.", "Protejo o time e conduzo.", "Centralizo a decisão.", "Coloco ordem na war room." };
                if (tom == TomResposta.Neutra) return new string[] { "Cada frente segue sua parte.", "Entro se a discussão travar.", "Deixo os líderes conduzirem.", "Acompanho sem interferir muito.", "Cobro atualização por área.", "Mantenho a sala rodando." };
                return new string[] { "Alguém precisa ser cobrado.", "Vou cortar a discussão agora.", "Quem causou sai da frente.", "Imponho a decisão e pronto.", "Não temos tempo pra consenso.", "A equipe que aguente a pressão." };

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (tom == TomResposta.Boa) return new string[] { "Estabilizo antes de mexer.", "Contorno sem apagar evidência.", "Isolo o impacto no cliente.", "Investigo com segurança.", "Reduzo dano e preservo causa.", "Faço correção controlada." };
                if (tom == TomResposta.Neutra) return new string[] { "Aplico o contorno rápido.", "Depois vemos causa raiz.", "Restauro primeiro, explico depois.", "Faço voltar e monitoro.", "Corto o fluxo problemático.", "Priorizo tirar do ar o erro." };
                return new string[] { "Mudo direto em produção.", "Derruba e sobe de novo.", "Desfaço sem investigar.", "Forço o rollback agora.", "Apago o que está quebrando.", "Depois entendemos o estrago." };

            case CategoriaSoftSkill.Adaptabilidade:
                if (tom == TomResposta.Boa) return new string[] { "Reorganizo pelo risco atual.", "Mudo o plano com critério.", "Renegocio com transparência.", "Priorizo estabilidade agora.", "Corto escopo sem esconder impacto.", "Adapto sem perder controle." };
                if (tom == TomResposta.Neutra) return new string[] { "Ajusto o plano no caminho.", "Seguimos com o que der.", "Mudo só o essencial.", "Seguro decisões grandes agora.", "Faço uma correção de rota.", "Depois replanejamos melhor." };
                return new string[] { "Agora não dá pra mudar.", "Mantemos o plano original.", "A crise não muda a meta.", "Não vou abrir renegociação.", "O cliente vai ter que esperar.", "Mudar agora piora tudo." };

            case CategoriaSoftSkill.Empatia:
                if (tom == TomResposta.Boa) return new string[] { "Protejo o time agora.", "Cobrança fica pro pós-crise.", "Conduzo sem expor ninguém.", "Acalmo antes de cobrar.", "Assumo a frente da conversa.", "O time precisa respirar." };
                if (tom == TomResposta.Neutra) return new string[] { "Cobro sem alongar.", "Mantenho o foco técnico.", "Deixo emoção pra depois.", "Faço uma conversa objetiva.", "Evito exposição pública.", "Seguramos o clima agora." };
                return new string[] { "Agora é hora de cobrar.", "Quem errou precisa falar.", "Não vou aliviar ninguém.", "A equipe precisa sentir o peso.", "Medo faz parte da crise.", "Depois a gente vê o clima." };
        }

        return new string[] { "Vou responder." };
    }

    string CriarRespostaBoa(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior) return "Vou comunicar o risco com clareza, dizer o que já sabemos, o que ainda estamos investigando e qual é o próximo status.";
                if (fase == FaseProfissional.MedioPleno) return "Vou alinhar com dev, QA e produto o impacto real antes de prometer qualquer coisa.";
                return "Antes de seguir, eu vou perguntar o que está faltando e deixar registrado pra evitar retrabalho.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior) return "Vou tirar o foco da culpa e organizar o time em frentes claras: estabilização, investigação e comunicação.";
                if (fase == FaseProfissional.MedioPleno) return "Vou chamar os envolvidos, separar fato de opinião e destravar uma decisão que todos consigam seguir.";
                return "Vou pedir ajuda mostrando o que eu já tentei, assim a pessoa não precisa começar do zero comigo.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior) return "Primeiro estabilizo o impacto no cliente. Depois conduzo a investigação da causa raiz sem apagar evidências.";
                if (fase == FaseProfissional.MedioPleno) return "Vou resolver o bloqueio, mas sem criar uma gambiarra que vire dívida técnica maior depois.";
                return "Vou reproduzir o erro, anotar o passo a passo e testar uma correção pequena antes de mexer em tudo.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior) return "Vou reorganizar o plano com base no risco atual e explicar o que muda, o que fica e por quê.";
                if (fase == FaseProfissional.MedioPleno) return "Vou renegociar prioridade e prazo com o time, em vez de só aceitar a mudança no escuro.";
                return "Vou confirmar o novo combinado e ajustar minha tarefa antes de continuar no caminho errado.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior) return "Vou proteger a equipe de exposição desnecessária, assumir a condução da crise e tratar o erro como aprendizado depois.";
                if (fase == FaseProfissional.MedioPleno) return "Vou conversar com a pessoa em particular, entender a sobrecarga e ajudar a redistribuir sem expor ninguém.";
                return "Vou ouvir primeiro, entender o que aconteceu e responder de um jeito que ajude em vez de aumentar a pressão.";
        }

        return "Vou agir com calma, clareza e responsabilidade.";
    }

    string CriarRespostaMedia(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior) return "Vou passar só o essencial agora e completar os detalhes quando tivermos mais certeza.";
                if (fase == FaseProfissional.MedioPleno) return "Vou seguir com a parte mais urgente e aviso o resto do time quando tiver algo mais concreto.";
                return "Vou tentar seguir com o que entendi. Se eu travar ou aparecer erro, aí eu pergunto.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior) return "Vou deixar cada frente tocar sua parte e entro se o conflito começar a atrapalhar demais.";
                if (fase == FaseProfissional.MedioPleno) return "Vou ajudar no que depende de mim, mas prefiro não entrar muito na discussão entre eles.";
                return "Vou fazer minha parte primeiro. Se alguém pedir ajuda, eu tento apoiar.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior) return "Vou aplicar o contorno mais rápido agora e depois vemos se precisa de uma solução definitiva.";
                if (fase == FaseProfissional.MedioPleno) return "Vou corrigir o ponto mais urgente primeiro e investigo a causa com mais calma depois.";
                return "Vou testar uma solução simples primeiro. Se não funcionar, procuro outra saída.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior) return "Vou manter o plano atual até ter certeza de que a mudança de direção é definitiva.";
                if (fase == FaseProfissional.MedioPleno) return "Vou ajustar o que for urgente, mesmo que algumas pontas fiquem pra depois.";
                return "Vou mudar o que pediram, mas preciso de um tempo pra entender tudo.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior) return "Vou evitar expor as pessoas, mas agora o foco principal precisa ser dar uma resposta rápida.";
                if (fase == FaseProfissional.MedioPleno) return "Vou dar espaço pra pessoa, mas sem assumir responsabilidades que não são minhas.";
                return "Vou evitar piorar o clima e seguir com meu trabalho.";
        }

        return "Vou resolver o mais urgente agora e ajustar o restante depois.";
    }

    string CriarRespostaRuim(FaseProfissional fase, CategoriaSoftSkill categoria, int indice)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior) return "Não vou alarmar ninguém. É melhor resolver em silêncio e só comentar se alguém perguntar.";
                if (fase == FaseProfissional.MedioPleno) return "Quem precisar saber que venha atrás. Eu não vou gastar tempo explicando tudo de novo.";
                return "Se a descrição está ruim, o problema não é meu. Vou fazer do jeito que eu entendi.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior) return "Se o time está brigando, cada um que defenda sua parte. Eu só vou garantir que minha decisão prevaleça.";
                if (fase == FaseProfissional.MedioPleno) return "Isso atrasou porque alguém não fez a própria parte. Primeiro precisamos apontar quem errou.";
                return "Vou cuidar só da minha entrega. Se outra pessoa atrasar, não é comigo.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior) return "Vou aplicar a correção mais rápida direto em produção. Depois a gente vê se deu efeito colateral.";
                if (fase == FaseProfissional.MedioPleno) return "Não precisa investigar tanto. Vou mudar o que parece errado e torcer pra funcionar.";
                return "Vou chutar uma solução. Se quebrar, alguém mais experiente arruma depois.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior) return "Essa mudança chegou tarde demais. Vou manter o plano antigo e quem discordar que justifique.";
                if (fase == FaseProfissional.MedioPleno) return "Toda hora mudam prioridade. Vou continuar no que eu já estava fazendo.";
                return "Não vou mudar agora. Se queriam diferente, deveriam ter avisado antes.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior) return "Medo de errar todo mundo tem. Agora não é hora de acolher ninguém, é hora de cobrar resultado.";
                if (fase == FaseProfissional.MedioPleno) return "Se a pessoa está sobrecarregada, deveria ter falado antes. Não vou assumir problema dos outros.";
                return "Cada um lida com sua pressão. Eu tenho meus próprios problemas.";
        }

        return "Vou fazer do meu jeito e evitar me envolver.";
    }

    string CriarReacaoBoa(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Certo. Isso dá direção sem maquiar o problema. O time consegue agir e o cliente recebe uma resposta responsável.";

        if (fase == FaseProfissional.MedioPleno)
            return "Boa. Você não tratou isso como uma disputa, e sim como um problema de alinhamento que precisa virar plano.";

        return "Boa. Isso mostra maturidade pra pedir clareza antes de transformar uma dúvida pequena em retrabalho.";
    }

    string CriarReacaoMedia(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Pode segurar o impacto agora, mas deixa risco pra depois. Em crise, o curto prazo importa, mas confiança também.";

        if (fase == FaseProfissional.MedioPleno)
            return "Funciona parcialmente, mas ainda deixa gente desalinhada. O problema pode voltar com outro nome na próxima reunião.";

        return "Dá pra seguir, mas você ainda fica muito dependente de perceber o problema tarde. Comunicação cedo evitaria isso.";
    }

    string CriarReacaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, int indice)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Isso piora a crise. Quando a pressão sobe, esconder informação ou procurar culpado costuma quebrar a confiança antes de corrigir o sistema.";

        if (fase == FaseProfissional.MedioPleno)
            return "Essa postura aumenta atrito. Como pleno, você precisa reduzir ruído, não empurrar o problema pra outra pessoa.";

        return "Esse caminho pode parecer mais fácil agora, mas passa a impressão de falta de responsabilidade e cria retrabalho pro time.";
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
        texto.text = opcoes[indice].textoOpcao;
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

        if (exibindoReacaoEscolha)
        {
            exibindoReacaoEscolha = false;

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

    void EscolherOpcao(OpcaoEscolha opcao)
    {
        pontosFaseAtual += opcao.pontosAprovacao;
        AtualizarMedidor();

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
        }

        ultimaRespostaJogador = opcao.respostaJogador;
        ultimaReacaoNPC = opcao.reacaoNPC;

        exibindoReacaoEscolha = true;

        if (opcao.proximoNo == -1 || opcao.proximoNo >= nos.Count)
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

    void MostrarResultadoFase()
    {
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(true);

        porcentagemFase = ((float)pontosFaseAtual / pontosMaximosFase) * 100f;

        bool aprovado = porcentagemFase >= PorcentagemNecessaria(faseAtual);
        string textoAprovacao;

        if (faseAtual == FaseProfissional.FacilJunior)
        {
            proximaFaseDepoisResultado = aprovado ? FaseProfissional.MedioPleno : FaseProfissional.FacilJunior;
            textoAprovacao = aprovado ? "Aprovado para a 2ª Fase - Média (Pleno)." : "Você precisa repetir a 1ª Fase - Fácil.";
            finalizarDepoisResultado = false;
        }
        else if (faseAtual == FaseProfissional.MedioPleno)
        {
            proximaFaseDepoisResultado = aprovado ? FaseProfissional.DificilSenior : FaseProfissional.MedioPleno;
            textoAprovacao = aprovado ? "Aprovado para a 3ª Fase - Difícil (Sênior)." : "Você precisa repetir a 2ª Fase - Média.";
            finalizarDepoisResultado = false;
        }
        else
        {
            textoAprovacao = aprovado ? "Você concluiu a fase Sênior com bom desempenho." : "Você concluiu a fase Sênior, mas precisa melhorar suas soft skills.";
            finalizarDepoisResultado = true;
        }

        if (textoResultadoFase != null)
        {
            textoResultadoFase.text =
                "Resultado da " + NomeFase(faseAtual) + "\n\n" +
                "Aprovação necessária: " + PorcentagemNecessaria(faseAtual).ToString("F0") + "%\n" +
                "Sua aprovação: " + porcentagemFase.ToString("F0") + "%\n\n" +
                textoAprovacao + "\n\n" +
                "Comunicação: " + comunicacao + "\n" +
                "Trabalho em Equipe: " + trabalhoEquipe + "\n" +
                "Resolução de Problemas: " + resolucaoProblemas + "\n" +
                "Adaptabilidade: " + adaptabilidade + "\n" +
                "Empatia: " + empatia;
        }
    }

    void ContinuarDepoisResultadoFase()
    {
        if (painelResultadoFase != null)
            painelResultadoFase.SetActive(false);

        if (finalizarDepoisResultado)
        {
            MostrarFinal();
            return;
        }

        IniciarFase(proximaFaseDepoisResultado);
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