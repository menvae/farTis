layout(std140, set = 0, binding = 0) uniform m_MotionBlurParameters
{
    vec2 g_TexSize;
    vec2 g_Direction;
    int g_Radius;
    float g_Sigma;
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

vec4 blur(int radius, vec2 direction, vec2 texCoord, vec2 texSize, float sigma)
{
    float factor = computeGauss(0.0, sigma);
    vec4 sum = texture(sampler2D(m_Texture, m_Sampler), texCoord) * factor;
    float totalFactor = factor;

    for (int i = 2; i <= 200; i += 2)
    {
        float x = float(i) - 0.5;
        factor = computeGauss(x, sigma) * 2.0;
        totalFactor += 2.0 * factor;

        sum += texture(sampler2D(m_Texture, m_Sampler), texCoord + direction * x / texSize) * factor;
        sum += texture(sampler2D(m_Texture, m_Sampler), texCoord - direction * x / texSize) * factor;

        if (i >= radius) break;
    }

    return sum / totalFactor;
}

void main()
{
    vec2 uv = gl_FragCoord.xy / g_TexSize;
    o_Colour = blur(g_Radius, g_Direction, uv, g_TexSize, g_Sigma);
}
