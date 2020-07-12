using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NBitcoin;

namespace Berlino.KeyManagement
{
	public class HDPubKey : IEquatable<HDPubKey>
	{
		public PubKey PubKey { get; }

		public KeyPath FullKeyPath { get; }

		public string Label { get; }

		public KeyState KeyState { get; private set; }
		
		public uint Index { get; }

		public bool IsInternal { get; }
		//////////////////////

		public Script P2pkScript => PubKey.ScriptPubKey;
		public Script P2pkhScript => PubKey.Hash.ScriptPubKey;
		public Script P2wpkhScript => PubKey.WitHash.ScriptPubKey;
		public Script P2shOverP2wpkhScript => P2wpkhScript.Hash.ScriptPubKey;
		public KeyId PubKeyHash => PubKey.Hash;


		public HDPubKey(PubKey pubKey, KeyPath fullKeyPath, string label)
		{
			PubKey = pubKey;
			FullKeyPath = fullKeyPath;
			Label = label;
			KeyState = KeyState.Unused;

			Index = FullKeyPath.Indexes[4];

			IsInternal = FullKeyPath.Indexes[3] switch {
				0 => false,
				1 => true,
				_ => throw new ArgumentException(nameof(FullKeyPath))
			};
		}

		public void MarkAsUsed()
		{
			KeyState = KeyState.Used;
		}

		public BitcoinPubKeyAddress GetP2pkhAddress(NBitcoin.Network network)
			=> (BitcoinPubKeyAddress)PubKey.GetAddress(ScriptPubKeyType.Legacy, network);

		public BitcoinWitPubKeyAddress GetP2wpkhAddress(NBitcoin.Network network)
			=> PubKey.GetSegwitAddress(network);

		public BitcoinScriptAddress GetP2shOverP2wpkhAddress(NBitcoin.Network network)
			=> P2wpkhScript.GetScriptAddress(network);

		public bool ContainsScript(Script scriptPubKey)
			=> Scripts().Contains(scriptPubKey);

		private IEnumerable<Script> Scripts()
		{
			yield return P2wpkhScript;
			yield return P2pkhScript;
			yield return P2pkScript;
			yield return P2shOverP2wpkhScript;
		}

		public override bool Equals(object? obj)
			=> obj is HDPubKey pubKey && this == pubKey;

		public bool Equals(HDPubKey? other) =>
			other is { } && this == other;

		public override int GetHashCode() =>
			HashCode.Combine(PubKey, Label, KeyState, Index, IsInternal, FullKeyPath);

		public static bool operator ==(HDPubKey x, HDPubKey y) =>
			x.IsInternal == y.IsInternal
			&& x.FullKeyPath == y.FullKeyPath
			&& x.KeyState == y.KeyState
			&& x.PubKey == y.PubKey
			&& x.Label == y.Label;

		public static bool operator !=(HDPubKey x, HDPubKey y) =>
			!(x == y);
	}
}
