using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicPInvoke
{
    public interface IExternalCallable
    {
        void CallMethod(string name, params object[] args);
    }

    [PInvokerContext]
    public abstract class PInvoker : ContextBoundObject, IExternalCallable
    {
        protected static int s_instanceCount = 0;

        private readonly Architecture _arch = Environment.Is64BitProcess ? Architecture.x64 : Architecture.x86;
        private bool _initialized = false;

        private readonly Type _hostClass;

        private AssemblyBuilder _assembly;
        private ModuleBuilder _module;
        private TypeBuilder _classBuilder;
        private Type _class;

        /// <summary>
        /// We use a BlockingCollection that wrapps a ConcurrentQueue, and we set the
        /// maximum number of items in the queue to 1 to force each value to be read
        /// from the data structure before a new value can be added to it.
        /// </summary>
        private readonly BlockingCollection<object> _results = new BlockingCollection<object>(1);

        /// <summary>
        /// This is a special field that must be explicitly returned by any [PInvokable] stub
        /// methods.
        /// </summary>
        protected object Result
        {
            get => _results.Take();
            private set => _results.Add(value);
        }

        /// <summary>
        /// The name of the library without its file extension
        /// </summary>
        private ArchTuple<string> _dllName;
        public ArchTuple<string> DllName
        {
            get => _dllName;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllName = value;
            }
        }

        /// <summary>
        /// Optional prefix for the library name
        /// </summary>
        private ArchTuple<string> _dllNamePrefix;
        public ArchTuple<string> DllNamePrefix
        {
            get => _dllNamePrefix;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllNamePrefix = value;
            }
        }

        /// <summary>
        /// Optional suffix for the library name
        /// </summary>
        private ArchTuple<string> _dllNameSuffix;
        public ArchTuple<string> DllNameSuffix
        {
            get => _dllNameSuffix;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllNameSuffix = value;
            }
        }

        /// <summary>
        /// Optional path to the library
        /// </summary>
        private ArchTuple<string> _dllPath;
        public ArchTuple<string> DllPath
        {
            get => _dllPath;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllPath = value;
            }
        }

        /// <summary>
        /// Calling convention for the library's exported functions
        /// </summary>
        private ArchTuple<CallingConvention> _dllCallingConv;
        public ArchTuple<CallingConvention> DllCallingConv
        {
            get => _dllCallingConv;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllCallingConv = value;
            }
        }

        /// <summary>
        /// Character set for strings in calls to the library
        /// </summary>
        private ArchTuple<CharSet> _dllCharSet;
        public ArchTuple<CharSet> DllCharSet
        {
            get => _dllCharSet;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _dllCharSet = value;
            }
        }

        /// <summary>
        /// String name transformation function used to generate the library's
        /// exported function name from the name of the PInvokable method
        /// </summary>
        private ArchTuple<Func<string, string>> _methodNameTransform;
        public ArchTuple<Func<string, string>> MethodNameTransform
        {
            get => _methodNameTransform;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _methodNameTransform = value;
            }
        }

        /// <summary>
        /// Optional prefix for the library's exported function names
        /// </summary>
        private ArchTuple<string> _methodNamePrefix;
        public ArchTuple<string> MethodNamePrefix
        {
            get => _methodNamePrefix;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _methodNamePrefix = value;
            }
        }

        /// <summary>
        /// Optional suffix for the library's exported function names
        /// </summary>
        private ArchTuple<string> _methodNameSuffix;
        public ArchTuple<string> MethodNameSuffix
        {
            get => _methodNameSuffix;
            set {
                if (_initialized) throw new InvalidOperationException("This property can't be changed after initialization.");

                _methodNameSuffix = value;
            }
        }

        protected PInvoker()
        {
            _hostClass = this.GetType();
            InitDefaults();
            BuildAssembly(MakeAssemblyName(_hostClass));
        }

        /// <summary>
        /// This method must be called by an inheritting class after the custom naming properties
        /// for the target library have been set but before any calls are made to a [PInvokable] 
        /// method
        /// </summary>
        public void FinalizeInit()
        {
            if (_initialized) throw new InvalidOperationException("This instance has already been intitialized.");

            var dllName = ComposeDllName();

            // enumerate the PInvokable methods of the class hierarchy
            var methods = _hostClass.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods) {
                if (null != method.GetCustomAttribute<PInvokableAttribute>()) {
                    var args = method.GetParameters();

                    var paramTypes = (Type[])Array.CreateInstance(typeof(Type), args.Length);
                    for (var i = 0; i < args.Length; ++i) {
                        paramTypes[i] = args[i].ParameterType;
                    }

                    var entryName = MethodNamePrefix[_arch] 
                                  + MethodNameTransform[_arch](method.Name)
                                  + MethodNameSuffix[_arch];
                    var methodBuilder = _classBuilder.DefinePInvokeMethod(
                        name: method.Name,
                        dllName: dllName,
                        entryName: entryName,
                        parameterTypes: paramTypes,
                        returnType: method.ReturnType,
                        attributes: MethodAttributes.Public | MethodAttributes.Static,
                        callingConvention: CallingConventions.Standard,
                        nativeCallConv: DllCallingConv[_arch],
                        nativeCharSet: DllCharSet[_arch]
                    );
                    methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags()
                                                       | MethodImplAttributes.PreserveSig);
                }
            }

            _class = _classBuilder.CreateType();

            _initialized = true;
        }

        /// <summary>
        /// Call the given static method on the object that was dynamically created during instantiation
        /// </summary>
        /// <param name="name">the method name</param>
        /// <param name="args">the method parameters to pass to the call</param>
        public void CallMethod(string name, params object[] args)
        {
            if (!_initialized) throw new InvalidOperationException("FinalizeInit() must be run before making any calls.");

            var method = _class.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (null == method) {
                throw new InvalidOperationException(name + " is not a static method on " + _class.Name + ".");
            }

            if (typeof(void) == method.ReturnType) {
                method.Invoke(null, args);
            } else {
                Result = method.Invoke(null, args);
            }
        }

        private void InitDefaults()
        {
            DllName = new ArchTuple<string>("");
            DllNamePrefix = new ArchTuple<string>("");
            DllNameSuffix = new ArchTuple<string> {
                [Architecture.x86] = "",
                [Architecture.x64] = "64"
            };
            DllPath = new ArchTuple<string>("");
            DllCallingConv = new ArchTuple<CallingConvention>(CallingConvention.StdCall);
            DllCharSet = new ArchTuple<CharSet>(CharSet.Unicode);
            MethodNameTransform = new ArchTuple<Func<string, string>>(NameTransforms.NoOp);
            MethodNamePrefix = new ArchTuple<string>("");
            MethodNameSuffix = new ArchTuple<string>("");
        }

        private void BuildAssembly(string name)
        {
            var asmName = new AssemblyName(name);

            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            _module = _assembly.DefineDynamicModule(name);
            _classBuilder = _module.DefineType("PInvokeWrapper", TypeAttributes.Public | TypeAttributes.UnicodeClass);
        }

        private string MakeAssemblyName(Type hostClass)
        {
            return "_PInvoker_" + hostClass.Name + "_" + s_instanceCount++.ToString();
        }

        private string ComposeDllName()
        {
            var name = DllName[_arch];
            if (string.IsNullOrEmpty(name)) {
                var className = _hostClass.Name;
                if (className.EndsWith("Wrapper")) {
                    name = className.Substring(0, className.Length - 7);
                } else {
                    throw new InvalidOperationException("Couldn't infer the Dll name");
                }
            }
            name = DllNamePrefix[_arch] + name + DllNameSuffix[_arch] + ".dll";

            return name;
        }
    }
}
