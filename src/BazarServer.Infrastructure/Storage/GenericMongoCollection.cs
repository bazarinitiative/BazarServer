using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace BazarServer.Infrastructure.Storage
{
	public class GenericMongoCollection<TEntity> : IGenericMongoCollection<TEntity> where TEntity : class
	{
		IMongoCollection<TEntity> set;
		IMongoContext db;

		public GenericMongoCollection(IMongoContext mongoContext)
		{
			this.db = mongoContext;
			var name = typeof(TEntity).Name;
			set = db.Client.GetDatabase(MongoContext.dbName).GetCollection<TEntity>(name);
		}

		public async Task AddAsync(TEntity item)
		{
			await Task.CompletedTask;

			throw new Exception("Here we never use add to minimize duplicate data, because we don't use transaction nor unique index. Please use upsert instead.");
		}

		public async Task RemoveAsync(Expression<Func<TEntity, bool>> filter)
		{
			await set.DeleteManyAsync(filter);
		}

		public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
		{
			try
			{
				var ret = await set.AsQueryable().FirstOrDefaultAsync(filter);
				return ret;
			}
			catch (Exception ex)
			{
				var ss = ex.ToString();

				throw;
			}
		}

		public async Task UpdateAsync(Expression<Func<TEntity, bool>> filter, TEntity entity)
		{
			await set.FindOneAndReplaceAsync(filter, entity, new FindOneAndReplaceOptions<TEntity>() { IsUpsert = false });
		}

		public IMongoQueryable<TEntity> GetQueryable()
		{
			return set.AsQueryable();
		}

		public async Task<List<TEntity>> PageAsync<TField>(Expression<Func<TEntity, bool>> filter,
													 Expression<Func<TEntity, TField>> orderby,
													 int page,
													 int pageSize,
													 bool descending = true)
		{
			var qry = set.AsQueryable().Where(filter);

			if (descending)
			{
				var list = await qry.OrderByDescending(orderby)
					.Skip(page * pageSize)
					.Take(pageSize)
					.ToListAsync();
				return list;
			}
			else
			{
				var list = await qry.OrderBy(orderby)
					.Skip(page * pageSize)
					.Take(pageSize)
					.ToListAsync();
				return list;
			}
		}

		public async Task<List<TEntity>> InAsync<TField>(string fieldName, IEnumerable<TField> fieldValues)
		{
			try
			{
				var filter = Builders<TEntity>.Filter.In(fieldName, fieldValues);
				var find = await set.FindAsync(filter);
				var list = await find.ToListAsync();
				return list;
			}
			catch (Exception ex)
			{
				var ss = ex.ToString();

				throw;
			}
		}

		public async Task<List<TEntity>> InFilterAsync<TField>(string fieldName, IEnumerable<TField> fieldValues, Expression<Func<TEntity, bool>> filter)
		{
			try
			{
				var ft1 = Builders<TEntity>.Filter.In(fieldName, fieldValues);
				var ft2 = Builders<TEntity>.Filter.Where(filter);
				var ft = Builders<TEntity>.Filter.And(ft1, ft2);
				var find = await set.FindAsync(ft);
				var list = await find.ToListAsync();
				return list;
			}
			catch (Exception ex)
			{
				var ss = ex.ToString();

				throw;
			}
		}

		public async Task UpsertAsync(Expression<Func<TEntity, bool>> filter, TEntity entity)
		{
			await set.FindOneAndReplaceAsync(filter, entity, new FindOneAndReplaceOptions<TEntity>() { IsUpsert = true });
		}

		public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter)
		{
			var ret = await set.AsQueryable().Where(filter).ToListAsync();
			return ret;
		}

		public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
		{
			var ret = await set.CountDocumentsAsync(filter);
			return ret;
		}

		public async Task<List<TEntity>> Random(int count)
		{
			var ret = await set.AsQueryable().Sample(count).ToListAsync();
			return ret;
		}

		public async Task<List<TEntity>> Search<TKey>(List<string> ay, int startIdx, int endIdx, Expression<Func<TEntity, TKey>>? keySelector = null)
		{
			var search = string.Join(' ', ay);
			var filter = Builders<TEntity>.Filter.Text(search);
			var query = set.AsQueryable().Where(_ => filter.Inject());
			if (keySelector != null)
			{
				query = query.OrderByDescending(keySelector);
			}
			var ret = await query.Skip(startIdx).Take(endIdx - startIdx).ToListAsync();
			return ret;
		}
	}

}
