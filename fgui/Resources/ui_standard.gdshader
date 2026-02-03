shader_type canvas_item;

// 核心：使用 instance 关键字实现单 DrawCall 合批
instance uniform float gray_amount : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    
    // 心理学加权灰度
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    vec3 gray_color = vec3(gray);
    
    // 基础颜色混合
    COLOR.rgb = mix(color.rgb, gray_color, gray_amount);
    COLOR.a = color.a;
    
    // 必须叠加 Godot 原生的色彩调节，否则 FGUI 的颜色、淡入淡出动效会失效
    COLOR *= MODULATE;
}
