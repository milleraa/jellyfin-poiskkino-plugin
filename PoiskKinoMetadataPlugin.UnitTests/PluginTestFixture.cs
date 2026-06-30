using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Moq;

namespace PoiskKinoMetadataPlugin.UnitTests;

public class PluginTestFixture : IDisposable
{
    public const string TestApiKey = "test-api-key";

    public Mock<ILoggerFactory> LoggerFactoryMock { get; } = new();

    public PluginTestFixture()
    {
        LoggerFactoryMock
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
    }

    public static void SetUpPlugin(string? apiKey = TestApiKey)
    {
        var appPathsMock = new Mock<IApplicationPaths>();
        appPathsMock.Setup(p => p.ProgramDataPath).Returns("/tmp/programdata");
        appPathsMock.Setup(p => p.WebPath).Returns("/tmp/web");
        appPathsMock.Setup(p => p.ProgramSystemPath).Returns("/tmp/system");
        appPathsMock.Setup(p => p.DataPath).Returns("/tmp/data");
        appPathsMock.Setup(p => p.ImageCachePath).Returns("/tmp/imagecache");
        appPathsMock.Setup(p => p.PluginsPath).Returns("/tmp/plugins");
        appPathsMock.Setup(p => p.PluginConfigurationsPath).Returns("/tmp/config");
        appPathsMock.Setup(p => p.LogDirectoryPath).Returns("/tmp/logs");
        appPathsMock.Setup(p => p.ConfigurationDirectoryPath).Returns("/tmp/configdir");
        appPathsMock.Setup(p => p.SystemConfigurationFilePath).Returns("/tmp/system.xml");
        appPathsMock.Setup(p => p.CachePath).Returns("/tmp/cache");
        appPathsMock.Setup(p => p.TempDirectory).Returns("/tmp/temp");
        appPathsMock.Setup(p => p.VirtualDataPath).Returns("/tmp/virtualdata");
        appPathsMock.Setup(p => p.TrickplayPath).Returns("/tmp/trickplay");
        appPathsMock.Setup(p => p.BackupPath).Returns("/tmp/backup");

        var xmlSerializerMock = new Mock<IXmlSerializer>();
        xmlSerializerMock
            .Setup(s => s.DeserializeFromFile(It.IsAny<Type>(), It.IsAny<string>()))
            .Returns((Type type, string path) => Activator.CreateInstance(type)!);

        var pluginLoggerMock = new Mock<ILogger<Plugin>>();

        var plugin = new Plugin(
            appPathsMock.Object,
            xmlSerializerMock.Object,
            pluginLoggerMock.Object);

        plugin.Configuration.ApiKey = apiKey ?? string.Empty;
    }

    public static Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }

    public void Dispose()
    {
        typeof(Plugin).GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, null);
    }
}

[CollectionDefinition("PluginInstanceCollection")]
public class PluginInstanceCollectionDefinition : ICollectionFixture<PluginTestFixture>
{
}

[Collection("PluginInstanceCollection")]
public class PluginTestBase
{
    protected PluginTestFixture Fixture { get; }

    public PluginTestBase(PluginTestFixture fixture)
    {
        Fixture = fixture;
    }
}
