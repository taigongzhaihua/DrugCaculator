using DrugCalculator.DataAccess;
using NLog;
using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace DrugCalculator.Services;

public static class EncryptionService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;
    private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string DbDirectory = Path.Combine(AppDataPath, AppName);
    private static readonly string DbPath = Path.Combine(DbDirectory, "EncryptionDatabase.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";
    private static readonly DbManager DbManager = new(ConnectionString);

    static EncryptionService()
    {
        try
        {
            Logger.Info("初始化 EncryptionService");
            EnsureDatabase();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化 EncryptionService 时发生错误");
            throw new InvalidOperationException("初始化 EncryptionService 时发生错误: " + ex.Message, ex);
        }
    }

    // 确保数据库和目录存在
    private static void EnsureDatabase()
    {
        try
        {
            if (!Directory.Exists(DbDirectory))
            {
                Logger.Info($"{DbDirectory} 不存在，正在创建");
                Directory.CreateDirectory(DbDirectory);
                Logger.Info("目录创建成功");
            }

            if (!HasWritePermission(DbDirectory)) throw new UnauthorizedAccessException($"没有对目录 {DbDirectory} 的写入权限。");

            if (!File.Exists(DbPath))
            {
                Logger.Info($"{DbPath} 不存在，正在创建");
                SQLiteConnection.CreateFile(DbPath);
                Logger.Info("数据库文件创建成功");
            }

            Logger.Debug($"ConnectionString = '{ConnectionString}'");
            DbManager.CreateTableIfNotExists("KeyTable",
                ("KeyName", "TEXT PRIMARY KEY NOT NULL UNIQUE"),
                ("EncryptedKey", "TEXT NOT NULL"),
                ("EncryptedIv", "TEXT NOT NULL")
            );
            Logger.Info("数据库表 KeyTable 确保存在");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "确保数据库和目录存在时发生错误");
            throw new InvalidOperationException("确保数据库和目录存在时发生错误: " + ex.Message, ex);
        }
    }

    // 检查目录的写入权限
    private static bool HasWritePermission(string path)
    {
        try
        {
            Logger.Debug($"检查目录 {path} 的写入权限");
            // 尝试在路径中创建一个文件，以测试写入权限
            var testFile = Path.Combine(path, "test-file.tmp");
            using var fs = File.Create(testFile, 1, FileOptions.DeleteOnClose);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"没有对目录 {path} 的写入权限");
            return false;
        }
    }

    // 加密明文并保存密钥到数据库
    public static string Encrypt(string plainText, string keyName)
    {
        try
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            var protectedKey = ProtectedData.Protect(aes.Key, null, DataProtectionScope.CurrentUser);
            var protectedIv = ProtectedData.Protect(aes.IV, null, DataProtectionScope.CurrentUser);

            SaveKeyToDatabase(keyName, protectedKey, protectedIv);
            Logger.Info($"密钥已成功保存到数据库，KeyName: {keyName}");

            using var encryption = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryption, CryptoStreamMode.Write);
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(plainText);
            }

            Logger.Info("加密操作成功");
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加密时发生错误");
            throw;
        }
    }

    // 解密密文
    public static string Decrypt(string encryptedText, string keyName)
    {
        try
        {
            var (protectedKey, protectedIv) = GetKeyFromDatabase(keyName);

            if (protectedKey == null || protectedIv == null)
            {
                throw new InvalidOperationException("无法从数据库中找到对应的密钥或初始向量。");
            }

            var key = ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
            var iv = ProtectedData.Unprotect(protectedIv, null, DataProtectionScope.CurrentUser);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            var encryptedBytes = Convert.FromBase64String(encryptedText);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(encryptedBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            Logger.Info("解密操作成功");
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "解密时发生错误");
            throw;
        }
    }

    // 保存密钥到数据库
    private static void SaveKeyToDatabase(string keyName, byte[] protectedKey, byte[] protectedIv)
    {
        try
        {
            const string sql = """
                               INSERT INTO KeyTable (KeyName, EncryptedKey, EncryptedIv)
                                                   VALUES (@KeyName, @EncryptedKey, @EncryptedIv)
                                                   ON CONFLICT(KeyName) DO UPDATE SET EncryptedKey = @EncryptedKey, EncryptedIv = @EncryptedIv;
                               """;

            DbManager.Execute(sql,
                new
                {
                    KeyName = keyName,
                    EncryptedKey = Convert.ToBase64String(protectedKey),
                    EncryptedIv = Convert.ToBase64String(protectedIv)
                });
            Logger.Info($"密钥 {keyName} 已成功保存到数据库");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"保存密钥 {keyName} 到数据库时发生错误");
            throw;
        }
    }

    // 从数据库中获取密钥和初始向量
    private static (byte[] protectedKey, byte[] protectedIv) GetKeyFromDatabase(string keyName)
    {
        try
        {
            Logger.Info($"从数据库中获取密钥: {keyName}");
            // 从数据库中查询密钥和初始向量
            var result = DbManager.QuerySingleOrDefault<(string EncryptedKey, string EncryptedIv)>("KeyTable",
                " WHERE KeyName = @KeyName",
                ["EncryptedKey", "EncryptedIv"], new { KeyName = keyName });
            // 如果数据库中没有找到对应的密钥或初始向量，则返回 null
            if (result is not { EncryptedKey: not null, EncryptedIv: not null })
            {
                Logger.Warn($"未找到密钥: {keyName}");
                return (null, null);
            }

            var protectedKey = Convert.FromBase64String(result.EncryptedKey);
            var protectedIv = Convert.FromBase64String(result.EncryptedIv);
            Logger.Info($"密钥 {keyName} 已成功从数据库中获取");
            return (protectedKey, protectedIv);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"从数据库中获取密钥 {keyName} 时发生错误");
            throw;
        }
    }
}
