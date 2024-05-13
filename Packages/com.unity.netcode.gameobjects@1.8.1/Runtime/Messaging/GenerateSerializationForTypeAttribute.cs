using System;

namespace Unity.Netcode
{
    /// <summary>
    /// Specifies a specific type that needs serialization to be generated by codegen.
    /// This is only needed in special circumstances where manual serialization is being done.
    /// If you are making a generic network variable-style class, use <see cref="GenerateSerializationForGenericParameterAttribute"/>.
    /// <br />
    /// <br />
    /// This attribute can be attached to any class or method anywhere in the codebase and
    /// will trigger codegen to generate serialization code for the provided type. It only needs
    /// to be included once type per codebase, but including it multiple times for the same type
    /// is safe.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = true)]
    public class GenerateSerializationForTypeAttribute : Attribute
    {
        internal Type Type;

        public GenerateSerializationForTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
