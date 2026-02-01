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

    #region TimeScaleControl

    [Header("Time Scale Control Settings")]
    private float originalTimeScale = 1f;
    private float originalFixedDelta = 0.02f;
    private Coroutine timeScaleCoroutine;

    /// <summary>
    /// Smoothly set Time.timeScale to <paramref name="targetScale"/> over <paramref name="transition"/> seconds,
    /// keep it for <paramref name="duration"/> seconds (measured in realtime), then restore to the original time scale.
    /// </summary>
    /// <param name="targetScale">Target Time.timeScale value (absolute).</param>
    /// <param name="duration">How long (in seconds, realtime) to keep the target scale before restoring.</param>
    /// <param name="transition">How long (in seconds, realtime) to transition into/out of the target scale.</param>
    public void SetTimeScaleFor(float targetScale, float duration, float transition = 0.05f)
    {
        // Stop any existing routine and restore baseline before starting a new one
        if (timeScaleCoroutine != null)
        {
            StopCoroutine(timeScaleCoroutine);
            timeScaleCoroutine = null;
            RestoreTimeScaleImmediate();
        }

        originalTimeScale = Time.timeScale;
        originalFixedDelta = Time.fixedDeltaTime;
        timeScaleCoroutine = StartCoroutine(DoTimeScaleRoutine(targetScale, duration, transition));
    }

    /// <summary>
    /// Cancel any active time scale effect and immediately restore original time settings.
    /// </summary>
    public void CancelTimeScale()
    {
        if (timeScaleCoroutine != null)
        {
            StopCoroutine(timeScaleCoroutine);
            timeScaleCoroutine = null;
        }
        RestoreTimeScaleImmediate();
    }

    private IEnumerator DoTimeScaleRoutine(float targetScale, float duration, float transition)
    {
        // Transition in (using unscaled time so the effect duration is realtime)
        float elapsed = 0f;
        float startScale = Time.timeScale;
        while (elapsed < transition)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transition);
            Time.timeScale = Mathf.Lerp(startScale, targetScale, t);
            Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
            yield return null;
        }

        // Ensure exact target
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;

        // Hold for duration (realtime)
        float hold = 0f;
        while (hold < duration)
        {
            hold += Time.unscaledDeltaTime;
            yield return null;
        }

        // Transition out back to originalTimeScale
        elapsed = 0f;
        float fromScale = Time.timeScale;
        while (elapsed < transition)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transition);
            Time.timeScale = Mathf.Lerp(fromScale, originalTimeScale, t);
            Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
            yield return null;
        }

        // Final restore
        RestoreTimeScaleImmediate();
        timeScaleCoroutine = null;
    }

    private void RestoreTimeScaleImmediate()
    {
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;
    }
    #endregion TimeScaleControl

    #region CameraShake

    private Coroutine cameraShakeCoroutine;
    private Vector3 camOriginalLocalPos;
    private Quaternion camOriginalLocalRot;

    /// <summary>
    /// Shakes the main camera for a short time. Uses unscaled time so it is unaffected by Time.timeScale.
    /// </summary>
    /// <param name="duration">Duration in seconds (real-time).</param>
    /// <param name="magnitude">Maximum positional offset in world units.</param>
    /// <param name="frequency">How quickly the shake updates (higher = jitterier).</param>
    public void HitCameraShake(float duration = 0.2f, float magnitude = 0.15f, float frequency = 30f)
    {
        // stop previous
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
            cameraShakeCoroutine = null;
            RestoreCameraTransform();
        }

        var cam = Camera.main ?? Camera.current;
        if (cam == null)
        {
            Debug.LogWarning("HitCameraShake: no main camera found.");
            return;
        }

        camOriginalLocalPos = cam.transform.localPosition;
        camOriginalLocalRot = cam.transform.localRotation;
        cameraShakeCoroutine = StartCoroutine(DoCameraShake(cam.transform, duration, magnitude, frequency));
    }

    private IEnumerator DoCameraShake(Transform camTransform, float duration, float magnitude, float frequency)
    {
        float elapsed = 0f;
        float interval = 1f / Mathf.Max(1f, frequency);
        float timer = 0f;
        Vector3 lastOffset = Vector3.zero;

        while (elapsed < duration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            timer += dt;

            if (timer >= interval)
            {
                timer = 0f;
                // new random offset; damp over time
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                Vector3 rnd = Random.insideUnitSphere * magnitude * damper;
                // apply only X and Y for screen shake to avoid moving camera forward/back
                rnd.z = 0f;

                // remove last offset then apply new
                camTransform.localPosition = camOriginalLocalPos + rnd;
                // small rotation tilt based on offset
                camTransform.localRotation = camOriginalLocalRot * Quaternion.Euler(new Vector3(-rnd.y, rnd.x, 0f) * 5f);
                lastOffset = rnd;
            }

            yield return null;
        }

        RestoreCameraTransform();
        cameraShakeCoroutine = null;
    }

    private void RestoreCameraTransform()
    {
        var cam = Camera.main ?? Camera.current;
        if (cam == null) return;
        cam.transform.localPosition = camOriginalLocalPos;
        cam.transform.localRotation = camOriginalLocalRot;
    }

    private void OnDisable()
    {
        // ensure camera restored if this component is disabled
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
            cameraShakeCoroutine = null;
        }
        RestoreCameraTransform();
    }

    #endregion CameraShake
}
