2025/12/22：  
<blockquote>更新了一系列的优化和Bug修复：<br>
  <blockquote>增加GTextField.SetTextTranslater，提供对显示文本进行处理的接口<br>
  修复初始化后GRoot的大小不能立即为正确值的问题<br>
  修复屏幕拉伸情况下字体显示不正常的问题<br>
  修复异步操作未在主线程完成的问题<br>
  修复GRoot被异常放大2倍的问题<br>
  修复RichTextField和InputTextField有可能发生无限UpdateMesh的问题<br>
  取消了NImage,NShape,TextField的_Process定义，以避免_Process带来的巨大消耗，这些的IDisplayObject的onUpdate委托将是不起作用的<br>
  优化Window的显隐和DisplayGear带来的显隐，不再从节点树移出，而是只修改Visible属性<br>
  优化整体性能，以低消耗的Node2D节点替换高消耗的Control节点，同时自行实现focus机制<br>
  将原有的NContainer分为NContainer和NClipContainer，NContainer改为继承于Node2D以提高效率，但不支持裁剪和遮罩<br></blockquote>
</blockquote>
2025/11/12：
<blockquote>&emsp;&emsp;完成了对字体部分的自绘改造，现在GTextField，GRichTextField，GTextInput不在基于godot自身的相关控件实现，而是自绘实现，仅由godot字体提供字形数据，现在可以完美支持FGui的各种文字效果，比如UBB,比如内嵌图片。由于godot自己的字形缓冲纹理过小切不可更改，导致中文字的渲染dc异常的高，所以字形缓冲纹理也自行构建，大大降低了dc，字形缓冲纹理大小可油UIConfig.glyphCacheTexSize设置，UIConfig.textOutlineType可设置描边的实现方式，TextFormat.TextOutlineType.Godot采用godot提供的阴影，效果更好但至少会使所需dc翻倍且占用额外的字形缓冲纹理。UIConfig.minFontSize/UIConfig.maxFontSize/UIConfig.fontSizeLevels可以对所以用的采样字体大小进行标准化，从而减少实际用于采样的字体数量。另外由于godot的字体不支持访问字体本身提供的斜体和粗体，所以斜体和粗体的表现会和FGui在编辑器中的表现略有差异。</blockquote>
2025/9/22：  
<blockquote>&emsp;&emsp;增加了对组件遮罩的支持，实现上使用了godot本身的遮罩功能，不过他不支持反向遮罩，所以反向遮罩是通过反转渲染的Alpha值，并增加空白部分的渲染来实现。</blockquote>


