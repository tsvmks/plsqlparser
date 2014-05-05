// 
//  Copyright 2010  Deveel
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

namespace Deveel.Data.DbSystem {
	/// <summary>
	/// This object represents the lowest level <see cref="DataTable"/> information 
	/// of a given <see cref="VirtualTable"/>.
	/// </summary>
	/// <remarks>
	/// Since it is possible to make any level of <see cref="VirtualTable"/>'s, it 
	/// is useful  to be able to resolve an <i>n leveled</i> <see cref="VirtualTable"/> 
	/// to a single level table.
	/// This object is used to collect information as the <see cref="JoinedTable.ResolveToRawTable(RawTableInformation)"/> 
	/// method is walking throught the <see cref="VirtualTable"/>'s ancestors.
	/// </remarks>
	internal sealed class RawTableInformation {
		/// <summary>
		/// A Vector containing a list of DataTables, and 'row index' IntegerVectors
		/// of the given rows in the table.
		/// </summary>
		private readonly List<RawTableElement> rawInfo;

		internal RawTableInformation() {
			rawInfo = new List<RawTableElement>();
		}

		public void Add(IRootTable table, IList<long> rowSet) {
			RawTableElement elem = new RawTableElement();
			elem.Table = table;
			elem.RowSet = rowSet;
			rawInfo.Add(elem);
		}

		public Table[] GetTables() {
			int size = rawInfo.Count;
			Table[] list = new Table[size];
			for (int i = 0; i < size; ++i) {
				list[i] = (Table) rawInfo[i].Table;
			}
			return list;
		}

		public IList<long>[] GetRows() {
			int size = rawInfo.Count;
			IList<long>[] list = new IList<long>[size];
			for (int i = 0; i < size; ++i) {
				list[i] = rawInfo[i].RowSet;
			}
			return list;
		}

		private RawTableElement[] GetSortedElements() {
			var list = new RawTableElement[rawInfo.Count];
			rawInfo.CopyTo(list);
			//TODO: SortUtil.QuickSort(list);
			Array.Sort(list);
			return list;
		}

		public void Union(RawTableInformation info) {

			// Number of Table 'columns'

			int colCount = rawInfo.Count;

			// Get the sorted RawTableElement[] from each raw table information object.

			RawTableElement[] merge1 = GetSortedElements();
			RawTableElement[] merge2 = info.GetSortedElements();

			// Validates that both tables being merges are of identical type.

			int size1 = -1;
			int size2 = -1;

			// First check number of tables in each merge is correct.

			if (merge1.Length != merge2.Length)
				throw new ApplicationException("Incorrect format in table union");

			// Check each table in the merge1 set has identical length row_sets

			for (int i = 0; i < merge1.Length; ++i) {
				if (size1 == -1) {
					size1 = merge1[i].RowSet.Count;
				} else {
					if (size1 != merge1[i].RowSet.Count) {
						throw new ApplicationException("Incorrect format in table union");
					}
				}
			}

			// Check each table in the merge2 set has identical length row_sets

			for (int i = 0; i < merge2.Length; ++i) {

				// Check the tables in merge2 are identical to the tables in merge1
				// (Checks the names match, and the validColumns filters are identical
				//  see DataTableBase.TypeEquals method).

				if (!merge2[i].Table.Equals(merge1[i].Table)) {
					throw new ApplicationException("Incorrect format in table union");
				}

				if (size2 == -1) {
					size2 = merge2[i].RowSet.Count;
				} else {
					if (size2 != merge2[i].RowSet.Count) {
						throw new ApplicationException("Incorrect format in table union");
					}
				}
			}

			// If size1 or size2 are -1 then we have a corrupt table.  (It will be
			// 0 for an empty table).

			if (size1 == -1 || size2 == -1)
				throw new ApplicationException("Incorrect format in table union");

			// We don't need information in 'raw_info' vector anymore so clear it.
			// This may help garbage collection.

			rawInfo.Clear();

			// Merge the two together into a new list of RawRowElement[]

			int mergeSize = size1 + size2;
			var elems = new RawRowElement[mergeSize];
			int elemsIndex = 0;

			for (int i = 0; i < size1; ++i) {
				var e = new RawRowElement();
				e.RowVals = new long[colCount];

				for (int n = 0; n < colCount; ++n) {
					e.RowVals[n] = merge1[n].RowSet[i];
				}
				elems[elemsIndex] = e;
				++elemsIndex;
			}

			for (int i = 0; i < size2; ++i) {
				var e = new RawRowElement();
				e.RowVals = new long[colCount];

				for (int n = 0; n < colCount; ++n) {
					e.RowVals[n] = merge2[n].RowSet[i];
				}
				elems[elemsIndex] = e;
				++elemsIndex;
			}

			// Now sort the row elements into order.

			//TODO: SortUtil.QuickSort(elems);
			Array.Sort(elems);

			// Set up the 'raw_info' vector with the new RawTableElement[] removing
			// any duplicate rows.

			for (int i = 0; i < colCount; ++i) {
				RawTableElement e = merge1[i];
				e.RowSet.Clear();
			}
			RawRowElement previous = null;
			RawRowElement current = null;
			for (int n = 0; n < mergeSize; ++n) {
				current = elems[n];

				// Check that the current element in the set is not a duplicate of the
				// previous.

				if (previous == null || previous.CompareTo(current) != 0) {
					for (int i = 0; i < colCount; ++i) {
						merge1[i].RowSet.Add(current.RowVals[i]);
					}
					previous = current;
				}
			}

			for (int i = 0; i < colCount; ++i) {
				rawInfo.Add(merge1[i]);
			}

		}

