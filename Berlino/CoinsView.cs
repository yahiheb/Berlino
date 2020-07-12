using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace Berlino
{
	public class CoinsView : ICoinsView
	{
		private IEnumerable<SmartCoin> _coins;

		public CoinsView(IEnumerable<SmartCoin> coins)
		{
			_coins = coins;
		}

		public ICoinsView Spent() => new CoinsView(_coins.Where(x => !x.Unspent));
		
		public ICoinsView Unspent() => new CoinsView(_coins.Where(x => x.Unspent));

		public ICoinsView Confirmed() => new CoinsView(_coins.Where(x => x.Confirmed));

		public ICoinsView Unconfirmed() => new CoinsView(_coins.Where(x => !x.Confirmed));

		public ICoinsView AtBlockHeight(uint height) => new CoinsView(_coins.Where(x => x.Height == height));

		public ICoinsView OutPoints(IEnumerable<OutPoint> outPoints) => OutPoints(outPoints.ToHashSet());

		public ICoinsView OutPoints(HashSet<OutPoint> outPoints) => new CoinsView(_coins.Where(x => outPoints.Contains(x.OutPoint)));

		public SmartCoin GetByOutPoint(OutPoint outpoint) => _coins.FirstOrDefault(x => x.OutPoint == outpoint);

		public Money TotalAmount() => _coins.Sum(x => x.Amount);

		public SmartCoin[] ToArray() => _coins.ToArray();

		public IEnumerator<SmartCoin> GetEnumerator() => _coins.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _coins.GetEnumerator();
	}
}
