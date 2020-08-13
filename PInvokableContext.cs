using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;

namespace DynamicPInvoke
{
    /// <summary>
    /// This attribute is set to indicate that the method signature should be used to dynamically 
    /// generate a PInvoke call that is implicitly invoked before the body of the method whenever
    /// a call is made.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class PInvokableAttribute : Attribute { }

    /// <summary>
    /// The context attribute that causes the 
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    internal sealed class PInvokerContextAttribute : Attribute, IContextAttribute
    {
        private bool _initialized = false;

        public void GetPropertiesForNewContext(IConstructionCallMessage msg)
        {
            msg.ContextProperties.Add(new PInvokerContextProperty());
            _initialized = true;
        }

        public bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            return _initialized;
        }
    };

    internal sealed class PInvokerContextProperty : IContextProperty, IContributeObjectSink, IContextPropertyActivator
    {
        private PInvokerSink _sink = null;

        #region IContextProperty

        public string Name => "PInvoker";

        public void Freeze(Context newContext) { }

        public bool IsNewContextOK(Context newCtx)
        {
            return true;
        }

        #endregion // IContextProperty

        #region IContributeObjectSink

        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            _sink = new PInvokerSink(nextSink);
            return _sink;
        }

        #endregion // IContributeObjectSink

        #region IContextPropertyActivator

        public bool IsOKToActivate(IConstructionCallMessage msg)
        {
            return true;
        }

        public void CollectFromClientContext(IConstructionCallMessage msg) { }

        public bool DeliverClientContextToServerContext(IConstructionCallMessage msg)
        {
            return true;
        }

        public void CollectFromServerContext(IConstructionReturnMessage msg)
        {
            var retProxy = msg.ReturnValue as ObjRef;
            try {
                _sink.SetCallable((IExternalCallable)RemotingServices.Unmarshal(retProxy, true));
            }
            catch(InvalidCastException ex) {
                throw new InvalidOperationException("PInvokableContextAttribute may only be applied to to a class that implements IExternalCallable", ex);
            }
        }

        public bool DeliverServerContextToClientContext(IConstructionReturnMessage msg)
        {
            return true;
        }

        #endregion // IContextPropertyActivator
    }

    internal sealed class PInvokerSink : IMessageSink
    {
        private IMessageSink _next;
        private IExternalCallable _pinvoker = null;

        public PInvokerSink(IMessageSink next)
        {
            _next = next;
        }

        #region IMessageSink

        public IMessageSink NextSink => _next;

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            throw new InvalidOperationException("PInvoker doesn't suppport asynchronous calls");
        }

        public IMessage SyncProcessMessage(IMessage msg)
        {
            // if this message is a method call tagged [PInvokable]
            if (msg is IMethodMessage call) {
                var attribs = call.MethodBase.GetCustomAttributes(typeof(PInvokableAttribute), false);
                if (0 < attribs.Length) {
                    DoPInvoke(call);
                }
            }
            return _next.SyncProcessMessage(msg);
        }

        #endregion // IMessageSink

        public void SetCallable(IExternalCallable callable)
        {
            _pinvoker = callable;
        }

        private void DoPInvoke(IMethodMessage msg)
        {
            if (null != _pinvoker) {
                _pinvoker.CallMethod(msg.MethodName, msg.Args);
            }
        }
    }
}
