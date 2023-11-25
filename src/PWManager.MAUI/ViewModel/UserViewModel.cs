using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PWManager.MAUI.ViewModel
{
    internal class UserViewModel : INotifyPropertyChanged
    {
        private readonly IRepository<User> _repository;

        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                if (_users != value)
                {
                    _users = value;
                    OnPropertyChanged(nameof(Users));
                }
            }
        }

        public UserViewModel(IRepository<User> repository)
        {
            _repository = repository;

            Users = new ObservableCollection<User>(_repository.GetAllAsync().Result);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
