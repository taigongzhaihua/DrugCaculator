using DrugCalculator.Models;
using NPinyin;
using System;

namespace DrugCalculator.Utilities.Helpers
{
    /// <summary>
    /// 提供汉字转换为拼音或拼音首字母的辅助方法。
    /// 使用 NPinyin 库进行转换。
    /// </summary>
    public static class PinyinHelper
    {
        /// <summary>
        /// 获取药物名称的拼音首字母。
        /// </summary>
        /// <param name="drug">药物对象，其名称需要转换为拼音首字母。</param>
        /// <returns>药物名称对应的拼音首字母字符串。</returns>
        /// <exception cref="ArgumentNullException">如果 drug 或 drug.Name 为 null 时抛出。</exception>
        public static string GetFirstLetter(Drug drug)
        {
            // 检查 drug 和 drug.Name 是否为 null
            if (drug == null) throw new ArgumentNullException(nameof(drug), @"药物对象不能为空。");
            if (string.IsNullOrWhiteSpace(drug.Name))
                throw new ArgumentNullException(nameof(drug.Name), @"药物名称不能为空。");

            // 使用 NPinyin 库将药物名称转换为拼音首字母
            var firstLetter = Pinyin.GetInitials(drug.Name);

            return firstLetter;
        }

        /// <summary>
        /// 获取药物名称的全拼。
        /// </summary>
        /// <param name="drug">药物对象，其名称需要转换为全拼。</param>
        /// <returns>药物名称对应的拼音全拼字符串。</returns>
        /// <exception cref="ArgumentNullException">如果 drug 或 drug.Name 为 null 时抛出。</exception>
        public static string GetPinyin(Drug drug)
        {
            // 检查 drug 和 drug.Name 是否为 null
            if (drug == null) throw new ArgumentNullException(nameof(drug), @"药物对象不能为空。");
            if (string.IsNullOrWhiteSpace(drug.Name))
                throw new ArgumentNullException(nameof(drug.Name), @"药物名称不能为空。");

            // 使用 NPinyin 库将药物名称转换为拼音全拼
            var pinyin = Pinyin.GetPinyin(drug.Name);

            return pinyin;
        }
    }
}