using BazarServer.Entity.Storage;
using Common.Utils;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace BazarServer.Infrastructure.Storage
{
	public class MongoContext : IMongoContext
	{
		public static string dbName { get; } = "BazarServer";

		private string ConnectionString { get; }

		public MongoClient Client { get; private set; }

		private IMongoDatabase db;

		public MongoContext()
		{
			ConnectionString = ConfigHelper.GetConfigValue(null, "BazarMongodb");

			var cc = new MongoClient(ConnectionString);
			db = cc.GetDatabase(dbName);

			var pack = new ConventionPack
			{
				new IgnoreExtraElementsConvention(true)
			};
			ConventionRegistry.Register("myconvention", pack, t => true);

			BuildIndex<UserCommand>(nameof(UserCommand.commandID));
			BuildIndex<UserCommand>(nameof(UserCommand.userID));
			BuildIndex<UserCommand>(nameof(UserCommand.commandTime));
			BuildIndex<UserCommand>(nameof(UserCommand.receiveTime));
			BuildIndex<UserCommand>(nameof(UserCommand.receiveOffset));

			BuildIndex<FailureCommand>(nameof(FailureCommand.commandID));

			BuildIndex<UserInfo>(nameof(UserInfo.userID));

			BuildIndex<UserPic>(nameof(UserPic.userID));

			BuildIndex<UserStatistic>(nameof(UserStatistic.userID));

			BuildIndex<Channel>(nameof(Channel.channelID));
			BuildIndex<Channel>(nameof(Channel.userID));

			BuildIndex<ChannelMember>(nameof(ChannelMember.channelID));
			BuildIndex<ChannelMember>(nameof(ChannelMember.userID));

			BuildIndex<Following>(nameof(Following.userID));
			BuildIndex<Following>(nameof(Following.targetID));
			BuildIndex<Following>(nameof(Following.commandTime));

			BuildIndex<Post>(nameof(Post.postID));
			BuildIndex<Post>(nameof(Post.userID));
			BuildIndex<Post>(nameof(Post.threadID));
			BuildIndex<Post>(nameof(Post.replyTo));
			BuildIndex<Post>(nameof(Post.commandTime));

			BuildIndex<PostStatistic>(nameof(PostStatistic.postID));

			BuildIndex<PostMeta>(nameof(PostMeta.postID));

			BuildIndex<Like>(nameof(Like.userID));
			BuildIndex<Like>(nameof(Like.postID));

			BuildIndex<NotifyMessage>(nameof(NotifyMessage.notifyID));
			BuildIndex<NotifyMessage>(nameof(NotifyMessage.userID));
			BuildIndex<NotifyMessage>(nameof(NotifyMessage.notifyTime));

			BuildIndex<NotifyGetTime>(nameof(NotifyGetTime.userID));

			BuildIndex<PeerServer>(nameof(PeerServer.BaseUrl));

			BuildFullTextIndex<UserInfo>();
			BuildFullTextIndex<Post>();

			Client = new MongoClient(ConnectionString);
		}

		private void BuildFullTextIndex<T>()
		{
			var tblName = typeof(T).Name;
			try
			{
				//see also https://stackoverflow.com/questions/41356544/full-text-search-in-mongodb-in-net/41357957
				//cosmos may not support this? see also https://docs.microsoft.com/en-us/azure/cosmos-db/mongodb/mongodb-indexing
				//maybe need this: https://docs.microsoft.com/en-us/answers/questions/389626/does-azure-cosmodb-support-full-text-search.html
				var keys = Builders<T>.IndexKeys.Text("$**");
				var model = new CreateIndexModel<T>(keys);
				var ss = db.GetCollection<T>(tblName).Indexes.CreateOne(model);
			}
			catch (Exception ex)
			{
				MailHelper.ReportMail($"fail to BuildFullTextIndex: {tblName}", ex);
			}
		}

		public void BuildIndex<T>(string field)
		{
			var tblName = typeof(T).Name;
			try
			{
				var keys = Builders<T>.IndexKeys.Ascending(field);
				var pp = new CreateIndexModel<T>(keys);
				var ss = db.GetCollection<T>(tblName).Indexes.CreateOne(pp);

				return;
			}
			catch (Exception ex)
			{
				MailHelper.ReportMail($"fail to BuildIndex: {tblName}", ex);
			}
		}

	}
}
