using MongoDB.Driver;

namespace BazarServer.Infrastructure.Storage
{
	/// <summary>
	/// will read config and construct MongoClient.
	/// </summary>
	public interface IMongoContext
	{
		MongoClient Client { get; }

		/// <summary>
		/// We don't use uniqueKey because of CosmosDB partition.
		/// We tolerate duplicate data for better performance and better capacity.
		/// We don't use transaction for the same reason.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="field"></param>
		void BuildIndex<T>(string field);
	}
}