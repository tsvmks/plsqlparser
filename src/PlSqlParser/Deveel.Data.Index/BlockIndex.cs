// 
//  Copyright 2010-2014 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Index {
	///<summary>
	/// An implementation of <see cref="BlockIndexBase"/> that stores 
	/// all values in blocks that are entirely stored in main memory.
	///</summary>
	/// <remarks>
	/// This type of structure is useful for large in-memory lists in which a
	/// dd/remove performance must be fast.
	/// </remarks>
	public class BlockIndex<T> : BlockIndexBase<T> where T : IComparable {
		public BlockIndex() {
		}

		public BlockIndex(IEnumerable<T> values)
			: base(values) {
		}

		public BlockIndex(IIndex<T> index)
			: base(index) {
		}

		protected BlockIndex(IEnumerable<IIndexBlock<T>> blocks)
			: base(blocks) {
		}

		protected override IIndexBlock<T> NewBlock() {
			return new Block<T>(512);
		}

		#region Block

		protected class Block<T> : IIndexBlock<T> where T : IComparable {
			private Block<T> next;
			private Block<T> prev;
			private T[] array;
			private int count;
			private bool changed;

			protected Block() {	
			}

			public Block(int blockSize)
				: this() {
				array = new T[blockSize];
				count = 0;
			}

			protected T[] BaseArray {
				get { return array; }
				set { array = value; }
			}

			protected virtual int ArrayLength {
				get { return array.Length; }
			}

			public IEnumerator<T> GetEnumerator() {
				return new Enumerator(this);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			IIndexBlock<T> IIndexBlock<T>.Next {
				get { return Next; }
				set { Next = (Block<T>) value; }
			}

			public Block<T> Next {
				get { return next; }
				set { next = value; }
			}

			IIndexBlock<T> IIndexBlock<T>.Previous {
				get { return Previous; }
				set { Previous = (Block<T>) value; }
			}

			public Block<T> Previous {
				get { return prev; }
				set { prev = value; }
			}

			public bool HasChanged {
				get { return changed; }
			}

			public int Count {
				get { return count; }
				protected set { count = value; }
			}

			public bool IsFull {
				get { return count >= ArrayLength; }
			}

			public bool IsEmpty {
				get { return count <= 0; }
			}

			public virtual T Top {
				get { return GetArray(true)[count - 1]; }
			}

			public virtual T Bottom {
				get {
					if (count <= 0)
						throw new ApplicationException("no bottom value.");

					return GetArray(true)[0];
				}
			}

			public T this[int index] {
				get { return GetArray(true)[index]; }
				set { 
					changed = true;
					GetArray(false)[index] = value;
				}
			}

			protected virtual T[] GetArray(bool readOnly) {
				return array;
			}

			public bool CanContain(int number) {
				return count + number + 1 < ArrayLength;
			}

			public void Add(T value) {
				changed = true;
				T[] arr = GetArray(false);
				arr[count] = value;
				++count;
			}

			public T RemoveAt(int index) {
				changed = true;
				T[] arr = GetArray(false);
				T val = arr[index];
				Array.Copy(array, index + 1, arr, index, (count - index));
				--count;
				return val;
			}

			public int IndexOf(T value) {
				T[] arr = GetArray(true);
				for (int i = count - 1; i >= 0; --i) {
					if (arr[i].Equals(value))
						return i;
				}
				return -1;
			}

			public int IndexOf(T value, int startIndex) {
				T[] arr = GetArray(true);
				for (int i = startIndex; i < count; ++i) {
					if (arr[i].Equals(value))
						return i;
				}
				return -1;
			}

			public void Insert(int index, T value) {
				changed = true;
				T[] arr = GetArray(false);
				Array.Copy(array, index, arr, index + 1, (count - index));
				++count;
				arr[index] = value;
			}

			public void MoveTo(IIndexBlock<T> destBlock, int destIndex, int length) {
				Block<T> block = (Block<T>)destBlock;

				T[] arr = GetArray(false);
				T[] dest_arr = block.GetArray(false);

				// Make room in the destination block
				int destb_size = block.Count;
				if (destb_size > 0) {
					Array.Copy(dest_arr, 0, dest_arr, length, destb_size);
				}
				// Copy from this block into the destination block.
				Array.Copy(arr, count - length, dest_arr, 0, length);
				// Alter size of destination and source block.
				block.count += length;
				count -= length;
				// Mark both blocks as changed
				changed = true;
				block.changed = true;
			}

			public void CopyTo(IIndexBlock<T> destBlock) {
				Block<T> block = (Block<T>)destBlock;
				T[] destArr = block.GetArray(false);
				Array.Copy(GetArray(true), 0, destArr, 0, count);
				block.count = count;
				block.changed = true;
			}

			public int CopyTo(T[] destArray, int arrayIndex) {
				Array.Copy(GetArray(true), 0, destArray, arrayIndex, count);
				return count;
			}

			public void Clear() {
				changed = true;
				count = 0;
			}

			public int BinarySearch(object key, IIndexComparer<T> comparer) {
				T[] arr = GetArray(true);
				int low = 0;
				int high = count - 1;

				while (low <= high) {
					int mid = (low + high) / 2;
					int cmp = comparer.CompareValue(arr[mid], (DataObject) key);

					if (cmp < 0)
						low = mid + 1;
					else if (cmp > 0)
						high = mid - 1;
					else
						return mid; // key found
				}
				return -(low + 1);  // key not found.
			}

			public int SearchFirst(object key, IIndexComparer<T> comparer) {
				T[] arr = GetArray(true);
				int low = 0;
				int high = count - 1;

				while (low <= high) {
					if (high - low <= 2) {
						for (int i = low; i <= high; ++i) {
							int cmp1 = comparer.CompareValue(arr[i], (DataObject) key);
							if (cmp1 == 0)
								return i;
							if (cmp1 > 0)
								return -(i + 1);
						}
						return -(high + 2);
					}

					int mid = (low + high) / 2;
					int cmp = comparer.CompareValue(arr[mid], (DataObject) key);

					if (cmp < 0) {
						low = mid + 1;
					} else if (cmp > 0) {
						high = mid - 1;
					} else {
						high = mid;
					}
				}
				return -(low + 1);  // key not found.

			}

			public int SearchLast(object key, IIndexComparer<T> comparer) {
				T[] arr = GetArray(true);
				int low = 0;
				int high = count - 1;

				while (low <= high) {

					if (high - low <= 2) {
						for (int i = high; i >= low; --i) {
							int cmp1 = comparer.CompareValue(arr[i], (DataObject) key);
							if (cmp1 == 0)
								return i;
							if (cmp1 < 0)
								return -(i + 2);
						}
						return -(low + 1);
					}

					int mid = (low + high) / 2;
					int cmp = comparer.CompareValue(arr[mid], (DataObject) key);

					if (cmp < 0) {
						low = mid + 1;
					} else if (cmp > 0) {
						high = mid - 1;
					} else {
						low = mid;
					}
				}
				return -(low + 1);  // key not found.
			}

			public int SearchFirst(T value) {
				T[] arr = GetArray(true);
				int low = 0;
				int high = count - 1;

				while (low <= high) {

					if (high - low <= 2) {
						for (int i = low; i <= high; ++i) {
							if (arr[i].Equals(value))
								return i;
							if (arr[i].CompareTo(value) < 0)
								return -(i + 1);
						}
						return -(high + 2);
					}

					int mid = (low + high) / 2;

					if (arr[mid].CompareTo(value) > 0) {
						low = mid + 1;
					} else if (arr[mid].CompareTo(value) < 0) {
						high = mid - 1;
					} else {
						high = mid;
					}
				}
				return -(low + 1);  // key not found.
			}

			public int SearchLast(T value) {
				T[] arr = GetArray(true);
				int low = 0;
				int high = count - 1;

				while (low <= high) {

					if (high - low <= 2) {
						for (int i = high; i >= low; --i) {
							if (arr[i].CompareTo(value) == 0)
								return i;
							if (arr[i].CompareTo(value) > 0)
								return -(i + 2);
						}
						return -(low + 1);
					}

					int mid = (low + high) / 2;

					if (arr[mid].CompareTo(value) > 0) {
						low = mid + 1;
					} else if (arr[mid].CompareTo(value) < 0) {
						high = mid - 1;
					} else {
						low = mid;
					}
				}
				return -(low + 1);  // key not found.
			}

			#region Enumerator

			class Enumerator : IEnumerator<T> {
				private readonly Block<T> block;
				private int index;
				private T[] array;

				public Enumerator(Block<T> block) {
					this.block = block;
					array = block.GetArray(true);
					index = -1;
				}

				public void Dispose() {
				}

				public bool MoveNext() {
					return ++index < array.Length;
				}

				public void Reset() {
					array = block.GetArray(true);
					index = -1;
				}

				public T Current {
					get { return array[index]; }
				}

				object IEnumerator.Current {
					get { return Current; }
				}
			}

			#endregion
		}

		#endregion
	}
}