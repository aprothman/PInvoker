using System;
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
        /// This is a special field that must be explicitly returned by any [PInvokable] stub
        /// methods.
        /// </summary>
        protected object Result { get; private set; } = null;

        /// <summary>
        /// The name of the library without its file extension
        /// </summary>
        public ArchTuple<string> DllName { get; set; }

        /// <summary>
        /// Optional prefix for the library name
        /// </summary>
        public ArchTuple<string> DllNamePrefix { get; set; }

        /// <summary>
        /// Optional suffix for the library name
        /// </summary>
        public ArchTuple<string> DllNameSuffix { get; set; }

        /// <summary>
        /// Optional path to the library
        /// </summary>
        public ArchTuple<string> DllPath { get; set; }

        /// <summary>
        /// Calling convention for the library's exported functions
        /// </summary>
        public ArchTuple<CallingConvention> DllCallingConv { get; set; }

        /// <summary>
        /// Character set for strings in calls to the library
        /// </summary>
        public ArchTuple<CharSet> DllCharSet { get; set; }

        /// <summary>
        /// String name transformation function used to generate the library's
        /// exported function name from the name of the PInvokable method
        /// </summary>
        public ArchTuple<Func<string, string>> MethodNameTransform { get; set; }

        /// <summary>
        /// Optional prefix for the library's exported function names
        /// </summary>
        public ArchTuple<string> MethodNamePrefix { get; set; }

        /// <summary>
        /// Optional suffix for the library's exported function names
        /// </summary>
        public ArchTuple<string> MethodNameSuffix { get; set; }

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
                                                       | MethodImplAttributes.PreserveSig
                                                       | MethodImplAttributes.);
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

            Result = _class.GetMethod(name, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);
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
