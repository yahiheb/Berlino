using System;
using NBitcoin;

namespace Berlino
{
	public class SmartTransaction : IEquatable<SmartTransaction>
	{
		public Transaction Transaction { get; }

		public uint Height { get; }

		public uint256? BlockHash { get; }

		public int BlockIndex { get; }

		public DateTimeOffset FirstSeen { get; }

		public bool IsReplacement { get; }

		public bool Confirmed => Height < uint.MaxValue;

		public uint256 GetHash() => Transaction.GetHash();

		public uint GetConfirmationCount(uint bestHeight) => !Confirmed ? 0 : bestHeight - Height + 1;

		public bool IsRBF => !Confirmed && (Transaction.RBF || IsReplacement);

		public TxInList Inputs => Transaction.Inputs;

		public TxOutList Outputs => Transaction.Outputs;
		
		public SmartTransaction(Transaction transaction, uint height, uint256? blockHash = null, int blockIndex = 0, bool isReplacement = false, DateTimeOffset firstSeen = default)
		{
			Transaction = transaction;
			Transaction.PrecomputeHash(false, true);
			Height = height;
			BlockHash = blockHash;
			BlockIndex = blockIndex;
			FirstSeen = firstSeen == default ? DateTimeOffset.UtcNow : firstSeen;
			IsReplacement = isReplacement;
		}

		public override bool Equals(object? obj) => Equals(obj as SmartTransaction);

		public bool Equals(SmartTransaction? other) => other is { } && this == other;

		public override int GetHashCode() => GetHash().GetHashCode();

		public static bool operator ==(SmartTransaction x, SmartTransaction y) => y?.GetHash() == x?.GetHash();

		public static bool operator !=(SmartTransaction x, SmartTransaction y) => !(x == y);
	}
}