using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WindowsShutdown
{
    static class XmlHelper
    {
        /// <summary>
        /// searches an element with the specified name, starts looking in parent, looks also in childelements 
        /// </summary>
        /// <param name="parent">the item where the search is initiated</param>
        /// <param name="ElementName">the name of the element (e.g.: MessageGroup)</param>
        /// <returns>null if there was no element found</returns>
        public static XElement GetElement(this XElement parent, string ElementName)
        {
            // check if the parent is the element to look for
            if (parent.Name == ElementName)
            {
                return parent;
            }
            // if not, check all its child elements
            if (parent.HasElements)
            {
                foreach (XElement child in parent.Elements())
                {
                    XElement element = GetElement(child, ElementName);
                    if (element != null)
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        public static string GetElementValueInRoot(this XDocument doc, string elementName)
        {
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == elementName)
                    return element.Value;
            }

            return string.Empty;
        }             

        public static ViewModel ReadConfig(string path)
        {
            var doc = XDocument.Load(path);

            ViewModel vm = new ViewModel();
            vm.ShutdownMode = (WindowsShutdownMode)(Enum.Parse(typeof(WindowsShutdownMode),doc.GetElementValueInRoot("Mode")));
            var t = doc.GetElementValueInRoot("ShutdownTimer").Split(new char[] {':'});
            int i = 0;
            foreach (var s in t)
            {
                vm.Timer[i] = s;
                i++;
            }            
            try
            {
                vm.ShutdownDate = DateTime.Parse(doc.GetElementValueInRoot("ShutdownTime"));
                vm.Date[0] = vm.ShutdownDate.ToString("HH");
                vm.Date[1] = vm.ShutdownDate.ToString("mm");
                vm.Date[2] = vm.ShutdownDate.ToString("ss");
            }
            catch (Exception ex)
            {                
                vm.ShutdownDate = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            }
            vm.Dayly = Convert.ToBoolean(doc.GetElementValueInRoot("Dayly"));
            vm.TimeRemainingVisibility = System.Windows.Visibility.Hidden;

            return vm;
        }

        public static void SaveConfig(ViewModel vm, string path)
        {
            var doc = new XDocument();
            doc.Add(new XElement("Config"));
            doc.Root.Add(new XElement("Mode",Enum.GetName(typeof(WindowsShutdownMode),vm.ShutdownMode)));
            doc.Root.Add(new XElement("ShutdownTimer",$"{vm.Timer[0]}:{vm.Timer[1]}:{vm.Timer[2]}"));
            vm.ShutdownDate = vm.ShutdownDate.Date.AddHours(Convert.ToDouble(vm.Date[0]))
                .AddMinutes(Convert.ToDouble(vm.Date[1]))
                .AddSeconds(Convert.ToDouble(vm.Date[2]));
            doc.Root.Add(new XElement("ShutdownTime", vm.ShutdownDate.ToString()));
            doc.Root.Add(new XElement("Dayly", vm.Dayly.ToString()));
            doc.Save(path);
        }
    }
}
