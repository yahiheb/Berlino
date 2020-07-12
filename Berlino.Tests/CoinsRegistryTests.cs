#if false
using System;
using NBitcoin;
using Xunit;

namespace Berlino.Tests
{
	public class CoinsRegistryTests
	{
		[Fact]
		public void Test1()
		{
			var registry = new CoinsRegistry();
			var key0 = new Key();
			var key1 = new Key();

			var stx0 = TransactionBuilders.CreateCreditingTransaction(key0.PubKey.Hash, Money.Coins(2));
			var stx1 = TransactionBuilders.CreateCreditingTransaction(key1.PubKey.Hash, Money.Coins(5));

			var coin0 = new SmartCoin(stx0, 0);
			Assert.True(registry.TryAdd(coin0));
			Assert.False(registry.TryAdd(coin0));
			Assert.False(registry.TryAdd(coin0.AsSpentBy(uint256.One)));

			var registeredCoin = Assert.Single(registry.AsCoinsView());
			Assert.Equal(registeredCoin, coin0);
			Assert.True(registeredCoin.Unspent);

			var coin1 = new SmartCoin(stx1, 0);
			Assert.True(registry.TryAdd(coin1));
			////////////

			var (key2, keyChange) = (new Key(), new Key());
			var stx2 = TransactionBuilders.CreateSpendingTransaction(new[] { coin0.GetCoin(), coin1.GetCoin() }, key2.PubKey.Hash, keyChange.PubKey.Hash);

			registry.Spend(coin0, stx2.GetHash());
			registry.Spend(coin1, stx2.GetHash());
			var spentCoin = Assert.Single(registry.AsCoinsView(), coin => coin == coin0);
			Assert.False(spentCoin.Unspent);
			Assert.Equal(spentCoin, coin0);

			var coin2 = new SmartCoin(stx2, 0);
			var coin3 = new SmartCoin(stx2, 1);
			Assert.True(registry.TryAdd(coin2));
			Assert.True(registry.TryAdd(coin3));
			////////////

			var changes = registry.Undo(stx0.GetHash());
			Assert.Empty(registry);
			var removedCoin = Assert.Single(changes.toRemove);
			Assert.Empty(changes.toAdd);


		}
	}
}
#endif