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
    public class DeepSeekService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // API Key 由用户设置并保存在设置中
        private static string ApiKey => GetApiKeyFromSettings();
        private const string ApiUrl = "https://api.deepseek.com/chat/completions";
        private const string JsonMediaType = "application/json";

        // HttpClient 单例实例，以避免套接字耗尽问题
        private static readonly HttpClient HttpClientInstance = new();

        static DeepSeekService()
        {
            // 设置 HttpClient 的默认请求头
            HttpClientInstance.DefaultRequestHeaders.Add("User-Agent", "DeepSeekServiceClient");
        }

        public static string GetApiKeyFromSettings()
        {
            // 获取用户设置中的 API Key，并进行解密
            var encryptedApiKey = Settings.Default.DeepSeekApiKey;
            if (!string.IsNullOrEmpty(encryptedApiKey))
                return EncryptionService.Decrypt(encryptedApiKey, "DeepSeekApiKey");
            Console.WriteLine(@"API密钥未在应用程序设置中配置。");
            return null;
        }

        public static async Task<List<DrugCalculationRule>> GenerateDrugCalculationRulesAsync(Drug drug)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Headers.Add("Accept", JsonMediaType);
                request.Headers.Add("Authorization", $"Bearer {ApiKey}");

                var usageDescription = drug.Usage;

                // 构建请求数据
                const string prompt = $"根据用户给出的用法用量描述，生成 DrugCalculationRule 集合。每个 DrugCalculationRule 的结构如下：" +
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

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // 提取生成的 JSON 内容
                var generatedJson = ExtractGeneratedJson(jsonResponse);

                // 解析生成的 DrugCalculationRule 集合
                var rules = ParseDrugCalculationRules(generatedJson);
                foreach (var rule in rules)
                {
                    rule.DrugId = drug.Id;
                }

                return rules;
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Logger.Error($"DeepSeekService：生成和保存计算规则时发生错误: {ex.Message}", ex);
                throw;
            }
        }

        // 提取生成的 JSON 内容
        private static string ExtractGeneratedJson(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
            {
                Logger.Warn("DeepSeekService：JSON响应内容为空或null。");
                throw new ArgumentException(@"DeepSeekService：JSON响应内容为空或null。", nameof(jsonResponse));
            }

            try
            {
                var result = JsonConvert.DeserializeObject<DeepSeekResponse>(jsonResponse);
                if (result?.Choices != null && result.Choices.Count != 0 && result.Choices[0].Message != null)
                    return result.Choices[0].Message.Content;
                Logger.Warn("DeepSeekService：无法从响应中提取生成的JSON内容。");
                throw new InvalidOperationException("DeepSeekService：无法从响应中提取生成的JSON内容。");

            }
            catch (JsonException ex)
            {
                Logger.Error("DeepSeekService：无法解析响应的JSON内容，请检查返回的格式是否正确。", ex);
                throw new JsonException("DeepSeekService：无法解析响应的JSON内容，请检查返回的格式是否正确。", ex);
            }
        }

        // 解析生成的 DrugCalculationRule 集合
        private static List<DrugCalculationRule> ParseDrugCalculationRules(string generatedJson)
        {
            if (string.IsNullOrEmpty(generatedJson))
            {
                return [];
            }

            try
            {
                var response = JsonConvert.DeserializeObject<GeneratedResponse>(generatedJson);
                var rules = response?.DrugCalculationRules ?? [];

                // 记录解析后的规则
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

    public class DeepSeekResponse
    {
        // DeepSeek API 的响应内容，包含多个 Choice 对象
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        // 每个 Choice 包含一个 Message 对象
        public Message Message { get; set; }
    }

    public class Message
    {
        // Message 的内容
        public string Content { get; set; }
    }
    // 生成的响应类定义
    public class GeneratedResponse
    {
        public List<DrugCalculationRule> DrugCalculationRules { get; set; }
    }

}
