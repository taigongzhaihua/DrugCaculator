using ExcelDataReader;
using NLog;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace DrugCalculator.DataAccess;

public class ExcelManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static DataTable Read(string filePath)
    {
        // 日志输出，显示文件路径
        Logger.Info($@"调用 Read 方法，文件路径：{filePath}");

        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Logger.Warn($@"文件 '{filePath}' 不存在。");
            throw new FileNotFoundException($"文件 '{filePath}' 不存在。");
        }

        // 注册编码提供程序，以支持读取包含特定编码的 Excel 文件
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 打开文件流以读取 Excel 文件
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        Logger.Info(@"文件成功打开。");

        // 使用 ExcelDataReader 创建读取器对象
        using var reader = ExcelReaderFactory.CreateReader(stream);
        Logger.Info(@"正在读取 Excel 文件...");

        // 将 Excel 文件转换为 DataSet，对应的 DataTable 使用第一行为列标题
        var result = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        // 检查工作簿中是否包含任何表格
        if (result == null || result.Tables.Count == 0)
        {
            Logger.Info(@"工作簿中不包含任何表格。");
            throw new InvalidDataException("工作簿中不包含任何表格。");
        }

        Logger.Info(@"成功读取表格。");
        // 返回第一个表格的数据
        return result.Tables[0];
    }


    // ReSharper disable once UnusedMember.Global
    public static void Write(DataTable table, string filePath)
    {
        // 日志输出，显示文件路径
        Logger.Info($@"调用 Write 方法，文件路径：{filePath}");

        // 检查 DataTable 是否为空或不包含任何数据
        if (table == null || table.Columns.Count == 0 || table.Rows.Count == 0)
        {
            Logger.Warn("DataTable 为空或不包含任何数据，无法写入。");
            throw new ArgumentException("DataTable 为空或不包含任何数据，无法写入。");
        }

        // 设置 ExcelPackage 的许可证上下文为非商业用途
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // 创建新的 ExcelPackage 对象
        using var package = new ExcelPackage();
        Logger.Info("正在创建新的 Excel 文件...");

        // 创建一个新的工作表，并命名为 "Sheet1"
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        var columnCount = table.Columns.Count;

        // 写入列标题到工作表的第一行
        Logger.Info(@"正在写入列标题...");
        for (var i = 0; i < columnCount; i++) worksheet.Cells[1, i + 1].Value = table.Columns[i].ColumnName;

        // 写入每一行数据到工作表
        Logger.Info(@"正在写入行数据...");
        for (var i = 0; i < table.Rows.Count; i++)
            for (var j = 0; j < columnCount; j++)
            {
                var cellValue = table.Rows[i][j];
                // 如果单元格值为 DBNull，则写入 null
                worksheet.Cells[i + 2, j + 1].Value = cellValue is DBNull ? null : cellValue;
            }

        // 获取目标文件的目录路径
        var directoryPath = Path.GetDirectoryName(filePath);
        // 如果目录不存在，则创建目录
        if (!Directory.Exists(directoryPath))
        {
            Logger.Info($@"目录 '{directoryPath}' 不存在，正在创建...");
            Directory.CreateDirectory(directoryPath!);
        }

        // 保存 Excel 文件到指定路径
        var fileInfo = new FileInfo(filePath);
        Logger.Info(@"正在保存 Excel 文件...");
        package.SaveAs(fileInfo);
        Logger.Info($@"文件已成功保存到 '{filePath}'。");
    }
}