using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeldyshPreprintSystem.Tools
{
    public static class EditorHelper
    {
        public static bool IsSubmissionClosed(int status)
        {
            if (status < 15)
                return false;
            else
                return true;
        }

        public static Dictionary<int, string> StatusActionDictionary
        {
            get
            {
                Dictionary<int, string> dict = new Dictionary<int, string>();
                dict.Add(1, "PaperSubmissionController.Success");
                dict.Add(2, "ClarifyContent");
                dict.Add(3, "ContentClarified");
                dict.Add(4, "ContentPassedToCorrector");
                dict.Add(5, "ContentCorrectionOver");
                dict.Add(6, "ContentCorrectionPassed");
                dict.Add(7, "MarkupAuthorFinishing");
                dict.Add(8, "MarkupFinishingOver");
                dict.Add(9, "SignalPermitted");
                dict.Add(10, "SignalReady");
                dict.Add(11, "SignalCorrected");
                dict.Add(12, "PrintingPermitted");
                dict.Add(13, "PrintingProcessing");
                dict.Add(14, "WebSiteReady");
                dict.Add(15, "ELibraryReady");
                dict.Add(16, "ExtraTasks");
                dict.Add(17, "WebSiteReplacement");
                dict.Add(18, "WebSiteReplacementPermitted");
                dict.Add(19, "WebSiteReplacementDone");
                dict.Add(20, "ExtraPrinting");
                dict.Add(21, "ExtraPrintingPermitted");
                dict.Add(22, "ExtraPrintingDone");
                return dict;
            }
        }


        public static string[] GetPossibleActions(int status)//role:action;
        {
            switch (status)
            {
                case 1:
                    return new string[] { "admin:ClarifyContent:Уточнить", "admin:ContentPassedToCorrector:Корректору", "admin:MarkupAuthorFinishing:Доверстка", "admin:SignalPermitted:Сигнал" };
                case 2:
                    return new string[] { "user:ContentClarified:Готово" };
                case 3:
                    return new string[] { "admin:ClarifyContent:Уточнить", "admin:ContentPassedToCorrector:Корректору", "admin:MarkupAuthorFinishing:Доверстка", "admin:SignalPermitted:Сигнал" };
                case 4:
                    return new string[] { "corrector:ContentCorrectionOver:Готово (корректор)" };
                case 5:
                    return new string[] { "user:ContentCorrectionPassed:Готово (автор)" };
                case 6:
                    return new string[] { "admin:MarkupAuthorFinishing:Доверстка", "admin:SignalPermitted:Сигнал" };
                case 7:
                    return new string[] { "user:MarkupFinishingOver:Готово (автор)" };
                case 8:
                    return new string[] { "admin:MarkupAuthorFinishing:Доверстка", "admin:SignalPermitted:Разрешить сигнал" };
                case 9:
                    return new string[] { "typographer:SignalReady:Сигнал готов" };
                case 10:
                    return new string[] { "user:ContentPassedToCorrector:Корректору", "user:SignalPermitted:Допсигнал", "user:PrintingPermitted:Тираж", "user:SignalCorrected:Сигнал скорректирован" };
                case 11:
                    return new string[] { "admin:PrintingPermitted:Тираж", "admin:SignalPermitted:Допсигнал" };
                case 12:
                    return new string[] { "typographer:PrintingProcessing:Готово (тираж)" };
                case 13:
                    return new string[] { "internetguy:WebSiteReady:Готово (сайт)" };//typographer:WebSiteReady:Готово (тираж)
                case 14:
                    return new string[] { "internetguy:ELibraryReady:Готово (e-library)" };
                case 15:
                    return new string[] { "admin:ExtraTasks:Препринт опубликован" };
                case 16:
                    return new string[] { "user:WebSiteReplacement:Файл изменен + Требуется замена на сайте", "user:ExtraPrinting:Дополнительный тираж"};
                case 17:
                    return new string[] { "admin:WebSiteReplacementPermitted:Заменить на сайте" };
                case 18:
                    return new string[] { "internetguy:WebSiteReplacementDone:Готово (замена на сайте)" };
                case 19:
                    return new string[] { "internetguy:FOO:BAR" };
                case 20:
                    return new string[] { "admin:ExtraPrintingPermitted:Доптираж" };
                case 21:
                    return new string[] { "typographer:ExtraPrintingDone:Готово (доптираж)" };
                case 22:
                    return new string[] { "typographer:FOO:BAR" };
                default:
                    return new string[] { "WRONGSTATUSCODE" };
            }
        }

        public static string GetCorrespondingAction(int status)
        {
            switch (status)
            {
                case 2:
                    return "ClarifyContent";
                case 3:
                    return "ContentClarified";
                case 4:
                    return "ContentPassedToCorrector";
                case 5:
                    return "ContentCorrectionOver";
                case 6:
                    return "ContentCorrectionPassed";
                case 7:
                    return "MarkupAuthorFinishing";
                case 8:
                    return "MarkupFinishingOver";
                case 9:
                    return "SignalPermitted";
                case 10:
                    return "SignalReady";
                case 11:
                    return "SignalCorrected";
                case 12:
                    return "PrintingPermitted";
                case 13:
                    return "PrintingProcessing";
                case 14:
                    return "WebSiteReady";
                case 15:
                    return "ELibraryReady";
                case 16:
                    return "ExtraTasks";
                case 17:
                    return "WebSiteReplacement";
                case 18:
                    return "WebSiteReplacementPermitted";
                case 19:
                    return "WebSiteReplacementDone";
                case 20:
                    return "ExtraPrinting";
                case 21:
                    return "ExtraPrintingPermitted";
                case 22:
                    return "ExtraPrintingDone";
                default:
                    return string.Empty;
            }
        }

        public static string GetStatusDescription(int status)
        {
            switch (status)
            {
                case 0:
                    return "Заявка подана, ожидается отправка уведомлений";
                case 1:
                    return "Получены заявка и оригинал-макет";
                case 2:
                    return "Материалы на уточнении у автора";
                case 3:
                    return "Получены уточненные материалы";
                case 4:
                    return "Материалы у корректора";
                case 5:
                    return "Завершена корректура";
                case 6:
                    return "Получены скорректированные материалы";
                case 7:
                    return "Материалы на авторской доверстке";
                case 8:
                    return "Завершена доверстка";
                case 9:
                    return "Файлы проверены, разрешен сигнал";
                case 10:
                    return "Напечатан сигнал";
                case 11:
                    return "Файл сигнала скорректирован";
                case 12:
                    return "Разрешен тираж";
                case 13:
                    return "Выпущен тираж";
                case 14:
                    return "Размещено на сайте";
                case 15:
                    return "Размещено в eLibrary";
                case 16:
                    return "Дополнительные задания";
                case 17:
                    return "Замена на сайте";
                case 18:
                    return "Санкционирована замена на сайте";
                case 19:
                    return "Произведена замена на сайте";
                case 20:
                    return "Дополнительный тираж";
                case 21:
                    return "Санкционирован дополнительный тираж";
                case 22:
                    return "Изготовлен дополнительный тираж";
                default:
                    return "Неизвестный код статуса";
            }
        }

        public static bool IsMessageNeeded(string actionName)
        {
            List<string> messageNeeded = new List<string>(){ "ClarifyContent", "ContentPassedToCorrector", "ContentClarified", "ContentCorrectionOver"};
            if (messageNeeded.IndexOf(actionName) > -1)
                return true;
            else
                return false;
        }

        public static bool IsFileNeeded(string actionName)
        {
            List<string> messageNeeded = new List<string>() { "ContentCorrectionOver" };
            if (messageNeeded.IndexOf(actionName) > -1)
                return true;
            else
                return false;
        }
    }
}