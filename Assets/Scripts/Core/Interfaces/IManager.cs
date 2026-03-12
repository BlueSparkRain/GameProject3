namespace Core.Interfaces
{
    public interface IManager
    {
        public void MgrInit(GameRoot gameRoot);
        public void MgrUpdate(float deltatime) ;
        public void MgrDispose();
    }
}

