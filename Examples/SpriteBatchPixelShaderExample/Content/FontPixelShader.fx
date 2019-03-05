
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

float4 FragmentFont(VertexToFragment input) : COLOR0
{
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