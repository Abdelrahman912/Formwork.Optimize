using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FormworkOptimize.App.Utils
{
    public static class Memoization
    {
        public class ElementKey : IEquatable<ElementKey>
        {
            public Element Element { get; }

            public ElementKey(Element element)
            {
                Element = element;
            }

            public bool Equals(ElementKey other) =>
                Element.Id.IntegerValue == other.Element.Id.IntegerValue;

            public override int GetHashCode() =>
                Element.Id.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj is ElementKey)
                    return (obj as ElementKey).Element.Id.IntegerValue == Element.Id.IntegerValue;
                return false;
            }

        }

        public class LevelKey : IEquatable<LevelKey>
        {
            public Level Level { get; }

            public LevelKey(Level level)
            {
                Level = level;
            }

            public bool Equals(LevelKey other) =>
                Level.Id.IntegerValue == other.Level.Id.IntegerValue;

            public override int GetHashCode() =>
                Level.Id.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj is LevelKey)
                    return (obj as LevelKey).Level.Id.IntegerValue == Level.Id.IntegerValue;
                return false;
            }

        }

        public class FloorsKey : IEquatable<FloorsKey>
        {
            public Floor HostFloor { get; }

            public Floor SupportedFloor { get;  }

            public FloorsKey(Floor hostFloor , Floor supportedFloor)
            {
                HostFloor = hostFloor;
                SupportedFloor = supportedFloor;
            }

            public bool Equals(FloorsKey other) =>
                HostFloor.Id.IntegerValue == other.HostFloor.Id.IntegerValue && 
                SupportedFloor.Id.IntegerValue == other.SupportedFloor.Id.IntegerValue;

            public override int GetHashCode() =>
                HostFloor.Id.GetHashCode() + SupportedFloor.Id.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj is FloorsKey)
                {
                    var other = obj as FloorsKey;
                    return this.Equals(other);
                }
                    
                return false;
            }

        }

        public class FloorKey : IEquatable<FloorKey>
        {
            public Floor Floor { get; }

            public FloorKey(Floor floor)
            {
                Floor = floor;
            }

            public bool Equals(FloorKey other) =>
                Floor.Id.IntegerValue == other.Floor.Id.IntegerValue;

            public override int GetHashCode() =>
                Floor.Id.GetHashCode();

            public override bool Equals(object obj)
            {
                if (obj is FloorKey)
                    return (obj as FloorKey).Floor.Id.IntegerValue == Floor.Id.IntegerValue;
                return false;
            }

        }

        public static Func<T, Validation<R>> MemoizeWeak<T, R>(this Func<T, Validation<R>> func, TimeSpan ttl)
        where T : class, IEquatable<T>
        where R : class
        {
            var keyStore = new ConcurrentDictionary<int, T>();
            T ReduceKey(T obj)
            {
                var oldObj = keyStore.GetOrAdd(obj.GetHashCode(), obj);
                return obj.Equals(oldObj) ? oldObj : obj;
            };

            var cache = new ConditionalWeakTable<T, Tuple<Validation<R>, DateTime>>();

            Tuple<Validation<R>, DateTime> FactoryFunc(T key) =>
              Tuple.Create(func(key), DateTime.Now + ttl);


            return (arg) =>
            {
                var key = ReduceKey(arg);
                Tuple<Validation<R>, DateTime> value = null;
                var isinDict = cache.TryGetValue(key , out value);
                if (isinDict && value.Item2 > DateTime.Now && value.Item1.IsValid)
                    return value.Item1;
                var result = FactoryFunc(key);
                cache.Remove(key);
                cache.Add(key, result);
                return result.Item1;
            };
        }

        public static Func<T, R> MemoizeWeak<T, R>(this Func<T, R> func, TimeSpan ttl)
         where T : class, IEquatable<T>
         where R : class
        {
            var keyStore = new ConcurrentDictionary<int, T>();
            T ReduceKey(T obj)
            {
                var oldObj = keyStore.GetOrAdd(obj.GetHashCode(), obj);
                return obj.Equals(oldObj) ? oldObj : obj;
            };

            var cache = new ConditionalWeakTable<T, Tuple<R, DateTime>>();

            Tuple<R, DateTime> FactoryFunc(T key) =>
              Tuple.Create(func(key), DateTime.Now + ttl);


            return (arg) =>
            {
                var key = ReduceKey(arg);
                var value = cache.GetValue(key, FactoryFunc);
                if (value.Item2 > DateTime.Now)
                    return value.Item1;
                var result = FactoryFunc(key);
                cache.Remove(key);
                cache.Add(key, result);
                return result.Item1;
            };
        }
    }
}
