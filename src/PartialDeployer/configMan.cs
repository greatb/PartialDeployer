namespace PartialDeployer
{
    using System;
    using System.IO;
    using System.Configuration;
    using System.Collections.Specialized;

    public class configMan
    {
        KeyValueConfigurationCollection _KeyValueConfigs;

        public configMan(string fileName)
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
                Console.WriteLine("Config File not found");
                Console.WriteLine("Quitting...");
                Environment.Exit(1);
            }
        }

        public string getString(string KeyName)
        {
            return _KeyValueConfigs["Server"].Value.ToString();
        }

        public int GetInt(string KeyName)
        {
            return int.Parse(_KeyValueConfigs["Server"].Value);
        }

        public string ReleaseNameTemplate
        {
            get
            {
                return getString("ReleaseNameTemplate");
            }
        }

        public string APP_Enviro
        {
            get
            {
                return getString("APP_Enviro");
            }
        }

        public string LOG_Filename
        {
            get
            {
                return getString("LOG_Filename");
            }
        }

        public string DIR_Source
        {
            get
            {
                return getString("DIR_Source");
            }
        }
        public string DIR_Product
        {
            get
            {
                return getString("DIR_Product");
            }
        }
        public string DIR_Deploy
        {
            get
            {
                return getString("DIR_Deploy");
            }
        }
        public string DIR_Release
        {
            get
            {
                return getString("DIR_Release");
            }
        }
        public string DIR_Backup
        {
            get
            {
                return getString("DIR_Backup");
            }
        }

        public string URL_Backup
        {
            get
            {
                return getString("URL_Backup");
            }
        }
        public string URL_SqlBackup
        {
            get
            {
                return getString("URL_SqlBackup");
            }
        }

        public string FTP_Server
        {
            get
            {
                return getString("FTP_Server");
            }
        }
        public string FTP_UserName
        {
            get
            {
                return getString("FTP_UserName");
            }
        }
        public string FTP_PassWord
        {
            get
            {
                return getString("FTP_PassWord");
            }
        }
        public string FTP_Folder
        {
            get
            {
                return getString("FTP_Folder");
            }
        }
    }
}
