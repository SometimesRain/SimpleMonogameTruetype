## A dead simple TrueType or OpenType runtime font renderer for MonoGame
Built on top of the excellent C library [stb_truetype](https://github.com/nothings/stb).

![](/Pictures/SpriteBatchExample.png)

### Usage

```C#
// Load font (accepts filename or system font name)
Font font = new Font("Arial");

// Generate bitmap data
BitmapData data = font.GenerateBitmapData("Hello, MonoGame!", 64);

// Create a texture and set its data
Texture2D fontTexture = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);
fontTexture.SetData(data.Alphas);
```

### Examples included

* Rendering with SpriteBatch
* Rendering with custom pixel shader
* Resizable text field
* Input field
* Rendering without SpriteBatch
