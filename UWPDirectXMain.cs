using UWPPlayground.Common;
using UWPPlayground.Content;

namespace UWPPlayground
{
    public class UWPDirectXMain
    {
        private Sample3DSceneRenderer _sceneRenderer;
        private StepTimer _timer = StepTimer.Create();

        public void Update()
        {
            _timer.Tick(() => _sceneRenderer.Update(ref _timer));
        }

        public bool Render()
        {
            if (_timer.FrameCount == 0)
                return false;

            return _sceneRenderer.Render();
        }

        public void OnSuspending()
        {
            _sceneRenderer.SaveState();
        }

        public void OnResuming()
        {

        }

        public void OnWindowSizeChanged()
        {
            _sceneRenderer.CreateWindowSizeDependentResources();
        }

        public void OnDeviceRemoved()
        {
            _sceneRenderer.SaveState();
            _sceneRenderer = null;
        }

        public void CreateRenderers(DeviceResources deviceResources)
        {
            _sceneRenderer = new Sample3DSceneRenderer(deviceResources);
            OnWindowSizeChanged();
        }
    }
}