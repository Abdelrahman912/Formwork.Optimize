using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = System.ValueTuple;
using CSharp.Functional.Extensions;


namespace CSharp.Functional.Constructs
{
    public struct Exceptional<T>
    {

        #region Properties
        internal Exception Ex { get; }

        internal T Value { get; }

        public bool Success => Ex == null;

        public bool Exception => Ex != null;

        #endregion

        #region Constructors
        internal Exceptional(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            Ex = ex;
            Value = default(T);
        }

        internal Exceptional(T right)
        {
            Value = right;
            Ex = null;
        }

        #endregion

        #region Methods

        public static implicit operator Exceptional<T>(Exception left) => new Exceptional<T>(left);

        public static implicit operator Exceptional<T>(T right) => new Exceptional<T>(right);

        public TR Match<TR>(Func<Exception, TR> Exception, Func<T, TR> Success)
           => this.Exception ? Exception(Ex) : Success(Value);

        public Unit Match(Action<Exception> Exception, Action<T> Success)
           => Match(Exception.ToFunc(), Success.ToFunc());

        public override string ToString()
           => Match(
              ex => $"Exception({ex.Message})",
              t => $"Success({t})");

        #endregion



    }
}
