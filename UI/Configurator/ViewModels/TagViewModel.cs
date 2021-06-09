using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Configurator.Data;
using Configurator.Model;

namespace Configurator.ViewModels
{
    public class TagViewModel
    {
        public void SaveMessageToDrafts(string text, string subjects, object toEditValue, object fromEditValue)
        {
            Tag currentTag = new Tag();
            //currentTag.MailType = MailType.Draft;
            //SetMessageData(currentTag, text, subjects, toEditValue, fromEditValue);
            DeviceDataModel.Tags.Add(currentTag);
        }
        void SetTagData(Tag currentTag, string text, string subject, object toEditValue, object fromEditValue)
        {
            //currentTag.Date = DateTime.Now;
            //currentTag.Text = text;
            //currentTag.SetPlainText(ObjectHelper.GetPlainTextFromMHT(text));
            //string subj = subject;
            //if (string.IsNullOrEmpty(subject) || subject == " Subject:")
            //{
            //    subj = "Subject";
            //}
            //currentTag.Subject = subj;
            //currentTag.MailType = MailType.Draft;
            //IList<object> toContacts = toEditValue as IList<object>;
            //if (toContacts != null && toContacts.Count == 1)
            //    currentTag.Email = toContacts.First().ToString();
            //IList<object> fromContacts = fromEditValue as IList<object>;
            //if (fromContacts != null && fromContacts.Count == 1)
            //    currentTag.From = fromContacts.First().ToString();
            //else currentTag.From = Utils.Utils.MessageFrom;
        }
    }
}
