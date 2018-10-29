using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using Noterium.Core.Annotations;
using Noterium.Core.DataCarriers;
using Noterium.Core.DropBox;
using Noterium.Core.Utilities;

namespace Noterium.Core.Security
{
    public class EncryptionManager : INotifyPropertyChanged
    {
        public delegate SecureString NeedPasswordEventHandler();

        private readonly DataStore _dataStore;
        private bool _alwaysRequirePassword;
        private bool _isAuthenticated;
        private SecureString _password;
        private bool _secureNotesEnabled;

        public EncryptionManager(DataStore dataStore)
        {
            _dataStore = dataStore;
            SecureNotesEnabled = File.Exists(_dataStore.MasterPasswordFile);
        }

        public bool SecureNotesEnabled
        {
            get => _secureNotesEnabled;
            private set
            {
                _secureNotesEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set
            {
                _isAuthenticated = value;
                OnPropertyChanged();
            }
        }

        public bool AlwaysRequirePassword
        {
            get => _alwaysRequirePassword;
            private set
            {
                _alwaysRequirePassword = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event NeedPasswordEventHandler OnPasswordNeeded;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Authenticate(bool saveAuthentication, SecureString password = null)
        {
            if (password == null)
                password = GetPassword(false);

            if (!ValidatePassword(password))
                return false;

            _password = password;

            if (saveAuthentication)
                AlwaysRequirePassword = false;

            IsAuthenticated = true;

            return true;
        }

        public bool ValidatePassword(SecureString password)
        {
            var content = File.ReadAllText(_dataStore.MasterPasswordFile, Encoding.UTF8);

            content = FileSecurity.Decrypt(content, password);
            if (content == null)
                return false;

            var hash = FileSecurity.GetHashString(FileSecurity.ConvertToUnSecureString(password));

            if (!content.Equals(hash))
                return false;
            return true;
        }

        /*
		 * Applikationen startar
		 * SecureNotesEnabled
		 * Authenticate
		 * Save password?
		 *	- Yes
		 *		IsAuthenticated = true
		 *		AlwaysRequirePassword = false
		 *		Password stored for read and write
		 *  - No
		 *		IsAuthenticated = true
		 *		AlwaysRequirePassword = true
		 *		Password needed to open
		 *		Password stored for writing (Until secure note unreloaded?)
		 *
		 *
		 * Open note
		 * IsSecure
		 * Decrypt -> AlwaysRequirePassword?
		 *	Yes
		 *		OnPasswordNeeded
		 *	No
		 *		Has password
		 *
		 * Decrypt Text
		 */

        public void DisableSecureNotes(SecureString pass)
        {
            if (!SecureNotesEnabled || !ValidatePassword(pass))
                return;

            foreach (var n in Hub.Instance.Storage.GetAllNotes())
                if (n.Encrypted)
                {
                    n.Encrypted = false;
                    n.Save();
                }

            File.Delete(_dataStore.MasterPasswordFile);
            SecureNotesEnabled = false;
            _password = null;
        }

        public void EnableSecureNotes(SecureString pass)
        {
            if (SecureNotesEnabled)
                return;

            var content = FileSecurity.GetHashString(FileSecurity.ConvertToUnSecureString(pass));
            var encryptedPass = Encrypt(content, pass);

            File.WriteAllText(_dataStore.MasterPasswordFile, encryptedPass, Encoding.UTF8);

            IsAuthenticated = true;
            SecureNotesEnabled = true;
            StorePassword(pass);

            //foreach (Note n in GetAllNotes())
            //	n.Save();
        }

        public SecureString GetPassword(bool requireAutnentication = true)
        {
            if (requireAutnentication && !IsAuthenticated)
                throw new UserNotAuthenticatedException("User is not authenticated.");

            if (!AlwaysRequirePassword && _password != null)
                return _password;

            if (OnPasswordNeeded != null)
            {
                var pass = OnPasswordNeeded();
                if (ValidatePassword(pass))
                    return pass;
            }

            throw new BadPasswordException();
        }

        public void StorePassword(SecureString pass)
        {
            _password = pass;
        }

        public string Decrypt(string text, SecureString pass = null)
        {
            var password = pass ?? GetPassword();
            return FileSecurity.Decrypt(text, password);
        }

        public string Encrypt(string text, SecureString pass = null)
        {
            var password = pass ?? GetPassword();
            return FileSecurity.Encrypt(text, password);
        }

        public void SwitchTextEncryptionMode(Note note)
        {
            if (note.Encrypted)
            {
                if (!string.IsNullOrWhiteSpace(note.Text))
                    note.Text = Encrypt(note.Text);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(note.Text))
                    note.Text = Decrypt(note.Text);
            }
        }
    }

    public class UserNotAuthenticatedException : Exception
    {
        public UserNotAuthenticatedException(string userIsNotAuthenticated) : base(userIsNotAuthenticated)
        {
        }
    }

    public class BadPasswordException : Exception
    {
    }
}