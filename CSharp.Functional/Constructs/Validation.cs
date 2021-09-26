using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSharp.Functional.Extensions.ValidationExtension;
using Unit = System.ValueTuple;

namespace CSharp.Functional.Constructs
{
    public static class Validation
    {
        public struct Invalid
        {
            internal IEnumerable<Error> Errors;
            public Invalid(IEnumerable<Error> errors) { Errors = errors; }
        }
    }

    public struct Validation<T>
    {

        #region Properties

        internal IEnumerable<Error> Errors { get; }

        internal T Value { get; }

        public bool IsValid { get; set; }

        public static Func<T, Validation<T>> Return = t => Valid(t);

        #endregion

        #region Contsructors

        private Validation(IEnumerable<Error> errors)
        {
            IsValid = false;
            Errors = errors;
            Value = default(T);
        }

        internal Validation(T right)
        {
            IsValid = true;
            Value = right;
            Errors = Enumerable.Empty<Error>();
        }

        #endregion

        #region Methods

        public static implicit operator Validation<T>(Error error)=>
            new Validation<T>(new[] { error });

        public static implicit operator Validation<T>(Validation.Invalid left)=>
            new Validation<T>(left.Errors);

        public static implicit operator Validation<T>(T right)=>
            new Validation<T>(right);

        public TR Match<TR>(Func<IEnumerable<Error>, TR> invalid, Func<T, TR> valid) =>
            IsValid ? valid(Value) : invalid(Errors);


        public Unit Match(Action<IEnumerable<Error>> invalid, Action<T> valid) =>
            Match(invalid.ToFunc(), valid.ToFunc());

        public IEnumerable<T> AsEnumerable()
        {
            if(IsValid)
                yield return Value;
        }

        #endregion


    }

}
