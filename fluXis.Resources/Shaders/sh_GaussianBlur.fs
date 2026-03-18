layout(std140, set = 0, binding = 0) uniform m_BlurParameters
{
    vec2 g_TexSize;
    int g_Radius;
    float g_Sigma;
    vec2 g_Direction;
};

layout(set = 1, binding = 0) uniform texture2D m_Texture;
layout(set = 1, binding = 1) uniform sampler m_Sampler;

layout(location = 2) in vec2 v_TexCoord;
layout(location = 0) out vec4 o_Colour;

#define INV_SQRT_2PI 0.39894

float computeGauss(in float x, in float sigma)
{
    return INV_SQRT_2PI * exp(-0.5*x*x / (sigma*sigma)) / sigma;
}

void main()
{
    vec2 uv = gl_FragCoord.xy / g_TexSize;
    float factor = computeGauss(0.0, g_Sigma);
    vec4 sum = texture(sampler2D(m_Texture, m_Sampler), uv) * factor;
    float totalFactor = factor;

    for (int i = 2; i <= 200; i += 2)
    {
        float x = float(i) - 0.5;
        factor = computeGauss(x, g_Sigma) * 2.0;
        totalFactor += 2.0 * factor;

        sum += texture(sampler2D(m_Texture, m_Sampler), uv + g_Direction * x / g_TexSize) * factor;
        sum += texture(sampler2D(m_Texture, m_Sampler), uv - g_Direction * x / g_TexSize) * factor;

        if (i >= g_Radius) break;
    }

    o_Colour = sum / totalFactor;
}