﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace S22.Imap.Auth.Sasl.Mechanisms {
	/// <summary>
	/// Implements the Sasl Cram-Md5 authentication method as described in
	/// RFC 2195.
	/// </summary>
	internal class SaslCramMd5 : SaslMechanism {
		bool Completed = false;

		/// <summary>
		/// True if the authentication exchange between client and server
		/// has been completed.
		/// </summary>
		public override bool IsCompleted => Completed;

	    /// <summary>
		/// The IANA name for the Cram-Md5 authentication mechanism as described
		/// in RFC 2195.
		/// </summary>
		public override string Name => "CRAM-MD5";

	    /// <summary>
		/// The username to authenticate with.
		/// </summary>
		string Username {
			get => Properties.ContainsKey("Username") ?
			    Properties["Username"] as string : null;
	        set => Properties["Username"] = value;
	    }

		/// <summary>
		/// The password to authenticate with.
		/// </summary>
		string Password {
			get => Properties.ContainsKey("Password") ?
			    Properties["Password"] as string : null;
		    set => Properties["Password"] = value;
		}

		/// <summary>
		/// Private constructor for use with Sasl.SaslFactory.
		/// </summary>
		private SaslCramMd5() {
			// Nothing to do here.
		}

		/// <summary>
		/// Creates and initializes a new instance of the SaslCramMd5 class
		/// using the specified username and password.
		/// </summary>
		/// <param name="username">The username to authenticate with.</param>
		/// <param name="password">The plaintext password to authenticate
		/// with.</param>
		/// <exception cref="ArgumentNullException">Thrown if the username
		/// or the password parameter is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the username
		/// parameter is empty.</exception>
		public SaslCramMd5(string username, string password) {
			username.ThrowIfNull("username");
			if (username == string.Empty)
				throw new ArgumentException("The username must not be empty.");
			password.ThrowIfNull("password");

			Username = username;
			Password = password;
		}

		/// <summary>
		/// Computes the client response to the specified Cram-Md5 challenge.
		/// </summary>
		/// <param name="challenge">The challenge sent by the server</param>
		/// <returns>The response to the Cram-Md5 challenge.</returns>
		/// <exception cref="SaslException">Thrown if the response could not
		/// be computed.</exception>
		protected override byte[] ComputeResponse(byte[] challenge) {
			// Precondition: Ensure username and password are not null and
			// username is not empty.
			if (string.IsNullOrEmpty(Username) || Password == null) {
				throw new SaslException("The username must not be null or empty and " +
					"the password must not be null.");
			}
			// Sasl Cram-Md5 does not involve another roundtrip.
			Completed = true;
			// Compute the encrypted challenge as a hex-string.
			string hex = string.Empty;
			using (var hmac = new HMACMD5(Encoding.ASCII.GetBytes(Password))) {
				byte[] encrypted = hmac.ComputeHash(challenge);
				hex = BitConverter.ToString(encrypted).ToLower().Replace("-",
					string.Empty);
			}
			return Encoding.ASCII.GetBytes(Username + " " + hex);	
		}
	}
}
