using System;
using System.Collections.Generic;
using Base36Encoder;
using Portable.Licensing;

namespace Noteray.LicenseManager
{
	public class LicenseGenerator
	{
		private static readonly LicenseGenerator _instance = new LicenseGenerator();

// Explicit static constructor to tell C# compiler
// not to mark type as beforefieldinit
		static LicenseGenerator()
		{
		}

		private LicenseGenerator()
		{
		}

		public static LicenseGenerator Instance
		{
			get { return _instance; }
		}

		public string GetLightVersionLicense(string privateKey, string passPhrase)
		{
			var license = License.New()
				.WithUniqueIdentifier(Guid.NewGuid())
				.As(LicenseType.Trial)
				//.ExpiresAt(DateTime.Now.AddDays(30))
				//.WithMaximumUtilization(5)
				.WithProductFeatures(new Dictionary<string, string>  
											  {  
												  {"Secure Notes", "no"},
												  {"Max Notes", "20"}
											  })
				.LicensedTo("John Doe", "john.doe@example.com")
				.CreateAndSignWithPrivateKey(privateKey, passPhrase);

			return license.ToString();
		}

	}
}