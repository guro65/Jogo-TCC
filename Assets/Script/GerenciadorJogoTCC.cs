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

    [Header("Tela inicial")]
    public TMP_InputField campoNome;
    public TMP_Dropdown dropdownGenero;
    public Button botaoComecar;

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
    private bool exibindoReacaoEscolha;
    private bool aguardandoResultadoFase;
    private int proximoNoAposReacao;
    private bool finalizarDepoisResultado;

    private bool textoDigitando;
    private Coroutine rotinaDigitacao;
    private string textoCompletoNPC = "";
    private string textoCompletoJogador = "";

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

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        AtualizarMedidor();
    }

    void PrepararJogador()
    {
        nomeJogador = campoNome != null ? campoNome.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(nomeJogador))
        {
            Debug.LogWarning("Digite um nome antes de continuar.");
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
        MostrarNoAtual();
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
        return CriarQuestoesPorModelo(
            FaseProfissional.FacilJunior,
            personagensJunior,
            "uma task no Jira com descrição incompleta",
            "um pull request com comentários de revisão",
            "um bug simples encontrado pelo QA",
            "uma mudança pequena de requisito no meio da sprint"
        );
    }

    List<QuestaoTI> CriarQuestoesPleno()
    {
        return CriarQuestoesPorModelo(
            FaseProfissional.MedioPleno,
            personagensPleno,
            "uma sprint atrasada por falta de alinhamento",
            "um conflito entre desenvolvedor e QA",
            "uma prioridade técnica competindo com demanda urgente do produto",
            "uma refatoração necessária em código legado"
        );
    }

    List<QuestaoTI> CriarQuestoesSenior()
    {
        return CriarQuestoesPorModelo(
            FaseProfissional.DificilSenior,
            personagensSenior,
            "um incidente crítico em produção",
            "uma war room com cliente impactado",
            "uma decisão de arquitetura com risco técnico",
            "um conflito entre pessoas experientes da equipe"
        );
    }

    List<QuestaoTI> CriarQuestoesPorModelo(FaseProfissional fase, List<DadosPersonagem> personagens, string tema1, string tema2, string tema3, string tema4)
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

        for (int i = 0; i < TOTAL_PERGUNTAS_POR_FASE; i++)
        {
            CategoriaSoftSkill categoria = categorias[i % categorias.Length];

            DadosPersonagem npc = personagens[i % 3];
            DadosPersonagem esquerda = personagens[0];
            DadosPersonagem centro = personagens[1];
            DadosPersonagem direita = personagens[2];

            string tema = ObterTema(i, tema1, tema2, tema3, tema4);

            questoes.Add(Q(
                categoria,
                npc,
                esquerda,
                centro,
                direita,
                EscolherEmocaoNPCDaPergunta(fase, categoria),
                EscolherEmocaoJogadorAoOuvir(fase, categoria),
                CriarFalaNPC(fase, categoria, npc, tema, i),
                CriarTextoBotaoBom(fase, categoria),
                CriarRespostaBoa(fase, categoria, tema),
                CriarReacaoBoa(fase, categoria),
                CriarTextoBotaoMedio(fase, categoria),
                CriarRespostaMedia(fase, categoria, tema),
                CriarReacaoMedia(fase, categoria),
                CriarTextoBotaoRuim(fase, categoria),
                CriarRespostaRuim(fase, categoria, tema),
                CriarReacaoRuim(fase, categoria)
            ));
        }

        return questoes;
    }

    string ObterTema(int indice, string tema1, string tema2, string tema3, string tema4)
    {
        switch (indice % 4)
        {
            case 0:
                return tema1;
            case 1:
                return tema2;
            case 2:
                return tema3;
            default:
                return tema4;
        }
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
        if (fase == FaseProfissional.DificilSenior)
            return Emocao.Neutro;

        if (categoria == CategoriaSoftSkill.Empatia)
            return Emocao.Neutro;

        return Emocao.Neutro;
    }

    string CriarFalaNPC(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, string tema, int indice)
    {
        string cargo = string.IsNullOrWhiteSpace(npc.cargoOuFuncao) ? "profissional de TI" : npc.cargoOuFuncao;
        string assinatura = npc.nomePersonagem + " (" + cargo + "): ";
        string contexto = DescreverContextoProfissional(fase, tema);
        string tom = ObterTomPorPersonalidade(npc);

        if (fase == FaseProfissional.FacilJunior)
        {
            switch (categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return assinatura + tom + contexto + " Antes de começar, quero entender como você vai tirar dúvidas sem deixar isso virar retrabalho.";

                case CategoriaSoftSkill.TrabalhoEquipe:
                    return assinatura + tom + contexto + " Isso não afeta só a sua entrega. Como você pretende colaborar com o time sem ficar dependente de alguém resolver por você?";

                case CategoriaSoftSkill.ResolucaoProblemas:
                    return assinatura + tom + contexto + " Antes de pedir uma resposta pronta, me mostre como você investigaria o problema.";

                case CategoriaSoftSkill.Adaptabilidade:
                    return assinatura + tom + contexto + " A prioridade mudou no meio do caminho. Quero ver como você se reorganiza sem travar.";

                case CategoriaSoftSkill.Empatia:
                    return assinatura + tom + contexto + " Uma pessoa do time já está no limite. A forma como você responder agora pode acalmar a situação ou piorar o clima.";
            }
        }

        if (fase == FaseProfissional.MedioPleno)
        {
            switch (categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return assinatura + tom + contexto + " Devs, QA e produto estão interpretando a situação de formas diferentes. Como você alinha todo mundo antes que isso vire atraso?";

                case CategoriaSoftSkill.TrabalhoEquipe:
                    return assinatura + tom + contexto + " O time começou a se dividir, e ninguém quer ceder. Como você ajuda a destravar a colaboração?";

                case CategoriaSoftSkill.ResolucaoProblemas:
                    return assinatura + tom + contexto + " A solução rápida parece tentadora, mas pode gerar dívida técnica. Qual caminho você propõe?";

                case CategoriaSoftSkill.Adaptabilidade:
                    return assinatura + tom + contexto + " O planejamento mudou e ainda precisamos entregar com qualidade. Como você reorganiza suas prioridades?";

                case CategoriaSoftSkill.Empatia:
                    return assinatura + tom + contexto + " Uma pessoa do time está sobrecarregada e os erros começaram a aparecer. Como você lida com isso sem expor ninguém?";
            }
        }

        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return assinatura + tom + contexto + " A liderança e o cliente já perceberam o impacto. Como você comunica riscos sem criar pânico e sem esconder o problema?";

            case CategoriaSoftSkill.TrabalhoEquipe:
                return assinatura + tom + contexto + " A pressão subiu e as pessoas começaram a se atacar. Como você puxa o time de volta para a solução?";

            case CategoriaSoftSkill.ResolucaoProblemas:
                return assinatura + tom + contexto + " A correção mais rápida resolve agora, mas pode comprometer estabilidade depois. Como você decide?";

            case CategoriaSoftSkill.Adaptabilidade:
                return assinatura + tom + contexto + " A direção do projeto mudou e o time espera uma decisão firme. Como você conduz essa virada?";

            case CategoriaSoftSkill.Empatia:
                return assinatura + tom + contexto + " Depois do ocorrido, parte da equipe está com medo de assumir responsabilidades. Como você reconstrói confiança sem ignorar o erro?";
        }

        return assinatura + "Temos uma situação importante para resolver. Quero ouvir sua decisão.";
    }

    string DescreverContextoProfissional(FaseProfissional fase, string tema)
    {
        switch (tema)
        {
            case "uma task no Jira com descrição incompleta":
                return "A task chegou com informações pela metade e o prazo continua correndo.";

            case "um pull request com comentários de revisão":
                return "O pull request voltou com comentários importantes de revisão.";

            case "um bug simples encontrado pelo QA":
                return "O QA encontrou um bug simples, mas ele bloqueia a validação da entrega.";

            case "uma mudança pequena de requisito no meio da sprint":
                return "Produto pediu uma mudança pequena, mas ela apareceu no meio da sprint.";

            case "uma sprint atrasada por falta de alinhamento":
                return "A sprint atrasou porque cada área seguiu com uma interpretação diferente.";

            case "um conflito entre desenvolvedor e QA":
                return "Dev e QA estão discordando do problema e a discussão já começou a atrapalhar a entrega.";

            case "uma prioridade técnica competindo com demanda urgente do produto":
                return "Existe uma melhoria técnica importante competindo com uma demanda urgente de produto.";

            case "uma refatoração necessária em código legado":
                return "O código legado precisa de refatoração, mas qualquer mudança ali exige cuidado.";

            case "um incidente crítico em produção":
                return "Um incidente crítico acabou de atingir produção.";

            case "uma war room com cliente impactado":
                return "A war room foi aberta e o cliente já está diretamente impactado.";

            case "uma decisão de arquitetura com risco técnico":
                return "A decisão de arquitetura vai afetar o projeto por meses.";

            case "um conflito entre pessoas experientes da equipe":
                return "Duas pessoas experientes do time discordam e a tensão está contaminando a reunião.";
        }

        return "Temos uma situação envolvendo " + tema + ".";
    }

    string ObterTomPorPersonalidade(DadosPersonagem npc)
    {
        if (npc == null)
            return "";

        switch (npc.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                return "Vamos com calma. ";

            case PersonalidadePersonagem.Calmo:
                return "Respira e analisa comigo. ";

            case PersonalidadePersonagem.Extrovertido:
                return "Beleza, vamos resolver isso do jeito certo. ";

            case PersonalidadePersonagem.Timido:
                return "Eu sei que a situação é desconfortável, mas precisamos falar sobre ela. ";

            case PersonalidadePersonagem.Irritado:
                return "Já perdemos tempo demais com isso. ";

            case PersonalidadePersonagem.Serio:
                return "Vou ser direto. ";

            case PersonalidadePersonagem.Competitivo:
                return "Esse é o tipo de situação que separa quem só executa de quem realmente cresce. ";

            case PersonalidadePersonagem.Engracado:
                return "Parece que o sistema resolveu testar nossa paciência hoje. ";

            case PersonalidadePersonagem.Inseguro:
                return "Não quero que isso saia do controle. ";
        }

        return "";
    }

    string CriarTextoBotaoBom(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou centralizar as informações.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou alinhar com os envolvidos.";
                return "Vou perguntar antes de seguir.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou trazer o time de volta ao foco.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou juntar quem precisa decidir.";
                return "Vou ajudar sem empurrar o problema.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou medir o risco antes de agir.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou investigar antes de corrigir.";
                return "Vou testar e pedir ajuda com dados.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou reorganizar o plano do time.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou ajustar as prioridades.";
                return "Vou confirmar o que mudou.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou cuidar do time sem esconder fatos.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou conversar antes que piore.";
                return "Vou ouvir antes de responder.";

            default:
                return "Vou agir com cuidado.";
        }
    }

    string CriarTextoBotaoMedio(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Resolvo primeiro, aviso depois.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Falo só com quem estiver perto.";
                return "Vou seguir com o que entendi.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Cada um cuida da sua parte.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Ajudo só no que for comigo.";
                return "Faço minha parte primeiro.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Aplico um contorno rápido.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Corrijo o urgente agora.";
                return "Tento uma solução simples.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Mudo só quando for definitivo.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Ajusto o que der agora.";
                return "Vou mudar, mas com calma.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Evito expor, mas sigo cobrando.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Dou espaço, sem assumir tudo.";
                return "Melhor não me envolver muito.";

            default:
                return "Resolvo o urgente primeiro.";
        }
    }

    string CriarTextoBotaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Melhor não alarmar ninguém.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Quem precisar, que pergunte.";
                return "Vou fazer do jeito que entendi.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Minha decisão vai prevalecer.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Primeiro precisamos achar o culpado.";
                return "Se atrasarem, não é comigo.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Mudo direto em produção.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou mexer e torcer pra funcionar.";
                return "Vou chutar uma solução.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou manter o plano antigo.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Cansei dessas mudanças.";
                return "Avisaram tarde demais.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Agora não é hora de acolher.";
                if (fase == FaseProfissional.MedioPleno)
                    return "A pessoa devia ter avisado.";
                return "Cada um lida com sua pressão.";

            default:
                return "Vou fazer do meu jeito.";
        }
    }

    string CriarRespostaBoa(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou centralizar as informações, comunicar o risco com transparência e manter liderança, cliente e equipe atualizados com o mesmo contexto.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou chamar as pessoas envolvidas, confirmar o entendimento de cada área e registrar um alinhamento claro para evitar ruído.";
                return "Vou perguntar o que não ficou claro, confirmar o entendimento e avisar cedo caso algo possa atrasar.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou interromper a troca de acusações, separar fatos de opiniões e conduzir o time para uma decisão conjunta.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou aproximar as áreas envolvidas, dividir responsabilidades e garantir que ninguém fique bloqueado sozinho.";
                return "Vou ajudar no que estiver ao meu alcance e pedir orientação quando precisar, sem jogar o problema para outra pessoa.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou avaliar impacto, risco e prazo antes de escolher uma solução. Se precisar de contorno, ele será documentado e acompanhado.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou investigar a causa, comparar alternativas e propor uma solução que resolva sem criar uma dívida técnica desnecessária.";
                return "Vou reproduzir o problema, revisar as informações disponíveis e pedir ajuda com dados concretos caso eu trave.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou reorganizar o plano, explicar as mudanças para o time e proteger o que for mais crítico para a entrega.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou revisar prioridades, negociar o que precisa sair primeiro e ajustar minha entrega sem perder qualidade.";
                return "Vou entender a nova prioridade, ajustar minha tarefa e confirmar o que precisa ser entregue primeiro.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou reconhecer o impacto na equipe, criar um espaço seguro para levantar os fatos e focar em aprendizado, não em caça aos culpados.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou conversar com a pessoa em particular, entender a sobrecarga e ajudar a reorganizar as tarefas antes que piore.";
                return "Vou ouvir a pessoa, oferecer ajuda e evitar qualquer comentário que aumente a pressão.";
        }

        return "Vou agir com clareza, responsabilidade e respeito pelo time.";
    }

    string CriarRespostaMedia(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou resolver primeiro com quem está mais perto do problema e depois repasso o resumo para o restante.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou falar com quem eu achar mais importante agora e depois vejo se precisa envolver mais alguém.";
                return "Vou tentar seguir com o que entendi. Se aparecer erro, eu pergunto.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou deixar cada pessoa cuidar da própria parte e intervenho se o conflito atrapalhar demais.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou ajudar na parte que depende de mim, mas não quero me envolver muito na discussão dos outros.";
                return "Vou fazer minha parte e ajudar se alguém pedir diretamente.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou aplicar o contorno mais rápido agora e depois avaliamos se precisa de uma solução definitiva.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou corrigir o ponto mais urgente primeiro e depois investigo a causa com mais calma.";
                return "Vou testar uma solução simples primeiro. Se não funcionar, procuro outra saída.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou aceitar a mudança, mas mantenho o plano atual até ter certeza de que a nova direção é definitiva.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou ajustar o que for urgente, mesmo que algumas coisas fiquem para resolver depois.";
                return "Vou mudar o que me pedirem, mas preciso de um tempo para entender tudo.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou evitar expor as pessoas, mas agora o foco principal precisa ser entregar uma resposta rápida.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Vou dar espaço para a pessoa, mas sem assumir responsabilidades que não são minhas.";
                return "Vou evitar piorar a situação e seguir com meu trabalho.";
        }

        return "Vou resolver o mais urgente agora e depois ajusto o restante.";
    }

    string CriarRespostaRuim(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                if (fase == FaseProfissional.DificilSenior)
                    return "Não vou alarmar ninguém. É melhor resolver em silêncio e só comentar se alguém perguntar.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Quem precisar saber que venha atrás. Eu não vou gastar tempo explicando tudo de novo.";
                return "Se a descrição está ruim, o problema não é meu. Vou fazer do jeito que eu entendi.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                if (fase == FaseProfissional.DificilSenior)
                    return "Se o time está brigando, cada um que defenda sua parte. Eu só vou garantir que a minha decisão prevaleça.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Isso atrasou porque alguém não fez a própria parte. Primeiro precisamos apontar quem errou.";
                return "Vou cuidar só da minha entrega. Se outra pessoa atrasar, não é comigo.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                if (fase == FaseProfissional.DificilSenior)
                    return "Vou aplicar a correção mais rápida direto em produção. Depois a gente vê se deu algum efeito colateral.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Não precisa investigar tanto. Vou mudar o que parece errado e torcer para funcionar.";
                return "Vou chutar uma solução. Se quebrar, alguém mais experiente arruma depois.";

            case CategoriaSoftSkill.Adaptabilidade:
                if (fase == FaseProfissional.DificilSenior)
                    return "Essa mudança chegou tarde demais. Vou manter o plano antigo e quem discordar que justifique.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Toda hora mudam prioridade. Vou continuar no que eu já estava fazendo.";
                return "Não vou mudar agora. Se queriam diferente, deveriam ter avisado antes.";

            case CategoriaSoftSkill.Empatia:
                if (fase == FaseProfissional.DificilSenior)
                    return "Medo de errar todo mundo tem. Agora não é hora de acolher ninguém, é hora de cobrar resultado.";
                if (fase == FaseProfissional.MedioPleno)
                    return "Se a pessoa está sobrecarregada, deveria ter falado antes. Não vou assumir problema dos outros.";
                return "Cada um lida com sua pressão. Eu tenho meus próprios problemas.";
        }

        return "Vou fazer do meu jeito e evitar me envolver.";
    }

    string CriarReacaoBoa(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Boa decisão. Você tratou o problema com maturidade, protegeu a equipe e manteve a responsabilidade técnica visível.";

        if (fase == FaseProfissional.MedioPleno)
            return "Boa postura. Você pensou na entrega sem esquecer comunicação, colaboração e impacto no time.";

        return "Boa resposta. Você mostrou vontade de aprender, pediu clareza e evitou transformar dúvida em retrabalho.";
    }

    string CriarReacaoMedia(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Pode funcionar no curto prazo, mas ainda falta visão de liderança. Um sênior precisa cuidar também de confiança, processo e impacto.";

        if (fase == FaseProfissional.MedioPleno)
            return "Você resolveu parte do problema, mas ainda deixou pontas soltas. Como pleno, é importante prevenir novos desalinhamentos.";

        return "Você tentou seguir em frente, mas poderia ter comunicado melhor e pedido apoio com mais clareza.";
    }

    string CriarReacaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Essa decisão aumenta o risco da crise. Em nível sênior, esconder informações, impor decisões ou culpar pessoas pode causar mais dano que o erro inicial.";

        if (fase == FaseProfissional.MedioPleno)
            return "Essa postura enfraquece a confiança do time. Um pleno precisa reduzir atrito, não transformar pressão em conflito.";

        return "Essa escolha pode gerar retrabalho e passar a impressão de falta de responsabilidade. Como júnior, pedir clareza é melhor do que agir no escuro.";
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

            ConfigurarBotaoEscolha(botaoEscolha1, textoEscolha1, noAtual.opcoes, 0);
            ConfigurarBotaoEscolha(botaoEscolha2, textoEscolha2, noAtual.opcoes, 1);
            ConfigurarBotaoEscolha(botaoEscolha3, textoEscolha3, noAtual.opcoes, 2);
        }
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