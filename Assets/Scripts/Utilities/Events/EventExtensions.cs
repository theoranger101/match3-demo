using System;

namespace Utilities.Events
{
    public static class EventExtensions
    {
        public static T SendGlobal<T>(this T evt, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => GEM.SendEvent(evt, channel);

        public static void AddListener<T>(this object context, EventListener<T> listener,
            int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Subscribe(listener, context, channel);
        
        public static void RemoveListener<T>(this object context, EventListener<T> listener,
            int channel = EventDispatcher.DefaultChannel)
            where T : Event, new() => EventDispatcher.Unsubscribe(listener, channel);

        public static void SendEvent<T>(this object context, int channel = EventDispatcher.DefaultChannel)
            where T : Event<T>, new()
        {
            using (var evt = Event<T>.Get())
            {
                SendEvent(context, evt, channel);
            }
        }

        public static T SendEvent<T>(this object context, T evt, int channel = EventDispatcher.DefaultChannel)
            where T : Event, new()
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Event cannot be null.");
            }

            return EventDispatcher.SendEvent(evt, context, channel);
        }
    }
}