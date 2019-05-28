using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace strange.unittests
{
    public class SignalMediatorTwo : IMediator
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

        //[ListensTo(typeof(OneArgSignal))]
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
}