shader_type canvas_item;

// --- 基础实例参数 ---
instance uniform float gray_amount : hint_range(0.0, 1.0) = 0.0;

// --- 特效参数 ---
instance uniform float shine_speed : hint_range(0.0, 10.0) = 0.0;
instance uniform float shine_width : hint_range(0.0, 1.0) = 0.2;
instance uniform vec4 shine_color : source_color = vec4(1.0, 1.0, 1.0, 0.5);
instance uniform float hue_shift : hint_range(-1.0, 1.0) = 0.0;

// --- 混合模式参数 ---
// 0: Normal, 1: Screen, 2: Overlay
instance uniform int blend_mode = 0;

// 辅助函数
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    vec3 color = tex_color.rgb;

    // 1. 色相偏移
    if (abs(hue_shift) > 0.001) {
        vec3 hsv = rgb2hsv(color);
        hsv.x = fract(hsv.x + hue_shift);
        color = hsv2rgb(hsv);
    }

    // 2. 变灰
    float gray = dot(color, vec3(0.299, 0.587, 0.114));
    color = mix(color, vec3(gray), gray_amount);

    // 3. 流光
    if (shine_speed > 0.0) {
        float shine_pos = fract(TIME * shine_speed);
        float edge0 = shine_pos - shine_width;
        float edge1 = shine_pos;
        float shine_mask = smoothstep(edge0, shine_pos, UV.x + UV.y) * smoothstep(shine_pos + shine_width, shine_pos, UV.x + UV.y);
        color += shine_color.rgb * shine_mask * shine_color.a;
    }

    // 最终准备输出的颜色 (叠加 FGUI 的颜色调节)
    vec4 final_output = vec4(color, tex_color.a) * MODULATE;

    // 4. 高级混合模式 (Screen / Overlay 对背景)
    if (blend_mode > 0) {
        // 采样当前屏幕背景
        vec3 base = texture(screen_texture, SCREEN_UV).rgb;
        vec3 blend = final_output.rgb;
        vec3 result;

        if (blend_mode == 1) { // Screen
            result = 1.0 - (1.0 - base) * (1.0 - blend);
        } 
        else if (blend_mode == 2) { // Overlay
            result = mix(
                2.0 * base * blend,
                1.0 - 2.0 * (1.0 - base) * (1.0 - blend),
                step(0.5, dot(base, vec3(0.299, 0.587, 0.114))) // 使用亮度判断
            );
        }
        
        final_output.rgb = mix(final_output.rgb, result, final_output.a);
    }

    COLOR = final_output;
}
