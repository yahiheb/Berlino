using System;
using System.Linq;
using System.Collections.Generic;
using NBitcoin;
using System.Collections;

namespace Berlino.KeyManagement
{
	public class KeysRegistry : IEnumerable<HDPubKey>
	{
		public uint MinGapLimit { get; }
		public ExtPubKey ExtPubKey { get; }
		public KeyPath AccountKeyPath { get; }
		private List<HDPubKey> _hdPubKeys = new List<HDPubKey>();
		private List<byte[]> _hdPubKeyScriptBytes = new List<byte[]>();
		private Dictionary<Script, HDPubKey> _scriptHdPubKeyMap = new Dictionary<Script, HDPubKey>();

		public KeysRegistry(ExtPubKey extPubKey, KeyPath accountKeyPath, uint minGapLimit)
		{
			ExtPubKey = extPubKey;
			AccountKeyPath = accountKeyPath;
			MinGapLimit = minGapLimit;
		}

		public IEnumerable<HDPubKey> Used =>
			_hdPubKeys.Where(x => x.KeyState == KeyState.Used);

		public IEnumerable<HDPubKey> Unused =>
			_hdPubKeys.Where(x => x.KeyState == KeyState.Unused);

		public HDPubKey GenerateNewKey(string knownBy) =>
			CacheHdPubKey(DerivePubKeyFromIndex(NextAvailableIndex(), knownBy));


		public void EnsureEnoughKeys()
		{
			while (CountConsecutiveUnusedKeys() < MinGapLimit)
			{
				GenerateNewKey(string.Empty);
			}
		}
		
		public uint CountConsecutiveUnusedKeys()
		{
			var keyIndexes = Unused.Select(x => x.Index).ToArray();

			var hs = keyIndexes.ToHashSet();
			var largerConsecutiveSequence = 0u;

			for (var i = 0; i < keyIndexes.Length; ++i)
			{
				if (!hs.Contains(keyIndexes[i] - 1))
				{
					var j = keyIndexes[i];
					while (hs.Contains(j))
					{
						j++;
					}

					var sequenceLength = j - keyIndexes[i];
					if (largerConsecutiveSequence < sequenceLength)
					{
						largerConsecutiveSequence = sequenceLength;
					}
				}
			}
			return largerConsecutiveSequence;
		}

		public bool TryGetKeyForScriptPubKey(Script scriptPubKey, out HDPubKey? key)
			=> _scriptHdPubKeyMap.TryGetValue(scriptPubKey, out key);

		public IEnumerator<HDPubKey> GetEnumerator()
			=> _hdPubKeys.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		 => GetEnumerator();

		private HDPubKey CacheHdPubKey(HDPubKey hdPubKey)
		{
			_hdPubKeys.Add(hdPubKey);
			_hdPubKeyScriptBytes.Add(hdPubKey.P2wpkhScript.ToCompressedBytes());
			_scriptHdPubKeyMap.Add(hdPubKey.P2wpkhScript, hdPubKey);
			return hdPubKey;
		}

		private HDPubKey DerivePubKeyFromIndex(int index, string knownBy)
		{
			var path = new KeyPath((uint)index);
			var fullPath = AccountKeyPath.Derive((uint)index);
			var pubKey = ExtPubKey.Derive(path).PubKey;
			var hdPubKey = new HDPubKey(pubKey, fullPath, knownBy);
			return hdPubKey;
		}

		private int NextAvailableIndex()
		{
			// BIP44-ish derivation scheme
			// m / purpose' / coin_type' / account' / change / address_index
			var index = 0;

			var indexes = _hdPubKeys.Select(x => (int)x.Index).ToArray();
			if (indexes.Any())
			{
				var largestIndex = indexes.Max();
				var smallestMissingIndex = largestIndex;
				Span<bool> present = stackalloc bool[largestIndex + 1];
				for (int i = 0; i < indexes.Length; ++i)
				{
					present[indexes[i]] = true;
				}
				for (int i = 1; i < present.Length; ++i)
				{
					if (!present[i])
					{
						smallestMissingIndex = i - 1;
						break;
					}
				}

				index = indexes[smallestMissingIndex] + 1;
			}

			return index;
		}
	}
}
