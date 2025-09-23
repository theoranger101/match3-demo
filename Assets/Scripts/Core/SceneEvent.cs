using Utilities.Events;

namespace Core
{
    public enum SceneEventType
    {
        Loading = 0,
        Loaded = 1,
    }
    
    public class SceneEvent : Event<SceneEvent>
    {
        public static SceneEvent Get()
        {
            var evt = GetPooledInternal();
            return evt;
        }
    }
}