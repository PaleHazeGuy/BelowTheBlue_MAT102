namespace BelowTheBlue.Input
{
    public interface IPlayerInputSource
    {
        bool IsAvailable { get; }
        PlayerInputState Read();
    }
}
