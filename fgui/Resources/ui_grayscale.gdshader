shader_type canvas_item;

// 使用 instance uniform 确保 1000 个元件共用一个材质也能独立变灰
instance uniform float gray_amount : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
    // 心理学加权灰度公式
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    vec3 gray_color = vec3(gray);
    // 在彩色和灰色之间插值
    COLOR.rgb = mix(color.rgb, gray_color, gray_amount);
    COLOR.a = color.a;
    // 叠加 Godot 原有的 Modulate 颜色（FGUI 很多效果依赖这个）
    COLOR *= MODULATE;
}
