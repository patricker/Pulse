namespace Pulse.Base
{
    public interface IInputProvider : IProvider
    {
        PictureList GetPictures(PictureSearch ps);
    }
}
