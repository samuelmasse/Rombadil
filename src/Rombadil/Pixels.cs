namespace Rombadil;

public class Pixels(Vector2i size)
{
    private readonly byte[] data = new byte[size.X * size.Y * 3];

    public Vector2i Size => size;
    public byte[] Data => data;

    public ref byte this[int index] => ref data[index];
}
