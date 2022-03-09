using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace DataGridViewAutoFilter
{
    internal class LanguageHandler
    {
        internal static LanguageHandler handler = new LanguageHandler();
        internal static ComboBox languageSelector;
        internal static string userChoice;
        internal static ResourceManager resourceManager;

        private LanguageHandler()
        {
        }

        /// <summary>
        /// Checks the for language set by the user
        /// </summary>
        internal void CheckUserChoice()
        {
            resourceManager = new ResourceManager("DataGridViewAutoFilter.lang_en", Assembly.GetExecutingAssembly());
        }
    }
}