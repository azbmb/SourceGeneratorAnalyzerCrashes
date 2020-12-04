using Common.SourceGenerators;

namespace Example
{
    [AutoCloneable]
    public partial class Config
    {
        public static Config Default { get; } = new Config();

        public string UserName { get; set; }

        protected Config()
        {
            UserName = string.Empty;

            // This extension method is created by the source generator and will crash the
            // the Roslyn analyzer process after a short wait when a rename on the property
            // named Default in this class is attempted.
            var willCrashRoslynAnalyzerProcess = Default
                .WithUserName("test");
        }

        // Attempting to rename the userName parameter will also crash the analyzer.
        public void WriteUserName(string userName)
        {
            var config = new Config
            {
                UserName = userName
            };
            System.Console.WriteLine(config.UserName);
        }

        // Attempting to rename the userName parameter will also crash the analyzer.
        public void DoNothing(string userName)
        {
        }
    }
}
