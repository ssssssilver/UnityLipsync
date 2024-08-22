using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendTreeBlend : MonoBehaviour
{
    [SerializeField] private Animator animator;
    //写一个方法 获取aniamtor中的Blend属性,让它的值以一定的速度在0-1之间来回变化
    private void Start()
    {
        StartCoroutine(Blend());
    }
    private IEnumerator Blend()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            for (float i = 0; i <= 1f; i += Time.deltaTime * 0.2f)
            {
                animator.SetFloat("Blend", i);
                yield return 0;
            }
            yield return new WaitForSeconds(5);
            for (float i = 1; i >= 0; i -= Time.deltaTime * 0.2f)
            {
                animator.SetFloat("Blend", i);
                yield return 0;
            }
        }
    }
}
