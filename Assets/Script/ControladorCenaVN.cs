using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ControladorCenaVN : MonoBehaviour
{
    [Header("Imagens dos personagens")]
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;
    public Image imagemJogador;

    [Header("Cores")]
    public Color corFalando = Color.white;
    public Color corNaoFalando = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Comportamento visual dos NPCs")]
    [Tooltip("Se estiver marcado, apenas o NPC que está falando fica visível. Os outros são desativados.")]
    public bool mostrarApenasNpcFalando = true;

    [Tooltip("Se estiver marcado, o NPC que fala dá um pequeno pulinho.")]
    public bool animarNpcFalando = true;

    [Tooltip("Altura do pulinho do NPC que está falando.")]
    public float alturaPulinho = 18f;

    [Tooltip("Duração total do pulinho.")]
    public float duracaoPulinho = 0.18f;

    [Tooltip("Aumenta levemente o tamanho do NPC quando ele fala.")]
    public bool aumentarNpcFalando = true;

    [Tooltip("Escala aplicada no NPC quando ele fala.")]
    public float escalaFalando = 1.04f;

    [Header("Efeito opcional")]
    [Tooltip("Prefab opcional de efeito visual para aparecer no NPC que está falando. Pode deixar vazio.")]
    public GameObject efeitoFalandoPrefab;

    [Tooltip("Tempo para destruir o efeito visual depois de criado.")]
    public float tempoDestruirEfeito = 1f;

    private Coroutine rotinaEsquerda;
    private Coroutine rotinaCentro;
    private Coroutine rotinaDireita;

    private Vector3 escalaOriginalEsquerda = Vector3.one;
    private Vector3 escalaOriginalCentro = Vector3.one;
    private Vector3 escalaOriginalDireita = Vector3.one;

    private Vector2 posOriginalEsquerda;
    private Vector2 posOriginalCentro;
    private Vector2 posOriginalDireita;

    private bool iniciouPosicoes;

    void Awake()
    {
        SalvarTransformacoesOriginais();
    }

    void Start()
    {
        SalvarTransformacoesOriginais();
    }

    void SalvarTransformacoesOriginais()
    {
        if (iniciouPosicoes)
            return;

        if (imagemEsquerda != null)
        {
            escalaOriginalEsquerda = imagemEsquerda.rectTransform.localScale;
            posOriginalEsquerda = imagemEsquerda.rectTransform.anchoredPosition;
        }

        if (imagemCentro != null)
        {
            escalaOriginalCentro = imagemCentro.rectTransform.localScale;
            posOriginalCentro = imagemCentro.rectTransform.anchoredPosition;
        }

        if (imagemDireita != null)
        {
            escalaOriginalDireita = imagemDireita.rectTransform.localScale;
            posOriginalDireita = imagemDireita.rectTransform.anchoredPosition;
        }

        iniciouPosicoes = true;
    }

    public void AtualizarPersonagem(Image imagemDestino, DadosPersonagem personagem, Emocao emocao, bool mostrar)
    {
        if (imagemDestino == null)
            return;

        if (!mostrar || personagem == null)
        {
            imagemDestino.gameObject.SetActive(false);
            return;
        }

        imagemDestino.sprite = personagem.ObterSpritePorEmocao(emocao);
        imagemDestino.color = corNaoFalando;

        // Se mostrarApenasNpcFalando estiver ligado, quem decide se aparece é o método DestacarFalante/AnimarFalante.
        if (!mostrarApenasNpcFalando)
            imagemDestino.gameObject.SetActive(true);
    }

    public void AtualizarJogador(AparenciaJogador aparencia, Emocao emocao)
    {
        if (imagemJogador == null || aparencia == null)
            return;

        imagemJogador.gameObject.SetActive(true);
        imagemJogador.sprite = aparencia.ObterSpritePorEmocao(emocao);
    }

    // Mantido com esse nome porque o GerenciadorJogoTCC pode estar chamando AnimarFalante().
    public void AnimarFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        DestacarFalante(falante, esquerda, centro, direita);
    }

    // Método principal: destaca/ativa apenas quem está falando.
    public void DestacarFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        SalvarTransformacoesOriginais();

        bool esquerdaFalando = falante != null && falante == esquerda;
        bool centroFalando = falante != null && falante == centro;
        bool direitaFalando = falante != null && falante == direita;

        if (mostrarApenasNpcFalando)
        {
            AtualizarVisibilidadeFalante(imagemEsquerda, esquerdaFalando);
            AtualizarVisibilidadeFalante(imagemCentro, centroFalando);
            AtualizarVisibilidadeFalante(imagemDireita, direitaFalando);
        }
        else
        {
            if (imagemEsquerda != null)
            {
                imagemEsquerda.gameObject.SetActive(esquerda != null);
                imagemEsquerda.color = esquerdaFalando ? corFalando : corNaoFalando;
            }

            if (imagemCentro != null)
            {
                imagemCentro.gameObject.SetActive(centro != null);
                imagemCentro.color = centroFalando ? corFalando : corNaoFalando;
            }

            if (imagemDireita != null)
            {
                imagemDireita.gameObject.SetActive(direita != null);
                imagemDireita.color = direitaFalando ? corFalando : corNaoFalando;
            }
        }

        if (esquerdaFalando)
            AnimarImagemFalante(imagemEsquerda, ref rotinaEsquerda, posOriginalEsquerda, escalaOriginalEsquerda);

        if (centroFalando)
            AnimarImagemFalante(imagemCentro, ref rotinaCentro, posOriginalCentro, escalaOriginalCentro);

        if (direitaFalando)
            AnimarImagemFalante(imagemDireita, ref rotinaDireita, posOriginalDireita, escalaOriginalDireita);
    }

    void AtualizarVisibilidadeFalante(Image imagem, bool estaFalando)
    {
        if (imagem == null)
            return;

        imagem.gameObject.SetActive(estaFalando);
        imagem.color = estaFalando ? corFalando : corNaoFalando;
    }

    void AnimarImagemFalante(Image imagem, ref Coroutine rotina, Vector2 posOriginal, Vector3 escalaOriginal)
    {
        if (imagem == null)
            return;

        if (!imagem.gameObject.activeSelf)
            imagem.gameObject.SetActive(true);

        if (rotina != null)
            StopCoroutine(rotina);

        if (animarNpcFalando)
            rotina = StartCoroutine(PulinhoFalante(imagem, posOriginal, escalaOriginal));
        else
        {
            imagem.rectTransform.anchoredPosition = posOriginal;
            imagem.rectTransform.localScale = aumentarNpcFalando ? escalaOriginal * escalaFalando : escalaOriginal;
        }

        CriarEfeitoFalando(imagem);
    }

    IEnumerator PulinhoFalante(Image imagem, Vector2 posOriginal, Vector3 escalaOriginal)
    {
        RectTransform rect = imagem.rectTransform;

        float metadeDuracao = duracaoPulinho / 2f;
        float tempo = 0f;

        Vector2 posTopo = posOriginal + new Vector2(0f, alturaPulinho);
        Vector3 escalaAlvo = aumentarNpcFalando ? escalaOriginal * escalaFalando : escalaOriginal;

        while (tempo < metadeDuracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / metadeDuracao);

            rect.anchoredPosition = Vector2.Lerp(posOriginal, posTopo, t);
            rect.localScale = Vector3.Lerp(escalaOriginal, escalaAlvo, t);

            yield return null;
        }

        tempo = 0f;

        while (tempo < metadeDuracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / metadeDuracao);

            rect.anchoredPosition = Vector2.Lerp(posTopo, posOriginal, t);
            rect.localScale = Vector3.Lerp(escalaAlvo, escalaOriginal, t);

            yield return null;
        }

        rect.anchoredPosition = posOriginal;
        rect.localScale = escalaOriginal;
    }

    void CriarEfeitoFalando(Image imagem)
    {
        if (efeitoFalandoPrefab == null || imagem == null)
            return;

        GameObject efeito = Instantiate(efeitoFalandoPrefab, imagem.transform);
        efeito.transform.SetAsLastSibling();
        efeito.transform.localPosition = Vector3.zero;
        efeito.transform.localScale = Vector3.one;

        Destroy(efeito, tempoDestruirEfeito);
    }

    public void EsconderTodos()
    {
        if (rotinaEsquerda != null) StopCoroutine(rotinaEsquerda);
        if (rotinaCentro != null) StopCoroutine(rotinaCentro);
        if (rotinaDireita != null) StopCoroutine(rotinaDireita);

        rotinaEsquerda = null;
        rotinaCentro = null;
        rotinaDireita = null;

        if (imagemEsquerda != null)
        {
            imagemEsquerda.rectTransform.anchoredPosition = posOriginalEsquerda;
            imagemEsquerda.rectTransform.localScale = escalaOriginalEsquerda;
            imagemEsquerda.gameObject.SetActive(false);
        }

        if (imagemCentro != null)
        {
            imagemCentro.rectTransform.anchoredPosition = posOriginalCentro;
            imagemCentro.rectTransform.localScale = escalaOriginalCentro;
            imagemCentro.gameObject.SetActive(false);
        }

        if (imagemDireita != null)
        {
            imagemDireita.rectTransform.anchoredPosition = posOriginalDireita;
            imagemDireita.rectTransform.localScale = escalaOriginalDireita;
            imagemDireita.gameObject.SetActive(false);
        }

        if (imagemJogador != null)
            imagemJogador.gameObject.SetActive(false);
    }
}
