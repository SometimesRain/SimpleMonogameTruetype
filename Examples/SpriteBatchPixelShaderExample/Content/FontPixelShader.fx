
//--------------------------- BASIC PROPERTIES ------------------------------

texture Texture;

sampler2D textureSampler = sampler_state {
	Texture = (Texture);
	MagFilter = None;
	MinFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

//--------------------------- DATA STRUCTURES -------------------------------

struct VertexToFragment
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

//------------------------------- SHADERS -----------------------------------

float3 ColorFromHue(in float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}

float Wrap(in float value)
{
	return value - (int)value;
}

float4 FragmentFont(VertexToFragment input) : COLOR0
{
	if (input.Color.a == 0)
		input.Color.rgb = ColorFromHue(Wrap(input.Color.r + (1 - input.TexCoord.x)));
	
	input.Color.a = tex2D(textureSampler, input.TexCoord).a;
	return input.Color;
}

//------------------------------ TECHNIQUES ---------------------------------

technique Font
{
	pass Pass1
	{
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
		PixelShader = compile ps_4_0 FragmentFont();
	}
}