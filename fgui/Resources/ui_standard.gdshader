shader_type canvas_item;

// --- 基础参数 ---
uniform float gray_amount : hint_range(0.0, 1.0) = 0.0;

// --- 特效参数 ---
uniform float shine_speed : hint_range(0.0, 10.0) = 0.0;
uniform float shine_width : hint_range(0.0, 1.0) = 0.2;
uniform vec4 shine_color : source_color = vec4(1.0, 1.0, 1.0, 0.5);
uniform float hue_shift : hint_range(-1.0, 1.0) = 0.0;

// --- 混合模式参数 ---
// 0: Normal(Mix), 1: Screen, 2: Overlay, 3: Multiply
uniform float blend_mode = 0.0;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

// 传递原始顶点色（不含 TEXTURE），避免 fragment 中 COLOR 已乘过 TEXTURE 导致双重乘法
varying vec4 vertex_modulate;

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

void vertex() {
    // COLOR 此时 = 顶点色 × modulate × self_modulate（尚未乘 TEXTURE）
    vertex_modulate = COLOR;
}

void fragment() {
    // 手动采样纹理，不依赖 COLOR（因为 fragment 的 COLOR 已被 Godot 乘过 TEXTURE）
    vec4 tex_color = texture(TEXTURE, UV);
    vec3 color = tex_color.rgb;

    // 1. 色相偏移
    if (abs(hue_shift) > 0.001) {
        vec3 hsv = rgb2hsv(color);
        hsv.x = fract(hsv.x + hue_shift);
        color = hsv2rgb(hsv);
    }

    // 2. 流光
    if (shine_speed > 0.0) {
        float shine_pos = fract(TIME * shine_speed);
        float edge0 = shine_pos - shine_width;
        float shine_mask = smoothstep(edge0, shine_pos, UV.x + UV.y)
                         * smoothstep(shine_pos + shine_width, shine_pos, UV.x + UV.y);
        color += shine_color.rgb * shine_mask * shine_color.a;
    }

    // 用 vertex_modulate（不含 TEXTURE）乘以手动采样的纹理 —— 与默认管线一致
    vec4 final_output = vec4(color, tex_color.a) * vertex_modulate;

    // 3. 变灰（必须在乘以 modulate 之后，否则 modulate 颜色会重新染色）
    // FairyGUI 编辑器在 sRGB 空间直接算灰度，不做 gamma 往返
    if (gray_amount > 0.001) {
        float gray = dot(final_output.rgb, vec3(0.299, 0.587, 0.114));
        final_output.rgb = mix(final_output.rgb, vec3(gray), gray_amount);
    }

    // 4. 混合模式（Overlay / Multiply 需要读背景）
    // 注意: Screen 模式不支持（Godot 无法模拟 Unity 的 Blend One OneMinusSrcColor）
    if (blend_mode > 1.5) {
        if (final_output.a < 0.001) {
            discard;
        }

        vec3 base = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;
        vec3 result;

        if (blend_mode < 2.5) {
            // Overlay
            result = mix(
                2.0 * base * final_output.rgb,
                1.0 - 2.0 * (1.0 - base) * (1.0 - final_output.rgb),
                step(0.5, dot(base, vec3(0.299, 0.587, 0.114)))
            );
            final_output.rgb = mix(base, result, final_output.a);
        }
        else {
            // Multiply
            result = pow(base * final_output.rgb, vec3(0.8));
            final_output.rgb = mix(base, result, final_output.a);
        }

        final_output.a = 1.0;
    }

    COLOR = final_output;
}
