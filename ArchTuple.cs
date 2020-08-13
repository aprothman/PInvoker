using System;
using System.Collections.Generic;

namespace DynamicPInvoke
{
    public enum Architecture
    {
        x86,
        x64
    }

    public class ArchTuple<T>
    {
        private Dictionary<Architecture, T> _data;

        public ArchTuple()
        {
            _data = new Dictionary<Architecture, T>(2);
        }

        public ArchTuple(T value)
        {
            _data = new Dictionary<Architecture, T>(2);
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
    }
}
