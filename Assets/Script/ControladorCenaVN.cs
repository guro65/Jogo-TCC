using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ControladorCenaVN : MonoBehaviour
{
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;
    public Image imagemJogador;

    public Color corFalando = Color.white;
    public Color corNaoFalando = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Efeitos visuais de fala")]
    public bool usarAnimacaoDeFala = true;
    public float alturaPulo = 18f;
    public float duracaoPulo = 0.18f;
    public float escalaAoFalar = 1.04f;
    public GameObject efeitoFalaPrefab;

    private Coroutine animacaoAtual;

    public void AtualizarPersonagem(Image imagemDestino, DadosPersonagem personagem, Emocao emocao, bool mostrar)
    {
        if (imagemDestino == null)
            return;

        if (!mostrar || personagem == null)
        {
            imagemDestino.gameObject.SetActive(false);
            return;
        }

        imagemDestino.gameObject.SetActive(true);
        imagemDestino.sprite = personagem.ObterSpritePorEmocao(emocao);
        imagemDestino.color = corNaoFalando;
    }

    public void AtualizarJogador(AparenciaJogador aparencia, Emocao emocao)
    {
        if (imagemJogador == null || aparencia == null)
            return;

        imagemJogador.gameObject.SetActive(true);
        imagemJogador.sprite = aparencia.ObterSpritePorEmocao(emocao);
    }

    public void DestacarFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        if (imagemEsquerda != null)
            imagemEsquerda.color = (falante == esquerda) ? corFalando : corNaoFalando;

        if (imagemCentro != null)
            imagemCentro.color = (falante == centro) ? corFalando : corNaoFalando;

        if (imagemDireita != null)
            imagemDireita.color = (falante == direita) ? corFalando : corNaoFalando;
    }

    public void AnimarFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        if (!usarAnimacaoDeFala || falante == null)
            return;

        Image imagemFalante = ObterImagemDoFalante(falante, esquerda, centro, direita);

        if (imagemFalante == null || !imagemFalante.gameObject.activeInHierarchy)
            return;

        if (animacaoAtual != null)
            StopCoroutine(animacaoAtual);

        animacaoAtual = StartCoroutine(AnimarImagemFalante(imagemFalante));
    }

    Image ObterImagemDoFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        if (falante == esquerda) return imagemEsquerda;
        if (falante == centro) return imagemCentro;
        if (falante == direita) return imagemDireita;

        return null;
    }

    IEnumerator AnimarImagemFalante(Image imagem)
    {
        RectTransform rect = imagem.GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        Vector2 posicaoOriginal = rect.anchoredPosition;
        Vector3 escalaOriginal = rect.localScale;

        if (efeitoFalaPrefab != null)
        {
            GameObject efeito = Instantiate(efeitoFalaPrefab, rect.parent);
            RectTransform rectEfeito = efeito.GetComponent<RectTransform>();

            if (rectEfeito != null)
                rectEfeito.anchoredPosition = rect.anchoredPosition;

            Destroy(efeito, 1.2f);
        }

        float metadeDuracao = duracaoPulo / 2f;
        float tempo = 0f;

        while (tempo < metadeDuracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / metadeDuracao);
            rect.anchoredPosition = Vector2.Lerp(posicaoOriginal, posicaoOriginal + Vector2.up * alturaPulo, t);
            rect.localScale = Vector3.Lerp(escalaOriginal, escalaOriginal * escalaAoFalar, t);
            yield return null;
        }

        tempo = 0f;

        while (tempo < metadeDuracao)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / metadeDuracao);
            rect.anchoredPosition = Vector2.Lerp(posicaoOriginal + Vector2.up * alturaPulo, posicaoOriginal, t);
            rect.localScale = Vector3.Lerp(escalaOriginal * escalaAoFalar, escalaOriginal, t);
            yield return null;
        }

        rect.anchoredPosition = posicaoOriginal;
        rect.localScale = escalaOriginal;
    }

    public void EsconderTodos()
    {
        if (imagemEsquerda != null) imagemEsquerda.gameObject.SetActive(false);
        if (imagemCentro != null) imagemCentro.gameObject.SetActive(false);
        if (imagemDireita != null) imagemDireita.gameObject.SetActive(false);
        if (imagemJogador != null) imagemJogador.gameObject.SetActive(false);
    }
}