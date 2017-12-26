namespace WHICHEATSERVER.Core.Tools
{
    using System.Reflection;
    using Configuration;
    using System;


    public class SystemConfigLoader
    {
        private static string path = AppDomain.CurrentDomain.BaseDirectory + "config.ini";
        private static SystemConfigModel config = null;

        public static SystemConfigModel Current
        {
            get
            {
                if (config == null)
                    config = GetConfig();
                return config;
            }
        }


        private static SystemConfigModel GetConfig()
        {
            SystemConfigModel result = new SystemConfigModel();
            INIDocument doc = new INIDocument(path);
            doc.Load();
            result = new SystemConfigLoader().GetValue(doc);
            return result;
        }

        private SystemConfigModel GetValue(INIDocument doc)
        {
            SystemConfigModel config = new SystemConfigModel();
            doc.Load();
            Type clazz = config.GetType();
            foreach (INISection section in doc.Sections)
            {
                foreach (INIKey key in section.Keys)
                {
                    PropertyInfo pi = clazz.GetProperty(key.Name);
                    object value = key.Value;
                    if (pi.PropertyType == typeof(int))
                        value = Convert.ToInt32(value);
                    if (pi != null && !pi.PropertyType.IsGenericType)
                    {
                        pi.SetValue(config, value, null);
                    }
                }
            }
            return config;
        }
    }
}
