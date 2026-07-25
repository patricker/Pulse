namespace Pulse.Base
{
    public interface IOutputProvider : IProvider
    {
        void ProcessPicture(PictureBatch p, string config);
    }
}
