using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using Xamarin.Forms;

namespace StrangeIOC.extensions.context.impl
{
    public class XamarinContextView : Application, IContextView
    {
        public new Page MainPage
        {
            get { return base.MainPage; }
            set
            {
                if (base.MainPage != null)
                {
                    ((XamarinContext)context).mediationBinder.Trigger(MediationEvent.DESTROYED, base.MainPage);
                }
                base.MainPage = value;
                ((XamarinContext)context).mediationBinder.Trigger(MediationEvent.AWAKE, value);
            }
        }

        public bool requiresContext { get; set; }
        public bool registeredWithContext { get; set; }
        public bool autoRegisterWithContext { get; }
        public IContext context { get; set; }
    }
}
