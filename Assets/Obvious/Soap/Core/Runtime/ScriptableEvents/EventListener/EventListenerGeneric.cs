﻿using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace Obvious.Soap
{
    public abstract class EventListenerGeneric<T> : EventListenerBase
    {
        protected virtual EventResponse<T>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEvent<T>, EventResponse<T>> _dictionary =
            new Dictionary<ScriptableEvent<T>, EventResponse<T>>();

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var eventResponse in EventResponses)
            {
                if (toggle)
                {
                    eventResponse.ScriptableEvent.RegisterListener(this);
                    if (!_dictionary.ContainsKey(eventResponse.ScriptableEvent))
                        _dictionary.Add(eventResponse.ScriptableEvent, eventResponse);
                }
                else
                {
                    eventResponse.ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponse.ScriptableEvent))
                        _dictionary.Remove(eventResponse.ScriptableEvent);
                }
            }
        }
        
        internal void OnEventRaised(ScriptableEvent<T> eventRaised, T param, bool debug = false)
        {
            var eventResponse = _dictionary[eventRaised];
            if (eventResponse.Delay > 0)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(Cr_DelayInvokeResponse(eventRaised, eventResponse, param, debug));
                else
                    DelayInvokeResponseAsync(eventRaised, eventResponse, param, debug, _cancellationTokenSource.Token);
            }
            else
                InvokeResponse(eventRaised, eventResponse, param, debug);
        }
        
        private IEnumerator Cr_DelayInvokeResponse(ScriptableEvent<T> eventRaised, EventResponse<T> eventResponse, T param,
            bool debug)
        {
            yield return new WaitForSeconds(eventResponse.Delay);
            InvokeResponse(eventRaised, eventResponse, param, debug);
        }

        private async void DelayInvokeResponseAsync(ScriptableEvent<T> eventRaised, EventResponse<T> eventResponse, T param,
            bool debug, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay((int)(eventResponse.Delay * 1000), cancellationToken);
                InvokeResponse(eventRaised, eventResponse, param, debug);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InvokeResponse(ScriptableEvent<T> eventRaised, EventResponse<T> eventResponse, T param, bool debug)
        {
            eventResponse.Response?.Invoke(param);
            if (debug)
                Debug(eventRaised);
        }

        protected virtual void Debug(ScriptableEvent<T> eventRaised)
        {
            var response = _dictionary[eventRaised].Response;
            var registeredListenerCount = response.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#f75369>[Event] </color>");
                sb.Append(eventRaised.name);
                sb.Append(" => ");
                sb.Append(response.GetPersistentTarget(i).name);
                sb.Append(".");
                sb.Append(response.GetPersistentMethodName(i));
                sb.Append("()");
                UnityEngine.Debug.Log(sb.ToString(), gameObject);
            }
        }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var eventResponse in EventResponses)
            {
                var registeredListenerCount = eventResponse.Response.GetPersistentEventCount();

                for (var i = 0; i < registeredListenerCount; i++)
                {
                    if (eventResponse.Response.GetPersistentMethodName(i) == methodName)
                    {
                        var sb = new StringBuilder();
                        sb.Append($"<color=#f75369>{methodName}()</color>");
                        sb.Append(" is called by: <color=#f75369>[Event] </color>");
                        sb.Append(eventResponse.ScriptableEvent.name);
                        UnityEngine.Debug.Log(sb.ToString(), gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }

        [System.Serializable]
        public class EventResponse<U>
        {
            public virtual ScriptableEvent<U> ScriptableEvent { get; }
            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float Delay;
            public virtual UnityEvent<U> Response { get; }
        }
    }
}