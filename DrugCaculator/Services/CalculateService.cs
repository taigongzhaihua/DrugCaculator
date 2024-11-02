using DrugCaculator.Models;
using NCalc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WpfApp2.Services
{
    public class CalculateService
    {
        // 计算并返回符合条件的第一个规则的结果，结果为JSON对象
        public string CalculateRules(IEnumerable<DrugCalculationRule> rules, int age, string ageUnit, double weight)
        {
            Console.WriteLine(@"开始执行 CalculateRules 方法");
            // 遍历每个规则，找到符合条件的第一个规则并返回计算结果
            foreach (var rule in rules)
            {
                Console.WriteLine($@"正在评估 DrugId 为 {rule.DrugId} 的规则");
                // 如果条件满足，则计算该规则的结果
                if (EvaluateCondition(rule.Condition, age, weight, ageUnit))
                {
                    Console.WriteLine($@"条件满足，计算 DrugId 为 {rule.DrugId} 的规则");
                    var result = CalculateResult(rule, age, ageUnit, weight);
                    Console.WriteLine($@"计算得出剂量：{result}，适用于 DrugId 为 {rule.DrugId} 的规则");
                    var resultObject = new Dictionary<string, object>
                    {
                        { "Dosage", result },
                        { "Unit", rule.Unit },
                        { "Frequency", rule.Frequency },
                        { "Route",rule.Route}
                    };
                    // 返回符合条件的规则的结果，序列化为JSON字符串
                    return JsonSerializer.Serialize(resultObject);
                }
                else
                {
                    Console.WriteLine($@"条件不满足，跳过 DrugId 为 {rule.DrugId} 的规则");
                }
            }

            // 如果没有符合条件的规则，返回一个空的JSON对象
            Console.WriteLine(@"没有符合条件的规则");
            return JsonSerializer.Serialize(new Dictionary<string, object>());
        }

        // 计算单个规则的结果
        private static double CalculateResult(DrugCalculationRule rule, int age, string ageUnit, double weight)
        {
            Console.WriteLine($@"计算 DrugId 为 {rule.DrugId} 的结果");
            // 获取条件中使用的年龄单位并将年龄转换为目标单位
            var ageInTargetUnit = ConvertAgeToTargetUnit(age, ageUnit, GetAgeUnitFromCondition(rule.Condition));
            Console.WriteLine($@"转换后的年龄：{ageInTargetUnit} {GetAgeUnitFromCondition(rule.Condition)}");

            // 使用NCalc库计算公式结果，将年龄和体重作为参数传入
            var expression = new Expression(rule.Formula)
            {
                Parameters = { ["Age"] = ageInTargetUnit, ["Weight"] = weight }
            };

            // 计算并返回公式结果
            if (expression.HasErrors())
            {
                throw new InvalidOperationException("表达式解析错误: " + expression.Error);
            }

            var result = Convert.ToDouble(expression.Evaluate());
            Console.WriteLine($@"计算结果：{result}");
            return result;
        }

        // 解析条件并进行条件检查
        private static bool EvaluateCondition(string condition, int age, double weight, string ageUnit)
        {
            Console.WriteLine($@"评估条件：{condition}");
            // 获取条件中使用的年龄单位并将年龄转换为目标单位
            var ageInTargetUnit = ConvertAgeToTargetUnit(age, ageUnit, GetAgeUnitFromCondition(condition));
            Console.WriteLine($@"用于条件评估的转换后的年龄：{ageInTargetUnit} {GetAgeUnitFromCondition(condition)}");

            // 将条件字符串转换为可计算的表达式
            var parsedCondition = ParseCondition(condition);
            Console.WriteLine($@"解析后的条件：{parsedCondition}");

            // 使用NCalc库解析条件，将年龄和体重作为参数传入
            var expr = new Expression(parsedCondition)
            {
                Parameters = { ["Age"] = ageInTargetUnit, ["Weight"] = weight }
            };

            // 计算并返回条件的布尔值结果
            if (expr.HasErrors())
            {
                throw new InvalidOperationException("表达式解析错误: " + expr.Error);
            }

            var conditionResult = (bool)expr.Evaluate();
            Console.WriteLine($@"条件评估结果：{conditionResult}");
            return conditionResult;
        }

        // 获取条件中使用的年龄单位
        private static string GetAgeUnitFromCondition(string condition)
        {
            Console.WriteLine($@"从条件中确定年龄单位：{condition}");
            // 根据条件字符串中的内容，确定年龄单位是“year”还是“month”，默认返回“year”
            var ageUnit = condition.Contains("year") ? "year" : (condition.Contains("month") ? "month" : "year");
            Console.WriteLine($@"确定的年龄单位：{ageUnit}");
            return ageUnit;
        }

        // 将age转换为目标单位
        private static int ConvertAgeToTargetUnit(int age, string currentUnit, string targetUnit)
        {
            Console.WriteLine($@"将年龄从 {currentUnit} 转换为 {targetUnit}");
            var convertedAge = currentUnit == targetUnit ? age :
                               currentUnit == "year" && targetUnit == "month" ? age * 12 :
                               currentUnit == "month" && targetUnit == "year" ? age / 12 :
                               age;
            Console.WriteLine($@"转换后的年龄：{convertedAge} {targetUnit}");
            return convertedAge;
        }

        // 解析条件字符串
        private static string ParseCondition(string condition)
        {
            Console.WriteLine($@"解析条件字符串：{condition}");
            // 解析“Age at [x, y]”或“Weight at [x, y]”格式的条件，将其转换为范围表达式
            condition = Regex.Replace(condition, @"(Age|Weight) at \[(\d+),(\d+)\]", m =>
            {
                var variable = m.Groups[1].Value;
                var minValue = int.Parse(m.Groups[2].Value);
                var maxValue = int.Parse(m.Groups[3].Value);
                // 返回转换后的范围表达式，例如“(Age >= minValue && Age <= maxValue)”
                var rangeExpression = $"({variable} >= {minValue} && {variable} <= {maxValue})";
                Console.WriteLine($@"解析后的范围表达式：{rangeExpression}");
                return rangeExpression;
            });

            // 去掉单位，例如“Age > x year”或“Weight < x Kg”转换为“Age > x”或“Weight < x”
            var finalCondition = condition
                    .Replace("year", "")
                    .Replace("month", "")
                    .Replace("Kg", "");
            Console.WriteLine($@"最终解析后的条件：{finalCondition}");
            return finalCondition;
        }
    }
}

