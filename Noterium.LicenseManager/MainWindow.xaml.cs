using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Noteray.LicenseManager.Annotations;
using Portable.Licensing;
using License = Portable.Licensing.License;

namespace Noteray.LicenseManager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		private string _publicKey;
		private string _privateKey;

		public MainWindow()
		{
			InitializeComponent();
		}

		public string PublicKey
		{
			get { return _publicKey; }
			set { _publicKey = value; OnPropertyChanged();
			}
		}

		public string PrivateKey
		{
			get { return _privateKey; }
			set { _privateKey = value; OnPropertyChanged();
			}
		}

		private void GenerateKeysButtonClick(object sender, RoutedEventArgs e)
		{
			var keyGenerator = Portable.Licensing.Security.Cryptography.KeyGenerator.Create();
			var keyPair = keyGenerator.GenerateKeyPair();
			PrivateKey = keyPair.ToEncryptedPrivateKeyString(PassPrhaseTextBox.Text);
			PublicKey = keyPair.ToPublicKeyString();

			PrivateKeyTextBox.Text = PrivateKey;
			PublicKeyTextBox.Text = PublicKey;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void LightVersionLicenseClick(object sender, RoutedEventArgs e)
		{
			
		}

		private void SaveKeyFilesButtonClick(object sender, RoutedEventArgs e)
		{
			
		}
	}
}
