using DrugCalculator.Models;
using NCalc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DrugCalculator.Services;

/// <summary>
/// 提供计算药物用量和评估条件的服务。
/// 根据药物计算规则（DrugCalculationRule）和输入的年龄、体重等参数，返回符合条件的剂量信息。
/// </summary>
public partial class CalculateService
{
    /// <summary>
    /// 遍历药物计算规则，根据输入参数找到第一个符合条件的规则，并返回计算结果。
    /// </summary>
    /// <param name="rules">药物计算规则集合。</param>
    /// <param name="age">输入的年龄值。</param>
    /// <param name="ageUnit">年龄的单位（year 或 month）。</param>
    /// <param name="weight">输入的体重值。</param>
    /// <returns>符合条件的规则计算结果，格式为 JSON 字符串。如果无规则符合条件，则返回空的 JSON 对象。</returns>
    public static string CalculateRules(IEnumerable<DrugCalculationRule> rules, int age, string ageUnit, double weight)
    {
        Console.WriteLine(@"开始执行 CalculateRules 方法");
        // 遍历规则集合
        foreach (var rule in rules)
        {
            Console.WriteLine($@"正在评估 DrugId 为 {rule.DrugId} 的规则");

            // 判断规则条件是否满足
            if (EvaluateCondition(rule.Condition, age, weight, ageUnit))
            {
                Console.WriteLine($@"条件满足，计算 DrugId 为 {rule.DrugId} 的规则");

                // 计算符合条件的规则结果
                var result = CalculateResult(rule, age, ageUnit, weight);
                Console.WriteLine($@"计算得出剂量：{result}，适用于 DrugId 为 {rule.DrugId} 的规则");

                // 构建结果对象
                var resultObject = new Dictionary<string, object>
                {
                    { "Dosage", result },             // 计算剂量
                    { "Unit", rule.Unit },            // 剂量单位
                    { "Frequency", rule.Frequency },  // 用药频率
                    { "Route", rule.Route }           // 给药途径
                };

                // 将结果对象序列化为 JSON 格式
                return JsonSerializer.Serialize(resultObject);
            }
            else
            {
                Console.WriteLine($@"条件不满足，跳过 DrugId 为 {rule.DrugId} 的规则");
            }
        }

        // 若无规则符合条件，返回空 JSON 对象
        Console.WriteLine(@"没有符合条件的规则");
        return JsonSerializer.Serialize(new Dictionary<string, object>());
    }

    /// <summary>
    /// 计算单个规则的结果，根据输入参数和规则的公式计算剂量。
    /// </summary>
    /// <param name="rule">药物计算规则。</param>
    /// <param name="age">输入的年龄值。</param>
    /// <param name="ageUnit">年龄的单位（year 或 month）。</param>
    /// <param name="weight">输入的体重值。</param>
    /// <returns>根据公式计算出的剂量值。</returns>
    private static double CalculateResult(DrugCalculationRule rule, int age, string ageUnit, double weight)
    {
        Console.WriteLine($@"计算 DrugId 为 {rule.DrugId} 的结果");

        // 转换年龄为目标单位
        var ageInTargetUnit = ConvertAgeToTargetUnit(age, ageUnit, GetAgeUnitFromCondition(rule.Condition));
        Console.WriteLine($@"转换后的年龄：{ageInTargetUnit} {GetAgeUnitFromCondition(rule.Condition)}");

        // 使用 NCalc 解析公式，并设置参数值
        var expression = new Expression(rule.Formula)
        {
            Parameters = { ["Age"] = ageInTargetUnit, ["Weight"] = weight }
        };

        // 检查表达式是否有错误
        if (expression.HasErrors()) throw new InvalidOperationException("表达式解析错误: " + expression.Error);

        // 计算并返回结果
        var result = Convert.ToDouble(expression.Evaluate());
        Console.WriteLine($@"计算结果：{result}");
        return result;
    }

