/* Copyright © 2002-2004 by Aidant Systems, Inc., and by Jason Smith. */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Iesi_NTS.Collections.Generic
{
	/// <summary><p>A collection that contains no duplicate elements.  This class models the mathematical
	/// <c>Set</c> abstraction, and is the base class for all other <c>Set</c> implementations.  
	/// The order of elements in a set is dependant on (a)the data-structure implementation, and 
	/// (b)the implementation of the various <c>Set</c> methods, and thus is not guaranteed.</p>
	///  
	/// <p>None of the <c>Set</c> implementations in this library are guranteed to be thread-safe
	/// in any way.</p>
	/// 
	/// <p>The following table summarizes the binary operators that are supported by the <c>Set</c> class.</p>
	/// <list type="table">
	///		<listheader>
	///			<term>Operation</term>
	///			<term>Description</term>
	///			<term>Method</term>
	///			<term>Operator</term>
	///		</listheader>
	///		<item>
	///			<term>Union (OR)</term>
	///			<term>Element included in result if it exists in either <c>A</c> OR <c>B</c>.</term>
	///			<term><c>Union()</c></term>
	///			<term><c>|</c></term>
	///		</item>
	///		<item>
	///			<term>Intersection (AND)</term>
	///			<term>Element included in result if it exists in both <c>A</c> AND <c>B</c>.</term>
	///			<term><c>InterSect()</c></term>
	///			<term><c>&amp;</c></term>
	///		</item>
	///		<item>
	///			<term>Exclusive Or (XOR)</term>
	///			<term>Element included in result if it exists in one, but not both, of <c>A</c> and <c>B</c>.</term>
	///			<term><c>ExclusiveOr()</c></term>
	///			<term><c>^</c></term>
	///		</item>
	///		<item>
	///			<term>Minus (n/a)</term>
	///			<term>Take all the elements in <c>A</c>.  Now, if any of them exist in <c>B</c>, remove
	///			them.  Note that unlike the other operators, <c>A - B</c> is not the same as <c>B - A</c>.</term>
	///			<term><c>Minus()</c></term>
	///			<term><c>-</c></term>
	///		</item>
	/// </list>
	/// </summary>
	/// <remarks>
    ///     Set&lt;T&gt; supports operator between Set&lt;T&gt; and ISet, however, the ISet must contains only elements of type T
	/// </remarks>
	[Serializable]
    public abstract class Set<T> : ISet<T>, ISet
	{
        #region members of ISet<T>
        /// <summary>
        /// Performs a "union" of the two sets, where all the elements
        /// in both sets are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// Neither this set nor the input set are modified during the operation.  The return value
        /// is a <c>Clone()</c> of this set with the extra elements added in.
        /// </summary>
        /// <param name="a">A collection of elements.</param>
        /// <returns>A new <c>Set</c> containing the union of this <c>Set</c> with the specified collection.
        /// Neither of the input objects is modified by the union.</returns>
        public virtual ISet<T> Union(ISet<T> a)
        {
            ISet<T> resultSet = this.Clone();
            if (a != null)
                resultSet.AddAll(a);
            return resultSet;
        }



        /// <summary>
        /// Performs a "union" of two sets, where all the elements
        /// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// The return value is a <c>Clone()</c> of one of the sets (<c>a</c> if it is not <c>null</c>) with elements of the other set
        /// added in.  Neither of the input sets is modified by the operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing the union of the input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static ISet<T> Union(ISet<T> a, ISet<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.Clone();
            else if (b == null)
                return a.Clone();
            else
                return a.Union(b);
        }

        /// <summary>
        /// Performs a "union" of two sets, where all the elements
        /// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
        /// The return value is a <c>Clone()</c> of one of the sets (<c>a</c> if it is not <c>null</c>) with elements of the other set
        /// added in.  Neither of the input sets is modified by the operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing the union of the input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static Set<T> operator |(Set<T> a, Set<T> b)
        {
            return (Set<T>)Union(a, b);
        }

        /// <summary>
        /// Performs an "intersection" of the two sets, where only the elements
        /// that are present in both sets remain.  That is, the element is included if it exists in
        /// both sets.  The <c>Intersect()</c> operation does not modify the input sets.  It returns
        /// a <c>Clone()</c> of this set with the appropriate elements removed.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <returns>The intersection of this set with <c>a</c>.</returns>
        public virtual ISet<T> Intersect(ISet<T> a)
        {
            ISet<T> resultSet = this.Clone();
            if (a != null)
                resultSet.RetainAll(a);
            else
                resultSet.Clear();
            return resultSet;
        }



        /// <summary>
        /// Performs an "intersection" of the two sets, where only the elements
        /// that are present in both sets remain.  That is, the element is included only if it exists in
        /// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
        /// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
        /// elements from the intersect operation. 
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>The intersection of the two input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static ISet<T> Intersect(ISet<T> a, ISet<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
            {
                return b.Intersect(a);
            }
            else
                return a.Intersect(b);
        }

        /// <summary>
        /// Performs an "intersection" of the two sets, where only the elements
        /// that are present in both sets remain.  That is, the element is included only if it exists in
        /// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
        /// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
        /// elements from the intersect operation. 
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>The intersection of the two input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static Set<T> operator &(Set<T> a, Set<T> b)
        {
            return (Set<T>)Intersect(a, b);
        }

        /// <summary>
        /// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
        /// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
        /// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
        /// of this <c>Set</c> containing the elements from the operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <returns>A set containing the elements from this set with the elements in <c>a</c> removed.</returns>
        public virtual ISet<T> Minus(ISet<T> a)
        {
            ISet<T> resultSet = this.Clone();
            if (a != null)
                resultSet.RemoveAll(a);
            return resultSet;
        }



        /// <summary>
        /// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
        /// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
        /// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
        /// of set <c>a</c> containing the elements from the operation. 
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
        public static ISet<T> Minus(ISet<T> a, ISet<T> b)
        {
            if (a == null)
                return null;
            else
                return a.Minus(b);
        }

        /// <summary>
        /// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
        /// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
        /// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
        /// of set <c>a</c> containing the elements from the operation. 
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
        public static Set<T> operator -(Set<T> a, Set<T> b)
        {
            return (Set<T>)Minus(a, b);
        }


        /// <summary>
        /// Performs an "exclusive-or" of the two sets, keeping only the elements that
        /// are in one of the sets, but not in both.  The original sets are not modified
        /// during this operation.  The result set is a <c>Clone()</c> of this set containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <returns>A set containing the result of <c>a ^ b</c>.</returns>
        public virtual ISet<T> ExclusiveOr(ISet<T> a)
        {
            ISet<T> resultSet = this.Clone();
            foreach (T element in a)
            {
                if (resultSet.Contains(element))
                    resultSet.Remove(element);
                else
                    resultSet.Add(element);
            }
            return resultSet;
        }


        /// <summary>
        /// Performs an "exclusive-or" of the two sets, keeping only the elements that
        /// are in one of the sets, but not in both.  The original sets are not modified
        /// during this operation.  The result set is a <c>Clone()</c> of one of the sets
        /// (<c>a</c> if it is not <c>null</c>) containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing the result of <c>a ^ b</c>.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static ISet<T> ExclusiveOr(ISet<T> a, ISet<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.Clone();
            else if (b == null)
                return a.Clone();
            else
                return a.ExclusiveOr(b);
        }

        /// <summary>
        /// Performs an "exclusive-or" of the two sets, keeping only the elements that
        /// are in one of the sets, but not in both.  The original sets are not modified
        /// during this operation.  The result set is a <c>Clone()</c> of one of the sets
        /// (<c>a</c> if it is not <c>null</c>) containing
        /// the elements from the exclusive-or operation.
        /// </summary>
        /// <param name="a">A set of elements.</param>
        /// <param name="b">A set of elements.</param>
        /// <returns>A set containing the result of <c>a ^ b</c>.  <c>null</c> if both sets are <c>null</c>.</returns>
        public static Set<T> operator ^(Set<T> a, Set<T> b)
        {
            return (Set<T>)ExclusiveOr(a, b);
        }

        /// <summary>
        /// Adds the specified element to this set if it is not already present.
        /// </summary>
        /// <param name="o">The object to add to the set.</param>		
        public abstract bool Add(T o);


        /// <summary>
        /// Adds all the elements in the specified collection to the set if they are not already present.
        /// </summary>
        /// <param name="c">A collection of objects to add to the set.</param>
        /// <returns><c>true</c> is the set changed as a result of this operation, <c>false</c> if not.</returns>
        public abstract bool AddAll(ICollection<T> c);


        /// <summary>
        /// Returns <c>true</c> if the set contains all the elements in the specified collection.
        /// </summary>
        /// <param name="c">A collection of objects.</param>
        /// <returns><c>true</c> if the set contains all the elements in the specified collection, <c>false</c> otherwise.</returns>
        public abstract bool ContainsAll(ICollection<T> c);

        /// <summary>
        /// Removes the specified element from the set.
        /// </summary>
        /// <param name="o">The element to be removed.</param>
        /// <returns><c>true</c> if the set contained the specified element, <c>false</c> otherwise.</returns>
        public abstract bool Remove(T o);
        /// <summary>
        /// Remove all the specified elements from this set, if they exist in this set.
        /// </summary>
        /// <param name="c">A collection of elements to remove.</param>
        /// <returns><c>true</c> if the set was modified as
        /// a result of this operation.</returns>
        public abstract bool RemoveAll(ICollection<T> c);


        /// <summary>
        /// Retains only the elements in this set that are contained in the specified collection.
        /// </summary>
        /// <param name="c">Collection that defines the set of elements to be retained.</param>
        /// <returns><c>true</c> if this set changed as a result of this operation.</returns>
        public abstract bool RetainAll(ICollection<T> c);

        /// <summary>
        /// Removes all objects from the set.
        /// </summary>
        public abstract void Clear();


        /// <summary>
        /// Returns <c>true</c> if this set contains no elements.
        /// </summary>
        public abstract bool IsEmpty { get;}

        /// <summary>
        /// Returns <c>true</c> if this set contains the specified element.
        /// </summary>
        /// <param name="o">The element to look for.</param>
        /// <returns><c>true</c> if this set contains the specified element, <c>false</c> otherwise.</returns>
        public abstract bool Contains(T o);

	    #endregion

        #region members of ISet


        bool ISet.Add(object o)
        {
            return Add((T)o);
        }
	    
        bool ISet.AddAll(ICollection c)
        {
            return AddAll(getCollectionWrapper(c));
        }

        bool ISet.Contains(object o)
        {
            return Contains((T)o);
        }

        bool ISet.Remove(object o)
        {
            return Remove((T)o);
        }

        bool ISet.ContainsAll(ICollection c)
        {
            return ContainsAll(getCollectionWrapper(c));
        }

        bool ISet.RetainAll(ICollection c)
        {
            return RetainAll(getCollectionWrapper(c));
        }

        bool ISet.RemoveAll(ICollection c)
        {
            return RemoveAll(getCollectionWrapper(c));
        }

        ISet ISet.Union(ISet a)
        {
            return (ISet)Union(getWrapper(a));
        }
        
	    ISet ISet.Minus(ISet a)
        {

            return (ISet)Minus(getWrapper(a));
        }
	    
        ISet ISet.Intersect(ISet a)
        {
            return (ISet)Intersect(getWrapper(a));
        }

        ISet ISet.ExclusiveOr(ISet a)
        {
            return (ISet)ExclusiveOr(getWrapper(a));
        }
	    
        #endregion
	    
        #region members of ICloneable
        /// <summary>
        /// Returns a clone of the <c>Set</c> instance.  This will work for derived <c>Set</c>
        /// classes if the derived class implements a constructor that takes no arguments.
        /// </summary>
        /// <returns>A clone of this object.</returns>
        public virtual ISet<T> Clone()
        {
            Set<T> newSet = (Set<T>)Activator.CreateInstance(this.GetType());
            newSet.AddAll(this);
            return newSet;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        
        #endregion
		
	    #region members of ICollection
        
	    void ICollection.CopyTo(Array array, int index)
        {
            CheckTargetArray(array, index);
	        T[] buf = new T[this.Count];
	        CopyTo(buf, index);
            Array.Copy(buf, 0, array, index, this.Count);
        }

        /// <summary>
        /// The number of elements currently contained in this collection.
        /// </summary>
        public abstract int Count { get;}

    

        /// <summary>
        /// Returns <c>true</c> if the <c>Set</c> is synchronized across threads.  Note that
        /// enumeration is inherently not thread-safe.  Use the <c>SyncRoot</c> to lock the
        /// object during enumeration.
        /// </summary>
        public abstract bool IsSynchronized { get;}

        /// <summary>
        /// An object that can be used to synchronize this collection to make it thread-safe.
        /// When implementing this, if your object uses a base object, like an <c>IDictionary</c>,
        /// or anything that has a <c>SyncRoot</c>, return that object instead of "<c>this</c>".
        /// </summary>
        public abstract object SyncRoot { get;} 
        #endregion
	    
        #region members of ICollection<T>


        void ICollection<T>.Add(T o)
        {
            Add(o);
        }

        /// <summary>
        /// Copies the elements in the <c>Set</c> to an array.  The type of array needs
        /// to be compatible with the objects in the <c>Set</c>, obviously.
        /// </summary>
        /// <param name="array">An array that will be the target of the copy operation.</param>
        /// <param name="index">The zero-based index where copying will start.</param>
        public abstract void CopyTo(T[] array, int index);

        /// <summary>
        /// Returns whether this collection is readonly.
        /// </summary>
        public virtual bool IsReadOnly { get { return false; } }

          #endregion

        #region members of IEnumerable<T>
        /// <summary>
        /// Gets an enumerator for the elements in the <c>Set</c>.
        /// </summary>
        /// <returns>An <c>IEnumerator</c> over the elements in the <c>Set</c>.</returns>
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        } 
        #endregion

        #region Private methods
        private ISet<T> getWrapper(ISet set)
        {
            if (set == null) return null;
            return new SetWrapper<T>(set);
        }

        private ICollection<T> getCollectionWrapper(ICollection c)
        {
            if (c == null) return null;
            return new CollectionWrapper<T>(c);
        }

        private void CheckTargetArray(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank > 1)
                throw new ArgumentException(
                    "Argument cannot be multidimensional.", "array");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex",
                    arrayIndex, "Argument cannot be negative.");
            if (arrayIndex >= array.Length)
                throw new ArgumentException(
                    "Argument must be less than array length.", "arrayIndex");

            if (this.Count > array.Length - arrayIndex)
                throw new ArgumentException(
                    "Argument section must be large enough for collection.", "array");
        }
        
        #endregion
	}
}
