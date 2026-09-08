using UnityEngine;
using TMPro;

public class IntroPrompt : MonoBehaviour
{
    public Movement movement; 
    public GameObject promptText;

    public float fadeOutDelay = 0.5f;

    private bool started = false;
    private float timer = 0f;

    public float showDelay = 1f;
    private bool shown = false;

    public float fadeCharDuration = 0.04f;
    private TMP_Text tmp;

    private bool fading = false;

    void Start()
    {
        if (movement != null) movement.frozen = true; 
        if (promptText != null) promptText.SetActive(false);
        if (promptText != null) tmp = promptText.GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!shown)
        {
            timer += Time.deltaTime;
            if (timer >= showDelay)
            {
                shown = true;
                timer = 0f;
                if (promptText != null) promptText.SetActive(true);
            }
            return;
        }

        if (started)
        {
            timer += Time.deltaTime;
            if (timer >= fadeOutDelay && !fading)
            {
                fading = true;
                StartCoroutine(FadeOutRightToLeft());
            }
            return;
        }

        var kb = UnityEngine.InputSystem.Keyboard.current;
        var pad = UnityEngine.InputSystem.Gamepad.current;

        bool pressed =
            (kb != null && kb.anyKey.wasPressedThisFrame) ||
            (pad != null && (pad.leftStick.ReadValue().sqrMagnitude > 0.1f ||
                             pad.dpad.ReadValue().sqrMagnitude > 0.1f ||
                             pad.buttonSouth.wasPressedThisFrame));

        if (pressed)
        {
            started = true;
            if (movement != null) movement.frozen = false;
        }
    }

    System.Collections.IEnumerator FadeOutRightToLeft()
    {
        if (tmp == null) { promptText.SetActive(false); yield break; }

        tmp.ForceMeshUpdate();
        int total = tmp.textInfo.characterCount;

        for (int i = total; i >= 0; i--)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(fadeCharDuration * (i / (float)total +0.03f));
        }
        promptText.SetActive(false);
    }
}