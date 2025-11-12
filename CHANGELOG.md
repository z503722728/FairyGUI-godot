2025/11/12：  
&emsp;&emsp;完成了对字体部分的自绘改造，现在GTextField，GRichTextField，GTextInput不在基于godot自身的相关控件实现，而是自绘实现，仅由godot字体提供字形数据，现在可以完美支持FGui的各种文字效果，比如UBB,比如内嵌图片。由于godot自己的字形缓冲纹理过小切不可更改，导致中文字的渲染dc异常的高，所以字形缓冲纹理也自行构建，大大降低了dc，字形缓冲纹理大小可油UIConfig.glyphCacheTexSize设置，UIConfig.textOutlineType可设置描边的实现方式，TextFormat.TextOutlineType.Godot采用godot提供的阴影，效果更好但至少会使所需dc翻倍且占用额外的字形缓冲纹理。UIConfig.minFontSize/UIConfig.maxFontSize/UIConfig.fontSizeLevels可以对所以用的采样字体大小进行标准化，从而减少实际用于采样的字体数量。另外由于godot的字体不支持访问字体本身提供的斜体和粗体，所以斜体和粗体的表现会和FGui在编辑器中的表现略有差异。
2025/9/22：  
&emsp;&emsp;增加了对组件遮罩的支持，实现上使用了godot本身的遮罩功能，不过他不支持反向遮罩，所以反向遮罩是通过反转渲染的Alpha值，并增加空白部分的渲染来实现  

