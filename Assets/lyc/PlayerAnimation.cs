using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimation : MonoBehaviour
{
    public Transform armature;
    public float speed = 2f;

    public Vector2 upAmplitudeRange = new Vector2(0.006f, 0.012f);
    public Vector2 upFrequencyRange = new Vector2(0.8f, 1.2f);

    public Vector2 forwardAmplitudeRange = new Vector2(0.003f, 0.008f);
    public Vector2 forwardFrequencyRange = new Vector2(0.6f, 1.0f);

    public float baseAmplitude = 1f;

    public float rootRotateSpeed = 6f;
    public float rootMaxAngle = 10f;

    class TentacleData
    {
        public Transform root;

        public float upAmplitude;
        public float upFrequency;

        public float forwardAmplitude;
        public float forwardFrequency;

        public float maxIndex;

        public Quaternion originalLocalRotation;
        public float rootPhase;
    }

    class BoneData
    {
        public Transform bone;
        public Transform parent;
        public Vector3 originalLocalPos;
        public TentacleData tentacle;
        public float phase;
        public float index01;
    }

    List<BoneData> bones = new List<BoneData>();
    Dictionary<Transform, TentacleData> tentacleMap =
        new Dictionary<Transform, TentacleData>();

    void Start()
    {
        if (armature == null) return;

        Transform[] all = armature.GetComponentsInChildren<Transform>();

        foreach (Transform t in all)
        {
            if (t == armature) continue;
            if (t.parent == null) continue;

            Transform root = FindRootBone(t);
            if (root == t) continue;

            int index = GetChildIndexFromRoot(t, root);

            if (!tentacleMap.TryGetValue(root, out TentacleData tentacle))
            {
                tentacle = new TentacleData();
                tentacle.root = root;

                tentacle.upAmplitude =
                    Random.Range(upAmplitudeRange.x, upAmplitudeRange.y);
                tentacle.upFrequency =
                    Random.Range(upFrequencyRange.x, upFrequencyRange.y);

                tentacle.forwardAmplitude =
                    Random.Range(forwardAmplitudeRange.x, forwardAmplitudeRange.y);
                tentacle.forwardFrequency =
                    Random.Range(forwardFrequencyRange.x, forwardFrequencyRange.y);

                tentacle.originalLocalRotation = root.localRotation;
                tentacle.rootPhase = Random.value * Mathf.PI * 2f;
                tentacle.maxIndex = 0f;

                tentacleMap.Add(root, tentacle);
            }

            tentacle.maxIndex =
                Mathf.Max(tentacle.maxIndex, index);

            BoneData b = new BoneData();
            b.bone = t;
            b.parent = t.parent;
            b.originalLocalPos = t.localPosition;
            b.tentacle = tentacle;
            b.phase = index * 0.5f * Mathf.PI;
            b.index01 = index;

            bones.Add(b);
        }

        foreach (var b in bones)
        {
            if (b.tentacle.maxIndex > 0f)
            {
                b.index01 /= b.tentacle.maxIndex;
            }
        }
    }

    void LateUpdate()
    {
        float time = Time.time * speed;

        foreach (var pair in tentacleMap)
        {
            TentacleData t = pair.Value;

            float angle =
                Mathf.Sin(time * rootRotateSpeed + t.rootPhase)
                * rootMaxAngle;

            t.root.localRotation =
                t.originalLocalRotation *
                Quaternion.AngleAxis(angle, Vector3.right);
        }

        foreach (var b in bones)
        {
            float attenuation =
                Mathf.Lerp(1f, 0.5f, b.index01);

            float upWave =
                Mathf.Sin(
                    time * 2f * b.tentacle.upFrequency
                    + b.phase
                )
                * b.tentacle.upAmplitude
                * attenuation * baseAmplitude;

            float forwardWave =
                Mathf.Sin(
                    time * 2f * b.tentacle.forwardFrequency
                    + b.phase
                )
                * b.tentacle.forwardAmplitude
                * attenuation * baseAmplitude;

            Vector3 worldOffset =
                upWave * Vector3.up +
                forwardWave * Vector3.forward;

            Vector3 localOffset =
                b.parent.InverseTransformVector(worldOffset);

            b.bone.localPosition =
                b.originalLocalPos + localOffset;
        }
    }

    public void Attack()
    {
        StartCoroutine(OneZeroOne(0.2f, value => baseAmplitude = value));
    }

    System.Collections.IEnumerator OneZeroOne(float k, System.Action<float> setter)
    {
        float t = 0f;

        while (t < k)
        {
            t += Time.deltaTime;
            setter(Mathf.Lerp(1f, 0f, t / k));
            yield return null;
        }

        t = 0f;

        while (t < k)
        {
            t += Time.deltaTime;
            setter(Mathf.Lerp(0f, 1f, t / k));
            yield return null;
        }

        setter(1f);
    }

    Transform FindRootBone(Transform t)
    {
        Transform current = t;
        while (current.parent != armature && current.parent != null)
        {
            current = current.parent;
        }
        return current;
    }

    int GetChildIndexFromRoot(Transform t, Transform root)
    {
        int index = 0;
        Transform current = t;
        while (current != root && current.parent != null)
        {
            index++;
            current = current.parent;
        }
        return index - 1;
    }
}
