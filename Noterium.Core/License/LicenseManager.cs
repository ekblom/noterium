using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Noterium.Core.Helpers;
using Portable.Licensing.Prime;
using Portable.Licensing.Prime.Validation;

namespace Noterium.Core.License
{
    public class LicenseManager
    {
        private readonly Storage _storage;
#if DEBUG
        private readonly string publicKeyV1 = "MIIBKjCB4wYHKoZIzj0CATCB1wIBATAsBgcqhkjOPQEBAiEA/////wAAAAEAAAAAAAAAAAAAAAD///////////////8wWwQg/////wAAAAEAAAAAAAAAAAAAAAD///////////////wEIFrGNdiqOpPns+u9VXaYhrxlHQawzFOw9jvOPD4n0mBLAxUAxJ02CIbnBJNqZnjhE50mt4GffpAEIQNrF9Hy4SxCR/i85uVjpEDydwN9gS3rM6D0oTlF2JjClgIhAP////8AAAAA//////////+85vqtpxeehPO5ysL8YyVRAgEBA0IABLeu5+Zff1Dw1oiVaSDlWkrN0Tz5hp22ZNdeuZ0QcYdCJd65aBrXlLVe+bBCYpf/dcHm3zmkCJPnDzETj8mCkFU=";
#else
        private string publicKeyV1 = "MIICITAjBgoqhkiG9w0BDAEDMBUEEIuRLU3SDIJ6IhslQfRx1BgCAQoEggH4VMrtjmBZ8QaXHcopgX8DMuAHo6OBNyJ+3CbQEh6aO+D/E328THVxV6RRBb620XuSk3h5Zd4IeARQdssVwnDQG4UL2U5bAgrHSvp3UBTb+Y8/fHTvrkGSWJ0ZejR2sCr78C4njF7K57pEWRdt/kiREYVa7wxobVWmByfrX7n28rOhRgj/x/HiaGJkZBHwtHZ+QrVsrK+tDNe6eokaFBZIl74kb1COBMih/a4mdg4OCIKMG2ioCNodk/ut7qVXPc7G+/Rjcjs00PHQlwyiGbhli3ajKVA5pshMG2IEEDNYnBQlc592RYnv8jScPpGFk6fLkNSYflDsT+Rij1m1WZjEnRrpUMjntPHQvJdJwqTyjE/lgMuZvpdA5/XWFIEQJKYaaZyBQ/SlBM0WF/Tk0JNiftYP5fN+O0wAipf3egu4+10n36BRf9zDRvIo5xroXBFTNnloEyuZEvLEiyNErQid/4CKJu0bXVePtWnnc5PIIjMKr/mLeIRy6wlNh8sOACftQSl9fnN2Z+5bEdafSTf4vcSapLZ4k2XkiwtnxcC5bDK+C8+PEzpXuz8AMzWbjgEebsBI3XHlHIKHaqvAXqDIFyhMywmT0fFAY6lqlCu/lAdPZC4LX0S4Qs0C/0SNMg55h9+NN5FcDfZK0VSq31dVMAaPru8KIIKO";
#endif

        public bool ValidLicense { get; private set; }

        public Portable.Licensing.Prime.License License { get; private set; }

        public LicenseManager(Storage storage)
        {
            _storage = storage;
            ValidLicense = false;
        }

        public void LoadLicense()
        {
            var di = new DirectoryInfo(_storage.DataStore.RootFolder);
            var files = di.GetFiles("*.lic");
            if (files.Length > 0)
            {
                var fi = files[0];
                if (fi.Length == 0)
                    return;

                var stream = fi.OpenRead();
                License = Portable.Licensing.Prime.License.Load(stream);
                stream.Close();

                var validationFailures = License.Validate()
                    .ExpirationDate()
                    .When(lic => lic.Type == LicenseType.Standard)
                    .And()
                    .Signature(publicKeyV1)
                    .AssertValidLicense().ToList();

                ValidLicense = !(validationFailures.Any());
            }
        }

        public void InitTrailLicense()
        {
            var licenseXml = GetTrailLicense();
            licenseXml.ContinueWith(SetTrailLicense);
        }

        private void SetTrailLicense(Task<string> obj)
        {
            if (obj != null && obj.Exception == null)
            {
                File.WriteAllText(_storage.DataStore.RootFolder + "\\license.lic", obj.Result);
                LoadLicense();
            }
        }

        private string GetMachineKey()
        {
            var os = new ManagementObject("Win32_OperatingSystem=@");
            var serial = (string) os["SerialNumber"];

            var userName = Environment.UserName;

            return (serial + userName).GetMD5Hash();
        }

        private async Task<string> GetTrailLicense()
        {
            var machineKey = GetMachineKey();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://dev.noterium.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                // New code:
                var response = await client.GetAsync("api/license/GetTrailLicense/" + machineKey);
                if (response.IsSuccessStatusCode)
                {
                    var product = await response.Content.ReadAsStringAsync();
                    return product;
                }
            }

            return null;
        }
    }
}