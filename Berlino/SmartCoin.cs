using Berlino.KeyManagement;
using NBitcoin;
using System;
using System.Linq;

namespace Berlino
{
	public class SmartCoin : IEquatable<SmartCoin>
	{
		public OutPoint OutPoint { get; }

		public TxOut TxOut { get; }

		public Money Amount => TxOut.Value;

		public uint Height { get; set; }
	
		public OutPoint[] SpentOutputs { get; }

		public bool IsReplaceable { get; }

		public uint256 SpenderTransactionId { get; }

		public bool IsLikelyCoinJoinOutput { get; }

		public uint AnonymitySet { get; }

		public HDPubKey HDPubKey { get; }

		public bool Confirmed => Height < uint.MaxValue; 

		public bool Unspent => SpenderTransactionId == uint256.Zero;

		public SmartCoin(SmartTransaction stx, uint index, HDPubKey pubKey)
			: this( new OutPoint(stx.Transaction, index), stx.Outputs[index], stx.Inputs.Select(i=>i.PrevOut).ToArray(), stx.Height, stx.IsRBF, uint256.Zero, pubKey)
		{
		}

		public SmartCoin(OutPoint outPoint, TxOut txOut, OutPoint[] spentOutputs, uint height, bool replaceable, uint256 spenderTransactionId, HDPubKey pubKey)
		{
			OutPoint = outPoint;
			TxOut = txOut;

			Height = height;
			SpentOutputs = spentOutputs;
			IsReplaceable = replaceable;
			IsLikelyCoinJoinOutput = false;
			SpenderTransactionId = spenderTransactionId;

			AnonymitySet = 1;
			HDPubKey = pubKey;
		}

		public SmartCoin AsSpentBy(uint256 spenderTx)
			=> new SmartCoin(OutPoint, TxOut, SpentOutputs, Height, IsReplaceable, spenderTx, HDPubKey);

		public Coin GetCoin() => new Coin(OutPoint, TxOut);

		public override bool Equals(object? obj) => Equals(obj as SmartCoin);

		public bool Equals(SmartCoin? other) => other is { } && this == other;

		public override int GetHashCode() => OutPoint.GetHashCode(); // HashCode.Combine(OutPoint, SpenderTransactionId, Height);

		public static bool operator == (SmartCoin x, SmartCoin y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}
			else if (x is null || y is null)
			{
				return false;
			}
			else
			{
				return x.OutPoint == y.OutPoint;
					// && x.SpenderTransactionId == y.SpenderTransactionId
					// && x.Height == y.Height;
			}
		}

		public static bool operator != (SmartCoin x, SmartCoin y) => !(x == y);
	}
}
