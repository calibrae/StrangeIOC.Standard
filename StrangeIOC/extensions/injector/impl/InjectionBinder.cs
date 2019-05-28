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
 * @class strange.extensions.injector.impl.InjectionBinder
 * 
 * The Binder for creating Injection mappings.
 * 
 * @see strange.extensions.injector.api.IInjectionBinder
 * @see strange.extensions.injector.api.IInjectionBinding
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using strange.extensions.injector.api;
using strange.extensions.reflector.impl;
using strange.framework.api;
using strange.framework.impl;
using Binder = strange.framework.impl.Binder;

namespace strange.extensions.injector.impl
{
    public class InjectionBinder : Binder, IInjectionBinder
    {
        private IInjector _injector;

        protected Dictionary<Type, Dictionary<Type, IInjectionBinding>> suppliers =
            new Dictionary<Type, Dictionary<Type, IInjectionBinding>>();

        public InjectionBinder()
        {
            injector = new Injector();
            injector.binder = this;
            injector.reflector = new ReflectionBinder();
        }

        public object GetInstance(Type key)
        {
            return GetInstance(key, null);
        }

        public virtual object GetInstance(Type key, object name)
        {
            var binding = GetBinding(key, name);
            if (binding == null)
            {
                throw new InjectionException("InjectionBinder has no binding for:\n\tkey: " + key + "\nname: " + name,
                    InjectionExceptionType.NULL_BINDING);
            }

            var instance = GetInjectorForBinding(binding).Instantiate(binding, false);
            injector.TryInject(binding, instance);

            return instance;
        }

        public T GetInstance<T>()
        {
            var instance = GetInstance(typeof(T));
            var retv = (T) instance;
            return retv;
        }

        public T GetInstance<T>(object name)
        {
            var instance = GetInstance(typeof(T), name);
            var retv = (T) instance;
            return retv;
        }

        public IInjector injector
        {
            get => _injector;
            set
            {
                if (_injector != null)
                {
                    _injector.binder = null;
                }

                _injector = value;
                _injector.binder = this;
            }
        }

        public new IInjectionBinding Bind<T>()
        {
            return base.Bind<T>() as IInjectionBinding;
        }

        public IInjectionBinding Bind(Type key)
        {
            return base.Bind(key) as IInjectionBinding;
        }

        public new virtual IInjectionBinding GetBinding<T>()
        {
            return base.GetBinding<T>() as IInjectionBinding;
        }

        public new virtual IInjectionBinding GetBinding<T>(object name)
        {
            return base.GetBinding<T>(name) as IInjectionBinding;
        }

        public new virtual IInjectionBinding GetBinding(object key)
        {
            return base.GetBinding(key) as IInjectionBinding;
        }

        public new virtual IInjectionBinding GetBinding(object key, object name)
        {
            return base.GetBinding(key, name) as IInjectionBinding;
        }

        public int ReflectAll()
        {
            var list = new List<Type>();
            foreach (var pair in bindings)
            {
                var dict = pair.Value;
                foreach (var bPair in dict)
                {
                    var binding = bPair.Value;
                    var t = binding.value is Type ? (Type) binding.value : binding.value.GetType();
                    if (list.IndexOf(t) == -1)
                    {
                        list.Add(t);
                    }
                }
            }

            return Reflect(list);
        }

        public int Reflect(List<Type> list)
        {
            var count = 0;
            foreach (var t in list)
            {
                //Reflector won't permit primitive types, so screen them
                if (t.IsPrimitive || t == typeof(decimal) || t == typeof(string))
                {
                    continue;
                }

                count++;
                injector.reflector.Get(t);
            }

            return count;
        }

        public IInjectionBinding GetSupplier(Type injectionType, Type targetType)
        {
            if (suppliers.ContainsKey(targetType))
            {
                if (suppliers[targetType].ContainsKey(injectionType))
                {
                    return suppliers[targetType][injectionType];
                }
            }

            return null;
        }

