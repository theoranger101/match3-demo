using System.Collections.Generic;

namespace Utilities.Events
{
    public static class EventDispatcher
    {
        public static readonly object GlobalContext = new();
        public static object CurrentContext = GlobalContext;
        
        public const int DefaultChannel = -1;
        
        private static Dictionary<object, Dictionary<int, EventListenerCollection>> eventChannelsByContext = new();

        private static bool TryGetEventListenerCollection(object context, int channel, out EventListenerCollection listeners, bool createIfNonExists = true)
        {
            if (context == null)
            {
                context = GlobalContext;
            }
            
            if (!eventChannelsByContext.TryGetValue(context, out var listenersByChannel))
            {
                if (!createIfNonExists)
                {
                    listeners = null;
                    return false;
                }
                
                listenersByChannel = new Dictionary<int, EventListenerCollection>();
                eventChannelsByContext[context] = listenersByChannel;
            }

            if (!listenersByChannel.TryGetValue(channel, out listeners))
            {
                if (!createIfNonExists)
                {
                    listeners = null;
                    return false;
                }
                
                listeners = new EventListenerCollection();
                listenersByChannel[channel] = listeners;
            }

            return true;
        }
        
        public static void Subscribe<T>(EventListener<T> listener, object context = null, int channel = DefaultChannel) 
            where T : Event, new()
        {
            TryGetEventListenerCollection(context, channel, out var listeners);
            listeners.AddListener(listener);
        }

        public static void Unsubscribe<T>(EventListener<T> listener, object context = null, int channel = DefaultChannel) where T : Event
        {
            if(!TryGetEventListenerCollection(context, channel, out var listeners, false))
            {
                return;
            }
            
            listeners.RemoveListener(listener);
        }
        
        public static T SendEvent<T>(T evt, object context = null, int channel = DefaultChannel) 
            where T : Event, new()
        {
            if (!TryGetEventListenerCollection(context, channel, out var listeners, false))
            {
                return evt;
            }

            var oldContext = CurrentContext;

            CurrentContext = context;

            listeners.SendEvent(evt);
            
            CurrentContext = oldContext;

            return evt;
        }
    }
}