using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEye : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public int blinkLeftBlendIndex; // 眨眼表情在BlendShapes中的索引
    public int blinkRightBlendIndex; // 眨眼表情在BlendShapes中的索引
    public float blinkWeight = 0.0f; // 眨眼表情的初始权重值
    public float blinkDuration = 0.2f; // 眨眼动画持续时间
    public float blinkInterval = 3.0f; // 眨眼间隔时间

    private float blinkTimer = 0.0f; // 计时器，用于控制眨眼间隔
    void Start()
    {
        // 设置眨眼表情的初始权重值
        skinnedMeshRenderer.SetBlendShapeWeight(blinkLeftBlendIndex, blinkWeight);
        skinnedMeshRenderer.SetBlendShapeWeight(blinkRightBlendIndex, blinkWeight);
    }

    void Update()
    {
        blinkTimer += Time.deltaTime;

        // 如果计时器超过了眨眼间隔时间，就触发眨眼动画
        if (blinkTimer >= blinkInterval)
        {
            StartCoroutine(BlinkCoroutine());
            blinkTimer = 0.0f; // 重置计时器
        }
    }

    IEnumerator BlinkCoroutine()
    {
        // 将眨眼表情的权重值逐渐变为100，然后再逐渐恢复为0，实现眨眼动画
        for (float t = 0.0f; t < blinkDuration; t += Time.deltaTime)
        {
            float weight = Mathf.Lerp(blinkWeight, 100.0f, t / blinkDuration);

            skinnedMeshRenderer.SetBlendShapeWeight(blinkLeftBlendIndex, weight);
            skinnedMeshRenderer.SetBlendShapeWeight(blinkRightBlendIndex, weight);
            yield return null;
        }

        for (float t = 0.0f; t < blinkDuration; t += Time.deltaTime)
        {
            float weight = Mathf.Lerp(100.0f, blinkWeight, t / blinkDuration);
            skinnedMeshRenderer.SetBlendShapeWeight(blinkLeftBlendIndex, weight);
            skinnedMeshRenderer.SetBlendShapeWeight(blinkRightBlendIndex, weight);

            yield return null;
        }

        // 将眨眼表情的权重值恢复为初始值
        skinnedMeshRenderer.SetBlendShapeWeight(blinkLeftBlendIndex, blinkWeight);
        skinnedMeshRenderer.SetBlendShapeWeight(blinkRightBlendIndex, blinkWeight);
    }
}
