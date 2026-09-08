using System.Collections;
using UnityEngine;

public class EndingSequence : MonoBehaviour
{
    public SpriteRenderer[] fadeOutSprites;

    public SpriteRenderer blackBlock;
    public SpriteRenderer whiteBlock;

    public SpriteRenderer titleImage;
    public Vector3 titleOffset = new Vector3(0f, -2f, 0f);

    public float fadeOutDuration = 2f; 
    public float pauseBeforeTitle = 1f; 
    public float titleFadeInDuration = 2f;

    public CanvasGroup credits;
    public float creditsDelay = 2f;
    public float creditsFadeInDuration = 2f;

    public void PlayEnding()
    {
        StartCoroutine(EndingRoutine());
    }

    IEnumerator EndingRoutine()
    {
        if (credits != null) credits.alpha = 0f;

        float t = 0f;
        Color[] startColors = new Color[fadeOutSprites.Length];
        for (int i = 0; i < fadeOutSprites.Length; i++)
            if (fadeOutSprites[i] != null) startColors[i] = fadeOutSprites[i].color;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeOutDuration;
            for (int i = 0; i < fadeOutSprites.Length; i++)
            {
                if (fadeOutSprites[i] == null) continue;
                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, 0f, t);
                fadeOutSprites[i].color = c;
            }
            yield return null;
        }

        yield return new WaitForSeconds(pauseBeforeTitle);

        if (titleImage != null && blackBlock != null)
        {
            titleImage.transform.position = blackBlock.transform.position + titleOffset;
            titleImage.gameObject.SetActive(true);

            Color tc = titleImage.color;
            tc.a = 0f;
            titleImage.color = tc;

            float t2 = 0f;
            while (t2 < 1f)
            {
                t2 += Time.deltaTime / titleFadeInDuration;
                tc.a = Mathf.Lerp(0f, 1f, t2);
                titleImage.color = tc;
                yield return null;
            }

            if (credits != null)
            {
                yield return new WaitForSeconds(creditsDelay);

                float t3 = 0f;
                while (t3 < 1f)
                {
                    t3 += Time.deltaTime / creditsFadeInDuration;
                    credits.alpha = Mathf.Lerp(0f, 1f, t3);
                    yield return null;
                }
            }

            Debug.Log("Ending complete");
        }
    }
}