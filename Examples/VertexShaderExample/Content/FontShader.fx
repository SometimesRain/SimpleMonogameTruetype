
//--------------------------- BASIC PROPERTIES ------------------------------

float2 HalfWindowSize;
float2 Position;
float2 Size;
float4 Color;

texture Texture;

sampler2D textureSampler = sampler_state {
	Texture = (Texture);
	MagFilter = None;
	MinFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

//--------------------------- DATA STRUCTURES -------------------------------

struct ApplicationToVertex
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexToFragment
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

//------------------------------- SHADERS -----------------------------------

//Not using the standard world, view and projection matrices
//  Instead maps the model coordinates to screen using simple 2D math
//    Top-left corner of the screen is (0, 0) and bottom-right is (width, height)
VertexToFragment VertexFont(ApplicationToVertex input)
{
    VertexToFragment output;
	
	input.Position.y = -input.Position.y;
	input.Position.xy *= Size / HalfWindowSize;
	input.Position.xy -= 1;
	
	//Position is the top-left coordinate
    output.Position.xy = input.Position.xy + (Position + Size / 2) / HalfWindowSize;
	
	output.Position.y = -output.Position.y;
    output.Position.z = 0;
    output.Position.w = 1;
	
	output.TexCoord.x = input.TexCoord.x;
	output.TexCoord.y = 1 - input.TexCoord.y;

    return output;
}

float4 FragmentFont(VertexToFragment input) : COLOR0
{
	return float4(Color.rgb, tex2D(textureSampler, input.TexCoord).a);
}

//------------------------------ TECHNIQUES ---------------------------------

technique Font
{
	pass Pass1
	{
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
		VertexShader = compile vs_4_0 VertexFont();
		PixelShader = compile ps_4_0 FragmentFont();
	}
}