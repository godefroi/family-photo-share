using System;
using System.Linq;
using System.Collections.Generic;

namespace photo_share_site
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<IEnumerable<T>> GroupIn<T>(this IEnumerable<T> input, int size)
		{
			var en  = input.GetEnumerator();
			var cur = new List<T>();

			while( en.MoveNext() )
			{
				cur.Add(en.Current);

				if( cur.Count >= size )
				{
					yield return cur;
					cur = new List<T>();
				}
			}

			if( cur.Count > 0 )
				yield return cur;
		}
	}
}
