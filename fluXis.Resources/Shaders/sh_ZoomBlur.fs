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

#define SAMPLES 200

void main()
{
    vec2 uv = v_TexCoord;
    vec2 delta = uv - g_Pos;

    float sigma = (g_Sigma - 0.5) * 4.0;
    float scaleStep = sigma / float(SAMPLES);

    vec4 color = vec4(0.0);
    float validSamples = 0.0;

    for (int i = 0; i < SAMPLES; ++i)
    {
        float scale = 1.0 - float(i) * scaleStep;
        vec2 sampleUV = g_Pos + delta * scale;

        vec2 inside = step(vec2(0.0), sampleUV) * step(sampleUV, vec2(1.0));
        float factor = inside.x * inside.y;

        color += texture(sampler2D(m_Texture, m_Sampler), sampleUV) * factor;
        validSamples += factor;
    }

    if (validSamples > 0.0)
        o_Colour = color / validSamples;
    else
        o_Colour = texture(sampler2D(m_Texture, m_Sampler), uv);
}