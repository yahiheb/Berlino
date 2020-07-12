using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace Berlino
{
	public class CoinsRegistry : ICoinsView
	{
		private static List<SmartCoin> EmptyListOfCoins = new List<SmartCoin>(0);

		private HashSet<SmartCoin> _coins;
		private Dictionary<uint256, List<SmartCoin>> _coinsCreatedByTransaction;
		private Dictionary<uint256, List<SmartCoin>> _coinsDestroyedByTransaction;


		public CoinsRegistry()
		{
			_coins = new HashSet<SmartCoin>();
			_coinsCreatedByTransaction = new Dictionary<uint256, List<SmartCoin>>();
			_coinsDestroyedByTransaction = new Dictionary<uint256, List<SmartCoin>>();
		}

		public bool IsEmpty => !_coins.Any();

		private CoinsView AsSpentCoinsView() => new CoinsView(_coins.Where(x => !x.Unspent));

		public SmartCoin GetByOutPoint(OutPoint outpoint) => AsCoinsView().GetByOutPoint(outpoint);

		public ICoinsView AsCoinsView() => new CoinsView(_coins);

		public ICoinsView AtBlockHeight(uint height) => AsCoinsView().AtBlockHeight(height);

		public ICoinsView Confirmed() => AsCoinsView().Confirmed();

		public ICoinsView ChildrenOf(SmartCoin coin) => new CoinsView(_coins.Where(x => x.OutPoint.Hash == coin.SpenderTransactionId));

		public ICoinsView DescendantOf(SmartCoin coin)
		{
			IEnumerable<SmartCoin> Generator(SmartCoin scoin)
			{
				foreach (var child in ChildrenOf(scoin))
				{
					foreach (var childDescendant in Generator(child))
					{
						yield return childDescendant;
					}

					yield return child;
				}
			}

			return new CoinsView(Generator(coin));
		}

		public ICoinsView DescendantOfAndSelf(SmartCoin coin) => new CoinsView(DescendantOf(coin).Append(coin) );

		public IEnumerator<SmartCoin> GetEnumerator() => AsCoinsView().GetEnumerator();

		public ICoinsView OutPoints(IEnumerable<OutPoint> outPoints) => AsCoinsView().OutPoints(outPoints);

		public ICoinsView CreatedBy(uint256 txid) => new CoinsView(_coinsCreatedByTransaction.TryGet(txid) ?? EmptyListOfCoins );

		public ICoinsView SpentBy(uint256 txid) => new CoinsView(_coinsDestroyedByTransaction.TryGet(txid) ?? EmptyListOfCoins );

		public ICoinsView SpentBy(TxInList txIns) => OutPoints(txIns.Select(x => x.PrevOut));

		public SmartCoin[] ToArray() => AsCoinsView().ToArray();

		public Money TotalAmount() => AsCoinsView().TotalAmount();

		public ICoinsView Unconfirmed() => AsCoinsView().Unconfirmed();

		public ICoinsView Unspent() => AsCoinsView().Unspent();

		public ICoinsView Spent() => AsCoinsView().Spent();

		IEnumerator IEnumerable.GetEnumerator() => AsCoinsView().GetEnumerator();

		public bool TryAdd(SmartCoin coin)
		{
			if (!_coins.Add(coin))
			{
				return false;
			}

			if (_coinsCreatedByTransaction.TryGetValue(coin.OutPoint.Hash, out var coinList))
			{
				coinList.Add(coin);
			}
			else
			{
				_coinsCreatedByTransaction[coin.OutPoint.Hash] = new List<SmartCoin>{ coin };
			}
			return true;
		}

		public void Spend(SmartCoin spentCoin, uint256 spenderTx)
		{
			if (_coins.Remove(spentCoin))
			{
				
				_coins.Add(spentCoin.AsSpentBy(spenderTx));
				if (_coinsDestroyedByTransaction.TryGetValue(spenderTx, out var coinList))
				{
					coinList.Add(spentCoin);
				}
				else
				{
					_coinsDestroyedByTransaction[spenderTx] = new List<SmartCoin>{ spentCoin };
				}
			}
		}

		public void SwitchToUnconfirmFromBlock(uint blockHeight)
		{
			foreach (var coin in AsCoinsView().AtBlockHeight(blockHeight))
			{
				var descendantCoins = DescendantOfAndSelf(coin);
				foreach (var toSwitch in descendantCoins)
				{
					toSwitch.Height = uint.MaxValue;
				}
			}
		}

		public (ICoinsView toRemove, ICoinsView toAdd) Undo(uint256 txId)
		{
			var toRemove = new List<SmartCoin>();
			var toAdd = new List<SmartCoin>();

			// remove recursively the coins created by the transaction
			foreach (var createdCoin in CreatedBy(txId).ToList())
			{
				toRemove.AddRange(Remove(createdCoin));
			}

			// destroyed (spent) coins are now (unspent)
			toAdd.AddRange(SpentBy(txId));

			_coinsCreatedByTransaction.Remove(txId);
			_coinsDestroyedByTransaction.Remove(txId);

			return (new CoinsView(toRemove), new CoinsView(toAdd));
		}

		public ICoinsView Remove(SmartCoin coin)
		{
			var coinsToRemove = DescendantOfAndSelf(coin);
			foreach (var toRemove in coinsToRemove)
			{
				_coins.Remove(toRemove);

				_coinsCreatedByTransaction[toRemove.OutPoint.Hash].Remove(toRemove);
				if(!toRemove.Unspent)
				{
					_coinsDestroyedByTransaction[toRemove.SpenderTransactionId].Remove(toRemove);
				}
			}
			return coinsToRemove;
		}
	}
}
