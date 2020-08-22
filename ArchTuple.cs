using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DynamicPInvoke
{
    public class ArchTuple<T>
    {
        private readonly Dictionary<Architecture, T> _data;

        public ArchTuple()
        {
            _data = new Dictionary<Architecture, T>(4);
        }

        public ArchTuple(T value)
        {
            _data = new Dictionary<Architecture, T>(4);
            SetValues(value);
        }

        public T this[Architecture key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public void SetValues(T value)
        {
            foreach (var arch in (Architecture[])Enum.GetValues(typeof(Architecture))) {
                _data[arch] = value;
            }
        }

        public static implicit operator ArchTuple<T>(T value) => new ArchTuple<T>(value);
        public static implicit operator T(ArchTuple<T> tuple) => tuple[RuntimeInformation.OSArchitecture];
    }
}
