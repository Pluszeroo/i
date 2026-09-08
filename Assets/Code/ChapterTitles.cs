using System.Collections;
using UnityEngine;
using TMPro;

public class ChapterTitles : MonoBehaviour
{
    public CanvasGroup chapter1;
    public CanvasGroup chapter2;
    public CanvasGroup chapter3;

    public float fadeInDuration = 1.5f;
    public float holdDuration = 2.5f;
    public float fadeOutDuration = 1.5f;

    private bool[] played = new bool[3];

    void Start()
    {
        if (chapter1 != null) chapter1.alpha = 0f;
        if (chapter2 != null) chapter2.alpha = 0f;
        if (chapter3 != null) chapter3.alpha = 0f;
    }

    public void ShowChapter(int index)   // 1, 2, 3
    {
        if (index < 1 || index > 3) return;
        if (played[index - 1]) return;
        played[index - 1] = true;

        CanvasGroup cg = index == 1 ? chapter1 : (index == 2 ? chapter2 : chapter3);
        if (cg != null) StartCoroutine(FadeInOut(cg));
    }

    IEnumerator FadeInOut(CanvasGroup cg)
    {
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / fadeInDuration; cg.alpha = t; yield return null; }
        cg.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < 1f) { t += Time.deltaTime / fadeOutDuration; cg.alpha = 1f - t; yield return null; }
        cg.alpha = 0f;
    }
}