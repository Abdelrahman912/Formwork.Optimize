using System;
using System.Collections.Generic;
using static CSharp.Functional.Extensions.OptionExtension;

namespace CSharp.Functional.Constructs
{
    public struct Option<T>
    {
        private readonly bool _isSome;
        private readonly T _value;

        private Option(T value)
        {
            _isSome = true;
            _value = value;
        }

        public static implicit operator Option<T>(Option.None _) =>
            new Option<T>();

        public static implicit operator Option<T>(Option.Some<T> some) =>
            new Option<T>(some.Value);

        public static implicit operator Option<T>(T value) =>
           value == null ? None : Some(value);

        public R Match<R>(Func<R> none, Func<T, R> some) =>
            _isSome == true ? some(_value) : none();

        public IEnumerable<T> AsEnumerable()
        {
            if (_isSome)
                yield return _value;
        }


    }

    public static class Option
    {
        public struct None
        {
            internal static readonly None Default = new None();
        }

        public struct Some<T>
        {
            internal T Value { get; }
            public Some(T value)
            {
                if (value == null)
                    throw new ArgumentNullException();
                Value = value;
            }
        }

    }
}
