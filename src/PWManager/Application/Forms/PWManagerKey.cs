using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts;
using PWManager.Domain.Model;
using PWManager.Infra.Helpers;
using PWManager.Infra.Services;

namespace PWManager.Application.Forms
{
    public partial class PWManagerKey : Form
    {
        private IServiceProvider _serviceProvider;
        private IFirestoreRepository<User> _firestoreRepository;
        private IRepository<User> _repository;

        public PWManagerKey(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _firestoreRepository = _serviceProvider.GetRequiredService<IFirestoreRepository<User>>();
            _repository = _serviceProvider.GetRequiredService<IRepository<User>>();

            DataUpdate();
            InitializeComponent();
        }

        private void DataUpdate()
        {
            var obj = _firestoreRepository.GetAsync("67b4bba7-f0ef-46af-8896-df9571f6209a", ObjectExtensions.DictionaryToEntity<User>).Result;

            _firestoreRepository.DeleteAllAsync().Wait();

            var users = _repository.GetAllAsync().Result;
            foreach (var user in users)
            {
                _firestoreRepository.AddAsync(user, ObjectExtensions.EntityToDictionary).Wait();
            }

            var userFs = _firestoreRepository.GetAllAsync(ObjectExtensions.DictionaryToEntity<User>).Result;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            EncryptorService.EncryptorPassword = txtEncryptor.Text;

            PWManager manager = new PWManager(_serviceProvider);
            manager.Show();

            manager.Closed += (_, args) => this.Close();

            this.Hide();
        }
    }
}
