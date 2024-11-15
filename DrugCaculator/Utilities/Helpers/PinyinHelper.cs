using DrugCalculator.Models;
using NPinyin;

namespace DrugCalculator.Utilities.Helpers;

public static class PinyinHelper
{
    // 假设这里有一个实现将汉字转换为拼音首字母的逻辑
    public static string GetFirstLetter(Drug drug)
    {
        var drugName = drug.Name;
        // 实现将汉字转换为拼音首字母的逻辑
        var firstLetter = Pinyin.GetInitials(drugName);

        return firstLetter;
    }

    public static string GetPinyin(Drug drug)
    {
        var drugName = drug.Name;
        // 实现将汉字转换为拼音的逻辑
        var pinyin = Pinyin.GetPinyin(drugName);

        return pinyin;
    }
}