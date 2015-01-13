namespace PartialDeployer
{
    using System;
    using System.IO;
    using System.Configuration;
    using System.Collections.Specialized;
    using log4net;

    public class configMan
    {
        private static readonly ILog log = LogManager.GetLogger("configMan");

        KeyValueConfigurationCollection _KeyValueConfigs;

        public configMan(string fileName = "")
        {
            if (File.Exists(fileName))
            {
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = fileName;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                _KeyValueConfigs = config.AppSettings.Settings;
            }
            else
            {
                log.Info("Config File not found");
                Console.WriteLine("Quitting...");
                Environment.Exit(1);
            }
        }

        public string GetString(string KeyName)
        {
            try
            {
                return _KeyValueConfigs[KeyName].Value.ToString();
            }
            catch(Exception ex)
            {
                log.ErrorFormat("{0} - Keyname : {1}", ex.Message, KeyName);
                return string.Empty;
            }
        }

        public int GetInt(string KeyName)
        {
            try
            {
                return int.Parse(_KeyValueConfigs["Server"].Value);
            }
            catch(Exception ex)
            {
                log.ErrorFormat("{0} - Keyname : {1}", ex.Message, KeyName);
                return -1;
            }
        }

        public string ReleaseNameTemplate
        {
            get
            {
                return GetString("ReleaseNameTemplate");
            }
        }

        public string APP_Enviro
        {
            get
            {
                return GetString("APP_Enviro");
            }
        }

        public string LOG_Filename
        {
            get
            {
                return GetString("LOG_Filename");
            }
        }

        public string DIR_Source
        {
            get
            {
                return GetString("DIR_Source");
            }
        }
        public string DIR_Product
        {
            get
            {
                return GetString("DIR_Product");
            }
        }
        public string DIR_Deploy
        {
            get
            {
                return GetString("DIR_Deploy");
            }
        }
        public string DIR_Release
        {
            get
            {
                return GetString("DIR_Release");
            }
        }
        public string DIR_Backup
        {
            get
            {
                return GetString("DIR_Backup");
            }
        }

        public string URL_Backup
        {
            get
            {
                return GetString("URL_Backup");
            }
        }
        public string URL_SqlBackup
        {
            get
            {
                return GetString("URL_SqlBackup");
            }
        }

        public string FTP_Server
        {
            get
            {
                return GetString("FTP_Server");
            }
        }
        public string FTP_UserName
        {
            get
            {
                return GetString("FTP_UserName");
            }
        }
        public string FTP_PassWord
        {
            get
            {
                return GetString("FTP_PassWord");
            }
        }
        public string FTP_Folder
        {
            get
            {
                return GetString("FTP_Folder");
            }
        }
    }
}
