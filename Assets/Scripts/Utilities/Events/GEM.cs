namespace Utilities.Events
{
    public static class GEM
    {
        public static void Subscribe<T>(EventListener<T> handler, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Subscribe(handler, null, channel);

        public static void Unsubscribe<T>(EventListener<T> handler, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Unsubscribe(handler, channel);
        
        public static T SendEvent<T>(T evt, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.SendEvent(evt, null, channel);
    }
}