        public void Unsupply(Type injectionType, Type targetType)
        {
            var binding = GetSupplier(injectionType, targetType);
            if (binding != null)
            {
                suppliers[targetType].Remove(injectionType);
                binding.Unsupply(targetType);
            }
        }

        public void Unsupply<T, U>()
        {
            Unsupply(typeof(T), typeof(U));
        }

        protected virtual IInjector GetInjectorForBinding(IInjectionBinding binding)
        {
            return injector;
        }

        public override IBinding GetRawBinding()
        {
            return new InjectionBinding(resolver);
        }

        protected override IBinding performKeyValueBindings(List<object> keyList, List<object> valueList)
        {
            IBinding binding = null;

            // Bind in order
            foreach (var key in keyList)
            {
                // If this is called from another assembly, so trying to get the type from the calling assembly. It will prolly need some other work
                var keyType = Type.GetType(key as string) ??
                              Type.GetType(key + "," + Assembly.GetCallingAssembly().FullName);

                if (keyType == null)
                {
                    throw new BinderException(
                        "A runtime Injection Binding has resolved to null. Did you forget to register its fully-qualified name?\n Key:" +
                        key, BinderExceptionType.RUNTIME_NULL_VALUE);
                }

                if (binding == null)
                {
                    binding = Bind(keyType);
                }
                else
                {
                    binding = binding.Bind(keyType);
                }
            }

            foreach (var value in valueList)
            {
                // If this is called from another assembly, so trying to get the type from the calling assembly. It will prolly need some other work
                var valueType = Type.GetType(value as string) ??
                                Type.GetType(value + "," + Assembly.GetCallingAssembly().FullName);
                if (valueType == null)
                {
                    throw new BinderException(
                        "A runtime Injection Binding has resolved to null. Did you forget to register its fully-qualified name?\n Value:" +
                        value, BinderExceptionType.RUNTIME_NULL_VALUE);
                }

                binding = binding.To(valueType);
            }

            return binding;
        }

        /// Additional options: ToSingleton, CrossContext
        protected override IBinding addRuntimeOptions(IBinding b, List<object> options)
        {
            base.addRuntimeOptions(b, options);
            var binding = b as IInjectionBinding;
            if (options.IndexOf("ToSingleton") > -1)
            {
                binding.ToSingleton();
            }

            if (options.IndexOf("CrossContext") > -1)
            {
                binding.CrossContext();
            }

            var dict = options.OfType<Dictionary<string, object>>();
            if (dict.Any())
            {
                var supplyToDict = dict.First(a => a.Keys.Contains("SupplyTo"));
                if (supplyToDict != null)
                {
                    foreach (var kv in supplyToDict)
                    {
                        if (kv.Value is string)
                        {
                            var valueType = Type.GetType(kv.Value as string);
                            binding.SupplyTo(valueType);
                        }
                        else
                        {
                            var values = kv.Value as List<object>;
                            for (int a = 0, aa = values.Count; a < aa; a++)
                            {
                                var valueType = Type.GetType(values[a] as string);
                                binding.SupplyTo(valueType);
                            }
                        }
                    }
                }
            }

            return binding;
        }

        protected override void resolver(IBinding binding)
        {
            var iBinding = binding as IInjectionBinding;
            var supply = iBinding.GetSupply();

            if (supply != null)
            {
                foreach (var a in supply)
                {
                    var aType = a as Type;
                    if (suppliers.ContainsKey(aType) == false)
                    {
                        suppliers[aType] = new Dictionary<Type, IInjectionBinding>();
                    }

                    var keys = iBinding.key as object[];
                    foreach (var key in keys)
                    {
                        var keyType = key as Type;
                        if (suppliers[aType].ContainsKey(keyType) == false)
                        {
                            suppliers[aType][keyType] = iBinding;
                        }
                    }
                }
            }

            base.resolver(binding);
        }
    }
}