layout(set = 0, binding = 0) uniform texture2D u_Original;
layout(set = 0, binding = 1) uniform sampler s_Original;

layout(set = 1, binding = 0) uniform texture2D u_Blurred;
layout(set = 1, binding = 1) uniform sampler s_Blurred;

layout(set = 2, binding = 0) uniform m_BlurComposeParameters
{
    vec2 g_texSize;
    float g_Strength;
};

layout(location = 2) in vec2 v_TexCoord;
layout(location = 0) out vec4 o_Colour;

void main()
{
    vec2 UV = v_TexCoord * g_texSize;
    vec3 scene = texture(sampler2D(u_Original, s_Original), v_TexCoord).rgb;
    vec3 blur  = texture(sampler2D(u_Blurred,  s_Blurred),  UV).rgb;
    o_Colour = vec4(scene + blur * g_Strength, 1.0);
}