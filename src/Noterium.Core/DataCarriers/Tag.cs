using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media;
using Newtonsoft.Json;
using Noterium.Core.Annotations;

namespace Noterium.Core.DataCarriers
{
    public class Tag : INotifyPropertyChanged, IComparable, IComparable<Tag>, IEquatable<Tag>
    {
        private Color _color;
        private int _instances;
        private string _name;

        public Tag()
        {
            _color = Color.FromArgb(204, 17, 158, 218);
        }

        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name == null)
                    _name = value.ToLower();
                else
                    throw new NotSupportedException("Cant change name of a tag.");
            }
        }

        [DataMember]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("instances", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        [DataMember]
        public int Instances
        {
            get => _instances;
            set
            {
                _instances = value;
                OnPropertyChanged();
            }
        }

        public int CompareTo(object obj)
        {
            return string.Compare(Name, ((Tag) obj).Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public int CompareTo(Tag other)
        {
            return string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool Equals(Tag other)
        {
            return Name.Equals(other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}