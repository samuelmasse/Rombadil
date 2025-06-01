namespace Rombadil;

public class Canvas : IDisposable
{
    public event Action<double>? Render;

    private readonly Pixels pixels;
    private readonly GameWindow window;

    private int vao;
    private int vbo;
    private int ebo;
    private int program;
    private int texture;

    public Canvas(Pixels pixels)
    {
        this.pixels = pixels;

        float ntscPixelAspect = 8f / 7f;
        var size = Monitors.GetPrimaryMonitor().ClientArea.Size * 4 / 5;
        int scale = Math.Min(size.X / (int)(pixels.Size.X * ntscPixelAspect), size.Y / pixels.Size.Y);
        int correctedWidth = (int)(pixels.Size.X * ntscPixelAspect) * scale;
        int correctedHeight = pixels.Size.Y * scale;

        window = new(new(), new()
        {
            Title = "Rombadil",
            ClientSize = (correctedWidth, correctedHeight),
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
        GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);

        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, pixels.Size.X, pixels.Size.Y,
            PixelFormat.Rgb, PixelType.UnsignedByte, pixels.Data);

        GL.UseProgram(program);
        GL.BindVertexArray(vao);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        window.SwapBuffers();
    }

    private void LoadVao()
    {
        float[] vertices = [
            -1f, -1f, 0f, 1f,
            1f, -1f, 1f, 1f,
            1f,  1f, 1f, 0f,
            -1f,  1f, 0f, 0f
        ];

        uint[] indices = [
            0, 1, 2,
            2, 3, 0
        ];

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

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
        var vertexSource = File.ReadAllText("Shader.vert");
        var fragSource = File.ReadAllText("Shader.frag");

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

    public void Dispose() => window.Dispose();
}
