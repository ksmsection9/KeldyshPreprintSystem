using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
namespace KeldyshPreprintSystem.Tools
{
    public static class ConfigurationResourcesHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Dictionary<string, Dictionary<string, string>> resources = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> translation = new Dictionary<string,string>();
        static object lockTrigger = new Object();
        public static System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<string, Dictionary<string, string>>> Resources
        {
            get
            {
                LoadResources();
                return new System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<string, Dictionary<string, string>>>(resources.ToList());
            }
        }

        private static void LoadResources()
        {
            if (resources.Count > 0)
                return;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("~/Content/resources.xml"));
                foreach (XmlNode child in doc.FirstChild.NextSibling.ChildNodes)
                {
                    string attrVal = child.Attributes["translation"].Value;
                    translation.Add(child.Name.Trim(), attrVal);
                    var dic = new Dictionary<string, string>();
                    foreach (XmlNode values in child.ChildNodes)
                    {
                        dic.Add(values.Name.Trim(), values.InnerText);
                        if(!translation.ContainsKey(values.Name.Trim()))
                            translation.Add(values.Name.Trim(), values.Attributes["translation"].Value);
                    }
                    resources.Add(child.Name, dic);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public static string GetResourceKeyTranslation(string key)
        {
            return translation[key];
        }

        public static string GetResource(string area, string key)
        {
            LoadResources();
            try
            {
                return resources[area][key].Trim();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                foreach (string s in resources.Keys)
                    logger.Error("Key: " + s);
                return String.Empty;
            }
        }

        public static string SetResourceVal(string area, string key, string value)
        {
            lock (lockTrigger)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath("~/Content/resources.xml"));
                foreach (XmlNode node in doc.GetElementsByTagName(key))
                    if (node.ParentNode.Name == area)
                        node.InnerText = value;
                doc.Save(System.Web.HttpContext.Current.Server.MapPath("~/Content/resources.xml"));
                return "success";
            }
        }

        public static string GetResourceVal(string area, string key, Models.PaperSubmissionModel paper)
        {
            if (paper == null)
                throw new ArgumentNullException("paper");
            LoadResources();
            try
            {
                string allAuthors = string.Empty;
                string firstAuthor = string.Empty;
                foreach(var author in paper.Authors)
                {
                    string name = author.FirstnameRussian.FirstOrDefault() + "." + (author.PatronymRussian != null ? author.PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + author.LastnameRussian;
                    allAuthors += name;
                    if (string.IsNullOrEmpty(firstAuthor) && paper.Authors.First().Id == author.Id)
                        firstAuthor = name;
                    if (paper.Authors.Last().Id != author.Id)
                    {
                        allAuthors += ", ";
                    }
                }     
                return (resources[area][key]).Replace("@contactname@", paper.ContactName).Replace("@title@", paper.TitleRussian).Replace("@id@", paper.Id.ToString()).Replace("@firstauthor@", firstAuthor).Replace("@allauthors@", allAuthors).Trim();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                foreach (string s in resources.Keys)
                    logger.Error("Key: " + s);
                return String.Empty;
            }
        }

        public static string GetResourceVal(string area, string key, Models.PaperSubmissionModel paper, string[] editor)
        {
            if (paper == null)
                throw new ArgumentNullException("paper");
            if (editor == null)
                throw new ArgumentNullException("editor");

            LoadResources();
            try
            {
                string allAuthors = string.Empty;
                string firstAuthor = string.Empty;
                foreach (var author in paper.Authors)
                {
                    string name = author.FirstnameRussian.FirstOrDefault() + "." + (author.PatronymRussian != null ? author.PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + author.LastnameRussian;
                    allAuthors += name;
                    if (string.IsNullOrEmpty(firstAuthor) && paper.Authors.First().Id == author.Id)
                        firstAuthor = name;
                    if (paper.Authors.Last().Id != author.Id)
                    {
                        allAuthors += ", ";
                    }
                }
                return (resources[area][key]).Replace("@contactname@", paper.ContactName).Replace("@title@", paper.TitleRussian).Replace("@id@", paper.Id.ToString()).Replace("@firstauthor@", firstAuthor).Replace("@allauthors@", allAuthors).Replace("@editorialboardmembershortname@", editor[1]).Replace("@editorialboardmembergreeting@", editor[2]).Trim();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                foreach (string s in resources.Keys)
                    logger.Error("Key: " + s);
                return String.Empty;
            }
        }

        public static string GetStringWithSubstitutions(string text, Models.PaperSubmissionModel paper, Models.Author author)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (paper == null)
                throw new ArgumentNullException("paper");
            if (author == null)
                throw new ArgumentNullException("author");

            string allAuthors = string.Empty;
            string firstAuthor = string.Empty;
            foreach (var a in paper.Authors)
            {
                string name = a.FirstnameRussian.FirstOrDefault() + "." + (a.PatronymRussian != null ? a.PatronymRussian.FirstOrDefault() + "." : string.Empty) + " " + a.LastnameRussian;
                allAuthors += name;
                if (string.IsNullOrEmpty(firstAuthor) && paper.Authors.First().Id == a.Id)
                    firstAuthor = name;
                if (paper.Authors.Last().Id != a.Id)
                {
                    allAuthors += ", ";
                }
            }

            return text.Replace("@firstname@", author.FirstnameRussian).Replace("@patronymic@", author.PatronymRussian).Replace("@lastname@", author.LastnameRussian).Trim().Replace("@contactname@", paper.ContactName).Replace("@title@", paper.TitleRussian).Replace("@id@", paper.Id.ToString()).Replace("@firstauthor@", firstAuthor).Replace("@allauthors@", allAuthors).Trim();

        }

        public static string GetResourceVal(string area, string key, Models.Author author)
        {
            if (author == null)
                throw new ArgumentNullException("author");
            LoadResources();
            try
            {
                return (resources[area][key]).Replace("@firstname@", author.FirstnameRussian).Replace("@patronymic@", author.PatronymRussian).Replace("@lastname@", author.LastnameRussian).Trim();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                foreach (string s in resources.Keys)
                    logger.Error("Key: " + s);
                return String.Empty;
            }
        }

    //    public static List<string[]> GetEditorialMail()
    //    {
    //        string raw = GetResource("mailinglist", "EditorialBoard");
    //        string[] rawArray = raw.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
    //        List<string[]> result = new List<string[]>();
    //        foreach (string s in rawArray)
    //        {
    //            try
    //            {
    //                string[] entry = { s.Split(',')[0].Trim(), s.Split(',')[1].Trim(), s.Split(',')[2].Trim() };
    //                result.Add(entry);
    //            }
    //            catch (IndexOutOfRangeException) { continue; }
    //        }
    //        return result;
    //    }
    }
}