		public void RemoveDuplicates() {
			// If no tables in duplicate then return

			if (rawInfo.Count == 0) {
				return;
			}

			// Get the length of the first row set in the first table.  We assume that
			// the row set length is identical across each table in the Vector.

			RawTableElement elen = rawInfo[0];
			int len = elen.RowSet.Count;
			if (len == 0) {
				return;
			}

			// Create a new row element to sort.

			var elems = new RawRowElement[len];
			int width = rawInfo.Count;

			// Create an array of RawTableElement so we can quickly access the data

			var rdup = new RawTableElement[width];
			rawInfo.CopyTo(rdup);

			// Run through the data building up a new RawTableElement[] array with
			// the information in every raw span.

			for (int i = 0; i < len; ++i) {
				var e = new RawRowElement();
				e.RowVals = new long[width];
				for (int n = 0; n < width; ++n) {
					e.RowVals[n] = rdup[n].RowSet[i];
				}
				elems[i] = e;
			}

			// Now 'elems' it an array of individual RawRowElement objects which
			// represent each individual row in the table.

			// Now sort and remove duplicates to make up a new set.

			//TODO: SortUtil.QuickSort(elems);
			Array.Sort(elems);

			// Remove all elements from the raw_info Vector.

			rawInfo.Clear();

			// Make a new set of RawTableElement[] objects

			RawTableElement[] tableElements = rdup;

			// Set up the 'raw_info' vector with the new RawTableElement[] removing
			// any duplicate rows.

			for (int i = 0; i < width; ++i) {
				tableElements[i].RowSet.Clear();
			}
			RawRowElement previous = null;
			RawRowElement current = null;
			for (int n = 0; n < len; ++n) {
				current = elems[n];

				// Check that the current element in the set is not a duplicate of the
				// previous.

				if (previous == null || previous.CompareTo(current) != 0) {
					for (int i = 0; i < width; ++i) {
						tableElements[i].RowSet.Add(current.RowVals[i]);
					}
					previous = current;
				}
			}

			for (int i = 0; i < width; ++i) {
				rawInfo.Add(tableElements[i]);
			}

		}

		private sealed class RawTableElement : IComparable {

			public IRootTable Table;
			public IList<long> RowSet;

			public int CompareTo(Object o) {
				RawTableElement rte = (RawTableElement) o;
				return Table.GetHashCode() - rte.Table.GetHashCode();
			}

		}

		private sealed class RawRowElement : IComparable {
			internal long[] RowVals;

			public int CompareTo(Object o) {
				RawRowElement rre = (RawRowElement) o;

				int size = RowVals.Length;
				for (int i = 0; i < size; ++i) {
					long v1 = RowVals[i];
					long v2 = rre.RowVals[i];
					if (v1 != v2) {
						return (int)(v1 - v2);
					}
				}
				return 0;
			}
		}
	}
}