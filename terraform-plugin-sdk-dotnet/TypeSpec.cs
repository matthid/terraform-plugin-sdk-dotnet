using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Matthid.TerraformSdk
{
    public abstract class TypeSpec
    {
        public interface ITypeVisitor<T>
        {
            T Visit(Primitive t);
            T Visit(Collection t);
            T Visit(Object t);
            T Visit(Tuple t);
            T Visit(Dynamic t);
        }

        public abstract T Accept<T>(ITypeVisitor<T> visitor);

        public sealed class Primitive : TypeSpec
        {
            public enum PrimitiveType
            {
                Bool,
                Number,
                String
            }

            public PrimitiveType Type { get; }

            public Primitive(PrimitiveType type)
            {
                Type = type;
            }

            public override T Accept<T>(ITypeVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }


        public sealed class Collection : TypeSpec
        {
            public enum CollectionType
            {
                List,
                Map,
                Set
            }

            public CollectionType Type { get; }

            public TypeSpec ElementType { get; }

            public Collection(TypeSpec elementType, CollectionType collectionType)
            {
                Type = collectionType;
                ElementType = elementType;
            }

            public override T Accept<T>(ITypeVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public sealed class Object : TypeSpec
        {
            public IReadOnlyDictionary<string, TypeSpec> AttributeTypes { get; }

            public Object(IEnumerable<(string name, TypeSpec type)> attributes)
            {
                AttributeTypes = attributes.ToDictionary(kv => kv.name, kv => kv.type);
            }

            public Object(IReadOnlyDictionary<string, TypeSpec> attributes)
                : this (attributes.Select(kv => (kv.Key, kv.Value)))
            {
            }

            public override T Accept<T>(ITypeVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public sealed class Tuple : TypeSpec
        {
            public IReadOnlyList<TypeSpec> ElementTypes { get; }

            public Tuple(IEnumerable<TypeSpec> elementTypes)
            {
                ElementTypes = elementTypes.ToList();
            }

            public override T Accept<T>(ITypeVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public sealed class Dynamic : TypeSpec
        {
            public Dynamic()
            {
            }

            public override T Accept<T>(ITypeVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
    }

    public static class TypeSpecUtils
    {
        private class ToJsonVisitor: TypeSpec.ITypeVisitor<bool>
        {
            private readonly StringBuilder builder;

            public ToJsonVisitor(StringBuilder builder)
            {
                this.builder = builder;
            }

            public bool Visit(TypeSpec.Primitive t)
            {
                string primitiveType;
                switch (t.Type)
                {
                    case TypeSpec.Primitive.PrimitiveType.Bool:
                        primitiveType = "bool";
                        break;
                    case TypeSpec.Primitive.PrimitiveType.Number:
                        primitiveType = "number";
                        break;
                    case TypeSpec.Primitive.PrimitiveType.String:
                        primitiveType = "string";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"unknown primitive '{t.Type}'");
                }
                builder.Append("\"");
                builder.Append(primitiveType);
                builder.Append("\"");
                return true;
            }

            public bool Visit(TypeSpec.Collection t)
            {
                string collectionType;
                switch (t.Type)
                {
                    case TypeSpec.Collection.CollectionType.List:
                        collectionType = "list";
                        break;
                    case TypeSpec.Collection.CollectionType.Map:
                        collectionType = "map";
                        break;
                    case TypeSpec.Collection.CollectionType.Set:
                        collectionType = "set";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"unknown collection type '{t.Type}'");
                }

                builder.Append("[");
                builder.Append("\"");
                builder.Append(collectionType);
                builder.Append("\"");
                builder.Append(",");
                t.ElementType.Accept(this);
                builder.Append("]");
                return true;
            }

            public bool Visit(TypeSpec.Object t)
            {
                builder.Append("[\"object\",{");
                bool isFirst = true;
                foreach (var item in t.AttributeTypes)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        builder.Append(",");
                    }

                    builder.Append("\"");
                    builder.Append(item.Key);
                    builder.Append("\":");
                    item.Value.Accept(this);
                }
                builder.Append("}]");
                return true;
            }

            public bool Visit(TypeSpec.Tuple t)
            {
                builder.Append("[\"tuple\",[");

                bool isFirst = true;
                foreach (var item in t.ElementTypes)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        builder.Append(",");
                    }

                    item.Accept(this);
                }
                builder.Append("]]");
                return true;
            }

            public bool Visit(TypeSpec.Dynamic t)
            {
                builder.Append("\"dynamic\"");
                return true;
            }
        }

        public static string ToJson(this TypeSpec type)
        {
            var builder = new StringBuilder();
            type.Accept(new ToJsonVisitor(builder));
            return builder.ToString();
        }

        public static byte[] GoLangMarshalJson(this string jsonString)
        {
            var bytes = Encoding.UTF8.GetBytes(jsonString);

            return bytes;
        }
    }
}
