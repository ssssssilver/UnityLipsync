## 说明
运行环境
Unity2021.3.18f1c1
HDRP12.1.10

工程中的模型与动画是通过CC4与IClone8生成,并通过插件cc_unity_tools_HDRP导入到工程中   
本质上还是对接quest的OVRLipSync那一套，只是将CC4的模型与OVRLipSync重新对接了一次。  
驱动嘴型需要把脸部的部分动画删除，ctrl+d把模型的动画复制一份,然后手动删除即可
![avatar](/output/1.jpg)
参考视频
<video width="320" height="240" controls>
  <source src="/output/中.mp4" type="video/mp4">
</video>
<video width="320" height="240" controls>
  <source src="/output/英.mp4" type="video/mp4">
</video>



参考资料
https://developer.oculus.com/documentation/unity/audio-ovrlipsync-viseme-reference/
https://github.com/soupday/cc_unity_tools_HDRP
https://www.youtube.com/watch?v=va-qyFa312g