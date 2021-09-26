using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;
using static CSharp.Functional.Extensions.ValidationExtension;
using System.Linq;

namespace CSharp.Functional
{
    public static class Functional
    {
        public static Unit Unit() => default(Unit);

        public static Validation<List<T>>  PopOutValidation<T>(this List<Validation<T>> validations)
        {
            (var valids, var errors) = validations.Aggregate(Tuple.Create(new List<T>(), new List<Error>()), (soFar, current) =>
            {
                current.Match(errs => { soFar.Item2.AddRange(errs); Unit(); }, valid => { soFar.Item1.Add(valid); Unit(); });
                return soFar;
            });

            if (errors.Count > 0)
                return Invalid(errors);
            else
                return Valid(valids);
        }

    }
}
