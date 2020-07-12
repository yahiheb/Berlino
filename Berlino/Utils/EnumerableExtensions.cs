using System;
using System.Collections.Generic;

namespace System.Linq
{
	static class EnumerableExtensions
	{
		public static IEnumerable<(T item, uint index)> WithIndex<T>(this IEnumerable<T> self)
			=> self.Select((item, index) => (item, (uint)index));
	}
}
