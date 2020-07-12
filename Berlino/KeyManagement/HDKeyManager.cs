using NBitcoin;

namespace Berlino.KeyManagement
{
	public class HDKeyManager
	{
		public const uint AbsoluteMinGapLimit = 21;
		private static KeyPath DefaultAccountKeyPath = KeyPath.Parse("m/84'/0'/0'");

		public ExtKey? ExtKey { get; }
		public ExtPubKey ExtPubKey { get; }

		public bool IsWatchOnly => ExtKey == null;
		public uint MinGapLimit { get; }
		public KeyPath AccountKeyPath { get; }
		public KeysRegistry ExternalKeys { get; }
		public KeysRegistry InternalKeys { get; }

		internal HDKeyManager(ExtKey extKey, KeyPath accountKeyPath, uint minGapLimit = AbsoluteMinGapLimit)
		{
			AccountKeyPath = accountKeyPath;
			ExtKey = extKey;
			ExtPubKey = extKey.Derive(AccountKeyPath).Neuter();
			MinGapLimit = minGapLimit;
			ExternalKeys = new KeysRegistry(ExtPubKey.Derive(0), AccountKeyPath.Derive(0), MinGapLimit);
			InternalKeys = new KeysRegistry(ExtPubKey.Derive(1), AccountKeyPath.Derive(1), MinGapLimit);
		}

		internal HDKeyManager(ExtPubKey extPubKey, KeyPath accountKeyPath, uint minGapLimit = AbsoluteMinGapLimit)
		{
			AccountKeyPath = accountKeyPath ?? DefaultAccountKeyPath;
			MinGapLimit = minGapLimit;
			ExtPubKey = extPubKey;
			ExternalKeys = new KeysRegistry(extPubKey.Derive(0), AccountKeyPath.Derive(0), MinGapLimit);
			InternalKeys = new KeysRegistry(extPubKey.Derive(1), AccountKeyPath.Derive(1), MinGapLimit);
		}

		public static HDKeyManager Create(out Mnemonic mnemonic)
		{
			mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
			var extKey = mnemonic.DeriveExtKey();
			return new HDKeyManager(extKey, DefaultAccountKeyPath);
		}

		public static HDKeyManager Recover(Mnemonic mnemonic)
		{
			var extKey = mnemonic.DeriveExtKey();
			return new HDKeyManager(extKey, DefaultAccountKeyPath);
		}

		public bool TryGetKeyForScriptPubKey(Script scriptPubKey, out HDPubKey? key)
		{
			return ExternalKeys.TryGetKeyForScriptPubKey(scriptPubKey, out key) 
				|| InternalKeys.TryGetKeyForScriptPubKey(scriptPubKey, out key);
		}
	}
}
