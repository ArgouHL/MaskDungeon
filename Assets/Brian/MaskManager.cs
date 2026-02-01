using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskManager : MonoBehaviour
{
   [SerializeField] Sprite[] maskIcons;

   public Image maskImage;
   public Image[] sideMasks;

    [Header("Side Mask Fade Settings")]
    public float sideFadeOutBeforeChange = 0.25f;
    public float sideFadeInDuration = 0.5f;

    // track running coroutines per side index so we can cancel/replace
    private Dictionary<int, Coroutine> sideCoroutines = new Dictionary<int, Coroutine>();

    private void Awake()
    {
        // disable maskImage and all side masks initially
        if (maskImage != null) maskImage.enabled = false;
        if (sideMasks != null)
        {
            for (int i = 0; i < sideMasks.Length; i++)
            {
                if (sideMasks[i] != null) sideMasks[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// Update side masks according to the targets list.
    /// Behavior:
    /// 1) Any currently enabled side mask will be faded to transparent (alpha->0) then disabled.
    /// 2) Starting from the end of the targets list, for each side mask index i (0..), check the corresponding
    ///    value targets[targets.Count-1 - i]. If that value >= 1, set sideMasks[i].sprite = maskIcons[value-1].
    ///    Otherwise leave/disable the side mask.
    /// 3) For side masks that received a new sprite, enable them and fade their alpha from 0 to original.
    /// </summary>
    public void UpdateSideMasks(List<int> targets)
    {
        StartCoroutine(UpdateSideMasksRoutine(targets));
    }

    private System.Collections.IEnumerator UpdateSideMasksRoutine(List<int> targets)
    {
        if (sideMasks == null) yield break;

        // store original alphas
        float[] originalAlphas = new float[sideMasks.Length];
        for (int i = 0; i < sideMasks.Length; i++)
        {
            originalAlphas[i] = 1f;
        }

        // 1) Fade currently enabled masks to transparent then disable
        for (int i = 0; i < sideMasks.Length; i++)
        {
            var img = sideMasks[i];
            if (img == null) continue;
            if (!img.enabled) continue;

            // stop existing coroutine for this side
            if (sideCoroutines.ContainsKey(i))
            {
                StopCoroutine(sideCoroutines[i]);
                sideCoroutines.Remove(i);
            }

            // start fade to 0 then disable
            var c = StartCoroutine(FadeToAlphaAndDisable(img, originalAlphas[i], 0f, sideFadeOutBeforeChange, i));
            sideCoroutines[i] = c;
        }

        // wait for fade out to finish
        yield return new WaitForSeconds(sideFadeOutBeforeChange);

        // 2) Assign new sprites based on the targets list from its end
        int countTargets = (targets != null) ? targets.Count : 0;
        for (int i = 0; i < sideMasks.Length; i++)
        {
            var img = sideMasks[i];
            if (img == null) continue;

            int idxFromEnd = countTargets - 1 - i -1;
            if (idxFromEnd >= 0)
            {
                int val = targets[idxFromEnd];
                if (val >= 1 && val <= maskIcons.Length)
                {
                    // set sprite (val is 1-based) and prepare to enable + fade in
                    img.sprite = maskIcons[val - 1];
                    // ensure transparent before fade in
                    var ccol = img.color;
                    ccol.a = 0f;
                    img.color = ccol;
                    img.enabled = true;

                    // stop existing coroutine if any
                    if (sideCoroutines.ContainsKey(i))
                    {
                        StopCoroutine(sideCoroutines[i]);
                        sideCoroutines.Remove(i);
                    }

                    var coro = StartCoroutine(FadeToAlpha(img, 0f, originalAlphas[i], sideFadeInDuration, i));
                    sideCoroutines[i] = coro;
                    continue;
                }
            }

            // no valid target for this side -> ensure disabled
            if (sideCoroutines.ContainsKey(i))
            {
                StopCoroutine(sideCoroutines[i]);
                sideCoroutines.Remove(i);
            }
            img.enabled = false;
        }
    }

    private System.Collections.IEnumerator FadeToAlphaAndDisable(Image img, float from, float to, float duration, int sideIndex)
    {
        yield return FadeToAlpha(img, from, to, duration, sideIndex);
        if (img != null) img.enabled = false;
        if (sideCoroutines.ContainsKey(sideIndex)) sideCoroutines.Remove(sideIndex);
    }

    private System.Collections.IEnumerator FadeToAlpha(Image img, float from, float to, float duration, int sideIndex)
    {
        if (img == null) yield break;
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
        if (sideCoroutines.ContainsKey(sideIndex)) sideCoroutines.Remove(sideIndex);
    }

    // smoothly change to black in 1s (not imediately), then reverse to new icon and white in 0.5s
    public System.Collections.IEnumerator MaskIconChange(int target)
    {
        float duration = 1f;
        float elapsed = 0f;
        Color originalColor = Color.white;

        if (maskImage.enabled == false)
        {
            maskImage.color = Color.black;
            maskImage.enabled = true;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                maskImage.color = Color.Lerp(originalColor, Color.black, t);
                yield return null;
            }
        }

        if(target < 0 || target >= maskIcons.Length)
        {
            maskImage.enabled = false;
            yield break;
        }

        maskImage.sprite = maskIcons[target];

        duration = 0.5f;
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            maskImage.color = Color.Lerp(Color.black, originalColor, t);
            yield return null;
        }
    }
}
