using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class AudioForReallusionLipSync : MonoBehaviour
{
    [Tooltip("Which lip sync provider to use for viseme computation.")]
    public OVRLipSync.ContextProviders provider = OVRLipSync.ContextProviders.Enhanced;
    [Tooltip("Enable DSP offload on supported Android devices.")]
    public bool enableAcceleration = true;
    [SerializeField] private uint Context = 0;
    [SerializeField] public float gain = 1.0f;

    [Tooltip("Skinned Mesh Renderer Component for the head of the character.")]
    public SkinnedMeshRenderer HeadSkinnedMeshRenderer;

    [Tooltip("Skinned Mesh Renderer Component for the teeth of the character, if available. Leave empty if not.")]
    public SkinnedMeshRenderer TeethSkinnedMeshRenderer;

    [Tooltip("Skinned Mesh Renderer Component for the tongue of the character, if available. Leave empty if not.")]
    public SkinnedMeshRenderer TongueSkinnedMeshRenderer;

    [Tooltip("Game object with the bone of the jaw for the character, if available. Leave empty if not.")]
    public GameObject jawBone;

    [Tooltip("Game object with the bone of the tongue for the character, if available. Leave empty if not.")]
    public GameObject tongueBone; // even though actually tongue doesn't have a bone

    [Tooltip("Set a custom position for the tongue bone so that it looks natural.")]
    [SerializeField]
    private Vector3 tongueBoneOffset = new(-0.01f, 0.015f, 0f);

    [Tooltip("The index of the first blendshape that will be manipulated.")]
    public int firstIndex;
    /// <summary>
    /// 音频
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;

    /// <summary>
    /// blendshape权重倍数
    /// </summary>
    public float blendWeightMultiplier = 100f;
    /// <summary>
    /// 设置每个口型对应的blendershape的索引
    /// </summary>
    [Header("设置元音对应的blendershape的索引值")]
    public VisemeBlenderShapeIndexMap m_VisemeIndex;

    /// <summary>
    /// 音素分析结果
    /// </summary>
    private OVRLipSync.Frame frame = new OVRLipSync.Frame();
    protected OVRLipSync.Frame Frame
    {
        get
        {
            return frame;
        }
    }

    private void Awake()
    {
        if (Context == 0)
        {
            if (OVRLipSync.CreateContext(ref Context, provider, 0, enableAcceleration)
                != OVRLipSync.Result.Success)
            {
                Debug.LogError("OVRLipSyncContextBase.Start ERROR: Could not create" +
                    " Phoneme context.");
                return;
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        ProcessAudioSamplesRaw(data, channels);
    }

    /// <summary>
    /// Pass F32 PCM audio buffer to the lip sync module
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="channels">Channels.</param>
    public void ProcessAudioSamplesRaw(float[] data, int channels)
    {
        // Send data into Phoneme context for processing (if context is not 0)
        lock (this)
        {
            if (OVRLipSync.IsInitialized() != OVRLipSync.Result.Success)
            {
                return;
            }
            var frame = this.Frame;
            OVRLipSync.ProcessFrame(Context, data, frame, channels == 2);
        }
    }

    private void Update()
    {
        if (this.Frame != null)
        {
            SetBlenderShapes();
            if (tongueBone != null) tongueBone.transform.localPosition = tongueBoneOffset;
        }
        else
        {
            jawBone.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
            tongueBone.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -5.0f);

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(1 + firstIndex, 0f); // V_Explosive

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(2 + firstIndex, 0f); // V_Dental_Lip

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex, 0f); // Mouth_Drop_Lower

            TongueSkinnedMeshRenderer.SetBlendShapeWeight(2 + firstIndex, 0f); // V_Tongue_Out

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex, 0f); // Mouth_Shrug_Upper

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(7 + firstIndex, 0f); // V_Lip_Open

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(76 + firstIndex, 0f); // Mouth_Press_L

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(77 + firstIndex, 0f); // Mouth_Press_R

            HeadSkinnedMeshRenderer.SetBlendShapeWeight(3 + firstIndex, 0f); // V_Tight_O

            // Jaw Bone
            jawBone.transform.localEulerAngles
                = new Vector3(0.0f, 0.0f, -90.0f);

            // Tongue Bone
            tongueBone.transform.localEulerAngles
                = new Vector3(0.0f, 0.0f, -5f);
        }

    }


    private void SetBlenderShapes()
    {

        float weight;
        float alpha = 1.0f;

        float weightMultiplier = 100f;

        jawBone.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
        tongueBone.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -5.0f);

        // Set blend shape weights for various visemes on the HeadSkinnedMeshRenderer.
        // PP
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(1 + firstIndex,
            Frame.Visemes[1] * weight * alpha * weightMultiplier); // V_Explosive

        // FF
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(2 + firstIndex,
            Frame.Visemes[2] * weight * alpha * weightMultiplier); // V_Dental_Lip

        // TH
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[3] * weight * alpha * weightMultiplier); // Mouth_Drop_Lower

        // DD
        weight = 0.2f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[4] * weight / 0.7f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[4] * weight / 0.7f * alpha * weightMultiplier); // Mouth_Shrug_Upper

        // KK
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[5] * weight / 1.5f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[5] * weight / 1.5f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // CH
        weight = 0.7f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[6] * weight / 2.7f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[6] * weight / 2.7f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(7 + firstIndex,
            Frame.Visemes[6] * weight / 2.7f * alpha * weightMultiplier); // V_Lip_Open

        // SS
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[7] * weight / 1.5f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[7] * weight / 1.5f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // NN
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[8] * weight / 2.0f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[8] * weight / 2.0f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // RR
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[9] * weight / 0.9f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // AA
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[10] * weight / 2.0f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // EE
        weight = 0.7f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[11] * weight * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 0.3f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[11] * weight * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // II
        weight = 0.7f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(117 + firstIndex,
            Frame.Visemes[12] * weight / 1.2f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(117)); // Mouth_Drop_Lower
        weight = 0.5f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(114 + firstIndex,
            Frame.Visemes[12] * weight / 1.2f * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(114)); // Mouth_Shrug_Upper

        // OO
        weight = 1.2f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(3 + firstIndex,
           Frame.Visemes[13] * weight * alpha * weightMultiplier); // V_Tight_O

        // UU
        weight = 1.0f;
        HeadSkinnedMeshRenderer.SetBlendShapeWeight(3 + firstIndex,
            Frame.Visemes[14] * weight * alpha * weightMultiplier
            + HeadSkinnedMeshRenderer.GetBlendShapeWeight(3)); // V_Tight_O

        // Adjust the jaw and tongue bone rotations based on the specific viseme values.
        jawBone.transform.localEulerAngles
            = new Vector3(0.0f, 0.0f, -90.0f - (
                    0.2f * Frame.Visemes[3]
                    + 0.1f * Frame.Visemes[4]
                    + 0.5f * Frame.Visemes[5]
                    + 0.2f * Frame.Visemes[8]
                    + 0.2f * Frame.Visemes[9]
                    + 1.0f * Frame.Visemes[10]
                    + 0.2f * Frame.Visemes[11]
                    + 0.3f * Frame.Visemes[12]
                    + 0.8f * Frame.Visemes[13]
                    + 0.3f * Frame.Visemes[14]
                )
                / (0.2f + 0.1f + 0.5f + 0.2f + 0.2f + 1.0f + 0.2f + 0.3f + 0.8f + 0.3f)
                * 30f);

        // Tongue Bone
        tongueBone.transform.localEulerAngles
            = new Vector3(0.0f, 0.0f, (
                    0.1f * Frame.Visemes[3]
                    + 0.2f * Frame.Visemes[8]
                    + 0.15f * Frame.Visemes[9]
                )
                / (0.1f + 0.2f + 0.15f)
                * 80f - 5f);

    }

    private int GetBlenderShapeIndexByName(string _name)
    {
        if (_name == "sil")
        {
            return 999;
        }
        if (_name == "PP")
        {
            return m_VisemeIndex.PP;
        }
        if (_name == "FF")
        {
            return m_VisemeIndex.FF;
        }
        if (_name == "TH")
        {
            return m_VisemeIndex.TH;
        }
        if (_name == "DD")
        {
            return m_VisemeIndex.DD;
        }

        if (_name == "kk")
        {
            return m_VisemeIndex.kk;
        }
        if (_name == "CH")
        {
            return m_VisemeIndex.CH;
        }
        if (_name == "SS")
        {
            return m_VisemeIndex.SS;
        }
        if (_name == "nn")
        {
            return m_VisemeIndex.nn;
        }
        if (_name == "RR")
        {
            return m_VisemeIndex.RR;
        }
        if (_name == "aa")
        {
            return m_VisemeIndex.aa;
        }

        if (_name == "E")
        {
            return m_VisemeIndex.E;
        }
        if (_name == "ih")
        {
            return m_VisemeIndex.ih;
        }
        if (_name == "oh")
        {
            return m_VisemeIndex.oh;
        }
        if (_name == "ou")
        {
            return m_VisemeIndex.ou;
        }

        return m_VisemeIndex.ou;
    }

    [System.Serializable]
    public class VisemeBlenderShapeIndexMap
    {
        public int sil;
        public int PP;
        public int FF;
        public int TH;
        public int DD;
        public int kk;
        public int CH;
        public int SS;
        public int nn;
        public int RR;
        public int aa;
        public int E;
        public int ih;
        public int oh;
        public int ou;

    }


    private SkinnedMeshRenderer GetHeadSkinnedMeshRendererWithRegex(Transform parentTransform)
    {
        // Initialize a variable to store the found SkinnedMeshRenderer.
        SkinnedMeshRenderer findFaceSkinnedMeshRenderer = null;

        // Define a regular expression pattern for matching child object names.
        Regex regexPattern = new("(.*_Head|CC_Base_Body)");

        // Iterate through each child of the parentTransform.
        foreach (Transform child in parentTransform)
            // Check if the child's name matches the regex pattern.
            if (regexPattern.IsMatch(child.name))
            {
                // If a match is found, get the SkinnedMeshRenderer component of the child.
                findFaceSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                // If a SkinnedMeshRenderer is found, break out of the loop.
                if (findFaceSkinnedMeshRenderer != null) break;
            }

        // Return the found SkinnedMeshRenderer (or null if none is found).
        return findFaceSkinnedMeshRenderer;
    }


    /// <summary>
    ///     This function finds the Teeth skinned mesh renderer components, if present,
    ///     in the children of the parentTransform using regex.
    /// </summary>
    /// <param name="parentTransform">The parent transform whose children are searched.</param>
    /// <returns>The SkinnedMeshRenderer component of the Teeth, if found; otherwise, null.</returns>
    private SkinnedMeshRenderer GetTeethSkinnedMeshRendererWithRegex(Transform parentTransform)
    {
        // Initialize a variable to store the found SkinnedMeshRenderer for teeth.
        SkinnedMeshRenderer findTeethSkinnedMeshRenderer = null;

        // Define a regular expression pattern for matching child object names.
        Regex regexPattern = new("(.*_Teeth|CC_Base_Body)");

        // Iterate through each child of the parentTransform.
        foreach (Transform child in parentTransform)
            // Check if the child's name matches the regex pattern.
            if (regexPattern.IsMatch(child.name))
            {
                // If a match is found, get the SkinnedMeshRenderer component of the child.
                findTeethSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                // If a SkinnedMeshRenderer is found, break out of the loop.
                if (findTeethSkinnedMeshRenderer != null) break;
            }

        // Return the found SkinnedMeshRenderer for teeth (or null if none is found).
        return findTeethSkinnedMeshRenderer;
    }


    /// <summary>
    ///     This function finds the Tongue skinned mesh renderer components, if present,
    ///     in the children of the parentTransform using regex.
    /// </summary>
    /// <param name="parentTransform">The parent transform whose children are searched.</param>
    /// <returns>The SkinnedMeshRenderer component of the Tongue, if found; otherwise, null.</returns>
    private SkinnedMeshRenderer GetTongueSkinnedMeshRendererWithRegex(Transform parentTransform)
    {
        // Initialize a variable to store the found SkinnedMeshRenderer for the tongue.
        SkinnedMeshRenderer findTongueSkinnedMeshRenderer = null;

        // Define a regular expression pattern for matching child object names.
        Regex regexPattern = new("(.*_Tongue|CC_Base_Body)");

        // Iterate through each child of the parentTransform.
        foreach (Transform child in parentTransform)
            // Check if the child's name matches the regex pattern.
            if (regexPattern.IsMatch(child.name))
            {
                // If a match is found, get the SkinnedMeshRenderer component of the child.
                findTongueSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

                // If a SkinnedMeshRenderer is found, break out of the loop.
                if (findTongueSkinnedMeshRenderer != null) break;
            }

        // Return the found SkinnedMeshRenderer for the tongue (or null if none is found).
        return findTongueSkinnedMeshRenderer;
    }
}
