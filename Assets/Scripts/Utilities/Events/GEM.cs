namespace Utilities.Events
{
    public static class GEM
    {
        public static void Subscribe<T>(EventListener<T> handler, int channel = EventDispatcher.DefaultChannel, Priority priority = Priority.Normal)
            where T : Event, new() => EventDispatcher.Subscribe(handler, null, channel, priority);

        public static void Unsubscribe<T>(EventListener<T> handler, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Unsubscribe(handler, channel);
        
        public static void Unsubscribe<T>(EventListener<T> handler, object context, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Unsubscribe(handler, context, channel);
        
        public static T SendEvent<T>(T evt, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.SendEvent(evt, null, channel);
    }
}