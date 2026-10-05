namespace DesignStudio.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private static readonly IReadOnlyDictionary<string, (string En, string Ar)> Strings =
        new Dictionary<string, (string, string)>
        {
            ["app.title"] = ("Design Studio", "Design Studio"),
            ["app.subtitle"] = ("Design-to-Fabrication Platform", "منصة التصميم إلى التصنيع"),
            ["project.new"] = ("New Project", "مشروع جديد"),
            ["project.open"] = ("Open Project", "فتح مشروع"),
            ["project.save"] = ("Save Project", "حفظ المشروع"),
            ["room.create"] = ("Create Room", "إنشاء غرفة"),
            ["workspace.rooms"] = ("Rooms", "الغرف"),
            ["workspace.walls"] = ("Walls", "الجدران"),
            ["workspace.doors"] = ("Doors", "الأبواب"),
            ["workspace.windows"] = ("Windows", "النوافذ"),
            ["workspace.properties"] = ("Properties", "الخصائص"),
            ["workspace.relationships"] = ("Relationships", "العلاقات"),
            ["workspace.validation"] = ("Validation", "التحقق"),
            ["workspace.designView"] = ("Design View", "مساحة التصميم"),
            ["language.english"] = ("English", "الإنجليزية"),
            ["language.arabic"] = ("Arabic", "العربية"),
            ["workspace.design"] = ("Design", "التصميم"),
            ["workspace.model"] = ("Model", "النموذج"),
            ["workspace.documents"] = ("Documents", "المستندات"),
            ["edit.extend"] = ("Extend 500 mm", "تمديد 500 مم"),
            ["edit.shrink"] = ("Shrink 500 mm", "تقليص 500 مم"),
            ["edit.undo"] = ("Undo", "تراجع"),
            ["edit.redo"] = ("Redo", "إعادة"),
            ["workspace.selectedWall"] = ("Selected Wall", "الجدار المحدد"),
            ["workspace.length"] = ("Length", "الطول"),
            ["workspace.selectedOpening"] = ("Selected Opening", "الفتحة المحددة"),
            ["workspace.openingKind"] = ("Type", "النوع"),
            ["workspace.offset"] = ("Offset", "الإزاحة"),
            ["workspace.width"] = ("Width", "العرض"),
            ["workspace.height"] = ("Height", "الارتفاع"),
            ["workspace.sill"] = ("Sill", "منسوب جلسة النافذة"),
            ["edit.applyOpening"] = ("Apply Opening Dimensions", "تطبيق أبعاد الفتحة"),
            ["edit.addDoor"] = ("Add Door", "إضافة باب"),
            ["edit.addWindow"] = ("Add Window", "إضافة نافذة"),
            ["edit.addCabinet"] = ("Add Cabinet", "إضافة خزانة"),
            ["workspace.selectedCabinet"] = ("Selected Cabinet", "الخزانة المحددة"),
            ["workspace.cabinetPosition"] = ("Position", "الموضع"),
            ["workspace.cabinetWidth"] = ("Width", "العرض"),
            ["workspace.cabinetDepth"] = ("Depth", "العمق"),
            ["workspace.cabinetRotation"] = ("Rotation", "الدوران"),
            ["status.invalidDimension"] = ("Invalid dimension value", "قيمة بُعد غير صالحة"),
            ["status.editRejected"] = ("Edit rejected by validation", "تم رفض التعديل بسبب القيود"),
            ["opening.door"] = ("Door", "باب"),
            ["opening.window"] = ("Window", "نافذة"),
            ["status.ready"] = ("Ready", "جاهز"),
            ["status.saved"] = ("Project saved", "تم حفظ المشروع"),
            ["status.opened"] = ("Project opened", "تم فتح المشروع"),
            ["status.noProject"] = ("No active project", "لا يوجد مشروع نشط"),
            ["recovery.available"] = ("A recovery snapshot is available", "توجد نسخة استرداد متاحة"),
            ["recovery.saved"] = ("Recovery snapshot saved", "تم حفظ نسخة الاسترداد"),
            ["recovery.restored"] = ("Recovery snapshot restored", "تم استرداد نسخة المشروع"),
            ["recovery.cleared"] = ("Recovery snapshot cleared", "تم حذف نسخة الاسترداد")
        };

    public Language CurrentLanguage { get; private set; } = Language.English;

    public event EventHandler? LanguageChanged;

    public void SetLanguage(Language language)
    {
        if (CurrentLanguage == language) return;
        CurrentLanguage = language;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Get(string key)
    {
        if (!Strings.TryGetValue(key, out var value))
            return $"[{key}]";

        return CurrentLanguage == Language.Arabic ? value.Ar : value.En;
    }
}
