﻿using BazarServer.Entity.Storage;

namespace BazarServer.Application.PeerServers
{
	public interface IPeerManager
	{

		Task<List<UserCommand>> RetrieveUserCommandBatch(long lastOffset, int forwardCount);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		int GetServerCount();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="topNReputation">top N servers of highest Reputation</param>
		/// <returns></returns>
		List<PeerServer> GetAllServers(int topNReputation);

		/// <summary>
		/// some remote ask to register a new peer
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <returns></returns>
		Task<(bool success, string msg)> RegisterAsync(string baseUrl);

		/// <summary>
		/// start a timer thread to sync data with peers
		/// </summary>
		/// <returns></returns>
		Task Start();

		Task Stop();
	}
}
