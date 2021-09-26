using CSharp.Functional.Constructs;
using System;
using Unit = System.ValueTuple;

namespace CSharp.Functional.Extensions
{
    public static class ExceptionalExtension
    {

        public static Exceptional<T> Exceptional<T>(T value) => new Exceptional<T>(value);


        public static Exceptional<R> Map<T, R>(this Exceptional<T> exceptional, Func<T, R> f) =>
            exceptional.Match<Exceptional<R>>(exp => exp,
                                              data => f(data));

        public static Exceptional<R> Select<T, R>(this Exceptional<T> exceptional, Func<T, R> f) =>
            exceptional.Map(f);

        public static Exceptional<Unit> ForEach<T>(this Exceptional<T> exceptional, Action<T> action) =>
           exceptional.Match<Exceptional<Unit>>(exp => exp,
                                             data => action.ToFunc().Invoke(data));

        public static Exceptional<R> Bind<T, R>(this Exceptional<T> exceptional, Func<T, Exceptional<R>> f) =>
           exceptional.Match(exp => exp,
                             data => f(data));

        public static Exceptional<RR> SelectMany<T, R, RR>(this Exceptional<T> exceptional, Func<T, Exceptional<R>> bind, Func<T, R, RR> project) =>
            exceptional.Match(exp => exp,
                              t => bind(t).Match<Exceptional<RR>>(exp=>exp,r=>project(t,r)));

    }
}
