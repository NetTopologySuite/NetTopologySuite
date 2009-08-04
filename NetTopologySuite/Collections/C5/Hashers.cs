/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SCG = System.Collections.Generic;

namespace C5
{
    /// <summary>
    /// Utility class for building default generic equalityComparers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EqualityComparer<T>
    {
        private static readonly Type equalityequalityComparer = typeof (EquatableEqualityComparer<>);
        private static readonly Type icollection = typeof (ICollection<>);
        private static readonly Type iequalitytype = typeof (IEquatable<T>);
        private static readonly Type isequenced = typeof (ISequenced<>);

        private static readonly Type orderedcollectionequalityComparer = typeof (SequencedCollectionEqualityComparer<,>);

        private static readonly Type unorderedcollectionequalityComparer =
            typeof (UnsequencedCollectionEqualityComparer<,>);

        private static IEqualityComparer<T> cachedDefault = null;

        //TODO: find the right word for initialized+invocation 
        /// <summary>
        /// A default generic equality comparer for type T. The procedure is as follows:
        /// <list>
        /// <item>If T is a primitive type (char, sbyte, byte, short, ushort, int, uint, float, double, decimal), 
        /// the equalityComparer will be a standard equalityComparer for that type</item>
        /// <item>If the actual generic argument T implements the generic interface
        /// <see cref="T:C5.ISequenced`1"/> for some value W of its generic parameter T,
        /// the equalityComparer will be <see cref="T:C5.SequencedCollectionEqualityComparer`2"/></item>
        /// <item>If the actual generic argument T implements 
        /// <see cref="T:C5.ICollection`1"/> for some value W of its generic parameter T,
        /// the equalityComparer will be <see cref="T:C5.UnsequencedCollectionEqualityComparer`2"/></item>
        /// <item>If T is a type implementing <see cref="T:C5.IEquatable`1"/>, the equalityComparer
        /// will be <see cref="T:C5.EquatableEqualityComparer`1"/></item>
        /// <item>If T is a type not implementing <see cref="T:C5.IEquatable`1"/>, the equalityComparer
        /// will be <see cref="T:C5.NaturalEqualityComparer`1"/> </item>
        /// </list>   
        /// The <see cref="T:C5.IEqualityComparer`1"/> object is constructed when this class is initialised, i.e. 
        /// its static constructors called. Thus, the property will be the same object 
        /// for the duration of an invocation of the runtime, but a value serialized in 
        /// another invocation and deserialized here will not be the same object.
        /// </summary>
        /// <value></value>
        public static IEqualityComparer<T> Default
        {
            get
            {
                if (cachedDefault != null)
                    return cachedDefault;

                Type t = typeof (T);

                if (t.IsValueType)
                {
                    if (t.Equals(typeof (char)))
                        return cachedDefault = (IEqualityComparer<T>) (CharEqualityComparer.Default);

                    if (t.Equals(typeof (sbyte)))
                        return cachedDefault = (IEqualityComparer<T>) (SByteEqualityComparer.Default);

                    if (t.Equals(typeof (byte)))
                        return cachedDefault = (IEqualityComparer<T>) (ByteEqualityComparer.Default);

                    if (t.Equals(typeof (short)))
                        return cachedDefault = (IEqualityComparer<T>) (ShortEqualityComparer.Default);

                    if (t.Equals(typeof (ushort)))
                        return cachedDefault = (IEqualityComparer<T>) (UShortEqualityComparer.Default);

                    if (t.Equals(typeof (int)))
                        return cachedDefault = (IEqualityComparer<T>) (IntEqualityComparer.Default);

                    if (t.Equals(typeof (uint)))
                        return cachedDefault = (IEqualityComparer<T>) (UIntEqualityComparer.Default);

                    if (t.Equals(typeof (long)))
                        return cachedDefault = (IEqualityComparer<T>) (LongEqualityComparer.Default);

                    if (t.Equals(typeof (ulong)))
                        return cachedDefault = (IEqualityComparer<T>) (ULongEqualityComparer.Default);

                    if (t.Equals(typeof (float)))
                        return cachedDefault = (IEqualityComparer<T>) (FloatEqualityComparer.Default);

                    if (t.Equals(typeof (double)))
                        return cachedDefault = (IEqualityComparer<T>) (DoubleEqualityComparer.Default);

                    if (t.Equals(typeof (decimal)))
                        return cachedDefault = (IEqualityComparer<T>) (DecimalEqualityComparer.Default);
                }
                Type[] interfaces = t.GetInterfaces();
                if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(isequenced))
                    return
                        createAndCache(
                            orderedcollectionequalityComparer.MakeGenericType(new Type[] {t, t.GetGenericArguments()[0]}));
                foreach (Type ty in interfaces)
                {
                    if (ty.IsGenericType && ty.GetGenericTypeDefinition().Equals(isequenced))
                        return
                            createAndCache(
                                orderedcollectionequalityComparer.MakeGenericType(new Type[]
                                                                                      {t, ty.GetGenericArguments()[0]}));
                }
                if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(icollection))
                    return
                        createAndCache(
                            unorderedcollectionequalityComparer.MakeGenericType(new Type[]
                                                                                    {t, t.GetGenericArguments()[0]}));
                foreach (Type ty in interfaces)
                {
                    if (ty.IsGenericType && ty.GetGenericTypeDefinition().Equals(icollection))
                        return
                            createAndCache(
                                unorderedcollectionequalityComparer.MakeGenericType(new Type[]
                                                                                        {t, ty.GetGenericArguments()[0]}));
                }
                if (iequalitytype.IsAssignableFrom(t))
                    return createAndCache(equalityequalityComparer.MakeGenericType(new Type[] {t}));
                else
                    return cachedDefault = NaturalEqualityComparer<T>.Default;
            }
        }

        private static IEqualityComparer<T> createAndCache(Type equalityComparertype)
        {
            return
                cachedDefault =
                (IEqualityComparer<T>)
                (equalityComparertype.GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null,
                                                                                                                 null));
        }
    }

    /// <summary>
    /// A default item equalityComparer calling through to
    /// the GetHashCode and Equals methods inherited from System.Object.
    /// </summary>
    [Serializable]
    public sealed class NaturalEqualityComparer<T> : IEqualityComparer<T>
    {
        private static NaturalEqualityComparer<T> cached;

        private NaturalEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static NaturalEqualityComparer<T> Default
        {
            get { return cached ?? (cached = new NaturalEqualityComparer<T>()); }
        }

        //TODO: check if null check is reasonable
        //Answer: if we have struct C<T> { T t; int i;} and implement GetHashCode as
        //the sum of hashcodes, and T may be any type, we cannot make the null check 
        //inside the definition of C<T> in a reasonable way.

        #region IEqualityComparer<T> Members

        /// <summary>
        /// Get the hash code with respect to this item equalityComparer
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(T item)
        {
            return item == null ? 0 : item.GetHashCode();
        }

        /// <summary>
        /// Check if two items are equal with respect to this item equalityComparer
        /// </summary>
        /// <param name="item1">first item</param>
        /// <param name="item2">second item</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(T item1, T item2)
        {
            return item1 == null ? item2 == null : item1.Equals(item2);
        }

        #endregion
    }

    /// <summary>
    /// A default equality comparer for a type T that implements System.IEquatable<typeparamref name="T"/>. 
    /// 
    /// The equality comparer forwards calls to GetHashCode and Equals to the IEquatable methods 
    /// on T, so Equals(T) is called, not Equals(object). 
    /// This will save boxing abd unboxing if T is a value type
    /// and in general saves a runtime type check.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class EquatableEqualityComparer<T> : IEqualityComparer<T> where T : IEquatable<T>
    {
        private static EquatableEqualityComparer<T> cached = new EquatableEqualityComparer<T>();

        private EquatableEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static EquatableEqualityComparer<T> Default
        {
            get { return cached ?? (cached = new EquatableEqualityComparer<T>()); }
        }

        #region IEqualityComparer<T> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetHashCode(T item)
        {
            return item == null ? 0 : item.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public bool Equals(T item1, T item2)
        {
            return item1 == null ? item2 == null : item1.Equals(item2);
        }

        #endregion
    }

    /// <summary>
    /// A equalityComparer for a reference type that uses reference equality for equality and the hash code from object as hash code.
    /// </summary>
    /// <typeparam name="T">The item type. Must be a reference type.</typeparam>
    [Serializable]
    public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private static ReferenceEqualityComparer<T> cached;

        private ReferenceEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static ReferenceEqualityComparer<T> Default
        {
            get { return cached ?? (cached = new ReferenceEqualityComparer<T>()); }
        }

        #region IEqualityComparer<T> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int GetHashCode(T item)
        {
            return RuntimeHelpers.GetHashCode(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public bool Equals(T i1, T i2)
        {
            return ReferenceEquals(i1, i2);
        }

        #endregion
    }

    /// <summary>
    /// An equalityComparer compatible with a given comparer. All hash codes are 0, 
    /// meaning that anything based on hash codes will be quite inefficient.
    /// <para><b>Note: this will give a new EqualityComparer each time created!</b></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ComparerZeroHashCodeEqualityComparer<T> : IEqualityComparer<T>
    {
        private IComparer<T> comparer;

        /// <summary>
        /// Create a trivial <see cref="T:C5.IEqualityComparer`1"/> compatible with the 
        /// <see cref="T:C5.IComparer`1"/> <code>comparer</code>
        /// </summary>
        /// <param name="comparer"></param>
        public ComparerZeroHashCodeEqualityComparer(IComparer<T> comparer)
        {
            if (comparer == null)
                throw new NullReferenceException("Comparer cannot be null");
            this.comparer = comparer;
        }

        #region IEqualityComparer<T> Members

        /// <summary>
        /// A trivial, inefficient hash fuction. Compatible with any equality relation.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>0</returns>
        public int GetHashCode(T item)
        {
            return 0;
        }

        /// <summary>
        /// Equality of two items as defined by the comparer.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public bool Equals(T item1, T item2)
        {
            return comparer.Compare(item1, item2) == 0;
        }

        #endregion
    }

    /// <summary>
    /// Prototype for a sequenced equalityComparer for something (T) that implements ISequenced[W].
    /// This will use ISequenced[W] specific implementations of the equality comparer operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    [Serializable]
    public class SequencedCollectionEqualityComparer<T, W> : IEqualityComparer<T>
        where T : ISequenced<W>
    {
        private static SequencedCollectionEqualityComparer<T, W> cached;

        private SequencedCollectionEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static SequencedCollectionEqualityComparer<T, W> Default
        {
            get { return cached ?? (cached = new SequencedCollectionEqualityComparer<T, W>()); }
        }

        #region IEqualityComparer<T> Members

        /// <summary>
        /// Get the hash code with respect to this sequenced equalityComparer
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(T collection)
        {
            return collection.GetSequencedHashCode();
        }

        /// <summary>
        /// Check if two items are equal with respect to this sequenced equalityComparer
        /// </summary>
        /// <param name="collection1">first collection</param>
        /// <param name="collection2">second collection</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(T collection1, T collection2)
        {
            return collection1 == null ? collection2 == null : collection1.SequencedEquals(collection2);
        }

        #endregion
    }

    /// <summary>
    /// Prototype for an unsequenced equalityComparer for something (T) that implements ICollection[W]
    /// This will use ICollection[W] specific implementations of the equalityComparer operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    [Serializable]
    public class UnsequencedCollectionEqualityComparer<T, W> : IEqualityComparer<T>
        where T : ICollection<W>
    {
        private static UnsequencedCollectionEqualityComparer<T, W> cached;

        private UnsequencedCollectionEqualityComparer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static UnsequencedCollectionEqualityComparer<T, W> Default
        {
            get { return cached ?? (cached = new UnsequencedCollectionEqualityComparer<T, W>()); }
        }

        #region IEqualityComparer<T> Members

        /// <summary>
        /// Get the hash code with respect to this unsequenced equalityComparer
        /// </summary>
        /// <param name="collection">The collection</param>
        /// <returns>The hash code</returns>
        [Tested]
        public int GetHashCode(T collection)
        {
            return collection.GetUnsequencedHashCode();
        }


        /// <summary>
        /// Check if two collections are equal with respect to this unsequenced equalityComparer
        /// </summary>
        /// <param name="collection1">first collection</param>
        /// <param name="collection2">second collection</param>
        /// <returns>True if equal</returns>
        [Tested]
        public bool Equals(T collection1, T collection2)
        {
            return collection1 == null ? collection2 == null : collection1.UnsequencedEquals(collection2);
        }

        #endregion
    }
}

#if EXPERIMENTAL
namespace C5.EqualityComparerBuilder
{

  /// <summary>
  /// IEqualityComparer factory class: examines at instatiation time if T is an
  /// interface implementing "int GetHashCode()" and "bool Equals(T)".
  /// If those are not present, MakeEqualityComparer will return a default equalityComparer,
  /// else this class will implement IequalityComparer[T] via Invoke() on the
  /// reflected method infos.
  /// </summary>
  public class ByInvoke<T> : SCG.IEqualityComparer<T>
  {
    internal static readonly System.Reflection.MethodInfo hinfo, einfo;


    static ByInvoke()
    {
      Type t = typeof(T);

      if (!t.IsInterface) return;

      BindingFlags f = BindingFlags.Public | BindingFlags.Instance;

      hinfo = t.GetMethod("GetHashCode", f, null, new Type[0], null);
      einfo = t.GetMethod("Equals", f, null, new Type[1] { t }, null);
    }


    private ByInvoke() { }

/// <summary>
/// 
/// </summary>
/// <returns></returns>
    public static SCG.IEqualityComparer<T> MakeEqualityComparer()
    {
      if (hinfo != null && einfo != null)
        return new ByInvoke<T>();
      else
        return NaturalEqualityComparer<T>.Default;
    }

/// <summary>
/// 
/// </summary>
/// <param name="item"></param>
/// <returns></returns>
    public int GetHashCode(T item)
    {
      return (int)(hinfo.Invoke(item, null));
    }

/// <summary>
/// 
/// </summary>
/// <param name="i1"></param>
/// <param name="i2"></param>
/// <returns></returns>
    public bool Equals(T i1, T i2)
    {
      return (bool)(einfo.Invoke(i1, new object[1] { i2 }));
    }
  }



  /// <summary>
  /// Like ByInvoke, but tries to build a equalityComparer by RTCG to
  /// avoid the Invoke() overhead. 
  /// </summary>
  public class ByRTCG
  {
    private static ModuleBuilder moduleBuilder;

    private static AssemblyBuilder assemblyBuilder;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hinfo"></param>
    /// <param name="einfo"></param>
    /// <returns></returns>
    public static SCG.IEqualityComparer<T> CreateEqualityComparer<T>(MethodInfo hinfo, MethodInfo einfo)
    {
      if (moduleBuilder == null)
      {
        string assmname = "LeFake";
        string filename = assmname + ".dll";
        AssemblyName assemblyName = new AssemblyName("LeFake");
        AppDomain appdomain = AppDomain.CurrentDomain;
        AssemblyBuilderAccess acc = AssemblyBuilderAccess.RunAndSave;

        assemblyBuilder = appdomain.DefineDynamicAssembly(assemblyName, acc);
        moduleBuilder = assemblyBuilder.DefineDynamicModule(assmname, filename);
      }

      Type t = typeof(T);
      Type o_t = typeof(object);
      Type h_t = typeof(SCG.IEqualityComparer<T>);
      Type i_t = typeof(int);
      //TODO: protect uid for thread safety!
      string name = "C5.Dynamic.EqualityComparer_" + Guid.NewGuid().ToString();
      TypeAttributes tatt = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
      TypeBuilder tb = moduleBuilder.DefineType(name, tatt, o_t, new Type[1] { h_t });
      MethodAttributes matt = MethodAttributes.Public | MethodAttributes.Virtual;
      MethodBuilder mb = tb.DefineMethod("GetHashCode", matt, i_t, new Type[1] { t });
      ILGenerator ilg = mb.GetILGenerator();

      ilg.Emit(OpCodes.Ldarg_1);
      ilg.Emit(OpCodes.Callvirt, hinfo);
      ilg.Emit(OpCodes.Ret);
      mb = tb.DefineMethod("Equals", matt, typeof(bool), new Type[2] { t, t });
      ilg = mb.GetILGenerator();
      ilg.Emit(OpCodes.Ldarg_1);
      ilg.Emit(OpCodes.Ldarg_2);
      ilg.Emit(OpCodes.Callvirt, einfo);
      ilg.Emit(OpCodes.Ret);

      Type equalityComparer_t = tb.CreateType();
      object equalityComparer = equalityComparer_t.GetConstructor(new Type[0]).Invoke(null);

      return (SCG.IEqualityComparer<T>)equalityComparer;
    }

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <returns></returns>
    public static SCG.IEqualityComparer<T> build<T>()
    {
      MethodInfo hinfo = ByInvoke<T>.hinfo, einfo = ByInvoke<T>.einfo;

      if (hinfo != null && einfo != null)
        return CreateEqualityComparer<T>(hinfo, einfo);
      else
        return EqualityComparer<T>.Default;
    }

/// <summary>
/// 
/// </summary>
    public void dump()
    {
      assemblyBuilder.Save("LeFake.dll");
    }
  }
}
#endif