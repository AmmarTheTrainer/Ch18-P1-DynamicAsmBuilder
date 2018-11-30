using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Ch18_P1_DynamicAsmBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateMyAsm();
        }

        private static void CreateMyAsm(AppDomain curAppDomain)
        {
            // Establish general assembly characteristics.
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "MyAssembly";
            assemblyName.Version = new Version("1.0.0.0");
            // Create new assembly within the current AppDomain.
            AssemblyBuilder assembly =
            curAppDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            // Given that we are building a single-file
            // assembly, the name of the module is the same as the assembly.
            ModuleBuilder module =
            assembly.DefineDynamicModule("MyAssembly", "MyAssembly.dll");
            // Define a public class named "HelloWorld".
            TypeBuilder helloWorldClass = module.DefineType("MyAssembly.HelloWorld", TypeAttributes.
            Public);
            // Define a private String member variable named "theMessage".
            FieldBuilder msgField =
            helloWorldClass.DefineField("theMessage", Type.GetType("System.String"),
            FieldAttributes.Private);

            // Create the custom ctor.
            Type[] constructorArgs = new Type[1];
            constructorArgs[0] = typeof(string);
            ConstructorBuilder constructor =
            helloWorldClass.DefineConstructor(MethodAttributes.Public,
            CallingConventions.Standard,
            constructorArgs);
            ILGenerator constructorIL = constructor.GetILGenerator();
            constructorIL.Emit(OpCodes.Ldarg_0);
            Type objectClass = typeof(object);

            ConstructorInfo superConstructor = objectClass.GetConstructor(new Type[0]);
            constructorIL.Emit(OpCodes.Call, superConstructor);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, msgField);
            constructorIL.Emit(OpCodes.Ret);
            // Create the default ctor.
            helloWorldClass.DefineDefaultConstructor(MethodAttributes.Public);
            // Now create the GetMsg() method.
            MethodBuilder getMsgMethod =
            helloWorldClass.DefineMethod("GetMsg", MethodAttributes.Public,
            typeof(string), null);
            ILGenerator methodIL = getMsgMethod.GetILGenerator();
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, msgField);
            methodIL.Emit(OpCodes.Ret);

            // Create the SayHello method.
            MethodBuilder sayHiMethod =
            helloWorldClass.DefineMethod("SayHello",
            MethodAttributes.Public, null, null);
            methodIL = sayHiMethod.GetILGenerator();
            methodIL.EmitWriteLine("Hello from the HelloWorld class!");
            methodIL.Emit(OpCodes.Ret);
            // "Bake" the class HelloWorld.
            // (Baking is the formal term for emitting the type.)
            helloWorldClass.CreateType();
            // (Optionally) save the assembly to file.
            assembly.Save("MyAssembly.dll");
        }
    }
}
