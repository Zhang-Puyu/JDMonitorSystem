using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TreeNodeViewModel> _childNodes
            = new ObservableCollection<TreeNodeViewModel>();
        private string _name = string.Empty;
        private object? _tag = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TreeNodeViewModel(in string name)
        {
            Name = name;
            ChildNodes = new ObservableCollection<TreeNodeViewModel>();
            Tag = null;
        }
        public TreeNodeViewModel(in string name, in object? tag)
        {
            Name = name;
            ChildNodes = new ObservableCollection<TreeNodeViewModel>();
            Tag = tag;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public object? Tag
        {
            get { return _tag; }
            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public ObservableCollection<TreeNodeViewModel> ChildNodes
        {
            get { return _childNodes; }
            set
            {
                if (_childNodes != value)
                {
                    _childNodes = value;
                    OnPropertyChanged(nameof(ChildNodes));
                }
            }
        }

        public void Add(in TreeNodeViewModel childNode)
        {
            ChildNodes.Add(childNode);
        }

        public void Add(in IEnumerable<TreeNodeViewModel> childNodes)
        {
            foreach (var node in childNodes)
            {
                ChildNodes.Add(node);
            }
        }

        public void Add(in string chilNodeName)
        {
            ChildNodes.Add(new TreeNodeViewModel(chilNodeName));
        }

        public void Add(in IEnumerable<string> chilNodeNames)
        {
            foreach (var name in chilNodeNames)
            {
                ChildNodes.Add(new TreeNodeViewModel(name));
            }
        }

        public void Clear()
        {
            _childNodes?.Clear();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
