using System.Collections.Generic;
using NBitcoin;

namespace Berlino.Tests
{
	public static class TransactionBuilders
	{
		public static SmartTransaction CreateSpendingTransaction(Coin coin, IDestination destination = null, uint height = 0)
		{
			var tx = Network.RegTest.CreateTransaction();
			tx.Inputs.Add(coin.Outpoint, Script.Empty, WitScript.Empty);
			tx.Outputs.Add(coin.Amount, destination.ScriptPubKey ?? Script.Empty);
			return new SmartTransaction(tx, height);
		}

		public static SmartTransaction CreateSpendingTransaction(IEnumerable<Coin> coins, IDestination destination, IDestination destinationChange, bool replaceable = false, uint height = 0)
		{
			var tx = NBitcoin.Network.RegTest.CreateTransaction();
			var amount = Money.Zero;
			foreach (var coin in coins)
			{
				tx.Inputs.Add(coin.Outpoint, Script.Empty, WitScript.Empty, replaceable 
					? Sequence.MAX_BIP125_RBF_SEQUENCE 
					: Sequence.SEQUENCE_FINAL);
				amount += coin.Amount;
			}
			tx.Outputs.Add(amount.Multiply(0.6m), destination.ScriptPubKey ?? Script.Empty);
			tx.Outputs.Add(amount.Multiply(0.4m), destinationChange.ScriptPubKey);
			return new SmartTransaction(tx, height);
		}

		public static SmartTransaction CreateCreditingTransaction(IDestination destination, Money amount, uint height = 0)
		{
			var tx = NBitcoin.Network.RegTest.CreateTransaction();
			tx.Version = 1;
			tx.LockTime = LockTime.Zero;
			tx.Inputs.Add(GetRandomOutPoint(), new Script(OpcodeType.OP_0, OpcodeType.OP_0), sequence: Sequence.Final);
			tx.Inputs.Add(GetRandomOutPoint(), new Script(OpcodeType.OP_0, OpcodeType.OP_0), sequence: Sequence.Final);
			tx.Outputs.Add(amount, destination.ScriptPubKey);
			return new SmartTransaction(tx, height);
		}

		public static OutPoint GetRandomOutPoint()
		{
			return new OutPoint(RandomUtils.GetUInt256(), 0);
		}
	}

	static class  MoneyExtensions
	{
		public static Money Multiply (this Money amount, decimal mul)
		{
			return Money.Satoshis(amount.Satoshi * mul);
		}
	}

}
