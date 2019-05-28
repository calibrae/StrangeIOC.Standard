using System.Reflection;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.dispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;
using strange.extensions.implicitBind.api;
using strange.extensions.implicitBind.impl;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.sequencer.api;
using strange.extensions.sequencer.impl;
using Xamarin.Forms;

namespace strange.unittests
{
    public class MockContext : CrossContext
    {
        public string[] ScannedPackages = { };

        public MockContext()
        {
        }

 

        public MockContext( bool autoStartup) : base( autoStartup)
        {
        }

        public Assembly Assembly { get; set; }

        /// A Binder that maps Events to Commands
        public ICommandBinder commandBinder { get; set; }

        /// A Binder that serves as the Event bus for the ViewedContext
        public IEventDispatcher dispatcher { get; set; }

        /// A Binder that maps Views to Mediators
        public IMediationBinder mediationBinder { get; set; }

        /// A Binder that maps Events to Sequences
        public ISequencer sequencer { get; set; }

        public IImplicitBinder implicitBinder { get; set; }

        protected override void mapBindings()
        {
            implicitBinder.Assembly = Assembly.GetExecutingAssembly();

            base.mapBindings();
            implicitBinder.ScanForAnnotatedClasses(ScannedPackages);
        }

        public void ScanForAnnotatedClasses(params string[] namespaces)
        {
            implicitBinder.ScanForAnnotatedClasses(namespaces);
        }

        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            injectionBinder.Bind<IInjectionBinder>().ToValue(injectionBinder);
            injectionBinder.Bind<IContext>().ToValue(this).ToName(ContextKeys.CONTEXT);
            injectionBinder.Bind<ICommandBinder>().To<EventCommandBinder>().ToSingleton();
            //This binding is for local dispatchers
            injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>();
            //This binding is for the common system bus
            injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>().ToSingleton()
                .ToName(ContextKeys.CONTEXT_DISPATCHER);
            injectionBinder.Bind<IMediationBinder>().To<MediationBinder>().ToSingleton();
            injectionBinder.Bind<ISequencer>().To<EventSequencer>().ToSingleton();
            injectionBinder.Bind<IImplicitBinder>().To<ImplicitBinder>().ToSingleton();
        }

        protected override void instantiateCoreComponents()
        {
            base.instantiateCoreComponents();
            commandBinder = injectionBinder.GetInstance<ICommandBinder>();

            dispatcher = injectionBinder.GetInstance<IEventDispatcher>(ContextKeys.CONTEXT_DISPATCHER);
            mediationBinder = injectionBinder.GetInstance<IMediationBinder>();
            sequencer = injectionBinder.GetInstance<ISequencer>();
            implicitBinder = injectionBinder.GetInstance<IImplicitBinder>();

            (dispatcher as ITriggerProvider).AddTriggerable(commandBinder as ITriggerable);
            (dispatcher as ITriggerProvider).AddTriggerable(sequencer as ITriggerable);
        }

        public override void AddView(object view)
        {
            mediationBinder.Trigger(MediationEvent.AWAKE, view as Element);
        }

        public override void RemoveView(object view)
        {
            mediationBinder.Trigger(MediationEvent.DESTROYED, view as Element);
        }
    }
}