layout(std140, set = 0, binding = 0) uniform m_BlurParameters
{
    vec2 g_TexSize;
    float g_Sigma;
    vec2 g_Pos;
};

layout(set = 1, binding = 0) uniform texture2D m_Texture;
layout(set = 1, binding = 1) uniform sampler m_Sampler;

layout(location = 2) in vec2 v_TexCoord;
layout(location = 0) out vec4 o_Colour;

#define SAMPLES 128

void main()
{
    vec2 uv = v_TexCoord;
    vec2 delta = uv - g_Pos;

    float aspect = g_TexSize.x / g_TexSize.y;
    float invAspect = 1.0 / aspect;
    delta.x *= aspect;

    float angleStep = ((g_Sigma - 0.5) * 4.0) / float(SAMPLES);
    float sinStep = sin(angleStep);
    float cosStep = cos(angleStep);

    vec4 color = vec4(0.0);
    float validSamples = 0.0;
    vec2 rotated = delta;

    for (int i = 0; i < SAMPLES; ++i)
    {
        vec2 sampleUV = g_Pos + vec2(rotated.x * invAspect, rotated.y);

        vec2 inside = step(vec2(0.0), sampleUV) * step(sampleUV, vec2(1.0));
        float factor = inside.x * inside.y;

        color += texture(sampler2D(m_Texture, m_Sampler), sampleUV) * factor;
        validSamples += factor;

        rotated = vec2(
            cosStep * rotated.x - sinStep * rotated.y,
            sinStep * rotated.x + cosStep * rotated.y
        );
    }

    if (validSamples > 0.0)
        o_Colour = color / validSamples;
    else
        o_Colour = texture(sampler2D(m_Texture, m_Sampler), uv);
}