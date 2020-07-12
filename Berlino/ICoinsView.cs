using System;
using System.Collections.Generic;
using NBitcoin;

namespace Berlino
{
	public interface ICoinsView : IEnumerable<SmartCoin>
	{
		ICoinsView AtBlockHeight(uint height);

		ICoinsView Confirmed();

		ICoinsView OutPoints(IEnumerable<OutPoint> outPoints);

		SmartCoin[] ToArray();

		Money TotalAmount();

		ICoinsView Unconfirmed();

		ICoinsView Spent();

		ICoinsView Unspent();

		SmartCoin GetByOutPoint(OutPoint outpoint);
	}
}
