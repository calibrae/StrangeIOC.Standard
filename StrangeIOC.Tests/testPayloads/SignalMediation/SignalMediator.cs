using strange.extensions.context.api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace strange.unittests
{
    public class SignalMediator : IMediator
    {
        public static int Value;
        public IContextView contextView { get; set; }

        public void PreRegister()
        {
        }

        public void OnRegister()
        {
        }

        public void OnRemove()
        {
        }

        //[ListensTo(typeof (OneArgSignal))]
        public void OneArgMethod(int myArg)
        {
            Value += myArg;
        }

        public void OnEnabled()
        {
        }

        public void OnDisabled()
        {
        }
    }

    public class NoArgSignal : Signal
    {
    }

    public class OneArgSignal : Signal<int>
    {
    }

    public class TwoArgSignal : Signal<int, bool>
    {
    }
}