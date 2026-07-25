namespace Pulse.Base
{
    public interface IProvider
    {
        void Activate(object args);
        void Deactivate(object args);
        void Initialize(object args);
    }
}
