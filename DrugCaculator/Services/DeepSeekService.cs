using DrugCalculator.Models;
using DrugCalculator.Properties;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 提供与 DeepSeek API 交互的服务类，用于根据药物的用法描述生成计算规则。
    /// </summary>
    public class DeepSeekService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // 从应用设置中获取 API Key
        private static string ApiKey => GetApiKeyFromSettings();

        // DeepSeek API 的基本 URL
        private const string ApiUrl = "https://api.deepseek.com/chat/completions";

        // JSON 媒体类型
        private const string JsonMediaType = "application/json";

        // HttpClient 单例实例，用于发送 HTTP 请求
        private static readonly HttpClient HttpClientInstance = new();

        // 静态构造函数：设置 HttpClient 默认请求头
        static DeepSeekService()
        {
            HttpClientInstance.DefaultRequestHeaders.Add("User-Agent", "DeepSeekServiceClient");
        }

        /// <summary>
        /// 从应用程序设置中获取加密的 API Key，并进行解密。
        /// </summary>
        /// <returns>解密后的 API Key。</returns>
        public static string GetApiKeyFromSettings()
        {
            var encryptedApiKey = Settings.Default.DeepSeekApiKey;
            if (!string.IsNullOrEmpty(encryptedApiKey))
                return EncryptionService.Decrypt(encryptedApiKey, "DeepSeekApiKey");

            Console.WriteLine(@"API密钥未在应用程序设置中配置。");
            return null;
        }

        /// <summary>
        /// 根据药物的用法描述，通过 DeepSeek API 生成计算规则。
        /// </summary>
        /// <param name="drug">包含用法描述的药物对象。</param>
        /// <returns>生成的 DrugCalculationRule 集合。</returns>
        public static async Task<List<DrugCalculationRule>> GenerateDrugCalculationRulesAsync(Drug drug)
        {
            try
            {
                // 创建 HTTP 请求
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);

                // 设置请求头，包括授权信息
                request.Headers.Add("Accept", JsonMediaType);
                request.Headers.Add("Authorization", $"Bearer {ApiKey}");

                // 药物用法描述
                var usageDescription = drug.Usage;

                // 构建 DeepSeek API 请求数据
                const string prompt = "根据用户给出的用法用量描述，生成 DrugCalculationRule 集合。每个 DrugCalculationRule 的结构如下：" +
                   "\nCondition: 条件，对年龄和体重进行判断，格式为[<变量(Age/Weight)> <运算符(\"<\"/\">\"/\"<=\"/\">=\"/\"=\"/\"at [x,y]\")> <数字或范围> <单位(year/month/Kg)>]<逻辑符(||或&&)>[条件2]，一般成人剂量若未特殊指明，均为12岁以上。" +
                   "\n注意：年龄必须为int类型，每个DrugCalculationRule中只能出现一个年龄单位，若遇到类似【6个月-2岁】的范围，可将其拆分为两个DrugCalculationRule，Condition分别为Age at [6,11] month 和 Age at [1,2] year，" +
                   "\n单位必须接在范围后面，不可写成[1 month,2 year]、[1 month,2 year)、[0.5,1] year等，不可出现中文。" +
                   "\nFormula: 运算式，直接数值或涉及变量Age/Weight的可解析的运算式。不可写成2-4之类的范围，遇到范围可取最小值。不可写入中文，若需根据情况调整的，可写入-1" +
                   "\nUnit: 剂量单位（mg/g/包/粒/片/支/ml/U/IU）。" +
                   "\nFrequency: 用药频次（PID、QID、TID、BID、QD、QN、Q2H、Q4H、Q6H、Q8H、Q12H、QOD、Q3D、QW、SOS、ST），最好只使用一个频次。" +
                   "\nRoute: 给药途径（口服,含服,静脉注射,静脉滴注,肌肉注射,皮下注射,皮内注射,局麻用,外用,外敷,外涂,外洗,外贴,外喷,舌下含服,直肠给药,灌肠,肛门塞入,鼻腔喷雾,口腔喷雾,吸入,雾化吸入,滴耳,滴眼,透皮贴片,阴道给药）" +
                   "\n请以JSON格式返回集合。" +
                   "\n注意：1.若多条DrugCalculationRule中Condition相同，若标明有\"一般剂量\"/\"维持剂量\"，只保留一般剂量/维持剂量，若未标明，则只保留每日给药量最大的一个。" +
                   "\n2.Condition和Formula必须严格按照给出格式书写，不可出现(Age/Weight)之外的任何变量，不得包含中文。" +
                   "\n示例：" +
                   "\n{" +
                   "\n  \"DrugCalculationRules\": [" +
                   "\n       {" +
                   "\n         \"Condition\": \"Age < 3 year && Weight < 40 Kg\"," +
                   "\n         \"Formula\": \"7.5\"," +
                   "\n         \"Unit\": \"mg\"," +
                   "\n         \"Frequency\": \"TID\"," +
                   "\n         \"Route\": \"口服\"" +
                   "\n       }," +
                   "\n       {" +
                   "\n         \"Condition\": \"Age at [3,14] year\"," +
                   "\n         \"Formula\": \"31.25*Weight\"," +
                   "\n         \"Unit\": \"mg\"," +
                   "\n         \"Frequency\": \"TID\"," +
                   "\n         \"Route\": \"口服\"" +
                   "\n       }" +
                   "\n  ]" +
                   "\n}";

                var requestData = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = prompt },
                        new { role = "user", content = usageDescription }
                    },
                    model = "deepseek-chat",
                    frequency_penalty = 0,
                    max_tokens = 2048,
                    presence_penalty = 0,
                    response_format = new { type = "json_object" },
                    stop = (object)null,
                    stream = false,
                    stream_options = (object)null,
                    temperature = 0.2,
                    top_p = 1,
                    tools = (object)null,
                    tool_choice = "none",
                    logprobs = false,
                    top_logprobs = (object)null
                };

                // 设置请求内容
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, JsonMediaType);
                request.Content = content;

                // 发送 HTTP 请求并获取响应
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode(); // 检查响应状态

                // 获取响应的 JSON 内容
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // 提取生成的 JSON 内容
                var generatedJson = ExtractGeneratedJson(jsonResponse);

                // 解析生成的 DrugCalculationRule 集合
                var rules = ParseDrugCalculationRules(generatedJson);
                foreach (var rule in rules) rule.DrugId = drug.Id;

                return rules;
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Logger.Error($"DeepSeekService：生成和保存计算规则时发生错误: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 提取 DeepSeek API 响应中的生成 JSON 数据。
        /// </summary>
        /// <param name="jsonResponse">DeepSeek API 返回的 JSON 响应字符串。</param>
        /// <returns>提取到的 JSON 数据。</returns>
        private static string ExtractGeneratedJson(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
            {
                Logger.Warn("DeepSeekService：JSON响应内容为空或null。");
                throw new ArgumentException(@"DeepSeekService：JSON响应内容为空或null。", nameof(jsonResponse));
            }

            try
            {
                // 解析 DeepSeek API 响应
                var result = JsonConvert.DeserializeObject<DeepSeekResponse>(jsonResponse);
                if (result?.Choices != null && result.Choices.Count != 0 && result.Choices[0].Message != null)
                    return result.Choices[0].Message.Content;

                Logger.Warn("DeepSeekService：无法从响应中提取生成的JSON内容。");
                throw new InvalidOperationException("DeepSeekService：无法从响应中提取生成的JSON内容。");
            }
            catch (JsonException ex)
            {
                Logger.Error("DeepSeekService：无法解析响应的JSON内容，请检查返回的格式是否正确。");
                throw new JsonException("DeepSeekService：无法解析响应的JSON内容，请检查返回的格式是否正确。", ex);
            }
        }

        /// <summary>
        /// 解析生成的 JSON 数据为 DrugCalculationRule 集合。
        /// </summary>
        /// <param name="generatedJson">生成的 JSON 数据。</param>
        /// <returns>解析后的 DrugCalculationRule 集合。</returns>
        private static List<DrugCalculationRule> ParseDrugCalculationRules(string generatedJson)
        {
            if (string.IsNullOrEmpty(generatedJson)) return [];

            try
            {
                // 将生成的 JSON 数据反序列化为对象
                var response = JsonConvert.DeserializeObject<GeneratedResponse>(generatedJson);
                var rules = response?.DrugCalculationRules ?? [];

                // 记录解析结果
                Logger.Debug($"DeepSeekService：解析后的 DrugCalculationRule 集合: {JsonConvert.SerializeObject(rules)}");

                return rules;
            }
            catch (JsonException ex)
            {
                Logger.Error("DeepSeekService：无法解析生成的JSON内容，请检查返回的格式是否正确。", ex);
                throw new JsonException("DeepSeekService：无法解析生成的JSON内容，请检查返回的格式是否正确。", ex);
            }
        }
    }

    /// <summary>
    /// DeepSeek API 响应类，包含多个 Choice 对象。
    /// </summary>
    public class DeepSeekResponse
    {
        public List<Choice> Choices { get; set; }
    }

    /// <summary>
    /// Choice 对象，表示 DeepSeek API 响应的一个选择项。
    /// </summary>
    public class Choice
    {
        public Message Message { get; set; }
    }

    /// <summary>
    /// Message 对象，表示 DeepSeek API 响应中的消息内容。
    /// </summary>
    public class Message
    {
        public string Content { get; set; }
    }

    /// <summary>
    /// 生成的响应对象，包含 DrugCalculationRule 集合。
    /// </summary>
    public class GeneratedResponse
    {
        public List<DrugCalculationRule> DrugCalculationRules { get; set; }
    }
}
