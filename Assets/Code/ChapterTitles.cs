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

    public CanvasGroup subtitleLine;
    public float subtitleDelay = 3f;

    private bool[] played = new bool[3];

    public float chapter1Delay = 2f;

    public CanvasGroup subtitleLine2;
    public float subtitle2Delay = 3f;

    public CanvasGroup subtitleLine3;
    public float subtitle3Delay = 0.5f;

    void Start()
    {
        if (chapter1 != null) chapter1.alpha = 0f;
        if (chapter2 != null) chapter2.alpha = 0f;
        if (chapter3 != null) chapter3.alpha = 0f;
        if (subtitleLine != null) subtitleLine.alpha = 0f;
        if (subtitleLine2 != null) subtitleLine2.alpha = 0f;
        if (subtitleLine3 != null) subtitleLine3.alpha = 0f;
    }

    public void ShowChapter(int index)   // 1, 2, 3
    {
        if (index < 1 || index > 3) return;
        if (played[index - 1]) return;
        played[index - 1] = true;

        CanvasGroup cg = index == 1 ? chapter1 : (index == 2 ? chapter2 : chapter3);

        if (index == 1)
        {
            if (cg != null) StartCoroutine(DelayedFade(cg, chapter1Delay));
        }
        else
        {
            if (cg != null) StartCoroutine(FadeInOut(cg));
        }

        if (index == 1 && subtitleLine != null)
            StartCoroutine(ShowSubtitle());

        if (index == 2 && subtitleLine2 != null)
            StartCoroutine(ShowSubtitle2());

        if (index == 3 && subtitleLine3 != null)
            StartCoroutine(ShowSubtitle3());
    }

    IEnumerator DelayedFade(CanvasGroup cg, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return FadeInOut(cg);
    }

    IEnumerator ShowSubtitle()
    {
        yield return new WaitForSeconds(subtitleDelay);
        yield return FadeInOut(subtitleLine);
    }

    IEnumerator ShowSubtitle2()
    {
        yield return new WaitForSeconds(subtitle2Delay);
        yield return FadeInOut(subtitleLine2);
    }

    IEnumerator ShowSubtitle3()
    {
        yield return new WaitForSeconds(subtitle3Delay);
        yield return FadeInOut(subtitleLine3);
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