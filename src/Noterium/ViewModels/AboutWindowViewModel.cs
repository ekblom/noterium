using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace Noterium.ViewModels
{
    public class AboutWindowViewModel : NoteriumViewModelBase
    {
        public AboutWindowViewModel()
        {
            #region About

            var assembly = Assembly.GetEntryAssembly();
            Version = assembly.GetName().Version.ToString();
            Title = assembly.GetName().Name;

            var copyright = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var description = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
            var company = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;

            if (copyright != null)
                Copyright = copyright.Copyright;
            if (description != null)
                Description = description.Description;
            if (company != null)
                Publisher = company.Company;

            HyperlinkText = "https://github.com/ekblom/noterium/";
            License = "Open source";


            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = path + "\\LICENSE-3RD-PARTY.txt";
            if (File.Exists(filePath))
                ThirtPartyLicenses = File.ReadAllText(filePath);

            #endregion
        }

        #region Fields

        private string _title;
        private string _description;
        private string _version;
        private ImageSource _publisherLogo;
        private string _copyright;
        private string _additionalNotes;
        private string _hyperlinkText;
        private Uri _hyperlink;
        private string _publisher;
        private bool _isSemanticVersioning;
        private string _license;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the application title.
        /// </summary>
        /// <value>The application title.</value>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string License
        {
            get => _license;
            set
            {
                _license = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the application info.
        /// </summary>
        /// <value>The application info.</value>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets if Semantic Versioning is used.
        /// </summary>
        /// <see cref="http://semver.org/" />
        /// <value>The bool that indicats whether Semantic Versioning is used.</value>
        public bool IsSemanticVersioning
        {
            get => _isSemanticVersioning;
            set
            {
                _isSemanticVersioning = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the application version.
        /// </summary>
        /// <value>The application version.</value>
        public string Version
        {
            get
            {
                // ReSharper disable once RedundantAssignment
                var suffix = string.Empty;
#if DEBUG
				suffix = ".debug";
#endif
                var version = _version;
                if (IsSemanticVersioning)
                {
                    var tmp = _version.Split('.');
                    version = $"{tmp[0]}.{tmp[1]}.{tmp[2]}";
                }

                return version + suffix;
            }
            set
            {
                if (_version != value)
                {
                    _version = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the publisher logo.
        /// </summary>
        /// <value>The publisher logo.</value>
        public ImageSource PublisherLogo
        {
            get => _publisherLogo;
            set
            {
                if (!Equals(_publisherLogo, value))
                {
                    _publisherLogo = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the publisher.
        /// </summary>
        /// <value>The publisher.</value>
        public string Publisher
        {
            get => _publisher;
            set
            {
                if (_publisher != value)
                {
                    _publisher = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the copyright label.
        /// </summary>
        /// <value>The copyright label.</value>
        public string Copyright
        {
            get => _copyright;
            set
            {
                if (_copyright != value)
                {
                    _copyright = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the hyperlink text.
        /// </summary>
        /// <value>The hyperlink text.</value>
        public string HyperlinkText
        {
            get => _hyperlinkText;
            set
            {
                try
                {
                    Hyperlink = new Uri(value);
                    _hyperlinkText = value;
                    RaisePropertyChanged();
                }
                catch
                {
                    // ignored
                }
            }
        }

        public Uri Hyperlink
        {
            get => _hyperlink;
            set
            {
                if (_hyperlink != value)
                {
                    _hyperlink = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the further info.
        /// </summary>
        /// <value>The further info.</value>
        public string AdditionalNotes
        {
            get => _additionalNotes;
            set
            {
                if (_additionalNotes != value)
                {
                    _additionalNotes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public object ThirtPartyLicenses { get; }

        #endregion
    }
}