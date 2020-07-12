using System;
using System.Runtime.Serialization;

namespace Berlino.KeyManagement
{
	[Serializable]
	internal class HDPubKeyNotFoundException : Exception
	{
		public HDPubKeyNotFoundException()
		{
		}

		public HDPubKeyNotFoundException(string message) : base(message)
		{
		}

		public HDPubKeyNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HDPubKeyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
