using System;
using Castle.Windsor;
using KuraSharp.Extensions;
using Serilog;

namespace KuraSharp.Services; 

public class ServiceManager : IDisposable {
    static readonly ILogger Logger = Log.Logger.ForType<ServiceManager>();

    public static ServiceManager Instance { get; } = new();

    public WindsorContainer Container { get; }

    ServiceManager() {
        Container = new WindsorContainer();
    }

    public void Init() {
        Container.Install(new ServicesInstaller());

        Logger.Information("Initialized");
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        Container.Dispose();
    }
}