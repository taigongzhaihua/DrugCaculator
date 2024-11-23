using DrugCalculator.DataAccess;
using NLog;
using System;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 提供加密和解密操作的服务，支持对称加密并通过 SQLite 数据库存储密钥。
    /// </summary>
    public static class EncryptionService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // 应用程序名称，用于定义应用专属的存储路径
        private static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;

        // 应用程序数据路径，用于存储数据库文件
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // 数据库目录和文件路径
        private static readonly string DbDirectory = Path.Combine(AppDataPath, AppName);
        private static readonly string DbPath = Path.Combine(DbDirectory, "EncryptionDatabase.db");

        // 数据库连接字符串
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        // 数据库管理器实例，用于操作数据库
        private static readonly DbManager DbManager = new(ConnectionString);

        // 静态构造函数，初始化服务时确保数据库和目录存在。
        // 如果发生异常，整个服务将无法正常工作。
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

        /// <summary>
        /// 确保数据库和目录存在，如果不存在则创建。
        /// 数据库存储在用户的 ApplicationData 路径下，确保对不同用户独立存储。
        /// 数据库表 KeyTable 包含密钥名称和加密密钥，设计为保证密钥的唯一性。
        /// </summary>
        private static void EnsureDatabase()
        {
            try
            {
                // 创建数据库目录
                if (!Directory.Exists(DbDirectory))
                {
                    Logger.Info($"{DbDirectory} 不存在，正在创建");
                    Directory.CreateDirectory(DbDirectory);
                    Logger.Info("目录创建成功");
                }

                // 检查目录写入权限
                if (!HasWritePermission(DbDirectory))
                {
                    throw new UnauthorizedAccessException($"没有对目录 {DbDirectory} 的写入权限。");
                }

                // 创建数据库文件
                if (!File.Exists(DbPath))
                {
                    Logger.Info($"{DbPath} 不存在，正在创建");
                    SQLiteConnection.CreateFile(DbPath);
                    Logger.Info("数据库文件创建成功");
                }

                // 创建数据库表
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

        /// <summary>
        /// 检查指定目录是否具有写入权限。
        /// 如果目录无法写入，可能是用户权限不足或目录被保护。
        /// 推荐检查用户权限或确保应用在合适的环境下运行。
        /// </summary>
        /// <param name="path">目标目录路径。</param>
        /// <returns>如果具有写入权限返回 true，否则返回 false。</returns>

        private static bool HasWritePermission(string path)
        {
            try
            {
                Logger.Debug($"检查目录 {path} 的写入权限");
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

        /// <summary>
        /// 加密给定的明文并保存加密密钥到数据库。
        /// 加密密钥和初始向量使用 Windows 数据保护 API（ProtectedData）加密，
        /// 只能在当前用户环境下解密。
        /// 加密后的密文以 Base64 格式返回。
        /// </summary>
        /// <param name="plainText">需要加密的明文。</param>
        /// <param name="keyName">密钥的名称，用于标识存储的密钥。</param>
        /// <returns>加密后的密文（Base64 编码）。</returns>

        public static string Encrypt(string plainText, string keyName)
        {
            try
            {
                using var aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                // 使用 DataProtection API 加密密钥和 IV
                var protectedKey = ProtectedData.Protect(aes.Key, null, DataProtectionScope.CurrentUser);
                var protectedIv = ProtectedData.Protect(aes.IV, null, DataProtectionScope.CurrentUser);

                // 将加密的密钥和 IV 保存到数据库
                SaveKeyToDatabase(keyName, protectedKey, protectedIv);

                // 使用 AES 进行加密操作
                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
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

        /// <summary>
        /// 解密给定的密文。
        /// 密钥和初始向量从数据库中读取，并使用 Windows 数据保护 API 解密。
        /// 解密后的明文直接返回。
        /// </summary>
        /// <param name="encryptedText">需要解密的密文（Base64 编码）。</param>
        /// <param name="keyName">密钥的名称，用于标识存储的密钥。</param>
        /// <returns>解密后的明文。</returns>

        public static string Decrypt(string encryptedText, string keyName)
        {
            try
            {
                var (protectedKey, protectedIv) = GetKeyFromDatabase(keyName);

                if (protectedKey == null || protectedIv == null)
                {
                    throw new InvalidOperationException($"无法从数据库中找到对应的密钥或初始向量: {keyName}");
                }

                // 解密密钥和 IV
                var key = ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
                var iv = ProtectedData.Unprotect(protectedIv, null, DataProtectionScope.CurrentUser);

                // 使用 AES 进行解密操作
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

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

        /// <summary>
        /// 将加密密钥和初始向量保存到数据库。
        /// 如果密钥名称已存在，则使用 ON CONFLICT 更新现有记录。
        /// </summary>
        /// <param name="keyName">密钥的名称。</param>
        /// <param name="protectedKey">加密后的密钥。</param>
        /// <param name="protectedIv">加密后的初始向量。</param>

        private static void SaveKeyToDatabase(string keyName, byte[] protectedKey, byte[] protectedIv)
        {
            try
            {
                const string sql = """
                INSERT INTO KeyTable (KeyName, EncryptedKey, EncryptedIv)
                VALUES (@KeyName, @EncryptedKey, @EncryptedIv)
                ON CONFLICT(KeyName) DO UPDATE SET EncryptedKey = @EncryptedKey, EncryptedIv = @EncryptedIv;
                """;

                DbManager.Execute(sql, new
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

        /// <summary>
        /// 从数据库中获取加密的密钥和初始向量。
        /// 如果未找到密钥，返回 null；否则返回加密后的密钥和初始向量。
        /// </summary>
        /// <param name="keyName">密钥的名称。</param>
        /// <returns>加密后的密钥和初始向量。</returns>

        private static (byte[] protectedKey, byte[] protectedIv) GetKeyFromDatabase(string keyName)
        {
            try
            {
                var result = DbManager.QuerySingleOrDefault<(string EncryptedKey, string EncryptedIv)>(
                    "KeyTable", " WHERE KeyName = @KeyName",
                    ["EncryptedKey", "EncryptedIv"],
                    new { KeyName = keyName });

                if (result is { EncryptedKey: not null, EncryptedIv: not null })
                    return (Convert.FromBase64String(result.EncryptedKey),
                        Convert.FromBase64String(result.EncryptedIv));
                Logger.Warn($"未找到密钥: {keyName}");
                return (null, null);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"从数据库中获取密钥 {keyName} 时发生错误");
                throw;
            }
        }
    }
}
