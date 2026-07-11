namespace Rombadil.Nes.Emulator;

internal sealed class NesCartridgeRam(
    int volatileLength,
    int nonVolatileLength,
    NesBatteryRam battery,
    int batteryOffset)
{
    private readonly byte[] volatileData = new byte[volatileLength];

    public int Length => volatileData.Length + nonVolatileLength;

    public byte Read(int index)
    {
        if (Length == 0)
            return 0;

        index %= Length;
        return index < nonVolatileLength
            ? battery.Read(batteryOffset + index)
            : volatileData[index - nonVolatileLength];
    }

    public void Write(int index, byte value)
    {
        if (Length == 0)
            return;

        index %= Length;
        if (index < nonVolatileLength)
            battery.Write(batteryOffset + index, value);
        else
            volatileData[index - nonVolatileLength] = value;
    }
}
