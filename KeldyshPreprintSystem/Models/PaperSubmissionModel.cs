using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KeldyshPreprintSystem.Models
{
    public enum FieldsOfResearch
    {
        Unselected,
        MathModelling,// "Математическое моделирование в актуальных проблемах науки и техники",
        MathProblems,// "Математические вопросы и теория численных методов",
        MechTasks,// "Теоретические и прикладные задачи механики",
        Programming// "Программирование, параллельные вычисления, мультимедиа"
    }

    public static class FieldOfResearchConverter
    {
        public static string[] RussianOriginal = { "Математическое моделирование в актуальных проблемах науки и техники", "Математические вопросы и теория численных методов", "Теоретические и прикладные задачи механики", "Программирование, параллельные вычисления, мультимедиа" };
        public static string GetRussian(FieldsOfResearch fos)
        {
            switch (fos)
            {
                case FieldsOfResearch.MathModelling:
                    return "Математическое моделирование в актуальных проблемах науки и техники";
                case FieldsOfResearch.MathProblems:
                    return "Математические вопросы и теория численных методов";
                case FieldsOfResearch.MechTasks:
                    return "Теоретические и прикладные задачи механики";
                case FieldsOfResearch.Programming:
                    return "Программирование, параллельные вычисления, мультимедиа";
                default://never happen
                    throw new ArgumentException("Wrong argument", "fos");
            }
        }

        public static FieldsOfResearch GetFieldOfResearch(string fos)
        {
            switch (fos)
            {
                case "Математическое моделирование в актуальных проблемах науки и техники":
                    return FieldsOfResearch.MathModelling;
                case "Математические вопросы и теория численных методов":
                    return FieldsOfResearch.MathProblems;
                case "Теоретические и прикладные задачи механики":
                    return FieldsOfResearch.MechTasks;
                case "Программирование, параллельные вычисления, мультимедиа":
                    return FieldsOfResearch.Programming;
                default://could happen very likely
                    throw new ArgumentException("Wrong argument", "fos");
            }
        }
    }

    public class Author
    {
        [Key]
        [System.Xml.Serialization.XmlIgnore]
        public int Id { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public int PaperSubmissionModelId { get; set; }
        //[Required]
        //[ForeignKey("PaperSubmissionModelId")]
        [System.Xml.Serialization.XmlIgnore]
        public virtual PaperSubmissionModel PaperSubmissionModel { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя автора на русском языке.")]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Имя должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string FirstnameRussian { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию автора на русском языке.")]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Фамилия должна начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string LastnameRussian { get; set; }

        //[Required]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Отчество должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string PatronymRussian { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя автора на английском языке.")]
        [RegularExpression(@"[A-Z][a-zA-Z\—\-\-\.\s]*", ErrorMessage = "Имя должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв английского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string FirstnameEnglish { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию автора на английском языке.")]
        [RegularExpression(@"[A-Z][a-zA-Z\—\-\-\.\s]*", ErrorMessage = "Фамилия должна начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв английского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string LastnameEnglish { get; set; }

        //[Required]
        [RegularExpression(@"[A-Z][a-zA-Z\—\-\-\.\s]*", ErrorMessage = "Отчество должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв английского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string PatronymEnglish { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите email адрес для каждого автора.")]
        [Email(ErrorMessage = "Адрес электронной почты не должен содержать пробелов, а имя пользователя почты и название домена должны быть разделены символов \"@\", например, test@test.ru. Пожалуйста, введите корректный email адрес.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Адрес электронной почты не должен содержать пробелов, а имя пользователя почты и название домена должны быть разделены символов \"@\", например, test@test.ru. Пожалуйста, введите корректный email адрес.")]
        // [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Пожалуйста, введите корректный email адрес.")]
        public string Email { get; set; }

        [DataType(DataType.Url, ErrorMessage = "Адрес персональной страницы должен начинаться с указания протокола (например http://) и не должен содержать пробелов. Пожалуйста, введите корректный адрес персональной страницы.")]
        [Url(UrlOptions.RequireProtocol, ErrorMessage = "Адрес персональной страницы должен начинаться с указания протокола (например http://) и не должен содержать пробелов. Пожалуйста, введите корректный адрес персональной страницы.")]
        public string PersonalWeb { get; set; }

        public string PlaceOfWork { get; set; }
        public string SPIN { get; set; }
        public string ResearcherID { get; set; }
        public string ORCID { get; set; }
    }

    public class Reviewer
    {
        [Key]
        [System.Xml.Serialization.XmlIgnore]
        public int Id { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public int PaperSubmissionModelId { get; set; }
        //[Required]
        //[ForeignKey("PaperSubmissionModelId")]
        [System.Xml.Serialization.XmlIgnore]
        public virtual PaperSubmissionModel PaperSubmissionModel { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя рецензента.")]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Имя рецензента должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию рецензента.")]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Фамилия рецензента должна начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string Lastname { get; set; }

        //[Required]
        [RegularExpression(@"[А-Я][\p{IsCyrillic}\—\-\-\.\s]*", ErrorMessage = "Отчество рецензента должно начинаться с заглавной буквы и не содержать недопустимых символов.\r\nПожалуйста, используйте только символы букв русского алфавита, пробелы, знаки точка, тире и дефис.")]
        public string Patronym { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите должность рецензента.")]
        public string JobTitle { get; set; }
        [Required(ErrorMessage = "Пожалуйста, введите ученую степень рецензента.")]
        public string Degree { get; set; }
    }

    public class PaperSubmissionModel
    {
        [Key]
        [System.Xml.Serialization.XmlIgnore]
        public int Id { get; set; }

        public string authorsIndex { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите хотя бы одного автора.")]
        [System.Xml.Serialization.XmlIgnore]
        public virtual ICollection<Author> Authors { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите информацию о рецензенте.")]
        public virtual Reviewer Reviewer { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите название препринта на русском языке.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Название препринта на русском языке слишком длинное (более 200 символов) или короткое (менее 3 символов). ")]
        public string TitleRussian { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите название препринта на английском языке.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Название препринта на английском языке слишком длинное (более 200 символов) или слишком короткое (менее 3 символов).")]
        public string TitleEnglish { get; set; }


        [Required(ErrorMessage = "Пожалуйста, введите аннотацию препринта на русском языке.")]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        //[StringLength(500, MinimumLength = 3, ErrorMessage = "Аннотация на русском языке слишком длинная или короткая")]
        public string AbstractRussian { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите аннотацию препринта на английском языке.")]
        //[StringLength(500, MinimumLength = 3, ErrorMessage = "Аннотация на английском языке слишком длинная или короткая")]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string AbstractEnglish { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите ключевые слова препринта на русском языке.")]
        //[RegularExpression(@"[^a-zA-Z]*", ErrorMessage = "Ключевые слова на русском языке содержат недопустимые символы")]
        //[StringLength(500, MinimumLength = 3, ErrorMessage = "Не указаны ключевые слова на русском языке")]
        public string KeywordsRussian { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите ключевые слова препринта на английском языке.")]
        //[RegularExpression(@"[^а-яА-я]*", ErrorMessage = "Ключевые слова на английском языке содержат недопустимые символы")]
        //[StringLength(500, MinimumLength = 3, ErrorMessage = "Не указаны ключевые слова на английском языке")]
        public string KeywordsEnglish { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите язык(языки) публикации.")]
        public string Languages { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите число страниц публикации.")]
        [Range(3, 200, ErrorMessage = "Пожалуйста, проверьте введенное число страниц в препринте.")]//может больше
        [Integer(ErrorMessage = "Пожалуйста, укажите число страниц публикации.")]
        public int NumberOfPages { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите число желаемых авторских копий препринта.")]
        [Range(0, 1000, ErrorMessage = "Проверьте введенное число желаемых авторских экземпляров препринта.")]
        [Integer(ErrorMessage = "Пожалуйста, введите число желаемых авторских копий препринта.")]
        public int NumberOfAuthorsCopies { get; set; }

        [RequiredFOS(ErrorMessage = "Пожалуйста, выберите рубрику.")]
        public FieldsOfResearch FieldOfResearch { get; set; }

        [RegularExpression(@"[0-9,.+:\[\]\(\)]*", ErrorMessage = "УДК содержит недопустимые символы.")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Введенное УДК слишком длинное. Проверьте правильность введеного УДК.")]
        public string UDK { get; set; }

        [Required(ErrorMessage = "Пожалуйста, скопируйте список литературы вашей публикации и вставьте в соответствующее поле.")]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Список литературы слишком мал или не указан.")]
        public string Bibliography { get; set; }

        public string FinancialSupport { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя автора, уполномоченного для контактов с редакцией.")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите контактный номер телефона.")]
        [DataType(DataType.PhoneNumber)]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Пожалуйста, заполните поле рецензии (желательно не более 2000 символов).")]
        //[UIHint("tinymce_full_review")]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [StringLength(3000, ErrorMessage = "Длина рецензии не должна превышать 3000 символов вместе с пробелами.")]
        public string Review { get; set; }

        [NotMapped]
        [ValidateFile(ErrorMessage = "Пожалуйста, приложите файл препринта.")]
        [FileExtensions("doc,docx,pdf", ErrorMessage = "Недопустимое расширение файла. Допустимые расширения: .doc, .docx и .pdf.")]
        [System.Xml.Serialization.XmlIgnore]
        public HttpPostedFileBase Attachment { get; set; }

        /// <summary>
        /// </summary>
        public int submissionState { get; set; }

        public string submissionDate { get; set; }

        public string lastStatusChangeDate { get; set; }

        public string Owner { get; set; }
    }

    public class FakePreprintAttachment : HttpPostedFileBase
    {
        Stream stream = null;
        string contentType;
        string fileName;
        int contentLength;
        byte[] fileData = null;
        public FakePreprintAttachment(string path)
        {
            //this.stream = File.Open(path, FileMode.Open);
            switch (Path.GetExtension(path))
            {
                case "pdf":
                    this.contentType = "application/pdf";
                    break;
                case "doc":
                    this.contentType = "application/msword";
                    break;
                case "docx":
                    this.contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
            }
            this.fileName = Path.GetFileName(path);
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                contentLength = fileData.Length;
            }
        }

        //~FakePreprintAttachment()
        //{
        //    //stream.Close();
        //}

        public override int ContentLength
        {
            get { return contentLength; }
        }

        public override string ContentType
        {
            get { return contentType; }
        }

        public override string FileName
        {
            get { return fileName; }
        }

        public override Stream InputStream
        {
            get { return stream; }
        }

        public override void SaveAs(string filename)
        {
            if (fileData != null)
            {
                File.WriteAllBytes(filename, fileData);
            }
            throw new NotImplementedException();
        }
    }

    public class ValidateFileAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var f = value as FakePreprintAttachment;
            if (f != null)
                return true;
            var file = value as HttpPostedFileBase;
            if (file == null)
            {
                return false;
            }

            if (file.ContentLength > 1)
            {
                return true;
            }

            return false;
        }
    }

    public class RequiredFOSAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var fos = (FieldsOfResearch)value;
            if (fos == FieldsOfResearch.Unselected)
            {
                return false;
            }

            return true;
        }
    }
}