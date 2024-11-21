// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppression either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member",
        Target = "~M:DrugCalculator.Services.EncryptionService.Encrypt(System.String,System.String)~System.String")]
[assembly:
    SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member",
        Target = "~M:DrugCalculator.Services.EncryptionService.Decrypt(System.String,System.String)~System.String")]
[assembly:
    SuppressMessage("Usage", "CA2211:非常量字段应当不可见", Justification = "<挂起>", Scope = "member",
        Target = "~F:DrugCalculator.Utilities.Converters.StringIsNullToVisibilityConverter.Instance")]
[assembly:
    SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member",
        Target = "~M:DrugCalculator.View.Windows.MainWindow.#ctor")]
[assembly:
    SuppressMessage("Style", "IDE0019:使用模式匹配", Justification = "<挂起>", Scope = "member",
        Target = "~M:DrugCalculator.ViewModels.LogViewerViewModel.LogFilter(System.Object)~System.Boolean")]
[assembly: SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member", Target = "~M:DrugCalculator.Services.TrayService.CreateNotifyIcon")]
[assembly: SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member", Target = "~M:DrugCalculator.Services.TrayService.NotifyIcon_MouseUp(System.Object,System.Windows.Forms.MouseEventArgs)")]
[assembly: SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>", Scope = "member", Target = "~M:DrugCalculator.Services.TrayService.Dispose(System.Boolean)")]
