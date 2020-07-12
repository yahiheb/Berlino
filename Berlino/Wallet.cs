using System;
using System.Collections.Generic;
using System.Linq;
using Berlino.KeyManagement;
using NBitcoin;

namespace Berlino
{
	public class Wallet
	{
		public HDKeyManager KeyManager { get; }

		public CoinsRegistry Coins { get; }

		public Wallet(HDKeyManager keyManager)
		{
			KeyManager = keyManager;
			Coins = new CoinsRegistry();
		}


		private void ProcessTransaction(SmartTransaction tx)
		{
			if (!tx.Transaction.IsCoinBase && !Coins.CreatedBy(tx.GetHash()).Any()) 
			{
				// Transactions we already have and processed would be "double spends" but they shouldn't.
				var doubleSpends = Coins.SpentBy(tx.Transaction.Inputs);

				if (doubleSpends.Any())
				{
					if (!tx.Confirmed)
					{
						// if the received transaction is spending at least one input already
						// spent by a previous unconfirmed transaction signaling RBF then it is not a double
						// spending transaction but a replacement transaction.
						var isReplacementTx = doubleSpends.Any(x => x.IsReplaceable && !x.Confirmed);
						if (!isReplacementTx)
							return;

						// Undo the replaced transaction by removing the coins it created (if other coin
						// spends it, remove that too and so on) and restoring those that it replaced.
						// After undoing the replaced transaction it will process the replacement transaction.
						var replacedTxId = doubleSpends.First().OutPoint.Hash;
						var (replaced, restored) = Coins.Undo(replacedTxId);
					}
					else // new confirmation always enjoys priority
					{
						// remove double spent coins recursively (if other coin spends it, remove that too and so on), will add later if they came to our keys
						foreach (var doubleSpentCoin in doubleSpends)
						{
							Coins.Remove(doubleSpentCoin);
						}

						var unconfirmedDoubleSpentTxId = doubleSpends.First().OutPoint.Hash;
					}
				}
			}

			foreach (var (output, i) in tx.Outputs.WithIndex())
			{
				// If transaction received to any of the wallet keys:
				if (!KeyManager.TryGetKeyForScriptPubKey(output.ScriptPubKey, out var foundKey))
					continue;

				foundKey!.MarkAsUsed();

				var newCoin = new SmartCoin(tx, i, foundKey); // Do not inherit locked status from key, that's different.

				// If we did not have it.
				if (Coins.TryAdd(newCoin))
				{
					// Make sure there's always 21 clean keys generated and indexed.
					var keyRegistry = foundKey.IsInternal ? KeyManager.InternalKeys : KeyManager.ExternalKeys; 
					keyRegistry.EnsureEnoughKeys();
				}
				else // If we had this coin already.
				{
					if (newCoin.Confirmed) // Update the height of this old coin we already had.
					{
						var oldCoin = Coins.GetByOutPoint(new OutPoint(tx.GetHash(), i));
						oldCoin.Height = newCoin.Height;
					}
				}
			}

			var alreadySpentCoins = Coins
				.SpentBy(tx.Transaction.Inputs)
				.Where(coin => coin.SpenderTransactionId == tx.GetHash());
			foreach (var coin in alreadySpentCoins)
			{
				Coins.Spend(coin, tx.GetHash());
			}
		}

		public void UndoBlock(uint blockHeight)
		{
			Coins.SwitchToUnconfirmFromBlock(blockHeight);
		}
	}
}