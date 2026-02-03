shader_type canvas_item;

// --- 基础实例参数 ---
instance uniform float gray_amount : hint_range(0.0, 1.0) = 0.0;

// --- 流光效果 (Shine) 参数 ---
instance uniform float shine_speed : hint_range(0.0, 10.0) = 0.0; // 速度为0即关闭
instance uniform float shine_width : hint_range(0.0, 1.0) = 0.2;
instance uniform vec4 shine_color : source_color = vec4(1.0, 1.0, 1.0, 0.5);

// --- 色相偏移 (Hue Shift) 参数 ---
instance uniform float hue_shift : hint_range(-1.0, 1.0) = 0.0; // 偏移为0即关闭

// RGB 转 HSV 工具函数
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// HSV 转 RGB 工具函数
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    vec3 color = tex_color.rgb;

    // 1. 应用色相偏移 (Hue Shift)
    if (abs(hue_shift) > 0.001) {
        vec3 hsv = rgb2hsv(color);
        hsv.x = fract(hsv.x + hue_shift);
        color = hsv2rgb(hsv);
    }

    // 2. 应用变灰 (Grayscale)
    float gray = dot(color, vec3(0.299, 0.587, 0.114));
    color = mix(color, vec3(gray), gray_amount);

    // 3. 应用流光 (Shine)
    if (shine_speed > 0.0) {
        // 使用 UV 和时间计算倾斜的光带
        float shine_pos = fract(TIME * shine_speed);
        float edge0 = shine_pos - shine_width;
        float edge1 = shine_pos;
        // 简单的线性光带计算
        float shine_mask = smoothstep(edge0, shine_pos, UV.x + UV.y) * smoothstep(shine_pos + shine_width, shine_pos, UV.x + UV.y);
        color += shine_color.rgb * shine_mask * shine_color.a;
    }

    COLOR = vec4(color, tex_color.a) * MODULATE;
}
