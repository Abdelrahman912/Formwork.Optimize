using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Comparers
{
    public class GenericComparer<T> : IEqualityComparer<T>
    {

        #region Fileds

        private readonly Func<T, T, bool> _equality;

        private readonly Func<T, int> _hash;

        #endregion

        #region Constructors

        public GenericComparer(Func<T, T, bool> equality,Func<T,int> hash =null)
        {
          _equality = equality ?? ((a, b) => false);
            _hash = hash ?? (obj => obj.GetHashCode());
        }

        #endregion

        #region Methods

        public bool Equals(T x, T y) =>
          _equality(x, y);


        public int GetHashCode(T obj) => 
           _hash(obj);
        

        #endregion

    }

    public static class GenericComparer
    {

        public static GenericComparer<T> Create<T>(Func<T, T, bool> equality,Func<T,int> hash=null) =>
         new GenericComparer<T>(equality,hash);

    }
}
