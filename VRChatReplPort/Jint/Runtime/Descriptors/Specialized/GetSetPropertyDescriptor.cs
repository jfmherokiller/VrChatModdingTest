﻿using Jint.Native;
using Jint.Native.Function;

namespace Jint.Runtime.Descriptors.Specialized
{
    public sealed class GetSetPropertyDescriptor : PropertyDescriptor
    {
        private JsValue _get;
        private JsValue _set;

        public GetSetPropertyDescriptor(JsValue get, JsValue set, bool? enumerable = null, bool? configurable = null)
        : base(null, writable: null, enumerable: enumerable, configurable: configurable)
        {
            _get = get;
            _set = set;
        }

        internal GetSetPropertyDescriptor(JsValue get, JsValue set, PropertyFlag flags)
            : base(null, flags)
        {
            _get = get;
            _set = set;
        }

        public GetSetPropertyDescriptor(PropertyDescriptor descriptor) : base(descriptor)
        {
            _get = descriptor.Get;
            _set = descriptor.Set;
        }

        public override JsValue Get => _get;
        public override JsValue Set => _set;

        internal void SetGet(JsValue getter)
        {
            _get = getter;
        }
        
        internal void SetSet(JsValue setter)
        {
            _set = setter;
        }

        internal sealed class ThrowerPropertyDescriptor : PropertyDescriptor
        {
            private readonly Engine _engine;
            private readonly string _message;
            private JsValue _thrower;
            
            public ThrowerPropertyDescriptor(Engine engine, PropertyFlag flags, string message)
                : base(flags)
            {
                _engine = engine;
                _message = message;
            }

            public override JsValue Get => _thrower ??= new ThrowTypeError(_engine, _message) { _prototype = _engine.Function.PrototypeObject};
            public override JsValue Set => _thrower ??= new ThrowTypeError(_engine, _message) { _prototype = _engine.Function.PrototypeObject};

            protected internal override JsValue CustomValue
            {
                set => ExceptionHelper.ThrowInvalidOperationException("making changes to throw type error property's descriptor is not allowed");
            }
        }
    }
}