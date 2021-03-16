//© Copyright 2021 Denis Voitenko MIT License
//GitHub repository: https://github.com/DenVot/BotBotya

using DiscordBot.Modules;
using System;
using System.Reflection;

namespace DiscordBot.Collections
{
    /// <summary>
    /// Усовершенствованная коллекция модулей. В разработке... Не использовать!
    /// </summary>
    public class ModulesCollectionGeneric : ModulesCollection
    {
        //private readonly IServiceProvider Set;

        public ModulesCollectionGeneric(IServiceProvider set) => throw new NotImplementedException();
        //{
        //    Set = set;
        //}

        public void AddModule<T>() => throw new NotImplementedException();
        //{
        //    var currentAssembly = GetAssembly();

        //    List<Type> allTypes = currentAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(IModule))).ToList();

        //    if (allTypes.Contains(typeof(T)))
        //    {
        //        var currentType = allTypes.Where(x => x.Name == typeof(T).Name).First();
        //        var constructor = currentType.GetConstructors().Where(x => x.IsPublic).First();
        //        var allParams = constructor.GetParameters();

        //        var allTypesOfServiceProvider = Set.GetServices<Type>();

        //    }
        //    else
        //        throw new NotExtendsModuleInterfaceException(typeof(T));
        //}

        private Assembly GetAssembly() => typeof(IModule).Assembly;
    }
}
