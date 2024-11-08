using Dapper;
using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace DrugCaculator.Services;

public static class EncryptionService
{
    private static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;
    private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string DbDirectory = Path.Combine(AppDataPath, AppName);
    private static readonly string DbPath = Path.Combine(DbDirectory, "EncryptionDatabase.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    static EncryptionService()
    {
        try
        {
            EnsureDatabase();
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"初始化 EncryptionService 时发生错误: {ex.Message}");
            throw new InvalidOperationException("初始化 EncryptionService 时发生错误: " + ex.Message, ex);
        }
    }

    private static void EnsureDatabase()
    {
        try
        {
            if (!Directory.Exists(DbDirectory))
            {
                Console.WriteLine($@"{DbDirectory}不存在，正在创建");
                Directory.CreateDirectory(DbDirectory);
                Console.WriteLine(@"创建成功");
            }
            if (!HasWritePermission(DbDirectory))
            {
                throw new UnauthorizedAccessException($"没有对目录 {DbDirectory} 的写入权限。");
            }

            if (!File.Exists(DbPath))
            {
                Console.WriteLine($@"{DbPath} 不存在，正在创建");
                SQLiteConnection.CreateFile(DbPath);
                Console.WriteLine(@"创建成功");
            }
            Console.WriteLine($@"ConnectionString = '{ConnectionString}'");
            using var connection = new SQLiteConnection(ConnectionString);
            Console.WriteLine(@"数据库连接成功");
            connection.Open();
            connection.Execute("""
                               CREATE TABLE IF NOT EXISTS KeyTable (
                                                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                    KeyName TEXT NOT NULL UNIQUE,
                                                                    EncryptedKey TEXT NOT NULL,
                                                                    EncryptedIv TEXT NOT NULL
                                                                );
                               """);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("确保数据库和目录存在时发生错误: " + ex.Message, ex);
        }
    }
    private static bool HasWritePermission(string path)
    {
        try
        {
            // 尝试在路径中创建一个文件，以测试写入权限
            var testFile = Path.Combine(path, "test-file.tmp");
            using var fs = File.Create(testFile, 1, FileOptions.DeleteOnClose);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static string Encrypt(string plainText, string keyName)
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        var protectedKey = ProtectedData.Protect(aes.Key, null, DataProtectionScope.CurrentUser);
        var protectedIv = ProtectedData.Protect(aes.IV, null, DataProtectionScope.CurrentUser);

        SaveKeyToDatabase(keyName, protectedKey, protectedIv);

        using var encryption = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryption, CryptoStreamMode.Write);
        using (var writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string encryptedText, string keyName)
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
        return reader.ReadToEnd();
    }

    private static void SaveKeyToDatabase(string keyName, byte[] protectedKey, byte[] protectedIv)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        const string sql = """
                           INSERT INTO KeyTable (KeyName, EncryptedKey, EncryptedIv)
                                               VALUES (@KeyName, @EncryptedKey, @EncryptedIv)
                                               ON CONFLICT(KeyName) DO UPDATE SET EncryptedKey = @EncryptedKey, EncryptedIv = @EncryptedIv;
                           """;

        connection.Execute(sql, new { KeyName = keyName, EncryptedKey = Convert.ToBase64String(protectedKey), EncryptedIv = Convert.ToBase64String(protectedIv) });
    }

    private static (byte[] protectedKey, byte[] protectedIv) GetKeyFromDatabase(string keyName)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        const string sql = "SELECT EncryptedKey, EncryptedIv FROM KeyTable WHERE KeyName = @KeyName";
        var result = connection.QuerySingleOrDefault<(string EncryptedKey, string EncryptedIv)>(sql, new { KeyName = keyName });

        if (result is not { EncryptedKey: not null, EncryptedIv: not null }) return (null, null);
        var protectedKey = Convert.FromBase64String(result.EncryptedKey);
        var protectedIv = Convert.FromBase64String(result.EncryptedIv);
        return (protectedKey, protectedIv);

    }
}