using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Orel
{
    internal class ORELObjectEnumerator<T> : IEnumerator<KeyValuePair<string, object>>
    {
        private bool _enumeratingInReferer;
        private EnumeratorWapper<T> _refererEnumerator;
        private IEnumerator<KeyValuePair<string, object>> _internalEnumerator;

        public ORELObjectEnumerator(IEnumerator<KeyValuePair<string, T>> refererEnumerator,
            IEnumerator<KeyValuePair<string, object>> internalEnumerator)
        {
            _refererEnumerator = new EnumeratorWapper<T>(refererEnumerator);
            _internalEnumerator = internalEnumerator;
            _enumeratingInReferer = _refererEnumerator != null;
        }

        private KeyValuePair<string, object> GetCurrent()
        {
            var current = _enumeratingInReferer ? _refererEnumerator.Current : _internalEnumerator.Current;
            return current;
        }

        public KeyValuePair<string, object> Current => GetCurrent();

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _refererEnumerator?.Dispose();
            _internalEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (_enumeratingInReferer)
            {
                _enumeratingInReferer = _refererEnumerator.MoveNext();
                if (_enumeratingInReferer) return true;
            }
            return _internalEnumerator.MoveNext();
        }

        public void Reset()
        {
            if (_refererEnumerator != null)
            {
                _refererEnumerator.Reset();
                _enumeratingInReferer = true;
            }
            _internalEnumerator.Reset();
        }
    }

    internal class EnumeratorWapper<T> : IEnumerator<KeyValuePair<string, object>>
    {
        private IEnumerator<KeyValuePair<string, T>> _baseEnumerator;
        public EnumeratorWapper(IEnumerator<KeyValuePair<string, T>> baseEnumerator)
        {
            _baseEnumerator = baseEnumerator;
        }

        public KeyValuePair<string, object> Current
        {
            get
            {
                return KeyValuePair.Create(_baseEnumerator.Current.Key, (object)_baseEnumerator.Current.Value);
            }
        }
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _baseEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _baseEnumerator.MoveNext();
        }

        public void Reset()
        {
            _baseEnumerator.Reset();
        }
    }
}
