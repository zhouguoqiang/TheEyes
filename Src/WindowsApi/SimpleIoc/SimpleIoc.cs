using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIoc
{

    //
    // 摘要:
    //     The generic Service Locator interface. This interface is used to retrieve services
    //     (instances identified by type and optional name) from a container.
    public interface IServiceLocator : IServiceProvider
    {
        //
        // 摘要:
        //     Get all instances of the given serviceType currently registered in the container.
        //
        // 参数:
        //   serviceType:
        //     Type of object requested.
        //
        // 返回结果:
        //     A sequence of instances of the requested serviceType.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is are errors resolving the service instance.
        IEnumerable<object> GetAllInstances(Type serviceType);
        //
        // 摘要:
        //     Get all instances of the given TService currently registered in the container.
        //
        // 类型参数:
        //   TService:
        //     Type of object requested.
        //
        // 返回结果:
        //     A sequence of instances of the requested TService.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is are errors resolving the service instance.
        IEnumerable<TService> GetAllInstances<TService>();
        //
        // 摘要:
        //     Get an instance of the given serviceType.
        //
        // 参数:
        //   serviceType:
        //     Type of object requested.
        //
        // 返回结果:
        //     The requested service instance.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is an error resolving the service instance.
        object GetInstance(Type serviceType);
        //
        // 摘要:
        //     Get an instance of the given named serviceType.
        //
        // 参数:
        //   serviceType:
        //     Type of object requested.
        //
        //   key:
        //     Name the object was registered with.
        //
        // 返回结果:
        //     The requested service instance.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is an error resolving the service instance.
        object GetInstance(Type serviceType, string key);
        //
        // 摘要:
        //     Get an instance of the given TService.
        //
        // 类型参数:
        //   TService:
        //     Type of object requested.
        //
        // 返回结果:
        //     The requested service instance.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is are errors resolving the service instance.
        TService GetInstance<TService>();
        //
        // 摘要:
        //     Get an instance of the given named TService.
        //
        // 参数:
        //   key:
        //     Name the object was registered with.
        //
        // 类型参数:
        //   TService:
        //     Type of object requested.
        //
        // 返回结果:
        //     The requested service instance.
        //
        // 异常:
        //   T:Microsoft.Practices.ServiceLocation.ActivationException:
        //     if there is are errors resolving the service instance.
        TService GetInstance<TService>(string key);
    }

    //
    // 摘要:
    //     定义一种检索服务对象的机制，服务对象是为其他对象提供自定义支持的对象。
    public interface IServiceProvider
    {
        //
        // 摘要:
        //     获取指定类型的服务对象。
        //
        // 参数:
        //   serviceType:
        //     一个对象，它指定要获取的服务对象的类型。
        //
        // 返回结果:
        //     serviceType 类型的服务对象。 - 或 - 如果没有 serviceType 类型的服务对象，则为 null。
        object GetService(Type serviceType);
    }

    #region IOC
    /// <summary>
    /// this is a simple Ioc frame
    /// </summary>
    public class SimpleIoc//: IServiceLocator, IServiceProvider
    {
        /// <summary>
        /// objects that would keep alive until the Revit closed
        /// </summary>
        private static Dictionary<Type, object> _singleInstances = new Dictionary<Type, object>();

        /// <summary>
        /// objects that should release after a command completed,calling the Clear method        
        /// </summary>              
        private static Dictionary<Type, Dictionary<string, object>> _instances = new Dictionary<Type, Dictionary<string, object>>();

        /// <summary>
        /// Type and ConstructorInfo map
        /// </summary>
        private static Dictionary<Type, ConstructorInfo> _factory = new Dictionary<Type, ConstructorInfo>();

        /// <summary>
        /// the default name of an instance
        /// </summary>
        private static readonly string _defaultKey = Guid.NewGuid().ToString();
        private static readonly object _locker = new object();
        private static SimpleIoc _default = new SimpleIoc();
        public static SimpleIoc Default
        {
            get
            {
                return _default;
            }
        }

        private SimpleIoc()
        {

        }

        public TType GetInstance<TType>(string name = null)
            where TType : class
        {
            lock (_locker)
            {
                TType result = null;
                Type type = typeof(TType);

                if (name == null)
                {
                    name = _defaultKey;
                }
                if (!_instances.ContainsKey(type))
                {
                    if (_factory.ContainsKey(type))
                    {
                        _instances.Add(type, new Dictionary<string, object>());
                    }
                    else
                    {
                        throw new Exception("the type " + type.ToString() + " has not been registered");
                    }
                }


                if (_instances[type].ContainsKey(name))
                {
                    result = _instances[type][name] as TType;
                }
                else
                {
                    object newVaule = CreateInstance(type);
                    _instances[type].Add(name, newVaule);
                    result = newVaule as TType;
                }


                return result;
            }
        }

        /// <summary>
        /// register a type,
        /// if the class's constructor is not unique, 
        /// the parms is need to find the which constructor will be add to the _factory
        /// </summary>
        /// <typeparam name="TType">the class will be registered</typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public bool Register<TType>(Type[] parms = null) where TType : class
        {
            lock (_locker)
            {
                Type type = typeof(TType);
                ConstructorInfo currentConstructorInfo = null;
                if (!_factory.ContainsKey(type))
                {
                    currentConstructorInfo = GetConstructorInfo<TType>(parms);
                    if (currentConstructorInfo == null)
                    {
                        throw new Exception("there is no ConstructorInfo available");
                    }
                    _factory.Add(type, currentConstructorInfo);
                }
                return true;
            }
        }

        /// <summary>
        /// register a interface with a class that implement the interface,
        /// and when creating a instance of the interface,the type's constructor will be called.
        /// if the class's constructor is not unique, 
        /// the parms is need to find the which constructor will be add to the _factory
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TType"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public bool Register<TInterface, TType>(Type[] parms = null)
            where TInterface : class
            where TType : class
        {
            lock (_locker)
            {
                Type type = typeof(TInterface);
                ConstructorInfo currentConstructorInfo = null;
                if (!_factory.ContainsKey(type))
                {
                    currentConstructorInfo = GetConstructorInfo<TType>(parms);
                    if (currentConstructorInfo == null)
                    {
                        throw new Exception("there is no ConstructorInfo available");
                    }
                    _factory.Add(type, currentConstructorInfo);
                }
                return true;
            }
        }

        /// <summary>
        /// create a instance of the class,and add it to the _singleInstances dictionary,
        /// the instance will keep alive, until the Revit closed, it is a application scope instance
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public bool RegisterSingle<TType>(Type[] parms = null)
        {
            lock (_locker)
            {
                Type type = typeof(TType);
                ConstructorInfo cons = GetConstructorInfo<TType>(parms);
                if (cons == null)
                {
                    throw new Exception("there is no ConstructorInfo available");
                }
                object ins = CreateInstance(cons);
                _singleInstances.Add(type, ins);
                return true;
            }
        }

        public bool RegisterSingle<TInterface, TType>(Type[] parms = null)
        {
            lock (_locker)
            {
                Type type = typeof(TInterface);
                object ins = CreateInstance(typeof(TType));
                _singleInstances.Add(type, ins);
                return true;
            }
        }

        /// <summary>
        /// get instance in the _singleInstances dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSingleInstance<T>()
        {
            lock (_locker)
            {
                Type type = typeof(T);
                if (!_singleInstances.ContainsKey(type))
                {
                    throw new Exception("the type " + type.ToString() + " has not been registered before");
                }
                return (T)_singleInstances[type];
            }
        }

        /// <summary>
        /// get the constructor of the type,and the type must have at least one public constructor.
        /// if the constructor is unique, the parms paramter make no sense,
        /// otherwise the parms is used to find the right constructor
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        private ConstructorInfo GetConstructorInfo<TType>(Type[] parms = null)
        {
            Type type = typeof(TType);
            ConstructorInfo[] consInfos = type.GetConstructors();
            if (consInfos == null || consInfos.Length == 0)
                return null;
            if (consInfos.Length == 1)
            {
                return consInfos.First();
            }
            ConstructorInfo result = null;
            if (parms != null && parms.Length != 0)
            {
                foreach (ConstructorInfo c in consInfos)
                {
                    ParameterInfo[] ps = c.GetParameters();
                    if (ps.Length == parms.Length)
                    {
                        bool flag = true;
                        foreach (ParameterInfo tp in ps)
                        {
                            if (!parms.Contains(tp.ParameterType))
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            result = c;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// may this is not a good solution 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        public void ResisterInstance(object obj, string name = null)
        {
            lock (_locker)
            {
                Type type = obj.GetType();
                if (!_instances.ContainsKey(type))
                {
                    _instances.Add(type, new Dictionary<string, object>());
                }
                else
                {
                    string nm = null;
                    if (name == null)
                    {
                        nm = nameof(obj);
                    }
                    else
                    {
                        nm = name;
                    }
                    if (_instances[type].ContainsKey(nm))
                    {
                        throw new Exception("a instance with the same name has already registered");
                    }
                    else
                    {
                        _instances[type].Add(nameof(obj), obj);
                    }
                }
            }
        }

        public void RegisterDefaultInstance(object obj, bool replaceformer = false)
        {
            lock (_locker)
            {
                Type type = obj.GetType();
                if (!_instances.ContainsKey(type))
                {
                    _instances.Add(type, new Dictionary<string, object>());
                }


                if (_instances[type].ContainsKey(_defaultKey))
                {
                    if (replaceformer)
                        throw new Exception("a instance with the same name has already registered");
                    else
                    {
                        _instances[type][_defaultKey] = obj;
                    }
                }
                else
                {
                    _instances[type].Add(_defaultKey, obj);
                }

            }
        }

        /// <summary>
        /// create instance of the type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object CreateInstance(Type serviceType)
        {
            object result = null;
            if (!_factory.ContainsKey(serviceType))
            {
                throw new Exception("the type " + serviceType.ToString() + " has not been registered");
            }
            result = CreateInstance(_factory[serviceType]);
            return result;
        }

        /// <summary>
        /// create instance use the ConstructorInfo
        /// </summary>
        /// <param name="consInfo"></param>
        /// <returns></returns>
        private object CreateInstance(ConstructorInfo consInfo)
        {
            object result = null;
            if (consInfo == null)
            {
                throw new Exception("the ConstructorInfo does not exist");
            }
            ParameterInfo[] psInfo = consInfo.GetParameters();
            if (psInfo == null || psInfo.Length == 0)
            {
                result = consInfo.Invoke(null);
            }
            else
            {
                object[] parms = new object[psInfo.Length];
                for (int i = 0; i < psInfo.Length; i++)
                {
                    Type ptype = psInfo[i].ParameterType;
                    parms[i] = CreateInstance(ptype);
                }
                result = consInfo.Invoke(parms);

            }
            return result;
        }

        /// <summary>
        /// get the constructor of the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        private ConstructorInfo GetConstructorInfo(Type type, Type[] objs = null)
        {
            ConstructorInfo[] cons = type.GetConstructors();
            if (cons == null || cons.Count() == 0)
            {
                throw new Exception("the type does not contains a public constructor");
            }
            if (objs == null)
            {
                return cons.First();
            }
            else
            {
                return type.GetConstructor(objs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                _factory.Clear();
                _instances.Clear();
            }
        }


    }
    #endregion

    /// <summary>
    /// A very simple IOC container with basic functionality needed to register and resolve
    /// instances. If needed, this class can be replaced by another more elaborate
    /// IOC container implementing the IServiceLocator interface.
    /// The inspiration for this class is at https://gist.github.com/716137 but it has
    /// been extended with additional features.
    /// </summary>
    public interface ISimpleIoc : IServiceLocator, IServiceProvider
    {
        /// <summary>
        /// Checks whether at least one instance of a given class is already created in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <returns>True if at least on instance of the class is already created, false otherwise.</returns>
        bool ContainsCreated<TClass>();
        /// <summary>
        /// Checks whether the instance with the given key is already created for a given class
        /// in the container.
        /// </summary>
        /// <typeparam name="TClass">The class that is queried.</typeparam>
        /// <param name="key">The key that is queried.</param>
        /// <returns>True if the instance with the given key is already registered for the given class,
        /// false otherwise.</returns>
        bool ContainsCreated<TClass>(string key);
        /// <summary>
        /// Gets a value indicating whether a given type T is already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <returns>True if the type is registered, false otherwise.</returns>
        bool IsRegistered<T>();
        /// <summary>
        /// Gets a value indicating whether a given type T and a give key
        /// are already registered.
        /// </summary>
        /// <typeparam name="T">The type that the method checks for.</typeparam>
        /// <param name="key">The key that the method checks for.</param>
        /// <returns>True if the type and key are registered, false otherwise.</returns>
        bool IsRegistered<T>(string key);
        /// <summary>
        /// Registers a given type for a given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        void Register<TInterface, TClass>() where TInterface : class where TClass : class;
        /// <summary>
        /// Registers a given type for a given interface with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        void Register<TInterface, TClass>(bool createInstanceImmediately) where TInterface : class where TClass : class;
        /// <summary>
        /// Registers a given type.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        void Register<TClass>() where TClass : class;
        /// <summary>
        /// Registers a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        void Register<TClass>(bool createInstanceImmediately) where TClass : class;
        /// <summary>
        /// Registers a given instance for a given type.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        void Register<TClass>(Func<TClass> factory) where TClass : class;
        /// <summary>
        /// Registers a given instance for a given type with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        void Register<TClass>(Func<TClass> factory, bool createInstanceImmediately) where TClass : class;
        /// <summary>
        /// Registers a given instance for a given type and a given key.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        void Register<TClass>(Func<TClass> factory, string key) where TClass : class;
        /// <summary>
        /// Registers a given instance for a given type and a given key with the possibility for immediate
        /// creation of the instance.
        /// </summary>
        /// <typeparam name="TClass">The type that is being registered.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        /// <param name="key">The key for which the given instance is registered.</param>
        /// <param name="createInstanceImmediately">If true, forces the creation of the default
        /// instance of the provided class.</param>
        void Register<TClass>(Func<TClass> factory, string key, bool createInstanceImmediately) where TClass : class;
        /// <summary>
        /// Resets the instance in its original states. This deletes all the
        /// registrations.
        /// </summary>
        void Reset();
        /// <summary>
        /// Unregisters a class from the cache and removes all the previously
        /// created instances.
        /// </summary>
        /// <typeparam name="TClass">The class that must be removed.</typeparam>
        void Unregister<TClass>() where TClass : class;
        /// <summary>
        /// Removes the given instance from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="instance">The instance that must be removed.</param>
        void Unregister<TClass>(TClass instance) where TClass : class;
        /// <summary>
        /// Removes the instance corresponding to the given key from the cache. The class itself remains
        /// registered and can be used to create other instances.
        /// </summary>
        /// <typeparam name="TClass">The type of the instance to be removed.</typeparam>
        /// <param name="key">The key corresponding to the instance that must be removed.</param>
        void Unregister<TClass>(string key) where TClass : class;
    }

    public class TestClass : IServiceLocator, IServiceProvider
    {
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType, string key)
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>()
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
