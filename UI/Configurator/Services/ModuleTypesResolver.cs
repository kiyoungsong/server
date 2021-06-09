using System;
using Configurator.Modules;

namespace Configurator.Services
{
    public interface IModuleTypesResolver
    {
        string GetName(ModuleType moduleType);
        string GetTypeName(ModuleType moduleType);
        System.Guid GetId(ModuleType moduleType);
        ModuleType GetMainModuleType(ModuleType type);
        ModuleType GetAccordionModuleType(ModuleType type);
    }
    class ModuleTypesResolver : IModuleTypesResolver
    {
        public string GetName(ModuleType moduleType)
        {
            if (moduleType == ModuleType.Unknown)
                return null;
            return moduleType.ToString();
        }
        public string GetTypeName(ModuleType moduleType)
        {
            if (moduleType == ModuleType.Unknown)
                return null;
            return moduleType.ToString();
        }
        public Guid GetId(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.DeviceModule: return new Guid("d4f3a688-3785-41f3-8b62-97dafdbb33f1");
                case ModuleType.TagViewer: return new Guid("d4f3a688-3785-41f3-8b62-97dafdbb33f3");
                default: return Guid.Empty;
            }
        }
        public ModuleType GetMainModuleType(ModuleType type)
        {
            return type;
        }
        public ModuleType GetAccordionModuleType(ModuleType type)
        {
            return type;
        }
    }
}
