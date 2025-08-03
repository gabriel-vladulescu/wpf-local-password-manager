using AccountManager.Models;

namespace AccountManager.Services.Interfaces
{
    public interface IJsonService
    {
        void SaveData(AccountData data);
        AccountData LoadData();
        void SaveDataAsync(AccountData data);
        System.Threading.Tasks.Task<AccountData> LoadDataAsync();
        bool BackupData(AccountData data, string backupPath = null);
        AccountData RestoreData(string backupPath);
    }
}