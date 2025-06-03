namespace Rombadil;

public class Canvas : IDisposable
{
    public event Action<double>? Render;

    private readonly Pixels pixels;
    private readonly GameWindow window;
    private readonly float[] vertices = new float[16];

    private int vao;
    private int vbo;
    private int ebo;
    private int program;
    private int texture;

    private WindowState previousState;
    private bool isFullscreen;

    public Canvas(Pixels pixels)
    {
        this.pixels = pixels;

        float ntscPixelAspect = 8f / 7f;
        float correctedPixelWidth = pixels.Size.X * ntscPixelAspect;
        float targetAspect = correctedPixelWidth / pixels.Size.Y;
        var size = Monitors.GetPrimaryMonitor().ClientArea.Size * 4 / 5;
        float scale = MathF.Min(size.X / correctedPixelWidth, size.Y / pixels.Size.Y);
        int correctedWidth = (int)Math.Round(correctedPixelWidth * scale);
        int correctedHeight = (int)Math.Round(pixels.Size.Y * scale);

        var icon = Png.Open(Content("Icon.png"));
        var data = new byte[icon.Width * icon.Height * 4];

        for (int y = 0; y < icon.Height; y++)
        {
            for (int x = 0; x < icon.Width; x++)
            {
                var pixel = icon.GetPixel(x, y);
                int index = (y * icon.Width + x) * 4;
                data[index + 0] = pixel.R;
                data[index + 1] = pixel.G;
                data[index + 2] = pixel.B;
                data[index + 3] = pixel.A;
            }
        }

        window = new(new(), new()
        {
            Title = "Rombadil",
            ClientSize = (correctedWidth, correctedHeight),
            Icon = new([new(icon.Width, icon.Height, data)])
        })
        {
            UpdateFrequency = 60
        };

        window.Load += () =>
        {
            LoadVao();
            LoadProgram();
            LoadTexture();
        };

        window.RenderFrame += (e) =>
        {
            if (window.IsKeyPressed(Keys.F11))
                ToggleFullscreen();

            Render?.Invoke(e.Time);
            Present();
        };
        window.Resize += (e) => Present();

    }

    public void Run()
    {
        window.Run();
    }

    public bool IsKeyDown(Keys keys) => window.IsKeyDown(keys);
    public bool IsKeyPressed(Keys keys) => window.IsKeyPressed(keys);

    private void Present()
    {
        int x = 0;
        int y = 0;
        int w = window.ClientSize.X;
        int h = window.ClientSize.Y;

        float targetAspect = (pixels.Size.X * 8f / 7f) / pixels.Size.Y;
        float windowAspect = (float)w / h;

        if (windowAspect > targetAspect)
        {
            w = (int)Math.Round(h * targetAspect);
            x = (window.ClientSize.X - w) / 2;
        }
        else
        {
            h = (int)Math.Round(w / targetAspect);
            y = (window.ClientSize.Y - h) / 2;
        }

        SetVertices(x, y, w, h);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

        GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);

        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, pixels.Size.X, pixels.Size.Y,
            PixelFormat.Rgb, PixelType.UnsignedByte, pixels.Data);

        GL.UseProgram(program);
        GL.BindVertexArray(vao);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        window.SwapBuffers();
    }

    private void ToggleFullscreen()
    {
        if (isFullscreen)
        {
            window.WindowState = previousState;
            isFullscreen = false;
        }
        else
        {
            previousState = window.WindowState;

            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;

            window.WindowState = WindowState.Fullscreen;
            isFullscreen = true;
        }
    }

    private void SetVertices(int x, int y, int width, int height)
    {
        float left = 2f * x / window.ClientSize.X - 1f;
        float right = 2f * (x + width) / window.ClientSize.X - 1f;
        float top = 1f - 2f * y / window.ClientSize.Y;
        float bottom = 1f - 2f * (y + height) / window.ClientSize.Y;

        vertices[0] = left; vertices[1] = bottom;
        vertices[2] = 0f; vertices[3] = 1f;

        vertices[4] = right; vertices[5] = bottom;
        vertices[6] = 1f; vertices[7] = 1f;

        vertices[8] = right; vertices[9] = top;
        vertices[10] = 1f; vertices[11] = 0f;

        vertices[12] = left; vertices[13] = top;
        vertices[14] = 0f; vertices[15] = 0f;
    }

    private void LoadVao()
    {
        uint[] indices = [
            0, 1, 2,
            2, 3, 0
        ];

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        GL.BindVertexArray(0);
    }

    private void LoadProgram()
    {
        var vertexSource = File.ReadAllText(Content("Shader.vert"));
        var fragSource = File.ReadAllText(Content("Shader.frag"));

        program = GL.CreateProgram();
        int vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vertexSource);
        GL.CompileShader(vs);
        GL.AttachShader(program, vs);

        int fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, fragSource);
        GL.CompileShader(fs);
        GL.AttachShader(program, fs);

        GL.LinkProgram(program);
        GL.DetachShader(program, vs);
        GL.DetachShader(program, fs);
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
    }

    private void LoadTexture()
    {
        texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
            pixels.Size.X, pixels.Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, pixels.Data);
    }

    private string Content(string file) => Path.Combine(AppContext.BaseDirectory, file);

    public void Dispose() => window.Dispose();
}
