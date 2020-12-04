The static property `Config.Default` and the method parameters in `src\Example\Config.cs` cannot be renamed when the Source Generators generate static extenstion methods for the Config class from the generator in `src\Common.SourceGenerators\SourceGenerator.cs`.

The source generator seems to work -- the code is generated, the project builds, the files are emitted when the emit files option is enabled -- however, if I go to rename a propery (CTRL R + CTRL R), Visual Studio will crash outright when using the 32-bit Roslyn Analyzer process or Visual Studio will eventually throw a yellow exception bar notification if using the 64-bit analyzer.

Is there something I could have done wrong when making the analyzer, or is this some sort of bug?

## Config.cs
```csharp
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
```
## The generated class outputs
```csharp
using System;

namespace Example
{
    public partial class Config : Common.IAutoCloneable<Config>
    {
        public Config(Config other)
        {
            UserName = other.UserName;
        }

        public Config Clone() => new Config(this);
    }
}
```

```csharp
using System;

namespace Example
{
    public static class ConfigAutoCloneableExtensions
    {
        public static T WithUserName<T>(this T target, string userName)
            where T : Config, Common.IAutoCloneable<T>
        {
            var clone = ((Common.IAutoCloneable<T>)target).Clone();
            clone.UserName = userName;
            return clone;
        }
    }
}
```

## Various encountered exceptions in Visual Studio from this problem

```
StreamJsonRpc.ConnectionLostException : The JSON-RPC connection with the remote party was lost before the request could complete.
   at async StreamJsonRpc.JsonRpc.InvokeCoreAsync(<Unknown Parameters>)
   at async StreamJsonRpc.JsonRpc.InvokeCoreAsync[TResult](<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.RemoteEndPoint.InvokeAsync(<Unknown Parameters>)
   at Microsoft.VisualStudio.Telemetry.WindowsErrorReporting.WatsonReport.GetClrWatsonExceptionInfo(Exception exceptionObject)
```

```
StreamJsonRpc.ConnectionLostException : The JSON-RPC connection with the remote party was lost before the request could complete.
   at async StreamJsonRpc.JsonRpc.InvokeCoreAsync(<Unknown Parameters>)
   at async StreamJsonRpc.JsonRpc.InvokeCoreAsync[TResult](<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.RemoteEndPoint.InvokeAsync[T](<Unknown Parameters>)
```

```
System.ObjectDisposedException : Cannot access a disposed object.
Object name: 'HubClient'.
   at Microsoft.ServiceHub.Client.HubClient.ThrowIfDisposed()
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
   at Microsoft.VisualStudio.Telemetry.WindowsErrorReporting.WatsonReport.GetClrWatsonExceptionInfo(Exception exceptionObject)
```

```
System.OperationCanceledException : The operation was canceled.
   at System.Threading.CancellationToken.ThrowOperationCanceledException()
   at async Microsoft.ServiceHub.Client.HubControllerClient.StartAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.LaunchOrFindControllerAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetLocationServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Utility.Shared.ServiceHubRetry.ExecuteAsync[TReturnType](<Unknown Parameters>)
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at async Microsoft.ServiceHub.Utility.Shared.ServiceHubRetry.ExecuteAsync[TReturnType](<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.<>c__DisplayClass9_0.<RequestServiceChannelAsync>b__0(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Utility.Shared.ServiceHubRetry.ExecuteAsync[TReturnType](<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at async Microsoft.ServiceHub.Utility.Shared.ServiceHubRetry.ExecuteAsync[TReturnType](<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at async Microsoft.ServiceHub.Utility.Shared.ServiceHubRetry.ExecuteAsync[TReturnType](<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RemoteServiceBrokerWrapper.RequestServiceChannelAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Framework.RemoteServiceBroker.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.GetPipeAsync(<Unknown Parameters>)
   at async Microsoft.ServiceHub.Client.HubClient.RequestServiceAsync(<Unknown Parameters>)
   at async Microsoft.CodeAnalysis.Remote.ServiceHubRemoteHostClient.RequestServiceAsync(<Unknown Parameters>)
```
