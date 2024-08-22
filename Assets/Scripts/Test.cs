using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {

    }
    bool isPlay = false;
    float blend = 0;
    //播放音源
    IEnumerator PlayAudioSource()
    {
        yield return 0;
        if (audioSource != null)
        {
            audioSource.Play();
            isPlay = true;
            yield return new WaitForSeconds(audioSource.clip.length);
            isPlay = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            audioSource.Play();
        }
        //blend根据isPlay线性 插值
        blend = Mathf.Lerp(blend, isPlay ? 1 : 0, Time.deltaTime * 5);
        animator.SetFloat("Blend", blend);
    }
}
