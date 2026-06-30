using System.Globalization;
using SwitchBack.Config;

namespace SwitchBack.App;

public sealed class LocalizationService
{
    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        ["WindowTitle"] = "SwitchBack Settings",
        ["Subtitle"] = "Fix text typed with the wrong keyboard layout.",
        ["Status"] = "Status",
        ["EnableConversion"] = "Enable global text conversion",
        ["UsageHint"] = "Select text in any application, switch Windows to the language you intended, then press the hotkey.",
        ["InterfaceLanguage"] = "Interface language",
        ["SystemLanguage"] = "Use Windows language",
        ["Conversion"] = "Conversion",
        ["Direction"] = "Direction",
        ["FollowWindowsLanguage"] = "Follow active Windows language",
        ["Auto"] = "Auto detect from text",
        ["EnglishToThai"] = "English QWERTY → Thai Kedmanee",
        ["ThaiToEnglish"] = "Thai Kedmanee → English QWERTY",
        ["LayoutA"] = "Keyboard layout A",
        ["LayoutB"] = "Keyboard layout B",
        ["MixedText"] = "Mixed text",
        ["TargetLanguageOnly"] = "Convert toward the active language; keep correct text",
        ["SwapBothLayouts"] = "Swap both keyboard layouts",
        ["RestoreClipboard"] = "Restore the previous Clipboard after conversion (recommended)",
        ["GlobalHotkey"] = "Global hotkey",
        ["HotkeyHint"] = "One shortcut triggers conversion; the active Windows language chooses the direction.",
        ["Windows"] = "Windows",
        ["StartWithWindows"] = "Start SwitchBack with Windows",
        ["Notifications"] = "Show a notification after conversion",
        ["PrivacyTitle"] = "Privacy: ",
        ["Privacy"] = "SwitchBack works 100% offline. Selected text is processed in memory and is never sent to a server.",
        ["HideToTray"] = "Hide to tray",
        ["SaveSettings"] = "Save settings",
        ["SettingsSaved"] = "Settings saved.",
        ["Settings"] = "Settings",
        ["Enabled"] = "Enabled",
        ["Exit"] = "Exit",
        ["ConversionFailed"] = "Conversion failed",
        ["TextConverted"] = "Text converted",
        ["CharactersConverted"] = "{0} characters converted.",
        ["NeedsAttention"] = "SwitchBack needs attention",
        ["CouldNotChangeStatus"] = "Could not change status",
        ["AlreadyRunning"] = "SwitchBack is already running in the system tray.",
        ["SelectModifiers"] = "Choose at least one modifier key (Ctrl, Shift, Alt, or Win).",
        ["SelectTwoLayouts"] = "Choose two different, supported keyboard layouts.",
        ["ServicesNotReady"] = "Application services are not ready."
    };

    private static readonly IReadOnlyDictionary<string, string> Thai = new Dictionary<string, string>
    {
        ["WindowTitle"] = "ตั้งค่า SwitchBack",
        ["Subtitle"] = "แก้ข้อความที่พิมพ์ด้วยภาษาคีย์บอร์ดผิด",
        ["Status"] = "สถานะ",
        ["EnableConversion"] = "เปิดใช้งานการแปลงข้อความทั่ว Windows",
        ["UsageHint"] = "คลุมข้อความในโปรแกรมใดก็ได้ เปลี่ยน Windows เป็นภาษาที่ต้องการ แล้วกดปุ่มลัด",
        ["InterfaceLanguage"] = "ภาษาของโปรแกรม",
        ["SystemLanguage"] = "ใช้ภาษาของ Windows",
        ["Conversion"] = "การแปลงภาษา",
        ["Direction"] = "ทิศทาง",
        ["FollowWindowsLanguage"] = "ตามภาษาปัจจุบันของ Windows",
        ["Auto"] = "ตรวจจากข้อความอัตโนมัติ",
        ["EnglishToThai"] = "อังกฤษ QWERTY → ไทย Kedmanee",
        ["ThaiToEnglish"] = "ไทย Kedmanee → อังกฤษ QWERTY",
        ["LayoutA"] = "รูปแบบคีย์บอร์ด A",
        ["LayoutB"] = "รูปแบบคีย์บอร์ด B",
        ["MixedText"] = "ข้อความผสม",
        ["TargetLanguageOnly"] = "แปลงเข้าภาษาปัจจุบัน และเก็บข้อความที่ถูกไว้",
        ["SwapBothLayouts"] = "สลับคีย์บอร์ดทั้งสองภาษา",
        ["RestoreClipboard"] = "คืนค่า Clipboard เดิมหลังแปลง (แนะนำ)",
        ["GlobalHotkey"] = "ปุ่มลัดส่วนกลาง",
        ["HotkeyHint"] = "ใช้ปุ่มลัดชุดเดียว โดยภาษาปัจจุบันของ Windows เป็นตัวเลือกทิศทาง",
        ["Windows"] = "Windows",
        ["StartWithWindows"] = "เปิด SwitchBack พร้อม Windows",
        ["Notifications"] = "แสดงการแจ้งเตือนหลังแปลง",
        ["PrivacyTitle"] = "ความเป็นส่วนตัว: ",
        ["Privacy"] = "SwitchBack ทำงาน Offline 100% ข้อความถูกประมวลผลในหน่วยความจำและไม่ส่งขึ้น Server",
        ["HideToTray"] = "ซ่อนไปที่ Tray",
        ["SaveSettings"] = "บันทึกการตั้งค่า",
        ["SettingsSaved"] = "บันทึกการตั้งค่าแล้ว",
        ["Settings"] = "ตั้งค่า",
        ["Enabled"] = "เปิดใช้งาน",
        ["Exit"] = "ออก",
        ["ConversionFailed"] = "แปลงข้อความไม่สำเร็จ",
        ["TextConverted"] = "แปลงข้อความแล้ว",
        ["CharactersConverted"] = "แปลงแล้ว {0} ตัวอักษร",
        ["NeedsAttention"] = "SwitchBack ต้องการการตั้งค่า",
        ["CouldNotChangeStatus"] = "เปลี่ยนสถานะไม่สำเร็จ",
        ["AlreadyRunning"] = "SwitchBack กำลังทำงานอยู่ใน System Tray แล้ว",
        ["SelectModifiers"] = "เลือกปุ่ม Ctrl, Shift, Alt หรือ Win อย่างน้อยหนึ่งปุ่ม",
        ["SelectTwoLayouts"] = "เลือกรูปแบบคีย์บอร์ดที่รองรับและไม่ซ้ำกันสองแบบ",
        ["ServicesNotReady"] = "บริการของโปรแกรมยังไม่พร้อม"
    };

    private IReadOnlyDictionary<string, string> _strings = English;

    public UiLanguageMode CurrentMode { get; private set; }

    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : English[key];

    public void Apply(UiLanguageMode mode)
    {
        CurrentMode = mode;
        var useThai = mode == UiLanguageMode.Thai ||
            mode == UiLanguageMode.System && CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "th";
        _strings = useThai ? Thai : English;
    }

    public string GetConversionModeName(ConversionMode mode) => this[mode.ToString()];

    public string GetMixedTextPolicyName(MixedTextPolicy policy) => this[policy.ToString()];
}
