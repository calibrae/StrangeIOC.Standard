using strange.extensions.context.api;
using strange.extensions.mediation.api;
using Xamarin.Forms;

namespace strange.extensions.context.impl
{
    public class XamarinContextView : Application, IContextView
    {
        public new Page MainPage
        {
            get => base.MainPage;
            set
            {
                if (base.MainPage != null)
                {
                    ((XamarinContext) context).mediationBinder.Trigger(MediationEvent.DESTROYED, base.MainPage);
                }

                base.MainPage = value;
                ((XamarinContext) context).mediationBinder.Trigger(MediationEvent.AWAKE, value);
            }
        }

        public bool requiresContext { get; set; }
        public bool registeredWithContext { get; set; }
        public bool autoRegisterWithContext { get; }
        public IContext context { get; set; }
    }
}