/*
 * Copyright 2013 ThirdMotion, Inc.
 *
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *
 *		http://www.apache.org/licenses/LICENSE-2.0
 *
 *		Unless required by applicable law or agreed to in writing, software
 *		distributed under the License is distributed on an "AS IS" BASIS,
 *		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *		See the License for the specific language governing permissions and
 *		limitations under the License.
 */

/**
 * @interface strange.extensions.mediation.impl.MediationBinding
 * 
 * Subclass of Binding for MediationBinding.
 * 
 * I've provided MediationBinding, but at present it comforms
 * perfectly to Binding.
 */

using System;
using System.Reflection;
using strange.extensions.mediation.api;
using strange.framework.api;
using strange.framework.impl;
using Binder = strange.framework.impl.Binder;

namespace strange.extensions.mediation.impl
{
    public class MediationBinding : Binding, IMediationBinding
    {
        protected ISemiBinding _abstraction;
        protected ISemiBinding _viewModel;


        public MediationBinding(Binder.BindingResolver resolver) : base(resolver)
        {
            _abstraction = new SemiBinding();
            _abstraction.constraint = BindingConstraintType.ONE;

            _viewModel = new SemiBinding();
            _viewModel.constraint = BindingConstraintType.ONE;
        }

        IMediationBinding IMediationBinding.ToMediator<T>()
        {
            return base.To(typeof(T)) as IMediationBinding;
        }


        IMediationBinding IMediationBinding.ToAbstraction<T>()
        {
            var abstractionType = typeof(T).GetTypeInfo();
            if (key != null)
            {
                var keyType = (key as Type).GetTypeInfo();
                if (abstractionType.IsAssignableFrom(keyType) == false)
                    throw new MediationException(
                        "The View " + key + " has been bound to the abstraction " + typeof(T) +
                        " which the View neither extends nor implements. ", MediationExceptionType.VIEW_NOT_ASSIGNABLE);
            }

            _abstraction.Add(abstractionType);
            return this;
        }

        IMediationBinding IMediationBinding.ToViewModel<T>()
        {
            _viewModel.Add(typeof(T).GetTypeInfo());
            return this;
        }

        IMediationBinding IMediationBinding.NoChildren()
        {
            NoChildrenSwitch = true;
            return this;
        }

        IMediationBinding IMediationBinding.ForceBindingContext()
        {
            ForceBindingContextSwitch = true;
            return this;
        }

        public bool ForceBindingContextSwitch { get; set; }

        public bool NoChildrenSwitch { get; private set; }


        public object abstraction => _abstraction.value == null ? BindingConst.NULLOID : _abstraction.value;

        public object viewModel => _viewModel.value == null ? BindingConst.NULLOID : _viewModel.value;

        public new IMediationBinding Bind<T>()
        {
            return base.Bind<T>() as IMediationBinding;
        }

        public new IMediationBinding Bind(object key)
        {
            return base.Bind(key) as IMediationBinding;
        }

        public new IMediationBinding To<T>()
        {
            return base.To<T>() as IMediationBinding;
        }

        public new IMediationBinding To(object o)
        {
            return base.To(o) as IMediationBinding;
        }
    }
}