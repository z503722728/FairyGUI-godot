shader_type canvas_item;

// 模糊程度，对应 textureLod 的参数
uniform float blur_amount : hint_range(0.0, 5.0) = 2.5;
// 背景混色比例，用于叠加 UIConfig.modalLayerColor
uniform float color_mix : hint_range(0.0, 1.0) = 0.3;

// Godot 4.x: screen_texture 必须声明为 uniform sampler2D
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

void fragment() {
    // 对当前屏幕下方已渲染的 Viewport 进行采样
    vec4 screen_color = textureLod(screen_texture, SCREEN_UV, blur_amount);
    
    // COLOR 承载 FGUI 设置的遮罩色 (通过 CanvasItem.modulate)
    vec3 final_color = mix(screen_color.rgb, COLOR.rgb, COLOR.a * color_mix);
    
    COLOR = vec4(final_color, 1.0);
}
