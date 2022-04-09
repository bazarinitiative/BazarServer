using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Repository;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BazarServer.Infrastructure.Users.Tests
{

	[TestClass()]
	public class UserRepositoryTests : BaseTest
	{

		[TestMethod()]
		public async Task UserRepositoryTestAsync()
		{
			IUserRepository userRepository = provider.GetService<IUserRepository>();
			var ret = await userRepository.GetUserStatisticAsync("111");

			ret.Should().BeNull();
		}

		[TestMethod()]
		public void AddUserAsyncTest()
		{
		}

		[TestMethod()]
		public void GetUserInfoAsyncTest()
		{
		}

		[TestMethod()]
		public void GetUserRetrieveAsyncTest()
		{
		}

		[TestMethod()]
		public void GetUserStatisticAsyncTest()
		{
		}

		[TestMethod()]
		public void UpdateUserStatisticAsyncTest()
		{
		}

		[TestMethod()]
		public void UpdateUserAsyncTest()
		{
		}

		[TestMethod()]
		public void UploadUserAsyncTest()
		{
		}
	}
}