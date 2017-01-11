using System.Collections.Generic;
using System.Linq;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;

namespace Nancy.Rest.Module
{
    public class DefaultRestBootstrapper : DefaultNancyBootstrapper
    {
        //We Need to skip this assembly from the available modules.
        private ModuleRegistration[] modules;
        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return
                    modules
                    ??
                    (modules = AppDomainAssemblyTypeScanner
                                        .TypesOf<INancyModule>(ScanMode.ExcludeNancyNamespace)
                                        .NotOfType<DiagnosticModule>()
                                        .Select(t => new ModuleRegistration(t))
                                        .ToArray());
            }
        }
    }
}
