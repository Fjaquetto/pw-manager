using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.Forms;
using PWManager.Domain.DataContracts;
using PWManager.Infra.Context.Firestore.DataContracts;
using PWManager.Infra.Context.Firestore;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;

namespace PWManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "PWManager");
            Directory.CreateDirectory(appFolder);
            string dbFilePath = Path.Combine(appFolder, "pwmanager.db");
            string firebaseKeyPath = Path.Combine(appFolder, "pw-manager-19ac9-firebase-adminsdk-n1cen-82c8520949.json");

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPWDbContextFactory>(new PWDbContextFactory(dbFilePath))
                .AddScoped<PWDbContext>(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext())
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddSingleton<IFirestoreContext>(new FirestoreContext("pw-manager-19ac9"))
                .AddScoped<IFirestoreRepository, FirestoreRepository>()
                .AddSingleton<IFirebaseService>(new FirebaseService(firebaseKeyPath))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
                context.Database.Migrate();
            }

            var firebaseService = serviceProvider.GetRequiredService<FirebaseService>();
            firebaseService.InitializeFirebase();

            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new PWManagerKey(serviceProvider));
        }
    }
}