using BazarServer.Entity.Storage;

namespace BazarServer.Infrastructure.Repository
{
	public interface IPostRepository
	{
		Task<Post?> GetPostAsync(string postID);

		Task<Dictionary<string, PostStatistic>> GetPostStatisticAsync(List<string> postIDs);

		/// <summary>
		/// get posts of one user. latest at top.
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="onlyOriginalPost"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		Task<List<Post>> GetPostsByUserAsync(string userID, bool onlyOriginalPost, int page, int pageSize);

		/// <summary>
		/// return null if not found
		/// </summary>
		/// <param name="postID"></param>
		/// <returns></returns>
		Task<PostStatistic?> GetPostStatisticAsync(string postID);

		/// <summary>
		/// return next N posts where post.commandTime >= lastPostTime and lang is empty
		/// </summary>
		/// <param name="lastPostTime"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		Task<List<Post>> GetPostNoLang(long lastPostTime, int count);

		/// <summary>
		/// timeline of all known posts. latest at top.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <param name="lang"></param>
		/// <returns></returns>
		Task<List<Post>> TimelineAsync(int page, int pageSize, string? lang);

		/// <summary>
		/// upsert
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		Task SaveAsync(Post model);

		/// <summary>
		/// update or insert
		/// </summary>
		/// <param name="postID"></param>
		/// <param name="addReplyCount"></param>
		/// <param name="addRepostCount"></param>
		/// <param name="addLikeCount"></param>
		/// <returns></returns>
		Task UpsertPostStatisticAsync(string postID, int addReplyCount, int addRepostCount, int addLikeCount);

		/// <summary>
		/// update or insert
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		Task UpsertPostMeta(PostMeta model);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ay"></param>
		/// <param name="startIdx"></param>
		/// <param name="endIdx"></param>
		/// <returns></returns>
		Task<List<Post>> Search(List<string> ay, int startIdx, int endIdx);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="postID"></param>
		/// <returns></returns>
		Task<PostMeta?> GetPostMeta(string postID);

		/// <summary>
		/// deleted post is also exist.
		/// </summary>
		/// <param name="postID"></param>
		/// <returns></returns>
		Task<bool> IsExistPostAsync(string postID);

		/// <summary>
		/// return if user had liked these posts.
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="postIDs"></param>
		/// <returns></returns>
		Task<Dictionary<string, bool>> GetPostLikeAsync(string userID, List<string> postIDs);

		/// <summary>
		/// return likeID or ""
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="postID"></param>
		/// <returns></returns>
		Task<bool> GetPostLikeAsync(string userID, string postID);

		/// <summary>
		/// get direct replies for the post
		/// </summary>
		/// <param name="postID"></param>
		/// <param name="page"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		Task<List<string>> GetRepliesAsync(string postID, int page, int pageSize);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="postIDs"></param>
		/// <returns></returns>
		Task<Dictionary<string, Post>> GetPostsAsync(List<string> postIDs);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		Task<List<Post>> GetRandomPost(int count);
		Task UpsertBookmark(Bookmark model);
		Task<List<Bookmark>> GetUserBookmarks(string userID, int page, int pageSize);
		Task<Dictionary<string, bool>> GetPostBookmarkAsync(string userID, List<string> postIDs);
		Task<bool> GetPostBookmarkAsync(string userID, string postID);
	}
}