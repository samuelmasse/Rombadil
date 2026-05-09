namespace Rombadil;

public class RombadilWindow : IDisposable
{
    public event Action<double>? Render;
    public event Action? Load;
    public event Action? Unload;

    private readonly byte[] framebuffer;
    private readonly Vector2i framebufferSize;
    private readonly GameWindow window;
    private readonly float[] vertices = new float[16];
    private readonly Stopwatch lastMouse = new();

    private int vao;
    private int vbo;
    private int ebo;
    private int program;
    private int texture;

    private WindowState previousState;
    private bool isFullscreen;

    private Vector2 lastMousePosition;

    public Memory<byte> Framebuffer => framebuffer;

    public RombadilWindow()
    {
        var assembly = Assembly.GetExecutingAssembly();

        framebufferSize = (256, 240);
        framebuffer = new byte[framebufferSize.X * framebufferSize.Y * 3];

        float ntscPixelAspect = 8f / 7f;
        float correctedPixelWidth = framebufferSize.X * ntscPixelAspect;
        float targetAspect = correctedPixelWidth / framebufferSize.Y;
        var size = Monitors.GetPrimaryMonitor().ClientArea.Size * 4 / 5;
        float scale = MathF.Min(size.X / correctedPixelWidth, size.Y / framebufferSize.Y);
        int correctedWidth = (int)Math.Round(correctedPixelWidth * scale);
        int correctedHeight = (int)Math.Round(framebufferSize.Y * scale);

        using var iconStream = assembly.GetManifestResourceStream("Rombadil.Icon.png");
        var icon = Png.Open(iconStream);
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
            UpdateFrequency = 240
        };

        window.Load += () =>
        {
            LoadVao();
            LoadProgram();
            LoadTexture();
            Load?.Invoke();
        };

        window.RenderFrame += (e) =>
        {
            if (window.IsKeyPressed(Keys.F11) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ToggleFullscreen();
            ToggleMouse();

            Render?.Invoke(e.Time);

            Present();
        };
        window.Resize += (e) => Present();
        window.Unload += () => Unload?.Invoke();
    }

    public void Run() => window.Run();

    public bool IsKeyDown(Keys keys) => window.IsKeyDown(keys);
    public bool IsKeyPressed(Keys keys) => window.IsKeyPressed(keys);

    public bool IsControllerButtonDown(int index, ControllerButtons button)
    {
        var state = window.JoystickStates[index];
        if (state == null)
            return false;

        if ((int)button >= state.ButtonCount)
            return false;

        return state.IsButtonDown((int)button);
    }

    public float GetAxis(int index, ControllerAxis axis)
    {
        var state = window.JoystickStates[index];
        if (state == null)
            return 0;

        if ((int)axis >= state.ButtonCount)
            return 0;

        return state.GetAxis((int)axis);
    }

    private void Present()
    {
        int x = 0;
        int y = 0;
        int w = window.ClientSize.X;
        int h = window.ClientSize.Y;

        float targetAspect = (framebufferSize.X * 8f / 7f) / framebufferSize.Y;
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

        GL.Viewport(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y);

        GL.ClearColor(0, 0, 0, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, framebufferSize.X, framebufferSize.Y,
            PixelFormat.Rgb, PixelType.UnsignedByte, framebuffer);

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

    private void ToggleMouse()
    {
        if (window.MousePosition != lastMousePosition)
        {
            lastMouse.Restart();
            lastMousePosition = window.MousePosition;
        }

        if (lastMouse.Elapsed.TotalSeconds > 1)
        {
            if (window.CursorState != CursorState.Hidden)
                window.CursorState = CursorState.Hidden;
        }
        else if (window.CursorState != CursorState.Normal)
            window.CursorState = CursorState.Normal;
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
        program = GL.CreateProgram();
        int vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, RombadilShaders.Vert);
        GL.CompileShader(vs);
        GL.AttachShader(program, vs);

        int fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, RombadilShaders.Frag);
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
            framebufferSize.X, framebufferSize.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, framebuffer);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(vao);
        GL.DeleteBuffer(ebo);
        GL.DeleteBuffer(vbo);
        GL.DeleteProgram(program);
        GL.DeleteTexture(texture);

        window.Dispose();
    }
}
