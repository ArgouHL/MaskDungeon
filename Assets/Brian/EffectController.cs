using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EffectController : MonoBehaviour
{
    #region WhiteEmissionFlash
    
    [Header("White Emission Flash Settings")]
    [Tooltip("Duration of the white emission flash in seconds")]
    public float flashDuration = 0.3f;
    [Tooltip("Emission intensity multiplier for the white flash")]
    public float emissionIntensity = 1f;

    // Keep track of the active coroutine so we can cancel/restore properly
    private Coroutine activeFlash;
    private List<MaterialState> materialStates = new List<MaterialState>();

    // Internal struct to save and restore material emission state
    private class MaterialState
    {
        public Material material;
        public bool hadKeyword;
        public Color originalColor;
        public bool hasProperty;
    }

    /// <summary>
    /// Trigger a white emission flash on all SkinnedMeshRenderer materials under this GameObject.
    /// </summary>
    /// <param name="duration">Flash duration in seconds. If <= 0 uses the configured flashDuration.</param>
    /// <param name="intensity">Emission intensity multiplier. If <= 0 uses the configured emissionIntensity.</param>
    public void TriggerWhiteEmission(float duration = -1f, float intensity = -1f)
    {
        if (duration <= 0f) duration = flashDuration;
        if (intensity <= 0f) intensity = emissionIntensity;

        // Stop any running flash and restore materials to their previous state
        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
            RestoreMaterials();
            activeFlash = null;
        }

        activeFlash = StartCoroutine(DoWhiteFlash(duration, intensity));
    }

    private IEnumerator DoWhiteFlash(float duration, float intensity)
    {
        materialStates.Clear();

        // Find all SkinnedMeshRenderers on this GameObject and children
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var rend in renderers)
        {
            if (rend == null) continue;

            // Use renderer.materials to get per-renderer instances (this creates instances if needed)
            var mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null) continue;

                var state = new MaterialState();
                state.material = mat;
                state.hasProperty = mat.HasProperty("_EmissionColor");
                state.hadKeyword = mat.IsKeywordEnabled("_EMISSION");
                state.originalColor = state.hasProperty ? mat.GetColor("_EmissionColor") : Color.black;
                materialStates.Add(state);

                // Enable emission and set to white * intensity
                if (state.hasProperty)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.white * intensity);
                }
            }
        }

        // Wait for the requested duration
        yield return new WaitForSeconds(duration);

        // Restore previous material states
        RestoreMaterials();
        activeFlash = null;
    }

    private void RestoreMaterials()
    {
        foreach (var s in materialStates)
        {
            if (s == null || s.material == null) continue;
            if (s.hasProperty)
            {
                s.material.SetColor("_EmissionColor", s.originalColor);
                if (!s.hadKeyword)
                    s.material.DisableKeyword("_EMISSION");
            }
        }
        materialStates.Clear();
    }

    private void OnDestroy()
    {
        // Ensure materials are restored if object is destroyed while flashing
        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
            activeFlash = null;
        }
        RestoreMaterials();
    }

    #endregion WhiteEmissionFlash

    #region PopDamage
    
    [Header("Damage Popup Settings")]
    [Tooltip("Prefab optional: if assigned, will be used instead of creating runtime TextMeshProUGUI objects")]
    public GameObject popPrefab;

    [Tooltip("Vertical offset from the transform or head where the popup will appear")]
    public Vector3 popOffset = new Vector3(0f, 1.8f, 0f);

    [Tooltip("How high (world units) the popup will rise during the animation")]
    public float popRise = 0.6f;

    [Tooltip("Duration of the popup (seconds)")]
    public float popDuration = 0.9f;

    [Tooltip("Minimum font size for damage text")]
    public float popMinSize = 18f;
    [Tooltip("Maximum font size for damage text")]
    public float popMaxSize = 48f;
    [Tooltip("Multiplier for mapping damage to font size")]
    public float popSizePerDamage = 0.6f;

    /// <summary>
    /// Spawn a damage popup above this GameObject's head and animate it (rise + fade).
    /// </summary>
    /// <param name="damage">Damage amount to display (int)</param>
    public void PopDamage(int damage)
    {
        // Determine spawn position: prefer child named 'Head' or 'head'
        Transform head = transform.Find("Head");
        if (head == null) head = transform.Find("head");
        Vector3 startPos = (head != null) ? head.position + popOffset : transform.position + popOffset;

        if (popPrefab != null)
        {
            // If a prefab is provided, instantiate and attempt to find a TextMeshProUGUI inside
            var go = Instantiate(popPrefab, startPos, Quaternion.identity, transform);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
                tmp.fontSize = Mathf.Clamp(popMinSize + damage * popSizePerDamage, popMinSize, popMaxSize);
            }
            // animate regardless of prefab contents
            StartCoroutine(AnimatePopup(go.transform, popDuration));
            return;
        }

        // Create runtime Canvas (world space) and TextMeshProUGUI
        GameObject canvasGO = new GameObject("_PopCanvas", typeof(Canvas));
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.position = startPos;
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        var rect = canvasGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 60f);

        // Add the text object
        GameObject textGO = new GameObject("_PopText", typeof(RectTransform));
        textGO.transform.SetParent(canvasGO.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;

        var tmpComp = textGO.AddComponent<TextMeshProUGUI>();
        tmpComp.alignment = TextAlignmentOptions.Center;
        tmpComp.text = damage.ToString();
        tmpComp.raycastTarget = false;
        tmpComp.color = Color.white;
        tmpComp.fontSize = Mathf.Clamp(popMinSize + damage * popSizePerDamage, popMinSize, popMaxSize);

        // scale down the canvas a bit so it looks like UI in world
        canvasGO.transform.localScale = Vector3.one * 0.01f;

        // Start animation coroutine
        StartCoroutine(AnimatePopup(canvasGO.transform, popDuration));
    }

    private IEnumerator AnimatePopup(Transform popupTransform, float duration)
    {
        if (popupTransform == null) yield break;

        // Cache starting values
        Vector3 startPos = popupTransform.position;
        Vector3 endPos = startPos + Vector3.up * popRise;
        float elapsed = 0f;

    // Find any TMP component to fade alpha
    var tmp = popupTransform.GetComponentInChildren<TextMeshProUGUI>();
    Color original = tmp != null ? tmp.color : Color.white;

    // Cache camera for billboard facing
    Camera cam = Camera.main;
    if (cam == null) cam = Camera.current;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // ease out for movement
            float moveT = 1f - Mathf.Pow(1f - t, 3f);
            popupTransform.position = Vector3.Lerp(startPos, endPos, moveT);

            // make the popup face the camera (billboard)
            if (cam != null)
            {
                // face the camera while keeping the up vector world-up to avoid roll
                popupTransform.rotation = Quaternion.LookRotation(popupTransform.position - cam.transform.position, Vector3.up);
            }

            // fade out towards the end
            float alpha = Mathf.Lerp(1f, 0f, t);
            if (tmp != null)
            {
                var c = original;
                c.a = alpha;
                tmp.color = c;
            }

            // slight scale up then down for pop effect
            float scale = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
            popupTransform.localScale = Vector3.one * 0.01f * scale;

            yield return null;
        }

        // destroy popup gameobject
        Destroy(popupTransform.gameObject);
    }
    
    #endregion PopDamage
}
