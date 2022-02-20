namespace CBodies
{
    public interface ICBodyObserver
    {
        public void OnShapeUpdate();

        public void OnShadingUpdate();

        public void OnPhysicsUpdate();

        public void OnInitialUpdate();
    }
}