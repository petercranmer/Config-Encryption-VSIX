using System;
using System.Configuration;

namespace ConfigSectionDecryptor.Services
{
    public class ConfigurationEncryptionService
    {
        public bool DecryptConfigSection(string filename, string configSection)
        {
            var configFile = this.GetConfigFile(filename);

            var section = configFile.GetSection(configSection) as ConfigurationSection;

            if (section.SectionInformation.IsProtected)
            {
                // Remove encryption.
                section.SectionInformation.UnprotectSection();
            }

            configFile.Save();

            return true;
        }

        public bool EncryptConfigSection(string filename, string configSection)
        {
            var configFile = this.GetConfigFile(filename);

            var protectionProvider = this.GetProtectionProvider(configFile);

            var section = configFile.GetSection(configSection) as ConfigurationSection;

            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection(protectionProvider);
            }

            configFile.Save();

            return true;
        }

        private string GetProtectionProvider(Configuration configFile)
        {
            var protectedConfigurationSection = configFile.GetSection("configProtectedData") as ProtectedConfigurationSection;
            return protectedConfigurationSection.DefaultProvider;
        }

        private Configuration GetConfigFile(string filename)
        {
            var configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = filename;

            var configFile = ConfigurationManager.OpenMappedExeConfiguration(
                configFileMap,
                ConfigurationUserLevel.None
            );

            return configFile;
        }
    }
}
