using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class FastCopy
	{
		static Action<S, T> CreateCopier<S, T>()
		{
			var target = Expression.Parameter(typeof(T));
			var source = Expression.Parameter(typeof(S));
			var props1 = typeof(S).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead).ToList();
			var props2 = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite).ToList();

			var props = props1.Where(x => props2.Where(y => y.Name == x.Name).Any());

			var block = Expression.Block(
				from p in props
				select Expression.Assign(Expression.Property(target, p.Name), Expression.Property(source, p.Name)));
			return Expression.Lambda<Action<S, T>>(block, source, target).Compile();
		}

		static Action<S, T> CreateCopier2<S, T>()
		{
			var target = Expression.Parameter(typeof(T));
			var source = Expression.Parameter(typeof(S));
			var props1 = typeof(S).GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();
			var props2 = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();

			var props = props1.Where(x => props2.Where(y => y.Name == x.Name).Any());

			var block = Expression.Block(
				from p in props
				select Expression.Assign(Expression.Field(target, p.Name), Expression.Field(source, p.Name)));
			return Expression.Lambda<Action<S, T>>(block, source, target).Compile();
		}

		static ConcurrentDictionary<string, object> actions = new ConcurrentDictionary<string, object>();

		/// <summary>
		/// copy public properties and fields. based on name, ignore difference
		/// </summary>
		/// <typeparam name="S"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void Copy<S, T>(S from, T to)
		{
			CopyProperties(from, to);
			CopyFields(from, to);
		}

		/// <summary>
		/// copy public properties with same name, ignore difference
		/// </summary>
		/// <typeparam name="S"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void CopyProperties<S, T>(S from, T to)
		{
			string name = string.Format("prop_{0}_{1}", typeof(S), typeof(T));

			if (!actions.TryGetValue(name, out object? obj))
			{
				var ff = CreateCopier<S, T>();
				actions.TryAdd(name, ff);
				obj = ff;
			}
			Action<S, T> act = (Action<S, T>)obj;
			act(from, to);
		}

		/// <summary>
		/// copy public fields with same name, ignore difference
		/// </summary>
		/// <typeparam name="S"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void CopyFields<S, T>(S from, T to)
		{
			string name = string.Format("field_{0}_{1}", typeof(S), typeof(T));

			if (!actions.TryGetValue(name, out object? obj))
			{
				var ff = CreateCopier2<S, T>();
				actions.TryAdd(name, ff);
				obj = ff;
			}
			Action<S, T> act = (Action<S, T>)obj;
			act(from, to);
		}
	}
}
