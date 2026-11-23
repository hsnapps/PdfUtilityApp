using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace PdfUtilityApp.Services
{
    public static class LocalizationManager
    {
        private static readonly string SettingsPath =
            Path.Combine(AppContext.BaseDirectory, "ui_language.json");

        public static string CurrentLanguage { get; private set; } = LoadSavedLanguage();
        public static bool IsArabic => CurrentLanguage == "ar";

        private static readonly Dictionary<string, string> EnToAr = new(StringComparer.Ordinal)
        {
            // MainForm
            ["PDF Utility Tool"] = "أداة تعديل ملفات PDF",
            ["Select a PDF Tool you wish to use:"] = "اختر أداة PDF التي ترغب في استخدامها:",
            ["1. Merge PDF Files"] = "1. دمج ملفات PDF",
            ["2. Split or Reorder Pages"] = "2. تقسيم أو إعادة ترتيب الصفحات",
            ["3. Encrypt and Decrypt Options"] = "3. خيارات التشفير وفك التشفير",
            ["4. Rotate or Crop PDF Pages"] = "4. تدوير أو قص صفحات PDF",
            ["5. Reformat Multiple Pages"] = "5. إعادة تنسيق صفحات متعددة",
            ["6. Add Header and Footer"] = "6. إضافة رأس وتذييل الصفحة",
            ["7. Add Watermark by Text"] = "7. إضافة علامة مائية نصية",
            ["8. Add Watermark by Image"] = "8. إضافة علامة مائية كصورة",
            ["9. Convert Images to PDF"] = "9. تحويل الصور إلى PDF",
            ["10. Convert PDF to Images"] = "10. تحويل PDF إلى صور",
            ["11. PDF Form Field Operations"] = "11. عمليات حقول نماذج PDF",
            ["12. Add PDF Information"] = "12. تعديل معلومات ملف PDF",
            ["Exit"] = "خروج",

            // Common Buttons & Labels
            ["Save As ..."] = "حفظ باسم ...",
            ["Save As"] = "حفظ باسم",
            ["Save As Image ..."] = "حفظ كصورة ...",
            ["Close"] = "إغلاق",
            ["Cancel"] = "إلغاء",
            ["Move Up"] = "تحريك لأعلى",
            ["Move Down"] = "تحريك لأسفل",
            ["Remove"] = "إزالة",
            ["Sort"] = "فرز",
            ["Save"] = "حفظ",
            ["Change Color"] = "تغيير اللون",
            ["Left:"] = "يسار:",
            ["Right:"] = "يمين:",
            ["Top:"] = "أعلى:",
            ["Bottom:"] = "أسفل:",
            ["Width:"] = "العرض:",
            ["Height:"] = "الارتفاع:",
            ["Rows:"] = "الصفوف:",
            ["Columns:"] = "الأعمدة:",
            ["Portrait"] = "عمودي",
            ["Landscape"] = "أفقي",
            ["Order:"] = "الترتيب:",
            ["Background"] = "الخلفية",
            ["Foreground"] = "المقدمة",
            ["Font Encode:"] = "ترميز الخط:",
            ["Font Name:"] = "اسم الخط:",
            ["Font Style:"] = "نمط الخط:",
            ["Font Size:"] = "حجم الخط:",
            ["Font Color:"] = "لون الخط:",
            ["New PDF Page Size:"] = "حجم صفحة PDF الجديد:",
            ["Horizontal:"] = "أفقي:",
            ["Vertical:"] = "عمودي:",

            // 1. Merge PDF Files
            ["Merge PDF Files"] = "دمج ملفات PDF",
            ["File Name"] = "اسم الملف",
            ["Pages"] = "الصفحات",
            ["Path"] = "المسار",
            ["Add a PDF File"] = "إضافة ملف PDF",
            ["Add PDF Files under a Folder"] = "إضافة ملفات PDF من مجلد",
            ["You can also drag and drop PDFs\ninto this dialog."] = "يمكنك أيضاً سحب وإفلات ملفات PDF\nداخل هذه النافذة.",
            ["Add Bookmark using File Name (Professional User Only)"] = "إضافة إشارة مرجعية باسم الملف (للمستخدم المحترف فقط)",

            // 2. Split or Reorder Pages
            ["Split or Reorder Pages"] = "تقسيم أو إعادة ترتيب الصفحات",
            ["Split Pages From"] = "تقسيم الصفحات من",
            ["To"] = "إلى",
            ["Reorder Pages"] = "إعادة ترتيب الصفحات",
            ["Pages Example: 1, 3-5, even, odd, reverse, last"] = "مثال الصفحات: 1, 3-5, even, odd, reverse, last",
            ["Delete Pages"] = "حذف الصفحات",
            ["Pages Example: 1, 3-5, even, odd, last"] = "مثال الصفحات: 1, 3-5, even, odd, last",
            ["Keep Bookmarks"] = "الاحتفاظ بالإشارات المرجعية",
            ["Save the other pages as a PDF file"] = "حفظ الصفحات الأخرى كملف PDF",
            ["Extract Pages As Separate Files"] = "استخراج الصفحات كملفات منفصلة",
            ["Open the Extracted PDF"] = "فتح ملف PDF المستخرج",

            // 3. Encrypt and Decrypt
            ["Encrypt and Decrypt Options"] = "خيارات التشفير وفك التشفير",
            ["User Password"] = "كلمة مرور المستخدم",
            ["Owner Password"] = "كلمة مرور المالك",
            ["Encryption Level"] = "مستوى التشفير",
            ["40-bit RC4"] = "تشفير 40-bit RC4",
            ["128-bit RC4"] = "تشفير 128-bit RC4",
            ["Security Options"] = "خيارات الأمان والصلاحيات",
            ["Printing"] = "الطباعة",
            ["Modify Contents"] = "تعديل المحتوى",
            ["Copy"] = "النسخ",
            ["Modify Annotations"] = "تعديل التعليقات التوضيحية",
            ["Fill In"] = "تعبئة النماذج",
            ["Screen Readers"] = "قارئات الشاشة",
            ["Assembly"] = "تجميع المستند",
            ["Degraded Printing"] = "طباعة منخفضة الدقة",
            ["Encrypt As ..."] = "تشفير باسم ...",
            ["Decrypt As ..."] = "فك التشفير باسم ...",

            // 4. Rotate or Crop
            ["Rotate or Crop PDF Pages"] = "تدوير أو قص صفحات PDF",
            ["Select Page:"] = "اختر الصفحة:",
            ["Landscape Page only"] = "الصفحات الأفقية فقط",
            ["Portrait Page Only"] = "الصفحات العمودية فقط",
            ["All Pages"] = "جميع الصفحات",
            ["Odd Page"] = "الصفحات الفردية",
            ["Even Page"] = "الصفحات الزوجية",
            ["Rotate by"] = "تدوير بمقدار",
            ["Crop Box:"] = "مربع القص:",
            ["Left Margin:"] = "الهامش الأيسر:",
            ["Right Margin:"] = "الهامش الأيمن:",
            ["Bottom Margin:"] = "الهامش السفلي:",
            ["Top Margin:"] = "الهامش العلوي:",
            ["Page"] = "الصفحة",
            ["Orient"] = "الاتجاه",
            ["Rotation"] = "التدوير",
            ["Rotate By"] = "تدوير بمقدار",
            ["L Margin"] = "هامش يسار",
            ["R Margin"] = "هامش يمين",
            ["B Margin"] = "هامش أسفل",
            ["T Margin"] = "هامش أعلى",

            // 5. Reformat Multiple Pages
            ["Reformat Multiple Pages"] = "إعادة تنسيق صفحات متعددة",
            ["Original PDF Page Layout:"] = "تخطيط صفحات PDF الأصلية:",
            ["Margin from Original PDF Page to New PDF Page Edge:"] = "الهامش من صفحة PDF الأصلية إلى حافة الصفحة الجديدة:",
            ["Space between Original PDF Pages"] = "المسافة بين صفحات PDF الأصلية",
            ["Show Note Lines"] = "إظهار سطور الملاحظات",
            ["Show Original PDF Page's Border"] = "إظهار حدود صفحة PDF الأصلية",
            ["Line Color"] = "لون الخط",
            ["Line Width"] = "سُمك الخط",

            // 6. Add Header and Footer
            ["Add Header and Footer"] = "إضافة رأس وتذييل الصفحة",
            ["Add Header"] = "إضافة رأس الصفحة",
            ["Add Footer"] = "إضافة تذييل الصفحة",
            ["Header"] = "رأس الصفحة",
            ["Footer"] = "تذييل الصفحة",
            ["Add on First Page"] = "إضافة في الصفحة الأولى",
            ["Show Page Number"] = "إظهار رقم الصفحة",
            ["Text before Page Number:"] = "النص قبل رقم الصفحة:",
            ["Text after Number:"] = "النص بعد الرقم:",
            ["Alignment"] = "المحاذاة",
            ["Page Offset:"] = "إزاحة ترقيم الصفحات:",
            ["Text Properties:"] = "خصائص النص:",
            ["Margin From Edges:"] = "الهوامش من الحواف:",
            ["Resume to Last Options"] = "استعادة آخر الخيارات",

            // 7 & 8. Watermarks
            ["Add Watermark by Text"] = "إضافة علامة مائية نصية",
            ["String:"] = "النص:",
            ["Stroke Color:"] = "لون الحد الخارجي:",
            ["Layout:"] = "التخطيط:",
            ["Draw Type:"] = "نوع الرسم:",
            ["Stroke Size:"] = "حجم الحد:",
            ["percent of the actual font size"] = "بالمئة من حجم الخط الفعلي",
            ["Save Options (Professional User Only)"] = "حفظ الخيارات (للمستخدم المحترف فقط)",
            ["Name:"] = "الاسم:",
            ["Select String"] = "اختر النص المحفوظ",
            ["Add Watermark by Image"] = "إضافة علامة مائية كصورة",
            ["Rotate (Degree):"] = "التدوير (بالدرجات):",
            ["Position:"] = "الموضع:",
            ["Center of the page"] = "وسط الصفحة",
            ["Any Position:"] = "موضع مخصص:",
            ["URL"] = "رابط تشعبي",

            // 9 & 10. Images <-> PDF
            ["Convert Images to PDF"] = "تحويل الصور إلى PDF",
            ["Select images (Click the title to edit)"] = "اختر الصور (انقر على العنوان لتعديله)",
            ["You can drag and drop image files into this dialog."] = "يمكنك سحب وإفلات ملفات الصور داخل هذه النافذة.",
            ["Add an Image"] = "إضافة صورة",
            ["Add all Images under a Folder"] = "إضافة جميع الصور من مجلد",
            ["Title"] = "العنوان",
            ["Name"] = "الاسم",
            ["Size (point)"] = "الحجم (نقطة)",
            ["Show Title"] = "إظهار العنوان",
            ["Use Image Size to decide Page Size with DPI:"] = "استخدام حجم الصورة لتحديد حجم الصفحة بدقة DPI:",
            ["Image Layout:"] = "تخطيط الصور:",
            ["Margin from Image to New PDF Page Edge:"] = "الهامش من الصورة إلى حافة صفحة PDF الجديدة:",
            ["Space between Images:"] = "المسافة بين الصور:",
            ["Use Image Compress  (Professional User Only)"] = "ضغط الصور (للمستخدم المحترف فقط)",
            ["Convert PDF to Images"] = "تحويل PDF إلى صور",
            ["Select Page from"] = "اختر الصفحة من",
            ["Image Resolution (6-600 dpi):"] = "دقة الصورة (6-600 dpi):",
            ["Color Bits"] = "عمق الألوان (بت)",
            ["Page Number Digits:"] = "خانات رقم الصفحة:",
            ["Image Type"] = "نوع الصورة",
            ["JPG Quality"] = "جودة JPG",
            ["Single File with Multiple Pages"] = "ملف واحد متعدد الصفحات",

            // 11 & 12. Form Fields & PDF Info
            ["PDF Form Field Operations"] = "عمليات حقول نماذج PDF",
            ["Form Operation"] = "عملية النموذج",
            ["Delete all PDF Form Fields"] = "حذف جميع حقول نماذج PDF",
            ["Flatten all PDF Form Fields"] = "دمج وتثبيت جميع حقول نماذج PDF",
            ["List all PDF Form Fields's Name and Value"] = "استخراج أسماء وقيم جميع حقول PDF",
            ["PDF Editor for a Single Field"] = "تعديل حقل واحد في PDF",
            ["Value:"] = "القيمة:",
            ["Add PDF Information"] = "تعديل معلومات ملف PDF",
            ["Title:"] = "العنوان:",
            ["Author:"] = "المؤلف:",
            ["Subject:"] = "الموضوع:",
            ["Keywords:"] = "الكلمات المفتاحية:",
            ["Created"] = "تاريخ الإنشاء",
            ["Creator:"] = "البرنامج المنشئ:",
            ["Producer:"] = "البرنامج المُنتِج:",
            ["PDF Version:"] = "إصدار PDF:"
        };

        private static string LoadSavedLanguage()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var doc = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SettingsPath));
                    if (doc != null && doc.TryGetValue("language", out var lang) && (lang == "ar" || lang == "en"))
                        return lang;
                }
            }
            catch { }
            return "en";
        }

        public static void SetLanguage(string langCode)
        {
            CurrentLanguage = langCode == "ar" ? "ar" : "en";
            try
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, string> { ["language"] = CurrentLanguage });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        public static string Translate(string text)
        {
            if (!IsArabic || string.IsNullOrEmpty(text))
                return text;

            if (EnToAr.TryGetValue(text, out var translated))
                return translated;

            // Handle dynamic header: "There are X pages in filename.pdf"
            if (text.StartsWith("There are ") && text.Contains(" pages in "))
            {
                var parts = text.Substring("There are ".Length).Split(new[] { " pages in " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                    return $"يوجد {parts[0]} صفحات في الملف {parts[1]}";
            }

            return text;
        }

        /// <summary>
        /// Translates all controls and mirrors horizontal coordinates into RTL when Arabic is active.
        /// Call at the end of InitializeComponent() / InitializeLayout() on every Form.
        /// </summary>
        public static void Apply(Form form)
        {
            form.Text = Translate(form.Text);

            if (!IsArabic)
            {
                form.RightToLeft = RightToLeft.No;
                form.RightToLeftLayout = false;
                return;
            }

            form.RightToLeft = RightToLeft.Yes;
            form.RightToLeftLayout = true;

            ApplyToContainer(form, form.ClientSize.Width);
        }

        private static void ApplyToContainer(Control parent, int containerWidth)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // Keep the language toggle button text untouched ("English")
                if (ctrl.Name != "btnLanguageToggle")
                {
                    ctrl.Text = Translate(ctrl.Text);
                }

                // Translate ListView column headers
                if (ctrl is ListView lv)
                {
                    foreach (ColumnHeader col in lv.Columns)
                    {
                        col.Text = Translate(col.Text);
                    }
                }

                // Mirror X coordinate inside non-docked containers (Form & GroupBox)
                if (parent is Form || parent is GroupBox)
                {
                    if (ctrl.Dock == DockStyle.None)
                    {
                        ctrl.Left = Math.Max(6, containerWidth - ctrl.Right);
                    }
                }

                // Flip horizontal alignment on MainForm tool buttons
                if (ctrl is Button btn && btn.TextAlign == ContentAlignment.MiddleLeft)
                {
                    btn.TextAlign = ContentAlignment.MiddleRight;
                    btn.Padding = new Padding(0, 0, 10, 0);
                }

                if (ctrl.HasChildren)
                {
                    int childContainerWidth = ctrl is GroupBox ? ctrl.Width : ctrl.ClientSize.Width;
                    ApplyToContainer(ctrl, childContainerWidth);
                }
            }
        }
    }
}