    /// <summary>
    /// 评估规则的条件是否满足。
    /// </summary>
    /// <param name="condition">规则的条件字符串。</param>
    /// <param name="age">输入的年龄值。</param>
    /// <param name="weight">输入的体重值。</param>
    /// <param name="ageUnit">年龄的单位（year 或 month）。</param>
    /// <returns>条件是否满足。</returns>
    private static bool EvaluateCondition(string condition, int age, double weight, string ageUnit)
    {
        Console.WriteLine($@"评估条件：{condition}");

        // 转换年龄为条件中使用的单位
        var ageInTargetUnit = ConvertAgeToTargetUnit(age, ageUnit, GetAgeUnitFromCondition(condition));
        Console.WriteLine($@"用于条件评估的转换后的年龄：{ageInTargetUnit} {GetAgeUnitFromCondition(condition)}");

        // 将条件字符串解析为可计算的表达式
        var parsedCondition = ParseCondition(condition);
        Console.WriteLine($@"解析后的条件：{parsedCondition}");

        // 使用 NCalc 解析条件表达式
        var expr = new Expression(parsedCondition)
        {
            Parameters = { ["Age"] = ageInTargetUnit, ["Weight"] = weight }
        };

        // 检查表达式是否有错误
        if (expr.HasErrors()) throw new InvalidOperationException("表达式解析错误: " + expr.Error);

        // 返回条件计算结果
        var conditionResult = (bool)expr.Evaluate();
        Console.WriteLine($@"条件评估结果：{conditionResult}");
        return conditionResult;
    }

    /// <summary>
    /// 获取条件中指定的年龄单位。
    /// </summary>
    /// <param name="condition">规则的条件字符串。</param>
    /// <returns>条件中指定的年龄单位（year 或 month）。如果未明确指定，默认为 year。</returns>
    private static string GetAgeUnitFromCondition(string condition)
    {
        Console.WriteLine($@"从条件中确定年龄单位：{condition}");

        // 根据条件字符串包含的单位，判断年龄单位
        var ageUnit = condition.Contains("year") ? "year" : condition.Contains("month") ? "month" : "year";
        Console.WriteLine($@"确定的年龄单位：{ageUnit}");
        return ageUnit;
    }

    /// <summary>
    /// 将输入的年龄值转换为目标单位。
    /// </summary>
    /// <param name="age">输入的年龄值。</param>
    /// <param name="currentUnit">当前年龄单位（year 或 month）。</param>
    /// <param name="targetUnit">目标年龄单位（year 或 month）。</param>
    /// <returns>转换后的年龄值。</returns>
    private static int ConvertAgeToTargetUnit(int age, string currentUnit, string targetUnit)
    {
        Console.WriteLine($@"将年龄从 {currentUnit} 转换为 {targetUnit}");

        // 根据单位转换规则计算结果
        var convertedAge = currentUnit == targetUnit ? age :
            currentUnit == "year" && targetUnit == "month" ? age * 12 :
            currentUnit == "month" && targetUnit == "year" ? age / 12 :
            age;

        Console.WriteLine($@"转换后的年龄：{convertedAge} {targetUnit}");
        return convertedAge;
    }

    /// <summary>
    /// 解析条件字符串，将其转换为可计算的表达式。
    /// </summary>
    /// <param name="condition">条件字符串。</param>
    /// <returns>转换后的表达式字符串。</returns>
    private static string ParseCondition(string condition)
    {
        Console.WriteLine($@"解析条件字符串：{condition}");

        // 转换 "Age at [x, y]" 或 "Weight at [x, y]" 格式为范围表达式
        condition = AtRegex().Replace(condition, m =>
        {
            var variable = m.Groups[1].Value;
            var minValue = int.Parse(m.Groups[2].Value);
            var maxValue = int.Parse(m.Groups[3].Value);

            // 返回转换后的范围表达式
            var rangeExpression = $"({variable} >= {minValue} && {variable} <= {maxValue})";
            Console.WriteLine($@"解析后的范围表达式：{rangeExpression}");
            return rangeExpression;
        });

        // 去掉单位（year, month, Kg）以生成纯条件
        var finalCondition = condition
            .Replace("year", "")
            .Replace("month", "")
            .Replace("Kg", "");

        Console.WriteLine($@"最终解析后的条件：{finalCondition}");
        return finalCondition;
    }

    [GeneratedRegex(@"(Age|Weight) at \[(\d+),(\d+)\]")]
    private static partial Regex AtRegex();
}