namespace Rombadil;

public class RombadilWindow : IDisposable
{
    public event Action<double>? Render;

    private readonly byte[] framebuffer;
    private readonly Queue<short> samples;

    private readonly Vector2i framebufferSize;
    private readonly GameWindow window;
    private readonly float[] vertices = new float[16];

    private int vao;
    private int vbo;
    private int ebo;
    private int program;
    private int texture;

    private WindowState previousState;
    private bool isFullscreen;

    private Thread? audioThread;
    private bool running;
    private ALDevice device;
    private ALContext context;
    private int source;
    private int[] buffers = [];
    private AudioBuffer audioBuffer = new(0xFFFF, 16);

    public Memory<byte> Framebuffer => framebuffer;
    public Queue<short> Samples => samples;

    public RombadilWindow()
    {
        framebufferSize = (256, 240);
        framebuffer = new byte[framebufferSize.X * framebufferSize.Y * 3];
        samples = [];

        float ntscPixelAspect = 8f / 7f;
        float correctedPixelWidth = framebufferSize.X * ntscPixelAspect;
        float targetAspect = correctedPixelWidth / framebufferSize.Y;
        var size = Monitors.GetPrimaryMonitor().ClientArea.Size * 4 / 5;
        float scale = MathF.Min(size.X / correctedPixelWidth, size.Y / framebufferSize.Y);
        int correctedWidth = (int)Math.Round(correctedPixelWidth * scale);
        int correctedHeight = (int)Math.Round(framebufferSize.Y * scale);

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

            while (samples.Count > 0)
                audioBuffer.Add(samples.Dequeue());
            audioBuffer.Submit();

            Present();
        };
        window.Resize += (e) => Present();
        window.Load += () => StartAudio();
        window.Unload += () => StopAudio();
    }

    private void StartAudio()
    {
        device = ALC.OpenDevice(null);
        context = ALC.CreateContext(device, (int[])null!);
        ALC.MakeContextCurrent(context);

        source = AL.GenSource();
        buffers = AL.GenBuffers(3);

        running = true;
        audioThread = new Thread(AudioLoop) { IsBackground = true };
        audioThread.Start();
    }

    private void StopAudio()
    {
        running = false;
        audioThread?.Join();

        AL.SourceStop(source);
        AL.DeleteSource(source);
        AL.DeleteBuffers(buffers);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
    }

    private void AudioLoop()
    {
        foreach (var buffer in buffers)
        {
            AL.BufferData(buffer, ALFormat.Mono16, audioBuffer.Output, 44100);
            AL.SourceQueueBuffer(source, buffer);
        }

        AL.SourcePlay(source);

        var sample = new short[0xFFFF];
        while (audioBuffer.Delay < 5 && running)
            Thread.Sleep(1);

        while (running)
        {
            Console.WriteLine(audioBuffer.Delay);

            while (audioBuffer.Delay < 2 && running)
            {
                Console.WriteLine("too early");
                while (audioBuffer.Delay < 5 && running)
                    Thread.Sleep(1);
            }

            while (audioBuffer.Delay > 6)
            {
                Console.WriteLine("too late");
                audioBuffer.Retrieve();
            }

            int target = audioBuffer.Delay switch
            {
                2 => 738,
                3 => 737,
                4 => 735,
                5 => 734,
                6 => 730,
                _ => 736
            };

            AL.GetSource(source, ALGetSourcei.BuffersProcessed, out int processed);
            for (int i = 0; i < processed; i++)
            {
                int buffer = AL.SourceUnqueueBuffer(source);
                audioBuffer.Retrieve();
                var src = audioBuffer.Output;
                var dst = sample.AsSpan()[..target];
                ResampleSinc(src, dst);
                AL.BufferData(buffer, ALFormat.Mono16, (ReadOnlySpan<short>)dst, 44100);
                AL.SourceQueueBuffer(source, buffer);
            }

            AL.GetSource(source, ALGetSourcei.SourceState, out int state);
            if ((ALSourceState)state != ALSourceState.Playing)
            {
                AL.SourcePlay(source);
                Console.WriteLine("restart");
            }

            Thread.Sleep(2);
        }
    }

    public static void ResampleSinc(ReadOnlySpan<short> src, Span<short> dst, int taps = 32)
    {
        int srcLen = src.Length;
        int dstLen = dst.Length;
        double rate = (double)srcLen / dstLen;

        for (int i = 0; i < dstLen; i++)
        {
            double pos = i * rate;
            int center = (int)pos;
            double frac = pos - center;

            double sum = 0;
            double norm = 0;

            for (int t = -taps; t <= taps; t++)
            {
                int idx = center + t;
                if (idx < 0 || idx >= srcLen)
                    continue;

                double x = t - frac;
                double sinc = x == 0 ? 1 : Math.Sin(Math.PI * x) / (Math.PI * x);
                double window = 0.5 + 0.5 * Math.Cos(Math.PI * x / taps);
                double weight = sinc * window;

                sum += src[idx] * weight;
                norm += weight;
            }

            dst[i] = (short)Math.Clamp(sum / norm, short.MinValue, short.MaxValue);
        }
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

        GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);

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
            framebufferSize.X, framebufferSize.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, framebuffer);
    }

    private string Content(string file) => Path.Combine(AppContext.BaseDirectory, file);

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
