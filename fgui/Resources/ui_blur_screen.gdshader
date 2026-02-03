shader_type canvas_item;

// 模糊程度，对应 textureLod 的参数
uniform float blur_amount : hint_range(0.0, 5.0) = 2.5;
// 背景混色比例，用于叠加 UIConfig.modalLayerColor
uniform float color_mix : hint_range(0.0, 1.0) = 0.3;

void fragment() {
    // 关键：对当前屏幕下方已渲染的 Viewport 进行采样
    // 使用 textureLod 实现硬件加速的模糊效果
    vec4 screen_color = textureLod(screen_texture, SCREEN_UV, blur_amount);
    
    // 叠加遮罩颜色 (MODULATE 携带了 FGUI 设置的遮罩色)
    vec3 final_color = mix(screen_color.rgb, MODULATE.rgb, MODULATE.a * color_mix);
    
    COLOR = vec4(final_color, 1.0);
}
