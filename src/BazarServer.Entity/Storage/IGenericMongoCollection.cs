using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq.Expressions;

namespace BazarServer.Entity.Storage
{
	public interface IGenericMongoCollection<TEntity> where TEntity : class
	{
		/// <summary>
		/// beware! use Upsert instead!
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		Task AddAsync(TEntity item);

		/// <summary>
		/// return first document that satisfy filter, or default value.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);

		/// <summary>
		/// remove all documents that satisfy filter.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		Task RemoveAsync(Expression<Func<TEntity, bool>> filter);

		/// <summary>
		/// update
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		Task UpdateAsync(Expression<Func<TEntity, bool>> filter, TEntity entity);

		/// <summary>
		/// update or insert
		/// </summary>
		/// <returns></returns>
		Task UpsertAsync(Expression<Func<TEntity, bool>> filter, TEntity entity);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IMongoQueryable<TEntity> GetQueryable();

		/// <summary>
		/// mongodb.linq do not support List.Contains, so we implement one.
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="fieldName"></param>
		/// <param name="fieldValues"></param>
		/// <returns></returns>
		Task<List<TEntity>> InAsync<TField>(string fieldName, IEnumerable<TField> fieldValues);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="fieldName"></param>
		/// <param name="fieldValues"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		Task<List<TEntity>> InFilterAsync<TField>(string fieldName, IEnumerable<TField> fieldValues, Expression<Func<TEntity, bool>> filter);

		/// <summary>
		/// paging, you know
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="filter"></param>
		/// <param name="orderby"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <param name="descending"></param>
		/// <returns></returns>
		Task<List<TEntity>> PageAsync<TField>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TField>> orderby, int page, int pageSize, bool descending = true);

		/// <summary>
		/// get list by filter
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter);

		/// <summary>
		/// count by filter
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		Task<long> CountAsync(Expression<Func<TEntity, bool>> filter);

		/// <summary>
		/// get random documents from db.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		Task<List<TEntity>> Random(int count);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ay"></param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		Task<List<TEntity>> Search(List<string> ay, int maxCount);
	}
}